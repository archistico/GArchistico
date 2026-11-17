# M4E.1-R1 — Refactoring iniziale Studio e Fase D approvata

**Data:** 17 novembre 2026. **Base:** M4D.4-R2. Questa consegna implementa il **primo incremento A1** e registra la **Fase D**; non realizza ancora l'anteprima nativa, l'8K o il compositore universale.

## Lavoro effettuato in A1-R1

`StudioWindow.cs` passa da **1.103** a **189** righe: mantiene dipendenze, stato, costruttore e lifecycle; i metodi sono ripartiti in partial, conservando il codice dei controlli, gli eventi, temi, gestione cancellazione, rendering ed export:

| File | Responsabilità |
| --- | --- |
| `StudioWindow.cs` | Inizializzazione, stato e wiring degli eventi |
| `StudioWindow.Layout.cs` | Layout, tema, factory dei controlli |
| `StudioWindow.Catalog.cs` | Catalogo, preferiti, ricette recenti e preferenze |
| `StudioWindow.RecipeEditor.cs` | Editor dei parametri, validazione visiva, aggiornamento ricetta |
| `StudioWindow.Preview.cs` | Preview raster, scheduler, cancellazione, zoom e viewport |
| `StudioWindow.Playback.cs` | Player e avanzamento frame esistente, senza nuove funzioni |
| `StudioWindow.FileOperations.cs` | Apri/salva ricette, export PNG, file picker, write atomica |
| `StudioWindow.Status.cs` | Messaggi di stato e errori |

La logica `BuildRecipe` delega a `StudioRecipeFormParser.Parse` nel progetto Application: gestisce `UInt64`, dimensioni canvas, input numerici/colori invalidi, frame/FPS e validazione del catalogo senza importare Avalonia. Il comportamento precedente resta il contratto; i test `StudioRecipeFormParserTests` controllano i valori accettati e i rifiuti, anche il seed speciale di Domain Warp.

**Limite esplicito:** i partial condividono ancora i campi UI di `StudioWindow`. È una decomposizione in responsabilità e un primo isolamento della logica *pura*, non ancora controller ViewModel completi. Non dichiarare A1 chiusa: il prossimo passo deve estrarre `PreviewController`/`ExportController` e verificare lifecycle, rimozione handler, cancellazione e threading del dispatcher con smoke headless.

## Contratti non modificati

- I 17 preset JSON embedded sono invariati; non modificare ID o versione algoritmi.
- Nessun cambiamento al layout o alle interazioni della GUI, inclusi avvio massimizzato e picker colore; il titolo della finestra indica la nuova revisione.
- Nessun cambiamento al renderer, al formato JSON v1 o alle regole di memoria.
- Export PNG locale atomico; token di cancellazione, semaforo di serializzazione e invalidazione delle preview precedenti conservati.
- A2: anteprima a dimensioni finali **da sviluppare**. A4: supporto fino all'8K **da sviluppare**.

## Fase D — Workflow dell'artista

**Approvata in roadmap, non sviluppata in questa consegna.** Sequenza A → B → D; animazioni rinviate a dopo, salvo eventuali decisioni esplicite successive.

| Incremento | Funzione | Contratto di accettazione |
| --- | --- | --- |
| D1 | Varianti + esplorazione dei seed | Griglia bounded, seed esatto per ciascuna cella, generazione cancellabile e nessun batch di PNG HD in RAM |
| D2 | Confronto A/B | Stessa dimensione, sincronizzazione pan/zoom opzionale, differenze fra parametri e scambio/applicazione della variante |
| D3 | Galleria locale | File locali referenziati, miniature su disco versionate, cache con limite, ricette recuperabili, gestire file mancanti |
| D4 | Contact sheet | Foglio di provini PNG con nomi, seed e metadati; resa stabile, streaming/thumbnail bounded, export atomico |
| D5 | Preset riutilizzabili | Nome, categoria e tag, import/export JSON, aggiornamento esplicito di preset, mantenimento delle ricette precedenti |
| D6 | Vettoriale SVG selettivo | Recorder di primitive geometriche, stroke/curve/poligoni, seed riproducibile; dichiarare raster-only e preservare alpha e clipping |

**Proposta di percorso:** iniziare da D1 perché prepara il modello dati e il generatore batch necessario a D2/D4; sviluppare D3 e D5 prima di D6. La qualità di SVG dipende dai singoli algoritmi: non è sufficiente incorporare un'immagine raster in un file `.svg`.

## Stato di verifica

- Baseline confermata in documentazione: M4D.4-R1, 859/859 test Windows.
- M4D.4-R2 e M4E.1-R1: gate Windows da eseguire quando possibile.
- Controllo statico in questa consegna: tutti i file della precedente revisione preservati, ripartizione dei metodi, parser e test nuovi, preset embedded, manifest e ZIP verificati.

### Comandi Windows (PowerShell 7)

Aprire la cartella radice `GenerativeArt` contenente la solution ed eseguire:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando pulisce e poi compila/testa. Avviare lo Studio con:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```
