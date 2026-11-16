using System.Numerics;

namespace GenerativeArt.Core.Geometry;

/// <summary>Operazioni geometriche pure su Vector2; nessuna dipendenza da Skia.</summary>
public static class Geometry2D
{
    public static Vector2 Rotate(Vector2 point, float radians)
    {
        if (!float.IsFinite(radians)) throw new ArgumentOutOfRangeException(nameof(radians));
        float c = MathF.Cos(radians);
        float s = MathF.Sin(radians);
        return new Vector2(point.X * c - point.Y * s, point.X * s + point.Y * c);
    }

    public static Vector2 Lerp(Vector2 from, Vector2 to, float amount)
    {
        if (!float.IsFinite(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
        return from + (to - from) * amount;
    }

    public static Vector2 PointOnCircle(Vector2 center, float radius, float angleRadians)
    {
        if (!float.IsFinite(radius) || radius < 0) throw new ArgumentOutOfRangeException(nameof(radius));
        if (!float.IsFinite(angleRadians)) throw new ArgumentOutOfRangeException(nameof(angleRadians));
        return center + new Vector2(MathF.Cos(angleRadians), MathF.Sin(angleRadians)) * radius;
    }

    /// <summary>Curva quadratica per 0 <= t <= 1.</summary>
    public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        ValidatePoint(p0, nameof(p0)); ValidatePoint(p1, nameof(p1)); ValidatePoint(p2, nameof(p2));
        ValidateUnit(t, nameof(t));
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    /// <summary>Curva cubica di Bézier per 0 <= t <= 1.</summary>
    public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        ValidatePoint(p0, nameof(p0)); ValidatePoint(p1, nameof(p1));
        ValidatePoint(p2, nameof(p2)); ValidatePoint(p3, nameof(p3));
        ValidateUnit(t, nameof(t));
        float u = 1f - t;
        return u*u*u*p0 + 3f*u*u*t*p1 + 3f*u*t*t*p2 + t*t*t*p3;
    }

    /// <summary>Derivata non normalizzata della curva cubica. Può essere il vettore zero.</summary>
    public static Vector2 CubicBezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        ValidatePoint(p0, nameof(p0)); ValidatePoint(p1, nameof(p1));
        ValidatePoint(p2, nameof(p2)); ValidatePoint(p3, nameof(p3));
        ValidateUnit(t, nameof(t));
        float u = 1f - t;
        return 3f*u*u*(p1-p0) + 6f*u*t*(p2-p1) + 3f*t*t*(p3-p2);
    }

    /// <summary>Distanza dal segmento; se A==B restituisce la distanza dal punto A.</summary>
    public static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        ValidatePoint(point, nameof(point)); ValidatePoint(a, nameof(a)); ValidatePoint(b, nameof(b));
        Vector2 ab = b - a;
        float squaredLength = ab.LengthSquared();
        if (squaredLength == 0f) return Vector2.Distance(point, a);
        float t = Math.Clamp(Vector2.Dot(point - a, ab) / squaredLength, 0f, 1f);
        return Vector2.Distance(point, a + t * ab);
    }

    private static void ValidatePoint(Vector2 point, string name)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
            throw new ArgumentOutOfRangeException(name, "Coordinate non finite.");
    }

    private static void ValidateUnit(float value, string name)
    {
        if (!float.IsFinite(value) || value < 0f || value > 1f)
            throw new ArgumentOutOfRangeException(name);
    }
}
