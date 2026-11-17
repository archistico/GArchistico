namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// Posizionamento C4 ispirato a Cincinnus.kt (Gȧrt): 60 main, 1000 tentativi,
/// 300 patch, griglia da 20 px. Le misure sono pixel logici del canvas.
/// Per preview ridotte applicare uno scale proporzionale, non un SS raster.
/// </summary>
public sealed record CincinnusAdaptiveOptions
{
    public int MainStrokeCount { get; init; } = 60;
    public int MaximumPlacementAttempts { get; init; } = 1_000;
    public int MaximumPatchAttempts { get; init; } = 300;
    public float CoverageCell { get; init; } = 20f;
    public float StepSize { get; init; } = 3f;
    public float MaximumTraceLength { get; init; } = 9_000f;
    public float Margin { get; init; } = 500f;
    public float MinimumRadius { get; init; } = 25f;
    public float MinimumAverageHalfWidth { get; init; } = 30f;
    public float MaximumHalfWidth { get; init; } = 120f;
    public float HalfWidthFactor { get; init; } = .4f;
    public float MaximumStrokeLength { get; init; } = 1_600f;
    public float MinimumStrokeShare { get; init; } = .5f;
    public float DryExponent { get; init; } = 4f;
    public float HookRadiusFactor { get; init; } = 1f;
    public int MaximumCoverageCells { get; init; } = 150_000;
    public int MaximumTotalStrokes { get; init; } = 1_400;
    public int MaximumSpinePoints { get; init; } = 400_000;

    internal void Validate(int width, int height)
    {
        if (width is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(width));
        if (height is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(height));
        if (MainStrokeCount is < 1 or > 300) throw new ArgumentOutOfRangeException(nameof(MainStrokeCount));
        if (MaximumPlacementAttempts is < 1 or > 20_000) throw new ArgumentOutOfRangeException(nameof(MaximumPlacementAttempts));
        if (MaximumPatchAttempts is < 0 or > 20_000) throw new ArgumentOutOfRangeException(nameof(MaximumPatchAttempts));
        Positive(CoverageCell, nameof(CoverageCell));
        Positive(StepSize, nameof(StepSize));
        if (StepSize < .25f) throw new ArgumentOutOfRangeException(nameof(StepSize));
        Positive(MaximumTraceLength, nameof(MaximumTraceLength));
        if (MaximumTraceLength < 40 * StepSize) throw new ArgumentOutOfRangeException(nameof(MaximumTraceLength));
        if (MaximumTraceLength / StepSize > CincinnusPathGeometry.MaximumStepsPerDirection)
            throw new ArgumentOutOfRangeException(nameof(MaximumTraceLength), "Troppi passi RK4 per traccia.");
        if (!float.IsFinite(Margin) || Margin < 0 || Margin > 100_000) throw new ArgumentOutOfRangeException(nameof(Margin));
        Positive(MinimumRadius, nameof(MinimumRadius));
        Positive(MinimumAverageHalfWidth, nameof(MinimumAverageHalfWidth));
        Positive(MaximumHalfWidth, nameof(MaximumHalfWidth));
        if (MaximumHalfWidth > 10_000f || MinimumAverageHalfWidth > MaximumHalfWidth)
            throw new ArgumentOutOfRangeException(nameof(MaximumHalfWidth));
        Positive(HalfWidthFactor, nameof(HalfWidthFactor));
        Positive(MaximumStrokeLength, nameof(MaximumStrokeLength));
        if (MaximumStrokeLength < 20 * StepSize) throw new ArgumentOutOfRangeException(nameof(MaximumStrokeLength));
        if (!float.IsFinite(MinimumStrokeShare) || MinimumStrokeShare is < .1f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(MinimumStrokeShare));
        if (!float.IsFinite(DryExponent) || DryExponent is < .1f or > 16f)
            throw new ArgumentOutOfRangeException(nameof(DryExponent));
        Positive(HookRadiusFactor, nameof(HookRadiusFactor));
        if (MaximumCoverageCells is < 4 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(MaximumCoverageCells));
        if (MaximumTotalStrokes is < 1 or > 25_000)
            throw new ArgumentOutOfRangeException(nameof(MaximumTotalStrokes));
        if (MaximumSpinePoints is < 40 or > 8_000_000)
            throw new ArgumentOutOfRangeException(nameof(MaximumSpinePoints));
        // Validare la griglia prima di allocare array (incluse dimensioni non divisibili per la cella).
        long nx = (long)Math.Floor((double)width / CoverageCell) + 1;
        long ny = (long)Math.Floor((double)height / CoverageCell) + 1;
        if (nx * ny > MaximumCoverageCells)
            throw new ArgumentOutOfRangeException(nameof(CoverageCell), "La griglia supera MaximumCoverageCells.");
    }

    private static void Positive(float n, string name)
    {
        if (!float.IsFinite(n) || n <= 0f || n > 100_000f) throw new ArgumentOutOfRangeException(name);
    }
}
