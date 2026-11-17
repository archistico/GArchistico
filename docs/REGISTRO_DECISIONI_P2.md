> **Decisione implementata M4E.4-V1-R1 — 17/11/2026:** su richiesta dell'utente il Voronoi acquisisce mosaici posteriori visibili attraverso tessere assenti, ombre, rilievo e blur. La modalità è opt-in (`depth-style=layered`), con controlli opzionali nel JSON v1; flat e file dei preset personali restano invariati. Base di compositing esistente riusata, budget dedicati prima di superfici/output, senza dichiarare chiusa la Fase B o modificare Crinis/Cincinnus. **1.260 test Windows passati e build Release zero avvisi/errori**. [Guida e compatibilità](M4E4_V1_VORONOI_PROFONDO.md).

> **Correzione tecnica C5-R1 — 17/11/2026:** validazione Windows completa superata da Codex (1.225 test, build Release zero avvisi/errori). La risoluzione dei campioni Auto Cincinnus riusa palette e campionamento dello sfondo del pittore; non cambiano le formule di rendering né le scelte artistiche/priorità precedenti. Correzioni e limiti documentati nel [rapporto](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). README rimane descrizione/installazione/esecuzione con screenshot; esiti, cronologia e roadmap nei file dedicati.

> **Aggiornamento Cincinnus C1 — 17/11/2026:** implementazione del solo kernel geometrico **approvata esplicitamente** dall'utente e inserita in Algorithms senza modificare Crinis, Studio o catalogo. Non confondere con approvazione di un rendering finale o modifica alla roadmap G4-R3/G5. Test Windows C1 ancora da confermare. Dettagli [C1](CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md).

> **Nota di precedenza (17/11/2026):** decisioni P2 originarie e proposte non ancora adottate. Le decisioni più recenti su anteprima nativa, Fasi A/B/D, stato Crinis e G4 sono consolidate in [DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md](DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md). La baseline validata è G4-R2.

> **Aggiornamento 16/11/2026:** M3C.2-R1 verificata su Windows; aggiornati gli stati delle decisioni già realizzate. Questo registro contiene anche proposte future, non tutte approvate o implementate.

# Registro delle decisioni di prodotto P2

Distinguere **vincolante**, **proposto** e **da decidere**. Le proposte di questo documento non equivalgono a funzionalità già implementate.

| ID | Stato | Scelta | Motivazione / condizione |
|---|---|---|---|
| P2-01 | Vincolante | C#/.NET 10, PowerShell minimo; XAML/AXAML permesso | L'utente vieta Python, Go, Kotlin, Java e altri linguaggi applicativi non autorizzati, **non** XAML; in caso di dubbio chiedere. |
| P2-02 | Vincolante | Motore headless indipendente dalla GUI | Già confermato da M2–M3B |
| P2-03 | Vincolante | Gȧrt e jdxyw fonte progettuale, non runtime | Evita doppia toolchain e duplicazioni |
| P2-04 | Vincolante | ZIP completo ogni consegna di codice, test e stato espliciti | Continuità e regressioni controllate |
| P2-05 | Approvato come visione | Prodotto con 3 ingressi: libreria, CLI, Studio | Sfrutta un solo motore |
| P2-06 | Approvato come ordine dei lavori | Anticipare una GUI minima dopo M3C | Convalida esperienza d'uso prima di molte decine di generatori |
| P2-07 | Implementato in M3C.1 | Catalogo metadati espliciti, ID e versione | Parametri condivisi fra CLI/GUI/recipe |
| P2-08 | Implementato in M3C.2 | Ricetta JSON versionata prima di PNG embedded | Formato verificabile e facilmente migrabile |
| P2-09 | Proposto | SVG vettoriale solo con geometria nativa | Nessun falso vector export |
| P2-10 | Proposto | Sweep lazy e coda bounded | Impedisce esplosioni combinatorie |
| P2-11 | Proposto | 3 opere pilota: parametrica, pixel, flow | Attraversano tutti i percorsi prima dell'espansione |
| P2-12 | Vincolante | Animazioni codificate progressivamente | Evitare memoria proporzionale al numero di frame |
| P2-13 | Proposto | Preset JSON; eventuale SQLite in futuro | Evitare complessità senza dati persistenti voluminosi |
| P2-14 | Da decidere | Licenza del nuovo framework | Prima di release pubblica, rispettando MIT/BSD e dipendenze native |
| P2-15 | Da decidere | MP4 backend/FFmpeg e distribuzione | Audit licenze, codec, RID e gestione del processo |
| P2-16 | Da decidere | HDR `Half`/`float`/`double` e formati | Benchmark memoria e differenze visive |
| P2-17 | Da decidere | Primo target release Windows o tre OS simultanei | CI tre OS pianificata, disponibilità di prove reali necessaria |
| P2-18 | Da decidere | Linguaggio UI iniziale, localizzazione | Italiano iniziale proposto; risorse predisposte per i18n |
| P2-19 | Rinviata | Plugin caricabili a runtime, marketplace, editor nodale | Superficie d'attacco e complessità prematura |
| P2-20 | Rinviata | Supporto GPU obbligatorio | CPU/Skia headless prima, GPU solo dopo benchmark |

**Quando un punto diventa approvato:** registrarlo in `docs/ADR.md` e aggiornare esclusivamente `docs/ROADMAP.md` per stato e priorità. Questo documento rimane registro delle proposte e delle scelte aperte.


## Aggiornamento animazioni — 16 novembre 2026

Il feedback dell'utente promuove le animazioni del progetto Go a priorità esplicita. Decisioni approvate in fase di progetto: metadati Animated/SeamlessLoop, fase periodica deterministica, PNG progressivi, preview Play/Pausa/Loop nel primo Studio. GIF/APNG a streaming anticipati; video/motion blur temporale successivi. Le decisioni tecniche sui codec nativi, licenze e UI saranno formalizzate prima dell'implementazione corrispondente.
