> **STATO CORRENTE M4E.5-P4K-R1 — 17/11/2026:** priorità richiesta dall'utente: **4K per tutti i generatori e le ricette, con massima complessità e supersampling 2×; 8K generale successivamente**. Disponibili pulsante Studio **4K · qualità massima**, CLI `prepare-4k`, profilo opzionale `production` e 34 nuovi esempi: catalogo **30 generatori / 30 preset / 216 ricette**. **1.371 test Windows passati**, build Release zero avvisi/errori; azione Studio verificata sui 30 generatori e 30 file preset invariati, inclusi i 17 personali. [Guida e limiti](M4E5_4K_PRODUZIONE.md), [prove e misure](validation/P4K_R1_REPORT.md). Consegna completa `GenerativeArt_M4E5_P4K_R1_COMPLETO.zip`. Restano distinti QA artistico dell'utente, desktop/DPI, altri OS, Fase B/D e Crinis R10 in pausa. README solo descrizione/installazione/esecuzione con screenshot. **I blocchi delle versioni precedenti sotto sono archivio storico.**
> **Decisione implementata M4E.4-V1-R1 — 17/11/2026:** su richiesta dell'utente il Voronoi acquisisce mosaici posteriori visibili attraverso tessere assenti, ombre, rilievo e blur. La modalità è opt-in (`depth-style=layered`), con controlli opzionali nel JSON v1; flat e file dei preset personali restano invariati. Base di compositing esistente riusata, budget dedicati prima di superfici/output, senza dichiarare chiusa la Fase B o modificare Crinis/Cincinnus. **1.260 test Windows passati e build Release zero avvisi/errori**. [Guida e compatibilità](M4E4_V1_VORONOI_PROFONDO.md).

> **Correzione tecnica C5-R1 — 17/11/2026:** validazione Windows completa superata da Codex (1.225 test, build Release zero avvisi/errori). La risoluzione dei campioni Auto Cincinnus riusa palette e campionamento dello sfondo del pittore; non cambiano le formule di rendering né le scelte artistiche/priorità precedenti. Correzioni e limiti documentati nel [rapporto](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). README rimane descrizione/installazione/esecuzione con screenshot; esiti, cronologia e roadmap nei file dedicati.

> **Aggiornamento 17/11/2026 — decisione di continuità C4:** dopo la conferma di C2 compilata/testata e la consegna C3, l’utente ha richiesto di **continuare lo sviluppo anche senza poter testare temporaneamente**. Si prosegue con C4 Adaptive main/filler/patch preservando C1/C2 validate, C3 non ancora validata, catalogo 29/29/173 e tutti i generatori esistenti. Non attribuire all’utente un giudizio visivo su Cincinnus. C5 Studio/CLI/JSON soltanto dopo i gate e il QA artistico. [Scheda C4](CINCINNUS_C4_ADAPTIVE_2026-11-17.md).

> **Conferma 17/11/2026:** l'utente ha comunicato «compila e test ok» per **Cincinnus C2**, come già fatto per C1. L'avanzamento conseguente è **C3**: pianificatore di pennellate, rendering Skia a setole e strumento C# di anteprima PNG (C3 ancora da compilare e testare). Le scelte vincolanti restano: C#/.NET, no linguaggi Kotlin/Go/Python nel prodotto, niente sostituzione di Crinis R10, catalogo precedente integro, arte Cincinnus Simple prima di Adaptive C4, integrazione Studio/CLI/JSON solo in C5. [C3](CINCINNUS_C3_PENNELLO_VOLUMETRICO_2026-11-17.md). Lo stato C2 nei paragrafi sotto è storico.

> **Aggiornamento 17/11/2026 — Cincinnus:** l’utente conferma build/test Windows superati per **C1** e lo sviluppo prosegue con **C2 Simple**, implementata in sorgenti C# e test ma non ancora validata tramite .NET. Mantiene l’autonomia da Crinis e il catalogo 29/29/173; prima versione visiva prevista per C3, Adaptive in C4, Studio/CLI C5. Il ramo G4-R2 QA estetico rimane aperto. [Dettagli C2](CINCINNUS_C2_STREAMLINES_SIMPLE_2026-11-17.md).

> **Decisione successiva confermata (17/11/2026):** l'utente ha richiesto esplicitamente di **realizzare il primo kernel geometrico C# di Cincinnus** dopo lo studio H3. È stato implementato C1 in Algorithms con test numerici; la build/test C1 non è ancora confermata su Windows. La decisione approva **C1**, non implica che Cincinnus sia un renderer finito né sostituisce il QA artistico G4. [Scheda C1](CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md).

