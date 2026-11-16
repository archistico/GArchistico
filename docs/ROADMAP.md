# ROADMAP UNICA — GenerativeArt Studio

**Aggiornamento:** 16 novembre 2026, M4D.2-R1 (pennelli/paesaggi, cinque generatori iniziali ampliati; 15 generatori, 63 ricette, build Release e 733/733 test Windows da Codex, 14 preset Full HD, sequenza CLI e Studio verificati). Questa è **l'unica roadmap autorevole**. Per lo stato verificato usare `CURRENT_STATE.md`.

## 1. Obiettivo finale

Un solo **framework C#/.NET 10**, una CLI headless e uno **Studio Avalonia (XAML/AXAML consentito; prima versione code-only)**, per scegliere, generare, parametrizzare, animare, confrontare ed esportare opere di arte generativa. Fonti artistiche: Gȧrt e jdxyw/generativeart, senza dipendenze di runtime dai linguaggi originali. Priorità: riproducibilità, prestazioni limitate da budget, qualità visuale e **animazioni**.

## 2. Stato milestone (verificato versus pianificato)

| Milestone | Stato al 16/11/2026 | Evidenza / limite |
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
| M4–M8 — Motore artistico ed export avanzato | Pianificati | Vedi sotto; non dichiararli già implementati |

Non abbiamo ancora risultati documentati per Linux/macOS, stress di animazioni lunghe o parità visiva multipiattaforma.

### M4C.1-R1 — Finitura grana/dithering (implementazione non ancora validata)

Grana per-pixel deterministica da seed e coordinate, con dimensione cella 1–12 pixel, e retino ordinato Bayer 4×4/8×8 con 2–32 livelli RGB/canale. Configurazione facoltativa JSON v1 per tutti e nove i generatori, default spento. La finitura si applica **dopo** l'eventuale downsampling fine-2x; alpha intatto, texture stabile nei loop; in M4C.2-R3 il precedente limite delle finiture viene elevato a 16.777.216 pixel finali in standard. Disponibili test dedicati e quattro ricette Full HD; QA del nuovo limite ancora aperto. Dettagli [`M4C1_GRANA_DITHERING.md`](M4C1_GRANA_DITHERING.md).

### M4C.2-R1 — Materia cartacea e pigmenti (implementazione in attesa di QA Windows)

Trama procedurale cotton/rough, pigmenti granulari da noise spaziale e assorbimento locale del colore scuro lungo i bordi, con ring buffer di tre righe. JSON v1 e comportamento storico preservati, cinque esempi Full HD e test dedicati. Specifica [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md).

**Priorità prossima:** M4C.3 compositing a livelli e maschere, M4B domain warping/loop fluidi; gate Windows e QA per R2–M4C.2 alla prima occasione.

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

**M4B.1-R1 implementata e verificata su Windows:** `DomainWarpNoise2D`, LIC discreta su RK2, artwork `domain-warp`/`lic-flow`, sette ricette Full HD e streamlines fino a 10000. Build Release, 558/558 test e output Full HD verificati da Codex. Dettagli in [`M4B1_DOMAIN_WARP_LIC.md`](M4B1_DOMAIN_WARP_LIC.md). **Da sviluppare in M4B.2:** derivate noise, noise 4D ciclico e loop dei nuovi artwork. Gate futuro animazioni: phase 0/1 equivalente, budget per campioni, risultati seeded e test numerici.

### M4C — Pixel/colore/post-processing

**Già implementati (gate aperto):** dithering Bayer, grana seeded, carta cotone/ruvida, granulazione e assorbimento. **Da sviluppare:** livelli/maschere/blending M4C.3, convolution controllata senza aloni, bloom, vignettatura e gestione consapevole lineare/sRGB; HDR solo dopo benchmark. **Gate:** filtri stabili e deterministici con alpha corretto, limiti memoria.

### M4D — Pennelli e materia

**M4D.2-R1 implementata su richiesta di nuovi algoritmi e primi generatori più ricchi:** setole con pressione/rastremazione e profili frattali, artwork pennelli/paesaggi; cinque generatori iniziali estesi e nuovi default per lavori Studio. Build Release, 733/733 test, 14 preset Full HD, sequenza CLI e smoke Studio verificati. Guida [`M4D2_PITTURA_EVOLUZIONE.md`](M4D2_PITTURA_EVOLUZIONE.md). Restano pennelli/simulazioni più avanzate e compositore generale.

**M4D.1-R1 implementata su richiesta di dettaglio e controllo:** Guilloché e incisione tonale, otto ricette Full HD, colori e precisione indipendenti; Domain Warp esteso con default neutrali; input numerico fine e protezione degli editor rimossi nello Studio. Build Release, 626/626 test, PNG Full HD e smoke controlli verificati da Codex su Windows. Guida [`M4D1_DETTAGLIO_CONTROLLO.md`](M4D1_DETTAGLIO_CONTROLLO.md). Questo incremento non chiude tutta M4D né il compositore M4C.3.

Setole, pressione, pennellate organiche, simulazioni più ricche dell.acquerello, marbling, incisione e ombre. I semplici filtri cartacei/pigmenti sono già disponibili in M4C.2. **Crinis** è benchmark estetico prioritario. **Gate:** primitive riutilizzabili e opere visive di confronto.

### M5A — Ricette evolve, varianti e Sweeper

Preset, confronto A/B, batch lazy con limiti, galleria, preferiti, contact sheet/poster. Scoring estetico solo opzionale e assistivo; non accumulare combinazioni o frame.

### M5B — Metadati PNG, ricerca, ergonomia

Ricetta embedded PNG con limiti/CRC/versione, ripristino immagini, indicizzazione catalogo e thumbnail cache bounded.

### M5C — GIF/APNG in streaming

Encoder progressivi con tempo fotogrammi corretto, cancellazione e cleanup; **prima del video**, a causa dell'alta priorità delle animazioni Go. **Gate:** file decodificabili e memoria non proporzionale al numero totale dei frame.

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
