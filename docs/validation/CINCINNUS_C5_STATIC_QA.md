> **Aggiornamento:** questo documento descrive il controllo statico originario. Il successivo **C5-R1 è stato compilato e testato realmente su Windows da Codex: 1.225 test superati, tre PNG CLI e Studio headless verificati**. Vedere il [rapporto Windows](../CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). Le diciture «non eseguiti» sotto si riferiscono alla preparazione precedente.

# Cincinnus C5 — rapporto QA statico (17/11/2026)

## Controlli eseguiti

- Archivio sorgente: estratto integralmente da C4; modifica circoscritta all'integrazione C5.
- Verifica JSON tramite `jq`: preset e tre nuove ricette sintatticamente corretti, nomi e versioni coerenti.
- Confronto file: verificare che 29 preset embedded precedenti, 173 esempi JSON precedenti e sorgenti dei renderer Crinis/G4 non differiscano da C4 (vedere inventario finale).
- Schema: descriptor distinto con ID stabile; UI usa catalogo e preset embedded; runtime usa il renderer condiviso con la CLI.
- Nuove verifiche predisposte: `CincinnusC5CatalogTests`, `CincinnusC5RenderingTests`. Inoltre i test esistenti con attesa catalogo 29 sono aggiornati a 30.
- Vincoli: 256..2048 px, cap 2.097.152 pixel, fine-2x limitato; renderer non attiva 8K.

## Non eseguiti e non attestabili

- `dotnet build`/`dotnet test` (SDK .NET non presente nell'ambiente di preparazione).
- PNG di output C5 e giudizio artistico comparato; il codice può richiedere correzioni al primo gate Windows.
- Stress memoria, preview GUI reale e packaging multipiattaforma.

**Stato:** C1+C2 Windows validate; C3+C4+C5 implementate ma non validate Windows. Il controllo statico non sostituisce i gate runtime.

## Riscontro concreto del confronto C4→C5

Controllo `diff -qr` del pacchetto C4 estratto contro C5:

- `CrinisMicroRenderer.cs` e `GartCellularRenderer.cs`: hash SHA-256 identici al pacchetto precedente.
- `src/GenerativeArt.Application/Defaults/`: **1 file soltanto aggiunto**, i 29 vecchi JSON sono identici.
- `examples/recipes/`: **1 cartella soltanto aggiunta**, i 173 vecchi esempi sono identici.
- Totali JSON parsati con `jq`: **176** esempi e **30** preset embedded, senza errori sintattici.
- Le modifiche al runtime condiviso sono circoscritte al nuovo `case "cincinnus-bristles"`, al metodo canvas per il painter C3 e all'override di sfondo opzionale.
- I test sui conteggi del catalogo storico sono allineati a 30, con elenco esatto dei nuovi ID.

- Controllo aggiuntivo: tutti i **229 file JSON** presenti nell'intero albero del progetto sono stati analizzati sintatticamente tramite `jq` senza errori (206 tra preset ed esempi, altri 23 JSON di progetto/documentazione).
- Confronto statico dello schema C5: **21 parametri obbligatori** presenti in preset e in tutti e tre gli esempi; gli override di colore e i controlli standard di finitura sono opzionali.
