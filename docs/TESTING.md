> **STATO CORRENTE M4E.5-P4K-R1 — 17/11/2026:** priorità richiesta dall'utente: **4K per tutti i generatori e le ricette, con massima complessità e supersampling 2×; 8K generale successivamente**. Disponibili pulsante Studio **4K · qualità massima**, CLI `prepare-4k`, profilo opzionale `production` e 34 nuovi esempi: catalogo **30 generatori / 30 preset / 216 ricette**. **1.371 test Windows passati**, build Release zero avvisi/errori; azione Studio verificata sui 30 generatori e 30 file preset invariati, inclusi i 17 personali. [Guida e limiti](M4E5_4K_PRODUZIONE.md), [prove e misure](validation/P4K_R1_REPORT.md). Consegna completa `GenerativeArt_M4E5_P4K_R1_COMPLETO.zip`. Restano distinti QA artistico dell'utente, desktop/DPI, altri OS, Fase B/D e Crinis R10 in pausa. README solo descrizione/installazione/esecuzione con screenshot. **I blocchi delle versioni precedenti sotto sono archivio storico.**
> **STATO CORRENTE M4E.4-V1-R1 — 17/11/2026:** Mosaico Voronoi ampliato con modalità `layered`, tessere mancanti, mosaici posteriori indipendenti, smussi/luce, ombre gaussiane, distanza e sfocatura. **Build Release zero avvisi/errori e 1.260/1.260 test Windows** (Codex, SDK 10.0.201: Core 115, Algorithms 254, Application 220, Rendering.Skia 671). 30 generatori, 30 preset incorporati e **182 ricette**, sei nuove Full HD, due fine-2x. PNG storici Voronoi identici in quattro casi; tutti i 30 file preset, inclusi i 17 personali, invariati. Editor/CLI e anteprima nativa verificati; 8K conservato per flat, non per il percorso filtrato. [Guida, confronto e verifiche](M4E4_V1_VORONOI_PROFONDO.md). Le note C5 e precedenti sotto sono storiche; restano aperti giudizio artistico dell'utente, desktop/DPI, altri OS e stress. Crinis/Cincinnus e G4 non modificati da questo incremento.

> **VALIDAZIONE WINDOWS C5-R1 — 17/11/2026:** corretti tre problemi riprodotti: xUnit2012 nel test del pittore, conteggio dei pigmenti che includeva pigment-granulation e campioni Auto Cincinnus mancanti. **Build Release zero avvisi/errori e 1.225/1.225 test superati da Codex, SDK 10.0.201** (Core 115, Algorithms 249, Application 219, Rendering.Skia 642); gate rapido 21 test già inclusi nel totale. Tool CincinnusPreview compilato; 176 ricette validate, tre PNG CLI 768×768 decodificati e Studio headless verificato su sei combinazioni palette/modalità. Catalogo/preset/esempi 30/30/176. [Rapporto e log](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). Qualità artistica Cincinnus/G4, desktop/DPI e stress ai limiti ancora aperti; Crinis R10 in pausa. **Le note di validazione C5/C4/C3 precedenti sotto sono storiche.**

**Cincinnus C2 — test nuovi (17/11/2026):** aggiunto `CincinnusSimplePlacementTests.cs` (12 casi); la C1 è confermata compilata/testata dall’utente su Windows. C2 richiede gate Release completo e, facoltativamente, filtro `FullyQualifiedName~CincinnusSimplePlacementTests`. Il test statico dell’archivio non sostituisce `dotnet test`. Consultare [QA C2](validation/CINCINNUS_C2_STATIC_QA.md).

> **Nuovo test pack Cincinnus C1:** `tests/GenerativeArt.Algorithms.Tests/CincinnusGeometryTests.cs`. Copre campo generato/esplicito, determinismo, RK4, raggi di curvatura, chiusura, limiti e cancellazione. Per eseguire: `dotnet test .\tests\GenerativeArt.Algorithms.Tests\GenerativeArt.Algorithms.Tests.csproj -c Release --filter FullyQualifiedName~CincinnusGeometryTests`. **Test scritti ma non ancora eseguiti sulla C1 in questa consegna**: SDK .NET non disponibile; eseguire il gate Windows completo prima di dichiarare la C1 validata. Ultima baseline confermata G4-R2.

