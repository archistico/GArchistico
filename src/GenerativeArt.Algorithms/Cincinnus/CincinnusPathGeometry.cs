using System.Numerics;
using GenerativeArt.Algorithms.Flow;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// Tracce Cincinnus senza rasterizzazione: passo direzionale RK4, bordo esteso,
/// ritorni quasi chiusi e raggi di curvatura stimati su una finestra locale.
/// Le routine restano utilizzabili con campi sintetici nei test.
/// </summary>
public static class CincinnusPathGeometry
{
    public const int MaximumStepsPerDirection = 20_000;
    public const float StraightBendRadius = 1_000_000f;

    public static IReadOnlyList<Vector2> TraceForward(IVectorField2D field, Vector2 start,
        int width, int height, CincinnusTraceOptions? options = null,
        CancellationToken cancellationToken = default)
        => TraceDirection(field, start, width, height, options, reverse: false, cancellationToken: cancellationToken);

    public static IReadOnlyList<Vector2> TraceBackward(IVectorField2D field, Vector2 start,
        int width, int height, CincinnusTraceOptions? options = null,
        CancellationToken cancellationToken = default)
        => TraceDirection(field, start, width, height, options, reverse: true, cancellationToken: cancellationToken);

    /// <summary>
    /// Traccia entrambe le direzioni e concatena i rami senza duplicare il seed.
    /// È una primitiva per le fasi successive, NON il posizionatore Simple/Adaptive.
    /// </summary>
    public static IReadOnlyList<Vector2> TraceBidirectional(IVectorField2D field, Vector2 start,
        int width, int height, CincinnusTraceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Vector2> backward = TraceBackward(field, start, width, height, options, cancellationToken);
        IReadOnlyList<Vector2> forward = TraceForward(field, start, width, height, options, cancellationToken);
        var result = new List<Vector2>(backward.Count + forward.Count - 1);
        for (int i = backward.Count - 1; i > 0; i--) result.Add(backward[i]);
        result.AddRange(forward);
        return result;
    }

    /// <summary>
    /// Primo indice dopo il distacco da start (&gt; minDistanceAway) che torna
    /// entro returnStepFactor*step dal punto iniziale. -1 se la curva non si chiude.
    /// Cincinnus.kt usa 30 px e 2 passi sul cammino in avanti.
    /// </summary>
    public static int FindFirstReturn(IReadOnlyList<Vector2> points, float stepSize,
        float minDistanceAway = 30f, float returnStepFactor = 2f)
    {
        ArgumentNullException.ThrowIfNull(points);
        CheckPositive(stepSize, nameof(stepSize));
        CheckPositive(minDistanceAway, nameof(minDistanceAway));
        CheckPositive(returnStepFactor, nameof(returnStepFactor));
        ValidatePoints(points);
        if (points.Count < 2) return -1;

        double awaySquared = (double)minDistanceAway * minDistanceAway;
        double returnDistance = (double)returnStepFactor * stepSize;
        double returnSquared = returnDistance * returnDistance;
        bool wentAway = false;
        Vector2 start = points[0];
        for (int i = 1; i < points.Count; i++)
        {
            double dx = (double)points[i].X - start.X;
            double dy = (double)points[i].Y - start.Y;
            double distanceSquared = dx * dx + dy * dy;
            if (distanceSquared > awaySquared) wentAway = true;
            else if (wentAway && distanceSquared < returnSquared) return i;
        }
        return -1;
    }

    /// <summary>
    /// Stima r = (i1-i0)*step / |wrapPi(heading[i1]-heading[i0])|,
    /// come bends() di Cincinnus.kt. Il valore 1e6 rappresenta una curva dritta
    /// o numericamente senza rotazione; non è una misura fisica esatta.
    /// </summary>
    public static float[] EstimateBendRadii(IReadOnlyList<Vector2> points,
        float stepSize = 3f, int headingWindow = 4)
    {
        ArgumentNullException.ThrowIfNull(points);
        CheckPositive(stepSize, nameof(stepSize));
        if (headingWindow < 1 || headingWindow > 1024)
            throw new ArgumentOutOfRangeException(nameof(headingWindow));
        ValidatePoints(points);
        if (points.Count < 2) throw new ArgumentException("Servono almeno due punti.", nameof(points));

        int n = points.Count;
        var result = new float[n];
        for (int k = 0; k < n; k++)
        {
            int i0 = Math.Clamp(k - headingWindow, 0, n - 2);
            int i1 = Math.Clamp(k + headingWindow, 0, n - 2);
            Vector2 a = points[i0 + 1] - points[i0];
            Vector2 b = points[i1 + 1] - points[i1];
            float angle0 = MathF.Atan2(a.Y, a.X);
            float angle1 = MathF.Atan2(b.Y, b.X);
            float turn = angle1 - angle0;
            if (turn > MathF.PI) turn -= 2f * MathF.PI;
            if (turn < -MathF.PI) turn += 2f * MathF.PI;
            result[k] = i0 == i1 || MathF.Abs(turn) < 1e-4f
                ? StraightBendRadius
                : (float)((i1 - i0) * (double)stepSize / MathF.Abs(turn));
        }
        return result;
    }

    private static IReadOnlyList<Vector2> TraceDirection(IVectorField2D field, Vector2 start,
        int width, int height, CincinnusTraceOptions? options, bool reverse,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (width is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(width));
        if (height is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(height));
        options ??= new CincinnusTraceOptions();
        CheckPositive(options.StepSize, nameof(options.StepSize));
        CheckPositive(options.MaximumLength, nameof(options.MaximumLength));
        if (!float.IsFinite(options.Margin) || options.Margin < 0 || options.Margin > 100_000)
            throw new ArgumentOutOfRangeException(nameof(options.Margin));
        double steps = Math.Floor((double)options.MaximumLength / options.StepSize);
        if (steps < 1 || steps > MaximumStepsPerDirection)
            throw new ArgumentOutOfRangeException(nameof(options.MaximumLength),
                "La lunghezza deve permettere da 1 a 20.000 passi per direzione.");
        var bounds = new FlowBounds(-options.Margin, -options.Margin,
            width + options.Margin, height + options.Margin);
        return FlowTracer.Trace(field, start, (int)steps,
            reverse ? -options.StepSize : options.StepSize,
            FlowIntegrationMethod.Rk4, bounds, cancellationToken: cancellationToken);
    }

    private static void CheckPositive(float value, string argumentName)
    {
        if (!float.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(argumentName);
    }

    private static void ValidatePoints(IReadOnlyList<Vector2> points)
    {
        foreach (Vector2 point in points)
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                throw new ArgumentException("La traccia contiene coordinate non finite.", nameof(points));
    }
}
