# Cincinnus C5-R1 — correzioni e validazione Windows

17 novembre 2026. Verifica della copia di lavoro aggiornata dall'utente, senza ripristinare le vecchie versioni del progetto o sostituire i suoi avanzamenti. **Build Release senza avvisi/errori e 1.225/1.225 test superati**, con .NET SDK 10.0.201 su Windows. Catalogo di 30 generatori, 30 preset incorporati e 176 ricette.

L'utente ha successivamente confermato «ora compila». Il conteggio dei test e gli smoke qui riportati derivano dall'esecuzione Codex; non viene attribuita all'utente una conferma della suite o del risultato artistico.

## Errori riprodotti e corretti

| Problema | Causa | Correzione |
| --- | --- | --- |
| Compilazione interrotta da `xUnit2012`, `CincinnusBristlePainterTests.cs:57` | `Assert.True(collection.Any(...))` non rispetta l'analizzatore xUnit; il progetto tratta gli avvisi come errori | `Assert.Contains(collection, predicate)`, mantenendo il controllo sui pixel interni diversi dallo sfondo |
| Gate catalogo: attesi 8 pigmenti, trovati 9 | Il filtro sul prefisso `pigment-` contava anche il parametro numerico `pigment-granulation` | Verifica dell'elenco esatto `pigment-1` … `pigment-8`, limitato ai parametri di tipo Color |
| Suite rendering: `Artwork non presente nella mappa dei campioni` | Cincinnus non era collegato ad `ArtworkRolePalettePreview`; lo Studio usa questa funzione durante la costruzione dei campioni Auto | Aggiunto il collegamento alle palette del renderer Cincinnus per sfondo e otto pigmenti |

I campioni Auto riutilizzano le palette del renderer. Per lo sfondo si usa lo stesso campionamento a tono 0,02 del pittore: `Sample` diventa interno all'assembly, senza cambiare calcolo o disegno. Sono aggiunti quattro casi di regressione: sfondo delle tre palette coerente con il pittore e rifiuto di ruoli inesistenti/non cromatici. Non vengono disattivati analizzatori o rimossi controlli.

File C# interessati: `CincinnusBristlePainterTests.cs`, `CincinnusC5CatalogTests.cs`, `CincinnusC5RenderingTests.cs`, `ArtworkRolePalettePreview.cs`, `CincinnusRenderer.cs`, `CincinnusBristlePainter.cs`. Preset, JSON, budget, posizionamento e formule pittoriche non vengono modificati da questa correzione.

## Risultati effettivi

`tools/validate.ps1` ha completato clean Release, restore, build Release, gate catalogo e suite completa.

| Progetto | Test superati |
| --- | ---: |
| Core | 115 |
| Algorithms | 249 |
| Application | 219 |
| Rendering.Skia | 642 |
| **Totale suite** | **1.225** |

Il gate rapido Application passa **21 test**, già inclusi nei 219; non vengono sommati nuovamente al totale.

- Tool separato `tools/GenerativeArt.CincinnusPreview`: build Release riuscita, zero avvisi/errori.
- **176 ricette** deserializzate e validate anche attraverso `ArtworkRenderEngine`, comprese le animate con frame 0.
- Tre PNG CLI C5 **768×768** generati e decodificati con Skia: Simple crimson seed 3, Adaptive ocean seed 4, Simple moss seed 11. Dimensioni e contenuto non uniforme verificati; hash nel report.
- Studio headless: selezione Cincinnus con tutte le **sei combinazioni** delle tre palette e delle due modalità. Nove campioni Auto costruiti e aggiornati, parametri caricati conservati, nessuna eccezione dell'editor.

## Evidenze

- [Prima esecuzione, errore di compilazione](validation/cincinnus-c5-r1/initial.log).
- [Dopo la prima correzione, errore conteggio pigmenti](validation/cincinnus-c5-r1/r1.log).
- [Dopo il conteggio corretto, errore campioni Auto](validation/cincinnus-c5-r1/r2.log).
- [Validazione finale riuscita](validation/cincinnus-c5-r1/r3.log).
- [Build tool autonomo](validation/cincinnus-c5-r1/preview-tool-build.log), [Studio headless](validation/cincinnus-c5-r1/studio.log), [CLI](validation/cincinnus-c5-r1/cli.log), [ricette](validation/cincinnus-c5-r1/recipes.log), [report dimensioni/hash](validation/cincinnus-c5-r1/smoke-report.json).

PNG effettivamente generati:

- [Simple crimson](validation/cincinnus-c5-r1/cincinnus-simple-crimson-768.png).
- [Adaptive ocean](validation/cincinnus-c5-r1/cincinnus-adaptive-ocean-768.png).
- [Simple moss](validation/cincinnus-c5-r1/cincinnus-simple-moss-768.png).

Il controllo tecnico dei PNG non è un'approvazione estetica o una misura di copertura completa. Il confronto con Gȧrt, il giudizio dell'utente e i benchmark ai limiti del canvas restano aperti; Crinis R10 rimane in pausa e G4 mantiene il proprio QA artistico aperto. Non viene dichiarato uno smoke desktop/DPI o multipiattaforma.

## Ripetere la validazione

PowerShell 7, dalla cartella della solution:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Tool autonomo, fuori dalla solution:

```powershell
dotnet build .\tools\GenerativeArt.CincinnusPreview -c Release
```

La validazione Codex ha usato pacchetti sotto `artifacts/nuget` e la cache locale `C:\Users\Utente\.nuget\packages` come sorgente, con audit online e telemetria Avalonia di build disattivati. Lo script è stato eseguito in un processo PowerShell con ExecutionPolicy Bypass; non vengono modificate configurazioni globali. La prova UI usa Avalonia.Headless 12.1.4 in un harness temporaneo, senza nuove dipendenze di produzione.

Consegna integrale: `GenerativeArt_M4E3_CINCINNUS_C5_R1_FIX_VALIDAZIONE_COMPLETO.zip`. README mantenuto descrittivo, con screenshot e istruzioni di installazione/esecuzione; cronologia e risultati dei test nei documenti dedicati.
