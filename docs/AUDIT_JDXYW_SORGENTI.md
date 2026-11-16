# Audit sorgenti completo a livello di sottosistemi — jdxyw/generativeart

**Data:** 16 novembre 2026  
**Fonte primaria:** ZIP caricato `jdxyw-generativeart-master.zip`, contenuto repository Go al commit indicato nell'archivio (`94bc7231d62ec996d4777c6cd4429c88e3b87b7e`).  
**SHA-256 ZIP fonte:** `cff928e6e26fde9d9a28d46e097f153034e10434ba253b6e884e0b004ce0529a`.  
**Scopo:** assimilare le migliori capacità senza aggiungere Go, Kotlin o Python al prodotto C#/.NET 10.

## 1. Perimetro e metodo

- **399 file** nello ZIP, **385 sorgenti Go**, **10 file `*_test.go`**, **54 funzioni `Test...`** dichiarate; circa **81.800 righe fisiche `.go`** (inclusi commenti e righe vuote).
- `arts/` contiene **166 file Go**, tra cui il registro e un file di test: da `arts/registry.go` risultano **164 descrittori registrati**, oltre a **un alias risolvibile** con categoria. Il README dichiara 164, correttamente: l’ulteriore ID `escherdrostewave` è un alias in `Lookup()`, non un nuovo descrittore nell’array.
- Categorie nel registro: `geom` **69**, `media` **31**, `flow` **28**, `chaos` **19**, `bio` **17**. L'inventario completo è `docs/CATALOGO_JDXYW_GENERATORI.csv`.
- La libreria Go dichiara `go 1.26.4` in `go.mod`. Nell'ambiente di analisi è disponibile Go 1.23.2: **non sono stati eseguiti i test Go**, né benchmark, né confronti visivi originali. Le analisi riportate sono letture dei sorgenti e inventari statici, non certificazioni di correttezza.
- Gli elementi indicati come non identificati in Gȧrt non rappresentano prova dell'assenza assoluta di una funzione simile.

## 2. Struttura del progetto e architettura

| Parte | Funzione | Sorgente rappresentativo |
|---|---|---|
| `common/` | RNG, Simplex/Perlin 2D–4D, Worley, colori, geometria | `common/noise.go`, `common/color.go`, `common/conformal.go` |
| `render/` | Canvas lineare HDR, rasterizzazione, vector recorder, SDF, post-processing, fisica della luce | `render/canvas.go`, `render/vector_bridge.go`, `render/sdf2d.go`, `render/postprocess.go` |
| `render/field_engine.go` | Streamlines, isolinee, LIC, Poisson sampling, rilassamento CVT, reaction-diffusion | `render/field_engine.go` |
| `arts/` | Catalogo di opere; molti generatori con parametri propri | `arts/registry.go`, 164 descrittori |
| `platform.go` | GIF, APNG, PNG16, HDR, PNG recipe, SVG/PDF e animazione | `platform.go` |
| `poster.go` | Poster, studi multiseed, contact sheet | `poster.go` |
| `cmd/genart/` | CLI, randomizzazione, scoring e server studio | `cmd/genart/main.go`, `cmd/genart/studio.go`, `cmd/genart/studio_ui.go` |

**Risultato:** il Go è forte soprattutto come catalogo artistico e laboratorio compositivo, mentre Gȧrt ha già una libreria Kotlin estesa di algoritmi matematici, rendering Skia e pennelli. Il nostro prodotto è un **terzo framework**, non il porting letterale di entrambi.

## 3. Capacità di alto valore da integrare

### P0 — Fondamenta da acquisire presto

