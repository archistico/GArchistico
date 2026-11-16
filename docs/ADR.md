## ADR-M4C2-001 — Finitura materica opzionale (16 novembre 2026)

- **Decisione implementata; gate Windows aperto:** il catalogo condiviso aggiunge ai nove generatori quattro parametri `Materia` facoltativi nel JSON v1, tutti di default spenti. `ArtisticFinishOptions` esteso in modo additivo e nessun cambio agli ID dei generatori, ai seed o a `algorithmVersion`.
- **Decisione implementata:** `MaterialPostProcessor` calcola fibra e deposito su raster finale tramite noise interpolato seeded, prima della grana e del dithering di M4C.1. La componente di assorbimento legge il raster originale con un ring buffer di massimo tre righe, evitando una seconda immagine completa in RAM. Il modello è artistico, non una simulazione fisica del pigmento.
- **Decisione implementata:** alpha e via PNG senza finitura invariati, limite di 4.194.304 pixel già previsto per la finitura. Regole di cancellazione e frame stateless conservate.

---

> **Stato operativo:** M3C.2-R1 ora validata su Windows (build, xUnit, JSON e PNG), confermata dall'utente. Le diciture temporanee di questo registro sono storiche; vedi [`CURRENT_STATE.md`](CURRENT_STATE.md).

# Decisioni architetturali — stato iniziale

- **ADR-001 (operativa, da validare con compilazione):** C#/.NET 10 e quattro progetti, Core e Algorithms senza UI.
- **ADR-002 (operativa):** `CanvasSize` rifiuta dimensioni non positive o oltre 67.108.864 pixel; area calcolata in 64 bit. Il limite può cambiare con una configurazione di budget futura.
- **ADR-003 (operativa):** `Color32` conserva ARGB straight; la premoltiplicazione è esplicita, il colore alpha zero diventa nero trasparente durante roundtrip premul.
- **ADR-004 (operativa):** `SplitMix64Random` è RNG v1, `Fork` deriva sottostream dal seed iniziale e da ID UTF-8 FNV-1a. Non è Kotlin Random.
- **ADR-005 (operativa):** frame time derivato da indice/FPS, non da callback di rendering o tempo di sistema.
- **ADR-006 (operativa):** Perlin classico con tabella originale di Gȧrt, test di compatibilità numerica; differenziare eventuali noise seeded.
- **ADR-007 (proposta):** rendering headless via SkiaSharp adapter, nessuna dipendenza UI nei contratti.
- **ADR-008 (proposta):** encoder a streaming, GIF con timing quantizzato in modo esplicito, FFmpeg opzionale.
- **ADR-009 (proposta):** pixel buffer con stride, limiti di allocazione e controllo ownership.

Nessuna di queste decisioni implica che i nuovi assembly siano stati compilati. La scelta delle API e i test sono parte del progetto ma la compilazione .NET resta un gate aperto.

## M4C.1-R1 — Finitura artistica post-raster, seeded e budget limitato (16/11/2026)

**Decisione:** i controlli di grana e dithering sono parametri opzionali delle ricette JSON v1, condivisi da tutti gli artwork. Applicazione centralizzata dopo rendering e dopo eventuale downsampling fine-2x; se disattivati, output attraverso lo stesso percorso PNG delle revisioni precedenti. Grana mono per coordinate+seed e Bayer ordinato per canale RGB, alpha invariato e texture spaziale stabile in animazione.

**Motivi:** non duplicare algoritmi nei nove renderer, conservare risultati storici e riproducibilità, evitare flicker nei loop; limitare l'occupazione transitoria a 4.194.304 pixel finali quando serve un PixelBuffer addizionale.

**Trade-off:** la conversione da Skia a buffer gestito usa una bitmap temporanea e al momento privilegia robustezza/semplicità rispetto a una lettura bulk ottimizzata. Manca benchmark reale Windows e QA cross-platform; error diffusion, trama carta, grana in spazio colore lineare e maschere avanzate sono differiti. Schema ID, versioni algoritmo e vecchi JSON invariati. **Gate Windows R2/R3/M4C.1 aperto.**


