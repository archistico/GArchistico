using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// C4: versione Adaptive indipendente da C2 Simple. Distribuzione su celle meno coperte,
/// pennellate main/filler, rimozione dei tratti a uncino e secondo passaggio patch.
/// Modello di riferimento: lines/cincinnus/Cincinnus.kt, non equivalenza Kotlin bit per bit.
/// </summary>
public static class CincinnusAdaptivePlacer
{
    private sealed record Candidate(CincinnusBrushStroke Stroke, float Size);

    public static CincinnusAdaptiveLayout Generate(int width, int height, long seed = 3,
        CincinnusAdaptiveOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new CincinnusAdaptiveOptions();
        options.Validate(width, height);
        cancellationToken.ThrowIfCancellationRequested();
        CincinnusFlowField field = CincinnusFlowField.Create(width, height, seed,
            cancellationToken: cancellationToken);
        return Place(field, width, height, seed, options, cancellationToken);
    }

    /// <summary>Accetta un campo sintetico, per test di riempimento senza dipendere dai vortici.</summary>
    public static CincinnusAdaptiveLayout Place(IVectorField2D field, int width, int height,
        long seed = 3, CincinnusAdaptiveOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        options ??= new CincinnusAdaptiveOptions();
        options.Validate(width, height);
        cancellationToken.ThrowIfCancellationRequested();
        var root = new SplitMix64Random(unchecked((ulong)seed));
        IRandomSource primaryRandom = root.Fork("cincinnus/adaptive/main/v1");
        IRandomSource headRandom = root.Fork("cincinnus/adaptive/heads/v1");
        IRandomSource patchRandom = root.Fork("cincinnus/adaptive/patch/v1");
        var cover = new CincinnusCoverageGrid(width, height, options.CoverageCell, options.MaximumCoverageCells);
        var placed = new List<Candidate>();
        int placedPoints = 0;
        int attempts = 0;
        bool safety = false;

        while ((placed.Count < options.MainStrokeCount || cover.UncoveredCellCount > 0) &&
            attempts < options.MaximumPlacementAttempts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (placed.Count >= options.MaximumTotalStrokes) { safety = true; break; }
            attempts++;
            Vector2 start = cover.Pick(primaryRandom);
            Candidate? stroke = TryCreate(field, start, width, height, options,
                primaryRandom, headRandom, root, placed.Count + attempts, placed.Count < options.MainStrokeCount,
                cancellationToken);
            if (stroke is null)
            {
                // Come l'originale, tentare un seed laterale e penalizzare il seed originale
                // soltanto dopo il fallimento del primo tentativo.
                Vector2 aside = Aside(field, start, options.MinimumRadius);
                stroke = TryCreate(field, aside, width, height, options,
                    primaryRandom, headRandom, root, placed.Count + attempts + 7919, placed.Count < options.MainStrokeCount,
                    cancellationToken);
                cover.Mark(start);
            }
            if (stroke is null) continue;
            if (stroke.Stroke.Spine.Count > options.MaximumSpinePoints - placedPoints)
            {
                safety = true;
                break;
            }
            cover.Add(stroke.Stroke, cancellationToken);
            placed.Add(stroke);
            placedPoints += stroke.Stroke.Spine.Count;
        }

        // Conserva l'ordine di appartenenza main/filler PRIMA della pulizia degli uncini.
        Candidate[] rawMain = placed.Take(options.MainStrokeCount).OrderByDescending(s => s.Size).ToArray();
        Candidate[] rawFiller = placed.Skip(options.MainStrokeCount).OrderByDescending(s => s.Size).ToArray();
        CincinnusBrushStroke[] main = rawMain.Select(s => TrimHooks(s.Stroke, options))
            .Where(s => s is not null).Select(s => s!).ToArray();
        CincinnusBrushStroke[] filler = rawFiller.Select(s => TrimHooks(s.Stroke, options))
            .Where(s => s is not null).Select(s => s!).ToArray();

        // La copertura originale delle spine NON è quella effettiva dopo il ritaglio.
        // Rifare il passaggio per evitare di perdere le aree rimaste scoperte.
        var patchCover = new CincinnusCoverageGrid(width, height, options.CoverageCell, options.MaximumCoverageCells);
        foreach (CincinnusBrushStroke s in filler) patchCover.Add(s, cancellationToken);
        foreach (CincinnusBrushStroke s in main) patchCover.Add(s, cancellationToken);
        int beforePatch = patchCover.UncoveredCellCount;
        var patchStrokes = new List<CincinnusBrushStroke>();
        int patchAttempts = 0;
        while (patchCover.UncoveredCellCount > 0 && patchAttempts < options.MaximumPatchAttempts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (placed.Count + patchStrokes.Count >= options.MaximumTotalStrokes)
            {
                safety = true;
                break;
            }
            patchAttempts++;
            Vector2 start = patchCover.Pick(patchRandom);
            Candidate? stroke = TryCreate(field, start, width, height, options,
                patchRandom, patchRandom, root, placed.Count + patchAttempts + 40_000, false, cancellationToken);
            if (stroke is null)
            {
                stroke = TryCreate(field, Aside(field, start, options.MinimumRadius), width, height, options,
                    patchRandom, patchRandom, root, placed.Count + patchAttempts + 50_000, false, cancellationToken);
                patchCover.Mark(start);
            }
            if (stroke is null) continue;
            if (stroke.Stroke.Spine.Count > options.MaximumSpinePoints - placedPoints)
            {
                safety = true;
                break;
            }
            // L'originale non applica unhooked alle patch: conservarne la copertura integrale.
            patchCover.Add(stroke.Stroke, cancellationToken);
            patchStrokes.Add(stroke.Stroke);
            placedPoints += stroke.Stroke.Spine.Count;
        }

        // Il budget di tentativi è un limite esplicito, non garanzia di copertura completa.
        if (attempts == options.MaximumPlacementAttempts &&
            (placed.Count < options.MainStrokeCount || cover.UncoveredCellCount > 0)) safety = true;
        if (patchAttempts == options.MaximumPatchAttempts && patchCover.UncoveredCellCount > 0) safety = true;

        return new CincinnusAdaptiveLayout(field, main, filler, patchStrokes.ToArray(),
            attempts, patchAttempts, beforePatch, patchCover.UncoveredCellCount,
            patchCover.CellCount, placedPoints, safety);
    }