# Decisioni consolidate — chat GenerativeArt Studio fino al 17/11/2026

**Richiesta successiva, 17/11/2026:** studiare attentamente **Cincinnus** di Gȧrt per una possibile futura implementazione in C#. H3 ne documenta sorgenti e piano, senza attribuire all’utente la decisione di iniziare il porting, né modificare le priorità già espresse sulla verifica G4-R2. [Dossier tecnico](STUDIO_CINCINNUS_GART_2026-11-17.md).


**Finalità:** registro delle istruzioni dell'utente e dei loro aggiornamenti di priorità. Basato sulla conversazione di sviluppo e sui file della baseline **M4E.3-G4-R2**. Si distingue fra **decisione vincolante**, **realizzato**, **da validare visivamente**, **pianificato** e **sospeso**. **Ultima conferma:** l'utente ha compilato e testato con successo la G4-R2 su Windows.

## 1. Contratti di progetto non negoziabili

| ID | Decisione consolidata | Stato |
|---|---|---|
| D-01 | **Solo C#/.NET 10** per motore, UI, CLI, test; strumenti operativi PowerShell limitati. Niente Python, Kotlin, Go, Java né altri linguaggi applicativi senza autorizzazione. XAML/AXAML Avalonia sono consentiti. | **Vincolante** |
| D-02 | Architettura separata `Core`, `Algorithms`, `Application`, `Rendering.Skia`, `Cli`, `Studio`, con test indipendenti dalla GUI. | Realizzata |
| D-03 | Gȧrt (Kotlin) e `jdxyw/generativeart` (Go) come **ispirazione/sorgenti da studiare**, non dipendenze runtime o richiesta di clone bit-identico. | **Vincolante** |
| D-04 | Ricette **JSON v1** versionate, seed e parametri deterministici, palette/colore, dimensioni, alpha e quality; validazione rigorosa, import/export, output atomico e cancellazione. Non rompere ricette precedenti senza decisione esplicita. | Realizzato, preservare |
| D-05 | **Ogni consegna** contenente avanzamenti: ZIP **completo**, non frammenti; documenti aggiornati (README, roadmap, stato, changelog, handoff, guida nuova chat), test e stato di validazione esplicito, comandi Windows in italiano. | **Vincolante** |
| D-06 | Quando l'utente non può testare subito, continuare lo sviluppo senza blocchi inutili; distinguere controlli statici da build/test e non ripetere spiegazioni sull'SDK se non servono. | **Vincolante** |
| D-07 | Non alterare silenziosamente UI e UX già approvate, preset personali, PNG storici, parametri e semantiche; test di regressione e compatibilità per gli incrementi. | **Vincolante** |

## 2. Studio: richieste approvate e avanzamento

- **D-08 — Avvio massimizzato:** aprire lo Studio con finestra massimizzata (non vero fullscreen che nasconde barra del titolo/taskbar). Intervento storico M4C.2-R2; verificato nelle iterazioni successive.
- **D-09 — `StudioWindow.cs` troppo concentrato:** dividere responsabilità e rendere testabile lifecycle, catalogo, editor, preferenze, preview, export; primi partial e `StudioRenderCoordinator` introdotti. **A1 non chiusa**: restano controller indipendenti, gestione eventi tardivi, DPI/cancel e test GUI.
- **D-10 — Menu catalogo:** i pulsanti degli algoritmi/ricette nella colonna sinistra devono avere **larghezza uguale al menu** e fra loro, senza layout irregolare. Funzione già introdotta; preservare.
- **D-11 — Parametri creativi professionali:** tutti i parametri di ricetta devono comparire nell'editor; palette e **singoli colori** editabili, compreso alpha; input HEX/HSV/RGB e precisione decimale elevata. Allargare limiti troppo restrittivi (Poisson, raggio punti, spessori) dove il budget consente. Mostrare chiaramente quando un controllo è inapplicabile ma conservarne il valore JSON.
- **D-12 — Anteprima statica esatta:** render alla **risoluzione finale reale**, non preview ricalcolata più piccola. Dimensioni visualizzazione/viewport regolabili; **Adatta** mostra tutta l'opera senza deformazioni, **100%** rappresenta pixel reali con pan/zoom. In modalità statica preview ed export con stessa ricetta devono produrre gli stessi pixel/alpha; test automatici esistono, smoke manuali per nuovi generatori da registrare.
- **D-13 — Dimensioni e qualità:** progredire da 4K verso 8K, senza limiti arbitrari di 4 milioni di pixel quando il percorso può gestirne di più; controllare memoria/AA, materiale, qualità `fine-2x` e workload per renderer. **8K UHD** realizzato soltanto per quattro renderer Skia geometrici. Non promettere ancora 8192×8192 o 8K universale.
- **D-14 — Default personali:** le **17 ricette JSON allegate** diventano preset embedded per i relativi generatori; nessuna alterazione dei seed e delle animazioni presenti. Restano distinti dai valori di catalogo e dalle ricette importate. Nuovi generatori possono avere nuovi preset iniziali ma non sovrascrivono le 17 originali.
- **D-15 — Nuovi algoritmi/ricette:** sono centrali per valutare davvero il motore; includere esempi con seed, colori e parametri riproducibili, stress geometrico e qualità visiva, non solo generatori nominali.

