# Stato delle correzioni — G4-R2 (17/11/2026)

**G4-R2 compilata e test passati su Windows** (conferma utente). CS0117 (`Math.Hypot`/GeometryDistance) e CS0136 (conflitto `ink` in Rete) sono **risolti** e non devono essere ripresentati come regressioni correnti. Gli interventi precedenti qui sotto sono **storici**.

---

# Storico sintetico delle correzioni già risolte

Questo documento unifica i vecchi file `FIX_*.md` e le note di verifica delle milestone iniziali. Documenta anche le correzioni più recenti, distinguendo quelle confermate da quelle in attesa di gate. Per lo stato attuale usare [`CURRENT_STATE.md`](CURRENT_STATE.md).

| Fase | Problema originario | Risoluzione | Stato successivo |
| --- | --- | --- | --- |
| **M4E.3-G4-R1.1 → R1.2** | In `GartCellularRenderer.RenderRete` CS0136: conflitto fra `double ink` (copertura locale) e `uint ink` (palette) | Nomi distinti `wallCoverage` e `wallArgb`, senza alterare le espressioni; mantenuto il fix `Math.Hypot`/`GeometryDistance` della R1.1 | Fix R1.2 nei sorgenti, build/test integrali da confermare su Windows |
| M2A-R1 | `CS1673` in `PremultipliedColor32` per lambda/funzione locale che catturava l'istanza `struct` | Helper statico con alpha esplicito, test aggiunto | Build confermata nella serie M2A |
| M2A-R2 | Test round-trip sRGB falliva sulla soglia 0,04045 con tolleranza `1e-12` | Caso di soglia gestito nei test, formule standard invariate | Suite confermata verde nella serie M2A |
| M2B-R1 | SkiaSharp: `SKNativeObject.IsDisposed` non pubblico e `DrawBitmap` con overload obsoleto | Proprietà/API pubbliche e `SKSamplingOptions`, test bitmap disposed | Build/test/PNG confermati su Windows |
| M3A-R1 | `CS0019` per precedenza di `i % 3 switch` | Parentesi `(i % 3) switch` | Build/test/immagine confermati su Windows |
| **M3C.2-R1** | Test `RecipeTests.TrailingCommaAndCommentsAreRejected` richiedeva l'eccezione *esatta* `JsonException`, ma il parser sollevava la derivata `JsonReaderException` | Due `Assert.ThrowsAny<JsonException>`; nessuna modifica alle regole del parser | **Build, suite xUnit, JSON e PNG confermati dall'utente** |
| **M4C.2-R1 → R2** | CS8506 e CS8131 nella conversione HSV, tuple con zeri `int` e valori `double` nello `switch` | `0d` nei sei rami, sei nuovi test cromatici; finestra Studio massimizzata su richiesta | Fix implementato in R2; build Windows da confermare |

La presenza di un vecchio log 288/289 non indica che il problema JSON sia ancora attivo: riguarda la versione prima della R1. Per futuri problemi registrare diagnosi e test di regressione in `CHANGELOG.md`, aggiungendo una specifica separata soltanto se la complessità lo giustifica.
