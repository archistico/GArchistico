# Piano concordato — Fasi A/B/D e ricette iniziali (aggiornato M4E.1-R1)

**Data:** 17 novembre 2026. **Base:** M4D.4-R1, 17 generatori, 81 ricette di esempio e risultato Windows registrato 859/859. Le modifiche di M4D.4-R2 ai default e alla documentazione non hanno ancora una nuova validazione Windows registrata.

## Decisioni di prodotto approvate

1. Smontare progressivamente `StudioWindow.cs` in componenti piccoli e testabili **senza cambiare la GUI approvata**.
2. Per le opere statiche l'anteprima va **calcolata alla risoluzione finale**, con lo stesso raster e la stessa pipeline del PNG esportato: l'adattamento al riquadro deve essere soltanto una trasformazione di visualizzazione, non una seconda renderizzazione ridotta. Vietato ridurre silenziosamente la risoluzione.
3. Aggiungere **altre ricette originali e nuovi algoritmi** utili a stressare geometria, alpha, colori, dettagli, performance, riproducibilità e composizioni.
4. Estendere la dimensione e la **qualità effettiva** del rendering fino a **8K**: almeno 7680×4320 per l'orizzontale, con valutazione distinta di 8192×8192 per poster quadrati. Non basta aumentare il controllo della dimensione: occorrono budget memoria e lavoro, strategie a tasselli quando compatibili, filtri e antialiasing verificati.
5. **Solo dopo la Fase A**, sviluppare il **compositore universale M4C.3** (Fase B): layer di generatori differenti, maschere, blend e opacità.
6. Le animazioni, inclusi noise 4D ciclico, GIF/APNG e timeline, restano in backlog: **non sono il prossimo obiettivo**.

## Fase A — Consolidamento e qualità (ordine vincolante)

| Passo | Priorità | Intervento | Condizioni di accettazione |
| --- | --- | --- | --- |
| **A0** | P0 | Baseline e distribuzione | Verificare coerenza ZIP/manifesto e recuperare le **52 anteprime storiche mancanti** dal pacchetto M4D.4-R1; nessun collegamento documentale rotto. Default 17 JSON attivati in M4D.4-R2. |
| **A1** | P1 | Rifattorizzare `StudioWindow.cs` | Componenti isolati per catalogo/preferiti, editore parametri, stato ricetta, controller preview e export; nessuna regressione UI, eventi tardivi, gestione cancellazione, tema e preferenze. Introdurre test headless per gli scenari di lavoro. |
| **A2** | P1 | Preview nativa 1:1 con output | Il renderer calcola i pixel alle dimensioni `width`×`height` finali; `Adatta` usa una scala visiva, `100%` è pixel reale. Stessi seed, phase, colori, finiture e qualità. Riutilizzare il raster per export quando identico, se possibile; quando il budget non è sufficiente dare un errore chiaro, mai una preview approssimata spacciata per finale. Preview in background con cancellazione/debounce e nessun accumulo dei vecchi bitmap. |
| **A3** | P1 | Ricette e nuovi algoritmi per QA artistico | Nuovi esempi per ogni classe di generatore, inclusi alpha, HDR-like tonemapping simulato ove sensato, bordi, grande densità, anisotropia e varianti artistiche. Aggiungere nuovi algoritmi modulari selezionati per varietà e testabilità: ad esempio attractor 2D, reaction-diffusion 2D e quasicristalli come candidati, non ancora approvati come implementazione. Golden PNG, roundtrip JSON e benchmark. |
| **A4** | P1 | Rendering 8K e maggiore qualità | Sblocco progressivo da 4096 per lato a 7680×4320, poi verifica separata di 8192×8192; validare ciascun renderer e post-process, non solo il limite generale. Gestione memoria con stima e soglie esplicite, operazioni a tile/streaming quando realizzabili, cancellazione, alpha, DPI, tempi, confronto di qualità standard/AA. |
| **A5** | P2 | Gate di rilascio | Regressioni Windows, smoke CLI/Studio, confronto PNG storici, verifica hash dello ZIP, memoria e tempi per 4K e 8K, documentazione coerente. |

**Nota sull'8K:** 7680×4320 = 33.177.600 pixel, circa 126,6 MiB per **un solo buffer RGBA8**; 8192×8192 = 67.108.864 pixel, circa 256 MiB per buffer. Buffer duplicati, 2× supersampling, filtri, preview e PNG possono moltiplicare il picco. Occorre quindi misurare per renderer e introdurre tecniche più efficienti: non portare semplicemente `Maximum*Pixels` a numeri maggiori.

### Contratto di anteprima A2

- L'anteprima statica è **lo stesso raster del risultato finale**; la visualizzazione può essere ridotta solo tramite zoom/fit del viewport.
- La risoluzione dell'anteprima non deve diventare un vincolo di generazione separato. `Raster = output` diventa comportamento normale per le opere statiche.
- Per render pesanti, mostrare progresso/operazione in corso e poter annullare, senza sostituire silenziosamente una preview a bassa risoluzione.
- Solo le sequenze temporali, in una fase futura, potranno avere un *proxy* esplicitamente etichettato e distinto dall'output.
- Gate pixel: stesso PNG decodificato produce gli stessi pixel/alpha dell'anteprima, a parità di ricetta; verificare panning, 100%, Adatta e scaling DPI.

