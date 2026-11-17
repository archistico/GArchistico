# M4E.3-G3-R1 — Fascia ed Eclectic (17 novembre 2026)

## Contesto e problemi risolti

**Gate G2-R2 ricevuto dall'utente:** 1040 test superati su 1041, unico errore `CatalogTests.CatalogHasStableSortedIdsAndLoopCapability`. Il test elencava `crinis-bristles` prima di `cotton-circles`, pur usando il catalogo `StringComparer.Ordinal`. Corretta la lista di attesa: `cotton-circles` precede `crinis-bristles` e `spaced-streamlines` precede `spiral-flamebrush`. L'algoritmo di ordinamento e la UI non sono stati cambiati.

**Sviluppo A3-G3:** due nuovi generatori parametrici originali in C#/.NET 10 ispirati ai concetti dei file Gȧrt `arts/flowforce/src/fascia/Fascia.kt` e `arts/flowforce/src/flowforce/eclectic/Eclectic.kt` / `two/Eclectic2.kt`. Il repository di riferimento è BSD-2-Clause (avviso in `licenses/GART_BSD2.txt`). Non è un porting letterale né un confronto pixel-per-pixel con i PNG originali.

## Funzionalità implementate

### `fascia-ribbons` — Nastri d'inchiostro

- Campo continuo con deriva, vortici locali e ondulazione deterministica.
- Curve integrate tramite punto medio; nastri antialias per segmenti con normale e rastremazione.
- Transizione *discreta* fra due pigmenti (o verso il colore carta) a microcelle di retinatura ancorate alle coordinate e al seed. Non viene interpolato il colore RGB.
- Parametri: `bands`, `steps`, `step-size`, `band-width`, `taper`, `whirls`, `frequency`, `drift`, `waviness`, `fade`, `droplet-size`, `drop-density`, palette `ink/copper/lagoon`, ruoli cromatici, finiture condivise.
- Budget preventivo: fino a 220.000 moduli di retinatura. Dimensione massima 4096×4096 standard; qualità 2× secondo il budget generale.

**Limitazione nota:** la retinatura procede per microcelle trapezoidali, non per un campo SDF per-pixel come nell'implementazione Gȧrt; la resa e le prestazioni andranno confrontate su PNG reali. In particolare, i giunti dei segmenti possono evidenziare sottile sovrapposizione antialias quando la curva è molto piegata.

### `eclectic-trails` — Tracce in collisione

- Crescita delle traiettorie nel campo, con integrazione a punto medio e arresto sui bordi o vicino alle tracce già depositate.
- Indice spaziale a celle per evitare la scansione quadratica di tutti i punti durante le collisioni.
- Tratti a spessore variabile per traccia, controllo della distanza minima, lunghezza minima, opacità e palette `chalk/neon/earth`.
- Budget preventivo: fino a 140.000 passaggi di integrazione.

**Limitazione nota:** l'indice controlla la vicinanza dei campioni alle *tracce precedentemente confermate*, non le auto-intersezioni della stessa traiettoria durante la sua crescita. La prima versione privilegia prestazioni, determinismo e controllo creativo.

## Architettura

- `src/GenerativeArt.Algorithms/Flow/GartFlowGeometry2D.cs`: campo, integrazione e indice spaziale senza Skia.
- `src/GenerativeArt.Application/Catalog/GartFlowArtworkDescriptors.cs`: metadati e controlli Studio, valori numerici validati.
- `src/GenerativeArt.Rendering.Skia/GartFlowRenderer.cs`: palette, geometria di nastri e tracciatura.
- `ArtworkRenderEngine`: validazione, cancellazione e passaggio a PNG nella pipeline condivisa anteprima/export.
- `ArtworkRolePalettePreview`: campioni effettivi per i ruoli Auto dei due nuovi algoritmi.
- Due preset iniziali embedded, otto ricette Full HD in `examples/recipes/m4e3-g3/`.

## Compatibilità

**27 generatori, 136 ricette, 27 preset embedded** (di cui 17 personali originali). Tutti i 25 preset e 128 esempi della precedente G2-R2 sono conservati senza modifiche; nessuna modifica ai renderer G1/G2 o alle ricette storiche.

## Regressioni

- Correzione ordinamento ID in `CatalogTests`, conservando l'elenco esplicito e ordinato.
- Aggiornamento conteggio del catalogo a 27, dei controlli Finitura/Materia e delle ricette embedded.
- Nuovi test `GartFlowGeometryTests`, `GartG3CatalogTests`, `GartG3RenderingTests` per coordinate, collisioni, determinismo, seed, palette, PNG, JSON v1, 2×/finiture, cancellazione, budget e immagini.
- Test dell'anteprima nativa identica all'export esteso a entrambi i generatori.

**Stato gate:** G2-R2 (utente) 1040/1041, unico test ordinamento; G3-R1 compilazione e test ancora da confermare. Non attribuire risultati non eseguiti.

## Test Windows

Aprire PowerShell 7 nella cartella principale `GenerativeArt`. Il comando pulisce bin/obj, compila Release e lancia la suite completa:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Smoke CLI facoltativo (il PNG destinazione non deve esistere):

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e3-g3\fascia-ink-spray-1920.json .\fascia-prova.png

dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e3-g3\eclectic-neon-rivers-1920.json .\eclectic-prova.png
```

In Studio: selezionare Fascia ed Eclectic, modificare palette, seed, numero di nastri/tracce e verificare `Adatta`, `100%`, controlli Auto, dimensioni preview originali ed export identico.

## Roadmap successiva

G3-R2: confronto qualitativo e profiling raster, miglioramento retinatura/percorso in caso di artefatti. G4: Ocellus e Rete (SDF e tessiture cellulari); G5: Accretion/Citymap; G6: Nimbus/volumi. Restano A1, A4/8K, A5. Fase B compositore e Fase D workflow artista dopo; animazioni avanzate in futuro.
