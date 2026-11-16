# Test, build e smoke test — GenerativeArt

## M4D.2-R1 — Pittura e generatori iniziali

**Codex Windows, SDK 10.0.201: build Release senza avvisi/errori e 733/733 test** (Core 115, Algorithms 162, Application 133, Rendering.Skia 323). Quattordici preset Full HD renderizzati/decodificati, tutte le 63 ricette validate, confronto a seed 42 e PNG dei 13 generatori precedenti identici via SHA-256. Smoke Studio su nuovi artwork e cinque renderer ampliati: default nuovi e ripristino JSON verificati. Esportazione CLI di 12 PNG Full HD distinti, decodifica di tutti i frame. Guida [`M4D2_PITTURA_EVOLUZIONE.md`](M4D2_PITTURA_EVOLUZIONE.md), evidenze `docs/validation/M4D2_R1_*`. Desktop/DPI, poster dei nuovi artwork e altri OS restano aperti.

## M4D.1-R1 — Dettaglio/controllo verificati

**Codex, Windows, SDK 10.0.201: build Release con zero avvisi/errori e 626/626 test** (Core 115, Algorithms 158, Application 128, Rendering.Skia 225). Otto nuovi PNG Full HD generati/decodificati, tre in fine-2x; tutte le 49 ricette validate. Smoke headless del pannello dei nuovi artwork e Domain Warp, cambio rapido generatore, decimali esatti e slider fini. I tre PNG Domain Warp precedenti conservano SHA-256 identico. Guida e comandi in [`M4D1_DETTAGLIO_CONTROLLO.md`](M4D1_DETTAGLIO_CONTROLLO.md); evidenze `docs/validation/M4D1_R1_*`. Poster grandi, desktop/DPI e altri OS restano da provare. Gli stati sotto sono storici.

## M4B.1-R1 — Nuovi artwork e controlli, suite superata

**Codex su Windows, SDK 10.0.201: build Release senza avvisi/errori e 558/558 test** (Core 115, Algorithms 150, Application 126, Rendering.Skia 167). Sette ricette renderizzate/decodificate in Full HD e picker verificato headless a 280/320/480/620 px, conferma/annulla inclusi. Test numerici per domain warp/LIC, budget pre-output, cancellazione, parametri, alpha/JSON e tracing oltre 500 linee su Full HD. Dettagli e comandi riproducibili in [`M4B1_DOMAIN_WARP_LIC.md`](M4B1_DOMAIN_WARP_LIC.md); log e dati in `docs/validation`. Prove desktop/DPI reali, poster dei nuovi artwork e altri OS ancora aperti. Gli stati precedenti sotto sono storici.

## M4C.2-R4 — Test budget materia corretti, suite superata

Il log utente R3 riportava **513/514**: `MaterialBudgetRejectsLargeCanvasBeforeOutput` attendeva ancora un'eccezione per 4000×4000 standard. R4 verifica quattro dimensioni consentite senza rendering e quattro combinazioni vietate prima della scrittura. **Codex ha eseguito build Release (zero avvisi/errori) e 521/521 test su Windows**, SDK 10.0.201, con `tools/validate.ps1` e pacchetti locali. Dettagli di riproduzione e limiti in [`M4C2_R4_FIX_TEST_BUDGET.md`](M4C2_R4_FIX_TEST_BUDGET.md); log incluso in `docs/validation/M4C2_R4_windows.log`. Gli stati delle revisioni precedenti sotto sono storici. Lo smoke GUI e i poster reali non sono certificati dalla suite.

## M4C.2-R3 — Regressione R2 (506/507), 5 MP e preview

Conferma Windows utente: M4C.2-R2 compila e passa 506 dei 507 test; fallimento in `RecipeTests.MissingParametersAreRejected` per trailing comma introdotta dal test. In R3 la fixture usa `JsonNode` e verifica `ArgumentException` dopo aver rimosso soltanto `scale` da un JSON valido. Il parser di produzione rimane invariato.

