# M4A.1-R4 — Campi scalari, Marching Squares e Curve topografiche

**Data:** 16 novembre 2026. **Stato:** M4A.1-R2 validata su Windows (build, test, Studio e CLI). R3 compila, ma fallisce 1 test su 371 per un limite di 48 milioni superiore alla complessità del caso che si voleva bloccare. **R4** corregge il limite a 40 milioni senza limitare le esportazioni 1600×1600 e 4000×4000 standard; successivamente l'utente ha confermato «funziona tutto» su Windows.

## Obiettivo e confini dell'incremento

Primo passo della M4A: isolinee con raccordo, campo scalare deterministico, primo artwork artistico utilizzabile con CLI e Studio senza nuovi controlli GUI. **Non è la chiusura completa della M4A**: SDF 2D/CSG e Poisson sono stati implementati nella successiva M4A.2-R1 (ancora da validare); tassellazioni restano in roadmap.

## Algoritmo riutilizzabile (`GenerativeArt.Algorithms/Contours`)

- `MarchingSquares.Trace(ReadOnlySpan<float>, columns, rows, level, CancellationToken)` riceve una griglia row-major con campioni finiti; restituisce curve con punti `Vector2` in coordinate di griglia e proprietà `Closed`.
- Le intersezioni dei bordi sono calcolate con interpolazione lineare; gli archi condivisi vengono valutati con orientamento coerente e raccordati per coordinate esatte, senza rounding né quantizzazione.
- Nei casi ambigui 5 e 10 viene utilizzato il determinante del bilineare; in parità si usa il valore al centro. Si evitano segmenti di lunghezza nulla; i valori esattamente sulla soglia sono considerati interni.
- Vengono processate prima le curve aperte e poi i cicli, con ordine stabile. Input non valido, non finito, griglia oltre 4.194.304 campioni e cancellazione generano errori espliciti.
- Limite noto: un vertice esattamente sull'isolivello può creare un raccordo di grado maggiore di 2; la politica deterministica segue il primo arco non ancora visitato. Non rivendichiamo un ordinamento topologico canonico in queste singolarità.

## Generatore `topographic-contours` v1

- Noise Simplex 2D seedato con quattro ottave su una griglia regolare.
- Colorazione topografica per bande con interpolazione bilineare del medesimo campo campionato; contorni antialias disegnati con path Skia connessi, non una draw call per ciascun segmento. **R2:** costruzione delle polilinee con `SKPathBuilder.MoveTo` e `LineTo`, e `Detach()` per il path finale; `SKPath.MoveTo/LineTo` risulta obsoleto in SkiaSharp 4.153.2 e impediva la build con warnings-as-errors. **R3:** budget topografico riallineato al tetto statico del runtime per i PNG ad alta risoluzione.
- Parametri: `frequency` (0.002–0.05, default 0.012); `contours` (4–24, default 12); `cell-size` (3–20 pixel, default 6); `line-width` (0.5–3, default 1.15); palette `atlas`/`nocturne`/`terra`.
- Limiti: almeno 16×16 e non oltre 16.777.216 pixel (fino a 4096×4096 nel runtime statico corrente). Inoltre celle × livelli fino a 40.000.000. Cancellazione controllata durante campionamento, colorazione e tracciamento.
- Contratti invariati: JSON `formatVersion=1`, `algorithmVersion=1`, seed `UInt64`, niente animazione per questo artwork; nessun aggiornamento distruttivo alle vecchie ricette.
- Il nuovo artwork appare automaticamente nello Studio (catalogo comune): anteprima, pan/zoom, export PNG e preferiti sfruttano i componenti esistenti. Non è dichiarata parità pixel-per-pixel con opere Gȧrt, solo ispirazione estetica.

## Comandi Windows — PowerShell 7, nella cartella di `GenerativeArt.sln`

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Per generare il PNG di esempio (la destinazione deve essere libera):

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release -- describe topographic-contours
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-contours.json .\output\topographic-m4a1.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-poster-4000.json .\output\topographic-poster-4000.png
```

## Gate e smoke visivi

1. `validate.ps1` completamente verde, inclusi nuovi `MarchingSquaresTests`, test render topografico, roundtrip, catalogo aggiornato a **6** ID esatti e `SurfaceDrawPolylineScalesPointsWithoutClosingPath` per verificare la migrazione SKPathBuilder.
2. Aprire Studio, cercare **Curve topografiche**, variare `contours` e `frequency`, cambiare palette, verificare che la preview si aggiorni e che pan/rotella restino invariati.
3. Esportare PNG 800×600, 1600×1600 e almeno un 4000×4000. Confrontare due generazioni con stesso seed e stesso backend (determinismo); seed diverso/parametri diversi devono generare risultati differenti.
4. Provare un export oltre i limiti o con combinazioni troppo dense (`cell-size=3`, `contours=24` su 4000×4000): rifiuto controllato, nessun file parziale, Studio utilizzabile dopo l'errore.
5. Controllare visivamente contorni nei bordi, iso-cicli chiusi, assenza di linee spurie e tempi/allocazioni su risoluzioni medie. Test numerici + PNG decodificabile non equivalgono a uno smoke estetico.
6. In uno smoke di regressione, preferiti/tema, pan/zoom e Play/Pausa di `looping-flow` devono restare funzionanti; il nuovo artwork è statico.

**Non eseguiti qui:** `dotnet build`, xUnit, rendering reale PNG e test di interfaccia Windows: SDK .NET 10 assente. Non confondere i controlli ZIP/JSON e il codice test con un gate eseguito.

## Seguito della M4A

1. SDF 2D con boolean operations e trasformazioni.
2. Campionamento Poisson con budget e determinismo.
3. Tassellazioni e nuovi artwork visualmente caratterizzati; validazione con riferimenti artistici.
4. M4B noise derivativo e ciclico, domain warp, loop di alta qualità, mantenendo in priorità le animazioni.