## Decisioni M1B

- **ADR-010 (operativa, da convalidare con build):** `NormalizedRgba` è sRGB straight in double [0,1], `Color32` resta ARGB byte; conversione 8 bit con `MidpointRounding.AwayFromZero`. Non confondere con RGB lineare né con premultiplied.
- **ADR-011 (operativa):** Hue HSV/HSL in gradi [0,360). Per acromatici H=0, anche quando Gȧrt restituisce `NaN`; il mix segue l'arco corto e l'esatto antipodo privilegia -180°. Alpha preservato.
- **ADR-012 (operativa):** OKLab conserva la pipeline XYZ/Bradford di Gȧrt per compatibilità verificabile; la componente L può superare lievemente 1 per errori numerici (il bianco Kotlin è ~1.000001), non viene troncata in ingresso. Limitazione del gamut solo in conversione a sRGB.
- **ADR-013 (operativa):** `SeededPerlin3D` usa Fisher–Yates con `SplitMix64-v1`, indipendente dalla tabella Kotlin fissa e da OpenSimplex. ID `seeded-perlin3d-splitmix64-v1`; un cambio della permutazione richiede un nuovo ID.
- **ADR-014 (operativa):** fBm normalizzato: ottave 1..16, lacunarity finita positiva, persistence finita [0,1]; ingressi/coordinate non finiti vietati. Le curve Bézier valide usano t in [0,1].

Tutte le decisioni indicate come operative descrivono il **codice scritto**, non una compilazione o un test .NET riuscito.

## Decisioni di pianificazione P1 (2026-11-16)

- **ADR-015 (definitiva, chiarita in M3D.1-R2):** implementazioni, algoritmi, logica GUI, test e benchmark in C#/.NET 10; un minimo di PowerShell operativo. Avalonia XAML/AXAML è consentito per dichiarare la UI. Nessun codice eseguibile Python, Go, Kotlin, Java o linguaggi ulteriori senza autorizzazione dell'utente. JSON, configurazioni e documentazione sono ammessi.
- **ADR-016 (definitiva):** il repository Gȧrt originale e gli harness usati per l'audit rimangono esterni; nel progetto conservare solo vettori numerici/attribuzioni rilevanti. Il codice `.cs` M1B non cambia con la pianificazione P1.
- **ADR-017 (gate):** chiudere formalmente M0 solo dopo restore/build/test .NET 10 realmente riusciti e tracciati; non estendere M1 prima di aver risolto i problemi di base dimostrati dal compilatore.
- **ADR-018 (architettura proposta):** progetti aggiunti solo al primo bisogno verificabile; Core e Algorithms puri, Application indipendente da Skia concreto, UI e CLI come composition root.
- **ADR-019 (superata in M3D.1-R2):** l'ipotesi iniziale di Avalonia esclusivamente code-only non è un requisito dell'utente. XAML/AXAML è permesso e valutabile nelle prossime revisioni; il primo Studio resta code-only perché il fix attuale riguarda il bootstrap.
- **ADR-020 (decisione rinviata):** backend MP4/FFmpeg e licenze: scegliere solo a M6; non includere binari codec senza verifica.

## Decisioni M1C (2026-11-16)

- **ADR-021 (eccezione temporanea):** data l'indisponibilità dell'SDK in ambiente e la temporanea impossibilità dell'utente di testare, autorizzato un incremento limitato di funzioni pure M1, senza dipendenze esterne né chiusura G0/G1. Le API rimangono provvisorie fino a build e xUnit realmente riusciti. Non estendere M2 prima di superare G0.
- **ADR-022 (implementata, da verificare):** `System.Numerics.Matrix3x2` come rappresentazione nativa di trasformazioni 2D, convenzione row-vector e composizione applicativa documentata `first * second`; valori non finiti rifiutati, inversione esplicita.
- **ADR-023 (implementata, da verificare):** sRGB encoded `NormalizedRgba` e RGB lineare straight `LinearRgba` sono tipi differenti; trasferimento IEC 61966-2-1 con alpha invariato.
- **ADR-024 (implementata, da verificare):** gradienti immutabili con stop strettamente crescenti; interpolazioni `Srgb` e `LinearSrgb`, alpha straight e clamping esterno; nessuna allocazione del buffer grafico.

