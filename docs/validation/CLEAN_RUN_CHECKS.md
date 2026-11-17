# Verifica degli script clean e run

Gli script nella cartella principale richiedono PowerShell 5.1 o successivo. Il codice applicativo, i generatori e le ricette non cambiano.

## clean.ps1

- Eseguito `-WhatIf` sul repository reale: ricerca completata senza cancellazioni.
- Su una fixture isolata nel workspace: eliminate cinque cartelle `bin`/`obj`, incluse sottocartelle e percorsi con spazi. Il file sorgente sentinella e le cache NuGet sono rimasti presenti.
- Esecuzione ripetuta sulla fixture: successo con zero cartelle da eliminare.
- Una junction verso un albero esterno alla fixture viene ignorata. Una junction annidata dentro `bin` interrompe la pulizia con exit code 1, conservando il file sentinella della destinazione. I collegamenti di prova sono stati rimossi dopo il controllo.
- Prima di ogni cancellazione ricorsiva vengono controllati percorso assoluto, appartenenza alla cartella del progetto e nome `bin`/`obj`.

La pulizia effettiva del repository non è stata avviata durante la verifica; le cancellazioni reali riguardano solo file creati per la fixture.

## run.ps1

Verificato con un comando `dotnet` simulato, senza aprire finestre GUI:

- Progetto Studio individuato tramite percorso assoluto, anche chiamando lo script da un'altra directory.
- Release e compilazione predefinite; Debug e `-NoBuild` inoltrati correttamente.
- Directory corrente e variabile di telemetria ripristinate al termine.
- Exit code di `dotnet` propagato, incluso un errore simulato con codice 17.

Questi controlli riguardano i wrapper PowerShell; la suite applicativa non è stata rieseguita per questo aggiornamento.
