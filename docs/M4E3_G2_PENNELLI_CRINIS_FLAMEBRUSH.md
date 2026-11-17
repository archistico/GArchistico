# M4E.3-G2-R1 — Crinis e Flamebrush (17 novembre 2026)

## Obiettivo e fonti

Secondo incremento della roadmap A3-G1…G6, basato sulla lettura dei sorgenti Gȧrt:

- `arts/lines/src/lines/crinis/Crinis.kt` e `gart/src/main/kotlin/dev/oblac/gart/brush/bristles.kt`;
- `arts/bubbles/src/flamebrush/Flamebrush.kt` e `SpiralBrush.kt`.

Gȧrt usa una **spine** con normale e semilarghezza lungo l'arco, su cui scorrono le setole; le bande si separano anche per le ombre. Flamebrush deposita un piccolo pennello spiraliforme in punti successivi di orbite. Questa versione C# implementa le idee matematiche in modo parametrico, non è una copia pixel-per-pixel, né traduce direttamente il codice Kotlin. Per attribuzione/licenza vedere `licenses/GART_BSD2.txt` e `docs/THIRD_PARTY_NOTICES.md`.

## Componenti

- `GenerativeArt.Algorithms/Brushes/BristleSpine2D.cs`: campionamento puro di una curva portante, normale e larghezza variabile; privo di dipendenze Skia.
- `GenerativeArt.Algorithms/Brushes/SpiralStamp2D.cs`: distribuzione deterministica di punti lungo una spirale attorno a un centro.
- `GenerativeArt.Application/Catalog/GartBrushArtworkDescriptors.cs`: due descrittori completi, parametri validati e colori Auto.
- `GenerativeArt.Rendering.Skia/GartBrushRenderer.cs`: due renderer che disegnano sulla superficie Skia comune.
- `ArtworkRenderEngine`: validazione preventiva dei budget e dispatch dei due renderer. Studio e CLI usano la stessa pipeline.
- `ArtworkRolePalettePreview`: tre palette per generatore con ruoli cromatici modificabili singolarmente.

## 24° generatore — `crinis-bristles`

La curva viene costruita con variazioni periodiche lente della curvatura e una componente di richiamo orbitale. Il corpo è un poligono attraversato da filamenti con offset normale indipendente, taper alle estremità, irregolarità controllata e interruzioni deterministiche per l'inchiostro secco. Due passaggi di ombra a bassa opacità distinguono i fasci sovrapposti: è un'approssimazione pittorica, non una sfocatura fisica.

Controlli: numero di fasci, ampiezza, distanza e spessore delle setole, campioni della curva, curvatura, swirl, intensità ombra, secchezza, tre palette, sfondo/tre fibre/ombra personalizzabili, qualità, grana/dithering/carta/pigmento.

Budget: massimo **3 milioni di campioni dei filamenti** per render; limite dimensioni standard già previsto dai renderer (non ancora abilitato 8K).

## 25° generatore — `spiral-flamebrush`

Orbite concentriche percorse con stampi puntinati a spirale. Numero di punti, numero di timbri, fase progressiva, numero di giri, raggio e dispersione cambiano struttura e densità. Seed riproducibile e opacità regolabile. Le singole particelle sono cerchi antialias, non sprite pre-renderizzati.

Controlli: orbite, timbri, particelle, raggio e distanza orbite, raggio pennello, giri, velocità di rotazione, dispersione, diametro e opacità punti, tre palette, sfondo/tre colori personalizzabili e finiture.

Budget: massimo **120.000 particelle** per rendering. Richiede per ora raster standard ≤4096×4096; 8K rimandato a benchmark reali.

## Ricette Full HD

Otto JSON aggiunti in `examples/recipes/m4e3-g2/`:

- Crinis Indigo Twists
- Crinis Copper Woven
- Crinis Forest Relief
- Crinis Silk Fibres
- Flamebrush Blue Whorls
- Flamebrush Ember Coils
- Flamebrush Orchid Cloud
- Flamebrush Mint Orbits

Tutti con 1920×1080 e seed distinti. Nessuna ricetta precedente modificata.

## Contratti e verifiche

**25 generatori / 128 ricette**; 25 default incorporati, inclusi i 17 preset personali invariati. JSON v1 e algoritmi storici invariati. Test aggiunti:

- `GartBrushGeometryTests`: determinismo, geometrie finite, normali unitarie, estremità assottigliate, input invalidi.
- `GartBrushRenderingTests`: PNG deterministico, variazione seed/parametri/colori, fine-2x e materia, cancellazione, rifiuto dei carichi eccessivi.
- `GartBrushCatalogTests`: default embedded, roundtrip JSON, otto ricette v1.
- `CatalogTests` / `StudioBundledDefaultsTests`: inventario da 23 a 25.

La baseline Windows confermata dall'utente è **M4E.2-R5 (compilazione/test passati)**. La precedente **M4E.3-G1-R1** e questa G2 **non risultano ancora validate su Windows**; non attribuire loro esiti positivi senza test.

## Validazione Windows (PowerShell 7)

Dalla cartella che contiene `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Render di esempio CLI:

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e3-g2\crinis-indigo-twists-1920.json .\crinis-indigo.png
```

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e3-g2\flamebrush-ember-coils-1920.json .\flamebrush-ember.png
```

Studio:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

## Prossimi incrementi

- **G2-R2 eventuale:** migliorare clipping, selezione della copertura dei fasci, pressioni e ombre senza alterare i seed esistenti (eventuali algoritmi v2 espliciti).
- **A3-G3:** Fascia, Eclectic e trame con nastri, stroke compositing e retinatura locale.
- Continuare A1 (controller GUI), A4 (ottimizzazione 8K) e A5 (benchmark); poi B (compositore), D (Workflow artista). Animazioni rinviate.
