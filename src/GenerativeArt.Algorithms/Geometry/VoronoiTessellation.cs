using System.Numerics;

namespace GenerativeArt.Algorithms.Geometry;

/// <summary>Una cella Voronoi convessa, ritagliata nel rettangolo richiesto.</summary>
public sealed class VoronoiCell
{
    public Vector2 Site { get; }
    public IReadOnlyList<Vector2> Vertices { get; }
    public Vector2 Centroid { get; }
    public double Area { get; }

    internal VoronoiCell(Vector2 site, Vector2[] vertices, Vector2 centroid, double area)
    {
        Site = site;
        Vertices = Array.AsReadOnly(vertices);
        Centroid = centroid;
        Area = area;
    }
}

/// <summary>
/// Voronoi 2D delimitato, deterministico e senza Skia: per ogni seme ritaglia
/// un poligono convesso con i semipiani dei siti rivali. Adatto a un numero
/// limitato di celle; non è un'implementazione Fortune per grandi insiemi.
/// </summary>
public static class VoronoiTessellation
{
    public const int MaximumSites = 256;
    public const int MaximumRelaxations = 4;

    public static IReadOnlyList<VoronoiCell> Build(IReadOnlyList<Vector2> sites,
        float width, float height, CancellationToken cancellationToken = default)
    {
        Validate(sites, width, height);
        cancellationToken.ThrowIfCancellationRequested();
        var result = new VoronoiCell[sites.Count];
        for (int i = 0; i < sites.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var polygon = new List<PointD>(4)
            {
                new(0, 0), new(width, 0), new(width, height), new(0, height)
            };
            Vector2 site = sites[i];
            for (int j = 0; j < sites.Count; j++)
            {
                if (i == j) continue;
                if ((j & 15) == 0) cancellationToken.ThrowIfCancellationRequested();
                Vector2 other = sites[j];
                double dx = other.X - (double)site.X;
                double dy = other.Y - (double)site.Y;
                // Le coordinate alla stessa distanza soddisfano a*x + b*y = c.
                double a = 2d * dx, b = 2d * dy;
                double c = (double)other.X * other.X + (double)other.Y * other.Y -
                    (double)site.X * site.X - (double)site.Y * site.Y;
                polygon = Clip(polygon, a, b, c);
                if (polygon.Count == 0) break;
            }
            if (polygon.Count < 3)
                throw new InvalidOperationException("Voronoi: cella degenere; controllare i semi.");
            Vector2[] vertices = polygon.Select(p => new Vector2(
                Math.Clamp((float)p.X, 0, width), Math.Clamp((float)p.Y, 0, height))).ToArray();
            (Vector2 centroid, double area) = Measure(polygon);
            if (area <= 0 || !double.IsFinite(area))
                throw new InvalidOperationException("Voronoi: area della cella non valida.");
            result[i] = new VoronoiCell(site, vertices, centroid, area);
        }
        return Array.AsReadOnly(result);
    }

    /// <summary>Centroidi iterativi di Lloyd, mantenendo invariato l'ordine dei siti.</summary>
    public static IReadOnlyList<Vector2> Relax(IReadOnlyList<Vector2> sites,
        float width, float height, int iterations, CancellationToken cancellationToken = default)
    {
        Validate(sites, width, height);
        if (iterations < 0 || iterations > MaximumRelaxations)
            throw new ArgumentOutOfRangeException(nameof(iterations));
        cancellationToken.ThrowIfCancellationRequested();
        Vector2[] current = sites.ToArray();
        for (int k = 0; k < iterations; k++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<VoronoiCell> cells = Build(current, width, height, cancellationToken);
            for (int i = 0; i < current.Length; i++)
                current[i] = new Vector2(
                    Math.Clamp(cells[i].Centroid.X, 0, MathF.BitDecrement(width)),
                    Math.Clamp(cells[i].Centroid.Y, 0, MathF.BitDecrement(height)));
        }
        return Array.AsReadOnly(current);
    }

    private static void Validate(IReadOnlyList<Vector2> sites, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(sites);
        if (!float.IsFinite(width) || !float.IsFinite(height) || width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Area Voronoi non valida.");
        if (sites.Count < 1 || sites.Count > MaximumSites)
            throw new ArgumentOutOfRangeException(nameof(sites), $"Da 1 a {MaximumSites} semi.");
        for (int i = 0; i < sites.Count; i++)
        {
            Vector2 site = sites[i];
            if (!float.IsFinite(site.X) || !float.IsFinite(site.Y) ||
                site.X < 0 || site.Y < 0 || site.X >= width || site.Y >= height)
                throw new ArgumentOutOfRangeException(nameof(sites), "Seme non finito o fuori dall'area.");
            for (int j = 0; j < i; j++)
                if (Vector2.DistanceSquared(site, sites[j]) < 1e-8f)
                    throw new ArgumentException("Semi Voronoi coincidenti o troppo vicini.", nameof(sites));
        }
    }

    private static List<PointD> Clip(List<PointD> polygon, double a, double b, double c)
    {
        var result = new List<PointD>(polygon.Count + 2);
        if (polygon.Count == 0) return result;
        PointD previous = polygon[^1];
        double previousDistance = a * previous.X + b * previous.Y - c;
        bool previousInside = previousDistance <= 0;
        foreach (PointD current in polygon)
        {
            double distance = a * current.X + b * current.Y - c;
            bool currentInside = distance <= 0;
            if (previousInside != currentInside)
            {
                double ratio = previousDistance / (previousDistance - distance);
                ratio = Math.Clamp(ratio, 0, 1);
                result.Add(new PointD(
                    previous.X + (current.X - previous.X) * ratio,
                    previous.Y + (current.Y - previous.Y) * ratio));
            }
            if (currentInside) result.Add(current);
            previous = current;
            previousDistance = distance;
            previousInside = currentInside;
        }
        // Tagli che passano per un vertice possono generare punti adiacenti
        // duplicati. Scartarli evita micro-spigoli e disturbi nel rendering.
        for (int i = result.Count - 1; i >= 0 && result.Count > 1; i--)
        {
            PointD p = result[i], q = result[(i + 1) % result.Count];
            if ((p.X - q.X) * (p.X - q.X) + (p.Y - q.Y) * (p.Y - q.Y) < 1e-18)
                result.RemoveAt(i);
        }
        return result;
    }

    private static (Vector2 Centroid, double Area) Measure(List<PointD> polygon)
    {
        double twiceArea = 0, weightedX = 0, weightedY = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            PointD a = polygon[i], b = polygon[(i + 1) % polygon.Count];
            double cross = a.X * b.Y - b.X * a.Y;
            twiceArea += cross;
            weightedX += (a.X + b.X) * cross;
            weightedY += (a.Y + b.Y) * cross;
        }
        if (twiceArea <= 0 || !double.IsFinite(twiceArea))
            return (Vector2.Zero, 0);
        return (new Vector2((float)(weightedX / (3 * twiceArea)),
            (float)(weightedY / (3 * twiceArea))), twiceArea * .5);
    }

    private readonly record struct PointD(double X, double Y);
}
