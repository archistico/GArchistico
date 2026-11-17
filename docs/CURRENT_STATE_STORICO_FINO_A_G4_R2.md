## 17/11/2026 — M4E.3-G4-R2: Ocellus interstizi e rilievo; Rete nodi e pellicole

**Baseline certificata dall'utente:** G4-R1.2 compila e tutti i test passano su Windows. Il blocco G4-R2 prosegue da quella versione, mantenendo integralmente Crinis R10 e gli hotfix R1.1/R1.2.

**R2 implementata nei sorgenti:** Ocellus aggiunge `pore-strength`/`pore-radius` e `rim-light`/`rim-width`; Rete aggiunge `junction-strength`/`junction-radius`, `node-glow` e `membrane-opacity`. I nuovi controlli sono opzionali e neutri per vecchie ricette JSON v1; i **preset iniziali Studio** dei due generatori sono invece espressamente aggiornati per mostrare gli effetti.

**Catalogo:** 29 generatori, 29 preset incorporati, 173 ricette JSON (6 nuove in `examples/recipes/m4e3-g4-r2/`); 17 preset personali e tutti gli altri generatori invariati. Nuovi test Algorithms/Application/Rendering, note e comparazioni nella [scheda G4-R2](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md).

**Gate aperto:** la R2 richiede ancora build Release e test .NET su Windows (`clear && dotnet clean && .\tools\validate.ps1`) e verifica visiva A/B. Dopo il gate decidere se proseguire G4-R3 o passare a G5. Compositore B, workflow artista D, animazioni evolute più avanti.

---

# Stato corrente — M4E.3-G4-R1.2 (17 novembre 2026)

**Ultimo riscontro Windows:** dopo il fix CS0117 R1.1, i progetti Algorithms.Tests e Application.Tests compilano, mentre `GenerativeArt.Rendering.Skia` fallisce su **CS0136** (`GartCellularRenderer.cs:152`): dichiarazioni in conflitto di `ink` (copertura per pixel e colore palette). Nessuna suite completa G4 è ancora confermata come superata.

**R1.2:** rinominati `double ink` in `wallCoverage` e `uint ink` in `wallArgb` con tutti i riferimenti correlati. Cambiamento limitato alla leggibilità/validità C#; rendering e compatibilità delle ricette invariati. La correzione `GeometryDistance.Hypotenuse` introdotta in R1.1 è preservata. **29 generatori, 29 preset, 167 esempi JSON, 17 preset personali invariati.**

**Prossimo gate Windows:** dalla radice della solution, in PowerShell 7, `clear && dotnet clean && .\tools\validate.ps1`. Controllare compilazione e suite, poi verificare visivamente Ocellus/Rete e corrispondenza preview/export. Successivamente G4-R2 (Ocellus microfori e Rete nodi/pellicola); Crinis R10 resta conservata e in pausa.

---

# Stato corrente — M4E.3-G4-R1.1 (17 novembre 2026)

**Errore riscontrato su Windows:** G4-R1 non compila a causa di 4 × CS0117 su `Math.Hypot` nei kernel Ocellus/Rete. Poiché la build si è fermata prima del completamento della solution, **non vi è ancora un risultato xUnit G4-R1**.

**Hotfix consegnato:** distanza euclidea implementata con `GeometryDistance.Hypotenuse` per .NET 10, sostituzione completa degli usi anche nei renderer Skia e nell'oracolo dei test. Test numerici aggiuntivi. Zero modifiche a preset, palette, algoritmi grafici e ricette. **29 generatori, 29 preset incorporati, 167 JSON**.

**Verifica da eseguire:** `clear && dotnet clean && .\tools\validate.ps1` su Windows/PowerShell 7. Il risultato atteso è l'assenza di CS0117 e il completamento di tutte le suite; segnalare eventuali altri errori senza attribuire esiti non confermati.

**Roadmap:** prima chiudere il gate G4-R1.1 con build, test e confronto Ocellus/Rete; poi G4-R2 qualità artistica (microfori Ocellus, nodi/pellicole Rete), G5 nuove opere. Crinis R10 resta conservato; B compositore, D workflow e animazioni dopo.

---

# Stato corrente — M4E.3-G4-R1 (17 novembre 2026)

**Dove siamo:** terminata la fase prioritaria Crinis e passati a **Ocellus/Rete**, su richiesta esplicita. Crinis R10 non cancellata né considerata conclusa artisticamente; ultima compilazione/test riferiti dall’utente: R9. R10 e G4 richiedono validazione Windows.

**Fatto:** 2 nuovi algoritmi autonomi, 2 preset Full HD, 8 ricette, palette e selettore Auto, test aggiuntivi e documentazione. **29 generatori, 29 default embedded, 167 ricette JSON**. I 17 preset originali dell’utente non sono stati modificati.

**Stato QA:** controllo statico delle ricette e inventario. Suite .NET Release e verifica visiva non ancora eseguite/confermate per G4; utilizzare `clear && dotnet clean && .\tools\validate.ps1` da PowerShell 7. Ocellus/Rete sono prime reinterpretazioni, non repliche bit-a-bit di Gȧrt.

