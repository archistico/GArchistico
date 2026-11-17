# Handoff rapido — M4E.1-R3 (17 novembre 2026)

**Base di ripresa:** ZIP completo **M4E.1-R3**, non R2. Leggere `CURRENT_STATE.md`, `ROADMAP.md`, `M4E1_R3_FIX_TEST_STREAMLINES.md` e `M4E1_R2_PREVIEW_NATIVA_COORDINATORE.md`.

- **Esito Windows R2 fornito dall'utente:** build riuscita, **879/880 test**, solo `spaced-streamlines/frequency` fallito in `ArtworkEvolutionTests.cs:145`.
- **Root cause:** i 17 preset Studio contengono `spaced-streamlines` con `field=vortex`, campo matematico indipendente da `frequency`; il test `EveryArtisticControlChangesTheRaster` ereditava quel preset. Con `field=curl` la frequenza controlla realmente il campo. Corretto il test per questa combinazione, aggiunta regressione che fissa la semantica di `vortex`.
- **Impatto:** nessun cambiamento di codice runtime, GUI, seed, preset, ricette o PNG. **R3 test Windows da eseguire**; il risultato 879/880 si riferisce esclusivamente alla R2. Nessuna dichiarazione di successo anticipata.
- **Prossimo:** riprendere Fase A1 controller/headless e UX condizionale dei parametri inapplicabili, A2 QA native preview, A3 nuovi algoritmi/ricette e A4 8K, quindi B compositore e D workflow. Animazioni avanzate rinviate.
- **PowerShell 7 dalla radice solution:** `clear && dotnet clean && .\tools\validate.ps1`.

---

# Handoff precedente — M4E.1-R2 (17 novembre 2026)

**Aprire prima** `CURRENT_STATE.md`, `ROADMAP.md`, `M4E1_R2_PREVIEW_NATIVA_COORDINATORE.md` e `M4E0_PIANO_A_B_DEFAULTS.md`.

- Base: **M4E.1-R1** (refactoring iniziale `StudioWindow`, 17 JSON embedded dei generatori, Fase D in roadmap). **R2** introduce `StudioRenderCoordinator` nel progetto Application, condiviso tra preview ed export, con gate unico e test headless.
- **Anteprima R2**: sempre alle dimensioni finali. Rimossi i campi «Raster max» e il pulsante «Raster = output»; `Adatta`, `100%`, pan e zoom agiscono sulla vista. Se budget insufficiente, errore esplicito; nessun fallback raster. Renderer 8K **non** ancora implementato.
- **Prossimo sviluppo**: A1 completare controller indipendenti e smoke lifecycle/headless; A2 verificare GUI, DPI, pixel/alpha reali e cancellazione; **A0** resta aperta per 52 immagini storiche assenti; poi **A3** nuovi algoritmi e ricette, **A4** 8K e qualità, **A5** QA.
- **Fasi future**: B compositore universale, D varianti/seed, A/B, galleria, contact sheet, preset, SVG selettivo; animazioni evolute rinviate.
- **Verifiche allora disponibili:** M4D.4-R1: 859/859; successivamente l'utente ha eseguito R2 su Windows, ottenendo 879/880 (dettagli nella sezione R3 sopra).
- **Vincoli**: C#/.NET 10, Avalonia/Skia, minimo PowerShell, niente Python/Go/Kotlin/Java nel progetto; ZIP completo, preset e ricette originali invariati, README conciso, JSON v1 riproducibile.
- **PowerShell 7, dalla radice**: `clear && dotnet clean && .\tools\validate.ps1`; avvio con `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.

---

# Handoff rapido — M4E.1-R1 (17 novembre 2026)

**Aprire prima** `CURRENT_STATE.md`, `ROADMAP.md`, `M4E0_PIANO_A_B_DEFAULTS.md` e `M4E1_A1_WORKFLOW_D.md`.

- Base della consegna: M4D.4-R2 con 17 JSON embedded; nuovo refactoring **A1 primo passo**: `StudioWindow.cs` 189 righe, 7 partial per ambito, `StudioRecipeFormParser` in Application e test xUnit. Nessuna variazione artistica/UI deliberata.
- Baseline Windows storica: **M4D.4-R1 859/859**; R2 e M4E.1-R1 non ancora validate. Non dichiarare i nuovi test superati senza esecuzione.
- Prossimo sviluppo **A1-R2**: isolare controller preview/export con contratto di threading, cancellazione e test headless; poi **A2** preview realmente 1:1 output, **A3** nuove ricette/algoritmi, **A4** 8K, **A5** QA. **A0** 52 immagini storiche mancanti rimane aperto.
- Dopo A: **Fase B** compositore, poi **Fase D** workflow dell'artista (variazioni, seed explorer, A/B, galleria, contact sheet, preset, SVG selettivo). Animazioni in seguito.
- Vincoli: C#/.NET 10 + Avalonia/Skia, solo minimo PowerShell, ZIP completo ogni consegna, no Python/Go/Kotlin/Java nel progetto, JSON v1/seed/GUI invariati, README pulito, test Windows in italiano.
- Comandi: `clear && dotnet clean && .\tools\validate.ps1`; `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.