1. **Streamlines uniformemente distanziate:** `render/field_engine.go:18-195`, RK4 bidirezionale, griglia spaziale e generazione di semi ortogonali. M3B ne riprende i principi matematici in una nuova implementazione C# con budget per linee, candidati e campioni. Gȧrt include già RK4 e streamlines; non duplicare l'integratore.
2. **Derivate del noise e curl analitico:** `common/noise.go:503-647`, funzione differenziale e campi per incisioni/filamenti; il nostro M3B introduce soltanto curl **a differenze finite**. Portare le derivate analitiche in un incremento successivo, senza dichiararle già presenti.
3. **Noise per animazioni cicliche 4D:** `common/noise.go:314-477`, `FBM4DLoop` e `Curl4DLoop`. Utile quando realizzeremo timeline e GIF/APNG senza salti di loop.
4. **Marching Squares con ricucitura dei segmenti:** `render/field_engine.go:206-413`, curve di livello in noise e mappe topografiche. Va gestita la connettività dei casi ambigui e testata ogni configurazione.
5. **Registro e catalogo generatori:** `arts/registry.go` espone identificativo, categoria, palette, parametri e factory; da noi useremo metadati C# tipizzati, schema versionato, limiti e validazione (non reflection di campi interni).
6. **Recipe PNG:** `platform.go:961-1177`, parametri e seed incorporati come chunk PNG. Definire prima uno schema JSON versionato e verificare CRC, limiti di dimensione, metadata ostili e coerenza fra ID e parametri.

### P1 — Motore artistico avanzato

7. **LIC – Line Integral Convolution:** `render/field_engine.go:462-708`. Texture di flusso per meteorologia, correnti e materiali; attenzione alla complessità per pixel e passo.
8. **SDF 2D e CSG:** `render/sdf2d.go` per forma, unione, intersezione, sottrazione, smoothing, bevel e ombre analitiche. Gȧrt contiene già parti di SDF, quindi consolidare interfacce e non replicare funzioni.
9. **Colori e palette avanzate:** `render/canvas.go` e `common/color.go`, OKLAB, OKLCH/HCL, palette coseno, gamut mapping. Il nostro Core ha già OKLab, sRGB lineare e gradienti; aggiungere soltanto spazi e funzioni mancanti.
10. **Canvas HDR opzionale:** `render/canvas.go:482-550`, 4 canali `float64` per pixel in luce lineare. A 3840×2160 occupa ~253 MiB per buffer, prima di scratch, filtri e output. In C# partire da `float` con strategia esplicita precisione/memoria e confronto di qualità.
11. **Pipeline post-processing componibile:** `render/postprocess.go`: bloom, carta, vignettatura, tone mapping, aberrazione cromatica, bassorilievo e Kuwahara anisotropico. Le operazioni devono mantenere ordine e configurazione espliciti.
12. **Registrazione vettoriale, SVG/PDF e plotter:** `render/vector_bridge.go`, inclusi strokes, livelli e ottimizzazione percorso 2-opt. Per noi serve un recorder indipendente da Skia e una dichiarazione di esportabilità per ogni opera; non chiamare vettoriale un raster incorporato.
13. **Poster/contact sheet:** `poster.go`, `RenderPoster`; creare formati utili per confronto palette/seed, con salvataggio progressivo e preset.
14. **Scoring visuale e curatela:** `render/aesthetic.go`, `cmd/genart/studio.go` (comando `curate`). È una metrica euristica (contrasto, gamma, salienza, entropia), **non un giudizio oggettivo della qualità artistica**. Per la GUI suggerire ranking configurabile e anteprima umana.
15. **Pennelli e materiali:** `render/brushes.go`, `render/engraving_engine.go`, `render/pigment_engine.go`: sand line, spline, guilloché, pigmenti, incisioni. Gȧrt possiede già pennelli; sfruttare entrambi i cataloghi progettuali.

### P2 — Famiglie scientifiche e generative di specializzazione

16. `render/sdf3d.go`, `render/sdf3d_optics.go`, `render/gbuffer_engine.go`: raymarching, SDF3D, ottica dielettrica, materiali e rendering multi-buffer.
17. `render/field_engine.go:710-1282`: campionamento Poisson, rilassamento Voronoi/CVT, reaction-diffusion anisotropa e solver screened Poisson.
18. `common/noise.go:764-1054`: domain warp, flow noise, Gabor noise, erosione noise, derivate fBm 3D e curl 3D.
19. `common/conformal.go` e `common/spherical_harmonics.go`: geometria conforme, trasformazioni complesse e armoniche sferiche.
20. `arts/`: quasi-cristalli Ammann–Beenker/Penrose, mosaici, phy­sa­rum, sandpile, attractors, geometrie iperboliche, texture tessili, op art, incisioni. **Acquisire prima i motori generali**, poi selezionare opere di riferimento per verificare composizioni end-to-end.

## 4. Punti positivi da preservare