    private static Vector2 Aside(IVectorField2D field, Vector2 point, float radius)
    {
        Vector2 v = field.Sample(point.X, point.Y);
        if (!float.IsFinite(v.X) || !float.IsFinite(v.Y)) return point;
        float len = v.Length();
        return len > 1e-8f ? point + new Vector2(-v.Y, v.X) * (1.5f * radius / len)
            : point + new Vector2(1.5f * radius, 0);
    }

    private static Candidate? TryCreate(IVectorField2D field, Vector2 start, int width, int height,
        CincinnusAdaptiveOptions options, IRandomSource rnd, IRandomSource heads,
        SplitMix64Random root, int ordinal, bool low, CancellationToken cancellationToken)
    {
        float margin = options.Margin;
        if (!float.IsFinite(start.X) || !float.IsFinite(start.Y) ||
            start.X < -margin || start.Y < -margin || start.X > width + margin || start.Y > height + margin)
            return null;
        var trace = new CincinnusTraceOptions
        {
            StepSize = options.StepSize,
            MaximumLength = options.MaximumTraceLength,
            Margin = margin
        };
        IReadOnlyList<Vector2> front = CincinnusPathGeometry.TraceForward(field, start, width, height,
            trace, cancellationToken);
        if (front.Count < 40) return null;
        int round = CincinnusPathGeometry.FindFirstReturn(front, options.StepSize,
            Math.Min(30f, 10f * options.StepSize));
        float size;
        if (round > 0) size = round * options.StepSize / MathF.Tau;
        else if (field is CincinnusFlowField vortices)
            size = vortices.Vortices.Min(v => Vector2.Distance(start, v.Center));
        else size = Math.Min(width, height) * .5f;
        if (size < options.MinimumRadius) return null;
        float halfWidth = Math.Min(options.MaximumHalfWidth, size * options.HalfWidthFactor *
            (.85f + .3f * rnd.NextFloat()));
        if (halfWidth <= 0f) return null;
        float[] landing = CreateHead(halfWidth, heads);
        float backLength = Math.Max(options.StepSize, landing[64] + 20f);
        IReadOnlyList<Vector2> back = CincinnusPathGeometry.TraceBackward(field, start, width, height,
            trace with { MaximumLength = Math.Min(options.MaximumTraceLength, backLength) }, cancellationToken);
        var full = new List<Vector2>(back.Count + front.Count - 1);
        for (int i = back.Count - 1; i > 0; i--) full.Add(back[i]);
        full.AddRange(front);
        int most;
        if (round > 0)
        {
            float cap = Math.Min(.9f * round * options.StepSize * (.95f + .08f * rnd.NextFloat())
                + 120f + .4f * halfWidth, options.MaximumStrokeLength);
            most = Math.Clamp((int)(cap / options.StepSize), 2, Math.Min(round, full.Count));
        }
        else
        {
            int lastVisible = -1;
            for (int i = 0; i < front.Count; i++)
                if (front[i].X > -halfWidth && front[i].X < width + halfWidth &&
                    front[i].Y > -halfWidth && front[i].Y < height + halfWidth) lastVisible = i;
            most = Math.Clamp(back.Count - 1 + lastVisible + 2, 2, full.Count);
            most = Math.Min(most, Math.Max(2, (int)(options.MaximumStrokeLength / options.StepSize)));
        }
        int end = Math.Clamp((int)(most * (options.MinimumStrokeShare +
            (1f - options.MinimumStrokeShare) * rnd.NextFloat())), 2, full.Count);
        if (low && full[end - 1].Y < height / 2f)
        {
            // Variante 'low' originale: preferire terminazioni interne alla metà inferiore.
            var candidates = new List<int>();
            for (int e = Math.Max(2, (int)(most * options.MinimumStrokeShare)); e <= most; e++)
            {
                Vector2 q = full[e - 1];
                float inset = Math.Min(60f, Math.Min(width, height) * .1f);
                if (q.Y > height * .5f && q.Y < height - inset &&
                    q.X > inset && q.X < width - inset) candidates.Add(e);
            }
            if (candidates.Count > 0) end = candidates[rnd.NextInt32Inclusive(0, candidates.Count - 1)];
        }
        if (end < 20) return null;
        Vector2[] points = full.Take(end).ToArray();
        float[] radii = CincinnusPathGeometry.EstimateBendRadii(points, options.StepSize);
        int window = (int)Math.Clamp(Math.Ceiling(2d * halfWidth / options.StepSize), 4d, (double)points.Length);
        float[] minRadii = new float[points.Length];
        for (int k = 0; k < points.Length; k++)
        {
            float minRadius = float.MaxValue;
            for (int j = Math.Max(0, k - window); j <= Math.Min(points.Length - 1, k + window); j++)
                minRadius = Math.Min(minRadius, .8f * radii[j]);
            minRadii[k] = minRadius;
        }
        float phase = 100f * rnd.NextFloat();
        float arc = 0f;
        var spine = new CincinnusSpineSample[points.Length];
        double sumHalf = 0;
        for (int k = 0; k < points.Length; k++)
        {
            Vector2 a = points[Math.Max(0, k - 1)];
            Vector2 b = points[Math.Min(points.Length - 1, k + 1)];
            Vector2 diff = b - a;
            float mag = diff.Length();
            if (mag < 1e-6f || !float.IsFinite(mag)) return null;
            Vector2 normal = new(-diff.Y / mag, diff.X / mag);
            if (k > 0) arc += Vector2.Distance(points[k - 1], points[k]);
            float t = k / (points.Length - 1f);
            float load = 1f - MathF.Pow(t, options.DryExponent);
            float landingScale = 1f + .12f * (1f - SmoothStep(0f, .05f, t));
            float half = halfWidth * (.75f + .25f * load) * landingScale *
                (1f + .2f * MathF.Sin(t * 3f * MathF.Tau + phase));
            // Finestra mediata della disponibilità di curvatura, versione conservativa.
            float room = minRadii[k];
            half = Math.Max(.05f, Math.Min(half, room));
            sumHalf += half;
            spine[k] = new CincinnusSpineSample(points[k], normal, arc, half);
        }
        if (sumHalf / spine.Length < options.MinimumAverageHalfWidth) return null;
        ulong salt = root.Fork("cincinnus/adaptive/stroke/" + ordinal).NextUInt64();
        float averageBend = (float)radii.Average(r => 1d / Math.Max(1e-6f, r));
        var brush = new CincinnusBrushStroke(spine, landing, averageBend, salt, options.DryExponent);
        return new Candidate(brush, size);
    }

