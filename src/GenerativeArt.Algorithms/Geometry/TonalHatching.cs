namespace GenerativeArt.Algorithms.Geometry;

public readonly record struct HatchCoverage(double Primary, double Cross);

/// <summary>Tratteggio analitico periodico con copertura filtrata e strati tonali progressivi.</summary>
public sealed class TonalHatching
{
    private readonly double[] _cos;
    private readonly double[] _sin;
    public double Spacing { get; }
    public double LineWidth { get; }
    public int Layers => _cos.Length;

    public TonalHatching(double spacing = 6, double lineWidth = .7, double angleDegrees = 25,
        double crossAngleDegrees = 60, int layers = 3)
    {
        if (!double.IsFinite(spacing) || spacing is < .001 or > 1000
            || !double.IsFinite(lineWidth) || lineWidth <= 0 || lineWidth > spacing
            || !double.IsFinite(angleDegrees) || Math.Abs(angleDegrees) > 360
            || !double.IsFinite(crossAngleDegrees) || crossAngleDegrees is < 0 or > 180
            || layers is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(spacing));
        Spacing = spacing;
        LineWidth = lineWidth;
        _cos = new double[layers];
        _sin = new double[layers];
        for (int i = 0; i < layers; i++)
        {
            double angle = (angleDegrees + i * crossAngleDegrees) * Math.PI / 180;
            // Proiezione sulla normale: il tratto segue l'angolo scelto.
            _cos[i] = -Math.Sin(angle);
            _sin[i] = Math.Cos(angle);
        }
    }

    public HatchCoverage Sample(double x, double y, double darkness, double footprint = 1)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y) || Math.Abs(x) > 1e7 || Math.Abs(y) > 1e7
            || !double.IsFinite(darkness) || darkness is < 0 or > 1
            || !double.IsFinite(footprint) || footprint <= 0 || footprint > 16)
            throw new ArgumentOutOfRangeException(nameof(darkness));
        double primary = 0, cross = 0;
        for (int i = 0; i < Layers; i++)
        {
            double strength = Math.Clamp(darkness * Layers - i, 0, 1);
            if (strength == 0) continue;
            double position = (x * _cos[i] + y * _sin[i]) / Spacing + i * .173;
            double distance = Math.Abs(position - Math.Round(position)) * Spacing;
            double halfWidth = LineWidth * (.25 + .75 * strength) * .5;
            double aa = footprint * .5;
            double coverage = strength * (1 - SmoothStep(halfWidth - aa, halfWidth + aa, distance));
            if (i == 0) primary = coverage;
            else cross = 1 - (1 - cross) * (1 - coverage);
        }
        return new HatchCoverage(primary, cross);
    }

    private static double SmoothStep(double low, double high, double value)
    {
        double t = Math.Clamp((value - low) / (high - low), 0, 1);
        return t * t * (3 - 2 * t);
    }
}
