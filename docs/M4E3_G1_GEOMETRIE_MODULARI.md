# M4E.3-G1 — Geometrie modulari Gȧrt (17 novembre 2026)

## Scopo

Primo incremento A3-G1 derivato dallo studio diretto dei sorgenti originali Gȧrt. Tre generatori **autonomi**, parametrici e riproducibili in C#/.NET 10:

| Nuovo generatore | Riferimento sorgente Gȧrt | Astrazione portata nel framework |
| --- | --- | --- |
| `alien-letters` | `arts/alien/src/alien/AlienLetter.kt` (variante V3) | Griglie di glifi da sottocelle, rettangoli e barre estese |
| `cotton-circles` | `arts/cotton/src/cotton/circles/CottonCircles.kt` | Quadrati con archi e orientamenti a 90°, reticolo variabile |
| `rigor-grid` | `arts/igor/src/rigor/Rigor.kt` | Vocabolario di tasselli periodici, quattro lettere IGOR, deformazione locale |

Il progetto **non riproduce bit-a-bit** la sequenza casuale Kotlin, lo shader `NoiseColor` o l'immagine originale: le opere sono reinterpretazioni algoritmiche configurabili. La generazione è deterministica tramite il RNG `SplitMix64Random` del framework. I nuovi renderer condividono la pipeline CLI/Studio, palette Auto e override cromatici, JSON v1, alpha, finiture e modalità fine-2x entro i budget precedenti.

## Codice

- `Algorithms/Pattern/ModularTileGrammar.cs`: semi e forme deterministici indipendenti da Skia.
- `Application/Catalog/ModularArtworkDescriptors.cs`: 3 descrittori e i controlli esposti dallo Studio.
- `Rendering.Skia/GartModularRenderer.cs`: rendering geometrico sulla superficie Skia; nessun buffer raster addizionale.
- `Rendering.Skia/ArtworkRolePalettePreview.cs`: campioni palette Auto nel picker.
- `Application/Defaults/{alien-letters,cotton-circles,rigor-grid}.json`: 3 nuovi preset incorporati.
- `examples/recipes/m4e3-g1/`: 9 ricette di studio Full HD (tre per algoritmo).

### Controlli principali

**Alien Letters:** righe/colonne, sottogriglia, densità dei blocchi, probabilità delle barre unite, arrotondamento, scala, palette. La versione V3 Gȧrt originale aveva 4×4 grandi blocchi con sottogriglia 5×5; questa reinterpretazione regola entrambi.

**Cotton Circles:** righe/colonne, margine, fuga, dimensione arco, forma dei motivi (semicerchio/quarto/misto), palette. Il riferimento original usa tasselli senza fuga, semicerchi orientati e colori con texture di turbolenza; il nostro output riusa finiture materiche ma non copia lo shader originale.

**Rigor:** periodicità del blocco (due colonne × N righe), colore accento, lettere `IGOR` opzionali, inclinazione lettere, distanza di influenza, torsione e spostamento dei tasselli vicini, scala, palette. `letters=none` produce una composizione modulare puramente geometrica.

## Compatibilità e limiti

- **23 generatori** e **120 ricette JSON** complessive.
- Nessuna modifica ai 20 generatori storici, ai loro renderer e ai **17 preset personali originari**. Le 111 ricette precedenti rimangono byte-identiche.
- JSON v1 invariato. Le nuove opzioni di finitura sono facoltative per le ricette nuove come per le vecchie.
- Fino a 4096×4096 nel budget standard. **8K non ancora abilitato** per i nuovi generatori, anche se disegnano direttamente su Skia: l'estensione A4 necessita di benchmark/QA.
- Le composizioni sono **ispirate** ai metodi originali, non copie esatte delle opere Gȧrt. Vedere `licenses/GART_BSD2.txt` e `docs/THIRD_PARTY_NOTICES.md` per la licenza e le attribuzioni.

## Verifiche predisposte

- `ModularTileGrammarTests`: riproducibilità del seed, copertura delle griglie e limiti.
- `GartModularRenderingTests`: PNG deterministico, variazioni cromatiche/geometriche, fine-2x, JSON e cancellazione.
- `GartG1RecipeFixturesTests`: preset embedded, compatibilità con i preset personali e nove studi completi.
- `CatalogTests` e `StudioBundledDefaultsTests`: inventario aggiornato da 20 a 23.

Questi test **devono ancora essere eseguiti su Windows**. Non sono segnalati come già superati.

## Comandi Windows (PowerShell 7)

Dalla directory di `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per aprire lo Studio:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Per renderizzare uno studio di prova:

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e3-g1\cotton-circles-lagoon-1920.json .\cotton-lagoon.png
```

## Prossime attività

1. Gate Windows della G1, verifica visiva e possibile bilanciamento delle nuove ricette.
2. A3-G2: pennelli a setole e nastri pittorici (Crinis, Flamebrush); non anticipare animazioni.
3. A3-G3: Fascia, retinatura locale e flussi cromatici. Continuare A4/8K e A1/QA in parallelo.
4. Dopo Fase A: B compositore universale, D workflow dell'artista, animazioni evolute successivamente.