## 3. Roadmap concordata e cambi di priorità

| ID | Decisione | Stato |
|---|---|---|
| D-16 | **Fase A** consolidamento: A0 packaging e asset; A1 refactor Studio; A2 preview esatta; A3 catalogo/ricette artistiche; A4 8K e qualità; A5 QA di rilascio. | In avanzamento, **non tutta chiusa** |
| D-17 | **Fase B — Compositore universale**, dopo A: livelli di generatori differenti, maschere, alpha, blend, trasformazioni ed export deterministico. | **Pianificata, non sviluppata** |
| D-18 | **Fase D — Workflow dell'artista**, dopo B: varianti, seed explorer, confronto A/B, galleria, contact sheet, preset riutilizzabili, **poi SVG** realmente vettoriale ove tecnicamente possibile. | **Approvata e pianificata**, non confondere con funzionalità già operative |
| D-19 | Le animazioni di `jdxyw/generativeart` sono artisticamente importanti, e in origine era prevista una fase temporale deterministica/streaming. La decisione successiva le **rinvia** rispetto alle priorità artistiche A/B/D attuali. Non eliminare i loop, timeline/frames già presenti; niente nuovo sviluppo animazione adesso. | **Rinviate**, non cancellate |
| D-20 | Preferire step progressivi testabili, con esempi e confronto visivo; distinguere «build e test ok» da «qualità artistica approvata». | **Vincolante** |

## 4. Riferimenti e realizzazioni artistiche, in ordine cronologico

1. **Gȧrt / jdxyw audit:** confronto delle tecniche, elenco opere preferite; non portare tutta la libreria Kotlin o Go nella soluzione. La scelta di quali algoritmi sviluppare si basa su utilità geometrica e valore artistico.
2. **M4A–M4E2:** palette/colore, SDF, Poisson, Voronoi, topografia, superfici, LIC, pattern natura, fini 2×, grana/carta/pigmento e primi quattro percorsi 8K. Gli interventi risolvono bug in compilazione, parser JSON, test di budget e regressioni della frequenza Vortex/Streamlines.
3. **G1 — modulari:** `alien-letters`, `cotton-circles`, `rigor-grid` da Gȧrt; test catalogo all'ordinamento degli ID.
4. **G2/G3 — pennelli e fasce:** `spiral-flamebrush`, `crinis-bristles`, `fascia-ribbons`, `eclectic-trails`; ampliamenti del volume e delle ricette con attenzione alla poetica della trama e dell'opera Gȧrt.
5. **Crinis R2–R5:** eliminare la banda opaca indesiderata (`body-opacity=0` deve davvero rimuovere corpo/ombra pieni), più profondità, bordi tessili frastagliati, microfibre distinte, pigmento e colori. Le immagini fornite mostrano che serve cura della materia, non aumentare semplicemente il numero di linee.
6. **Crinis R6–R9:** profondità tra fasci, luce davanti/dietro, rear softness, spessori più corposi, contatto e luce del gruppo. L'utente ha apprezzato soprattutto **la profondità tra le pennellate**.
7. **Crinis R10:** problema irrisolto **all'interno dello stesso fascio**: fibre ancora troppo sottili, piatte e poco volumetriche. Implementati tre sottostrati (retro/medio/fronte), filamenti principali, luce trasversale e occlusione interna. **R10 non dichiarata artisticamente soddisfacente né verificata separatamente come release**; lo sviluppo è stato **sospeso dall'utente** per passare ad altri algoritmi, non annullato.
8. **G4-R1:** Ocellus e Rete come nuove famiglie distinte; **G4-R1.1** corregge `Math.Hypot`/CS0117 (.NET 10), **G4-R1.2** corregge conflitto `ink`/CS0136. L'utente ha confermato build e test R1.2.
9. **G4-R2 (baseline attuale):** Ocellus con interstizi/luce bordo; Rete con nodi, glow e membrana interna; nuovi parametri opzionali, sei ricette A/B, 29 generatori e 173 esempi. **Utente conferma compilazione e test passati**. La **valutazione visiva** G4-R2 non è stata comunicata.

