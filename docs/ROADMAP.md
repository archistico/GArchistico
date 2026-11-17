> **STATO CORRENTE M4E.4-V1-R1 — 17/11/2026:** Mosaico Voronoi ampliato con modalità `layered`, tessere mancanti, mosaici posteriori indipendenti, smussi/luce, ombre gaussiane, distanza e sfocatura. **Build Release zero avvisi/errori e 1.260/1.260 test Windows** (Codex, SDK 10.0.201: Core 115, Algorithms 254, Application 220, Rendering.Skia 671). 30 generatori, 30 preset incorporati e **182 ricette**, sei nuove Full HD, due fine-2x. PNG storici Voronoi identici in quattro casi; tutti i 30 file preset, inclusi i 17 personali, invariati. Editor/CLI e anteprima nativa verificati; 8K conservato per flat, non per il percorso filtrato. [Guida, confronto e verifiche](M4E4_V1_VORONOI_PROFONDO.md). Le note C5 e precedenti sotto sono storiche; restano aperti giudizio artistico dell'utente, desktop/DPI, altri OS e stress. Crinis/Cincinnus e G4 non modificati da questo incremento.

> **VALIDAZIONE WINDOWS C5-R1 — 17/11/2026:** corretti tre problemi riprodotti: xUnit2012 nel test del pittore, conteggio dei pigmenti che includeva pigment-granulation e campioni Auto Cincinnus mancanti. **Build Release zero avvisi/errori e 1.225/1.225 test superati da Codex, SDK 10.0.201** (Core 115, Algorithms 249, Application 219, Rendering.Skia 642); gate rapido 21 test già inclusi nel totale. Tool CincinnusPreview compilato; 176 ricette validate, tre PNG CLI 768×768 decodificati e Studio headless verificato su sei combinazioni palette/modalità. Catalogo/preset/esempi 30/30/176. [Rapporto e log](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). Qualità artistica Cincinnus/G4, desktop/DPI e stress ai limiti ancora aperti; Crinis R10 in pausa. **Le note di validazione C5/C4/C3 precedenti sotto sono storiche.**

> **STATO OPERATIVO C5 — 17/11/2026:** Cincinnus integrato *sperimentalmente* come 30° generatore nel catalogo/Studio/CLI, 30 preset embedded e 176 esempi JSON (173 storici + 3 C5). Supporta Simple/Adaptive, otto pigmenti modificabili singolarmente, tre palette, parametri/ombre e budget 2048 px / 2.097.152 pixel. **C1 e C2 test/build Windows confermati; C3, C4 e C5 non ancora compilati/testati dall'utente**, nessun giudizio estetico PNG. Leggere [scheda C5](CINCINNUS_C5_STUDIO_CLI_JSON_2026-11-17.md) e [QA](validation/CINCINNUS_C5_STATIC_QA.md). Le intestazioni C4/C3 seguenti sono storiche.

> **CINCINNUS C4 (17/11/2026) — REALIZZATA IN CODICE, GATE APERTO:** C1/C2 validate Windows dall’utente. C3 pennello volumetrico implementata ma non validata né visivamente approvata; C4 aggiunge **Adaptive main/filler/patch** con griglia di copertura riprocessata dopo taglio degli uncini, test e PNG di prova. **C4 ancora da compilare/testare su Windows**. Prossimo: gate C3+C4 e A/B artistico Simple/Adaptive, poi C5 Studio/CLI/JSON e parametri creativi. Nessun cambio a 29/29/173 né a Crinis/G4. [Dettagli C4](CINCINNUS_C4_ADAPTIVE_2026-11-17.md). Le note storiche sottostanti non sono lo stato corrente.

> **AVANZAMENTO CINCINNUS C3 — 17/11/2026:** C1 campo+RK4 **validata Windows**; C2 distribuzione Simple **validata Windows**; **C3 pennellate e Skia implementata, in attesa di build/test ed esame visivo**. È disponibile un tool C# autonomo di esportazione PNG; non c'è ancora un generatore nel catalogo Studio (29/29/173). **C4**: distribuzione Adaptive main/filler/patch e confronto; **C5**: registrazione Studio, CLI, JSON, controlli artistici e QA. G4-R2 A/B Ocellus/Rete, Crinis R10 sospeso e fasi A/B/D restano distinti. [C3](CINCINNUS_C3_PENNELLO_VOLUMETRICO_2026-11-17.md). Le note C2/C1 sotto sono cronologia.

> **AVANZAMENTO CINCINNUS — 17/11/2026:** **C1 completata e validata dall’utente Windows**, **C2 posizionatore Simple implementato e in attesa di build/test Windows**, **C3** pennellate e setole, **C4** Adaptive main/filler/patch, **C5** integrazione Studio/CLI/JSON e QA. C2 conserva i 29 generatori e non genera PNG; la valutazione estetica G4-R2 rimane indipendente e aperta. Vedi [C2](CINCINNUS_C2_STREAMLINES_SIMPLE_2026-11-17.md). Le note C1 «non ancora validata» sotto sono storico.

> **Aggiornamento C1 — 17/11/2026:** su richiesta esplicita dell'utente, è stato **sviluppato il primo kernel geometrico autonomo di Cincinnus** (vortici maximin fuori tela, somma campo, RK4, stima ritorni e curvature) con test xUnit. **Codice consegnato, build/test C1 ancora da eseguire su Windows.** La precedente G4-R2 è l'ultima base validata. **Prossimi passi Cincinnus:** gate C1 → **C2** streamlines equidistanti Simple (separazione e controllo singularità) → **C3** pennello a setole/ombre → **C4** copertura Adaptive main/filler/patch → **C5** catalogo/Studio/CLI/JSON e QA visivo. Le milestone G4-QA Ocellus/Rete e A0/A1/A4/A5 rimangono aperte; Cincinnus è un avanzamento A3 in parallelo, non una scelta già approvata fra G4-R3 e G5. [Dettagli C1](CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md).

# ROADMAP — stato corrente e priorità G4-R2 (17/11/2026)

**Nota H3 — nuovo studio richiesto:** documentato [Cincinnus](STUDIO_CINCINNUS_GART_2026-11-17.md) (versioni originale e Simple di Gȧrt). Tracciato un **filone candidato C0–C5** per un generatore C# indipendente, con priorità di implementazione *non ancora approvata*; la milestone G4-QA visiva rimane aperta e la scelta G4-R3/G5 non è stata presa. Il codice validato resta G4-R2.


**Nota H2, solo documentazione:** acquisito il sorgente originale Gȧrt e verificati i percorsi della galleria GitHub in [GART_RIFERIMENTI_ORIGINALI_2026-11-17.md](GART_RIFERIMENTI_ORIGINALI_2026-11-17.md). Il QA G4 comprende ora il benchmark contro le **immagini autentiche** di Ocellus/Rete, oltre all’A/B interno R1/R2. Nessuna priorità o decisione G4-R3/G5 è stata cambiata. La versione di codice resta M4E.3-G4-R2.

