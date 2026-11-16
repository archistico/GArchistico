# M2B — Renderer SkiaSharp e PNG headless

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Stato:** implementato; validazione con .NET 10 e librerie native da eseguire. **Data:** 16 novembre 2026.

## Contratto del formato pixel

- Core: `PixelBuffer` rappresenta `uint` ARGB straight (`0xAARRGGBB`). `StridePixels` è il passo di riga in **pixel** e l'API di riga esclude il padding.
- Adapter: `SkiaPixelAdapter.ToBitmap(PixelBuffer)` crea una nuova `SKBitmap` proprietaria in `SKColorType.Bgra8888` e `SKAlphaType.Unpremul`. La conversione imposta esplicitamente i byte nell'ordine `[B,G,R,A]`, senza fare cast di `uint` o dipendere dall'endianness della macchina. Tiene una sola riga di buffer temporaneo; rispetta lo stride del Core. L'utente della bitmap deve invocare `Dispose()`.
- `ToPixelBuffer(SKBitmap)` utilizza `GetPixel` per gestire i formati supportati da Skia. È una prima implementazione conservativa, non un percorso bulk ottimizzato. Restituisce un nuovo buffer posseduto dal chiamante, con stride pari alla larghezza.
- La superficie `SkiaRasterSurface` è raster CPU con `Bgra8888/Premul`. La copia `DrawPixels` effettua compositing SrcOver. I valori RGB di pixel pienamente trasparenti possono non sopravvivere al rendering premoltiplicato; non vanno usati come metadati nascosti.

## Risorse e memoria

- `SKBitmap`, `SKImage`, `SKData`, `SKSurface` e `SKPaint` vengono sempre rilasciati mediante `Dispose`/`using` dal loro proprietario.
- `SKCanvas` appartiene alla superficie e non è rilasciato separatamente.
- Nessun riferimento nativo alla memoria del `PixelBuffer`: l'adapter **copia** i dati. La bitmap può quindi sopravvivere al buffer sorgente senza puntatori invalidi.
- `CanvasSize` limita il numero di pixel. Non esiste ancora un budget cumulativo per render multipli simultanei: la concorrenza va limitata dall'applicazione in una milestone successiva.
- Gli stream forniti dal chiamante restano aperti dopo `Write`/`WritePng`; metodi `Save`/`SavePng` aprono e chiudono il file di propria proprietà. L'eventuale creazione delle directory è responsabilità del chiamante, ad eccezione della CLI dimostrativa.

## API

- `SkiaPixelAdapter.ToBitmap`, `SkiaPixelAdapter.ToPixelBuffer`.
- `SkiaPngEncoder.Write(PixelBuffer, Stream)` e `Save(PixelBuffer, string)`.
- `SkiaRasterSurface(CanvasSize)`: `Clear`, `DrawPixels`, `DrawLine`, `DrawCircle`, `WritePng`, `SavePng`, `Dispose`. Richiede coordinate finite e raggio/spessore positivi. Non è thread-safe.
- `GenerativeArt.Cli`: `render-demo [file.png] [width] [height]`. Default: demo.png, 800×600. Le dimensioni esplicite devono essere tra 1 e 4096; il limite Core resta attivo.

## Test di accettazione (nuovi 11 xUnit)

1. Mappatura byte BGRA e alpha straight, inclusi canali RGB nascosti con alpha 0 nella bitmap nativa.
2. Roundtrip managed/nativo con stride non compatto.
3. Gestione bitmap nulle o non valide; sorgente nullo.
4. PNG decodificabile, firma e dimensioni, pixel opachi noti.
5. Conservazione di alpha semi-trasparente (con tolleranza RGB per premoltiplicazione).
6. Stream aperto dopo export, input invalidi.
7. Composizione di `PixelBuffer` e primitive vettoriali sulla superficie.
8. `Dispose` idempotente e accesso dopo dispose proibito.
9. Validazione di coordinate/raggio/spessore.
10. Disegno di una linea e decodifica PNG.

## Rischi aperti

- **Prima build SkiaSharp 4.x:** verificare disponibilità pacchetti NuGet e compatibilità API; nessuna compilazione attestata su M2B.
- **Dipendenze native:** `SkiaSharp.NativeAssets.Linux.NoDependencies` incluso per esecuzione/CI Linux. macOS, RID e packaging verranno trattati prima della distribuzione.
- **Precisione visiva:** Skia può modificare valori RGB di ±1 nei passaggi premul; i test evitano hash byte-per-byte del PNG.
- **Prestazioni:** `ToPixelBuffer` usa `GetPixel` per semplicità/portabilità; da ottimizzare dopo benchmark. M2B non implementa snapshot differenziali, streaming video o gestione GPU.
- **Scrittura su disco:** il salvataggio su file non è ancora atomico. Diventerà parte della gestione robusta degli export.
