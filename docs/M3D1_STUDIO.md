# M3D.1-R3 — GenerativeArt Studio: pan e zoom nell'anteprima

**16 novembre 2026 · Documento storico di M3D.1-R3.** L'utente ha successivamente confermato **compilazione e test OK, pan e zoom perfetti su Windows per R3**. La successiva M3D.2-R1 è descritta in [`M3D2_EXPORT_PARAMETRI.md`](M3D2_EXPORT_PARAMETRI.md). Le note che seguono descrivono lo stato al momento della creazione di R3.

## Novità M3D.1-R3 — navigazione con mouse

- **Pan:** tenere premuto il **tasto sinistro del mouse** sull'anteprima e trascinare, anche se l'immagine è più piccola dell'area visibile. La capture del puntatore mantiene il drag affidabile quando il cursore esce dal riquadro; `PointerCaptureLost` interrompe l'operazione. Una parte dell'immagine resta sempre recuperabile.
- **Zoom rotella:** su una tacca aumenta/diminuisce di circa il 15%. La posizione del **cursore è il centro dello zoom**: l'elemento puntato resta sotto il mouse, salvo raggiungimento dei limiti di pan. Intervallo 25–200%, condiviso con lo slider.
- **Slider e pulsante Centra:** lo slider resta operativo, scala attorno al centro dell'anteprima e viene aggiornato quando si usa la rotella; `Centra` ripristina soltanto la posizione, senza modificare il fattore di zoom.
- **Performance:** pan e zoom cambiano solo dimensioni e posizione della bitmap già visualizzata; **non avviano il renderer Skia** e non modificano seed, parametri o export. La stessa inquadratura persiste durante i frame consecutivi con dimensioni identiche.
- **Tecnica:** `PreviewViewport` è un controllo Avalonia isolato, `StudioViewportState` contiene la matematica pura in Application (unità DIP), con sei test xUnit. Il precedente `ScrollViewer` è sostituito da un viewport clipping; non ci sono più scrollbar automatiche.

**Test manuale prioritario:** con Plasma, ingrandisci su un dettaglio vicino a un bordo, trascina con sinistro, usa Centra, cambia palette e poi verifica Play/Pausa con `looping-flow`. Il dettaglio non deve scivolare durante la rotella e la vista non deve ricentrarsi a ogni frame. Comandi al termine di questo documento.

## Obiettivo realizzato

Un progetto aggiuntivo `src/GenerativeArt.Studio`, host desktop Avalonia **12.1.6** e .NET 10. La UI corrente è in C#; **XAML/AXAML è consentito** e può essere introdotto in successive revisioni. Nessun JavaScript. I cinque generatori e i loro descrittori vengono letti da `Application`; la superficie PNG viene resa dal medesimo `Rendering.Skia.ArtworkRenderEngine` già usato dalla CLI.

La finestra include una barra superiore (apri/salva JSON v1, esporta PNG), catalogo ricercabile, anteprima centrale con zoom 25–200%, editor dei parametri generato dai metadati (slider interi/numerici, scelta palette, checkbox/colore quando previsti), seed UInt64, risoluzione, pulsante di rigenerazione. `looping-flow` dispone di slider di fase, Play/Pausa e ritorno al primo frame, con FPS e numero frame modificabili.


## Correzione M3D.1-R2 — entry point e ciclo di vita

