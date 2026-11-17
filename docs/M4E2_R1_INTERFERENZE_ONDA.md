# M4E.2-R1 — Fase A3: Interferenze d'onda / Moiré

**Data:** 17 novembre 2026. **Base:** M4E.1-R5.

## Obiettivo e implementazione

Primo **nuovo generatore** della Fase A3, senza modificare né i 17 generatori preesistenti né i 17 JSON personali.

- `wave-interference`, versione algoritmo 1, raster statico: sovrapposizione di 2–12 sorgenti radiali con fase seeded, disposizione anulare/lineare/casuale e attenuazione esponenziale.
- Nucleo matematico puro `WaveInterferenceField2D` (Algorithms) normalizzato sul lato minore dell'immagine; valori di uscita garantiti in [0,1]. Modalità: `interference`, `caustics` e `ribbons`.
- Opzioni: frequenza, distanza, deformazione Simplex seeded, attenuazione, contrasto, gamma, tre palette e quattro colori impostabili singolarmente (Auto o HEX/alpha).
- Renderer `WaveInterferenceRenderer` (Rendering.Skia), integrato nel catalogo Application e nel runtime condiviso Studio/CLI. Supporta finiture esistenti, `fine-2x`, cancellazione e PNG. Non introduce dipendenze.
- **18° preset incorporato separato** in `Defaults/wave-interference.json`; i 17 preset forniti dall'utente restano identici. **Cinque nuove ricette Full HD**, ora 96 esempi totali.
- Preflight `MaximumWaveSamples = 150_000_000` contributi radiali (pixel nativi × sorgenti), oltre ai budget generali già presenti. Nessuna promessa di 8K in questa revisione.

## Prove aggiunte

- Algorithms: riproducibilità e effetto del seed per tutte le modalità/layout; limiti [0,1], efficacia frequenza/geometria, parametri invalidi.
- Application: caricamento e roundtrip JSON dei cinque esempi Full HD; default incorporato nuovo e preset personale `noise-flow` invariato; catalogo a 18 elementi.
- Rendering.Skia: PNG decodificabile a 128×96, seed, parametri, palette, colori, roundtrip, `fine-2x` con carta, cancellazione e budget lavoro.

**Verifica eseguita nel pacchetto:** integrità degli allegati personali, JSON e manifesto. **Test Windows:** la baseline confermata rimane M4E.1-R3; R4, R5 e questa R1 non sono ancora stati validati dall'utente.

## Come provare su Windows

Da PowerShell 7, **nella cartella che contiene `GenerativeArt.sln`**, eseguire:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Il comando pulisce e avvia la validazione completa. A test conclusi, aprire lo Studio:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Selezionare **Interferenze d'onda · Moiré**, quindi confrontare `ring`, `line`, `scatter` e i tre stili. Usare **Apri ricetta** per caricare uno dei JSON in `examples/recipes/m4e2-r1/`. L'anteprima deve usare sempre le dimensioni finali, indipendentemente da Adatta/100%. Per confrontare l'esportazione usare un nome PNG di destinazione non esistente.

## Prossimi passi

**A1:** isolare ancora controller e lifecycle Avalonia; **A3:** più algoritmi e validazioni artistiche; **A4:** budget per renderer, scalabilità 8K (7680×4320) e qualità, eventualmente tile/bande per rendere sostenibile il raster. Solo successivamente **B** compositore universale, **D** workflow artista e animazioni in fase differita.
