> **Aggiornamento documentale — galleria README:** 30 miniature 480×270, una per algoritmo, ricavate dai render 4K verificati e collegate alle ricette. Controllati immagini e collegamenti; nessuna modifica al codice applicativo. ZIP completo aggiornato: `GenerativeArt_M4E5_P4K_R1_README_GALLERIA_COMPLETO.zip`. I risultati della suite riportati sotto appartengono alla precedente validazione del codice.
> **STATO CORRENTE M4E.5-P4K-R1 — 17/11/2026:** priorità richiesta dall'utente: **4K per tutti i generatori e le ricette, con massima complessità e supersampling 2×; 8K generale successivamente**. Disponibili pulsante Studio **4K · qualità massima**, CLI `prepare-4k`, profilo opzionale `production` e 34 nuovi esempi: catalogo **30 generatori / 30 preset / 216 ricette**. **1.371 test Windows passati**, build Release zero avvisi/errori; azione Studio verificata sui 30 generatori e 30 file preset invariati, inclusi i 17 personali. [Guida e limiti](M4E5_4K_PRODUZIONE.md), [prove e misure](validation/P4K_R1_REPORT.md). Consegna completa `GenerativeArt_M4E5_P4K_R1_COMPLETO.zip`. Restano distinti QA artistico dell'utente, desktop/DPI, altri OS, Fase B/D e Crinis R10 in pausa. README solo descrizione/installazione/esecuzione con screenshot. **I blocchi delle versioni precedenti sotto sono archivio storico.**
> **HANDOFF CORRENTE VORONOI V1-R1 — 17/11/2026:** usare `GenerativeArt_M4E4_V1_R1_VORONOI_PROFONDO_COMPLETO.zip`. [Guida](M4E4_V1_VORONOI_PROFONDO.md). Voronoi layered: 2–8 piani con tessere mancanti, smussi, ombre gaussiane e blur crescente. 22 nuovi controlli JSON opzionali; flat mantiene percorso/8K e preset personali precedenti. Layered: massimo 8.388.608 pixel nativi, 1.400 celle totali, 1.600.000 unità geometriche più DepthLayerPolicy; non rimuovere i guard. **1.260 test Windows superati, build Release zero avvisi/errori**, sei PNG Full HD (due fine-2x), 182 ricette valide, Studio/CLI e quattro PNG precedenti identici verificati. 30 generatori e 30 file preset identici, inclusi i 17 personali. Screenshot Studio reale a risoluzione nativa. Nessuna modifica a Crinis/Cincinnus/G4 o alle fasi A/B/D. README solo descrizione/installazione/esecuzione con screenshot. Le note seguenti sono storia delle consegne.

> **HANDOFF CORRENTE C5-R1 — 17/11/2026:** usare `GenerativeArt_M4E3_CINCINNUS_C5_R1_FIX_VALIDAZIONE_COMPLETO.zip`. Validazione completa Windows eseguita da Codex: clean/restore/build Release, zero avvisi/errori, **1.225 test superati**; tool autonomo compilato. Risolti xUnit2012, conteggio degli otto colori e mappa Auto Cincinnus. Studio headless: nove campioni in sei combinazioni, ripristino parametri; 176 ricette valide e tre PNG CLI C5 decodificati. [Rapporto](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). Catalogo 30, embedded 30, esempi 176, preset personali 17 invariati. Non confondere verifica tecnica con approvazione estetica; Crinis/G4 e fasi A/B/D mantengono le priorità. README solo descrizione/installazione/esecuzione con screenshot; risultati e storia nei documenti separati. Le note di handoff precedenti sotto sono storiche.