Nel test Windows della R1 il compilatore ha segnalato `CS0118` (`Application` risolto come spazio dei nomi `GenerativeArt.Application`) e `CS1061` (`Application.Run` inesistente nell'API utilizzata). Non si trattava di un errore degli algoritmi o delle ricette.

- `Program.cs` configura `StudioApp` e usa `StartWithClassicDesktopLifetime(args)`.
- `StudioApp.cs` eredita **esplicitamente** da `global::Avalonia.Application`; inizializza FluentTheme e tema scuro, quindi registra `StudioWindow` come finestra principale tramite `IClassicDesktopStyleApplicationLifetime`.
- I renderer, i descrittori dei generatori, i test e le ricette JSON rimangono invariati.
- Nessuna compilazione R2 era stata eseguita nell'ambiente di preparazione, **ma l'utente ha successivamente confermato compilazione, test e Studio funzionante su Windows**. Il gate da eseguire ora riguarda R3.
- L'utente ha chiarito: **XAML/AXAML consentito**, nessun divieto imposto per Avalonia; non introdurre linguaggi applicativi extra (Python, Go, Kotlin, Java ecc.) senza autorizzazione.

## Modello di esecuzione

- Le richieste di preview sono ritardate di 140 ms durante l'editing per evitare render inutili e vengono cancellate tramite `CancellationTokenSource` alla modifica successiva.
- `SemaphoreSlim(1)` serializza le superfici Skia tra preview ed esportazioni, così non accumuliamo lavori grafici concorrenti.
- I render avvengono via `Task.Run` fuori dal thread UI. Il PNG rientra come `byte[]` e viene convertito in una bitmap Avalonia; la bitmap precedente è rilasciata dopo il cambio sorgente.
- L'anteprima usa il ridimensionamento deterministico `StudioPreviewPolicy`: lato <= 800×600, massimo 480.000 pixel. Seed, parametri, conteggio frame e FPS non vengono alterati; il file finale usa invece la ricetta a risoluzione piena.
- Durante Play il debounce viene disabilitato e, se un frame è ancora in esecuzione, si **saltano** i tick successivi invece di accumulare render o cancellare ogni fotogramma prima che inizi.
- `StudioPreviewPolicy` è testata in Application senza Avalonia. `AtomicOutputFiles` scrive su file temporaneo nella stessa directory, poi pubblica mediante rename, con pulizia in caso di errore; sovrascrittura richiesta esplicitamente dal picker.
- I renderer stessi continuano a imporre i budget di M3C.3. Un render non valido mostra un messaggio invece di allocare a oltranza.

### Limiti intenzionali del primo rilascio

- Non sono implementati GIF/APNG/MP4 né esportazione dell'intera sequenza dalla GUI. Per un'opera animata `Esporta PNG` salva **solo il fotogramma visualizzato**. Per la sequenza usare `render-recipe` dalla CLI.
- Il playback è *best effort*: la fluidità non è garantita se il tempo di rendering supera l'intervallo fra fotogrammi. Non sono ancora disponibili cache thumbnail, editing timeline professionale, undo/redo completo o varianti batch.
- L'export mantiene una sola immagine PNG in memoria; l'app non accumula tutte le immagini della sequenza.
- L'operazione di export completo, a differenza delle preview, non ha ancora un pulsante di annullamento: occorre introdurre la cancellazione del job in M3D.2.
- Il file picker su provider virtuali non consente di garantire un rename atomico; quello locale usa `AtomicOutputFiles`.
- Nessuna prova di compilazione in questa consegna (SDK .NET non disponibile qui); la documentazione delle API Avalonia 12 è stata controllata prima dell'implementazione, ma il gate reale dipende dai test utente.

## I comandi Windows (PowerShell 7)

Dopo aver estratto lo ZIP nuovo **in una cartella separata**, entra nella directory `GenerativeArt` con `GenerativeArt.sln`. NuGet deve poter scaricare Avalonia 12.1.6 al primo restore.

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Se tutti i test passano, apri il primo Studio:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Per la regressione di catalogo CLI e il loop, senza sovrascrivere output esistenti:

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\looping-flow.json .\output\m3d1-loop
```

## Smoke desktop richiesto

1. Selezionare tre generatori diversi (Plasma, Noise Flow e Flow Loop); verificare che cambio catalogo e seed aggiornino la preview senza blocchi.
2. Modificare palette, ottave, frequenza o linee. Verificare pan sinistro, zoom con rotella sotto il cursore, slider sincronizzato, limiti e Centra, senza rigenerazione per sola navigazione.
3. In `looping-flow`, usare Play/Pausa e slider frame: la UI deve rimanere utilizzabile anche quando il rendering è lento.
4. Salvare JSON e riaprirlo, verificando seed, parametri, dimensioni e animazione. Confrontare con `recipe-validate` dalla CLI.
5. Esportare PNG statico e il singolo frame animato; aprire i file, controllare dimensioni e qualità. Provare file esistente e verifica della conferma di sovrascrittura.
6. Chiudere la finestra durante una preview e verificare che non compaiano eccezioni native. Inviare errori o screenshot se necessari.

## Lavoro successivo (M3D.1-R3 → M3D.2)

Priorità 1: confermare build/test R3 e smoke del mouse e playback su Windows; correggere eventuali incongruenze API. Poi: progressi, pulsante Stop per export, stime costi/risorse, salvataggio transazionale cross-provider, parametri numerici con campo modificabile a precisione piena e preview in scala differenziata. Continuare verso M4 (nuovi algoritmi/estetica) e M5 (animazioni GIF/APNG streaming) mantenendo il motore condiviso.
