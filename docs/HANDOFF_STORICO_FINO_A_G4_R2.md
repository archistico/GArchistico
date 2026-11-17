## 17/11/2026 — M4E.3-G4-R2: Ocellus interstizi e rilievo; Rete nodi e pellicole

**Baseline certificata dall'utente:** G4-R1.2 compila e tutti i test passano su Windows. Il blocco G4-R2 prosegue da quella versione, mantenendo integralmente Crinis R10 e gli hotfix R1.1/R1.2.

**R2 implementata nei sorgenti:** Ocellus aggiunge `pore-strength`/`pore-radius` e `rim-light`/`rim-width`; Rete aggiunge `junction-strength`/`junction-radius`, `node-glow` e `membrane-opacity`. I nuovi controlli sono opzionali e neutri per vecchie ricette JSON v1; i **preset iniziali Studio** dei due generatori sono invece espressamente aggiornati per mostrare gli effetti.

**Catalogo:** 29 generatori, 29 preset incorporati, 173 ricette JSON (6 nuove in `examples/recipes/m4e3-g4-r2/`); 17 preset personali e tutti gli altri generatori invariati. Nuovi test Algorithms/Application/Rendering, note e comparazioni nella [scheda G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md).

**Gate aperto:** la R2 richiede ancora build Release e test .NET su Windows (`clear && dotnet clean && .\tools\validate.ps1`) e verifica visiva A/B. Dopo il gate decidere se proseguire G4-R3 o passare a G5. Compositore B, workflow artista D, animazioni evolute più avanti.

---

# Handoff — M4E.3-G4-R1.2 hotfix CS0136 (17/11/2026)

- **Usare il nuovo ZIP integrale R1.2.** Include Ocellus/Rete G4-R1, il fix CS0117 R1.1 e il nuovo fix CS0136 nel renderer di Rete.
- **Log utente:** la build R1.1 ha compilato Algorithms.Tests e Application.Tests, poi si è fermata in `src/GenerativeArt.Rendering.Skia/GartCellularRenderer.cs` a riga 152 con CS0136: `double ink` confligge con `uint ink` della palette alla riga 169.
- **Fix R1.2:** `wallCoverage` rappresenta la copertura normalizzata del bordo; `wallArgb` contiene il colore opaco della palette. Sostituiti tutti gli usi con nomi disambiguati. Nessun effetto artistico intenzionale, nessuna modifica a formule, catalogo o JSON.
- **QA:** controllo statico degli identificatori e manifest; **build e suite intera da confermare su Windows**. Comando PowerShell 7: `clear && dotnet clean && .\tools\validate.ps1`.
- **Roadmap:** dopo gate Windows, smoke di Ocellus/Rete a risoluzione finale e poi G4-R2 (microfori Ocellus, nodi/pellicole Rete). Crinis R10 in pausa, compositore B e workflow D successivi, animazioni differite. Numeri: 29 generatori, 167 esempi JSON, 17 preset personali preservati.

---

# Handoff — M4E.3-G4-R1.1 hotfix CS0117 (17/11/2026)

- **Baseline:** G4-R1 aggiunge Ocellus/Rete, ma la build Windows fallisce con 4 × CS0117: `Math.Hypot` inesistente su net10.0. Non c'è ancora un risultato di test G4 completo.
- **Modifica R1.1:** introdotto `src/GenerativeArt.Algorithms/Geometry/GeometryDistance.cs`; sostituite le quattro chiamate in `OcellusField.cs`/`ReteFoamField.cs` e le altre due in `GartCellularRenderer.cs`. Il test `GartG4GeometryTests` conserva un oracolo matematico indipendente. Nuovo `GeometryDistanceTests` sulle distanze e sulle grandezze estreme.
- **Nessun altro cambiamento funzionale:** 29 generatori, 167 ricette, 17 preset personali invariati, roadmap G4-R2 invariata.
- **Gate:** avviare `clear && dotnet clean && .\tools\validate.ps1` da PowerShell 7 nella root. Test e resa visiva ancora da verificare su Windows. Tutte le fonti sono nel nuovo ZIP completo R1.1; non ripartire dal vecchio ZIP R1.

---

# Handoff — M4E.3-G4-R1 (17/11/2026)

- Nuova decisione utente: sospendere sviluppo Crinis (R10 conservata, non ancora verificata su Windows) e procedere ad altre opere.
- Implementati `ocellus-eyespots` e `rete-foam`, con 8 nuove ricette FullHD, 2 preset iniziali, kernel in Algorithms, renderer condiviso Skia, supporto CLI/Studio, palette Auto e test. **29 generatori / 167 esempi**, 17 preset personali invariati.
- Riferimenti originali: `gart-main/arts/bubbles/src/ocellus/Ocellus.kt`, `gart-main/arts/cell/src/rete/Rete.kt` (BSD-2-Clause, Igor Spasić). Implementazione C# autonoma; miglioramenti G4-R2 ancora necessari.
- Per validare: dalla root PowerShell 7 `clear && dotnet clean && .\tools\validate.ps1`; poi due esempi con CLI e confronto visivo. Non attribuire test Windows positivi a R10/G4 senza l’esito utente. Ultima conferma esplicita: R9.
- Prossimi: G4-R2, poi G5; A1/A4/A5 aperte, Fase B/D dopo A, animazioni avanzate rimandate.
- Leggere `docs/CURRENT_STATE.md`, `docs/M4E3_G4_OCELLUS_RETE.md`, `docs/ROADMAP.md` e `docs/CHANGELOG.md`.

---

# Handoff — GenerativeArt M4E.3-G3-R10 (17/11/2026)

**Partire dallo ZIP integrale R10**, non da patch isolate. **R9 compilata/testata con successo dall'utente**; richiesta nuova: profondità *dentro lo stesso fascio*, fibre più importanti (il feedback conserva la profondità tra fasci). Baseline del confronto: `/examples/recipes/m4e3-g3-r10/crinis-r9-originale-utente.json` copia immutata dell'ultimo file utente, seed `7775761298886397793`, 1920×1080.

**R10 sviluppata:** `CrinisMicroRenderer` disegna tre sottostrati per fascio con seed indipendenti dall'ordine, permette filamenti dominanti più spessi, illumina la sezione e rafforza il contatto interno. 4 nuovi parametri opzionali neutri in JSON, preset Studio nuovi lavori con `hair-width=3.2`; i parametri e i JSON R9 senza le nuove opzioni devono rimanere identici in output. 3 ricette R10, 159 totali, 27 generatori. Codice esclusivamente C#/.NET; Avalonia/Skia; minimo PowerShell.

