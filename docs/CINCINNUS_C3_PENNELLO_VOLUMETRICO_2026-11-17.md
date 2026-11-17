# Cincinnus C3 — pennellate e setole (17 novembre 2026)

**Stato operativo:** l'utente ha confermato il 17/11/2026 **build e test Windows superati per C1 e C2**, senza comunicare il conteggio. **C3 è stata implementata, ma la compilazione e i test di C3 non sono ancora stati eseguiti in questo ambiente privo di SDK .NET.** Prima consegna visiva sperimentale: **non** dichiarare Cincinnus finito né esteticamente approvato.

## Obiettivo

Trasformare le traiettorie Simple C2 in vere pennellate pittoriche, con segmentazione e sovrapposizione, spine campionate, semilarghezza controllata dalla curvatura, testate oblique, carico pigmento, fibre, luce e ombra. Conservare la distinzione tra **Simple (C2/C3)** e **Adaptive (C4)**. Non modificare il generatore Crinis R10, G4-R2 e i JSON storici.

## File C3

| Percorso | Responsabilità |
|---|---|
| `src/GenerativeArt.Algorithms/Cincinnus/CincinnusStrokeOptions.cs` | Controlli di taglio, overlap, spessore, essiccazione e budget |
| `src/GenerativeArt.Algorithms/Cincinnus/CincinnusBrushStroke.cs` | SpinePoint, stroke readonly, head, load, bend, risultato con flag budget |
| `src/GenerativeArt.Algorithms/Cincinnus/CincinnusStrokePlanner.cs` | Da `CincinnusSimpleLayout` a strokes, testata e ordine di posa |
| `src/GenerativeArt.Algorithms/Cincinnus/CincinnusSimpleLayout.cs` | Una sola estensione compatibile: proprietà `Separation`, nessun cambio alla logica C2 |
| `src/GenerativeArt.Rendering.Skia/CincinnusPaintOptions.cs` | Palette e controlli di materia, ombra, illuminazione, punti |
| `src/GenerativeArt.Rendering.Skia/CincinnusBristlePainter.cs` | Painter `SKCanvas` dedicato, senza registrazione nel catalogo |
| `tools/GenerativeArt.CincinnusPreview/` | Programma console C# che genera PNG di prova anche senza Studio |
| `tests/GenerativeArt.Algorithms.Tests/CincinnusStrokePlannerTests.cs` | Unit test delle spine, budget, testata, determinismo e parametri |
| `tests/GenerativeArt.Rendering.Skia.Tests/CincinnusBristlePainterTests.cs` | Test dei PNG, palette, pitch, seed e cancellazione |

## Porting del modello originale

Sorgenti Gȧrt analizzati: `arts/lines/src/lines/cincinnus/CincinnusSimple.kt` (`cut`, `strokeOf`, `bends`, `headOf`, `shadow`) e `gart/src/main/kotlin/dev/oblac/gart/brush/bristles.kt` (`Spine`, `Bristles`, `drawBristles`). Nessun sorgente Kotlin incorporato o utilizzato a runtime.

1. **Taglio:** per linea si formano pezzi fino a 1.600 px, con primi tratti da almeno 200 px e successivi da almeno 600 px di lunghezza obiettivo; una parte del precedente si sovrappone al successivo per 150–350 px. La spina effettiva contiene almeno 16 punti. Su tele piccole lo strumento di anteprima scala i valori; il planner usa pixel logici espliciti.
2. **Curvatura:** si confrontano direzioni a quattro campioni prima e dopo; il raggio locale limita la semilarghezza a `0.8 * min(raggi nella finestra)`; normale calcolata dalle posizioni adiacenti. Ordine di posa a curvatura crescente.
3. **Testa:** profilo trasversale su 129 campioni, con andamento curvo, slant, irregolarità e valori non negativi; ogni setola comincia dopo il proprio `LandingOffset`.
4. **Pigmento:** `Load(t)=1-t^4` per default, con progressiva rarefazione e schiarimento. Sono presenti gap locali e grandi streak asciutte, campionate in modo deterministico per seed.
5. **Profondità:** corpo pigmentato sotto le fibre, doppia ombra morbida e modulazione della luce in funzione della normale e della posizione trasversale della setola.
6. **Colore:** palette originale crimson a otto campioni, passata come proprietà sostituibile. **La prima C3 interpola i canali RGB linearmente**, non usa la interpolazione percettiva OKLab originale. Questo è un limite di fedeltà cromatica da risolvere nella rifinitura artistica.

