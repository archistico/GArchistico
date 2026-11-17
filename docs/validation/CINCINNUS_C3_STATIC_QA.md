# QA statico — Cincinnus C3 (17/11/2026)

**Validazione precedente:** C1 e C2 compilate e testate con successo dall'utente su Windows, senza numero test fornito. **C3 non è stata compilata né testata con dotnet nell'ambiente di preparazione (SDK .NET assente).** Gli esiti seguenti riguardano la struttura del pacchetto, non l'esecuzione .NET o la qualità visiva.

## Contratti verificati / da verificare

- Nuovi algoritmi in C# senza dipendenze Kotlin/Go: planner e model di stroke in `Algorithms`.
- Painter C# SkiaSharp autonomo e strumento PNG standalone, senza introduzione di un artwork registrato.
- Cambiamento compatibile in C2: solo proprietà di lettura `CincinnusStreamline.Separation`, nessuna variazione dell'algoritmo di posizionamento.
- Riproducibilità: seed indipendente per piano/fibre/colori/sporco, senza RNG globale (test predisposti, da eseguire).
- Protezioni: validazioni numeriche, budget di stroke/spine, cancellazione, dimensioni preview limitate a 2048 px (test predisposti, da eseguire).
- **Non equivalenza esplicita** con rumore/GPU/OKLab originali Gȧrt; la resa artistica richiede confronto A/B visivo.
- Nessun cambiamento di renderer preesistente Crinis/G4-R2, catalogo, JSON di esempio o preset embedded.

## Gate Windows obbligatorio

```powershell
clear && dotnet clean && .\tools\validate.ps1
dotnet build .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release
dotnet run --project .\tools\GenerativeArt.CincinnusPreview\GenerativeArt.CincinnusPreview.csproj -c Release -- .\cincinnus-c3-preview.png 1 768 768
```

Il primo comando esercita i nuovi test dei progetti `Algorithms.Tests` e `Rendering.Skia.Tests` inclusi nella solution. Il secondo compila il tool preview *non* incluso nella solution; il terzo lo esegue. La PNG è un artefatto di prova da ispezionare al 100%, **non un output già verificato**.

**Prossimo passo:** gate tecnico e confronto artistico; quindi C4 Adaptive, C5 catalogo, CLI e Studio.

## Inventario e confronto statico del pacchetto

- **173** JSON in `examples/recipes` e **29** JSON preset embedded; entrambi gli insiemi sono identici al pacchetto C2, byte per byte nel controllo differenziale.
- **8 nuovi sorgenti C#**: 3 in Algorithms, 2 nel modulo Skia, 2 nei test, 1 nello strumento preview. Il tool include anche un progetto `.csproj` nuovo.
- **Un solo file `.cs` preesistente toccato**: `CincinnusSimpleLayout.cs`, per rendere accessibile la `Separation` nominale, senza alterare C2.
- `GenerativeArt.sln`, `tools/validate.ps1` e i sorgenti dei renderer storici sono rimasti invariati.
- Manifest SHA-256 rigenerato alla fine; controllo `sha256sum -c` separato dai test .NET.
