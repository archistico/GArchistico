# M4C.2-R3 — Correzione regressione JSON, dimensioni anteprima e budget dei poster

**Data:** 16 novembre 2026 · **Stato:** implementazione C# completa; build e test Windows di R3 da confermare. Base R2 compilata su Windows, con 506/507 test riusciti.

## 1. Difetto realmente segnalato

Il test `RecipeTests.MissingParametersAreRejected` cercava di simulare una ricetta JSON che non contiene `scale` sostituendo una sottostringa. Dopo l'eliminazione restava una **trailing comma**, quindi il parser JSON falliva prima della validazione semantica con `JsonReaderException`, mentre il test attendeva `ArgumentException` per il parametro mancante.

In R3 il test usa `JsonNode` e `AsObject().Remove("scale")`, dunque genera JSON ben formato, senza `scale`. Il parser di produzione **rimane rigoroso**: non abilitiamo trailing comma e non cambiamo schema JSON.

## 2. Immagini 2500×2000 e finiture

La risoluzione 2500×2000 corrisponde a **5.000.000 pixel**, superiore al budget di 4.194.304 delle finiture R1/R2. Un secondo budget di 4.194.304 era presente nel renderer `plasma-noise`. In R3 entrambi i limiti specifici sono rimossi o adeguati al limite generale statico, senza bypassare altri controlli.

| Rendering | Dimensioni 2500×2000 | Motivo |
| --- | --- | --- |
| Qualità `standard`, grana/carta/dithering | Ammesse se il generatore non ha altri vincoli | 5 MP sotto 16.777.216 globali |
| Plasma `standard` con materia | Ammesso | Rimosso limite Plasma autonomo a 4 MP |
| `fine-2x` 2500×2000 | Rifiutato | 4× sottopixel = 20 MP > 16.777.216 temporanei |
| Loop animato | Dipende da frameCount e dimensioni | Limite cumulativo 40 milioni di pixel invariato |
| Altri generatori | Dipende dal budget specifico | Topografia, Poisson, Voronoi possono avere proprie condizioni |

La finitura richiede conversioni aggiuntive Skia ↔ pixel gestiti, quindi l'uso di memoria aumenta con la risoluzione. L'elaborazione resta deterministica per seed e posizione e mantiene l'alpha. Non si introduce una nuova dipendenza e non si modifica il PNG predefinito quando gli effetti sono disattivati.

## 3. Tre dimensioni indipendenti nello Studio

1. **Output — Larghezza/Altezza px:** dimensioni della ricetta salvata e del PNG esportato; non vengono modificate dai controlli anteprima.
2. **Raster MAX anteprima — Larghezza/Altezza px:** limite massimo per il raster realmente calcolato dalla preview (default **800×600**). La `StudioPreviewPolicy` conserva il rapporto d'aspetto dell'output, senza inventare una geometria diversa. Il limite configurabile è tra **64 e 4096 px per asse**, con budget massimo pari al prodotto dei due valori.
3. **Riquadro MAX — Larghezza/Altezza DIP:** dimensioni massime assegnate all'area visibile dello Studio (default **1200×900 DIP**). Il riquadro si riduce comunque allo spazio disponibile nella finestra; DPI/DIP sono misure logiche della UI, non pixel dell'export.

Comandi di visualizzazione:

- **Adatta:** calcola lo zoom necessario affinché tutti e quattro i bordi dell'immagine siano visibili, con un margine interno, e centra il risultato. È attivo automaticamente quando arrivano immagini nuove o cambia la dimensione della finestra, fino a quando l'utente imposta uno zoom manuale.
- **100%:** visualizzazione 1:1 in coordinate logiche, con possibilità di pan usando il mouse. Non altera il bitmap esportato.
- **Raster = output:** imposta i limiti raster anteprima uguali alle dimensioni dell'output. Serve a esaminare il risultato a risoluzione piena, ma può essere lento e resta soggetto ai budget del renderer.
- **Zoom/rotella:** zoom manuale ancorato al cursore; disattiva temporaneamente l'auto-fit. **Centra** ricentra senza cambiare la scala.

La barra di stato distingue espressamente `Anteprima: W×H` da `output: W×H`, per evitare di confondere un raster veloce con la risoluzione finale. La preview conserva seed e parametri artistici. Parametri di soli viewport e massimi raster preview **non vengono inseriti nel JSON della ricetta**, che rimane formato v1.

## 4. Test e checklist Windows

Regression test in `RecipeTests`, `ArtworkRenderEngineTests`, `ArtisticPostProcessorTests`, `StudioViewportStateTests`, `StudioPreviewPolicyTests`.

Eseguire dalla directory di `GenerativeArt.sln` usando PowerShell 7:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Poi:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Verifiche manuali: scegliere **Plasma**, impostare output **2500×2000**, qualità `standard`, grana >0, carta `cotton`; esportare PNG e controllare la dimensione effettiva. In anteprima verificare Adatta, 100%, pan, zoom, resize, riquadro max, raster 800×600 e Raster = output. Verificare che `fine-2x` su 2500×2000 comunichi il limite del supersampling, non quello obsoleto delle finiture. Ripetere una seconda volta con gli stessi seed e parametri per il determinismo. Monitorare il picco RAM e il tempo sui poster grandi.

## 5. Prossima fase

Riprendere M4C.3 (livelli, maschere e blend) e M4B (domain warping, animazioni cicliche); prima di aumentare ancora la dimensione delle texture, misurare i costi in RAM e CPU e valutare elaborazione a righe/tiles del post-processing.
