# GenerativeArt Studio — Visione di prodotto

**Versione:** P2, 16 novembre 2026  
**Stato:** piano di prodotto, non implementazione.  
**Baseline corrente del codice:** M3C.2-R1, confermata su Windows dall'utente (compilazione, test xUnit, generazione ricetta JSON e PNG). Il resto della visione descrive funzionalità progettate, non già realizzate.

## 1. Identità

GenerativeArt è un ambiente nativo C#/.NET 10 per *creative coding* e arte generativa. È composto da un motore riutilizzabile, una CLI headless e un'applicazione desktop **GenerativeArt Studio** basata su Avalonia. I tre accessi condividono catalogo, parametri, ricette, renderer e versioni degli algoritmi. Non è un porting 1:1 delle librerie Gȧrt e jdxyw/generativeart e non esegue Go, Kotlin, Python o servizi web per disegnare.

**Promessa principale:** un'opera costruita nello Studio può essere rigenerata dalla CLI utilizzando la stessa ricetta, senza riimplementare la logica. La parità rigorosa dei byte raster fra sistemi operativi non è promessa in presenza di differenze Skia/font/codec: il contratto deterministico va definito per algoritmo e verificato con tolleranze documentate.

## 2. Destinatari

1. **Artista/esploratore**: seleziona un generatore, cambia seed, colori e parametri, confronta, salva ed esporta senza programmare.
2. **Sviluppatore C#**: crea o integra algoritmi e artwork tramite API pubbliche, testabili senza GUI.
3. **Utente batch/automazione**: riproduce preset, crea varianti, poster e animazioni con CLI e file di ricetta.

## 3. Flusso principale nello Studio

1. **Esplora** un catalogo navigabile con filtri per famiglia, output (raster/vettoriale/animato), tags, descrizione e anteprima.
2. **Seleziona** un generatore: identificativo stabile, versione, preset predefinito, note sul costo computazionale e requisiti.
3. **Configura** dimensioni, seed, palette e parametri tramite controlli generati da schemi tipizzati; mostra errori prima del rendering.
4. **Genera l'anteprima** a risoluzione ridotta, con progressi, annullamento, memorie/risorse preventivate e debounce delle modifiche.
5. **Confronta** varianti in una galleria; blocca parametri, cambia seed, memorizza preferiti e visualizza le differenze.
6. **Salva la ricetta** in JSON versionato; eventualmente incorpora metadati sicuri nel PNG.
7. **Esporta** a risoluzione finale e in un formato supportato dal tipo di artwork, dichiarando eventuali limiti o conversioni.

## 4. Punti di forza da perseguire

- **Determinismo verificabile:** seed esplicito, versionamento algoritmi, RNG substream identificati, nessun random globale; fallire con un errore chiaro se una ricetta richiede un'implementazione incompatibile.
- **Un'architettura comune:** nessuna logica artistica specifica della CLI o dei ViewModel Avalonia.
- **Qualità dell'output:** colori e alpha corretti, controlli espliciti su lineare/sRGB, pipeline di export progressiva, vettoriale nativo quando possibile.
- **Esplorazione efficace:** Sweeper con spazio dei parametri controllato, generazione lazy, galleria comparativa e preset riusabili.
- **Prestazioni sane:** controllo del numero di task, budget per memoria/tile/frame/iterazioni, cancellazione; nessun accumulo necessario di tutti i fotogrammi.
- **Sicurezza locale:** nessun servizio HTTP necessario; ricette lette come dati non fidati, limiti di dimensione/profondità, path sicuri, esportazioni atomiche.

## 5. Perimetro per release

### Primo prodotto utilizzabile (R0, dopo M3C–M3D)
- Catalogo con almeno tre generatori end-to-end rappresentativi: grafica parametrica (Spirograph/Lissajous), noise/pixel (Plasma), flow (Even Streamlines).
- CLI `list`, `describe`, `render`; editor parametri e preview minimale Avalonia in C# (con XAML/AXAML consentito).
- Seed e ricette JSON riproducibili, salvataggio PNG, errori leggibili, cancellazione della preview.
- Test end-to-end su Windows e pipeline CI Linux; macOS almeno da verificare prima della prima distribuzione dichiarata multipiattaforma.

### Studio creativo (R1)
- Catalogo di famiglie diversificate; Marching Squares, SDF 2D, palette, noise periodico, LIC, filtri e pennelli.
- Preset, storia delle generazioni, comparazione A/B, Sweeper e contact sheet.
- Ricetta incorporabile nei PNG con limiti e lettura sicura.

### Produzione avanzata (R2)
- SVG/PDF realmente vettoriali dove supportati, poster, plotter e archivi di esportazione.
- Animazioni con timeline e codifica incrementale; GIF/APNG e video se licenze e distribuzione lo consentono.
- Profilazione risorse e operazioni ad alta risoluzione; supporto HDR/16-bit solo quando prestazioni e casi d'uso lo giustificano.

### Laboratorio specialistico (R3)
- Fluidi, reaction-diffusion, marbling, frattali, tassellazioni avanzate, grafica 3D e simulazioni evolute; aggiunte selettive, non clonazione indiscriminata di 164 opere.

## 6. Esplicitamente fuori perimetro iniziale

- Clone pixel-identico di ogni opera Go/Kotlin; IDE/linguaggio di scripting interno; editor a nodi; marketplace/plugin dinamici; account, cloud sync o server web; AI generativa integrata; elaborazione GPU obbligatoria.
- Inserire un formato non supportato solo perché il Go lo dichiara: ogni formato richiede test di lettura/export e un livello di qualità definito.

## 7. Dati e persistenza

Ricette e preset inizialmente in JSON versionato, trasferibili e diffabili. Galleria e cache preview possono usare file/metadati locali; valutare SQLite **solo quando** ricerca, indicizzazione, grandi volumi o transazioni lo richiederanno. Non introdurre database per un catalogo iniziale di poche opere.

## 8. Vincoli non negoziabili

Implementazione, algoritmi, CLI, desktop e test **solo C#/.NET 10**. Un minimo script PowerShell per validazione/esecuzione. File di progetto, JSON, CSV e Markdown sono dati/configurazione, non altri linguaggi applicativi. Avalonia con interfaccia attualmente code-only; XAML/AXAML è consentito e potrà essere introdotto dove utile. Nessuna dipendenza runtime da Gȧrt o dal progetto Go. ZIP completo con roadmap, changelog, handoff e risultato effettivo dei test a ogni consegna di codice.
