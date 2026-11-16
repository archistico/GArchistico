# Registro modifiche — GenerativeArt Studio

## 2026-11-16 — M4D.3-R1 — Profondità, carta e bokeh

- Nuovi algoritmi `OrganicAperture` e `DepthParticleField`; artwork `paper-cut` e `bokeh-depth` con ombre gaussiane, strati ordinati, luce e fuoco controllabili. Catalogo a 17.
- Livelli riutilizzabili SrcOver con blur, drop shadow e opacità di gruppo; aperture even-odd e dischi illuminati. Budget prima delle superfici/output, cancellazione e alpha premoltiplicato.
- Paesaggi a strati con sei parametri opzionali: lontananza sfocata, ombre morbide direzionali e bordi luminosi. Nuovi lavori Studio con profondità attiva; JSON precedenti e default catalogo preservati.
- Dieci ricette Full HD, due fine-2x, 73 ricette totali; anteprime e confronto effetti spenti/accesi. Screenshot Studio aggiornato. README mantiene solo descrizione, installazione ed esecuzione.
- Build Release senza avvisi/errori, **801/801 test Windows**; smoke Full HD/Studio e 15 PNG precedenti identici. Guida `M4D3_PROFONDITA.md`, evidenze `docs/validation/M4D3_R1_*`. M4C.3 resta aperta per editor livelli/maschere/blend; nessuna nuova dipendenza di produzione.

## 2026-11-16 — M4D.2-R1 — Pittura ed evoluzione dei generatori iniziali

- Nuovi algoritmi `OrganicStroke` e `FractalProfile1D`; artwork `flow-brush` e `strata-landscape` con setole/pressione, campi curl/vortice/braid, profili frattali, luce, atmosfera e ombre. Catalogo a 15.
- Curve parametriche, Noise Flow, Plasma, Streamlines distanziate e Flow Loop ampliati con controlli artistici opzionali. Noise Flow fino a 10000 tracce. Nuovi lavori Studio con preset più ricchi; apertura JSON e default di catalogo preservati.
- Quattordici ricette Full HD e anteprime; 63 ricette totali. Confronto dei cinque generatori iniziali a pari seed/risoluzione. Tredici renderer precedenti verificati con PNG identici via SHA-256.
- FlowTracer cancellabile internamente, budget per vertici/setole/campioni e sequenze. Modificare solo opacità nei flussi storici conserva le traiettorie. Esportazione CLI verificata su 12 fotogrammi Full HD distinti.
- Codex Windows: build Release senza avvisi/errori, **733/733 test**, quattordici PNG Full HD e smoke Studio. Screenshot aggiornato; README mantiene solo descrizione/installazione/esecuzione, senza changelog o roadmap. Guida `M4D2_PITTURA_EVOLUZIONE.md`, log/dati `docs/validation/M4D2_R1_*`.

## 2026-11-16 — M4D.1-R1 — Dettaglio e controllo

- Algoritmi `GuillocheCurve` (trocoidi razionali chiuse con armoniche) e `TonalHatching` (copertura tonale filtrata, fino a quattro direzioni). Artwork `guilloche` e `tonal-engraving`, colori indipendenti, finiture e fine-2x; catalogo a 13.
- Otto ricette Full HD con anteprime: tre Guilloché, tre incisioni e due Domain Warp avanzati; 49 JSON validati. Controlli per precisione, strati, geometria, inquadratura, tratteggio, toni e maschere locali.
- Dieci controlli Domain Warp opzionali: lacunarità, persistenza, scala deformante, rotazione, assi, offset, contrasto/gamma. Tre PNG precedenti confrontati via SHA-256 e identici.
- Passo fine degli slider e conservazione dell'input esatto. Corretto crash/modifiche da TextChanged ritardati dopo il cambio di generatore, emerso nello smoke dello Studio.
- Codex Windows: **626/626 test**, build Release senza avvisi/errori, otto PNG Full HD generati/decodificati, controlli e input preciso verificati headless. Log/dati in `docs/validation`, guida in `M4D1_DETTAGLIO_CONTROLLO.md`. Dipendenze di produzione invariate.

## 2026-11-16 — M4B.1-R1 — Domain Warp/LIC, streamlines dense e picker

- Algoritmi riutilizzabili `DomainWarpNoise2D` e `LineIntegralConvolution`; artwork `domain-warp` e `lic-flow`, tre palette/quattro ruoli colore, materia/finiture e fine-2x. Catalogo a 11, sette ricette Full HD con anteprime.
- Streamlines da 500 a 10000 massime, distanza/spessore opzionali, candidati adattivi fino a 200000 e distribuzione su tutto il canvas nelle impostazioni estese. Cancellazione durante il tracing; vecchi parametri/default conservati.
- Picker ridimensionabile con footer Annulla/Conferma fuori dallo scroll e altezza adattata allo schermo in DIP.
- Build Release con zero avvisi/errori, **558/558 test**, sette PNG Full HD generati/decodificati, copertura delle due ricette dense su 64 blocchi. Smoke picker headless a quattro altezze, inclusi conferma/annulla. Evidenza Windows Codex in `docs/validation`; desktop/DPI e altri OS da verificare.
- Tool C# opzionale per riprodurre gli smoke; unica nuova dipendenza del tool Avalonia.Headless 12.1.4, prodotto invariato nelle dipendenze.

## 2026-11-16 — M4C.2-R4 — Correzione test budget materia

