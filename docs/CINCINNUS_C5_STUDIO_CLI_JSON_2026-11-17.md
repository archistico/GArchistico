# Cincinnus C5 — integrazione sperimentale Studio / CLI / JSON

**Data:** 17 novembre 2026. **C5-R1: build Release e 1.225/1.225 test Windows eseguiti con successo da Codex, SDK 10.0.201**. Tool autonomo compilato, 176 ricette valide, tre PNG CLI e Studio headless verificati. [Correzioni, evidenze e limiti](CINCINNUS_C5_R1_VALIDAZIONE_WINDOWS_2026-11-17.md). Il giudizio artistico rimane aperto.

## Obiettivo e perimetro

Registrare un nuovo generatore Cincinnus senza riscrivere il motore C1–C4, senza sostituire Crinis R10 e senza modificare gli algoritmi Ocellus/Rete G4-R2. C5 rende utilizzabili entrambe le strategie di distribuzione della geometria C2 (Simple) e C4 (Adaptive) con il pittore Skia C3, tramite il normale contratto `ArtworkRecipe` e `ArtworkRenderEngine` condiviso da CLI e Studio.

## Codice collegato

- `src/GenerativeArt.Application/Catalog/CincinnusArtworkDescriptor.cs`: ID stabile `cincinnus-bristles`, schema tipizzato e palette/colore singolo, modalità Simple/Adaptive.
- `src/GenerativeArt.Application/Catalog/BuiltInArtworkCatalog.cs`: registrazione del 30° generatore.
- `src/GenerativeArt.Application/Defaults/cincinnus-bristles.json`: preset iniziale Full HD, seed 3, Simple.
- `src/GenerativeArt.Rendering.Skia/CincinnusRenderer.cs`: convalida budget, costruisce campo a vortici, Simple/Adaptive, painter Skia e opzioni cromatiche.
- `src/GenerativeArt.Rendering.Skia/ArtworkRenderEngine.cs`: route Validate/Render, disponibile per CLI e Studio senza duplicazione.
- `src/GenerativeArt.Rendering.Skia/SkiaRasterSurface.cs`: invocazione interna del pittore sul canvas di proprietà della superficie.
- `src/GenerativeArt.Rendering.Skia/CincinnusPaintOptions.cs` e `CincinnusBristlePainter.cs`: override opzionale del background; default C3 invariato quando non specificato.
- `src/GenerativeArt.Application/Rendering/StudioParameterApplicability.cs`: disattivazione visiva dei controlli di una modalità non selezionata; i valori rimangono nei documenti JSON.
- `examples/recipes/cincinnus-c5/`: tre esempi 768×768 con seed differenti e palette Crimson/Ocean/Moss; i 173 esempi storici rimangono invariati.
- Test catalogo, JSON, UI applicability e percorso `ArtworkRenderEngine`→PNG in `tests/.../CincinnusC5*Tests.cs`.

## Controlli creativi

Il catalogo espone: modalità, vortici, distanza Simple, semilarghezza Simple, numero main Adaptive, tentativi/patch, passo griglia, ampiezza Adaptive; distanza setole, rapporto spessore fibre, oscuramento del corpo; raggi e opacità di due ombre, profondità tonale, variazione verticale, granelli; tre palette e **otto override indipendenti** `pigment-1` … `pigment-8`, con `auto` per il colore della palette. `background-color` usa un override separato. I valori di distanza/ombra nominali sono riferiti al canvas 2000px e scalati usando `min(width,height)/2000`; per il pennello è imposta una distanza minima di **0,8 px** alle risoluzioni piccole per evitare un'esplosione di fibre. Gli override non modificano la palette embedded.

Sono disponibili anche i controlli standard `RasterFinishing()` in qualità, finitura e materia. Il colore del pigmento è calcolato in RGB, non in OKLab; non è ancora un porting artistico certificato rispetto all'originale Gȧrt.

## Budget e limiti

- C5 solo statico; ogni dimensione fra **256 e 2048 px**, massimo **2.097.152 pixel** complessivi. Il preset 1920×1080 è sotto il cap. Non abilitare 8K per Cincinnus.
- `render-quality=fine-2x` disponibile solo fino a 524.288 pixel logici. I cap di C2/C3/C4 continuano ad applicarsi.
- `require-complete=false` (default) permette a una composizione sperimentale di essere disegnata anche se il posizionatore segnala un budget di copertura esaurito. **Questo NON significa copertura completa.** Se il controllo è `true`, il renderer rifiuta la composizione incompleta senza scrivere il PNG. Il test di copertura numerico resta a carico del layout C4.
- La gestione della cancellazione è passata a ogni stadio, mentre la codifica PNG avviene soltanto dopo `Render()` completo. Render e validate devono continuare a rispettare i cap preesistenti.
- Non ci sono anteprime C5 preconfezionate nel pacchetto: nessun render artistico è stato approvato.

## Comandi Windows — PowerShell 7

Dopo aver estratto lo ZIP integrale e aperto PowerShell nella cartella `GenerativeArt`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Lo script controlla clean, restore, build Release, gate rapido Application (ora incluso C5) e tutti i test della solution. Per eseguire solo i test nuovi C5:

```powershell
dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --filter 'FullyQualifiedName~CincinnusC5CatalogTests'
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --filter 'FullyQualifiedName~CincinnusC5RenderingTests'
```

Per confermare il round-trip JSON e generare immagini comparabili:

```powershell
dotnet run --project .\src\GenerativeArt.Cli -c Release -- list
dotnet run --project .\src\GenerativeArt.Cli -c Release -- describe cincinnus-bristles
dotnet run --project .\src\GenerativeArt.Cli -c Release -- recipe-validate .\examples\recipes\cincinnus-c5\cincinnus-simple-crimson-768.json
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\cincinnus-c5\cincinnus-simple-crimson-768.json .\cincinnus-c5-simple.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\cincinnus-c5\cincinnus-adaptive-ocean-768.json .\cincinnus-c5-adaptive.png
```

Lo Studio desktop deve mostrare **30 generatori**, includendo `Cincinnus · pennellate fluenti`; selezionare modalità e verificare che i controlli dell'altra modalità siano disattivati ma conservati nel JSON. La preview usa la risoluzione finale come negli altri generatori statici; verificare a 100% e confrontare con il PNG CLI.

### Gate tecnici e verifiche artistiche

1. **Chiuso in C5-R1:** build Release e suite completa, inclusi C3/C4/C5; 1.225 test superati. Editor verificato headless, non su desktop/DPI reale.
2. **Generazione/decodifica tecnica eseguita in C5-R1** sui tre esempi 768×768 ai seed 3/4/11. Resta da giudicare artisticamente vuoti, capelli, ordine dei tratti, ombre e saturazione.
3. Misurare tempo, picchi di RAM e comportamento di Stop/annulla su 768, 1920×1080 e un canvas al limite del budget.
4. Confrontare l'effetto visivo con gli algoritmi Gȧrt e non dichiarare la fedeltà estetica finché manca un'immagine di riferimento autentica Cincinnus.
5. **Chiuso in C5-R1:** corretti errori di compilazione/test e campioni Auto mancanti. Baseline tecnica Windows verificata da Codex; la sperimentalità artistica rimane.

**Decisioni invariate:** 17 preset personali salvaguardati; Crinis R10 sospeso, G4-R2 QA visivo aperto; 8K solo per quattro renderer geometrici già autorizzati; Fase B compositore dopo A e Fase D workflow dopo B, animazioni avanzate rinviate.
