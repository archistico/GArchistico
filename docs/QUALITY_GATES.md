# Quality gates e criteri di accettazione

## Gate M4D.3-R1 — Profondità (verificato Windows headless)

Build Release senza avvisi/errori, 801/801 test; dieci nuovi PNG Full HD decodificati, due fine-2x, tutte le 73 ricette validate. Ombre gaussiane, blur alpha-safe, opacità di gruppo, aperture even-odd, seed/JSON e budget prima dell'output coperti dai test. Studio: controlli/default/ripristino/input numerico verificati headless. Quindici PNG precedenti identici a seed 42 / 160×120. [Guida ed evidenze](M4D3_PROFONDITA.md). Restano desktop/DPI reali, poster nuovi 4K e altri OS; i gate sotto documentano le revisioni storiche.

## Gate M4C.2-R3 — Test JSON, poster e anteprima (aperto)

Riscontro Windows R2: compilazione OK, 506/507 test superati; `RecipeTests.MissingParametersAreRejected` costruiva un JSON malformato per la trailing comma. Verificare R3 con suite completa, validazione semantica della rimozione `scale`, rendering ed export **2500×2000 standard con grana e/o carta** (anche Plasma), output PNG 2500×2000 decodificabile e alpha preservato. Verificare che `fine-2x` continui a rifiutare 2500×2000 prima di esportare (budget sottopixel) e che l'anteprima regolabile mostri tutta la composizione con `Adatta`, senza deformazioni, anche dopo resize; `100%` permette pan con bordi corretti. Misurare RAM/performance per poster grandi. Testare anche `Raster = output` e riquadro con dimensioni più piccole dello schermo. Dettagli `M4C2_R3_PREVIEW_E_BUDGET.md`.

## Gate M4C.2-R2 — Hotfix (aperto)

La build M4C.2-R1 è **fallita realmente su Windows** con CS8506/CS8131 su `StudioColorValue.cs:54`. In R2: ricontrollare build Release e suite xUnit, sei test HSV di settore, avvio Studio massimizzato con titolo e taskbar visibili, possibilità di ripristino/ridimensionamento. Le restanti verifiche materiche della R1 restano necessarie.


## Gate M4C.2-R1 — matericità raster (aperto)

Requisiti: build e suite; ricette JSON v1 caricate con nuovi parametri opzionali e vecchi default invariati; alpha e compositing trasparente preservati; carta cotton/rough, pigmento e assorbimento producono differenze per seed a parità di input; texture ferma nei loop. **Budget originale R1 di 4.194.304 pixel sostituito in R3 da 16.777.216** in standard; il renderer può avere ulteriori guard. Protocollo [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md).


**16 novembre 2026:** l'utente ha confermato **M4A.3-R1 Voronoi: compilazione e suite test riuscite su Windows**. M4A.2-R1 aveva già build, xUnit, Studio e CLI OK; M4A.1-R4 topografia poster era stata confermata funzionante. **M4A.4-R1** introduce parametri creativi, colori per ruolo, Poisson esteso e default Full HD; build e suite Windows della R1 **confermate dall'utente**. **R2** aggiunge colori e picker; **R3** aggiunge campionamento fine 2×, budget e PNG alpha-safe. R2/R3 e M4C.1/M4C.2 hanno gate Windows ancora aperto. Il gate artistico richiede anche confronto PNG manuale, non soltanto test sintetici.

## Gate M4C.1-R1 — grana e dithering ordinato (aperto)

Gate: 1) `dotnet clean` + `tools/validate.ps1` con SDK .NET 10; 2) `ArtisticPostProcessorTests` e `CatalogTests` tutti passati; 3) preview ed export PNG 1920×1080 di 4 ricette nuove; 4) confronti con finitura disattivata e default R3; 5) prova 2500×2000 standard con finitura **accettata** e `fine-2x` rifiutato se oltre budget; 6) verifiche alpha su sfondi chiari/scuri, riproducibilità e RAM/CPU Full HD e loop. Specifica [`M4C1_GRANA_DITHERING.md`](M4C1_GRANA_DITHERING.md).

## Gate M4A.4-R3 — campionamento 2× (aperto)

Per chiudere il gate: compilazione Release e suite xUnit R2/R3 intere; `RasterQualityTests`; export di esempio standard/fine-2x a Full HD decodificabile, confronto trasparenza e bordo Skia, test rifiuto 4000×4000 fine e assenza di file temporanei; monitorare RAM delle superfici native e un ciclo playback; nessun cambio del rendering default. Controlli statici e ZIP valido **non bastano** a chiuderlo.

## G0 — Controlli obbligatori per ogni modifica di codice

1. Solution .NET 10, restore, build Release, tutti gli xUnit.
2. Test di regressione per ogni bug e smoke reale per ogni nuovo percorso di rendering/esportazione.
3. Contratti/parametri validati, budget dimensioni/CPU/memoria, `IDisposable` su risorse native e cleanup di output incompleti.
4. Nessuna dipendenza di build o runtime Go/Kotlin/Python; PowerShell ridotto a `tools/validate.ps1`.
5. Diff di file e documentazione, ZIP completo e tracciabilità della versione esatta testata.

