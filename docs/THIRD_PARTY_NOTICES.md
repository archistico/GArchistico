# Attribuzioni e licenze di terze parti

## Gȧrt

- Progetto: https://github.com/igr/gart
- Autore: Igor Spasić; licenza BSD-2-Clause; testo incluso in `licenses/GART_BSD2.txt`.
- I sorgenti `Perlin.kt`, `PerlinPermTable.kt`, le formule colore e altri componenti originali sono serviti da riferimento per il Core; conservarne le attribuzioni prima della pubblicazione. Gli harness Kotlin/Python non appartengono al repository C#.

## SkiaSharp

- `SkiaSharp` 4.153.2 e `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.153.2: pacchetti .NET per il motore grafico Skia, progetto https://github.com/mono/SkiaSharp, licenza MIT per i binding (consultare anche gli avvisi di copyright e licenze delle dipendenze native prima del rilascio).
- Versioni centralizzate in `Directory.Packages.props`; per le versioni future verificare release notes e condizioni di ridistribuzione delle librerie native.
- Release notes https://mono.github.io/SkiaSharp/docs/releases/index.html

## Riferimenti M4D.1

Guilloché usa un'implementazione C# autonoma delle equazioni di ipotrocoidi/epitrocoidi e delle armoniche radiali. `TonalHatching` è un filtro analitico autonomo di tratti periodici e copertura tonale; per il contesto scientifico vedere gli autori di [Real-Time Hatching](https://gfx.cs.princeton.edu/proj/hatching/). Non sono importati codice, immagini o tonal art maps del paper. Le nuove ricette contengono soltanto parametri del framework.

## Test

xUnit, runner Visual Studio, Microsoft.NET.Test.Sdk e coverlet sono dipendenze di sviluppo e restano versionati in `Directory.Packages.props`.

Il tool facoltativo `tools/GenerativeArt.Smoke` usa Avalonia.Headless 12.1.4 (Avalonia Team, MIT), separato dalla solution e dal prodotto. Il progetto del tool contiene il pin della sua dipendenza.

## Riferimenti algoritmici M4B.1

Domain warping applica il concetto generale di perturbare il dominio di un campo scalare con altri campi seeded. La LIC discreta è un'implementazione C# autonoma del principio descritto da Brian Cabral e Leith Casey Leedom, [Imaging Vector Fields Using Line Integral Convolution, SIGGRAPH 1993](https://cgl.ethz.ch/teaching/scivis_common/Literature/CabralLeedom93.pdf). Nessun sorgente, testo, figura o asset del paper è incluso; il link serve come riferimento scientifico.

La licenza per il nuovo framework GenerativeArt andrà scelta formalmente prima della distribuzione pubblica.

## jdxyw/generativeart — nuova fonte analizzata

- Repository: https://github.com/jdxyw/generativeart, autore jdxyw, licenza MIT (`https://github.com/jdxyw/generativeart/blob/master/LICENSE`, copyright 2021).
- Per questa consegna è stata integrata esclusivamente **documentazione di analisi e pianificazione**. Nessun sorgente Go o asset Go è stato importato/portato nel progetto C#.
- Qualunque futuro porting che incorpori codice richiederà verifica del file sorgente specifico, attribuzione e conservazione dell’avviso di copyright/licenza MIT. Gli eventuali riferimenti a opere/artisti originali necessitano attenzione distinta.

### Fonte Go esaminata direttamente in M3B

Analizzato lo ZIP completo `jdxyw-generativeart-master.zip` (commit dell'archivio `94bc7231d62ec996d4777c6cd4429c88e3b87b7e`). Licenza originale MIT: `licenses/JDXYW_MIT.txt`. Implementazioni nuove come la griglia bilineare, il campo curl e il tracing spaziale sono realizzate in C# con API del nostro framework, basate su concetti algoritmici generali: **nessun file `.go` incluso**. Per un futuro porting fedele di specifici sorgenti mantenere tutti gli avvisi di copyright e licenza pertinenti.
