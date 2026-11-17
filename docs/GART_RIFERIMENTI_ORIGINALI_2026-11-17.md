# Riferimenti Gȧrt — sorgenti originali e galleria ufficiale (17 novembre 2026)

**Approfondimento H3:** aggiunta analisi dedicata a **Cincinnus**, distinto da Crinis: [sorgenti, Brush, RK4, algoritmo adattivo e Simple, porting](STUDIO_CINCINNUS_GART_2026-11-17.md). In questo snapshot Cincinnus non compare nella galleria illustrata e non possediamo una sua PNG originale autenticata.


**Consegna G4-R2-H2:** integrazione documentale dei nuovi materiali forniti dall'utente. **Il codice GenerativeArt Studio rimane M4E.3-G4-R2, compilato e testato con esito positivo su Windows secondo la conferma del 17/11/2026**. Non è stata eseguita una nuova build .NET per questa integrazione puramente documentale; rimane aperta la valutazione artistica G4-R2.

## Provenienza e limiti

- **Sorgente esaminata:** archivio fornito `gart-main-pulito(2).zip`, radice `gart-main/`: 1.357 voci di archivio, 846 file `.kt`, 9 file `.md`; sorgenti Kotlin originali consultabili senza introdurli come dipendenza C#.
- **Galleria pubblica ufficiale:** <https://github.com/igr/gart> (`README.md`); l'indice contiene **296 voci di opera con collegamenti a originale e miniatura**.
- **Lo ZIP del sorgente allegato non contiene PNG né GIF**: gli esempi visivi sono sul repository pubblico. Sei miniature originali (Ocellus, Rete, Nimbus, Pebble2, Accretion, Crinis) sono state aperte e viste direttamente dal repository al 17/11/2026; il resto dei percorsi è trascritto dai link del README sorgente, non singolarmente testato online.
- **Copyright/licenza:** la radice di Gȧrt contiene `LICENSE` BSD-2-Clause, copyright 2022 Igor Spasić. Mantenere attribuzioni e verificare i diritti prima di distribuire file di opere originali; questa integrazione **non incorpora immagini o codice Kotlin** nel pacchetto GenerativeArt Studio.
- **Versione:** per algoritmi usare il sorgente ZIP allegato (snapshot fornito); per immagini usare i link alla galleria GitHub `main` (possono evolvere nel tempo). Non presumere che le due versioni corrispondano esattamente o che i seed predefiniti coincidano.

## Criterio corretto di confronto

1. **A/B interno**: render G4-R1 e G4-R2 dalle sei coppie JSON già presenti (`examples/recipes/m4e3-g4/` e `examples/recipes/m4e3-g4-r2/`), con lo stesso seed, dimensioni e viewport `Adatta`/`100%`.
2. **Benchmark artistico esterno**: confrontare il risultato con l'opera originale Gȧrt, considerando silhouette, topologia, densità, texture, volumi, palette e gerarchia percettiva. **Non** pretendere identità pixel-a-pixel, dati gli algoritmi, seed, colori e dimensioni potenzialmente diversi.
3. Distinguere risultati realmente osservati da ipotesi: la **qualità estetica della R2 non è stata ancora approvata dall'utente**. Decidere fra G4-R3 e G5 soltanto dopo il confronto o una scelta esplicita.

## Approfondimento algoritmico sulle famiglie prioritarie

| Opera | File sorgente nell'archivio Gȧrt | Caratteristiche verificate / implicazioni C# |
|---|---|---|
| **Ocellus** | `arts/bubbles/src/ocellus/Ocellus.kt` | Occhi a 13 livelli e unioni SDF *smooth-min*; dopo la colorazione, flood-fill di piccole regioni enclavate e ridisegno con bordo di 4 px. La nostra G4-R2 aggiunge pori euristici per prossimità di tre superfici (`OcellusField.JunctionPore`), **non equivale** alla classificazione topologica delle regioni dell'originale. Candidato G4-R3: modellazione delle cavità tramite etichettatura di componenti con budget di memoria. |
| **Rete** | `arts/cell/src/rete/Rete.kt` | Schiuma a dischi di dimensioni diverse; campo basato sulla **distanza alle superfici (Apollonius)**, non semplice Voronoi dei centri; pareti tramite distanza al secondo vicino e *smooth-min*; copie posteriori sfocate e desaturate; celle progressivamente più piccole vicino alla zona di drenaggio. La nostra R2 riproduce alcuni principi ma le pareti, pellicole e i nodi **non sono attestati equivalenti**. |
| **Nimbus** | `arts/clouds/src/nimbus/Nimbus.kt` | Una nuvola illustrata composta da sfere e protuberanze ricorsive, intersezione mediante raggi (*ray marching* / primo impatto), luce a **tre toni piatti**, base ritagliata e ventre sagomato, cielo viola e pianura. Non basta una texture di rumore generico. Candidato G5 distinto. |
| **Pebble2** | `arts/bubbles/src/bubbles/pebble/Pebbles.kt` | Forme organiche da cammini sovrapposti/spline, tratteggi interni clip, profili ripetuti e contorni cromatici. La corrispondenza **esatta della variante `Pebble2`** al file Kotlin rimane da confermare: il nome della galleria identifica l'immagine, mentre il file implementa `pebble` senza un selettore esplicito `Pebble2`. |
| **Accretion** | `arts/cell/src/accretion/Accretion.kt` | Colonizzazione **DLA** (diffusion-limited aggregation) su reticolo toroidale: più semi, walker, congelamento per contatto, mappa delle età e texture/alone; i canali fra colonie derivano dalla competizione. Un semplice rumore frattale non replica il meccanismo. Candidato G5 ad alto costo computazionale. |
| **Glst/Glst2** | `arts/flowforce/src/flowforce/glst/glst.kt` | Tracciamento di streamline in tre scale (larghe/medie/sottili); evitare sovrapposizioni, segmentare colori, segnare punti. Collegamento esatto tra variante `Glst2` e file da verificare. Candidato G5. |
| **Crinis** | `arts/lines/src/lines/crinis/Crinis.kt` | Fasci su spine curve, pennello `Bristles`, ombre morbide fra i fasci e sovracampionamento. La G3-R10 C# resta **sospesa e non approvata esteticamente**, in particolare nella volumetria delle fibre all'interno dello stesso fascio. |

