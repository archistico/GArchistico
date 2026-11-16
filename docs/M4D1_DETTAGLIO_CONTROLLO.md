# M4D.1-R1 — Guilloché, incisione e controllo del dettaglio

16 novembre 2026. Incremento richiesto dall'utente dopo M4B.1: nuove ricette e algoritmi con più dettaglio e controllo. **13 generatori, 49 ricette d'esempio, 626/626 test superati**, otto nuovi PNG Full HD verificati. Nessuna nuova dipendenza di produzione.

## Guilloché: geometria, trama e precisione

L'algoritmo riutilizzabile `GuillocheCurve` genera ipotrocoidi/epitrocoidi a rapporto razionale, con modulazione radiale. Riduce il rapporto degli ingranaggi tramite MCD per ottenere il periodo di chiusura; chiude esattamente la polilinea e calcola un minimo di campionamento in base alle frequenze. Il renderer disegna una curva alla volta, trasformando un buffer riutilizzato: non accumula tutte le curve in memoria.

| Controllo | Effetto pratico |
| --- | --- |
| curve: inside / outside | Ingranaggio interno o esterno; due famiglie geometriche |
| fixed-teeth / moving-teeth | Rapporto e numero degli intrecci; inside richiede moving < fixed |
| pen-radius: 0–1,6 | Distanza del pennino; aumenta anse e sovrapposizioni |
| waves: 1–32, wave-amount: 0–0,3 | Frequenza e intensità della modulazione radiale |
| samples: 512–48000 | Precisione della polilinea; il minimo geometrico viene applicato automaticamente |
| layers: 1–200 | Numero di curve sovrapposte |
| inner-scale, layer-bias | Dimensione interna e distribuzione della distanza fra curve |
| twist: −180…180° | Rotazione progressiva fra curva interna ed esterna |
| scale, rotation, center-x/y | Inquadratura, orientamento relativo e posizione |
| line-width, opacity | Spessore e opacità del tratto, indipendenti dal numero di curve |
| palette e tre colori tratto | Gradiente cromatico fra le curve, sfondo separato con alpha |

Il seed stabilisce fase delle armoniche e orientamento iniziale; rotation modifica l'orientamento rispetto a quello del seed. Per alleggerire una zona troppo fitta ridurre layers/opacity, aumentare inner-scale oppure modificare twist. Aumentare samples migliora la geometria, mentre fine-2x migliora il campionamento raster: sono controlli distinti.

## Incisione: tratteggio tonale e maschera

`TonalHatching` valuta la copertura di tratti periodici con filtro di bordo e fino a quattro direzioni. Le direzioni aggiuntive entrano progressivamente nelle zone scure: il controllo tonale aumenta la copertura senza rimuovere i segni precedenti. La prima direzione e gli incroci hanno colori indipendenti. Il renderer usa un campo Domain Warp per modulare tono e ondulazione.

| Controllo | Effetto pratico |
| --- | --- |
| field: terrain / rings / marble | Rilievo tonale, anelli o venature |
| frequency, octaves, warp-amount | Scala e dettaglio del campo che guida il tono |
| bands: 2–24 | Numero di anelli/venature; non agisce su terrain |
| spacing: 2–32, line-width: 0,1–3 | Distanza e spessore dei tratti; spessore non superiore alla distanza |
| hatch-layers: 1–4 | Numero delle direzioni di tratteggio |
| angle / cross-angle | Direzione primaria e incremento angolare degli incroci |
| bend: 0–64 | Ondulazione dei tratti guidata dal campo |
| contrast / tone-offset / gamma | Contrasto, quantità di zone inchiostrate e distribuzione dei mezzitoni |
| tone-direction: normal / inverted | Inversione dei toni che guidano il tratteggio |
| wash: 0–0,5 | Campitura sotto i segni, separata dal loro spessore |
| mask: none / circle / diamond | Area inchiostrata; lo sfondo resta indipendente |
| mask-radius / mask-feather | Estensione e sfumatura del bordo; agiscono sulle maschere attive |
| center-x/y | Posizione del campo e della maschera |

Spessori, distanza e ondulazione dei nuovi artwork sono riferiti a un lato minimo di **1000 px**: su 1920×1080 si moltiplicano per 1,08. Campo e geometria mantengono la composizione quando si cambia risoluzione o si genera una preview proporzionale. Per vedere i tratti più sottili usare 100% o Raster = output.

Le maschere di questa opera sono locali al suo renderer. Un compositore generale di livelli, ricette annidate e blend M4C.3 resta da implementare.

## Domain Warp: dieci controlli aggiuntivi

| Controllo | Default neutro | Uso |
| --- | ---: | --- |
| lacunarity | 2 | Rapporto di frequenza fra ottave, 1–3,5 |
| persistence | 0,5 | Peso del dettaglio fine, 0,1–0,9 |
| warp-frequency | 1 | Scala indipendente del campo deformante, 0,2–3 |
| rotation | 0° | Orientamento del dominio, −180…180° |
| stretch-x / stretch-y | 1 / 1 | Scala dei due assi, 0,25–4; valori diversi allungano le venature |
| offset-x / offset-y | 0 / 0 | Spostamento nel dominio noise, −8…8 |
| contrast / gamma | 1 / 1 | Separazione dei toni e distribuzione del gradiente |