- Riprodotto il fallimento R3 segnalato dall'utente (513/514): il test attendeva il rifiuto di un poster 4000×4000 standard consentito dai nuovi budget.
- Sostituito con quattro casi validi verificati senza rendering e quattro casi non validi verificati prima dell'output; coperti limite standard per entrambi gli assi e limite fine-2x. Nessuna modifica al codice di produzione.
- Build Release riuscita con zero avvisi/errori e **521/521 test superati da Codex su Windows**, SDK 10.0.201. Pacchetti locali e telemetria Avalonia disattivata durante la verifica; smoke GUI e poster manuali restano aperti.
- Documentazione, manifest SHA-256 e ZIP completo aggiornati. Evidenza in `docs/validation/M4C2_R4_windows.log`.

## 2026-11-16 — M4C.2-R3 — Test JSON, poster 5 MP e anteprima completa

- **Riscontro Windows M4C.2-R2:** build riuscita; su 507 test, 506 superati e `RecipeTests.MissingParametersAreRejected` fallito perché la sostituzione testuale generava una virgola finale JSON. Il test ora usa `JsonNode` per togliere la proprietà `scale` in modo sintatticamente valido; parser runtime invariato.
- **Finiture:** `ArtisticFinishOptions.MaximumFinishPixels` passa da 4.194.304 a 16.777.216. Aggiornati il gate motore e quello della superficie Skia. Manteniamo il limite 4096×4096 statico globale. Il limite Plasma separato a 4 MP è eliminato: 2500×2000 con grana/carta/dithering non è più rifiutato solo per la dimensione. Per `fine-2x` resta il limite dei 16 MP sottopixel, quindi fino a 4 MP finali.
- **Preview Studio:** campi separati per massimo raster anteprima e massimo riquadro visibile; `Adatta`, `100%` e `Raster = output`; zoom automatico proporzionale al ridimensionamento e stato che confronta le dimensioni preview/output.
- Test aggiornati per limite poster, Plasma 5 MP (validazione senza allocazione grande), fitting/proporzioni, preview a risoluzione piena. Nessuna modifica a `algorithmVersion`, JSON v1, seed, colori o rendering predefinito.
- Archivio R3 integrale; build/test Windows R3 non ancora confermati.

---

## 2026-11-16 — M4C.2-R2 — Hotfix switch HSV / avvio Studio massimizzato (conferma Windows attesa)

- **Bug segnalato dall'utente su M4C.2-R1:** `StudioColorValue.cs(54,44)` CS8506 e CS8131. Causa: il compilatore C# non riesce a inferire un unico tipo per le tuple dello `switch` che mescolano componenti `double` con zeri `int`.
- **Fix:** uso di `0d` in ogni posizione a zero dei sei rami; nessuna modifica al calcolo HSV, RGB, alpha o al formato ricetta JSON v1. Aggiunte sei varianti cromatiche nei test `StudioColorValueTests`.
- **Studio:** apertura automatica in stato `Maximized` (titolo e taskbar rimangono accessibili); ripristino e ridimensionamento consentiti.
- Questo archivio contiene il progetto completo M4C.2 + hotfix; compilazione/test della R2 non ancora confermati. Il blocco di compilazione della R1 è invece reale e registrato.

## 2026-11-16 — M4C.2-R1 — Carta, materia e pigmenti (GATE WINDOWS APERTO)

- Aggiunti quattro parametri opzionali **MATERIA** su nove generatori: `paper-texture` none/cotton/rough, `paper-strength`, `pigment-granulation`, `ink-absorption`; default spenti, formato ricetta v1 e ID/versione algoritmi invariati.
- `MaterialPostProcessor` realizza fibre procedurali multiscala, granulazione cromatica guidata dalla luminanza e assorbimento locale basato sui pixel originali con sole tre righe di buffer. Texture seeded stabile sul canvas e alpha invariato.
- Pipeline con materiali prima di grana e dithering della M4C.1, dopo fine-2x. La via PNG senza effetti rimane invariata; il limite 4.194.304 pixel per finitura già esistente resta attivo.
- Nuovi `MaterialPostProcessorTests`, aggiornamento `CatalogTests`, cinque ricette Full HD, specifica dedicata e documenti operativi allineati. La baseline Windows confermata resta M4A.4-R1; nessun nuovo gate dichiarato chiuso.

---


## 2026-11-16 — M4C.1-R1 — Grana e dithering deterministici (GATE WINDOWS APERTO)

- Baseline utente **M4A.4-R1** valida; revisioni **R2/R3** ancora da compilare e testare. Nessun test Windows dichiarato in questa consegna.
- Aggiunti a tutte le 9 opere quattro controlli **FINITURA** facoltativi nel JSON v1: `grain-amount` (default 0), `grain-size` (default 1), `dither-mode` (default none; Bayer 4×4 o 8×8), `dither-levels` (default 8). `ArtworkParameterSpec.Integer` esteso con `optionalInJson`, lasciando invariato il comportamento delle vecchie chiamate.
- Nuovi `ArtisticFinishOptions` e `ArtisticPostProcessor`: grana da hash seed/coordinate senza Random globale, retino ordinato, alpha intatto e texture fissa su ogni frame del loop. `ArtworkRenderEngine` trasmette seed e token di cancellazione fino alla codifica.
- `SkiaRasterSurface.WritePng` conserva il vecchio ramo non modificato quando gli effetti sono spenti; quando attivi applica la finitura dopo l'eventuale riduzione 2×. Budget **4.194.304 pixel finali** verificato prima di render e output; poster 4000×4000 standard senza finitura ancora consentiti quando supportati.
- `ArtisticPostProcessorTests` e `CatalogTests` coprono default, JSON, nove generatori, alpha, riproducibilità, cancellazione, budget e supersampling; nuovi quattro esempi Full HD.
- Aggiornati documenti, handoff, roadmap e manifesto. **Verifiche statiche e ZIP possibili; build/xUnit/smoke non eseguibili per assenza .NET SDK**. Specifica: `docs/M4C1_GRANA_DITHERING.md`.

