> **Avanzamento G4-R2 (17/11/2026).** L'utente ha confermato build e test G4-R1.2 superati. La R2 amplia Ocellus (interstizi a tre bordi, luce degli anelli) e Rete (nodi a tre celle e pellicole entro le celle), con otto controlli opzionali, sei ricette A/B, suite aggiornata e preset Studio nuovi. La nota tecnica completa è in [M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md](M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md). R2 deve ancora superare build/test Windows e confronto visivo.

---

> **Aggiornamento R1.2 (17/11/2026).** Nuovo log Windows: i test Algorithms/Application compilano, ma il renderer Rete non compila a causa di CS0136 (`GartCellularRenderer.cs:152`), dovuto al riutilizzo del nome locale `ink` per due scopi diversi. Rinominata la copertura del bordo in `wallCoverage` e il colore palette in `wallArgb`, senza modifiche a matematica, composizione o JSON. La correzione `Math.Hypot` di R1.1 resta inclusa. Validazione Release/test ancora aperta.

> **Aggiornamento R1.1 (17/11/2026).** La build Windows della prima G4-R1 ha evidenziato quattro errori CS0117: `Math.Hypot` non esiste in .NET 10. L'hotfix introduce in Algorithms `GeometryDistance.Hypotenuse` con normalizzazione contro overflow/underflow intermedi, sostituisce tutti gli usi anche nel rendering Skia e aggiorna l'oracolo di test Rete con una formula `Math.Sqrt` indipendente. Aggiunta copertura numerica in `GeometryDistanceTests`. **La geometria artistica, le ricette JSON e i preset restano invariati.** Compilazione/test di R1.1 da confermare su Windows prima di procedere a R2.

# M4E.3-G4-R1 — Ocellus e Rete (17/11/2026)

## Decisione artistica e stato

Su richiesta dell'utente **Crinis passa in pausa dopo R10** e il prossimo ramo artistico è **G4**. La precedente regola della roadmap «non iniziare G4 finché Crinis non è completo» è superata dalla nuova decisione; restano tutte le opere, i preset, i test e i documenti di Crinis. **R10 non ha ancora conferma dei test Windows**; l'ultima conferma esplicita è R9.

**G4-R1 è implementata nei sorgenti e predisposta per la suite; compilazione/test Windows e valutazione visiva non sono ancora confermati.**

## Fonti studiate direttamente

- `gart-main/arts/bubbles/src/ocellus/Ocellus.kt` — Gȧrt, Igor Spasić, BSD-2-Clause.
- `gart-main/arts/cell/src/rete/Rete.kt` — Gȧrt, Igor Spasić, BSD-2-Clause.
- Controlli e formule sono stati ricostruiti in **C#/.NET**, senza importare codice Kotlin, librerie Kotlin né immagini originali. Le nuove sequenze pseudo-casuali usano **SplitMix64**, quindi non riproducono i pixel originali di Gȧrt.

### Ocellus: `ocellus-eyespots`

Il codice originale crea occhi distribuiti casualmente, ciascuno con un nucleo e profondità assegnata; a ogni livello i dischi vengono fusi con una *smooth min*, più ampia vicino al livello di base. Gli anelli esterni sono progressivamente più larghi; il rendering compone livelli cromatici concentrici.

**Nella nostra R1:** kernel `OcellusField` nel modulo Algorithms con generazione seeded, profondità fino a 13, geometria SDF e smooth-min; renderer `GartCellularRenderer` che calcola le coperture alla risoluzione finale. Controlli: `eyes`, `min-depth`, `depth-bias`, `core-radius`, `room`, `margin`, `band`, `growth`, `fuse`, `pool` e cinque palette, con quattro colori di anello e sfondo modificabili.

**Differenza dal riferimento:** il passaggio di *flood-fill* in due iterazioni che colma piccole regioni encluse (*specks*) è rimandato a G4-R2; l'antialiasing è un campionamento SDF locale, non il modello a gradiente del Kotlin.

### Rete: `rete-foam`

Il codice originale impacchetta dischi di molti raggi e disegna le pareti fra le superfici più vicine (schiuma di tipo Apollonius, diversa dal Voronoi a punti). Le pareti si allargano ai nodi; un campo di drenaggio riduce le celle, mentre copie più lontane della rete sono sfocate e più chiare.

**Nella nostra R1:** kernel `ReteFoamField` con inserimento di celle dal grande al piccolo (quota per classe di scala e massimo 20.000 tentativi), variazione noise, drenaggio geometrico e griglia spaziale per trovare le tre superfici più vicine. Il renderer modella l'inchiostro in base alla distanza fra superfici, usa lo smooth-min dei primi due bordi, aggiunge una pellicola luminosa e compone da retro a fronte fino a cinque reti con sfocatura separabile. Controlli su numero/scala/variazione/densità delle celle, fusione, froth, drain, sink, spessore, nodi, pellicola, livelli, blur, opacità, fade, zoom e palette, con tre colori indipendenti.

