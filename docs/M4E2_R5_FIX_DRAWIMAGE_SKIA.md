# M4E.2-R5 — Correzione CS0618 in ArtisticScanlineFinisher

**Data:** 17 novembre 2026 · **Base:** ZIP M4E.2-R4.

## Problema rilevato su Windows

La compilazione di `GenerativeArt.Rendering.Skia` si interrompeva con `CS0618` in `ArtisticScanlineFinisher.cs`, perché `SKCanvas.DrawImage(SKImage, float, float, SKPaint)` è obsoleto nella versione SkiaSharp 4.153.2 utilizzata dal progetto. `Directory.Build.props` imposta `TreatWarningsAsErrors=true`.

## Intervento

- Sostituita soltanto la chiamata obsoleta con l'overload `DrawImage(SKImage, SKRect, SKSamplingOptions)` già utilizzato in `SkiaRasterSurface.cs`.
- La destinazione è un rettangolo della **stessa dimensione** dell'immagine sorgente. È specificato `SKFilterMode.Nearest` per evitare interpolazione nella copia 1:1 prima delle finiture a righe.
- Aggiunto `ScanlinePngCopiesSourcePixelsWithoutResampling`, che confronta pixel per pixel una bitmap opaca multicolore prima e dopo l'encoding PNG. Restano attivi i test R4 per il confronto tra kernel per righe e storico, alpha, cancellazione e budget.
- Nessun intervento su algoritmi, regole del catalogo, JSON, preset, menu Studio, risoluzione dell'anteprima o dimensioni dell'esportazione.

## Stato delle verifiche

- **M4E.2-R1**: compilazione e test confermati dall'utente.
- **M4E.2-R4**: compilazione **fallita** su Windows, `CS0618`; l'esecuzione della suite completa non è quindi stata completata.
- **M4E.2-R5**: sorgente corretta, test di regressione predisposto; compilazione Windows e test da confermare.

## Comandi Windows (PowerShell 7, cartella con `GenerativeArt.sln`)

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Se vuoi isolare la compilazione del renderer:

```powershell
dotnet build .\src\GenerativeArt.Rendering.Skia\GenerativeArt.Rendering.Skia.csproj -c Release
```

Per verificare il test aggiunto:

```powershell
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --filter 'FullyQualifiedName~ScanlinePngCopiesSourcePixelsWithoutResampling'
```

Un successivo smoke manuale 8K con finiture va eseguito solo dopo il successo della compilazione e dei test; vedere `M4E2_R4_FINITURE_8K_RIGHE.md`.

## Prossimi lavori

Rimane confermata la Fase A4 (ottimizzazione dei renderer per 8K e benchmark), seguita dalla Fase B (compositore universale) e dalla Fase D (workflow artista). Animazioni evolute rinviate.
