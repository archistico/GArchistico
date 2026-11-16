# Audit comparativo — jdxyw/generativeart (Go) versus Gȧrt (Kotlin) versus GenerativeArt (C#)

**Data:** 16 novembre 2026. **Ambito:** indagine del repository pubblico `https://github.com/jdxyw/generativeart` (branch `master`) e confronto con lo ZIP completo `gart-main.zip` già studiato in A1–A5; baseline C# M3A. **Stato:** audit documentale e statico dei principali sottosistemi accessibili via GitHub; **non** clonazione/esecuzione integrale del progetto Go. Il branch remoto non è congelato ad uno SHA specifico, e quindi i dettagli vanno riverificati prima di tradurre codice.

## Risultato principale

Il repository Go corrente **non è soltanto una collezione di piccoli generatori**. Il README del branch `master` dichiara 164 tipi/opere. La documentazione e i sorgenti esaminati mostrano: canvas HDR lineare in float64, compositing e post-processing, registrazione di primitive vettoriali per SVG/PDF, SVG per plotter, PNG 16-bit, HDR RGBE, APNG e GIF, metadata PNG riproducibili, una CLI con registro di generatori, contact sheet/poster, noise periodico 4D, SDF/CSG 2D e tecniche di raymarching/3D nelle opere.

**Non significa** che Gȧrt manchi di effetti simili: possiede già Perlin/Simplex/Worley, Oklab, flow2, Voronoi, SDF parziali, bloom, pennelli, fluidi e rendering 3D. Il valore aggiunto Go è spesso nella sistematizzazione delle API, nel **percorso di esportazione** o nella **qualità delle composizioni**, non nell'esclusiva degli algoritmi.

## 1 — Architettura Go osservata

| Area | Evidenza | Cosa interessa in C# | Giudizio |
|---|---|---|---|
| `generativeart.go` / `Canva` | factory e configurazioni canvas, seed per canvas | contesto di rendering immutabile e scoped RNG | già in parte disponibile |
| `arts/registry.go` | elenco dei generatori invocabili | registry C# tipizzato con parametri e versioni | nuovo per noi |
| `render/canvas.go` | buffer lineare RGBA `float64` straight alpha; blending; registrazione vettoriale | canvas HDR opzionale e contratto compositing | importante |
| `render/postprocess.go` | pipeline post-effetti componibili, bloom e texture carta | filtri modulabili e testabili | importante |
| `render/field_engine.go` | RK4 bidirezionale, spatial grid, spacing delle streamlines, marching squares | estendere M3B | già previsto ma reference utile |
| `render/sdf2d.go` | SDF circle, box, polygon e operatori CSG | pacchetto geometry/SDF2D indipendente da Skia | nuova estensione significativa |
| `render/vector_bridge.go` | operazioni registrate e writer SVG/PDF | command recording per export vettoriale | importante |
| `platform.go` | PNG recipe, GIF/APNG, export 16-bit/HDR, temporale | output e tracciabilità degli artworks | importante |
| `poster.go` | opere composte in poster/contatti | Sweeper e galleria | nuovo prodotto |
| `cmd/genart` | comandi list, render, inspect, poster | CLI C# evolutiva | già prevista, specifica utile |

## 2 — Funzionalità da integrare: classificazione

### A. Alta priorità: funzionalità davvero mancanti nella nostra baseline

**A1. Registro delle opere con metadati tipizzati.** Introdurre un `ArtworkDescriptor` stabile (`id`, `version`, nome, categoria, parametri validati, seed policy, capability static/animated, output). Distinguere il renderer (processo) dalla definizione algoritmica (descrizione). CLI: `list`, `info`, `render` oltre a `render-demo`/`render-flow`. Non usare reflection indiscriminata dei campi: definire schema pubblico esplicito. Registrare l'origine Gȧrt / jdxyw / originale.

**A2. Ricette riproducibili e verificabili.** Una ricetta JSON versionata deve specificare artwork + versione algoritmo + seed + risoluzione + colori + parametri + pipeline effetti + versione del renderer. Possibile embedding PNG con chunk `tEXt` dopo implementazione C#, ma salvare prima JSON esterno in modo indipendente dalla codifica. Verificare CRC, dimensioni massime, charset, chunk multipli e parsing di file ostili. Import ed export non devono eseguire codice.

**A3. Composizione ed effetti.** Separare una pipeline pixel 8-bit (leggera) da un canvas lineare in float per algoritmi che beneficiano dell'HDR. Validare le formule Porter–Duff e i blend-mode con fixture numeriche; usare buffer limitati e `ArrayPool<T>` dove sensato. Prima di introdurre float64 su tutte le immagini misurare memoria/prestazioni. Riferimento: nel Go il buffer è 4×float64, quindi circa **32 byte/pixel** per il buffer principale, prima degli scratch; 3840×2160 richiede ~265 MB solo per questo buffer (8.294.400 × 32 = 265.420.800 byte, ~253 MiB).

