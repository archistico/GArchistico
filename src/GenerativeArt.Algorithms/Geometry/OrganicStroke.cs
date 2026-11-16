using System.Numerics;
using GenerativeArt.Algorithms.Noise;

namespace GenerativeArt.Algorithms.Geometry;

public sealed record BristleStroke(Vector2[] Points, double InkWeight);

/// <summary>Setole lungo una polilinea: pressione, rastremazione e irregolarità seeded.</summary>
public static class OrganicStroke
{
    public static IEnumerable<BristleStroke> Build(IReadOnlyList<Vector2> centerline,
        int bristles, float width, double taper, double roughness, ulong seed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(centerline);
        if (bristles is < 2 or > 40 || !float.IsFinite(width) || width is <= 0 or > 200
            || !double.IsFinite(taper) || taper is < 0 or > 4
            || !double.IsFinite(roughness) || roughness is < 0 or > 1
            || centerline.Count > 10001 || (long)centerline.Count * bristles > 400_000)
            throw new ArgumentOutOfRangeException(nameof(bristles));
        foreach (Vector2 point in centerline)
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                throw new ArgumentException("La linea deve contenere coordinate finite.", nameof(centerline));
        cancellationToken.ThrowIfCancellationRequested();
        return Enumerate();

        IEnumerable<BristleStroke> Enumerate()
        {
            if (centerline.Count < 2) yield break;
            var noise = new SeededSimplexNoise(seed);
            for (int bristle = 0; bristle < bristles; bristle++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var points = new Vector2[centerline.Count];
                double side = (double)bristle / (bristles - 1) - .5;
                for (int i = 0; i < points.Length; i++)
                {
                    if ((i & 63) == 0) cancellationToken.ThrowIfCancellationRequested();
                    Vector2 direction = centerline[Math.Min(i + 1, points.Length - 1)] - centerline[Math.Max(0, i - 1)];
                    double length = Math.Sqrt((double)direction.X * direction.X + (double)direction.Y * direction.Y);
                    Vector2 normal = length > 1e-9 ? new Vector2((float)(-direction.Y / length), (float)(direction.X / length)) : Vector2.UnitY;
                    double t = (double)i / (points.Length - 1);
                    double pressure = .08 + .92 * Math.Pow(Math.Max(0, Math.Sin(Math.PI * t)), taper);
                    double jitter = roughness * .12 * noise.Sample2D(t * 6 + 19, bristle * .21 + 7);
                    points[i] = centerline[i] + normal * (float)(width * pressure * (side + jitter));
                }
                double weight = .55 + .45 * (.5 + .5 * noise.Sample2D(bristle * .31, 11.3));
                yield return new BristleStroke(points, weight);
            }
        }
    }
}
