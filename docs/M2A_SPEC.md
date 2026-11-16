# M2A — PixelBuffer puramente gestito (preparatorio)

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Data:** 16 novembre 2026 · **Stato:** codice e test xUnit scritti, **build/test .NET non eseguiti**.

## Perché questo incremento precede il rendering

Nella baseline M1D sono aperti G0 (compilazione) e G1 (test Core). La roadmap vieta di dichiarare iniziato o validato M2 prima di G0. Poiché l'SDK .NET 10 non è disponibile e l'utente ha richiesto continuità, M2A è **una deroga limitata ai contratti dati del Core**, senza dipendenze SkiaSharp, export PNG, interfaccia grafica né codice nativo. Nessun gate viene chiuso per effetto di questa consegna.

## Contratto pubblico

| Tipo | Responsabilità |
|---|---|
| `PixelRectangle` | Coordinate intere, larghezza e altezza >= 0; origine anche negativa, bordi finali esclusi calcolati in `long`. |
| `PixelStorageFormat.PackedArgb32Straight` | Convenzione numerica `uint 0xAARRGGBB` con alpha straight. Non si assume alcun byte order. |
| `PixelBuffer` | Array `uint[]` posseduto, dimensioni `CanvasSize`, stride in **pixel**, budget memoria, API lettura/scrittura, righe `Span<uint>`, clipping e copia. |

### Semantica

- L'origine è in alto a sinistra; coordinate intere `(x,y)` in `[0, Width) × [0, Height)`.
- `StridePixels >= Width`; per ciascuna riga `y` l'indice è `y * StridePixels + x`.
- Si allocano `StridePixels * Height` elementi `uint`, verificati in `long` prima dell'allocazione e limitati a `CanvasSize.MaximumPixelCount` (67.108.864 pixel, 256 MiB a 4 byte/pixel).
- L'array è **gestito e posseduto** da `PixelBuffer`; non serve `IDisposable` per questo tipo. Gli adapter Skia futuri possiederanno e rilasceranno esplicitamente le proprie risorse native.
- I colori sono `Color32` ARGB straight, anche quando alpha=0 ed RGB != 0. Non convertire implicitamente in premultiplied: una conversione esplicita è richiesta dall'adapter.
- `GetPixel` e `SetPixel` lanciano `ArgumentOutOfRangeException` fuori area; `TryGetPixel` e `TrySetPixel` non scrivono fuori area. `TryGetPixel` restituisce colore trasparente se la coordinata non è valida.
- `Clear` e `FillRectangle` toccano esclusivamente i pixel visibili, non il padding. `FillRectangle` restituisce il numero `long` di pixel scritti.
- `GetPackedArgbRow` e `GetWritablePackedArgbRow` espongono solo i `Width` pixel di una riga, non il padding. Il chiamante del writable span è responsabile di scrivere `uint` nel formato dichiarato.
- `CopyPackedArgbTo` copia i soli pixel visibili in una destinazione `Span<uint>` di stride configurabile; valida la lunghezza **prima** di ogni modifica. Non converte endianness né alpha e non tocca il padding della destinazione.
- Nessuna API C# offre per ora una vista in byte di memoria reinterpretata: `0xAARRGGBB` è una **convenzione numerica**, non una descrizione del layout fisico RGBA/BGRA.
- Il buffer non è thread-safe per scritture concorrenti alle stesse celle; le regioni disgiunte sono responsabilità dell'esecutore esterno.

## Casi limite coperti da test predisposti

1. Stride maggiore della larghezza e allocazione in byte.
2. Dimensione nulla, stride inferiore alla larghezza, budget anche con padding.
3. Coordinate negative, bordo destro e inferiore, nessun alias con righe precedenti.
4. Alpha=0 con canali RGB non nulli conservati in formato straight.
5. Righe writable limitate alla larghezza logica.
6. `Clear` con alpha diverso da 255.
7. Riempimento rettangolare con origine negativa, rettangoli vuoti, grandi interi senza overflow.
8. Copia con stride di destinazione e protezione del padding.
9. Copia con spazio insufficiente: fallisce prima di scrivere.
10. Destinazione senza padding dopo l'ultima riga accettata.

Queste sono **specifiche di comportamento** e test xUnit **non eseguiti**; non sono una prova che l'implementazione compili. I test preesistenti di M1D rimangono inclusi e non modificati.

## Cosa resta esplicitamente fuori da M2A

- SkiaSharp, rendering, rasterizzazione di primitive, export PNG.
- Buffer pooled/pinned, `MemoryOwner`, unmanaged/native ownership e zero-copy.
- Conversioni in byte order BGRA/RGBA, alpha premoltiplicato, endian e color profile.
- Texture GPU, surface lifecycle, dispatcher UI, animazioni.

Questi elementi dovranno essere trattati dopo una build xUnit effettiva e in una milestone di rendering successiva, mantenendo contratti chiari fra managed e native.

## Gate G0/G1/G2

- **G0:** aperto; `dotnet restore/build/test` non eseguiti nell'ambiente corrente.
- **G1:** aperto; test matematici/colore/noise predisposti ma non eseguiti.
- **G2:** non iniziato come rendering; M2A è un'implementazione preparatoria del Core, **non** un passaggio validato a SkiaSharp.

Quando disponibile un SDK .NET 10: eseguire `tools/validate.ps1`, correggere build/test su **tutta la solution** e solo allora decidere l'API di interop Skia e la pipeline PNG.
