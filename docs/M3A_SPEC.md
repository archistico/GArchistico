# M3A — Campi vettoriali e traiettorie

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Data:** 16 novembre 2026. **Stato:** implementazione C# preparata; build e test specifici M3A non eseguiti nell'ambiente di produzione dello ZIP. La baseline M2B-R1 è stata compilata, testata e usata per generare PNG con successo sul Windows dell'utente.

## Obiettivo

Portare le basi di `flow2` di Gȧrt in un'API C# indipendente da SkiaSharp, senza inserire nel pacchetto sorgenti Kotlin. Il riferimento tecnico dell'analisi resta `gart/src/main/kotlin/dev/oblac/gart/flow2/VectorField.kt` e `Integrator.kt`, consultati nello ZIP originale esterno al progetto.

## Contratti

- `IVectorField2D.Sample(float x, float y)` restituisce `System.Numerics.Vector2`, senza dipendere dal rendering. X cresce a destra e Y verso il basso.
- `VectorFields`: `FromFunction`, `Constant`, `Angles`, `Vortex`, `Add` e `Scale`. Le funzioni callback non devono modificare stato condiviso se usate in parallelo.
- `FlowIntegrationMethod`: Euler, Rk2, Rk4 (1, 2, 4 campionamenti).
- `FlowIntegrator.TryAdvance`: comportamento direction predefinito (normalizzazione dei vettori, `step` in pixel); `velocity=true` usa il modulo completo (`step` in unità di tempo). Un passo negativo percorre il campo in senso contrario.
- Campi nulli, vettori più corti di `1e-6`, valori NaN/Inf, assenza di progresso o overflow interrompono l'integrazione ritornando `false` senza cambiare il punto di uscita.
- `FlowBounds`: sinistra/alto inclusi; destra/basso esclusi. Dimensioni e coordinate finite.
- `FlowTracer.Trace`: include il punto iniziale; interrompe prima di uscire dai bordi; massimo 200.000 passi per singola richiesta. Il risultato è `IReadOnlyList<Vector2>`.
- Il metodo evita allocazioni per ogni passo, ma produce una lista finale di punti, necessaria all'utente; il prossimo incremento potrà offrire un sink/streaming per traiettorie enormi.

## Prima integrazione visibile

Nuovo comando headless `render-flow [file.png] [larghezza] [altezza] [seed]` in `GenerativeArt.Cli`. Utilizza Simplex seeded e SplitMix64 per ottenere linee di flusso riproducibili, poi impiega la superficie SkiaSharp esistente. Mantiene il comando storico `render-demo` inalterato.

Esecuzione in Windows PowerShell, dalla cartella `GenerativeArt`:

```powershell
.\tools\validate.ps1
dotnet run --project src/GenerativeArt.Cli -c Release -- render-demo .\output\demo.png 800 600
dotnet run --project src/GenerativeArt.Cli -c Release -- render-flow .\output\flow.png 800 600 42
```

## Testing

Tre nuovi file xUnit: `FlowFieldsTests.cs`, `FlowIntegratorTests.cs`, `FlowTracerTests.cs`.
Comprendono test di vettori costanti e vortici, composizione e validazione; test di precisione numerica e numeri di campionamenti Euler/RK2/RK4; arresto su NaN, infinity e velocità nulla; verifica di rettangoli semichiusi, percorsi costanti, limiti di passi, parametri non validi e determinismo.

Non si dichiara che i nuovi test siano passati senza esecuzione `dotnet test`. Il test visuale manuale deve accertare che `flow.png` venga creato, si apra e contenga linee visibili; la presenza di un file PNG da sola non costituisce prova di un rendering corretto.

## Aree successive M3B

- Griglia campionata `VectorGrid2D` (interpolazione bilineare) e test sui bordi.
- Flow fields derivati da curl del noise con differenze centrali.
- Tracciamento bidirezionale e streamlines con distanza minima.
- Opera geometrica `Spirograph` e test di output visivo.
- Benchmark su grandi volumi e confronto Windows/Linux per eventuali differenze numeriche.

## Rischi noti

- Non garantita uguaglianza bit-a-bit con Gȧrt: uso di `System.Numerics.Vector2` e interpolazione `float`; puntiamo alla compatibilità semantica e ai test numerici.
- Il renderer attuale crea una `SKPaint` per segmento; un batching con `SKPath` sarà opportuno se si produrranno molte centinaia di migliaia di segmenti.
- I campi definiti con callback possono lanciare eccezioni: il framework non le ingoia intenzionalmente.
