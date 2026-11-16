using System.Numerics;

namespace GenerativeArt.Algorithms.Geometry;

/// <summary>Trocoide chiusa con rapporto razionale degli ingranaggi e modulazione radiale.</summary>
public sealed class GuillocheCurve
{
    private readonly double _rollingRadius;
    private readonly double _centerRadius;
    private readonly double _penRadius;
    private readonly double _waveAmount;
    private readonly double _phase;
    private readonly int _waves;
    private readonly bool _outside;
    public int PeriodTurns { get; }
    public int MinimumSamples { get; }
    public double EnvelopeRadius { get; }

    public GuillocheCurve(int fixedTeeth = 64, int movingTeeth = 17, double penRadius = .48,
        bool outside = false, int waves = 7, double waveAmount = .06, double phase = 0)
    {
        if (fixedTeeth is < 3 or > 128 || movingTeeth is < 2 or > 96
            || (!outside && movingTeeth >= fixedTeeth))
            throw new ArgumentOutOfRangeException(nameof(movingTeeth), "In ipotrocoide l'ingranaggio mobile deve essere più piccolo del fisso.");
        if (!double.IsFinite(penRadius) || penRadius is < 0 or > 1.6
            || !double.IsFinite(waveAmount) || waveAmount is < 0 or > .3
            || !double.IsFinite(phase) || Math.Abs(phase) > 1_000_000 || waves is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(penRadius));
        int a = fixedTeeth, b = movingTeeth;
        while (b != 0) (a, b) = (b, a % b);
        PeriodTurns = movingTeeth / a;
        _rollingRadius = (double)movingTeeth / fixedTeeth;
        _centerRadius = outside ? 1 + _rollingRadius : 1 - _rollingRadius;
        _penRadius = penRadius;
        _outside = outside;
        _waves = waves;
        _waveAmount = waveAmount;
        _phase = phase;
        EnvelopeRadius = (_centerRadius + penRadius) * (1 + waveAmount);
        int gearCycles = (outside ? fixedTeeth + movingTeeth : fixedTeeth - movingTeeth) / a;
        MinimumSamples = Math.Max(512, 16 * Math.Max(PeriodTurns, Math.Max(gearCycles,
            waveAmount > 0 ? waves * PeriodTurns : 0)));
    }

    public Vector2 Sample(double t)
    {
        if (!double.IsFinite(t) || Math.Abs(t) > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(t));
        double theta = _centerRadius / _rollingRadius * t;
        double wave = 1 + _waveAmount * Math.Sin(_waves * t + _phase);
        double x = _centerRadius * Math.Cos(t) + (_outside ? -1 : 1) * _penRadius * Math.Cos(theta);
        double y = _centerRadius * Math.Sin(t) - _penRadius * Math.Sin(theta);
        return new Vector2((float)(x * wave), (float)(y * wave));
    }

    public Vector2[] CreatePolyline(int requestedSamples, CancellationToken cancellationToken = default)
    {
        if (requestedSamples is < 512 or > 48_000)
            throw new ArgumentOutOfRangeException(nameof(requestedSamples));
        cancellationToken.ThrowIfCancellationRequested();
        int count = Math.Max(requestedSamples, MinimumSamples);
        var points = new Vector2[count + 1];
        for (int i = 0; i < count; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            points[i] = Sample(i * (Math.Tau * PeriodTurns / count));
        }
        points[count] = points[0];
        return points;
    }
}
