# M4E.1-R4 — Parametri condizionali e isolamento editor

Data: 17 novembre 2026. Baseline confermata dall'utente: **M4E.1-R3 compila, supera i test, anteprima corretta ed export identico all'anteprima**. La R4 è una nuova implementazione, da validare separatamente.

## Motivazione

Nel campo `vortex` di Spaced Streamlines il parametro `frequency` non viene utilizzato dal renderer: il test R2 che presumeva un cambiamento del raster è stato corretto in R3. Nella R4 rendiamo esplicito all'artista quando un controllo non ha effetto, **senza modificare il disegno né il JSON**.

## Codice introdotto

- `StudioParameterApplicability` (Application, indipendente da Avalonia): per `(artworkId, parametro, valori)` restituisce `null` se il controllo è pertinente oppure un motivo testuale se non lo è.
- `StudioWindow.ParameterControls.cs`: costruzione dei controlli tipizzati separata dalla selezione della ricetta.
- `StudioWindow.ParameterApplicability.cs`: aggiornamento dinamico delle abilitazioni e dei messaggi nella colonna parametri.
- `StudioWindow.RecipeEditor.cs`: rimangono selezione opera, generazione della ricetta e gestione dei cambiamenti; nessuna duplicazione dei renderer.

## Condizioni attuali

| Generatori | Controllo | Quando non è applicabile |
| --- | --- | --- |
| Noise Flow, Streamlines, Flow Brush, LIC Flow | `frequency` | `field = vortex` |
| Noise Flow | `curvature` | `field = curl` o `vortex` |
| Flow Brush | `curvature` | `field` diverso da `braid` |
| Opere con finitura | `grain-size` | `grain-amount = 0` |
| Opere con dithering | `dither-levels` | `dither-mode = none` |
| Opere con carta | `paper-strength` | `paper-texture = none` |
| Incisione tonale | `mask-radius` e `mask-feather` | `mask = none` |

Sono **regole di presentazione**, non regole per scartare parametri in fase di salvataggio. Cambiando nuovamente la modalità, i valori numerici precedenti vengono recuperati. Se un campo numerico aveva una stringa parziale/non valida quando viene disabilitato, l'editor ripristina il valore valido già presente nella ricetta, per non lasciare un errore invisibile bloccante.

## Contratti e compatibilità

- Non cambiano renderer, colori, algoritmi, preset embedded, 81 ricette, serializzazione JSON v1, risoluzione anteprima, preview/export o limiti raster.
- I valori dei parametri inapplicabili sono **conservati** nel JSON, e la validazione degli input del parser resta invariata.
- Le dipendenze sono intenzionalmente un primo insieme di casi verificati nei renderer: in A1 si potrà allargare il catalogo senza introdurre condizioni non dimostrate.
- L'interfaccia rimane quella approvata, con una breve frase informativa sotto il controllo inattivo.

## Test aggiunti

`StudioParameterApplicabilityTests` copre frequenza/vortice e riattivazione curl (4 opere), torsione per tipi di campo, finiture/carta, maschera, preservazione dei valori JSON e immutabilità dei preset.

**Stato test:** la R3 è confermata dall'utente; R4 non ancora confermata su Windows. Verifica da PowerShell 7 nella directory della solution:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Successivamente aprire lo Studio e verificare che selezionando **Spaced Streamlines → campo vortex/curl** il controllo `frequency` venga disabilitato/riabilitato, e che i suoi valori restino invariati dopo il salvataggio e la riapertura di una ricetta. Verificare inoltre la conservazione di anteprima=export.

## Prossimi passi

Fase A1: ulteriore isolamento dei controller/dispatcher; A0: recupero anteprime storiche mancanti; A3: nuovi generatori e ricette; A4: qualità 8K con budget; A5: QA e benchmark. In seguito Fase B compositore e D workflow artista. Animazioni avanzate rimandate.
