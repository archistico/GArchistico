> **Aggiornamento successivo, 17/11/2026:** dopo questa verifica statica, l’utente ha confermato **build e test Windows G4-R2 superati**. Il testo originale rimane come evidenza della fase di controllo precedente; la prova visuale artistica resta aperta. [Stato corrente](../CURRENT_STATE.md).

# Verifica statica — M4E.3-G4-R2 (17 novembre 2026)

**Baseline Windows confermata dall'utente:** G4-R1.2, compilazione e test passati.

Questa verifica statica R2 è stata eseguita sui file della revisione consegnata; **non** sostituisce build o test .NET.

- 173 esempi JSON sintatticamente validi (`jq empty`), inclusi 6 nuovi in `examples/recipes/m4e3-g4-r2`.
- 167 file JSON delle ricette preesistenti confrontati **byte-per-byte** con lo ZIP G4-R1.2: tutti identici.
- 27 preset embedded non appartenenti a Ocellus/Rete confrontati **byte-per-byte** con G4-R1.2: tutti identici.
- I due preset di avvio G4 attivano intenzionalmente gli effetti R2, senza cambiare seed, dimensioni, palette o parametri preesistenti.
- Test di catalogo, geometria, rendering e preview/esportazione aggiornati; compilazione e suite .NET **non eseguite in questa sessione**.
- Prima di considerare la R2 validata: PowerShell 7 nella root, `clear && dotnet clean && .\tools\validate.ps1`, poi confronto visuale A/B a Full HD.

Vedi [`../M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md`](../M4E3_G4_R2_RILIEVO_MICROFORI_PELLICOLE.md).