## 5. Riferimenti creativi per futuri algoritmi

L'utente ha indicato esplicitamente come rilevanti le opere Gȧrt: **Alien Letters V3, Flamebrush1, Ocellus, Pebble2, Accretion, Rete, Nimbus, Cotton Circles, Cotton Circles2, Eclectic2, Eclectic, Spring, Cyanowaves, Fascia Pal5 Seed30, Fascia, Glst2, Monolith, Perl 1, Vorflow, Fluid Wind, February, Rigor, Sabulum, Strata, Undula, LetterO, Citymap, Crinis** e ulteriori opere censite in [`RIFERIMENTI_ARTISTICI_PREFERITI.md`](RIFERIMENTI_ARTISTICI_PREFERITI.md). Gli esempi Go selezionati e i loop animati restano riferimenti futuri, **non funzionalità automaticamente implementate**.

**Candidati immediati post-G4:** studio G5 di **Nimbus, Pebble/Pebble2, Accretion, Glst/Glst2**. Non fissare l'ordine senza una successiva decisione o una motivazione tecnica esplicita. Se si continua G4-R3, distinguere topologia delle cavità reali da pori euristici, e tensione/curvatura fisica della pellicola Rete dal solo post-processing pittorico.

## 6. Test, qualità e obblighi di consegna

- Validazione Windows abituale: `clear && dotnet clean && .\tools\validate.ps1` in PowerShell 7; indicare esplicitamente se confermata dall'utente, eseguita localmente o non disponibile.
- Conservare numeri **reali**, non conteggi dedotti: **G4-R2 build e test OK**, ma numero xUnit non riportato. Lo smoke artistico G4-R2 resta aperto.
- Tutti i nuovi generatori devono avere descrittore/parametri/colore Auto, preset embedded valido, JSON v1, test su seed/PNG/alpha/preview-export/cancellazione/budget, esempi artistici e documentazione.
- Mantener distinta l'**equivalenza PNG** dalla verifica percettiva: i test non provano automaticamente la fedeltà a Gȧrt.
- Specificare limiti veri: Ocellus/Rete non garantiti 8K; fine-2x e filtri soggetti a budget; compositore universal e SVG non ancora presenti.
- Archiviare bug storici risolti senza attribuirli alla versione attuale: CS8506/CS8131 colori HSV, xUnit2020, catalogo ID, `detail-style`, CS0117, CS0136.
- Documentazione e handoff sono **parte obbligatoria** dell'incremento; evitare ambiguità con vecchie sezioni storiche e ripulire titoli «versione corrente» non più veri.

## 7. Decisioni ancora da prendere (non inventare approvazioni)

- Prossima tappa artistica esatta: altra iterazione **G4-R3** oppure nuovo blocco **G5** e primo candidato da implementare.
- Livello finale accettabile di somiglianza visiva a Gȧrt, soprattutto per Crinis e dettagli strutturali di Ocellus/Rete: servono confronti con immagini di riferimento e criteri percettivi.
- Distribuzione multipiattaforma, packaging, licenza del framework e audit transitive dependency; eventuale backend video, SQLite per grande galleria, HDR/GPU e tile 8K.

Per riferimenti tecnici dettagliati: [ROADMAP](ROADMAP.md), [ADR](ADR.md), [G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md), [Crinis R10](M4E3_G3_R10_CRINIS_PROFONDITA_INTERNA.md).

## Nota di provenienza successiva (H2)

Il 17/11/2026 è stato allegato nuovamente il sorgente Gȧrt e indicato il repository ufficiale per recuperare le immagini. Si tratta di **fonti per studio e confronto**, non di una modifica alle decisioni vincolanti né di una validazione estetica della G4-R2. Vedi [mappa ufficiale di immagini e sorgenti](GART_RIFERIMENTI_ORIGINALI_2026-11-17.md).
