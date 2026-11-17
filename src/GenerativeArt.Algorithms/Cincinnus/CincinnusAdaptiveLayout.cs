using GenerativeArt.Algorithms.Flow;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// Risultato C4 puramente geometrico, ordinato in livelli pittorici.
/// Patch prima di filler e main (dal fondo verso il primo piano), come Gȧrt.
/// </summary>
public sealed class CincinnusAdaptiveLayout
{
    internal CincinnusAdaptiveLayout(IVectorField2D field, CincinnusBrushStroke[] main,
        CincinnusBrushStroke[] filler, CincinnusBrushStroke[] patches,
        int placementAttempts, int patchAttempts, int bareBefore, int bareAfter,
        int coverageCells, int usedSpinePoints, bool safetyLimitReached)
    {
        Field = field;
        Main = Array.AsReadOnly(main);
        Filler = Array.AsReadOnly(filler);
        Patches = Array.AsReadOnly(patches);
        StrokesInPaintOrder = Array.AsReadOnly(patches.Concat(filler).Concat(main).ToArray());
        PlacementAttempts = placementAttempts;
        PatchAttempts = patchAttempts;
        UncoveredCellsBeforePatches = bareBefore;
        UncoveredCellsAfterPatches = bareAfter;
        CoverageCells = coverageCells;
        SpinePointCount = usedSpinePoints;
        SafetyLimitReached = safetyLimitReached;
    }

    public IVectorField2D Field { get; }
    public IReadOnlyList<CincinnusBrushStroke> Main { get; }
    public IReadOnlyList<CincinnusBrushStroke> Filler { get; }
    public IReadOnlyList<CincinnusBrushStroke> Patches { get; }
    public IReadOnlyList<CincinnusBrushStroke> StrokesInPaintOrder { get; }
    public int PlacementAttempts { get; }
    public int PatchAttempts { get; }
    public int UncoveredCellsBeforePatches { get; }
    public int UncoveredCellsAfterPatches { get; }
    public int CoverageCells { get; }
    public int SpinePointCount { get; }
    public bool SafetyLimitReached { get; }
}