> **STATO OPERATIVO C5 — 17/11/2026:** Cincinnus integrato *sperimentalmente* come 30° generatore nel catalogo/Studio/CLI, 30 preset embedded e 176 esempi JSON (173 storici + 3 C5). Supporta Simple/Adaptive, otto pigmenti modificabili singolarmente, tre palette, parametri/ombre e budget 2048 px / 2.097.152 pixel. **C1 e C2 test/build Windows confermati; C3, C4 e C5 non ancora compilati/testati dall'utente**, nessun giudizio estetico PNG. Leggere [scheda C5](CINCINNUS_C5_STUDIO_CLI_JSON_2026-11-17.md) e [QA](validation/CINCINNUS_C5_STATIC_QA.md). Le intestazioni C4/C3 seguenti sono storiche.

> **HANDOFF C4 — 17/11/2026:** ripartire da `GenerativeArt_M4E3_CINCINNUS_C4_ADAPTIVE_COMPLETO.zip`, NON da C3. C1 e C2 **confermate con build/test Windows**; C3 e nuova C4 **non ancora testate** in .NET né valutate tramite PNG. C4 aggiunge `CincinnusAdaptive{Options,Layout,Placer}` e `CincinnusCoverageGrid`, con test `CincinnusAdaptivePlacementTests` e modalità `adaptive` nel preview. Leggere [scheda C4](CINCINNUS_C4_ADAPTIVE_2026-11-17.md), [QA](validation/CINCINNUS_C4_STATIC_QA.md). Gate PowerShell 7: `clear && dotnet clean && .\tools\validate.ps1`, più build tool separata e A/B Simple/Adaptive. Poi **C5 integrazione catalogo/Studio/CLI/JSON e QA artistico**, da iniziare soltanto dopo il gate. Catalogo 29/29/173 invariato. Il testo C3 sotto è storico.

> **HANDOFF OPERATIVO C3 — 17/11/2026.** ZIP integrale: `GenerativeArt_M4E3_CINCINNUS_C3_PENNELLO_VOLUMETRICO_COMPLETO.zip`. **C1+C2 validati su Windows dall'utente; C3 nuovo e da validare.** Implementati: Algorithms `CincinnusStroke*`, SKIA `CincinnusBristlePainter` / `CincinnusPaintOptions`, unit test e strumento autonomo `tools/GenerativeArt.CincinnusPreview` per PNG (progetto separato dalla solution). Leggere [scheda C3](CINCINNUS_C3_PENNELLO_VOLUMETRICO_2026-11-17.md). Test Windows: `clear && dotnet clean && .\tools\validate.ps1`, poi compilare e usare il tool preview separato. Prossimo C4 Adaptive main/filler/patch; poi C5 integrazione Studio/CLI/JSON. C3 non cambia il catalogo 29/29/173 né Crinis/G4. Il testo C2/C1 sotto è cronologia.

> **HANDOFF OPERATIVO C2 — 17/11/2026.** ZIP da usare: `GenerativeArt_M4E3_CINCINNUS_C2_STREAMLINES_SIMPLE_COMPLETO.zip`. **C1: compilazione e test superati su Windows** per conferma dell’utente del 17/11. **C2: codice/test nuovi, non ancora eseguiti qui.** Leggere [C2](CINCINNUS_C2_STREAMLINES_SIMPLE_2026-11-17.md), [QA C2](validation/CINCINNUS_C2_STATIC_QA.md). Dopo estrazione, eseguire `clear && dotnet clean && .\tools\validate.ps1` in PowerShell 7. Prossimo: C3 pennellate e prime immagini, mantenendo Crinis R10 e G4-R2 invariati. Vecchi handoff C1/H3/H2/H1 sotto sono storici.

