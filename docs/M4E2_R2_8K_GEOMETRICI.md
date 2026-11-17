# M4E.2-R2 — Fase A4: primo supporto 8K UHD per generatori geometrici

**Data:** 17 novembre 2026. **Base:** M4E.2-R1, 18 generatori, 96 esempi, 17 preset personali invariati.

## Risultato di questa revisione

Il renderer accetta ora opere **statiche** fino a **7680×4320 = 33.177.600 pixel**, orientamento orizzontale o verticale, **soltanto in `standard`** e per quattro generatori che disegnano direttamente sulla superficie Skia:

- `guilloche` — intrecci, curve e incisioni geometriche;
- `voronoi-mosaic` — poligoni e tassellazioni;
- `spaced-streamlines` — curve tracciate su un campo vettoriale;
- `poisson-stippling` — punti antialias direttamente sulla superficie.

Ogni lato deve inoltre essere **≤8192 pixel**. Il formato quadrato **8192×8192 non è abilitato**: supera il budget di questa prima fase. Le altre 14 famiglie mantengono il limite precedentemente supportato **4096×4096** e i propri budget specifici. Non è stata disattivata alcuna protezione contro configurazioni eccessive.

La nuova classe `RasterCanvasBudgetPolicy` valida il canvas **prima di creare la superficie o scrivere byte PNG**. Sono rimaste indipendenti le altre restrizioni:

| Componente | Comportamento R2 |
| --- | --- |
| Raster geometrico in `standard` | 7680×4320 per i quattro generatori indicati |
| Altri renderer | Budget precedente, normalmente fino a 4096×4096 |
| Supersampling `fine-2x` | 16.777.216 sottopixel totali, invariato |
| Grana, dithering e materia | 16.777.216 pixel finali, invariato |
| Animazioni | Limiti storici invariati, nessuna estensione 8K |
| Anteprima Studio | Pixel effettivi del canvas; `Adatta`/`100%` soltanto sulla vista |

**Memoria:** un singolo buffer BGRA a 7680×4320 occupa circa **126,6 MiB**. Snapshot, encoder PNG, preview Avalonia e processo possono richiedere memoria aggiuntiva; i numeri non costituiscono garanzia di consumo massimo. Questo incremento non realizza ancora tile/streaming raster o un allocatore con budget di memoria dinamico.

## Quattro nuovi studi 8K

Nella cartella `examples/recipes/m4e2-r2/`:

1. `guilloche-security-8k.json` — intrecci da stampa di sicurezza;
2. `voronoi-glass-8k.json` — mosaico geometrico con 192 siti;
3. `streamlines-aurora-8k.json` — flussi ad alta densità;
4. `poisson-cobalt-8k.json` — stippling a 25.000 punti massimi.

Sono ricette autonome JSON v1. Non sostituiscono i preset personali e non vengono caricate automaticamente al posto di quelli dell'utente. I 96 esempi precedenti rimangono inalterati; totale previsto: **100 esempi**.

## Validazione Windows

Aprire **PowerShell 7** nella cartella che contiene `GenerativeArt.sln`.

1. Controllare compilazione, regressioni, JSON e test ordinari:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

2. Come smoke **opzionale** e volutamente costoso, produrre un PNG 8K in una cartella temporanea (destinazione nuova: la CLI non sovrascrive):

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e2-r2\voronoi-glass-8k.json .\prova-voronoi-8k.png
```

3. Aprire il PNG e controllare **7680×4320**, l'assenza di tagli e la resa a 100%. Importare lo stesso JSON nello Studio, usare **Adatta** e **100%**, quindi esportare con un altro nome; verificare visivamente e, se necessario, confrontare gli SHA-256 dei due PNG.

4. Ripetere progressivamente con Guilloché, Streamlines e Poisson. Su macchine con memoria limitata usare inizialmente risoluzioni intermedie. Se il sistema esaurisce memoria o Skia rifiuta un'allocazione, annotare il generatore, i parametri e la dimensione: il successivo lavoro sui buffer/tile dipenderà da queste misure.

## Test introdotti

- `RasterCanvasBudget8KTests`: accettazione senza allocazione di tutti e quattro i renderer, rifiuto degli algoritmi raster-heavy, limiti di lato e area, `fine-2x` e finiture su 8K, conservazione della preview esatta.
- `EightKRecipeFixturesTests`: quattro JSON v1 validi, dimensioni, parametri e roundtrip.
- I test ordinari **non renderizzano immagini 8K**: la generazione Full HD/8K reale è una prova manuale esplicita.

## Continuità e prossime attività

Preservati catalogo, versione degli algoritmi, 17 preset personali, 96 ricette precedenti, qualità e comportamenti delle immagini 4K già supportate. L'unica variazione prevista nel CLI è l'accettazione di dimensioni fino a 8192 durante `recipe-new`; il budget reale resta validato dal renderer quando si genera.

**A4 successiva:** ottimizzare i renderer con buffer PixelBuffer/float per render 8K, valutare finiture per bande, monitorare memoria e throughput, aumentare qualità geometrica dove utile, poi proporre 8192×8192 con una pipeline in tile. **A1/A0/A5** restano aperte. In seguito **B** compositore universale, **D** workflow dell'artista; animazioni evolute ancora rinviate.

**Stato verifiche:** controllo statico della struttura e dei JSON; il gate build/test Windows della nuova R2 resta aperto. Baseline confermata dall'utente: M4E.1-R3.
