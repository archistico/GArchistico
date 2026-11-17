# Cincinnus C1 — kernel geometrico autonomo C#

**Revisione:** M4E.3-Cincinnus-C1, 17/11/2026. **Origine:** G4-R2-H3. **Stato:** implementazione C# consegnata, **build/test .NET della C1 da verificare su Windows** (l'ambiente di preparazione non dispone di `dotnet`). **Ultima baseline compilata e testata dall'utente:** G4-R2. Questa milestone NON introduce un nuovo renderer e NON completa Cincinnus.

## 1. Perimetro esatto

La richiesta è realizzare **il primo kernel geometrico C#** di Cincinnus, senza anticipare l'integrazione artistica. In `GenerativeArt.Algorithms/Cincinnus/` sono stati aggiunti:

- `CincinnusFlowField.cs`: vortici, selezione maximin dei centri esterni, campo composito deterministico.
- `CincinnusTraceOptions.cs`: parametri passo/lunghezza/margine con default originali della variante completa.
- `CincinnusPathGeometry.cs`: tracciamento RK4 avanti, indietro e concatenato; primo ritorno vicino al seed; stima dei raggi di curvatura.
- `tests/GenerativeArt.Algorithms.Tests/CincinnusGeometryTests.cs`: test numerici, determinismo, bounds, input invalidi, cancellazione e compatibilità fisica con il vortice comune esistente.

**Nessuna modifica** a `CrinisMicroRenderer`, `GartBrushRenderer`, Studio, CLI, catalogo, ricette, JSON, preset, shader, budget 8K, file di solution o progetti. La cartella di sorgenti è automaticamente inclusa dal progetto SDK-style `GenerativeArt.Algorithms.csproj`; la nuova classe di test è automaticamente inclusa da `GenerativeArt.Algorithms.Tests.csproj`.

## 2. Origine matematica verificata nel sorgente Gȧrt

Fonti allegate dall'utente (non incluse nel nostro ZIP):

- `arts/lines/src/lines/cincinnus/Cincinnus.kt`: `field()`, `offPage()`, `strokeFrom()`, `bends()`;
- `arts/lines/src/lines/cincinnus/CincinnusSimple.kt`: `field()`, `hasEye()`, `cut()`;
- `gart/src/main/kotlin/dev/oblac/gart/flow2/VectorField.kt`: legge effettiva di `vortex()` e somma di campi;
- `gart/src/main/kotlin/dev/oblac/gart/flow2/Integrator.kt`: RK4 normalizzato, stop su vettori nulli o bordi;
- `gart/src/main/kotlin/dev/oblac/gart/flow2/Streamlines.kt`: posizionatore separato (ancora fuori da C1).

Per ogni centro \(c_i=(c_{x,i},c_{y,i})\) e punto \(p=(x,y)\), il vortice contribuisce con

\[
\mathbf v_i(p)=\frac{1}{\sqrt{r_i^2}\left(1+r_i^2/R_i^2\right)}
\begin{bmatrix}-\Delta y_i s_i\\ \Delta x_i s_i\end{bmatrix},\qquad
r_i^2=\Delta x_i^2+\Delta y_i^2.
\]

Al centro \(r_i=0\), il contributo è zero. La somma \(\sum_i\mathbf v_i\) **non aggiunge drift**: una cancellazione esatta produce vettore zero, e l'integratore interrompe il percorso. Nelle coordinate canvas X cresce a destra e Y verso il basso; lo spin negativo usato da Cincinnus produce rotazione antioraria.

**Selezione dei centri:** l'originale propone per ciascuno dei cinque centri 12 candidati esterni nel rettangolo `[-0.4N, 1.4N]` (su ciascun asse), richiede un'uscita di oltre `0.08N` e sceglie il candidato che massimizza la minima distanza dai centri già scelti. C1 conserva il criterio e i range. Spin = `-(0.6..1.4)`; reach = `0.3N*(0.7..1.3)`. Il valore di partenza è seed `3`, proprio della variante completa.

La geometria originale nasce su tela quadrata 2000×2000. **Estensione intenzionale per canvas rettangolari:** X scala con larghezza, Y con altezza, while `N` per reach e margine di esclusione viene sostituito dal **lato minore**. Questa è una nostra scelta, non comportamento documentato dall'originale.

**RNG non bit-identico:** Gȧrt impiega `kotlin.random.Random` e una strategia separata per le sequenze geometriche. C1 usa `SplitMix64Random` e due sotto-stream deterministici `cincinnus/positions/v1` e `cincinnus/dynamics/v1`. Stesso seed e parametri in C# devono generare lo stesso campo; **il seed 3 Kotlin non produrrà le stesse coordinate**. Il numero di tentativi per scegliere un punto esterno è limitato a 4096 per prevenire loop infiniti.

## 3. Tracciamento e misure

`CincinnusPathGeometry.TraceForward` e `TraceBackward` **riusano** `FlowTracer` con `FlowIntegrationMethod.Rk4`; non introducono una seconda implementazione dell'integratore. L'integratore normale utilizza la **direzione** del campo e interpreta il passo come distanza, non come tempo. Backward imposta passo negativo.

