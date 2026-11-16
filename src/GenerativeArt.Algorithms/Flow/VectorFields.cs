using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>Factory di campi vettoriali indipendenti da SkiaSharp.</summary>
public static class VectorFields
{
    public static IVectorField2D FromFunction(Func<float, float, Vector2> sample)
    {
        ArgumentNullException.ThrowIfNull(sample);
        return new DelegateField(sample);
    }

    public static IVectorField2D Constant(Vector2 direction)
    {
        EnsureFinite(direction, nameof(direction));
        return new DelegateField((_, _) => direction);
    }

    /// <summary>Angoli in radianti: 0 verso destra, π/2 verso il basso.</summary>
    public static IVectorField2D Angles(Func<float, float, float> angle)
    {
        ArgumentNullException.ThrowIfNull(angle);
        return new DelegateField((x, y) =>
        {
            float a = angle(x, y);
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        });
    }

    /// <summary>
    /// Vortice intorno a center. spin positivo ruota in senso orario sul canvas;
    /// pull positivo punta al centro. Al centro il campo vale (0,0).
    /// reach opzionale attenua l'ampiezza come 1/(1+(r/reach)^2).
    /// </summary>
    public static IVectorField2D Vortex(Vector2 center, float spin = 1f,
        float pull = 0f, float reach = float.PositiveInfinity)
    {
        EnsureFinite(center, nameof(center));
        if (!float.IsFinite(spin)) throw new ArgumentOutOfRangeException(nameof(spin));
        if (!float.IsFinite(pull)) throw new ArgumentOutOfRangeException(nameof(pull));
        if (!(float.IsPositiveInfinity(reach) || (float.IsFinite(reach) && reach > 0)))
            throw new ArgumentOutOfRangeException(nameof(reach));

        return new DelegateField((x, y) =>
        {
            double dx = (double)x - center.X;
            double dy = (double)y - center.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance == 0) return Vector2.Zero;
            double fade = float.IsPositiveInfinity(reach)
                ? 1d : 1d / (1d + (distance / reach) * (distance / reach));
            double scale = fade / distance;
            return new Vector2((float)((-dy * spin - dx * pull) * scale),
                               (float)((dx * spin - dy * pull) * scale));
        });
    }

    public static IVectorField2D Add(IVectorField2D first, IVectorField2D second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return new DelegateField((x, y) => first.Sample(x, y) + second.Sample(x, y));
    }

    public static IVectorField2D Scale(IVectorField2D field, float factor)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (!float.IsFinite(factor)) throw new ArgumentOutOfRangeException(nameof(factor));
        return new DelegateField((x, y) => field.Sample(x, y) * factor);
    }

    private static void EnsureFinite(Vector2 value, string name)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
            throw new ArgumentOutOfRangeException(name);
    }

    private sealed class DelegateField(Func<float, float, Vector2> sample) : IVectorField2D
    {
        public Vector2 Sample(float x, float y) => sample(x, y);
    }
}
