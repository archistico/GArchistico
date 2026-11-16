# GenerativeArt — framework e Studio di arte generativa

**Versione corrente: M4D.1-R1 · 16 novembre 2026 (dettaglio e controllo).** Guilloché e incisione tonale, dieci nuovi controlli Domain Warp, otto ricette Full HD e input numerico fine. **Build Release senza avvisi/errori, 626/626 test Windows**, otto PNG verificati e smoke Studio headless eseguito da Codex. [Guida ai controlli](docs/M4D1_DETTAGLIO_CONTROLLO.md).

GenerativeArt è un framework **C#/.NET 10** ispirato a [Gȧrt](https://github.com/igr/gart) e [jdxyw/generativeart](https://github.com/jdxyw/generativeart). Un unico motore riutilizzabile offre **CLI headless + Studio desktop**. I repository originali sono fonti di studio, non dipendenze di runtime.

Per passare a una nuova conversazione partire da [`docs/NUOVA_CHAT.md`](docs/NUOVA_CHAT.md), [`docs/HANDOFF.md`](docs/HANDOFF.md), [`docs/CURRENT_STATE.md`](docs/CURRENT_STATE.md) e [`docs/M4D1_DETTAGLIO_CONTROLLO.md`](docs/M4D1_DETTAGLIO_CONTROLLO.md). Questo archivio contiene l'intero progetto **M4D.1-R1**, non una patch.

## M4D.1-R1 — Dettaglio e controllo

- **Guilloché · intrecci:** ingranaggi interni/esterni, armoniche, precisione fino a 48.000 campioni richiesti, fino a 200 curve, progressione, twist, spessore, opacità e colori indipendenti.
- **Incisione · tratteggio:** campi di rilievo/anelli/venature, fino a quattro direzioni, distanza, spessore, angoli, ondulazione, contrasto, gamma, campitura e maschere sfumate.
- **Domain Warp ampliato:** dieci controlli opzionali per dettaglio frattale, deformazione indipendente, rotazione, assi, spostamento e toni. Le tre ricette Full HD precedenti mantengono PNG identici byte per byte.
- **Studio:** slider con passo fine proporzionato al parametro, input esatto senza perdita di precisione e correzione degli eventi ritardati durante il cambio generatore.
- **Otto nuove ricette Full HD**, tre in fine-2x; catalogo a **13 generatori**, **49 ricette** totali. Anteprime incluse in `examples/previews`.

Per iniziare aprire `guilloche-banknote-1920.json`, `engraving-terrain-banknote-1920.json` o `domain-warp-silk-detail-1920.json`. La [guida ai controlli](docs/M4D1_DETTAGLIO_CONTROLLO.md) spiega l'effetto di ogni gruppo e come scegliere densità e precisione.

## M4B.1-R1 — Nuovi algoritmi, ricette e controlli

