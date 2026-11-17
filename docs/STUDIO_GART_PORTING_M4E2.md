# Studio sorgenti Gȧrt → GenerativeArt Studio — Analisi tecnica e roadmap artistica

**Data:** 17 novembre 2026  
**Stato:** studio/decisione architetturale, **nessuna modifica ai generatori C#**  
**Baseline C#:** GenerativeArt Studio **M4E.2-R5**, compilazione e test Windows confermati dall'utente.  
**Fonte:** archivio caricato `gart-main-pulito(1).zip`, root `gart-main/`, senza JPG/PNG delle opere.  
**Vincoli:** implementare solo in **C#/.NET 10 con SkiaSharp e Avalonia**; XAML/AXAML consentito; niente dipendenze Java/Kotlin/Go/Python. Non alterare i 17 preset personali, il formato ricetta v1 o gli output storici.

## 1. Sintesi per le decisioni

- La directory `arts/` contiene **297 sorgenti `.kt`**; non si tratta di 297 algoritmi indipendenti. La libreria Gȧrt in `gart/src/main/kotlin/dev/oblac/gart/` contiene **384 sorgenti `.kt`**, fra motori, utility, tipi e supporti. Le opere sono programmi dedicati, spesso composti da più algoritmi e funzioni di rendering.
- Il nostro Studio contiene **20 generatori** e **111 ricette** al baseline. Possiede già noise, domain warp, flussi, RK, streamlines con separazione, Poisson, Voronoi, SDF, booleane, Gray–Scott, fillotassi, finiture e alcune forme di profondità. Il **riuso** è quindi alto per una parte del catalogo, ma non per i nuovi motori sottostanti a Crinis, Fascia, Rete, Ocellus, Accretion e Nimbus.
- La massima resa artistica non deriva dal porting indiscriminato di `main()` Kotlin: conviene creare **kernel riutilizzabili** separati dalla presentazione, seguiti da ricette stilistiche. Ogni ricetta deve conservare seed, parametri e dimensioni e usare una sola pipeline per preview ed export.
- I primi investimenti consigliati sono: (A) **grammatica geometrica e stile** per Alien Letters V3/Cotton Circles/Rigor; (B) **Bristles/Spine** per Crinis e Flamebrush; (C) **Flow Bands** per Fascia, Eclectic, Glst e Cyanowaves; (D) **campi organici** per Ocellus/Rete; (E) **crescita per agenti** per Accretion e filoni futuri.
- **Nessuna promessa di identità visiva pixel-perfect:** molte opere Gȧrt usano RNG impliciti, palette, ordine di composizione, filtri e impostazioni di campionamento differenti. La fedeltà si verifica con reference PNG che non sono inclusi nell'archivio consegnato.

## 2. Metodo e affidabilità della ricostruzione

Analisi diretta degli `.kt` sotto `arts/` e dei kernel sotto `gart/src/main/kotlin/dev/oblac/gart/`. Sono state esaminate implementazioni, commenti tecnici, chiamate a Skia e parametri; confronto con `GenerativeArt.Algorithms`, `GenerativeArt.Rendering.Skia` e catalogo `GenerativeArt.Application` della R5. I collegamenti di `README.md` di Gȧrt rimandano a PNG assenti dall'archivio: dunque i nomi delle varianti e le tecniche sono verificabili, **la somiglianza visiva definitiva no**. Il documento `docs/RIFERIMENTI_ARTISTICI_PREFERITI.md` del nostro progetto elenca **48 preferenze Gȧrt** e indica **Crinis** come una preferenza principale.

**Gradi di certezza:** **C** = corrispondenza sorgente/tecnica direttamente verificata; **V** = sorgente della famiglia verificato, ma variante di galleria non associabile con certezza; **I** = proposta nostra d'implementazione (non presente come tale in Gȧrt).

## 3. Anatomia tecnica Gȧrt e corrispondenza C#