> **NUOVO HANDOFF C1 — 17/11/2026.** Usare lo ZIP `GenerativeArt_M4E3_CINCINNUS_C1_KERNEL_GEOMETRICO_COMPLETO.zip`. Sorgenti nuovi: `src/GenerativeArt.Algorithms/Cincinnus/{CincinnusFlowField,CincinnusTraceOptions,CincinnusPathGeometry}.cs`; test `tests/GenerativeArt.Algorithms.Tests/CincinnusGeometryTests.cs`; documenti [C1](CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md) e [verifica statica](validation/CINCINNUS_C1_STATIC_QA.md). **Gate Windows ancora aperto**: eseguire `clear && dotnet clean && .\tools\validate.ps1` e poi, facoltativamente, filtro `FullyQualifiedName~CincinnusGeometryTests`. Ultimo codice confermato build/test verdi: **G4-R2**, non attribuire lo stesso esito automaticamente a C1. C1 non è nel catalogo e non produce PNG; **C2** prossimo sviluppo geometrico. Test/ricette/Crinis/G4 storici preservati. Le istruzioni H3 e H2 sotto riportate sono cronologia dei passaggi precedenti.

# Handoff operativo — GenerativeArt Studio G4-R2-H1

**Aggiornamento H3 — Cincinnus (studio, non porting):** leggere [dossier Cincinnus](STUDIO_CINCINNUS_GART_2026-11-17.md) prima di intervenire su Crinis o sul nuovo generatore. Due sorgenti originali diversi (`Cincinnus.kt`, `CincinnusSimple.kt`), nessun PNG originale Cincinnus nello ZIP sorgente, nessuna modifica C# in H3. Mantenere la baseline G4-R2 già validata Windows e non interpretare la richiesta di studio come approvazione della sostituzione di G4-R3/G5.


**Aggiornamento H2 — fonti artistiche:** l’utente ha fornito `gart-main-pulito(2).zip` e il repository <https://github.com/igr/gart> per le immagini. Consultare [`GART_RIFERIMENTI_ORIGINALI_2026-11-17.md`](GART_RIFERIMENTI_ORIGINALI_2026-11-17.md) per il confronto G4-R1/R2/Gȧrt, i riferimenti originali e i candidati G5. H2 non tocca il codice R2 validato; lo ZIP sorgente Gȧrt è **un allegato separato** e non è incluso nel nostro ZIP C#.

**17 novembre 2026.** Questo documento è quello da leggere per primo nella nuova chat. Il codice non è cambiato rispetto a **M4E.3-G4-R2, compilata e testata con successo su Windows dall'utente**. Il suffisso H1 identifica esclusivamente un riallineamento documentale.

## Materiale da trasferire

- ZIP autosufficiente `GenerativeArt_M4E3_G4_R2_H1_PASSAGGIO_CHAT_COMPLETO.zip`: include solution, progetti, test, defaults, 173 esempi, licenze, riferimenti numerici, workflow, `tools/validate.ps1`, documentazione e manifest.
- Opzionale: `docs/NUOVA_CHAT.md`, testo pronto da incollare nella nuova chat insieme allo ZIP.
- **Non è necessario allegare versioni vecchie** per proseguire: servono solo per studi storici o confronti intenzionali. Per la fedeltà visiva Gȧrt si potranno allegare in seguito render di riferimento, se disponibili.

## Ordine di lettura nella nuova chat

1. [Stato corrente](CURRENT_STATE.md): versione validata, ambiti non ancora verificati.
2. [Decisioni consolidate](DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md): **vincoli e richieste dell'utente**, inclusi cambi di priorità.
3. [Roadmap](ROADMAP.md): priorità aggiornata e cronologia.
4. [Audit del pacchetto](AUDIT_HANDOFF_G4_R2.md): inventario, file invariati, problemi documentali risolti e debiti residui.
5. [Tecnica G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md) e [G4-R1](M4E3_G4_OCELLUS_RETE.md): algoritmo e test.
6. [Stato Crinis R10](M4E3_G3_R10_CRINIS_PROFONDITA_INTERNA.md) soltanto se si torna a Crinis.

## Decisioni vincolanti

