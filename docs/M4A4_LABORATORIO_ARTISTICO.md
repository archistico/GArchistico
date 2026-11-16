# M4A.4-R1 — Laboratorio artistico, libertà dei parametri e qualità visiva

**16 novembre 2026 — compilazione e suite xUnit M4A.4-R1 confermate dall'utente su Windows.**
Baseline confermata dall'utente: **M4A.3-R1 Voronoi, compilazione e test superati**. Lo Studio/CLI della fase precedente sono funzionanti.

## Problema osservato e obiettivo

I nove algoritmi sono utilizzabili ma molte opere appaiono troppo geometriche o uniformi: palette e stile sono soprattutto hard-coded nei renderer. L'obiettivo è fare dello Studio uno strumento artistico che espone tutte le proprietà di ricetta, permette di sostituire i colori di ogni ruolo, offre maggiore libertà per densità/tratto e conserva la distinzione tra anteprima veloce e opera finale per stampa.

## Intervento implementato in questa revisione

1. **Dimensioni nuove ricette Studio**: 1920×1080, per tutti i nove generatori. Le ricette JSON aperte mantengono la propria dimensione. `StudioPreviewPolicy` continua a renderizzare una preview max 800×600 / 480.000 pixel: su Full HD diventa **800×450**; **il PNG finale usa 1920×1080** (o il valore impostato dall'utente).
2. **Flow Loop**: il motore ora consente una singola immagine fino a 4096 px per lato nel suo budget statico, conservando la soglia **40 milioni di pixel cumulativi** per sequenza. Il default Studio è **16 frame, 18 FPS**, perciò 1920×1080×16 = 33.177.600 pixel, valido. Non usare 36 frame a Full HD senza cambiare dimensione o budget.
3. **Pannello ricetta**: pannello destro più ampio, parametri sempre presenti e raggruppati per categoria. Rimangono scrollabili, con slider e campo numerico esatto per le grandezze. Nessuna perdita di pan/zoom o preferenze.
4. **Colori per ruolo**: i parametri `ColorOverride` accettano `auto`, `#RRGGBB` o `#AARRGGBB`; `auto` segue la palette di preset, gli esadecimali la sovrascrivono singolarmente. Swatch, campo e pulsante di ripristino. Implementato per: topografia (linee secondarie/principali + sei fasce), Poisson (sfondo + tre pigmenti), SDF (sfondo, linee, riempimento, accento) e Voronoi (sfondo, fuga, accento + cinque tessere). I cinque generatori storici sono stati completati nella successiva **M4A.4-R2**; per lo stato attuale vedere [`M4A4_R2_COLORI_HSV.md`](M4A4_R2_COLORI_HSV.md).
5. **Poisson**: `spacing` 0.001–0.2 (prima 0.012–0.1), raggio 0.1–128 (prima 0.5–5), fino a 50.000 punti, griglia al massimo 6 milioni di celle. Nuovi controlli: variazione dimensione, frequenza pigmento, forma `solid`/`ring`/`mixed`. Limiti rimangono per impedire allocazioni eccessive. Cambiare raggio dei cerchi non modifica la distanza minima dei **centri**: le aree dipinte possono sovrapporsi.
6. **Curve topografiche**: stile `precise`, `handdrawn` (deformazione coerente nello spazio, seedata), `soft` (un passaggio Chaikin, mantiene estremi e anelli chiusi). `stroke-roughness` in pixel, spessore linee da 0.1 a 20. Il campo scalare e Marching Squares non sono stati modificati. I vecchi output rimangono con `precise` di default.
7. **Ricette JSON v1 storiche**: i soli nuovi parametri sono marcati `OptionalInJson` e vengono riempiti coi default durante il caricamento; le proprietà sconosciute/duplicate e l'assenza di parametri storici restano errori. In serializzazione le ricette vengono aggiornate coi parametri attuali senza cambiare `formatVersion` o gli ID/versioni degli algoritmi.
8. **Nuovi esempi**: `topographic-handdrawn-1920.json`, `topographic-soft-1920.json`, `poisson-ink-study-1920.json`, `voronoi-stained-glass-1920.json`. Mostrano l'effetto di densità e pigmenti personalizzati, ma la qualità artistica finale richiede smoke visivo su Windows.

## Antialiasing: stato effettivo e sviluppo futuro

`SkiaRasterSurface` usa già `SKPaint.IsAntialias=true` per `DrawLine`, `DrawCircle`, `DrawPolygon` e `DrawPolyline`; i percorsi hanno giunzioni arrotondate. **Non significa** che i renderer che calcolano colori per singolo pixel (es. `SdfOrbitRenderer`, bande topografiche) abbiano copertura subpixel o antialias completo. Non è stato aggiunto in questa revisione alcun toggle AA, supersampling o encoder a 16 bit.

**Roadmap qualità** (da implementare e misurare, in quest'ordine):

- AA analitico dei contorni raster, derivato dalla distanza e dalla dimensione del pixel, con compositing sRGB/lineare esplicito;
- preview qualità bassa/media/alta e selezione filtro di downsampling, mantenendo interazione rapida;
- supersampling x2/x4 **con memoria preventivata**, tile/padding quando applicabile: a 4000×4000, un buffer BGRA a 4× area richiede già circa 256 MB *per buffer*, senza contare gli altri;
- trattamento colore (linear-light/sRGB), dithering opzionale, grana carta/pigmento riproducibile, penne con pressione e sovrapposizioni multilivello;
- export poster con dimensioni fisiche, DPI e eventuale SVG/PDF, indipendente dal mero numero di pixel.

## Gate di accettazione Windows

Da PowerShell 7 nella cartella che contiene `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Verifiche manuali: tutti i generatori nuovi da catalogo nascono 1920×1080, l'anteprima è scalata, pan/rotella identici alla baseline, loop parte con 16 frame. Aprire un vecchio JSON M4A.3 senza override e verificare che mantenga la palette. Cambiare un solo colore, poi premere Auto: il resto della palette non deve cambiare. Provare `handdrawn`/`soft` sullo stesso seed; Poisson `spacing=0.006`, `dot-size=8`, stile mixed e personalizzazione pigmento. Salvare/reimportare JSON, esportare PNG, usare Annulla export. Aprire il PNG reale e giudicare la resa: **test xUnit verde non prova la qualità artistica**.

## Cosa resta da fare

- **Completato in M4A.4-R2 (gate Windows aperto):** color picker visuale HSV/RGB/alpha e ruoli colore anche per flow, parametriche, Plasma e animazioni; documentazione dedicata R2.
- Parametri artistici per livelli, pressione e texture su tutti i generatori; preset davvero differenziati (incisione, pennino, acquerello, morbido, materico) con esempi e controlli.
- Profilazione CPU e RAM per Poisson densissimo, topografia 4000×4000, export/cancellazione e sequenze animate.
- Renderer di qualità AA raster e supersampling progressivo, controlli espliciti di qualità e export per la stampa.
- M4B noise/domain warp e animazioni, M4C colore/post-processing e M4D pennelli, con lo Studio come laboratorio di riferimento.

I comandi della CLI scrivono solo su percorsi nuovi. Le immagini di esempio non vengono generate nell'ambiente di preparazione (SDK .NET 10 non installato).