| In Gȧrt | Responsabilità nel nostro C# | Stato e osservazioni |
|---|---|---|
| `Gartvas` / `Gartmap` / `Pixels` | `SkiaRasterSurface`, `SkiaPixelAdapter`, `Core.PixelBuffer` | Concetti già presenti; evitare conversioni dell'intero raster a 8K. |
| `noise`, `OpenSimplexNoise`, `PerlinNoise` | `Algorithms/Noise` | Già presenti, ma formule e seed vanno confrontati per una replica fedele. |
| `flow2/VectorField.kt`, `Streamlines.kt`, `Integrator.kt` | `Algorithms/Flow` | Curl/vortex, RK, streamlines e vincoli spaziali già presenti; da estendere alle **bande con spessore variabile**. |
| `gfx`/`Skia` paths, booleani, curve | `Algorithms/Geometry` + renderer Skia | Presenti in parte; mancano una grammatica di tasselli e un rasterizzatore di ribbon capace di stampare pattern locali. |
| `brush/bristles.kt` | **Nuovo** `Algorithms/Brushes` e Skia `BristleRenderer` | Base necessaria per Crinis, Flamebrush, Monet2. Non equivale all'attuale semplice Flow Brush. |
| `marbling/Marbling.kt` | Futuro `Algorithms/Marbling` | Trasformazioni inverse bijettive, campionamento indipendente per pixel; interessante per render tile/8K. |
| `colonization/SpaceColonization.kt` | Futuro `Algorithms/Growth` | Motore di crescita di venature, alberi e fulmini su attrattori; vicino ma distinto da DLA. |
| `physarum/Physarum.kt` | Futuro `Algorithms/Agents` | Campo di feromoni e simulazione per agenti; non è una variante di Gray–Scott. |
| `pack/CirclePacker.kt`, `hashgrid/BucketGrid` | Nuovi utility `SpatialIndex2D` / packer | Importanti per Rete e composizioni con collisioni e ampie risoluzioni. |
| `reactiondiffusion/GrayScott.kt` e altri modelli | `ReactionDiffusionField2D` | Gray–Scott già implementato, FitzHugh–Nagumo e Belousov–Zhabotinsky non equivalenti; da valutare dopo QA. |
| `color/space` (Oklab/Oklch) e palette | `Core/Colors`, catalogo | Ruoli colore personalizzati già presenti; Oklab/Oklch da valutare per gradienti più regolari. |
| `dither/*`, `halftone/CMYKProcessor.kt` | Finiture artistico/scanline | Bayer/grana presenti; stampa CMYK e diffusione errore ancora opportunità distinte. |
| `march`, `attractor`, `fluid`, `ocean` | Futuri moduli specifici | Alto costo e spesso buffer estesi; non anticipare animazioni. |

### Regole progettuali per ogni nuovo algoritmo

1. Un tipo/simulatore puro in `Algorithms` con RNG **esplicito** e limiti su lavoro/memoria; nessun uso di `Random.Shared`, `DateTime.Now` o funzioni non seeded che cambino i risultati.
2. Un renderer in `Rendering.Skia` condiviso da Studio e CLI, con controllo cancellazione e separazione tra **coordinate logiche** e dimensioni output.
3. Un descrittore/catalogo in `Application` con opzioni classificate (geometria, composizione, stile, colore, finitura), metadata sulle dipendenze dei parametri e `artworkId` stabile.
4. Preset predefinito nuovo, **almeno tre ricette** artisticamente differenti per ogni algoritmo sostanziale, incluse prove Full HD, 4K e 8K soltanto quando l'algoritmo supera il budget.
5. Test su determinismo, variazione significativa dei parametri *applicabili*, output e alpha, validazione JSON, cancellazione, risorse e confronto preview/export.
6. Documentare esplicitamente differenze rispetto a Gȧrt, valore degli asset mancanti e copyright/licenza.

## 4. Porting delle opere Gȧrt preferite

**Chiave tipologia:** `N` = nuovo generatore; `E` = estensione di quello esistente; `R` = ricetta/stile che riusa componenti, talvolta con nuove primitive; `P` = posticipare. Le etichette sono **nostre raccomandazioni**, non una classificazione originale di Gȧrt.

