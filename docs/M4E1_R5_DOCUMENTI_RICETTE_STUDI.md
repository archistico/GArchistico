# M4E.1-R5 — Documenti ricetta e studi creativi (Fase A1 + A3)

**Data:** 17 novembre 2026. **Base:** archivio completo M4E.1-R4. **Stato:** implementato; validazione Windows da eseguire.

## Scopo

Continuare la riduzione delle responsabilità di `StudioWindow` evitando modifiche al motore artistico e introdurre nuovi casi di sperimentazione per i renderer esistenti. La schermata, l'anteprima nativa, i 17 preset utente e i risultati PNG esistenti restano invariati.

## A1 — Servizio documenti ricetta

`GenerativeArt.Application/Files/StudioRecipeDocuments.cs` centralizza:

- Lettura asincrona da uno `Stream` anche non seekable, massimo **65.536 byte UTF-8**; un byte aggiuntivo basta per bloccare l'import.
- Deserializzazione rigorosa del JSON v1, seguita dalla validazione del renderer per il primo frame quando è presente animazione.
- Serializzazione canonica via `ArtworkRecipeJson`.
- Scrittura su file locale con `AtomicOutputFiles`, e scrittura su stream per i provider virtuali, sui quali l'atomicità non è garantita.
- Supporto della cancellazione durante lettura e scrittura. La proprietà degli stream resta al chiamante.

`StudioWindow.FileOperations.cs` continua a gestire soltanto la scelta dei file tramite Avalonia, la selezione dell'opera importata, i messaggi e l'export PNG, ma non implementa più la lettura e la scrittura dei documenti JSON. Il coordinatore `StudioRenderCoordinator` già gestisce preview ed export PNG. Non è stato cambiato alcun renderer.

## A3 — Dieci nuove ricette sperimentali Full HD

Le ricette sono in `examples/recipes/m4e1-r5/`. Sono documenti JSON v1 autonomi, **non** sostituiscono i 17 default embedded, usano qualità `standard` e hanno ciascuna il proprio seed. Le combinazioni mescolano valori **già presenti** in esempi dello stesso generatore, senza estendere il catalogo dei parametri.

| Ricetta | Generatore | Scopo della prova |
| --- | --- | --- |
| `sdf-chromatic-fog-1920.json` | SDF Orbits | Macchie, grana, fusione e ombre |
| `sdf-geode-glass-1920.json` | SDF Orbits | Vetro, rilievo e profondità |
| `flow-brush-silk-rain-1920.json` | Flow Brush | Densità setole e rastremazione |
| `domain-warp-silk-topaz-1920.json` | Domain Warp | Deformazione, banda e palette |
| `paper-cut-porcelain-wave-1920.json` | Paper Cut | Rilievo ritagliato e ombre |
| `strata-aurora-ridge-1920.json` | Strata Landscape | Atmosfera, strati e altezza |
| `engraving-verdigris-1920.json` | Tonal Engraving | Tratteggio e frequenza campo |
| `guilloche-moiré-1920.json` | Guilloché | Densità di curve e armoniche |
| `voronoi-ceramic-moon-1920.json` | Voronoi Mosaic | Fughe e trattamento materico |
| `lic-curl-bronze-1920.json` | LIC Flow | Campi, contrasto e lunghezza |

**Gate visuale ancora aperto:** prima dell'uso in produzione renderizzare i dieci esempi e controllare leggibilità, copertura/alpha, eventuali tempi eccessivi e confronto dei risultati artistici. La sola correttezza sintattica del JSON non garantisce qualità visiva.

## Nuove regressioni

- `StudioRecipeDocumentsTests`: roundtrip e proprietà degli stream; documento oltre limite senza seek; JSON malformato o vuoto; validazione renderer preventiva; cancellazione lettura; file locale atomico e caricamento; cancellazione salvataggio che conserva la versione esistente.
- `ExperimentalRecipeFixturesTests`: presenza di dieci esempi copiati nell'output test, parsing JSON con catalogo, risoluzione, seed diversi, roundtrip e generatori interessati.

**Nessuna modifica alle ricette storiche**, ai 17 preset, al formato JSON v1, ai 17 generatori e ai limiti del renderer. L'8K resta **A4**, non implementato. A1 rimane aperta per isolamento del lifecycle e dei controller UI. A0 (anteprime mancanti in archivio storico) rimane aperta. B (compositore) e D (workflow artista) seguono A; animazioni avanzate rimandate.

## Verifica Windows (PowerShell 7)

Da `GenerativeArt/`, eseguire:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Per uno smoke manuale dello Studio:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Aprire una ricetta da `examples/recipes/m4e1-r5/`; verificare `Adatta`, `100%` e l'identità tra anteprima nativa ed export. Verificare l'apertura/salvataggio e il comportamento informativo dei parametri inattivi introdotto in R4.
