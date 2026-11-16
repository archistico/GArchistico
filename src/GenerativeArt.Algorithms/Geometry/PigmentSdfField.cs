using System.Numerics;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Geometry;

public enum PigmentSdfLayout { Orbital, Cluster, River }
public readonly record struct PigmentSdfSample(float Distance, Vector4 ColorWeights);

public sealed record PigmentSdfOptions
{
    public int Blobs { get; init; } = 10;
    public PigmentSdfLayout Layout { get; init; } = PigmentSdfLayout.Cluster;
    public float Radius { get; init; } = .18f;
    public float RadiusVariation { get; init; } = .55f;
    public float Spread { get; init; } = 1;
    public float Elongation { get; init; } = 1.7f;
    public float Fusion { get; init; } = .1f;
    public float ColorMix { get; init; } = .85f;
    public float Warp { get; init; } = .055f;
    public float WarpFrequency { get; init; } = 2.8f;
}

/// <summary>
/// Unione morbida di ellissi seeded, dominio deformato e quattro pesi cromatici.
/// Le distanze ellittiche/deformate sono approssimate: il renderer misura il footprint sul raster.
/// </summary>
public sealed class PigmentSdfField
{
    private readonly record struct Blob(Vector2 Center, float Radius, float InverseAspect,
        float Cos, float Sin, Vector4 Color);
    private readonly Blob[] _blobs;
    private readonly PigmentSdfOptions _options;
    private readonly SeededSimplexNoise _noise;

    public PigmentSdfField(ulong seed, Vector2 extent, PigmentSdfOptions? options = null)
    {
        var o = options ?? new PigmentSdfOptions();
        if (!float.IsFinite(extent.X) || !float.IsFinite(extent.Y) || extent.X is <= 0 or > 128 || extent.Y is <= 0 or > 128
            || o.Blobs is < 2 or > 40 || !Enum.IsDefined(o.Layout)
            || !In(o.Radius, .03f, .36f) || !In(o.RadiusVariation, 0, .85f)
            || !In(o.Spread, .1f, 1.5f) || !In(o.Elongation, 1, 3)
            || !In(o.Fusion, .002f, .2f) || !In(o.ColorMix, 0, 1)
            || !In(o.Warp, 0, .2f) || !In(o.WarpFrequency, .3f, 10))
            throw new ArgumentOutOfRangeException(nameof(options), "Parametri del campo di pigmenti fuori intervallo.");
        _options = o;
        _noise = new SeededSimplexNoise(seed ^ 0xB1A5C0F3UL);
        var random = new SplitMix64Random(seed);
        float phase = random.NextFloat() * MathF.Tau;
        _blobs = new Blob[o.Blobs];
        for (int i = 0; i < _blobs.Length; i++)
        {
            float t = (i + .5f) / _blobs.Length;
            float angle = t * MathF.Tau + phase + (random.NextFloat() - .5f) * .55f;
            Vector2 center = o.Layout switch
            {
                PigmentSdfLayout.Orbital => new Vector2(MathF.Cos(angle) * .34f, MathF.Sin(angle) * .34f),
                PigmentSdfLayout.River => new Vector2((t - .5f) * .95f,
                    MathF.Sin(t * MathF.Tau + phase) * .22f + (random.NextFloat() - .5f) * .22f),
                _ => new Vector2(MathF.Cos(i % 3 * MathF.Tau / 3 + phase) * .23f + (random.NextFloat() - .5f) * .36f,
                    MathF.Sin(i % 3 * MathF.Tau / 3 + phase) * .23f + (random.NextFloat() - .5f) * .36f)
            };
            center *= extent * o.Spread;
            float radius = o.Radius * (1 + (random.NextFloat() * 2 - 1) * o.RadiusVariation);
            float ellipseAngle = random.NextFloat() * MathF.Tau;
            float aspect = 1 + (o.Elongation - 1) * random.NextFloat();
            Vector4 color = (i % 4) switch
            {
                0 => Vector4.UnitX, 1 => Vector4.UnitY, 2 => Vector4.UnitZ, _ => Vector4.UnitW
            };
            _blobs[i] = new Blob(center, radius, 1 / aspect, MathF.Cos(ellipseAngle), MathF.Sin(ellipseAngle), color);
        }
    }

    private static bool In(float v, float min, float max) => float.IsFinite(v) && v >= min && v <= max;

    public PigmentSdfSample Sample(Vector2 point)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || Math.Abs(point.X) > 10000 || Math.Abs(point.Y) > 10000)
            throw new ArgumentOutOfRangeException(nameof(point));
        var o = _options;
        if (o.Warp > 0)
        {
            Vector2 q = point * o.WarpFrequency;
            point += o.Warp * new Vector2((float)_noise.Sample2D(q.X + 17, q.Y + 29),
                (float)_noise.Sample2D(q.X + 53, q.Y + 11));
        }
        float distance = float.PositiveInfinity, closest = float.PositiveInfinity, total = 0;
        Vector4 weights = Vector4.Zero, nearestColor = Vector4.UnitX;
        foreach (var blob in _blobs)
        {
            Vector2 p = point - blob.Center;
            float x = (p.X * blob.Cos + p.Y * blob.Sin) * blob.InverseAspect;
            float y = -p.X * blob.Sin + p.Y * blob.Cos;
            float length = MathF.Sqrt(x * x + y * y), d = length - blob.Radius;
            if (d < closest) { closest = d; nearestColor = blob.Color; }
            distance = float.IsPositiveInfinity(distance) ? d : SignedDistance2D.SmoothUnion(distance, d, o.Fusion);
            float influence = Math.Max(0, 1 - length / (blob.Radius + o.Fusion * (.15f + o.ColorMix * .85f)));
            influence *= influence * influence;
            influence *= o.ColorMix + (1 - o.ColorMix) * influence * influence * influence;
            weights += blob.Color * influence; total += influence;
        }
        weights = total > 1e-12f ? weights / total : nearestColor;
        return new PigmentSdfSample(distance, weights);
    }
}
