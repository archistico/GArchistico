> **STATO CORRENTE M4E.4-V1-R1 — 17/11/2026:** Mosaico Voronoi ampliato con modalità `layered`, tessere mancanti, mosaici posteriori indipendenti, smussi/luce, ombre gaussiane, distanza e sfocatura. **Build Release zero avvisi/errori e 1.260/1.260 test Windows** (Codex, SDK 10.0.201: Core 115, Algorithms 254, Application 220, Rendering.Skia 671). 30 generatori, 30 preset incorporati e **182 ricette**, sei nuove Full HD, due fine-2x. PNG storici Voronoi identici in quattro casi; tutti i 30 file preset, inclusi i 17 personali, invariati. Editor/CLI e anteprima nativa verificati; 8K conservato per flat, non per il percorso filtrato. [Guida, confronto e verifiche](M4E4_V1_VORONOI_PROFONDO.md). Le note C5 e precedenti sotto sono storiche; restano aperti giudizio artistico dell'utente, desktop/DPI, altri OS e stress. Crinis/Cincinnus e G4 non modificati da questo incremento.

> **VALIDAZIONE WINDOWS C5-R1 — 17/11/2026:** corretti tre problemi riprodotti: xUnit2012 nel test del pittore, conteggio dei pigmenti che includeva pigment-granulation e campioni Auto Cincinnus mancanti. **Build Release zero avvisi/errori e 1.225/1.225 test superati da Codex, SDK 10.0.201** (Core 115, Algorithms 249, Application 219, Rendering.Skia 642); gate rapido 21 test già inclusi nel totale. Tool CincinnusPreview compilato; 176 ricette validate, tre PNG CLI 768×768 decodificati e Studio headless verificato su sei combinazioni palette/modalità. Catalogo/preset/esempi 30/30/176. [Rapporto e log](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). Qualità artistica Cincinnus/G4, desktop/DPI e stress ai limiti ancora aperti; Crinis R10 in pausa. **Le note di validazione C5/C4/C3 precedenti sotto sono storiche.**

> **STATO OPERATIVO C5 — 17/11/2026:** Cincinnus integrato *sperimentalmente* come 30° generatore nel catalogo/Studio/CLI, 30 preset embedded e 176 esempi JSON (173 storici + 3 C5). Supporta Simple/Adaptive, otto pigmenti modificabili singolarmente, tre palette, parametri/ombre e budget 2048 px / 2.097.152 pixel. **C1 e C2 test/build Windows confermati; C3, C4 e C5 non ancora compilati/testati dall'utente**, nessun giudizio estetico PNG. Leggere [scheda C5](CINCINNUS_C5_STUDIO_CLI_JSON_2026-11-17.md) e [QA](validation/CINCINNUS_C5_STATIC_QA.md). Le intestazioni C4/C3 seguenti sono storiche.

> **STATO OPERATIVO C4 — 17/11/2026:** partendo da C3 è stato aggiunto il posizionamento **Cincinnus Adaptive**, con griglia di copertura, main/filler, ritaglio uncini, seconda griglia post-ritaglio e patch, nuova modalità PNG nel tool autonomo e test dedicati. **C1 e C2 build/test confermate Windows; C3 e C4 NON ancora confermate Windows, né valutate artisticamente.** Nessuna nuova registrazione Studio/CLI, invariati catalogo 29/29/173 e Crinis/G4. Leggere [C4](CINCINNUS_C4_ADAPTIVE_2026-11-17.md), [QA statico](validation/CINCINNUS_C4_STATIC_QA.md). Le note anteriori sono storiche.

> **STATO OPERATIVO C3 — 17/11/2026:** C1 e C2 **compilate e testate con successo su Windows per conferma dell'utente** (conteggio non disponibile). C3 ora implementa il pianificatore delle pennellate, il rendering Skia autonomo e una CLI di prova che salva PNG senza registrare un nuovo artwork. **C3 da compilare/testare e valutare visivamente**; non è approvata artisticamente. [Scheda C3](CINCINNUS_C3_PENNELLO_VOLUMETRICO_2026-11-17.md), [QA statico](validation/CINCINNUS_C3_STATIC_QA.md). Catalogo 29/29/173, Crinis R10 e G4-R2 invariati. I blocchi C2/C1 sotto sono storico.