## Link alla galleria ufficiale delle opere preferite

**Origini dei link:** attributi `href`/`src` del README nell'archivio Gȧrt; percorsi relativi al repository `igr/gart`. Le immagini sono originali dell'autore, non generazioni sintetiche o approssimazioni.

| Opera | Immagine originale (GitHub) | Miniatura (GitHub) |
|---|---|---|
| Alien Letters V3 | [Immagine](https://github.com/igr/gart/blob/main/arts/alien/alien-letters-v3.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/alien/alien-letters-v3_thumb.png) |
| Flamebrush1 | [Immagine](https://github.com/igr/gart/blob/main/arts/bubbles/flamebrush1.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/bubbles/flamebrush1_thumb.png) |
| Ocellus | [Immagine](https://github.com/igr/gart/blob/main/arts/bubbles/ocellus.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/bubbles/ocellus_thumb.png) |
| Pebble2 | [Immagine](https://github.com/igr/gart/blob/main/arts/bubbles/pebble2.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/bubbles/pebble2_thumb.png) |
| Accretion | [Immagine](https://github.com/igr/gart/blob/main/arts/cell/accretion.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/cell/accretion_thumb.png) |
| Rete | [Immagine](https://github.com/igr/gart/blob/main/arts/cell/rete.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/cell/rete_thumb.png) |
| Nimbus | [Immagine](https://github.com/igr/gart/blob/main/arts/clouds/nimbus.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/clouds/nimbus_thumb.png) |
| Cotton Circles | [Immagine](https://github.com/igr/gart/blob/main/arts/cotton/cotton-circles.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/cotton/cotton-circles_thumb.png) |
| Cotton Circles2 | [Immagine](https://github.com/igr/gart/blob/main/arts/cotton/cotton-circles2.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/cotton/cotton-circles2_thumb.png) |
| Eclectic2 | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/Eclectic2.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/Eclectic2_thumb.png) |
| Eclectic | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/Eclectic.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/Eclectic_thumb.png) |
| Spring | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/Spring.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/Spring_thumb.png) |
| Cyanowaves | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/cyanowaves.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/cyanowaves_thumb.png) |
| Fascia Pal5 Seed30 | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/fascia-pal5-seed30.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/fascia-pal5-seed30_thumb.png) |
| Fascia | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/fascia.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/fascia_thumb.png) |
| Glst2 | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/glst2.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/glst2_thumb.png) |
| Monolith | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/monolith.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/monolith_thumb.png) |
| Perl 1 | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/perl_1.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/perl_1_thumb.png) |
| Vorflow | [Immagine](https://github.com/igr/gart/blob/main/arts/flowforce/vorflow.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/flowforce/vorflow_thumb.png) |
| Fluid Wind | [Immagine](https://github.com/igr/gart/blob/main/arts/fluid/fluid-wind.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/fluid/fluid-wind_thumb.png) |
| February | [Immagine](https://github.com/igr/gart/blob/main/arts/hills/february.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/hills/february_thumb.png) |
| Rigor | [Immagine](https://github.com/igr/gart/blob/main/arts/igor/rigor.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/igor/rigor_thumb.png) |
| Sabulum | [Immagine](https://github.com/igr/gart/blob/main/arts/layers/sabulum.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/layers/sabulum_thumb.png) |
| Strata | [Immagine](https://github.com/igr/gart/blob/main/arts/layers/strata.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/layers/strata_thumb.png) |
| Undula | [Immagine](https://github.com/igr/gart/blob/main/arts/layers/undula.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/layers/undula_thumb.png) |
| LetterO | [Immagine](https://github.com/igr/gart/blob/main/arts/lettero/LetterO.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/lettero/LetterO_thumb.png) |
| Citymap | [Immagine](https://github.com/igr/gart/blob/main/arts/lines/citymap.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/lines/citymap_thumb.png) |
| Crinis | [Immagine](https://github.com/igr/gart/blob/main/arts/lines/crinis.png) | [Anteprima](https://raw.githubusercontent.com/igr/gart/main/arts/lines/crinis_thumb.png) |

## Come procedere

- **Prima:** QA visivo delle sei coppie R1/R2 Ocellus e Rete, riferendosi anche alle immagini originali pubbliche per definire una griglia di criteri estetici, senza dare per scontata l'approvazione della R2.
- **Poi:** valutazione tecnica e artistica di G4-R3 (cavità reali, tensione delle membrane) contro il primo G5 (Nimbus/Pebble/Accretion/Glst). Evitare un porting massivo e mantenere lo stack C#/.NET 10, Avalonia, SkiaSharp.
- **Non completato:** non sono stati eseguiti render del nostro Studio né test .NET in questa revisione documentale, non sono state ricostruite le anteprime mancanti `examples/previews/` e non sono state pubblicate copie delle opere originali.
