using System.Numerics;
using GenerativeArt.Algorithms.Noise;

namespace GenerativeArt.Algorithms.Geometry;

/// <summary>Contorno radiale chiuso: noise campionato sul cerchio, senza discontinuità alla cucitura.</summary>
public sealed class OrganicAperture
{
    private readonly SeededSimplexNoise _noise;
    public OrganicAperture(ulong seed) => _noise = new SeededSimplexNoise(seed);

    public Vector2[] Build(Vector2 center, float radius, int samples = 720, int lobes = 5,
        double ripple = .12, double roughness = .08, double rotation = 0,
        double stretch = 1, double depth = 0, CancellationToken cancellationToken = default)
    {
        if (!float.IsFinite(center.X) || !float.IsFinite(center.Y)
            || !float.IsFinite(radius) || radius is <= 0 or > 20000
            || samples is < 64 or > 4096 || lobes is < 1 or > 24
            || !double.IsFinite(ripple) || ripple is < 0 or > .35
            || !double.IsFinite(roughness) || roughness is < 0 or > .25
            || !double.IsFinite(rotation) || Math.Abs(rotation) > 10000
            || !double.IsFinite(stretch) || stretch is < .25 or > 4
            || !double.IsFinite(depth) || depth is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(radius), "Parametri del contorno fuori intervallo.");
        cancellationToken.ThrowIfCancellationRequested();
        var points = new Vector2[samples + 1];
        double c = Math.Cos(rotation), s = Math.Sin(rotation);
        for (int i = 0; i < samples; i++)
        {
            if ((i & 127) == 0) cancellationToken.ThrowIfCancellationRequested();
            double a = Math.Tau * i / samples, x = Math.Cos(a), y = Math.Sin(a);
            double noise = .72 * _noise.Sample3D(x * 1.6 + 17, y * 1.6 + 31, depth * 1.8)
                + .28 * _noise.Sample3D(x * 4.8 + 9, y * 4.8 + 23, depth * 2.4);
            double r = radius * (1 + ripple * Math.Cos(lobes * a) + roughness * noise);
            double px = r * x * stretch, py = r * y;
            points[i] = center + new Vector2((float)(px * c - py * s), (float)(px * s + py * c));
        }
        points[^1] = points[0];
        return points;
    }
}
