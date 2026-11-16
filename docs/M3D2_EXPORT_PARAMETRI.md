# M3D.2-R1 — Export PNG cancellabile e parametri esatti

**16 novembre 2026 · Stato: implementato, build/test Windows di questa revisione da eseguire.**
La baseline M3D.1-R3 è stata confermata su Windows dall'utente: build, test, pan e zoom perfetti. Questa revisione non modifica `PreviewViewport` né `StudioViewportState`.

## Obiettivo e funzionalità

1. **Annulla export** accanto a **Esporta PNG**. Il pulsante si attiva soltanto a destinazione scelta; il footer mostra una barra di avanzamento *indeterminata* e lo stato (attesa renderer / generazione e scrittura). Non sono indicate percentuali inventate: il renderer è sincrono e non fornisce progressi granulari.
2. **Output locale senza buffer PNG managed completo:** `AtomicOutputFiles.WriteGeneratedAsync` riceve `Action<Stream,CancellationToken>`; su un worker ThreadPool apre un file `.partial` nella directory finale, passa lo stream direttamente al renderer, esegue flush su disco e pubblica tramite `File.Move` a generazione terminata. Il PNG non attraversa un `MemoryStream` + `ToArray` durante l'export su disco locale. Skia mantiene comunque internamente l'encode e le superfici native: non si promette RAM nulla.
3. **Cancellazione reale:** il token attraversa attesa del semaforo e motore di rendering. Se il token viene annullato prima della pubblicazione, il file temporaneo viene eliminato e l'eventuale precedente resta intatto. Un export per volta; niente accumulo di code. La chiusura dello Studio cancella un export in corso.
4. **Preview e rendering serializzati:** l'export cancella la preview pendente e interrompe Play; nuove modifiche non creano altre preview durante il job. Una preview differita riparte dopo il job se la ricetta è cambiata. Pan e zoom restano locali alla vista.
5. **Provider virtuali:** il file picker può offrire URI non locali. Per questi casi resta un fallback a un buffer `byte[]` singolo; non è possibile garantire il rename atomico né il ripristino di una destinazione virtuale parzialmente sovrascritta. Tale limite è intenzionale e non viene attribuito al percorso locale.
6. **Numeri esatti:** nei parametri `Integer` e `Number` è disponibile un TextBox accanto allo slider. I valori validi vengono normalizzati con `ArtworkParameterSpec.ValidateValue`; per i `Number` sono ammessi `.` e `,` come separatore decimale, mentre il JSON rimane sempre in cultura invariante. Una stringa invalida resta visibile in rosso mentre si corregge, **non modifica la ricetta** e viene ripristinata all'ultimo valore valido quando si perde il focus. Le ricette già caricate mantengono la precisione del valore (double round-trip).

## File e test

- `src/GenerativeArt.Application/Files/AtomicOutputFiles.cs`: aggiunto producer streaming con generazione off-thread. Il metodo `WriteAsync` esistente è mantenuto per JSON e compatibilità.
- `src/GenerativeArt.Application/Rendering/StudioParameterInput.cs`: parsing adattato all'input desktop, senza cambiare il formato ricetta.
- `src/GenerativeArt.Studio/StudioWindow.cs`: pulsanti, stato job, scrittura locale via stream, gestione provider virtuali, editor numerico.
- `tests/GenerativeArt.Application.Tests/AtomicOutputFilesTests.cs`: generazione diretta, errore renderer, cancellazione, divieto sovrascrittura non autorizzata.
- `tests/GenerativeArt.Application.Tests/StudioParameterInputTests.cs`: precisione, virgola, input invalidi e interi.
- `tests/GenerativeArt.Rendering.Skia.Tests/ArtworkRenderEngineTests.cs`: PNG prodotto via streaming decodificabile e identico a quello del runtime headless.

## Comandi Windows (PowerShell 7)

Estrarre lo ZIP completo in una cartella pulita, aprire PowerShell nella cartella con `GenerativeArt.sln` ed eseguire:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Dopo compilazione e test riusciti:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Test mirati facoltativi per isolare un errore:

```powershell
dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --filter "FullyQualifiedName~AtomicOutputFilesTests|FullyQualifiedName~StudioParameterInputTests"
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --filter "FullyQualifiedName~AtomicStreamingExportMatchesRendererAndDecodesPng"
```

## Smoke manuale richiesto su Windows

1. Con `plasma-noise`, trascina con il sinistro e ingrandisci con la rotella su un punto decentrato: pan/zoom non devono regredire.
2. Modifica un `Number` con `0,025` e poi con `0.0123456789` **se nell'intervallo del parametro**; confronta il JSON esportato. Prova un valore non valido, verifica il colore rosso e il ripristino all'uscita dal campo.
3. Esporta un PNG locale e aprilo: le dimensioni devono corrispondere a larghezza/altezza della ricetta (non dell'anteprima). Prova un file esistente, conferma sovrascrittura dal picker e verifica che il risultato venga aggiornato.
4. Avvia un export sufficientemente pesante (per esempio Plasma 2500×2000 in modalità standard con grana, consentito dalla M4C.2-R3), premi **Annulla export** mentre lavora: il vecchio file deve restare intatto e non devono restare file nascosti `.partial`. Il limite precedente di Plasma a 4.194.304 pixel è stato rimosso in R3; il budget statico generale resta 4096×4096. Per una cancellazione effettiva la macchina deve avere tempo di interagire prima che il render termini.
5. Esporta il singolo frame di `looping-flow` e riprova Play/Pausa, modifiche di parametri, chiusura durante l'export. Il PNG completo di tutta la sequenza resta per ora una funzione della CLI.

## Limiti e avanzamento successivo

- SDK .NET 10 assente nell'ambiente di preparazione: **non dichiarare build/xUnit passati** per M3D.2-R1 fino al test Windows.
- Non esiste ancora progress reporting numerico dal renderer, né una cache di thumbnail, né una UI recenti/preferiti o chiaro/scuro.
- L'export SVG/PDF/GIF/APNG/video è pianificato in M5/M6; le animazioni devono mantenere elaborazione progressiva senza accumulo di frame.
- Prossima slice M3D.2: preferiti/recenti locali leggeri e tema chiaro/scuro; poi M4A/M4B per espandere il motore artistico.