---

# Handoff — GenerativeArt Studio M4D.4-R2 (17/11/2026)

**Partire dal progetto completo M4D.4-R2, non dalle precedenti versioni R1.** È stata accettata la roadmap in due blocchi: **Fase A** (distribuzione, modularizzazione `StudioWindow`, anteprima nativa identica all'output, nuovi algoritmi/ricette, output 8K/qualità e benchmark) **prima** della **Fase B M4C.3** (compositore universale con livelli, maschere e blend). **Animazioni rinviate**.

**Fatto in R2:** incorporati come risorse dell'assembly i 17 file JSON forniti dall'utente (`src/GenerativeArt.Application/Defaults/`); l'inizializzazione dei nuovi lavori Studio usa seed, dimensioni, eventuale animazione e tutti i parametri/colori degli allegati, non solo i precedenti 7 override. Le ricette importate conservano i loro valori e il catalogo non cambia. Test aggiornati per i nuovi preset. Verifica Windows **ancora da eseguire**; baseline M4D.4-R1 con 859/859 test. Nessuna modifica ai renderer: il prodotto non è ancora 8K e l'anteprima non è ancora obbligatoriamente nativa.

**Problema preesistente in A0:** le 52 immagini di anteprima referenziate nel vecchio manifesto erano assenti nell'archivio ricevuto. R2 include tutti i sorgenti ricevuti e un manifesto nuovo coerente, ma non ricrea queste immagini. Recuperarle o rigenerarle con il motore a milestone A0.

**Prossima attività di codice:** A1 modularizzazione minima senza modificare UI, quindi A2 preview fedele; la roadmap approvata è in `docs/M4E0_PIANO_A_B_DEFAULTS.md` e `docs/ROADMAP.md`. A ogni consegna riportare stato e prossimo passo, ZIP completo e comandi Windows in italiano. Solo C#/.NET 10 + Avalonia/SkiaSharp + PowerShell minimo; nessun Python/Go/Kotlin/Java nel progetto.

**Validazione Windows (PowerShell 7, radice solution):** `clear && dotnet clean && .\tools\validate.ps1`.
**Avvio:** `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.

---

# Handoff — GenerativeArt Studio M4D.4-R1 (16/11/2026)

## Base di partenza, stato e preferenze

**ZIP corrente:** `GenerativeArt_M4D4_R1_SDF_MACCHIE_FUSIONI_COMPLETO.zip`, integrale. **M4D.4-R1 — Orbite SDF riprogettato:** macchie organiche, fusione di quattro pigmenti, venature, rilievo, piani e antialiasing locale. **17 generatori, 81 ricette, build Release senza avvisi/errori e 859/859 test Windows** (Codex, SDK 10.0.201). Otto nuovi PNG Full HD, due fine-2x; controlli Studio, JSON, export CLI e 15 PNG precedenti identici verificati. Confronto AA contro riferimento 4×: errore alpha ridotto del 71,3% nel caso registrato. Guida [`M4D4_SDF_MACCHIE_FUSIONI.md`](M4D4_SDF_MACCHIE_FUSIONI.md), evidenze `docs/validation/M4D4_R1_*`. Desktop/DPI reali, altri OS, nuovi poster 4K ed editor livelli/maschere/blend restano aperti. Restore da cache NuGet locale, audit online e telemetria build disattivati. L'utente vuole macchie/fusione colore, complessità, controllo e migliore antialiasing.

## Novità M4D.4-R1 — Orbite SDF riprogettato

- `PigmentSdfField`: unione morbida di ellissi seeded, domain warp e quattro pesi cromatici normalizzati. `SdfPixelCoverage`: copertura locale di bordo/tratto, conservazione dell'energia dei tratti subpixel.
- `SdfPigmentRenderer`: stili pigment/glass, tre layout, quattro pigmenti con alpha, miscela RGB lineare o logaritmica approssimata, marbling, rilievo/riflessi, ombre gaussiane e blur per piano.
- `SdfPigmentControls.All` centralizza 31 nuovi controlli opzionali nei JSON v1. I default del catalogo restano classic; nuovi lavori Studio pigment/sorbet, 14 macchie per piano e due piani. Ripristino JSON conservativo. Ruoli colore vecchi riutilizzati, più pigmento 4/ombra; sei palette.
- Antialiasing anche standard, footprint da differenze centrali su tre righe, nessun campo float a pieno canvas. Fine-2x ammesso sui nuovi default Full HD. Budget SDF 260 milioni di valutazioni (incluse righe/colonne esterne), pixel nativi ≤ 8.388.608 più DepthLayerPolicy, prima di superfici/output.
- Otto ricette Full HD, due fine-2x, 81 ricette totali; screenshot Studio e confronti artistico/AA. Build Release e 859/859 test. Otto PNG, Studio (36 slider SDF), CLI fine-2x e 15 hash PNG precedenti verificati.
- Guida `M4D4_SDF_MACCHIE_FUSIONI.md`, evidenze `docs/validation/M4D4_R1_*`. Modi tool `sdf`, `sdf-studio`, `sdf-compare`, `sdf-aa`; `depth-compat` conserva la verifica degli hash precedenti. M4C.3 editor/maschere/blend e noise 4D restano futuri.

## Novità M4D.3-R1 — Profondità

- `OrganicAperture` e `DepthParticleField`; artwork `paper-cut` e `bokeh-depth`. Catalogo a 17, dieci nuove ricette Full HD, 73 totali, due fine-2x e confronto effetti spenti/accesi.
- `SkiaRasterSurface.DrawLayer` e `LayerEffects`: blur gaussiano, drop shadow e opacità SrcOver di gruppo. Un piano filtrato alla volta; nessun annidamento filtrato. Default neutro uguale al disegno diretto. Alpha premoltiplicato e ripristino canvas anche dopo errore/cancellazione.
- Paesaggi a strati: sei nuovi controlli JSON opzionali e neutrali. `soft-shadow > 0` sostituisce le precedenti ombre a tre tratti; atmosfera storica invariata. Nuovi lavori Studio con effetti attivi, import JSON conservativo.
- `DepthLayerPolicy`: massimo 8.388.608 pixel nativi e 800 milioni di unità di lavoro filtrato prima di allocazione/output. Budget aggiuntivi per vertici/copertura dischi. Non rimuoverli per ammettere tutti i massimi degli slider insieme.
- Build Release e 801/801 test, dieci PNG Full HD, tutte le 73 ricette validate, smoke Studio con default/ripristino, quindici PNG precedenti identici. Guida `M4D3_PROFONDITA.md`, log/dati `docs/validation/M4D3_R1_*`. `depth-compat` del tool ripete gli hash dei default storici a seed 42 / 160×120.
- Base di compositing già riutilizzata da tre generatori; M4C.3 resta aperta per editor livelli, maschere arbitrarie e blend configurabili. Nessuna nuova dipendenza di produzione.

## Novità M4D.2-R1 — Pittura e generatori iniziali

- `OrganicStroke` e `FractalProfile1D`; artwork `flow-brush` e `strata-landscape`. Catalogo a 15, quattordici nuove ricette Full HD, 63 totali, anteprime e confronto a pari seed dei primi cinque generatori.
- Estensioni opzionali di Curve parametriche, Noise Flow, Plasma, Streamlines distanziate e Flow Loop. `StudioRecipeDefaults.ParametersFor` applicato solo ai nuovi lavori, mai alle ricette ripristinate. CLI/API conserva i default del catalogo; 13 PNG precedenti identici tramite hash.
- `LegacyArtworkControls` centralizza i default neutrali; renderer storici conservati accanto alle varianti estese. Cambiare solo opacità/spessore nei flussi storici non deve spostare le tracce. FlowTracer cancellabile internamente.
- Build Release e 733/733 test Windows, quattordici PNG Full HD, smoke Studio e sequenza CLI 12 frame verificati. Guida `M4D2_PITTURA_EVOLUZIONE.md`, evidenze `docs/validation/M4D2_R1_*`. Le comparazioni degli hash riguardano seed 42 / 160×120, non tutti i possibili input.
- Nessuna nuova dipendenza di produzione. Compositore generale M4C.3 e noise 4D rimangono futuri; le sovrapposizioni dei paesaggi appartengono al loro renderer.

## Novità M4D.1-R1 — Dettaglio e controllo

- Algoritmi `GuillocheCurve` e `TonalHatching`, artwork `guilloche`/`tonal-engraving`, catalogo a 13; otto ricette Full HD e anteprime, 49 ricette totali.
- Guilloché: rapporto degli ingranaggi, armoniche, curve/precisione, twist e progressione, colori/opacità. Incisione: campi, tratteggio progressivo, angoli, toni e maschere locali. Dimensioni dei tratti riferite a lato minimo 1000 px.
- Dieci controlli Domain Warp opzionali, default neutrali; tre PNG Full HD M4B.1 identici via SHA-256. Mantenere questa compatibilità.
- Slider con passi adatti ai piccoli intervalli, input esatto preservato. Ogni editor ha una revisione: eventi ritardati degli editor rimossi devono essere ignorati.
- Build Release e 626/626 test Windows; otto PNG Full HD e smoke Studio headless verificati. Guida `M4D1_DETTAGLIO_CONTROLLO.md`, log/dati `docs/validation/M4D1_R1_*`.
- Compositore generale M4C.3 e nuovi loop M4B.2 ancora futuri. Nessuna nuova dipendenza di produzione.

## Novità M4B.1-R1 richieste dall'utente

- `DomainWarpNoise2D` e `LineIntegralConvolution` in Algorithms; nuovi artwork `domain-warp` e `lic-flow`, tre palette, quattro colori indipendenti, finiture e fine-2x. Catalogo a 11.
- Sette JSON Full HD e anteprime incluse. Domain Warp: circa 1–1,4 s; LIC: 9,5–12,4 s; streamlines dense: 5,5–15,9 s nel processo locale misurato, senza garanzia su altri PC.
- Streamlines: massimo 10.000, distanza/spessore opzionali; candidati fino a 200.000 distribuiti sul canvas nelle impostazioni estese, cancellazione dentro il tracer. Default storici preservati.
- Picker: footer fisso fuori dallo scroll, dimensioni contenute e altezza limitata all'area utile in DIP. Conferma/annulla verificati headless a quattro altezze; desktop/DPI reali ancora da provare.
- Tool opzionale `tools/GenerativeArt.Smoke`, fuori dalla solution, per ripetere smoke picker/render; Avalonia.Headless dipende solo dal tool. Nessuna nuova dipendenza di produzione.

## Novità M4C.2-R3 (correzioni richieste dall'utente)

- Il test `MissingParametersAreRejected` ora usa `JsonNode` per rimuovere `scale`: il vecchio `Replace` generava JSON invalido con trailing comma e provocava `JsonReaderException` anziché `ArgumentException`. Parser di produzione immutato.
- Limite finitura da 4.194.304 a 16.777.216 pixel. Rimosso anche tetto indipendente di Plasma 4.194.304; 2500×2000 in standard è accettato con materia/grana/dithering. Budget generale 4096 per lato e `fine-2x` con 16.777.216 sottopixel rimangono; altri renderer e loop conservano guard specifici.
- `StudioWindow`: campi di risoluzione raster anteprima (max) e dimensione massima riquadro anteprima (DIP) separati da dimensioni output; pulsante `Raster = output`.
- `PreviewViewport` / `StudioViewportState`: auto-fit dopo un render e dopo resize, pulsanti `Adatta` e `100%`; pan/zoom manuali preservati, no distorsione. Barra di stato mostra dimensioni raster preview e output.
- Aggiunte regressioni su fitting, proporzioni, preview full-resolution e validazione del caso 2500×2000.

## Novità M4C.2-R2 (fix su M4C.2-R1)

- `StudioColorValue.FromHsv`: uso di `0d` per uniformare le tuple di tutti i rami dello `switch` a `double` (CS8506/CS8131).
- `StudioWindow`: `WindowState = Avalonia.Controls.WindowState.Maximized` per aprire lo Studio massimizzato, mantenendo titolo e taskbar.
- `StudioColorValueTests`: sei nuovi casi delle tonalità a metà settore, preservazione RGB e alpha.
- **Nessuna nuova milestone artistica in questa patch**, nessun cambio di output delle ricette o di dipendenze.

## Novità M4C.2-R1

- Quattro controlli facoltativi in sezione **Materia** per ogni artwork (`paper-texture`, `paper-strength`, `pigment-granulation`, `ink-absorption`), default neutri e ricette v1 compatibili.
- `MaterialPostProcessor.cs` in `src/GenerativeArt.Rendering.Skia/`: rumore spaziale interpolato seeded, carte `cotton`/`rough`, pigmento granulare e deposizione/assorbimento localizzato lungo i bordi.
- `ArtisticFinishOptions` esteso senza rompere il costruttore precedente a quattro argomenti; `ArtisticPostProcessor` applica prima il nuovo materiale e poi M4C.1 (grana/dithering) se richiesti.
- Zero variazioni a `SkiaRasterSurface`/CLI/Studio: il percorso comune e i controlli generati dal catalogo rendono il gruppo nuovo accessibile in entrambi.
- Cinque nuovi JSON Full HD, test `MaterialPostProcessorTests`, parametri e controlli `CatalogTests` aggiornati.

## Vincoli importanti

- Stack **C#/.NET 10**, Avalonia (XAML consentito), SkiaSharp, minimo PowerShell; non introdurre Kotlin, Go, Python, Java nel prodotto.
- ID dei diciassette generatori, `algorithmVersion`, schema JSON v1, seed, default cromatici, CLI, preferiti e layout Studio: preservare.
- **README:** solo descrizione del progetto, installazione ed esecuzione, con screenshot sotto il titolo. Changelog, roadmap, risultati dei test e note di consegna restano nei file separati; non reinserirli nel README.
- Finiture dopo downsample a risoluzione finale; alpha invariato; effetti spenti conservano la codifica PNG storica; niente accumulo di frame in memoria.
- Post-processing fino a 16.777.216 pixel finali in modalità standard (limite globale). In fine-2x il limite di 16.777.216 sottopixel implica max 4.194.304 pixel finali; un poster grande può richiedere tempi/RAM elevati.
- Il nuovo assorbimento è un'approssimazione dei bordi, non una simulazione fisica dell'acquerello.

## Prossime attività

1. Estendere i livelli filtrati M4D.3 a editor/maschere arbitrarie/blend configurabili M4C.3, mantenendo output deterministici.
2. M4B.2 noise ciclico e nuovi effetti animati; domain warping e LIC già disponibili in M4B.1. Priorità a loop morbidi e export progressivo nel modello.
3. Sviluppo M4D pennelli organici, M5 export media.
4. All'occasione valida le revisioni recenti: suite, alpha, PNG Full HD, memory budget, confronto default storico, carico CPU e comportamento loop.

## Validazione Windows — PowerShell 7

Aprire la cartella con `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Documenti prioritari: `CURRENT_STATE.md`, `ROADMAP.md`, `M4C2_MATERIA_CARTA_PIGMENTI.md`, `TESTING.md`, `QUALITY_GATES.md`. Conservare l'intero ZIP a ogni evoluzione.