> **STATO OPERATIVO C2 — 17/11/2026.** Baseline Windows **C1 confermata compilata/testata dall’utente** (conteggio test non comunicato). La nuova **C2 è implementata ma non ancora compilata/testata su Windows**: 3 sorgenti C# aggiunti al progetto Algorithms, 1 file xUnit aggiunto, distribuzione Simple con griglia spaziale e cutoff, ordinamento bidirezionale, clearance/taper e gestione limiti. Vedi [scheda C2](CINCINNUS_C2_STREAMLINES_SIMPLE_2026-11-17.md) e [QA statico](validation/CINCINNUS_C2_STATIC_QA.md). **Nessun rendering Cincinnus**; catalogo 29, preset 29, ricette 173; Crinis R10 e G4-R2 invariati. Le vecchie formule «C1 ancora da testare» sotto sono *storiche*.

> **AGGIORNAMENTO OPERATIVO C1 — 17/11/2026.** Codice corrente: **M4E.3-Cincinnus-C1**, evoluzione additiva di G4-R2-H3. Nuovi file `src/GenerativeArt.Algorithms/Cincinnus/*` e `tests/GenerativeArt.Algorithms.Tests/CincinnusGeometryTests.cs`: vortici fuori tela maximin, somma campo, tracing RK4 con limiti/cancellazione, ritorni e curvature. **Non è ancora un renderer**; 29 generatori/29 preset/173 ricette, 17 personali, UI e Crinis R10 immutati. **C1: build e test .NET NON eseguiti nell'ambiente di preparazione (SDK assente), in attesa del gate Windows.** L'ultima baseline con build/test superati confermati dall'utente resta **G4-R2**. Leggere [C1](CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md). Il testo successivo documenta la baseline precedente e va interpretato storicamente.

# Stato attuale — GenerativeArt Studio (17 novembre 2026)

**Nota H3 — studio Cincinnus:** il 17/11/2026 sono stati analizzati i due sorgenti originali di Cincinnus e le dipendenze di Gȧrt. Documento operativo: [STUDIO_CINCINNUS_GART_2026-11-17.md](STUDIO_CINCINNUS_GART_2026-11-17.md). Cincinnus **non è presente nel catalogo**: restano **29 generatori / 29 embedded / 173 ricette**; Crinis R10 resta in pausa. H3 non modifica codice o test G4-R2. Non è disponibile nel pacchetto originale una PNG autenticata di Cincinnus, perciò non attribuire alcuna approvazione estetica.


**Aggiornamento H2 (solo documentazione, 17/11/2026):** ricevuto e ispezionato anche lo ZIP sorgente Gȧrt; verificata la galleria immagini sul repository ufficiale. La [mappa delle 28 opere e dei sorgenti](GART_RIFERIMENTI_ORIGINALI_2026-11-17.md) supporta il confronto estetico G4-QA e la scelta successiva G4-R3/G5. **La baseline C# e i risultati Windows rimangono invariati.**

**Versione codice:** `M4E.3-G4-R2` · **Consegna documentale:** `G4-R2-H1` (stesso codice).

## Stato della validazione

| Ambito | Stato verificabile | Fonte/limite |
|---|---|---|
| Build Release della G4-R2 su Windows | **Passata, confermata dall'utente** | Messaggio utente del 17/11/2026: «compila e test ok» |
| Suite .NET/xUnit G4-R2 su Windows | **Passata, confermata dall'utente** | Conteggio non comunicato; non inventarlo |
| Verifica statica dell'archivio R2 | **Passata** | 173 JSON di esempio validi; 596 file nel manifest originale senza mismatch |
| Smoke artistico delle sei ricette G4-R2 | **Da documentare** | Nessun A/B Full HD Ocellus/Rete confermato |
| Identità preview nativa/export G4-R2 | **Coperta da test; smoke GUI specifico da documentare** | Non confondere test automatizzati con ispezione manuale |
| Linux/macOS, stress 8K, memoria GPU/CPU | **Non attestato su questa baseline** | Richiedono prove specifiche |
| Handoff G4-R2-H1 | **Solo documentazione** | Sorgenti, test, ricette e preset identici alla G4-R2 validata |