**Da fare:** gate Windows, rendering visivo Full HD, G4-R2 per specks Ocellus e nodi/film/drenaggio Rete; poi G5 algoritmi ancora mancanti, A4 e A5, Fase B e D. Animazioni avanzate differite.

**Guida principale:** [M4E3_G4_OCELLUS_RETE.md](M4E3_G4_OCELLUS_RETE.md).

---

# Stato corrente — M4E.3-G3-R10 (17 novembre 2026)

**Dove siamo:** il feedback utente conferma compilazione e test **R9 passati**. Il rendering mostrato ha buona profondità tra fasci; nel singolo fascio i fili appaiono troppo minuti e poco tridimensionali. La ricetta allegata, conservata integra, contiene `hair-width=5.767` ma i fili restano limitati dalla distanza locale.

**Implementato R10:** quattro parametri opzionali per volume **interno** al fascio (profondità a tre strati, filamenti principali più corposi, luce di sezione, ombre di contatto interne). Preset nuovi lavori Studio attivo, mentre i JSON precedenti conservano il percorso/pixel R9 se i nuovi campi non sono presenti. Tre ricette A/B R10; **27 generatori, 27 preset incorporati, 159 esempi JSON**. Si mantiene `body-opacity=0` senza silhouette opaca.

**QA:** test aggiunti; R10 **non ancora compilata/testata su Windows**. Avviare `clear && dotnet clean && .\tools\validate.ps1` e confrontare le due ricette indaco 1920×1080 al 100% e a immagine completa. Risultato artistico finale non ancora confermato.

**Prossimi lavori dopo i test:** perfezionare lo shading interno alla singola fascia (luce dipendente dalla tangente, micro-ombre ad incroci, calibro fisico), poi eventualmente blur selettivo delle fibre arretrate con budget di memoria controllato. R11/R12 pianificate, G4 Ocellus/Rete dopo stabilizzazione. Restano A1/A4/A5, B compositore, D workflow dell'artista; animazioni rinviate.

**Guida e criteri:** [`M4E3_G3_R10_CRINIS_PROFONDITA_INTERNA.md`](M4E3_G3_R10_CRINIS_PROFONDITA_INTERNA.md) e [`ROADMAP.md`](ROADMAP.md).

---

# Stato corrente — M4E.3-G3-R9 (17 novembre 2026)

**Focus attuale: Crinis, volume del fascio.** Aggiunti compressione trasversale monotona, rigonfiamento lungo la curva e un'illuminazione che considera il gruppo di fibre oltre al singolo filo. Nessuna silhouette della pennellata quando `body-opacity=0`, e protezione del diametro rispetto al passo reale. Sono presenti 3 nuovi controlli in Studio (gruppo `Volume fascio`) e 3 nuove ricette in `examples/recipes/m4e3-g3-r9/`.

**Contratto:** vecchi JSON v1 accettati con nuovi controlli neutrali a zero; il preset Studio per nuovi lavori attiva intenzionalmente le nuove funzioni. Limite di `hair-width` elevato da 5 a 8 per rendere valide le varianti di R7 e R8 senza ritoccarle. Catalogo: **27 generatori, 27 preset integrati, 156 ricette di esempio JSON**.

Il preset di un nuovo lavoro Studio usa anche `hair-width=1.65` (era 0.67), senza cambiare le ricette salvate precedentemente.

**Qualità:** test unitari e test di rendering aggiunti/aggiornati, ma **compilazione e xUnit non eseguiti** in questo ambiente (SDK .NET assente). L'ultimo esito esplicito di compilazione/test OK riferito dall'utente precede le consegne R7/R8; non considerare R9 validata su Windows. La verifica statica e JSON non equivale a quella .NET.

**Cosa resta:** verificare su Windows; confrontare artisticamente R8/R9 su una stessa ricetta e seed; proseguire con occlusione più realistica fra pennellate, se necessaria. Dopo la stabilizzazione di Crinis riprendere la roadmap A3-G4 (Ocellus/Rete), A1/A4/A5, compositore B e workflow D; animazioni avanzate ancora in roadmap.

**Guida:** [`M4E3_G3_R9_CRINIS_VOLUME_FASCI.md`](M4E3_G3_R9_CRINIS_VOLUME_FASCI.md). **Comando:** `clear && dotnet clean && .\tools\validate.ps1`.

---

# Stato corrente — M4E.3-G3-R8 (17 novembre 2026)

**Baseline confermata dall'utente:** G3-R7 prosegue correttamente e la direzione artistica resta Crinis.

**Intervento R8:** il focus e' la separazione dei piani. Il renderer micro ora aggiunge una stratificazione di ombre di contatto lungo il lato in ombra di ogni fibra; questo aiuta i filamenti in primo piano a sedersi visivamente sopra quelli retrostanti, riducendo l'effetto piatto anche senza usare un corpo pieno del fascio. In parallelo, la lettura dei piani arretrati e' stata resa un po' piu' soffice.

**Asset di verifica:** nuova cartella `examples/recipes/m4e3-g3-r8/` con due ricette di studio sull'indigo denso. Totale corrente: **27 generatori, 27 preset incorporati, 153 ricette esempio JSON**.