**A4. Esportazione vettoriale SVG/PDF e plotter.** Registrare linee/path/testo come comandi vettoriali e non convertirli in bitmap per poi inserirli in SVG (che è valido ma non scalabile artisticamente). Pianificare output PDF mediante API .NET/SkiaSharp verificate; per plotter, layer per penna e percorso ottimizzato, preservando la geometria originale. Distinguere possibilità reale di export vettoriale da embedding raster fallback.

**A5. Contact sheet e poster.** Costruire composizioni di molte miniature con titolo, seed e parametri, evitando accumulo illimitato di immagini a piena risoluzione. Collegare a futura UI Avalonia e Sweeper. Nel primo MVP bastano layout regolari, ordinamento e una prova di rendering riproducibile.

### B. Alta/media priorità: algoritmi e nuove espressività

**B1. SDF 2D composabili.** Primitive circle, box, rounded box, segment, polygon, star e heart; operatori union/intersection/subtraction/smooth union; normals con gradienti e bevel/shadow basati sulla distanza. Gȧrt dispone già di `gfx/sdf.kt` con rounded box e ellipsoid; il nuovo valore è un'API 2D completa e componibile, non l'idea generale di SDF.

**B2. Marching Squares.** Isolinee subpixel da noise/campi scalari e collegamento ordinato dei segmenti; test su celle ambigue 5/10, soglia esatta, bordi, orientamento e continuità. Nello ZIP Gȧrt non risultano file di produzione denominati `marching*`; assenza del nome **non prova** assenza totale della tecnica.

**B3. Flow even spacing.** Estendere gli integratori M3A con tracing bidirezionale e griglia spaziale per mantenere la distanza `DTest`. Riferimenti sia `Gȧrt flow2` sia Jobard–Lefer nel Go. Test su campi costanti, vortici, campo zero, singolarità, determinismo, falsi positivi in spatial grid; `O(n²)` solo come baseline, ottimizzazione successiva.

**B4. Noise periodico (loop perfetti).** Integrare campionamento 4D (x,y, cos(t), sin(t)) per seam temporale, curl 4D e domain warp. Gȧrt offre noise avanzato; il porting deve giustificare un'interfaccia uniforme e seed verificati, non duplicare algoritmi esistenti.

**B5. Palette coseno e ulteriori blending percettivi.** Palette di tipo Inigo Quilez `a + b*cos(2π(c*t+d))`, parametrizzate e testate, e cache dei color stop in Oklab. C# possiede già `HslColor`, `OklabColor`, `ColorGradient`, `LinearRgba`: estendere questi contratti, non creare un sistema parallelo.

**B6. Effetti fotografici/artigianali.** Bloom in luce lineare, paper grain, vignette, deckle border, depth of field; test su alpha, edge modes, scala degli effetti, determinismo; alcuni effetti (es. bloom) esistono già in Gȧrt, ma la pipeline componibile è un miglioramento.

**B7. Plotter art / ottimizzazione percorsi.** Distinguere il disegno artistico dal percorso fisico della penna. Ordinamento tratte per colore e 2-opt su distanze pen-up, layer per colore, clipping; accuratezza e leggibilità meglio di microottimizzazione prematura.

### C. Successive / sperimentali

- 3D SDF raymarching, primitive vetro/metallo, rifrazione e caustiche, G-buffer, HDR bloom avanzato.
- 3D terreno, spherical harmonics, curvatura Weingarten, Hopf/Lorenz, lenti relativistiche: prima verificare formule, stabilità e benchmark.
- Tassellazioni Penrose, isohedral / Escher-inspired, Apollonian packings, quasicristalli; prima controllare diritti su asset e riferimenti artistici, non copiare la grafica delle opere esistenti.
- Animazione APNG, GIF con palette condivisa, temporal anti-aliasing, motion blur. Rimane obbligatoria la pipeline **streaming**, anche se il progetto Go esaminato conserva `c.frames` in memoria.

## 3 — Gap reali: niente falsi positivi

| Tema | In Gȧrt ZIP | Nel nostro C# M3A | Go | Decisione |
|---|---|---|---|---|
| Simplex / Perlin | sì | sì (M1) | sì | non duplicare |
| Worley | sì | non ancora | sì | aggiungere solo con contratto nuovo |
| Voronoi | sì | non ancora | sì | recuperare prima l'audit Gȧrt |
| Bloom | sì (Skia) | no | sì (HDR) | valutare differenza di pipeline |
| Oklab / gradienti | sì | sì (M1) | sì | completare, non rifare |
| Flow RK4 | sì (`flow2`) | sì (M3A) | sì | migliorare con spacing |
| SDF | alcune primitive | no | API 2D componibili | nuova API più ampia |
| Marching Squares | non identificato per nome | no | sì | importante |
| GIF/video | sì | no | sì GIF/APNG | streaming nel porting |
| SVG/PDF vettoriale | non identificato nei principali moduli ZIP | no | sì | importante |
| Plotter SVG | non identificato | no | sì | molto interessante |
| PNG recipe | non identificato | no | sì | alta priorità |
| Poster/contact sheets | non identificato | no | sì | alta priorità |
| HDR buffer lineare | non identificato come sottosistema dedicato | no | sì | dopo benchmark |
| Stile quasicrystal/isohedral/Escher | composizioni molto varie | no | sì | preset/opere in futuro |