## Inventario

- **29** artwork/generatori registrati e **29** preset in `src/GenerativeArt.Application/Defaults/`.
- **173** ricette in `examples/recipes/`; gli otto studi G4-R1 e i sei G4-R2 restano disponibili.
- **17** preset incorporati provenienti dai JSON originari dell'utente, con seed/parametri/animazione propri. I 2 preset nuovi Ocellus/Rete sono stati modificati di proposito tra G4-R1.2 e G4-R2; i restanti 27 embedded invariati nella verifica R2.
- Sei progetti applicativi (`Core`, `Algorithms`, `Application`, `Rendering.Skia`, `Cli`, `Studio`); quattro progetti di test, soluzione `GenerativeArt.sln`.

## Risultati tecnici ora presenti

- Architettura C#/.NET 10: motore e geometrie testabili senza UI; rendering Skia condiviso tra CLI e Studio; UI Avalonia.
- Studio: avvio massimizzato, selezione/catalogo e pulsanti a larghezza uniforme, parametri e colori individuali, palette, campi condizionali, pan/zoom, anteprima statica calcolata a dimensioni finali, import/export JSON, PNG, preferenze e temi; `StudioWindow` ripartita in partial, ma refactoring A1 **non concluso**.
- Ricette JSON v1 con validazione rigorosa, versioni algoritmi e seed espliciti, compatibilità dei parametri opzionali, export con pubblicazione atomica e cancellazione.
- Qualità: standard e `fine-2x` nei budget ammessi; grana/dithering, materia carta e pigmenti, alpha; 8K UHD solo per i quattro renderer geometrici abilitati, non 8K universale.
- Crinis R10: sottostrati interni, filamenti evidenziati, illuminazione trasversale e ombre interne; **qualità artistica non convalidata come definitiva** e sviluppo sospeso per decisione successiva.
- G4-R2: `ocellus-eyespots` con fori euristici/luce di bordo e `rete-foam` con giunzioni, bagliore nodi e pellicola interna; otto controlli opzionali R2, vecchi JSON neutri.

## Attività aperte e ordine

1. **G4-QA visivo**: confronti A/B delle sei ricette Full HD con i riferimenti G4-R1, stessi seed/dimensioni, vista `Adatta`/`100%`, alpha e export; registrare se le microcavità e le membrane sono artisticamente soddisfacenti.
2. **A0 packaging**: le **anteprime storiche `examples/previews/` non sono nel pacchetto**; recuperarle o rigenerarle con prove/attribuzioni (non inventare screenshot) e fare audit completo delle risorse.
3. **A1/A4/A5**: completare estrazione controller/lifecycle UI, stress controllato 4K/8K, benchmark risorse e gate cross-platform quando disponibili.
4. **A3**: scegliere il prossimo lavoro artistico, fra G4-R3 (topologia cavità Ocellus, curvatura/tensione pellicole Rete) oppure G5 (Nimbus, Pebble/Pebble2, Accretion, Glst/Glst2) **senza dichiarare una scelta non ancora fatta**.
5. **Fase B**, poi **Fase D**; animazioni avanzate rinviate. Le capacità di animazione già implementate non vanno rimosse.

## Avvertenze per la nuova chat

- Non ripartire da G4-R1.2, G3-R10 o da blocchi storici di `ROADMAP.md`: **la baseline è G4-R2**.
- Non riaprire come bug attivi `Math.Hypot`/CS0117, `ink`/CS0136 o i test storici: le rispettive correzioni sono già presenti e la G4-R2 compila e passa i test Windows.
- Non affermare che G4-R2 ha già superato una prova estetica, una verifica Linux/macOS o una generazione reale 8K.
- Alla prossima modifica di codice: ZIP **integrale** e aggiornamento di README, CHANGELOG, ROADMAP, CURRENT_STATE, HANDOFF, NUOVA_CHAT, registro decisioni e verifiche.

Vedi [decisioni consolidate](DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md), [audit H1](AUDIT_HANDOFF_G4_R2.md), [handoff](HANDOFF.md).