Sono tutti facoltativi nei JSON v1. Verificati i tre PNG Full HD della precedente M4B.1: **SHA-256 identici** dopo l'estensione. Prova in [`validation/M4D1_R1_compatibility.json`](validation/M4D1_R1_compatibility.json).

## Controlli nello Studio

Passo degli slider adattato all'intervallo: le frequenze piccole supportano passi di 0,00001, gli interi restano interi. Il campo esatto conserva valori più precisi del passo dello slider e accetta la virgola italiana. Il tooltip mostra intervallo e passo; lo slider evita decimali inutilmente lunghi dopo il trascinamento.

Corretto un errore emerso nello smoke: eventi TextChanged ritardati di editor rimossi potevano leggere parametri del generatore successivo, causando KeyNotFoundException o modifiche indesiderate. Ogni pannello ha ora una revisione e ignora gli eventi dei controlli precedenti. Lo stesso controllo protegge la conferma di un picker aperto da un editor ormai sostituito.

Smoke headless sullo Studio reale: cambio da Plasma ai nuovi generatori e Domain Warp, costruzione di tutti gli editor e campioni colore; input `0,0008123456789` conservato esattamente, slider a `0.00183` sincronizzato, `NaN` rifiutato. Log in [`validation/M4D1_R1_studio.log`](validation/M4D1_R1_studio.log). Verifica desktop e DPI reali ancora aperta.

## Otto nuove ricette Full HD

| File in examples/recipes | Scopo | Secondi locali |
| --- | --- | ---: |
| guilloche-banknote-1920.json | Rosetta delicata, 64 curve, carta cotone | 0,49 |
| guilloche-copper-orbits-1920.json | Trocoidi esterne, 32 curve, rotazione progressiva | 0,44 |
| guilloche-midnight-lace-1920.json | 72 curve, 18000 campioni, fine-2x | 0,30 |
| engraving-terrain-banknote-1920.json | Tre direzioni su rilievo tonale | 1,17 |
| engraving-copper-rings-1920.json | Anelli con maschera circolare sfumata | 0,77 |
| engraving-midnight-diamond-1920.json | Quattro direzioni, maschera a rombo, fine-2x | 1,53 |
| domain-warp-silk-detail-1920.json | Deformazione lenta, assi indipendenti, fine-2x | 4,02 |
| domain-warp-copper-ribbons-1920.json | Nastri diagonali, contrasto e gamma personalizzati | 1,30 |

PNG generati e decodificati a 1920×1080; anteprime in `examples/previews`. Il processo locale ha raggiunto circa **186 MiB** di working set di picco cumulativo; non è una misura isolata per algoritmo. Tempi e memoria non sono garanzie su altri PC. Report in [`validation/M4D1_R1_render-report.json`](validation/M4D1_R1_render-report.json).

## Budget e verifica

- Rimangono i limiti globali 4096×4096 e 16.777.216 pixel; fine-2x massimo 16.777.216 sottopixel. I due nuovi artwork richiedono almeno 64 px per lato.
- Guilloché: massimo quattro milioni di vertici complessivi, calcolati su campioni effettivi × curve prima di allocare la superficie.
- Incisione: massimo 400 milioni di campioni noise stimati, prima di allocazione/output. I tratti sono un campo analitico, non milioni di oggetti in memoria.
- **Codex, Windows, SDK 10.0.201:** build Release con zero avvisi/errori; **626/626 test** (Core 115, Algorithms 158, Application 128, Rendering.Skia 225). Log in [`validation/M4D1_R1_windows.log`](validation/M4D1_R1_windows.log).
- Test su chiusura, inviluppo, casi circolari, copertura tonale monotona, periodicità con tolleranza numerica, controlli effettivi, alpha/materiali, fine-2x, JSON storico, input preciso, cancellazione e budget prima della scrittura.

```powershell
clear && dotnet clean && .\tools\validate.ps1
dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- studio .
dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- detail .
```

Il tool detail valida tutte le 49 ricette e genera gli otto PNG in `artifacts/m4d1-smoke`. I comandi picker/render della milestone precedente restano disponibili. Il tool è separato dalla solution e usa la dipendenza Headless già introdotta in M4B.1. Nell'ambiente ristretto sono usati cache NuGet locale, audit online disabilitato e telemetria Avalonia disattivata solo per il processo di validazione. Restano da verificare poster 4K dei nuovi artwork, Linux/macOS e comportamento desktop su monitor differenti.

## Provenienza

Implementazione C# autonoma delle equazioni trocoidali e di un tratteggio analitico; nessun codice/asset esterno importato. Il controllo tonale del tratteggio appartiene alla famiglia di tecniche illustrata anche dagli autori di [Real-Time Hatching](https://gfx.cs.princeton.edu/proj/hatching/); qui non sono implementate le loro tonal art maps o il rendering 3D del paper.
