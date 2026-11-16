# M1D — Specifica Simplex 2D/3D

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Data:** 16 novembre 2026 · **Stato:** implementato in C#, **non compilato e test xUnit non eseguiti**.

## Perimetro

M1D aggiunge il Simplex classico (permutazione fissa di Gȧrt) e il Simplex con seed per le opere proprie del framework. Entrambi sono puramente matematici e non dipendono da SkiaSharp, Avalonia, Kotlin o altre toolchain di produzione.

| Classe | API / contratto |
|---|---|
| `ClassicSimplexNoise` | `Sample2D(x,y)` e `Sample3D(x,y,z)` statici; tabella fissa condivisa con ClassicPerlin3D. `AlgorithmId = gart-classic-simplex-fixedperm-v1`. |
| `SeededSimplexNoise` | Costruttore `(ulong seed)`; `Seed`, `Sample2D`, `Sample3D`. `AlgorithmId = seeded-simplex2d3d-splitmix64-v1`. |
| `SimplexKernel` | Implementazione interna comune, senza array per campionamento; gradiente 12 direzioni, formula 2D/3D classica. |
| `NoisePermutation256` | Shuffle Fisher–Yates degli interi 0..255 via SplitMix64-v1; condiviso con `SeededPerlin3D` per evitare duplicazione della politica RNG. |

## Compatibilità e versionamento

- Classic: il risultato sui punti validi mira a riprodurre `gart/src/main/kotlin/dev/oblac/gart/noise/SimplexNoise.kt`; la tabella fissa proviene da `PerlinPermTable.kt` ed è contenuta nella libreria C# in `ClassicPerlin3D`.
- Seeded: identico kernel matematico, ma nuova permutazione stabile derivata da SplitMix64-v1; **non** equivale a `OpenSimplexNoise.kt` di Gȧrt e non è compatibile con la sua permutazione seeded.
- Le due modalità sono rappresentate da tipi separati, impedendo che una futura variazione del seed modifichi le opere classiche. Eventuali modifiche alla sequenza RNG, alla costruzione della permutazione o alla formula richiedono un nuovo `AlgorithmId`.
- Le permutazioni sono private; dopo la costruzione i campionamenti non modificano stato e possono essere chiamati contemporaneamente da più thread.

## Contratti numerici

- Input: coordinate `double` finite. `NaN` e infinito generano `ArgumentOutOfRangeException`.
- Le celle skewed devono ricadere nell'intervallo di `int`, altrimenti lanciamo `ArgumentOutOfRangeException`, invece di riprodurre la saturazione della conversione Kotlin per coordinate irrealistiche.
- Gli output teorici sono circa [-1,+1], **non vengono clamped**. Non promettiamo una perfetta normalizzazione all'intervallo chiuso.
- Il Simplex **non è periodico con uno spostamento di 256 sugli assi cartesiani**, a differenza del Perlin di partenza. Non aggiungere un test di periodicità errato.
- Campioni validi confrontati con tolleranza assoluta `1e-12`. Il risultato può differire al di sotto di questa soglia per dettagli della floating point evaluation tra piattaforme.

## Test predisposti

- 9 vettori golden 2D e 9 vettori golden 3D della formula fissa originale Gȧrt.
- 8 golden seeded (2 seed × 2 coordinate × 2 dimensioni), ottenuti dalla formula Simplex originale con permutazione SplitMix64-v1 e conservati come dati/versione nel progetto C#.
- Riproducibilità stesso seed, differenziazione fra seed, indipendenza fra versioni, risultati indipendenti dall'ordine delle chiamate, letture concorrenti, campioni all'origine e rifiuto di input non finiti o fuori griglia.
- I valori sono incorporati in `tests/GenerativeArt.Algorithms.Tests/SimplexNoiseTests.cs`; copie consultabili in `reference/gart/simplex_classic_golden.json` e `simplex_seeded_v1_golden.json`.

## Gate non ancora soddisfatti

1. Compilare con SDK .NET 10 (`dotnet build GenerativeArt.sln -c Release`).
2. Avviare tutti gli xUnit su .NET 10 e verificare i 26 golden.
3. Eseguire il workflow CI Windows e Linux.
4. Solo dopo dichiarare G0/G1 completati. Non si passa al codice di rendering M2 in assenza del gate.

Il repository contiene soltanto `.cs` per implementazione e test. L'unico script operativo è `tools/validate.ps1`. Le fixture `.json` sono dati, non programmi.