# Verifica corrente — G4-R2 (17/11/2026)

**Conferma dell'utente:** la G4-R2 **compila e supera i test Windows**. Non è stato comunicato il numero di test; non riportare un conteggio inventato. Le verifiche statiche pre-conferma in `validation/M4E3_G4_R2_STATIC_VALIDATION.md` sono storiche, mentre la revisione H1 cambia esclusivamente la documentazione e non richiede una nuova build per stabilire l'identità dei sorgenti.

Da PowerShell 7 nella root, per ogni futura modifica al codice:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

**Ancora da documentare:** confronti visivi R1/R2 Full HD su Ocellus/Rete, verifica 100% della tessitura/membrana, smoke reale CLI/GUI di questi nuovi percorsi, benchmark 4K/8K e piattaforme non Windows. I test automatici non equivalgono a giudizio estetico. [Piano QA](ROADMAP.md), [audit](AUDIT_HANDOFF_G4_R2.md).

---

## 17/11/2026 — M4E.3-G4-R2: Ocellus interstizi e rilievo; Rete nodi e pellicole

**Baseline certificata dall'utente:** G4-R1.2 compila e tutti i test passano su Windows. Il blocco G4-R2 prosegue da quella versione, mantenendo integralmente Crinis R10 e gli hotfix R1.1/R1.2.

**R2 implementata nei sorgenti:** Ocellus aggiunge `pore-strength`/`pore-radius` e `rim-light`/`rim-width`; Rete aggiunge `junction-strength`/`junction-radius`, `node-glow` e `membrane-opacity`. I nuovi controlli sono opzionali e neutri per vecchie ricette JSON v1; i **preset iniziali Studio** dei due generatori sono invece espressamente aggiornati per mostrare gli effetti.

**Catalogo:** 29 generatori, 29 preset incorporati, 173 ricette JSON (6 nuove in `examples/recipes/m4e3-g4-r2/`); 17 preset personali e tutti gli altri generatori invariati. Nuovi test Algorithms/Application/Rendering, note e comparazioni nella [scheda G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md).

**Gate aperto:** la R2 richiede ancora build Release e test .NET su Windows (`clear && dotnet clean && .\tools\validate.ps1`) e verifica visiva A/B. Dopo il gate decidere se proseguire G4-R3 o passare a G5. Compositore B, workflow artista D, animazioni evolute più avanti.

---

## Gate M4E.3-G4-R1.2 — CS0136 renderer Rete (17/11/2026)

**Ultimo log Windows:** Algorithms.Tests e Application.Tests hanno compilato; Rendering.Skia si è fermato su CS0136 nella doppia definizione locale `ink`. R1.2 introduce `wallCoverage` e `wallArgb` con formule identiche; conserva il fix numerico R1.1. Non sono ancora disponibili risultati validi dell'intera suite per G4-R1.2.

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

**Risultato atteso:** compilazione Release dell'intera solution senza CS0117/CS0136 e tutti i test verdi. I test G4 esistenti (`GartG4RenderingTests`) verificano Rete con palette, pareti e coperture variabili, determinismo, serializzazione, finiture e cancellazione. Dopo il gate eseguire smoke visivo Ocellus/Rete nativi ed export.

---

## Gate M4E.3-G4-R1.1 — correzione CS0117 (17/11/2026)

**Esito G4-R1 Windows:** build fallita su 4 chiamate `Math.Hypot` nei due kernel G4, prima della validazione globale.

**Fix R1.1:** funzione stabile `GeometryDistance.Hypotenuse` e sostituzione dei 6 usi in produzione; il test brute-force usa `Math.Sqrt` per non dipendere dalla funzione verificata. Nuova classe `GeometryDistanceTests` per precisione, underflow/overflow e input non finiti.

**Verifiche attese su Windows PowerShell 7:**

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Risultato richiesto: build di Algorithms, Rendering.Skia e dell'intera solution senza CS0117, tutti i test verdi. In caso di altri errori riportare il primo. La suite R1.1 **non è ancora stata eseguita su Windows**.

---

