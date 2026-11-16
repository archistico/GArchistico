# Provenienza delle fixture di riferimento (soltanto dati)

Le fixture numeriche furono estratte durante l'audit di Gȧrt e della nuova implementazione C# M1B. **Il progetto non contiene né richiede sorgenti Kotlin, Python o harness di altri linguaggi**: solo dati JSON utili al confronto.

- `perlin_golden.json`: campioni ottenuti originariamente dall'algoritmo Perlin 3D di Gȧrt con tabella fissa.
- `permutation256_gart.json`: dati di permutazione della relativa implementazione.
- `color_kotlin_golden.json`: campioni delle conversioni HSV/HSL/OKLab dell'implementazione Gȧrt caratterizzata durante l'audit.
- `seeded_perlin3d_v1_golden.json`: dati di riferimento preliminari per la variante nuova `seeded-perlin3d-splitmix64-v1`; non certificano compatibilità con Gȧrt.
- `splitmix64_v1_golden.json`: vettori del RNG versione 1.
- `simplex_classic_golden.json`: 18 campioni della implementazione fissa `SimplexNoise.kt` del repository originale.
- `simplex_seeded_v1_golden.json`: 8 campioni della stessa formula con permutazione SplitMix64-v1; **non** è la implementazione OpenSimplex originale.

**Le fixture sono dati storici, non una prova di test C# superati.** I relativi test C# dovranno essere compilati ed eseguiti con .NET 10 e un eventuale cambio di algoritmo richiederà nuovo ID/versione. La licenza BSD-2-Clause e le attribuzioni di Gȧrt sono in `licenses/GART_BSD2.txt` e `docs/THIRD_PARTY_NOTICES.md`.
