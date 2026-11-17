# Cincinnus C4 — Distribuzione Adaptive (17/11/2026)

## Stato e provenienza

- **Base di lavoro:** pacchetto integrale **M4E.3-Cincinnus-C3**, senza modificare Crinis R10 o Ocellus/Rete G4-R2.
- **C1 e C2:** compilazione/test superati su Windows, confermati dall'utente il 17/11/2026.
- **C3:** implementata (stroke planner e painter Skia), **non ancora compilata/testata su Windows né approvata visivamente**.
- **C4:** nuovi sorgenti e test predisposti; **non ancora compilata/testata su Windows**, perché manca l'SDK .NET nell'ambiente di preparazione.
- **Catalogo invariato:** 29 generatori, 29 preset embedded, 173 esempi JSON. Tool preview separato dalla solution, nessun nuovo renderer registrato.

## Riferimento primario

Archivio Gȧrt fornito dall'utente: `arts/lines/src/lines/cincinnus/Cincinnus.kt`, righe 64–80 (costanti), 94–105 (testa), 107–135 (vortici e spostamento laterale), 150–227 (stroke/uncini), 230–281 (cover), 296–362 (main/filler/patch e ordine di posa). Repository: https://github.com/igr/gart.

**C4 non è una traduzione letterale**, e non promette equivalenza pixel/seed con Kotlin. C# usa SplitMix64, campo/trace C1, geometria indipendente dal raster e parametri scalabili per rettangoli e preview.

## API create

| File | Responsabilità |
| --- | --- |
| `CincinnusAdaptiveOptions.cs` | 60 principali, 1000 tentativi, 300 patch, griglia 20 px, passo 3, margine 500, cap espliciti |
| `CincinnusCoverageGrid.cs` | cella meno occupata via reservoir sampling; una marca per stroke/cella; testata, pigmento asciutto, riduzione coda |
| `CincinnusAdaptivePlacer.cs` | forward/backward RK4, scelta raggio/semilarghezza, testa irregolare, larghezza guidata dalla curvatura, riempimenti, taglio uncini, secondo cover, patch |
| `CincinnusAdaptiveLayout.cs` | collezioni readonly Main/Filler/Patches, ordine di posa, metriche prima/dopo patch, flag budget |
| `CincinnusAdaptivePlacementTests.cs` | nuovi test xUnit, da eseguire Windows |

### Fasi di posa

1. Selezionare un punto in una delle celle con occupazione minima; tentare `strokeFrom`; se non valido provare un seed laterale e penalizzare il primo punto.
2. Accettare una pennellata e segnare le celle occupate in modo **binario per stroke**; continuare fino al raggiungimento del numero di principali e della copertura, rispettando il budget.
3. Dividere gli stroke accettati in `main` e `filler` nell'ordine di accettazione, ordinarli per raggio decrescente e rimuovere tratti troppo a uncino con `TrimHooks`.
4. Ricostruire una **seconda griglia** dalla geometria davvero conservata dopo il taglio: questo evita di attribuire copertura a tratti poi eliminati.
5. Aggiungere le patch nella griglia ricostruita con massimo 300 tentativi; **ordine di rendering**: patch sotto, poi filler, quindi main.
6. Segnalare quando un limite interrompe il lavoro; riportare sempre `UncoveredCellsBeforePatches` e `UncoveredCellsAfterPatches`. Il conteggio di celle non è una misura fisica in percentuale di pixel: è copertura approssimata su griglia.

### Budget e determinismo

- `MaximumPlacementAttempts`, `MaximumPatchAttempts`, `MaximumTotalStrokes`, `MaximumSpinePoints`, `MaximumCoverageCells`, massimo 20.000 passi RK4 per direzione.
- Check di cancellazione durante placement, tracciamento e cover; le liste di risultati espongono viste di sola lettura.
- Due esecuzioni con stesso campo, seed e opzioni devono produrre la stessa geometria. Seed diversi non corrispondono a quelli di Kotlin.
- Nessuna modifica a `CincinnusSimplePlacer`, `CincinnusStrokePlanner`, renderer esistenti e preset.

## Differenze tecniche aperte con l'originale

- Il profilo di testata e la modulazione di larghezza usano rumore armonico deterministico e un filtro di curvatura **conservativo**; non sono ancora una riproduzione identica del noise originale, né della doppia finestra `lows`/`rooms`.
- `low` è una preferenza di terminazione nella metà inferiore, ma non riproduce il medesimo consumo di PRNG di Kotlin.
- Le patch non subiscono unhook, come nel sorgente. I main e i filler sì.
- Il renderer di anteprima **riutilizza C3** su un'unica lista `patch → filler → main`: ombre, fibre e variazioni cromatiche C3 non equivalgono alla posa/codifica nativa dell'originale Gȧrt; i puntini non sono per ora intercalati esattamente al medesimo indice.
- La C4 rimane **geometria e tool di prova**, non un generatore Studio/CLI. Riconciliazione artistica completa in C5 e follow-up di pittura dopo la visione dei PNG.

## Istruzioni Windows — PowerShell 7

Prerequisiti: SDK **.NET 10**, PowerShell 7, cartella `GenerativeArt` estratta interamente. Nessun SDK Kotlin o Java richiesto.

Gate canonico (include i test `CincinnusAdaptivePlacementTests`, ma non la tool preview separata dalla solution):

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Test selettivi:

```powershell
dotnet test .\tests\GenerativeArt.Algorithms.Tests\GenerativeArt.Algorithms.Tests.csproj -c Release --filter "FullyQualifiedName~CincinnusAdaptivePlacementTests"
```

Build della tool (completa e obbligatoria per provare il PNG):

```powershell
dotnet build .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release
```

Preview confronto stesso seed, stessa risoluzione, geometria diversa:

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release -- .\output\cincinnus-simple-seed3.png 3 768 768 simple
dotnet run --project .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release -- .\output\cincinnus-adaptive-seed3.png 3 768 768 adaptive
```

**Risultato atteso:** due PNG distinti, output con numero di main/filler/patch e celle scoperte per adaptive. Se compaiono messaggi di budget incompleto, non considerarli render completi. Non cancellare i PNG: confrontare ingrandendo al 100%, verificare colmi di pigmento, testate, scarti, curvatura, ombre, ritmo e aree troppo vuote. Nessun A/B visivo è attestato al momento.

## Stato roadmap

- **C1** kernel vortici/RK4: Windows OK.
- **C2** linee equidistanti Simple: Windows OK.
- **C3** stroke/pennello Skia/PNG: codice pronto, Windows e QA artistico ancora da effettuare.
- **C4** Adaptive main/filler/patch: codice e test pronti, Windows e QA artistico ancora da effettuare.
- **C5** integrazione Studio/CLI/JSON/controlli per-colore, comparazione visiva, performance e qualità: non iniziata.
