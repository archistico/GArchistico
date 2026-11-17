using System.Numerics;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Geometry;

public enum VoronoiHolePattern { Random, Clustered }

/// <summary>Un piano geometrico con aperture reali e selezione esatta, indipendente dai colori.</summary>
public sealed class VoronoiLayerPlan
{
    public IReadOnlyList<VoronoiCell> Cells { get; }
    public IReadOnlyList<bool> Missing { get; }

    private VoronoiLayerPlan(IReadOnlyList<VoronoiCell> cells, bool[] missing)
    { Cells = cells; Missing = Array.AsReadOnly(missing); }

    public static VoronoiLayerPlan Build(ulong seed, int siteCount, float width, float height,
        float jitter = .7f, int relaxations = 1, double missingFraction = .35,
        VoronoiHolePattern pattern = VoronoiHolePattern.Clustered, Vector2 offset = default,
        CancellationToken cancellationToken = default)
    {
        if (siteCount is < 2 or > VoronoiTessellation.MaximumSites ||
            !float.IsFinite(width) || !float.IsFinite(height) || width <= 0 || height <= 0 ||
            !float.IsFinite(jitter) || jitter is < 0 or > .95f || relaxations is < 0 or > 3 ||
            !double.IsFinite(missingFraction) || missingFraction is < 0 or > 1 || !Enum.IsDefined(pattern) ||
            !float.IsFinite(offset.X) || !float.IsFinite(offset.Y) || Math.Abs(offset.X) > 10000 || Math.Abs(offset.Y) > 10000)
            throw new ArgumentOutOfRangeException(nameof(siteCount), "Parametri del piano Voronoi non validi.");
        cancellationToken.ThrowIfCancellationRequested();
        var random = new SplitMix64Random(seed);
        int columns = Math.Max(1, Math.Min(siteCount, (int)Math.Round(Math.Sqrt(siteCount * (double)width / height))));
        int rows = (int)Math.Ceiling(siteCount / (double)columns);
        var sites = new Vector2[siteCount];
        float Wrap(float value, float extent)
        {
            float r = value % extent;
            if (r < 0) r += extent;
            return Math.Clamp(r, 0, MathF.BitDecrement(extent));
        }
        for (int i = 0; i < siteCount; i++)
        {
            if ((i & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            int x = i % columns, y = i / columns;
            float fx = (float)((x + .5 + (random.NextDouble() - .5) * jitter) * width / columns);
            float fy = (float)((y + .5 + (random.NextDouble() - .5) * jitter) * height / rows);
            sites[i] = offset == Vector2.Zero
                ? new Vector2(Math.Clamp(fx, 0, MathF.BitDecrement(width)), Math.Clamp(fy, 0, MathF.BitDecrement(height)))
                : new Vector2(Wrap(fx + offset.X, width), Wrap(fy + offset.Y, height));
        }
        var relaxed = VoronoiTessellation.Relax(sites, width, height, relaxations, cancellationToken);
        var cells = VoronoiTessellation.Build(relaxed, width, height, cancellationToken);
        var holes = random.Fork("voronoi/depth/holes/v1");
        var noise = new SeededSimplexNoise(seed ^ 0xB70D42UL);
        double unit = Math.Min(width, height);
        var scores = new (int Index, double Score)[siteCount];
        for (int i = 0; i < siteCount; i++)
        {
            if ((i & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            var c = cells[i].Centroid;
            double score = holes.NextDouble();
            if (pattern == VoronoiHolePattern.Clustered)
                score = .8 * noise.Sample2D(c.X / unit * 2.6 + 13, c.Y / unit * 2.6 + 29) + .2 * score;
            scores[i] = (i, score);
        }
        Array.Sort(scores, (a, b) =>
        { int score = b.Score.CompareTo(a.Score); return score != 0 ? score : a.Index.CompareTo(b.Index); });
        var missing = new bool[siteCount];
        int count = (int)Math.Floor(siteCount * missingFraction);
        for (int i = 0; i < count; i++) missing[scores[i].Index] = true;
        return new VoronoiLayerPlan(cells, missing);
    }
}