## Decisioni M1D (2026-11-16)

- **ADR-025 (implementata, non validata):** Simplex classico 2D/3D con permutazione fissa Gȧrt, separato esplicitamente dalla variante seeded; entrambe usano un kernel puro C#. ID `gart-classic-simplex-fixedperm-v1` e `seeded-simplex2d3d-splitmix64-v1`. Non è OpenSimplex.
- **ADR-026 (implementata, non validata):** shuffle Fisher–Yates a 256 elementi condiviso da `SeededPerlin3D` e `SeededSimplexNoise`; stessa sequenza SplitMix64-v1 già introdotta in M1B. Cambiamenti incompatibili obbligano a nuovo ID e golden.
- **ADR-027 (implementata, non validata):** coordinate Simplex non finite o con celle skewed oltre `Int32` rifiutate. I risultati non sono clamped; l'algoritmo è immutabile dopo la creazione.
- **ADR-028 (eccezione limitata):** ulteriore avanzamento matematico M1D senza SDK dopo M1C, richiesto per continuità, ma nessun ampliamento della superficie grafica e **G0/G1 ancora aperti**. Stop al codice di rendering finché non sarà eseguibile una build.

## Decisioni M2A preparatorio (2026-11-16)

- **ADR-029 (eccezione circoscritta, da validare):** per proseguire senza SDK, autorizzato soltanto il buffer pixel managed nel Core e i relativi test, non l'adapter SkiaSharp né rendering/export. Questo non chiude G0/G1 né attiva G2.
- **ADR-030 (implementata, non validata):** `PixelBuffer` possiede internamente un `uint[]` con stride in pixel, `CanvasSize` e budget massimo comprensivo del padding. Le operazioni rettangolari validano i bordi in `long`; il tipo managed non usa `IDisposable`.
- **ADR-031 (implementata, non validata):** formato logico `uint 0xAARRGGBB`, alpha straight. Nessuna assunzione di endianness per accesso a byte o reinterpretazione come bitmap Skia. Conversione premultiplied sarà compito esplicito dell'adapter M2.
- **ADR-032 (implementata, non validata):** le API di riga espongono solo `Width` pixel, la copia con stride convalida prima di mutare e il padding non fa parte dell'immagine. Accessi invalidi: strict con eccezione oppure Try* senza scritture esterne.

## Decisione M2A-R2 (2026-11-16)

- **ADR-033 (test numerici, implementata ma da validare):** mantenere le funzioni di trasferimento IEC sRGB con cutoff pubblicati `0.04045` (sRGB) e `0.0031308` (linear). Non forzare l'invertibilità esatta nel punto di soglia, dove gli arrotondamenti normativi introducono una discontinuità di circa `2.95919e-8` nella composizione `ToSrgb(ToLinear(0.04045))`. Il test di roundtrip rigoroso (`1e-12`) esclude esclusivamente il breakpoint, verificato invece da test numerico dedicato; golden di produzione invariati. Build Release precedente riuscita, 179/180 test xUnit eseguiti su Windows prima del fix; G0 ancora aperto fino al nuovo esito.


## Decisioni M2B (2026-11-16)

