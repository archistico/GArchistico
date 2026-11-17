# Cincinnus C2 — posizionatore geometrico Simple (17 novembre 2026)

**Stato:** implementato in C#/.NET 10 come codice `Algorithms`, con test xUnit aggiunti; **build e test C2 NON eseguiti nell'ambiente di preparazione, privo dell'SDK .NET**. La precedente **C1 è stata compilata e testata con successo dall'utente** il 17/11/2026; non attribuire automaticamente tale esito al nuovo codice C2. Nessun renderer Cincinnus è stato registrato nello Studio o nella CLI.

## Obiettivo

Da un campo vettoriale C1 ottenere un insieme ordinato di traiettorie *Simple* distanziate, con punti, lunghezza geometrica, distanza dalle altre linee (*clearance*) e coefficiente *taper*. Questi dati saranno l'input di C3 per suddividere le linee in pennellate e costruire le spine delle setole. **Non è ancora un'immagine o un porting artistico completo.**

## Sorgenti studiati

- Gȧrt allegato dall'utente: `arts/lines/src/lines/cincinnus/CincinnusSimple.kt` (invocazione a `flow.streamlines`, `GAP=110`, `DS=3`, `minLength=300`, `margin=3*GAP*1.25`).
- Gȧrt: `gart/src/main/kotlin/dev/oblac/gart/flow2/Streamlines.kt` (`Placer`, `SpacingGrid`, `seedAlong`, `isFree`, `grow`, `clearance`, `taper`).
- Motore C1: `CincinnusFlowField`, `FlowIntegrator.TryAdvance` (RK4), `CincinnusPathGeometry` (misure e strumenti generali).

La logica è **reimplementata in C#**, senza dipendenze runtime da Kotlin/Go né redistribuzione dei sorgenti originali.

## Nuove API

| File | Contratto |
|---|---|
| `CincinnusSimpleOptions.cs` | Separazione, distanza di arresto, passo RK4, lunghezze, margine e budget di sicurezza |
| `CincinnusSimpleLayout.cs` | `CincinnusStreamline` (punti readonly, clearance readonly, lunghezza, `Taper(i)`), `CincinnusSimpleLayout` (campo, linee, flag di truncation, semi provati) |
| `CincinnusSimplePlacer.cs` | `Generate(width,height,seed,options,token)` e `Place(field,width,height,seed,options,token)` |
| `CincinnusSimplePlacementTests.cs` | Regressioni geometriche e di sicurezza con campi sintetici, vortici e seed |

### Algoritmo

1. Creare una griglia di semi nel canvas esteso: i centri delle celle di larghezza `Separation`, con parte dell'ultima cella dentro il bordo, in ordine pseudo-casuale deterministico.
2. Estrarre un seme dalla coda dei candidati laterali (o dalla griglia iniziale, quando la coda è vuota). Accettarlo solo se distante almeno `max(TestDistance, 0.99*Separation)` da qualunque traccia già accettata.
3. Crescere in avanti usando RK4 a direzione normalizzata e poi all'indietro con il budget di lunghezza rimasto. Arrestare su bordo esteso, mancanza di direzione, distanza `< TestDistance` dalle altre linee o ritorno vicino a una parte *non locale* della stessa linea. La distanza firmata dal seme distingue i punti locali da una vera auto-intersezione.
4. Se la linea è più corta di `MinimumLength`, rimuoverne **tutti** i punti dall'indice spaziale, rendendo nuovamente libero quello spazio.
5. Accettare la linea in ordine di flusso (`backward reversed + seed + forward`) e generare due semi per ogni punto, su entrambi i lati della normale locale a distanza `Separation`.
6. Al termine valutare la distanza di ogni punto della linea più vicina, saturata a `Separation`. `Taper(i) = clamp((clearance[i]-TestDistance)/(Separation-TestDistance),0,1)`; se le due distanze coincidono, `Taper=1`.

L'indice è un dizionario di celle quadrate di lato `Separation`; per le collisioni è sufficiente guardare le celle 3×3 adiacenti, perché `TestDistance <= Separation`. Il contenuto è **per invocazione**, senza cache o RNG globale. Cancella tramite `CancellationToken` anche durante il posizionamento e la calcolazione finale della clearance.

