> **STATO CORRENTE M4E.5-P4K-R1 — 17/11/2026:** priorità richiesta dall'utente: **4K per tutti i generatori e le ricette, con massima complessità e supersampling 2×; 8K generale successivamente**. Disponibili pulsante Studio **4K · qualità massima**, CLI `prepare-4k`, profilo opzionale `production` e 34 nuovi esempi: catalogo **30 generatori / 30 preset / 216 ricette**. **1.371 test Windows passati**, build Release zero avvisi/errori; azione Studio verificata sui 30 generatori e 30 file preset invariati, inclusi i 17 personali. [Guida e limiti](M4E5_4K_PRODUZIONE.md), [prove e misure](validation/P4K_R1_REPORT.md). Consegna completa `GenerativeArt_M4E5_P4K_R1_COMPLETO.zip`. Restano distinti QA artistico dell'utente, desktop/DPI, altri OS, Fase B/D e Crinis R10 in pausa. README solo descrizione/installazione/esecuzione con screenshot. **I blocchi delle versioni precedenti sotto sono archivio storico.**
## 17 novembre 2026 — M4E.4-V1-R1: Voronoi con mosaici in profondità

- Nuova modalità layered del Mosaico Voronoi: 2–8 piani indipendenti, selezione esatta e progressiva di tessere mancanti random/clustered, fondo più denso e scarto fra piani.
- Smussi e facce illuminate, ombre gaussiane fra piani, oscuramento/atmosfera/blur con la distanza. Palette posteriore e colori ombra/velatura, alpha conservata.
- 22 controlli opzionali JSON; Studio con 27 editor numerici e 10 colori, controlli condizionali. Modalità flat, resa precedente e tutti i file dei 30 preset incorporati (17 personali) invariati.
- Sei nuove ricette Full HD, due fine-2x, 182 esempi totali; catalogo a 30. Confronto piatto/piani/profondità, anteprime e screenshot reale Studio con rendering nativo.
- Build Release senza avvisi/errori, **1.260/1.260 test Windows**, 182 ricette valide, sei PNG decodificati, Studio/CLI e quattro hash PNG precedenti verificati. Guard geometrici/raster prima dell'output; 8K flat preservato, percorso filtrato con cap dedicati.
- [Guida ed evidenze](M4E4_V1_VORONOI_PROFONDO.md); ZIP integrale. Nessuna nuova dipendenza di produzione, nessuna modifica a Crinis/Cincinnus/G4.

## 17 novembre 2026 — Cincinnus C5-R1: correzioni e validazione Windows

- Corretto errore di compilazione xUnit2012 nel controllo dei pixel del pennello, mantenendo l'asserzione equivalente con Assert.Contains.
- Corretto il test degli otto pigmenti: verifica degli ID colore esatti, escludendo il parametro numerico pigment-granulation.
- Collegati i nove campioni Auto di Cincinnus alle palette del renderer e al campionamento dello sfondo già usato dal pittore. Aggiunte quattro regressioni per le tre palette e i ruoli inesistenti/non cromatici.
- Clean/restore/build Release senza avvisi/errori, **1.225/1.225 test Windows**; gate rapido 21 test incluso nel totale, tool autonomo compilato, 176 ricette validate, tre PNG CLI 768×768 e sei combinazioni Studio headless.
- Nessuna modifica ai preset, ai parametri JSON, ai budget, alla geometria o alle formule pittoriche; aggiornati documenti, manifest e ZIP integrale. README riportato a descrizione/installazione/esecuzione con screenshot.
- [Rapporto ed evidenze](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). Approvazione estetica, desktop/DPI e stress restano aperti.

> **STATO OPERATIVO C5 — 17/11/2026:** Cincinnus integrato *sperimentalmente* come 30° generatore nel catalogo/Studio/CLI, 30 preset embedded e 176 esempi JSON (173 storici + 3 C5). Supporta Simple/Adaptive, otto pigmenti modificabili singolarmente, tre palette, parametri/ombre e budget 2048 px / 2.097.152 pixel. **C1 e C2 test/build Windows confermati; C3, C4 e C5 non ancora compilati/testati dall'utente**, nessun giudizio estetico PNG. Leggere [scheda C5](CINCINNUS_C5_STUDIO_CLI_JSON_2026-11-17.md) e [QA](validation/CINCINNUS_C5_STATIC_QA.md). Le intestazioni C4/C3 seguenti sono storiche.

## 17/11/2026 — M4E.3-Cincinnus-C5 — integrazione catalogo e rendering condiviso (sperimentale)

- Inserito `cincinnus-bristles` come **30° generatore** e preset embedded, con modalità Simple e Adaptive e parametri artistici tipizzati.
- Collegate `CincinnusFlowField`, `SimplePlacer`, `AdaptivePlacer`, `StrokePlanner` e `CincinnusBristlePainter` al renderer standard condiviso da Studio e CLI.
- Palette Crimson originale e due varianti Ocean/Moss, tutti gli otto pigmenti singolarmente sovrascrivibili, controllo sfondo e due livelli d'ombra.
- Validazione dei budget (256..2048 pixel per lato, max 2.097.152 pixel; fine-2x limitato), conservazione dei controlli inattivi nella ricetta e modalità `require-complete` per fallire su copertura incompleta.
- Aggiunto 1 preset e 3 esempi; totale **30 generatori / 30 preset embedded / 176 esempi JSON**. **29 vecchi preset, 173 vecchi esempi e i renderer Crinis/G4 non cambiano**.
- Creati test Application `CincinnusC5CatalogTests` e rendering `CincinnusC5RenderingTests`, aggiornati gli assert del catalogo storico e il gate rapido `validate.ps1`.
- Controlli statici: sintassi dei 206 JSON, confronto dei file storici, catalogo/schema e manifest (vedi rapporto C5). **Build e test C3+C4+C5 non eseguiti**: SDK .NET assente. Nessun PNG ancora giudicato valido artisticamente.

[Specifiche, comandi e gate](CINCINNUS_C5_STUDIO_CLI_JSON_2026-11-17.md) | [QA statico](validation/CINCINNUS_C5_STATIC_QA.md).

---

## 17/11/2026 — M4E.3-Cincinnus-C4 — algoritmo Adaptive main/filler/patch

- Nuovi sorgenti Algorithms: `CincinnusAdaptiveOptions`, `CincinnusCoverageGrid`, `CincinnusAdaptiveLayout`, `CincinnusAdaptivePlacer` (sottostream RNG e RK4 C1, seconda copertura post-trim e patch).
- Nuovi 7 test in `CincinnusAdaptivePlacementTests.cs`; **predisposti, non ancora eseguiti**.
- Preview C# aggiornata: argomento facoltativo `simple|adaptive` per PNG comparabili senza registrare Cincinnus nel catalogo.
- Nessuna modifica a Crinis, Ocellus/Rete, 29 preset embedded, 173 ricette e altri sorgenti storici; invariato il catalogo 29 generatori.
- **Conferme Windows:** C1 e C2; **da verificare Windows:** C3 e C4. Nessun giudizio estetico ancora disponibile.
- [Dettagli](CINCINNUS_C4_ADAPTIVE_2026-11-17.md) / [QA statico](validation/CINCINNUS_C4_STATIC_QA.md).

---

## 17/11/2026 — M4E.3-Cincinnus-C3 — pennellate volumetriche e PNG di prova

- **Gate Windows C2:** l'utente conferma che **compilazione e test C2 passano**; C1 già confermata positiva. Conteggio test non disponibile.
- **Algorithms:** aggiunti `CincinnusStrokeOptions`, `CincinnusBrushStroke`/`CincinnusStrokePlan`, `CincinnusStrokePlanner`: segmenti sovrapposti, ordini di posa per curvatura, spine a normale e larghezza variabile, profili di testata, essiccazione e limiti espliciti.
- **C2:** esteso il contratto `CincinnusStreamline.Separation` come sola property di lettura; posizionamento e test C2 invariati.
- **Rendering.Skia:** aggiunti `CincinnusPaintOptions` e `CincinnusBristlePainter`, con otto colori crimson personalizzabili, corpo/ombre, fibre, load, gap asciutti e luce trasversale. Modulo autonomo che accetta `SKCanvas`; nessuna modifica a Crinis/Ocellus/Rete.
- **Tool C#:** `tools/GenerativeArt.CincinnusPreview` produce PNG di prova 256–2048 px senza registrare Cincinnus nel catalogo. Va compilato separatamente dalla solution.
- **Test:** nuovi `CincinnusStrokePlannerTests` e `CincinnusBristlePainterTests`; C3 non ancora eseguita in .NET, verifiche statiche/documentali e zip SHA-256 distinte dal gate Windows.
- **Invarianti:** catalogo 29 generatori, 29 preset embedded e 173 ricette invariati; Crinis R10 e G4-R2 preservati. Punti aperti: resa artistica, OKLab, Adaptive C4, Studio/CLI C5.
- **Documenti:** [C3](CINCINNUS_C3_PENNELLO_VOLUMETRICO_2026-11-17.md), [QA statico](validation/CINCINNUS_C3_STATIC_QA.md), roadmap e handoff aggiornati.

---

## 17/11/2026 — M4E.3-Cincinnus-C2 — posizionamento Simple delle streamline

- **Conferma Windows C1:** l’utente comunica «Compila e test ok» per il kernel geometrico C1 già consegnato; numero dei test non disponibile.
- **Algorithms:** nuovi `CincinnusSimpleOptions`, `CincinnusSimpleLayout`/`CincinnusStreamline` e `CincinnusSimplePlacer`, basati sulla distribuzione di `flow2/Streamlines.kt` di Gȧrt: bidirezionale RK4, griglia spaziale, dSep/dTest, esclusione dei ritorni non locali, semi laterali, griglia fallback, scarto/rollback di linee corte, clearance e taper. Limiti a punti, linee e semi con flag di distribuzione parziale.
- **Test:** aggiunta `CincinnusSimplePlacementTests` per determinismo, campi costanti/circolari, canvas rettangolari, rollback, limiti, collezioni immutabili, cancellazione e validazione parametri.
- **Invarianti:** nessuna modifica ai sorgenti precedenti C1, ai renderer Crinis/Ocellus/Rete, al catalogo (29), ai preset (29) o alle ricette (173). Nessun rendering Cincinnus nel prodotto.
- **QA:** controllo statico e manifest del pacchetto; **C2 non ancora compilata/testata con dotnet in questo ambiente**. Scheda [C2](CINCINNUS_C2_STREAMLINES_SIMPLE_2026-11-17.md), [QA statico C2](validation/CINCINNUS_C2_STATIC_QA.md). Prossimo: C3.

---

## 17/11/2026 — M4E.3-Cincinnus-C1: prima implementazione geometrica C#

