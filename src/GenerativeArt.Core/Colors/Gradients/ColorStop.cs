namespace GenerativeArt.Core.Colors.Gradients;

/// <summary>Stop immutabile; posizione normalizzata [0,1].</summary>
public readonly record struct ColorStop
{
    public double Position { get; }
    public NormalizedRgba Color { get; }

    public ColorStop(double position, NormalizedRgba color)
    {
        Position = NormalizedRgba.Unit(position, nameof(position));
        Color = color;
    }
}
