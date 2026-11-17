# M4E.2-R4 — Finiture artistiche 8K a righe (Fase A4)

**Data:** 17 novembre 2026. **Base:** M4E.2-R3 (20 generatori, 108 ricette). La versione M4E.2-R1 è stata confermata compilata e testata dall'utente; R2/R3/R4 non hanno ancora una validazione Windows documentata.

## Implementazione

- L'8K UHD statico (dimensioni di riferimento: **7680×4320 o 4320×7680**, al massimo 33.177.600 pixel) con grana, dithering Bayer 4/8, carta cotone/ruvida, granulazione e assorbimento è consentito **solo** per Guilloché, Mosaico Voronoi, Streamlines distanziate e Stippling Poisson, con `render-quality=standard`.
- In `SkiaRasterSurface.WriteFinished` le immagini fino a 16.777.216 pixel seguono **senza deviazioni il vecchio percorso PixelBuffer** (incluso il medesimo encoder). Per quelle superiori a 16.777.216 pixel viene usato `ArtisticScanlineFinisher`.
- L'implementazione nuova usa una **bitmap Skia 8K aggiuntiva** per la finitura e **quattro righe uint più un buffer di byte** (~160 KB a larghezza 8192) invece di un PixelBuffer intero da 128 MiB. Lo snapshot originale e gli eventuali buffer interni di codifica PNG esistono ancora: **non è un encoder PNG interamente streaming**.
- Materiale e grana condividono i kernel per riga dei processori storici; la coordinata Y assoluta mantiene le texture e i pattern Bayer allineati su tutta l'immagine. Per l'assorbimento si conservano le righe originali precedente/corrente/successiva, in modo da non leggere i pixel già rifiniti.
- Conversione di alpha esplicita tra BGRA premoltiplicato Skia e ARGB straight Core. Il canale A non viene modificato dal postprocess; sotto alpha parziale si verificano possibili minimi arrotondamenti RGB durante la conversione, senza cambiare la riproducibilità di seed e ricetta.
- Restano invariati i limiti **4K degli altri 16 generatori**, il budget **fine-2x**, l'8K quadrato 8192×8192 (non disponibile) e l'assenza di un compositore universale.

## Studi 8K nuovi

Cartella `examples/recipes/m4e2-r4/`:

1. `voronoi-cotton-pigment-8k.json` — mosaico con carta cotone, pigmento e assorbimento.
2. `guilloche-security-print-8k.json` — stampa di sicurezza con Bayer 8, carta ruvida e grana.
3. `streamlines-aurora-ink-8k.json` — linee con texture carta, pigmento e assorbimento.

I **20 preset incorporati**, i **17 default personali** e le **108 ricette preesistenti** non sono stati modificati. Totale esempi **111**.

## Test aggiunti

- Equivalenza di tutti i kernel (grana, Bayer, cotone, ruvido, assorbimento, combinati) **pixel per pixel** fra `PixelBuffer` e `ArtisticScanlineFinisher` su una bitmap opaca piccola.
- Conservazione alpha con pixel trasparenti e semitrasparenti; cancellazione prima della modifica.
- Validazione 8K senza allocazione del raster, su tutti e quattro i generatori e tutte le finiture supportate; persistenza dei blocchi per renderer pesanti e `fine-2x`.
- Parsing e roundtrip JSON delle tre ricette 8K nuove.

**Importante:** questi test sono scritti e inclusi, ma non ancora eseguiti su Windows per questa revisione. Non chiamarli `passati` prima del riscontro.

## Verifica su Windows

Dalla cartella principale del progetto in **PowerShell 7**, con SDK .NET 10:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Poi smoke 8K effettivo (PNG molto grandi, RAM e CPU dipendono dalla macchina):

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e2-r4\voronoi-cotton-pigment-8k.json .\prova-voronoi-carta-8k.png
```

Confermare dimensioni 7680×4320, assenza di eccezioni, ripetibilità di seed, corrispondenza preview/export e disponibilità di memoria sul PC.

## Prossimi passi

**A4:** riduzione delle ulteriori copie complete, eventuale PNG a blocchi e 8K per renderer raster-heavy solo dopo profilazione memoria/CPU. **A1:** ulteriore isolamento dei controller Studio. **A0/A5:** integrità degli asset, benchmark e gate Windows. Quindi B compositing universale e D workflow dell'artista; animazioni avanzate più avanti.
