# M4A.4-R3 — Campionamento raster 2× e qualità PNG

**16 novembre 2026 · implementato, gate Windows ancora aperto.** Base M4A.4-R2 (non ancora validata su Windows); ultima revisione dichiarata compilata e testata dall'utente: **M4A.4-R1**. Nessuna compilazione o suite xUnit R3 è stata eseguita in questo ambiente, perché non è disponibile .NET SDK.

## Scopo e limiti

La linea Skia disponeva già di `IsAntialias = true` per stroke, poligoni e cerchi. Questa R3 non presenta quell'AA come novità: introduce un **supersampling opzionale 2×**. Il generatore renderizza la scena su una superficie nativa grande il doppio in ciascuna dimensione, quindi Skia la riduce al canvas richiesto con filtro lineare. Il ridimensionamento avviene in **BGRA premoltiplicato**, per non mescolare i colori RGB invisibili dei pixel a alpha nullo. L'output è sempre PNG alle **dimensioni della ricetta**, non un PNG 2×.

## Controllo per opera

Tutti e nove i generatori hanno un nuovo parametro tipizzato nel gruppo `Qualità`:

| `render-quality` | Significato |
| --- | --- |
| `standard` (predefinito) | Percorso raster M4A.4-R2 invariato: stesso numero di pixel, stesse coordinate e stessa codifica |
| `fine-2x` | 4 sottopixel per pixel di uscita, riduzione con sampling lineare; maggiore lavoro CPU e RAM |

Il parametro è facoltativo nei JSON v1. Una ricetta vecchia priva della chiave viene caricata con `standard`. La versione di schema **resta v1**, la versione algoritmo **resta 1** e gli ID delle opere **non cambiano**. Una qualità sconosciuta, es. `fine-3x`, è rifiutata dal descrittore. Entrambe le modalità mantengono lo stesso seed, tempi/frame e controlli artistici; il *fine* cambia appositamente i pixel risultanti. Non si promette identità PNG byte-per-byte fra versioni native Skia o sistemi differenti.

**Tratti e geometria:** parametric, noise-flow, spaced-streamlines, looping-flow, Poisson, topografia e Voronoi disegnano nelle coordinate originali tramite un `SKCanvas.Scale(2)`; i tratti mantengono lo stesso spessore *logico*.

**Raster calcolati per pixel:** Plasma e Orbite SDF ricampionano effettivamente il campo a passo 1/2 e disegnano un `PixelBuffer` nativo 2× senza scalarlo nuovamente. Le coordinate del noise rimangono nel sistema originale (niente cambio accidentale di frequenza). I background raster tradizionali (es. gradiente Noise Flow) mantengono invece il campionamento a pixel logici e si compongono correttamente sulla superficie 2×.

## Budget e trattamento memoria

`ArtworkRenderEngine.Validate()` verifica **prima delle allocazioni native e prima di scrivere nella destinazione**:

- dimensione base: massimo 4096 per lato e 16.777.216 pixel, più gli attuali budget specifici di ogni opera;
- `fine-2x`: massimo **16.777.216 pixel temporanei**, cioè `width × height × 4 <= 16.777.216` (fino a 2048×2048, oppure Full HD 1920×1080);
- per canvas più grandi (es. poster 4000×4000), usare `standard`, che rimane disponibile;
- l'algoritmo di loop elabora **un fotogramma per invocazione**, senza accumulare l'animazione; resta attivo il budget cumulativo preesistente di 40 milioni di pixel di uscita;
- ogni superficie, snapshot, bitmap e buffer viene creato e rilasciato durante una singola chiamata, senza cache condivise.

**Non è una soglia di RAM di processo certificata:** il limite riguarda il numero di pixel della superficie 2×. Nella modalità fine il processo può occupare più copie native/managed durante composizione e PNG. Servono stress test reali per poster e sessioni di playback; non dichiarare uno stress gate chiuso prima delle misure.

## Esempi di confronto

Nella directory `GenerativeArt` aperta con **PowerShell 7** e .NET SDK 10, dopo la suite standard:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per uno smoke riproducibile, creare la directory `output` senza cancellare gli output personali, poi generare due PNG diversi dalla stessa composizione:

```powershell
New-Item -ItemType Directory -Force .\output | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\noise-flow-pastel-1920.json .\output\noise-standard-r3.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\noise-flow-fine-2x-1920.json .\output\noise-fine-r3.png
```

Entrambi i PNG devono misurare **1920×1080**, avere lo stesso seed/composizione ma dettagli dei tratti differenti. Il comando CLI **non sovrascrive**: se l'output esiste, usare nomi nuovi o spostare manualmente i propri risultati. Sono inclusi anche `voronoi-fine-2x-1920.json` e `plasma-fine-2x-1920.json` per provare geometrie e raster per-pixel.

Aprire lo Studio con:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Selezionare una delle nove opere, scorrere fino al gruppo **QUALITÀ**, cambiare `Campionamento raster` tra `standard` e `fine-2x`; verificare anteprima, export, salvataggio/ricaricamento JSON, colori alpha e cancellazione. Provare `4000×4000` con `fine-2x`: l'export deve fallire con **messaggio di budget**, non allocare o sostituire il PNG esistente. Riprovare con `standard`.

## Test aggiunti e gate

- `RasterQualityTests`: default = `standard`, identità default/standard, modalità 2× deterministica e dimensioni stabili per tutti e nove i renderer, PNG decodificabile, compatibilità JSON legacy, qualità invalida, risoluzioni Full HD/2048/4000, fallimento *prima di scrivere*, alpha premoltiplicato, buffer nativi, loop e cancellazione.
- `CatalogTests`: parametro finale aggiunto al Plasma e controllo catalogo a nove. `StudioCreativeControlsTests`: nuovo default per i JSON storici.
- Controlli statici di sorgenti/JSON e integrità ZIP in ambiente di preparazione; build, xUnit, smoke visuali e stress RAM sono **da eseguire su Windows**.

## Successivo

Dopo validazione R2/R3: migliorare la preview in caso di export 2× fuori budget; aggiungere **grana controllata**, eventuale dithering e filtri alpha-safe come milestone M4C. Poi **M4B noise/domain warping e loop** per ampliare davvero le forme artistiche. Non introdurre nuovi linguaggi oltre a C#/.NET, Avalonia e minimo PowerShell.