## Avanzamento A1 — M4E.1-R1 e R2

**Implementato come primo incremento, non chiusura della fase.** La classe `StudioWindow` passa da 1.103 linee a 189; sette partial delimitano layout/tema, catalogo/preferenze, editor ricetta, preview, playback, operazioni file e stato. La logica di parsing dello stato testuale è estratta in `StudioRecipeFormParser` nel livello Application, con test headless sui valori corretti e sui rifiuti. I campi condivisi e i gestori di eventi restano nella finestra: il secondo incremento A1 deve isolare controller indipendenti, preservando il lifecycle e aggiungendo test Studio headless. `A2` è stata implementata nel percorso Studio in **M4E.1-R2** (senza raster ridotto), ma richiede QA GUI/pixel/DPI. `A4` (8K) non è ancora implementata. R2 introduce anche `StudioRenderCoordinator` per serializzazione, cancellazione ed export locale atomico; A1 è ancora parziale, con controller UI/lifecycle da separare. Vedi [`M4E1_A1_WORKFLOW_D.md`](M4E1_A1_WORKFLOW_D.md).

## Fase B — Compositore universale M4C.3

Solo dopo chiusura A0–A5. Riutilizzare le primitive `DrawLayer` / `LayerEffects` già presenti, senza legare gli algoritmi alla finestra Avalonia.

- Ricetta composita versionata: lista ordinata di generatori/ricette con seed e dimensioni coerenti.
- Livelli visibili/disattivati, opacità, posizione e trasformazioni, modalità di fusione SrcOver/Multiply/Screen/Overlay (da confermare con test sul colore premoltiplicato).
- Maschere alfa/geometriche e un primo editor di livelli nello Studio.
- Esportazione riproducibile di un unico PNG; cancellazione e budget di memoria per layer; senza accumulare tutti i raster intermedi.
- Test di serializzazione, seed, blending sRGB/lineare dichiarato, alpha, clipping e compatibilità.

## Fase D — Workflow dell’artista (approvata, dopo la Fase B)

1. **D1 — Varianti e seed explorer:** generare un insieme finito/limitato di varianti a parametri fissi, seed diversi; riprodurre ogni cella con la propria ricetta; annullare batch e non accumulare PNG Full HD in RAM.
2. **D2 — Confronto A/B:** due configurazioni affiancate, blocco seed opzionale, differenze parametriche leggibili, sincronizzazione zoom/pan, ripristino A o B.
3. **D3 — Galleria locale:** salvare anteprime/cache e metadati con limite, gestire file eliminati o spostati, privacy locale e nessun cloud obbligatorio.
4. **D4 — Contact sheet:** esportare fogli di provini con etichette seed, algoritmo, preset; thumbnails bounded, output atomico e cancellabile.
5. **D5 — Preset riutilizzabili:** salvataggio, catalogazione, import/export JSON versionato senza cambiare la semantica delle ricette esistenti.
6. **D6 — Export vettoriale selettivo:** successivamente SVG da primitive geometriche vere (curve, tracce, poligoni), dichiarando esplicitamente i generatori raster-only; nessun SVG simulato inserendo un PNG.

**Ordine concordato:** A → B → D; animazioni più avanti. Dettagli e criteri in [`M4E1_A1_WORKFLOW_D.md`](M4E1_A1_WORKFLOW_D.md).

## Animazioni — rinviate

M4B.2, M5C, timeline e GIF/APNG restano in backlog. Il funzionamento animato già presente non va rimosso, ma non costituisce un gate della Fase A, salvo le regressioni dei default e del selettore.

## M4D.4-R2: default da 17 file allegati (implementato nel codice)

I seguenti **17 file JSON forniti dall'utente** sono stati copiati come risorse embedded in `src/GenerativeArt.Application/Defaults/`. Le risorse sono interpretate dal codec rigoroso `ArtworkRecipeJson`: parametri, colori, seed, dimensioni e animazione provengono dal JSON originale. Quando viene selezionato un generatore per un **nuovo** lavoro, `StudioWindow.SelectArtwork` usa la relativa ricetta; l'importazione di un JSON salvato continua a prevalere sui default e non riscrive i valori storici.

`bokeh-depth`, `domain-warp`, `flow-brush`, `guilloche`, `lic-flow`, `looping-flow`, `noise-flow`, `paper-cut`, `parametric-curves`, `plasma-noise`, `poisson-stippling`, `sdf-orbits`, `spaced-streamlines`, `strata-landscape`, `tonal-engraving`, `topographic-contours`, `voronoi-mosaic`.

Tutti hanno dimensione 1920×1080. Il seed personalizzato di **Domain Warp** è `6582940917790807322`, non `42`. `looping-flow` mantiene 16 frame a 18 fps; le animazioni restano rinviate come sviluppo. `voronoi-mosaic` ha `render-quality=fine-2x` come specificato. Non vengono modificati i default del **catalogo** né le ricette già salvate; nessun nuovo formato JSON. Nessun file JSON deve essere cercato nella directory di avvio: sono embedded nell'assembly.

**Test da confermare:** tutti i 17 embedded presenti e validi; campi campione, seed e animazione; selezione generatori Studio; import e roundtrip JSON; compatibilità delle immagini storiche. Questa revisione non implementa ancora A1, A2, A3, A4 o la Fase B.
