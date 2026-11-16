# Politica definitiva del repository — linguaggi e pulizia

## Regole

1. **Solo C#** per ogni sorgente di produzione, test, esempio, benchmark, CLI e GUI.
2. PowerShell ammesso soltanto per script operativi minimali di build, validazione e avvio dei run. Evitare duplicazione dei comandi `dotnet` in molti script.
3. Nessun Python, Kotlin, Java, Go, Bash, JavaScript, TypeScript, F#, C/C++ sorgente o SkSL custom mantenuto nel repository. File XAML/AXAML per l'interfaccia Avalonia sono ammessi; lo Studio attuale rimane code-only per scelta implementativa, non per divieto. Le librerie NuGet native sono dipendenze precompilate e richiedono controllo licenze.
4. Ammessi file di soluzione/progetto/configurazione (`.sln`, `.csproj`, `.props`, `global.json`, NuGet lock), dati (`.json`), documentazione (`.md`) e workflow CI YAML, che non sono sorgenti del programma.
5. La caratterizzazione storica del repository Gȧrt rimane negli archivi di audit separati. Nel prodotto restano solo attribuzioni BSD-2-Clause e fixture golden numeriche se utilizzate dai test C#.
6. Non includere output compilati, JAR, ambiente Python, toolchain Kotlin, cache o archivi di Gȧrt nel repository di prodotto.

## Pulizia della baseline in questa consegna

- Eliminati dalla **copia di consegna**, non dalla sorgente originale caricata, i generatori Python, i harness Kotlin, lo script Bash e i validatori Python.
- Conservate le fixture JSON numeriche come dati non eseguibili, ridenominando `seeded_perlin3d_python_golden.json` in `seeded_perlin3d_v1_golden.json`.
- Nessun file `.cs`, `.csproj`, `.sln`, `.props`, `.ps1` modificato durante questa consegna.
- Aggiornata la documentazione per non prescrivere più tool Kotlin/Python/Bash.

## Controllo per le prossime consegne

La validazione formale dovrà includere, implementata in C# o con controllo minimale PowerShell, una scansione delle estensioni sorgente vietate. I risultati devono figurare nel report dei test: non basta affermare che il progetto è 'solo C#'.