---

## 2026-11-16 — M4A.4-R3 — Supersampling e PNG alpha-safe (GATE WINDOWS APERTO)

- Ricevuta richiesta di proseguire senza poter testare. Baseline confermata su Windows **M4A.4-R1**; M4A.4-R2 ancora da validare, non attribuirle test già passati.
- Nuovo parametro facoltativo `render-quality` su **9 generatori**: `standard` (percorso originale) / `fine-2x` (campionamento 4 sottopixel/pixel, downsampling nel formato BGRA premoltiplicato). JSON v1 e algorithmVersion invariati.
- `RasterQualityPolicy` applica in `Validate()` un limite di **16.777.216 pixel temporanei** a 2×. L'export a 4000×4000 resta possibile in modalità standard, non fine-2x; messaggi prima dell'allocazione e scrittura.
- `SkiaRasterSurface` supporta scala raster 1 o 2 e riduzione lineare alpha-safe tramite `SKSamplingOptions` senza API obsolete. `ArtworkRenderEngine` e `SdfOrbitRenderer` campionano pixel reali 2× per Plasma/SDF, a coordinate logiche costanti. Nessun caching frame, stream PNG lasciato aperto.
- Aggiunto `RasterQualityTests` e aggiornato `CatalogTests` e `StudioCreativeControlsTests`; tre esempi qualità fine-2x a 1920×1080; documentazione e manifesto aggiornati.
- **Verifiche disponibili:** struttura sorgenti/ricette, ZIP e SHA-256; **non eseguiti** build .NET 10, xUnit, rendering PNG e test visuali/memoria. Gate Windows R2/R3 aperto.

---


## 2026-11-16 — M4A.4-R2 — Colori per tutti gli artwork e picker HSV/RGB/Alpha (GATE WINDOWS APERTO)

- **L'utente ha confermato M4A.4-R1: compilazione e suite xUnit riuscite su Windows.** Questa R2 è un incremento nuovo e deve essere nuovamente validata.
- I cinque generatori storici (`parametric-curves`, `noise-flow`, `spaced-streamlines`, `looping-flow`, `plasma-noise`) ora espongono ruoli colore con `Auto`, RGB e alpha. Sfondo gradiente dei primi due preservato quando Auto; override esplicito sfondo uniforme.
- Studio: pulsante **Scegli…**, finestra HSV/RGB/Alpha/HEX con conferma/annulla; campione Auto coerente con la palette corrente su tutti i nove generatori, e aggiornamento cambiando preset.
- Nuovo `StudioColorValue` testabile senza GUI; catalogo e renderer aggiornati senza introdurre pacchetti o variare schema JSON v1, ID e versioni algoritmo. Migrazione opzionale delle vecchie ricette.
- Test `StudioColorValueTests`, `ArtisticColorRoleTests`, test catalogo aggiornato; cinque nuove ricette Full HD di studio cromatico e specifica `M4A4_R2_COLORI_HSV.md`.
- Non implementati ancora: AA raster analitico, supersampling limitato, opzioni DPI, armonie colore e texture materiche. Il colore usa sRGB straight-alpha in input; le superfici native usano premoltiplicazione Skia.
- Ambiente di preparazione privo di SDK .NET: verifica statica e integrità archivio sì; **nessuna compilazione/test xUnit/GUI R2 eseguita** qui. Comando Windows: `clear && dotnet clean && .\tools\validate.ps1`.

## 2026-11-16 — M4A.4-R1 — Laboratorio artistico, parametri e qualità (GATE WINDOWS APERTO)

- L'utente ha confermato **M4A.3-R1 Voronoi: compilazione e test superati su Windows**, quindi chiuso il gate non ancora documentato nell'handoff H1.
- Nuove ricette create nello Studio: 1920×1080; anteprima limitata a 800×600/480.000 pixel. `looping-flow` ammette il Full HD con 16 frame default nel budget di 40 milioni di pixel cumulativi.
- Pannello dei parametri più ampio, gruppi per categoria, campo HEX e swatch per colore, pulsante Auto (ritorno alla palette) e input validato senza salvare stringhe cromatiche errate.
- `ArtworkParameterSpec.OptionalInJson` e `ColorOverride`: nuove proprietà facoltative in importazione, valori canonici e retrocompatibilità delle vecchie ricette JSON v1. Colori per ruolo in Topografia, SDF, Poisson e Voronoi; le palette restano preset di partenza.
- Poisson: gamma spacing 0.001–0.2 e raggio 0.1–128, fino a 50.000 punti e 6M celle, variabilità raggio, noise pigmento, punti solid/ring/mixed.
- Topografia: tratti precise/handdrawn/soft, deformazione seedata e algoritmo Chaikin riutilizzabile in `GenerativeArt.Algorithms`, rugosità e spessori più ampi.
- Quattro esempi Full HD nuovi per confrontare stile organico/morbido, stippling denso e mosaico colorato. Test xUnit aggiunti per retrocompatibilità, colore, default Full HD, validazione del loop, parametri e differenze raster; documentazione e handoff allineati.
- **Non ancora implementati:** vero ColorPicker visuale, modifica di ogni colore dei cinque generatori storici, supersampling/AA raster, texture ricche. SDK .NET 10 assente qui: build/test/smoke R1 da eseguire su Windows.