**Prossimo passo:** se il salto visivo e' ancora insufficiente, lavorare su compressione/espansione dei fasci e su un'illuminazione piu' ampia per gruppi di fibre (non solo per il singolo filo), sempre restando su Crinis prima di passare a G4.

---

# Stato corrente — M4E.3-G3-R7 (17 novembre 2026)

**Baseline confermata:** M4E.3-G3-R6 compila e passa i test su Windows (feedback utente).

**Intervento R7:** focus esclusivo su Crinis micro. Le fibre non sono state aumentate semplicemente di numero: ora ogni filo ha piu' corpo percepito, variazione di spessore lungo il percorso e un piccolo modello luce/ombra locale che lo rende piu' tondeggiante. Obiettivo: ridurre l'effetto capello sottile/piatto e avvicinarsi a una materia piu' tessile e tridimensionale.

**Asset di verifica:** importata la ricetta reale ad alta densita' dell'utente e aggiunta una variante `crinis-indigo-bold-volume-1920.json`. Totale corrente: **27 generatori, 27 preset incorporati, 151 ricette esempio JSON**.

**Prossimo passo:** verificare il salto visivo sul caso indigo denso; poi lavorare su occlusione fra fasci e contatto tra piani (fibre davanti che leggono sopra quelle dietro) prima di passare a G4 Ocellus/Rete.

---

# Stato corrente — M4E.3-G3-R5 (17 novembre 2026)

**Baseline confermata:** M4E.3-G3-R2 compilata e testata. In R3: compilazione riuscita, 1056/1088 test superati. In R4: segnalato il fallimento del test di catalogo `AllArtworksExposeOptionalFinishingControlsWithNeutralDefaults`.

**Intervento R5:** classificazione di `ink-flecks` corretta (gruppo `Microfibre`, non `Finitura`); miglioramento artistico delle microfibre con limite di diametro dipendente dalla densità reale e modulazione localizzata del pigmento. La ricetta originale dell'utente rimane byte-identica; 3 nuove ricette, totale **149 esempi JSON**. 27 generatori, 27 preset incorporati e 17 preset personali conservati. Nessuna modifica agli altri 26 generatori. Verifiche Windows e confronto visivo non ancora confermati.

**Prossimo passo:** verificare test R5 e confrontare i due studi indigo; perfezionare ulteriormente il modello di materia, poi G4 Ocellus/Rete. Fasi B compositore e D workflow artista confermate, animazioni rimandate. Vedi `M4E3_G3_R5_CRINIS_FIBRE_SEPARATE.md`.

---

# Stato corrente — M4E.3-G3-R4 (17 novembre 2026)

**Baseline Windows integralmente validata:** M4E.3-G3-R2. **G3-R3:** build Release riuscita ma 32 test falliti su 1088; 1056 riusciti. Tutti i fallimenti riconducono a un catalogo Application compilato apparentemente precedente alla R3, privo del parametro `detail-style`; il sorgente dello ZIP R3 contiene gia' il parametro. **G3-R4:** risoluzione mirata e gate Release coerente, da confermare con la suite Windows. Nessuna nuova ricetta: 27 generatori e 146 esempi JSON.

- `tools/validate.ps1` pulisce `Release`, ripristina, compila, esegue il gate Crinis del catalogo e poi tutte le suite.
- `GartBrushRenderer` gestisce oggetti ricetta storici privi di `detail-style` usando `classic`.
- A3-G4 Ocellus/Rete rimane il prossimo blocco artistico dopo la stabilizzazione R4; A1/A4/A5 ancora aperte, B compositore e D workflow artista successivi; animazioni rinviate.

Guida: [`M4E3_G3_R4_FIX_RELEASE_CRINIS.md`](M4E3_G3_R4_FIX_RELEASE_CRINIS.md).

---

# Stato corrente — M4E.3-G3-R3 (17 novembre 2026)

**Baseline confermata dall'utente:** M4E.3-G3-R2, compilazione e test Windows superati. **G3-R3:** nuova implementazione microfibre/Cincinnus, in attesa di verifica Windows. 27 generatori, 27 preset, 146 ricette JSON di esempio (inclusi i 6 nuovi studi).

**Correzione richiesta:** quando `body-opacity=0`, spariscono realmente riempimento e ombre geometriche dell'intero fascio; le ombre proprie delle fibre sono indipendenti. Nuovo `detail-style=micro` con densità, microtrama, taper, rilievo, copertura e flecks; palette cremisi e 2 studi 2000×2000. Nessuna modifica agli altri 26 generatori e ai 17 preset personali. Rimossa una proprietà root non supportata dai 4 JSON G3-R2.

**Roadmap:** validazione e confronto visivo dei PNG quadrati; poi A3-G4 Ocellus/Rete, qualità 8K/A4 e consolidamento A1/A5. Fasi B compositore e D workflow artista successive; animazioni rinviate.

---

# M4E.3-G3-R1 — Fascia/Eclectic + correzione catalogo (17/11/2026)

