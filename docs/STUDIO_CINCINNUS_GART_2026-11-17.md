> **AGGIORNAMENTO SUCCESSIVO (C1, 17/11/2026):** il primo **kernel geometrico C# è ora implementato**, secondo la richiesta successiva dell'utente. Per API e limiti consultare [CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md](CINCINNUS_C1_KERNEL_GEOMETRICO_2026-11-17.md). Il testo seguente è il **dossier originale H3**, scritto quando ancora non era presente codice Cincinnus: i suoi riferimenti a «non implementato» vanno letti come **stato storico H3**. C1 non ha ancora superato il gate di build/test Windows né introdotto renderer o preset.

# Gȧrt Cincinnus — studio sorgente, architettura e piano di porting C#

**Data:** 17 novembre 2026. **Consegna:** G4-R2-H3, aggiornamento esclusivamente documentale. **Baseline di codice:** M4E.3-G4-R2, compilazione e test Windows confermati dall'utente prima di H3; il conteggio non è disponibile. **Cincinnus NON è ancora implementato né validato artisticamente nello Studio.**

## 1. Fonti, provenienza e limiti delle evidenze

Fonte primaria: archivio fornito dall'utente `gart-main-pulito(2).zip`, cartella `gart-main/arts/lines/src/lines/cincinnus/`. Sorgenti esaminati:

- `Cincinnus.kt` (364 righe): variante completa, default `seed=3`, output `cincinnus-original`.
- `CincinnusSimple.kt` (248 righe): variante a streamline separate, default `seed=1`, output `cincinnus`.
- `gart/src/main/kotlin/dev/oblac/gart/brush/bristles.kt` (Spine, SpinePoint, Bristles, outline, drawBristles; 294 righe).
- `gart/src/main/kotlin/dev/oblac/gart/flow2/VectorField.kt`, `Integrator.kt`, `Streamlines.kt` (vortici, integrazione numerica e collocazione delle streamline).
- `arts/lines/src/lines/crinis/Crinis.kt` (per confronto con Crinis).
- Nostri `CrinisMicroRenderer.cs`, `BristleSpine2D.cs`, `GartBrushRenderer.cs`, `GartBrushArtworkDescriptors.cs`.

Repository pubblico: <https://github.com/igr/gart>. **Attenzione:** nel README/galleria del sorgente fornito la voce `Cincinnus` non appare tra le opere illustrate e lo ZIP non contiene PNG di Cincinnus. L'immagine di `Crinis` presente in galleria NON va scambiata per Cincinnus. Il risultato visivo puntuale non è stato confrontato con una PNG di Cincinnus originale; per il confronto definitivo serve una renderizzazione del sorgente originale o un file di riferimento autentico. Il contenuto del repository remoto può essere cambiato dopo la data dello ZIP.

I due file Cincinnus non sono equivalenti fra loro: **non** inferire che un'unica immagine o seed dimostri la fedeltà di entrambi. Il presente rapporto deriva dall'ispezione reale dei file allegati; descrizioni estetiche sono ipotesi tecniche finché non esistono immagini comparabili.

## 2. Obiettivo artistico e differenza da Crinis

**Crinis** percorre curve con curvature lente/orbita attorno a un *eye*, disponendo fasci su una griglia di copertura (`Crinis.kt`). **Cincinnus** genera invece un campo vettoriale dalla **somma di vortici esterni alla tela**, ne traccia percorsi, e applica pennellate a setole con estremità irregolari, curvature limitate e forti sovrapposizioni. La versione completa aggiunge cicli di copertura, filler e patch. Entrambi condividono il pennello `Bristles` originale, non la legge di movimento delle curve.

Per lo Studio C# conviene aggiungere in futuro una famiglia/generatore **autonomo** (`cincinnus-bristles`, ID proposto, NON implementato) e condividere motori geometrici e pittorici con `crinis-bristles` senza modificare il rendering R10 già consegnato.

