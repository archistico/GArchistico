# Nuova chat — GenerativeArt Studio M4D.3-R1

Allegare `GenerativeArt_M4D3_R1_PROFONDITA_COMPLETO.zip` e scrivere:

---

Continuiamo da **M4D.3-R1**. Leggi `docs/CURRENT_STATE.md`, `docs/HANDOFF.md`, `docs/ROADMAP.md` e `docs/M4D3_PROFONDITA.md`.

C#/.NET 10, Avalonia/SkiaSharp; niente Go/Kotlin/Java/Python nell'applicazione. **Build Release senza avvisi/errori e 801/801 test Windows** (Codex, SDK 10.0.201). Catalogo a 17, 73 ricette: nuovi carta ritagliata/bokeh con ombre, piani e fuoco; paesaggi migliorati. Dieci ricette Full HD verificate, due fine-2x, smoke Studio e confronto effetti spenti/accesi. Quindici PNG precedenti identici a seed 42 / 160×120.

L'utente vuole **nuovi algoritmi/ricette con dettaglio, controllo e profondità tramite ombre, livelli e sfocati**, migliorando anche i primi generatori. `SkiaRasterSurface.DrawLayer`/`LayerEffects` sono già riutilizzati da tre generatori; M4C.3 resta aperta per editor livelli/maschere arbitrarie/blend. Nuovi lavori Studio con `StudioRecipeDefaults.ParametersFor`; ripristino JSON conservativo e default catalogo storici. Nuovi parametri nei vecchi artwork sempre opzionali. Preservare ID/versioni/schema v1/seed/alpha/CLI e budget; massimo livelli filtrati 8.388.608 pixel nativi e 800 milioni di unità raster. Nessun accumulo di frame o annidamento di livelli filtrati. Consegnare **ZIP integrale** con codice, test, ricette, anteprime, licenze e documentazione.

**README:** solo descrizione del progetto, installazione ed esecuzione, con screenshot sotto il titolo. Changelog, roadmap, numeri dei test e note di consegna nei documenti separati.

Proseguire con algoritmi ricchi, M4C.3 editor livelli/maschere/blend e M4B.2 noise ciclico/animazioni fluide. Desktop/DPI reali, nuovi poster 4K e altri OS restano da verificare.

Validazione: `clear && dotnet clean && .\tools\validate.ps1`.
Smoke: `dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- depth-studio .`; modalità `depth` per dieci render Full HD, `depth-compare` per il confronto, `depth-compat` per gli hash dei 15 default precedenti.

---