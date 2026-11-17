# M4E.5-P4K-R1 — verifica Windows del 17/11/2026

## Gate automatici

Build Release con **zero avvisi e zero errori**, dopo clean/restore/build tramite `tools/validate.ps1`. **1.371 test superati**: Core 115, Algorithms 262, Application 281, Rendering.Skia 713; il gate rapido di 21 test è già compreso nel totale. [Log completo](P4K_R1_tests.log).

I test comprendono la preparazione UHD massima di tutti i generatori, convalida DCI/verticale, conservazione di seed/colori/alpha/animazioni, JSON v1, guardie prima dell'output, esclusione dell'8K generale, determinismo e dimensioni PNG, parità fra calcolo seriale e parallelo, distanza e copertura Poisson, distribuzione globale dei semi Streamlines e cancellazione. I test piccoli non sostituiscono le misure a piena risoluzione riportate sotto.

La preparazione massima dei **216 esempi JSON** e dei **30 preset incorporati** supera la convalida del renderer: [preflight](P4K_R1_preflight.json). Non sono stati renderizzati tutti i 216 esempi: la matrice reale comprende un preset massimo per ciascun generatore e quattro varianti che esercitano altri percorsi (Voronoi stratificato, Cincinnus Adaptive, SDF glass e Crinis classic).

Il pulsante **4K · qualità massima** è stato attivato nello Studio headless per tutti i 30 generatori; ogni ricetta risultante è identica a quella preparata dal servizio condiviso usato dalla CLI, incluse le opere animate. Il preview in coda è stato cancellato alla chiusura; questo controllo verifica l'azione dell'editor, non 30 render GUI interattivi. [Risultati Studio](P4K_R1_studio.json). Screenshot reale dello Studio aggiornato con il nuovo comando nel README.

## Compatibilità

Conservati tutti i 740 file elencati nel manifest precedente, più il manifest stesso. **182 vecchi esempi e 30 file preset incorporati invariati byte per byte**, inclusi i 17 personali. Quattro PNG storici Voronoi e un PNG Poisson `balanced` rimangono identici. L'indice dei cappi delle Streamlines conserva il PNG 4K prima della successiva correzione della distribuzione: questa prova è distinta dal nuovo ordinamento dei semi. [Inventario](P4K_R1_compatibility.json), [hash e confronto algoritmi](P4K_R1_algorithm-compatibility.json).

Solo il profilo `production` modifica distribuzione dei semi Streamlines, popolamento globale Poisson, tratto continuo delle Streamlines e campionamento 2× Ocellus/Rete. I budget precedenti e le ricette `balanced` mantengono la semantica precedente. Nessun nuovo pacchetto NuGet di produzione.

Una ripetizione della build ha incontrato DLL temporaneamente bloccate. Dopo `dotnet build-server shutdown`, lo script completo è passato senza cambiare impostazioni del progetto né chiudere applicazioni dell'utente. Il log collegato documenta l'esecuzione finale riuscita.

## Render reali e lettura delle misure

Ogni caso usa **3840×2160 finali**, profilo `production`, campionamento `fine-2x` e controlli di complessità massimizzati. I PNG vengono decodificati e confrontati con le dimensioni della ricetta. Per le animazioni si misura solo il frame 0; non si afferma di aver verificato intere sequenze 4K.

Ambiente Windows, SDK 10.0.201, 32 processori logici, memoria disponibile al runtime circa 32 GiB; il parallelismo raster è limitato a 16 worker. Tempi comprensivi di rendering, finiture e codifica PNG; picco working set del singolo processo, rilevato dopo la decodifica. Sono misure indicative su questo host, con attività di sviluppo presenti: non garanzie di latenza né misure di un banco prova isolato. Non vengono ridotti risoluzione o parametri per rispettare un tempo massimo.

La revisione della prima contact sheet ha rilevato che i massimi delle Streamlines e di Poisson riempivano soltanto parte della tela. Le versioni finali distribuiscono i semi su tutto il dominio. Poisson sale a 500.000 punti espliciti; i controlli di distanza restano attivi. L'ordinamento globale delle Streamlines può richiedere più lavoro della scansione precedente, perché produce curve lunghe anche nelle regioni prima vuote.

