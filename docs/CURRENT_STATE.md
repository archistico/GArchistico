# Stato corrente — GenerativeArt Studio M4E.1-R3 (fix test dipendente dal campo)

**17 novembre 2026.** Ricevuta conferma Windows della versione **M4E.1-R2: compilazione riuscita, 879/880 test superati**, con una sola regressione `EveryArtisticControlChangesTheRaster` per `spaced-streamlines/frequency`. Il test partiva dal preset dell'utente con `field=vortex`, che non usa la frequenza. In R3 il test imposta `field=curl` prima di verificarne l'effetto; nuova regressione esplicita conferma che il vortice è indipendente dalla frequenza. **Codice di produzione, preset e 81 ricette invariati.** Test R3 ancora da confermare.

Prosegue Fase A: A0 archivio e anteprime storiche mancanti; A1 isolamento controller e UX dei parametri applicabili; A2 preview nativa implementata con QA GUI/DPI ancora aperto; A3 nuove ricette/algoritmi; A4 output 8K con budget e quality gate; A5 stabilizzazione. Poi Fase B compositore e Fase D workflow dell'artista, mentre le animazioni evolute restano rinviate. Dettagli in `M4E1_R3_FIX_TEST_STREAMLINES.md`.

---

# Stato precedente — GenerativeArt Studio M4E.1-R2 (A1 incrementale + A2 anteprima reale)

**17 novembre 2026.** La finestra Studio usa adesso il **raster nativo con le dimensioni della ricetta finale**, senza preview ridotta né campi di dimensionamento raster. `Adatta`, `100%`, rotella e riquadro configurabile operano solo sulla vista. `StudioRenderCoordinator` (Application, senza Avalonia) serializza preview ed export, gestisce i token e l'output PNG atomico; nuovi test unitari e rendering Skia confrontano i percorsi. Nessun algoritmo o JSON modificato: **17 generatori, 81 ricette di esempio, 17 preset embedded** conservati.

**Stato:** implementazione R2 pronta per validazione Windows, non ancora confermata. Ultima baseline Windows documentata **M4D.4-R1, 859/859 test**. A1 richiede ancora controller indipendenti e QA dispatcher; A2 richiede smoke GUI/DPI e verifica pixel; A0 immagini storiche mancanti, A3 nuovi algoritmi/ricette e A4 8K rimangono aperti. Output statico limitato per ora a 4096×4096 e al budget del renderer; qualità fine-2x ha limite più stretto. **Fase B compositore**, poi **Fase D workflow artista** (seed, varianti, confronto A/B, galleria, contact sheet, preset e SVG selettivo); animazioni evolute rinviate. Dettagli in `M4E1_R2_PREVIEW_NATIVA_COORDINATORE.md`.

---

# Stato corrente — GenerativeArt Studio M4E.1-R1 (A1 iniziale + roadmap D)

**17 novembre 2026.** La versione precedente M4D.4-R2 integra 17 JSON originali come preset Studio. Questa revisione avvia **A1**, ripartendo `StudioWindow.cs` da 1.103 a 189 linee più sette partial per responsabilità; `StudioRecipeFormParser` in Application isola la costruzione/validazione del modello dai controlli Avalonia e aggiunge test xUnit dedicati. GUI, renderer, JSON, seed, export e preview hanno intenzionalmente lo stesso comportamento della R2. **A1 è solo parziale**, il passo successivo sono controller indipendenti e verifiche lifecycle/headless.

**Fase D — Workflow dell'artista** approvata e inserita dopo il compositore Fase B: varianti/seed, confronto A/B, galleria, contact sheet, preset, SVG selettivo in seguito. **Animazioni rinviate**.

**Baseline Windows confermata:** M4D.4-R1, 859/859 test. R2 e M4E.1-R1 da confermare su Windows; non attribuire alla R1 nuove conferme. Nessuna implementazione A2 (anteprima a risoluzione finale), A3 (nuovi algoritmi), A4 (8K), Fase B o D. Rimane aperta l'incompletezza delle 52 immagini del vecchio pacchetto di origine. Documenti guida: `ROADMAP.md`, `M4E0_PIANO_A_B_DEFAULTS.md`, `M4E1_A1_WORKFLOW_D.md`, `HANDOFF.md`.

---

# Stato corrente — GenerativeArt Studio M4D.4-R2 (roadmap + default da allegati)