**Gate prima di dichiarare R10 stabile:** `clear && dotnet clean && .\tools\validate.ps1` in PowerShell 7 su Windows, poi confronto visivo PNG R9 vs R10 con stesso seed e palette. I test nuovi sono stati predisposti ma **non eseguiti** qui. Non dichiarare successi finché non arrivano risultati.

**Continua:** R11 volume interno più sofisticato/luce lungo la curva; R12 micro-occlusione in incroci e sfocatura retro selettiva dopo valutazione budget. Poi G4 Ocellus/Rete e A1/A4/A5, quindi B compositore e D workflow dell'artista. Animazioni avanzate rinviate. **A ogni consegna aggiornare README, roadmap, changelog, CURRENT_STATE, HANDOFF e ZIP completo.**

**Documento tecnico:** `docs/M4E3_G3_R10_CRINIS_PROFONDITA_INTERNA.md`.

---

# Handoff — M4E.3-G3-R9 (17 novembre 2026)

**Richiesta utente:** continuare lo sviluppo di Crinis, privilegiando la tridimensionalità e mantenendo aggiornata tutta la documentazione di avanzamento. Non iniziare G4 prima della stabilizzazione artistica Crinis.

**Base e modifica:** il pacchetto deriva direttamente dalla R8. `CrinisMicroRenderer` applica `ShapeBundleLateral` (monotona per compressione tra -.35 e .7), `BundleSectionScale` (intervallo [.73,1.27]) precalcolata una sola volta per punto e fascio, diameter limiting locale e light-ramp trasversale. Nuovi parametri opzionali v1 catalogo: `bundle-compression`, `bundle-volume`, `bundle-lighting`, default zero per ricette storiche. Studio fresh default: 0.28/0.68/0.82 e `hair-width=1.65` (prima 0.67). Il catalogo ora ammette `hair-width` fino a 8 px per preservare R7/R8. `body-opacity=0` conserva la regola nessuna silhouette del fascio.

**QA:** test applicativi/catalogo e rendering estesi. Nuove 3 ricette studio R9; totale 156 esempi e 27 generatori. Controllare obbligatoriamente la validazione Windows, **non eseguita qui** per assenza SDK .NET. I test del JSON potrebbero evidenziare regressioni che il solo `jq` non rileva. Comando: `clear && dotnet clean && .\tools\validate.ps1`.

**Controllo artistico:** comparare R8 vs R9 su seed 42, preferibilmente a zoom 100%: leggibilità degli intrecci, distinzione dei fili, assenza di banda opaca, differenza cromatica dei piani. In caso di resa ancora piatta, la priorità successiva è un'occlusione tra fasci più globale ma con memoria limitata.

**Riferimento:** `docs/M4E3_G3_R9_CRINIS_VOLUME_FASCI.md` e `docs/CURRENT_STATE.md`.

---

# Handoff — M4E.3-G3-R8

- **Direttiva utente:** continuare su Crinis e tenere la documentazione sempre aggiornata.
- **Revisione corrente:** `CrinisMicroRenderer` ora usa una **ombra di contatto multi-pass** per ciascun run di fibra (`contactOuter`, `contactMid`, `darkPath`) prima del corpo del filamento. Questo aumenta l'occlusione percepita tra elementi davanti/dietro senza violare il vincolo `body-opacity=0` = niente silhouette piena del fascio.
- **Lettura dei piani:** incrementata leggermente la separazione dei livelli arretrati con blur/opacita' piu' incisivi.
- **QA/examples:** `GenerativeArt.Application.Tests` include ora anche `CrinisR7Recipes` e `CrinisR8Recipes`; nuova cartella `examples/recipes/m4e3-g3-r8/` con 2 ricette. Totale 153 JSON di esempio.
- **Comando Windows:** `clear && dotnet clean && .\tools\validate.ps1`. Se il feedback successivo chiedera' ancora piu' tridimensionalita', il prossimo step e' introdurre massa di fascio/compressione locale e lighting di gruppo oltre al lighting del singolo filo.

---

# Handoff — M4E.3-G3-R7

- **Baseline utente confermata:** G3-R6 compilazione e test OK su Windows. Nuovo feedback artistico: le fibre hanno piu' profondita' ma restano troppo sottili/piatte.
- **Revisione corrente:** in `CrinisMicroRenderer` il raggio del filo cresce con un modello di volume (`strandVolume` + `bulge`) e con una soglia minima meno capillare; dopo il riempimento del hull vengono tracciati un corpo centrale, un bordo in ombra e uno in luce per simulare una sezione piu' cilindrica.
- **QA aggiunto:** `CrinisMicroRenderingTests` verifica che `hair-width` modifichi davvero il render, anche in una ricetta molto densa ispirata al caso reale dell'utente.
- **Ricette di lavoro:** nuova cartella `examples/recipes/m4e3-g3-r7/` con `crinis-user-dense-indigo-original.json` e `crinis-indigo-bold-volume-1920.json`. Totale 151 JSON.
- **Comando Windows:** `clear && dotnet clean && .\tools\validate.ps1` da PowerShell 7 nella root del progetto. Se il salto visivo e' ancora insufficiente, il passo successivo e' introdurre occlusione locale tra fasci e shadowing di contatto per separare nettamente davanti/dietro.

---

# Handoff — M4E.3-G3-R5

- **Riscontro R4:** segnalato fallimento `CatalogTests.AllArtworksExposeOptionalFinishingControlsWithNeutralDefaults`, causato dal gruppo `Finitura` improprio del solo parametro Crinis `ink-flecks`. Correzione in `GartBrushArtworkDescriptors.cs`: gruppo `Microfibre`.
- **Intervento artistico R5:** `CrinisMicroRenderer` limita in modo morbido il diametro in base al passo effettivo dei fili (`fiber-coverage`); modulazione delle tonalità e dei depositi sul singolo filo (`pigment-contrast`). Modalità classica invariata.
- **Ricetta reale utente:** seed=42, indigo, 1920x1080, `hair-width=2.629`, `bristle-pitch=1.36`, `fiber-density=1.836`, `body-opacity=0.081`. Conservata esatta in `examples/recipes/m4e3-g3-r5/crinis-user-reference-original.json` e variante A/B con base a zero.
- **QA:** test C# aggiornati; compilazione e test Windows R5 da confermare. `clear && dotnet clean && .\tools\validate.ps1` dalla root PowerShell 7.
- **Stato:** 27 generatori, 149 ricette esempio, 17 preset personali intatti. G4 Ocellus/Rete dopo la stabilizzazione visiva Crinis; A1/A4/A5 e fasi B/D successive, animazioni rinviate.