- Ricevuto esito G2-R2: **1040/1041 test**; unica regressione `CatalogTests.CatalogHasStableSortedIdsAndLoopCapability` dovuta a un ordine alfabetico atteso invertito (`crinis` / `cotton`). Corretta **solo la lista di attesa**, in entrambe le coppie invertite (`cotton`/`crinis`, `spaced`/`spiral`); non l'ordinamento del prodotto.
- A3-G3: due generatori `fascia-ribbons` e `eclectic-trails` (campi vettoriali, nastri retinati, crescita con collisioni), otto nuove ricette, due preset embedded. Totale **27 algoritmi / 136 ricette**. Tutti i preset e le ricette precedenti preservati.
- Test nuovi per geometria, serializzazione, cambi di parametri, seed, preview/export, fine-2x, finiture, limiti e cancellazione. Guida `M4E3_G3_FASCIA_ECLECTIC.md`.
- **G3-R1 da validare su Windows**. Fase successiva G4 Ocellus/Rete; A1 e A4/A5 restano aperte; B/D dopo e animazioni rinviate.

---

# Stato corrente — M4E.3-G2-R2 (17 novembre 2026)

**Hotfix del gate Windows:** compilazione di G2-R1 fallita a causa del diagnostico `xUnit2020` in `GartBrushCatalogTests.cs`. G2-R2 sostituisce `Assert.True(false, ...)` con `Assert.Fail(...)` nel solo test interessato. **Nessun sorgente di produzione, preset o ricetta modificato.** Rimangono 25 generatori e 128 ricette. Compilazione e test R2 da confermare; ultima baseline completamente validata dall’utente M4E.2-R5. Roadmap invariata: G3 Fascia/Eclectic, A1/A4/A5 e successivamente B/D; animazioni rinviate.

---

# Stato corrente — M4E.3-G2-R1 (17 novembre 2026)

**Fase A3-G2:** due generatori Crinis/Flamebrush, 8 ricette Full HD, 25 generatori e 128 ricette. Sorgenti C# con kernel in Algorithms, rendering Skia, 25 preset embedded; 17 preset personali e 120 JSON precedenti intatti. Test/validazione Windows G1 e G2 da eseguire. Ultima baseline Windows confermata M4E.2-R5. Dettagli [scheda G2](M4E3_G2_PENNELLI_CRINIS_FLAMEBRUSH.md). Prossima fase G3 Fascia/Eclectic, 8K e A1/A5. B compositore e D Workflow dell’artista in seguito; animazioni rinviate.

---

# Stato corrente — M4E.3-G1-R1 (17 novembre 2026)

**A3-G1 implementata nel codice:** tre generatori di geometrie modulari ispirati allo studio del codice Gȧrt: `alien-letters`, `cotton-circles`, `rigor-grid`. Catalogo **23 generatori**, **120 ricette** (nove nuove Full HD) e **23 preset incorporati** (17 personali invariati più sei successivi). Nuove primitive seeded in Algorithms, descrittori e parametri nell'Application, rendering Skia condiviso, palette Auto complete, test dedicati. Guida: [M4E3_G1_GEOMETRIE_MODULARI.md](M4E3_G1_GEOMETRIE_MODULARI.md).

**Baseline Windows confermata dall'utente:** M4E.2-R5, compilazione e test superati. **Questa G1 non è ancora stata compilata/testata su Windows**. Le verifiche statiche e il manifesto non sostituiscono il gate C#.

**Roadmap:** dopo il gate G1 → G2 Crinis/Flamebrush, G3 Fascia/Eclectic, poi G4 Ocellus/Rete, G5 Accretion/Citymap, G6 Nimbus/Strata; continuano A1 e A4/8K, A5 QA. Successivamente B compositore e D workflow dell'artista; animazioni evolute rinviate.

---

# Aggiornamento ricerca Gȧrt — 17/11/2026

**Baseline operativa confermata:** M4E.2-R5, **compilazione e tutti i test Windows superati** secondo il feedback dell’utente. La presente consegna è esclusivamente documentale: **nessuna modifica a sorgenti, preset, ricette, test o renderer**. Stato invariato: 20 generatori, 111 ricette. Aggiunto [STUDIO_GART_PORTING_M4E2.md](STUDIO_GART_PORTING_M4E2.md), con confronto codice Gȧrt/C# e matrice G1–G6. Pronto il passaggio alla prima implementazione A3-G1. PNG/JPG della galleria originale assenti nell’archivio Gȧrt; confronti visivi ancora da svolgere.

---

# Stato corrente — M4E.2-R5 (17 novembre 2026)

**Hotfix di compilazione:** corretto `CS0618` nell'uso di SkiaSharp `DrawImage` in `ArtisticScanlineFinisher.cs` (copia nativa 1:1 con `SKSamplingOptions(SKFilterMode.Nearest)`). Inserito test per il mantenimento dei pixel nell'encoding PNG. Nessun cambiamento agli algoritmi, ai 20 preset, alle 111 ricette, alla preview o all'export.

**Ultima baseline Windows confermata:** M4E.2-R1. **R4:** compilazione fallita con `CS0618`, come comunicato dall'utente. **R5:** correzione applicata, nuova compilazione/test non ancora verificati. Per i dettagli: [`M4E2_R5_FIX_DRAWIMAGE_SKIA.md`](M4E2_R5_FIX_DRAWIMAGE_SKIA.md).

---

# Stato corrente — M4E.2-R4 (17 novembre 2026)

