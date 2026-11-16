# M4C.2-R1 — Carta, granulazione pigmenti e assorbimento

**Data:** 16 novembre 2026. **Stato:** codice e test predisposti; la baseline Windows più recente confermata resta M4A.4-R1. Lo sviluppo procede senza richiedere una validazione dell'utente a ogni consegna.

## Obiettivo artistico

Introdurre irregolarità organiche controllabili come se il disegno fosse realizzato su un supporto reale, senza imporre filtri irreversibili alle vecchie ricette. La texture non è un'immagine esterna ripetuta; è una funzione spaziale continua da seed e coordinate, calcolata durante il post-processing finale.

Nel pannello **MATERIA** di tutti e nove i generatori sono disponibili quattro controlli facoltativi:

| Parametro JSON v1 | Valori ammessi | Default | Significato |
| --- | --- | --- | --- |
| `paper-texture` | `none`, `cotton`, `rough` | `none` | Seleziona la struttura fibrosa del supporto |
| `paper-strength` | 0–0.4 | 0 | Quanto la struttura schiarisce/scurisce i colori |
| `pigment-granulation` | 0–0.4 | 0 | Depositi di pigmento a scale multiple, più visibili nei valori scuri |
| `ink-absorption` | 0–0.4 | 0 | Limitato trasferimento del colore scuro nei pixel vicini più chiari |

- **Cotton**: rumore interpolato a tre scale, con fibre allungate (2×24 pixel) e irregolarità morbide (36×36/8×8).
- **Rough**: grana ruvida orientata (45×18, 12×4, 2×23) con una tessitura più marcata.
- **Pigmento**: due rumori interpolati (9×9 e 2×3), pesati sulla luminanza iniziale; nessun rumore temporale durante il playback.
- **Assorbimento**: valuta il vicino originale più scuro tra quattro adiacenti; ignora i pixel trasparenti; la forza è limitata dalla differenza di luminanza. Per evitare contaminazione tra righe già modificate mantiene **tre righe originali** (O(larghezza) memoria aggiuntiva).

Non viene simulato un motore fisico di acquerello, né la dispersione del colore in coordinate geometriche: si tratta di una **approssimazione raster parametrica** per effetti materici. Gli algoritmi restano autonomi dalla GUI e dagli artwork.

## Pipeline e contratti

`ArtworkRenderEngine` valida la ricetta, controlla il limite delle finiture (**16.777.216 pixel finali da M4C.2-R3**, originariamente 4.194.304), renderizza con Skia, applica eventuale downsample 2× e passa il raster in `ArtisticPostProcessor`:

1. `MaterialPostProcessor.Apply`: supporto, granulosità e assorbimento, conservando l'alpha straight.
2. `ArtisticPostProcessor`: grana e dithering ordinato esistenti da M4C.1, se attivi.
3. Codifica PNG con percorso condiviso CLI/Studio.

L'effetto produce gli stessi pixel se coincidono seed, ricetta, dimensione e frame sorgente. La texture è **fissa nelle coordinate del canvas**, anche nei loop, ma non segue oggetti che si muovono. Questo è voluto, come un foglio di carta fermo sotto il disegno.

Le opzioni senza materiale (`paper-strength=0`, `pigment-granulation=0`, `ink-absorption=0`) evitano il passaggio materiale. Tutte le finiture completamente disattivate utilizzano il percorso di export PNG storico senza conversione aggiuntiva. Vengono mantenuti ID, versioni algoritmo, schema ricetta v1 e nomi dei comandi CLI.

**Limite importante (aggiornato R3):** i poster a 4000×4000 sono ora disponibili anche con materia/grana in modalità **standard**, per i generatori che supportano tale dimensione; consumano più RAM. In `fine-2x` il budget dei sottopixel esclude poster di 5 MP. La struttura materiale non introduce dipendenze esterne.

## Esempi da confrontare

Le cinque ricette 1920×1080 aggiunte in `examples/recipes/`:

- `parametric-cotton-ink-1920.json` — carta cotone chiara, curve color inchiostro e fine-2x;
- `topographic-rough-paper-1920.json` — topografia dal tratto disegnato e superficie ruvida;
- `poisson-cotton-pigments-1920.json` — puntinismo su supporto cotonoso e pigmenti smorzati;
- `voronoi-rough-glaze-1920.json` — mosaico con texture ruvida e assorbimento ai bordi;
- `plasma-granulated-1920.json` — pigmento granulato senza tipo di carta (controllo indipendente).

### Comandi Windows (PowerShell 7)

Nella radice del progetto, dove si trova `GenerativeArt.sln`, per compilare e provare tutti gli xUnit:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per confrontare la stessa opera con e senza effetto:

```powershell
New-Item -ItemType Directory -Force .\output | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\parametric-ink-1920.json .\output\parametric-senza-materia.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\parametric-cotton-ink-1920.json .\output\parametric-cotone.png
dotnet run --project .\src\GenerativeArt.Cli -c Release -- render-recipe .\examples\recipes\topographic-rough-paper-1920.json .\output\topografia-ruvida.png
```

**Attenzione:** per confronto rigoroso della *sola* finitura usare la stessa ricetta/seed e cambiare soltanto i quattro parametri M4C.2; i due esempi parametrici hanno anche seed e scelte cromatiche differenti.

Per aprire lo Studio:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Se i nomi di file nella cartella `output` esistono già, cambiare il nome destinazione (il writer rifiuta sovrascritture involontarie).

## Test predisposti e gate

`MaterialPostProcessorTests.cs` copre struttura deterministica e cambiamento del seed, cotton/rough, alpha invariato e pixel completamente trasparenti, assorbimento ai bordi senza contaminazione in-place, default non invasivi, eccezioni per parametri errati, parametri disponibili per nove artwork, roundtrip JSON v1 e legacy, PNG reale al 2×, budget e cancellazione. `CatalogTests` verifica anche l'ordine esatto dei nuovi parametri.

**Gate di accettazione:** build Release e suite xUnit intera; confronti visivi a pari seed prima/dopo, esame dei bordi antialiasati e delle semitrasparenze, memoria/tempo Full HD, animazione con texture ferma sul supporto, poster standard senza finitura ancora validi. Non considerare chiuso il gate sulla base della sola lettura statica.

## Prossimi passi

1. **M4C.3**: compositing per livelli, maschere e blending di pigmenti per combinazioni più controllabili; valutare texture più efficienti per grandi formati.
2. **M4B**: domain warping, nuovi campi di flusso e transizioni con loop deterministico; priorità elevata alle animazioni ispirate al repository Go.
3. **M4D**: pennelli a pressione/spessore e tracce organiche con editor più professionale.
4. **M5**: esportazione incrementale per animazione, senza memorizzare tutti i frame.
