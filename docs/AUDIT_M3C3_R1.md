# Audit del codice ricevuto e delle modifiche M3C.3-R1

**16 novembre 2026 — perimetro:** archivio `GenerativeArt_M3C2_R1_DOCUMENTAZIONE_HANDOFF_COMPLETO.zip` e codice introdotto nella consegna M3C.3-R1. Audit prevalentemente statico: **non sono disponibili compilatore .NET 10, xUnit né Skia eseguibile** nel presente ambiente.

## Baseline e riscontri dal materiale ricevuto

- **Affidabile, già confermato dall'utente:** M3C.2-R1 compila/testa su Windows e produce JSON/PNG. L'errore JSON 288/289 era stato corretto prima dello ZIP ricevuto.
- **Disallineamento funzionale reale nel codice ricevuto:** il catalogo definisceva quattro artwork ma `ArtworkRecipeCli.Render` instradava solo `parametric-curves` e `looping-flow`; gli altri due venivano rifiutati nonostante ricette valide.
- **Rischio di evoluzione:** `Program`, `FlowDemo`, `EvenFlowDemo` e `LoopFramesDemo` contenevano rendering e policy di output direttamente negli host CLI. La GUI futura avrebbe dovuto copiare le implementazioni.
- **Possibili regressioni/output:** i percorsi legacy usavano `SavePng` su `FileMode.Create`; potevano sovrascrivere il file; nel loop non era presente cleanup esplicito dei frame parziali. La nuova policy mantiene la destinazione nuova/vuota e migliora il rollback.
- **Limite di sicurezza/performance:** `ArtworkRecipeJson.Deserialize(string)` convertiva prima l'intera stringa in UTF-8; ora rifiuta subito stringhe di lunghezza superiore a 64 KiB. In `ArtworkRecipeFiles.SaveNew` ora un write fallito tenta il cleanup del JSON incompleto.

## Interventi effettuati

1. Contratto Application per PNG su stream con frameIndex, validazione completa e cancellazione.
2. Runtime raster comune per cinque artwork, ricette v1 e versioni storiche inalterate.
3. Parametri `lines`/`max-lines` collegati al rendering reale; aggiunto `plasma-noise` con simplex multiscala e palette.
4. Quattro comandi legacy instradati nel runtime senza importare toolchain Go/Kotlin.
5. Output nuovo `CreateNew`, cleanup file prodotti durante esecuzioni fallite; manifest per ultimo.
6. Test xUnit del runtime, dei parametri/seed, budget, cancellazione, PNG e cleanup; esempi JSON supplementari.

## Rischi residui e criteri di chiusura

| Priorità | Verifica ancora necessaria | Perché |
| --- | --- | --- |
| P0 | Compilazione Release e suite .NET 10 Windows | Le modifiche non sono compilate nell'ambiente attuale |
| P0 | Smoke 5 renderer, loop e comandi legacy | Il test in memoria non sostituisce l'esportazione reale |
| P1 | Profilazione RAM/CPU Plasma 4M pixel e streamlines 500 | Budget numerici esistono, tempi non misurati |
| P1 | Diagnosi native Linux/macOS | SkiaSharp native assets e output non ancora provati |
| P1 | Gestione cancellazione interna del tracciatore e della griglia | La cancellazione è cooperativa ai confini delle operazioni |
| P2 | Output transazionale a staging con commit atomico | Manifest-last non rende atomico il salvataggio dei frame |
| P2 | Evoluzione formati GIF/APNG progressivi | Oggi soltanto PNG multipli |

**Decisione:** non avviare un secondo motore per Avalonia. Riutilizzare Application+Rendering.Skia dopo gate M3C.3. Per Studio M3D.1 serve soprattutto l'adapter UI, un controller di preview cancellabile e lo stato di riproduzione del loop.
