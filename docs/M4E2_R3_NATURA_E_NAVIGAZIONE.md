# M4E.2-R3 — Nuovi algoritmi naturali e navigazione uniforme

**17 novembre 2026 · Base: M4E.2-R2.** Fase A3, con miglioramento mirato allo Studio.

## Risultato

- **20 generatori**: aggiunti `phyllotaxis` e `reaction-diffusion`, entrambi raster, statici, seeded, JSON v1 e disponibili in Studio e CLI.
- **108 esempi JSON**: otto nuovi studi Full HD in `examples/recipes/m4e2-r3/`, quattro per generatore. I 100 precedenti sono invariati.
- **20 preset embedded**: conservati gli originali 18 (di cui 17 personali) e aggiunti `Defaults/phyllotaxis.json` e `Defaults/reaction-diffusion.json`.
- **Menu sinistro**: schede generatori a larghezza di colonna omogenea (`Stretch`); le ricette recenti hanno un pulsante di apertura esteso a tutta la riga, con comando separato di rimozione sovrapposto a destra. Titoli lunghi con ellissi, tooltip sui percorsi recenti. Si conservano ordinamento, filtro, preferiti e tema.

## Algoritmo 19: Fillotassi

- `PhyllotaxisPattern` in Algorithms genera coordinate senza memorizzare la lista completa: legge numero di semi, angolo di divergenza, irregolarità, numero di bracci, distribuzione botanica/rosetta/galassia.
- `PhyllotaxisRenderer` in Skia disegna dischi, anelli o pennellate con antialiasing, opacità e ruoli cromatici indipendenti (`background-color`, `seed-color-1..3`).
- Tre palette: botanic, petals e luminous. I 17 preset storici non sono alterati.
- Budget massimo di 16.000 semi da catalogo; qualità `fine-2x` e finiture riusano le policy globali.

## Algoritmo 20: Reazione–diffusione Gray–Scott

- `ReactionDiffusionField2D` in Algorithms: due specie U e V, inizializzazione riproducibile da seed, laplaciano a nove punti, condizioni periodiche, integrazione esplicita. Quattro array di `double` della griglia, riutilizzati ad ogni iterazione.
- `ReactionDiffusionRenderer` calcola la simulazione su griglia controllata da `cell-size` e campiona bilinearmente per il raster finale (anche 2×). Modalità: pigment, contours, islands. Palette: tidal, copper, lichen e quattro ruoli colore Auto/HEX/alpha.
- Budget **≤350.000 celle**, **≤75 milioni di aggiornamenti** per render; verifica prima dell'allocazione di Skia e della simulazione. Cancellazione ad ogni iterazione e riga d'output.
- I parametri chimici sono una simulazione visiva discreta, non una previsione fisica quantitativa. La griglia periodica evita discontinuità di bordo nella simulazione; la generazione può produrre tessiture organiche o cellule, secondo feed, kill e diffusività.

## Ricette Full HD aggiunte

| File | Algoritmo | Obiettivo |
| --- | --- | --- |
| `phyllotaxis-sunflower-gold-1920.json` | Fillotassi | distribuzione aurea densa |
| `phyllotaxis-rose-petals-1920.json` | Fillotassi | rosetta, petali e carta |
| `phyllotaxis-luminous-galaxy-1920.json` | Fillotassi | galassia ad anelli luminosi |
| `phyllotaxis-organic-corolla-1920.json` | Fillotassi | corolla irregolare con grana |
| `reaction-diffusion-tidal-1920.json` | Reazione–diffusione | depositi di pigmento acquatico |
| `reaction-diffusion-copper-coral-1920.json` | Reazione–diffusione | cellule e coralli ramati |
| `reaction-diffusion-lichen-cells-1920.json` | Reazione–diffusione | licheni su carta ruvida |
| `reaction-diffusion-isolines-1920.json` | Reazione–diffusione | bande isocromatiche chimiche |

## Qualità e limiti

I due nuovi renderer usano il budget standard massimo 4096×4096 (e quello più restrittivo del 2×), **non** il profilo 8K sperimentale della R2. Il profilo 8K precedente resta valido sui soli quattro generatori geometrici supportati. Finiture e post-processing mantengono i limiti esistenti. Lo Studio continua a calcolare l'anteprima alla **stessa risoluzione dell'export**: l'adattamento modifica solo la vista.

## Test aggiunti e controlli di accettazione

- Algorithms: seed, determinismo, coordinate finite, tre disposizioni fillotattiche, reazione–diffusione deterministica, concentrazioni limitate, rifiuto del sovraccarico.
- Application: descrittori, embedded defaults, otto JSON completi, serializzazione, parametri condizionali, conservazione dei preset personali.
- Rendering.Skia: PNG, dimensioni, seed, cambiamento di parametri e colori, fine-2x, materia, cancellazione, rifiuto delle simulazioni troppo costose senza output parziale.
- Studio manuale: ridimensionare la finestra e scorrere il menu: ogni scheda algoritmo e ogni apertura ricetta recente deve occupare la stessa larghezza. Verificare che il comando × cancelli solo la cronologia. Testare i filtri e lo switch tema.
- Generare uno dei nuovi JSON, verificare `Adatta`, `100%` e che l'export coincida con l'anteprima a ricetta/seed uguali.

### Comandi Windows (PowerShell 7)

Nella cartella principale che contiene `GenerativeArt.sln`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Se la suite riesce:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

CLI, per una prova diretta:

```powershell
dotnet run --project .\src\GenerativeArt.Cli\GenerativeArt.Cli.csproj -c Release -- render-recipe .\examples\recipes\m4e2-r3\phyllotaxis-rose-petals-1920.json .\prova-fillotassi.png
```

**Stato validazione:** controlli statici/struttura/JSON/manifesto; suite Windows nuova da eseguire. Ultima baseline utente confermata: M4E.1-R3. Non dichiarare già testata la M4E.2-R3.

## Roadmap prossima

Fase A4: estendere il rendering con budget e ottimizzazioni dei buffer fino all'8K; Fase A1: completare refactoring dei controller; A5: QA su memoria/renderer e integrità asset. Successivamente B compositore universale, D workflow dell'artista; animazioni avanzate rimandate.
