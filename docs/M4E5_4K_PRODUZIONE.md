# M4E.5-P4K-R1 — produzione 4K con massimo dettaglio

## Uso

Tutti i **30 generatori** possono renderizzare ed esportare in **3840×2160**, oppure **2160×3840**, con `render-profile=production` e `render-quality=fine-2x`. Il raster temporaneo è 7680×4320 e viene ridotto una sola volta al PNG finale. Il profilo ammette anche DCI 4096×2160 e il suo orientamento verticale: massimo 4096 px per lato, **8.847.360 pixel finali / 35.389.440 nativi**. Non comprende 4096×4096.

Nello Studio premere **4K · qualità massima**, sotto le dimensioni. L'azione prepara una nuova configurazione dell'editor e avvia l'anteprima nativa. Salvare la ricetta, poi esportare il PNG; Adatta/100% agiscono solo sulla vista. Seed, palette, colori individuali e alpha, stili, impostazioni compositive e animazione sono conservati. I file dei preset non vengono riscritti. Tutti i parametri restano modificabili.

CLI, da PowerShell nella cartella della soluzione:

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- prepare-4k .\examples\recipes\voronoi-depth-v1\voronoi-depth-glass-windows-1920.json .\output\voronoi-4k.json
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\output\voronoi-4k.json .\output\voronoi-4k.png
```

Per conservare il dettaglio originale aggiungere `--keep-complexity` al comando `prepare-4k`. Nell'editor l'equivalente è selezionare il profilo `production`, dimensioni UHD e campionamento `fine-2x` senza premere il pulsante di massimizzazione. Le destinazioni CLI devono essere nuove. Per un'opera animata `prepare-4k` conserva frame/FPS e `render-recipe` esporta la sequenza; nello Studio è possibile esportare il fotogramma corrente. Nessuna nuova funzionalità temporale introdotta.

## Significato di complessità massima

Il comando porta agli estremi del catalogo i controlli di quantità e campionamento pertinenti, riducendo le distanze/griglie dove producono più dettaglio. Non massimizza opacità, dimensioni delle macchie, sfocatura, spessore o feed/kill: questi controlli descrivono la composizione e alzarli insieme distruggerebbe il risultato. Gli stili restano quelli della ricetta: per esempio, il Voronoi piatto resta piatto; la variante stratificata pronta è `production-4k/voronoi-layered.json`.

| Generatore | Quantità/precisione del profilo massimo |
|---|---|
| Alien Letters | 12×12 celle, 10 suddivisioni |
| Bokeh | 4.000 particelle, 32 piani |
| Cincinnus | 12 vortici, 200 principali, 5.000 tentativi e 2.000 patch, 20.000 punti, distanza/passo setole minimo |
| Cotton Circles | 48×48 celle |
| Crinis | 100 fasci, 1.200 campioni, 8 vortici, 6.000 particelle; densità/microdettaglio massimi, passo setole minimo |
| Domain Warp | 6 ottave, 3 livelli di deformazione |
| Eclectic | 500 tracce × 800 passi, 8 vortici |
| Fascia | 70 fasce × 700 passi, 8 vortici |
| Flow Brush | 600 pennellate × 800 passi × 40 setole |
| Guilloché | 200 livelli × 48.000 campioni, 32 onde |
| LIC | 48 passi per direzione, campionamento nativo 2× |
| Looping Flow | 2.000 linee × 800 passi |
| Noise Flow | 10.000 linee × 1.000 passi |
| Ocellus | 80 occhi, profondità minima 13; raster nativo 2× |
| Paper Cut | 64 livelli × 4.096 campioni |
| Curve parametriche | 160 livelli × 24.000 campioni |
| Fillotassi | 16.000 semi |
| Plasma | 6 ottave |
| Poisson | fino a 500.000 punti, distanza minima; distribuzione globale |
| Reazione–diffusione | celle da 3 px, 1.200 iterazioni, 128 inoculi |
| Rete | 850 bolle × 5 livelli, densità massima; raster nativo 2× |
| Rigor | 16×18 celle |
| SDF | 40 macchie × 6 livelli |
| Streamlines distanziate | fino a 10.000 linee × 800 passi per direzione, distanza 1 px |
| Flamebrush | 25 anelli × 300 timbri × 60 particelle |
| Strata | 120 livelli × 4.096 campioni, 7 ottave |
| Incisione tonale | 4 tratteggi, 5 ottave; distanza minima compatibile con lo spessore |
| Topografia | 24 isolivelli, celle da 3 px |
| Voronoi | 196 siti, 3 rilassamenti; 8 piani e densità posteriore massima quando `layered` |
| Interferenze | 12 sorgenti |

Questi numeri sono limiti di generazione e campionamento, non una promessa sul numero di elementi visibili: Poisson rispetta le distanze, le streamlines terminano ai bordi/incontri/cappi, Cincinnus rispetta i criteri geometrici e la disponibilità di spazio. Restano le saturazioni interne artistiche già previste nei renderer e l'opzione `require-complete`; non si cambia il seed finché viene accettato un risultato.

## Motore e compatibilità

- `render-profile` è opzionale nel JSON v1 e vale `balanced` quando assente. I budget precedenti restano attivi per le ricette esistenti. Il profilo di produzione è esplicito e condiviso da Studio e CLI.
- Estesi i budget di superfici, filtri, campioni LIC, simulazione Gray–Scott, tracciamento e pittura. Il controllo dimensionale precede allocazione e scrittura. Nessun downsampling nascosto, nessun abbassamento automatico dei parametri per far passare il controllo.
- I campi raster lavorano per righe disgiunte, al massimo con 16 worker CPU. La simulazione sincronizza le righe prima di scambiare gli array. SDF usa bande con righe di bordo per conservare le normali. Il canvas Skia non viene usato contemporaneamente da più thread.
- Ocellus/Rete in produzione campionano davvero il raster 2×; il percorso `balanced` conserva i pixel storici. Le streamlines di produzione disegnano una polilinea per curva, evitando l'accumulo alpha delle estremità di ogni segmento. Il controllo dei cappi usa un indice spaziale con le stesse distanze float della versione precedente. I semi iniziali delle streamlines coprono tutta la tela anche quando si raggiunge presto il limite di linee. Poisson distribuisce candidati globali prima della crescita locale Bridson, sempre rispettando la distanza minima; fino a 500.000 punti nel profilo di produzione (50.000 in balanced).
- I piani filtrati vengono elaborati in successione. La memoria dipende da superfici temporanee, campi e filtri: il solo framebuffer non descrive il consumo del processo. Eseguire esportazioni pesanti in successione e attendere il completamento dell'anteprima.
- L'indice spaziale delle Streamlines di produzione miscela entrambe le coordinate nell'hash. Evita le collisioni sistematiche dell'hash Int64 sulle chiavi impacchettate; il dizionario non viene enumerato, perciò non cambia l'ordine delle curve. Il caso UHD massimo completa il render misurato in 28,56 secondi, mentre la prova con la vecchia funzione hash era ancora attiva dopo oltre 17 minuti ed è stata cancellata.
- **30 file preset incorporati, inclusi i 17 personali, invariati byte per byte**. I 182 esempi precedenti sono conservati; aggiunte 34 ricette in `examples/recipes/production-4k`: una per generatore e quattro varianti di percorso.

## Verifica riproducibile

```powershell
.\tools\validate.ps1
.\tools\validate-4k.ps1 -OutputDirectory artifacts/prova-4k
dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- four-k-studio .
dotnet run --project .\tools\GenerativeArt.Smoke -c Release -- voronoi-compat .
```

Usare una nuova cartella per ciascun audit 4K. `validate-4k.ps1` prepara i massimi, convalida gli esempi e renderizza i 30 preset in processi separati; salva ricette, PNG, hash, tempi comprensivi di codifica e picco working set per processo, quindi una contact sheet. L'audit automatico delle animazioni renderizza solo il frame 0, non tutte le sequenze. `FourKValidation render <ricetta> <cartella>` permette di misurare altre varianti. `sheet` crea la vista d'insieme; `details` estrae sei crop centrali 512×512 al 100%, senza ricampionamento.

I risultati della consegna sono nel [rapporto di validazione](validation/P4K_R1_REPORT.md), con [contact sheet](images/production-4k-contact-sheet.png) e [crop nativi](images/production-4k-details.png). La convalida preventiva di tutte le ricette è distinta dal render reale dei 30 generatori e delle quattro varianti aggiuntive. I test non equivalgono all'approvazione artistica dell'utente o a una prova desktop/DPI interattiva.

## Passo successivo

**L'export 8K generale è successivo**, come richiesto dall'utente. Un PNG finale 8K con supersampling 2× richiede quattro volte la superficie di lavoro del 4K: serviranno nuove misure e una strategia per memoria/filtri/campi, senza estendere indiscriminatamente questi limiti. I quattro percorsi geometrici `balanced` che già ammettevano 8K rimangono disponibili; `production` respinge l'8K prima di scrivere output.
