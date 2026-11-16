using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Noise;

namespace GenerativeArt.Algorithms.Animation;

/// <summary>
/// Campo angolare con campionamento circolare di Simplex nello spazio delle coordinate.
/// La fase 0 equivale alla fase 1 (campionamento periodico); è indipendente dalla GUI.
/// </summary>
public sealed class LoopNoiseField2D : IVectorField2D
{
    private readonly SeededSimplexNoise _noise;
    private readonly double _frequency, _offsetX, _offsetY;

    public LoopNoiseField2D(ulong seed, double phase, double frequency = 0.008,
        double loopRadius = 1.8)
    {
        if (!double.IsFinite(phase) || phase < 0 || phase > 1)
            throw new ArgumentOutOfRangeException(nameof(phase));
        if (!double.IsFinite(frequency) || frequency <= 0 || frequency > 1)
            throw new ArgumentOutOfRangeException(nameof(frequency));
        if (!double.IsFinite(loopRadius) || loopRadius <= 0 || loopRadius > 100)
            throw new ArgumentOutOfRangeException(nameof(loopRadius));
        _noise = new SeededSimplexNoise(seed);
        _frequency = frequency;
        double a = Math.Tau * phase;
        _offsetX = Math.Cos(a) * loopRadius;
        _offsetY = Math.Sin(a) * loopRadius;
    }

    public Vector2 Sample(float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y)) return Vector2.Zero;
        double n = _noise.Sample2D(x * _frequency + _offsetX, y * _frequency + _offsetY);
        double angle = n * Math.PI * 3.2 + (double)x * .0025;
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }
}