**Stato QA 17/11/2026 — M4E.3-G4-R1:** nuova suite `GartG4GeometryTests`, `GartG4CatalogTests`, `GartG4RenderingTests` e 2 nuovi casi `StudioExactPreviewRenderingTests`. Test e build Windows G4 ancora da verificare. Eseguire `clear && dotnet clean && .\tools\validate.ps1` da PowerShell 7 nella root; consultare [scheda G4](M4E3_G4_OCELLUS_RETE.md) per smoke di CLI/Studio.

---

## M4E.3-G3-R10 — test aggiunti, verifica Windows aperta (17/11/2026)

L'utente conferma R9 build/test riusciti. R10 aggiunge test per 4 controlli opzionali Crinis, JSON precedenti, differenza di calibro, PNG storici, disegno a tre strati, seed, PNG leggibile e cancellazione. Ricette in `CrinisR10Recipes` copiate nell'output del progetto Application.Tests.

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Da confrontare anche due PNG indaco di uguale seed `7775761298886397793`: `examples/recipes/m4e3-g3-r10/crinis-r9-originale-utente.json` e `crinis-indigo-r10-tridimensionale-1920.json`. **R10 non ancora eseguita nella suite Windows**; verifiche statiche/JSON non equivalgono ai test .NET. Scheda: [R10](M4E3_G3_R10_CRINIS_PROFONDITA_INTERNA.md).

---

## G3-R3 — nuovi test Crinis

- `CrinisMicroRenderingTests`: invisibilità del fondo e delle ombre poligonali a opacità zero (`classic` e `micro`), modifiche dei controlli, seed e determinismo, budget preventivi, JSON, annullamento, preview/export identici.
- `CrinisMicroCatalogTests`: preset Studio aggiornato, vecchie ricette con nuovi parametri mancanti e validazione stretta di tutti i quattro JSON G3-R2 e dei sei JSON G3-R3.
- Da controllare su Windows dopo compilazione: due rendering 2000×2000 `crinis-cincinnus-*.json`, tempi, qualità e assenza di fondi opachi. Ultima G3-R2 compilata/testata secondo feedback utente; non attribuire l'esito alla R3.

# M4E.3-G3-R1 — Fascia/Eclectic + correzione catalogo (17/11/2026)

- Ricevuto esito G2-R2: **1040/1041 test**; unica regressione `CatalogTests.CatalogHasStableSortedIdsAndLoopCapability` dovuta a un ordine alfabetico atteso invertito (`crinis` / `cotton`). Corretta **solo la lista di attesa**, in entrambe le coppie invertite (`cotton`/`crinis`, `spaced`/`spiral`); non l'ordinamento del prodotto.
- A3-G3: due generatori `fascia-ribbons` e `eclectic-trails` (campi vettoriali, nastri retinati, crescita con collisioni), otto nuove ricette, due preset embedded. Totale **27 algoritmi / 136 ricette**. Tutti i preset e le ricette precedenti preservati.
- Test nuovi per geometria, serializzazione, cambi di parametri, seed, preview/export, fine-2x, finiture, limiti e cancellazione. Guida `M4E3_G3_FASCIA_ECLECTIC.md`.
- **G3-R1 da validare su Windows**. Fase successiva G4 Ocellus/Rete; A1 e A4/A5 restano aperte; B/D dopo e animazioni rinviate.

---

# Validazione — M4E.3-G2-R2 (17/11/2026)

**Esito Windows G2-R1:** build interrotta dal diagnostico dell’analizzatore `xUnit2020` nel test `GartBrushCatalogTests.EightGartBrushStudiesRemainValidJsonV1` (riga 42). I progetti di produzione compilati nel log non dimostrano che l’intera suite sia stata eseguita.

**Fix G2-R2:** sostituzione `Assert.True(false, ... )` → `Assert.Fail(...)`, senza variazioni alla logica della prova. Controllata staticamente l’assenza dello stesso antipattern nei sorgenti C#. **Non dichiarare test R2 passati finché non viene eseguito il gate Windows.**

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Risultato atteso: nessun `xUnit2020`; completamento di compilazione e test. In caso di fallimento, conservare il primo errore e il riepilogo.

