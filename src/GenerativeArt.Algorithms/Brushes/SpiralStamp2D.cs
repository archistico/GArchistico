using System.Numerics;

namespace GenerativeArt.Algorithms.Brushes;

/// <summary>Traccia una spirale di punti intorno a un centro, utilizzabile come stampo per pennelli.</summary>
public static class SpiralStamp2D
{
    public static Vector2[] Points(Vector2 center, float radius, int count, double phase, double turns)
    {
        if (count is < 2 or > 256 || !float.IsFinite(radius) || radius <= 0 ||
            !float.IsFinite(center.X) || !float.IsFinite(center.Y) ||
            !double.IsFinite(phase) || !double.IsFinite(turns) || turns is < .25 or > 12)
            throw new ArgumentOutOfRangeException(nameof(count), "Spirale del pennello non valida.");
        var result = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            double t = (i + .5) / count;
            double a = phase + t * Math.Tau * turns;
            float r = radius * (float)Math.Sqrt(t);
            result[i] = center + new Vector2(r * (float)Math.Cos(a), r * (float)Math.Sin(a));
        }
        return result;
    }
}