## 3. Parametri originali — solo quelli effettivamente configurabili

| Parametro | Originale `Cincinnus.kt` | `CincinnusSimple.kt` | Significato |
|---|---:|---:|---|
| Tela (`N`) | 2000×2000 fisso | 2000×2000 fisso | Coordinate artistiche logiche |
| `seed` | 3 | 1 | Seed predefinito diverso |
| `ss` | 3, intervallo 1–4 | 3, 1–4 | Supersampling e downsample |
| `strokes` | 60, 1–300 | non esiste | Numero richiesto di pennellate *main*, non numero totale di pennellate |
| `eyes` | 5, 1–12 | 5, 1–12 | Centri esterni dei vortici |
| `dots` | 4000, 0–20000 | 4000, 0–20000 | Sporco/speck neri |
| `rough` | 1, 0–2 | non esiste | Irregolarità della testa di pennellata |
| `gap` | non esiste | 110, 30–400 | Separazione fra streamline; semilarghezza `1.25*gap` |
| `out` | `cincinnus-original` | `cincinnus` | Nome file output |

**Costanti artistiche in `Cincinnus.kt`:** `REACH=.3`, `FAT=.4`, `SHUT=.9`, `LONG=1600`, `DRY=4`, `DEEP=.4`, `DS=3`, `RISE=2.5`, `PRESS=.2`, `SHORT=.5`, `SPREAD=12`, `HWMAX=120`, `HOOK=1`, `HWMIN=30`, `RMIN=25`, `MAXLEN=9000`, `MARGIN=500`. Non presentarle come controlli già disponibili nell'originale; sono candidati parametri addizionali della nostra implementazione.

**Costanti in `CincinnusSimple.kt`:** `REACH=.3`, `DS=3`, `DRY=4`, `DEEP=.25`, `FALL=.8`, `BAND=3`. Sono differenti dalle costanti della versione completa.

**Palette e luce comuni:** 8 tappe da scuro a rosa/carne: `#1A0308`, `#3D0713`, `#6E0F22`, `#A3142F`, `#CB1B45`, `#E0485A`, `#F08A80`, `#F9C9B6`. Luce `Vec2(-.21, -.31)`; body scuro del pennello e palette interpolata OKLab da `Bristles`. Fondo: colore della stessa palette a `t=.02`, non esattamente il primo colore.

## 4. Geometria e ordine delle operazioni — versione completa