**17 novembre 2026.** Da `GenerativeArt.zip` M4D.4-R1 sono stati integrati **17 JSON allegati come preset embedded per i nuovi lavori Studio**, compresi il seed speciale di Domain Warp, colori, dimensioni e animazione di Flow Loop. `StudioRecipeDefaults.RecipeFor` sostituisce la vecchia tabella parziale di override; selezione Studio e test correlati aggiornati. **Non sono stati ancora sviluppati** il refactoring di `StudioWindow.cs`, l'anteprima nativa obbligatoria, l'8K o il compositore: sono **in roadmap Fase A/B**. Le animazioni vengono dopo.

**Baseline Windows verificata:** M4D.4-R1, **859/859 test**. **R2:** codice e documentazione aggiornati, test Windows non ancora ricevuti. Lo ZIP M4D.4-R1 di partenza era privo di **52 immagini** elencate nel manifesto: la loro ricostruzione resta attività A0; il nuovo manifesto registra soltanto i file realmente inclusi, senza dichiarare recuperate le anteprime.

**Ordine successivo:** A0 integrità distribuzione; A1 refactoring StudioWindow; A2 preview 1:1; A3 nuovi algoritmi/ricette; A4 8K; A5 QA; **B compositore universale**. Dettagli: [`M4E0_PIANO_A_B_DEFAULTS.md`](M4E0_PIANO_A_B_DEFAULTS.md), [`ROADMAP.md`](ROADMAP.md).

---

# Stato corrente — GenerativeArt Studio M4D.4-R1

**Data:** 16 novembre 2026. **M4D.4-R1 — Orbite SDF riprogettato:** macchie organiche, fusione di quattro pigmenti, venature, rilievo, piani e antialiasing locale. **17 generatori, 81 ricette, build Release senza avvisi/errori e 859/859 test Windows** (Codex, SDK 10.0.201). Otto nuovi PNG Full HD, due fine-2x; controlli Studio, JSON, export CLI e 15 PNG precedenti identici verificati. Confronto AA contro riferimento 4×: errore alpha ridotto del 71,3% nel caso registrato. Guida [`M4D4_SDF_MACCHIE_FUSIONI.md`](M4D4_SDF_MACCHIE_FUSIONI.md), evidenze `docs/validation/M4D4_R1_*`. Desktop/DPI reali, altri OS, nuovi poster 4K ed editor livelli/maschere/blend restano aperti. Restore da cache NuGet locale, audit online e telemetria build disattivati.

## Stato incrementale

| Revisione | Stato funzionale | Stato Windows |
| --- | --- | --- |
| M0–M4A.4-R1 | Framework, Studio, nove generatori e controlli creativi iniziali | Confermato dall'utente |
| M4A.4-R2 | Ruoli colore, selettore HSV/RGB/alpha | Gate aperto |
| M4A.4-R3 | Supersampling 2× e PNG alpha-safe | Gate aperto |
| M4C.1-R1 | Grana seeded e dithering Bayer su nove generatori | Gate aperto |
| M4C.2-R1 | Carta cotone/ruvida, pigmenti granulari, assorbimento locale | **Build fallita su Windows: CS8506, CS8131** in `StudioColorValue.cs` |
| **M4C.2-R2** | Fix switch HSV e avvio Studio massimizzato, sei test aggiunti | **Build OK, 506/507 test superati** (conferma utente) |
| **M4C.2-R3** | Correzione test JSON, finiture e Plasma 5 MP, raster/riquadro anteprima configurabili, Adatta/100% | **Build OK, 513/514 test (utente); test budget materia obsoleto** |
| **M4C.2-R4** | Test materia allineati ai budget standard/fine-2x | **Build Release OK, 521/521 test (Codex Windows)** |
| **M4B.1-R1** | Domain Warp, LIC Flow, sette ricette, streamlines dense, picker con footer fisso | **Build Release OK, 558/558 test; sette PNG Full HD e smoke picker headless (Codex Windows)** |
| **M4D.1-R1** | Guilloché/incisione, dettaglio Domain Warp, slider fini, otto ricette | **Build Release OK, 626/626 test; otto PNG Full HD, smoke Studio headless e compatibilità PNG (Codex Windows)** |
| **M4D.2-R1** | Pennelli/paesaggi e cinque generatori iniziali ampliati, quattordici ricette | **Build Release OK, 733/733 test; Full HD, 12 frame CLI, Studio e compatibilità PNG verificati (Codex Windows)** |
| **M4D.3-R1** | Carta ritagliata/bokeh, livelli filtrati riutilizzabili, paesaggi con profondità e dieci ricette | **Build Release OK, 801/801 test; dieci PNG Full HD, Studio e 15 hash PNG precedenti verificati (Codex Windows)** |
| **M4D.4-R1** | Orbite SDF: macchie/fusioni, stili pigment/glass, antialiasing locale e otto ricette | **Build Release OK, 859/859 test; Full HD/fine-2x, Studio, CLI, copertura AA e hash PNG precedenti verificati** |

