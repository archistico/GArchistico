# Stato corrente — GenerativeArt Studio M4D.2-R1

**Data:** 16 novembre 2026. **M4D.2-R1: build Release senza avvisi/errori e 733/733 test superati da Codex su Windows, SDK 10.0.201.** Nuovi Pennelli nel flusso e Paesaggi a strati; Curve parametriche, Noise Flow, Plasma, Streamlines distanziate e Flow Loop ampliati. Catalogo a 15 e 63 ricette; 14 nuovi preset Full HD verificati, confronto a seed/risoluzione uguali e 12 frame CLI distinti decodificati. PNG dei 13 generatori precedenti identici via SHA-256 con parametri storici. Nuovi default Studio applicati solo ai lavori nuovi, ripristino JSON verificato headless. Guida [`M4D2_PITTURA_EVOLUZIONE.md`](M4D2_PITTURA_EVOLUZIONE.md), evidenze `docs/validation/M4D2_R1_*`. README descrittivo con screenshot, senza cronologia. Desktop/DPI, nuovi poster 4K e altri OS aperti. Cache NuGet locale, telemetria build disattivata e audit online non eseguito.

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

**Proseguire lo sviluppo**: M4C.3 compositing artistico per livelli/maschere; M4B.2 noise ciclico e animazioni fluide. Domain warping e LIC sono ora implementati e verificati in M4B.1. Restano verifiche desktop/DPI, poster grandi, animazioni lunghe e piattaforme diverse da Windows.

## Comandi Windows — PowerShell 7

Nella radice `GenerativeArt`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Per esempi, controlli dettagliati e aspettative vedere [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md), [`TESTING.md`](TESTING.md) e [`QUALITY_GATES.md`](QUALITY_GATES.md).
