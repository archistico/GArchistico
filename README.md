# GenerativeArt Studio

![GenerativeArt Studio con il mosaico Voronoi stratificato](docs/images/generativeart-studio.png)

GenerativeArt è un'applicazione desktop **C#/.NET 10** per creare arte generativa con **Avalonia** e **SkiaSharp**. Lo Studio e la CLI condividono lo stesso motore. Le ricette JSON conservano seed, dimensioni, algoritmo e parametri per riprodurre le opere.

Il catalogo comprende **30 generatori**, 30 preset incorporati e 216 ricette di esempio: flussi, pennelli, macchie SDF, paesaggi, incisioni, mosaici, pattern e campi di rumore. Cincinnus offre pennellate a setole con distribuzione Simple/Adaptive, tre palette e otto pigmenti modificabili singolarmente. Il mosaico Voronoi può sovrapporre più piani, con tessere mancanti, smussi illuminati, ombre e fondo sfocato. Colori e trasparenza, dettagli, ombre e finiture cartacee sono regolabili dall'editor.

Tutti i generatori offrono un profilo **4K UHD con supersampling 2×**: il calcolo avviene a 7680×4320 e il PNG finale misura 3840×2160, adatto anche a ritagli di dettaglio. Sono supportati il formato verticale e dimensioni personalizzate fino a 4096 px per lato e 8.847.360 pixel finali. L'anteprima statica usa la risoluzione finale; **Adatta** e **100%** controllano la visualizzazione. Il progetto supporta ricette animate e sequenze di fotogrammi per i generatori compatibili. L'export 8K UHD resta disponibile nei percorsi geometrici già previsti: Guilloché, Voronoi piatto, Streamlines distanziate e Stippling Poisson.

## Galleria degli algoritmi

Ogni miniatura mostra un render del progetto. Selezionala per aprire la ricetta JSON corrispondente; seed, colori e parametri sono modificabili nello Studio.

| | | |
| :---: | :---: | :---: |
| **Alien Letters · glifi modulari**<br>[<img src="docs/images/algorithms/alien-letters.png" alt="Alien Letters &#183; glifi modulari" width="280">](examples/recipes/production-4k/alien-letters.json) | **Bokeh · piani di luce**<br>[<img src="docs/images/algorithms/bokeh-depth.png" alt="Bokeh &#183; piani di luce" width="280">](examples/recipes/production-4k/bokeh-depth.json) | **Cincinnus · pennellate fluenti**<br>[<img src="docs/images/algorithms/cincinnus-bristles.png" alt="Cincinnus &#183; pennellate fluenti" width="280">](examples/recipes/production-4k/cincinnus-bristles.json) |
| **Cotton Circles · tasselli semicircolari**<br>[<img src="docs/images/algorithms/cotton-circles.png" alt="Cotton Circles &#183; tasselli semicircolari" width="280">](examples/recipes/production-4k/cotton-circles.json) | **Crinis · fasci di setole**<br>[<img src="docs/images/algorithms/crinis-bristles.png" alt="Crinis &#183; fasci di setole" width="280">](examples/recipes/production-4k/crinis-bristles.json) | **Domain Warp · venature**<br>[<img src="docs/images/algorithms/domain-warp.png" alt="Domain Warp &#183; venature" width="280">](examples/recipes/production-4k/domain-warp.json) |
| **Eclectic · tracce in collisione**<br>[<img src="docs/images/algorithms/eclectic-trails.png" alt="Eclectic &#183; tracce in collisione" width="280">](examples/recipes/production-4k/eclectic-trails.json) | **Fascia · nastri d'inchiostro**<br>[<img src="docs/images/algorithms/fascia-ribbons.png" alt="Fascia &#183; nastri d&#39;inchiostro" width="280">](examples/recipes/production-4k/fascia-ribbons.json) | **Pennelli nel flusso**<br>[<img src="docs/images/algorithms/flow-brush.png" alt="Pennelli nel flusso" width="280">](examples/recipes/production-4k/flow-brush.json) |
| **Guilloché · intrecci**<br>[<img src="docs/images/algorithms/guilloche.png" alt="Guilloch&#233; &#183; intrecci" width="280">](examples/recipes/production-4k/guilloche.json) | **LIC Flow · fibre**<br>[<img src="docs/images/algorithms/lic-flow.png" alt="LIC Flow &#183; fibre" width="280">](examples/recipes/production-4k/lic-flow.json) | **Flow Loop**<br>[<img src="docs/images/algorithms/looping-flow.png" alt="Flow Loop" width="280">](examples/recipes/production-4k/looping-flow.json) |
| **Noise Flow**<br>[<img src="docs/images/algorithms/noise-flow.png" alt="Noise Flow" width="280">](examples/recipes/production-4k/noise-flow.json) | **Ocellus · occhi stratificati**<br>[<img src="docs/images/algorithms/ocellus-eyespots.png" alt="Ocellus &#183; occhi stratificati" width="280">](examples/recipes/production-4k/ocellus-eyespots.json) | **Carta ritagliata · tunnel**<br>[<img src="docs/images/algorithms/paper-cut.png" alt="Carta ritagliata &#183; tunnel" width="280">](examples/recipes/production-4k/paper-cut.json) |
| **Curve parametriche**<br>[<img src="docs/images/algorithms/parametric-curves.png" alt="Curve parametriche" width="280">](examples/recipes/production-4k/parametric-curves.json) | **Fillotassi · spirali botaniche**<br>[<img src="docs/images/algorithms/phyllotaxis.png" alt="Fillotassi &#183; spirali botaniche" width="280">](examples/recipes/production-4k/phyllotaxis.json) | **Plasma Noise**<br>[<img src="docs/images/algorithms/plasma-noise.png" alt="Plasma Noise" width="280">](examples/recipes/production-4k/plasma-noise.json) |
| **Stippling Poisson**<br>[<img src="docs/images/algorithms/poisson-stippling.png" alt="Stippling Poisson" width="280">](examples/recipes/production-4k/poisson-stippling.json) | **Reazione–diffusione · Gray–Scott**<br>[<img src="docs/images/algorithms/reaction-diffusion.png" alt="Reazione–diffusione &#183; Gray–Scott" width="280">](examples/recipes/production-4k/reaction-diffusion.json) | **Rete · schiuma cellulare**<br>[<img src="docs/images/algorithms/rete-foam.png" alt="Rete &#183; schiuma cellulare" width="280">](examples/recipes/production-4k/rete-foam.json) |
| **Rigor · griglia tipografica**<br>[<img src="docs/images/algorithms/rigor-grid.png" alt="Rigor &#183; griglia tipografica" width="280">](examples/recipes/production-4k/rigor-grid.json) | **Orbite SDF · macchie e fusioni**<br>[<img src="docs/images/algorithms/sdf-orbits.png" alt="Orbite SDF &#183; macchie e fusioni" width="280">](examples/recipes/production-4k/sdf-orbits.json) | **Streamlines distanziate**<br>[<img src="docs/images/algorithms/spaced-streamlines.png" alt="Streamlines distanziate" width="280">](examples/recipes/production-4k/spaced-streamlines.json) |
| **Flamebrush · spirali puntinate**<br>[<img src="docs/images/algorithms/spiral-flamebrush.png" alt="Flamebrush &#183; spirali puntinate" width="280">](examples/recipes/production-4k/spiral-flamebrush.json) | **Paesaggi a strati**<br>[<img src="docs/images/algorithms/strata-landscape.png" alt="Paesaggi a strati" width="280">](examples/recipes/production-4k/strata-landscape.json) | **Incisione · tratteggio**<br>[<img src="docs/images/algorithms/tonal-engraving.png" alt="Incisione &#183; tratteggio" width="280">](examples/recipes/production-4k/tonal-engraving.json) |
| **Curve topografiche**<br>[<img src="docs/images/algorithms/topographic-contours.png" alt="Curve topografiche" width="280">](examples/recipes/production-4k/topographic-contours.json) | **Mosaico Voronoi**<br>[<img src="docs/images/algorithms/voronoi-mosaic.png" alt="Mosaico Voronoi" width="280">](examples/recipes/production-4k/voronoi-layered.json) | **Interferenze d'onda · Moiré**<br>[<img src="docs/images/algorithms/wave-interference.png" alt="Interferenze d&#39;onda &#183; Moir&#233;" width="280">](examples/recipes/production-4k/wave-interference.json) |

