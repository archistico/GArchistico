# Audit di passaggio — GenerativeArt G4-R2 → H1 (17/11/2026)

**Scopo:** revisione approfondita della documentazione e dell'integrità del pacchetto per aprire una nuova chat. **Nessuna modifica funzionale:** tutti i `.cs`, `.csproj`, `.sln`, `.props`, `.ps1`, `.json`, `.png`, `.log`, `.csv`, workflow, immagini, licenze, golden e test sono conservati esattamente come nell'archivio G4-R2 ricevuto.

## 1. Baseline verificata

- L'utente conferma **G4-R2 compilata e con test passati** su Windows, il 17/11/2026; non è disponibile un log con il numero finale dei casi xUnit. Questa è un'attestazione della baseline di codice, **non** di una futura modifica.
- Nell'archivio R2 originale: **597 file**, inclusa la lista `MANIFEST_SHA256.txt`; **596 entry del manifest** confrontate con SHA-256 reale, **0 mismatch**. Il manifest si esclude da sé.
- Inventario codice: **250 file `.cs`**, **11 `.csproj`**, soluzione .NET 10: **6 progetti applicativi**, **4 progetti xUnit** e **1 progetto smoke** in `tools/GenerativeArt.Smoke/`. Nessun sorgente `.py`, `.go`, `.kt`, `.java`, `.js`, `.ts`, `.sh` nel prodotto.
- Inventario dati: **225 file JSON** nella baseline R2, inclusi **173** in `examples/recipes/`; tutte le **173 ricette di esempio** sono JSON sintatticamente valido e appartengono a **29 artwork ID** distinti. Presenti **29 default embedded**. Tutti i **29 ID** delle ricette hanno un default omonimo. La scansione dei **225 JSON** del repository, con lettura BOM-aware, non trova errori di parsing o chiavi duplicate; **un JSON storico di evidenza** (`docs/validation/M4D1_R1_compatibility.json`) ha un BOM UTF-8, non presente nelle 173 ricette di esempio.
- La precedente verifica R2 registra anche confronto **byte per byte** di tutte le **167** ricette preesistenti e dei **27** preset embedded estranei a Ocellus/Rete (vedi `docs/validation/M4E3_G4_R2_STATIC_VALIDATION.md`). È una dichiarazione della verifica R2 preesistente, non un nuovo test dinamico.
- Lo ZIP R2 originale è leggibile e integro. **Un nome di esempio con carattere accentato** risulta presentato con codifica CP437 errata da un lettore ZIP (`guilloche-moiré-1920.json`); l’estrazione conserva il nome Unicode previsto nei documenti e il **contenuto del file è identico**. Il nuovo ZIP H1 registra il nome in UTF-8 corretto. Questo H1 include un nuovo manifest aggiornato alla documentazione e un archivio ZIP nuovamente verificato.

## 2. Problemi documentali individuati e risolti

| Severità | Problema nella G4-R2 originaria | Correzione H1 |
|---|---|---|
| **Alta** | README, CURRENT_STATE, HANDOFF, NUOVA_CHAT, ROADMAP, TESTING e QUALITY_GATES continuavano a dire **«G4-R2 test non eseguiti»**. | Riallineamento alla **conferma positiva G4-R2** dell'utente, senza inventare numero test e lasciando aperto il QA artistico. |
| **Alta** | `NUOVA_CHAT.md` conteneva ripartenze multiple da G3/G4-R1.1/R1.2; la prima non era più l'ultima validata. | Nuovo testo unico breve, vecchia versione copiata in `NUOVA_CHAT_STORICO_FINO_A_G4_R2.md`. |
| **Alta** | `CURRENT_STATE` e `HANDOFF` mantenevano come attivi comandi e gate superati da revisioni successive. | Riscritti come documenti operativi correnti, copie storiche conservate senza cancellare informazioni. |
| **Media** | `docs/INDEX.md` apriva con molte dichiarazioni «revisione corrente» obsolete. | Ricostruito indice gerarchico con stato G4-R2 e rinvii allo storico. |
| **Media** | Quattro riferimenti a immagini e due link alla cartella **`examples/previews`** non presente nello ZIP, in `M4D2_PITTURA_EVOLUZIONE.md`, `M4D3_PROFONDITA.md` e `M4D4_SDF_MACCHIE_FUSIONI.md`. | Riferimenti visivi inesistenti segnalati esplicitamente e rimossi come link rotto; recupero delle anteprime lasciato aperto nel gate A0. |
| **Media** | Documenti progettuali precedenti contenevano ipotesi superate (preview ridotta, gate passati ancora marcati aperti). | Banner di precedenza verso lo stato attuale; sezioni storiche preservate per non cancellare provenienza e motivazioni. |
| **Bassa** | Metadato nome ZIP di `guilloche-moiré-1920.json` ambiguo in un lettore CP437. | Nuovo ZIP emesso con nome Unicode corretto; bytes JSON invariati. |
| **Bassa** | Registro decisioni non conteneva un riepilogo unitario delle scelte emerse in questa chat. | Creato `DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md` con stati vincolante/realizzato/pianificato/sospeso. |