    private static CincinnusBrushStroke? TrimHooks(CincinnusBrushStroke original,
        CincinnusAdaptiveOptions options)
    {
        IReadOnlyList<CincinnusSpineSample> points = original.Spine;
        if (points.Count < 21) return null;
        Vector2[] coords = points.Select(p => p.Position).ToArray();
        float[] radii = CincinnusPathGeometry.EstimateBendRadii(coords, options.StepSize);
        float maxHalf = points.Max(p => p.HalfWidth);
        int bestStart = 0, bestEnd = 0, runStart = 0;
        for (int k = 0; k <= points.Count; k++)
        {
            if (k == points.Count || radii[k] < options.HookRadiusFactor * maxHalf)
            {
                if (k - runStart > bestEnd - bestStart)
                {
                    bestStart = runStart;
                    bestEnd = k;
                }
                runStart = k + 1;
            }
        }
        if (bestEnd - bestStart < 21) return null;
        if (bestStart == 0 && bestEnd == points.Count) return original;
        var clipped = new CincinnusSpineSample[bestEnd - bestStart];
        float baseArc = points[bestStart].Arc;
        for (int i = 0; i < clipped.Length; i++)
            clipped[i] = points[bestStart + i] with { Arc = points[bestStart + i].Arc - baseArc };
        var landing = new float[129];
        for (int i = 0; i < landing.Length; i++)
            landing[i] = original.LandingOffset(-1f + 2f * i / (landing.Length - 1));
        return new CincinnusBrushStroke(clipped, landing, original.Bend,
            original.Seed, original.DryExponent);
    }

    private static float[] CreateHead(float halfWidth, IRandomSource rnd)
    {
        float lean = -.6f + 1.2f * rnd.NextFloat();
        float longness = .55f + .65f * rnd.NextFloat();
        float lumps = 1.5f + 2.5f * rnd.NextFloat();
        float phase = 100f * rnd.NextFloat();
        var values = new float[129];
        float min = float.MaxValue;
        for (int i = 0; i < values.Length; i++)
        {
            float t = -1f + 2f * i / (values.Length - 1);
            float raw = halfWidth * (.8f * longness * (1f - MathF.Sqrt(Math.Max(0f, 1f - t * t))) +
                longness * lean * t + .18f * MathF.Sin(t * lumps * 3.7f + phase) +
                .06f * MathF.Sin(t * 139.13f + phase));
            values[i] = raw;
            min = Math.Min(min, raw);
        }
        for (int i = 0; i < values.Length; i++) values[i] -= min;
        return values;
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}