**Differenza dal riferimento:** il modello della schiuma è una **prima reinterpretazione** che non include ancora bucket grid con ricerca progressiva per anelli, tutte le quattro superfici più vicine, gestione originale di film/core e controllo fine del wet gradient. Rete-R2 dovrà confrontare punti di strozzatura, superficie cellulare, bordo della pellicola e distribuzione delle celle con Gȧrt; non dichiarare equivalenza artistica prima del confronto dei PNG.

## Architettura e contratti

- 2 nuovi `ArtworkDescriptor` e 2 JSON incorporati in `src/GenerativeArt.Application/Defaults/`.
- 2 kernel puramente C# in `GenerativeArt.Algorithms/Geometry/`.
- 1 renderer unico in Skia, separato da Studio/CLI; `ArtworkRenderEngine` registra entrambe le opere per `Validate` e `Render`.
- 2 mapping completi nel selettore colori Auto (`ArtworkRolePalettePreview`).
- 8 JSON versionati in `examples/recipes/m4e3-g4/`, quattro Ocellus e quattro Rete, tutti 1920×1080, con seed distinti.
- Catalogo **29 generatori**, **29 preset incorporati**, **167 esempi JSON**; nessuna modifica ai 17 preset personali.
- Nessuna importazione di software aggiuntivo; rimangono C#/.NET 10, Avalonia/SkiaSharp, XAML consentito e minimo PowerShell. Animazioni avanzate differite.

## Prestazioni e limiti

- Ocellus: preventivo `width × height × 13 × eyes ≤ 900.000.000` unità di lavoro; limite raster storico 4096×4096, con controlli di qualità separati.
- Rete: massimo 4.194.304 pixel finali e `pixel × layers × bubbles ≤ 3.000.000.000`; griglia per celle + quattro buffer `float` transitori e `PixelBuffer` finale. La sfocatura tra livelli è separabile e non usa `DrawLayer`.
- Annullamento controllato per righe/campionamenti, verifica budget **prima di creare la superficie PNG** e prima di scrivere lo stream.
- Preview esatta: stesso renderer e stessa ricetta dell'export, senza riduzione della dimensione del documento. In `fine-2x` il campo per pixel è calcolato al formato finale, mentre la superficie Skia viene impostata a 2×; il dettaglio del campo non equivale a un autentico supersampling analitico 2×: migliorarlo in A4.
- 8K su questi generatori **non ancora disponibile**: non estendere per errore la policy dei renderer geometrici 8K.

## Test predisposti

- `GartG4GeometryTests`: distribuzione riproducibile, raggi annidati, smooth-min, vicini più prossimi della griglia confrontati con scansione esaustiva, cancellazione.
- `GartG4CatalogTests`: preset FullHD, lettura di otto JSON rigorosi, serializzazione canonica, seed unici, preservazione del default Crinis.
- `GartG4RenderingTests`: PNG deterministici, variazione seed e controlli, fine-2x + finiture, annullamento e budget in fase Validate.
- `StudioExactPreviewRenderingTests`: due nuovi casi di parità byte-a-byte fra anteprima nativa ed export.
- Allargati i due test storici sull'elenco immutabile dei generatori da 27 a 29 senza cambiare l'ordinamento del catalogo.

### Verifica Windows (PowerShell 7)

Aprire PowerShell 7 nella cartella dove si trova `GenerativeArt.sln` e lanciare:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando pulisce la solution, lo script fa un'ulteriore pulizia Release, ripristino pacchetti, compilazione Release e test (catalogo Crinis e G4, poi suite completa). L'esito atteso è `Build e test .NET completati con successo.`; in caso contrario fermarsi e comunicare l'errore.

Poi eseguire un render CLI di entrambe le opere, dopo la build:

```powershell
New-Item -ItemType Directory -Path .\output -Force | Out-Null
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\m4e3-g4\ocellus-solare-1920.json .\output\ocellus-g4.png
dotnet run --project .\src\GenerativeArt.Cli -c Release --no-build -- render-recipe .\examples\recipes\m4e3-g4\rete-seppia-1920.json .\output\rete-g4.png
```

Aprire lo Studio e confrontare `Adatta` e `100%` per sfumature delle SDF, bordi fra celle, pellicola e blur delle reti posteriori. Il test automatico non costituisce validazione della qualità artistica.

## Passi successivi

1. **G4-R2 Ocellus:** colmare microfori completamente circondati e migliorare AA del bordo, documentando costo/memoria.
2. **G4-R2 Rete:** quattro superfici, nodi allungati, pellicola e transizione drenaggio più fedele; confronti A/B con screenshot propri e riferimento Gȧrt, stessi temi/rapporti di scala.
3. **A5:** gate Windows completo, stress FHD in fine-2x, profili RAM e tempi; poi ottimizzazioni e poster per tipologia, senza abilitazioni 8K indiscriminate.
4. **G5:** nuove opere prioritarie (accrescimento, trame, fluido). Restano Fase B compositore, Fase D workflow artista e animazioni più avanti.