Regressioni nuove o aggiornate: `StudioPreviewPolicyTests` (preview a 2500×2000 e proporzioni), `StudioViewportStateTests` (Fit dei quattro bordi e resize), `ArtworkRenderEngineTests` (accetta 2500×2000 con finitura su Plasma, senza render pesante in unit test), `ArtisticPostProcessorTests` (poster 4000×4000 standard con grana), budget generali. Smoke richiesto: Studio massimizzato, output 2500×2000, raster preview 800×600 poi `Raster = output`, `Adatta`/`100%`, grana/carta, export decodificabile. `fine-2x` a 2500×2000 deve ancora essere rifiutato. Consultare `M4C2_R3_PREVIEW_E_BUDGET.md`.

Comando PowerShell 7 dalla cartella di `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```


## M4C.2-R2 — Hotfix compilazione segnalata (da verificare)

Su Windows l'utente ha eseguito il build di M4C.2-R1: due errori CS8506 e CS8131 in `StudioColorValue.FromHsv` prima dei test. R2 rende `double` tutti gli zeri nei sei rami del `switch`; sei nuovi dati di test coprono tonalità 30/90/150/210/270/330° con alpha. Lo Studio ora deve avviarsi **massimizzato**, conservando il controllo Ripristina e la taskbar. Non indicare la build R1 come mai tentata, né dichiarare la R2 validata fino a conferma Windows.


## Gate M4C.2-R1 — supporti cartacei e pigmenti

La suite `MaterialPostProcessorTests` è predisposta per verificare seed, cotton/rough, alpha/trasparenza, assorbimento locale senza contaminazione dell’ordine di scansione, default disattivati, JSON v1, nove artwork, PNG 2×, budget e cancellazione. `CatalogTests` include anche i nuovi parametri MATERIA. Per lo smoke con esempi Full HD vedere [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md).


**Stato aggiornato al 16/11/2026:** M4A.4-R1 **compila e supera i test su Windows** (conferma utente). M4A.4-R2 aggiunge editor HSV/RGB/Alpha; **R3** aggiunge supersampling 2× e `RasterQualityTests`: build/xUnit/Studio **R2, R3, M4C.1-R1 e M4C.2-R1** su Windows **ancora da eseguire**. Lo smoke PNG 4000×4000 è separato dai test che verificano la validazione senza allocare superfici giganti.

**Test aggiunti M4A.4-R1:** `StudioCreativeControlsTests` (Full HD e preview 800×450, compatibilità JSON vecchi, colori HEX/Auto, range Poisson), `ContourStrokeVariationTests` (precise, handdrawn seedato e soft con curve chiuse), estensioni `ArtworkRenderEngineTests` (colore per ruolo, variazione tratto, Poisson denso, loop Full HD e budget cumulativo) e aggiornamento del limite in `PoissonDiskSamplerTests`. Dopo la suite provare davvero lo Studio con le nuove ricette. La validazione dei parametri da sola non prova la resa estetica.

## Gate M4C.1-R1 — finitura grana/dithering (da eseguire su Windows)

`ArtisticPostProcessorTests` copre default neutri, coerenza del seed, Bayer 4/8, alpha, cancellazione, JSON v1, nove generatori, limite memoria e interazione con fine-2x. `CatalogTests` controlla schema parametri aggiornato. Su Windows avviare `clear && dotnet clean && .\tools\validate.ps1`; poi consultare i comandi di export e il protocollo di confronto visuale in [`M4C1_GRANA_DITHERING.md`](M4C1_GRANA_DITHERING.md). Il vecchio rifiuto 4000×4000 con finitura valeva solo fino alla M4C.2-R2: da R3 in modalità standard è consentito, se il generatore lo permette; `fine-2x` rimane soggetto al suo budget più stretto.

## Gate R3 — qualità raster (da eseguire su Windows)

Il nuovo `RasterQualityTests` verifica il campionamento 2× per tutti i nove generatori, le dimensioni PNG, il default storico, alpha, budget e loop. Non sono test già eseguiti. Dopo `clear && dotnet clean && .\tools\validate.ps1` confrontare due PNG Full HD standard e fine seguendo [`M4A4_R3_SUPERSAMPLING.md`](M4A4_R3_SUPERSAMPLING.md). La dimensione PNG finale deve restare **1920×1080**; per `fine-2x` a 4000×4000 la validazione deve fallire prima di creare un output. Valutare memoria reale/CPU su Full HD (non certificati).