- **Richiesta utente:** realizzare il primo kernel geometrico C# per l'opera **Cincinnus** studiata nel dossier H3, senza alterare Crinis o G4.
- **Algorithms:** aggiunto `CincinnusFlowField` (vortici tangenziali esterni al canvas selezionati per massima minima distanza, campo composito conforme alla legge Gȧrt, seed SplitMix64 riproducibile); `CincinnusTraceOptions` (passo/lunghezza/margine), `CincinnusPathGeometry` (RK4 avanti e indietro tramite FlowTracer, chiusura loop, raggi di curvatura e budget/cancellazione).
- **Test:** nuovo file `CincinnusGeometryTests.cs` per condizioni geometriche e numeriche, compatibilità con `VectorFields.Vortex`, clipping, determinismo, input invalidi, cancellazione. Nessun cambio ai test preesistenti.
- **Compatibilità:** nessun renderer, preset, ricetta JSON, modificatore UI o CLI aggiunto; 29 generatori, 29 preset, 173 ricette invariati. Crinis R10 e Ocellus/Rete G4-R2 byte-identici alla H3. Sorgenti originali Kotlin non redistribuiti.
- **QA:** controlli statici/manifest/archivio completati; **build e test C1 non eseguiti** (SDK .NET assente nell'ambiente di preparazione). Baseline G4-R2 confermata dall'utente nel passaggio precedente. [Scheda completa](CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md).
- **Prossimo:** verificare gate Windows, poi C2 posizionatore `Simple` e diagnostic geometrico; C3–C5 seguono la roadmap, senza anticipare un giudizio artistico.

---

## 17/11/2026 — G4-R2-H2: ricezione e analisi delle fonti Gȧrt originali

**17/11/2026 — H3 (solo documentazione):** studio sorgente Cincinnus Gȧrt (`Cincinnus.kt` e `CincinnusSimple.kt`, `Bristles`, `VectorField`, `Streamlines`), confronto con Crinis R10 C# e proposta di porting incrementale C1–C5. Creato [STUDIO_CINCINNUS_GART_2026-11-17.md](STUDIO_CINCINNUS_GART_2026-11-17.md); aggiornati riferimenti in README/CURRENT_STATE/ROADMAP/HANDOFF/INDEX/NUOVA_CHAT/decisioni. **Sorgenti C#, suite test, preset, 173 JSON e configurazioni immutati**; non è stato eseguito un nuovo test .NET. Baseline Windows validata M4E.3-G4-R2.


- Analizzati lo ZIP originale `gart-main-pulito(2).zip` e la galleria ufficiale `https://github.com/igr/gart`; catalogati i link a 28 opere selezionate, verificati i sei riferimenti visuali Ocellus/Rete/Nimbus/Pebble2/Accretion/Crinis e i loro sorgenti Kotlin.
- Aggiunto [`GART_RIFERIMENTI_ORIGINALI_2026-11-17.md`](GART_RIFERIMENTI_ORIGINALI_2026-11-17.md): provenienza, gallery index, differenze algoritmiche e criteri A/B con la nostra G4-R2.
- **Nessun sorgente C#, test, preset, ricetta o configurazione modificato**. Baseline Windows resta **M4E.3-G4-R2**, già validata dall'utente; H2 è solo documentazione. Il giudizio estetico G4-R2 resta aperto.
- Manifest SHA-256 rigenerato per tutti i file del pacchetto; Gȧrt originale e immagini non redistribuiti nello ZIP.

---

## 17/11/2026 — G4-R2-H1: consolidamento documentale e passaggio nuova chat

- **Conferma utente:** la **G4-R2 compila e supera tutti i test su Windows**. Il conteggio preciso non è stato comunicato. Rimane aperto il confronto visivo di Ocellus/Rete.
- Nessun sorgente di produzione, test, ricetta, preset o configurazione modificato. La baseline **codice resta G4-R2**, H1 indica soltanto documentazione/handoff.
- Rifatti README, INDEX, CURRENT_STATE, HANDOFF e NUOVA_CHAT, con vecchi testi conservati come file storici nello stesso `docs/`.
- Creati `DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md` e `AUDIT_HANDOFF_G4_R2.md`: richieste confermate, roadmap aggiornata, QA e capacità reali, rimozione di riferimenti documentali non presenti.
- Aggiornati ROADMAP, QUALITY_GATES, TESTING, guide G4 e registri; ricalcolato manifest SHA-256 e pacchetto completo.

---

## 17/11/2026 — M4E.3-G4-R2: Ocellus interstizi e rilievo; Rete nodi e pellicole

**Baseline certificata dall'utente:** G4-R1.2 compila e tutti i test passano su Windows. Il blocco G4-R2 prosegue da quella versione, mantenendo integralmente Crinis R10 e gli hotfix R1.1/R1.2.

**R2 implementata nei sorgenti:** Ocellus aggiunge `pore-strength`/`pore-radius` e `rim-light`/`rim-width`; Rete aggiunge `junction-strength`/`junction-radius`, `node-glow` e `membrane-opacity`. I nuovi controlli sono opzionali e neutri per vecchie ricette JSON v1; i **preset iniziali Studio** dei due generatori sono invece espressamente aggiornati per mostrare gli effetti.

**Catalogo:** 29 generatori, 29 preset incorporati, 173 ricette JSON (6 nuove in `examples/recipes/m4e3-g4-r2/`); 17 preset personali e tutti gli altri generatori invariati. Nuovi test Algorithms/Application/Rendering, note e comparazioni nella [scheda G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md).

**Gate aperto:** la R2 richiede ancora build Release e test .NET su Windows (`clear && dotnet clean && .\tools\validate.ps1`) e verifica visiva A/B. Dopo il gate decidere se proseguire G4-R3 o passare a G5. Compositore B, workflow artista D, animazioni evolute più avanti.

---

## 2026-11-17 — M4E.3-G4-R1.2 — Hotfix compilazione CS0136 in Rete

- **Segnalazione Windows R1.1:** build `GenerativeArt.Rendering.Skia` fallita con CS0136 in `GartCellularRenderer.cs` (riga 152): `double ink` nel ciclo interno e `uint ink` nella destrutturazione palette dello stesso metodo. Per le regole degli ambiti locali C# non possono condividere lo stesso identificatore.
- **Correzione:** rinominati rispettivamente `wallCoverage` e `wallArgb`, aggiornandone tutti gli utilizzi. **Operazione di solo refactoring**: nessuna formula, colore, composizione, budget, preset, ricetta o schema JSON modificati. Il fix CS0117 di R1.1 rimane incluso integralmente.
- **QA:** il log utente segnala compilazione riuscita di `GenerativeArt.Algorithms.Tests` e `GenerativeArt.Application.Tests`; il resto della build/suite è ancora bloccato fino alla verifica Windows di R1.2. Test rendering Rete già presenti per determinismo, palette, copertura artistica, JSON ed export.
- **Gate:** `clear && dotnet clean && .\tools\validate.ps1` in PowerShell 7. Obiettivo: build completa e tutte le suite xUnit superate, poi confronto artistico Full HD prima di G4-R2. Numeri invariati: **29 generatori, 29 preset incorporati, 167 ricette JSON**.

---

## 2026-11-17 — M4E.3-G4-R1.1 — Hotfix CS0117 distanza euclidea (.NET 10)

- **Segnalazione Windows G4-R1:** build fallita con quattro CS0117 in `OcellusField` e `ReteFoamField`: `System.Math.Hypot` non è disponibile in .NET 10. La build si è interrotta prima delle suite dipendenti.
- **Fix completo:** introdotto `GeometryDistance.Hypotenuse(dx,dy)`, numericamente stabile (scaling prima di `Sqrt`), e sostituiti **tutti e 6** gli usi in produzione (4 Algorithms, 2 Rendering.Skia). L'unico uso nel test di geometria è stato sostituito con un calcolo `Sqrt(dx²+dy²)` indipendente, per mantenere valido il confronto con l'oracolo brute-force.
- **Regression tests:** nuovo `GeometryDistanceTests` (triangoli pitagorici, simmetria, numeri estremi, valori non finiti). Nessun cambiamento a cataloghi, preset, ricette, default, schema JSON, renderer artistici o limiti di memoria.
- **Stato:** tutti i riferimenti `Math.Hypot` nel codice C# sono stati eliminati; validazione statica/archivio eseguita, **build e test Release G4-R1.1 ancora da confermare su Windows**. Il totale resta **29 generatori, 167 ricette JSON**.

---

## 2026-11-17 — M4E.3-G4-R1: due generatori cellulari Ocellus/Rete

- Implementati `ocellus-eyespots` (13 livelli ad anelli con smooth-min) e `rete-foam` (impacchettamento e campi di distanza delle superfici, drenaggio, film e piani sfocati), ispirati ai sorgenti Gȧrt originali, reimplementati in C#.
- Aggiunte 2 famiglie di parametri e 9 palette complessive (5 Ocellus, 4 Rete), ruoli colore Auto, 2 preset incorporati FullHD e **8 nuove ricette** (`examples/recipes/m4e3-g4`). Catalogo da 27 a **29 generatori**, esempi da 159 a **167**.
- Aggiunti test di geometria, catalogo, rendering, regressioni di ordinamento, preview nativa identica all’export; limitazioni computazionali verificate prima di renderizzare. Documentazione, roadmap, handoff, indice e manifest aggiornati.
- Crinis R10 conservata integralmente; cambio priorità chiesto dall’utente. G4-R1 ancora da compilare/testare su Windows; il superamento della suite non è stato attestato in questa consegna.

---

## 2026-11-17 — M4E.3-G3-R10 — Crinis profondità interna dei fasci

- **Baseline confermata dall'utente:** R9 compila, test superati; profondità tra fasci apprezzata, ma fibre piccole e fascia troppo piatta al suo interno.
- Nuovi controlli opzionali `intra-bundle-depth`, `featured-fibers`, `cross-section-light`, `internal-occlusion`, neutrali su JSON preesistenti. Studio li attiva per nuove opere e propone `hair-width=3.2`.
- Renderer: tre passaggi di disegno front/mid/back con semi indipendenti dall'ordine, maggiore corpo di una minoranza di filamenti, profilo di luce trasversale e ombre di contatto interne; `body-opacity=0` ancora senza sagoma opaca.
- QA: nuovi test Application e Rendering per catalogo, applicabilità, pixel storici, seme, dimensioni, calibro, occlusione interna e cancellazione.
- Esempi: originale utente R9 byte-identico + due nuove varianti R10; **159 ricette**, **27 generatori**. Aggiornati README, roadmap, guida tecnica, stato, handoff, nuova chat, indice e manifesto.
- **Stato R10:** verifiche statiche/JSON completate; compilazione e test .NET da eseguire su Windows prima di considerarla validata.

---

## 2026-11-17 — M4E.3-G3-R9 — Crinis volume del fascio e illuminazione collettiva

- **Priorità:** proseguimento di Crinis, non avvio G4. Obiettivo: dimensione plastica della pennellata senza trasformarla in superficie piena.
- **Tre controlli micro opzionali e indipendenti:** `bundle-compression` (-0.35/0.70), `bundle-volume` (0/1) e `bundle-lighting` (0/1); nel catalogo il default è neutro (0), nel preset di nuovi lavori Studio sono rispettivamente 0.28 / 0.68 / 0.82.
- **Preset nuovo lavoro più materico:** `hair-width` Studio portato da 0.67 a 1.65 (il valore catalogo/JSON storico resta 1).
- **Geometria:** addensamento/espansione trasversale monotoni; modulazione comune della sezione lungo il percorso; diametro dei filamenti limitato dal passo locale, per conservare la tessitura.
- **Colore e rilievo:** il lighting dei gruppi coordina luce/ombra del lato e della cresta attraverso le fibre; rimangono le ombre di contatto locali di R8 e `body-opacity=0` senza sagoma.
- **Ottimizzazione:** la modulazione viene calcolata una volta per campione di curva e riusata da tutti i filamenti del fascio.
- **Bugfix retrospettivo:** `hair-width` massimo 8 (era 5); riallinea catalogo e ricette R7/R8 già esistenti con valori oltre 5.
- **QA:** test per monotonicità della geometria, profilo del volume, determinismo, pixel diversi, compatibilità di ricette v1, condizioni Studio; registrate tre nuove ricette R9. Totale **156 ricette**, **27 generatori**; documentazione, indice, roadmap e handoff aggiornati.
- **Verifica:** nessuna esecuzione .NET nel container (SDK non presente); richiesta validazione Windows con `clear && dotnet clean && .\tools\validate.ps1`.

---

## 2026-11-17 — M4E.3-G3-R8 — Crinis occlusione locale e ombre di contatto

- **Riscontro utente R7:** continuare su Crinis; mantenere sempre aggiornata la documentazione.
- `CrinisMicroRenderer`: introdotta una **ombra di contatto multi-pass** per i singoli filamenti (outer/mid/core) in modo da far leggere meglio i fasci in primo piano sopra quelli dietro, senza reintrodurre la sagoma piena del fascio quando `body-opacity=0`.
- Rafforzata la lettura dei piani: i livelli arretrati ora possono risultare leggermente piu' soffici e meno opachi (`rear-softness` / focus), cosi' davanti e dietro si separano meglio.
- QA/catalogo: i test Application includono ora anche le cartelle esempio `m4e3-g3-r7` e `m4e3-g3-r8`.
- Aggiunte 2 nuove ricette in `examples/recipes/m4e3-g3-r8/` per studiare sovrapposizione e ombra di contatto. Totale **153 esempi JSON**.

---

## 2026-11-17 — M4E.3-G3-R7 — Crinis volume filamenti e rilievo cilindrico

- **Riscontro utente R6:** compilazione e test OK; feedback visivo: profondità migliorata, ma fibre ancora troppo sottili e piatte.
- `CrinisMicroRenderer`: aumentata la massa percepita del filamento con volume radiale lungo la corsa, rigonfiamento locale a bassa frequenza, spessori nominali piu' presenti e minima sezione non eccessivamente capillare.
- Aggiunto un secondo pass di modellazione sul singolo filo: corpo centrale, bordo in ombra e bordo in luce, per suggerire una sezione piu' cilindrica e meno piatta. Depositi/glint adeguati allo spessore medio del run.
- Nuovi test Rendering: `hair-width` entra nel gate dei controlli micro e viene verificato anche in una ricetta densa reale ad alta densita'.
- Aggiunte 2 ricette di studio in `examples/recipes/m4e3-g3-r7/` (ricetta utente importata + variante bold). Totale **151 esempi JSON**. Guida operativa Windows invariata: `clear && dotnet clean && .\tools\validate.ps1`.

---

## 2026-11-17 — M4E.3-G3-R5 — Crinis fibre separate e fix gruppo Finitura

- Report Windows R4: `CatalogTests.AllArtworksExposeOptionalFinishingControlsWithNeutralDefaults` fallisce per `ink-flecks` erroneamente in gruppo `Finitura`. Spostato in `Microfibre`, senza alterare i quattro controlli di finitura condivisi.
- Microfibre: nuovo diametro di filo limitato morbido dal passo reale per evitare l'aggregazione in nastri opachi ad alta densità; nuovo parametro `fiber-coverage` (default 0.8).
- Pigmento: nuovo `pigment-contrast` (default 0.6), con tonalità variabili fra fili e depositi discontinui lungo la fibra.
- Conservato senza variazioni il JSON fornito dall'utente, aggiunto confronto A/B Full HD e una ricetta cremisi 2000×2000 (149 esempi totali).
- Nuovi test sul catalogo, sui due controlli, su distribuzione dei diametri e su ricette storiche. Windows R5 da verificare. Guida: `M4E3_G3_R5_CRINIS_FIBRE_SEPARATE.md`.

---

## 2026-11-17 — M4E.3-G3-R4 — Fix 32 test Crinis / build Release coerente

- **Riscontro Windows R3:** build `Release` riuscita; **1056/1088** test superati, 32 falliti. Gli errori condividono un unico sintomo: `detail-style` (e altri controlli R3) assenti dal catalogo Application caricato dai test. Il file `GartBrushArtworkDescriptors.cs` presente nello ZIP R3 dichiara gia' tutti i parametri; i risultati sono compatibili con un assembly Application Release precedente rimasto in uso. Non e' stato possibile dimostrare la causa dalla sola traccia.
- **Fix di validazione:** `tools/validate.ps1` pulisce ora esplicitamente `Release` (`dotnet clean GenerativeArt.sln -c Release`) prima di restore/build/test, prevenendo la mescolanza Debug/Release dovuta al comando `dotnet clean` senza `-c`.
- **Gate precoce:** nuova classe `CrinisReleaseCatalogContractTests` verifica che il catalogo realmente compilato contenga tutti i 10 controlli aggiunti, che siano opzionali per i vecchi JSON e che il preset Studio sia in modalita' micro senza base. Il test mirato precede la suite completa.
- **Resilienza:** il renderer Crinis tratta l'assenza di `detail-style` in una ricetta storica come `classic` anziche' indicizzare senza controllo e produrre `KeyNotFoundException`. Il formato JSON v1 e la validazione rigorosa degli ID sconosciuti restano invariati.
- **Installazione:** estrarre lo ZIP in una cartella nuova e vuota, oppure eliminare le cartelle `bin`/`obj` dall'albero precedente. Non mescolare i sorgenti con output compilati delle revisioni precedenti.
- **Nessun cambiamento voluto ai PNG quando il catalogo R3 e' coerente**. 27 generatori, 27 preset, 146 ricette; 17 preset personali e le 6 ricette Cincinnus originali della R3 invariati. Test Windows R4 ancora da confermare.

---

## 2026-11-17 — M4E.3-G3-R3 — Crinis microfibre ispirato a Cincinnus

- Corretto il difetto visivo: `body-opacity=0` nasconde **anche le due ombre poligonali** che rimanevano sotto alle fibre nella G3-R2. Nessuna ombra di fascia quando il corpo è invisibile.
- Introdotto un renderer `detail-style=micro` indipendente dal classico: fibre molto più dense, microtrame, rastremazione e rilievo locali, copertura della superficie, più centri di curvatura e granelli di inchiostro.
- Aggiunta palette `crimson`; ampliati gli intervalli creativi e il budget controllato a 12M campioni della modalità micro. Preset Studio Crinis aggiornato, 17 preset personali e altri 26 generatori preservati.
- Aggiunte **6 ricette** (2 quadrate 2000×2000), da 140 a **146 esempi**. Corretti i 4 JSON R2 rimuovendo il campo `description` incompatibile con il parser v1.
- Nuovi test Rendering/Application, documentazione [M4E3_G3_R3_CRINIS_MICROFIBRE.md](M4E3_G3_R3_CRINIS_MICROFIBRE.md). Ultima baseline Windows verificata dall’utente: G3-R2; G3-R3 da verificare.

---

## M4E.3-G3-R2 — Crinis poetico e tessile
- Aggiunti i parametri `body-opacity`, `fiber-relief` e `fiber-fray` al generatore `crinis-bristles`.
- La base del fascio può essere resa invisibile; le fibre hanno ora maggiore rilievo con ombre/luci dedicate.
- Nuova resa tessile con bordi più soffici e sfrangiati.
- Aggiornato il preset incorporato e le quattro ricette Crinis esistenti; aggiunte quattro nuove ricette di studio.

# M4E.3-G3-R1 — Fascia/Eclectic + correzione catalogo (17/11/2026)

- Ricevuto esito G2-R2: **1040/1041 test**; unica regressione `CatalogTests.CatalogHasStableSortedIdsAndLoopCapability` dovuta a un ordine alfabetico atteso invertito (`crinis` / `cotton`). Corretta **solo la lista di attesa**, in entrambe le coppie invertite (`cotton`/`crinis`, `spaced`/`spiral`); non l'ordinamento del prodotto.
- A3-G3: due generatori `fascia-ribbons` e `eclectic-trails` (campi vettoriali, nastri retinati, crescita con collisioni), otto nuove ricette, due preset embedded. Totale **27 algoritmi / 136 ricette**. Tutti i preset e le ricette precedenti preservati.
- Test nuovi per geometria, serializzazione, cambi di parametri, seed, preview/export, fine-2x, finiture, limiti e cancellazione. Guida `M4E3_G3_FASCIA_ECLECTIC.md`.
- **G3-R1 da validare su Windows**. Fase successiva G4 Ocellus/Rete; A1 e A4/A5 restano aperte; B/D dopo e animazioni rinviate.

---

# M4E.3-G2-R2 — 17/11/2026 — Fix xUnit2020

- Il gate Windows della G2-R1 si arrestava in compilazione in `tests/GenerativeArt.Application.Tests/GartBrushCatalogTests.cs:42`: l’analizzatore xUnit2020 vieta `Assert.True(false, message)` e richiede `Assert.Fail(message)`.
- Sostituita soltanto quella istruzione nel test `EightGartBrushStudiesRemainValidJsonV1`. Controllata l’assenza di ulteriori `Assert.True(false, ...)` / `Assert.False(true, ...)` nei sorgenti C#.
- Nessuna modifica ai progetti di produzione, ai 25 generatori, ai 25 preset e alle 128 ricette. G2-R2 attende nuova compilazione ed esecuzione della suite completa su Windows; l’ultima baseline confermata integralmente rimane M4E.2-R5.

---

# M4E.3-G2-R1 — 17/11/2026

**A3-G2 implementata:** nuovi generatori `crinis-bristles` (curve portanti, setole, ombre, secchezza) e `spiral-flamebrush` (orbite e timbri puntinati spiraliformi), 8 nuove ricette Full HD. Totale 25 generatori e 128 JSON. Kernel Algorithms separato da Skia; 2 preset nuovi e tutte le risorse precedenti conservate. Budget preventivi 3M campioni setole / 120k particelle. Test Algorithms/Application/Rendering.Skia aggiunti. Guida [`M4E3_G2_PENNELLI_CRINIS_FLAMEBRUSH.md`](M4E3_G2_PENNELLI_CRINIS_FLAMEBRUSH.md). Baseline Windows confermata M4E.2-R5; G1/G2 non ancora validate.

---

## 2026-11-17 — M4E.3-G1-R1 — Geometrie modulari (Alien Letters, Cotton Circles, Rigor)

- Primo incremento del piano A3-G1 ricavato dallo studio diretto dei sorgenti Gȧrt. Tre generatori C# parametrici, con ricette v1 e seed riproducibili; nessuna dipendenza runtime da Kotlin.
- Creati `ModularTileGrammar` (Algorithms), `ModularArtworkDescriptors` (Application), `GartModularRenderer` (Skia), campioni di palette Auto e tre preset incorporati.
- Nove nuovi studi Full HD in `examples/recipes/m4e3-g1/`, da 111 a 120; catalogo da 20 a 23. I 17 preset personali originali e i 111 esempi precedenti conservano i loro byte.
- Test Algorithms, Application, Rendering.Skia per griglie, determinismo, variazione parametri, JSON, PNG, fine-2x, finiture e cancellazione; test inventario e file fixture aggiornati.
- M4E.2-R5: **compilazione e test confermati dall'utente**. M4E.3-G1-R1: test Windows **non ancora eseguiti**. Guida [`M4E3_G1_GEOMETRIE_MODULARI.md`](M4E3_G1_GEOMETRIE_MODULARI.md).

## 2026-11-17 — Studio comparativo Gȧrt su baseline M4E.2-R5 (sola documentazione)

- Confermata dall’utente build + test M4E.2-R5 superati.
- Analizzato direttamente l’archivio Gȧrt: struttura `arts`, utility `gart`, sorgenti favorite e gap rispetto ai 20 generatori C#.
- Aggiunto `docs/STUDIO_GART_PORTING_M4E2.md`: matrice tecnica, priorità Crinis/Fascia/Ocellus/Rete/Accretion, sottoroadmap A3-G1…G6, requisiti test, budget 8K, licenza BSD-2-Clause.
- Aggiornate roadmap, stato, handoff e indice. **Zero modifiche ai sorgenti, ai preset, alle ricette e ai test.**

---

## 2026-11-17 — M4E.2-R5 — Fix CS0618 SkiaSharp

- Correzione puntuale in `ArtisticScanlineFinisher.cs`: `DrawImage` usa ora `SKRect` e `SKSamplingOptions(SKFilterMode.Nearest)` invece dell'overload deprecato.
- Nuovo test PNG per verificare la copia pixel-per-pixel senza ricampionamento; nessun algoritmo, preset o ricetta modificato.
- **R4: build Windows fallita con CS0618** secondo il log utente. **R5: compilazione e test ancora da confermare**. Baseline verificata: M4E.2-R1.
- Guida: [`M4E2_R5_FIX_DRAWIMAGE_SKIA.md`](M4E2_R5_FIX_DRAWIMAGE_SKIA.md).

## 2026-11-17 — M4E.2-R4 — Post-processing 8K a righe

- A4: 8K UHD standard + grana/Bayer/carta/pigmento/assorbimento nei quattro generatori geometrici già abilitati in R2. Finiture applicate sulle righe; evita un PixelBuffer completo aggiuntivo oltre 16.777.216 pixel.
- Fattorizzati kernel per riga da `MaterialPostProcessor` e `ArtisticPostProcessor`; il percorso fino a 4K mantiene la codifica storica.
- +3 ricette 8K nuove, 111 totali. Nessuna modifica ai preset personali né alle 108 ricette precedenti. Test di equivalenza, alpha, budget 8K, parsing e cancellazione aggiunti.
- Baseline confermata dall'utente: M4E.2-R1 (build e test OK). **R2/R3/R4 da verificare su Windows**; nuovo rendering 8K reale non ancora eseguito.

# Registro modifiche — GenerativeArt Studio

## 2026-11-17 — M4E.2-R3 — Fillotassi, reazione–diffusione e pulsanti uniformi

- **Fase A3:** 19° e 20° generatore: fillotassi geometrica con distribuzioni a spirale e reazione–diffusione Gray–Scott con campionamento bilineare. Entrambi con parametri completi, colori indipendenti, seed, qualità e finiture; renderer Skia/CLI/Studio condiviso.
- **Otto** nuove ricette Full HD in `examples/recipes/m4e2-r3/`, da 100 a **108**; aggiunti due preset embedded, senza cambiare i 17 originali dell’utente e i precedenti 100 JSON.
- **Studio:** pulsanti del catalogo e apertura delle ricette recenti estesi uniformemente alla larghezza del menu, titoli con ellissi, azione `×` separata.
- **Budget:** reazione–diffusione al massimo 350.000 celle e 75M aggiornamenti; i nuovi renderer restano al profilo standard ≤4K, non ampliamento 8K.
- Nuovi test nelle tre suite; documentazione e manifesto aggiornati. **Da validare su Windows**; baseline confermata M4E.1-R3. [Guida](M4E2_R3_NATURA_E_NAVIGAZIONE.md).


## 2026-11-17 — M4E.2-R2 — Primo budget 8K UHD sui generatori geometrici

- Nuova `RasterCanvasBudgetPolicy`: supporto statico 7680×4320/4320×7680, massimo 8192 per lato e 33.177.600 pixel, limitato ai generatori `guilloche`, `spaced-streamlines`, `voronoi-mosaic`, `poisson-stippling`. Nessuna superficie 8K per i renderer che necessitano di buffer raster aggiuntivi.
- Supersampling e finiture mantengono limiti separati (16.777.216 sottopixel/pixel): nessuna promessa di `fine-2x` o carta/grana a 8K. Mantenuti i limiti delle animazioni.
- CLI `recipe-new` accetta lati fino a 8192 (validazione completa rimandata alla generazione); preview Studio continua a usare i pixel finali.
- Quattro esempi 8K, 100 ricette totali, 18 generatori, 17 preset utente non toccati. Nuovi test validazione 8K e fixture JSON. Guida `M4E2_R2_8K_GEOMETRICI.md`.
- Revisione non ancora validata Windows. Baseline utente M4E.1-R3; R4, R5 e M4E.2-R1 ancora da confermare.

## 2026-11-17 — M4E.2-R1 — Nuovo generatore Interferenze d'onda / Moiré

- **Fase A3**: aggiunto un nuovo generatore indipendente `wave-interference` (Algorithms + Rendering.Skia), basato su 2–12 sorgenti radiali, seed, disposizione, frequenza, deformazione Simplex, attenuazione, tre modalità e tre palette.
- Il catalogo passa a **18 generatori**; quattro ruoli colore Auto/HEX, finiture e qualità `fine-2x` sono riutilizzati senza cambiare il JSON v1. Nuovo preset incorporato separato; **17 preset personali e 91 ricette precedenti invariati**.
- **Cinque** studi Full HD in `examples/recipes/m4e2-r1/`: rosette, caustiche rame, nastri, scatter cromatico e Moiré su carta; totale **96 ricette**.
- Budget lavoro esplicito: fino a 150 milioni di contributi radiali (pixel nativi × sorgenti), prima dell'allocazione; resta il limite raster generale a 4096×4096, non ancora 8K.
- Test Algorithms per i campi, Rendering.Skia per PNG/seed/parametri/fine2x/cancellazione/budget, Application per i JSON e il nuovo preset. Documentazione [`M4E2_R1_INTERFERENZE_ONDA.md`](M4E2_R1_INTERFERENZE_ONDA.md).
- **Stato:** modifiche implementate, test Windows da confermare. Baseline utente **M4E.1-R3**; R4, R5 e questa revisione ancora senza feedback Windows.

## 2026-11-17 — M4E.1-R5 — Documenti ricetta e dieci studi creativi

- Avviata la separazione delle operazioni di import/salvataggio JSON da Avalonia con `StudioRecipeDocuments` (Application); import bounded 64 KiB, validazione prima dell'aggiornamento della UI, stream del chiamante non chiusi, file locali atomici; provider virtuali non atomici.
- `StudioWindow.FileOperations.cs` usa il servizio, non replica più buffering e serializzazione. UI e motore di rendering invariati.
- Dieci nuove ricette Full HD indipendenti, derivate da parametri già supportati; da 81 a 91 esempi, nessuna modifica ai 17 preset incorporati.
- Aggiunti 7 test del servizio e un test fixture per le dieci varianti. **Baseline Windows M4E.1-R3 validata**; R4 e R5 da testare. Guida `docs/M4E1_R5_DOCUMENTI_RICETTE_STUDI.md`.

## 2026-11-17 — M4E.1-R4 — UI parametri condizionali e refactoring

- **R3 validata dall'utente**: build e test passati, anteprime corrette, output identico all'anteprima. La regressione `spaced-streamlines/frequency` è risolta.
- Nuova policy `StudioParameterApplicability` in Application, testabile senza Avalonia: disabilitazione informativa di valori inapplicabili per vortex/curvature, grana, Bayer, carta, maschera. Non modifica JSON o output.
- Estratti in `StudioWindow.ParameterControls.cs` gli editor tipizzati; `StudioWindow.RecipeEditor.cs` gestisce selezione e ricetta; aggiornamento UI e reset di input temporaneamente invalidi in `StudioWindow.ParameterApplicability.cs`.
- Nuovi test per combinazioni condizionali, ripristino dell'applicabilità, serializzazione e preset immutati. Rendering, 17 preset e 81 ricette non modificati. **R4 da validare su Windows**. Guida: `M4E1_R4_PARAMETRI_CONDIZIONALI.md`.

## 2026-11-17 — M4E.1-R3 — Regressione frequency/vortex nei test Streamlines

- **Riscontro Windows M4E.1-R2:** compilazione riuscita; **879/880 test superati**. Unico fallimento: `ArtworkEvolutionTests.EveryArtisticControlChangesTheRaster(id: "spaced-streamlines", key: "frequency", value: "0.012")`.
- Causa: dopo l'integrazione dei 17 preset dell'utente, `spaced-streamlines` parte con `field=vortex`. Il campo vortice geometrico non usa `frequency`; il test, ereditando i preset Studio, pretendeva erroneamente un cambiamento del PNG. Il renderer non ignorava `frequency` nel campo `curl`.
- Corretto il test di efficacia del parametro configurando **`field=curl` per questa sola combinazione**. Aggiunto `SpacedStreamlinesVortexIsIndependentOfFrequency` per fissare esplicitamente la semantica del vortice.
- Nessuna modifica al motore, ai 17 preset embedded, alle 81 ricette, alla GUI, al formato JSON o agli hash PNG attesi. Da valutare in Fase A1 lo stato disabilitato/informativo del controllo `frequency` quando il campo selezionato non lo utilizza.
- **R3:** archivio preparato, test Windows da eseguire; non attribuire a R3 il risultato 879/880 della R2. Guida `docs/M4E1_R3_FIX_TEST_STREAMLINES.md`.

## 2026-11-17 — M4E.1-R2 — Anteprima a risoluzione reale e coordinatore render

- Rimosso dallo Studio il doppio raster dell'anteprima: preview sempre con `width`×`height` dell'output, seed, frame, qualità, parametri e pipeline invariati; `Adatta` e `100%` sono solo operazioni visive.
- Rimossi i controlli «Raster max» e «Raster = output»; mantenute le dimensioni del riquadro in DIP, il pan e lo zoom; stato esplicita i pixel realmente calcolati. Nessuna riduzione silenziosa davanti a limiti del renderer.
- Introdotto `StudioRenderCoordinator` nell'Application: unica serializzazione preview/export, cancellazione in attesa, `Task.Run` per il rendering sincrono, output locale atomico su stream senza PNG completo in RAM. A1 ancora parziale: controller e lifecycle UI da isolare.
- Nuovi test: full-res 1920×1080, 2500×2000, 4096×4096 con ricetta invariata, code rendering serializzate, cancellazione, sicurezza del file, PNG Skia identico tra anteprima ed export. Non modificati renderer, catalogo, ricette o 17 JSON embedded.
- Gate Windows di questa revisione **aperto**; ultima baseline documentata M4D.4-R1 con 859/859 test. Guida `docs/M4E1_R2_PREVIEW_NATIVA_COORDINATORE.md`.

## 2026-11-17 — M4E.1-R1 — Avvio refactoring Studio e Fase D

- Primo incremento **A1**: `StudioWindow.cs` da 1.103 a 189 righe; sette partial con layout/tema, catalogo/preferenze, editor, preview, playback, file/export e stato, senza cambiare la struttura UI.
- Creato `StudioRecipeFormParser` in Application; `BuildRecipe` delega al parser senza Avalonia. Aggiunte regressioni per seed (anche Domain Warp), dimensioni, animazione e valori invalidi.
- **Fase D** inserita dopo B: varianti/seed explorer, confronto A/B, galleria locale, contact sheet, preset riutilizzabili, SVG selettivo più avanti; animazioni rinviate. A1 non ancora chiusa; controller autonomi e test headless restano da fare.
- Nessun cambiamento voluto a renderer, preset embedded, formato JSON v1 o limiti raster. Baseline Windows confermata invariata (M4D.4-R1, 859/859); R2/R1 da verificare.


## 2026-11-17 — M4D.4-R2 — Roadmap A/B e 17 default da allegati

- Confermate le priorità: refactoring `StudioWindow.cs`, anteprima raster calcolata alla dimensione finale con fit solo visivo, nuovi generatori/ricette, aumento qualità e risoluzione fino a 8K; **poi** compositore universale, animazioni rinviate.
- Copiati **17 JSON forniti dall'utente** nel progetto come risorse embedded, consumate da `StudioRecipeDefaults.RecipeFor`: parametri/colori/seed/dimensioni/animazione completi per nuovi lavori Studio. Il seed Domain Warp non è più forzato a 42; Flow Loop conserva i propri 16 frame/18 fps. Le ricette importate e i default di catalogo restano invariati.
- Aggiornati `StudioWindow.SelectArtwork`, test dei preset e test che confrontavano i vecchi valori parziali. Nessuna modifica all'algoritmo, al renderer o al formato JSON v1.
- Aggiornati ROADMAP, HANDOFF, CURRENT_STATE, INDEX e guida Fase A/B. Segnalate le 52 preview mancanti nell'archivio iniziale come intervento A0. Manifesto SHA-256 rigenerato per i file effettivamente distribuiti.
- **Stato:** R2 da validare su Windows; ultima baseline confermata M4D.4-R1, 859/859 test. Funzioni 8K, preview nativa obbligatoria, refactoring e compositore **non ancora implementati**.


## 2026-11-16 — M4D.4-R1 — Orbite SDF: macchie e fusioni

- Nuovo campo `PigmentSdfField` e renderer SDF organico: ellissi deformate, unione morbida, fusione cromatica di quattro pigmenti, venature, rilievo/riflessi e piani con ombre/blur.
- Stili pigment/glass, layout cluster/orbital/river, sei palette e 36 controlli numerici Studio. Trentuno controlli nuovi facoltativi nei JSON v1. Nuovi lavori Studio con macchie; classico e ricette precedenti preservati.
- Copertura locale di bordi e tratti subpixel anche in standard; fine-2x disponibile. Confronto contro riferimento 4×, errore alpha ridotto del 71,3% nel caso documentato. Buffer per piano riutilizzato e tre righe di campioni, budget prima dell'output.
- Otto nuove ricette Full HD, due fine-2x; 81 ricette totali e 17 generatori. Confronti artistico/AA, screenshot README e guida `M4D4_SDF_MACCHIE_FUSIONI.md`.
- Build Release senza avvisi/errori, **859/859 test Windows**, Full HD/Studio/CLI e hash dei quindici renderer precedenti verificati. Nessuna nuova dipendenza di produzione.

## 2026-11-16 — M4D.3-R1 — Profondità, carta e bokeh

- Nuovi algoritmi `OrganicAperture` e `DepthParticleField`; artwork `paper-cut` e `bokeh-depth` con ombre gaussiane, strati ordinati, luce e fuoco controllabili. Catalogo a 17.
- Livelli riutilizzabili SrcOver con blur, drop shadow e opacità di gruppo; aperture even-odd e dischi illuminati. Budget prima delle superfici/output, cancellazione e alpha premoltiplicato.
- Paesaggi a strati con sei parametri opzionali: lontananza sfocata, ombre morbide direzionali e bordi luminosi. Nuovi lavori Studio con profondità attiva; JSON precedenti e default catalogo preservati.
- Dieci ricette Full HD, due fine-2x, 73 ricette totali; anteprime e confronto effetti spenti/accesi. Screenshot Studio aggiornato. README mantiene solo descrizione, installazione ed esecuzione.
- Build Release senza avvisi/errori, **801/801 test Windows**; smoke Full HD/Studio e 15 PNG precedenti identici. Guida `M4D3_PROFONDITA.md`, evidenze `docs/validation/M4D3_R1_*`. M4C.3 resta aperta per editor livelli/maschere/blend; nessuna nuova dipendenza di produzione.

## 2026-11-16 — M4D.2-R1 — Pittura ed evoluzione dei generatori iniziali

- Nuovi algoritmi `OrganicStroke` e `FractalProfile1D`; artwork `flow-brush` e `strata-landscape` con setole/pressione, campi curl/vortice/braid, profili frattali, luce, atmosfera e ombre. Catalogo a 15.
- Curve parametriche, Noise Flow, Plasma, Streamlines distanziate e Flow Loop ampliati con controlli artistici opzionali. Noise Flow fino a 10000 tracce. Nuovi lavori Studio con preset più ricchi; apertura JSON e default di catalogo preservati.
- Quattordici ricette Full HD e anteprime; 63 ricette totali. Confronto dei cinque generatori iniziali a pari seed/risoluzione. Tredici renderer precedenti verificati con PNG identici via SHA-256.
- FlowTracer cancellabile internamente, budget per vertici/setole/campioni e sequenze. Modificare solo opacità nei flussi storici conserva le traiettorie. Esportazione CLI verificata su 12 fotogrammi Full HD distinti.
- Codex Windows: build Release senza avvisi/errori, **733/733 test**, quattordici PNG Full HD e smoke Studio. Screenshot aggiornato; README mantiene solo descrizione/installazione/esecuzione, senza changelog o roadmap. Guida `M4D2_PITTURA_EVOLUZIONE.md`, log/dati `docs/validation/M4D2_R1_*`.

## 2026-11-16 — M4D.1-R1 — Dettaglio e controllo

- Algoritmi `GuillocheCurve` (trocoidi razionali chiuse con armoniche) e `TonalHatching` (copertura tonale filtrata, fino a quattro direzioni). Artwork `guilloche` e `tonal-engraving`, colori indipendenti, finiture e fine-2x; catalogo a 13.
- Otto ricette Full HD con anteprime: tre Guilloché, tre incisioni e due Domain Warp avanzati; 49 JSON validati. Controlli per precisione, strati, geometria, inquadratura, tratteggio, toni e maschere locali.
- Dieci controlli Domain Warp opzionali: lacunarità, persistenza, scala deformante, rotazione, assi, offset, contrasto/gamma. Tre PNG precedenti confrontati via SHA-256 e identici.
- Passo fine degli slider e conservazione dell'input esatto. Corretto crash/modifiche da TextChanged ritardati dopo il cambio di generatore, emerso nello smoke dello Studio.
- Codex Windows: **626/626 test**, build Release senza avvisi/errori, otto PNG Full HD generati/decodificati, controlli e input preciso verificati headless. Log/dati in `docs/validation`, guida in `M4D1_DETTAGLIO_CONTROLLO.md`. Dipendenze di produzione invariate.

## 2026-11-16 — M4B.1-R1 — Domain Warp/LIC, streamlines dense e picker

- Algoritmi riutilizzabili `DomainWarpNoise2D` e `LineIntegralConvolution`; artwork `domain-warp` e `lic-flow`, tre palette/quattro ruoli colore, materia/finiture e fine-2x. Catalogo a 11, sette ricette Full HD con anteprime.
- Streamlines da 500 a 10000 massime, distanza/spessore opzionali, candidati adattivi fino a 200000 e distribuzione su tutto il canvas nelle impostazioni estese. Cancellazione durante il tracing; vecchi parametri/default conservati.
- Picker ridimensionabile con footer Annulla/Conferma fuori dallo scroll e altezza adattata allo schermo in DIP.
- Build Release con zero avvisi/errori, **558/558 test**, sette PNG Full HD generati/decodificati, copertura delle due ricette dense su 64 blocchi. Smoke picker headless a quattro altezze, inclusi conferma/annulla. Evidenza Windows Codex in `docs/validation`; desktop/DPI e altri OS da verificare.
- Tool C# opzionale per riprodurre gli smoke; unica nuova dipendenza del tool Avalonia.Headless 12.1.4, prodotto invariato nelle dipendenze.

## 2026-11-16 — M4C.2-R4 — Correzione test budget materia

- Riprodotto il fallimento R3 segnalato dall'utente (513/514): il test attendeva il rifiuto di un poster 4000×4000 standard consentito dai nuovi budget.
- Sostituito con quattro casi validi verificati senza rendering e quattro casi non validi verificati prima dell'output; coperti limite standard per entrambi gli assi e limite fine-2x. Nessuna modifica al codice di produzione.
- Build Release riuscita con zero avvisi/errori e **521/521 test superati da Codex su Windows**, SDK 10.0.201. Pacchetti locali e telemetria Avalonia disattivata durante la verifica; smoke GUI e poster manuali restano aperti.
- Documentazione, manifest SHA-256 e ZIP completo aggiornati. Evidenza in `docs/validation/M4C2_R4_windows.log`.

## 2026-11-16 — M4C.2-R3 — Test JSON, poster 5 MP e anteprima completa

- **Riscontro Windows M4C.2-R2:** build riuscita; su 507 test, 506 superati e `RecipeTests.MissingParametersAreRejected` fallito perché la sostituzione testuale generava una virgola finale JSON. Il test ora usa `JsonNode` per togliere la proprietà `scale` in modo sintatticamente valido; parser runtime invariato.
- **Finiture:** `ArtisticFinishOptions.MaximumFinishPixels` passa da 4.194.304 a 16.777.216. Aggiornati il gate motore e quello della superficie Skia. Manteniamo il limite 4096×4096 statico globale. Il limite Plasma separato a 4 MP è eliminato: 2500×2000 con grana/carta/dithering non è più rifiutato solo per la dimensione. Per `fine-2x` resta il limite dei 16 MP sottopixel, quindi fino a 4 MP finali.
- **Preview Studio:** campi separati per massimo raster anteprima e massimo riquadro visibile; `Adatta`, `100%` e `Raster = output`; zoom automatico proporzionale al ridimensionamento e stato che confronta le dimensioni preview/output.
- Test aggiornati per limite poster, Plasma 5 MP (validazione senza allocazione grande), fitting/proporzioni, preview a risoluzione piena. Nessuna modifica a `algorithmVersion`, JSON v1, seed, colori o rendering predefinito.
- Archivio R3 integrale; build/test Windows R3 non ancora confermati.

---

## 2026-11-16 — M4C.2-R2 — Hotfix switch HSV / avvio Studio massimizzato (conferma Windows attesa)

- **Bug segnalato dall'utente su M4C.2-R1:** `StudioColorValue.cs(54,44)` CS8506 e CS8131. Causa: il compilatore C# non riesce a inferire un unico tipo per le tuple dello `switch` che mescolano componenti `double` con zeri `int`.
- **Fix:** uso di `0d` in ogni posizione a zero dei sei rami; nessuna modifica al calcolo HSV, RGB, alpha o al formato ricetta JSON v1. Aggiunte sei varianti cromatiche nei test `StudioColorValueTests`.
- **Studio:** apertura automatica in stato `Maximized` (titolo e taskbar rimangono accessibili); ripristino e ridimensionamento consentiti.
- Questo archivio contiene il progetto completo M4C.2 + hotfix; compilazione/test della R2 non ancora confermati. Il blocco di compilazione della R1 è invece reale e registrato.

## 2026-11-16 — M4C.2-R1 — Carta, materia e pigmenti (GATE WINDOWS APERTO)

- Aggiunti quattro parametri opzionali **MATERIA** su nove generatori: `paper-texture` none/cotton/rough, `paper-strength`, `pigment-granulation`, `ink-absorption`; default spenti, formato ricetta v1 e ID/versione algoritmi invariati.
- `MaterialPostProcessor` realizza fibre procedurali multiscala, granulazione cromatica guidata dalla luminanza e assorbimento locale basato sui pixel originali con sole tre righe di buffer. Texture seeded stabile sul canvas e alpha invariato.
- Pipeline con materiali prima di grana e dithering della M4C.1, dopo fine-2x. La via PNG senza effetti rimane invariata; il limite 4.194.304 pixel per finitura già esistente resta attivo.
- Nuovi `MaterialPostProcessorTests`, aggiornamento `CatalogTests`, cinque ricette Full HD, specifica dedicata e documenti operativi allineati. La baseline Windows confermata resta M4A.4-R1; nessun nuovo gate dichiarato chiuso.

---


## 2026-11-16 — M4C.1-R1 — Grana e dithering deterministici (GATE WINDOWS APERTO)

- Baseline utente **M4A.4-R1** valida; revisioni **R2/R3** ancora da compilare e testare. Nessun test Windows dichiarato in questa consegna.
- Aggiunti a tutte le 9 opere quattro controlli **FINITURA** facoltativi nel JSON v1: `grain-amount` (default 0), `grain-size` (default 1), `dither-mode` (default none; Bayer 4×4 o 8×8), `dither-levels` (default 8). `ArtworkParameterSpec.Integer` esteso con `optionalInJson`, lasciando invariato il comportamento delle vecchie chiamate.
- Nuovi `ArtisticFinishOptions` e `ArtisticPostProcessor`: grana da hash seed/coordinate senza Random globale, retino ordinato, alpha intatto e texture fissa su ogni frame del loop. `ArtworkRenderEngine` trasmette seed e token di cancellazione fino alla codifica.
- `SkiaRasterSurface.WritePng` conserva il vecchio ramo non modificato quando gli effetti sono spenti; quando attivi applica la finitura dopo l'eventuale riduzione 2×. Budget **4.194.304 pixel finali** verificato prima di render e output; poster 4000×4000 standard senza finitura ancora consentiti quando supportati.
- `ArtisticPostProcessorTests` e `CatalogTests` coprono default, JSON, nove generatori, alpha, riproducibilità, cancellazione, budget e supersampling; nuovi quattro esempi Full HD.
- Aggiornati documenti, handoff, roadmap e manifesto. **Verifiche statiche e ZIP possibili; build/xUnit/smoke non eseguibili per assenza .NET SDK**. Specifica: `docs/M4C1_GRANA_DITHERING.md`.

---

## 2026-11-16 — M4A.4-R3 — Supersampling e PNG alpha-safe (GATE WINDOWS APERTO)

- Ricevuta richiesta di proseguire senza poter testare. Baseline confermata su Windows **M4A.4-R1**; M4A.4-R2 ancora da validare, non attribuirle test già passati.
- Nuovo parametro facoltativo `render-quality` su **9 generatori**: `standard` (percorso originale) / `fine-2x` (campionamento 4 sottopixel/pixel, downsampling nel formato BGRA premoltiplicato). JSON v1 e algorithmVersion invariati.
- `RasterQualityPolicy` applica in `Validate()` un limite di **16.777.216 pixel temporanei** a 2×. L'export a 4000×4000 resta possibile in modalità standard, non fine-2x; messaggi prima dell'allocazione e scrittura.
- `SkiaRasterSurface` supporta scala raster 1 o 2 e riduzione lineare alpha-safe tramite `SKSamplingOptions` senza API obsolete. `ArtworkRenderEngine` e `SdfOrbitRenderer` campionano pixel reali 2× per Plasma/SDF, a coordinate logiche costanti. Nessun caching frame, stream PNG lasciato aperto.
- Aggiunto `RasterQualityTests` e aggiornato `CatalogTests` e `StudioCreativeControlsTests`; tre esempi qualità fine-2x a 1920×1080; documentazione e manifesto aggiornati.
- **Verifiche disponibili:** struttura sorgenti/ricette, ZIP e SHA-256; **non eseguiti** build .NET 10, xUnit, rendering PNG e test visuali/memoria. Gate Windows R2/R3 aperto.

---


## 2026-11-16 — M4A.4-R2 — Colori per tutti gli artwork e picker HSV/RGB/Alpha (GATE WINDOWS APERTO)

- **L'utente ha confermato M4A.4-R1: compilazione e suite xUnit riuscite su Windows.** Questa R2 è un incremento nuovo e deve essere nuovamente validata.
- I cinque generatori storici (`parametric-curves`, `noise-flow`, `spaced-streamlines`, `looping-flow`, `plasma-noise`) ora espongono ruoli colore con `Auto`, RGB e alpha. Sfondo gradiente dei primi due preservato quando Auto; override esplicito sfondo uniforme.
- Studio: pulsante **Scegli…**, finestra HSV/RGB/Alpha/HEX con conferma/annulla; campione Auto coerente con la palette corrente su tutti i nove generatori, e aggiornamento cambiando preset.
- Nuovo `StudioColorValue` testabile senza GUI; catalogo e renderer aggiornati senza introdurre pacchetti o variare schema JSON v1, ID e versioni algoritmo. Migrazione opzionale delle vecchie ricette.
- Test `StudioColorValueTests`, `ArtisticColorRoleTests`, test catalogo aggiornato; cinque nuove ricette Full HD di studio cromatico e specifica `M4A4_R2_COLORI_HSV.md`.
- Non implementati ancora: AA raster analitico, supersampling limitato, opzioni DPI, armonie colore e texture materiche. Il colore usa sRGB straight-alpha in input; le superfici native usano premoltiplicazione Skia.
- Ambiente di preparazione privo di SDK .NET: verifica statica e integrità archivio sì; **nessuna compilazione/test xUnit/GUI R2 eseguita** qui. Comando Windows: `clear && dotnet clean && .\tools\validate.ps1`.

## 2026-11-16 — M4A.4-R1 — Laboratorio artistico, parametri e qualità (GATE WINDOWS APERTO)

- L'utente ha confermato **M4A.3-R1 Voronoi: compilazione e test superati su Windows**, quindi chiuso il gate non ancora documentato nell'handoff H1.
- Nuove ricette create nello Studio: 1920×1080; anteprima limitata a 800×600/480.000 pixel. `looping-flow` ammette il Full HD con 16 frame default nel budget di 40 milioni di pixel cumulativi.
- Pannello dei parametri più ampio, gruppi per categoria, campo HEX e swatch per colore, pulsante Auto (ritorno alla palette) e input validato senza salvare stringhe cromatiche errate.
- `ArtworkParameterSpec.OptionalInJson` e `ColorOverride`: nuove proprietà facoltative in importazione, valori canonici e retrocompatibilità delle vecchie ricette JSON v1. Colori per ruolo in Topografia, SDF, Poisson e Voronoi; le palette restano preset di partenza.
- Poisson: gamma spacing 0.001–0.2 e raggio 0.1–128, fino a 50.000 punti e 6M celle, variabilità raggio, noise pigmento, punti solid/ring/mixed.
- Topografia: tratti precise/handdrawn/soft, deformazione seedata e algoritmo Chaikin riutilizzabile in `GenerativeArt.Algorithms`, rugosità e spessori più ampi.
- Quattro esempi Full HD nuovi per confrontare stile organico/morbido, stippling denso e mosaico colorato. Test xUnit aggiunti per retrocompatibilità, colore, default Full HD, validazione del loop, parametri e differenze raster; documentazione e handoff allineati.
- **Non ancora implementati:** vero ColorPicker visuale, modifica di ogni colore dei cinque generatori storici, supersampling/AA raster, texture ricche. SDK .NET 10 assente qui: build/test/smoke R1 da eseguire su Windows.

## 2026-11-16 — M4A.3-R1 · HANDOFF-H1 — Allineamento documentazione (SOLO DOCUMENTI)

- Consegnato un archivio completo della **M4A.3-R1 invariata nel codice**, per il passaggio a una nuova conversazione. Nessun sorgente C#, progetto, test, ricetta JSON, script o licenza modificato.
- Distinto in modo esplicito **ultimo gate Windows confermato: M4A.2-R1** da **ultima implementazione: M4A.3-R1 con gate ancora aperto**. Non sono stati eseguiti nuovi test .NET in questo handoff.
- Risolte le incongruenze nei documenti operativi: catalogo a **nove** generatori, M4A.2 già validata, architettura aggiornata dopo SDF/Poisson/Voronoi, comando PowerShell canonico e checklist del poster 4000×4000.
- Riscritti `CURRENT_STATE.md`, `HANDOFF.md`, `ARCHITETTURA_PRODOTTO.md`; aggiornati `README.md`, `INDEX.md`, `ROADMAP.md`, `TESTING.md`, `QUALITY_GATES.md`; creato `NUOVA_CHAT.md` con testo pronto da incollare.
- Materiale storico e specifiche delle milestone precedenti preservati; restano aperti smoke reale M4A.3, Linux/macOS, QA memoria/animazioni e qualità visiva.


## 2026-11-16 — M4A.3-R1 — Voronoi e Mosaico Voronoi (GATE WINDOWS APERTO)

- Ricevuta conferma Windows dell'utente: **M4A.2-R1 compila, test passati, Studio e CLI OK**; questa è la baseline.
- Nuovo `VoronoiTessellation` (Algorithms): clipping semipiani con precisione double, poligoni convessi limitati, area, centroidi, Lloyd iterativo, limiti e cancellazione. Nuova suite `VoronoiTessellationTests`.
- Nuovo renderer `VoronoiMosaicRenderer`: siti seeded jittered, palette glass/copper/lagoon, fughe e celle Skia; estesa `SkiaRasterSurface` con `DrawPolygon` via `SKPathBuilder` e test fill/bordo.
- Catalogo da **8 a 9 artwork**: nuovo `voronoi-mosaic`, parametri `sites`, `relaxations`, `jitter`, `gutter`, `palette`; dispatcher e validazione budget integrati, JSON v1 invariato.
- Due ricette di esempio, standard 1200×1200 e poster 4000×4000; test per PNG, seed, parametri, JSON, annullamento e validazione poster senza render gigante nella suite.
- Aggiornati Studio title, README, roadmap, handoff, quality gates, testing, index e specifica `M4A3_VORONOI_TASSELLAZIONI.md`. Nessuna modifica al viewport/animazioni o agli otto generatori preesistenti.
- SDK .NET 10 assente qui: **compilazione/test xUnit e smoke visuale M4A.3 ancora da eseguire**. Comando `clear && dotnet clean && .\tools\validate.ps1`.

## 2026-11-16 — M4A.2-R1 — Geometrie SDF e campionamento Poisson (GATE WINDOWS APERTO)

- Ricevuta conferma dall'utente che **M4A.1-R4 funziona completamente** su Windows, dopo il fix budget. Baseline preservata.
- Nuovo modulo C# `SignedDistance2D`: circle, box, rounded box, rotazione, CSG (`Union`, `Intersection`, `Difference`) e `SmoothUnion` con validazioni. Primo artwork `sdf-orbits` per geometrie implicitamente connesse, fasce, tre palette e parametri artistici.
- Nuovo modulo C# `PoissonDiskSampler`: campionamento Bridson seeded, lista attiva, griglia spaziale e limiti su celle/punti/tentativi; primo artwork `poisson-stippling` con pigmenti deterministici e tre palette.
- Catalogo passato da **sei a otto** opere; `ArtworkRenderEngine` condiviso espanso. Vecchi ID, algoritmi, JSON v1, Studio, pan/zoom, preferiti, temi e CLI legacy invariati.
- Suite ampliata con `SignedDistance2DTests`, `PoissonDiskSamplerTests`, casi rendering PNG, seed, parametri, JSON, cancellazione e validazione poster 4000×4000. Aggiunti quattro esempi JSON e specifica `M4A2_SDF_POISSON.md`.
- **SDK .NET 10 non disponibile qui**: build, test e immagini M4A.2 non eseguiti. Richiesto gate Windows completo e smoke visuale e di memoria sulle grandi risoluzioni.

## 2026-11-16 — M4A.1-R4 — Correzione regressione budget topografico (GATE WINDOWS APERTO)

- L'utente ha riportato che **M4A.1-R3 compila**, ma 1 test su 371 fallisce: `ArtworkRenderEngineTests.TopographicRecipeRoundTripBudgetAndCancellation` riga 147 (`Assert.Throws`, nessuna eccezione). 370 test erano superati.
- Causa verificata: il test attendeva il rifiuto di `4000×4000` con `cell-size=3`, `contours=24` (= 42.645.336 celle×livelli), mentre il budget R3 era stato alzato a 48.000.000.
- Correzione mirata: `TopographicContourRenderer.MaximumCellLevelBudget` = **40.000.000**. Invariato il limite di **16.777.216 pixel**, quindi `1600×1600`, `4000×4000` standard e fino a `4096×4096` per ricette ammissibili restano possibili.
- Test rafforzato: stesso 4000×4000 e passo 3 accetta 22 livelli (39.091.558), rifiuta 23 (40.868.447) e 24 (42.645.336). Nessun test di rendering 4000×4000 eseguito nell'ambiente di preparazione.
- Aggiunto esempio `examples/recipes/topographic-poster-4000.json` per lo smoke CLI reale di un poster 4000×4000; la suite xUnit verifica solo la validazione delle dimensioni, non genera il PNG enorme.
- Aggiornati titolo Studio, README, ROADMAP, HANDOFF, CURRENT_STATE, TESTING e QUALITY_GATES; algoritmi, viewport, preferiti, export atomico e schema JSON invariati.
- **.NET SDK 10 non disponibile qui:** serve gate Windows `clear && dotnet clean && .\tools\validate.ps1` e smoke reale dell'export PNG ad alta risoluzione.

## 2026-11-16 — M4A.1-R3 — Export topografico hi-res per stampa (GATE WINDOWS APERTO)

- Ricevuta conferma utente per **M4A.1-R2: compila, test OK, Studio OK, CLI OK** su Windows. Il sesto generatore funziona, ma in export reale il renderer `topographic-contours` rifiutava `1600×1600` con l'errore *"massimo 2.097.152 pixel per immagine"*.
- `TopographicContourRenderer.ValidateBudget` ora allinea il tetto pixel al budget statico del runtime: **16.777.216 pixel per immagine** (fino a `4096×4096`) e alza il limite di complessità a **48.000.000 celle × livelli**, sufficiente per poster `4000×4000` con impostazioni standard.
- Aggiornati i test `ArtworkRenderEngineTests`: `1600×1600` e `4000×4000` sono ora consentiti; restano rifiutati casi oltre 4096 px per lato o combinazioni troppo dense (`4000×4000`, `cell-size=3`, `contours=24`).
- Documentazione aggiornata: README, CURRENT_STATE, HANDOFF, TESTING, QUALITY_GATES, `M4A1_ISOLINEE.md`.
- SDK .NET 10 assente qui: **R3 da validare su Windows** con `clear && dotnet clean && .\tools\validate.ps1` e smoke CLI/Studio del PNG topografico hi-res.

## 2026-11-16 — M4A.1-R2 — Correzione compilazione SKPathBuilder (GATE WINDOWS APERTO)

- La build Windows di **M4A.1-R1** si è fermata in `SkiaRasterSurface.cs` alle chiamate obsolete `SKPath.MoveTo` e `SKPath.LineTo` (due CS0618; warnings treated as errors). La suite M4A.1-R1 non è stata completata.
- `SkiaRasterSurface.DrawPolyline` ora crea il percorso con `SKPathBuilder`, lo ottiene con `Detach()` e rilascia sia builder sia path; **nessun silenziamento di CS0618** e nessun downgrade di SkiaSharp.
- Aggiunto il test `SurfaceDrawPolylineScalesPointsWithoutClosingPath` per verificare coordinate scalate, segmento orizzontale/verticale e assenza di chiusura diagonale.
- Invariati Marching Squares, algoritmo topografico, catalogo a 6 generatori, ricette JSON v1, CLI, Studio, pan/zoom, temi ed export.
- R2 non compilata né testata in ambiente di preparazione (SDK .NET 10 non disponibile); richiesto gate Windows completo con `clear && dotnet clean && .\tools\validate.ps1` e smoke PNG topografico.

## 2026-11-16 — M4A.1-R1 — Marching Squares e Curve topografiche (BUILD WINDOWS FALLITA: CS0618)

- Ricevuta conferma utente per **M3D.2-R2: build, test, Studio, preferiti e tema tutti OK su Windows**. Baseline R2 validata.
- Primo incremento M4A: `MarchingSquares.Trace` con interpolazione coerente degli archi condivisi, raccordo delle linee, gestione selle 5/10 con decider bilineare e cancellazione.
- Nuovo generatore **`topographic-contours`**: terreno Simplex frattale, bande cromatiche, 4–24 isolinee, passo regolabile, 3 palette e spessore regolabile; path Skia collegati.
- Sei ID attesi nel catalogo; renderer e JSON v1 integrati, esempio fornito. Tutte le vecchie ricette/algoritmi restano invariati.
- Test xUnit estesi per topologia, contorni chiusi, budget, seed, parametri, PNG, cancellazione e ritorno JSON; documentazione M4A.1, README, ROADMAP, HANDOFF e QA aggiornati.
- **SDK .NET 10 assente durante la preparazione**: file JSON e archivi possono essere verificati staticamente, ma non è stata eseguita una nuova build o suite; gate Windows M4A.1 aperto.


## 2026-11-16 — M3D.2-R2 — Preferiti, recenti e temi (GATE WINDOWS APERTO)

- Ricevuta conferma dall'utente che **M3D.2-R1 compila, supera i test e Studio funziona** su Windows.
- Aggiunti `StudioPreferences`, file preferenze JSON schema v1 e palette chiara/scura in Application; scrittura atomica, lettura limitata a 64 KiB, massimo otto percorsi recenti e filtro preferiti.
- Nello Studio: stella per opera, filtro `Solo preferiti`, elenco ricette locali aperte/salvate, rimozione dalla lista senza eliminare il file, cambio tema in diretta con ripristino al riavvio. La colonna sinistra ora scorre.
- Nuovi `StudioPreferencesTests`: modello, dedup/limiti, roundtrip, versioni non valide, chiavi duplicate/ignote, dimensione eccessiva e palette.
- Aggiornati README, CURRENT_STATE, ROADMAP, HANDOFF, TESTING, QUALITY_GATES e specifica M3D.2-R2.
- Invariati `PreviewViewport`, rendering, algoritmi, CLI, ricette JSON v1 ed export R1. **Gate R2 non eseguito qui**: manca SDK .NET 10; richiesta la validazione Windows.

## 2026-11-16 — Conferma M3D.2-R1 Windows

- L'utente segnala compilazione riuscita, suite test OK e Studio funzionante. Lo stress della cancellazione durante export oneroso e i provider virtuali restano verifiche separate.

---

## M3D.2-R1 — 16 novembre 2026 — Export cancellabile e parametri numerici esatti (DA VALIDARE)

- **Conferma dall'utente:** M3D.1-R3 compila, suite xUnit verde, pan con sinistro e zoom con rotella perfetti su Windows. Base di questa revisione validata.
- Nuovo `AtomicOutputFiles.WriteGeneratedAsync`: output renderer direttamente su file temporaneo locale, senza un byte array PNG completo aggiuntivo; cancellazione e cleanup anche per eccezioni del renderer, pubblicazione dopo flush. `WriteAsync` esistente invariato.
- Studio: **Annulla export**, indicatore d'attività indeterminato, stato e serializzazione dei job; export full-resolution fuori dal thread UI, stop playback e preview differita. Per file virtuali fallback non atomico documentato.
- Numeri dei parametri: TextBox esatto accanto allo slider; input con `.` o `,`, canonicalizzazione invariante, gestione valori invalidi senza alterare ricette; nuovi `StudioParameterInput` e test dedicati.
- Test aggiunti in Application e Skia: export streaming decodificabile, errore, annullamento, sovrascrittura vietata, precisione e input invalidi.
- **M3D.2-R1 non compilata/testata qui**: .NET SDK 10 assente. Gate Windows aperto.

## M3D.1-R3 — 16 novembre 2026 — Pan sinistro e zoom a cursore (DA VALIDARE)

- Ricevuto feedback utente: **M3D.1-R2 build e test OK su Windows e Studio funzionante**. Il bootstrap Avalonia è quindi confermato, non più un gate pendente.
- Implementata `PreviewViewport` (Avalonia `Canvas`) per drag con tasto sinistro, pointer capture, zoom rotella ancorato al cursore e tasto **Centra**. Rimossi gli scrollbar automatici della preview; il vecchio slider resta sincronizzato.
- Aggiunto `StudioViewportState` in Application per offset/zoom in DIP, limiti 25–200%, pan con margine visibile e mantenimento della vista tra frame della stessa dimensione.
- **Sei nuovi test xUnit** sulle trasformazioni: centro, pan, zoom focale, mantenimento tra frame, resize, limiti e trascinamento estremo.
- Pan e zoom non innescano render e non cambiano ricetta o PNG esportato. Nessuna nuova dipendenza, nessun linguaggio di sviluppo aggiuntivo.
- **R3 non compilata/testata in ambiente di preparazione: .NET SDK 10 assente**. Il comando Windows obbligatorio è `clear && dotnet clean && .\tools\validate.ps1`, poi smoke mouse nello Studio.

## M3D.1-R2 — 16 novembre 2026 — Fix compilazione bootstrap Avalonia (DA RIVALIDARE)

- Ricevuto esito Windows per M3D.1-R1: build fallita con **CS0118** (`Application` visto come namespace) e **CS1061** (`Application.Run` inesistente) in `src/GenerativeArt.Studio/Program.cs`; non risulta alcuna esecuzione della GUI o suite R1 completa.
- Corretto il bootstrap con `AppBuilder.Configure<StudioApp>().UsePlatformDetect().StartWithClassicDesktopLifetime(args)` e nuova `StudioApp : global::Avalonia.Application` con `FluentTheme` e `IClassicDesktopStyleApplicationLifetime.MainWindow`.
- Nessun cambiamento a rendering, ricette JSON, catalogo, test o interfaccia funzionale. La verifica reale è ancora richiesta con `clear && dotnet clean && .\tools\validate.ps1` e avvio Studio.
- Corretto il vincolo documentale: **XAML/AXAML è permesso**; il progetto resta C#/.NET + PowerShell minimo, senza introdurre Python, Go, Kotlin, Java e altri linguaggi applicativi non autorizzati.
- Controllo archivio/integrità effettuato in ambiente di preparazione; SDK .NET 10 assente, **nessuna build R2 eseguita**.

## M3D.1-R1 — 16 novembre 2026 — Primo Studio Avalonia code-only (GATE APERTO)

- Ricevuta conferma Windows dall'utente: **M3C.3-R2 compila, suite xUnit e `list` OK**. Regressione CatalogTests R1 risolta.
- Nuovo progetto `GenerativeArt.Studio`, interfaccia C# senza XAML: catalogo ricercabile, parametri tipizzati, seed/risoluzione, preview zoom, JSON, export PNG, playback `looping-flow` Play/Pausa/fase.
- Anteprima in background con cancellazione delle richieste obsolete, debounce in editing, salto dei tick occupati in playback e serializzazione dei render.
- Aggiunti `StudioPreviewPolicy`, `AtomicOutputFiles` e 7 test xUnit Application nuovi, oltre alle versioni NuGet Avalonia 12.1.6.
- JSON v1, API CLI e algoritmi originari **non modificati**. NuGet/SDK per la UI nuovi; primo restore richiede la rete.
- **Gate .NET M3D.1 non eseguito qui**: l'SDK .NET 10 non è presente. Command Windows: `clear && dotnet clean && .\tools\validate.ps1`. Dopo test OK, `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.
- Limitazioni: export animazione GUI solo singolo PNG; GIF/APNG/video non realizzati; smoothness loop da misurare e migliorare.

## M3C.3-R2 — 16 novembre 2026 — Correzione regressione test catalogo (DA RIVALIDARE)

- Riscontro reale su Windows nella revisione R1: `GenerativeArt.Application.Tests.CatalogTests.CatalogHasStableSortedIdsAndLoopCapability` fallisce perché attendeva 4 descrittori e il catalogo contiene correttamente 5 opere dopo l'aggiunta di `plasma-noise`.
- Sostituita l'asserzione numerica obsoleta con un confronto dell'**elenco esatto degli ID ordinati**, includendo `plasma-noise`. Aggiunte verifiche mirate su capacità raster, assenza di animazione e parametri `frequency`/`octaves`/`palette`.
- Nessuna modifica ai renderer, ai descrittori di produzione, alle ricette o al formato JSON. Gli altri test non sono dichiarati superati senza esecuzione.
- Comando Windows richiesto dall'utente, nella directory `GenerativeArt` che contiene la solution: `clear && dotnet clean && .\tools\validate.ps1` (PowerShell 7). Validazione R2 in attesa dell'esecuzione su Windows: SDK .NET 10 assente qui.

## M3C.3-R1 — 16 novembre 2026 — IMPLEMENTATA, NON ANCORA VALIDATA CON .NET

- Aggiunto contratto `IArtworkRasterRenderer` in Application; implementato `ArtworkRenderEngine` Skia per tutte le opere da ricetta.
- Abilitato `render-recipe` per `noise-flow` (parametro `lines`) e `spaced-streamlines` (`max-lines`).
- Aggiunto generatore `plasma-noise` con Simplex fBm, frequenza, ottave e tre palette.
- Reindirizzati tutti e quattro i comandi CLI legacy allo stesso runtime.
- Introdotto salvataggio statico `CreateNew` e cleanup dei file frame incompleti; manifest scritto soltanto al termine.
- Aggiunti test PNG/seed/parametri/cancellazione/output rollback e ricette di esempio.
- Rafforzati i limiti in `ArtworkRecipeJson.Deserialize(string)` prima della conversione UTF-8 e cleanup del JSON incompleto in `ArtworkRecipeFiles.SaveNew`.
- Attenzione: **qui non è disponibile SDK .NET 10**; nessuna nuova compilazione, suite xUnit o smoke PNG eseguiti. La validazione precedente dell'utente si riferisce a M3C.2-R1.

## 2026-11-16 — Consolidamento documentazione / passaggio nuova chat (solo documenti)

- L'utente ha confermato **M3C.2-R1: compilazione, test xUnit, creazione JSON e PNG riuscite** su Windows. I vecchi riferimenti al blocco 288/289 riguardavano la versione precedente al fix JSON e non sono più attuali.
- Unificati README, stato corrente, roadmap, handoff e criteri di test; aggiornata la distinzione fra codice implementato, prove Windows e controlli futuri.
- Eliminati piani paralleli, log di verifica ridondanti e note singole dei fix, riassunte in `STORICO_CORREZIONI.md`.
- **Nessuna modifica a file C#, csproj, solution, test, ricette, script, licenze o fixture.** Nessuna nuova build eseguita durante questa sola revisione documentale.

---

## Cronologia originale delle consegne (stato al momento della stesura)

Le etichette «DA VALIDARE» o «NON ANCORA VALIDATA» nelle voci seguenti descrivono **la situazione storica all'atto della consegna**, non la baseline attuale. Per lo stato valido usare `CURRENT_STATE.md`.

## 2026-11-16 — M3C.2-R1: test JSON, eccezione derivata (DA RIVALIDARE)

- Segnalato da Windows: soluzione compilata; suite 289 test, 288 superati e uno non superato in `RecipeTests.TrailingCommaAndCommentsAreRejected`.
- Il parser `System.Text.Json` rifiuta correttamente i commenti, ma genera `JsonReaderException` (sottotipo di `JsonException`); `Assert.Throws<JsonException>` xUnit v2 richiede il tipo esatto.
- Corrette **solo due asserzioni** del medesimo test in `tests/GenerativeArt.Application.Tests/RecipeTests.cs`, usando `Assert.ThrowsAny<JsonException>` per commenti e trailing comma.
- Nessuna modifica ai sorgenti di produzione, allo schema, alle regole JSON, alle dipendenze o agli altri test.
- **Build e test dopo il fix non eseguiti in questo ambiente**: gate G3C.2 ancora aperto; riepilogo storico consolidato in `docs/STORICO_CORREZIONI.md`.

## 2026-11-16 — M3C.2: ricette JSON e rendering da ricetta (DA VALIDARE)

- Aggiunte `ArtworkRecipe`, `RecipeAnimation`, `ArtworkRecipeJson`, `ArtworkRecipeFiles` in Application; formato JSON v1 con parametri tipizzati, seed UInt64 decimale stringa e versione algoritmo esatta.
- Validazione ricette: proprietà duplicate/ignote rifiutate, versioni incompatibili, limiti dimensionali e di 64 KiB in lettura, no sovrascrittura dei preset.
- CLI `recipe-new`, `recipe-validate`, `recipe-show`, `render-recipe`; conservati i vecchi comandi.
- Primo dispatch reale: `parametric-curves` applica la scala; `looping-flow` applica raggio e palette oltre a seed e timing; le altre due demo rispondono esplicitamente con funzionalità ancora non disponibile.
- Suite xUnit Application estesa, ricette esempio, documentazione e roadmap aggiornate, nessuna nuova dipendenza NuGet, nessun linguaggio applicativo diverso da C#.
- **SDK .NET 10 non disponibile in questo ambiente**: build, xUnit e smoke M3C.2 non eseguiti. Ultima baseline confermata dall'utente: M3B.

## 2026-11-16 — M3C.1 + prime sequenze periodiche (DA VALIDARE)

- Conferma dell'utente: M3B compila, test superati e PNG creato correttamente su Windows (G3B chiuso).
- Priorità artistica aggiuntiva: le animazioni del repository Go diventano requisito di prodotto; analizzati `platform.go` e `poster.go` dell'archivio originale.
- Nuovo assembly `GenerativeArt.Application`: catalogo esplicito di quattro descrittori, capacità static/vector/animated/seamless, parametro tipizzato con validation, versioni e duplicate-check.
- Nuova CLI `list`, `describe`, più `render-loop-frames` (frame PNG separati con manifest, senza accumulo in RAM).
- Nuovi `LoopFrame` nel Core e `LoopNoiseField2D` in Algorithms; suite xUnit ampliata in Core, Algorithms e Application.
- Solution espansa a nove progetti senza nuovi package NuGet, senza introdurre Go/Kotlin/Python/XAML.
- Update della roadmap: preview loop con Studio M3D, GIF/APNG streaming anticipati a M5, video/motion blur a M6.
- **Non eseguiti build/test C# e smoke PNG della nuova versione**: SDK .NET non presente nell'ambiente di preparazione. Nuovi gate aperti.

## 2026-11-16 — P2.1: catalogo di preferenze artistiche

- Registrate 48 denominazioni Gȧrt e 4 del progetto Go con riferimenti ai file sorgente presenti negli ZIP forniti.
- Crinis evidenziato come benchmark estetico prioritario; famiglie di algoritmi riallineate ai gusti espressi.
- Identificate le varianti da confermare, inclusa la doppia denominazione Sixsix2/Sixsix 2.
- Nessun cambiamento a codice C#, test, dipendenze o script. Baseline M3B invariata, già validata dall’utente su Windows.

## 2026-11-16 — P2: revisione strategica dopo conferma M3B

- M3B/G3B chiuso su Windows in base al feedback utente: compilazione, xUnit e PNG riusciti.
- Nessuna modifica a sorgenti C#, progetti, test o script; nuova consegna esclusivamente documentale.
- Definita visione Framework + CLI + GenerativeArt Studio, con primo Studio anticipato a M3D.
- Roadmap unica riorganizzata: M3C registry/recipe/3 artwork; M3D GUI minima; M4 algoritmi; M5 varianti/metadata; M6 export/animazioni; M7 simulazioni/3D; M8 release.
- Nuovi: VISIONE_PRODOTTO, ESPERIENZA_STUDIO, ARCHITETTURA_PRODOTTO, CATALOGO_PRIORITA, REGISTRO_DECISIONI_P2; QUALITY_GATES aggiornato. Vecchi piani marcati storici.

# 16/11/2026 — M3B: integrazione flow e audit completo del repository Go (DA VALIDARE)

- Ricevuta conferma utente: la baseline **M3A-R1** compila, passa tutti gli xUnit e genera PNG su Windows; gate G3A chiuso.
- Nuove implementazioni C#: `VectorGrid2D`, `CurlNoiseField2D`, `EvenStreamlineOptions`, `EvenStreamlineTracer`; limiti di memoria/campioni/candidati.
- Nuovo comando CLI `render-flow-spaced` per streamlines RK4 con seed riproducibile; `render-demo` e `render-flow` conservati.
- Nuovi xUnit per griglie, curl, riproducibilità e limiti di spaziatura. M3B non compilato/testato nell'ambiente di consegna (SDK .NET assente).
- Analizzati **399 file (385 Go)** del repository caricato; inventario dei 164 descrittori, matrice comparativa, rischi ed espansione della roadmap.
- Nessun sorgente Go, Python, Kotlin, Bash, XAML o shader custom nel repository prodotto; nessun nuovo package NuGet.

# 16/11/2026 — M3A-R1 (fix compilazione + audit comparativo Go)

- Corretto `CS0019` in `FlowDemo.cs` sostituendo `i % 3 switch` con `(i % 3) switch`. Unico sorgente di produzione modificato.
- Integrato studio documentale di `jdxyw/generativeart`, matrice Gȧrt/Go/C# e roadmap prioritaria di integrazione, senza codice Go.
- Build e xUnit M3A-R1 non eseguiti qui (SDK .NET assente); test Windows antecedente fallito in CLI per CS0019.