Valori di default riferiti alla variante completa originale 2000×2000:

| Parametro | Valore | Motivazione |
|---|---:|---|
| `StepSize` | 3 pixel | Campionamento della spina (`DS`) |
| `MaximumLength` | 9.000 pixel | Budget massimo della traccia in avanti (`MAXLEN`) |
| `Margin` | 500 pixel | Dominio esteso `[-500, 2500]²` (`MARGIN`) |
| `MaximumStepsPerDirection` | 20.000 | Tetto protettivo C# addizionale |
| `FindFirstReturn` | allontanamento >30 pixel, ritorno <2 passi | Identificazione del primo cappio in `strokeFrom()` |
| `EstimateBendRadii` | finestra ±4 segmenti | Stima originale `bends()`, rettilineo = 1.000.000 |

`TraceBidirectional` concatena tracce indipendenti di pari budget senza duplicare il seed: è una **primitiva nostra per il C2**, non una pretesa traduzione letterale di `strokeFrom()`, che ricostruisce il tratto all'indietro in base alla testata del pennello.

**Limiti:** canvas da 1 a 32.768 pixel per lato, 1–12 vortici generati, 1–64 candidati per vortice, fino a 20.000 passi per direzione, margin finito non negativo, input e condizioni numeriche controllati. Il tracciamento e la generazione di centri ricevono `CancellationToken`. Le funzioni non mantengono cache condivisa: la determinazione dipende da seed, parametri e versione della logica.

## 4. Contratto minimo per la prossima fase

Esempio di utilizzo **da codice C#**, NON un comando CLI o una ricetta Studio:

```csharp
using GenerativeArt.Algorithms.Cincinnus;
using System.Numerics;

var field = CincinnusFlowField.Create(width: 2000, height: 2000, seed: 3);
var path = CincinnusPathGeometry.TraceForward(
    field, new Vector2(1000, 1000), 2000, 2000);
var bends = CincinnusPathGeometry.EstimateBendRadii(path);
int firstReturn = CincinnusPathGeometry.FindFirstReturn(path, stepSize: 3);
```

La traccia può essere vuota oltre al seed se il campo è nullo; `EstimateBendRadii` richiede almeno due punti. Le API sono **additive** e non registrano automaticamente un renderer.

## 5. Cosa manca per un Cincinnus disegnabile

- **C2:** selezione e collocazione delle streamline `Simple`, distanza tra linee, test di arresto e collisioni; eventuale filtro `hasEye()` specifico della variante Simple; debug geometrico attendibile.
- **C3:** trasformazione da traccia a `SpinePoint`, correzione delle curvature, testa irregolare, corpo, ombre, setole e pigmento.
- **C4:** griglia di copertura e stratificazione main/filler/patch della versione `Cincinnus.kt`.
- **C5:** ID artwork e preset, controlli colore/parametri in Studio e CLI, JSON, confronto visivo con una PNG originale realmente attribuita, benchmark, UI, test cross-platform e qualità.

**Non aggiungere** ora preset fittizi Cincinnus né etichettare generazioni Crinis come Cincinnus. Crinis R10 e G4-R2 restano immutati; il confronto artistico Ocellus/Rete è ancora aperto.

## 6. Verifica Windows obbligatoria dopo questa consegna

Aprire PowerShell 7 nella root con SDK .NET 10. Dopo aver estratto lo ZIP integrale:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Lo script ripulisce, ripristina le dipendenze NuGet, compila **Release** e lancia la suite xUnit dell'intera solution. L'esito atteso è **build riuscita, tutti i test verdi**; il conteggio dipende dalle versioni delle suite. Per eseguire anche il filtro C1, senza ripetere gli altri test:

```powershell
dotnet test .\tests\GenerativeArt.Algorithms.Tests\GenerativeArt.Algorithms.Tests.csproj -c Release --filter FullyQualifiedName~CincinnusGeometryTests --logger "console;verbosity=normal"
```

Se fallisce: conservare **il primo errore** e il relativo file/riga, oltre al sommario della suite; non considerare C1 validata finché il log di compilazione e test non è positivo. Questi comandi non modificano preset e non producono automaticamente PNG di Cincinnus.

## 7. Stato dei controlli

- **Verifica statica:** file C# e test presenti nella posizione SDK-style corretta; nessuna modifica al catalogo o alle risorse storiche; manifest SHA-256 e ZIP ricalcolati. Vedere [`validation/CINCINNUS_C1_STATIC_QA.md`](validation/CINCINNUS_C1_STATIC_QA.md).
- **Build/test eseguiti in questa consegna:** NON eseguiti (SDK .NET non presente nell'ambiente di preparazione). Questo non invalida la baseline precedente G4-R2, ma non equivale a una certificazione del codice C1.
- **Test estetici / renderer:** non applicabili a C1. Nessun rendering Cincinnus viene esposto nello Studio.