**A4 incrementale:** finiture con grana, dithering, carta, pigmenti e assorbimento sui quattro generatori 8K geometrici; percorso nuovo per righe oltre 16.777.216 pixel, percorso storico invariato sui canvas più piccoli. +3 ricette, totale 20 generatori / 111 ricette. Guida: [M4E2_R4_FINITURE_8K_RIGHE.md](M4E2_R4_FINITURE_8K_RIGHE.md).

**Baseline Windows confermata dall'utente:** M4E.2-R1, compilazione e test riusciti. Le modifiche R2, R3 e R4 richiedono validazione; R4 non è dichiarata passata. A1/A0/A5 restano parziali, A2 confermata in precedenza; poi B compositore, D workflow artista, animazioni avanzate rimandate.

---

# Stato corrente — GenerativeArt Studio M4E.2-R2 (A4 8K geometrico iniziale)


# Stato corrente — GenerativeArt Studio M4E.2-R3 (Fase A3 + navigazione)

**17 novembre 2026.** Dalla M4E.2-R2: due nuovi generatori `phyllotaxis` e `reaction-diffusion` (Gray–Scott), entrambi con kernel indipendente da Skia/Avalonia, descrittori, renderer, palette e preset embedded. Totale **20 generatori**, **108 ricette**. Tutti i **17 preset personali** e i 100 esempi precedenti rimangono invariati. Menu sinistro: schede algoritmi e pulsanti delle ricette recenti a larghezza uniforme. Nuovi test Algorithms/Application/Skia. Dettagli [`M4E2_R3_NATURA_E_NAVIGAZIONE.md`](M4E2_R3_NATURA_E_NAVIGAZIONE.md).

**Qualità:** preview a risoluzione finale conservata; 8K UHD già abilitato in R2 solo per i quattro generatori geometrici, non per i due nuovi. Simulazione Gray–Scott vincolata a 350k celle/75M aggiornamenti.

**Validazione:** nuova R3 ancora da verificare su Windows. Baseline confermata dall'utente M4E.1-R3 (build, test, anteprima ed export). R4/R5/M4E.2-R1/R2 non confermate. Fasi A0/A1/A4/A5 restano in corso; poi B compositore, D workflow dell'artista, animazioni avanzate rimandate.

---


**17 novembre 2026.** La R2 permette ai quattro generatori Skia diretti Guilloché, Voronoi, Streamlines distanziate e Poisson di validare immagini statiche fino a **7680×4320**, in orientamento orizzontale o verticale, qualità `standard` e finiture spente. Ogni lato ≤8192 e area ≤33.177.600 pixel. Gli altri renderer restano entro i loro limiti precedenti, e `fine-2x`, grana, carta, dithering e animazione conservano budget separati. +4 esempi 8K in `examples/recipes/m4e2-r2/` (100 JSON complessivi). **18 generatori, 17 preset personali intatti.**

**Stato:** codice e nuovi test predisposti; da verificare compilazione Windows e smoke 8K reale. Baseline confermata dall'utente M4E.1-R3 (build, test, preview/export coincidenti). R4/R5/M4E.2-R1/R2 non confermate. A4 rimane aperta per buffer/tile/finiture 8K e benchmark, A1/A0/A5 ancora parziali; dopo A completata B compositore e D workflow artista. Animazioni evolute rinviate. Guida [`M4E2_R2_8K_GEOMETRICI.md`](M4E2_R2_8K_GEOMETRICI.md).

---

# Stato corrente — GenerativeArt Studio M4E.2-R1 (primo nuovo generatore A3)

**17 novembre 2026.** La nuova revisione aggiunge `wave-interference`: campo d'interferenza d'onda seeded indipendente da Skia/Avalonia, renderer condiviso Studio/CLI, quattro ruoli cromatici, tre palette, tre modalità, finitura e `fine-2x`. **18 generatori, 96 ricette JSON**, 18 preset integrati di cui i **17 forniti dall'utente immutati**. Sono presenti test dedicati in Algorithms, Application e Rendering.Skia, con limite di 150 milioni di contributi radiali. Nessun cambiamento previsto ai rendering dei generatori storici.

**Validazione:** ZIP/JSON/manifesto verificabili staticamente; la baseline Windows confermata resta **M4E.1-R3** (build e test superati, preview ed export coincidenti). **R4, R5 e M4E.2-R1** devono ancora essere validate su Windows. A1 refactoring UI ancora aperto; A2 preview nativa verificata nei flussi utente; A3 avviata anche con nuovo algoritmo; A4 8K ancora da sviluppare; A0 immagini storiche mancanti. Poi **B** compositore, **D** workflow artista, animazioni dopo. Dettagli `M4E2_R1_INTERFERENZE_ONDA.md`.

---

# Stato corrente — GenerativeArt Studio M4E.1-R5 (servizio documenti + nuovi studi)

**17 novembre 2026.** La versione R5 è implementata a partire dalla R4, **senza aspettare i test Windows**: `StudioRecipeDocuments` in Application centralizza import limitato a 64 KiB, convalida, salvataggio JSON locale atomico e provider virtuali; Studio resta responsabile del file picker e dell'interfaccia. Aggiunti 7 test del servizio e 1 test parametrico delle 10 nuove ricette sperimentali Full HD in `examples/recipes/m4e1-r5/`. **17 generatori, 17 preset embedded invariati, 91 ricette**. Tutti i renderer e l'anteprima nativa sono invariati.

