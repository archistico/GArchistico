# Handoff — GenerativeArt Studio M4D.2-R1 (16/11/2026)

## Base di partenza, stato e preferenze

**ZIP corrente:** `GenerativeArt_M4D2_R1_PITTURA_EVOLUZIONE_COMPLETO.zip`, integrale. **733/733 test e build Release senza avvisi/errori (Codex, Windows, SDK 10.0.201)**. Due nuovi artwork e primi cinque generatori ampliati; 14 ricette Full HD, 12 frame CLI, confronto visuale, smoke Studio e compatibilità byte-per-byte dei 13 renderer precedenti verificati. Guida `M4D2_PITTURA_EVOLUZIONE.md`, dati in `docs/validation/M4D2_R1_*`. L'utente vuole nuovi algoritmi/ricette e generatori iniziali meno basici, con dettaglio e controllo.

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
- ID dei quindici generatori, `algorithmVersion`, schema JSON v1, seed, default cromatici, CLI, preferiti e layout Studio: preservare.
- **README:** solo descrizione del progetto, installazione ed esecuzione, con screenshot sotto il titolo. Changelog, roadmap, risultati dei test e note di consegna restano nei file separati; non reinserirli nel README.
- Finiture dopo downsample a risoluzione finale; alpha invariato; effetti spenti conservano la codifica PNG storica; niente accumulo di frame in memoria.
- Post-processing fino a 16.777.216 pixel finali in modalità standard (limite globale). In fine-2x il limite di 16.777.216 sottopixel implica max 4.194.304 pixel finali; un poster grande può richiedere tempi/RAM elevati.
- Il nuovo assorbimento è un'approssimazione dei bordi, non una simulazione fisica dell'acquerello.

## Prossime attività

1. M4C.3 livelli/maschere/blend controllati dall'utente, mantenendo output deterministici.
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
