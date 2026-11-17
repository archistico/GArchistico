using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>Vortice locale che contribuisce alla direzione del campo, senza stato grafico.</summary>
public readonly record struct GartFlowWhirl(Vector2 Center, float Spin, float Reach);

/// <summary>
/// Primitive G3 riutilizzabili: tracce integrate in un campo di drift, vortici e onde
/// e indice spaziale a celle per l'arresto delle tracce che si avvicinano tra loro.
/// Nessuna dipendenza da Skia, Kotlin, UI o generatori di numeri casuali globali.
/// </summary>
public static class GartFlowGeometry2D
{
    public static Vector2 Direction(Vector2 position, float frequency, float drift,
        ReadOnlySpan<GartFlowWhirl> whirls, float waviness)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) ||
            !float.IsFinite(frequency) || frequency < 0 || !float.IsFinite(drift) ||
            !float.IsFinite(waviness))
            throw new ArgumentOutOfRangeException(nameof(position));

        double wave = Math.Sin(position.X * frequency * .73 + Math.Cos(position.Y * frequency)) * waviness;
        Vector2 v = new((float)(Math.Cos(wave) * drift), (float)(Math.Sin(wave) * drift));
        for (int i = 0; i < whirls.Length; i++)
        {
            GartFlowWhirl whirl = whirls[i];
            if (!float.IsFinite(whirl.Reach) || whirl.Reach <= 0 || !float.IsFinite(whirl.Spin))
                throw new ArgumentOutOfRangeException(nameof(whirls));
            Vector2 delta = position - whirl.Center;
            float distance = delta.Length();
            if (distance > whirl.Reach || distance < .001f) continue;
            float falloff = (1 - distance / whirl.Reach);
            Vector2 tangent = new(-delta.Y / distance, delta.X / distance);
            v += tangent * (whirl.Spin * falloff * falloff);
        }
        if (v.LengthSquared() < .000001f) return Vector2.UnitX;
        return Vector2.Normalize(v);
    }

    public static List<Vector2> Trace(Vector2 start, int steps, float stepSize,
        int width, int height, Func<Vector2, Vector2> direction,
        CancellationToken cancellationToken = default)
    {
        if (steps is < 1 or > 5000 || !float.IsFinite(stepSize) || stepSize <= 0 ||
            width < 1 || height < 1)
            throw new ArgumentOutOfRangeException(nameof(steps));
        ArgumentNullException.ThrowIfNull(direction);
        var result = new List<Vector2>(steps + 1);
        Vector2 p = start;
        for (int i = 0; i < steps; i++)
        {
            if ((i & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (!Inside(p, width, height)) break;
            result.Add(p);
            Vector2 v = direction(p);
            if (!FiniteAndNonZero(v)) break;
            v = Vector2.Normalize(v);
            Vector2 half = p + .5f * stepSize * v;
            Vector2 middle = direction(half);
            if (!FiniteAndNonZero(middle)) break;
            p += stepSize * Vector2.Normalize(middle);
        }
        if (Inside(p, width, height)) result.Add(p);
        return result;
    }

    private static bool Inside(Vector2 p, int width, int height) =>
        float.IsFinite(p.X) && float.IsFinite(p.Y) &&
        p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height;

    private static bool FiniteAndNonZero(Vector2 p) =>
        float.IsFinite(p.X) && float.IsFinite(p.Y) && p.LengthSquared() > .000001f;
}

/// <summary>
/// Indice delle tracce già depositate, indipendente dall'ordine di rendering.
/// Memoria proporzionale ai punti depositati; ricerca limitata alle celle vicine.
/// </summary>
public sealed class GartTrailOccupancy2D
{
    private readonly Dictionary<(int X, int Y), List<Vector2>> _cells = new();
    private readonly float _cellSize;
    public GartTrailOccupancy2D(float minimumDistance)
    {
        if (!float.IsFinite(minimumDistance) || minimumDistance <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumDistance));
        _cellSize = minimumDistance;
    }

    private (int, int) Cell(Vector2 p) =>
        ((int)MathF.Floor(p.X / _cellSize), (int)MathF.Floor(p.Y / _cellSize));

    public bool IsOccupied(Vector2 point, float radius)
    {
        if (!float.IsFinite(radius) || radius <= 0 || !float.IsFinite(point.X) || !float.IsFinite(point.Y))
            throw new ArgumentOutOfRangeException(nameof(radius));
        var (cx, cy) = Cell(point);
        int reach = (int)MathF.Ceiling(radius / _cellSize);
        float radius2 = radius * radius;
        for (int y = cy - reach; y <= cy + reach; y++)
            for (int x = cx - reach; x <= cx + reach; x++)
                if (_cells.TryGetValue((x, y), out var points))
                    foreach (Vector2 p in points)
                        if (Vector2.DistanceSquared(p, point) < radius2) return true;
        return false;
    }

    public void Deposit(IEnumerable<Vector2> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        foreach (Vector2 point in points)
        {
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                throw new ArgumentException("Coordinate non finite.", nameof(points));
            var cell = Cell(point);
            if (!_cells.TryGetValue(cell, out var content)) _cells[cell] = content = [];
            content.Add(point);
        }
    }
}
