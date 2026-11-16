using System.Numerics;

namespace GenerativeArt.Algorithms.Flow;

/// <summary>
/// Tracciamento bidirezionale RK4 ispirato al metodo Jobard–Lefer.
/// Usa un indice spaziale per la distanza dalle linee già accettate.
/// Le distanze sono controllate tramite campionamento dei segmenti, non è una
/// dimostrazione geometrica esatta della distanza fra curve continue.
/// La coda e il numero delle linee hanno limiti espliciti.
/// </summary>
public static class EvenStreamlineTracer
{
    public static IReadOnlyList<IReadOnlyList<Vector2>> Trace(IVectorField2D field,
        EvenStreamlineOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(options);
        Validate(options);
        cancellationToken.ThrowIfCancellationRequested();

        float separation = options.Separation;
        float dTest = separation * options.TestDistanceFactor;
        var index = new PointIndex(options.Bounds, dTest);
        var result = new List<IReadOnlyList<Vector2>>();
        var queue = new Queue<Vector2>();
        int candidates = 0;

        double spacing = separation * 1.15d;
        double xmin = (double)options.Bounds.Left + options.Margin;
        double xmax = (double)options.Bounds.Right - options.Margin;
        double ymin = (double)options.Bounds.Top + options.Margin;
        double ymax = (double)options.Bounds.Bottom - options.Margin;
        double initialColumns = Math.Max(0, Math.Ceiling((xmax - xmin) / spacing - .5));
        double initialRows = Math.Max(0, Math.Ceiling((ymax - ymin) / spacing - .5));
        if (options.DistributeSeedBudget && initialColumns * initialRows > options.MaxCandidates)
            // Un reticolo troncato per righe lascerebbe vuota la parte bassa.
            // Distribuisce invece il budget sull'intero dominio e riserva semi laterali.
            spacing = Math.Max(spacing, Math.Sqrt((xmax - xmin) * (ymax - ymin)
                / Math.Max(1d, options.MaxCandidates * .75d)));

        bool Inside(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y)
            && p.X >= xmin && p.X < xmax && p.Y >= ymin && p.Y < ymax;

        void AddCandidate(Vector2 p)
        {
            if (candidates >= options.MaxCandidates || !Inside(p)) return;
            queue.Enqueue(p);
            candidates++;
        }

        // Reticolo iniziale deterministico con ordine di enumerazione stabile.
        for (double y = ymin + spacing * .5d; y < ymax && candidates < options.MaxCandidates; y += spacing)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (double x = xmin + spacing * .5d; x < xmax && candidates < options.MaxCandidates; x += spacing)
                AddCandidate(new Vector2((float)x, (float)y));
        }

        if (queue.Count == 0)
            AddCandidate(new Vector2((float)((xmin + xmax) * .5d), (float)((ymin + ymax) * .5d)));

        List<Vector2> TraceDirection(Vector2 seed, int sign)
        {
            var side = new List<Vector2>();
            Vector2 at = seed;
            for (int i = 0; i < options.MaxStepsPerDirection; i++)
            {
                if ((i & 15) == 0) cancellationToken.ThrowIfCancellationRequested();
                if (!FlowIntegrator.TryAdvance(field, at, sign * options.StepSize,
                    FlowIntegrationMethod.Rk4, out Vector2 next)) break;
                if (!Inside(next) || !index.ClearSegment(at, next, dTest)) break;
                // Interrompe i cappi senza penalizzare la normale distanza tra punti adiacenti.
                int skip = Math.Max(4, (int)Math.Ceiling(dTest / options.StepSize) + 2);
                bool loop = false;
                for (int j = 0; j + skip < side.Count; j++)
                {
                    if (Vector2.DistanceSquared(side[j], next) < dTest * dTest)
                    {
                        loop = true;
                        break;
                    }
                }
                if (loop) break;
                side.Add(next);
                at = next;
            }
            return side;
        }