## 2026-11-16 — M4A.3-R1 · HANDOFF-H1 — Allineamento documentazione (SOLO DOCUMENTI)

- Consegnato un archivio completo della **M4A.3-R1 invariata nel codice**, per il passaggio a una nuova conversazione. Nessun sorgente C#, progetto, test, ricetta JSON, script o licenza modificato.
- Distinto in modo esplicito **ultimo gate Windows confermato: M4A.2-R1** da **ultima implementazione: M4A.3-R1 con gate ancora aperto**. Non sono stati eseguiti nuovi test .NET in questo handoff.
- Risolte le incongruenze nei documenti operativi: catalogo a **nove** generatori, M4A.2 già validata, architettura aggiornata dopo SDF/Poisson/Voronoi, comando PowerShell canonico e checklist del poster 4000×4000.
- Riscritti `CURRENT_STATE.md`, `HANDOFF.md`, `ARCHITETTURA_PRODOTTO.md`; aggiornati `README.md`, `INDEX.md`, `ROADMAP.md`, `TESTING.md`, `QUALITY_GATES.md`; creato `NUOVA_CHAT.md` con testo pronto da incollare.
- Materiale storico e specifiche delle milestone precedenti preservati; restano aperti smoke reale M4A.3, Linux/macOS, QA memoria/animazioni e qualità visiva.


## 2026-11-16 — M4A.3-R1 — Voronoi e Mosaico Voronoi (GATE WINDOWS APERTO)

- Ricevuta conferma Windows dell'utente: **M4A.2-R1 compila, test passati, Studio e CLI OK**; questa è la baseline.
- Nuovo `VoronoiTessellation` (Algorithms): clipping semipiani con precisione double, poligoni convessi limitati, area, centroidi, Lloyd iterativo, limiti e cancellazione. Nuova suite `VoronoiTessellationTests`.
- Nuovo renderer `VoronoiMosaicRenderer`: siti seeded jittered, palette glass/copper/lagoon, fughe e celle Skia; estesa `SkiaRasterSurface` con `DrawPolygon` via `SKPathBuilder` e test fill/bordo.
- Catalogo da **8 a 9 artwork**: nuovo `voronoi-mosaic`, parametri `sites`, `relaxations`, `jitter`, `gutter`, `palette`; dispatcher e validazione budget integrati, JSON v1 invariato.
- Due ricette di esempio, standard 1200×1200 e poster 4000×4000; test per PNG, seed, parametri, JSON, annullamento e validazione poster senza render gigante nella suite.
- Aggiornati Studio title, README, roadmap, handoff, quality gates, testing, index e specifica `M4A3_VORONOI_TASSELLAZIONI.md`. Nessuna modifica al viewport/animazioni o agli otto generatori preesistenti.
- SDK .NET 10 assente qui: **compilazione/test xUnit e smoke visuale M4A.3 ancora da eseguire**. Comando `clear && dotnet clean && .\tools\validate.ps1`.

## 2026-11-16 — M4A.2-R1 — Geometrie SDF e campionamento Poisson (GATE WINDOWS APERTO)

- Ricevuta conferma dall'utente che **M4A.1-R4 funziona completamente** su Windows, dopo il fix budget. Baseline preservata.
- Nuovo modulo C# `SignedDistance2D`: circle, box, rounded box, rotazione, CSG (`Union`, `Intersection`, `Difference`) e `SmoothUnion` con validazioni. Primo artwork `sdf-orbits` per geometrie implicitamente connesse, fasce, tre palette e parametri artistici.
- Nuovo modulo C# `PoissonDiskSampler`: campionamento Bridson seeded, lista attiva, griglia spaziale e limiti su celle/punti/tentativi; primo artwork `poisson-stippling` con pigmenti deterministici e tre palette.
- Catalogo passato da **sei a otto** opere; `ArtworkRenderEngine` condiviso espanso. Vecchi ID, algoritmi, JSON v1, Studio, pan/zoom, preferiti, temi e CLI legacy invariati.
- Suite ampliata con `SignedDistance2DTests`, `PoissonDiskSamplerTests`, casi rendering PNG, seed, parametri, JSON, cancellazione e validazione poster 4000×4000. Aggiunti quattro esempi JSON e specifica `M4A2_SDF_POISSON.md`.
- **SDK .NET 10 non disponibile qui**: build, test e immagini M4A.2 non eseguiti. Richiesto gate Windows completo e smoke visuale e di memoria sulle grandi risoluzioni.

## 2026-11-16 — M4A.1-R4 — Correzione regressione budget topografico (GATE WINDOWS APERTO)