---

# Stato test — M4E.3-G1-R1 (17/11/2026)

L'utente ha confermato la **compilazione e il superamento dei test della M4E.2-R5**. Nella presente M4E.3-G1-R1 sono stati aggiunti tre generatori e test dedicati, **non ancora eseguiti su Windows**.

Avviare da PowerShell 7, dalla cartella `GenerativeArt` che contiene la solution:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per lo smoke di un nuovo PNG:

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e3-g1\rigor-letterpress-1920.json .\rigor-letterpress.png
```

Regressioni aggiunte: `ModularTileGrammarTests`, `GartModularRenderingTests`, `GartG1RecipeFixturesTests` / `GartG1NineExamplesTests`. Prestare attenzione a output, colori Auto e finiture, ricette e cancellazione. Non eseguire la ricetta 8K con i nuovi generatori: non sono ancora abilitati.

---

## M4E.2-R5 — Validazione dopo errore di compilazione CS0618

R4 falliva sul metodo deprecato `SKCanvas.DrawImage` in `ArtisticScanlineFinisher.cs`. R5 applica il metodo con `SKRect` e `SKSamplingOptions(SKFilterMode.Nearest)` e aggiunge il test `ScanlinePngCopiesSourcePixelsWithoutResampling` in `ArtisticScanlineFinisherTests.cs`. Il test verifica tutti i pixel della copia nativa prima/dopo PNG. La build e la suite della R5 sono da eseguire su Windows, senza attribuirle esiti già confermati.

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

---

## M4E.2-R4 — Test nuovi da confermare su Windows

Unit test equivalenza delle finiture su bitmap opache, alpha preservation/cancel, validazione 8K e fixture JSON nuove. Smoke manuale 8K con `examples/recipes/m4e2-r4/voronoi-cotton-pigment-8k.json`. Baseline R1 confermata, R2/R3/R4 ancora aperte. Comando: `clear && dotnet clean && .\tools\validate.ps1`.

# Test, build e smoke test — GenerativeArt

## M4E.2-R3 — Fase A3, due nuovi generatori (da validare su Windows)

La R3 introduce le suite `PatternGeometryTests`, `NaturePatternRendererTests` e `NaturePatternCatalogTests` (20 generatori, 108 ricette). I test coprono seed, colori, finiture, campionamento 2×, limiti di iterazione, cancellazione e JSON. Lo Studio ha anche la modifica dei pulsanti di navigazione a larghezza uniforme, da verificare manualmente in una finestra ridimensionata con ricette recenti e filtri attivi. Comando PowerShell 7 dalla radice: `clear && dotnet clean && .\tools\validate.ps1`. **Nessuna esecuzione Windows è stata confermata per R3; ultima baseline confermata dall’utente M4E.1-R3.** Guida: `M4E2_R3_NATURA_E_NAVIGAZIONE.md`.


## M4E.2-R2 — Budget 8K geometrico (17/11/2026)

Aggiunti `RasterCanvasBudget8KTests` e `EightKRecipeFixturesTests`: preflight 7680×4320 su quattro generatori, rifiuto dei renderer con buffer completi, del formato quadrato oltre area, di supersampling/finiture a 8K; validazione JSON v1 di quattro nuovi studi e roundtrip. La suite non alloca raster 8K. **Smoke 8K reale opzionale** e confronto Preview/Export descritti in [`M4E2_R2_8K_GEOMETRICI.md`](M4E2_R2_8K_GEOMETRICI.md). Baseline Windows confermata R3; R2 non ancora validata.

**Ultimo riscontro utente (17 novembre 2026):** M4E.1-R3 compila e supera i test Windows; anteprime visualizzate correttamente, output PNG uguale all’anteprima nella prova eseguita. **M4E.1-R4 e R5**: validazione Windows ancora da eseguire.

## M4E.1-R5 — Servizio documenti e dieci ricette sperimentali

**Ultima baseline confermata dall'utente: R3, compilazione e test passati, preview/export identici.** R4 e R5 non ancora validate su Windows.

Aggiunti sette test `StudioRecipeDocumentsTests` (import limitato anche con stream non seekable, JSON invalido, validazione prima dell'uso, cancellazione, stream, scrittura atomica) e un test delle dieci ricette `ExperimentalRecipeFixturesTests`. Le nuove ricette vengono copiate nell'output test tramite MSBuild, senza usare percorsi assoluti.

**PowerShell 7 dalla radice solution:**

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Smoke suggerito: import/export JSON da Studio, apertura di una ricetta in `examples/recipes/m4e1-r5/`, preview/export nativa, stato condizionale dei controlli R4. Test e verifica grafica devono essere confermati sulla piattaforma di riferimento.

---

## M4E.1-R3 — Correzione puntuale test streamlines (17/11/2026)

**Risultato ricevuto per M4E.1-R2:** build OK, **879/880 test**. Unico fallimento `ArtworkEvolutionTests.EveryArtisticControlChangesTheRaster` per `spaced-streamlines/frequency`: il preset utente seleziona `vortex`, un campo che non usa quel parametro.

**R3:** test aggiornato per usare `field=curl` nella verifica di `frequency`; nuova prova che `vortex` non cambia con la frequenza. Nessuna modifica al renderer. La suite R3 è **da validare**; il numero 879/880 non è un risultato R3. Istruzioni e spiegazione: [`M4E1_R3_FIX_TEST_STREAMLINES.md`](M4E1_R3_FIX_TEST_STREAMLINES.md).

Verifica PowerShell 7 (radice solution):

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

---

## M4D.4-R1 — SDF organico e antialiasing verificati

**Codex Windows, SDK 10.0.201: tools/validate.ps1 completato, build Release zero avvisi/errori e 859/859 test** (Core 115, Algorithms 175, Application 135, Rendering.Skia 434). Tutte le 81 ricette validate; otto nuovi PNG Full HD, due fine-2x. Studio: 36 slider SDF, palette/ruoli colore e scelte, nuovi default e ripristino JSON. CLI fine-2x identica al runtime headless; quindici PNG precedenti identici.

Test numerici di copertura, tratti subpixel conservativi, campo seeded e pesi cromatici normalizzati, separazione colore/geometria, alpha e pigmenti trasparenti, controllo dei budget/cancellazione. AA contro riferimento 4× con e senza warp; nel caso registrato errore medio alpha 14,35 vs 49,93 della soglia (−71,3%). Guida [`M4D4_SDF_MACCHIE_FUSIONI.md`](M4D4_SDF_MACCHIE_FUSIONI.md), evidenze `validation/M4D4_R1_*`. Tool: `sdf`, `sdf-studio`, `sdf-compare`, `sdf-aa` e `depth-compat`.

Restore da cache NuGet locale, pacchetti sotto artifacts/nuget, audit online e telemetria build disattivati nell'ambiente Codex; nessuna configurazione globale modificata. Desktop/DPI reali, altri OS e nuovi poster 4K restano da verificare. Gli stati sotto sono storici.

## M4D.3-R1 — Profondità verificata

**Codex Windows, SDK 10.0.201: tools/validate.ps1 completato, build Release senza avvisi/errori e 801/801 test** (Core 115, Algorithms 168, Application 134, Rendering.Skia 384). Dieci nuovi PNG Full HD, due fine-2x, tutte le 73 ricette validate; confronto degli effetti a seed/risoluzione uguali. Smoke Studio: due nuovi artwork, paesaggi e Domain Warp, slider/colori/scelte, default e ripristino JSON, input numerico esatto. Quindici PNG dei generatori precedenti identici a seed 42 / 160×120. Log e dati `validation/M4D3_R1_*`. Guida e comandi `depth`, `depth-studio`, `depth-compare` e `depth-compat` in [`M4D3_PROFONDITA.md`](M4D3_PROFONDITA.md).

Alpha del blur, ombra direzionale, opacità del gruppo, aperture e ripristino del canvas sono coperti da test nativi Skia; budget rifiutati prima dell'output e cancellazione verificati. Restore locale da cache NuGet con pacchetti sotto artifacts/nuget, audit online e telemetria build disattivati nell'ambiente Codex. Desktop/DPI reali, nuovi poster 4K, altri OS e animazioni lunghe restano da verificare. Gli stati sotto sono storici.

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
