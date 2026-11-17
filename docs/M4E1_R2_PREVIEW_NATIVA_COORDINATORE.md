# M4E.1-R2 — A1 coordinatore dei render e A2 anteprima a pixel reali

**Data:** 17 novembre 2026. **Base:** M4E.1-R1. **Stato:** implementato nel codice, gate Windows aperto.

## Scopo e comportamento utente

A partire da questa revisione la finestra Studio genera l'anteprima **con la stessa ricetta e le stesse dimensioni dell'export**: per un lavoro da 2500×2000 vengono calcolati proprio 5.000.000 pixel, non un surrogato 800×600. Si applicano gli stessi seed, colori, eventuale frame, qualità e finiture. Non è più presente il doppio campo «Raster max larghezza/altezza» né il pulsante «Raster = output», perché la scelta è sempre automatica e non disattivabile.

I due campi «Riquadro max larghezza/altezza DIP» continuano a controllare **solo lo spazio occupato nella GUI**. `Adatta` applica uno zoom proporzionale per mostrare l'immagine intera; `100%` mostra un pixel del bitmap per unità logica di visualizzazione (la corrispondenza fisica con pixel del monitor dipende dal DPI di sistema). Rotella e trascinamento conservano il comportamento precedente. La barra di stato mostra le dimensioni del raster effettivamente calcolato.

Se il renderer rifiuta una dimensione per budget, il messaggio viene mostrato come **errore**, senza riduzioni silenziose. Il massimo rimane **4096×4096, 16.777.216 pixel** statici e `fine-2x` resta soggetto al suo budget separato: **questa revisione non implementa ancora l'8K**. Il prezzo della fedeltà è maggiore tempo di calcolo e memoria per le anteprime molto grandi; nel prossimo A4 andrà introdotta una strategia esplicita e verificata per i renderer che la richiedono.

## Architettura: StudioRenderCoordinator

Nuovo `GenerativeArt.Application.Rendering.StudioRenderCoordinator`, agnostico rispetto ad Avalonia:

- Serializza tutte le chiamate anteprima/export a `IArtworkRasterRenderer` tramite un unico `SemaphoreSlim`.
- Esegue il lavoro sincrono del renderer sul ThreadPool, mantenendo libero il thread UI; `WaitAsync` e render rispettano il token.
- `RenderPngAsync` restituisce un singolo PNG per l'anteprima.
- `RenderToStreamAsync` lavora su stream del chiamante senza chiuderlo.
- `RenderToAtomicFileAsync` usa `AtomicOutputFiles` e scrive su file temporaneo con pubblicazione atomica (non crea un array PNG aggiuntivo per l'export locale).

`StudioWindow.Preview.cs` gestisce ancora revisioni e debounce da 140 ms, cancella i lavori obsoleti e rimpiazza/disposa il `Bitmap` precedente. `StudioWindow.FileOperations.cs` resta proprietario del file picker e dei messaggi di stato, ma non gestisce più il semaforo né il rendering. Anche mentre il dialogo di export è aperto, nuove anteprime vengono differite invece che avviate e poi accodate. `StudioWindow` non ha più `_renderGate`.

`StudioPreviewPolicy.CreateExactPreview` valida e restituisce la **stessa istanza** della ricetta originale, impedendo trasformazioni incidentali di risoluzione e parametri. La vecchia `CreatePreview` resta disponibile come API per strumenti/test storici, **ma non è più chiamata dallo Studio**.

## Regressioni introdotte

- `StudioPreviewPolicyTests` — 1920×1080, 2500×2000, 4096×4096: stessa istanza, dimensioni, seed, parametri e animazione invariati.
- `StudioRenderCoordinatorTests` — byte dell'anteprima, niente chiamate concorrenti, cancellazione in coda, export atomico interrotto senza sovrascrivere il file preesistente.
- `StudioExactPreviewRenderingTests` — reale rendering Skia per Plasma e Voronoi: byte PNG identici tra anteprima ed export, dimensioni bitmap decodificate corrette.

Il nuovo comportamento non richiede modifica alle ricette JSON v1, agli ID/versioni algoritmi, né ai 17 default embedded. Non sono state modificate primitive di rasterizzazione o algoritmi: le golden image esistenti devono restare uguali.

## Limiti dichiarati e prossimi incrementi

**A1 resta aperta:** il coordinatore è un componente headless, ma scheduler delle revisioni, UI state, file picker ed eventi sono ancora nelle partial di `StudioWindow`. Nel prossimo refactoring isolare `PreviewController`/`ExportController` e aumentare i test del lifecycle del dispatcher. Il coordinatore è il primo passo per rendere questi controller indipendenti.

**A2 percorso nativo implementato, QA ancora aperto:** validare su Windows l'anteprima reale e il confronto pixel/alpha fra output PNG e bitmap presentato; verificare `Adatta`, `100%`, rotella, DPI alto, cambio rapido seed, errori e cancellazione. Il bitmap di anteprima è decodificato in RAM e l'export fa un secondo rendering; il riuso del raster è una possibile ottimizzazione futura solo con corretta invalidazione di ricetta/seed/fotogramma.

**A3/A4:** sviluppare nuovi generatori/ricette, poi l'8K (7680×4320) con budget di memoria per ogni renderer e filtri/AA verificati. **B:** compositore universale dopo Fase A. **D:** workflow dell'artista dopo B. **Animazioni nuove rinviate.**

## Comandi Windows — PowerShell 7

Dalla cartella radice `GenerativeArt`:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Avvio GUI:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Verifica manuale consigliata: aprire un lavoro 1920×1080, verificare nello stato «Anteprima reale: 1920 × 1080 px», usare `Adatta` e poi `100%`, infine esportare e confrontare. Provare anche 2500×2000 con grana; non impostare `fine-2x` oltre il suo budget.