        while (queue.Count > 0 && result.Count < options.MaxLines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Vector2 seed = queue.Dequeue();
            if (!index.ClearPoint(seed, separation * .9f)) continue;
            List<Vector2> backward = TraceDirection(seed, -1);
            List<Vector2> forward = TraceDirection(seed, +1);
            if (backward.Count + forward.Count + 1 < options.MinimumPoints) continue;
            var line = new List<Vector2>(backward.Count + forward.Count + 1);
            for (int j = backward.Count - 1; j >= 0; j--) line.Add(backward[j]);
            line.Add(seed);
            line.AddRange(forward);
            if (!index.TryInsertPolyline(line, dTest * .4f, 2_000_000)) break;
            result.Add(line);

            // Nuovi semi ai due lati della normale, come nello schema Jobard–Lefer.
            for (int i = 1; i + 1 < line.Count; i += 2)
            {
                Vector2 tangent = line[i + 1] - line[i - 1];
                double len = Math.Sqrt((double)tangent.X * tangent.X + (double)tangent.Y * tangent.Y);
                if (!(len > 1e-6)) continue;
                Vector2 normal = new((float)(-tangent.Y / len), (float)(tangent.X / len));
                Vector2 offset = normal * (separation * 1.05f);
                Vector2 left = line[i] + offset, right = line[i] - offset;
                if (Inside(left) && index.ClearPoint(left, separation * .9f)) AddCandidate(left);
                if (Inside(right) && index.ClearPoint(right, separation * .9f)) AddCandidate(right);
            }
        }
        return result;
    }

    private static void Validate(EvenStreamlineOptions c)
    {
        if (!float.IsFinite(c.Separation) || c.Separation < .5f
            || !float.IsFinite(c.TestDistanceFactor) || c.TestDistanceFactor < .2f || c.TestDistanceFactor > 1f
            || c.Separation > 100_000f
            || !float.IsFinite(c.StepSize) || c.StepSize < .1f || c.StepSize > 100_000f
            || !float.IsFinite(c.Margin) || c.Margin < 0f
            || !((double)c.Bounds.Right - c.Bounds.Left > 2d * c.Margin)
            || !((double)c.Bounds.Bottom - c.Bounds.Top > 2d * c.Margin)
            || (double)c.Bounds.Right - c.Bounds.Left > 100_000d
            || (double)c.Bounds.Bottom - c.Bounds.Top > 100_000d
            || c.MaxStepsPerDirection < 1 || c.MaxStepsPerDirection > 2000
            || c.MinimumPoints < 2 || c.MinimumPoints > 2 * c.MaxStepsPerDirection + 1
            || c.MaxLines < 1 || c.MaxLines > 10_000
            || c.MaxCandidates < 1 || c.MaxCandidates > 200_000)
            throw new ArgumentOutOfRangeException(nameof(c), "Parametri delle streamlines non validi.");
    }

    private sealed class PointIndex
    {
        private readonly FlowBounds _domain;
        private readonly double _cell;
        private readonly Dictionary<long, List<Vector2>> _buckets = new();
        private int _storedSamples;

        public PointIndex(FlowBounds domain, float separation)
        {
            _domain = domain;
            _cell = separation;
        }

        private (int x, int y) Cell(Vector2 p) =>
            ((int)Math.Floor(((double)p.X - _domain.Left) / _cell),
             (int)Math.Floor(((double)p.Y - _domain.Top) / _cell));

        private static long Key(int x, int y) => ((long)x << 32) | (uint)y;

        public bool ClearPoint(Vector2 p, float minDistance)
        {
            (int cx, int cy) = Cell(p);
            int radius = (int)Math.Ceiling(minDistance / _cell);
            double limit = (double)minDistance * minDistance;
            for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (!_buckets.TryGetValue(Key(cx + dx, cy + dy), out List<Vector2>? items)) continue;
                    foreach (Vector2 q in items)
                    {
                        double ax = (double)p.X - q.X, ay = (double)p.Y - q.Y;
                        if (ax * ax + ay * ay < limit) return false;
                    }
                }
            return true;
        }

        public bool ClearSegment(Vector2 start, Vector2 end, float minDistance)
        {
            double length = Vector2.Distance(start, end);
            int samples = Math.Max(1, (int)Math.Ceiling(length / (minDistance * .4d)));
            if (samples > 100_000) return false;
            for (int i = 1; i <= samples; i++)
            {
                float t = (float)((double)i / samples);
                if (!ClearPoint(Vector2.Lerp(start, end, t), minDistance)) return false;
            }
            return true;
        }

        public bool TryInsertPolyline(IReadOnlyList<Vector2> points, float step, int maxStoredSamples)
        {
            // Verifica il budget prima di allocare: nessun inserimento parziale.
            long needed = 1;
            for (int i = 1; i < points.Count; i++)
            {
                double segmentLength = Vector2.Distance(points[i - 1], points[i]);
                needed += (int)Math.Max(1d, Math.Ceiling(segmentLength / step));
                if (needed + _storedSamples > maxStoredSamples) return false;
            }
            Put(points[0]);
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 a = points[i - 1], b = points[i];
                double length = Vector2.Distance(a, b);
                int n = Math.Max(1, (int)Math.Ceiling(length / step));
                for (int k = 1; k <= n; k++) Put(Vector2.Lerp(a, b, (float)((double)k / n)));
            }
            return true;
        }

        private void Put(Vector2 point)
        {
            (int x, int y) = Cell(point);
            long key = Key(x, y);
            if (!_buckets.TryGetValue(key, out List<Vector2>? list))
            {
                list = new List<Vector2>();
                _buckets.Add(key, list);
            }
            list.Add(point);
            _storedSamples++;
        }
    }
}