| Opera | Riscontro diretto dal Kotlin | Operazione C# | Priorità |
|---|---|---|---|
| **Alien Letters V3** | `arts/alien/src/alien/AlienLetter.kt`: griglia 4×4, sottocelle, omissioni, estensione rettangoli e palette V3. | **N piccolo** `tile-glyphs`: grammatica/shape/occupazione; 3–5 ricette tipografiche. | **Alta / rapida** |
| **Flamebrush1** | `arts/bubbles/src/flamebrush/Flamebrush.kt`: spazzola di punti animata lungo path circolari, scala e colore variabili. | **E** Bristles/Spine condiviso con Crinis; ricette spiral brush. | Alta |
| **Ocellus** | `arts/bubbles/src/ocellus/Ocellus.kt`: occhi sovrapposti in livelli SDF, fusione `smin`, profili colore e correzione specks via flood-fill. | **N** `layered-eyespots`: SDF pesato e topologia regioni; non basta `sdf-orbits` con altri colori. | **Altissima** |
| **Pebble2** | `arts/bubbles/src/bubbles/pebble/Pebbles.kt`: unione di dischi, Catmull–Rom, righe tratteggiate e contorni concentrici. La variante *Pebble2* della galleria non è identificabile con certezza. | **N/E** `soft-pebbles`, clipping paths, pattern; nuova combinazione. | Media |
| **Accretion** | `arts/cell/src/accretion/Accretion.kt`: diffusion-limited aggregation, walkers su griglia toroidale, età di congelamento, smoothing e bloom. | **N** `diffusion-aggregation`, simulazione per griglia e output separato. | Alta, costo elevato |
| **Rete** | `arts/cell/src/rete/Rete.kt`: circle packing multiscala, **Apollonius** (distanza alla superficie), muri tra celle, vortice/drain, più piani sfocati. | **N** `foam-rete`, spatial hash, distanza pesata, differenza fra superfici vicine e depth. | **Altissima**, complesso |
| **Nimbus** | `arts/clouds/src/nimbus/Nimbus.kt`: cumulo di sfere 3D con intersezioni, test di ombra/luce e ray tracing per pixel. | **N** `volumetric-clouds`: modello 3D/luce, render tile per 8K. | Alta artisticamente, tardiva tecnicamente |
| **Cotton Circles** | `arts/cotton/src/cotton/circles/CottonCircles.kt`: tasselli 10×10, semicerchi di archi su rettangoli, pigmento noise. | **R** sopra `tile-glyphs` + texture locali. | **Alta / rapida** |
| **Cotton Circles2** | `arts/cotton/src/cotton/circles2/CottonCircles2.kt`: cerchi concentrici, archi e particelle mosse da CircularFlow. | **R** versione statica per fase fissa; parte dinamica più avanti. | Media |
| **Eclectic / Eclectic2** | `arts/flowforce/src/flowforce/eclectic/...`: trail su Perlin con stop alla collisione fra tracciati, spessori elevati e cerchi di interruzione. | **E** streamlines con collisione a spessore variabile; `flow-ribbons` e ricette. | Alta |
| **Spring** | `arts/flowforce/src/flowforce/spring/Spring.kt`: `WaveFlow` e crescita temporale di trails. | **P/R** possibile poster a fase fissa, l'opera nativa è dinamica. | Dopo A/B/D |
| **Cyanowaves** | `arts/flowforce/src/flowforce/cyanowaves/CyanoWaves.kt`: streamlines Simplex trasformate in sequenze di punti colorati. | **E/R** nuovo stroke style `gradient-dots` per le streamlines. | **Alta / rapida** |
| **Fascia / Fascia Pal5 Seed30** | `arts/flowforce/src/fascia/Fascia.kt`: vortici + drift, streamlines in bande spesse, tagli, dissolvenza con **retinatura spaziale locale**, non dither globale. | **N** `flow-bands`: rasterizzazione di strisce e asse longitudinale, offset/fade seeded. Preset **pal=5 seed=30** documentato. | **Altissima** |
| **Glst2** | `arts/flowforce/src/flowforce/glst/glst.kt`: tracciati a tre densità, pesi 58/14/1,5 e esclusione collisioni tramite outline. La variante *Glst2* non è attribuibile con certezza. | **E/R** multi-scale streamlines con masks/collisioni. | Alta |
| **Monolith** | `arts/flowforce/src/flowforce/monolith/monolith.kt`: flusso Simplex, backbone, maschera rettangolare, distanza per colore, effetti dilate e punti. | **R/E** streamlines + forma di occlusione/mask; anticipo di alcune primitive della Fase B, **non** compositore universale. | Media |
| **Perl 1** | `arts/flowforce/src/flowforce/perl/Perl.kt`: streamlines e punti, larghezza variabile secondo x, composizione con cerchio; variante *Perl 1* non certa. | **R/E** pennellata variabile e interruzione dei tratti. | Media |
| **Vorflow** | `arts/flowforce/src/flowforce/vorflow/Vorflow.kt`: Poisson → triangoli Delaunay → celle Voronoi → campo che attrae particelle ai bordi; advezione di 5.000 punti. | **N/E** `voronoi-advection`: riusa Voronoi e Flow, ma crea un campo di bordo nuovo con indice di segmenti. | Alta |
| **Fluid Wind** | `arts/fluid/src/wind/FluidWind.kt`: `FluidSolver`, 50.000 particelle, forze di vento su Perlin variabile nel tempo, scie. | **P** simulazione fluidi vera (non semplice flow noise); animazione in futuro. | Rinviata |
| **February** | `arts/hills/src/hills2/Hills2.kt`: 16 layer di colline, sole e Poisson stipple condizionato alla regione. | **R/E** `strata-landscape` con maschere regioni e punti. | **Alta / rapida** |
| **Rigor** | `arts/igor/src/rigor/Rigor.kt`: griglia modulare con 4 glifi, variazioni geometriche, offset/rotazioni locali e supersampling. | **R/E** `tile-glyphs`; testo parametrico senza dipendenze da caratteri originali. | Alta |
| **Sabulum** | `arts/layers/src/sabulum/Sabulum.kt`: dune a curve armoniche e livelli profondità, fronti, toni, effetti di luce e carta. | **E** `strata-landscape`: dune nuove, family palette, shadow e point/pigment. | Alta |
| **Strata** | `arts/layers/src/strata/Strata.kt`: famiglie di superfici ondulate, apertura centrale, ellisse/orbe, ombre e soft paper. | **E** `strata-landscape` con nuova modalità multi-famiglia e orb. | **Alta** |
| **Undula** | `arts/layers/src/undula/Undula.kt`: nastri di onde a rilievo, piani laterali scuri e shader lineari. | **N/E** `relief-ribbons`, possibili primitive riusabili dal futuro compositore. | Media |
| **LetterO** | `arts/lettero/src/lettero/Lettero.kt`: reticolo tipografico ripetuto e opera a frame con lettere diverse. | **R** variante poster statica per seed/stringa; timeline in futuro. | Media |
| **Citymap** | `arts/lines/src/lines/CityMap.kt`: crescita segmentale iterativa con ramificazione e limite trial/bordi. | **N** `street-growth`: grafo stradale seeded, collisione e gerarchia strade. | Alta |
| **Crinis** *(preferito importante)* | `arts/lines/src/lines/crinis/Crinis.kt` + `gart/.../brush/bristles.kt`: spina dorsale curvilinea; centinaia di setole per banda; selezione delle aree poco occupate; ombre fra bande, supersampling e grana. | **N** `bristle-bands`; prima costruire `Spine`, `Bristles`, soft shadow e copertura. Non sostituibile con Flow Brush attuale. | **Altissima** |
| **TapesB** | `arts/lines/src/lines/tapes/TapesB.kt`: nastri da path chiusi, trasformazione in segmenti/quadrilateri e clipping/grunge. | **E/N** ribbon geometry; attenzione alle varianti non seeded come `currentNanoTime`. | Media |
| **Monet2** | `arts/monet/src/monet/Monet2.kt`: strati di poligoni deformati e alpha, campo complesso e diffusione di pennellate. | **E** Bristles/brush + style multilayer; ordine di compositing deterministico. | Media |