## Prerequisiti e comando canonico

- SDK .NET 10 compatibile con `global.json`.
- NuGet accessibile al primo ripristino pacchetti; asset nativi SkiaSharp compatibili con il sistema.
- Da PowerShell aperto nella cartella che contiene `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando combinato usa `&&` e richiede **PowerShell 7+**. Da Windows PowerShell 5.1 eseguire invece i tre comandi su righe separate controllando gli eventuali errori; non eseguire le verifiche dalla cartella esterna alla solution.

Lo script esegue `dotnet --version`, `dotnet restore`, `dotnet build -c Release --no-restore` e `dotnet test -c Release --no-build` su tutta la solution. Non usare test vecchi copiati da cartelle precedenti. **Non cancellare indiscriminatamente le cartelle dell'utente**: se una versione appare stantia, estrarre un nuovo ZIP in una cartella di lavoro separata.

## Regressione catalogo osservata su Windows (R1 → R2)

- In R1: `Assert.Equal(4, catalog.All.Count)` falliva perché `BuiltInArtworkCatalog.Create()` restituisce correttamente cinque opere dopo `plasma-noise`.
- In R2: il test confronta gli ID attesi, verifica ordine, capacità e parametri del Plasma. **Suite R2 superata** sul PC Windows dell'utente.
- Per isolare il test: `dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --filter "FullyQualifiedName~CatalogHasStableSortedIdsAndLoopCapability"`.

## Smoke CLI già confermati e da estendere

- **Confermati:** build, suite e `list` R2, e in precedenza JSON/PNG nella baseline M3C.2; legacy `render-demo`, `render-flow` e `render-flow-spaced` validati in precedenza.
- **Da verificare specificamente:** comando `render-loop-frames` e `render-recipe` con `looping-flow`, numero PNG/manifest, fase di chiusura del loop, comportamento di due run con lo stesso seed, e il rifiuto di cartella frame non vuota.
- **Da verificare su altri OS:** restore/build/test e rendering nativo su Linux/macOS; non dichiarare portabilità provata senza esito.

Comandi manuali riproducibili in [`README.md`](../README.md) e [`M3C2_RICETTE.md`](M3C2_RICETTE.md). L'output di `recipe-new` non sovrascrive: usare destinazioni nuove.

## Criteri sui test

| Categoria | Aspetti da proteggere |
| --- | --- |
| Core | dimensioni, overflow, colori, alpha, stride, clipping, RNG, tempo frame |
| Algorithms | Perlin/Simplex golden numerici, integratori Euler/RK2/RK4, curl, densità streamlines, griglie, loop |
| Rendering.Skia | bitmap BGRA/ARGB, risorse native, export PNG, trasparenza |
| Application | registry e schemi, ricette v1, tipi JSON, limiti di 64 KiB, proprietà duplicate/ignote, versioni, nessuna sovrascrittura |
| CLI (prossima espansione) | ricetta→render, valori effettivamente applicati, smoke output, regressione comandi legacy |

**Determinismo:** mantenere la stessa versione di algoritmo e seed nei test numerici; non promettere identità byte-per-byte tra PNG generati da backend/OS diversi senza verifica. Le fixture di origine Kotlin conservate in `reference/gart/` sono **dati**, non codice eseguito.

## Definizione di completamento

**Codice scritto** non significa compilato. **Compilazione confermata** non significa test superati. **Test superati** non significa che un nuovo renderer abbia prodotto un'immagine corretta. Registrare ogni evidenza in `CURRENT_STATE.md` e `ROADMAP.md` e riportare sempre chi ha eseguito la verifica. Questa politica vale anche se la toolchain .NET non è disponibile nell'ambiente di preparazione.

## Smoke storico M3C.3-R2 (conservato come regressione legacy)

1. `dotnet --version`: verificare SDK .NET 10; `clear && dotnet clean && .\tools\validate.ps1` da PowerShell 7 nella directory solution; conservare il log completo degli errori.
2. Eseguire `dotnet run --project src/GenerativeArt.Cli -c Release -- list`: **cinque artwork nella storica M3C.3-R2; nove nella M4A.3-R1 corrente**; `describe plasma-noise` deve mostrare tre parametri.
3. Usare i tre esempi in `examples/recipes/` con `render-recipe` in cartella `output` e PNG **non esistenti**. Verificare che i tre PNG siano decodificabili e diversi variando `lines`, `max-lines` e i parametri Plasma.
4. Creare il loop da `examples/recipes/looping-flow.json`: contare fotogrammi e `manifest.json`, aprire primo e ultimo frame, verificare riproducibilità e continuità al raccordo.
5. Eseguire tutti i legacy `render-demo`, `render-flow`, `render-flow-spaced`, `render-loop-frames` in nuovi percorsi. Rieseguire sullo stesso file e verificare **rifiuto senza sovrascrittura**.
6. Verificare il test `RecipeOutputWriterTests.FailedFrameRenderRemovesOnlyItsOwnOutput`; nessun residuo PNG parziale. Documentare RAM/CPU e prova Windows; non dedurre esiti per Linux/macOS.

Comandi copiabili in [`M3C3_RUNTIME.md`](M3C3_RUNTIME.md).


## Gate M3D.1-R3 — Navigazione dell'anteprima

Da PowerShell 7 nella solution:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

La build R1 ha prodotto CS0118 e CS1061 in `Studio/Program.cs` (namespace `Application` e metodo `Run` inesistente); in R2 la classe `StudioApp` ha risolto entrambi. **L'utente ha confermato test, build e Studio funzionante in R2.** In R3 verificare in aggiunta i sei `StudioViewportStateTests`, gli esistenti `StudioPreviewPolicyTests` e `AtomicOutputFilesTests`, e la compilazione di `GenerativeArt.Studio`. Aprire la GUI:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Smoke R3: 1) trascinare con sinistro tenuto premuto anche con immagine più piccola del viewport; 2) zoom rotella su un dettaglio decentrato, che deve restare sotto il cursore; 3) verificare scala 25–200%, sincronizzazione slider e tasto Centra; 4) cambiare parametro mantenendo zoom/pan; 5) Play/Pausa su looping-flow senza salti di camera a ogni frame; 6) passare ad altra opera, aprire JSON ed esportare PNG. **L'utente ha successivamente confermato build, test e corretto pan/zoom R3.** Ulteriori controlli in [`M3D1_STUDIO.md`](M3D1_STUDIO.md).

## Gate M3D.2-R1 — export PNG sicuro e campi esatti

**Compilazione, suite e avvio Studio confermati dall'utente in M3D.2-R1** con `clear && dotnet clean && .\tools\validate.ps1` e avvio dello Studio come sopra. Restano alcune prove specifiche di cancellazione durante export oneroso, memoria e provider remoti non eseguite separatamente.

- Nuovi test Application: `AtomicOutputFilesTests` (stream diretto, cancellazione, errori, divieto sovrascrittura) e `StudioParameterInputTests` (precisione invariante, virgola italiana, invalid input).
- Nuovo test Rendering.Skia: `AtomicStreamingExportMatchesRendererAndDecodesPng` (file PNG decodificabile e coerente col motore headless).
- Smoke manuale: provare PNG a risoluzione piena, premere **Annulla export** durante un rendering oneroso, controllare integrità del file precedente e assenza di `.partial`; modificare numeri da TextBox, esportare ricetta JSON e rileggere i valori; nessuna regressione pan/zoom e Play/Pausa.
- Il file system locale offre pubblicazione atomica; i provider virtuali *non* garantiscono rollback, anche con token cancellabile. I testi di stato non rappresentano percentuali.

Specifiche e comandi mirati in [`M3D2_EXPORT_PARAMETRI.md`](M3D2_EXPORT_PARAMETRI.md).

## Gate M3D.2-R2 — Preferiti/recenti/tema (BUILD/TEST/STUDIO CONFERMATI)

In PowerShell 7 dalla solution:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per isolare i nuovi test: `dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --filter FullyQualifiedName~StudioPreferencesTests`.

Avvio desktop:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

1. Segnare Plasma e un altro generatore fra i preferiti, filtrare e riavviare: stato coerente.
2. Salvare/aprire una ricetta JSON locale; ripeterne l'apertura dall'elenco recenti, quindi dimenticare la voce senza cancellare il JSON.
3. Cambiare tema durante Play/Pausa e zoom/pan; le impostazioni restano dopo il riavvio e non alterano la ricetta.
4. Prova di settings malformato: la GUI parte comunque e avvisa; file su disco non sostituito automaticamente.
5. Ripetere smoke PNG/cancellazione e osservare la memoria per 200+ frame; non dedurre automaticamente risultati Linux/macOS o provider virtuali.

**Qui l'SDK .NET non è disponibile:** ispezione statica e check ZIP/manifest non dimostrano la compilazione della R2. Ulteriori dettagli in [`M3D2_PREFERENZE_TEMI.md`](M3D2_PREFERENZE_TEMI.md).

## Gate M4A.1-R4 — Curve topografiche hi-res (CONFERMATA SU WINDOWS)

**Baseline precedente M4A.1-R2:** compilazione, test, Studio e CLI confermati su Windows. **M4A.1-R1:** build Windows fallita con due CS0618 per `SKPath.MoveTo/LineTo` obsoleti. **M4A.1-R2:** build, test, Studio e CLI confermati su Windows. **M4A.1-R3:** build riuscita e 370/371 test superati, ma il test del budget topografico fallisce perché 42.645.336 operazioni sono inferiori al limite di 48 milioni. **M4A.1-R4:** soglia aggiornata a 40 milioni, test alle soglie di 22 e 23 contorni; l’utente ha successivamente confermato «funziona tutto» su Windows.

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Verifiche di `MarchingSquaresTests`: campi costanti, pendenza planare con stitching, saddle 5 con discriminante positivo/negativo, curva chiusa, coordinate ordinate riproducibili, input finiti e cancellazione. `ArtworkRenderEngineTests`: decodifica PNG del sesto generatore, seed, ogni parametro, JSON roundtrip, budget e cancellazione; in particolare `topographic-contours` deve accettare 1600×1600 e 4000×4000 standard, accettare passo 3 + 22 livelli su 4000×4000 e rifiutare passo 3 + 23 o 24 livelli, oltre a bloccare oltre 4096 px per lato. `CatalogTests`: **sei ID ordinati**. `SkiaRasterTests`: nuovo path polilinea decodificato, validazione e regressione `SurfaceDrawPolylineScalesPointsWithoutClosingPath` (scala, due segmenti, path non chiuso).

Dopo la suite:

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-contours.json .\output\topographic-m4a1.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-poster-4000.json .\output\topographic-poster-4000.png
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Aprire il PNG, controllare contorni e continuità a vista e annotare eventuali anomalie. Verificare il nuovo generatore in Studio e i vecchi con pan/zoom, preferiti, tema e playback inalterati. La CLI non sovrascrive: utilizzare un percorso output nuovo.

## Gate M4A.2-R1 — SDF 2D/CSG e Poisson disk (CONFERMATO BUILD/TEST/STUDIO/CLI WINDOWS)

1. Validazione completa `clear && dotnet clean && .\tools\validate.ps1` da PowerShell 7 nella directory della solution.
2. `SignedDistance2DTests`: distanze e segni analitici di circle/box/rounded box, trasformazioni, CSG e input invalidi.
3. `PoissonDiskSamplerTests`: seed deterministico, distanza minima fra tutte le coppie, limiti, cancellazione, griglia rifiutata prima di allocare.
4. `CatalogTests` in M4A.2: **otto ID ordinati esatti** (storico), capacità raster delle due nuove opere; `ArtworkRenderEngineTests`: PNG decodificabili, parametri operativi, seed/JSON, export cancellato e poster 4000×4000 ammessi in validazione.
5. CLI: `render-recipe` sui nuovi esempi, PNG reali e un poster SDF 4000×4000; verificare colorazione, contorni, punti non collidenti e integrità output.
6. Studio: modifica dei nuovi parametri, preferiti, tema, preview/zoom/pan, annullamento export; regressione topografia e loop.

Per i comandi e i budget vedere [`M4A2_SDF_POISSON.md`](M4A2_SDF_POISSON.md). L'SDK .NET 10 non è disponibile qui: la scrittura dei test **non** dimostra che siano stati eseguiti.

## Gate M4A.3-R1 — Voronoi e mosaico (DA VALIDARE)

1. PowerShell 7 in `GenerativeArt`: `clear && dotnet clean && .\tools\validate.ps1`.
2. `VoronoiTessellationTests`: rettangolo mono-sito, bisettrice due siti, partizione area, distanza minima ai siti, centroidi Lloyd, input degeneri e annullamento.
3. `SkiaRasterTests`: `FilledPolygonIsClosedAndRendersAntialiasedStroke`; nessuna API SKPath obsoleta.
4. `CatalogTests`: **nove ID ordinati esatti**. `ArtworkRenderEngineTests`: PNG decodificabili, seed ripetibile, parametri visibili, JSON v1, cancellazione e validazione poster 4000×4000 (senza generare un file gigante nella suite).
5. CLI: `list` e `render-recipe` sugli esempi `voronoi-mosaic.json` e `voronoi-poster-4000.json` con destinazioni nuove; misura tempo e memoria sul poster reale.
6. Studio: cerca **Mosaico Voronoi**, modifica siti, rilassamenti, jitter, fuga e palette; verifica anteprima, pan/zoom, tema, preferiti e PNG. Ripeti smoke di topografia e flow loop.

Il gate xUnit non dimostra da solo la qualità visiva del mosaico. Consultare [`M4A3_VORONOI_TASSELLAZIONI.md`](M4A3_VORONOI_TASSELLAZIONI.md) per contratti, limiti e istruzioni.

## Handoff H1: checklist minima per M4A.3-R1

**Versione controllata documentalmente:** M4A.3-R1 (codice invariato). **Ultima suite riferita come superata dall'utente:** M4A.2-R1. Evitare di copiare i totali xUnit di release precedenti come se fossero relativi a M4A.3.

1. Estrarre l'archivio integrale in una cartella nuova e aprire PowerShell **7+** nella directory contenente `GenerativeArt.sln`.
2. Eseguire `clear && dotnet clean && .\tools\validate.ps1` e conservare l'esito; se fallisce, indicare progetto, test e linee esatte.
3. Dopo test verdi, `list` deve riportare **nove** generatori; aprire lo Studio e controllare `voronoi-mosaic` con ogni parametro, oltre a preferiti, temi, pan/zoom e looping-flow.
4. Generare da CLI un PNG Voronoi standard e il poster 4000×4000. Aprirli effettivamente per verificare dimensioni, fughe, bordi e qualità della composizione; annotare durata e memoria del poster.
5. Eseguire una prova di annullamento durante un export oneroso e verificare integrità del file precedente e assenza di `.partial` persistenti. La CLI non sovrascrive destinazioni esistenti.
6. Solo dopo esito positivo registrare **M4A.3-R1 validata** in `CURRENT_STATE.md` e `ROADMAP.md`. In caso di errori, correggere con una nuova revisione con test di regressione prima di M4B.

Per la ripartenza nella nuova chat leggere `HANDOFF.md` e usare `NUOVA_CHAT.md` come testo di accompagnamento. Lo sviluppo successivo è **M4B** e le animazioni restano una priorità del prodotto.


## Gate M4A.4-R2 — Editor colore per tutte le opere (da verificare su Windows)

**M4A.4-R1 è confermata con compilazione e xUnit superati dall'utente**. R2 cambia codice e richiede una nuova esecuzione. Ambiente di preparazione senza SDK .NET, quindi i test R2 sono presenti ma non eseguiti.

Da PowerShell 7 nella directory contenente `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Questo avvia restore/build Release e **tutti** i test (inclusi i nuovi `StudioColorValueTests`, `ArtisticColorRoleTests` e `CatalogTests` aggiornato). Un gate verde non sostituisce lo smoke GUI e visivo:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Provare il ColorPicker su tutti i nove generatori: HSV, R/G/B/A, HEX, Auto, cambio palette, Annulla; salvare/ricaricare JSON, confrontare PNG, playback Flow Loop e trasparenze Plasma. Controllare pan/zoom, temi, recenti e annullamento export. CLI e file nuovi documentati in [`M4A4_R2_COLORI_HSV.md`](M4A4_R2_COLORI_HSV.md). Nessun test di memoria su 4000×4000 o piattaforme Linux/macOS è dichiarato riuscito in questa consegna.