È stato inoltre corretto l'hash dell'indice spaziale di produzione: il normale hash Int64 faceva XOR delle coordinate impacchettate, concentrando milioni di celle in poche migliaia di hash. La nuova combinazione evita queste collisioni, mantenendo uguaglianza delle chiavi e ordine delle liste. Con distribuzione globale, il run con l'hash precedente è stato cancellato dopo oltre 17 minuti; quello corretto ha completato PNG e codifica in **28,56 secondi**, con picco **1.670 MiB**. Il percorso `balanced` conserva il suo indice precedente. I confronti punto per punto fra i due percorsi sono nei test automatici.

La ripetizione 4K in un secondo processo ha richiesto 28,25 secondi e ha prodotto lo stesso PNG SHA-256 `cf5d90aededd8b7be358a66acf2b4e571266026a0d4124bc0ed6f7211e2bf860`, verificando anche che la diversa distribuzione hash fra processi non influisca sull'immagine. Nel set finale il caso più lento è Crinis micro (222,32 secondi); il picco di memoria maggiore è Streamlines (circa 1,63 GiB).

La [contact sheet](../images/production-4k-contact-sheet.png) mostra tutti i casi; i [crop nativi](../images/production-4k-details.png) contengono porzioni centrali 512×512 al 100%, senza ingrandimento o ricampionamento. Il [rapporto JSON](P4K_R1_renders.json) conserva seed, parametri completi, hash SHA-256, dimensioni, byte, tempi e memoria di ciascun PNG.

Questa verifica conferma il percorso tecnico di produzione 4K. Restano distinti la valutazione artistica dell'utente, i test desktop/DPI interattivi, gli altri sistemi operativi e l'export 8K generale richiesto come tappa successiva.

## Misure per ricetta

| Ricetta | Secondi | Picco MiB | PNG MiB |
|---|---:|---:|---:|
| alien-letters | 0,84 | 201 | 0,52 |
| bokeh-depth | 43,43 | 707 | 9,68 |
| cincinnus-adaptive | 15,47 | 244 | 20,70 |
| cincinnus-bristles | 12,39 | 247 | 18,91 |
| cotton-circles | 0,54 | 195 | 0,93 |
| crinis-bristles | 222,32 | 445 | 21,46 |
| crinis-classic | 19,01 | 364 | 19,99 |
| domain-warp | 5,17 | 537 | 18,27 |
| eclectic-trails | 0,63 | 199 | 2,14 |
| fascia-ribbons | 1,57 | 213 | 3,91 |
| flow-brush | 26,07 | 361 | 18,30 |
| guilloche | 10,74 | 336 | 12,10 |
| lic-flow | 21,71 | 790 | 16,21 |
| looping-flow | 3,55 | 342 | 7,26 |
| noise-flow | 9,18 | 362 | 19,13 |
| ocellus-eyespots | 18,83 | 537 | 2,62 |
| paper-cut | 55,93 | 682 | 10,59 |
| parametric-curves | 3,82 | 329 | 8,30 |
| phyllotaxis | 0,68 | 198 | 3,13 |
| plasma-noise | 2,02 | 536 | 22,52 |
| poisson-stippling | 6,20 | 385 | 16,66 |
| reaction-diffusion | 2,01 | 571 | 1,21 |
| rete-foam | 5,05 | 1176 | 2,67 |
| rigor-grid | 0,53 | 192 | 0,39 |
| sdf-glass | 14,57 | 844 | 2,23 |
| sdf-orbits | 18,56 | 852 | 7,64 |
| spaced-streamlines | 28,56 | 1670 | 22,56 |
| spiral-flamebrush | 1,25 | 214 | 4,55 |
| strata-landscape | 116,79 | 691 | 7,15 |
| tonal-engraving | 4,24 | 537 | 23,17 |
| topographic-contours | 4,12 | 372 | 18,32 |
| voronoi-layered | 8,73 | 706 | 4,21 |
| voronoi-mosaic | 2,74 | 344 | 8,58 |
| wave-interference | 2,50 | 536 | 4,60 |