## G1 — Contratti principali

| Area | Verifiche |
| --- | --- |
| Numerica/noise | seed e golden, coordinate limite, infinities/NaN, overflow, regressioni |
| Colori/pixel | sRGB/linear, alpha premoltiplicato, stride, clipping, formato bitmap nativo |
| Flussi | Euler/RK2/RK4, griglie bilineari, curl, bordi e separazione streamlines |
| Skia | PNG decodificabile, pixel e dimensioni, dispose, differenze raster multipiattaforma |
| Catalogo | ID univoci, tipi/limiti/default corretti, descrittori coerenti |
| Ricette | schema v1, seed UInt64, round-trip, unknown e duplicate keys, file massimo 64 KiB |
| Animazioni | fase ciclica, ordine frame, loop continuo, FPS e frame cap, memoria bounded |
| GUI futura | non blocco UI, preview cancellabili, rendering fuori UI thread, UX e salvataggi |
| Export futuro | SVG/PDF davvero vettoriali, GIF/APNG/video decodificabili, gestione tempo e cancellazione |

## G3C.1 — Catalogo e primi loop

**Implementato e compilato/testato nell'insieme M3C.2-R1**. Restano da documentare con una prova specifica: `list`, `describe looping-flow`, `render-loop-frames` con 24 frame e manifest, ispezione visiva della continuità fra ultimo e primo frame e stabilità della memoria su sequenze lunghe. Queste verifiche non bloccano retroattivamente il risultato positivo della suite R1.

## G3C.2 — Ricette JSON

**Build e test R1 confermati dall'utente** dopo il fix `ThrowsAny<JsonException>`; smoke JSON e PNG confermati. Restano consigliati smoke di `recipe-show`, animazione `looping-flow` e differenze visive per `scale`/`palette`, senza presentarli come già verificati. Il decoder rifiuta input malformati e file oltre 64 KiB nei test predisposti.

## G3C.3 — Runtime comune (codice implementato, gate aperto)

- `render-recipe` gestisce i quattro generatori originari, più il nuovo `plasma-noise` (codice predisposto).
- Variazione `lines`, `max-lines`, `scale`, `loop-radius` e palette applicata concretamente.
- Ricette di esempio compatibili, seed e versioni stabili, test su invalid input.
- Tutti i test xUnit passati e PNG/animazione smoke reali; nessuna regressione della CLI precedente.

## Gate multipiattaforma, prestazioni e release

Per il desktop e l'export: provare Windows, Linux e macOS (quando disponibili) con asset Skia, misurare allocazioni managed/native su 800×600, Full HD e 4K, verificare la memoria per un numero elevato di frame. Per una release pubblica verificare licenze e transitive native dependencies, decidere licenza del prodotto, pacchetti RID, stabilità ricette e percorsi di distribuzione. Nessuna dichiarazione di prestazioni o supporto universale senza prove.

**Legenda:** previsto → implementato → build confermata → test confermati → smoke confermato → gate chiuso. Una conferma dell'utente costituisce evidenza valida; i controlli statici o CRC degli archivi non sostituiscono una build.


## G3D.1-R2/R3 — First Desktop e navigazione (build, test e pan/zoom confermati)

1. Solution con Studio in Release, tutti gli xUnit vecchi e i sette nuovi test Application verdi.
2. Catalogo e tre rendering visivamente corretti in GUI (parametric, plasma, flow); seed/parametri e zoom rispondono.
3. Roundtrip JSON v1 e PNG alla risoluzione richiesta; errori corretti e conferma della sovrascrittura nel picker.
4. Play/Pausa e scrub loop senza blocco della UI e senza crescita della RAM proporzionale al numero dei frame; possibili frame saltati da registrare.
5. Chiusura durante rendering, apertura di input invalidi, prova di export su file esistente; file locali non parziali.
6. Verifica Windows prioritaria; Linux/macOS non dichiarati validati senza prova.

## G3D.2-R1 — Export streaming cancellabile e editor preciso (BUILD/TEST/STUDIO CONFERMATI)

1. Build e suite xUnit Windows complete, inclusi `StudioParameterInputTests`, `AtomicOutputFilesTests` e PNG streaming Skia.
2. Output locale pubblicato solo dopo terminazione completa e flush; cancellazione/errore non devono sostituire una destinazione esistente né lasciare `.partial`.
3. Cancel abilitato solo con export attivo, nessun doppio job, UI reattiva e preview aggiornata dopo eventuali modifiche durante export.
4. Test decimali `.` e `,`, limiti validi, precisione round-trip e serializzazione JSON v1 invariata.
5. Regressione pan/zoom dell'anteprima R3 e playback di Looping Flow; export frame singolo e provider virtuali non atomici dichiarati.

**Evidenza 16/11:** build, suite e avvio Studio confermati dall'utente; stress dell'export con cancellazione e provider remoti non dichiarati provati.

## G3D.2-R2 — Preferenze e aspetto (BUILD/TEST/STUDIO CONFERMATI)