1. **Vortici lontani dal centro dell'immagine.** `offPage` sorteggia punti in `[-0.4N, 1.4N]²` e ammette soltanto quelli esterni di almeno `.08N` a uno dei lati. `field` sceglie ogni centro come il più distante dai centri già scelti fra 12 candidati; lo spin è negativo, ampiezza casuale `.6..1.4`, raggio di influenza `N*.3 * (.7..1.3)`. Usa RNG separato `Random(seed*7919+7)` per la forma dei centri e il generatore principale per spin/reach. **Non usa** il controllo topologico `hasEye` presente soltanto in `Simple`.
2. **Campo totale.** Ogni vortice restituisce una tangente circolare attenuata per distanza, secondo `VectorField.vortex`, e il campo totale somma i contributi. Il tracciatore lavora sulla direzione normalizzata del campo (vedere `Integrator.kt`). Formula di un vortice senza `pull`, con `dx=x-cx`, `dy=y-cy`, `r²=dx²+dy²`, `R=reach`: `v = spin * (-dy, dx) / (sqrt(r²) * (1+r²/R²))`, zero al centro. La somma è **vettoriale prima** della normalizzazione della direzione.
3. **Copertura adattiva.** `Cover(20f)` usa una griglia `(2000/20+1)² = 101²` e pesca una cella col minor numero di pennellate sovrapposte. Il metodo `add` marca approssimativamente le celle attraversate dal corpo di ciascuna pennellata, valutando tangente/normale, larghezza, testa e asciugatura. La griglia è una misura discreta di copertura, **non** una maschera pixel-accurate.
4. **Tracciamento.** Da un punto di partenza si percorre il campo con `trace` RK4, passo `DS=3 px`, entro rettangolo ampliato di `500 px`, al massimo `9000/3` passi. Si rigettano tracce sotto 40 punti; si cerca un eventuale primo ritorno vicino all'origine dopo essersene allontanati. La dimensione di riferimento è derivata dal giro completo o dalla distanza al vortice più vicino. Si eliminano curve troppo minute (`RMIN=25`).
5. **Larghezza a curvatura controllata.** Semilarghezza di base `min(size*FAT*random(.85,1.15), HWMAX)`. Il raggio di curvatura locale è stimato da differenze di heading a distanza di ~4 campioni; si limita la semilarghezza a circa `0.8*raggio` aggregato su finestre (minimi e medie), evitando pieghe interne; modulazione di pressione/rumore e profilo `load(t)=1-t^4`. Si rigettano fasci con larghezza media inferiore a `HWMIN=30`.
6. **Testa irregolare.** `headOf(hw, rnd)` crea una funzione del traverso `u` in `[-1,+1]`: ellisse/semicerchio, inclinazione, rumore a basse frequenze, componenti hash e traslazione per avere minimo zero. Questa funzione determina *quando* ciascuna setola tocca la carta: non disegnare una banale testata piatta. Il parametro `rough` scala la deformazione solo nella versione completa.
7. **Selezione della lunghezza.** Le pennellate vanno sino alla chiusura parziale del giro o alla fuoriuscita; `LONG=1600` le limita. Una frazione casuale `SHORT..1` accorcia il percorso. All'inizio del riempimento il codice preferisce alcuni finali nella metà inferiore della tela. `unhooked` conserva il tratto continuo più lungo senza curvature localmente troppo strette (`HOOK`).
8. **Tre categorie.** Si accumulano pennellate fino ad avere almeno `strokes` o nessuna cella libera (limite `1000` tentativi). Le prime `strokes` compongono `main`; il resto è `filler`. Un secondo `Cover(20)` misura il risultato e tenta sino a `300` nuove `patches` per tappare spazi. `main` e `filler` sono ordinati per dimensione decrescente, poi filtrati dalla curvatura. **`strokes=60` non implica 60 pennellate totali.** Le patch vengono disegnate per prime, poi filler, infine main.
9. **Pigmento e ombre.** Ogni pennellata riceve due ombre morbide con blur sigma `60` e `10`, alpha `0xC0/0xD0`, traslazioni rispettive di `(10,20)` e `(2.5,5)`. Successivamente `drawBristles` disegna il corpo scuro sotto le setole, le singole setole con variabilità tonale e asciugatura; `tone=random(.42,.78)+DEEP*(depth^RISE-.5)-.1`. I `dots=4000` puntini neri sono distribuiti a livelli fra le pennellate, non applicati soltanto sopra tutto.
10. **Finitura.** Rendering a `SS=3` (default 6000×6000 campioni), poi downsampling a 2000×2000, grana `addGrain(.14, seed)` e salvataggio PNG.

**Nota di efficienza:** la condizione `while ((laid.size < STROKES || cover.bare() > 0) && tries++ < 1000)` implica che la quantità e i tempi sono condizionati dalla griglia, dal campo e dai tentativi, non da un numero fisso di segmenti.

## 5. Geometria — variante `CincinnusSimple.kt`