- L'utente ha riportato che **M4A.1-R3 compila**, ma 1 test su 371 fallisce: `ArtworkRenderEngineTests.TopographicRecipeRoundTripBudgetAndCancellation` riga 147 (`Assert.Throws`, nessuna eccezione). 370 test erano superati.
- Causa verificata: il test attendeva il rifiuto di `4000×4000` con `cell-size=3`, `contours=24` (= 42.645.336 celle×livelli), mentre il budget R3 era stato alzato a 48.000.000.
- Correzione mirata: `TopographicContourRenderer.MaximumCellLevelBudget` = **40.000.000**. Invariato il limite di **16.777.216 pixel**, quindi `1600×1600`, `4000×4000` standard e fino a `4096×4096` per ricette ammissibili restano possibili.
- Test rafforzato: stesso 4000×4000 e passo 3 accetta 22 livelli (39.091.558), rifiuta 23 (40.868.447) e 24 (42.645.336). Nessun test di rendering 4000×4000 eseguito nell'ambiente di preparazione.
- Aggiunto esempio `examples/recipes/topographic-poster-4000.json` per lo smoke CLI reale di un poster 4000×4000; la suite xUnit verifica solo la validazione delle dimensioni, non genera il PNG enorme.
- Aggiornati titolo Studio, README, ROADMAP, HANDOFF, CURRENT_STATE, TESTING e QUALITY_GATES; algoritmi, viewport, preferiti, export atomico e schema JSON invariati.
- **.NET SDK 10 non disponibile qui:** serve gate Windows `clear && dotnet clean && .\tools\validate.ps1` e smoke reale dell'export PNG ad alta risoluzione.

## 2026-11-16 — M4A.1-R3 — Export topografico hi-res per stampa (GATE WINDOWS APERTO)

- Ricevuta conferma utente per **M4A.1-R2: compila, test OK, Studio OK, CLI OK** su Windows. Il sesto generatore funziona, ma in export reale il renderer `topographic-contours` rifiutava `1600×1600` con l'errore *"massimo 2.097.152 pixel per immagine"*.
- `TopographicContourRenderer.ValidateBudget` ora allinea il tetto pixel al budget statico del runtime: **16.777.216 pixel per immagine** (fino a `4096×4096`) e alza il limite di complessità a **48.000.000 celle × livelli**, sufficiente per poster `4000×4000` con impostazioni standard.
- Aggiornati i test `ArtworkRenderEngineTests`: `1600×1600` e `4000×4000` sono ora consentiti; restano rifiutati casi oltre 4096 px per lato o combinazioni troppo dense (`4000×4000`, `cell-size=3`, `contours=24`).
- Documentazione aggiornata: README, CURRENT_STATE, HANDOFF, TESTING, QUALITY_GATES, `M4A1_ISOLINEE.md`.
- SDK .NET 10 assente qui: **R3 da validare su Windows** con `clear && dotnet clean && .\tools\validate.ps1` e smoke CLI/Studio del PNG topografico hi-res.

## 2026-11-16 — M4A.1-R2 — Correzione compilazione SKPathBuilder (GATE WINDOWS APERTO)

- La build Windows di **M4A.1-R1** si è fermata in `SkiaRasterSurface.cs` alle chiamate obsolete `SKPath.MoveTo` e `SKPath.LineTo` (due CS0618; warnings treated as errors). La suite M4A.1-R1 non è stata completata.
- `SkiaRasterSurface.DrawPolyline` ora crea il percorso con `SKPathBuilder`, lo ottiene con `Detach()` e rilascia sia builder sia path; **nessun silenziamento di CS0618** e nessun downgrade di SkiaSharp.
- Aggiunto il test `SurfaceDrawPolylineScalesPointsWithoutClosingPath` per verificare coordinate scalate, segmento orizzontale/verticale e assenza di chiusura diagonale.
- Invariati Marching Squares, algoritmo topografico, catalogo a 6 generatori, ricette JSON v1, CLI, Studio, pan/zoom, temi ed export.
- R2 non compilata né testata in ambiente di preparazione (SDK .NET 10 non disponibile); richiesto gate Windows completo con `clear && dotnet clean && .\tools\validate.ps1` e smoke PNG topografico.

## 2026-11-16 — M4A.1-R1 — Marching Squares e Curve topografiche (BUILD WINDOWS FALLITA: CS0618)

- Ricevuta conferma utente per **M3D.2-R2: build, test, Studio, preferiti e tema tutti OK su Windows**. Baseline R2 validata.
- Primo incremento M4A: `MarchingSquares.Trace` con interpolazione coerente degli archi condivisi, raccordo delle linee, gestione selle 5/10 con decider bilineare e cancellazione.
- Nuovo generatore **`topographic-contours`**: terreno Simplex frattale, bande cromatiche, 4–24 isolinee, passo regolabile, 3 palette e spessore regolabile; path Skia collegati.
- Sei ID attesi nel catalogo; renderer e JSON v1 integrati, esempio fornito. Tutte le vecchie ricette/algoritmi restano invariati.
- Test xUnit estesi per topologia, contorni chiusi, budget, seed, parametri, PNG, cancellazione e ritorno JSON; documentazione M4A.1, README, ROADMAP, HANDOFF e QA aggiornati.
- **SDK .NET 10 assente durante la preparazione**: file JSON e archivi possono essere verificati staticamente, ma non è stata eseguita una nuova build o suite; gate Windows M4A.1 aperto.


## 2026-11-16 — M3D.2-R2 — Preferiti, recenti e temi (GATE WINDOWS APERTO)

