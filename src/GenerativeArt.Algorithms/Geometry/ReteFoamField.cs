using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Geometry;

/// <summary>
/// Schiuma a dischi di raggi eterogenei (Apollonius-like): la differenza tra
/// le prime due distanze alle superfici controlla le pareti. Siti inseriti
/// da grandi a piccoli, con drenaggio e variazione controllata da noise.
/// Non è una trascrizione bit-a-bit del Rete originale Kotlin.
/// </summary>
public sealed class ReteFoamField
{
    public readonly record struct Cell(double X, double Y, double Radius);
    public readonly record struct NearestPair(double First, double Second, double Third, double FirstRadius, double SecondRadius);

    private readonly Cell[] _cells;
    private readonly int[][] _bins;
    private readonly int _columns;
    private readonly int _rows;
    private readonly double _bucketSize;
    private readonly double _searchMargin;
    public IReadOnlyList<Cell> Cells => Array.AsReadOnly(_cells);

    private ReteFoamField(Cell[] cells, int width, int height, double bigRadius)
    {
        _cells = cells;
        _bucketSize = Math.Max(8, Math.Min(width, height) / 28d);
        _searchMargin = Math.Max(_bucketSize, bigRadius * 1.6);
        _columns = (int)Math.Ceiling(width / _bucketSize);
        _rows = (int)Math.Ceiling(height / _bucketSize);
        var bins = new List<int>[_columns * _rows];
        for (int i = 0; i < bins.Length; i++) bins[i] = [];
        for (int i = 0; i < cells.Length; i++)
        {
            Cell c = cells[i];
            double reach = c.Radius + _searchMargin;
            int x0 = ClampColumn(c.X - reach), x1 = ClampColumn(c.X + reach);
            int y0 = ClampRow(c.Y - reach), y1 = ClampRow(c.Y + reach);
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++) bins[y * _columns + x].Add(i);
        }
        _bins = bins.Select(b => b.ToArray()).ToArray();
    }

    public static ReteFoamField Create(ulong seed, int width, int height, int count,
        double big, double small, double tierRatio, double variation, double density,
        double fusion, double froth, double drain, double sink, CancellationToken ct = default, bool production = false)
    {
        if (width < 1 || height < 1 || (long)width * height > (production ? 8_847_360L : 4_194_304L) || count is < 2 or > 850 ||
            !double.IsFinite(big) || !double.IsFinite(small) || big <= small || small <= 0 || big > .4 ||
            !double.IsFinite(tierRatio) || tierRatio is < .3 or >= 1 ||
            !double.IsFinite(variation) || variation is < 0 or > 1 ||
            !double.IsFinite(density) || density is < .1 or > 40 ||
            !double.IsFinite(fusion) || fusion is < 0 or > .5 ||
            !double.IsFinite(froth) || froth is < 0 or > 1 ||
            !double.IsFinite(drain) || drain is < 0 or > 1.5 ||
            !double.IsFinite(sink) || sink is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Parametri schiuma non validi.");

        double side = Math.Min(width, height), rBig = big * side, rMin = small * side;
        var random = new SplitMix64Random(seed);
        var noise = new SeededSimplexNoise(random.NextUInt64());
        var sites = new List<Cell>(count);
        double margin = rBig * .7;
        double drainX = width * 1.02, drainY = height * 1.05;
        int tier = 0;
        int tierCount = Math.Max(1, 1 + (int)Math.Ceiling(Math.Log(rMin / rBig) / Math.Log(tierRatio)));
        int perTierQuota = (int)Math.Ceiling(count / (double)tierCount);
        int totalAttempts = 0;
        int maximumAttempts = Math.Min(20_000, Math.Max(2_000, count * 80));
        for (double radiusTier = rBig; radiusTier >= rMin * .99 && sites.Count < count &&
             totalAttempts < maximumAttempts; radiusTier *= tierRatio, tier++)
        {
            double estimated = Math.Max(1, (width + 2 * margin) * (height + 2 * margin) /
                Math.Max(1, radiusTier * radiusTier));
            int attempts = Math.Min(4_000, (int)Math.Ceiling(estimated * density * .22));
            int tierLimit = Math.Min(count, (tier + 1) * perTierQuota);
            for (int attempt = 0; attempt < attempts && sites.Count < tierLimit &&
                 totalAttempts < maximumAttempts; attempt++, totalAttempts++)
            {
                if ((attempt & 63) == 0) ct.ThrowIfCancellationRequested();
                double x = -margin + (width + 2 * margin) * random.NextDouble();
                double y = -margin + (height + 2 * margin) * random.NextDouble();
                double distance = GeometryDistance.Hypotenuse(x - drainX, y - drainY);
                double wet = drain == 0 ? 0 : Math.Clamp(1 - distance / (drain * side + rBig), 0, 1);
                double localNoise = .5 + .5 * noise.Sample2D(x / side * 1.15 + 11, y / side * 1.15 + 7);
                double desired = rBig * Math.Pow(sink, wet) *
                    Math.Exp(-froth * Math.Clamp(localNoise, 0, 1) * 2.2);
                double radius = Math.Max(rMin, radiusTier * (1 - variation * random.NextDouble()));
                if (radius > desired * 1.35 || radius < desired * .23) continue;
                bool overlaps = false;
                foreach (Cell other in sites)
                {
                    double safe = (radius + other.Radius) * (1 - fusion * .5);
                    double dx = x - other.X, dy = y - other.Y;
                    if (dx * dx + dy * dy < safe * safe) { overlaps = true; break; }
                }
                if (!overlaps) sites.Add(new Cell(x, y, radius));
            }
            if (tier > 40) break;
        }
        // Anche una geometria estremamente rada produce un campo definito.
        if (sites.Count < 2)
        {
            sites.Add(new Cell(width * .35, height * .5, rMin));
            sites.Add(new Cell(width * .7, height * .5, rMin));
        }
        return new ReteFoamField(sites.ToArray(), width, height, rBig);
    }

    private int ClampColumn(double x) => Math.Clamp((int)Math.Floor(x / _bucketSize), 0, _columns - 1);
    private int ClampRow(double y) => Math.Clamp((int)Math.Floor(y / _bucketSize), 0, _rows - 1);

    public NearestPair Probe(double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y))
            throw new ArgumentOutOfRangeException(nameof(x));
        int[] indices = _bins[ClampRow(y) * _columns + ClampColumn(x)];
        double d0 = double.PositiveInfinity, d1 = double.PositiveInfinity, d2 = double.PositiveInfinity;
        double r0 = 0, r1 = 0;
        foreach (int i in indices)
        {
            Cell c = _cells[i];
            double distance = GeometryDistance.Hypotenuse(x - c.X, y - c.Y) - c.Radius;
            if (distance < d0) { d2 = d1; d1 = d0; r1 = r0; d0 = distance; r0 = c.Radius; }
            else if (distance < d1) { d2 = d1; d1 = distance; r1 = c.Radius; }
            else if (distance < d2) d2 = distance;
        }
        // Una superficie assente dai bin è necessariamente a distanza positiva
        // maggiore del margine. Se il secondo candidato supera il margine, la
        // ricerca esaustiva evita qualsiasi discontinuità tra celle della griglia.
        if (d2 > _searchMargin)
        {
            d0 = d1 = d2 = double.PositiveInfinity; r0 = r1 = 0;
            foreach (Cell c in _cells)
            {
                double distance = GeometryDistance.Hypotenuse(x - c.X, y - c.Y) - c.Radius;
                if (distance < d0) { d2 = d1; d1 = d0; r1 = r0; d0 = distance; r0 = c.Radius; }
                else if (distance < d1) { d2 = d1; d1 = distance; r1 = c.Radius; }
                else if (distance < d2) d2 = distance;
            }
        }
        return new NearestPair(d0, d1, d2, r0, r1);
    }

    /// <summary>
    /// Intensità di un nodo a tre celle: massima quando le tre superfici
    /// più vicine sono equidistanti; decresce senza discontinuità.
    /// </summary>
    public static double JunctionIntensity(NearestPair sample, double radius)
    {
        if (!double.IsFinite(radius) || radius <= 0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        double separation = Math.Max(0, (sample.Third - sample.First) * .5);
        return Math.Exp(-Math.Pow(separation / radius, 2));
    }
}
