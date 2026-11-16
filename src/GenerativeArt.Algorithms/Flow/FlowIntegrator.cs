using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>
/// Integrazione di un campo vettoriale in coordinate canvas.
/// La modalità direction (default) normalizza ogni campione e interpreta step
/// come distanza; velocity=true conserva il modulo e interpreta step come tempo.
/// I campioni nulli, non finiti o che producono un passo nullo interrompono la traccia.
/// </summary>
public static class FlowIntegrator
{
    private const double Still = 1e-6;
    private const double NoProgressFraction = 1e-3;

    public static bool TryAdvance(IVectorField2D field, Vector2 start, float step,
        FlowIntegrationMethod method, out Vector2 next, bool velocity = false)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (!float.IsFinite(step) || step == 0f)
            throw new ArgumentOutOfRangeException(nameof(step), "Il passo deve essere finito e diverso da zero.");
        if (!Enum.IsDefined(method)) throw new ArgumentOutOfRangeException(nameof(method));
        if (!IsFinite(start)) throw new ArgumentOutOfRangeException(nameof(start));

        next = start;
        bool unit = !velocity;
        if (!TrySample(field, start, unit, out Vector2 k1)) return false;

        Vector2 result;
        switch (method)
        {
            case FlowIntegrationMethod.Euler:
                if (!TryAdd(start, step, k1, out result)) return false;
                break;
            case FlowIntegrationMethod.Rk2:
                if (!TryAdd(start, (double)step * .5d, k1, out Vector2 middle)
                    || !TrySample(field, middle, unit, out Vector2 k2)
                    || !TryAdd(start, step, k2, out result)) return false;
                break;
            case FlowIntegrationMethod.Rk4:
                if (!TryAdd(start, (double)step * .5d, k1, out Vector2 m2)
                    || !TrySample(field, m2, unit, out Vector2 r2)
                    || !TryAdd(start, (double)step * .5d, r2, out Vector2 m3)
                    || !TrySample(field, m3, unit, out Vector2 r3)
                    || !TryAdd(start, step, r3, out Vector2 m4)
                    || !TrySample(field, m4, unit, out Vector2 r4)) return false;

                double newX = start.X + (double)step / 6d *
                    (k1.X + 2d * r2.X + 2d * r3.X + r4.X);
                double newY = start.Y + (double)step / 6d *
                    (k1.Y + 2d * r2.Y + 2d * r3.Y + r4.Y);
                if (!TryPoint(newX, newY, out result)) return false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(method));
        }

        if (result == start) return false;
        double dx = (double)result.X - start.X;
        double dy = (double)result.Y - start.Y;
        if (unit && Math.Sqrt(dx * dx + dy * dy) < NoProgressFraction * Math.Abs((double)step))
            return false;
        next = result;
        return true;
    }

    private static bool TrySample(IVectorField2D field, Vector2 point, bool normalize,
        out Vector2 direction)
    {
        direction = field.Sample(point.X, point.Y);
        if (!IsFinite(direction)) return false;
        double length = Math.Sqrt((double)direction.X * direction.X
                                  + (double)direction.Y * direction.Y);
        if (!(length > Still)) return false;
        if (normalize)
            direction = new Vector2((float)(direction.X / length), (float)(direction.Y / length));
        return true;
    }

    private static bool TryAdd(Vector2 start, double step, Vector2 velocity, out Vector2 result)
        => TryPoint(start.X + step * velocity.X, start.Y + step * velocity.Y, out result);

    private static bool TryPoint(double x, double y, out Vector2 result)
    {
        result = default;
        if (!double.IsFinite(x) || !double.IsFinite(y)
            || Math.Abs(x) > float.MaxValue || Math.Abs(y) > float.MaxValue) return false;
        result = new Vector2((float)x, (float)y);
        return IsFinite(result);
    }

    private static bool IsFinite(Vector2 point) =>
        float.IsFinite(point.X) && float.IsFinite(point.Y);
}
