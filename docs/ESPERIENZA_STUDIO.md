# GenerativeArt Studio — Esperienza utente prevista

**Stato al 16 novembre 2026:** Studio M3D.1-R3 **compilato, testato e pan/zoom approvati su Windows**; M3D.2-R1 aggiunge export PNG cancellabile, progress indeterminato ed editor numerico preciso, ma attende il gate Windows. XAML/AXAML è consentito, la versione attuale usa C# code-only. Le funzioni ulteriori descritte sotto (varianti, editor avanzato, timeline professionale, export completo animazioni) restano requisiti futuri.

## 1. Struttura iniziale della finestra

- **Barra superiore**: Nuova opera, Apri ricetta, Salva ricetta, Esporta, coda dei lavori, preferenze e stato rendering.
- **Colonna sinistra — catalogo**: filtro testuale, categorie, tag, supporto per statico/animato/vettoriale e miniature. Selezione sempre visibile.
- **Area centrale — canvas**: anteprima ridimensionabile con pan/zoom, sfondo a scacchiera per trasparenza, pulsanti zoom, anteprima/alta qualità e stato.
- **Colonna destra — parametri**: seed con blocco/randomizza, dimensioni, palette, gruppi coerenti di slider, campi numerici, toggle, colore; reset singolo e globale, errori di validazione vicino al controllo.
- **Pannello inferiore — varianti**: miniature, ordinamento per seed/parametri, confronto a due, selezione preferiti, dettaglio ricetta e stato di ogni job.

Prima realizzare una singola finestra leggibile e funzionante; non introdurre docking, workspace multipli o pannelli trascinabili finché non emergano necessità reali.

## 2. Percorsi principali

### A. Prima immagine
Apri Studio → catalogo → scegli opera → preview → cambia seed → esporta PNG. Nessuna conoscenza del linguaggio richiesta.

### B. Riproduzione
Apri ricetta `.json` → verifica generatore/versione/parametri → preview → render finale. Una ricetta non riconosciuta mostra la ragione del rifiuto, senza degradare silenziosamente a default.

### C. Sweep
Scegli 1–2 parametri o seed → imposta intervalli e numero massimo combinazioni → visualizza stima dei job/risorse → avvia batch con progressione → confronto miniature → conserva selezioni. Nessuna espansione eager del prodotto cartesiano.

### D. Animazione (playback e controllo fase in M3D.1; export avanzato successivo)
Seleziona artwork con supporto timeline → imposta FPS, durata, loop e campionamento temporale → preview a bassa qualità → esporta usando frame sink progressivo, stop/cancel e pulizia sicura dei file temporanei.

### E. Disegno vettoriale (fase successiva)
La GUI mostra il supporto nativo SVG/PDF dell'opera; in sua assenza permette raster con etichetta esplicita, **senza** presentarne il risultato come file vettoriale reale.

## 3. Convenzioni di interazione

- Modifica dei parametri con debounce e cancellazione delle preview obsolete.
- Rendering finale avviato esplicitamente; non rigenerare 4K a ogni movimento dello slider.
- Indicazione dei parametri cambiati rispetto ai default e delle condizioni che invalidano la ricetta.
- Preset ripristinabili, undo/redo inizialmente per modifica dei parametri; cronologia delle generazioni distinta dalla cronologia editor.
- Accessibilità da tastiera, contrasto chiaro/scuro e etichette traducibili; italiano come lingua iniziale, localizzazione futura prevista nei testi (no stringhe sparpagliate nelle view).
- Gestione errori con informazioni utilizzabili: invalid parameter, memory budget exceeded, output format unsupported, native dependency missing.

## 4. Regole di performance in UI

- Il thread UI non esegue simulazioni o accesso pixel intensivo.
- Anteprima a budget ridotto, job seriali o con concorrenza limitata e `CancellationToken` effettivo.
- Non mostrare anteprime concluse ma ormai obsolete; ogni preview è legata alla revisione corrente dei parametri.
- Le risorse grafiche native e le bitmap delle miniature hanno durata e cache controllate.

## 5. Criteri di accettazione della prima versione Desktop

1. Selezione di tre generatori diversi senza riavviare l'applicazione.
2. Editor di parametri derivato da schema, non scritto a mano per ognuna delle tre opere.
3. Due generazioni uguali (stesso seed e ricetta) con risultati coerenti nei limiti del renderer.
4. Annullamento di una preview lunga senza bloccare la finestra.
5. Esportazione PNG e salvataggio/riapertura della stessa ricetta.
6. Nessun XAML o JavaScript nella distribuzione applicativa.