- **Domain Warp · venature:** deformazione annidata del dominio Simplex/fBm, disegni marble/clouds/ridges, tre palette e quattro colori indipendenti.
- **LIC Flow · fibre:** texture che segue un campo curl o vortice, con lunghezza, contrasto e dimensione delle fibre regolabili. Catalogo a **11 generatori**, disponibili nello Studio e nella CLI.
- **Streamlines distanziate:** massimo aumentato da 500 a **10.000**; nuovi controlli **distanza 1–64 px** e **spessore 0,1–12 px**. Per riempire il Full HD partire da `spaced-streamlines-dense-1920.json` (8000 linee, distanza 5) o `spaced-streamlines-silk-1920.json` (10000, distanza 3,5). Il numero è un tetto: distanza e spazio disponibile determinano le tracce effettive.
- **Picker colore:** pulsanti Annulla/Conferma fissi fuori dallo scroll; finestra più compatta, ridimensionabile e limitata all'area utile dello schermo.
- **Sette ricette 1920×1080** in `examples/recipes`, con immagini ridotte in `examples/previews`.

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\domain-warp-ocean-marble-1920.json .\output\domain-warp.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\lic-curl-ocean-1920.json .\output\lic-flow.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\spaced-streamlines-dense-1920.json .\output\streamlines-dense.png
```

Creare `output` se necessario e scegliere destinazioni nuove. Budget, tempi misurati, elenco ricette e comandi di smoke in [`M4B1_DOMAIN_WARP_LIC.md`](docs/M4B1_DOMAIN_WARP_LIC.md).

## M4C.2-R3 — JSON corretto, poster con finitura e anteprima dimensionabile

- Il test `RecipeTests.MissingParametersAreRejected` ora elimina la proprietà `scale` tramite `JsonNode`, senza lasciare una virgola finale. Il parser JSON resta rigoroso: input sintatticamente non validi vengono ancora rifiutati.
- Rimosso il limite separato di 4.194.304 pixel per **grana, dithering, carta e pigmenti**: in modalità **standard** il limite generale resta **4096×4096 e 16.777.216 pixel**. Anche **Plasma standard** supporta ora 2500×2000. Il preset `fine-2x` mantiene il limite dei **16.777.216 sottopixel** (quindi max 4.194.304 pixel finali). Restano i budget specifici degli altri generatori e dei loop.
- Nello Studio si impostano separatamente **dimensioni output**, **limiti di risoluzione raster anteprima** e **dimensioni massime del riquadro di anteprima**. Il tasto **Adatta** mostra tutta l'immagine, **100%** controlla i pixel reali, **Raster = output** chiede l'anteprima a risoluzione piena quando consentita. Il rapporto d'aspetto non viene deformato.
- Lo status mostra sia i pixel realmente renderizzati in anteprima sia quelli impostati per l'export. Vedi [`docs/M4C2_R3_PREVIEW_E_BUDGET.md`](docs/M4C2_R3_PREVIEW_E_BUDGET.md).

## M4C.2-R2 — Correzione HSV e avvio massimizzato

Corretto `StudioColorValue.FromHsv`: i sei rami `switch` costruiscono ora tuple `(double, double, double)` omogenee (costanti `0d`) per risolvere gli errori **CS8506** e **CS8131** della build R1. Aggiunti sei casi di test per i settori HSV intermedi e la conservazione dell'alpha. Lo Studio parte con `WindowState.Maximized`: occupa l'area utile dello schermo e conserva barra del titolo e taskbar; è possibile ripristinarlo o ridimensionarlo. Nessun cambiamento a ricette, output default o algoritmi artistici.

## Nuovo in M4C.2-R1 — carta e pigmenti

Il nuovo gruppo **MATERIA** in tutti e nove i generatori offre `paper-texture` (`none`, `cotton`, `rough`), `paper-strength`, `pigment-granulation` e `ink-absorption`. I default sono neutri e compatibili con le ricette v1; la grana cartacea e i pigmenti sono deterministici da seed e coordinate, preservano alpha, usano il budget di finitura già esistente e non accumulano frame. Cinque ricette Full HD sono incluse. Consulta [`docs/M4C2_MATERIA_CARTA_PIGMENTI.md`](docs/M4C2_MATERIA_CARTA_PIGMENTI.md).

## Nuovo in M4C.1-R1 — finiture artistiche riproducibili

Nel pannello **FINITURA** di ciascuno dei nove generatori sono disponibili `grain-amount` (0–0.35, default 0), `grain-size` (1–12 px), `dither-mode` (`none`, `bayer4`, `bayer8`) e `dither-levels` (2–32). La grana è ancorata a seed e coordinate, anche nei loop; il dithering modifica i canali RGB ma non l'alpha. Senza effetti il percorso PNG rimane identico a R3. Gli effetti vengono applicati sul raster finale, dopo l'eventuale riduzione 2×. Dalla R3 il limite delle finiture coincide con il limite statico globale di 16.777.216 pixel (la modalità fine-2x ha ancora un limite specifico). Quattro nuove ricette Full HD incluse; vedere [`docs/M4C1_GRANA_DITHERING.md`](docs/M4C1_GRANA_DITHERING.md).

**Stato Windows corrente:** M4D.1-R1 compila e supera 626/626 test; otto nuovi PNG Full HD e controlli Studio headless verificati. Desktop/DPI reali e poster grandi restano da provare.

## R3 — qualità raster selezionabile

Nel pannello parametri di **tutti e nove** i generatori si trova **QUALITÀ → Campionamento raster**: `standard` lascia inalterato il rendering R2; `fine-2x` renderizza a risoluzione doppia per asse e produce un PNG finale alle dimensioni richieste, con riduzione premoltiplicata rispettosa dell'alpha. Per `fine-2x` il limite è **16.777.216 sottopixel** (Full HD consentito, poster 4000×4000 in standard). Il parametro è opzionale in JSON v1 e non altera le vecchie ricette. Tre esempi 1920×1080 inclusi: `noise-flow-fine-2x-1920.json`, `voronoi-fine-2x-1920.json`, `plasma-fine-2x-1920.json`.

Dettagli, budget, test e comandi CLI in [`docs/M4A4_R3_SUPERSAMPLING.md`](docs/M4A4_R3_SUPERSAMPLING.md). **Il gate Windows R3 è aperto**.

## Avvio rapido (Windows / PowerShell)

Prerequisiti: SDK .NET 10, connessione NuGet per il primo restore, sistema supportato dalle librerie native SkiaSharp. Aprire PowerShell nella cartella che contiene `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