- Stack **C#/.NET 10, Avalonia, SkiaSharp**; solo PowerShell minimo per operazioni; XAML/AXAML **consentiti**. Nessun Python/Kotlin/Go/Java o altra toolchain eseguibile nel prodotto senza autorizzazione.
- Un solo motore riusato da Studio e CLI; generazione deterministica via ricetta JSON versionata, palette e colori editabili, seed riproducibile.
- Per statici, **preview nativa = risoluzione finale**; Adatta/100% solo viewport. Niente preview ridotta mascherata da finale.
- Preservare **17 ricette personali di default**, i JSON storici e la GUI/UX approvata; menu a larghezza uniforme, interfaccia massimizzata, parametri condizionali, colori individuali, input numerico fine.
- Rendering grandi dimensioni **solo dove i singoli budget lo permettono**; non rimuovere limiti senza profiling/test, e non promettere 8192×8192.
- Fase **A** (consolidamento/qualità/catalogo), poi **B** (compositore universale), poi **D** (workflow artista con seed explorer, A/B, galleria, contact sheet, preset e SVG vero selettivo). Animazioni *avanzate* rinviate; non perdere il runtime animato esistente.
- Continuare con **nuovi algoritmi e ricette**. **Crinis R10 è in pausa**, non abbandonato né artisticamente completato. Prossima famiglia G4-R3 o G5 da scegliere dopo QA visivo.
- A ogni consegna: **ZIP completo, documentazione aggiornata, comandi Windows in italiano, stato test realistico, dove siamo/cosa resta**. Non fermarsi perché l'utente non può testare temporaneamente.

## Baseline di lavoro e test

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

L'utente ha confermato **G4-R2 compila e test passano**. Il conteggio non è stato comunicato. Questo pacchetto H1 modifica *solo* documenti/manifest e non richiede di ripetere test .NET per attribuire la validazione alla stessa base di codice. La valutazione visiva G4-R2 resta aperta.

## Percorsi principali

- `src/GenerativeArt.Application/Catalog/` — descrittori, parametri e colori; `Defaults/` — 29 preset embedded.
- `src/GenerativeArt.Algorithms/Geometry/OcellusField.cs` e `ReteFoamField.cs` — campi e geometria.
- `src/GenerativeArt.Rendering.Skia/GartCellularRenderer.cs` — rendering Ocellus/Rete.
- `src/GenerativeArt.Studio/StudioWindow*.cs` — Studio (partial + controller non ancora integralmente estratti).
- `tests/GenerativeArt.Algorithms.Tests/GartG4GeometryTests.cs`, `tests/GenerativeArt.Application.Tests/GartG4CatalogTests.cs`, `tests/GenerativeArt.Rendering.Skia.Tests/GartG4RenderingTests.cs` — G4 QA.
- `examples/recipes/m4e3-g4/`, `examples/recipes/m4e3-g4-r2/` — confronti G4-R1 e R2; `examples/recipes/m4e3-g3-r10/` — studi Crinis.
- `docs/validation/` — evidenze test storiche, static check G4-R2; non confondere questi report con la conferma dell'utente in chat.

## Primo lavoro consigliato

**Prima di modificare gli algoritmi**, confrontare gli esempi G4-R1 e G4-R2 a 1920×1080 (seed invariato) e annotare su quale effetto intervenire. Se si vuole diversificare il catalogo senza una lunga rifinitura G4, partire invece dallo studio di **Nimbus/Pebble/Accretion/Glst** e pianificare un primo generatore per volta, mantenendo test, source reference e preset.

### Vincoli e debiti non chiusi

A0: storiche anteprime `examples/previews` assenti; A1: lifecycle/controller UI; A4: generalizzazione 8K e profiling; A5: QA visivo/desktop e altre piattaforme; G4: microfori euristici e membrane non ancora fedeli come modelli geometrici/fisici; licenza pubblica e packaging finale da decidere.

**Archivio storico handoff precedente:** [HANDOFF_STORICO_FINO_A_G4_R2.md](HANDOFF_STORICO_FINO_A_G4_R2.md). Le istruzioni «usa R1.2» contenute lì sono **superate**.
