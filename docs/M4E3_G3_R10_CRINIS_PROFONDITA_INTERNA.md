# M4E.3-G3-R10 — Crinis: tridimensionalità dentro la pennellata

**Data:** 17 novembre 2026  
**Baseline validata:** R9, compilazione e test superati su Windows secondo l'utente.  
**Revisione R10:** sviluppata da quella baseline; da validare con .NET su Windows.  
**Linguaggi del progetto:** C#/.NET 10, Avalonia, SkiaSharp; script PowerShell minimi. Nessun codice Python aggiunto al progetto.

## Diagnosi dalla ricetta e dal PNG allegati

La ricetta originale `crinis-bristles(2).json` imposta `width=1920`, `height=1080`, `seed=7775761298886397793`, `bands=50`, `bristle-pitch=1.36`, `fiber-density=1.907`, `hair-width=5.767`, `fiber-coverage=1.019`, `body-opacity=0`, `detail-style=micro`, `render-quality=fine-2x`, con le opzioni R9 per volume/illuminazione del fascio. Anche con `hair-width` elevato, il renderer R9 usa `EffectiveFiberWidth` e la distanza locale reale tra centinaia di fili: il diametro di quasi tutti i tratti viene ridotto a una frazione del pixel nella sezione compressa. Il PNG allegato mostra buona gerarchia tra fasci ma poco rilievo **dentro** i fasci.

## R10: quattro controlli, gruppo «Profondità interna»

| Parametro JSON | Intervallo | Default catalogo | Nuovi lavori Studio | Funzione |
| --- | ---: | ---: | ---: | --- |
| `intra-bundle-depth` | 0–1 | 0 | 0.85 | Tre sottostrati di fibre, ordini indipendenti e contrasto tra retro/fronte |
| `featured-fibers` | 0–1 | 0 | 0.82 | Prominenza controllata di pochi filamenti più corposi rispetto alla microtrama |
| `cross-section-light` | 0–1 | 0 | 0.88 | Illuminazione trasversale a creste/avvallamenti lungo la sezione |
| `internal-occlusion` | 0–1 | 0 | 0.72 | Ombre di contatto differenziate fra strati interni (richiede profondità maggiore di 0) |

Il preset Studio per **nuove opere** porta `hair-width` da 1.65 a 3.2. Il valore di default del catalogo resta 1, e il JSON utente originale resta identico.

## Implementazione

1. In modalità micro, quando `intra-bundle-depth > 0` i semi di ogni fibra vengono fissati prima del disegno. Le fibre vengono tracciate in tre passaggi **retro → medio → fronte**. La loro casualità non dipende dall'ordine di disegno: seed e ricetta uguali generano gli stessi pixel a ogni rendering.
2. Per il nuovo controllo di calibro, circa un filamento ogni nove può emergere più spesso e gli altri rimangono una tessitura sottile. Il massimo è vincolato allo spessore richiesto: si evita un ispessimento indifferenziato di centinaia di filamenti.
3. Il profilo `cross-section-light` tiene conto di curvatura locale trasversale (sezione ellittica), direzione della luce e variazioni ampie in più creste. La luce è applicata **fibra per fibra** con mezzi toni; non c'è un overlay opaco della sagoma.
4. `internal-occlusion` intensifica in modo selettivo l'ombra di contatto solo quando la profondità interna è attiva. Lo strato posteriore ha toni/alpha più morbidi, quello frontale è più definito. R10 **non** aggiunge un vero filtro Gaussian su ogni sottostrato, che richiederebbe un ulteriore studio dei costi raster; il risultato resta 2.5D.
5. I quattro controlli sono `optionalInJson=true` con default di catalogo zero. Con `intra-bundle-depth=0` si conserva il percorso originale, il consumo della sequenza RNG e gli stessi pixel dei file storici. Nessuna variazione agli altri 26 generatori.

## Esempi per confronto A/B

- `crinis-r9-originale-utente.json`: copia **byte-identica** del JSON allegato R9; per riprodurre il punto di partenza.
- `crinis-indigo-r10-tridimensionale-1920.json`: stesso seed, stessa dimensione, stessa palette e stessi parametri R9; aggiunge esclusivamente i quattro controlli di profondità interna.
- `crinis-rame-r10-luce-e-fibre-1920.json`: studio più materico, con variazioni di luce/volume/densità per saggiare un secondo colore.

### Verifiche introdotte

- Applicability UI e contratto dei descrittori Application/Release, JSON precedenti con default neutri, tre nuove ricette.
- Test Skia: filamento principale più spesso del secondario e della microtrama, limite del calibro; differenza raster per tutti i controlli attivi; seed deterministico; dimensioni PNG; precedente PNG identico quando i nuovi controlli sono assenti/zero; cancellazione senza output parziale.
- Da effettuare su Windows: compilazione Release e suite intera; confrontare A/B a 100% e al Fit. Non è stato eseguito un test visivo quantitativo di equivalenza allo stile artistico Gȧrt.

## Comandi Windows (PowerShell 7)

Estrarre lo ZIP e aprire PowerShell 7 nella cartella con `GenerativeArt.sln`.

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando pulisce e verifica il progetto. Poi, per test mirati (dopo la build):

```powershell
dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~Crinis"
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~Crinis"
```

Per due PNG confrontabili (la CLI non sovrascrive file già esistenti):

```powershell
New-Item -ItemType Directory .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\m4e3-g3-r10\crinis-r9-originale-utente.json .\output\crinis-r9-confronto.png
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\m4e3-g3-r10\crinis-indigo-r10-tridimensionale-1920.json .\output\crinis-r10-confronto.png
```

**Lettura critica:** verificare se lo spessore dei filamenti principali aumenta veramente il rilievo interno senza fondersi in un nastro chiaro. Se il guadagno visivo è insufficiente, modificare il modello di shading reale e gli incroci di microstrati, non aumentare semplicemente il numero di filamenti.

**Roadmap successiva:** R11 shading anisotropo dipendente dalla curvatura locale e piccoli gruppi materici; R12 (eventuale) micro-occlusione su height-map a budget controllato. Poi G4/Ocellus e Rete, Fase A1/A4/A5, compositore B, workflow D; animazione solo più avanti.
