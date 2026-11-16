using System.Numerics;

namespace GenerativeArt.Algorithms.Geometry;

/// <summary>
/// Primitive signed-distance 2D (negativo all'interno, positivo all'esterno)
/// e operazioni CSG. Le misure sono espresse nelle unita' del chiamante.
/// </summary>
public static class SignedDistance2D
{
    public static float Circle(Vector2 point, float radius)
    {
        if (!float.IsFinite(radius) || radius <= 0)
            throw new ArgumentOutOfRangeException(nameof(radius));
        return point.Length() - radius;
    }

    public static float Box(Vector2 point, Vector2 halfExtents)
    {
        if (!float.IsFinite(halfExtents.X) || !float.IsFinite(halfExtents.Y) ||
            halfExtents.X <= 0 || halfExtents.Y <= 0)
            throw new ArgumentOutOfRangeException(nameof(halfExtents));
        Vector2 q = Vector2.Abs(point) - halfExtents;
        return Vector2.Max(q, Vector2.Zero).Length() + MathF.Min(MathF.Max(q.X, q.Y), 0);
    }

    public static float RoundedBox(Vector2 point, Vector2 halfExtents, float radius)
    {
        if (!float.IsFinite(radius) || radius < 0 ||
            !float.IsFinite(halfExtents.X) || !float.IsFinite(halfExtents.Y) ||
            halfExtents.X <= radius || halfExtents.Y <= radius)
            throw new ArgumentOutOfRangeException(nameof(radius), "Il raggio deve essere minore di entrambe le semidimensioni.");
        return Box(point, halfExtents - new Vector2(radius)) - radius;
    }

    public static Vector2 Rotate(Vector2 point, float angleRadians)
    {
        if (!float.IsFinite(angleRadians)) throw new ArgumentOutOfRangeException(nameof(angleRadians));
        float sin = MathF.Sin(angleRadians), cos = MathF.Cos(angleRadians);
        return new Vector2(cos * point.X - sin * point.Y, sin * point.X + cos * point.Y);
    }

    public static float Union(float a, float b) => MathF.Min(a, b);
    public static float Intersection(float a, float b) => MathF.Max(a, b);
    public static float Difference(float a, float b) => MathF.Max(a, -b);

    /// <summary>Unione morbida polinomiale, k positivo: k piccola tende all'unione ordinaria.</summary>
    public static float SmoothUnion(float a, float b, float k)
    {
        if (!float.IsFinite(k) || k <= 0) throw new ArgumentOutOfRangeException(nameof(k));
        float h = Math.Clamp(.5f + .5f * (b - a) / k, 0f, 1f);
        return b + (a - b) * h - k * h * (1f - h);
    }
}
