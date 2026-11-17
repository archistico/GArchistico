# QA statico — Cincinnus C2 (17/11/2026)

**Oggetto:** incremento additivo C2 rispetto alla versione C1 confermata compilata/testata su Windows dall'utente. QA statico non certifica la nuova build o l'esito dei test non eseguiti in questo ambiente.

## Verifiche richieste e limiti

- API C2 isolate in `GenerativeArt.Algorithms/Cincinnus`; nessun progetto/pacchetto NuGet aggiunto.
- Nuovi test `CincinnusSimplePlacementTests.cs` mirano a linee equidistanti, ordinamento del flusso, determinismo, campi nulli, rollback di linee corte, clearance e taper, ciclo chiuso, canvas rettangolari, sola lettura, budget e cancellazione.
- Nessuna nuova ricetta o preset: 29 generatori / 29 preset / 173 ricette. Non asserire risultato visivo.
- `hasEye` della variante Simple originale non ancora portato; differenza annotata nel documento tecnico.
- Tutti i file C# preesistenti devono restare byte-identici all'archivio C1 validato. I nuovi sorgenti C2 e i test sono l'unica modifica funzionale.
- Manifest SHA-256 ricalcolato sul contenuto distribuito con esclusione del manifest stesso; verifica file e integrità ZIP prima della consegna.

## Gate Windows

```powershell
clear && dotnet clean && .\tools\validate.ps1

dotnet test .\tests\GenerativeArt.Algorithms.Tests\GenerativeArt.Algorithms.Tests.csproj -c Release --filter "FullyQualifiedName~CincinnusSimplePlacementTests"
```

**Esito effettivo:** il nuovo codice C2 non ha ancora superato un gate build/test eseguito (SDK .NET 10 non presente qui). Conservare primo errore con file/riga se il gate fallisce. C1: conferma utente «Compila e test ok» del 17/11/2026.

## Esito del confronto sui file

- **292 file preesistenti** in `src/` e `tests/` confrontati con la C1 consegnata: **0 cambiati, 0 mancanti**.
- **3 sorgenti** C2 in `src/GenerativeArt.Algorithms/Cincinnus/`, **1 file di test** in `tests/GenerativeArt.Algorithms.Tests/`; **12 casi `[Fact]`** aggiunti.
- **173** JSON di ricetta e **29** preset embedded: tutti i **202** file superano il parsing JSON (`jq -e`).
- Nessun file `.csproj`, `.sln`, `.props`, shader, rendering, GUI, CLI o preset modificato; auto-inclusione sorgenti/test via convenzioni SDK-style.
- Non è disponibile l'SDK `dotnet` nell'ambiente: non si dichiara alcuna compilazione o test C2 riuscito.
