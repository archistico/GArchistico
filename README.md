# GenerativeArt

![GenerativeArt Studio con anteprima e controlli del generatore Domain Warp](docs/images/generativeart-studio.png)

GenerativeArt è un'applicazione desktop e un framework **C# / .NET 10** per creare arte generativa. Lo Studio offre un'interfaccia visuale; la CLI permette di generare le immagini da terminale usando lo stesso motore.

I generatori comprendono curve parametriche e Guilloché, flussi e streamlines, pennelli organici, paesaggi a strati, campi di rumore e Domain Warp, texture LIC, incisioni a tratteggio, isolinee, mosaici Voronoi, forme SDF e distribuzioni Poisson.

È possibile regolare geometria, densità, colori, trasparenza e finiture come carta, pigmenti, grana e dithering. Le ricette JSON conservano seed e parametri per riprodurre le opere. L'anteprima supporta zoom e spostamento, mentre l'esportazione produce PNG alla risoluzione scelta; i generatori animati dispongono anche di riproduzione e sequenze di fotogrammi.

## Installazione

Le istruzioni seguenti sono per Windows. Sono necessari:

- SDK **.NET 10**, compatibile con il file `global.json`.
- Una connessione a NuGet per il primo ripristino delle dipendenze.

Scarica ed estrai il progetto, quindi apri PowerShell nella cartella che contiene `GenerativeArt.sln` ed esegui:

```powershell
dotnet restore .\GenerativeArt.sln
dotnet build .\GenerativeArt.sln -c Release --no-restore
```

Per verificare l'installazione puoi eseguire i test:

```powershell
dotnet test .\GenerativeArt.sln -c Release --no-build
```

## Esecuzione

### Studio desktop

```powershell
dotnet run --project .\src\GenerativeArt.Studio -c Release --no-build
```

Scegli un generatore nel catalogo, modifica i parametri e osserva l'anteprima. Usa **Apri ricetta** per caricare uno degli esempi in [`examples/recipes`](examples/recipes), **Salva ricetta** per conservare le impostazioni ed **Esporta PNG** per salvare l'immagine.

Le dimensioni dell'output sono indipendenti da quelle dell'anteprima. **Adatta** mostra l'intera opera, **100%** permette di esaminare i pixel e **Raster = output** richiede l'anteprima alla risoluzione di esportazione.

### CLI

Elenca i generatori disponibili:

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- list
```

Genera un PNG da una ricetta:

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null

dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\domain-warp-ocean-marble-1920.json .\output\domain-warp.png
```

La CLI richiede un nome di destinazione nuovo e non sovrascrive i file esistenti. Le altre ricette pronte si trovano in [`examples/recipes`](examples/recipes), con anteprime in [`examples/previews`](examples/previews).
