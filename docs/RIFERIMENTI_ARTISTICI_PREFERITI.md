# Riferimenti artistici selezionati dall’utente — P2.1

**Aggiornamento H2, 17/11/2026:** ora sono disponibili lo ZIP sorgente originale `gart-main-pulito(2).zip` e la [galleria GitHub Gȧrt](https://github.com/igr/gart). I percorsi precisi a 28 immagini preferite e le note di verifica sono nel [documento sulle fonti originali](GART_RIFERIMENTI_ORIGINALI_2026-11-17.md). Le immagini non sono incluse nel pacchetto Gȧrt allegato.

**Data:** 16 novembre 2026. **Natura:** documento di progettazione; non aggiunge nuovi generatori C#.

## Interpretazione delle preferenze

- Le 48 denominazioni Gȧrt e le 4 denominazioni Go sono **riferimenti visivi esplicitamente scelti**, non un elenco già implementato.
- **Crinis** è stato indicato come uno dei preferiti ed è il **benchmark estetico prioritario** per i pennelli complessi.
- Le altre denominazioni non sono ordinate per preferenza; non inferire una classifica dalla posizione.
- **Sixsix2** e **Sixsix 2** sono entrambe preservate come scritte dall’utente; puntano al medesimo file Kotlin, ma si confermerà che sia lo stesso output visivo.
- Quando il nome contiene un suffisso o un preset che non si risolve direttamente nel codice, il collegamento è indicato come **candidato**, non come identità dimostrata.
- Gli ZIP originali caricati non contengono PNG/GIF/MP4; per verifiche di somiglianza cromatica e compositiva serviranno immagini originali o rendering comparativi.

## Catalogo Gȧrt (48 voci)

| Nome come indicato | Riferimento sorgente Kotlin | Identificazione | Tecnica/riuso per C# | Area roadmap |
|---|---|---|---|---|
| Alien Letters V3 | `arts/alien/src/alien/AlienLetter.kt` | Verificato: variante V3 | glifi e pattern tipografici | M4 |
| Flamebrush1 | `arts/bubbles/src/flamebrush/Flamebrush.kt` | Verificato: titolo generato | pennelli a spirale e particelle | M4 |
| Ocellus | `arts/bubbles/src/ocellus/Ocellus.kt` | Sorgente omonimo | forme organiche | M4 |
| Pebble2 | `arts/bubbles/src/bubbles/pebble/Pebbles.kt` | Candidato: variante 2 da confermare | forme morbide e spline | M4 |
| Accretion | `arts/cell/src/accretion/Accretion.kt` | Sorgente omonimo | aggregazione limitata da diffusione (DLA) | M7 |
| Rete | `arts/cell/src/rete/Rete.kt` | Sorgente omonimo | reti di dischi e campi di distanza | M4 |
| Nimbus | `arts/clouds/src/nimbus/Nimbus.kt` | Sorgente omonimo | nuvole e volume/luce | M7 |
| Cotton Circles | `arts/cotton/src/cotton/circles/CottonCircles.kt` | Sorgente omonimo | cerchi e texture | M4 |
| Cotton Circles2 | `arts/cotton/src/cotton/circles2/CottonCircles2.kt` | Sorgente omonimo | curve circolari e flow | M4 |
| Eclectic2 | `arts/flowforce/src/flowforce/eclectic/two/Eclectic2.kt` | Sorgente omonimo | flow field e linee | M4 |
| Eclectic | `arts/flowforce/src/flowforce/eclectic/Eclectic.kt` | Sorgente omonimo | flow field e linee | M4 |
| Spring | `arts/flowforce/src/flowforce/spring/Spring.kt` | Sorgente omonimo | flussi e movimento | M6 |
| Cyanowaves | `arts/flowforce/src/flowforce/cyanowaves/CyanoWaves.kt` | Sorgente omonimo | flow field e palette cianotipo | M4 |
| Fascia Pal5 Seed30 | `arts/flowforce/src/fascia/Fascia.kt` | Verificato: parametri pal=5, seed=30 | bande di flusso e dithering | M4 |
| Fascia | `arts/flowforce/src/fascia/Fascia.kt` | Sorgente omonimo | bande di flusso e dithering | M4 |
| Glst2 | `arts/flowforce/src/flowforce/glst/glst.kt` | Candidato: variante 2 da confermare | streamlines e grafica di linee | M4 |
| Monolith | `arts/flowforce/src/flowforce/monolith/monolith.kt` | Sorgente omonimo | flow field, composizione e maschere | M4 |
| Perl 1 | `arts/flowforce/src/flowforce/perl/Perl.kt` | Candidato: variante 1 da confermare | noise e streamlines | M4 |
| Vorflow | `arts/flowforce/src/flowforce/vorflow/Vorflow.kt` | Sorgente omonimo | Voronoi e flussi | M4 |
| Fluid Wind | `arts/fluid/src/wind/FluidWind.kt` | Sorgente omonimo | fluidi e particelle | M7 |
| February | `arts/hills/src/hills2/Hills2.kt` | Verificato: Gart.of("february") | paesaggi e stratificazioni | M4 |
| Rigor | `arts/igor/src/rigor/Rigor.kt` | Sorgente omonimo | composizioni strutturate | M4 |
| Sabulum | `arts/layers/src/sabulum/Sabulum.kt` | Sorgente omonimo | dune e stratificazione | M4 |
| Strata | `arts/layers/src/strata/Strata.kt` | Sorgente omonimo | contorni sovrapposti e ombre | M4 |
| Undula | `arts/layers/src/undula/Undula.kt` | Sorgente omonimo | forme ondulate e sfumature | M4 |
| LetterO | `arts/lettero/src/lettero/Lettero.kt` | Sorgente corrispondente | forme tipografiche | M4 |
| Citymap | `arts/lines/src/lines/CityMap.kt` | Sorgente omonimo | reticoli stradali e crescita ramificata | M4 |
| Crinis | `arts/lines/src/lines/crinis/Crinis.kt` | Sorgente omonimo — PREFERITO PRINCIPALE | setole, curve, ombre, grana e supersampling | M4 |
| TapesB | `arts/lines/src/lines/tapes/TapesB.kt` | Sorgente omonimo | nastri e clipping di path | M4 |
| Corona | `arts/mathy/src/corona/Corona.kt` | Sorgente omonimo | mappe caotiche e densità | M7 |
| Sigillum | `arts/mathy/src/sigillum/Sigillum.kt` | Sorgente omonimo | matematica dei numeri primi e sigilli | M7 |
| Monet2 | `arts/monet/src/monet/Monet2.kt` | Sorgente omonimo | pennellate e percorsi | M4 |
| Flip | `arts/palecircles/src/palecircles/flip/Flip.kt` | Sorgente omonimo | reticoli circolari | M4 |
| Shad | `arts/palecircles/src/palecircles/shad/Shad.kt` | Sorgente omonimo | cerchi, ombre e grana | M4 |
| Circl | `arts/pixelmania/src/pixelmania/foo/Circl.kt` | Sorgente omonimo | gradienti circolari e grana | M4 |
| Plasma3 | `arts/plasma/src/plasma/three/Plasma3.kt` | Sorgente omonimo | plasma e animazioni | M6 |
| Mirr | `arts/rayz/src/rayz/mirr/Mirr.kt` | Sorgente omonimo | simmetria e raggi | M4 |
| Divine Divide | `arts/rects/src/rects/divide/DivideRects.kt` | Verificato: Gart.of("divine-divide") | suddivisione rettangolare | M4 |
| Craticula | `arts/rects/src/craticula/Craticula.kt` | Sorgente omonimo | intersezioni fra griglie ruotate | M4 |
| Mondrian 03 | `arts/rects/src/rects/mondrian/MondrianRects.kt` | Candidato: variante 03 da confermare | griglie rettangolari e colori | M4 |
| Sf12 | `arts/sf/src/sf/SF12.kt` | Sorgente omonimo | pattern matematici | M4 |
| Sixsix2 | `arts/sixsix/src/sixsix/two/SixSix2.kt` | Verificato: sorgente versione 2 | celle e reticoli | M4 |
| Sixsix 2 | `arts/sixsix/src/sixsix/two/SixSix2.kt` | Probabile stessa opera di Sixsix2; da confermare | celle e reticoli | M4 |
| Perspective02 | `arts/skyscraper/src/perspect/PerspectiveCity.kt` | Candidato: variante 02 da confermare | prospettiva urbana | M7 |
| Skyscraper2 | `arts/skyscraper/src/skyscraper/SkyScrapers2.kt` | Sorgente versione 2 | edifici e prospettiva | M7 |
| Stripes | `arts/stripes/src/stripes/Stripes.kt` | Sorgente omonimo | strisce e texture | M4 |
| Pandora | `arts/td/src/t3d/pandora/pandora.kt` | Sorgente omonimo | 3D sperimentale | M7 |
| Nidus 1 | `arts/ticktiletock/src/ticktiletock/nidus/Nidus.kt` | Candidato: seed/variante 1 da confermare | regioni e composizioni cellulari | M4 |

## Catalogo jdxyw/generativeart (4 voci)

| Nome come indicato | Sorgente Go | Tecnica/riuso per C# | Area roadmap |
|---|---|---|---|
| abyssalcurrents | `generativeart-master/arts/abyssalcurrents.go` | correnti a getto, vortici di Kármán, loop | M6 |
| analyticalcurlsilk | `generativeart-master/arts/analyticalcurlsilk.go` | nastri 3D, curl noise e ordinamento profondità | M7 |
| coronalmagneticarcade | `generativeart-master/arts/coronalmagneticarcade.go` | archi magnetici, prominenze, eclissi | M7 |
| erosioncanyon | `generativeart-master/arts/erosioncanyon.go` | heightfield, erosione e contour/marching squares | M4/M7 |

## Priorità operative: dalle preferenze alle funzionalità riutilizzabili

### 1. Fondamenta condivise prima delle copie

- **M3C** — registry di artwork, schemi parametri, ricette versionate, preset e metadati per le capacità raster/vettoriale/animazione. Prevedere un campo `inspiration` solo documentale e distinto dall’ID dell’opera originale.
- **M3D** — Studio Avalonia iniziale: catalogo, anteprima, editor parametri e confronto con immagini di riferimento fornite, senza promettere parità prima dei test visivi.
- **M4** — pennelli a setole e pennellate stratificate, gradienti e palette, dithering, grana, maschere, curve di livello, campo di distanza, composizione di shape, sampling, Voronoi e path-clipping.
- **M6** — timeline, rumore 4D e loop periodici, export frame-by-frame in streaming.
- **M7** — volumetria/3D, fluidi, DLA, ottica e simulazioni approfondite.

### 2. Casi rappresentativi per la prima galleria

Questa è una **sequenza tecnica**, non una graduatoria dei gusti:

1. **Geometrie modulari**: `Stripes`, `Mondrian 03`, `Divine Divide`, `Sixsix2` — utili per testare forme, palette e layout.
2. **Linee e flussi**: `Cyanowaves`, `Eclectic`, `Vorflow`, `Fascia Pal5 Seed30` — richiedono flow fields, curve, Voronoi e dithering.
3. **Texture e materia**: `Cotton Circles2`, `Strata`, `Sabulum`, `Monet2` — richiedono pennelli, stratificazioni e post-processing.
4. **Benchmark hero: Crinis** — almeno setole distribuite lungo una spine, occlusione/ombre tra bande, supersampling, grana, parametri seed e qualità. Non pianificarlo come semplice effetto applicato al PNG.
5. **Estensione Go**: `erosioncanyon` dopo marching squares; `abyssalcurrents` dopo loop temporali; `analyticalcurlsilk` e `coronalmagneticarcade` quando il percorso 3D sarà pronto.

### 3. Specifica visiva minima per i generatori ispirati

Ogni artwork C# dovrà avere: ID/versione stabili, seed, palette e parametri espliciti, modalità di qualità, dimensioni indipendenti dal disegno logico, capacità dichiarate, cancellazione del rendering, preset e almeno un test deterministico. Per Crinis/Fascia/Strata/Go selezionati aggiungere anche una scheda di confronto visivo con criteri estetici osservabili.

### 4. Questioni ancora da verificare

- `Pebble2`, `Glst2`, `Perl 1`, `Mondrian 03`, `Perspective02`, `Nidus 1`: match con un sorgente di famiglia o titolo vicino, ma variante specifica non confermata.
- `Sixsix2` e `Sixsix 2`: stesso sorgente identificato; verificare se si intende un singolo preset o due risultati diversi.
- La somiglianza artistica è **un obiettivo progettuale**, non una garanzia di riproduzione pixel-per-pixel. Eventuali copie/derivazioni dirette richiedono gestione delle rispettive licenze e attribuzioni.

## Stato dei test

La baseline M3B ha superato compilazione, xUnit e rendering PNG su Windows secondo il feedback dell’utente. **Nessun codice C# è stato cambiato in P2.1, e non sono stati eseguiti nuovi test**.


## Priorità animazioni (16 novembre 2026)

L'utente apprezza molto anche **le animazioni** del progetto Go, non soltanto le quattro immagini/generatori già selezionati. Questo è un requisito di esperienza creativa: in GenerativeArt Studio anticipiamo un player di loop, un controllo di fase e generazione deterministica di frame. Lo studio diretto del Go evidenzia `AnimatedEngine`, parametri su timeline, supersampling temporale e poster animati. La prima implementazione C# M3C.1 offre soltanto sequenze PNG di loop: GIF/APNG, motion blur, poster animati e video restano da sviluppare e testare.