---

# Handoff — M4E.3-G3-R4

- **Riscontro Windows:** R3 build riuscita, 1056/1088 test passati, 32 falliti per parametri di Crinis non presenti nell'assembly Application in esecuzione (fra cui `detail-style`). L'archivio R3 contiene il descrittore corretto: possibile build Release incrementale rimasta su R2.
- **Revisione corrente R4:** nello script di validazione e' obbligatoria la pulizia Release prima di build e test, e un test di contratto del catalogo Crinis eseguito per primo. `GartBrushRenderer` usa `classic` quando un vecchio oggetto ricetta non contiene la nuova scelta.
- **Istruzione fondamentale:** estrarre R4 in **cartella vuota**; in PowerShell 7 da root: `clear && dotnet clean && .\tools\validate.ps1`. Lo script pulisce esplicitamente anche Release; se fallisce il gate iniziale, controllare che l'archivio sia stato estratto senza sovrapporre file vecchi.
- **Stato dei test:** compilazione R3 confermata, suite R3 NON superata; R4 da verificare su Windows. 27 generatori, 27 preset, 146 JSON. Le 6 ricette Cincinnus e i 17 preset personali sono invariati.
- **Dopo R4:** confronto visuale Cincinnus, poi G4 Ocellus/Rete, A4 8K e A1 refactoring; compositore B e workflow artista D in fasi successive; animazioni dopo.

---

# Handoff — M4E.3-G3-R3

