# M3D.2-R2 — Preferiti, ricette recenti e tema chiaro/scuro

**16 novembre 2026 · implementato, gate .NET/Windows non ancora eseguito su questa revisione.**

## Esperienza d'uso

- **Preferiti:** il pulsante `☆ Aggiungi ai preferiti` sotto il titolo attivo salva l'ID del generatore; il catalogo mostra `★` e la casella `Solo preferiti` filtra le opere. Non sono copie delle ricette: il favorito identifica la *famiglia* dell'opera e non memorizza seed o parametri.
- **Ricette recenti:** nella colonna sinistra appare l'elenco di **massimo 8 file JSON locali** aperti o salvati con successo. Clic sul nome per riaprire (con validazione JSON/renderer identica al file picker), `×` per dimenticare il percorso **senza eliminare il file**. Il nome mostra il tooltip del percorso completo. File virtuali/non locali non vengono aggiunti.
- **Temi:** nel pannello destro `ASPETTO / STUDIO`, pulsante `☀ Tema chiaro` / `☾ Tema scuro`. La modifica aggiorna i pennelli della UI già esistenti e il `RequestedThemeVariant` Fluent di Avalonia: non ricrea il viewport, non rigenera l'immagine e non cambia pan, zoom o frame. Il tema preferito è ripristinato al successivo avvio.
- **Persistenza locale:** `%LOCALAPPDATA%\GenerativeArt\studio-settings-v1.json` su Windows; posizione `LocalApplicationData` di .NET sugli altri sistemi. Non viene creato un database né modificato il formato delle ricette JSON v1.

## Contratto di persistenza

`StudioPreferences` è immutabile dall'esterno, espone metodi per tema/preferiti/recenti e limita rispettivamente a 128 ID e 8 percorsi. Normalizza percorsi assoluti e deduplica con confronto case-insensitive su Windows, case-sensitive altrove. Il file è JSON **schemaVersion 1** con `theme` (`dark`/`light`), `favorites` (array ID) e `recentRecipes` (array di percorsi locali).

`StudioPreferencesFile` legge al massimo **64 KiB**, rifiuta versioni e strutture incompatibili, scrive tramite `AtomicOutputFiles.WriteAsync` e quindi non lascia un file parziale se un salvataggio fallisce prima della pubblicazione. Le scritture concorrenti della GUI sono serializzate. Lo Studio riparte con valori predefiniti se le impostazioni sono danneggiate, mostrando un avviso persistente, senza cancellare automaticamente il file; una successiva modifica salvata volontariamente può sostituirlo. **Prima di salvare nuove preferenze dopo un avviso, copiare il vecchio JSON se serve per recuperarlo.**

I percorsi recenti rimangono solo nel dispositivo, non sono telemetria e non vengono inviati a servizi esterni. La selezione del file recente è un'azione esplicita dell'utente; un file spostato/mancante mostra errore e può essere dimenticato senza intaccare gli altri.

## Compatibilità e limiti

- Nessun nuovo linguaggio applicativo o pacchetto NuGet; codice C#/.NET 10. XAML è consentito in futuro, ma non richiesto da questo intervento.
- Non sono stati modificati `PreviewViewport`, `StudioViewportState`, motore Skia, catalogo, seed, schema `ArtworkRecipe`, CLI, export PNG e policy di preview.
- I token di colore Studio sono espliciti in `StudioThemePalette`; i controlli Fluent seguono il tema applicativo.
- Lo scroll della colonna sinistra evita che gli 8 file recenti rendano il catalogo inaccessibile su finestre più piccole.
- Non è un sistema di salvataggio automatico delle opere: una ricetta deve prima essere **salvata** dall'utente.

## Test implementati e gate Windows

`StudioPreferencesTests`: toggle preferiti/deduplicazione, recenti ordinati e limitati, roundtrip/versione, rifiuto preferenze malformate e oltre 64 KiB, mappatura palette; il resto dei test preesistenti deve rimanere verde.

Nella directory `GenerativeArt` che contiene la solution, PowerShell 7:

```powershell
clear && dotnet clean && .\tools\validate.ps1
```

Avviare la GUI:

```powershell
dotnet run --project .\src\GenerativeArt.Studio\GenerativeArt.Studio.csproj -c Release
```

Smoke da eseguire:

1. Segnare due opere come favorite, filtrare `Solo preferiti`, togliere la spunta e provare il riavvio: preferiti conservati.
2. Salvare o aprire una ricetta JSON locale, riaprirla dall'elenco recenti e verificare il seed/parametri; usare `×` e verificare che il file su disco esista ancora.
3. Passare da chiaro a scuro durante un'anteprima ingrandita/trascinata e durante playback `looping-flow`; non devono cambiare immagine, zoom/pan, frame o parametri. Riavviare e controllare il tema ripristinato.
4. Provare la colonna sinistra con diverse ricette e finestra minima. Copiare il file preferenze, corromperlo intenzionalmente, riavviare: lo Studio non deve bloccarsi e deve mostrare avviso.
5. Ripetere export PNG e cancellazione della M3D.2-R1. Verificare file esistenti preservati e nessuna crescita monotona della memoria durante 200+ frame, distinguendo RAM managed/native. Queste ultime sono **verifiche manuali da misurare**, non risultati già ottenuti.

**In questo ambiente non è presente .NET 10**: documentazione, ispezioni statiche e test del pacchetto non sostituiscono build o xUnit Windows.
