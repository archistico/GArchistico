namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// C3: valori in pixel logici, ripresi in parte da CincinnusSimple.kt.
/// Lo spessore è calcolato da Separation * HalfWidthFactor (110 * 1.25 nell'originale).
/// I budget evitano allocazioni incontrollate durante la suddivisione in pennellate.
/// </summary>
public sealed record CincinnusStrokeOptions
{
    public float HalfWidthFactor { get; init; } = 1.25f;
    public float FirstMinimumLength { get; init; } = 200f;
    public float SubsequentMinimumLength { get; init; } = 600f;
    public float MaximumStrokeLength { get; init; } = 1600f;
    public float MinimumOverlap { get; init; } = 150f;
    public float MaximumOverlap { get; init; } = 350f;
    public float DryExponent { get; init; } = 4f;
    public int MaximumStrokes { get; init; } = 4_096;
    public int MaximumSpinePoints { get; init; } = 400_000;

    internal void Validate()
    {
        if (!float.IsFinite(HalfWidthFactor) || HalfWidthFactor <= 0 || HalfWidthFactor > 8)
            throw new ArgumentOutOfRangeException(nameof(HalfWidthFactor));
        if (!float.IsFinite(FirstMinimumLength) || FirstMinimumLength < 3)
            throw new ArgumentOutOfRangeException(nameof(FirstMinimumLength));
        if (!float.IsFinite(SubsequentMinimumLength) || SubsequentMinimumLength < 3)
            throw new ArgumentOutOfRangeException(nameof(SubsequentMinimumLength));
        if (!float.IsFinite(MaximumStrokeLength) || MaximumStrokeLength < FirstMinimumLength ||
            MaximumStrokeLength < SubsequentMinimumLength || MaximumStrokeLength > 50_000)
            throw new ArgumentOutOfRangeException(nameof(MaximumStrokeLength));
        if (!float.IsFinite(MinimumOverlap) || MinimumOverlap < 0 || MinimumOverlap >= MaximumStrokeLength)
            throw new ArgumentOutOfRangeException(nameof(MinimumOverlap));
        if (!float.IsFinite(MaximumOverlap) || MaximumOverlap < MinimumOverlap || MaximumOverlap >= MaximumStrokeLength)
            throw new ArgumentOutOfRangeException(nameof(MaximumOverlap));
        if (!float.IsFinite(DryExponent) || DryExponent < .1f || DryExponent > 16)
            throw new ArgumentOutOfRangeException(nameof(DryExponent));
        if (MaximumStrokes is < 1 or > 50_000)
            throw new ArgumentOutOfRangeException(nameof(MaximumStrokes));
        if (MaximumSpinePoints is < 16 or > 8_000_000)
            throw new ArgumentOutOfRangeException(nameof(MaximumSpinePoints));
    }
}
