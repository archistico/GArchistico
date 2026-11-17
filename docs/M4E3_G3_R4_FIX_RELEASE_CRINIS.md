# M4E.3-G3-R4 — Coerenza Release e 32 test Crinis

## Risultato del gate Windows ricevuto

- Compilazione `net10.0` / `Release`: riuscita.
- xUnit: 1088 test, 1056 riusciti, **32 non riusciti**.
- Il log mostra ricette e renderer R3 ma un assembly `GenerativeArt.Application` che restituisce un catalogo senza `detail-style`. Questo provoca `KeyNotFoundException` nelle vecchie prove di Crinis, `ArgumentException` nelle prove micro e i fallimenti conseguenti di cancellazione e anteprima.
- Verificato direttamente il sorgente distribuito nel precedente ZIP R3: `src/GenerativeArt.Application/Catalog/GartBrushArtworkDescriptors.cs` **contiene** `detail-style`, `fiber-density`, `micro-detail`, `fiber-taper`, `fiber-shadow-depth`, `vortex-centers`, `ink-flecks`, oltre ai controlli della R2. Il report non prova che il file R3 sia stato compilato nell'assembly Release effettivamente caricato.

## Intervento

1. `tools/validate.ps1` esegue `dotnet clean GenerativeArt.sln -c Release` in modo automatico, prima del ripristino e della build Release. Il comando storico `dotnet clean` senza `-c Release` non garantiva la pulizia dell'output Release.
2. Gate rapido `CrinisReleaseCatalogContractTests`: verifica tutti i dieci controlli opzionali, il preset Studio micro senza banda opaca e il round-trip JSON; lancia anche `CrinisMicroCatalogTests` prima dei restanti test.
3. `GartBrushRenderer` usa la modalita' `classic` quando un'istanza in memoria proveniente da un vecchio catalogo e' priva della chiave `detail-style`, evitando il crash; cio' NON autorizza chiavi sconosciute nel parser JSON v1.
4. Aggiornati README, changelog, stato, roadmap e handoff. Le ricette e i preset non sono stati modificati.

## Installazione e verifica su Windows

**Non sovrapporre lo ZIP ai sorgenti gia' compilati.** Estrarre il pacchetto completo R4 in una cartella nuova e vuota, poi aprire PowerShell 7 dalla cartella `GenerativeArt` (che contiene `GenerativeArt.sln`).

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando `dotnet clean` iniziale e' compatibile con la procedura abituale, ma e' lo script ad eseguire ora la pulizia **Release** necessaria. Le fasi attese sono: clean Release, restore, build Release, test mirati Crinis/Application e suite completa.

Se il gate del catalogo fallisce di nuovo, eseguire su Windows:

```powershell
dotnet clean .\GenerativeArt.sln -c Release
dotnet build .\GenerativeArt.sln -c Release
dotnet test .\tests\GenerativeArt.Application.Tests\GenerativeArt.Application.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~CrinisReleaseCatalogContractTests" --logger "console;verbosity=normal"
```

**Risultato atteso:** tutti i controlli disponibili nell'assembly Application, esempi JSON leggibili, 32 regressioni risolte senza disabilitare i test. I risultati effettivi della R4 saranno confermati dal prossimo gate Windows; non vanno confusi con il risultato R3.

## Stato

- 27 generatori, 27 preset embedded, 146 ricette JSON: invariati.
- 17 preset personali originali: invariati.
- Nessun nuovo generatore, nessuna modifica richiesta ai vecchi rendering.
- Prossima attivita': verifica R4 e confronto visivo con Cincinnus, poi G4 Ocellus/Rete.
