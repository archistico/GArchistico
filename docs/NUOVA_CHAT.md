# Nuova chat — GenerativeArt Studio M4D.2-R1

Allegare `GenerativeArt_M4D2_R1_PITTURA_EVOLUZIONE_COMPLETO.zip` e scrivere:

---

Continuiamo da **M4D.2-R1**. Leggi `docs/CURRENT_STATE.md`, `docs/HANDOFF.md`, `docs/ROADMAP.md` e `docs/M4D2_PITTURA_EVOLUZIONE.md`.

C#/.NET 10, Avalonia/SkiaSharp; niente Go/Kotlin/Java/Python nell'applicazione. **Build Release senza avvisi/errori e 733/733 test Windows (Codex, SDK 10.0.201)**. Catalogo a 15: nuovi Pennelli nel flusso e Paesaggi a strati; Curve parametriche, Noise Flow, Plasma, Streamlines e Flow Loop ampliati. Quattordici ricette Full HD verificate (63 totali), 12 fotogrammi CLI distinti, smoke Studio e confronto prima/dopo. I 13 PNG dei generatori precedenti restano identici con i parametri storici.

L'utente vuole **nuovi algoritmi, dettaglio e controllo**, e vuole rendere più ricchi anche i primi generatori. I nuovi lavori dello Studio usano `StudioRecipeDefaults.ParametersFor`; il ripristino JSON conserva i valori caricati e il catalogo mantiene i default storici. Nuovi parametri sempre facoltativi nei JSON precedenti. Preservare seed, ID/versioni, alpha, API CLI/Studio e budget. Consegnare **ZIP completo** con codice, test, ricette, anteprime, licenze e documentazione.

**README:** solo descrizione del progetto, installazione ed esecuzione, con screenshot sotto il titolo. Non reinserire changelog, roadmap, numeri dei test o note di consegna nel README; usare i documenti separati.

Prossimo: compositore generale M4C.3, nuovi campi/noise ciclici M4B.2, pennelli e materia più avanzati. Le sovrapposizioni del paesaggio sono proprie del renderer, non un compositore generale già realizzato. Desktop/DPI reali, nuovi poster 4K e altri OS restano da verificare.

Validazione: `clear && dotnet clean && .\tools\validate.ps1`.
Smoke: `dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- evolution-studio .`, `evolution .` per i render e `compare .` per il confronto dei cinque generatori.

---