- Ricevuta conferma dall'utente che **M3D.2-R1 compila, supera i test e Studio funziona** su Windows.
- Aggiunti `StudioPreferences`, file preferenze JSON schema v1 e palette chiara/scura in Application; scrittura atomica, lettura limitata a 64 KiB, massimo otto percorsi recenti e filtro preferiti.
- Nello Studio: stella per opera, filtro `Solo preferiti`, elenco ricette locali aperte/salvate, rimozione dalla lista senza eliminare il file, cambio tema in diretta con ripristino al riavvio. La colonna sinistra ora scorre.
- Nuovi `StudioPreferencesTests`: modello, dedup/limiti, roundtrip, versioni non valide, chiavi duplicate/ignote, dimensione eccessiva e palette.
- Aggiornati README, CURRENT_STATE, ROADMAP, HANDOFF, TESTING, QUALITY_GATES e specifica M3D.2-R2.
- Invariati `PreviewViewport`, rendering, algoritmi, CLI, ricette JSON v1 ed export R1. **Gate R2 non eseguito qui**: manca SDK .NET 10; richiesta la validazione Windows.

## 2026-11-16 — Conferma M3D.2-R1 Windows

- L'utente segnala compilazione riuscita, suite test OK e Studio funzionante. Lo stress della cancellazione durante export oneroso e i provider virtuali restano verifiche separate.

---

## M3D.2-R1 — 16 novembre 2026 — Export cancellabile e parametri numerici esatti (DA VALIDARE)

- **Conferma dall'utente:** M3D.1-R3 compila, suite xUnit verde, pan con sinistro e zoom con rotella perfetti su Windows. Base di questa revisione validata.
- Nuovo `AtomicOutputFiles.WriteGeneratedAsync`: output renderer direttamente su file temporaneo locale, senza un byte array PNG completo aggiuntivo; cancellazione e cleanup anche per eccezioni del renderer, pubblicazione dopo flush. `WriteAsync` esistente invariato.
- Studio: **Annulla export**, indicatore d'attività indeterminato, stato e serializzazione dei job; export full-resolution fuori dal thread UI, stop playback e preview differita. Per file virtuali fallback non atomico documentato.
- Numeri dei parametri: TextBox esatto accanto allo slider; input con `.` o `,`, canonicalizzazione invariante, gestione valori invalidi senza alterare ricette; nuovi `StudioParameterInput` e test dedicati.
- Test aggiunti in Application e Skia: export streaming decodificabile, errore, annullamento, sovrascrittura vietata, precisione e input invalidi.
- **M3D.2-R1 non compilata/testata qui**: .NET SDK 10 assente. Gate Windows aperto.

## M3D.1-R3 — 16 novembre 2026 — Pan sinistro e zoom a cursore (DA VALIDARE)

- Ricevuto feedback utente: **M3D.1-R2 build e test OK su Windows e Studio funzionante**. Il bootstrap Avalonia è quindi confermato, non più un gate pendente.
- Implementata `PreviewViewport` (Avalonia `Canvas`) per drag con tasto sinistro, pointer capture, zoom rotella ancorato al cursore e tasto **Centra**. Rimossi gli scrollbar automatici della preview; il vecchio slider resta sincronizzato.
- Aggiunto `StudioViewportState` in Application per offset/zoom in DIP, limiti 25–200%, pan con margine visibile e mantenimento della vista tra frame della stessa dimensione.
- **Sei nuovi test xUnit** sulle trasformazioni: centro, pan, zoom focale, mantenimento tra frame, resize, limiti e trascinamento estremo.
- Pan e zoom non innescano render e non cambiano ricetta o PNG esportato. Nessuna nuova dipendenza, nessun linguaggio di sviluppo aggiuntivo.
- **R3 non compilata/testata in ambiente di preparazione: .NET SDK 10 assente**. Il comando Windows obbligatorio è `clear && dotnet clean && .\tools\validate.ps1`, poi smoke mouse nello Studio.

## M3D.1-R2 — 16 novembre 2026 — Fix compilazione bootstrap Avalonia (DA RIVALIDARE)

- Ricevuto esito Windows per M3D.1-R1: build fallita con **CS0118** (`Application` visto come namespace) e **CS1061** (`Application.Run` inesistente) in `src/GenerativeArt.Studio/Program.cs`; non risulta alcuna esecuzione della GUI o suite R1 completa.
- Corretto il bootstrap con `AppBuilder.Configure<StudioApp>().UsePlatformDetect().StartWithClassicDesktopLifetime(args)` e nuova `StudioApp : global::Avalonia.Application` con `FluentTheme` e `IClassicDesktopStyleApplicationLifetime.MainWindow`.
- Nessun cambiamento a rendering, ricette JSON, catalogo, test o interfaccia funzionale. La verifica reale è ancora richiesta con `clear && dotnet clean && .\tools\validate.ps1` e avvio Studio.
- Corretto il vincolo documentale: **XAML/AXAML è permesso**; il progetto resta C#/.NET + PowerShell minimo, senza introdurre Python, Go, Kotlin, Java e altri linguaggi applicativi non autorizzati.
- Controllo archivio/integrità effettuato in ambiente di preparazione; SDK .NET 10 assente, **nessuna build R2 eseguita**.

## M3D.1-R1 — 16 novembre 2026 — Primo Studio Avalonia code-only (GATE APERTO)