La riga usa **PowerShell 7+**. Per un test mirato delle preferenze: `dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --filter FullyQualifiedName~StudioPreferencesTests`.

Lo script esegue `dotnet --version`, restore, build Release e tutti i test xUnit. In caso di errore, conservare il log. **M4A.4-R1** è confermata compilata e testata su Windows. **M4A.4-R2** richiede nuova build/test e smoke colore.


### Avvia GenerativeArt Studio (M4D.1-R1)

Dopo il comando di validazione precedente, avvia il nuovo progetto desktop:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

La finestra C# / Avalonia permette di scegliere uno dei tredici generatori, modificare i parametri, aprire/salvare JSON v1 ed esportare PNG anche ad alta risoluzione per le opere supportate. **Anteprima R3 già validata:** sinistro+drag per il pan, rotella per lo zoom ancorato al cursore (25–200%), tasto Centra. **R1 validata:** campi numerici esatti accanto agli slider e PNG cancellabile con file temporaneo atomico su percorsi locali. **R2 validata:** stella per aggiungere generatori ai preferiti, filtro `Solo preferiti`, fino a otto ricette recenti locali nella barra sinistra, tema chiaro/scuro modificabile nel pannello destro e persistente al riavvio. I percorsi non vengono inviati a servizi esterni. `looping-flow` include Play/Pausa; la GUI esporta un frame, la sequenza completa resta nella CLI. Dettagli: [`docs/M3D2_PREFERENZE_TEMI.md`](docs/M3D2_PREFERENZE_TEMI.md), [`docs/M3D2_EXPORT_PARAMETRI.md`](docs/M3D2_EXPORT_PARAMETRI.md) e [`docs/M3D1_STUDIO.md`](docs/M3D1_STUDIO.md).

### Nuovo in M4A.4: controlli artistici e nuove ricette Full HD

- **Nuove opere nello Studio:** risoluzione della ricetta 1920×1080; preview interna predefinita max 800×600/480.000 pixel (per il 16:9: 800×450), ora configurabile separatamente da output e dimensione del riquadro. Il Flow Loop parte con 16 frame/18 FPS per restare nel budget cumulativo.
- **Colori indipendenti per tutti e nove i generatori (R2):** le palette rimangono preset, ogni ruolo può essere sovrascritto tramite HEX `#RRGGBB`/`#AARRGGBB`, campione, pulsante **Scegli…** per HSV/RGB/Alpha e pulsante **Auto** per ripristinare la tinta originale. In `parametric-curves` e `noise-flow` Auto conserva il gradiente di sfondo; un override lo sostituisce con tinta uniforme.
- **Stile e densità:** topografia `precise`, `handdrawn` o `soft`; Poisson fino a raggio 128, distanza relativa minima 0.001 e punti a anello/misti, con budget controllati.
- Le ricette JSON v1 vecchie restano apribili: i soli nuovi parametri ricevono default in importazione. La modifica non cambia gli ID né la versione degli algoritmi. I campioni Auto seguono la palette selezionata, mentre gli override espliciti restano indipendenti.