- **ADR-034 (gate):** sulla base del feedback utente, la baseline M2A-R2 ha compilazione e suite xUnit superati dopo estrazione in cartella pulita; G0/G1 sono chiusi per tale baseline. M2B richiede una nuova validazione con native assets.
- **ADR-035 (implementata, da validare):** SkiaSharp 4.153.2 è confinato in `GenerativeArt.Rendering.Skia`. Core/Algorithms non dipendono da Skia né da Avalonia. Per Linux il package native assets `NoDependencies` è referenziato esplicitamente.
- **ADR-036 (implementata, da validare):** adapter da PixelBuffer ARGB uint straight a BGRA byte Unpremul, con copia su SKBitmap autonoma. Nessun riutilizzo di memoria pinned o reinterpretazione implicita di endianness. Lo snapshot di superficie opera in Premul.
- **ADR-037 (implementata, da validare):** prima CLI C# `render-demo` direttamente sulla superficie raster, senza Application framework/GUI, per validare la pipeline end-to-end. Introduzione di Application e artwork runtime in M3.

## ADR M3A — Campi vettoriali managed e integrazione esplicita (proposta implementata)

- Usare `System.Numerics.Vector2` come risultato per campionamento, senza dipendenza da SkiaSharp.
- Separare avanzamento per direzione (norma 1) e per velocità, come `flow2` di Gȧrt.
- Euler/RK2/RK4 campionano rispettivamente 1/2/4 volte; `TryAdvance` segnala degenerazioni senza creare punti non finiti.
- Conservare i percorsi in un elenco soltanto al confine `FlowTracer`; limite esplicito di 200.000 passi.
- Il renderer CLI può usare i campi tramite Algorithms; il progetto Core rimane indipendente.
- M3A non cambia il formato dei pixel né le interfacce native Skia già confermate dai test Windows dell'utente.

## Decisioni M3B (16 novembre 2026)

- **ADR-031 (operativa da validare):** `VectorGrid2D` copia il vettore sorgente e usa bilinear in coordinate canvas; nodi al bordo inclusi per il sample, campo nullo fuori dal dominio; limite di 4.000.000 nodi.
- **ADR-032 (operativa da validare):** `CurlNoiseField2D` usa differenze finite centrate da Simplex seeded. Le derivate *analitiche* del progetto Go sono un'evoluzione separata, da verificare numericamente.
- **ADR-033 (operativa da validare):** `EvenStreamlineTracer` è bidirezionale RK4, con semi deterministici, lookup spaziale e limiti su candidati, curve, passi, dimensioni e 2.000.000 campioni; il vincolo di distanza è campionato.
- **ADR-034 (definitiva):** Gȧrt e jdxyw/generativeart rimangono **fonti di studio**, non dipendenze runtime. Un solo codice prodotto C#/.NET 10; mantenere avvisi MIT e BSD quando necessari.
- **ADR-035 (proposta M3C):** metadati delle opere pubblici e tipizzati con schema/versione, non reflection su campi privati. ID stabile e parametri validati prima di eseguire la renderizzazione.
- **ADR-036 (proposta M5/M6):** API e GUI locali; animazioni con streaming, mai accumulo obbligato di tutti i frame; ricette PNG con CRC e limiti di lettura.


## ADR M3C.1 — Opere registrate e fase ciclica

- **Catalogo:** ID ASCII kebab-case, versioni positive, metadati espliciti e parametri con tipo, default e limiti. Nessuna reflection dei campi privati. Le demo esistenti sono descritte ma non ancora migrate sul dispatcher unificato (M3C.3).
- **Animazione:** le capacità Animated/SeamlessLoop sono dichiarate e testabili. Il tempo del loop nasce da `LoopFrame` e non da `DateTime`/callback di GUI. Prima fase: funzione periodica che dipende da sin/cos, quindi fase 0 = fase 1 matematica.
- **Output:** ogni PNG è scritto e rilasciato prima del successivo; niente array di bitmap di tutta l'animazione, a differenza dell'implementazione Go studiata. Per GIF/APNG/video definire un sink bounded e gestione licenze/codec.
- **Priorità esperienza:** prima GUI Avalonia con Play/Pausa/Loop su un generatore; codec GIF/APNG anticipati rispetto al video avanzato.
- **Gate:** implementato come sorgenti; da validare realmente su .NET 10.