**Ulteriori preferenze nell'archivio storico del progetto:** `Corona`, `Sigillum`, `Shad`, `Craticula`, `Mondrian 03`, `Sixsix2`, `Perspective02`, `Skyscraper2`, `Nidus`, `Plasma3`, `Spuma` ecc. Sono candidate alla seconda campagna di classificazione; evitare di dichiararle implementate per sola somiglianza con un generatore corrente.

## 5. Nuove capacità nella libreria Gȧrt oltre alle opere favorite

1. **Marbling invertibile** — `gart/src/main/kotlin/dev/oblac/gart/marbling/Marbling.kt`: gocce/tines/comb come trasformazioni del piano; ogni pixel può essere determinato ripercorrendo all'indietro la sequenza. Adatto a tile rendering ad alta risoluzione, ma costo per pixel dipendente dal numero di operazioni.
2. **Space Colonization** — `.../colonization/SpaceColonization.kt`: attrattori e nodi cresciuti in `OPEN`/`CLOSED` con indice uniforme; utile per radici, foglie, neuroni e lightning.
3. **Physarum** — `.../physarum/Physarum.kt`: agenti con sensori direzionali e doppio campo di diffusione/decadimento; reti emergenti differenziate da DLA e Gray–Scott.
4. **Attractors** — `.../attractor/`: Clifford, Peter de Jong, Lorenz, Rossler, Thomas e altri. Candidate per arte con milioni di punti e densità; budget agenti/steps e binning necessario in 8K.
5. **Dither/halftone avanzati** — `.../dither/`, `.../halftone/`: algoritmi differenti da Bayer, incluso CMYK; utile quando la roadmap D arriverà a stampe/fogli contatto.
6. **Reaction diffusion estesa** — `.../reactiondiffusion/`: oltre Gray–Scott ci sono `FitzHughNagumo` e `BelousovZhabotinskyContinuous`, nuove strutture chimiche ma costo di iterazione da misurare.
7. **Circle packing e weighted cells** — `.../pack/`, `.../hashgrid/`: utile come infrastruttura condivisa per Pebble, Rete e future tessellazioni.