- **Conferma Windows più recente:** M4E.3-G3-R2, build e test superati (feedback dell'utente).
- **Revisione corrente:** G3-R3 Crinis microfibre, da verificare. Target visivo: due PNG Cincinnus 2000×2000 forniti dall'utente.
- **Comportamento essenziale:** `body-opacity=0` deve escludere TUTTE le sagome e ombre di fascia; lasciare le ombre locali delle singole fibre. Test `ZeroBodyOpacityEliminatesWholeBandShadow` in due modalità.
- **Nuovo percorso:** `CrinisMicroRenderer.cs`, registrato da `GartBrushRenderer` con `detail-style=micro`. Nuovi parametri sono `optionalInJson` per le ricette storiche.
- **Preset:** Studio Crinis micro di default; 17 preset personali non toccati. Sei nuove ricette e correzione della proprietà `description` in quattro esempi R2.
- **QA:** attenzione a tempi del render 2000×2000 in alta densità e confronto artistico; test di catalogo, budget, determinismo, preview/export, cancellazione e JSON. Documento `M4E3_G3_R3_CRINIS_MICROFIBRE.md`.
- **Prossime milestone:** G4 Ocellus/Rete dopo l'accettazione visiva Crinis; A4/8K, B compositore, D workflow. Animazioni rinviate.

---

### Stato rapido
- Ultima baseline validata dall'utente: M4E.3-G3-R1 (compila e test passati).
- Revisione corrente: M4E.3-G3-R2, mirata al miglioramento visivo di Crinis.
- Punto chiave: corpo/base del fascio opzionale, maggior rilievo delle fibre, resa più tessile e quattro nuove ricette di studio.

# M4E.3-G3-R1 — Fascia/Eclectic + correzione catalogo (17/11/2026)

- Ricevuto esito G2-R2: **1040/1041 test**; unica regressione `CatalogTests.CatalogHasStableSortedIdsAndLoopCapability` dovuta a un ordine alfabetico atteso invertito (`crinis` / `cotton`). Corretta **solo la lista di attesa**, in entrambe le coppie invertite (`cotton`/`crinis`, `spaced`/`spiral`); non l'ordinamento del prodotto.
- A3-G3: due generatori `fascia-ribbons` e `eclectic-trails` (campi vettoriali, nastri retinati, crescita con collisioni), otto nuove ricette, due preset embedded. Totale **27 algoritmi / 136 ricette**. Tutti i preset e le ricette precedenti preservati.
- Test nuovi per geometria, serializzazione, cambi di parametri, seed, preview/export, fine-2x, finiture, limiti e cancellazione. Guida `M4E3_G3_FASCIA_ECLECTIC.md`.
- **G3-R1 da validare su Windows**. Fase successiva G4 Ocellus/Rete; A1 e A4/A5 restano aperte; B/D dopo e animazioni rinviate.

---

# Handoff — M4E.3-G2-R2 (17/11/2026)

**Ripartire dalla G2-R2.** Il log Windows della G2-R1 indica `xUnit2020` in `tests/GenerativeArt.Application.Tests/GartBrushCatalogTests.cs:42` (`Assert.True(false, ...)`); sostituito con `Assert.Fail(...)`. La R2 conserva integralmente i componenti artistici di G2: 25 generatori, 128 ricette e 25 preset incorporati (17 personali originari invariati). La suite R2 non è ancora confermata sul PC utente; la M4E.2-R5 è l’ultima validata. Per il gate eseguire da PowerShell 7 nella cartella `GenerativeArt`: `clear && dotnet clean && .\tools\validate.ps1`. Se passa, proseguire con A3-G3 Fascia/Eclectic, mantenendo A1/A4/A5 e le fasi B/D pianificate; animazioni evolute rinviate.

---

# Handoff — M4E.3-G2-R1 (17/11/2026)

Ripartire da questo ZIP completo, non dalla versione G1. 25 generatori, 128 ricette, 25 preset (17 personali originali invariati). Nuovi Crinis/Flamebrush (kernel Algorithms, renderer Skia, catalogo, test); riferimenti `docs/M4E3_G2_PENNELLI_CRINIS_FLAMEBRUSH.md`. Non dichiarare gate Windows superati: la conferma utente più recente riguarda M4E.2-R5, G1/G2 ancora senza esito Windows. Comando: `clear && dotnet clean && .\tools\validate.ps1`. Prossima G3 Fascia/nastri, A1, A4/8K e A5, quindi B e D; animazioni rinviate.

---

# HANDOFF — M4E.3-G1-R1 — 17 novembre 2026

La baseline verificata su Windows è **M4E.2-R5** (compilazione e test passati, feedback utente). Da questa baseline documentata è stato implementato **A3-G1**: tre nuovi generatori C# `alien-letters`, `cotton-circles`, `rigor-grid`, con 9 ricette Full HD, 3 default embedded e nuove suite di test. **23 generatori e 120 ricette**. Questa revisione non è ancora validata su Windows. Usa `docs/M4E3_G1_GEOMETRIE_MODULARI.md` per dettagli e comandi, `docs/ROADMAP.md` per le priorità effettive.

Vincoli di continuità: codice C#/.NET 10 (Avalonia e SkiaSharp); mantenere tutti i 17 preset personali forniti, l'anteprima calcolata alla risoluzione finale, il menu sinistro uniforme e i 111 JSON precedenti. ZIP completo a ogni rilascio. Per test Windows usare sempre `clear && dotnet clean && .\tools\validate.ps1`; non attribuire test non eseguiti. L'8K G1 non è attivato: verificare budget e performance prima di proporlo. Creditare Gȧrt BSD-2-Clause nei contributi derivati.

Prossimi passi: smoke dei tre PNG, gate di test Windows, poi **A3-G2: Crinis/Flamebrush**, quindi G3 Fascia. In parallelo A1 e A4/8K, poi B compositore e D workflow dell'artista. Animazioni avanzate rinviate.

---

# Handoff studio Gȧrt — 17 novembre 2026, documentazione su baseline M4E.2-R5

L’utente ha **confermato la compilazione e il superamento dei test M4E.2-R5** e apprezza i due ultimi algoritmi. È richiesto di STUDIARE il codice e le opere Gȧrt per decidere un porting artistico in C#/.NET. Analisi consegnata in **`docs/STUDIO_GART_PORTING_M4E2.md`**: 297 sorgenti `.kt` sotto `arts`, 384 nel core Gȧrt, 25+ opere preferite esaminate, dipendenze e nuova sottoroadmap A3-G1…G6. **Nessun codice C# modificato** in questa consegna. Per il prossimo sviluppo partire da G1 Tile Grammar (Alien Letters V3, Cotton Circles e Rigor), poi G2 Crinis/Flamebrush e G3 Fascia/Eclectic. Validare sempre la precisione artistica con PNG di riferimento, attualmente non presenti nello ZIP Gȧrt. Conservare 20 generatori, 111 ricette, 17 preset personali, preview nativa e export equivalente. Non anticipare compositore B, workflow D o animazioni.

---

# Handoff rapido — M4E.2-R5 (17 novembre 2026)

**Usare lo ZIP integrale M4E.2-R5.** Risolve la build fallita della R4 per `CS0618` in `ArtisticScanlineFinisher.cs`: sostituito `canvas.DrawImage(source, 0, 0)` con l'overload SkiaSharp non obsoleto (`SKRect` + `SKSamplingOptions.Nearest`). Nuovo test di copia 1:1 del raster. Il resto della R4 (20 algoritmi, 111 ricette, finiture 8K a righe, menu uniforme, preset originali) è conservato. Dettagli in `docs/M4E2_R5_FIX_DRAWIMAGE_SKIA.md`.

**Baseline confermata**: M4E.2-R1 (build e test OK). **R4**: errore di compilazione CS0618 segnalato dall'utente. **R5**: non ancora validata. Validare in PowerShell 7: `clear && dotnet clean && .\tools\validate.ps1`. Dopo la validazione, smoke 8K con finiture; poi A4/A5, B compositore e D workflow. Animazioni in seguito.

---

# Handoff rapido — M4E.2-R4 (17 novembre 2026)

Usare **lo ZIP completo R4**. Introduce `ArtisticScanlineFinisher` e condivide i kernel Material/ArtisticPostProcessor sulle righe: 8K UHD standard con finiture per `guilloche`, `voronoi-mosaic`, `spaced-streamlines`, `poisson-stippling`. Output fino a 16.777.216 pixel resta sul percorso storico. +3 ricette 8K (`examples/recipes/m4e2-r4/`); 20 generatori, 111 ricette, 20 preset invariati, inclusi i 17 personali. Guida tecnica `docs/M4E2_R4_FINITURE_8K_RIGHE.md`.

**Ultima versione testata dall'utente: M4E.2-R1** (compilazione e test OK); R2/R3/R4 **non validate**. Da PowerShell 7: `clear && dotnet clean && .\tools\validate.ps1`. In seguito eseguire smoke 8K PNG e misura RAM; gli unit test non allocano 8K. **Prossimi:** A4 renderer raster-heavy e memoria, A1 controller, A0 asset e A5 benchmark; B compositore, D workflow, animazioni rinviate.

---

# Handoff rapido — M4E.2-R2 (17 novembre 2026)


# Handoff rapido — GenerativeArt M4E.2-R3 (17 novembre 2026)

Ripartire dallo **ZIP completo M4E.2-R3** (basato sulla R2): **20 generatori**, **108 ricette**, 20 preset incorporati (17 personali invariati + wave-interference + phyllotaxis + reaction-diffusion). Nuove famiglie A3: `PhyllotaxisPattern`/`PhyllotaxisRenderer` e `ReactionDiffusionField2D`/`ReactionDiffusionRenderer`. Quest'ultima usa budget 350k celle / 75M iterazioni complessive. Otto nuovi studi Full HD in `examples/recipes/m4e2-r3/`. **Menu sinistro**: tutte le schede catalogo e aperture recenti larghe come lo spazio disponibile, con pulsante per dimenticare la cronologia in overlay.

**Validazione:** baseline Windows confermata M4E.1-R3. R4/R5/M4E.2-R1/R2/R3 ancora senza riscontro Windows. Proseguire senza aspettare feedback, ma non dichiarare i nuovi test superati. PowerShell 7 dalla radice: `clear && dotnet clean && .\tools\validate.ps1`. Per smoke GUI controllare filtri, ricette recenti, resize menu, seed, anteprima 1:1 e export.

**Prossimo:** A4 memoria e 8K renderer raster-heavy, A1 controller Studio, A0 asset storici, A5 benchmark. Poi Fase B compositing, D workflow artista; animazioni avanzate rinviate. Guida [`M4E2_R3_NATURA_E_NAVIGAZIONE.md`](M4E2_R3_NATURA_E_NAVIGAZIONE.md).

---


**Ripartire dal nuovo ZIP completo M4E.2-R2.** Fase A4 avviata con `RasterCanvasBudgetPolicy`: canvas 8K UHD 7680×4320 (`standard`, senza finiture) per `guilloche`, `voronoi-mosaic`, `spaced-streamlines`, `poisson-stippling` soltanto. Altri generatori, fine2x e materia mantengono budget precedenti. Quattro ricette in `examples/recipes/m4e2-r2/` (100 JSON totali); test di budget e roundtrip. Nessun cambiamento ai 17 preset personali né alle 96 ricette precedenti.

**Validazione:** PowerShell 7: `clear && dotnet clean && .\tools\validate.ps1`; smoke facoltativo con `render-recipe` per il nuovo Voronoi 8K, dettagli in `M4E2_R2_8K_GEOMETRICI.md`. Baseline Windows confermata dall'utente M4E.1-R3; R4/R5/M4E.2-R1/R2 restano da validare.

**Prossimo:** A4 ottimizzazione renderer raster-heavy, finiture a bande, benchmark 8K e memoria; A1 controller Studio, A0 anteprime mancanti, A5 QA. Poi B compositore e D workflow artista; animazioni rinviate.

---

# Handoff rapido — M4E.2-R1 (17 novembre 2026)

**Base attuale:** ZIP integrale M4E.2-R1, discendente da M4E.1-R5. **Baseline Windows validata:** M4E.1-R3 (build, test, preview/export confermati); R4, R5 e M4E.2-R1 non ancora confermate.

**Nuovo A3:** `wave-interference` ID v1, kernel `WaveInterferenceField2D` (Algorithms), `WaveInterferenceRenderer` (Skia), descrittore parziale, preset embedded e cinque esempi Full HD in `examples/recipes/m4e2-r1/`; 18 generatori, 96 ricette. Campo deterministico per sorgenti radiali con tre stili/layout, campionamento 2x, finiture e colori Auto. Budget esplicito 150M contributi. **Preservati tutti i 17 preset personali e 91 JSON precedenti; non cambiare i relativi hash PNG.**

**Verifica Windows:** da PowerShell 7 nella radice solution `clear && dotnet clean && .\tools\validate.ps1`, poi aprire lo Studio con `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`, selezionare **Interferenze d'onda**, controllare campionamento native preview/export, provare cinque JSON e palette/parametri. Non dichiarare passata la suite nuova finché non è eseguita. I problemi eventualmente segnalati vanno corretti partendo da questa ZIP.

**Roadmap:** completare controller/lifecycle di A1, A3 altri algoritmi e verifiche artistiche, A4 8K con budget per renderer, A5 stabilizzazione, A0 recupero anteprime. Poi B compositore e D workflow artista; animazioni rimandate. Guida `M4E2_R1_INTERFERENZE_ONDA.md`.

---

# Handoff rapido — M4E.1-R5 (17 novembre 2026)

**Partire dallo ZIP integrale M4E.1-R5**. La baseline validata dall'utente è M4E.1-R3 (compilazione, suite, preview/export identici); R4 e R5 sono ancora da testare.

**R5:** nuovo servizio Application `StudioRecipeDocuments` (import JSON bounded, validazione, serializzazione e salvataggio atomico locale); `StudioWindow.FileOperations.cs` alleggerito. 7 test servizio + 1 test fixture. Dieci ricette sperimentali Full HD nella cartella `examples/recipes/m4e1-r5/`: 91 JSON in totale, 17 generatori/preset unchanged. Nessun renderer o default utente modificato.

**Verificare su Windows:** `clear && dotnet clean && .\tools\validate.ps1`; apertura/salvataggio ricette, 10 nuovi JSON, anteprima nativa/export, parametri condizionali R4. **Prossimo:** A1 lifecycle/controller GUI; A3 nuovi algoritmi, A4 8K (studio budget e QA), A0 recupero anteprime. B compositore e D workflow successivamente; animazioni rinviate. Doc `M4E1_R5_DOCUMENTI_RICETTE_STUDI.md`.

---

# Handoff rapido — M4E.1-R4 (17 novembre 2026)

**Base nuova:** progetto completo M4E.1-R4. **Baseline Windows confermata:** M4E.1-R3, compilazione e test riusciti, anteprime OK ed export identico alla preview (conferma utente). Non attribuire a R4 tale conferma.

**R4 implementata:** regole di applicabilità dei parametri in `Application/Rendering/StudioParameterApplicability.cs` (test indipendenti), estrazione editor in `StudioWindow.ParameterControls.cs`, attivazione/disattivazione informativa in `StudioWindow.ParameterApplicability.cs`. Non rimuove o azzera i valori validi nelle ricette o nei preset. Caso da verificare: Streamlines → vortex (`frequency` inattivo con motivazione), poi curl (`frequency` attivo).

**R4 da validare:** `clear && dotnet clean && .\tools\validate.ps1` su Windows PowerShell 7; smoke editor/JSON, preview/export. In caso di errori, correggere puntualmente partendo dalla R4.

**Roadmap:** A1 controller/lifecycle ancora aperti; A2 anteprima nativa validata nei casi provati; A0 immagini mancanti; A3 ricette/algoritmi; A4 8K; A5 QA. Quindi B compositore universale, D workflow artista. Animazioni avanzate rinviate. Leggere `M4E1_R4_PARAMETRI_CONDIZIONALI.md`.

---

# Handoff rapido — M4E.1-R3 (17 novembre 2026)

**Base di ripresa:** ZIP completo **M4E.1-R3**, non R2. Leggere `CURRENT_STATE.md`, `ROADMAP.md`, `M4E1_R3_FIX_TEST_STREAMLINES.md` e `M4E1_R2_PREVIEW_NATIVA_COORDINATORE.md`.

- **Esito Windows R2 fornito dall'utente:** build riuscita, **879/880 test**, solo `spaced-streamlines/frequency` fallito in `ArtworkEvolutionTests.cs:145`.
- **Root cause:** i 17 preset Studio contengono `spaced-streamlines` con `field=vortex`, campo matematico indipendente da `frequency`; il test `EveryArtisticControlChangesTheRaster` ereditava quel preset. Con `field=curl` la frequenza controlla realmente il campo. Corretto il test per questa combinazione, aggiunta regressione che fissa la semantica di `vortex`.
- **Impatto:** nessun cambiamento di codice runtime, GUI, seed, preset, ricette o PNG. **R3 test Windows da eseguire**; il risultato 879/880 si riferisce esclusivamente alla R2. Nessuna dichiarazione di successo anticipata.
- **Prossimo:** riprendere Fase A1 controller/headless e UX condizionale dei parametri inapplicabili, A2 QA native preview, A3 nuovi algoritmi/ricette e A4 8K, quindi B compositore e D workflow. Animazioni avanzate rinviate.
- **PowerShell 7 dalla radice solution:** `clear && dotnet clean && .\tools\validate.ps1`.

---

# Handoff precedente — M4E.1-R2 (17 novembre 2026)

**Aprire prima** `CURRENT_STATE.md`, `ROADMAP.md`, `M4E1_R2_PREVIEW_NATIVA_COORDINATORE.md` e `M4E0_PIANO_A_B_DEFAULTS.md`.

- Base: **M4E.1-R1** (refactoring iniziale `StudioWindow`, 17 JSON embedded dei generatori, Fase D in roadmap). **R2** introduce `StudioRenderCoordinator` nel progetto Application, condiviso tra preview ed export, con gate unico e test headless.
- **Anteprima R2**: sempre alle dimensioni finali. Rimossi i campi «Raster max» e il pulsante «Raster = output»; `Adatta`, `100%`, pan e zoom agiscono sulla vista. Se budget insufficiente, errore esplicito; nessun fallback raster. Renderer 8K **non** ancora implementato.
- **Prossimo sviluppo**: A1 completare controller indipendenti e smoke lifecycle/headless; A2 verificare GUI, DPI, pixel/alpha reali e cancellazione; **A0** resta aperta per 52 immagini storiche assenti; poi **A3** nuovi algoritmi e ricette, **A4** 8K e qualità, **A5** QA.
- **Fasi future**: B compositore universale, D varianti/seed, A/B, galleria, contact sheet, preset, SVG selettivo; animazioni evolute rinviate.
- **Verifiche allora disponibili:** M4D.4-R1: 859/859; successivamente l'utente ha eseguito R2 su Windows, ottenendo 879/880 (dettagli nella sezione R3 sopra).
- **Vincoli**: C#/.NET 10, Avalonia/Skia, minimo PowerShell, niente Python/Go/Kotlin/Java nel progetto; ZIP completo, preset e ricette originali invariati, README conciso, JSON v1 riproducibile.
- **PowerShell 7, dalla radice**: `clear && dotnet clean && .\tools\validate.ps1`; avvio con `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.

---

# Handoff rapido — M4E.1-R1 (17 novembre 2026)

**Aprire prima** `CURRENT_STATE.md`, `ROADMAP.md`, `M4E0_PIANO_A_B_DEFAULTS.md` e `M4E1_A1_WORKFLOW_D.md`.

- Base della consegna: M4D.4-R2 con 17 JSON embedded; nuovo refactoring **A1 primo passo**: `StudioWindow.cs` 189 righe, 7 partial per ambito, `StudioRecipeFormParser` in Application e test xUnit. Nessuna variazione artistica/UI deliberata.
- Baseline Windows storica: **M4D.4-R1 859/859**; R2 e M4E.1-R1 non ancora validate. Non dichiarare i nuovi test superati senza esecuzione.
- Prossimo sviluppo **A1-R2**: isolare controller preview/export con contratto di threading, cancellazione e test headless; poi **A2** preview realmente 1:1 output, **A3** nuove ricette/algoritmi, **A4** 8K, **A5** QA. **A0** 52 immagini storiche mancanti rimane aperto.
- Dopo A: **Fase B** compositore, poi **Fase D** workflow dell'artista (variazioni, seed explorer, A/B, galleria, contact sheet, preset, SVG selettivo). Animazioni in seguito.
- Vincoli: C#/.NET 10 + Avalonia/Skia, solo minimo PowerShell, ZIP completo ogni consegna, no Python/Go/Kotlin/Java nel progetto, JSON v1/seed/GUI invariati, README pulito, test Windows in italiano.
- Comandi: `clear && dotnet clean && .\tools\validate.ps1`; `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.

---

# Handoff — GenerativeArt Studio M4D.4-R2 (17/11/2026)

**Partire dal progetto completo M4D.4-R2, non dalle precedenti versioni R1.** È stata accettata la roadmap in due blocchi: **Fase A** (distribuzione, modularizzazione `StudioWindow`, anteprima nativa identica all'output, nuovi algoritmi/ricette, output 8K/qualità e benchmark) **prima** della **Fase B M4C.3** (compositore universale con livelli, maschere e blend). **Animazioni rinviate**.

**Fatto in R2:** incorporati come risorse dell'assembly i 17 file JSON forniti dall'utente (`src/GenerativeArt.Application/Defaults/`); l'inizializzazione dei nuovi lavori Studio usa seed, dimensioni, eventuale animazione e tutti i parametri/colori degli allegati, non solo i precedenti 7 override. Le ricette importate conservano i loro valori e il catalogo non cambia. Test aggiornati per i nuovi preset. Verifica Windows **ancora da eseguire**; baseline M4D.4-R1 con 859/859 test. Nessuna modifica ai renderer: il prodotto non è ancora 8K e l'anteprima non è ancora obbligatoriamente nativa.

**Problema preesistente in A0:** le 52 immagini di anteprima referenziate nel vecchio manifesto erano assenti nell'archivio ricevuto. R2 include tutti i sorgenti ricevuti e un manifesto nuovo coerente, ma non ricrea queste immagini. Recuperarle o rigenerarle con il motore a milestone A0.

**Prossima attività di codice:** A1 modularizzazione minima senza modificare UI, quindi A2 preview fedele; la roadmap approvata è in `docs/M4E0_PIANO_A_B_DEFAULTS.md` e `docs/ROADMAP.md`. A ogni consegna riportare stato e prossimo passo, ZIP completo e comandi Windows in italiano. Solo C#/.NET 10 + Avalonia/SkiaSharp + PowerShell minimo; nessun Python/Go/Kotlin/Java nel progetto.

**Validazione Windows (PowerShell 7, radice solution):** `clear && dotnet clean && .\tools\validate.ps1`.
**Avvio:** `dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release`.

---

# Handoff — GenerativeArt Studio M4D.4-R1 (16/11/2026)

## Base di partenza, stato e preferenze

**ZIP corrente:** `GenerativeArt_M4D4_R1_SDF_MACCHIE_FUSIONI_COMPLETO.zip`, integrale. **M4D.4-R1 — Orbite SDF riprogettato:** macchie organiche, fusione di quattro pigmenti, venature, rilievo, piani e antialiasing locale. **17 generatori, 81 ricette, build Release senza avvisi/errori e 859/859 test Windows** (Codex, SDK 10.0.201). Otto nuovi PNG Full HD, due fine-2x; controlli Studio, JSON, export CLI e 15 PNG precedenti identici verificati. Confronto AA contro riferimento 4×: errore alpha ridotto del 71,3% nel caso registrato. Guida [`M4D4_SDF_MACCHIE_FUSIONI.md`](M4D4_SDF_MACCHIE_FUSIONI.md), evidenze `docs/validation/M4D4_R1_*`. Desktop/DPI reali, altri OS, nuovi poster 4K ed editor livelli/maschere/blend restano aperti. Restore da cache NuGet locale, audit online e telemetria build disattivati. L'utente vuole macchie/fusione colore, complessità, controllo e migliore antialiasing.

## Novità M4D.4-R1 — Orbite SDF riprogettato

- `PigmentSdfField`: unione morbida di ellissi seeded, domain warp e quattro pesi cromatici normalizzati. `SdfPixelCoverage`: copertura locale di bordo/tratto, conservazione dell'energia dei tratti subpixel.
- `SdfPigmentRenderer`: stili pigment/glass, tre layout, quattro pigmenti con alpha, miscela RGB lineare o logaritmica approssimata, marbling, rilievo/riflessi, ombre gaussiane e blur per piano.
- `SdfPigmentControls.All` centralizza 31 nuovi controlli opzionali nei JSON v1. I default del catalogo restano classic; nuovi lavori Studio pigment/sorbet, 14 macchie per piano e due piani. Ripristino JSON conservativo. Ruoli colore vecchi riutilizzati, più pigmento 4/ombra; sei palette.
- Antialiasing anche standard, footprint da differenze centrali su tre righe, nessun campo float a pieno canvas. Fine-2x ammesso sui nuovi default Full HD. Budget SDF 260 milioni di valutazioni (incluse righe/colonne esterne), pixel nativi ≤ 8.388.608 più DepthLayerPolicy, prima di superfici/output.
- Otto ricette Full HD, due fine-2x, 81 ricette totali; screenshot Studio e confronti artistico/AA. Build Release e 859/859 test. Otto PNG, Studio (36 slider SDF), CLI fine-2x e 15 hash PNG precedenti verificati.
- Guida `M4D4_SDF_MACCHIE_FUSIONI.md`, evidenze `docs/validation/M4D4_R1_*`. Modi tool `sdf`, `sdf-studio`, `sdf-compare`, `sdf-aa`; `depth-compat` conserva la verifica degli hash precedenti. M4C.3 editor/maschere/blend e noise 4D restano futuri.

## Novità M4D.3-R1 — Profondità

- `OrganicAperture` e `DepthParticleField`; artwork `paper-cut` e `bokeh-depth`. Catalogo a 17, dieci nuove ricette Full HD, 73 totali, due fine-2x e confronto effetti spenti/accesi.
- `SkiaRasterSurface.DrawLayer` e `LayerEffects`: blur gaussiano, drop shadow e opacità SrcOver di gruppo. Un piano filtrato alla volta; nessun annidamento filtrato. Default neutro uguale al disegno diretto. Alpha premoltiplicato e ripristino canvas anche dopo errore/cancellazione.
- Paesaggi a strati: sei nuovi controlli JSON opzionali e neutrali. `soft-shadow > 0` sostituisce le precedenti ombre a tre tratti; atmosfera storica invariata. Nuovi lavori Studio con effetti attivi, import JSON conservativo.
- `DepthLayerPolicy`: massimo 8.388.608 pixel nativi e 800 milioni di unità di lavoro filtrato prima di allocazione/output. Budget aggiuntivi per vertici/copertura dischi. Non rimuoverli per ammettere tutti i massimi degli slider insieme.
- Build Release e 801/801 test, dieci PNG Full HD, tutte le 73 ricette validate, smoke Studio con default/ripristino, quindici PNG precedenti identici. Guida `M4D3_PROFONDITA.md`, log/dati `docs/validation/M4D3_R1_*`. `depth-compat` del tool ripete gli hash dei default storici a seed 42 / 160×120.
- Base di compositing già riutilizzata da tre generatori; M4C.3 resta aperta per editor livelli, maschere arbitrarie e blend configurabili. Nessuna nuova dipendenza di produzione.

## Novità M4D.2-R1 — Pittura e generatori iniziali

- `OrganicStroke` e `FractalProfile1D`; artwork `flow-brush` e `strata-landscape`. Catalogo a 15, quattordici nuove ricette Full HD, 63 totali, anteprime e confronto a pari seed dei primi cinque generatori.
- Estensioni opzionali di Curve parametriche, Noise Flow, Plasma, Streamlines distanziate e Flow Loop. `StudioRecipeDefaults.ParametersFor` applicato solo ai nuovi lavori, mai alle ricette ripristinate. CLI/API conserva i default del catalogo; 13 PNG precedenti identici tramite hash.
- `LegacyArtworkControls` centralizza i default neutrali; renderer storici conservati accanto alle varianti estese. Cambiare solo opacità/spessore nei flussi storici non deve spostare le tracce. FlowTracer cancellabile internamente.
- Build Release e 733/733 test Windows, quattordici PNG Full HD, smoke Studio e sequenza CLI 12 frame verificati. Guida `M4D2_PITTURA_EVOLUZIONE.md`, evidenze `docs/validation/M4D2_R1_*`. Le comparazioni degli hash riguardano seed 42 / 160×120, non tutti i possibili input.
- Nessuna nuova dipendenza di produzione. Compositore generale M4C.3 e noise 4D rimangono futuri; le sovrapposizioni dei paesaggi appartengono al loro renderer.

## Novità M4D.1-R1 — Dettaglio e controllo

- Algoritmi `GuillocheCurve` e `TonalHatching`, artwork `guilloche`/`tonal-engraving`, catalogo a 13; otto ricette Full HD e anteprime, 49 ricette totali.
- Guilloché: rapporto degli ingranaggi, armoniche, curve/precisione, twist e progressione, colori/opacità. Incisione: campi, tratteggio progressivo, angoli, toni e maschere locali. Dimensioni dei tratti riferite a lato minimo 1000 px.
- Dieci controlli Domain Warp opzionali, default neutrali; tre PNG Full HD M4B.1 identici via SHA-256. Mantenere questa compatibilità.
- Slider con passi adatti ai piccoli intervalli, input esatto preservato. Ogni editor ha una revisione: eventi ritardati degli editor rimossi devono essere ignorati.
- Build Release e 626/626 test Windows; otto PNG Full HD e smoke Studio headless verificati. Guida `M4D1_DETTAGLIO_CONTROLLO.md`, log/dati `docs/validation/M4D1_R1_*`.
- Compositore generale M4C.3 e nuovi loop M4B.2 ancora futuri. Nessuna nuova dipendenza di produzione.

## Novità M4B.1-R1 richieste dall'utente

- `DomainWarpNoise2D` e `LineIntegralConvolution` in Algorithms; nuovi artwork `domain-warp` e `lic-flow`, tre palette, quattro colori indipendenti, finiture e fine-2x. Catalogo a 11.
- Sette JSON Full HD e anteprime incluse. Domain Warp: circa 1–1,4 s; LIC: 9,5–12,4 s; streamlines dense: 5,5–15,9 s nel processo locale misurato, senza garanzia su altri PC.
- Streamlines: massimo 10.000, distanza/spessore opzionali; candidati fino a 200.000 distribuiti sul canvas nelle impostazioni estese, cancellazione dentro il tracer. Default storici preservati.
- Picker: footer fisso fuori dallo scroll, dimensioni contenute e altezza limitata all'area utile in DIP. Conferma/annulla verificati headless a quattro altezze; desktop/DPI reali ancora da provare.
- Tool opzionale `tools/GenerativeArt.Smoke`, fuori dalla solution, per ripetere smoke picker/render; Avalonia.Headless dipende solo dal tool. Nessuna nuova dipendenza di produzione.

## Novità M4C.2-R3 (correzioni richieste dall'utente)

- Il test `MissingParametersAreRejected` ora usa `JsonNode` per rimuovere `scale`: il vecchio `Replace` generava JSON invalido con trailing comma e provocava `JsonReaderException` anziché `ArgumentException`. Parser di produzione immutato.
- Limite finitura da 4.194.304 a 16.777.216 pixel. Rimosso anche tetto indipendente di Plasma 4.194.304; 2500×2000 in standard è accettato con materia/grana/dithering. Budget generale 4096 per lato e `fine-2x` con 16.777.216 sottopixel rimangono; altri renderer e loop conservano guard specifici.
- `StudioWindow`: campi di risoluzione raster anteprima (max) e dimensione massima riquadro anteprima (DIP) separati da dimensioni output; pulsante `Raster = output`.
- `PreviewViewport` / `StudioViewportState`: auto-fit dopo un render e dopo resize, pulsanti `Adatta` e `100%`; pan/zoom manuali preservati, no distorsione. Barra di stato mostra dimensioni raster preview e output.
- Aggiunte regressioni su fitting, proporzioni, preview full-resolution e validazione del caso 2500×2000.

## Novità M4C.2-R2 (fix su M4C.2-R1)

- `StudioColorValue.FromHsv`: uso di `0d` per uniformare le tuple di tutti i rami dello `switch` a `double` (CS8506/CS8131).
- `StudioWindow`: `WindowState = Avalonia.Controls.WindowState.Maximized` per aprire lo Studio massimizzato, mantenendo titolo e taskbar.
- `StudioColorValueTests`: sei nuovi casi delle tonalità a metà settore, preservazione RGB e alpha.
- **Nessuna nuova milestone artistica in questa patch**, nessun cambio di output delle ricette o di dipendenze.

## Novità M4C.2-R1

- Quattro controlli facoltativi in sezione **Materia** per ogni artwork (`paper-texture`, `paper-strength`, `pigment-granulation`, `ink-absorption`), default neutri e ricette v1 compatibili.
- `MaterialPostProcessor.cs` in `src/GenerativeArt.Rendering.Skia/`: rumore spaziale interpolato seeded, carte `cotton`/`rough`, pigmento granulare e deposizione/assorbimento localizzato lungo i bordi.
- `ArtisticFinishOptions` esteso senza rompere il costruttore precedente a quattro argomenti; `ArtisticPostProcessor` applica prima il nuovo materiale e poi M4C.1 (grana/dithering) se richiesti.
- Zero variazioni a `SkiaRasterSurface`/CLI/Studio: il percorso comune e i controlli generati dal catalogo rendono il gruppo nuovo accessibile in entrambi.
- Cinque nuovi JSON Full HD, test `MaterialPostProcessorTests`, parametri e controlli `CatalogTests` aggiornati.

## Vincoli importanti

- Stack **C#/.NET 10**, Avalonia (XAML consentito), SkiaSharp, minimo PowerShell; non introdurre Kotlin, Go, Python, Java nel prodotto.
- ID dei diciassette generatori, `algorithmVersion`, schema JSON v1, seed, default cromatici, CLI, preferiti e layout Studio: preservare.
- **README:** solo descrizione del progetto, installazione ed esecuzione, con screenshot sotto il titolo. Changelog, roadmap, risultati dei test e note di consegna restano nei file separati; non reinserirli nel README.
- Finiture dopo downsample a risoluzione finale; alpha invariato; effetti spenti conservano la codifica PNG storica; niente accumulo di frame in memoria.
- Post-processing fino a 16.777.216 pixel finali in modalità standard (limite globale). In fine-2x il limite di 16.777.216 sottopixel implica max 4.194.304 pixel finali; un poster grande può richiedere tempi/RAM elevati.
- Il nuovo assorbimento è un'approssimazione dei bordi, non una simulazione fisica dell'acquerello.

## Prossime attività

1. Estendere i livelli filtrati M4D.3 a editor/maschere arbitrarie/blend configurabili M4C.3, mantenendo output deterministici.
2. M4B.2 noise ciclico e nuovi effetti animati; domain warping e LIC già disponibili in M4B.1. Priorità a loop morbidi e export progressivo nel modello.
3. Sviluppo M4D pennelli organici, M5 export media.
4. All'occasione valida le revisioni recenti: suite, alpha, PNG Full HD, memory budget, confronto default storico, carico CPU e comportamento loop.

## Validazione Windows — PowerShell 7

Aprire la cartella con `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Documenti prioritari: `CURRENT_STATE.md`, `ROADMAP.md`, `M4C2_MATERIA_CARTA_PIGMENTI.md`, `TESTING.md`, `QUALITY_GATES.md`. Conservare l'intero ZIP a ogni evoluzione.