Prova gli esempi artistici R1 (file PNG di destinazione nuovi):

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-handdrawn-1920.json .\output\topographic-handdrawn-r1.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-soft-1920.json .\output\topographic-soft-r1.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\poisson-ink-study-1920.json .\output\poisson-ink-r1.png
```

**Esempi cromatici R2 Full HD:** `parametric-ink-1920.json`, `noise-flow-pastel-1920.json`, `spaced-streamlines-monochrome-1920.json`, `plasma-neon-1920.json`, `looping-flow-duotone-1920.json`. Aprirli nello Studio o con `render-recipe`; per un loop completo consultare il comando CLI `render-loop-frames` più avanti. Specifica R2: [`docs/M4A4_R2_COLORI_HSV.md`](docs/M4A4_R2_COLORI_HSV.md).

L'antialias delle primitive Skia esiste già; **il supersampling 2× opzionale è implementato in R3**, grana e dithering ordinato in M4C.1; texture materiche M4C.2 già implementate; AA analitico dedicato resta futuro. Specifica e priorità: [`docs/M4A4_LABORATORIO_ARTISTICO.md`](docs/M4A4_LABORATORIO_ARTISTICO.md).

### Prime immagini e catalogo

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project src/GenerativeArt.Cli -c Release -- list
dotnet run --project src/GenerativeArt.Cli -c Release -- describe looping-flow
dotnet run --project src/GenerativeArt.Cli -c Release -- render-demo .\output\demo.png 800 600
dotnet run --project src/GenerativeArt.Cli -c Release -- render-flow .\output\flow.png 800 600 42
dotnet run --project src/GenerativeArt.Cli -c Release -- render-flow-spaced .\output\spaced.png 800 600 42
```

### Ricette JSON v1 e runtime condiviso (baseline M3C.3-R2)

```powershell
dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-new parametric-curves .\output\curves.json 640 480 42
dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-validate .\output\curves.json
dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-show .\output\curves.json
dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\output\curves.json .\output\curves.png
```

`recipe-new` **non sovrascrive** JSON esistenti e tutti i renderer statici rifiutano una destinazione PNG già presente: scegliere nomi nuovi o pulire soltanto i propri output precedenti. Non cancellare indiscriminatamente file di lavoro.

### Tutti i renderer da ricetta, incluso Plasma

```powershell
dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\noise-flow.json .\output\noise.png
dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\spaced-streamlines.json .\output\spaced.png
dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\plasma-noise.json .\output\plasma.png
```

Le ricette v1 rimangono compatibili; `lines`, `max-lines`, `frequency`, `octaves` e `palette` controllano effettivamente il rendering.

### Nuovo in M4A.3: Mosaico Voronoi

`voronoi-mosaic` usa celle Voronoi ritagliate, rilassamento di Lloyd, fughe e tre palette (`glass`, `copper`, `lagoon`). **M4A.3 è stata compilata e testata con successo su Windows.** In M4A.4 sono disponibili anche override cromatici per sfondo, fuga, accento e cinque colori delle tessere. L'export ammette poster 4000×4000 entro il budget statico; la preview resta ridotta.

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\voronoi-mosaic.json .\output\voronoi-m4a3.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\voronoi-poster-4000.json .\output\voronoi-poster-4000.png
```

Specifiche e limiti in [`docs/M4A3_VORONOI_TASSELLAZIONI.md`](docs/M4A3_VORONOI_TASSELLAZIONI.md). La CLI non sovrascrive file già esistenti.

### Nuovo in M4A.1: Curve topografiche e isolinee

La sesta opera `topographic-contours` realizza fasce cromatiche e isolinee connesse su un campo Simplex seedato; regolabili frequenza, numero di contorni, passo, spessore e palette `atlas`, `nocturne`, `terra`. I metodi e il viewport degli altri generatori non sono stati modificati. Comando CLI (file destinazione nuovo):

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release -- describe topographic-contours
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-contours.json .\output\topographic-m4a1.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-poster-4000.json .\output\topographic-poster-4000.png
```