## 6. Matrice delle dipendenze: non basta una ricetta

| Famiglia | Possiamo già creare una ricetta nuova? | Serve davvero nuovo codice? | Motivo |
|---|---|---|---|
| Cyanowaves, February, parte di Monolith e Perl | **Parzialmente** | Sì, pochi nuovi `stroke-style`/mask | Le traiettorie/colline esistono, non lo styling esatto. |
| Alien Letters V3, Cotton Circles, Rigor | No, non fedelmente | Sì, **tile grammar** | Oggetti in celle, omissioni/merge/orientamento sono regole nuove. |
| Crinis, Flamebrush | No | Sì, `Spine` + bristle sampling + stroke compositor | Centinaia di fibre coordinate attorno a una stessa spina. |
| Fascia, Eclectic, Glst | No | Sì, `FlowBands` e `DistanceToPolyline` con spatial index | La larghezza, clearance e dissolvenza per tratto sono la vera forma dell'opera. |
| Ocellus, Rete | No | Sì, SDF gerarchici/Apollonius + cleanup | Il rendering della geometria cambia secondo *layer* e prossimità. |
| Accretion, Citymap | No | Sì, modelli di crescita | La forma evolve secondo regole discrete; non si ottiene da rumore statico. |
| Nimbus, Fluid Wind | No | Sì, ray tracing/solver | Modelli volumetrici o dinamici e budget differenti. |

## 7. Ordine consigliato per i prossimi incrementi A3 (senza anticipare animazioni)