## Decisioni M3C.2 — Ricette versionate (16 novembre 2026)

- **ADR-REC-001:** ricetta JSON v1 indipendente da Skia e GUI, contenente formato, ID/versione algoritmo, seed UInt64 decimale stringa, dimensioni, parametri JSON tipizzati e animation opzionale. Non migrare implicitamente algoritmi incompatibili.
- **ADR-REC-002:** `ArtworkParameterSpec` resta la fonte unica delle regole; codec respinge proprietà ignote/duplicate, tipi errati, file >64 KiB e input JSON fuori schema. Parametri completi e ordinati dopo validazione.
- **ADR-REC-003:** `SaveNew` non sovrascrive il file di ricetta. L'esportazione frame conserva i limiti esistenti e non accumula immagini in memoria.
- **ADR-REC-004:** primo dispatch ricetta circoscritto alle curve parametriche e al loop; le due demo flow restano renderizzabili tramite comandi legacy fino al runtime registrato M3C.3.
- **ADR-REC-005:** lo stato del gate per M3C.1/M3C.2 resta aperto finché .NET 10 non avrà eseguito build e test; lo schema ricetta potrà richiedere evoluzioni esplicite con nuova versione.


## ADR-M3C3-001 — Renderer a stream separato dalla CLI (16/11/2026)

Un contratto `IArtworkRasterRenderer` è dichiarato in Application e implementato dal runtime Skia. Tutti i comandi CLI usano la stessa implementazione con input `ArtworkRecipe` già validato. Il render di un frame non possiede lo stream, non conserva gli altri frame e rilascia Skia al termine; la futura GUI potrà fornire stream RAM e token di cancellazione senza dipendere da file e processi CLI.

## ADR-M3C3-002 — Output senza sovrascrittura, cleanup localizzato

Il writer CLI usa `CreateNew` per PNG e manifest. La directory frame deve essere nuova o vuota; il writer cancella su fallimento solo i file creati dalla sua invocazione e scrive il manifest per ultimo. Il manifest non è garanzia di commit atomico a livello filesystem. Il limite di rendering dipende dall'artwork oltre che dal canvas. In questa milestone non sono disponibili GIF e video.

## Decisioni M3D.1-R1 — 16 novembre 2026

- **ADR-050 (implementata, gate aperto):** Avalonia 12.1.6 scelta per la prima UI, interamente in C# senza `.axaml`/XAML. Studio inserito nella solution, condividendo lo stesso `ArtworkRegistry`, `ArtworkRecipe` e `IArtworkRasterRenderer` della CLI. SDK non disponibile qui: build/smoke Windows ancora richiesti.
- **ADR-051 (implementata, gate aperto):** `StudioPreviewPolicy` limita la ricetta di anteprima a 800×600 e 480.000 pixel preservando seed, versione, parametri, numero frame e FPS. Non modifica la ricetta di esportazione.
- **ADR-052 (implementata, gate aperto):** rendering Studio fuori dal thread UI, richieste obsolete cancellate, massimo un lavoro di rasterizzazione contemporaneamente; in playback senza debounce si saltano tick se il rendering non segue il ritmo. FPS non garantiti.
- **ADR-053 (implementata, gate aperto):** `AtomicOutputFiles` pubblica output su file system locale soltanto dopo completamento; sovrascrittura consentita solo su scelta esplicita nel picker. Lo stream dei provider virtuali non garantisce atomicità.
- **ADR-054 (scopo):** l'export PNG dello Studio per opere animate è solo il fotogramma corrente. Export di sequenze (già CLI), GIF/APNG/video e progressi/cancellazione avanzata in milestone successive.

## Decisione M3D.1-R2 — Correzione bootstrap (16 novembre 2026)

