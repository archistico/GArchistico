> **Nota di precedenza (17/11/2026):** documento originario con proposte e baseline storica. La versione compilata/testata corrente è G4-R2, con sei progetti di produzione e quattro di test. [Stato corrente](CURRENT_STATE.md) e [decisioni consolidate](DECISIONI_CONSOLIDATE_CHAT_2026-11-17.md) prevalgono sulle ipotesi superate.

# Architettura corrente — GenerativeArt M4C.2-R1

**16 novembre 2026.** Questo documento descrive i **riferimenti reali tra i progetti `.csproj`**. La baseline **M4A.4-R1 è compilata e testata su Windows** (utente); M4A.4-R2/R3, M4C.1-R1 e M4C.2-R1 hanno gate Windows aperto. Le vecchie proposte P2 sono storiche.

## 1. Solution — 10 progetti

| Progetto | Responsabilità |
| --- | --- |
| `GenerativeArt.Core` | Tipi geometrici, colori, RNG, `PixelBuffer` e contratti fondamentali |
| `GenerativeArt.Algorithms` | Noise, flussi, Marching Squares, SDF 2D, Poisson disk e Voronoi |
| `GenerativeArt.Application` | Catalogo, versioni, ricette JSON v1, interfaccia renderer, preferenze, preview policy, file atomici |
| `GenerativeArt.Rendering.Skia` | `ArtworkRenderEngine`, renderer per artwork, superfici native, output PNG, finitura grana/Bayer e carta/pigmenti alpha-safe |
| `GenerativeArt.Cli` | Host CLI e batch di frame PNG |
| `GenerativeArt.Studio` | Desktop Avalonia 12.x, gestione UI, preview/viewport e playback |
| `GenerativeArt.Core.Tests` | Test Core |
| `GenerativeArt.Algorithms.Tests` | Test matematici e di campionamento |
| `GenerativeArt.Application.Tests` | Contratti, catalogo, JSON, UI policy non grafica, file |
| `GenerativeArt.Rendering.Skia.Tests` | Test raster, encoder, runtime e CLI output writer |

## 2. Dipendenze reali

```text
GenerativeArt.Core
  ├── GenerativeArt.Algorithms
  └── GenerativeArt.Application

GenerativeArt.Rendering.Skia → Core + Algorithms + Application
GenerativeArt.Cli            → Core + Algorithms + Application + Rendering.Skia
GenerativeArt.Studio         → Application + Rendering.Skia (Core transitivo)
```

- `Algorithms` e `Application` sono indipendenti da SkiaSharp e Avalonia.
- `IArtworkRasterRenderer` è definita in Application; `ArtworkRenderEngine` è l'implementazione Skia comune a CLI e Studio. **Un solo renderer per artwork**, non una copia per la GUI.
- `BuiltInArtworkCatalog` contiene **9 descrittori** nella M4A.3-R1: `parametric-curves`, `noise-flow`, `spaced-streamlines`, `looping-flow`, `plasma-noise`, `topographic-contours`, `sdf-orbits`, `poisson-stippling`, `voronoi-mosaic`. L'ordine mostrato da `ArtworkRegistry` è alfabetico; gli ID/versioni preesistenti non cambiano.
- `VoronoiTessellation` è in Algorithms e non dipende da Skia. `VoronoiMosaicRenderer` lo usa per il nuovo artwork. `SkiaRasterSurface.DrawPolygon` usa `SKPathBuilder` ed evita API obsolete.
- L'interfaccia è scritta attualmente in C# code-only; **Avalonia XAML/AXAML è consentito**, senza vincolo a riscrivere l'interfaccia esistente.

## 3. Ricette, rendering e gestione file

- Ricette JSON v1: `formatVersion`, `artworkId`, `algorithmVersion`, `seed` UInt64 come stringa decimale, `width`, `height`, `parameters`; `animation` facoltativa per opere animate. Il parser controlla versioni, tipi, proprietà duplicate/sconosciute e dimensione del file (64 KiB).
- `ArtworkRenderEngine.Validate` impone un tetto globale di **4096 pixel per lato** e **16.777.216 pixel**, più limiti specifici. `topographic-contours` usa anche 40 milioni di celle×livelli; `plasma-noise` usa il budget globale dalla M4C.2-R3; `looping-flow` ha limiti di singolo frame e cumulativi.
- **R3:** `render-quality` è un parametro `Choice` opzionale, aggiunto ai nove descrittori; `RasterQualityPolicy` limita a 16.777.216 i pixel temporanei del campionamento fine 2×. `SkiaRasterSurface` esegue scaling/riduzione premoltiplicata; Plasma e SDF producono direttamente un buffer per-pixel al passo 1/2. `standard` non modifica il vecchio rendering.
- **M4C.2:** il post-processore comune riceve `ArtisticFinishOptions`: carta e pigmenti su raster finale con alpha invariato; `MaterialPostProcessor` usa rumore spaziale seeded e al massimo tre righe di originali per l’assorbimento. Dalla R3 il limite finitura in standard è 16.777.216 pixel; a effetti spenti la via PNG originale viene mantenuta.
- `StudioPreviewPolicy` abbassa **soltanto l'anteprima**, con default **800×600 / 480.000 pixel** e limiti raster personalizzabili dalla R3. `StudioViewportState.FitImage` calcola zoom proporzionale con margini, `PreviewViewport` mantiene auto-fit dopo render e resize salvo zoom manuale; il riquadro ha limiti in DIP configurabili. Export e ricette mantengono la risoluzione richiesta, se ammessa dal renderer.
- Il rendering di un PNG possiede una superficie Skia per richiesta. La CLI emette le sequenze di fotogrammi uno alla volta, senza accumularle tutte in RAM. La GUI espone Play/Pausa/Scrub `looping-flow`; il suo export produce un solo PNG (il batch è CLI).
- Lo Studio serializza i job di rendering; in editing usa debounce, preview cancellabili e, durante playback, salta tick che arrivano mentre il renderer è occupato.
- Il writer locale atomico mantiene il vecchio file in caso di errore/annullamento prima della pubblicazione. Provider virtuali senza semantica file-system locale **non garantiscono atomicità**. La CLI rifiuta di sovrascrivere file PNG già esistenti.
- Preferiti, ricette recenti e tema sono salvati separatamente dalle ricette, in impostazioni JSON locali versionate.

## 4. Rischi attuali e confini

- La M4A.4-R1 ha build e test validati su Windows; **M4A.4-R2/R3, M4C.1 e M4C.2 sono ancora da validare**. Il sistema colori per ruolo usa `ColorOverride` in catalogo, `ArtworkInk.Resolve` nel renderer e parametri opzionali in JSON per le vecchie ricette. Il modello di dipendenze rimane invariato.
- Il clip Voronoi per semipiani cresce quadraticamente con il numero di siti; la versione corrente è limitata per scelta. Non chiamarlo Delaunay/Fortune.
- L'ammissione teorica di un PNG 4000×4000 in `Validate` non equivale a prova di memoria/tempo o qualità artistica del render reale.
- Non sono ancora implementati GIF/APNG/video, SVG/PDF, motore Sweeper o metadati PNG per ricostruire automaticamente le ricette.
- Non sono documentati gate Linux/macOS né benchmark di memoria per export poster ripetuti e playback lungo.

**Prossimo lavoro:** M4C.3 compositing artistico a livelli/maschere, quindi M4B (domain warping e animazioni cicliche); gate Windows dell.intero ramo recente da completare appena possibile, seguendo `CURRENT_STATE.md`, `HANDOFF.md` e la roadmap unica.
