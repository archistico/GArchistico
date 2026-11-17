using System.Numerics;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// C3: trasforma le linee C2 in pennellate sovrapposte, genera la spina limitando
/// lo spessore nelle curve strette, testate oblique/irregolari e ordine pittorico.
/// Il planner non dipende da SkiaSharp ed è riproducibile per seed.
/// </summary>
public static class CincinnusStrokePlanner
{
    public static CincinnusStrokePlan Build(CincinnusSimpleLayout layout, long seed = 1,
        CincinnusStrokeOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(layout);
        return Build(layout.Lines, seed, options, cancellationToken);
    }

    public static CincinnusStrokePlan Build(IReadOnlyList<CincinnusStreamline> lines, long seed = 1,
        CincinnusStrokeOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lines);
        options ??= new CincinnusStrokeOptions();
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        var root = new SplitMix64Random(unchecked((ulong)seed));
        IRandomSource rnd = root.Fork("cincinnus/simple/strokes/v1");
        var strokes = new List<CincinnusBrushStroke>();
        int usedPoints = 0;
        bool limit = false;
        for (int lineIndex = 0; lineIndex < lines.Count && !limit; lineIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CincinnusStreamline line = lines[lineIndex];
            if (line.Points.Count < 16) continue;
            var arcs = new float[line.Points.Count];
            for (int i = 1; i < arcs.Length; i++)
                arcs[i] = arcs[i - 1] + Vector2.Distance(line.Points[i - 1], line.Points[i]);
            // L'originale usa hw=GAP*1.25, non la clearance locale.
            float hw = line.Separation * options.HalfWidthFactor;
            if (!float.IsFinite(hw) || hw > 20_000f)
                throw new ArgumentOutOfRangeException(nameof(options.HalfWidthFactor), "La semilarghezza supera il budget geometrico.");
            if (line.Length > 0 && line.Points.Count > 1)
            {
                // Il max della clearance è saturato a Separation: richiami riproducibili.
                int from = 0;
                int produced = 0;
                while (from < arcs.Length - 16)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    float minimum = produced == 0 ? options.FirstMinimumLength : options.SubsequentMinimumLength;
                    float target = minimum + (options.MaximumStrokeLength - minimum) * rnd.NextFloat();
                    int to = from + 1;
                    while (to < arcs.Length - 1 && arcs[to] - arcs[from] < target) to++;
                    int endExclusive = Math.Min(arcs.Length, to + 1);
                    int length = endExclusive - from;
                    if (length < 16) break;
                    if (strokes.Count >= options.MaximumStrokes || length > options.MaximumSpinePoints - usedPoints)
                    {
                        limit = true;
                        break;
                    }
                    ulong strokeSeed = rnd.NextUInt64();
                    var strokeRnd = new SplitMix64Random(strokeSeed);
                    strokes.Add(MakeStroke(line.Points, arcs, from, endExclusive, hw,
                        options.DryExponent, strokeSeed, strokeRnd));
                    usedPoints += length;
                    produced++;
                    if (endExclusive == arcs.Length) break;
                    float overlap = options.MinimumOverlap +
                        (options.MaximumOverlap - options.MinimumOverlap) * rnd.NextFloat();
                    int next = endExclusive - 1;
                    while (next > from + 8 && arcs[endExclusive - 1] - arcs[next] < overlap) next--;
                    from = Math.Max(from + 8, next);
                }
            }
        }
        // In Gȧrt le pennellate più curve vengono deposte per ultime.
        CincinnusBrushStroke[] ordered = strokes.OrderBy(s => s.Bend).ToArray();
        return new CincinnusStrokePlan(ordered, limit, usedPoints);
    }

    private static CincinnusBrushStroke MakeStroke(IReadOnlyList<Vector2> line, float[] arcs,
        int first, int endExclusive, float nominalHalf, float dryExponent,
        ulong strokeSeed, IRandomSource rnd)
    {
        int n = endExclusive - first;
        var bendRadii = new float[n];
        double bendTotal = 0;
        for (int k = 0; k < n; k++)
        {
            int a = Math.Max(0, k - 4), b = Math.Min(n - 2, k + 4);
            Vector2 va = line[first + a + 1] - line[first + a];
            Vector2 vb = line[first + b + 1] - line[first + b];
            double signed = Math.Atan2((double)va.X * vb.Y - (double)va.Y * vb.X,
                (double)va.X * vb.X + (double)va.Y * vb.Y);
            float span = Math.Max(.01f, arcs[first + b + 1] - arcs[first + a]);
            float radius = Math.Abs(signed) < 0.0001 ? 1_000_000f :
                (float)Math.Min(1_000_000d, span / Math.Abs(signed));
            bendRadii[k] = radius;
            bendTotal += 1d / radius;
        }
        float phase = rnd.NextFloat() * MathF.Tau;
        var spine = new CincinnusSpineSample[n];
        for (int k = 0; k < n; k++)
        {
            Vector2 prev = line[first + Math.Max(0, k - 1)];
            Vector2 next = line[first + Math.Min(n - 1, k + 1)];
            Vector2 tangent = next - prev;
            float tangentLength = tangent.Length();
            Vector2 normal = tangentLength > .000001f
                ? new Vector2(-tangent.Y / tangentLength, tangent.X / tangentLength)
                : Vector2.UnitY;
            float t = k / (n - 1f);
            float load = 1f - MathF.Pow(t, dryExponent);
            float wiggle = 1f + .2f * MathF.Sin(3f * MathF.Tau * t + phase);
            float radiusMin = bendRadii[k];
            int radiusWindow = Math.Min(128, (int)MathF.Ceiling(nominalHalf / Math.Max(1f, (arcs[^1] - arcs[0]) / Math.Max(1, arcs.Length - 1))));
            for (int j = Math.Max(0, k - radiusWindow); j <= Math.Min(n - 1, k + radiusWindow); j++)
                radiusMin = Math.Min(radiusMin, bendRadii[j]);
            float halfWidth = Math.Clamp(Math.Min(nominalHalf * (.75f + .25f * load) * wiggle,
                .8f * radiusMin), .05f, 20_000f);
            spine[k] = new CincinnusSpineSample(line[first + k], normal,
                arcs[first + k] - arcs[first], halfWidth);
        }
        float[] head = BuildLanding(nominalHalf, rnd);
        return new CincinnusBrushStroke(spine, head, (float)(bendTotal / n), strokeSeed, dryExponent);
    }

    private static float[] BuildLanding(float halfWidth, IRandomSource rnd)
    {
        const int count = 129;
        float longness = .55f + .65f * rnd.NextFloat();
        float tilt = (-.6f + 1.2f * rnd.NextFloat()) * longness;
        float lumps = 1.5f + 2.5f * rnd.NextFloat();
        float phase = 100f * rnd.NextFloat();
        float detail = MathF.Tau * rnd.NextFloat();
        var result = new float[count];
        float low = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            float t = -1f + 2f * i / (count - 1);
            float uneven = MathF.Sin(t * lumps * MathF.Tau + phase) +
                .33f * MathF.Sin(t * lumps * 2.3f + detail);
            float raw = halfWidth * (.8f * longness * (1f - MathF.Sqrt(Math.Max(0f, 1f - t * t)))
                + tilt * t + .15f * uneven);
            result[i] = raw;
            low = Math.Min(low, raw);
        }
        for (int i = 0; i < count; i++) result[i] -= low;
        return result;
    }
}