| Incremento proposto | Motore/utility prima delle ricette | Opere target | Ricette iniziali proposte | Gate specifico |
|---|---|---|---|---|
| **A3-G1 – Geometric Tile Grammar** | `TileLayout`, `TileGlyph`, occupancy/merge, random seeded | Alien Letters V3, Cotton Circles, Rigor | 4–6: Alien V3 reinterpretato, grafo tipografico, Cotton warm, Rigor rust | JSON, layout e seed, bounds/overlap, composizione 4K |
| **A3-G2 – Fibres & Bristle Bands** | `Spine`, `BristleStroke`, curvature sampler, mask shadow | **Crinis**, Flamebrush, Monet2 | 4–6: Crinis blue, ember, monochrome, spiral brush | Fidelity a risoluzioni diverse, no allocazioni per setola, determinismo |
| **A3-G3 – Flow Bands** | linee multi-scala, arc-length, clearance e spatial hash, disegno nastri, halftone locale | **Fascia** seed30/pal5, Eclectic, Glst2, Cyanowaves | 5–7: Fascia pal5 seed30, monochrome, Eclectic, Glst, cyanotype | Distanza, layer, alpha, hash stabili fra tile, 8K budget |
| **A3-G4 – Layered SDF Foam** | `SignedDistanceUnion`, `ApolloniusCells`, packing multi-grid, flood-fill specks | **Ocellus**, **Rete**, Pebble | 4–6: occhi pastello, foam nero, foam paper, fondale | Campi/contorni, anti-alias, ROI/tile, memoria/tempo |
| **A3-G5 – Organic Growth** | `DiffusionLimitedAggregation`, `SpaceColonization`, `StreetGrowth` | Accretion, Citymap, venature | 4–6: fern colonies, roots, arterial city, ink branches | Terminazione, costo bounded, scala indipendente dall'output |
| **A3-G6 – Material Landscapes & Volumes** | layer armonici, silhouette e ombre, raymarching ottimizzato | Sabulum, Strata, Undula, Nimbus | 4–6: dunes, orb cut-paper, relief, cloud | 4K/8K readiness distinta per stile; non forzare Nimbus 8K |

**Scelta operativa consigliata:** partire da **G1** per validare rapidamente il contratto ricetta/Studio; sviluppare subito dopo **G2 Crinis** e **G3 Fascia**, che massimizzano il salto artistico e producono componenti riusabili. G4 e G5 richiedono più attenzione a complessità e memoria.

## 8. Stima complessità, rischi e 8K

- **Alta risoluzione:** 7680×4320 = 33.177.600 pixel; una superficie BGRA 32-bit occupa circa **126,6 MiB**, prima di copie e filtri. Con quattro buffer l'ordine di grandezza supera 500 MiB. Il formato 8192² impiega **256 MiB per singolo buffer**.
- **Non tutti i motori devono usare lo stesso profilo:** tile grammar/brush geometrici possono usare Skia; Rete/Nimbus/Accretion richiedono strategie di campionamento, indici spaziali, simulazione a risoluzione logica e render per blocchi/bande.
- **8K e post-processing:** nel nostro Studio è già iniziato il percorso per righe, ma raster indipendenti, buffer dell'alpha, flood-fill su campo completo e supersampling richiedono un budget separato e QA reale.
- **Riproducibilità:** portando una tecnica Gȧrt spesso è meglio ricostruirne regole matematiche e design che replicare esattamente RNG Kotlin, colori o pixel. Se si richiede compatibilità pixel-perfect, diventa necessario un porting dedicato del RNG, delle palette e del campionamento.
- **Concorrenza e cancellazione:** per le simulazioni DLA/Physarum non aggiungere parallelismo che alteri l'ordine dei depositi senza test di determinismo. Le elaborazioni cancellate non devono sovrascrivere immagini già esportate.
- **Fasi B e D restano successive:** sviluppare i nuovi generatori come renderer indipendenti, ma preparare metadati per blend, maschere, layers e SVG futuri. Non trasformare le singole opere in un compositore universale prematuro.

