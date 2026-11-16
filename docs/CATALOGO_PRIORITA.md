# Catalogo delle capacità — cosa prendere dai due repository

**Fonti direttamente ispezionate:** `gart-main.zip`, `jdxyw-generativeart-master.zip` e la solution C# M3B.  
**Regole:** «presente Gȧrt/Go» = individuato nell'analisi dei sorgenti o dei rispettivi audit; «da sviluppare» = assente dalla solution C# M3B, non necessariamente dall'universo delle librerie .NET. L'etichetta di priorità esprime valore per il nostro prodotto, non un giudizio sui progetti originali.

| Famiglia | Capacità | Gȧrt | jdxyw Go | Nostro C# M3B | Piano |
|---|---|---|---|---|---|
| Fondamenta | Geometria 2D e colore ARGB | Sì | Sì | Sì | Consolidare |
| Fondamenta | HSV/HSL/OKLAB, gradienti sRGB/lineari | Sì | Sì | Sì | Ampliare gamut/OKLCH M4C |
| Fondamenta | Perlin, Simplex, fBm e RNG seeded | Sì | Sì | Sì | Conservare e regressioni |
| Fondamenta | Palette coseno | Non individuata | Sì | No | M3C.3 |
| Flow | Euler/RK2/RK4 | Sì | Sì | Sì | Conservare |
| Flow | Griglia bilineare, curl, streamline separation | Sì/parziale | Sì | Sì (M3B) | Benchmark M4B |
| Flow | Noise con derivate analitiche | Non uniforme | Sì | No | M4B |
| Flow | LIC | Non individuato | Sì | No | M4B |
| Noise | Periodic Simplex/fBm 4D | Parziale | Sì | No | M4B/M6C |
| Noise | Domain warp, Gabor, erosion noise | Varianti | Sì | No | M4B/M7B |
| Geometria | Marching Squares/isolinee | Non individuato | Sì | No | M4A |
| Geometria | SDF 2D + CSG | Parziale | Sì | No | M4A |
| Geometria | Voronoi/Delaunay/Poisson | Sì | Sì | No | M4A/M7B |
| Geometria | Quasicristalli, tessellazioni, geometria conforme | Varie | Sì | No | M7B |
| Pixel | Dithering, halftone, convolution | Sì | Parziale | No | M4C |
| Pixel | Effetti paper, bloom, grain, vignette | Varie | Sì | No | M4C |
| Pixel | Kuwahara anisotropico, incisione, post FX componibili | Varie | Sì | No | M4C/M4D |
| Pennelli | Brush tip, pressione, pigmenti e marbling | Sì | Sì | No | M4D |
| Output | PNG headless | Sì | Sì | Sì, validato | Conservare |
| Output | SVG/PDF realmente vettoriali | Non identificato | Sì | No | M6A/M6B |
| Output | Plotter SVG, ottimizzazione strokes | Non identificato | Sì | No | M6B |
| Output | Contact sheet/poster | Sweeper sheet | Sì | No | M5A |
| Output | PNG recipe incorporata | Non identificata | Sì | No | M5B |
| Output | HDR, tone mapping, PNG16 | Non engine autonomo individuato | Sì | No | M6D opzionale |
| Animazioni | Timeline/loop/video/GIF | Sì | Sì | No | M6C/M6D |
| Simulazioni | Particelle, fluidi, Gray-Scott | Sì | Sì | No | M7A |
| Simulazioni | Reaction-diffusion anisotropa | Non identificata | Sì | No | M7A |
| 3D | Software rasterizer, SDF, raymarching, caustiche | Parziale | Sì | No | M7C |
| Prodotto | Registro generatori | Opere esemplificative | 164 ID + alias | No | M3C.1 |
| Prodotto | Schemi parametri con vincoli | Non unitario | Reflection con limiti | No | M3C.1 esplicito |
| Prodotto | Recipe serializzabile/versionata | Non unitario | Sì | No | M3C.2 |
| Prodotto | Catalogo desktop e preview | Swing/examples | Studio web | No | M3D C# code-only |
| Prodotto | Sweeper e varianti | Sì | Poster sweep | No | M5A |
| Prodotto | Score estetico euristico | Non individuato | Sì | No | M5A opzionale |

## Tre opere pilota (R0)

1. **ParametricCurves** — evoluzione della CLI `render-demo`; vincola API path/linee, dimensioni, palette, trasformazioni. Richiama le composizioni parametriche presenti in entrambi i cataloghi. Output raster e, solo dopo M6A, eventualmente vettoriale.
2. **Plasma** — opera pixel/noise nuova per verificare `PixelBuffer`, fBm e palette, evitando accoppiamenti con Skia oltre l'output. Nella pianificazione originale Gȧrt era già identificata come opera rappresentativa.
3. **EvenStreamlines** — convergenza della CLI `render-flow-spaced`, testando RK4, griglia/curl, seed, bounds, rendering di molte linee e limiti di lavoro.

**Perché tre e non 164:** attraversano tre pipeline tecniche differenti e possono condividere lo stesso schema parametri, UI e ricette. Dopo il loro completamento, aggiungere nuove opere è un'attività ordinaria di catalogo invece di una modifica architetturale.

## Candidati per successive famiglie visualmente distinte

- **M4A**: Contours/BathymetricChart, SDF shape composition, Poisson dots, Voronoi mosaic.
- **M4B**: LIC Flow Texture, Curl Silk con derivate, Looping Noise.
- **M4C**: Halftone Print, Dithered Photography (input solo se autorizzato), Paper/Bloom.
- **M4D**: Guilloché, Pigment Marbling, Engraved Linework.
- **M7A/B/C**: Gray-Scott, Physarum, attractors, Penrose, spherical harmonics, SDF3D.

I nomi sopra sono **candidati di design**, non classi C# già implementate né impegni a riprodurre esattamente le opere originali. Prima di usare codice o asset altrui verificare licenze e attribuzioni. Non incorporare asset fotografici o opere protette soltanto perché illustrate nel README dei repository.

## Come accetteremo ogni nuova famiglia

- Un'API utilizzabile indipendentemente dalla GUI.
- Almeno un artwork registrato con preset, parametri validati, seed e smoke PNG.
- Test numerici/proprietà/casi degeneri, e golden visivi con tolleranza dichiarata.
- Misure di memoria e tempo in una matrice di benchmark appropriata.
- Documentazione di provenienza e eventuali licenze/attribuzioni.
- Nessuna duplicazione di algoritmi già presenti se una variante può essere espressa come parametro o strategia.
