using System.Numerics;
using GenerativeArt.Algorithms.Noise;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>
/// Campo rotazionale di un potenziale Simplex: (dP/dy, -dP/dx).
/// Le derivate sono differenze finite centrali in coordinate canvas.
/// Il seed è esplicito, il campo è immutabile e thread-safe per letture parallele.
/// </summary>
public sealed class CurlNoiseField2D : IVectorField2D
{
    private readonly SeededSimplexNoise _noise;
    public ulong Seed { get; }
    public float Frequency { get; }
    public float DerivativeStep { get; }

    public CurlNoiseField2D(ulong seed, float frequency = .008f, float derivativeStep = .5f)
    {
        if (!float.IsFinite(frequency) || frequency <= 0f)
            throw new ArgumentOutOfRangeException(nameof(frequency));
        if (!float.IsFinite(derivativeStep) || derivativeStep <= 0f)
            throw new ArgumentOutOfRangeException(nameof(derivativeStep));
        Seed = seed;
        Frequency = frequency;
        DerivativeStep = derivativeStep;
        _noise = new SeededSimplexNoise(seed);
    }

    public Vector2 Sample(float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y)) return Vector2.Zero;
        double eps = DerivativeStep;
        double freq = Frequency;
        double sx = x * freq, sy = y * freq, de = eps * freq;
        double derivativeY = (_noise.Sample2D(sx, sy + de) - _noise.Sample2D(sx, sy - de)) / (2d * eps);
        double derivativeX = (_noise.Sample2D(sx + de, sy) - _noise.Sample2D(sx - de, sy)) / (2d * eps);
        float vx = (float)derivativeY;
        float vy = (float)-derivativeX;
        return float.IsFinite(vx) && float.IsFinite(vy) ? new Vector2(vx, vy) : Vector2.Zero;
    }
}
