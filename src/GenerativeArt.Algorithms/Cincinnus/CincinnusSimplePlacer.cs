using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// C2: porting della logica geometrica di Gȧrt flow2/Streamlines.kt per
/// CincinnusSimple.kt. Non contiene pennelli, palette, bitmap né codice Kotlin.
/// </summary>
public static class CincinnusSimplePlacer
{
    /// <summary>
    /// Genera cinque vortici esterni e dispone le linee con RNG C# separato.
    /// I seed sono stabili nel porting, non bit-identici a Kotlin.
    /// </summary>
    public static CincinnusSimpleLayout Generate(int width, int height, long seed = 1,
        CincinnusSimpleOptions? options = null, CancellationToken cancellationToken = default)
    {
        // Validate the placement budget before spending time creating vortex candidates.
        PlacementSettings settings = PlacementSettings.Resolve(width, height, options);
        cancellationToken.ThrowIfCancellationRequested();
        CincinnusFlowField field = CincinnusFlowField.Create(width, height, seed,
            cancellationToken: cancellationToken);
        return PlaceValidated(field, seed, settings, cancellationToken);
    }

    /// <summary>
    /// Dispone le linee su un campo esplicito, anche sintetico: utile nei test.
    /// Crescita bidirezionale, soglia dTest, controllo autoreintersezioni, semi
    /// laterali e un passaggio finale di copertura su griglia sono indipendenti
    /// dal rendering e dalla selezione dei vortici.
    /// </summary>
    public static CincinnusSimpleLayout Place(IVectorField2D field, int width, int height,
        long seed = 1, CincinnusSimpleOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        PlacementSettings settings = PlacementSettings.Resolve(width, height, options);
        cancellationToken.ThrowIfCancellationRequested();
        return PlaceValidated(field, seed, settings, cancellationToken);
    }

    private static CincinnusSimpleLayout PlaceValidated(IVectorField2D field, long seed,
        PlacementSettings settings, CancellationToken cancellationToken)
    {
        var random = new SplitMix64Random(unchecked((ulong)seed));
        return new Placement(field, random.Fork("cincinnus/simple/placement/v1"),
            settings, cancellationToken).Run();
    }

