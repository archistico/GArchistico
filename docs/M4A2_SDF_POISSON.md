# M4A.2-R1 — Signed Distance Fields, CSG e Poisson disk

**Data:** 16 novembre 2026. **Stato:** implementata, build e suite Windows ancora da eseguire.

## Baseline verificata

L'utente ha confermato **M4A.1-R4 funzionante** dopo la correzione dei limiti per la stampa. M4A.1-R4 contiene sei opere, topografia con Marching Squares, export PNG fino a 4000×4000 se entro il budget, Studio Avalonia, pan/zoom, tema/preferiti, ricette JSON v1 e CLI. Nessuna parte di queste funzionalità è stata sostituita.

## Algoritmo 1 — SDF 2D e CSG

Nuovo modulo puro C# `GenerativeArt.Algorithms/Geometry/SignedDistance2D.cs`:

- `Circle`, `Box`, `RoundedBox`: distanze con segno, negative all'interno, positive all'esterno.
- `Rotate`: trasformazione 2D senza dipendenza dalla grafica.
- `Union`, `Intersection`, `Difference`: composizione CSG in termini di distanza.
- `SmoothUnion`: fusione morbida polinomiale con ampiezza `k > 0`.
- Validazioni su dimensioni, raggi, angoli e parametro di fusione.

Opera **`sdf-orbits`**, algoritmo v1, categoria *Geometria SDF*: forme seedate, fusione morbida tra cerchi e rettangolo arrotondato, foro per sottrazione, limite circolare per intersezione, anello e fasce equidistanti. Parametri `shape-scale` (0.5–1.3), `smoothness` (0.002–0.2), `bands` (4–25), `edge-width` (0.002–0.025), `palette` (`mineral`, `ember`, `iris`). Opera statica, supportata da CLI e Studio senza nuova UI specifica.

## Algoritmo 2 — Campionamento Poisson disk

Nuovo modulo puro C# `GenerativeArt.Algorithms/Sampling/PoissonDiskSampler.cs`:

- Variante Bridson: lista attiva, griglia spaziale accelerata e candidati casuali nell'anello `[r, 2r]`.
- `SplitMix64Random` del Core per determinismo sullo stesso runtime.
- Distanza minima fra i centri controllata nella griglia locale; zero sovrapposizioni della distanza minima geometrica.
- Budget: fino a **20.000 punti** nell'algoritmo, **2.000.000 celle** nella griglia, **1–60 tentativi** per punto attivo. Annullamento durante la generazione. `Validate` controlla il budget senza allocazioni.

Opera **`poisson-stippling`**, algoritmo v1, categoria *Campionamento*: punti disegnati in diverse palette e colorati mediante campo Simplex seedato. Parametri `spacing` relativo al lato minore (0.012–0.1), `max-points` (100–12.000), `dot-size` (0.5–5), `palette` (`paper`, `night`, `cobalt`). Per questo artwork, dimensione minima 64×64; il limite massimo statico globale rimane **4096×4096**. Il budget sul numero di punti evita che la qualità di stampa si traduca in generazione incontrollata.

## Integrazione, compatibilità e memoria

- Il catalogo sale da **6 a 8 generatori**. Gli ID sono parte del contratto e sono coperti dai test esatti e ordinati.
- `ArtworkRenderEngine.Validate` applica le regole prima di creare la superficie/output; `Render` usa i nuovi renderer headless `SdfOrbitRenderer` e `PoissonStippleRenderer`.
- Entrambe le opere conservano formato ricetta JSON **v1**, `algorithmVersion=1`, seed UInt64 stringa, PNG standard. Nessuna migrazione dei vecchi preset.
- Studio esistente legge i parametri dinamici dal catalogo; anteprima resta ridotta dalla `StudioPreviewPolicy`; export mantiene cancellazione, scrittura atomica locale e percorso della CLI senza sovrascrittura.
- SDF usa una superficie Skia e un `PixelBuffer` temporaneo dell'intera immagine; Poisson usa superficie Skia e griglia/punti bounded. Non si tratta ancora di un renderer tiled o di output vettoriale. Un PNG 4000×4000 può richiedere memoria significativa e tempo di elaborazione: misurare nel gate Windows.

## Test di regressione aggiunti

`SignedDistance2DTests` controlla valori analitici, segni, rotazione, CSG, continuità elementare e input invalidi. `PoissonDiskSamplerTests` verifica distanze quadratiche di tutte le coppie in un campione di test, confini, seed, budget e cancellazione. `CatalogTests` verifica otto ID, ordinamento e capacità raster. `ArtworkRenderEngineTests` verifica PNG decodificabili, determinismo, effetti dei parametri, JSON v1, cancellazione e validazione dei 4000×4000 senza creare un PNG gigante nei test.

## Comandi PowerShell 7 — dalla cartella `GenerativeArt`

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\sdf-orbits.json .\output\sdf-orbits-m4a2.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\poisson-stippling.json .\output\poisson-stippling-m4a2.png
```

Per i due poster SDF e Poisson, usare **destinazioni nuove**:

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\sdf-orbits-poster-4000.json .\output\sdf-orbits-poster-4000.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\poisson-stippling-poster-4000.json .\output\poisson-stippling-poster-4000.png
```

## Criteri del gate M4A.2

1. Build Release e suite xUnit complete; `list` deve riportare otto generatori, senza cambiare gli ID precedenti.
2. CLI: PNG SDF e Poisson apribili, seed ripetibile, parametri differenti con effetti visibili; poster SDF 4000×4000 realmente esportato.
3. Studio: le due nuove opere sono selezionabili, modificabili, possono entrare nei preferiti; pan/zoom e tema invariati.
4. Misurare memoria e tempo soprattutto su SDF 4000×4000; annullare un export grande e verificare assenza di `.partial`/file danneggiati.
5. Testare almeno una ricetta JSON precedente, `topographic-contours` 4000×4000 e Play/Pausa `looping-flow` per regressioni.

**Non certificato in questa consegna:** build .NET (SDK assente nell'ambiente di preparazione), PNG effettivi o esperienza visiva Windows; test e codice predisposti non equivalgono a risultati eseguiti.

## Prossime tappe

Raffinamento estetico dei due generatori dopo smoke visuale; tassellazioni/voronoi con spatial index, noise avanzato (derivate, warping), animazioni cicliche e export streaming GIF/APNG. L'SDF e Poisson sono algoritmi riutilizzabili, non equivalgono alla chiusura dell'intero programma M4.