- Ricevuta conferma Windows dall'utente: **M3C.3-R2 compila, suite xUnit e `list` OK**. Regressione CatalogTests R1 risolta.
- Nuovo progetto `GenerativeArt.Studio`, interfaccia C# senza XAML: catalogo ricercabile, parametri tipizzati, seed/risoluzione, preview zoom, JSON, export PNG, playback `looping-flow` Play/Pausa/fase.
- Anteprima in background con cancellazione delle richieste obsolete, debounce in editing, salto dei tick occupati in playback e serializzazione dei render.
- Aggiunti `StudioPreviewPolicy`, `AtomicOutputFiles` e 7 test xUnit Application nuovi, oltre alle versioni NuGet Avalonia 12.1.6.
- JSON v1, API CLI e algoritmi originari **non modificati**. NuGet/SDK per la UI nuovi; primo restore richiede la rete.
- **Gate .NET M3D.1 non eseguito qui**: l'SDK .NET 10 non è presente. Command Windows: `clear && dotnet clean && .\tools\validate.ps1`. Dopo test OK, `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.
- Limitazioni: export animazione GUI solo singolo PNG; GIF/APNG/video non realizzati; smoothness loop da misurare e migliorare.

## M3C.3-R2 — 16 novembre 2026 — Correzione regressione test catalogo (DA RIVALIDARE)

- Riscontro reale su Windows nella revisione R1: `GenerativeArt.Application.Tests.CatalogTests.CatalogHasStableSortedIdsAndLoopCapability` fallisce perché attendeva 4 descrittori e il catalogo contiene correttamente 5 opere dopo l'aggiunta di `plasma-noise`.
- Sostituita l'asserzione numerica obsoleta con un confronto dell'**elenco esatto degli ID ordinati**, includendo `plasma-noise`. Aggiunte verifiche mirate su capacità raster, assenza di animazione e parametri `frequency`/`octaves`/`palette`.
- Nessuna modifica ai renderer, ai descrittori di produzione, alle ricette o al formato JSON. Gli altri test non sono dichiarati superati senza esecuzione.
- Comando Windows richiesto dall'utente, nella directory `GenerativeArt` che contiene la solution: `clear && dotnet clean && .\tools\validate.ps1` (PowerShell 7). Validazione R2 in attesa dell'esecuzione su Windows: SDK .NET 10 assente qui.

## M3C.3-R1 — 16 novembre 2026 — IMPLEMENTATA, NON ANCORA VALIDATA CON .NET

- Aggiunto contratto `IArtworkRasterRenderer` in Application; implementato `ArtworkRenderEngine` Skia per tutte le opere da ricetta.
- Abilitato `render-recipe` per `noise-flow` (parametro `lines`) e `spaced-streamlines` (`max-lines`).
- Aggiunto generatore `plasma-noise` con Simplex fBm, frequenza, ottave e tre palette.
- Reindirizzati tutti e quattro i comandi CLI legacy allo stesso runtime.
- Introdotto salvataggio statico `CreateNew` e cleanup dei file frame incompleti; manifest scritto soltanto al termine.
- Aggiunti test PNG/seed/parametri/cancellazione/output rollback e ricette di esempio.
- Rafforzati i limiti in `ArtworkRecipeJson.Deserialize(string)` prima della conversione UTF-8 e cleanup del JSON incompleto in `ArtworkRecipeFiles.SaveNew`.
- Attenzione: **qui non è disponibile SDK .NET 10**; nessuna nuova compilazione, suite xUnit o smoke PNG eseguiti. La validazione precedente dell'utente si riferisce a M3C.2-R1.

## 2026-11-16 — Consolidamento documentazione / passaggio nuova chat (solo documenti)

- L'utente ha confermato **M3C.2-R1: compilazione, test xUnit, creazione JSON e PNG riuscite** su Windows. I vecchi riferimenti al blocco 288/289 riguardavano la versione precedente al fix JSON e non sono più attuali.
- Unificati README, stato corrente, roadmap, handoff e criteri di test; aggiornata la distinzione fra codice implementato, prove Windows e controlli futuri.
- Eliminati piani paralleli, log di verifica ridondanti e note singole dei fix, riassunte in `STORICO_CORREZIONI.md`.
- **Nessuna modifica a file C#, csproj, solution, test, ricette, script, licenze o fixture.** Nessuna nuova build eseguita durante questa sola revisione documentale.

---

## Cronologia originale delle consegne (stato al momento della stesura)

Le etichette «DA VALIDARE» o «NON ANCORA VALIDATA» nelle voci seguenti descrivono **la situazione storica all'atto della consegna**, non la baseline attuale. Per lo stato valido usare `CURRENT_STATE.md`.

## 2026-11-16 — M3C.2-R1: test JSON, eccezione derivata (DA RIVALIDARE)

- Segnalato da Windows: soluzione compilata; suite 289 test, 288 superati e uno non superato in `RecipeTests.TrailingCommaAndCommentsAreRejected`.
- Il parser `System.Text.Json` rifiuta correttamente i commenti, ma genera `JsonReaderException` (sottotipo di `JsonException`); `Assert.Throws<JsonException>` xUnit v2 richiede il tipo esatto.
- Corrette **solo due asserzioni** del medesimo test in `tests/GenerativeArt.Application.Tests/RecipeTests.cs`, usando `Assert.ThrowsAny<JsonException>` per commenti e trailing comma.
- Nessuna modifica ai sorgenti di produzione, allo schema, alle regole JSON, alle dipendenze o agli altri test.
- **Build e test dopo il fix non eseguiti in questo ambiente**: gate G3C.2 ancora aperto; riepilogo storico consolidato in `docs/STORICO_CORREZIONI.md`.

## 2026-11-16 — M3C.2: ricette JSON e rendering da ricetta (DA VALIDARE)