### Differenze deliberate e ancora aperte

- I seed C# SplitMix64 sono riproducibili nel nostro codice, **non bit-identici a Kotlin**. Rumore e disegno delle setole riproducono il comportamento pittorico generale ma non replicano le specifiche primitive `noise`/`drawBrush` dell'originale.
- Il painter C3 può usare i propri parametri direttamente; **non** è ancora registrato come generatore Studio o CLI, non ha ricette/preset nuovi e non supporta la modalità Adaptive. Il catalogo rimane 29 generatori / 29 preset / 173 JSON di esempio.
- La preview usa un tool autonomo limitato a **256–2048 px**, con coordinate e parametri scalati rispetto al riferimento originale 2000². Non equivale ad autorizzazione 8K universale o a rendering alla risoluzione finale dello Studio.
- Il comando di prova può produrre una prima PNG, ma **nessuna PNG è stata prodotta/valutata come evidenza artistica in questa consegna**: ambiente di preparazione senza SDK e librerie native .NET. Il gate visivo reale è ancora aperto.
- La verifica `hasEye` del campo e il controllo di copertura main/filler/patch della versione Adaptive sono da realizzare nelle prossime fasi. Anche la calibrazione dei colori in OKLab, il trattamento delle fibre su curvatura estrema, le texture di finitura e i benchmark sono aperti.

## Build e test su Windows — PowerShell 7

**Prerequisiti:** installare .NET SDK 10, estrarre lo ZIP integrale; aprire PowerShell 7 **nella cartella `GenerativeArt` che contiene `GenerativeArt.sln`**.

**Validazione completa** (pulizia, restore, build Release e test delle suite presenti nella solution):

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

**Test C3 mirati** (per indagare facilmente eventuali errori prima di ripetere tutta la suite):

```powershell
dotnet test .\tests\GenerativeArt.Algorithms.Tests\GenerativeArt.Algorithms.Tests.csproj -c Release --filter 'FullyQualifiedName~CincinnusStrokePlannerTests'
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --filter 'FullyQualifiedName~CincinnusBristlePainterTests'
```

**Prima anteprima Cincinnus** (dopo la build, comando separato; tool C# non aggiunto alla solution principale):

```powershell
dotnet run --project .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release -- .\cincinnus-c3-preview.png 1 768 768
```

Il tool accetta nell'ordine: `file.png`, `seed`, `larghezza`, `altezza`; il default è `cincinnus-c3-preview.png`, seed 1, 768². Produce un file PNG, stampa numero di linee e pennellate e segnala se la copertura è stata troncata dal budget di sicurezza. Non attende l'interfaccia Avalonia e non modifica le ricette.

Per una prova 1280×720:

```powershell
dotnet run --project .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release -- .\cincinnus-c3-1280x720.png 3 1280 720
```

**Risultato atteso**: nessun errore di compilazione, tutte le suite verdi, file PNG decodificabile e visivamente non vuoto. Confrontare composizione, testate, materia, sfumature, ombre, copertura e seed; non approvare automaticamente l'estetica solo perché i test sono verdi.

## Criteri di chiusura C3

- Build e suite xUnit Windows completate senza errori; tool preview compilato ed eseguito (è un progetto separato dalla solution e va verificato esplicitamente).
- Confronto fra seed 1 e 3 e fra 768² e 1280×720; verificare che la disposizione delle curve non lasci grandi vuoti indesiderati e che le fibre non collassino in bande piatte.
- Distinguere **testa inclinata**, pigmento in asciugatura, passaggi incrociati e volume del fascio anche a zoom 100%.
- Solo dopo il gate: **C4** distribuzione Adaptive (main/filler/patch), poi **C5** integrazione Studio/CLI/JSON e QA artistico; G4-R2 Ocellus/Rete QA indipendente ancora aperto.
