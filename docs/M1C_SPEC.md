# M1C — Specifica tecnica e criteri di accettazione

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Data:** 16 novembre 2026 · **Stato:** codice C# scritto, non compilato né eseguito in .NET 10.

## Scopo

Completare un sottoinsieme di M1 senza richiedere nuove librerie: trasformazioni affini 2D, conversioni standard tra sRGB codificato e RGB lineare, gradienti immutabili. Il modulo non richiama SkiaSharp né Avalonia.

## API introdotte

| File | Contratto |
|---|---|
| `Core/Geometry/Transform2D.cs` | Metodi `Translate`, `Rotate`, `Scale`, `Then`, `Point`, `Direction`, `TryInvert`, `Invert` su `System.Numerics.Matrix3x2` |
| `Core/Colors/SrgbTransfer.cs` | `ToLinear`/`ToSrgb` IEC 61966-2-1 con input in [0,1] e rifiuto di NaN/infinito |
| `Core/Colors/LinearRgba.cs` | RGBA lineare straight, componenti finite in [0,1], conversione esplicita da/verso sRGB, `Mix` lineare |
| `Core/Colors/Gradients/ColorStop.cs` | Posizione normalizzata e colore `NormalizedRgba` |
| `Core/Colors/Gradients/GradientInterpolationSpace.cs` | Due modalità: `Srgb` e `LinearSrgb` |
| `Core/Colors/Gradients/ColorGradient.cs` | Stop ordinati, difensive copy, accesso read-only, binary search e clamp |

## Convenzioni geometriche

- `System.Numerics.Matrix3x2` usa vettori riga: un punto è trasformato come `p * M`.
- `Then(first, second)` applica **prima first e poi second**, cioè calcola `first * second`.
- `Point` usa anche la traslazione; `Direction` ignora la traslazione.
- Matrici/coordinate non finite non sono ammesse.
- Una scala con un fattore zero è lecita (collasso geometrico), ma non invertibile; `TryInvert` restituisce `false`, `Invert` lancia `InvalidOperationException`.
- Trasformazioni che producono coordinate non finite lanciano `OverflowException`.
- Nessuna implicita conversione da gradi a radianti: `Rotate` accetta radianti.

## Convenzioni colore

- `NormalizedRgba`: sRGB **codificato**, straight alpha, double [0,1].
- `LinearRgba`: RGB **lineare**, straight alpha, double [0,1].
- `Color32`: byte ARGB straight; `PremultipliedColor32`: ARGB premoltiplicato.
- Canali sRGB: per `c <= 0.04045`, `c/12.92`; altrimenti `((c+0.055)/1.055)^2.4`.
- Canali lineari: per `c <= 0.0031308`, `12.92*c`; altrimenti `1.055*c^(1/2.4)-0.055`.
- L'alpha **non** viene trasformato dalla funzione di trasferimento.
- `LinearRgba.Mix` interpola canali RGB lineari e alpha straight separatamente.
- Non sono supportati HDR, gamut mapping o spazi RGB negativi; input fuori [0,1] viene rifiutato.

## Semantica gradienti

- Minimo due stop, posizioni finite in [0,1], **strettamente crescenti**, nessun duplicato.
- Il costruttore copia gli stop; il chiamante non può modificare il gradiente attraverso l'array iniziale.
- Campioni sotto il primo stop restituiscono il primo colore, sopra l'ultimo stop l'ultimo colore; input NaN/infiniti rifiutati.
- Campioni agli stop esatti restituiscono i valori originali.
- La ricerca dello stop avviene per bisezione (O(log n)); l'interpolazione di due colori usa `Srgb` oppure `LinearSrgb`.
- RGB e alpha sono interpolati **straight**. Questo non equivale a interpolazione alpha-premoltiplicata e può essere visivamente diverso su transizioni con trasparenza; una modalità premoltiplicata resta un'eventuale funzionalità futura.
- Il gradiente non esegue blending né rendering; produce valori RGBA puri.

## Risultati numerici attesi

- `SrgbTransfer.ToLinear(0.5)` ≈ `0.21404114048223255`.
- `SrgbTransfer.ToSrgb(0.5)` ≈ `0.7353569830524495`.
- A metà tra nero e bianco: gradiente `Srgb` ≈ `0.5`; gradiente `LinearSrgb` ≈ `0.7353569830524495`.
- `Then(Translate(10,20), Rotate(π/2))` su `(1,2)` ≈ `(-22,11)`; l'ordine inverso ≈ `(8,21)`.

## Test C# predisposti

- Convenzioni di moltiplicazione e differenza punto/vettore.
- Inversione di una trasformazione, scala singolare, pivot della rotazione.
- NaN/infinito, matrici non finite e overflow geometrico.
- Campioni normativi sRGB, roundtrip e alpha indipendente.
- Gradienti multi-stop, campioni esatti, clamping, distinzione sRGB/lineare, array difensivo, posizioni duplicate e invalidi.

**Gate di accettazione:** `dotnet restore`, `dotnet build -c Release`, `dotnet test -c Release` su SDK 10; nessun test intermittente, nuovi test xUnit riusciti, nessuna regressione nei test precedenti. Non ancora soddisfatto in questa consegna.