1. Costruisce un campo di 5 vortici esterni. Diversamente dalla completa, `field` **rigenera l'intero insieme** se `hasEye` intercetta un indice di rotazione negativo all'interno della griglia di celle da 40 px. Questo evita alcune singolarità/vortici visibili nella tela, a costo di possibili tentativi variabili.
2. Genera streamline equidistanti attraverso `VectorField.streamlines` con `dSep=gap=110`, `dTest=dSep/2`, `step=3`, `minLength=300`, `margin=3*(1.25*gap)`, integratore RK4. Il motore allunga le tracce in **entrambe** le direzioni, mantiene una distanza spaziale minima da linee precedenti e si ferma per collisioni, margini, campo nullo o lunghezza.
3. Taglia ciascuna streamline in tratti di massimo 1600 px circa, con sovrapposizioni casuali di 150–350 px; la prima lunghezza minima è 200 px, le successive 600 px. Alcune estremità sono ricercate intenzionalmente in basso.
4. La semilarghezza delle pennellate è costante di partenza `1.25*gap` ma viene ridotta su curvature strette, con `load(t)` e rumore; il pennello con larghezza totale `2.5*gap` copre quasi tutti gli interstizi in condizioni fortunate. Non rappresenta una garanzia universale di copertura.
5. Ordina le pennellate per `bend` **crescente**: quelle più curve vengono disegnate per ultime. Applica il gradiente tonale legato a ordine di disegno e posizione verticale `u=2*y/N-1` con termine `-FALL/2 * sign(u) * |u|^BAND`; grana, ombre e puntini neri come nella completa ma con algoritmo di distribuzione diverso.

**Sintesi del confronto:** `Simple` è preferibile per testare separazione e collisioni fra traiettorie; `Cincinnus.kt` è preferibile come specifica del posizionamento artistico stratificato e del riempimento adattivo. Sono due modalità algoritmiche da poter offrire nello Studio, non una mera impostazione di qualità di un unico procedimento.

## 6. Il pennello `Bristles` — elemento qualitativo indispensabile

La libreria disegna per ogni `Spine` sia un corpo di pigmento scuro (`tone - body=.38` sulla rampa), sia **molte setole** disposte con passo `pitch=4.6 px` e larghezze individuali casuali `.45..85 * pitch`. Il numero cresce con la larghezza massima del fascio. Ogni fibra viene disegnata come sequenza di segmenti dinamici; il programma simula vuoti asciutti, asimmetria, assorbimento e irregolarità del colore, con luce sui fasci. Una singola polilinea spessa, una silhouette opaca senza microfibre o un semplice blur non sono sostituti sufficientemente fedeli.

Il `body` dell'originale Cincinnus è **realmente disegnato** sotto le fibre (`bristles.kt` circa riga 165), con scostamento di luminosità; nella nostra Crinis R10 il parametro `body-opacity=0` può nascondere il corpo per preservare la visibilità dei singoli fili. Questo è un **conflitto di resa**, non da risolvere cambiando il comportamento storico di Crinis. La futura Cincinnus deve poter usare corpo, pigmento e fibre indipendenti, con un preset iniziale coerente all'originale.

## 7. Relazione con il codice C# già esistente

| Riutilizzabile | Dove | Scarto rispetto a Cincinnus |
|---|---|---|
| `BristleSpinePoint`, `BristleSpine2D` | `GenerativeArt.Algorithms/Brushes/` | Rappresentazione di spine e normali sì; il suo metodo `Build` segue leggi di curva Crinis, non i vortici compositi RK4 di Cincinnus. Meglio introdurre un costruttore spine da traccia campionata, non alterare quello già usato. |
| `CrinisMicroRenderer`, `GartBrushRenderer` | `GenerativeArt.Rendering.Skia/` | Microfibre, palette e stratificazione già in parte presenti; mancano la testata di atterraggio, il corpo pigmentato Cincinnus, il posizionamento a campo composito e la gestione distinta main/filler/patch. |
| `PatternPalette`, `ArtworkInk`, `SkiaRasterSurface` | `Rendering.Skia/` | Consentono colori per ruolo e raster Skia; verificare prima interpolazione cromatica (Gȧrt usa OKLab) e blur/antialias rispetto all'originale. |
| `ArtworkDescriptor`, recipe JSON | `Application/Catalog/`, `Application/Defaults/` | Aggiungere ID e descrittore con parametri espliciti e colori singoli, preservando la compatibilità delle 173 ricette. |
| `ArtworkRenderEngine` e CLI/Studio | `Rendering.Skia/`, `Studio/` | Nuovo dispatch, stima costo, controllo cancellazione, gestione progressi e output deterministico; mantenere anteprima statico alla risoluzione finale. |