**Baseline Windows confermata dall'utente:** M4E.1-R3: compilazione, suite, anteprima/export OK. **R4 e R5 attendono validazione**; non anticipare il risultato. A1 rimane aperta (controller UI/lifecycle), A2 confermata nei flussi provati, A3 ricette avviata ma nuovi algoritmi ancora pianificati, A4 8K ancora pianificata. A0 resta aperta per anteprime storiche mancanti. Seguono B compositore, D workflow artista, animazioni posticipate. Dettagli `M4E1_R5_DOCUMENTI_RICETTE_STUDI.md`.

---

# Stato corrente — GenerativeArt Studio M4E.1-R4 (controlli condizionali)

**17 novembre 2026.** L'utente ha confermato che **M4E.1-R3 compila, i test passano, l'anteprima è corretta e l'export coincide con l'anteprima**. Baseline R3 verificata. La nuova R4 è implementata, da validare separatamente.

**R4:** `StudioParameterApplicability` (Application), separazione degli editor tipizzati in `StudioWindow.ParameterControls.cs` e presentazione delle dipendenze in `StudioWindow.ParameterApplicability.cs`: vortex/frequency, torsione, grana, dithering, carta e maschere. I controlli inapplicabili vengono disabilitati con un messaggio, senza perdere i valori delle ricette. A1 prosegue, A2 anteprima a risoluzione nativa verificata dall'utente per il flusso provato; A3 nuovi algoritmi/ricette e A4 8K ancora aperti. Fasi B e D confermate, animazioni rinviate. Dettagli: `M4E1_R4_PARAMETRI_CONDIZIONALI.md`.

---

# Stato corrente — GenerativeArt Studio M4E.1-R3 (fix test dipendente dal campo)

**17 novembre 2026.** Ricevuta conferma Windows della versione **M4E.1-R2: compilazione riuscita, 879/880 test superati**, con una sola regressione `EveryArtisticControlChangesTheRaster` per `spaced-streamlines/frequency`. Il test partiva dal preset dell'utente con `field=vortex`, che non usa la frequenza. In R3 il test imposta `field=curl` prima di verificarne l'effetto; nuova regressione esplicita conferma che il vortice è indipendente dalla frequenza. **Codice di produzione, preset e 81 ricette invariati.** Test R3 ancora da confermare.

Prosegue Fase A: A0 archivio e anteprime storiche mancanti; A1 isolamento controller e UX dei parametri applicabili; A2 preview nativa implementata con QA GUI/DPI ancora aperto; A3 nuove ricette/algoritmi; A4 output 8K con budget e quality gate; A5 stabilizzazione. Poi Fase B compositore e Fase D workflow dell'artista, mentre le animazioni evolute restano rinviate. Dettagli in `M4E1_R3_FIX_TEST_STREAMLINES.md`.

---

# Stato precedente — GenerativeArt Studio M4E.1-R2 (A1 incrementale + A2 anteprima reale)

**17 novembre 2026.** La finestra Studio usa adesso il **raster nativo con le dimensioni della ricetta finale**, senza preview ridotta né campi di dimensionamento raster. `Adatta`, `100%`, rotella e riquadro configurabile operano solo sulla vista. `StudioRenderCoordinator` (Application, senza Avalonia) serializza preview ed export, gestisce i token e l'output PNG atomico; nuovi test unitari e rendering Skia confrontano i percorsi. Nessun algoritmo o JSON modificato: **17 generatori, 81 ricette di esempio, 17 preset embedded** conservati.

**Stato:** implementazione R2 pronta per validazione Windows, non ancora confermata. Ultima baseline Windows documentata **M4D.4-R1, 859/859 test**. A1 richiede ancora controller indipendenti e QA dispatcher; A2 richiede smoke GUI/DPI e verifica pixel; A0 immagini storiche mancanti, A3 nuovi algoritmi/ricette e A4 8K rimangono aperti. Output statico limitato per ora a 4096×4096 e al budget del renderer; qualità fine-2x ha limite più stretto. **Fase B compositore**, poi **Fase D workflow artista** (seed, varianti, confronto A/B, galleria, contact sheet, preset e SVG selettivo); animazioni evolute rinviate. Dettagli in `M4E1_R2_PREVIEW_NATIVA_COORDINATORE.md`.

---

# Stato corrente — GenerativeArt Studio M4E.1-R1 (A1 iniziale + roadmap D)

**17 novembre 2026.** La versione precedente M4D.4-R2 integra 17 JSON originali come preset Studio. Questa revisione avvia **A1**, ripartendo `StudioWindow.cs` da 1.103 a 189 linee più sette partial per responsabilità; `StudioRecipeFormParser` in Application isola la costruzione/validazione del modello dai controlli Avalonia e aggiunge test xUnit dedicati. GUI, renderer, JSON, seed, export e preview hanno intenzionalmente lo stesso comportamento della R2. **A1 è solo parziale**, il passo successivo sono controller indipendenti e verifiche lifecycle/headless.

