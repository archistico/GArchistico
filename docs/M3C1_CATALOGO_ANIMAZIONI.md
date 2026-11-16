# M3C.1 — Catalogo di generatori e prime fondamenta di animazione

> **Nota di stato (16/11/2026):** questa specifica conserva descrizioni e risultati del momento in cui fu scritta. La baseline M3C.2-R1 è stata successivamente compilata e testata con successo dall'utente su Windows; per lo stato attuale prevalgono [`CURRENT_STATE.md`](CURRENT_STATE.md) e [`ROADMAP.md`](ROADMAP.md). Non sono attestati benchmark o esecuzioni multipiattaforma.

**Data:** 16 novembre 2026. **Stato attuale:** questi componenti sono inclusi nella baseline M3C.2-R1 compilata e testata su Windows. Lo smoke manuale del loop completo non è stato esplicitamente confermato.

## Obiettivo

Dare una rappresentazione condivisa a tutte le opere, prima di costruire Studio Avalonia e ricette JSON. Anticipare le animazioni, che l'utente considera un riferimento artistico importante del repository `jdxyw/generativeart`.

## Fonte Go effettivamente analizzata

Nell'archivio `jdxyw-generativeart-master.zip`:

- `platform.go`: `AnimatedEngine.GenerativeFrame(c, frameIdx, totalFrames, phase)`, con `phase = frameIdx / totalFrames`, e rendering multi-frame.
- `platform.go`: opzioni di temporal supersampling e gestione dei frame; nel codice Go vengono conservati i fotogrammi in `c.frames`.
- `poster.go`: poster animati con diverse opere sincronizzate sulla medesima fase.
- `docs/roadmap.md`: parametri animabili tramite keyframe e interpolazione; è una funzionalità del progetto Go, non ancora del nostro C#.

Abbiamo adottato **il concetto di fase periodica** ma non l'accumulo in RAM di tutta la sequenza. Non è stato importato alcun sorgente Go nel prodotto.

## Contratti implementati

- `GenerativeArt.Application.Catalog.ArtworkDescriptor`: ID in kebab-case ASCII, versione positiva, categoria, titolo, descrizione, capacità e parametri.
- `ArtworkCapabilities`: Raster, Vector, Animated, SeamlessLoop. SeamlessLoop richiede Animated.
- `ArtworkParameterSpec`: tipi Integer, Number, Boolean, Color, Choice, default, intervalli, opzioni e valori canonici indipendenti dalla cultura locale. La futura ricetta JSON avrà valori JSON tipizzati: questa fase serve all'inventario/validazione da CLI.
- `ArtworkRegistry`: factory/registrazione esplicita, ordinamento deterministico, ID duplicati rifiutati, nessuna reflection sui campi privati delle opere.
- `BuiltInArtworkCatalog`: quattro descrittori — `parametric-curves`, `noise-flow`, `spaced-streamlines`, `looping-flow`.
- `LoopFrame`: fase [0,1), istante e indice validati; l'ultimo frame **non** duplica il primo.
- `LoopNoiseField2D`: un campo matematico periodico nel tempo tramite traslazione circolare della posizione di campionamento Simplex (sin/cos della fase).
- `LoopFramesDemo`: salva ogni PNG separatamente, rilascia la superficie native al termine del singolo frame, limite esplicito al numero di pixel cumulativi e alle dimensioni; non conserva bitmap precedenti in RAM.

## Funzionalità non ancora implementate

- Il catalogo **non è ancora il dispatch di rendering universale**. I comandi esistenti restano invariati; gli schemi ne anticipano la parametrizzazione in M3C.3.
- `render-loop-frames` produce **sequenza PNG e `manifest.json`**, non GIF, APNG o MP4. Non è ancora possibile riprodurre la sequenza direttamente dalla nostra GUI, che non esiste.
- **Nota storica M3C.1:** al momento di questa milestone non esistevano ricette JSON; **M3C.2 ora le implementa**. Restano assenti editor GUI, keyframe, codec animati, timeline di simulazione e motion blur.
- La chiusura matematica periodica della funzione del campo non garantisce da sola la perfetta fluidità percettiva: occorrono test visivi sul wrap e sulla densità dei tratti.

## Comandi

Dalla cartella `GenerativeArt`:

```powershell
dotnet run --project src/GenerativeArt.Cli -c Release -- list
dotnet run --project src/GenerativeArt.Cli -c Release -- describe looping-flow
dotnet run --project src/GenerativeArt.Cli -c Release -- render-loop-frames .\output\loop-frames 512 512 42 24 24
```

Il percorso dei frame deve essere nuovo o vuoto. Il default di fase è `index/frames`; la durata del loop è `frames/fps` secondi. Limiti CLI: da 2 a 120 fotogrammi, da 1 a 120 FPS, dimensioni 64–1024 per lato, max 40 milioni di pixel cumulativi. Sono limiti prudenti **di questa demo**, non limiti definitivi del framework.

## Quality gate

1. Restore e build Release di tutti i 9 progetti.
2. Test xUnit aggiunti per ID/versioni/capabilities, duplicati, default, tipi e input non validi.
3. Test per fase, periodo, determinismo e NaN nel campo di animazione.
4. CLI `list`, `describe`, vecchi tre comandi e nuovo `render-loop-frames` (smoke).
5. PNG e manifest validi, 24 file con naming stabile, memoria non proporzionale al numero dei frame.
6. Verifica visiva del salto ultimo→primo fotogramma e controlli successivi su più OS.

**Stato aggiornato:** compilazione e suite C# risultano superate nella baseline R1; lo smoke dedicato della sequenza e le prove visive/memoria restano aperti.

## Prossimi incrementi (ordine proposto)

1. **M3C.2:** recipe JSON con ID/versione/seed, dimensioni, parametri validati, schema animazione opzionale.
2. **M3C.3:** rendering genericamente registrato e tre opere end-to-end, alias legacy conservati.
3. **M3D.1:** Studio Avalonia code-only, con **preview Play/Pausa/Loop** su un generatore già animato, non soltanto immagine statica.
4. **M4/M5:** noise 4D periodico, interpolazione keyframe ed effetti temporali; export GIF/APNG a frame progressivi in fase anticipata rispetto alla roadmap precedente.
5. **M6:** motion blur temporale, video, poster animati e codec/librerie con licenze verificate; canale bounded e cancellazione.

## Vincolo linguistico

Solo sorgenti e test **C#/.NET 10**. L'unico script operativo è `tools/validate.ps1` in PowerShell. JSON, CSV e Markdown sono dati e documentazione, non programmi in un altro linguaggio.