## Default C2 e differenze intenzionali da Gȧrt

| Parametro | Default | Nota |
|---|---:|---|
| Seed `Generate` | 1 | CincinnusSimple originale, ma sequenza RNG **non identica** a Kotlin |
| `Separation` | 110 px | GAP originale |
| `TestDistance` | 55 px | 0.5*GAP, default di `streamlines()` |
| `StepSize` | 3 px | RK4 originale |
| `MinimumLength` | 300 px | Valore passato da Cincinnus Simple |
| `MaximumLength` | 9000 px | Limite **nostro**, Gȧrt usa infinito per Simple |
| `Margin` | 3.75*GAP = 412.5 px | `3 * hw`, con `hw=1.25*GAP` |
| `MaximumLines` | 2.048 | Limite C# di protezione |
| `MaximumPoints` | 250.000 | Limite C# di protezione |
| `MaximumPendingSeeds` | 500.000 | Evita crescita incontrollata della coda |
| `MaximumCoverCells` | 150.000 | Evita coperture eccessive con GAP minuscolo |

Con budget raggiunto la proprietà `SafetyLimitReached` **diventa true**: il chiamante non deve considerare la copertura completa. Un limite richiesto che non può essere rispettato (`step > TestDistance`, migliaia di celle oltre budget, float non finiti) produce subito eccezione. La capacità `MaximumLength / StepSize` non può eccedere 40.000 passi; gli input massimi non rappresentano una promessa prestazionale 8K.

**Non ancora implementato:** filtro dei *vortex eyes* `hasEye()` della variante `CincinnusSimple.kt` (può comportare linee con centri interni), taglio in pennellate (`cut`), ordinamento per curvatura delle pennellate, testa irregolare, setole, pigmento, ombre e grana. Sono previsti nei passi C3/C4 e in successive verifiche estetiche. La variante Adaptive `Cincinnus.kt` non usa questo posizionatore con griglia equidistante e richiederà una diversa distribuzione.

## Esempio C# (API, non comando CLI)

```csharp
using GenerativeArt.Algorithms.Cincinnus;

var layout = CincinnusSimplePlacer.Generate(2000, 2000, seed: 1);
foreach (var line in layout.Lines)
{
    // line.Points: curva in ordine lungo il campo, sola lettura
    // line.Clearance e line.Taper(i): futura guida allo spessore
}
if (layout.SafetyLimitReached)
    throw new InvalidOperationException("La distribuzione è stata interrotta da un budget di sicurezza.");
```

Questo esempio **non** esporta immagini: la geometria si renderà con C3/C5.

## Validazione Windows

Dopo aver estratto lo ZIP completo, aprire **PowerShell 7** nella directory con `GenerativeArt.sln`, con SDK .NET 10 installato:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando pulisce e lo script effettua restore, build Release e test completi. Una successiva verifica mirata non obbligatoria:

```powershell
dotnet test .\tests\GenerativeArt.Algorithms.Tests\GenerativeArt.Algorithms.Tests.csproj -c Release --filter "FullyQualifiedName~CincinnusSimplePlacementTests" --logger "console;verbosity=normal"
```

**Atteso:** compilazione riuscita e tutti i test superati. **Esito della nuova C2: da confermare su Windows.** La C1 della precedente consegna è confermata positiva, con conteggio test non fornito. Lo stato di qualità artistica G4-R2 resta separato: nessun A/B approvato.

## Prossimi passi

- **C3:** da linea a pennellate: tagli/overlap e ordinamento per curva, costruzione spine e modello delle testate, setole/ombre e primi PNG artistici; anteprima e palette precise.
- **C4:** distribuzione Adaptive con griglia di copertura *main/filler/patch*.
- **C5:** registrazione Studio/CLI/JSON, controlli professionali, comparazioni A/B, bench e packaging.

Gli stream `SplitMix64` e i seed C2 sono riproducibili all'interno della stessa versione, non pixel-identici all'implementazione Kotlin. Crinis R10, G4-R2 e i 17 preset personali rimangono immutati.