**Fase D — Workflow dell'artista** approvata e inserita dopo il compositore Fase B: varianti/seed, confronto A/B, galleria, contact sheet, preset, SVG selettivo in seguito. **Animazioni rinviate**.

**Baseline Windows confermata:** M4D.4-R1, 859/859 test. R2 e M4E.1-R1 da confermare su Windows; non attribuire alla R1 nuove conferme. Nessuna implementazione A2 (anteprima a risoluzione finale), A3 (nuovi algoritmi), A4 (8K), Fase B o D. Rimane aperta l'incompletezza delle 52 immagini del vecchio pacchetto di origine. Documenti guida: `ROADMAP.md`, `M4E0_PIANO_A_B_DEFAULTS.md`, `M4E1_A1_WORKFLOW_D.md`, `HANDOFF.md`.

---

# Stato corrente — GenerativeArt Studio M4D.4-R2 (roadmap + default da allegati)

**17 novembre 2026.** Da `GenerativeArt.zip` M4D.4-R1 sono stati integrati **17 JSON allegati come preset embedded per i nuovi lavori Studio**, compresi il seed speciale di Domain Warp, colori, dimensioni e animazione di Flow Loop. `StudioRecipeDefaults.RecipeFor` sostituisce la vecchia tabella parziale di override; selezione Studio e test correlati aggiornati. **Non sono stati ancora sviluppati** il refactoring di `StudioWindow.cs`, l'anteprima nativa obbligatoria, l'8K o il compositore: sono **in roadmap Fase A/B**. Le animazioni vengono dopo.

**Baseline Windows verificata:** M4D.4-R1, **859/859 test**. **R2:** codice e documentazione aggiornati, test Windows non ancora ricevuti. Lo ZIP M4D.4-R1 di partenza era privo di **52 immagini** elencate nel manifesto: la loro ricostruzione resta attività A0; il nuovo manifesto registra soltanto i file realmente inclusi, senza dichiarare recuperate le anteprime.

**Ordine successivo:** A0 integrità distribuzione; A1 refactoring StudioWindow; A2 preview 1:1; A3 nuovi algoritmi/ricette; A4 8K; A5 QA; **B compositore universale**. Dettagli: [`M4E0_PIANO_A_B_DEFAULTS.md`](M4E0_PIANO_A_B_DEFAULTS.md), [`ROADMAP.md`](ROADMAP.md).

---

# Stato corrente — GenerativeArt Studio M4D.4-R1

**Data:** 16 novembre 2026. **M4D.4-R1 — Orbite SDF riprogettato:** macchie organiche, fusione di quattro pigmenti, venature, rilievo, piani e antialiasing locale. **17 generatori, 81 ricette, build Release senza avvisi/errori e 859/859 test Windows** (Codex, SDK 10.0.201). Otto nuovi PNG Full HD, due fine-2x; controlli Studio, JSON, export CLI e 15 PNG precedenti identici verificati. Confronto AA contro riferimento 4×: errore alpha ridotto del 71,3% nel caso registrato. Guida [`M4D4_SDF_MACCHIE_FUSIONI.md`](M4D4_SDF_MACCHIE_FUSIONI.md), evidenze `docs/validation/M4D4_R1_*`. Desktop/DPI reali, altri OS, nuovi poster 4K ed editor livelli/maschere/blend restano aperti. Restore da cache NuGet locale, audit online e telemetria build disattivati.

## Stato incrementale

| Revisione | Stato funzionale | Stato Windows |
| --- | --- | --- |
| M0–M4A.4-R1 | Framework, Studio, nove generatori e controlli creativi iniziali | Confermato dall'utente |
| M4A.4-R2 | Ruoli colore, selettore HSV/RGB/alpha | Gate aperto |
| M4A.4-R3 | Supersampling 2× e PNG alpha-safe | Gate aperto |
| M4C.1-R1 | Grana seeded e dithering Bayer su nove generatori | Gate aperto |
| M4C.2-R1 | Carta cotone/ruvida, pigmenti granulari, assorbimento locale | **Build fallita su Windows: CS8506, CS8131** in `StudioColorValue.cs` |
| **M4C.2-R2** | Fix switch HSV e avvio Studio massimizzato, sei test aggiunti | **Build OK, 506/507 test superati** (conferma utente) |
| **M4C.2-R3** | Correzione test JSON, finiture e Plasma 5 MP, raster/riquadro anteprima configurabili, Adatta/100% | **Build OK, 513/514 test (utente); test budget materia obsoleto** |
| **M4C.2-R4** | Test materia allineati ai budget standard/fine-2x | **Build Release OK, 521/521 test (Codex Windows)** |
| **M4B.1-R1** | Domain Warp, LIC Flow, sette ricette, streamlines dense, picker con footer fisso | **Build Release OK, 558/558 test; sette PNG Full HD e smoke picker headless (Codex Windows)** |
| **M4D.1-R1** | Guilloché/incisione, dettaglio Domain Warp, slider fini, otto ricette | **Build Release OK, 626/626 test; otto PNG Full HD, smoke Studio headless e compatibilità PNG (Codex Windows)** |
| **M4D.2-R1** | Pennelli/paesaggi e cinque generatori iniziali ampliati, quattordici ricette | **Build Release OK, 733/733 test; Full HD, 12 frame CLI, Studio e compatibilità PNG verificati (Codex Windows)** |
| **M4D.3-R1** | Carta ritagliata/bokeh, livelli filtrati riutilizzabili, paesaggi con profondità e dieci ricette | **Build Release OK, 801/801 test; dieci PNG Full HD, Studio e 15 hash PNG precedenti verificati (Codex Windows)** |
| **M4D.4-R1** | Orbite SDF: macchie/fusioni, stili pigment/glass, antialiasing locale e otto ricette | **Build Release OK, 859/859 test; Full HD/fine-2x, Studio, CLI, copertura AA e hash PNG precedenti verificati** |