> **Fonte operativa unica per la nuova chat.** **G4-R2 compilata e testata con esito positivo su Windows** (conferma dell'utente del 17/11/2026). Il pacchetto documentale **H1 non cambia il codice**. Tutti gli aggiornamenti sotto la linea **ARCHIVIO CRONOLOGICO** descrivono lo stato *al momento della singola revisione*: eventuali formule «non validato» e «prossimo G4-R2» non sono più attuali.

## Obiettivo e milestone da completare

| Ordine | Milestone | Stato attuale | Prossimo criterio concreto |
|---|---|---|---|
| **1** | **G4-QA** — Ocellus/Rete R2 | **Build + test Windows superati**; qualità visiva da valutare | Confrontare 6 coppie R1/R2 Full HD (stessi seed), zoom 100%, fori Ocellus/nodi e membrane Rete, preview/export, annotare ciò che va migliorato |
| **2** | **A3** — nuovo studio artistico | 29 generatori, 173 ricette; Crinis R10 sospeso | In base al risultato G4, scegliere **G4-R3** (fori topologici/film curvi) **oppure G5** (Nimbus, Pebble, Accretion, Glst); non assumere decisione già presa |
| **3** | **A0** — asset e packaging | Manifest/ZIP coerenti; cartella storica `examples/previews` assente | Recuperare o generare/validare realmente anteprime storiche; nessun link documentale rotto |
| **4** | **A1** — Studio moduli/controller | Partial e coordinatore rendering introdotti | Separare controller/lifecycle, documentare regressioni UI, DPI, cancellazione |
| **5** | **A2** — preview finale | **Realizzata** come raster nativo nei statici, test presenti | Completare smoke GUI per i nuovi renderer e scenari ad alto costo; evitare proxy impliciti |
| **6** | **A4** — qualità e 8K | **Parziale**: quattro geometrie 8K UHD standard; finiture a righe | Stress/memoria/cancel, strategie tile per renderer pesanti, 8192² solo dopo progetto dedicato |
| **7** | **A5** — QA e release | Suite Windows G4-R2 passata; il gate di rilascio non è concluso | Test visuali, regressioni desktop/DPI, Linux/macOS, dipendenze/licenze, benchmark e packaging |
| **8** | **Fase B** — compositore universale | **Pianificata dopo A** | Ricetta composta, layer diversi, maschere, transform, blend/alpha, export affidabile |
| **9** | **Fase D** — workflow artista | **Approvata, pianificata dopo B** | Varianti e seed explorer → confronto A/B → galleria → contact sheet → preset → SVG vettoriale selettivo |
| **10** | Animazioni avanzate | **Rinviate** | Riprendere solo per decisione esplicita; conservare il funzionamento loop/frame esistente |

**Regole fisse:** C#/.NET 10 (XAML/AXAML consentito), ZIP completo ogni consegna, 17 preset personali intatti, colori/parametri precisi, preview statica nativa=output, limite memoria per renderer, status test accurato, documentazione sempre aggiornata. Per i dettagli [Decisioni consolidate](DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md).

---

## ARCHIVIO CRONOLOGICO — decisioni e gate come apparivano alle rispettive revisioni (non stato attuale)

## 17/11/2026 — M4E.3-G4-R2: Ocellus interstizi e rilievo; Rete nodi e pellicole

**Baseline certificata dall'utente:** G4-R1.2 compila e tutti i test passano su Windows. Il blocco G4-R2 prosegue da quella versione, mantenendo integralmente Crinis R10 e gli hotfix R1.1/R1.2.

**R2 implementata nei sorgenti:** Ocellus aggiunge `pore-strength`/`pore-radius` e `rim-light`/`rim-width`; Rete aggiunge `junction-strength`/`junction-radius`, `node-glow` e `membrane-opacity`. I nuovi controlli sono opzionali e neutri per vecchie ricette JSON v1; i **preset iniziali Studio** dei due generatori sono invece espressamente aggiornati per mostrare gli effetti.

**Catalogo:** 29 generatori, 29 preset incorporati, 173 ricette JSON (6 nuove in `examples/recipes/m4e3-g4-r2/`); 17 preset personali e tutti gli altri generatori invariati. Nuovi test Algorithms/Application/Rendering, note e comparazioni nella [scheda G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md).

**Gate aperto:** la R2 richiede ancora build Release e test .NET su Windows (`clear && dotnet clean && .\tools\validate.ps1`) e verifica visiva A/B. Dopo il gate decidere se proseguire G4-R3 o passare a G5. Compositore B, workflow artista D, animazioni evolute più avanti.

---

## Aggiornamento 17/11/2026 — M4E.3-G4-R1.2: hotfix CS0136 (G4 ancora in validazione)

**Log Windows R1.1:** dopo la correzione di `Math.Hypot` (CS0117), Algorithms.Tests e Application.Tests compilano; `GartCellularRenderer.RenderRete` si ferma su CS0136 per il doppio nome locale `ink` (copertura del bordo vs. colore della palette). **R1.2** assegna nomi distinti `wallCoverage` e `wallArgb` e conserva formule e risultati previsti.

**Da chiudere prima di G4-R2:** 1) build Release integrale; 2) tutti i test xUnit; 3) smoke Ocellus/Rete in Studio/CLI a 1920×1080; 4) confronto preview ed esportazione; 5) riscontro visivo delle due famiglie G4. Catalogo e ricette invariati (29 generatori, 167 JSON, 17 preset personali). **Non considerare G4-R1.2 validata finché non arriva il log positivo Windows.**

---

## Aggiornamento 17/11/2026 — M4E.3-G4-R1.1: blocco CS0117 risolto nei sorgenti, gate Windows aperto

**Riscontro utente:** la prima compilazione G4-R1 si è fermata su quattro chiamate a `Math.Hypot`, assente in .NET 10. Correzione mirata con helper C# `GeometryDistance.Hypotenuse`; sostituiti sei richiami di produzione e reso indipendente l'oracolo del test di Rete. Aggiunta copertura su grandezze estreme.

**Criteri di chiusura:** 1) build Release senza CS0117; 2) tutte le suite xUnit verdi; 3) import/render di Ocellus e Rete nello Studio e CLI, 1920×1080, confronto preview/esportazione; 4) nessuna regressione dei 167 JSON e dei 17 preset personali. **Sorgenti corretti, validazione Windows ancora aperta.** Nessun algoritmo/ricetta nuovo in questo hotfix. Dopo il gate si torna a G4-R2, non a Crinis.

---

## Aggiornamento 17/11/2026 — M4E.3-G4-R1: Ocellus / Rete (priorità aggiornata)

**Decisione utente:** passare da Crinis ad altri algoritmi e ricette. La priorità di completamento artistico Crinis R11/R12 è **sospesa**, non eliminata; R10 resta nel pacchetto e i test Windows della R10 non sono ancora confermati.

