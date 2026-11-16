# M3B — Campi vettoriali discreti e streamlines distanziate

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Stato:** implementazione scritta; **build/test xUnit di M3B non eseguiti** nell'ambiente di preparazione (SDK .NET 10 assente). La baseline M3A-R1 è stata validata dall'utente su Windows, inclusi PNG e test.

## Ambito

- `VectorGrid2D` (Algorithms/Flow): matrice immutabile di `Vector2` di dimensioni `Columns × Rows`, interpolazione bilineare, difesa contro i dati invalidi, inclusione dei nodi di bordo, vettore zero fuori dal dominio. Massimo 4 milioni di nodi (circa 32 MiB per array `Vector2`). `FromField()` acquisisce un campo continuo su griglia.
- `CurlNoiseField2D`: potenziale seeded Simplex, campionamento di un campo rotazionale `(dP/dy, -dP/dx)` con differenze finite centrate. `Frequency` e `DerivativeStep` sono parametri separati e validati. Nessuna derivata analitica al momento.
- `EvenStreamlineOptions` + `EvenStreamlineTracer`: integrazione bidirezionale con RK4 M3A, semi iniziali deterministici, ricerca spaziale di vicinato, semi successivi lungo le normali, controllo approssimato delle distanze dai tratti accettati.
- CLI `render-flow-spaced [file.png] [width] [height] [seed]` per PNG 800×600 di default; conserva `render-demo` e `render-flow`.
- Test xUnit su interpolazione, bordi, determinismo, degenerazioni, validazione dei parametri e distanza fra punti delle linee.

## Contratti numerici

- Coordinate `Vector2` in float; calcolo bilineare e distanze di vicinato internamente con `double` quando opportuno.
- Il dominio della griglia **include** i bordi come punti di campionamento; `FlowBounds` in tracing mantiene destro/inferiore esclusi. Non confondere i due contratti.
- Campo nullo: nessuna streamline; coordinate non finite: nessun campionamento valido; le due direzioni usano lo stesso seed.
- Coda massima `MaxCandidates`, linee massime `MaxLines`, massimi passi per lato, limite di 2.000.000 campioni nell'indice spaziale; budget memoria per griglie.
- Il vincolo di distanza è applicato a campioni lungo i segmenti. Il metodo non garantisce matematicamente la distanza fra tutte le curve continue quando si verificano singolarità o passi troppo lunghi.
- Il campo e la griglia sono immutabili in fase di utilizzo; l'algoritmo di tracciamento costruisce lo stato locale per invocazione.

## Differenza dai sorgenti di riferimento

- `render/field_engine.go` del Go fornisce l'idea delle streamlines RK4 separabili; `flow2` di Gȧrt fornisce un riferimento per le strategie numeriche e i buffer. La nostra implementazione è in C# e ha limiti espliciti di budget.
- La versione iniziale non include ancora LIC, derivative noise analitico, iso-contours o campi 4D ciclici.

## Verifica quando disponibile .NET 10

```powershell
.\tools\validate.ps1
dotnet run --project src/GenerativeArt.Cli -c Release -- render-flow-spaced .\output\flow-spaced.png 800 600 42
```

Il primo comando deve ripristinare, compilare e passare xUnit; il secondo deve produrre un PNG valido. Verificare anche determinismo a seed uguale, dimensioni PNG e costi di memoria nella GUI futura.