## 9. Protocollo di confronto artistico (prima di dichiarare una ricetta "fedele")

Per ogni opera scelta:

1. Registrare riferimento Gȧrt esatto (file sorgente, variante, parametri, seed se presente, licenza) e acquisire PNG dal riferimento **solo se disponibile**. Il repository caricato non contiene i PNG.
2. Creare una ricetta `examples/recipes/gart-studies/<famiglia>/...json` con nome descrittivo e campo di ispirazione nella documentazione, non come ID del generatore.
3. Misurare caratteristiche visive: densità, distribuzione spaziale, curvatura, spessore, componenti/ramificazioni, quantità di vuoti, colori, bordo e profondità. Evitare di promettere pixel equivalenti quando la tecnica è soltanto stilisticamente affine.
4. Eseguire almeno tre seed e almeno due aspect ratio; produrre un PNG Full HD e un PNG in dimensione superiore solo se ammessa dal budget.
5. Verificare parametri **realmente efficaci**, non chiedere che un controllo inapplicabile (es. frequenza di un campo vortex) cambi il raster.
6. Conservare il 100% delle ricette precedenti, 17 preset personali e baseline 20 generatori senza modifica, a meno di nuova versione dell'algoritmo documentata.

## 10. Licenze e attribuzione

`gart-main/LICENSE` è **BSD 2-Clause**, copyright **2022 Igor Spasić**. Nel caso di copia/adattamento di codice, mantenere il copyright, il testo di licenza e gli avvisi richiesti (in sorgenti/`licenses`/`THIRD_PARTY_NOTICES.md` secondo la modalità di distribuzione). Gȧrt dichiara inoltre dipendenze e palette da progetti terzi nel README: **non assumere automaticamente** che ogni palette, asset, font o dipendenza sia coperto soltanto dal BSD di Gȧrt. Per le opere originali e un'implementazione indipendente, documentare correttamente l'ispirazione.

## 11. Decisioni richieste? Nessuna: proposta pronta per la Fase A3

La fase corrente è **ricerca e progettazione**, senza modifiche agli algoritmi o ai preset del software. Il prossimo incremento consigliato è **A3-G1**, poi G2 e G3. Parallelamente, tenere aperta A4 8K e A5 benchmark; **non anticipare** compositore universale (B), workflow artistico (D) o animazioni avanzate.

### File sorgenti chiave per la ripresa

- Gȧrt: `arts/alien/src/alien/AlienLetter.kt`, `arts/lines/src/lines/crinis/Crinis.kt`, `gart/src/main/kotlin/dev/oblac/gart/brush/bristles.kt`, `arts/flowforce/src/fascia/Fascia.kt`, `arts/flowforce/src/flowforce/eclectic/`, `arts/bubbles/src/ocellus/Ocellus.kt`, `arts/cell/src/rete/Rete.kt`, `arts/cell/src/accretion/Accretion.kt`, `arts/clouds/src/nimbus/Nimbus.kt`.
- GenerativeArt R5: `src/GenerativeArt.Algorithms/{Flow,Geometry,Pattern}`, `src/GenerativeArt.Rendering.Skia/`, `src/GenerativeArt.Application/Catalog`, `docs/ROADMAP.md`, `docs/CURRENT_STATE.md`, `docs/RIFERIMENTI_ARTISTICI_PREFERITI.md`.

### Consultazione visiva del riferimento originale

Le immagini non sono nello ZIP `gart-main-pulito(1).zip`, ma la galleria online del repository originale ne presenta i titoli e i collegamenti: **https://github.com/igr/gart**; la pagina di progetto elenca anche **https://igo.rs/gart**. Utilizzare le immagini ufficiali come **riferimenti per la verifica**, non come rendering generati dal nostro Studio. Le conclusioni matematiche di questo studio sono tratte dal codice dell'archivio allegato, non da immagini di terzi recuperate tramite ricerca generica.