    private readonly record struct PlacementSettings(
        float Separation, float TestDistance, float Step, float MinLength,
        float MaxLength, float Left, float Top, float Right, float Bottom,
        int GridColumns, int GridRows, int MaxLines, int MaxPoints,
        int MaxPendingSeeds)
    {
        public static PlacementSettings Resolve(int width, int height, CincinnusSimpleOptions? options)
        {
            if (width is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(width));
            if (height is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(height));
            options ??= new CincinnusSimpleOptions();
            float sep = options.Separation;
            float test = options.TestDistance ?? sep * .5f;
            float margin = options.Margin ?? sep * 3.75f;
            if (!float.IsFinite(sep) || sep <= 0) throw new ArgumentOutOfRangeException(nameof(options.Separation));
            if (!float.IsFinite(test) || test <= 0 || test > sep)
                throw new ArgumentOutOfRangeException(nameof(options.TestDistance));
            if (!float.IsFinite(options.StepSize) || options.StepSize <= 0 || options.StepSize > test)
                throw new ArgumentOutOfRangeException(nameof(options.StepSize));
            if (!float.IsFinite(margin) || margin < 0 || margin > 100_000)
                throw new ArgumentOutOfRangeException(nameof(options.Margin));
            if (!float.IsFinite(options.MinimumLength) || options.MinimumLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.MinimumLength));
            if (!float.IsFinite(options.MaximumLength) || options.MaximumLength < options.MinimumLength
                || (double)options.MaximumLength / options.StepSize > 40_000)
                throw new ArgumentOutOfRangeException(nameof(options.MaximumLength));
            if (options.MaximumLines is < 1 or > 50_000) throw new ArgumentOutOfRangeException(nameof(options.MaximumLines));
            if (options.MaximumPoints is < 2 or > 8_000_000) throw new ArgumentOutOfRangeException(nameof(options.MaximumPoints));
            if (options.MaximumPendingSeeds is < 1 or > 2_000_000)
                throw new ArgumentOutOfRangeException(nameof(options.MaximumPendingSeeds));
            if (options.MaximumCoverCells is < 1 or > 2_000_000)
                throw new ArgumentOutOfRangeException(nameof(options.MaximumCoverCells));

            double cols = Math.Ceiling(((double)width + 2d * margin) / sep);
            double rows = Math.Ceiling(((double)height + 2d * margin) / sep);
            if (cols < 1 || rows < 1 || cols * rows > options.MaximumCoverCells)
                throw new ArgumentOutOfRangeException(nameof(options.MaximumCoverCells),
                    "La griglia di copertura supera il budget; aumentare Separation o ridurre Margin.");

            return new PlacementSettings(sep, test, options.StepSize, options.MinimumLength,
                options.MaximumLength, -margin, -margin, width + margin, height + margin,
                (int)cols, (int)rows, options.MaximumLines, options.MaximumPoints,
                options.MaximumPendingSeeds);
        }
    }

    private sealed class Placement
    {
        // Come in Gȧrt: un punto appartiene a una linea e conserva la distanza
        // firmata dal seed, così lo stesso tratto non collide con i vicini locali.
        private readonly record struct Vertex(Vector2 Point, float SignedArc, int Owner);
        private const float SeedRoomFactor = .99f;
        private const float SelfSkipFactor = 2f;

        private readonly IVectorField2D _field;
        private readonly IRandomSource _rnd;
        private readonly PlacementSettings _s;
        private readonly CancellationToken _cancel;
        private readonly List<Vertex> _vertices = [];
        private readonly Dictionary<(int X, int Y), List<int>> _grid = [];
        private readonly List<int[]> _lines = [];
        private readonly List<Vector2> _pending = [];
        private bool _safetyLimitReached;
        private int _attemptedSeeds;

        public Placement(IVectorField2D field, IRandomSource rnd,
            PlacementSettings settings, CancellationToken cancel)
        {
            _field = field;
            _rnd = rnd;
            _s = settings;
            _cancel = cancel;
        }

        public CincinnusSimpleLayout Run()
        {
            Vector2[] cover = BuildCoverSeeds();
            int coverIndex = 0;
            float seedRadius = MathF.Max(_s.TestDistance, _s.Separation * SeedRoomFactor);
            while (_pending.Count > 0 || coverIndex < cover.Length)
            {
                _cancel.ThrowIfCancellationRequested();
                if (_lines.Count >= _s.MaxLines || _vertices.Count >= _s.MaxPoints)
                {
                    _safetyLimitReached = true;
                    break;
                }

                Vector2 seed;
                if (_pending.Count > 0)
                {
                    int k = _rnd.NextInt32Inclusive(0, _pending.Count - 1);
                    seed = _pending[k];
                    _pending[k] = _pending[^1];
                    _pending.RemoveAt(_pending.Count - 1);
                }
                else seed = cover[coverIndex++];

                _attemptedSeeds++;
                if (Inside(seed) && IsFree(seed, seedRadius, -1, 0f)) Grow(seed);
            }

            var result = new CincinnusStreamline[_lines.Count];
            for (int i = 0; i < _lines.Count; i++)
            {
                if ((i & 15) == 0) _cancel.ThrowIfCancellationRequested();
                int[] ids = _lines[i];
                var points = new Vector2[ids.Length];
                var clearance = new float[ids.Length];
                double length = 0;
                for (int k = 0; k < ids.Length; k++)
                {
                    points[k] = _vertices[ids[k]].Point;
                    clearance[k] = Clearance(ids[k], i);
                    if (k > 0) length += Vector2.Distance(points[k - 1], points[k]);
                }
                result[i] = new CincinnusStreamline(points, clearance, (float)length,
                    _s.Separation, _s.TestDistance);
            }
            return new CincinnusSimpleLayout(_field, result, _safetyLimitReached, _attemptedSeeds);
        }

        private Vector2[] BuildCoverSeeds()
        {
            int count = checked(_s.GridColumns * _s.GridRows);
            var order = new int[count];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            for (int i = order.Length - 1; i > 0; i--)
            {
                if ((i & 1023) == 0) _cancel.ThrowIfCancellationRequested();
                int j = _rnd.NextInt32Inclusive(0, i);
                (order[i], order[j]) = (order[j], order[i]);
            }

            var seeds = new Vector2[count];
            for (int k = 0; k < count; k++)
            {
                int col = order[k] % _s.GridColumns;
                int row = order[k] / _s.GridColumns;
                float x0 = _s.Left + col * _s.Separation;
                float y0 = _s.Top + row * _s.Separation;
                seeds[k] = new Vector2((x0 + MathF.Min(x0 + _s.Separation, _s.Right)) * .5f,
                    (y0 + MathF.Min(y0 + _s.Separation, _s.Bottom)) * .5f);
            }
            return seeds;
        }

        private void Grow(Vector2 seed)
        {
            int owner = _lines.Count;
            int first = _vertices.Count;
            Add(seed, 0f, owner);
            float forward = Walk(seed, +_s.Step, owner, _s.MaxLength);
            int midpoint = _vertices.Count;
            float backward = Walk(seed, -_s.Step, owner, _s.MaxLength - forward);
            int end = _vertices.Count;
            if (end - first < 2 || (double)forward + backward < _s.MinLength)
            {
                Rollback(first);
                return;
            }

            var ids = new int[end - first];
            int k = 0;
            for (int id = end - 1; id >= midpoint; id--) ids[k++] = id;
            for (int id = first; id < midpoint; id++) ids[k++] = id;
            _lines.Add(ids);
            SeedAlong(ids);
        }

        private float Walk(Vector2 origin, float signedStep, int owner, float budget)
        {
            Vector2 position = origin;
            float walked = 0f;
            int maxSteps = Math.Min(40_000, (int)Math.Ceiling(budget / MathF.Abs(signedStep)) + 1);
            for (int i = 0; i < maxSteps; i++)
            {
                if ((i & 63) == 0) _cancel.ThrowIfCancellationRequested();
                if (_vertices.Count >= _s.MaxPoints)
                {
                    _safetyLimitReached = true;
                    break;
                }
                if (!FlowIntegrator.TryAdvance(_field, position, signedStep,
                    FlowIntegrationMethod.Rk4, out Vector2 next)) break;
                float distance = Vector2.Distance(position, next);
                if (!float.IsFinite(distance) || distance <= 0 || (double)walked + distance > budget) break;
                float arc = MathF.CopySign(walked + distance, signedStep);
                if (!Inside(next) || !IsFree(next, _s.TestDistance, owner, arc)) break;
                walked += distance;
                position = next;
                Add(position, arc, owner);
            }
            return walked;
        }

        private void SeedAlong(int[] ids)
        {
            for (int k = 0; k < ids.Length; k++)
            {
                if ((k & 127) == 0) _cancel.ThrowIfCancellationRequested();
                Vector2 a = _vertices[ids[Math.Max(k - 1, 0)]].Point;
                Vector2 b = _vertices[ids[Math.Min(k + 1, ids.Length - 1)]].Point;
                Vector2 direction = b - a;
                double length = Math.Sqrt((double)direction.X * direction.X +
                    (double)direction.Y * direction.Y);
                if (length < 1e-6) continue;
                var normal = new Vector2((float)(-direction.Y / length * _s.Separation),
                    (float)(direction.X / length * _s.Separation));
                Vector2 point = _vertices[ids[k]].Point;
                if (_pending.Count < _s.MaxPendingSeeds) _pending.Add(point + normal);
                else _safetyLimitReached = true;
                if (_pending.Count < _s.MaxPendingSeeds) _pending.Add(point - normal);
                else _safetyLimitReached = true;
            }
        }

        private bool Inside(Vector2 point) => float.IsFinite(point.X) && float.IsFinite(point.Y)
            && point.X >= _s.Left && point.X < _s.Right
            && point.Y >= _s.Top && point.Y < _s.Bottom;

        private (int X, int Y) Cell(Vector2 point) =>
            ((int)MathF.Floor((point.X - _s.Left) / _s.Separation),
             (int)MathF.Floor((point.Y - _s.Top) / _s.Separation));

        private void Add(Vector2 point, float arc, int owner)
        {
            var cell = Cell(point);
            if (!_grid.TryGetValue(cell, out List<int>? list)) _grid[cell] = list = [];
            list.Add(_vertices.Count);
            _vertices.Add(new Vertex(point, arc, owner));
        }

        private void Rollback(int first)
        {
            for (int id = _vertices.Count - 1; id >= first; id--)
            {
                var cell = Cell(_vertices[id].Point);
                List<int> list = _grid[cell];
                // This line is the last inserted owner. No other owner may have
                // inserted points between Grow() and Rollback().
                list.RemoveAt(list.Count - 1);
                if (list.Count == 0) _grid.Remove(cell);
            }
            _vertices.RemoveRange(first, _vertices.Count - first);
        }

        private bool IsFree(Vector2 candidate, float radius, int owner, float signedArc)
        {
            var (cx, cy) = Cell(candidate);
            double radius2 = (double)radius * radius;
            float selfSkip = SelfSkipFactor * _s.TestDistance;
            for (int y = cy - 1; y <= cy + 1; y++)
                for (int x = cx - 1; x <= cx + 1; x++)
                {
                    if (!_grid.TryGetValue((x, y), out List<int>? ids)) continue;
                    foreach (int id in ids)
                    {
                        Vertex existing = _vertices[id];
                        double dx = (double)existing.Point.X - candidate.X;
                        double dy = (double)existing.Point.Y - candidate.Y;
                        if (dx * dx + dy * dy < radius2 &&
                            (existing.Owner != owner ||
                             Math.Abs((double)existing.SignedArc - signedArc) > selfSkip))
                            return false;
                    }
                }
            return true;
        }

        private float Clearance(int vertexId, int owner)
        {
            Vector2 candidate = _vertices[vertexId].Point;
            var (cx, cy) = Cell(candidate);
            double best = (double)_s.Separation * _s.Separation;
            for (int y = cy - 1; y <= cy + 1; y++)
                for (int x = cx - 1; x <= cx + 1; x++)
                {
                    if (!_grid.TryGetValue((x, y), out List<int>? ids)) continue;
                    foreach (int id in ids)
                    {
                        Vertex existing = _vertices[id];
                        if (existing.Owner == owner) continue;
                        double dx = (double)existing.Point.X - candidate.X;
                        double dy = (double)existing.Point.Y - candidate.Y;
                        best = Math.Min(best, dx * dx + dy * dy);
                    }
                }
            return (float)Math.Sqrt(best);
        }
    }
}
