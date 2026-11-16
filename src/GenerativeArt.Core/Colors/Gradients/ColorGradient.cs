using System.Collections.ObjectModel;

namespace GenerativeArt.Core.Colors.Gradients;

/// <summary>
/// Gradiente immutabile. Gli stop devono essere in ordine strettamente crescente.
/// Fuori dagli stop estremi Sample restituisce il colore del primo/ultimo stop.
/// Alpha è interpolato straight; non viene effettuato compositing né premoltiplicazione.
/// </summary>
public sealed class ColorGradient
{
    private readonly ColorStop[] _stops;
    private readonly ReadOnlyCollection<ColorStop> _readOnlyStops;

    public GradientInterpolationSpace Space { get; }
    public IReadOnlyList<ColorStop> Stops => _readOnlyStops;

    public ColorGradient(GradientInterpolationSpace space, params ColorStop[] stops)
    {
        if (space is not GradientInterpolationSpace.Srgb and not GradientInterpolationSpace.LinearSrgb)
            throw new ArgumentOutOfRangeException(nameof(space));
        ArgumentNullException.ThrowIfNull(stops);
        if (stops.Length < 2)
            throw new ArgumentException("Un gradiente richiede almeno due stop.", nameof(stops));

        _stops = (ColorStop[])stops.Clone();
        for (int i = 1; i < _stops.Length; i++)
        {
            if (_stops[i].Position <= _stops[i - 1].Position)
                throw new ArgumentException("Gli stop devono avere posizioni strettamente crescenti.", nameof(stops));
        }
        _readOnlyStops = Array.AsReadOnly(_stops);
        Space = space;
    }

    /// <summary>Campiona una posizione finita, con clamp agli stop esterni.</summary>
    public NormalizedRgba Sample(double position)
    {
        if (!double.IsFinite(position)) throw new ArgumentOutOfRangeException(nameof(position));
        if (position <= _stops[0].Position) return _stops[0].Color;
        if (position >= _stops[^1].Position) return _stops[^1].Color;

        int low = 0, high = _stops.Length - 1;
        while (high - low > 1)
        {
            int middle = low + (high - low) / 2;
            if (position < _stops[middle].Position) high = middle;
            else low = middle;
        }
        if (position == _stops[low].Position) return _stops[low].Color;
        if (position == _stops[high].Position) return _stops[high].Color;

        double amount = (position - _stops[low].Position) /
            (_stops[high].Position - _stops[low].Position);
        NormalizedRgba a = _stops[low].Color, b = _stops[high].Color;
        return Space switch
        {
            GradientInterpolationSpace.Srgb => new NormalizedRgba(
                Math.Clamp(Mix(a.R, b.R, amount), 0d, 1d),
                Math.Clamp(Mix(a.G, b.G, amount), 0d, 1d),
                Math.Clamp(Mix(a.B, b.B, amount), 0d, 1d),
                Math.Clamp(Mix(a.A, b.A, amount), 0d, 1d)),
            GradientInterpolationSpace.LinearSrgb =>
                LinearRgba.FromSrgb(a).Mix(LinearRgba.FromSrgb(b), amount).ToSrgb(),
            _ => throw new InvalidOperationException("Spazio colore non supportato.")
        };
    }

    private static double Mix(double a, double b, double t) => a + (b - a) * t;
}
