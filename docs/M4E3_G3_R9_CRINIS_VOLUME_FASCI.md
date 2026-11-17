# M4E.3-G3-R9 — Crinis: volume collettivo, compressione e luce del fascio

**Data:** 17 novembre 2026  
**Ambito:** esclusivamente `crinis-bristles`, modalità `micro`, C#/.NET 10, SkiaSharp.  
**Origine:** prosecuzione richiesta dopo R8; priorità artistica confermata su Crinis, con documentazione aggiornata a ogni consegna.

## Problema da risolvere

Il lavoro R6–R8 ha introdotto fibre più corpose, rilievo del singolo filamento e ombre di contatto multilivello. La composizione può però apparire ancora piatta: le fibre sono illuminate individualmente, mentre la massa del fascio non ha un profilo luminoso coerente. Aggiungere soltanto spessore o densità farebbe sparire i vuoti nella trama.

## Intervento R9

1. **Compressione trasversale del fascio:** coordinata locale di ogni filamento `u ∈ [-1,1]`. Trasformazione `u' = u × [1 − c × (1 − u²)]`. I due margini restano fermi, il centro può essere compresso (`c > 0`) o espanso (`c < 0`); la derivata è `1 − c × (1 − 3u²)`, sempre almeno `0,3` entro gli intervalli permessi. Questo evita inversioni dell'ordine delle fibre.
2. **Rigonfiamento longitudinale:** scala di sezione comune a tutte le fibre `s(k) = 1 + v × [0,17 sin(0,025k + φ) + 0,10 sin(0,047k + 0,6φ)]`. Intervallo teorico `[0,73, 1,27]`. La scala viene calcolata una sola volta per punto di curva **per fascio**, non ripetuta per ogni filo.
3. **Spessore coerente con il passo reale:** il limite di diametro usa la derivata della compressione e la minima scala della sezione `0,73`, così si limita la fusione in un nastro uniforme nei punti più addensati. Rimane la saturazione continua preesistente.
4. **Illuminazione di gruppo:** un profilo trasversale stabile rende coerenti lato in luce, cresta e lato in ombra. Ombre e riflessi sono applicati **alle singole fibre**, non a un poligono pieno. La luce è più evidente nei piani frontali, le ombre rendono più leggibili quelli arretrati; il rilievo è modulato anche nei pass laterali dei fili.
5. **Nessun corpo reintrodotto:** `body-opacity=0` continua a eliminare il riempimento e l'ombra della sagoma del fascio. `shadow-strength` non deve avere effetti nel caso; restano ombre locali delle fibre.

## Controlli Studio aggiunti

| ID | Etichetta | Intervallo | Default catalogo (JSON storico) | Default Studio per nuovo lavoro |
|---|---|---:|---:|---:|
| `bundle-compression` | Compressione del fascio | `-0.35 … 0.70` | `0` | `0.28` |
| `bundle-volume` | Rigonfiamento del fascio | `0 … 1` | `0` | `0.68` |
| `bundle-lighting` | Luce sul volume del fascio | `0 … 1` | `0` | `0.82` |

Tutti sono in **Volume fascio**, disponibili solo con `detail-style=micro` e **opzionali nel JSON v1**. La loro assenza nelle vecchie ricette equivale a zero, per salvaguardare il risultato storico. Il preset Studio di un **nuovo** lavoro Crinis è intenzionalmente più plastico: non modifica i documenti personali né gli altri preset.

Il preset Studio per i nuovi lavori parte inoltre da `hair-width=1.65` (in precedenza `0.67`), così il rilievo di gruppo è visibile già al primo rendering. Questa modifica **non** riguarda il default del catalogo (`1`) né i file JSON preesistenti.

**Correzione di catalogo:** `hair-width` passa da massimo `5` a massimo `8` px. Le ricette di studio R7/R8 con valori `5.15` e `5.4` risultavano altrimenti non valide; il nuovo intervallo conserva quei lavori. La protezione locale del diametro e il budget di campioni continuano a essere applicati.

## Ricette R9

- `examples/recipes/m4e3-g3-r9/crinis-indigo-sculpted-volume-1920.json`: base di lavoro R8 con enfasi su rilievo e luce.
- `examples/recipes/m4e3-g3-r9/crinis-indigo-expanded-1920.json`: compressione negativa per una sezione più aperta, meno fasci.
- `examples/recipes/m4e3-g3-r9/crinis-copper-rimlight-1920.json`: confronto cromatico con luce laterale più intensa.

**Totale:** 27 generatori, 27 preset integrati, **156 file di esempio JSON**. Le tre ricette R9 sono nuove; le varianti originali R7/R8 restano disponibili.

## Test previsti

- In Application: preset nuovo, default neutri di ricette precedenti, presenza e opzionalità dei parametri nel catalogo, disponibilità dei controlli solo in modalità micro, validazione JSON delle ricette R7/R8/R9 e limite `hair-width=8`.
- In Rendering.Skia: modifica effettiva dei pixel per ciascun controllo di gruppo, monotonicità della mappatura su oltre 400 punti con valori negativi e positivi, diametro inferiore al passo compresso, limiti della modulazione, roundtrip vecchio JSON senza cambiamento dei byte, determinismo a seed fisso con fascio denso.
- Regole pregresse da verificare con suite intera: cancellazione, assenza di PNG parziale, `body-opacity=0`, preview a risoluzione identica all'export, limite preventivo di lavoro.

**Stato al confezionamento R9:** verifiche strutturali/JSON possibili in questo ambiente; **SDK .NET non disponibile**, quindi compilazione e xUnit Windows **non eseguiti** in questa consegna. Non indicare i test come passati senza output effettivo.

## Validazione Windows (PowerShell 7)

Estrarre lo ZIP completo e aprire PowerShell nella directory che contiene `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Lo script esegue il clean `Release`, restore, build, test di contratto del catalogo e suite completa. Per lanciare solo Crinis dopo la compilazione:

```powershell
dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~Crinis"
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~Crinis"
```

Per un confronto visivo: aprire in Studio una ricetta R9, salvare/esportare un PNG, poi portare **solo** `bundle-compression`, `bundle-volume` e `bundle-lighting` a zero. Confrontare zone in primo piano, curve in penombra e intrecci; per giudicare la microtrama usare lo zoom al 100%.

## Limiti e prossimo lavoro

R9 non è un motore di occlusione fisicamente corretto: il volume di fascio è un modello artistico 2.5D, la profondità resta basata sull'ordine di disegno e sulla morbidezza dei livelli. Prima di estendere il progetto a nuovi generatori, confrontare i risultati R8/R9 sulla stessa ricetta e sullo stesso seed; se ancora troppo piatti, progettare **auto-occlusione fra fasci** (maschera locale/height map o raster depth) con attenzione al budget memoria e senza alterare l'alpha.