- `render/field_engine.go`: separazione campo scalare/vettoriale, integrazione RK4 e indice spaziale per streamlines.
- `common/noise.go`: superficie algoritmica ricca, noise con derivate e periodici; utile per rendere le animazioni ripetibili.
- `arts/registry.go`: catalogo centralizzato con factory, selezione palette e metadata che possono alimentare CLI e GUI.
- `render/aesthetic.go`: analisi percettiva abbastanza economica per ordinare grandi raccolte di varianti.
- `platform.go`: salvataggi multipli e recipe recuperabili dai file prodotti.
- `poster.go`: creazione di risultati pronti per una galleria o un portfolio, non solo immagini grezze.

## 5. Criticità da **non** trasferire

| ID | Evidenza nel Go | Decisione per C# |
|---|---|---|
| GO-01 | `platform.go:62-143`, `144+`: memorizza tutti i frame in `c.frames`; l'elaborazione parallela moltiplica il picco di memoria | Frame producer e encoder streaming con cancellazione e budget |
| GO-02 | `render/canvas.go:504`: dimensioni negative trasformate in zero; `w*h*4` senza un budget esplicito di memoria | CanvasSize già validato; overflow e limiti fin dalla costruzione |
| GO-03 | `cmd/genart/studio.go:421-430`: web studio predefinito su `:8080`, quindi non limitato automaticamente a localhost | GUI locale Avalonia; nessun server di rete necessario per la preview |
| GO-04 | `platform.go:1137-1171`: parser ricetta scandisce i chunk ma non verifica esplicitamente il CRC del chunk ricetta; non emerge un limite di dimensione per il JSON | Parser con CRC, limiti su dimensioni, count e profondità JSON |
| GO-05 | `arts/registry.go:36-84`: ParamSpecs da reflection sui campi della struct, senza schema dichiarativo di range e regole incrociate | Metadata C# pubblici, versionati e con vincoli per ogni parametro |
| GO-06 | README: 164 generatori; registro: 164 ID registrati (+1 alias); il messaggio di `serve` parla di 148 | Inventario automatico dal registry, test unicità ID e documentazione generata |
| GO-07 | `render/vector_bridge.go`: il vettoriale può ricadere su embedding raster | Distinguere percorso vettoriale nativo da fallback, dichiarare il tipo di output |
| GO-08 | Solo 54 funzioni test, con 164 generatori: non dimostra copertura sufficiente delle singole opere | Golden numerici, property tests, export smoke e regressioni per ogni nuovo generatore |
| GO-09 | `render/aesthetic.go`: il punteggio combina euristiche percettive | Score dichiaratamente assistivo, non sostitutivo della selezione artistica |

Questi sono rischi e deduzioni dalla lettura del codice; non sono tutti bug riprodotti tramite esecuzione.

## 6. Convenzioni definitive per il nostro progetto

- Runtime, implementazione, tests, CLI, futuro Avalonia: **solo C#/.NET 10**; `tools/validate.ps1` unico script minimo. Dati CSV/JSON e documenti Markdown consentiti.
- Core e Algorithms non dipendono da SkiaSharp/Avalonia. Nessuna introduzione di un server web per il funzionamento ordinario.
- API generate per generatori ID/version + seed + parametri, con RNG substream indipendenti dall'ordine dei task.
- Rendering deterministico con budget per memoria/coda/tile e cancellazione; disponibilità di output `PNG`, poi recipe PNG, SVG, PDF, 16-bit/HDR, GIF/APNG/video.
- Porting di sorgenti coperti da MIT deve mantenere le attribuzioni; non copiare in blocco artwork, asset o riferimenti a stili protetti da altri diritti.

## 7. Risultato della fase

**Incluso ora in M3B (solo C#):** `VectorGrid2D`, `CurlNoiseField2D` a differenze finite, `EvenStreamlineTracer` con coda/budget limitato, comando `render-flow-spaced` e test xUnit. **Da verificare ancora sul tuo Windows:** compilation/test/raster. Nessun sorgente Go importato.

**Prossimi incrementi:** M3C registry tipizzato e palette coseno; M3D contour/Marching Squares; M3E noise 4D periodico e LIC; M4 texture/SDF, effetti; M5 ricette/GUI e output vettoriale; M6 animazioni e HDR; M7 tecniche avanzate. La roadmap di progetto autorevole rimane `docs/ROADMAP.md`.
