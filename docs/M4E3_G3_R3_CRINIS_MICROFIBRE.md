# M4E.3-G3-R3 — Crinis microfibre / direzione Cincinnus

## Obiettivo artistico

I due rendering rossi Cincinnus forniti dall’utente sono il riferimento visivo di questa revisione. L’obiettivo non è una copia pixel-perfect, ma una tessitura più ricca di microfilamenti, lumeggiature discontinue e profondità di sovrapposizione, senza il nastro geometrico opaco sotto alle fibre.

## Analisi della causa del fondo residuo

Nella G3-R2 il parametro `body-opacity` controllava solo il poligono centrale, mentre i **due poligoni d’ombra dell’intero fascio** rimanevano visibili anche a zero. Ora il renderer classico disegna tali ombre solo se `body-opacity` è positivo, scalando anche la loro intensità; nel renderer micro i poligoni di fascia vengono interamente omessi a `body-opacity=0`. Le ombre delle singole fibre sono un effetto distinto.

## Nuovo percorso `detail-style=micro`

Il nuovo `CrinisMicroRenderer` è isolato dal renderer classico e non modifica gli altri 26 generatori.

- **Densità:** nuovo controllo `fiber-density`, possibilità di ridurre `bristle-pitch` a 0,55 px e `hair-width` a 0,12 px, con budget preventivo sul numero di campioni.
- **Microfilamenti:** nastro geometrico per ciascuna fibra (anziché per tutto il fascio), larghezza variabile, bordi frastagliati e interruzioni deterministiche.
- **Volume:** ombra sottile per filo, luce lungo segmenti intermittenti e gradiente di toni tra fibre e fasci.
- **Rastremazione:** parametro `fiber-taper` per far assottigliare i filamenti alle estremità.
- **Copertura della composizione:** si aggiornano le celle percorse dalle curve, non soltanto la cella d’inizio; `vortex-centers` varia i centri di curvatura. Non è ancora una simulazione completa dei campi sovrapposti di Cincinnus.
- **Granelli d’inchiostro:** `ink-flecks` inserisce puntini scuri fra i vari strati, con seed deterministico.
- **Palette cremisi:** nuova variante `crimson` con campioni Auto/HEX nello Studio.
- **Controlli Studio condizionali:** l’ombra di fascio viene disabilitata se la base è invisibile; i controlli esclusivi micro sono disabilitati nello stile `classic` senza cancellare i valori JSON.

### Controlli nuovi

| Parametro | Default catalogo | Default Studio Crinis | Intervallo |
|---|---|---|---|
| `detail-style` | `classic` | `micro` | `classic`, `micro` |
| `fiber-density` | 1 | 1.25 | 0.5–3 |
| `micro-detail` | 0.55 | 0.72 | 0–1 |
| `fiber-taper` | 0.75 | 0.85 | 0–1 |
| `fiber-shadow-depth` | 0.48 | 0.53 | 0–1 |
| `vortex-centers` | 1 | 3 | 1–8 |
| `ink-flecks` | 0 | 300 | 0–6000 |

Tutti i parametri nuovi sono opzionali nell’import JSON v1 per mantenere leggibili le vecchie ricette Crinis. Il preset Studio ora offre un Crinis predefinito più dettagliato ma non troppo costoso; le ricette già salvate conservano i valori già serializzati e utilizzano `classic` quando la proprietà è assente.

## Ricette di prova

Cartella `examples/recipes/m4e3-g3-r3`:

- `crinis-cincinnus-crimson-2000.json` — riferimento cromatico e sovrapposizioni fitte, 2000×2000.
- `crinis-cincinnus-magenta-2000.json` — microtrama più frastagliata e contrasti elevati, 2000×2000.
- `crinis-ultralight-silk-1920.json` — fibre luminose senza base opaca.
- `crinis-botanical-flax-1920.json` — look tessile vegetale.
- `crinis-copper-velvet-1920.json` — corpo appena percettibile e volume delle fibre.
- `crinis-tissue-night-1920.json` — trama fitta blu notte.

Le quattro ricette introdotte nella R2 sono state corrette eliminando la proprietà root `description`: il codec v1 rigoroso non la accettava. **Nessuna proprietà aggiuntiva non prevista compare nei nuovi JSON.**

## Limiti e test

- Massimo teorico **12 milioni di campioni di setole** con `micro`, 3 milioni con `classic`; controllo prima del rendering PNG, oltre ai limiti raster generali.
- Test predisposti: assenza di ombre a fascia quando il corpo è invisibile (in entrambe le modalità), determinismo, seed, nuovi controlli, rigetto budget, serializzazione, import degli esempi JSON, cancellazione e preview/export identici.
- Il confronto visivo a grandezza reale sui due esempi 2000×2000 **non è ancora stato convalidato** contro i reference. Non dichiarare equivalenza visiva raggiunta prima dei rendering Windows.
- Il rendering ad altissima densità può richiedere significativamente più tempo della modalità classica; per rapide esplorazioni usare la nuova impostazione Studio o una ricetta Full HD meno fitta.

## Fonte artistica

Gȧrt di Igor Spasić, `arts/lines/src/lines/cincinnus/Cincinnus.kt` e `gart/src/main/kotlin/dev/oblac/gart/brush/bristles.kt`; ulteriori informazioni e licenza BSD-2-Clause in `THIRD_PARTY_NOTICES.md` e `licenses/GART_BSD2.txt`. Reimplementazione C# autonoma dei concetti geometrici, senza importazione di sorgenti Kotlin o immagini di terze parti.
