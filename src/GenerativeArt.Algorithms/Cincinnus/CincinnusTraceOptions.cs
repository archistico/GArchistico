namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// Parametri del tracciamento geometrico. I default 3/9000/500 sono quelli di
/// Cincinnus.kt per un canvas 2000×2000, non proporzioni automatiche della tela.
/// </summary>
public sealed record CincinnusTraceOptions
{
    public float StepSize { get; init; } = 3f;
    public float MaximumLength { get; init; } = 9000f;
    public float Margin { get; init; } = 500f;
}