## Installazione su Windows

Servono **.NET SDK 10**, compatibile con `global.json`, e una connessione NuGet per il primo ripristino. Estrai il progetto e apri PowerShell nella cartella che contiene `GenerativeArt.sln`:

```powershell
dotnet restore .\GenerativeArt.sln
dotnet build .\GenerativeArt.sln -c Release --no-restore
```

## Esecuzione

Avvia lo Studio con lo script, che ripristina le dipendenze e compila in Release:

```powershell
.\run.ps1
```

Per avviare una compilazione esistente usa `.\run.ps1 -NoBuild`; per la configurazione Debug usa `.\run.ps1 -Configuration Debug`.

Per eliminare tutte le cartelle `bin` e `obj` del progetto:

```powershell
.\clean.ps1
```

Puoi vedere quali cartelle verranno eliminate con `.\clean.ps1 -WhatIf`. Le cache NuGet vengono conservate. Gli script funzionano anche se richiamati da un'altra cartella; dopo la pulizia, `.\run.ps1` ricompila lo Studio.

Scegli un generatore, modifica i parametri e osserva l'anteprima. **Apri ricetta** carica uno degli esempi in [examples/recipes](examples/recipes); **Salva ricetta** conserva le impostazioni ed **Esporta PNG** genera l'immagine.

Per un background ad alta risoluzione premi **4K · qualità massima**, sotto larghezza e altezza. Imposta il formato UHD, il supersampling 2× e i massimi valori di densità, campioni e iterazioni pertinenti all'algoritmo; conserva seed, palette e scelte compositive. I parametri restano modificabili. Per mantenere il dettaglio corrente scegli invece **Profilo di lavorazione → production**, imposta le dimensioni e **Campionamento raster → fine-2x**. I render più densi possono richiedere alcuni minuti. Le ricette pronte si trovano in [examples/recipes/production-4k](examples/recipes/production-4k).

Per il Voronoi con profondità scegli **Composizione del mosaico → layered**, oppure apri una ricetta in [examples/recipes/voronoi-depth-v1](examples/recipes/voronoi-depth-v1).

La CLI permette di elencare i generatori e renderizzare una ricetta:

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null

dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- describe cincinnus-bristles

dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\cincinnus-c5\cincinnus-simple-crimson-768.json .\output\cincinnus-simple.png
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\cincinnus-c5\cincinnus-adaptive-ocean-768.json .\output\cincinnus-adaptive.png
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\voronoi-depth-v1\voronoi-depth-glass-windows-1920.json .\output\voronoi-profondo.png

dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- prepare-4k .\examples\recipes\voronoi-depth-v1\voronoi-depth-glass-windows-1920.json .\output\voronoi-4k.json
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\output\voronoi-4k.json .\output\voronoi-4k.png
```

La destinazione deve essere nuova: la CLI non sovrascrive file esistenti. Aggiungi `--keep-complexity` a `prepare-4k` per conservare la densità e il dettaglio della ricetta originale.
