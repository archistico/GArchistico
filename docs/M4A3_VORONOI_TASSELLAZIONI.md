# M4A.3-R1 — Voronoi 2D, Lloyd e Mosaico Voronoi

**Data:** 16 novembre 2026. **Stato:** implementata, gate di compilazione/test Windows da eseguire. **Baseline confermata:** M4A.2-R1, per cui l'utente ha segnalato compilazione, test, Studio e CLI corretti.

## Obiettivo

Introdurre una primitiva geometrica riutilizzabile per tassellazioni a celle convesse, e un nuovo generatore artistico statico che usi il medesimo motore del catalogo comune. L'estensione non modifica gli algoritmi precedenti, il viewport, le preferenze o lo schema delle ricette.

## Nuovo algoritmo `VoronoiTessellation`

- Input: semi ordinati (`IReadOnlyList<Vector2>`) all'interno di `[0,width) × [0,height)`, dimensioni finite positive; un massimo prudenziale di **256 semi**.
- `Build(...)`: per ogni seme inizia con il rettangolo completo e lo interseca con i semipiani di distanza minima rispetto a ogni altro seme. L'intersezione usa coordinate **double** e restituisce vertici `Vector2`, area e centroide. L'ordine delle celle corrisponde all'ordine dei semi.
- Le celle sono poligoni convessi ritagliati; si eliminano i duplicati consecutivi generati da tagli esattamente sui vertici.
- `Relax(...,iterations)`: rilassamento di Lloyd (siti spostati nei centroidi delle rispettive celle) per **0–4 iterazioni**. Il metodo non modifica l'input e verifica l'annullamento.
- Complessità di questa versione: **O(N²·V)**, con V numero di vertici intermedi. Questo metodo è semplice, deterministico e adatto a decine/centinaia di siti; **non** è Fortune né un indice spaziale per milioni di punti. Per i siti molto numerosi si progetterà un'altra strategia.
- Input fuori intervallo, siti coincidenti, aree degeneri e cancellazione sono gestiti esplicitamente.

## `voronoi-mosaic` — Mosaico Voronoi

Il nono generatore produce mosaici a celle con fughe colorate, riempimenti da palette e accenti guidati da noise, senza generare buffer managed per pixel. Siti su griglia jittered riproducibile, poi Lloyd e costruzione di poligoni. Il rendering usa `SkiaRasterSurface.DrawPolygon` con **SKPathBuilder**, fill e bordo antialias. Nessuna chiamata a `SKPath.MoveTo`/`LineTo` obsolete.

| Parametro | Intervallo | Default | Significato |
| --- | --- | --- | --- |
| `sites` | 16–196 | 96 | Numero di tessere |
| `relaxations` | 0–3 | 1 | Passaggi Lloyd |
| `jitter` | 0–0.95 | 0.7 | Irregolarità iniziale |
| `gutter` | 0.5–8 | 2.6 | Fuga relativa alla dimensione, ancorata a 800px |
| `palette` | glass / copper / lagoon | glass | Toni delle tessere |

**Limiti:** minimo **64×64**, massimo generale statico **4096×4096** e **16.777.216 pixel**. Export reali 4000×4000 ammessi in convalida senza allocazione nel test automatico. La preview rimane ridotta dalla politica condivisa, l'export atomico/cancellabile rimane lo stesso. I 4000×4000 devono comunque essere provati realmente su Windows.

## Integrazione

- Catalogo: da otto a **nove** artwork, gli ID/versioni precedenti non cambiano.
- CLI e Studio: stessa `ArtworkRenderEngine`, stesso JSON v1, seed UInt64, PNG e comandi già esistenti.
- Esempi: `examples/recipes/voronoi-mosaic.json`, `examples/recipes/voronoi-poster-4000.json`.
- Test: bisettrice esatta, partizione dell'area, relazione nearest-site, centroidi e rilassamento, determinismo, casi degeneri, annullamento, PNG, seed, palette e parametri.

## Comandi Windows (PowerShell 7 nella cartella `GenerativeArt`)

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\voronoi-mosaic.json .\output\voronoi-m4a3.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\voronoi-poster-4000.json .\output\voronoi-poster-4000.png
```

La CLI non sovrascrive file già esistenti; usare destinazioni nuove. Per il poster, misurare memoria/tempo e verificare PNG decodificabile; per lo Studio verificare ricerca, modifica di tutti i parametri, preferiti, tema, pan/zoom e cancellazione dell'export.

## Limiti aperti

- SDK .NET 10 assente nell'ambiente di preparazione: **nessuna build, suite xUnit o prova visiva della R1 eseguita qui**.
- Bordo/giunzioni dei poligoni e colorazione devono essere valutati artisticamente dopo lo smoke Windows.
- Mancano Fortune/Delaunay su scala elevata, tassellazioni periodiche, pattern esagonali e arte animata di questa famiglia.
- Gli altri renderer hanno limiti propri per l'export hi-res: non promettere 4000×4000 per ogni generatore; ad esempio Plasma ha un limite inferiore.