**G4-R1 implementata:** `ocellus-eyespots` e `rete-foam` (kernel C# indipendenti, due descrittori, due renderer condivisi, preset Full HD, palette Auto per tutti i ruoli). 8 nuove ricette artistiche: 4 Ocellus e 4 Rete. Totale **29 generatori, 29 preset embedded e 167 esempi JSON**; 17 preset personali intatti. Build e test Windows di G4 ancora da verificare. [Scheda tecnica e verifica](M4E3_G4_OCELLUS_RETE.md).

**Rimanente G4-R2:** Ocellus flood-fill dei piccoli fori, Rete quattro distanze + film/nodi/drenaggio più fedeli; A5 test Windows e confronto visuale, A4 budget/memoria/8K. Seguono G5 e le altre ricette preferite, poi **Fase B compositore universale**, **Fase D varianti/galleria/contact sheet/preset**. Animazione avanzata dopo. Questa decisione sostituisce il vincolo storico R10 di non iniziare G4.

---

## Aggiornamento 17/11/2026 — M4E.3-G3-R10: profondità interna dei fasci di Crinis

**Richiesta artistica confermata:** G3-R9 compila e tutti i test passano su Windows (esito utente). Il risultato è migliorato nella **profondità tra le pennellate**, ma l'interno di una singola pennellata appare ancora piatto e le fibre risultano troppo minute, anche con `hair-width=5.767` nella ricetta allegata. Preservare la profondità fra fasci già raggiunta.

### R10 — implementato nel codice; validazione Windows e valutazione artistica da fare

- **Fibre riconoscibili:** distinguere microfibre di trama e filamenti principali più corposi tramite `featured-fibers`, senza ispessire indiscriminatamente ogni filo e trasformare il fascio in una macchia opaca. Confrontare a zoom 100% e a immagine completa.
- **Profondità interna:** creare tre sottostrati nello stesso fascio (retro, medio, fronte); dipingere in quest'ordine con sottoseed indipendenti dall'ordine, usando `intra-bundle-depth` per ombreggiare i fili arretrati e far emergere quelli frontali.
- **Luce trasversale:** `cross-section-light` crea creste e avvallamenti luminosi attraverso il fascio, in aggiunta al profilo R9 `bundle-lighting`, per ottenere mezzi toni e rilievo dentro la pennellata.
- **Occlusione di contatto interna:** `internal-occlusion` modula le ombre dei filamenti in base al sottostrato; in Studio è inattivo quando la profondità interna è zero.
- **Compatibilità:** i quattro parametri sono opzionali in JSON v1 e neutri per le ricette R9 e precedenti; quando tutti zero, il percorso di disegno, ordine RNG e raster restano invariati. Per nuovi lavori Studio sono attivati nel preset Crinis. `body-opacity=0` continua a eliminare ogni corpo/ombra poligonale del fascio.
- **Qualità e budget:** nessun aumento del numero di filamenti/campioni; il limite lavori preesistente resta attivo, si aggiungono solo tre passaggi sugli indici per ordinare gli strati. I PNG vecchi e nuovi devono restare deterministici e l'anteprima esatta deve coincidere con export.
- **Comparazioni:** tre esempi in `examples/recipes/m4e3-g3-r10/`: originale utente R9 copiato byte-identico, variante indaco R10 con stesso seed/dimensioni, variante rame R10. Totale **159 ricette JSON**, **27 generatori**, **27 preset integrati**.

### Criteri di chiusura R10

1. `clear && dotnet clean && .\tools\validate.ps1`: build Release e suite intera verdi su Windows, incluso catalogo R10 e regressioni old-JSON.
2. Confronto A/B dell'originale R9 e della variante indaco R10 a **1920×1080**, stessi seed/palette, con **Adatta** e **100%**.
3. All'interno di un singolo fascio devono essere distinguibili alcuni filamenti più corposi, zone di luce/mezzotono/ombra e sovrapposizioni interne, conservando il dettaglio fine e senza fondo pieno.
4. Confermare differenza visiva fra fasci in primo piano e in secondo piano, nessun degrado alla cancellazione, finiture e fine-2x.

**Dopo R10:** se necessario, R11 dedicata a spessori fisici più eterogenei, luce variabile lungo la curva e micro-sfocatura selettiva reale delle fibre posteriori, con budget memoria preventivo; R12 eventuale mappa locale di altezza/contatto a livello di fascio per incroci più complessi. Non anticipare G4 Ocellus/Rete, compositore universale B o Fase D finché Crinis non raggiunge il target visivo. Animazioni dopo.

---

## Aggiornamento 17/11/2026 — M4E.3-G3-R9: volume e luce di gruppo Crinis

- **In corso:** priorità A3 Crinis, ora con compressione/espansione trasversale monotona, rigonfiamento locale e luce di gruppo regolabili. Preservati il tratto a singoli filamenti e i JSON storici (nuovi default opzionali a zero). `hair-width` allargato fino a 8 px per coerenza con gli esempi R7/R8.
- **Consegnato:** 3 nuove ricette (156 totali), test unitari e di rendering dedicati, documentazione R9 e handoff. **SDK .NET non disponibile** qui: build e suite complete richiedono ancora validazione Windows.
- **Prossimo gate:** test Windows e confronto visivo sulle stesse ricette/seed. **Solo dopo:** valutare auto-occlusione tra fasci. Rimangono A3-G4 Ocellus/Rete, A1 rifinitura del controller, A4 raster 8K, A5 QA; quindi compositore B e workflow D. Animazioni avanzate successive.

---

## Aggiornamento 17/11/2026 — G3-R5: densità materiale e contratto di catalogo

- **A5 QA:** riparato il gruppo `Finitura` di Crinis (`ink-flecks` ora in `Microfibre`), test di contratto dedicato. Attendere verifica Windows R5.
- **A3 Crinis (miglioramento artistico):** filamenti più distinti alle densità elevate tramite `fiber-coverage`, riflessi/depositi irregolari tramite `pigment-contrast`; due varianti A/B a partire dalla ricetta reale e un nuovo studio cremisi.
- **Dopo la verifica visiva:** approfondire resa di ombre stratificate, trama 3D e differenze da Cincinnus originale; successivamente G4 Ocellus/Rete. B e D come pianificato, animazioni dopo.

---

## Aggiornamento 17/11/2026 — A3-G3-R4: coerenza di compilazione Release

- **Blocco A5 prioritario:** R3 compilata con 32/1088 test falliti per il contratto dei parametri Crinis mancante nell'assembly Application caricato, non per 32 problemi di rendering indipendenti. Sorgenti R3 contengono gia' i metadati; la causa piu' probabile e' la combinazione di asset Release obsoleti e file aggiornati.
- R4: validazione con pulizia Release automatica, gate catalogo anticipato, fallback `classic` per ricette in memoria prive della scelta. Verificare la suite Windows e produrre i due studi 2000x2000 prima di dichiarare il blocco chiuso.
- **Ordine successivo invariato:** confronto Crinis/Cincinnus, A3-G4 Ocellus/Rete, A4 8K sicuro, A1/ A5 stabilizzazione, quindi B compositore universale e D workflow dell'artista; animazioni rinviate.

---

## Aggiornamento 17/11/2026 — A3-G3-R3: Crinis microfibre

- Correzione `body-opacity=0`: niente corpo né ombre poligonali della fascia, in modalità classica e micro.
- Nuovo renderer microfibre con opzioni di densità, microtrama, volume, taper, centri di curvatura e speckle d'inchiostro.
- Sei studi nuovi, due 2000×2000, per confronto con i rendering Cincinnus dell'utente; 27 generatori e 146 ricette totali.
- **Stato:** codice e test predisposti; R2 validata su Windows, R3 da validare e confrontare visivamente. Rimane prioritaria la verifica estetica della distanza da Gȧrt prima di passare a G4 Ocellus/Rete.
- Rimangono confermate B compositore, D workflow artista e animazioni rinviate.

# M4E.3-G3-R1 — Fascia/Eclectic + correzione catalogo (17/11/2026)

- Ricevuto esito G2-R2: **1040/1041 test**; unica regressione `CatalogTests.CatalogHasStableSortedIdsAndLoopCapability` dovuta a un ordine alfabetico atteso invertito (`crinis` / `cotton`). Corretta **solo la lista di attesa**, in entrambe le coppie invertite (`cotton`/`crinis`, `spaced`/`spiral`); non l'ordinamento del prodotto.
- A3-G3: due generatori `fascia-ribbons` e `eclectic-trails` (campi vettoriali, nastri retinati, crescita con collisioni), otto nuove ricette, due preset embedded. Totale **27 algoritmi / 136 ricette**. Tutti i preset e le ricette precedenti preservati.
- Test nuovi per geometria, serializzazione, cambi di parametri, seed, preview/export, fine-2x, finiture, limiti e cancellazione. Guida `M4E3_G3_FASCIA_ECLECTIC.md`.
- **G3-R1 da validare su Windows**. Fase successiva G4 Ocellus/Rete; A1 e A4/A5 restano aperte; B/D dopo e animazioni rinviate.

---

# Aggiornamento correttivo — M4E.3-G2-R2 (17/11/2026)

Hotfix `xUnit2020` sul test di catalogo G2. G2-R1 non supera la compilazione Windows; G2-R2 richiede nuova validazione. Nessun cambio alle priorità: **G3 Fascia/Eclectic** dopo il gate, consolidamento A1, supporto 8K/A4 e benchmark A5; B compositore e D workflow dell’artista pianificate, animazioni rinviate.

---

# Aggiornamento A3-G2 — M4E.3-G2-R1 (17/11/2026)

Implementati `crinis-bristles` e `spiral-flamebrush`: 2 kernel geometrici indipendenti da Skia, 2 renderer su percorso condiviso, 2 preset embedded, 8 ricette Full HD, nuovi test e budget preventivi. **25 generatori / 128 ricette**. Nessuna modifica ai 17 preset personali e alle 120 ricette G1. M4E.2-R5 confermata da test Windows, G1/G2 non ancora eseguite sul PC utente. Prossima G3 Fascia/Eclectic/Glst, continuare A1/A4/A5 e in seguito B/D; animazioni rinviate. Vedi [`M4E3_G2_PENNELLI_CRINIS_FLAMEBRUSH.md`](M4E3_G2_PENNELLI_CRINIS_FLAMEBRUSH.md).

---

# Aggiornamento A3-G1 — M4E.3-G1-R1 (17/11/2026)

**Sviluppato:** Alien Letters V3 (reinterpretazione modulare), Cotton Circles (tasselli semicerchi), Rigor (blocchi periodici/lettere con influenza locale). Tre descrittori, kernel seeded condiviso, tre percorsi di rendering Skia in un unico modulo, tre palette ciascuno, nove nuove ricette Full HD. Catalogo a **23 generatori / 120 ricette**; tutti i precedenti generatori, 17 preset personali e 111 ricette storiche restano invariati. [Scheda tecnica](M4E3_G1_GEOMETRIE_MODULARI.md). **Implementazione non ancora verificata dal gate Windows**; M4E.2-R5 confermata dall'utente.

**Segue G2** Crinis/Flamebrush (setole, spine e occlusione) → **G3** Fascia/Eclectic/Glst/Cyanowaves (nastri, retinatura locale) → G4 forme SDF → G5 crescita → G6 volumi. A1/A4/8K e A5 continuano; la Fase B compositore e la Fase D artista restano confermate, animazioni evolute dopo.

---

# Studio Gȧrt documentale — 17/11/2026 (M4E.2-R5, nessun codice modificato)

Nuova sottoroadmap di **Fase A3** basata sull’esame diretto dell’archivio `gart-main-pulito(1).zip` e sul confronto con la baseline C# M4E.2-R5 **validata dall’utente (build e test passati)**. Dettagli verificabili in [STUDIO_GART_PORTING_M4E2.md](STUDIO_GART_PORTING_M4E2.md).

Ordine proposto: **A3-G1** grammatica di tasselli (Alien Letters V3, Cotton Circles, Rigor) → **A3-G2** setole/brush (Crinis, Flamebrush, Monet2) → **A3-G3** bande di flusso (Fascia, Eclectic, Glst, Cyanowaves) → **A3-G4** SDF organici (Ocellus, Rete, Pebbles) → **A3-G5** crescita (Accretion, Citymap) → **A3-G6** paesaggi/volumi (Strata, Sabulum, Undula, Nimbus). La scelta è **una proposta di sviluppo, non un porting già effettuato**. Proseguono A1/A4/A5; Fase B compositore e Fase D artista confermate; animazioni avanzate rinviate. Produrre codice solo C#/.NET 10, con SkiaSharp/Avalonia, e preservare i 17 preset personali.

---

# Aggiornamento correttivo — M4E.2-R5 (17/11/2026)

Risoluzione del blocco compilazione `CS0618` nella pipeline di finiture 8K della R4, mediante l'overload di `SKCanvas.DrawImage` non obsoleto. Test di copia senza ricampionamento aggiunto. **Nessun cambiamento delle priorità:** A1 refactoring Studio, A4 8K, A5 benchmark; quindi B compositore e D workflow. M4E.2-R1 è l'ultima confermata dall'utente; R4 ha fallito la compilazione, R5 resta da validare.

---

# Aggiornamento A4 — M4E.2-R4 (17/11/2026)

Finiture artistiche 8K con postprocess per righe per quattro generatori Skia geometrici: nessuna duplicazione completa Core.PixelBuffer oltre 4K. Tre nuove ricette. **A4 non conclusa:** 8K raster-heavy, supersampling 8K, encoding completamente incrementale e benchmark memoria restano futuri. M4E.2-R1 validata dall'utente; R2/R3/R4 da validare. Non anticipare B compositore o D workflow artista; animazioni più avanti.

---

# ROADMAP UNICA — GenerativeArt Studio


**Aggiornamento 17 novembre 2026 — M4E.2-R3:** estesa **A3** con due generatori: Fillotassi e Reazione–diffusione Gray–Scott, otto studi Full HD, **20 generatori / 108 ricette**. In Studio i pulsanti nel menu sinistro sono uniformi in larghezza. I 17 preset personali originali e gli esempi precedenti restano invariati. **A4 8K:** resta limitata ai quattro renderer geometrici della R2; i due nuovi non sono ancora 8K. Segue A4 ottimizzazione raster e A1 controller, poi A5 QA, B compositore, D workflow artista; animazioni avanzate rinviate. Vedi [`M4E2_R3_NATURA_E_NAVIGAZIONE.md`](M4E2_R3_NATURA_E_NAVIGAZIONE.md).


**Aggiornamento 17 novembre 2026 — M4E.2-R2:** avviata **A4** con un profilo 8K UHD statico controllato per quattro renderer geometrici (`guilloche`, `voronoi-mosaic`, `spaced-streamlines`, `poisson-stippling`), 7680×4320 al massimo, `standard` senza finiture. +4 ricette 8K (100 in totale). **Non** sono abilitati 8K per tutti i generatori, `fine-2x` 8K, materia 8K o 8192². I 17 preset utente rimangono invariati. Consultare [`M4E2_R2_8K_GEOMETRICI.md`](M4E2_R2_8K_GEOMETRICI.md). Baseline Windows confermata R3; R4/R5/M4E.2-R1/R2 ancora da confermare.

**Aggiornamento:** 17 novembre 2026, **M4E.2-R1**: avviata A3 con il generatore Wave Interference/Moiré, cinque ricette Full HD e test dedicati; 18 generatori e 96 ricette. **I 17 preset personali sono invariati**, il nuovo generatore ha il proprio preset. La baseline Windows confermata resta M4E.1-R3 (build, suite e preview/export); R4, R5 e M4E.2-R1 attendono conferma. A1 ancora parziale; A2 confermata nei flussi provati. Questa è **l'unica roadmap autorevole**. Per lo stato verificato usare `CURRENT_STATE.md`.

## Piano di sviluppo prioritario concordato (17/11/2026)

**Fase A — prima di nuove funzionalità strutturali:** A0 distribuzione/manifesto (52 anteprime storiche mancanti) e 17 preset Studio (già integrati); **A1** modularizzare `StudioWindow.cs` (partial in R1, coordinatore render in R2, editor dei parametri e dipendenze condizionali in R4; controller/lifecycle ancora aperti); **A2** anteprima calcolata **alla risoluzione di output** con `Adatta`/`100%` solo visivi (conferma utente in R3, QA DPI e altri dispositivi aperto); **A3** nuove ricette e algoritmi per test artistici/tecnici; **A4** qualità e dimensioni di output **8K (7680×4320)**, con 8192×8192 da valutare separatamente; **A5** gate Windows, benchmark e archivio completo.

**Fase B — poi:** **M4C.3 compositore universale**, editor livelli, maschere, opacità e blend. L'API layer esistente non equivale a un compositore generale.

**Fase D — dopo B:** workflow dell’artista: varianti, esplorazione semi/seed, A/B, galleria, contact sheet, preset personali condivisibili, export vettoriale selettivo più avanti. Nessun obbligo di implementare funzioni vettoriali per renderer raster-only.

**Animazioni:** differite a una fase successiva, **dopo A/B/D secondo le future priorità**. Conservare gli strumenti animati attuali ma **non dare precedenza** a noise 4D, timeline, GIF/APNG e streaming rispetto ad A/B/D.

**Specifiche, rischi, budget e criteri di accettazione:** [`M4E0_PIANO_A_B_DEFAULTS.md`](M4E0_PIANO_A_B_DEFAULTS.md). Le sezioni storiche M4–M8 più sotto restano come inventario di capacità e backlog, non ridefiniscono l'ordine concordato.

## 1. Obiettivo finale

Un solo **framework C#/.NET 10**, una CLI headless e uno **Studio Avalonia (XAML/AXAML consentito; prima versione code-only)**, per scegliere, generare, parametrizzare, animare, confrontare ed esportare opere di arte generativa. Fonti artistiche: Gȧrt e jdxyw/generativeart, senza dipendenze di runtime dai linguaggi originali. Priorità attuale: riproducibilità, qualità visuale, preview fedele e grandi formati; **animazioni rinviate**.

## 2. Stato milestone (verificato versus pianificato)

| Milestone | Stato alla revisione del 17/11/2026 | Evidenza / limite |
| --- | --- | --- |
| Audit Gȧrt e Go | Completati per progettazione | Sorgenti studiati; non suite originali completa |
| M0 — Solution, CI, xUnit | Validata Windows | Compilazione e test confermati nelle iterazioni successive |
| M1 — Matematica, colori, noise | Validata Windows | Suite sulla baseline corrente riuscita |
| M2A/M2B — PixelBuffer, Skia, PNG | Validata Windows | Test e PNG confermati |
| M3A/M3B — Campi vettoriali, RK4, streamlines | Validata Windows | Test e `render-flow-spaced` confermati |
| M3C.1 — Catalogo, fase ciclica e frame PNG | **Codice compilato/testato come parte di R1** | Smoke manuale del loop completo non ancora confermato |
| **M3C.2-R1 — Ricette JSON** | **Build, xUnit, JSON e PNG confermati dall'utente** | Test fix JSON risolto; prova completa della sequenza animata ancora suggerita |
| **M3C.3-R2 — Runtime generico** | **Build, suite xUnit e `list` confermati su Windows dall'utente** | Il test catalogo è corretto; smoke specifico di nuovi PNG/loop ancora da confermare |
| **M3D.1-R2 — Primo Studio funzionante** | **Build, test e apertura Studio confermati su Windows** | L'utente ha confermato il successo; smoke approfondito non ancora documentato |
| **M3D.1-R3 — Pan/zoom anteprima** | **Build, xUnit e smoke mouse confermati su Windows dall'utente** | Pan sinistro e zoom rotella perfetti; nessuna regressione segnalata |
| **M3D.2-R1 — Export e parametri esatti** | **Build, xUnit e Studio confermati su Windows** | Lo stress della cancellazione durante export pesante non è misurato separatamente |
| **M3D.2-R2 — Preferiti, recenti e temi** | **Build, test, Studio, preferiti e temi OK su Windows** | Conferma utente 16/11/2026 |
| **M4A.1-R1 — Marching Squares e isolinee** | **Build Windows fallita (CS0618)** | SKPath.MoveTo/LineTo obsoleti nel renderer; altri test non conclusi |
| **M4A.1-R2 — Fix SKPathBuilder** | **Confermata su Windows** | Compilazione, suite, Studio e CLI OK (utente) |
| **M4A.1-R3 — Budget hi-res** | **Test Windows falliti: 370/371** | `ArtworkRenderEngineTests` attendeva un rifiuto con 48 milioni di celle×livelli, ma la ricetta ne richiede 42.645.336 |
| **M4A.1-R4 — Correzione budget contorni** | **Confermata funzionante su Windows** | L'utente riferisce «funziona tutto» dopo il fix; baseline del nuovo incremento |
| **M4A.2-R1 — SDF/CSG e Poisson disk** | **Validata su Windows** | Compilazione, suite, Studio e CLI OK (utente); catalogo a 8 |
| **M4A.3-R1 — Voronoi e tassellazioni** | **Compilazione e test passati su Windows** | Conferma utente 16/11/2026: catalogo a 9, Voronoi/Lloyd |
| **M4A.4-R1 — Laboratorio artistico** | **Build e suite xUnit confermate su Windows** | Conferma utente del 16/11; colori 4 opere, Poisson e topografia creativi, ricette Full HD |
| **M4A.4-R2 — Editor colore completo** | **Implementata; gate Windows aperto** | Ruoli per nove opere, HSV/RGB/alpha/HEX, Auto/JSON storico, nuovi test ed esempi |
| **M4A.4-R3 — qualità raster 2×** | **Implementata; gate Windows aperto** | Campionamento 2× per 9 opere, PNG premoltiplicato, pixel budget e suite dedicata; finitura introdotta successivamente in M4C.1 |
| **M4C.1-R1 — Finitura seeded grana/Bayer** | **Implementata, limite ampliato in M4C.2-R3** | Quattro parametri opzionali per nove artwork, default neutri, alpha preservato, post-processing dopo 2×; da R3 budget 16.777.216 pixel finali in standard |
| **M4C.2-R1 — Materia cartacea/pigmenti** | **Build Windows fallita: CS8506, CS8131** | Quattro controlli facoltativi per nove artwork, carta cotone/ruvida, pigmento granulare, assorbimento ai bordi, alpha/seed preservati; cinque esempi Full HD |
| **M4C.2-R2 — Hotfix HSV + avvio massimizzato** | **Build Windows OK, 506/507 test** | Sei tuple HSV coerenti (`0d`); unico fallimento in fixture JSON non valida |
| **M4C.2-R3 — JSON/5 MP/preview** | **Build OK, 513/514 test (utente)** | Test materia obsoleto: attende il rifiuto di 4000×4000 standard, ora ammesso |
| **M4C.2-R4 — Fix test budget materia** | **Build Release OK, 521/521 test (Codex Windows)** | Accettazione standard fino a 4096×4096; rifiuto oltre limite e fine-2x eccessivo prima dell'output; prodotto invariato; smoke manuali aperti |
| **M4B.1-R1 — Domain Warp/LIC e streamlines dense** | **Build Release e 558/558 test; sette PNG Full HD e picker headless OK (Codex Windows)** | Due nuovi algoritmi/artwork, catalogo a 11, sette ricette, massimo 10000 streamlines con distanza/spessore, footer picker fisso; desktop/DPI e altri OS aperti |
| **M4D.1-R1 — Dettaglio e controllo** | **Build Release e 626/626 test; otto PNG Full HD e controlli Studio headless (Codex Windows)** | Guilloché/incisione, dieci controlli Domain Warp, otto ricette, catalogo a 13; compositore M4C.3 e nuovi loop ancora futuri |
| **M4D.2-R1 — Pittura e generatori iniziali** | **Build Release e 733/733 test, 14 PNG Full HD, Studio e 12 frame CLI verificati (Codex Windows)** | Pennelli/paesaggi, evoluzione dei primi cinque renderer, catalogo a 15, 63 ricette, 13 PNG storici identici; desktop/DPI e altri OS aperti |
| **M4D.3-R1 — Profondità** | **Build Release e 801/801 test, dieci PNG Full HD e Studio verificati (Codex Windows)** | Carta/bokeh e livelli riutilizzabili, paesaggi migliorati, 17 generatori e 73 ricette; 15 PNG precedenti identici. Editor livelli/maschere/blend ancora aperto |
| **M4D.4-R1 — SDF macchie/fusioni** | **Build Release e 859/859 test; otto PNG Full HD, Studio, CLI e AA verificati** | SDF organico, pigmenti/vetro, 36 slider, sei palette, 81 ricette; classico e 15 hash PNG precedenti preservati |
| **M4D.4-R2 — Preset Studio da allegati** | **Codice e piano aggiornati; test Windows da confermare** | Diciassette ricette embedded; seed/parametri/animazione iniziali, compatibilità delle ricette importate |
| **Fase A (A0–A5)** | **A0 parziale, A1 in corso, A2 validata nel flusso provato, A3 nuovo algoritmo avviato** | Packaging storico ancora aperto; nuovi studi Full HD e Wave Interference; 8K ancora aperto |
| **M4E.1-R1 — Refactoring A1 (primo incremento)** | **Implementato; gate Windows aperto** | `StudioWindow.cs` ridotta a lifecycle e wiring; 7 partial per layout/catalogo/editor/preview/playback/file/stato, parser ricetta separato + test |
| **M4E.1-R2 — Coordinatore A1 + anteprima A2** | **Build Windows OK, 879/880 test (utente)** | Preview/output alla stessa risoluzione; un test sulla frequenza con campo vortex fallito, corretto in R3. A1 e QA A2 ancora aperti |
| **M4E.1-R3 — Fix test frequency/vortex** | **Validata dall’utente** | Build e test passati; preview ed export identici, confermati il 17/11/2026 |
| **M4E.1-R4 — Parametri condizionali** | **Implementata, test Windows aperti** | Controlli inapplicabili disabilitati senza perdere i dati; renderer e preset invariati |
| **M4E.1-R5 — Documenti + dieci studi** | **Implementata, test Windows aperti** | Servizio JSON in Application, sette test servizio + un test fixture, 91 ricette; nessun nuovo algoritmo e nessun cambio ai PNG storici |
| **M4E.2-R1 — Interferenze d'onda / Moiré (A3)** | **Implementata, gate Windows aperto** | Nuovo kernel a sorgenti radiali seeded e renderer, tre stili, 5 ricette Full HD, 18 generatori / 96 ricette, 17 preset personali byte-identici; limite di 150M contributi d'onda |
| **Fase B — M4C.3** | **Pianificata dopo A** | Compositore universale livelli/maschere/blend |
| **Fase D — Workflow dell’artista** | **Pianificata dopo B** | Esplora seed, varianti, confronto A/B, galleria, contact sheet, preset, SVG selettivo in fase successiva |
| M4–M8 — Motore artistico ed export avanzato | Pianificati in parte / storici | Vedi sotto; animazioni posticipate |

Non abbiamo ancora risultati documentati per Linux/macOS, stress di animazioni lunghe o parità visiva multipiattaforma.

### M4E.1-R1 — Primo refactoring Studio (A1)

La finestra monolitica è stata ripartita in otto file `StudioWindow*.cs`, preservando handler, lifecycle e disposizione UI. La conversione/validazione testuale della ricetta è ora in `StudioRecipeFormParser` (Application, senza Avalonia) con regressioni indipendenti. **Non è ancora chiusa A1**: controller autonomi per preview/export e ulteriori test Studio headless sono i successivi interventi, prima di A2. Roadmap D dettagliata in [`M4E1_A1_WORKFLOW_D.md`](M4E1_A1_WORKFLOW_D.md).

### M4C.1-R1 — Finitura grana/dithering (implementazione non ancora validata)

Grana per-pixel deterministica da seed e coordinate, con dimensione cella 1–12 pixel, e retino ordinato Bayer 4×4/8×8 con 2–32 livelli RGB/canale. Configurazione facoltativa JSON v1 per tutti e nove i generatori, default spento. La finitura si applica **dopo** l'eventuale downsampling fine-2x; alpha intatto, texture stabile nei loop; in M4C.2-R3 il precedente limite delle finiture viene elevato a 16.777.216 pixel finali in standard. Disponibili test dedicati e quattro ricette Full HD; QA del nuovo limite ancora aperto. Dettagli [`M4C1_GRANA_DITHERING.md`](M4C1_GRANA_DITHERING.md).

### M4C.2-R1 — Materia cartacea e pigmenti (implementazione in attesa di QA Windows)

Trama procedurale cotton/rough, pigmenti granulari da noise spaziale e assorbimento locale del colore scuro lungo i bordi, con ring buffer di tre righe. JSON v1 e comportamento storico preservati, cinque esempi Full HD e test dedicati. Specifica [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md).

**Priorità prossima aggiornata:** **A0–A5 Fase A** (prima refactoring della finestra e anteprima nativa, poi algoritmi/ricette e 8K), **Fase B M4C.3 solo dopo A**, animazioni rinviate. Vedi il piano concordato all’inizio della roadmap.

## 3. R0 — Primo Studio utilizzabile

### M3C.3 — Runtime comune e tre famiglie pilota (BUILD, SUITE E LIST CONFERMATI SU WINDOWS)

**Dipendenze:** M3C.2-R1 validata, catalogo e ricette v1 esistenti. **Codice realizzato in M3C.3-R1 e test catalogo corretto in R2**: runtime comune, dispatch 5 opere, generatori pilotati da ricetta, CLI compatibile, test e nuovo Plasma. Resta da approfondire il gate smoke PNG/loop.

1. Contratto renderer indipendente da CLI/Avalonia; versioni algoritmo, seed, parametri e capacità dell'opera.
2. Collegare `noise-flow` e `spaced-streamlines` a `render-recipe`, applicando rispettivamente `lines` e `max-lines` anche al contenuto effettivo dell'immagine.
3. Portare `parametric-curves` e `looping-flow` sul dispatch comune senza cambiare il significato delle ricette v1.
4. Aggiungere un artwork pixel/noise (prima famiglia Plasma) con descrittore, validazione parametri e rendering deterministico.
5. Integrare CLI `list`, `describe`, recipe, render e comandi legacy in una sola pipeline; messaggi di errore chiari per capacità non supportate.

**Gate M3C.3:** build Release, suite xUnit e `list` confermati dall'utente in R2; **restano** smoke approfonditi PNG statici/flow/Plasma, loop frame+manifest e compatibilità dei legacy.

### M3D.1 — Studio Avalonia minimo (R3 CONFERMATA SU WINDOWS)

- Nuovo progetto Desktop C#/.NET 10 senza server web locale. **XAML/AXAML è permesso**; in R2 l'interfaccia rimane code-only per limitare l'intervento al bootstrap.
- Catalogo ricercabile; anteprima centrale con zoom; pannello parametri tipizzato; palette, seed, rigenerazione e export PNG.
- **Animazioni già nel primo Studio:** Play/Pausa/Loop e controllo fase; preview cancellabile/debounced, rendering fuori dal thread UI.
- Aprire/salvare ricette v1; stessa libreria Application e stessi generatori della CLI.
- **Implementato e validato build/test/avvio in R2:** `GenerativeArt.Studio` Avalonia 12.1.6 con catalogo, pannello parametri tipizzato, anteprima, seed, JSON v1 e PNG, Play/Pausa/Loop/slider fase. `StudioPreviewPolicy` limita la risoluzione, `AtomicOutputFiles` evita la pubblicazione di file locali parziali.
- **R3 validata dall'utente:** `PreviewViewport` con pan sinistro, zoom rotella localizzato sul cursore, sincronizzazione slider, Centra, trasformazioni preservate tra frame della stessa dimensione e test xUnit senza GUI. Build Release, suite e smoke mouse confermati su Windows; monitorare RAM/dispose e comportamento Linux quando disponibile. Export animazioni completo resta CLI/M5.

### M3D.2 — Rifinitura Studio / R0

- Recenti/preferiti leggeri, UI chiaro/scuro, stato dei job, progressi, annullamento.
- Verifiche memoria native e managed, restore da cartella pulita, regressioni ricette, pacchetto release preliminare.
- Nessun marketplace plugin, editor nodale o login in questa fase.
- **M3D.2-R1 validata build/test/avvio dall'utente:** `AtomicOutputFiles.WriteGeneratedAsync` per export locale PNG via stream cancellabile senza buffer PNG managed aggiuntivo; progress bar indeterminata (non percentuali), Stop e preview differita, editor numerico con TextBox esatto e normalizzazione culturale.
- **M3D.2-R2 validata su Windows:** preferiti dei generatori, filtro, ricette JSON locali recenti (max 8), tema chiaro/scuro senza ricreare preview, JSON settings versionato locale e writer atomico, avviso su preferenze corrotte, test xUnit aggiuntivi.
- **Gate M3D.2 ancora da approfondire:** QA memoria reale su loop lunghi, restore in cartella pulita, provider virtuali e Linux/macOS; non garantire atomicità non disponibile.

## 4. R1 — Espansione artistica e laboratorio

### M4A — Campi scalari e geometria

Marching Squares (casi ambigui e stitching), SDF 2D e operazioni CSG, campionamento Poisson e prime tassellazioni. **Gate:** casi degeneri, contorni validi, una nuova opera per tecnica.

**M4A.1-R4 validata dall'utente:** Marching Squares, `topographic-contours` v1 (sei opere) ed export per poster, entro il budget hi-res.

**M4A.2-R1 validata su Windows:** `SignedDistance2D` con CSG e smooth union; `PoissonDiskSampler` seeded con griglia spaziale bounded; opere `sdf-orbits` e `poisson-stippling` (catalogo a otto), test e ricette. La milestone M4A.4 amplia i limiti Poisson a 50.000 punti / 6 milioni celle (ancora da validare). Dettagli in [`M4A2_SDF_POISSON.md`](M4A2_SDF_POISSON.md), isolinee in [`M4A1_ISOLINEE.md`](M4A1_ISOLINEE.md).

**M4A.3-R1 validata (build e suite Windows, utente):** `VoronoiTessellation` in Algorithms (semipiani, celle convesse, aree/centroidi, Lloyd), artwork `voronoi-mosaic` (catalogo a nove), poligoni Skia e ricetta poster 4000×4000. Scala limitata a 196 semi per opera per contenere la complessità quadratica; nessun Fortune/Delaunay già implementato. Dettagli in [`M4A3_VORONOI_TASSELLAZIONI.md`](M4A3_VORONOI_TASSELLAZIONI.md).

**M4A.4-R1 validata con build e suite su Windows:** Studio Full HD per nuove ricette, pannello colori HEX/Auto per ruoli, Poisson con limiti ampliati e variazioni di punti, topografia con stile organico/Chaikin, JSON v1 retrocompatibile per parametri aggiunti. Dettagli in [`M4A4_LABORATORIO_ARTISTICO.md`](M4A4_LABORATORIO_ARTISTICO.md).

**M4A.4-R2 implementata (gate Windows aperto):** ruoli colore dei cinque generatori storici, ColorPicker HSV/RGB/alpha e campioni del preset Auto per tutti e nove. Dettagli in [`M4A4_R2_COLORI_HSV.md`](M4A4_R2_COLORI_HSV.md). **M4A.4-R3 implementata (gate Windows aperto):** parametro render-quality su 9 opere, campionamento 2× bounded, output PNG con downsampling premoltiplicato e nuovi test. Vedi [`M4A4_R3_SUPERSAMPLING.md`](M4A4_R3_SUPERSAMPLING.md). **Sequenza aggiornata:** M4C.1 (grana/Bayer) e M4C.2 (carta/pigmenti) sono implementate, con gate Windows ancora aperto. Prossimi incrementi: M4C.3 compositing per livelli/maschere, M4B noise/domain warping/loop e M4D pennelli organici. Non disattivare i budget di memoria per ottenere risoluzioni più alte.

### M4B — Noise e flussi avanzati

**M4B.1-R1 implementata e verificata su Windows:** `DomainWarpNoise2D`, LIC discreta su RK2, artwork `domain-warp`/`lic-flow`, sette ricette Full HD e streamlines fino a 10000. Build Release, 558/558 test e output Full HD verificati da Codex. Dettagli in [`M4B1_DOMAIN_WARP_LIC.md`](M4B1_DOMAIN_WARP_LIC.md). **Da sviluppare in M4B.2, dopo le fasi A/B:** derivate noise, noise 4D ciclico e loop dei nuovi artwork. Gate futuro animazioni: phase 0/1 equivalente, budget per campioni, risultati seeded e test numerici.

### M4C — Pixel/colore/post-processing

**Implementati:** dithering Bayer, grana seeded, carta/pigmenti e, in M4D.3-R1, base di livelli SrcOver con blur gaussiano/drop shadow/opacità, aperture even-odd e alone bokeh. Build, alpha e budget verificati su Windows. **M4C.3 ancora da completare:** editor di livelli, maschere arbitrarie e modalità blend nelle ricette. Restano bloom generale, vignettatura e gestione lineare/sRGB; HDR solo dopo benchmark. **Gate:** filtri deterministici con alpha corretto e memoria limitata.

### M4D — Pennelli e materia

**M4D.4-R1 implementata e verificata:** riprogettazione di Orbite SDF in macchie/fusioni con quattro pigmenti, venature, rilievo, piani e AA locale; classico preservato per i JSON precedenti. Otto ricette Full HD, due fine-2x; 859/859 test, Studio/CLI e confronto AA contro riferimento 4×. Guida [`M4D4_SDF_MACCHIE_FUSIONI.md`](M4D4_SDF_MACCHIE_FUSIONI.md). Restano simulazioni pittoriche più avanzate e editor generale M4C.3.

**M4D.3-R1 implementata e verificata:** carta ritagliata/bokeh, livelli filtrati riutilizzabili e profondità nei paesaggi. Dieci ricette Full HD, due fine-2x; catalogo a 17, 73 ricette, 801/801 test, Studio e quindici PNG precedenti identici. Guida [`M4D3_PROFONDITA.md`](M4D3_PROFONDITA.md). Non chiude l'editor generale M4C.3.

**M4D.2-R1 implementata su richiesta di nuovi algoritmi e primi generatori più ricchi:** setole con pressione/rastremazione e profili frattali, artwork pennelli/paesaggi; cinque generatori iniziali estesi e nuovi default per lavori Studio. Build Release, 733/733 test, 14 preset Full HD, sequenza CLI e smoke Studio verificati. Guida [`M4D2_PITTURA_EVOLUZIONE.md`](M4D2_PITTURA_EVOLUZIONE.md). Restano pennelli/simulazioni più avanzate e compositore generale.

**M4D.1-R1 implementata su richiesta di dettaglio e controllo:** Guilloché e incisione tonale, otto ricette Full HD, colori e precisione indipendenti; Domain Warp esteso con default neutrali; input numerico fine e protezione degli editor rimossi nello Studio. Build Release, 626/626 test, PNG Full HD e smoke controlli verificati da Codex su Windows. Guida [`M4D1_DETTAGLIO_CONTROLLO.md`](M4D1_DETTAGLIO_CONTROLLO.md). Questo incremento non chiude tutta M4D né il compositore M4C.3.

Setole, pressione, pennellate organiche, simulazioni più ricche dell.acquerello, marbling, incisione e ombre. I semplici filtri cartacei/pigmenti sono già disponibili in M4C.2. **Crinis** è benchmark estetico prioritario. **Gate:** primitive riutilizzabili e opere visive di confronto.

### M5A — Ricette evolve, varianti e Sweeper (Fase D, dopo B)

Preset, confronto A/B, batch lazy con limiti, galleria, preferiti, contact sheet/poster. Scoring estetico solo opzionale e assistivo; non accumulare combinazioni o frame.

### M5B — Metadati PNG, ricerca, ergonomia

Ricetta embedded PNG con limiti/CRC/versione, ripristino immagini, indicizzazione catalogo e thumbnail cache bounded.

### M5C — GIF/APNG in streaming

Encoder progressivi con tempo fotogrammi corretto, cancellazione e cleanup; **dopo le Fasi A e B**; prima di eventuali codec video. Animazioni Go rinviate su richiesta del 17/11/2026. **Gate:** file decodificabili e memoria non proporzionale al numero totale dei frame.

## 5. R2 — Produzione professionale

- **M6A:** recorder di primitive + vero **SVG vettoriale**, distinguendo raster-only.
- **M6B:** PDF vettoriale e output per plotter; ottimizzazione tratti con limiti.
- **M6C:** timeline, fixed timestep per simulazioni, frame sink, motion blur temporale.
- **M6D:** video MP4 opzionale dopo analisi codec/licenze/RID; HDR e PNG16 solo se valore misurato.

## 6. R3 — Ricerca e algoritmi specialistici

- **M7A:** particelle, fluidi, Gray-Scott, reaction-diffusion anisotropa, automi.
- **M7B:** frattali, quasicristalli, attractors, tassellazioni matematiche avanzate.
- **M7C:** SDF3D/raymarching, clipping camera, luci, ottica/caustiche, 3D selettivo.
- **M8:** benchmark, hardening, CI multipiattaforma, packaging/distribuzione, documentazione finale, scelta licenza.

## 7. Regole per avanzare

Ogni milestone richiede codice C# con test xUnit, controllo dei parametri, budget e gestione errori; quando coinvolge rendering serve smoke di output. Lo stato si distingue in **previsto**, **implementato**, **build confermata**, **suite confermata**, **smoke confermato**, **gate chiuso**. Non promuovere uno stato sulla base di analisi statica o integrità ZIP. L'assenza di SDK in un ambiente non cancella le verifiche Windows già fornite dall'utente.

Aggiornare a ogni consegna **CURRENT_STATE, ROADMAP, CHANGELOG e HANDOFF**. Il **README** mantiene solo descrizione del progetto, installazione ed esecuzione, con screenshot sotto il titolo; aggiornarlo quando cambia l'uso del prodotto, senza inserirvi changelog, roadmap o note di consegna. Consegnare **ZIP completo**, con codice, test, esempi, licenze e documentazione, non soltanto differenze. Conservare il vincolo C# + PowerShell minimo.

**Riferimenti:** [`VISIONE_PRODOTTO.md`](VISIONE_PRODOTTO.md), [`ARCHITETTURA_PRODOTTO.md`](ARCHITETTURA_PRODOTTO.md), [`QUALITY_GATES.md`](QUALITY_GATES.md), [`RIFERIMENTI_ARTISTICI_PREFERITI.md`](RIFERIMENTI_ARTISTICI_PREFERITI.md).
