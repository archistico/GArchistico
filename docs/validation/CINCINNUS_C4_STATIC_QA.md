# Cincinnus C4 — verifica statica prima della compilazione

Data: 17/11/2026. **Verifiche di integrità non equivalgono a `dotnet build/test`**.

- Baseline: ZIP completo C3, nessuna modifica ai renderer Crinis/Ocellus/Rete e alle ricette/preset.
- Sorgenti C# nuovi: 4 file Algorithms (`AdaptiveOptions`, `CoverageGrid`, `AdaptiveLayout`, `AdaptivePlacer`).
- Test xUnit nuovi: `CincinnusAdaptivePlacementTests.cs` (7 casi dichiarati; non ancora eseguiti).
- Tool preview C# aggiornato: argomento facoltativo `simple|adaptive`, default `simple`, forma precedente a quattro argomenti ancora supportata.
- Contratti verificabili in codice: seed/versione PRNG locale, limiti, cancellazione, input NaN, coerenza cover a mark unico e secondo passaggio patch.
- Avvertenze: SDK .NET non disponibile qui, quindi compilazione, analisi statica del compilatore, test xUnit e generazione reale PNG **non eseguite**.
- Esito integrità JSON e controllo SHA-256 del pacchetto registrati nella consegna ZIP.

## Gate da eseguire su Windows

```powershell
clear && dotnet clean && .\tools\validate.ps1
dotnet build .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release
```

Il primo fallimento va diagnosticato e corretto senza considerare automatici i passaggi successivi.
