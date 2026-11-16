namespace GenerativeArt.Algorithms.Flow;

/// <summary>Limiti e soglie di generazione. Nessun seed implicito.</summary>
public sealed record EvenStreamlineOptions
{
    public required FlowBounds Bounds { get; init; }
    public float Separation { get; init; } = 14f;
    public float TestDistanceFactor { get; init; } = .54f;
    public float StepSize { get; init; } = 2f;
    public float Margin { get; init; } = 4f;
    public int MaxStepsPerDirection { get; init; } = 160;
    public int MinimumPoints { get; init; } = 5;
    public int MaxLines { get; init; } = 400;
    public int MaxCandidates { get; init; } = 20_000;
    public bool DistributeSeedBudget { get; init; } = true;
}
