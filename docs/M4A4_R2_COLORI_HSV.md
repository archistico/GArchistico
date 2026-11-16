# M4A.4-R2 — Ruoli colore su nove generatori, editor HSV/RGB/Alpha

**16 novembre 2026.** L'utente ha confermato **compilazione e suite xUnit passate per M4A.4-R1** su Windows. Questa nuova R2 **non è ancora stata compilata o testata su Windows**: l'ambiente di preparazione non possiede .NET SDK 10. La verifica statica, il controllo JSON e l'integrità ZIP non sostituiscono i test xUnit.

## Obiettivo e risultato

Il laboratorio artistico deve permettere di modificare *ogni ruolo cromatico* di tutti i nove generatori senza riscrivere il renderer o forzare l'intera palette. In R1 questa possibilità era limitata a Topografia, SDF, Poisson e Voronoi. R2 aggiunge:

- `parametric-curves`: background (Auto mantiene il gradiente), traccia principale e anello;
- `noise-flow`: background (Auto mantiene il gradiente) e tre inchiostri;
- `spaced-streamlines`: background e quattro inchiostri;
- `looping-flow`: background e quattro colori animati; la stessa sovrascrittura si applica a ogni frame;
- `plasma-noise`: quattro stop del gradiente, con interpolazione anche del canale alpha.

**Tutti gli override hanno valore predefinito `auto`**. I parametri aggiunti sono `OptionalInJson`; la serializzazione rimane **JSON v1**, gli ID e le versioni algoritmo non cambiano. La posizione dei seed, l'anteprima, le dimensioni, il budget e il playback non sono modificati. Gli output storici con Auto mantengono i colori precedenti. *Nota*: l'override di background su `parametric-curves` e `noise-flow` sostituisce **il gradiente originale con una tinta uniforme**. Se si vuole il gradiente, premere `Auto`.

## Selettore colore visuale

Nel pannello ricetta ogni ruolo presenta campione colore, testo HEX, **Scegli…** e **Auto**. Il campione Auto mostra il colore del preset selezionato, non un grigio generico; sui due background graduati è un campione indicativo, non l'intero gradiente. Cambiando la palette si aggiornano i campioni Auto; i colori personalizzati restano invariati.

Il pulsante **Scegli…** apre una finestra modale indipendente con:

1. slider HSV (tonalità 0–360°, saturazione/valore 0–100%);
2. slider Alpha (0–255), senza perdita dei canali RGB;
3. caselle numeriche R/G/B/A (0–255) con azione **Applica RGB**;
4. editor HEX `#RRGGBB` o `#AARRGGBB` con **Applica HEX**;
5. anteprima del colore e pulsanti **Conferma colore** / **Annulla**.

Il colore viene pubblicato nella ricetta **solo alla conferma**; nella finestra RGB ed HEX vengono applicati premendo l'omonimo tasto (i campi non confermati non modificano il colore). `StudioColorValue` contiene la conversione HSV↔RGB, l'alpha a 8 bit e la canonicalizzazione senza dipendenze Avalonia. Lo swatch non misura la conversione colorimetrica in gamma lineare: l'editor lavora in sRGB; il miglioramento dei raster è demandato a M4C.

## Test aggiunti e regressioni

- `StudioColorValueTests`: RGB, ARGB, trasparenza, esadecimale invalido, primari HSV, scala di grigi, roundtrip della griglia RGB e valori NaN/infinito.
- `ArtisticColorRoleTests`: copertura ruoli/palette di **tutti e nove** gli artwork, confronto PNG Auto/override nei cinque generatori storici, riproducibilità e roundtrip JSON v1, import di ricette legacy senza i nuovi campi, alpha PNG Plasma.
- `CatalogTests`: attesa dei quattro nuovi colori del Plasma. Tutti i test precedenti restano inclusi, non vengono rimossi.

Sono inclusi **cinque JSON dimostrativi Full HD**: `parametric-ink-1920`, `noise-flow-pastel-1920`, `spaced-streamlines-monochrome-1920`, `plasma-neon-1920` e `looping-flow-duotone-1920` in `examples/recipes/`. Sono composizioni dimostrative **da renderizzare e giudicare visivamente**, non immagini già esportate.

## Validazione su Windows (PowerShell 7+)

1. Estrarre lo ZIP in una cartella nuova e aprire PowerShell 7 **nella cartella che contiene `GenerativeArt.sln`**. Verificare SDK .NET 10 disponibile e primo restore NuGet completabile.
2. Eseguire l'intera validazione:

   ```powershell
   clear && dotnet clean && .\tools\validate.ps1
   ```

   Risultato atteso: exit code zero, build Release senza warning trattati come errori, suite xUnit tutta verde. In caso di errore riportare il progetto, il test e lo stack trace.
3. Avviare lo Studio:

   ```powershell
   dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
   ```

   Aprire ognuno dei nove artwork: provare **Scegli…**, slider HSV, RGB, alpha, HEX e Annulla; cambiare una sola tinta e poi Auto; cambiare palette con altri override impostati; esportare PNG, salvare/riaprire JSON e confrontare i colori. Confermare pan/zoom, tema, preferiti e playback.
4. Smoke CLI con file nuovi (la CLI non sovrascrive):

   ```powershell
   New-Item -ItemType Directory -Force .\output | Out-Null
   dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\plasma-neon-1920.json .\output\plasma-neon-r2.png
   dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\noise-flow-pastel-1920.json .\output\noise-flow-pastel-r2.png
   dotnet run --project .\src\GenerativeArt.Cli -c Release -- recipe-validate .\examples\recipes\looping-flow-duotone-1920.json
   ```

   Per il loop completo, usare il comando CLI `render-loop-frames` descritto nel README; non creare inutilmente decine di frame Full HD senza il budget necessario.

## Limiti e prossimo intervento

La preview è limitata a 480k pixel; le immagini finali seguono ancora i limiti specifici di ogni renderer (Plasma max 4.194.304 pixel, loop 40M pixel complessivi, statici max 4096×4096). RGB/HSV modificano un colore singolo, **non** implementano un generatore di armonie, gradienti multi-stop con posizione arbitraria, un vero spazio colore artistico OKLab oppure pennelli materici. Non sono stati implementati AA analitico, supersampling, DPI di stampa, SVG/PDF, LIC o domain warping.

**M4A.4-R3 / M4C.1:** misurare prima RAM/CPU su Full HD e poster; poi progettare qualità raster configurabile con AA di contorno e compositing colore ben definito. Contemporaneamente ampliare preset e texture con test di riproducibilità. **M4B:** domain warping e loop animati senza cache di frame. Nessun nuovo pacchetto NuGet è richiesto da R2.