## M4C.2-R3 — richieste emerse nella validazione Windows

1. `RecipeTests.MissingParametersAreRejected` manipolava testo JSON lasciando una trailing comma: ora il test rimuove realmente `scale` via `System.Text.Json.Nodes.JsonNode` e verifica il rifiuto semantico `ArgumentException`.
2. Budget finiture elevato da 4.194.304 a 16.777.216 pixel finali, con controllo prima del rendering. Rimosso il limite separato di Plasma 4.194.304; 2500×2000 (5.000.000 pixel) ora ammesso in `standard`, anche con carta/grana. `fine-2x` resta limitato a 16.777.216 sottopixel. Non sono aboliti i budget degli altri algoritmi o delle animazioni.
3. Studio: dimensione raster massima e dimensione riquadro in campi distinti, separati da larghezza/altezza output. `Adatta` usa lo zoom effettivo per mostrare tutti i bordi e `100%` usa scala 1:1; auto-fit al nuovo rendering e al resize. Barra di stato segnala dimensioni anteprima e output.
4. Regressioni aggiunte: JSON valido senza parametro richiesto, validazione Plasma 5 MP con finitura, fitting di varie proporzioni, preview raster 1:1 e preview limitata. Documentazione specifica: `M4C2_R3_PREVIEW_E_BUDGET.md`.

## M4C.2-R2 — bugfix e comportamento finestra

- Segnalazione utente dalla build della R1: `StudioColorValue.cs(54,44)` CS8506 e CS8131, entrambe causate dall'inferenza del tipo comune delle tuple nello `switch` HSV.
- Sostituiti gli zero `int` con `0d` in tutti e sei i rami; rimangono inalterate conversione numerica, formato HEX e alpha.
- In `StudioWindow` avvio `WindowState.Maximized` (finestra massimizzata, non borderless/kiosk). Sei nuovi casi di test HSV con tonalità intermedie.
- La build R2 è stata confermata; la suite R2 ha fallito 1/507 casi, corretto nel codice R3.

## M4C.2-R1 realizzata

1. Nuovo gruppo **MATERIA**: `paper-texture`, `paper-strength`, `pigment-granulation`, `ink-absorption` su tutti e nove i generatori, con default spenti e JSON v1 compatibile.
2. Motore `MaterialPostProcessor`: rumore interpolato a più scale/anisotropie, depositi cromatici guidati dalla luminanza, semplice assorbimento del colore scuro sui bordi; seed e coordinate fisse anche nei loop.
3. Preservazione alpha; assorbimento usa massimo tre righe originali, nessuna copia dell'intera immagine oltre al buffer di finitura già introdotto in M4C.1.
4. Integrazione nel post-processing comune dopo il campionamento 2× e prima di grana/Bayer; il ramo senza finitura resta invariato. Il limite storico 4.194.304 pixel delle finiture è stato ampliato a 16.777.216 nella R3.
5. Nuova suite `MaterialPostProcessorTests`, aggiornamento `CatalogTests`, cinque ricette Full HD, specifica `M4C2_MATERIA_CARTA_PIGMENTI.md` e documentazione operativa.

Le texture sono un'approssimazione raster procedurale, **non** un simulatore fisico di fluidi/pigmenti. Non ci sono nuovi pacchetti, linguaggi runtime o versioni di schema.

## Azione successiva

**Proseguire lo sviluppo**: nuovi algoritmi/ricette con dettaglio e profondità; estendere compositing a editor livelli/maschere/blend M4C.3, noise ciclico M4B.2 e animazioni fluide. SDF moderno ora disponibile in M4D.4; vecchie ricette SDF restano classic per compatibilità. Restano QA desktop/DPI, poster, altri OS e animazioni lunghe.

## Comandi Windows — PowerShell 7

Nella radice `GenerativeArt`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Per esempi, controlli dettagliati e aspettative vedere [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md), [`TESTING.md`](TESTING.md) e [`QUALITY_GATES.md`](QUALITY_GATES.md).
