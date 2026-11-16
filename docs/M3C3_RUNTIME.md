# M3C.3 — Runtime raster condiviso e primo Plasma

**Data:** 16/11/2026 · **Stato:** implementato in M3C.3-R1 e aggiornato in **M3C.3-R2**. Su Windows il test catalogo R1 è fallito perché attendeva 4 invece delle 5 opere previste. Il test è stato corretto in R2; **compilazione e suite R2 ancora da verificare**, così come gli smoke PNG/loop (SDK .NET 10 assente nell'ambiente di preparazione). Non confondere con la baseline M3C.2-R1 verificata dall'utente.

## Scopo

Passare dalle demo isolate a un percorso unico `catalogo → ricetta tipizzata → renderer → PNG`, riutilizzabile da CLI e futura GUI Avalonia. Lo schema ricetta **resta v1** e gli algoritmi esistenti **restano v1**: non è stata introdotta una migrazione implicita.

## Nuovi componenti

- `Application.Rendering.IArtworkRasterRenderer`: contratto senza Skia, CLI o GUI. `Validate(recipe, frameIndex?)` verifica schema/versione/budget prima dell'output; `Render(recipe, stream, frameIndex?, cancellationToken)` scrive **un solo PNG** senza chiudere lo stream del chiamante.
- `Rendering.Skia.ArtworkRenderEngine`: implementazione headless esplicita per **cinque** artwork. Ogni invocazione possiede e rilascia **una** `SkiaRasterSurface`. Gli stream sono di proprietà del chiamante.
- `Cli.RecipeOutputWriter`: esportazione statica con `FileMode.CreateNew`; animazione progressiva in directory nuova o vuota, con `manifest.json` scritto per ultimo. In caso di errore rimuove solo i file creati dalla singola invocazione, lasciando intatti file precedenti. Il manifest segnala il completamento logico, **non** una transazione filesystem atomica.
- Comandi legacy: reindirizzati allo stesso motore, con valori di default storici (`render-flow` calcola ancora le linee in base alle dimensioni; `render-flow-spaced` conserva 210 linee massime). Il PNG legacy non viene sovrascritto senza consenso, come già previsto per `render-recipe`.

## Artwork disponibili e parametri effettivi

| Artwork | Parametri | Effetto nel renderer | Limiti per render |
| --- | --- | --- | --- |
| `parametric-curves` v1 | `scale` 0.05–0.48 | raggio della curva | lato ≤4096, pixel ≤16.777.216 |
| `noise-flow` v1 | `lines` 1–500 | esatto numero massimo di tracce | lato ≤4096, pixel ≤16.777.216 |
| `spaced-streamlines` v1 | `max-lines` 1–500 | limite di linee in `EvenStreamlineOptions` | lato ≤4096, pixel ≤16.777.216 |
| `looping-flow` v1 | `loop-radius` 0.1–5; `palette` aurora/ember/mist | campo periodico e colori | lato 64–1024, frame 2–120, fps 1–120, totale pixel ≤40 milioni |
| **`plasma-noise` v1** | `frequency` 0.001–0.1; `octaves` 1–6; `palette` deep/ember/moss | fBm Simplex 2D e rampa cromatica | dalla M4C.2-R3: pixel ≤16.777.216 e lato ≤4096 in standard |

`plasma-noise` è la prima *famiglia pilota* pixel/noise, non un porting visualmente equivalente a un artwork di Gȧrt o Go. Il seed è riproducibile nel medesimo ambiente di rendering. `parametric-curves` mantiene il seed come metadato: la geometria non dipende da esso.

## Sicurezza, prestazioni e limiti

1. Ricette JSON rigorosamente validate dal codec M3C.2: massimo 64 KiB, proprietà ignote e duplicate rifiutate, ID/versioni esatti e limiti per parametro. Il renderer richiama `ValidateAgainst` anche quando viene usato senza CLI.
2. Il limite di `CanvasSize` (67 milioni di pixel) non coincide con il budget più conservativo **per artwork**, verificato prima di produrre un output. Le animazioni sono limitate anche da pixel cumulativi e non memorizzano tutte le bitmap.
3. `CancellationToken` controllato prima del render, per riga raster, prima dei tracciati e fra i frame. **Nota:** la generazione interna di una singola streamline/grid non è cancellabile al suo interno; è limitata dai budget.
4. Nessuna promessa di identità binaria PNG fra OS/architetture diverse; l'identità con lo stesso seed è coperta dai test sulla stessa toolchain/runtime.
5. Nessun GIF/APNG/MP4, SVG/PDF o GUI in questa milestone. La GUI M3D riutilizzerà il renderer esistente senza creare una seconda implementazione.

## Verifiche richieste per chiudere il gate

```powershell
# Entrare in GenerativeArt, directory con GenerativeArt.sln (PowerShell 7+)
clear && dotnet clean && .\tools\validate.ps1

# Nuova opera pixel/noise: destinazioni inesistenti
 dotnet run --project src/GenerativeArt.Cli -c Release -- recipe-new plasma-noise .\output\plasma.json 256 256 42
 dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\output\plasma.json .\output\plasma.png

# Due renderer ricetta prima non collegati
 dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\noise-flow.json .\output\noise.png
 dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\spaced-streamlines.json .\output\spaced.png

# Loop: verificare 8 frame PNG e manifest.json
 dotnet run --project src/GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\looping-flow.json .\output\loop-m3c3
```

I file e le directory di destinazione devono essere **nuovi o vuoti** secondo il tipo. Non considerare il test eseguito sulla sola base della presenza di questi comandi nella documentazione.

**Prossima milestone:** M3D.1 — Studio Avalonia code-only, anteprima statica e animata, editor parametri tipizzati, cancellazione preview, caricamento/salvataggio delle stesse ricette.
