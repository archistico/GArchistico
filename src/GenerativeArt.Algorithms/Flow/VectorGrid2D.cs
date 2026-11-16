using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>
/// Campo vettoriale immutabile campionato ai nodi di una griglia regolare.
/// Il dominio dei campioni include entrambi i bordi; fuori dominio restituisce zero.
/// Il colore, la GUI e Skia non sono dipendenze di questo algoritmo.
/// </summary>
public sealed class VectorGrid2D : IVectorField2D
{
    public const int MaximumNodes = 4_000_000;
    private readonly Vector2[] _nodes;

    public FlowBounds Domain { get; }
    public int Columns { get; }
    public int Rows { get; }

    public VectorGrid2D(FlowBounds domain, int columns, int rows, IReadOnlyList<Vector2> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ValidateDomain(domain);
        long count = (long)columns * rows;
        if (columns < 2 || rows < 2 || count > MaximumNodes)
            throw new ArgumentOutOfRangeException(nameof(columns), "Occorrono almeno due nodi per asse e al massimo quattro milioni in totale.");
        if (nodes.Count != count)
            throw new ArgumentException("Il numero dei vettori deve coincidere con Columns * Rows.", nameof(nodes));

        _nodes = new Vector2[(int)count];
        for (int i = 0; i < _nodes.Length; i++)
        {
            Vector2 v = nodes[i];
            if (!float.IsFinite(v.X) || !float.IsFinite(v.Y))
                throw new ArgumentOutOfRangeException(nameof(nodes), "Tutti i vettori devono essere finiti.");
            _nodes[i] = v;
        }
        Domain = domain;
        Columns = columns;
        Rows = rows;
    }

    /// <summary>Campiona il campo continuo ai nodi, mantenendo un buffer privato.</summary>
    public static VectorGrid2D FromField(FlowBounds domain, int columns, int rows, IVectorField2D field)
    {
        ArgumentNullException.ThrowIfNull(field);
        ValidateDomain(domain);
        long count = (long)columns * rows;
        if (columns < 2 || rows < 2 || count > MaximumNodes)
            throw new ArgumentOutOfRangeException(nameof(columns));
        var nodes = new Vector2[(int)count];
        double dx = ((double)domain.Right - domain.Left) / (columns - 1);
        double dy = ((double)domain.Bottom - domain.Top) / (rows - 1);
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                float sx = x == columns - 1 ? domain.Right : (float)(domain.Left + x * dx);
                float sy = y == rows - 1 ? domain.Bottom : (float)(domain.Top + y * dy);
                nodes[y * columns + x] = field.Sample(sx, sy);
            }
        return new VectorGrid2D(domain, columns, rows, nodes);
    }

    private static void ValidateDomain(FlowBounds domain)
    {
        if (!float.IsFinite(domain.Left) || !float.IsFinite(domain.Top)
            || !float.IsFinite(domain.Right) || !float.IsFinite(domain.Bottom)
            || !(domain.Right > domain.Left) || !(domain.Bottom > domain.Top))
            throw new ArgumentOutOfRangeException(nameof(domain), "Dominio della griglia non valido.");
    }

    public Vector2 Sample(float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y)
            || x < Domain.Left || x > Domain.Right || y < Domain.Top || y > Domain.Bottom)
            return Vector2.Zero;

        double gx = ((double)x - Domain.Left) / ((double)Domain.Right - Domain.Left) * (Columns - 1);
        double gy = ((double)y - Domain.Top) / ((double)Domain.Bottom - Domain.Top) * (Rows - 1);
        int ix = Math.Clamp((int)gx, 0, Columns - 2);
        int iy = Math.Clamp((int)gy, 0, Rows - 2);
        double tx = Math.Clamp(gx - ix, 0d, 1d);
        double ty = Math.Clamp(gy - iy, 0d, 1d);
        int a = iy * Columns + ix;
        Vector2 p00 = _nodes[a], p10 = _nodes[a + 1];
        Vector2 p01 = _nodes[a + Columns], p11 = _nodes[a + Columns + 1];
        double x0 = p00.X + (p10.X - (double)p00.X) * tx;
        double y0 = p00.Y + (p10.Y - (double)p00.Y) * tx;
        double x1 = p01.X + (p11.X - (double)p01.X) * tx;
        double y1 = p01.Y + (p11.Y - (double)p01.Y) * tx;
        return new Vector2((float)(x0 + (x1 - x0) * ty), (float)(y0 + (y1 - y0) * ty));
    }
}
