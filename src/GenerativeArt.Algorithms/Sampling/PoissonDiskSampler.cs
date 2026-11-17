using System.Numerics;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Sampling;

/// <summary>
/// Campionamento Bridson 2D, seeded e limitato: griglia spaziale, lista attiva
/// e massimo di tentativi per punto. Nessuna dipendenza da Skia o dall'interfaccia.
/// </summary>
public static class PoissonDiskSampler
{
    public const int MaximumGridCells = 6_000_000;
    public const int MaximumPoints = 50_000;
    public const int MaximumProductionPoints = 500_000;

    public static IReadOnlyList<Vector2> Sample(float width, float height, float minimumDistance,
        ulong seed, int maximumPoints = 2_500, int candidatesPerActive = 24,
        CancellationToken cancellationToken = default, bool production = false)
    {
        GridDimensions(width, height, minimumDistance, maximumPoints, candidatesPerActive, production,
            out int columns, out int rows, out float cellSize);
        cancellationToken.ThrowIfCancellationRequested();

        var rng = new SplitMix64Random(seed);
        int[] grid = new int[checked(columns * rows)]; // punto + 1, zero = libero
        var points = new List<Vector2>(Math.Min(maximumPoints, 256));
        var active = new List<int>();

        void Insert(Vector2 p)
        {
            int index = points.Count;
            points.Add(p);
            active.Add(index);
            int gx = Math.Min(columns - 1, (int)(p.X / cellSize));
            int gy = Math.Min(rows - 1, (int)(p.Y / cellSize));
            grid[gy * columns + gx] = index + 1;
        }

        // La conversione double -> float puo' arrotondare un valore appena
        // interno al bordo fino a width/height: mantenere anche il seed iniziale
        // nell'intervallo semiaperto, indipendentemente dal sistema.
        Insert(new Vector2(MathF.Min(MathF.BitDecrement(width), (float)(rng.NextDouble() * width)),
            MathF.Min(MathF.BitDecrement(height), (float)(rng.NextDouble() * height))));
        float minimumDistanceSquared = minimumDistance * minimumDistance;

        bool Safe(Vector2 point)
        {
            int gx = Math.Min(columns - 1, (int)(point.X / cellSize));
            int gy = Math.Min(rows - 1, (int)(point.Y / cellSize));
            for (int yy = Math.Max(0, gy - 2); yy <= Math.Min(rows - 1, gy + 2); yy++)
            for (int xx = Math.Max(0, gx - 2); xx <= Math.Min(columns - 1, gx + 2); xx++)
            {
                int neighbor = grid[yy * columns + xx];
                if (neighbor != 0 && Vector2.DistanceSquared(point, points[neighbor - 1]) < minimumDistanceSquared)
                    return false;
            }
            return true;
        }

        if (production)
        {
            // Distribuisce i punti sul dominio prima della crescita locale Bridson:
            // un limite finito non deve lasciare una sola isola ad alta risoluzione.
            int attempts = Math.Min(5_000_000, maximumPoints * 12);
            for (int i = 0; i < attempts && points.Count < maximumPoints; i++)
            {
                if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
                var point = new Vector2(MathF.Min(MathF.BitDecrement(width), (float)(rng.NextDouble() * width)),
                    MathF.Min(MathF.BitDecrement(height), (float)(rng.NextDouble() * height)));
                if (Safe(point)) Insert(point);
            }
        }

        while (active.Count > 0 && points.Count < maximumPoints)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int activeSlot = rng.NextInt32Inclusive(0, active.Count - 1);
            Vector2 origin = points[active[activeSlot]];
            bool accepted = false;
            for (int attempt = 0; attempt < candidatesPerActive; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double angle = 2d * Math.PI * rng.NextDouble();
                double radius = minimumDistance * Math.Sqrt(1d + 3d * rng.NextDouble());
                var point = new Vector2(
                    origin.X + (float)(radius * Math.Cos(angle)),
                    origin.Y + (float)(radius * Math.Sin(angle)));
                if (point.X < 0 || point.Y < 0 || point.X >= width || point.Y >= height)
                    continue;
                if (!Safe(point)) continue;
                Insert(point);
                accepted = true;
                break;
            }
            if (!accepted)
            {
                active[activeSlot] = active[^1];
                active.RemoveAt(active.Count - 1);
            }
        }
        return points.ToArray();
    }

    /// <summary>Valida senza allocare per impedire output eccessivi prima della scrittura.</summary>
    public static void Validate(float width, float height, float minimumDistance,
        int maximumPoints = 2_500, int candidatesPerActive = 24, bool production = false) =>
        GridDimensions(width, height, minimumDistance, maximumPoints, candidatesPerActive, production,
            out _, out _, out _);

    private static void GridDimensions(float width, float height, float minimumDistance,
        int maximumPoints, int candidatesPerActive, bool production, out int columns, out int rows, out float cellSize)
    {
        if (!float.IsFinite(width) || !float.IsFinite(height) || width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Area di campionamento non valida.");
        if (!float.IsFinite(minimumDistance) || minimumDistance <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumDistance));
        if (maximumPoints < 1 || maximumPoints > (production ? MaximumProductionPoints : MaximumPoints))
            throw new ArgumentOutOfRangeException(nameof(maximumPoints));
        if (candidatesPerActive is < 1 or > 60)
            throw new ArgumentOutOfRangeException(nameof(candidatesPerActive));
        cellSize = minimumDistance / MathF.Sqrt(2f);
        double cols = Math.Ceiling(width / (double)cellSize);
        double lines = Math.Ceiling(height / (double)cellSize);
        if (!double.IsFinite(cols) || !double.IsFinite(lines) ||
            cols > MaximumGridCells || lines > MaximumGridCells || cols * lines > MaximumGridCells)
            throw new ArgumentOutOfRangeException(nameof(minimumDistance),
                $"La griglia Poisson supera {MaximumGridCells:N0} celle.");
        columns = Math.Max(1, (int)cols);
        rows = Math.Max(1, (int)lines);
    }
}
