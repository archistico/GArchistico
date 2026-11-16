using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>Tracciamento deterministico di una singola traiettoria.</summary>
public static class FlowTracer
{
    public const int MaxSteps = 200_000;

    /// <summary>
    /// Include sempre il punto iniziale, se valido. Interrompe prima di uscire
    /// da bounds, o quando il campo non permette di avanzare. Il passo negativo
    /// percorre il flusso al contrario. Non modifica il campo.
    /// </summary>
    public static IReadOnlyList<Vector2> Trace(IVectorField2D field, Vector2 start,
        int steps, float step = 1f, FlowIntegrationMethod method = FlowIntegrationMethod.Rk2,
        FlowBounds? bounds = null, bool velocity = false)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (steps < 0 || steps > MaxSteps) throw new ArgumentOutOfRangeException(nameof(steps));
        if (!float.IsFinite(start.X) || !float.IsFinite(start.Y))
            throw new ArgumentOutOfRangeException(nameof(start));
        if (!float.IsFinite(step) || step == 0)
            throw new ArgumentOutOfRangeException(nameof(step));
        if (!Enum.IsDefined(method)) throw new ArgumentOutOfRangeException(nameof(method));
        if (bounds.HasValue && !bounds.Value.Contains(start))
            throw new ArgumentOutOfRangeException(nameof(start), "Il punto iniziale è esterno ai limiti.");

        var points = new List<Vector2>(Math.Min(steps + 1, 8192)) { start };
        Vector2 point = start;
        for (int i = 0; i < steps; i++)
        {
            if (!FlowIntegrator.TryAdvance(field, point, step, method, out Vector2 next, velocity)) break;
            if (bounds.HasValue && !bounds.Value.Contains(next)) break;
            points.Add(next);
            point = next;
        }
        return points;
    }
}
