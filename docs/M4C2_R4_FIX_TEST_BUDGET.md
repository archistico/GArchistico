# M4C.2-R4 — Correzione test budget materia

16 novembre 2026. Il log utente riportava 513/514 test superati su R3. Codex ha riprodotto il singolo fallimento in `MaterialBudgetRejectsLargeCanvasBeforeOutput`: il test richiedeva `ArgumentOutOfRangeException` per 4000×4000 standard con pigmenti, benché R3 ammetta questa risoluzione.

## Correzione

Solo `MaterialPostProcessorTests.cs` e documentazione: nessuna modifica ai limiti o al rendering di produzione.

- Accettazione tramite `Validate`, senza allocazione raster/PNG: 2500×2000, 4000×4000 e 4096×4096 standard; 2048×2048 fine-2x al limite dei sottopixel.
- Rifiuto tramite `Render` prima della scrittura: 4097×4096 e 4096×4097 standard; 2500×2000 e 4000×4000 fine-2x. Verificati eccezione, parametro `recipe` e stream vuoto.
- L'unico test precedente diventa otto casi: il totale passa da 514 a 521.

## Validazione eseguita da Codex su Windows

SDK .NET 10.0.201. Build Release: zero avvisi e zero errori. Suite: **521 superati, zero falliti, zero ignorati** (Core 115, Algorithms 140, Application 126, Rendering.Skia 140). I 15 casi di `MaterialPostProcessorTests` passano anche isolatamente.

Eseguiti `dotnet clean GenerativeArt.sln -c Release` e `tools/validate.ps1` (restore, build Release, test completi). Log: [`validation/M4C2_R4_windows.log`](validation/M4C2_R4_windows.log).

L'ambiente Codex richiede esecuzione PowerShell con policy applicata al solo processo, pacchetti dalla cache locale in `artifacts/nuget`, `RestoreSources` locale e `NuGetAudit=false` per evitare l'errore SSL NuGet. `AVALONIA_TELEMETRY_OPTOUT=1` evita la scrittura del log telemetrico fuori dal workspace. Queste impostazioni sono state usate solo per il processo di verifica; script, dipendenze e configurazione del prodotto restano invariati. L'audit vulnerabilità NuGet online non è stato eseguito.

Sul PC utente ripetere normalmente:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

I test di accettazione dei poster controllano i budget senza renderizzare immagini grandi. Smoke GUI, qualità visiva e misure di memoria/tempo sui poster restano da eseguire come descritto in [`M4C2_R3_PREVIEW_E_BUDGET.md`](M4C2_R3_PREVIEW_E_BUDGET.md).