*"Non identificato" è una valutazione dell'inventario disponibile, non prova matematica di assenza di qualsiasi codice equivalente.*

## 4 — Rischi tecnici e precauzioni riscontrate direttamente

1. **Memoria HDR**: `render.NewCanvas(w,h)` usa `make([]float64,w*h*4)`; non bisogna adottare indiscriminatamente questo formato su ogni preview. Serve verifica overflow e budget allocazione.
2. **Memoria animazioni**: `platform.go` in `DrawTimelapseFrames` alloca `c.frames = make([]*image.RGBA,totalFrames)` e memorizza ogni frame. Anche alcune funzioni di export costruiscono buffer di output completi; il nostro C# userà frame producer/consumer e file temporanei limitati quando necessario.
3. **Output vettoriale**: `WriteSVG` ricorre a embedding raster quando non ha command recording; distinguere chiaramente export vettoriale reale e inserimento immagine raster.
4. **Seed e concurrency**: il Go usa RNG scoped e clone per worker nelle animazioni; in C# definire sequenze indipendenti per artwork/worker/frame, non dipendere dall'ordine di scheduling.
5. **Ricette PNG**: schema, validazione, compatibilità versioni, limiti dimensioni e sicurezza di parsing; evitare serializzazione riflessiva di proprietà non volute.
6. **Parametri e API numerose**: per 164 tipi descritti, una factory monolitica genererebbe coupling; registro modulare per categoria e implementazioni a pacchetti.
7. **Portabilità e licenze**: Go MIT, Gȧrt BSD-2-Clause, SkiaSharp MIT + licenze native; preservare notizie d'autore ove si riusi codice. La nostra implementazione finale rimane **solo C# e Powershell minimo**.
8. **Non dichiarare equivalenza numerica**: i noise e i blend richiedono fixture e tolerances; il solo nome di un algoritmo non garantisce stessi pixel.

## 5 — Prove eseguite e non eseguite

- Verificata direttamente la sintassi in `FlowDemo.cs` (originale ZIP M3A) e applicata la correzione univoca `(i % 3)` in M3A-R1.
- Lettura diretta via GitHub di `render/canvas.go`, `field_engine.go`, `sdf2d.go`, `postprocess.go`, `vector_bridge.go`, `platform.go`, `poster.go`, `arts/registry.go`, `cmd/genart/main.go`, della roadmap e della licenza MIT.
- Verificata nel nostro archivio Gȧrt la presenza di `gfx/sdf.kt`, `fx/bloom.kt`, Worley, Voronoi, flow2 e colori.
- **Non** disponibile un checkout integrale Go nel contenitore (rete diretta bloccata); nessun `go test`, nessuna misura prestazionale né verifica visiva dei 164 generatori.
- SDK .NET non presente nel contenitore; nessuna build/test M3A-R1 eseguita qui. Il log Windows utente riguarda ancora M3A prima del fix.

## 6 — Sorgenti consultati (link diretti)

- Repository + presentazione delle 164 opere: https://github.com/jdxyw/generativeart
- Roadmap dettagliata: https://github.com/jdxyw/generativeart/blob/master/docs/roadmap.md
- Canvas HDR/blending: https://github.com/jdxyw/generativeart/blob/master/render/canvas.go
- Postprocessing: https://github.com/jdxyw/generativeart/blob/master/render/postprocess.go
- Campi e isolinee: https://github.com/jdxyw/generativeart/blob/master/render/field_engine.go
- SDF2D/CSG: https://github.com/jdxyw/generativeart/blob/master/render/sdf2d.go
- Vector bridge: https://github.com/jdxyw/generativeart/blob/master/render/vector_bridge.go
- PNG/GIF/APNG/HDR: https://github.com/jdxyw/generativeart/blob/master/platform.go
- Registro delle opere: https://github.com/jdxyw/generativeart/blob/master/arts/registry.go
- CLI: https://github.com/jdxyw/generativeart/blob/master/cmd/genart/main.go
- Poster: https://github.com/jdxyw/generativeart/blob/master/poster.go
- Licenza: https://github.com/jdxyw/generativeart/blob/master/LICENSE

## 7 — Conclusione

Adottare **due fonti d'ispirazione complementari**, non due runtime: Gȧrt resta forte su strumenti matematici/artistici eterogenei; jdxyw/generativeart porta idee ottime per prodotto finale (catalogo, recipe, export, poster) e sistemi di rendering precisi (HDR, SDF, vector, contour, effetti). Il C# già validato non deve essere rimpiazzato. Prima stabilizzare M3A-R1, poi svolgere M3B senza duplice implementazione di RK4; entrare nei nuovi moduli con quality gate e benchmark.
