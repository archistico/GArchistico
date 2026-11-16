# M4C.1-R1 — Grana artistica e dithering ordinato (16/11/2026)

## Obiettivo e stato

Primo post-processing artistico riproducibile per tutti i nove generatori, compatibile con la qualità raster `standard` / `fine-2x` e con il formato ricette **JSON v1**. **Sviluppato e controllato staticamente, non ancora compilato/testato su Windows**. La più recente conferma dell'utente resta **M4A.4-R1**; sono aperti anche i gate R2 e R3.

## Parametri nel pannello FINITURA (tutti i generatori)

| ID JSON | Tipo | Default | Intervallo | Significato |
|---|---|---|---|---|
| `grain-amount` | Number | `0` | 0–0.35 | Variazione di luminosità sRGB per pixel, 0 = disattivata |
| `grain-size` | Integer | `1` | 1–12 | Dimensione della cella quadrata di grana, in pixel finali |
| `dither-mode` | Choice | `none` | `none`, `bayer4`, `bayer8` | Nessun dithering / retino ordinato 4×4 / 8×8 |
| `dither-levels` | Integer | `8` | 2–32 | Numero di livelli per canale RGB quando il dithering è attivo |

Tutti i parametri sono **facoltativi in importazione**: le ricette precedenti non devono contenerli, e i default ne preservano il risultato. I campi Integer/Number/Choice sono resi dal pannello dinamico già presente nello Studio: non è stato creato un secondo pannello divergente per la CLI.

### Algoritmo

1. Il generatore produce il raster standard o 2×. Nel secondo caso Skia applica la riduzione alpha-premoltiplicata prima della finitura.
2. Se entrambi gli effetti sono disabilitati, lo snapshot viene codificato usando **lo stesso percorso PNG di R3**, senza ulteriori conversioni o allocazioni.
3. Se almeno un effetto è attivo, lo snapshot finale viene convertito in `PixelBuffer` ARGB non-premoltiplicato; un singolo passaggio sulle righe applica grana e, in seguito, quantizzazione ordinata RGB. La grana deriva da `seed`, X e Y (hash SplitMix64) e non consuma lo stato random di altri generatori.
4. L'alpha di ogni pixel viene **mantenuto**: non viene aggiunta grana/quantizzazione all'alpha; i pixel completamente trasparenti sono lasciati invariati. Il risultato passa a `SkiaPngEncoder` (conversione ARGB→BGRA esplicita).
5. La texture non dipende da `frameIndex` e non viene memorizzata una timeline in RAM. Durante un loop, il retino e la grana restano ancorati al canvas. **Non** è stato introdotto movimento del grano: si eviterà lo sfarfallio al punto di chiusura.

Il dithering opera sui valori RGB sRGB, non è error diffusion né quantizzazione ottimale percettiva. È una prima finitura *artistica*, non ancora un modulo di color management / CMYK. Per bordi trasparenti, valutare visivamente anche sfondi esterni chiari/scuri nella futura QA.

## Budget e limiti

- **Standard senza finitura:** rimangono i budget e le immagini di R3, inclusi poster 4000×4000 se il singolo algoritmo li consente.
- **Finitura attiva (aggiornamento M4C.2-R3):** massimo **16.777.216 pixel finali** (4096×4096 in standard, se ammesso dal renderer). Il post-processing richiede una bitmap Skia addizionale e un buffer gestito di 4 byte/pixel, oltre alle superfici già presenti. Il limite viene controllato da `ArtworkRenderEngine.Validate` **prima di disegnare o scrivere output**. La classe `SkiaRasterSurface` effettua un secondo controllo per i chiamanti diretti.
- **Fine 2× + finitura:** si applicano *entrambi* i budget (16.777.216 sottopixel temporanei e 16.777.216 pixel finali; quello dei sottopixel è vincolante già oltre 4.194.304 pixel finali).
- Il token di cancellazione viene controllato prima della finitura e a ogni riga. Non si dichiarano misurate CPU e RAM: serve test reale, specialmente su Full HD e durante riproduzione.