## M4C.2-R3 — richieste emerse nella validazione Windows

1. `RecipeTests.MissingParametersAreRejected` manipolava testo JSON lasciando una trailing comma: ora il test rimuove realmente `scale` via `System.Text.Json.Nodes.JsonNode` e verifica il rifiuto semantico `ArgumentException`.
2. Budget finiture elevato da 4.194.304 a 16.777.216 pixel finali, con controllo prima del rendering. Rimosso il limite separato di Plasma 4.194.304; 2500×2000 (5.000.000 pixel) ora ammesso in `standard`, anche con carta/grana. `fine-2x` resta limitato a 16.777.216 sottopixel. Non sono aboliti i budget degli altri algoritmi o delle animazioni.
3. Studio: dimensione raster massima e dimensione riquadro in campi distinti, separati da larghezza/altezza output. `Adatta` usa lo zoom effettivo per mostrare tutti i bordi e `100%` usa scala 1:1; auto-fit al nuovo rendering e al resize. Barra di stato segnala dimensioni anteprima e output.
4. Regressioni aggiunte: JSON valido senza parametro richiesto, validazione Plasma 5 MP con finitura, fitting di varie proporzioni, preview raster 1:1 e preview limitata. Documentazione specifica: `M4C2_R3_PREVIEW_E_BUDGET.md`.

## M4C.2-R2 — bugfix e comportamento finestra

- Segnalazione utente dalla build della R1: `StudioColorValue.cs(54,44)` CS8506 e CS8131, entrambe causate dall'inferenza del tipo comune delle tuple nello `switch` HSV.
- Sostituiti gli zero `int` con `0d` in tutti e sei i rami; rimangono inalterate conversione numerica, formato HEX e alpha.
- In `StudioWindow` avvio `WindowState.Maximized` (finestra massimizzata, non borderless/kiosk). Sei nuovi casi di test HSV con tonalità intermedie.
- La build R2 è stata confermata; la suite R2 ha fallito 1/507 casi, corretto nel codice R3.

## M4C.2-R1 realizzata

1. Nuovo gruppo **MATERIA**: `paper-texture`, `paper-strength`, `pigment-granulation`, `ink-absorption` su tutti e nove i generatori, con default spenti e JSON v1 compatibile.
2. Motore `MaterialPostProcessor`: rumore interpolato a più scale/anisotropie, depositi cromatici guidati dalla luminanza, semplice assorbimento del colore scuro sui bordi; seed e coordinate fisse anche nei loop.
3. Preservazione alpha; assorbimento usa massimo tre righe originali, nessuna copia dell'intera immagine oltre al buffer di finitura già introdotto in M4C.1.
4. Integrazione nel post-processing comune dopo il campionamento 2× e prima di grana/Bayer; il ramo senza finitura resta invariato. Il limite storico 4.194.304 pixel delle finiture è stato ampliato a 16.777.216 nella R3.
5. Nuova suite `MaterialPostProcessorTests`, aggiornamento `CatalogTests`, cinque ricette Full HD, specifica `M4C2_MATERIA_CARTA_PIGMENTI.md` e documentazione operativa.

Le texture sono un'approssimazione raster procedurale, **non** un simulatore fisico di fluidi/pigmenti. Non ci sono nuovi pacchetti, linguaggi runtime o versioni di schema.

## Azione successiva

**Proseguire lo sviluppo**: nuovi algoritmi/ricette con dettaglio e profondità; estendere compositing a editor livelli/maschere/blend M4C.3, noise ciclico M4B.2 e animazioni fluide. SDF moderno ora disponibile in M4D.4; vecchie ricette SDF restano classic per compatibilità. Restano QA desktop/DPI, poster, altri OS e animazioni lunghe.

## Comandi Windows — PowerShell 7

Nella radice `GenerativeArt`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Per esempi, controlli dettagliati e aspettative vedere [`M4C2_MATERIA_CARTA_PIGMENTI.md`](M4C2_MATERIA_CARTA_PIGMENTI.md), [`TESTING.md`](TESTING.md) e [`QUALITY_GATES.md`](QUALITY_GATES.md).


## Aggiornamento M4E.3-G3-R2
- Crinis migliorato con `body-opacity`, `fiber-relief` e `fiber-fray`.
- La base del fascio può essere portata a zero; le fibre possono risultare più tridimensionali e più tessili.
- Aggiunte quattro nuove ricette di studio dedicate a Crinis nella cartella `examples/recipes/m4e3-g3-r2/`.
