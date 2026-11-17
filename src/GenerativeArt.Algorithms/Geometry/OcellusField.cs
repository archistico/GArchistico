using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Geometry;

/// <summary>
/// Occhi stratificati e unione liscia di SDF circolari, ispirati al modello
/// a livelli dell'Ocellus originale Gȧrt. Seme e disposizione sono riproducibili;
/// il porting NON è bit-identico al generatore Kotlin.
/// </summary>
public static class OcellusField
{
    public const int MaximumLevels = 13;

    public readonly record struct Eye(double X, double Y, double CoreRadius, int Depth);

    public static Eye[] Generate(ulong seed, int width, int height, int count, int minDepth,
        double depthBias, double coreRadius, double spacing, double margin, CancellationToken ct = default)
    {
        if (width < 1 || height < 1 || count is < 1 or > 80 ||
            minDepth is < 1 or > MaximumLevels || !double.IsFinite(depthBias) || depthBias <= 0 ||
            !double.IsFinite(coreRadius) || coreRadius is <= 0 or > .4 ||
            !double.IsFinite(spacing) || spacing is < 0 or > .5 ||
            !double.IsFinite(margin) || margin is < -.3 or > .5)
            throw new ArgumentOutOfRangeException(nameof(count), "Parametri Ocellus non validi.");
        var rand = new SplitMix64Random(seed);
        double side = Math.Min(width, height);
        var eyes = new List<Eye>(count);
        for (int attempt = 0; attempt < 20_000 && eyes.Count < count; attempt++)
        {
            if ((attempt & 127) == 0) ct.ThrowIfCancellationRequested();
            int depth = Math.Clamp(MaximumLevels - (int)(Math.Pow(rand.NextDouble(), depthBias) *
                (MaximumLevels - minDepth + 1)), minDepth, MaximumLevels);
            double radius = side * (.006 + (coreRadius - .006) * rand.NextDouble());
            double x = width * (margin + (1 - 2 * margin) * rand.NextDouble());
            double y = height * (margin + (1 - 2 * margin) * rand.NextDouble());
            if (eyes.All(other => GeometryDistance.Hypotenuse(x - other.X, y - other.Y) >=
                radius + other.CoreRadius + spacing * side))
                eyes.Add(new Eye(x, y, radius, depth));
        }
        return eyes.ToArray();
    }

    /// <summary>Precalcola raggi per livello: livello basso ampio, livello alto stretto.</summary>
    public static double[,] Radii(IReadOnlyList<Eye> eyes, int height, int levels,
        double band, double growth)
    {
        ArgumentNullException.ThrowIfNull(eyes);
        if (height <= 0 || levels is < 2 or > MaximumLevels ||
            !double.IsFinite(band) || band <= 0 || !double.IsFinite(growth) || growth < 0)
            throw new ArgumentOutOfRangeException(nameof(band));
        var width = new double[levels + 1];
        for (int k = 1; k <= levels; k++)
        {
            double pos = (levels - k) / (double)Math.Max(1, levels - 2);
            width[k] = band * height * (k == 1 ? .6 : 1 + growth * pos * pos * pos);
        }
        var reach = new double[levels + 2];
        for (int k = levels; k >= 1; k--) reach[k] = reach[k + 1] + width[k];
        var radii = new double[eyes.Count, levels + 1];
        for (int i = 0; i < eyes.Count; i++)
            for (int k = 1; k <= Math.Min(levels, eyes[i].Depth); k++)
                radii[i, k] = eyes[i].CoreRadius + reach[k] - reach[Math.Min(levels, eyes[i].Depth)];
        return radii;
    }

    public static double SmoothMin(double a, double b, double k)
    {
        if (!(k > 0)) return Math.Min(a, b);
        double h = Math.Max(k - Math.Abs(a - b), 0) / k;
        return Math.Min(a, b) - h * h * k * .25;
    }

    /// <summary>
    /// Distanza firmata dall'unione morbida degli occhi al livello richiesto.
    /// Il segno negativo indica la zona occupata dalla forma. Restituisce +infinito
    /// quando nessun occhio contribuisce a quel livello.
    /// </summary>
    public static double SignedDistance(IReadOnlyList<Eye> eyes, double[,] radii,
        ReadOnlySpan<double> centerDistances, int level, double melt)
    {
        double nearest = double.PositiveInfinity;
        int contributors = 0;
        for (int i = 0; i < eyes.Count; i++)
        {
            if (level > eyes[i].Depth) continue;
            double distance = centerDistances[i] - radii[i, level];
            nearest = contributors++ == 0 ? distance : SmoothMin(nearest, distance, melt);
        }
        return nearest;
    }

    /// <summary>
    /// Piccola apertura dove si incontrano almeno tre bordi esterni. I tre valori
    /// sono distanze alle superfici, non ai centri: questo evita che occhi di
    /// dimensioni diverse producano giunzioni sistematicamente spostate.
    /// </summary>
    public static double JunctionPore(IReadOnlyList<Eye> eyes, double[,] radii,
        ReadOnlySpan<double> centerDistances, double radius)
    {
        if (eyes.Count < 3 || radius <= 0) return 0;
        if (!double.IsFinite(radius)) throw new ArgumentOutOfRangeException(nameof(radius));

        double first = double.PositiveInfinity, second = double.PositiveInfinity;
        double third = double.PositiveInfinity;
        for (int i = 0; i < eyes.Count; i++)
        {
            double d = centerDistances[i] - radii[i, 1];
            if (d < first) { third = second; second = first; first = d; }
            else if (d < second) { third = second; second = d; }
            else if (d < third) third = d;
        }

        double meeting = Math.Clamp(1 - (third - first) / (radius * 1.6), 0, 1);
        double boundary = Math.Clamp(1 - Math.Abs(first) / (radius * 1.4), 0, 1);
        return meeting * meeting * boundary * boundary;
    }

    /// <summary>Copertura antialias di un livello a partire da distanze dai centri precomputate.</summary>
    public static double Coverage(IReadOnlyList<Eye> eyes, double[,] radii, ReadOnlySpan<double> centerDistances,
        int level, double melt)
    {
        return Math.Clamp(.5 - SignedDistance(eyes, radii, centerDistances, level, melt), 0, 1);
    }
}