**Decisione tecnica raccomandata (non ancora implementata):** separare `CincinnusFlowField`, `CincinnusStreamlinePlacer`, `CincinnusStrokePlanner`, `CincinnusCoverageGrid` in **Algorithms**, e `CincinnusRenderer` con pennello e ombre in **Rendering.Skia**. Condividere eventuali routine di pittura con Crinis solo tramite API additiva e testate, senza regressioni R10. L'eventuale `CincinnusMode` deve restare indipendente dalla qualità di rendering e dal seed.

## 8. Controlli artistici raccomandati per lo Studio C#

Distinguere parametri di **composizione** (`mode=simple/adaptive`, `eyes`, `reach`, `spin-variation`, `stroke-count`, `gap`, `coverage-target`, `stroke-length`, `min-length`, `random-seed`) dai parametri di **pennello** (`brush-half-width`, `bristle-pitch`, `bristle-width`, `head-roughness`, `pigment-load`, `dryness`, `body-opacity`, `shadow-wide`, `shadow-tight`, `shadow-offset`, `light-x/y`, `vertical-shade`, `depth-contrast`, `grain`, `dots`).

Sono **proposte di design**, non parametri presenti ora nell'applicazione né direttamente esposti dall'originale. I controlli vanno raggruppati per sezioni, abilitati in modo condizionale secondo la modalità (`gap` solo Simple, `coverage-target`/`stroke-count` solo Adaptive), con input precisi e colori editabili **singolarmente** (otto stop della palette originale, colore fondo, corpo, ombra, sporco). I vecchi preset personali e i controlli di Crinis vanno lasciati invariati.

Per un primo confronto fedele usare canvas **2000×2000**, seed originali `3`/`1` secondo modalità, palette crimson sopra, e gli stessi default originali. Aggiungere formati alternativi solo con regole esplicite di scala, senza attribuire l'equivalenza visiva a un rapporto d'aspetto differente.

## 9. Rischi e requisiti non funzionali