Limiti renderer: 16×16 minimo, fino a 4096×4096 / 16.777.216 pixel e fino a 40.000.000 celle×livelli. Questo consente esportazioni come 1600×1600 e 4000×4000, mantenendo comunque un tetto per i casi estremi. Documentazione e smoke in [`docs/M4A1_ISOLINEE.md`](docs/M4A1_ISOLINEE.md). **Topografia M4A.1-R4 confermata funzionante su Windows; il nuovo gate riguarda M4A.4-R2.**

### Nuovo in M4A.2: SDF 2D, CSG e Poisson disk

Due moduli indipendenti da CLI/GUI ampliano il motore:

- **SignedDistance2D**: primitive cerchio, box e box arrotondato, rotazione, unione, intersezione, differenza e unione morbida. La nuova opera `sdf-orbits` genera forme implicite e fasce equidistanti con tre palette.
- **PoissonDiskSampler**: campionamento Bridson seeded, griglia spaziale, distanza minima fra i centri dei punti, massimo 50.000 punti/6.000.000 celle (nuovi limiti M4A.4, da validare su Windows) e annullamento. L'opera `poisson-stippling` offre scala del punto libera, forme ring/mixed e pigmenti personalizzabili.

Entrambe le opere possono essere esportate fino a **4000×4000** nel budget statico globale (limite attuale 4096×4096); l'anteprima resta ridotta. Il rendering effettivo a risoluzione poster richiede la prova Windows. La CLI richiede destinazioni nuove:

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\sdf-orbits.json .\output\sdf-orbits-m4a2.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\poisson-stippling.json .\output\poisson-stippling-m4a2.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\sdf-orbits-poster-4000.json .\output\sdf-orbits-poster-4000.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\poisson-stippling-poster-4000.json .\output\poisson-stippling-poster-4000.png
```

Specifica, test, complessità e limiti: [`docs/M4A2_SDF_POISSON.md`](docs/M4A2_SDF_POISSON.md).

### Anteprima delle animazioni: sequenze PNG, non video

```powershell
dotnet run --project src/GenerativeArt.Cli -c Release -- render-loop-frames .\output\loop-frames 512 512 42 24 24
dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\looping-flow.json .\output\loop-recipe
```

`looping-flow` produce fotogrammi PNG progressivi e un `manifest.json`; **GIF, APNG e MP4 non sono implementati**. Le directory di output dei frame devono essere nuove o vuote. Lo smoke JSON/immagine confermato su Windows si riferisce alla precedente M3C.2-R1; per questa revisione vanno ancora eseguiti build, test e smoke dei loop.

## Che cosa funziona oggi

- **Core:** geometria 2D, colori ARGB/sRGB/lineari/HSV/HSL/OKLab, gradienti, RNG deterministico, coordinate e `PixelBuffer`.
- **Algorithms:** Voronoi e Lloyd, SDF 2D/CSG, Poisson disk, Marching Squares e isolinee raccordate, Perlin/Simplex, fBm, flow fields, Euler/RK2/RK4, griglie vettoriali bilineari, curl noise e streamlines distanziate, campo periodico di animazione.
- **Rendering.Skia:** runtime condiviso dei tredici artwork, rendering raster e PNG, superfici native rilasciate dopo ogni frame.
- **Application:** tredici descrittori artwork, parametri tipizzati, ricette JSON v1 e contratto di renderer indipendente da GUI/CLI.
- **CLI:** demo legacy instradate nel runtime comune, catalogo, ricette e `render-recipe` per tutti e nove gli artwork, output senza sovrascrittura e cleanup dei file incompleti.
- **Tests:** quattro progetti xUnit (`Core`, `Algorithms`, `Rendering.Skia`, `Application`).

`noise-flow`, `spaced-streamlines` e `plasma-noise` sono disponibili anche tramite `render-recipe`. **M4A.3-R1 è stata confermata compilata e testata dall'utente su Windows; i nuovi controlli M4A.4 richiedono verifica.** Encoder GIF/video, SVG/PDF e Sweeper non sono ancora presenti.

## Progetti nella solution

| Progetto | Responsabilità |
| --- | --- |
| `GenerativeArt.Core` | Contratti e tipi base, senza GUI né SkiaSharp |
| `GenerativeArt.Algorithms` | Algoritmi matematici e generativi |
| `GenerativeArt.Rendering.Skia` | Rendering raster SkiaSharp e PNG |
| `GenerativeArt.Application` | Catalogo e ricette, senza dipendenza dalla GUI |
| `GenerativeArt.Cli` | Host console e demo |
| `GenerativeArt.Studio` | GUI Avalonia code-only con preview statiche/animate e ricette |
| `*.Tests` (4 progetti) | Test xUnit per le rispettive librerie, compresi test per streaming e precisione |

## Regole vincolanti

**C#/.NET 10** per implementazione e test, **PowerShell minimo** per la validazione. **Avalonia XAML/AXAML è consentito** per l'interfaccia, anche se lo Studio attuale è scritto in C#. Non introdurre Python, Go, Kotlin, Java o altri linguaggi applicativi non autorizzati; in caso di dubbio chiedere prima. JSON/CSV sono dati e Markdown è documentazione. Manteniamo un unico catalogo e un unico motore per CLI e Studio. Rendering e animazione non devono accumulare tutti i frame in memoria. Ogni incremento richiede test e un **ZIP completo**, non soltanto file modificati.

## Orientamento documentale

- **[Stato corrente](docs/CURRENT_STATE.md)** — cosa è verificato, cosa no.
- **[Handoff per nuova chat](docs/HANDOFF.md)** — contesto, file canonici, istruzioni per riprendere lo sviluppo.
- **[Messaggio pronto nuova chat](docs/NUOVA_CHAT.md)** — testo da incollare insieme allo ZIP completo.
- **[Roadmap unica](docs/ROADMAP.md)** — ordine delle milestone e gate di accettazione.
- **[Indice documentazione](docs/INDEX.md)** — specifiche, audit, decisioni e fonti storiche.
- **[Primo Studio Avalonia M3D.1](docs/M3D1_STUDIO.md)** — UI code-only, preview, animazioni, test e limiti.
- **[Studio M3D.2: export e parametri](docs/M3D2_EXPORT_PARAMETRI.md)** — nuovo export cancellabile, editor numerico preciso, gate Windows.
- **[Studio M3D.2-R2: preferiti e temi](docs/M3D2_PREFERENZE_TEMI.md)** — persistenza locale dei preferiti e delle ricette recenti, palette light/dark, test e prove GUI.
- **[M4A.2: SDF e Poisson](docs/M4A2_SDF_POISSON.md)** — algoritmi, generatori, budget, esempi e gate di validazione.
- **[Runtime condiviso M3C.3](docs/M3C3_RUNTIME.md)** — generatori, limiti, integrazione e test richiesti.
- **[Audit M3C.3-R1](docs/AUDIT_M3C3_R1.md)** — problemi individuati, correzioni e controlli ancora da eseguire.
- **[Ricette JSON v1](docs/M3C2_RICETTE.md)** — schema, limiti e comandi storici.
- **[Test e validazione](docs/TESTING.md)** — verifiche e limiti delle prove esistenti.
- **[Riferimenti artistici preferiti](docs/RIFERIMENTI_ARTISTICI_PREFERITI.md)** — Crinis prioritario, molte opere Gȧrt e animazioni Go.

Per licenze e attribuzioni: [`docs/THIRD_PARTY_NOTICES.md`](docs/THIRD_PARTY_NOTICES.md) e `licenses/`. La licenza definitiva di GenerativeArt è ancora da decidere prima della pubblicazione.
