# Nuova chat — GenerativeArt Studio M4D.4-R1

Allegare `GenerativeArt_M4D4_R1_SDF_MACCHIE_FUSIONI_COMPLETO.zip` e scrivere:

---

Continuiamo da **M4D.4-R1**. Leggi `docs/CURRENT_STATE.md`, `docs/HANDOFF.md`, `docs/ROADMAP.md` e `docs/M4D4_SDF_MACCHIE_FUSIONI.md`.

C#/.NET 10, Avalonia/SkiaSharp; niente Go/Kotlin/Java/Python nell'applicazione. **Build Release senza avvisi/errori e 859/859 test Windows** (Codex, SDK 10.0.201). Catalogo a 17, 81 ricette. Orbite SDF riprogettato con macchie deformabili, quattro pigmenti/fusione, marbling, rilievo, piani e AA locale; stili pigment/glass, sei palette, 36 slider. Otto nuove ricette Full HD, due fine-2x, Studio/CLI e 15 hash PNG precedenti verificati. Confronto AA contro riferimento 4×: −71,3% errore alpha nel caso registrato.

L'utente vuole nuovi algoritmi/ricette ricchi di dettaglio, controllo e profondità; per SDF ha chiesto una revisione completa con macchie/fusione colore, complessità e migliore antialiasing. `PigmentSdfField` e `SdfPixelCoverage` sono indipendenti da Skia. Default catalogo SDF classic per JSON precedenti; nuovi lavori Studio pigment/sorbet via `StudioRecipeDefaults.ParametersFor`. Conservare ripristino, ID/versioni/schema v1/seed/alpha e budget. Nuovi controlli nei vecchi artwork sempre opzionali. SDF moderno: 260 milioni di valutazioni e massimo 8.388.608 pixel nativi più DepthLayerPolicy. Un PixelBuffer riutilizzato e tre righe di campioni; nessun accumulo di frame.

Base `DrawLayer`/`LayerEffects` già disponibile per ombre gaussiane, blur e SrcOver; M4C.3 resta aperta per editor livelli/maschere arbitrarie/blend, M4B.2 per noise ciclico/animazioni fluide. Consegnare **ZIP integrale** con codice, test, ricette, anteprime, licenze e documentazione.

**README:** solo descrizione, installazione ed esecuzione, screenshot sotto il titolo; changelog/roadmap/test/consegne nei file separati.

Validazione: `clear && dotnet clean && .\tools\validate.ps1`.
Smoke: `dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- sdf-studio .`; `sdf` per otto render Full HD, `sdf-compare` per i tre stili, `sdf-aa` per il riferimento 4×, `depth-compat` per gli hash precedenti.
Restano desktop/DPI reali, altri OS, nuovi poster 4K e animazioni lunghe da verificare.

---