- **ADR-051 (implementata, da validare):** host Avalonia 12 con classe `StudioApp : Avalonia.Application`, inizializzazione del FluentTheme e registrazione della finestra mediante `IClassicDesktopStyleApplicationLifetime`. `Program.Main` usa `AppBuilder.Configure<StudioApp>().UsePlatformDetect().StartWithClassicDesktopLifetime(args)`, evitando la collisione tra `Application` tipo e `GenerativeArt.Application` namespace e la chiamata inesistente `Application.Run`. Nessuna modifica ai renderer.
- **ADR-052 (vincolo utente chiarito):** XAML/AXAML è esplicitamente consentito. Non scegliere altri linguaggi applicativi senza chiedere; C#/.NET e minimo PowerShell restano lo stack previsto.

## Decisione M3D.2-R1 — Esportazione PNG e precisione parametri (16 novembre 2026)

- **ADR-055 (implementata; da validare Windows):** per i file locali, un renderer sincrono produce direttamente nello stream di un file temporaneo nella stessa cartella della destinazione; produzione su worker ThreadPool, flush prima di `File.Move`, token per cancellare e rimozione `.partial` in tutti i percorsi non riusciti. Il file precedente non viene cancellato preventivamente. Il fallback per URI virtuali non promette atomicità.
- **ADR-056 (implementata; da validare Windows):** l'export GUI ha un solo job in esecuzione e un pulsante Annulla. Non si inventano percentuali: la progress bar è indeterminata, i messaggi mostrano la fase. La preview usa lo stesso semaforo e non crea una coda di lavori mentre l'export è attivo.
- **ADR-057 (implementata; da validare Windows):** input numerici esatti affiancano gli slider. I numeri reali accettano il separatore `,` oppure `.`, sono normalizzati in formato invariante tramite descrittori e conservano la rappresentazione round-trip. Gli input invalidi non aggiornano la ricetta e si ripristinano su perdita del focus. JSON v1 e motore non cambiano.

- **ADR-041 (M4A.2-R1; implementata, gate Windows aperto):** SignedDistance2D nel modulo Algorithms, con convenzione negativo=interno e composizione CSG, senza Skia/GUI. La prima opera SDF usa un solo PixelBuffer temporaneo per canvas e il budget statico esistente (4096×4096); la migrazione a tile va valutata dopo benchmark poster.
- **ADR-042 (M4A.2-R1; implementata, gate Windows aperto):** campionamento Bridson seeded tramite SplitMix64Random, griglia a celle di lato r/√2, ricerca vicini entro 2 celle, limite 2.000.000 celle e 20.000 punti. CLI/Studio usano il runtime comune e ricette JSON v1.


## ADR M4A.4-R3 — Supersampling opzionale e limiti di memoria (16 novembre 2026)

- **ADR-R3-001 (implementata; gate Windows aperto):** aggiungere un parametro `Choice` facoltativo `render-quality` a tutti i nove generatori, con `standard` predefinito e `fine-2x` a 4 sottopixel/pixel. La chiave può mancare nei JSON v1 storici; l'algoritmo, il seed, la versione e le dimensioni di output restano invariati. Questo non autorizza cambiare implicitamente le ricette già create.
- **ADR-R3-002 (implementata; gate Windows aperto):** superficie Skia BGRA premoltiplicata alla risoluzione nativa, scala canvas 2× per primitive geometriche e riduzione lineare sullo spazio premoltiplicato prima dell'encoder PNG. Le opere per-pixel Plasma/SDF producono buffer fisici 2×, campionando il campo alle coordinate logiche subpixel; gli altri background a pixel logici sono scalati senza duplicare l'elaborazione del campo.
- **ADR-R3-003 (implementata; gate Windows aperto):** limite tassativo di 16.777.216 pixel temporanei per `fine-2x`, controllato nel renderer prima di allocazioni e scrittura. Per 4000×4000 rimane `standard`; non dichiarare un limite rigoroso di RAM complessiva senza misurare bitmap, snapshot e buffer native/managed. Animazioni a un fotogramma per chiamata; nessuna cache temporale implicita.