## File modificati e aggiunti

- `src/GenerativeArt.Application/Catalog/BuiltInArtworkCatalog.cs`: quattro parametri facoltativi per tutte le nove opere.
- `src/GenerativeArt.Application/Catalog/ArtworkParameterSpec.cs`: anche il factory `Integer` accetta `optionalInJson`.
- `src/GenerativeArt.Rendering.Skia/ArtisticFinishOptions.cs`: parsing, default, validazione e budget.
- `src/GenerativeArt.Rendering.Skia/ArtisticPostProcessor.cs`: algoritmo managed row-by-row, senza buffer intermedi.
- `src/GenerativeArt.Rendering.Skia/SkiaRasterSurface.cs`: finitura dopo il downsampling e prima della codifica PNG.
- `src/GenerativeArt.Rendering.Skia/ArtworkRenderEngine.cs`: validazione preventiva e passaggio seed/annullamento.
- `src/GenerativeArt.Studio/StudioWindow.cs`: titolo di versione; il pannello viene popolato automaticamente dal catalogo.
- `tests/GenerativeArt.Rendering.Skia.Tests/ArtisticPostProcessorTests.cs`: regressioni seeded, alpha, quantizzazione, JSON, nove opere, budget, 2× e cancellazione.
- `tests/GenerativeArt.Application.Tests/CatalogTests.cs`: aggiornamento elenco esatto e controlli su tutti i default.

## Ricette artistiche nuove

- `plasma-grain-fine-1920.json`: 1920×1080, render 2× con grana colorata monocromatica.
- `voronoi-dither-glass-1920.json`: vetrate con quantizzazione a 6 livelli/canale (Bayer 4×4).
- `topographic-paper-grain-1920.json`: topografia soft e grana a celle più grandi.
- `noise-flow-print-dither-1920.json`: flussi e retino Bayer 8×8 con grana leggera.

## Gate da eseguire su Windows — PowerShell 7

Prerequisiti: .NET SDK 10, Windows compatibile con SkiaSharp, accesso NuGet al primo ripristino. Nella cartella `GenerativeArt` che contiene la solution:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per diagnosticare soltanto il nuovo incremento:

```powershell
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --filter FullyQualifiedName~ArtisticPostProcessorTests

dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --filter FullyQualifiedName~CatalogTests
```

Aprire il desktop e regolare **FINITURA** nel pannello destro:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Per confrontare due PNG con output stesso 1920×1080, usare ricette esistenti e nuove:

```powershell
New-Item -ItemType Directory -Force .\output | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\plasma-fine-2x-1920.json .\output\plasma-prima.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\plasma-grain-fine-1920.json .\output\plasma-grana.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\noise-flow-print-dither-1920.json .\output\noise-stampa.png
```

**Atteso (aggiornato M4C.2-R3):** PNG validi a risoluzione invariata; impostando `grain-amount=0` e `dither-mode=none`, hash PNG compatibile con R3 a parità di seed, versione Skia e ricetta. Con un effetto attivo cambia il contenuto, senza variare l'alpha rispetto allo snapshot di partenza. Un poster **4000×4000 con finitura standard** deve ora essere *accettato* in `Validate`, se il renderer supporta la dimensione; con `fine-2x` un poster del genere deve ancora essere rifiutato a causa dei sottopixel.

**Non ancora verificati:** compilazione .NET, test xUnit, parità esatta di hash su Windows, performance e memoria reale, analisi visiva su gradiente e alpha compositing, QA Linux/macOS.

## Passi successivi

1. Chiudere i gate Windows R2/R3/M4C.1 appena possibile, correggere eventuali regressioni in un archivio successivo.
2. Migliorare la resa della grana (distribuzione percettiva, controllo di texture carta) senza compromettere il percorso pixel originale.
3. Avviare **M4B**: domain warping e campi animati periodici, mantenendo memoria costante per fotogramma. Valutare poi filtri di materia e pennelli M4D.