1. Build Release e suite completa inclusi `StudioPreferencesTests`.
2. Segnare due generatori preferiti, provare `Solo preferiti`, chiudere/riaprire e confrontare lo stato.
3. Aprire/salvare JSON, riaprire dal catalogo recenti, dimenticare una voce senza cancellare il file; caricare file mancante e JSON malformato senza crash.
4. Provare dark/light a viewport ingrandito e con playback attivo: nessuna modifica a ricetta, pan, zoom, frame o renderer; tema persistente dopo riavvio.
5. Corruzione del file settings e limite 64 KiB: avviso non fatale, file non sovrascritto automaticamente, scrittura successiva atomica.
6. Smoke manuale di memoria su centinaia di frame e export; Linux/macOS solo con evidenza separata.

## G4A.1 — Isolinee e primo artwork topografico (R4 CONFERMATA SU WINDOWS)

- Algoritmo Marching Squares con curve aperte/chiuse e decider bilineare delle ambiguità; validazione di costanti, piano, casi 5/10, cancellazione e limiti del campo scalare.
- Catalogo aggiornato a **6 opere**, nessuna regressione su schema JSON v1 e descrittori precedenti.
- PNG nuovo decodificabile, parametrizzazione effettiva, stessa seed/ricetta → stesso PNG sullo stesso backend; seed differente altera il risultato.
- Limiti `topographic-contours`: almeno 16x16, fino a 16.777.216 pixel (4096×4096 nel runtime statico corrente), fino a 40.000.000 celle×livelli; errore prima dell'apertura dello stream di output.
- Smoke visivo: continuità linee, livello/colore, assenza spurie, confronto tra palette, pan/zoom Studio e regressione dei cinque generatori esistenti.
- **Regressione R1:** due CS0618 su `SKPath.MoveTo/LineTo` con `TreatWarningsAsErrors`, build Windows interrotta.
- **Correzione R2:** SKPathBuilder/Detach e test polilinea scalata non chiusa. La R2 è stata confermata su Windows; R3 ha incontrato la regressione di budget e R4 è stata confermata funzionante dopo la correzione.

## G4A.2-R1 — SDF e Poisson disk (BUILD, TEST, STUDIO, CLI CONFERMATI SU WINDOWS)

- Catalogo a **8** opere, schema JSON v1, nessuna regressione ID/versioni precedenti.
- SDF/CSG: distanze negative all'interno, trasformazioni e operazioni numeriche finite; `SmoothUnion` con `k>0`.
- Poisson: punti interni alla superficie, separazione minima, griglia al massimo 6.000.000 celle, massimo 50.000 punti (nuovi limiti M4A.4-R1 da validare), cancellazione effettiva. I raggi grafici possono sovrapporsi anche quando i centri rispettano la distanza.
- Renderer: PNG SDF e Poisson, seed deterministico, effetto dei parametri, limite statico globale 4096×4096 e preview ridotta. Nessun PNG gigante nella suite automatica.
- Windows: `validate.ps1`, CLI reali e Studio; stress separato 4000×4000 e rollback export. Linux/macOS da documentare se testati.

## M4A.3-R1 — Voronoi / tassellazioni (GATE WINDOWS APERTO)

- Compilazione Release con `clear && dotnet clean && .\tools\validate.ps1`, suite xUnit integralmente verde, catalogo a nove opere senza regressioni.
- Celle Voronoi convesse e limitate al rettangolo, partizione area e bisettrice corretta, Lloyd deterministico e cancellabile, gestione esplicita dei siti degeneri.
- PNG del nuovo artwork decodificabile, parametri e seed efficaci, stesso formato JSON v1; `SKPathBuilder` senza API obsolete.
- Render reale 4000×4000 in Studio/CLI con monitoraggio RAM/tempo, cancellazione senza file parziali; anteprima a budget ridotto.
- Pan e zoom, preferiti, temi, topografia, SDF, Poisson e Play/Pausa precedenti senza regressioni.
- Nessuna dichiarazione di chiusura prima dei test Windows dell'utente; SDK .NET 10 assente nell'ambiente di preparazione.


## G4A.4-R2 — Tutti i ruoli colore e picker HSV (GATE WINDOWS APERTO)

- Conferma baseline: **M4A.4-R1 compila e tutti gli xUnit passano su Windows** secondo l'utente. R2 richiede un nuovo `clear && dotnet clean && .\tools\validate.ps1`.
- `StudioColorValueTests`: RGB/ARGB, HSV, alpha, validità input e roundtrip; `ArtisticColorRoleTests`: mappa dei ruoli per nove artwork e palette, colore Auto invariato, nuovo override raster effettivo, PNG alpha e JSON v1 legacy.
- Studio: `Scegli…` e `Auto` per ogni colore, RGB/HEX applicati esplicitamente, picker confermato/annullato senza modificare il resto, swatch sincronizzato quando cambia preset. Niente regressioni a pan/zoom, slider numerici, preferiti, tema, export e playback.
- CLI: PNG di ogni esempio `*-1920.json` decodificabile (senza sovrascrittura); per i loop conservare il limite 40M pixel cumulativi.
- Il test sintetico non garantisce qualità artistica; occorrono confronto manuale su Full HD, stress di memoria e successivamente Linux/macOS.
