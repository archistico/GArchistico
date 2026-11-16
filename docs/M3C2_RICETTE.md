# M3C.2 — Ricette JSON v1 e primi due renderer pilotati dai dati

> **Evoluzione M3C.3-R1:** il dispatch oggi è stato esteso a cinque renderer nella nuova versione (ancora da compilare e testare); la descrizione sottostante documenta fedelmente la M3C.2-R1 storica. Vedi [`M3C3_RUNTIME.md`](M3C3_RUNTIME.md).

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Data:** 16 novembre 2026. **Stato attuale M3C.2-R1:** l'utente ha confermato **compilazione .NET 10, test xUnit, generazione JSON e PNG riuscite** su Windows dopo il fix. Il precedente errore 288/289 riguarda una versione antecedente a R1. Resta consigliata una prova specifica del loop animato completo; non ci sono conferme di esecuzione su Linux/macOS.

## Obiettivo

Offrire un formato condiviso, sicuro e riproducibile per descrivere una composizione e la sua animazione, prima di introdurre lo Studio Avalonia. Nessuna dipendenza nuova, nessun codice Go/Kotlin/Python incluso.

## Formato

Il contratto JSON v1 comprende:

| Chiave | Tipo | Regola |
| --- | --- | --- |
| `formatVersion` | intero | Deve essere `1` |
| `artworkId` | stringa | Deve esistere nel registro |
| `algorithmVersion` | intero | Deve corrispondere *esattamente* alla versione nel registro |
| `seed` | stringa | `UInt64` decimale canonico, così non perde precisione nei lettori JSON |
| `width`, `height` | interi | Positivi e nel budget di `CanvasSize` |
| `parameters` | oggetto | Tutti e soli i parametri dell'opera, con tipi JSON reali |
| `animation` | oggetto facoltativo | `frameCount` e `framesPerSecond`, solo su opere animate |

Un parametro `Number` viene scritto come numero JSON, `Boolean` come booleano, `Integer` come intero e `Choice`/`Color` come stringhe. Le chiavi sono ordinate per ID; i valori sono canonicalizzati con cultura invariante. Il serializzatore produce byte UTF-8 deterministici per la stessa ricetta.

- **Compatibilità:** versioni sconosciute non sono migrate automaticamente; la modifica incompatibile di un generatore richiede incremento di `algorithmVersion` e un'eventuale migrazione esplicita futura.
- **Validazione:** proprietà sconosciute o duplicate sono respinte anche negli oggetti annidati. Sono respinti numeri non finiti, stringhe dove è atteso un numero, scelte inesistenti, animazione per opere statiche, seed fuori intervallo e parametri mancanti.
- **Limiti:** massimo 64 KiB per file ricetta, profondità JSON 12; il parser non ammette commenti o virgole finali. `ArtworkRecipeFiles.Load` applica il limite *durante* la lettura, evitando di caricare file grandi in memoria.
- **Scrittura sicura:** `SaveNew` usa `FileMode.CreateNew` e rifiuta la sovrascrittura di una ricetta esistente.
- **Animazione:** il modello consente 2–10.000 frame e 1–240 FPS; **il renderer dimostrativo** `looping-flow` resta intenzionalmente limitato a 2–120 frame, 1–120 FPS, 64–1024 px per lato e 40 milioni di pixel cumulativi. La distinzione è esplicita.

## CLI: creare, validare, ispezionare e renderizzare

Dalla directory della solution:

```powershell
# Compila ed esegui tutti i test prima di usare il nuovo codice.
.\tools\validate.ps1

# Statico: genera il JSON con valori di default.
dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-new parametric-curves .\output\curves.json 640 480 42

# Verifica schema e versione; visualizza i parametri canonici.
dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-validate .\output\curves.json
dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-show .\output\curves.json

# Applica davvero la scala della ricetta al PNG.
dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\output\curves.json .\output\curves.png

# Animato: otto frame a 24 fps, senza GIF né MP4.
dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-new looping-flow .\output\loop.json 192 192 42 8 24
dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\output\loop.json .\output\loop-frames
```

**N.B.:** il file JSON da usare nella CLI deve essere scritto in una cartella nuova o con nome mai esistito: la creazione non sovrascrive. La cartella di output dei frame deve essere nuova o vuota.

Due ricette esempio sono disponibili in `examples/recipes/`. Per sperimentare con i parametri basta modificarne i valori JSON rispettando i tipi e i limiti; `recipe-validate` ne controlla la coerenza. Il codice non crea GIF o video in questa milestone.

## Integrazione eseguita e limiti dichiarati

- `parametric-curves`: `render-recipe` utilizza il valore `scale` e le dimensioni del JSON. È una curva deterministica, quindi il seed è documentato ma non influenza la sua forma.
- `looping-flow`: `render-recipe` applica `loop-radius`, `palette`, seed, dimensioni, numero di frame e FPS; usa lo stesso percorso di esportazione progressiva del comando legacy.
- `noise-flow` e `spaced-streamlines`: ricette creabili, validabili e ispezionabili; il comando generico di rendering risponde con un errore esplicito. Il loro dispatch con parametri effettivi è M3C.3.
- Restano utilizzabili senza cambiamenti i comandi `render-demo`, `render-flow`, `render-flow-spaced`, `render-loop-frames`.
- Non esistono ancora editor GUI, preset versionati fra release, ricette incorporate nel PNG, manifest di commit atomico, cancellazione del job o encoder GIF/MP4.
- La stessa ricetta e la stessa versione algoritmica dovrebbero produrre gli stessi contenuti su identica toolchain di rendering; il confronto byte-a-byte dei PNG tra sistemi operativi **non è promesso**.

## Test e gate

Test xUnit nuovi in `GenerativeArt.Application.Tests/RecipeTests.cs`: round-trip, canonicalizzazione, versioni, seed UInt64 massimo, validazione tipi/limiti, duplicate keys anche nei parametri, animazione in un'opera statica, dimensione massima dei file, impossibilità di sovrascrivere e immutabilità dei parametri.

**Gate G3C.2 (stato):** restore/build Release e suite xUnit superati su Windows secondo l'utente; smoke JSON e PNG riuscito. Da completare come controllo addizionale, senza annullare tale esito, la verifica manuale del loop: `recipe-new` → `recipe-validate` → `render-recipe` (PNG decodificabile e otto frame con manifest). Provare a modificare `scale` e `palette` e verificare che cambino effettivamente le immagini. La baseline M3C.2-R1 resta validata per il percorso statico JSON→PNG e per la suite; lo smoke animato completo non è ancora attestato.

## Prossimo passo

**M3C.3:** rendere omogeneo il dispatch dei quattro generatori, completare i parametri collegati a `FlowDemo` e `EvenFlowDemo`, aggiungere il generatore pixel/noise di riferimento e isolare ulteriormente il runtime dall'host CLI. Poi M3D: primo Studio Avalonia code-only, con anteprima e Play/Pausa/Loop.