## 3. Limiti rimasti (non corretti di nascosto)

1. **A0:** la cartella `examples/previews/` è assente nel pacchetto G4-R2 e nei vecchi ZIP esaminati (M4D4-R2/M4E1-R1). Il piano storico parla di **52 anteprime** da recuperare: il numero deriva dal piano, non da un inventario di file effettivamente trovati. Non è stato generato materiale fittizio.
2. **A1:** `StudioWindow*.cs` è già suddivisa in partial; servono ancora controller più indipendenti e regressioni lifecycle/DPI/annullamento.
3. **A2:** l'anteprima statica usa dimensioni native; la build/test G4-R2 è passata, ma il **confronto GUI manuale** di G4-R2 resta da documentare.
4. **A3/G4:** fori Ocellus basati sulle tre superfici più vicine, non su topologia flood-fill globale; nodi/membrane Rete modellati proceduralmente, non ottica fisicamente corretta. Fedeltà visiva da valutare con immagini e seed comparabili.
5. **A4:** 8K UHD solo quattro generatori geometrici in `standard`; nessun supporto 8192×8192 o full-catalogue, benchmark memoria/stress ancora aperti.
6. **A5:** desktop/DPI reali, Linux/macOS, performance ad alta densità, licenze packaging/release e confronto artistico; nessuna dichiarazione di gate chiuso senza evidenze.
7. **Crinis:** R10 in pausa; ultima richiesta estetica su profondità dentro lo stesso fascio rimane da verificare, nonostante la profondità tra fasci sia apprezzata.
8. **Roadmap:** B compositore, D workflow e nuova animazione non sono implementati da questo handoff.

## 4. Controlli finali H1 e come interpretarli

| Controllo | Criterio |
|---|---|
| Integrità ZIP | Archivio integro, unica cartella `GenerativeArt/` |
| Conservazione codice/ricette | SHA-256 identici a G4-R2 per ogni file **non documentale** |
| JSON | 173 ricette esempio parseabili, 29 ID generatori, 29 default embedded |
| Collegamenti Markdown | Nessun link locale rotto negli `.md` del progetto, incluse immagini |
| Manifest | Ricalcolato per tutta la nuova consegna, nessun mismatch |
| Esecuzione .NET | **Non rieseguita nell'operazione H1**: vale il riscontro dell'utente sulla baseline G4-R2, che H1 lascia invariata |
| QA visivo G4-R2 | **Ancora aperto**, non risolvibile tramite SHA o JSON |

## 5. File guida

- [README](../README.md), [CURRENT_STATE](CURRENT_STATE.md), [ROADMAP](ROADMAP.md), [HANDOFF](HANDOFF.md), [NUOVA_CHAT](NUOVA_CHAT.md).
- [Decisioni consolidate](DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md), [tecnica G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md), [storico correzioni](STORICO_CORREZIONI.md).
- [Indice completo](INDEX.md) e [`docs/validation`](validation/M4E3_G4_R2_STATIC_VALIDATION.md) per la tracciabilità.