- **Prestazioni e memoria:** con tela 2000² e supersampling 3 si alloca almeno il raster 6000², **~144 MB solo per un buffer RGBA 8-bit**, prima di copie, blur e downsample; con SS4 8000², **~256 MB** per buffer. Non promettere 8K/UHD per questo renderer. I costi crescono con lunghezza tracciati, numero di setole, copertura e blur.
- **Complessità delle griglie:** evitare scansioni non necessarie delle celle e allocazioni per tutti i candidati in ogni iterazione; eventuali accelerazioni spatial hash/buckets devono preservare la determinazione del risultato a seed costante.
- **Curvatura:** gestire tangenti quasi nulle, raggi estremi, NaN, bordi/out-of-bounds, autocollisioni, salti del tracciatore, limiti hard sulle iterazioni e annullamento cooperativo.
- **Determinismo:** scegliere e documentare un algoritmo RNG C# stabile, con stream separati per geometria, testa, fibre, patch e puntini. **Non promettere stessa immagine al pixel** fra Random Kotlin e C#, o fra Skiko/Skia e SkiaSharp; verificare invece riproducibilità interna e metriche/macrosagome rispetto ai riferimenti.
- **Asciugatura e colore:** conservare il profilo `1-t^4`, l'ordine dei layer, le funzioni per estremità e il gradiente di tono. Non ridurre a una texture casuale sovrapposta.
- **Licenze/provenienza:** repository Gȧrt pubblicato con licenza BSD-2-Clause; il porting deve essere autonomo e attribuito, senza incollare sorgenti Kotlin o incorporare immagini altrui nel prodotto. Se si decidesse di copiare/adattare codice originale, verificare prima obblighi di attribuzione e notice del progetto. I link pubblici non costituiscono autorizzazione a redistribuire tutte le immagini.
- **Sorgenti di prova assenti:** nel pacchetto fornito non è presente un PNG output Cincinnus. Per giudizi estetici servono immagini originali identificate con variante/seed/parametri, oppure rendering controllati del sorgente Gȧrt separati dal nostro prodotto (nessuna toolchain Kotlin nel progetto C#).

## 10. Piano di realizzazione incrementale (proposta)

**C0 — studio e documentazione (questa consegna).** Fonti individuate e analizzate; nessuna modifica funzionale. Non dichiarare completata la prova estetica.

**C1 — kernel deterministico C#.** `VortexField`, RK4 normalizzato, tracing doppia direzione, distanze e test topologici; test su cerchi, orbite esterne, casi limite, riproducibilità/limiti. Nessuna modifica Crinis.

**C2 — modalità Simple funzionale.** Streamline con distanza `gap`, segmentazione e overlap, curvatura e larghezza, campo e ordine di disegno; disegno inizialmente diagnostico di spines + occupazione (no finta resa finale).

**C3 — pennello volumetrico Cincinnus.** Corpo/ombre/setole, head-of e dryness, shader palette crimson a ruoli editabili; prima versione artistica 2000×2000 con cancellazione e budget; confronti con una PNG originale, quando disponibile.

**C4 — modalità Adaptive completa.** `Cover`, selezione least-covered, `main/filler/patch`, ordine livelli, distribuzione dei dots, criteri di stop e miglioramenti di performance senza modificare la resa inaspettatamente.

**C5 — Studio/CLI e QA.** Descrittore, preset embedded, ricette esemplificative, smoke 2000² e Full HD solo se definito l'adattamento, anteprima finale nativa/export, confronti A/B e test di regressione delle 173 ricette attuali; benchmark risorse e materiali, confronto qualitativo esplicito dell'utente.

Questi passaggi **non sostituiscono** la roadmap G4-QA: lo studio richiesto è ora acquisito; l'ordine dell'implementazione rispetto a G4-R3/G5 non è ancora deciso. Crinis R10 resta sospeso e non approvato esteticamente.

## 11. Matrice minima di test per l'implementazione futura

1. **VectorField:** campo nullo al centro, decadimento con reach, orientamento rotazione e somma vettoriale, assenza NaN.
2. **Trace RK4:** circolarità controllata, continuità, rispetto di bounds/margine, passo inverso e cancellazione.
3. **Streamlines:** separazione minima, chiusura per autocollisione, seed fissi, conteggio/nessuna linea e limiti.
4. **Curvature/width:** semilarghezza non negativa, limite su curve strette, estremità finite, nessun salto tra normali.
5. **Coverage:** mappatura dei confini, griglia al bordo, mai fuori indice, selezione celle poco coperte, filler/patch eseguiti condizionalmente.
6. **Bristles:** testata irregolare, dry-out al termine, corpo e setole, 8 stop colore individuali, alpha/blur.
7. **Render:** hash immagine stabilizzato (dopo prima baseline), nessun crash con `dots=0`, `strokes=1`, `eyes=1`, `rough=0/2`, `ss=1/4`; scadenza e cancellation token.
8. **Regressioni:** Crinis R10, tutti i 29 generatori, 29 embedded, 173 ricette, 17 preset personali. Nessun nuovo limite 8K esteso automaticamente.

## 12. Avvio Windows quando sarà implementato

Dal root `GenerativeArt`, PowerShell 7 con .NET SDK 10:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando riguarda la **baseline C# attuale**, non produce ancora Cincinnus. Per ora non c'è un comando `render-recipe` Cincinnus eseguibile: verrà documentato soltanto insieme al nuovo generatore e ai JSON reali. Questa consegna H3 cambia soltanto documentazione e manifest, quindi la conferma Windows della G4-R2 resta valida senza dichiarare nuovi test non eseguiti.
