# M4E.3-G3-R5 — Crinis, fibre separate e pigmento vivo

## Evidenza di partenza (17/11/2026)
- Screenshot reale 1920×1080, seed 42, palette indigo, `detail-style=micro`, `bands=24`.
- Parametri nel file allegato: `hair-width=2.629`, `bristle-pitch=1.36`, `fiber-density=1.836`, `body-opacity=0.081`, `fiber-relief=0.911`, `fiber-fray=0.648`, `ink-flecks=2964`.
- L'impressione di grandi nastri pieni deriva in parte dal rapporto fra diametro nominale e distanza reale dei filamenti, oltre che dalla presenza voluta di una piccola opacità del corpo (`0.081` non equivale a invisibile).

## Correzioni al codice
1. `CatalogTests.AllArtworksExposeOptionalFinishingControlsWithNeutralDefaults`: il parametro `ink-flecks` era classificato per errore in `Finitura`, gruppo riservato ai quattro controlli globali. Spostato in `Microfibre` senza cambiare ID, tipo, default o comportamento.
2. Il rendering `micro` usa un limite **morbido** al diametro della singola fibra basato sul passo reale (ampiezza del fascio / numero dei filamenti): `diametro_effettivo = diametro_richiesto * limite / (diametro_richiesto + limite)`. Più densità non comporta più una fusione automatica dei fili in un nastro opaco.
3. Nuovo controllo `fiber-coverage` (0.15…1.5, default 0.8): quanto largamente i filamenti riempiono il passo reale. Non agisce in modalità `classic`.
4. Nuovo controllo `pigment-contrast` (0…1, default 0.6): variazione di tono fra fili e piccole venature di pigmento lungo ciascuna fibra. Riproducibile a parità di seed.
5. `body-opacity=0` continua a escludere corpo e ombra poligonale; il corpo resta volontariamente visibile quando il parametro è maggiore di zero.
6. Nessun cambio ai 26 generatori non Crinis e alla modalità `classic`.

## Ricette per confronto A/B
Cartella: `examples/recipes/m4e3-g3-r5/`
- `crinis-user-reference-original.json`: copia byte-per-byte del JSON caricato dall'utente, **non modificata**.
- `crinis-user-reference-fine-weave.json`: stessa composizione/seed/palette indigo, base invisibile, fili meno saturi e pigmento più articolato; `fine-2x` per maggiore definizione.
- `crinis-cincinnus-deep-crimson-2000.json`: secondo studio cremisi 2000×2000 con trama separata.

## QA previsto
- Catalogo: il gruppo `Finitura` mantiene esattamente quattro controlli; i due nuovi controlli sono opzionali per i precedenti documenti JSON v1.
- Renderer: test geometrico sul nuovo calcolo del diametro; test di risposta dei parametri, seed deterministico, preview/export, cancellazione, budget.
- JSON: parsing rigoroso dei tre nuovi esempi, incluse impostazioni dell'utente prima della rifinitura.
- **Test Windows e verifica visiva R5: da confermare.** Non considerare la resa equivalente a Cincinnus senza confrontare le nuove immagini.

## Comandi PowerShell 7
Dalla cartella `GenerativeArt`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per un confronto a parità di seed, usare nello Studio i due JSON `crinis-user-reference-*.json` e verificare le differenze al 100% di zoom; controllare che la preview esatta corrisponda all'export.