- Aggiunte `ArtworkRecipe`, `RecipeAnimation`, `ArtworkRecipeJson`, `ArtworkRecipeFiles` in Application; formato JSON v1 con parametri tipizzati, seed UInt64 decimale stringa e versione algoritmo esatta.
- Validazione ricette: proprietà duplicate/ignote rifiutate, versioni incompatibili, limiti dimensionali e di 64 KiB in lettura, no sovrascrittura dei preset.
- CLI `recipe-new`, `recipe-validate`, `recipe-show`, `render-recipe`; conservati i vecchi comandi.
- Primo dispatch reale: `parametric-curves` applica la scala; `looping-flow` applica raggio e palette oltre a seed e timing; le altre due demo rispondono esplicitamente con funzionalità ancora non disponibile.
- Suite xUnit Application estesa, ricette esempio, documentazione e roadmap aggiornate, nessuna nuova dipendenza NuGet, nessun linguaggio applicativo diverso da C#.
- **SDK .NET 10 non disponibile in questo ambiente**: build, xUnit e smoke M3C.2 non eseguiti. Ultima baseline confermata dall'utente: M3B.

## 2026-11-16 — M3C.1 + prime sequenze periodiche (DA VALIDARE)

- Conferma dell'utente: M3B compila, test superati e PNG creato correttamente su Windows (G3B chiuso).
- Priorità artistica aggiuntiva: le animazioni del repository Go diventano requisito di prodotto; analizzati `platform.go` e `poster.go` dell'archivio originale.
- Nuovo assembly `GenerativeArt.Application`: catalogo esplicito di quattro descrittori, capacità static/vector/animated/seamless, parametro tipizzato con validation, versioni e duplicate-check.
- Nuova CLI `list`, `describe`, più `render-loop-frames` (frame PNG separati con manifest, senza accumulo in RAM).
- Nuovi `LoopFrame` nel Core e `LoopNoiseField2D` in Algorithms; suite xUnit ampliata in Core, Algorithms e Application.
- Solution espansa a nove progetti senza nuovi package NuGet, senza introdurre Go/Kotlin/Python/XAML.
- Update della roadmap: preview loop con Studio M3D, GIF/APNG streaming anticipati a M5, video/motion blur a M6.
- **Non eseguiti build/test C# e smoke PNG della nuova versione**: SDK .NET non presente nell'ambiente di preparazione. Nuovi gate aperti.

## 2026-11-16 — P2.1: catalogo di preferenze artistiche

- Registrate 48 denominazioni Gȧrt e 4 del progetto Go con riferimenti ai file sorgente presenti negli ZIP forniti.
- Crinis evidenziato come benchmark estetico prioritario; famiglie di algoritmi riallineate ai gusti espressi.
- Identificate le varianti da confermare, inclusa la doppia denominazione Sixsix2/Sixsix 2.
- Nessun cambiamento a codice C#, test, dipendenze o script. Baseline M3B invariata, già validata dall’utente su Windows.

## 2026-11-16 — P2: revisione strategica dopo conferma M3B

- M3B/G3B chiuso su Windows in base al feedback utente: compilazione, xUnit e PNG riusciti.
- Nessuna modifica a sorgenti C#, progetti, test o script; nuova consegna esclusivamente documentale.
- Definita visione Framework + CLI + GenerativeArt Studio, con primo Studio anticipato a M3D.
- Roadmap unica riorganizzata: M3C registry/recipe/3 artwork; M3D GUI minima; M4 algoritmi; M5 varianti/metadata; M6 export/animazioni; M7 simulazioni/3D; M8 release.
- Nuovi: VISIONE_PRODOTTO, ESPERIENZA_STUDIO, ARCHITETTURA_PRODOTTO, CATALOGO_PRIORITA, REGISTRO_DECISIONI_P2; QUALITY_GATES aggiornato. Vecchi piani marcati storici.

# 16/11/2026 — M3B: integrazione flow e audit completo del repository Go (DA VALIDARE)

- Ricevuta conferma utente: la baseline **M3A-R1** compila, passa tutti gli xUnit e genera PNG su Windows; gate G3A chiuso.
- Nuove implementazioni C#: `VectorGrid2D`, `CurlNoiseField2D`, `EvenStreamlineOptions`, `EvenStreamlineTracer`; limiti di memoria/campioni/candidati.
- Nuovo comando CLI `render-flow-spaced` per streamlines RK4 con seed riproducibile; `render-demo` e `render-flow` conservati.
- Nuovi xUnit per griglie, curl, riproducibilità e limiti di spaziatura. M3B non compilato/testato nell'ambiente di consegna (SDK .NET assente).
- Analizzati **399 file (385 Go)** del repository caricato; inventario dei 164 descrittori, matrice comparativa, rischi ed espansione della roadmap.
- Nessun sorgente Go, Python, Kotlin, Bash, XAML o shader custom nel repository prodotto; nessun nuovo package NuGet.

# 16/11/2026 — M3A-R1 (fix compilazione + audit comparativo Go)

- Corretto `CS0019` in `FlowDemo.cs` sostituendo `i % 3 switch` con `(i % 3) switch`. Unico sorgente di produzione modificato.
- Integrato studio documentale di `jdxyw/generativeart`, matrice Gȧrt/Go/C# e roadmap prioritaria di integrazione, senza codice Go.
- Build e xUnit M3A-R1 non eseguiti qui (SDK .NET assente); test Windows antecedente fallito in CLI per CS0019.

