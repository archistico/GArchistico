# M4E.1-R3 — Regressione test Streamlines / frequenza

## Riscontro e diagnosi

- **Ambiente segnalato dall'utente:** Windows, build della versione M4E.1-R2 riuscita.
- **Suite R2:** 880 test totali, 879 superati, 1 fallito (`ArtworkEvolutionTests.EveryArtisticControlChangesTheRaster("spaced-streamlines", "frequency", "0.012")`).
- **Causa:** `ArtworkEvolutionTests.Recipe` eredita `StudioRecipeDefaults.ParametersFor(id)`. Il JSON embedded del generatore contiene `field: "vortex"` e `frequency: 0.0035`. Il campo `vortex` viene prodotto da `VectorFields.Vortex` in `ArtisticFlow.Field` e non usa `frequency`. Per quel campo il PNG è giustamente invariato.
- Nel percorso `curl`, invece, `ArtworkRenderEngine.RenderSpacedFlow` passa `frequency` a `CurlNoiseField2D`. Questo è il contesto corretto per un test che richiede un raster diverso al variare del controllo.

## Correzione

In `tests/GenerativeArt.Rendering.Skia.Tests/ArtworkEvolutionTests.cs`:

1. Per la sola combinazione `spaced-streamlines` + `frequency`, impostare `field=curl` **prima di generare il PNG di riferimento**. Il valore `frequency` viene poi variato da quello del preset a `0.012`; le altre combinazioni restano invariate.
2. Aggiungere `SpacedStreamlinesVortexIsIndependentOfFrequency`: stesso seed e ricetta con `field=vortex`, due frequenze diverse, PNG identici.

Non sono stati modificati file di produzione, default, risoluzioni o renderer. La compatibilità delle ricette storiche e dei 17 JSON embedded è preservata.

## Limite UX distinto dal test

Nel pannello dei parametri, `frequency` può essere visibile anche scegliendo `vortex`, pur non incidendo sull'immagine. Questo è un comportamento condizionale dei campi, **non** la regressione di rendering segnalata. In A1 è pianificato un segnale informativo o la disabilitazione dei controlli inapplicabili, evitando cambiamenti alla semantica di `vortex` o ai PNG esistenti.

## Comandi PowerShell 7 — nella cartella `GenerativeArt`

Per verificare prima il test originariamente fallito e la nuova regressione:

```powershell
# Eseguire i due test mirati sul progetto Rendering.Skia.Tests
# Per sicurezza il filtro seleziona l'intero metodo parametrizzato.
dotnet test .\tests\GenerativeArt.Rendering.Skia.Tests\GenerativeArt.Rendering.Skia.Tests.csproj -c Release --filter 'FullyQualifiedName~EveryArtisticControlChangesTheRaster|FullyQualifiedName~SpacedStreamlinesVortexIsIndependentOfFrequency'
```

Per verificare la suite completa con le verifiche previste dal progetto:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

**Stato:** R3 preparata; non sono ancora disponibili risultati della suite Windows per questa revisione. Il numero 879/880 si riferisce esclusivamente a R2.
