using System.Numerics;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Core.Randomness;
using SkiaSharp;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// C3: motore pittorico headless per le pennellate Simple di Cincinnus.
/// Opera su un SKCanvas fornito dal chiamante: nessun renderer è registrato nel catalogo.
/// Disegna corpo pigmentato, ombre morbide e fibre con testate sfalsate, carico
/// progressivamente esaurito, zone asciutte e modulazione della luce trasversale.
/// </summary>
public static class CincinnusBristlePainter
{
    private const int MaximumBristlesPerStroke = 1_024;
    private const long MaximumBrushPointWork = 12_000_000L;

    public static void Paint(SKCanvas canvas, IReadOnlyList<CincinnusBrushStroke> strokes,
        int width, int height, long seed = 1, CincinnusPaintOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(strokes);
        if (width is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(width));
        if (height is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(height));
        options ??= new CincinnusPaintOptions();
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        // Budget preventivo: non iniziare a dipingere se la densità rischia di
        // moltiplicare centinaia di migliaia di campioni per migliaia di setole.
        long work = 0;
        foreach (CincinnusBrushStroke stroke in strokes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (stroke is null) throw new ArgumentException("Pennellata nulla.", nameof(strokes));
            if (stroke.Spine.Count < 16) continue;
            double hairs = Math.Ceiling(2d * stroke.Spine.Max(p => p.HalfWidth) / options.BristlePitch);
            if (hairs > MaximumBristlesPerStroke)
                throw new ArgumentOutOfRangeException(nameof(options.BristlePitch), "La densità di setole supera il budget.");
            work += (long)Math.Max(1d, hairs) * stroke.Spine.Count;
            if (work > MaximumBrushPointWork)
                throw new InvalidOperationException("Il budget di campioni pennello C3 è stato superato.");
        }
        // Copia della palette per prevenire mutazioni durante il rendering.
        SKColor[] ramp = options.Palette.ToArray();
        var random = new SplitMix64Random(unchecked((ulong)seed));
        IRandomSource tones = random.Fork("cincinnus/simple/tones/v1");
        IRandomSource dirt = random.Fork("cincinnus/simple/dirt/v1");
        SKColor background = Sample(ramp, .02f);
        canvas.Clear(options.BackgroundColor ?? background);

        float lightZ = MathF.Sqrt(Math.Max(0f, 1f - options.LightX * options.LightX -
            options.LightY * options.LightY));
        for (int i = 0; i < strokes.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CincinnusBrushStroke stroke = strokes[i];
            if (stroke.Spine.Count < 16) continue;
            int dots = options.DotCount / Math.Max(1, strokes.Count);
            for (int d = 0; d < dots; d++)
            {
                if ((d & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
                float x = width * dirt.NextFloat();
                float y = height * dirt.NextFloat();
                float radius = .8f + 6f * MathF.Pow(dirt.NextFloat(), 3);
                if (dirt.NextFloat() < (float)i / Math.Max(1, strokes.Count))
                {
                    using var dot = new SKPaint { IsAntialias = true,
                        Color = new SKColor(14, 4, 8, 190) };
                    canvas.DrawCircle(x, y, radius, dot);
                }
            }

            float depth = i / Math.Max(1f, strokes.Count - 1f);
            float yMid = stroke.Spine[stroke.Spine.Count / 2].Position.Y / height;
            float u = 2f * yMid - 1f;
            float gradient = -.5f * options.TopToBottomTone *
                MathF.Sign(u) * MathF.Pow(MathF.Abs(u), options.ToneBandExponent);
            float tone = .4f + .3f * tones.NextFloat() +
                options.DepthTone * (MathF.Pow(depth, 1.2f) - .5f) + gradient;
            using SKPath body = BuildBody(stroke);
            SKColor shadow = Sample(ramp, 0f);
            DrawShadow(canvas, body, shadow, options.WideShadowBlur,
                options.WideShadowOpacity, 10f, 20f);
            DrawShadow(canvas, body, shadow, options.TightShadowBlur,
                options.TightShadowOpacity, 2.5f, 5f);
            using (var paint = new SKPaint { Color = Sample(ramp, tone - options.BodyShade),
                IsAntialias = true, Style = SKPaintStyle.Fill })
                canvas.DrawPath(body, paint);
            DrawFibers(canvas, stroke, ramp, tone, options, lightZ, cancellationToken);
        }
    }

    private static SKPath BuildBody(CincinnusBrushStroke stroke)
    {
        IReadOnlyList<CincinnusSpineSample> points = stroke.Spine;
        using var builder = new SKPathBuilder();
        bool started = false;
        // Landing hull: bordo irregolare dal lato -1 al lato +1.
        for (int j = 0; j <= 64; j++)
        {
            float t = -1f + j / 32f;
            int k = FindArc(points, stroke.LandingOffset(t));
            Vector2 point = Offset(points[k], t, BodyTaper(stroke, k));
            if (!started) { builder.MoveTo(point.X, point.Y); started = true; }
            else builder.LineTo(point.X, point.Y);
        }
        int plusStart = FindArc(points, stroke.LandingOffset(1f));
        for (int k = plusStart + 1; k < points.Count; k++)
        {
            Vector2 p = Offset(points[k], 1f, BodyTaper(stroke, k));
            builder.LineTo(p.X, p.Y);
        }
        int minusStart = FindArc(points, stroke.LandingOffset(-1f));
        for (int k = points.Count - 1; k > minusStart; k--)
        {
            Vector2 p = Offset(points[k], -1f, BodyTaper(stroke, k));
            builder.LineTo(p.X, p.Y);
        }
        builder.Close();
        return builder.Detach();
    }

    private static float BodyTaper(CincinnusBrushStroke stroke, int k)
    {
        float arc = stroke.Spine[k].Arc;
        float tail = Math.Clamp((stroke.Length - arc - 35f) / 145f, 0f, 1f);
        float gone = 1f - stroke.Load(arc / Math.Max(stroke.Length, 1f));
        float dry = Math.Clamp((.6f - gone) / .4f, 0f, 1f);
        return MathF.Sqrt(tail * dry);
    }

    private static Vector2 Offset(CincinnusSpineSample point, float t, float taper)
        => point.Position + point.Normal * (point.HalfWidth * t * taper);

    private static int FindArc(IReadOnlyList<CincinnusSpineSample> spine, float arc)
    {
        int left = 0, right = spine.Count - 1;
        while (left < right)
        {
            int mid = (left + right) / 2;
            if (spine[mid].Arc < arc) left = mid + 1;
            else right = mid;
        }
        return left;
    }

    private static void DrawShadow(SKCanvas canvas, SKPath body, SKColor color,
        float blur, float opacity, float dx, float dy)
    {
        if (opacity <= 0f) return;
        using var filter = blur > 0 ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blur) : null;
        using var paint = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Fill,
            Color = color.WithAlpha((byte)Math.Clamp((int)(opacity * 255), 0, 255)),
            MaskFilter = filter
        };
        int saved = canvas.Save();
        try { canvas.Translate(dx, dy); canvas.DrawPath(body, paint); }
        finally { canvas.RestoreToCount(saved); }
    }

    private static void DrawFibers(SKCanvas canvas, CincinnusBrushStroke stroke,
        SKColor[] ramp, float tone, CincinnusPaintOptions options, float lightZ,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CincinnusSpineSample> spine = stroke.Spine;
        float half = spine.Max(p => p.HalfWidth);
        int count = (int)Math.Clamp(Math.Ceiling(2d * half / options.BristlePitch),
            1d, (double)MaximumBristlesPerStroke);
        var rng = new SplitMix64Random(stroke.Seed);
        IRandomSource rnd = rng.Fork("cincinnus/simple/hairs/v1");
        float step = Math.Max(.1f, stroke.Length / Math.Max(1, spine.Count - 1));

        for (int j = 0; j < count; j++)
        {
            if ((j & 15) == 0) cancellationToken.ThrowIfCancellationRequested();
            float side = -1f + 2f * (j + rnd.NextFloat()) / count;
            float diameter = options.BristlePitch *
                (.45f + .4f * rnd.NextFloat()) * options.HairWidthRatio;
            float baseTone = tone + .07f + .4f * (rnd.NextFloat() - .5f);
            float phase = 500f * rnd.NextFloat();
            int trim = Math.Min((int)((30f + 140f * side * side + 60f * rnd.NextFloat()) / step),
                Math.Max(1, spine.Count / 5));
            int start = Math.Clamp((int)((stroke.LandingOffset(side) + step * rnd.NextFloat()) / step),
                0, spine.Count - 1);
            int last = Math.Max(start, spine.Count - trim);
            var current = new List<SKPoint>(24);
            float accumulatedLoad = 0f;
            float accumulatedTone = 0f;
            int samples = 0;

            void Flush()
            {
                if (current.Count >= 2 && samples > 0)
                {
                    float avgLoad = accumulatedLoad / samples;
                    float opacity = Math.Clamp(.3f + .7f * avgLoad, .06f, 1f);
                    SKColor color = Sample(ramp, accumulatedTone / samples).WithAlpha(
                        (byte)Math.Clamp((int)(opacity * 235f), 1, 255));
                    using var builder = new SKPathBuilder();
                    for (int p = 0; p < current.Count; p++)
                    {
                        SKPoint q = current[p];
                        if (p == 0) builder.MoveTo(q.X, q.Y);
                        else builder.LineTo(q.X, q.Y);
                    }
                    using SKPath path = builder.Detach();
                    using var paint = new SKPaint
                    {
                        IsAntialias = true, Style = SKPaintStyle.Stroke,
                        StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round,
                        StrokeWidth = Math.Max(.1f, diameter * (.4f + .6f * avgLoad)),
                        Color = color
                    };
                    canvas.DrawPath(path, paint);
                }
                current.Clear();
                accumulatedLoad = accumulatedTone = 0f;
                samples = 0;
            }

            for (int k = start; k < last; k++)
            {
                CincinnusSpineSample sp = spine[k];
                float s = sp.Arc;
                float progress = s / Math.Max(1f, stroke.Length);
                float load = stroke.Load(progress);
                float gone = 1f - load;
                float shifted = side + .035f * Noise(s / 380f + phase, stroke.Seed + 17);
                float dry = Noise(s / 45f + phase * 1.3f, stroke.Seed + 23);
                float lim = .95f - .5f * shifted * shifted - .4f * gone;
                float longGap = Noise(s / 220f + phase * .7f, stroke.Seed + 41);
                bool gap = MathF.Abs(shifted) > 1.02f || dry > lim ||
                    (gone > .05f && longGap > 1.2f - 1.75f * MathF.Pow(gone, 1.5f));
                if (gap) { Flush(); continue; }
                Vector2 point = Offset(sp, shifted, 1f);
                float q = shifted * .85f;
                float lighting = sp.Normal.X * options.LightX * q +
                    sp.Normal.Y * options.LightY * q + MathF.Sqrt(Math.Max(0f, 1f - q * q)) * lightZ;
                float colorTone = baseTone + .22f * (lighting - lightZ)
                    + .14f * Noise(s / 1500f, stroke.Seed + 59)
                    + .2f * Noise(shifted * 2.2f + s / 700f, stroke.Seed + 67)
                    + .1f * Noise(s / 140f + phase * .7f, stroke.Seed + 71)
                    + .16f * gone;
                current.Add(new SKPoint(point.X, point.Y));
                accumulatedTone += colorTone;
                accumulatedLoad += load;
                samples++;
                if (current.Count >= 24)
                {
                    SKPoint end = current[^1];
                    Flush();
                    current.Add(end); // continuità fra segmenti consecutivi
                }
            }
            Flush();
        }
    }

    private static float Noise(float x, ulong seed)
    {
        double u = Math.Floor(x);
        long ix = (long)u;
        float fract = x - (float)u;
        float t = fract * fract * (3f - 2f * fract);
        float a = Hash(ix, seed);
        float b = Hash(ix + 1, seed);
        return a + (b - a) * t;
    }

    private static float Hash(long value, ulong seed)
    {
        ulong z = unchecked((ulong)value) ^ seed;
        z = unchecked((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94D049BB133111EBUL);
        z ^= z >> 31;
        return (z >> 40) / 16777216f;
    }

    internal static SKColor Sample(SKColor[] ramp, float tone)
    {
        float x = Math.Clamp(tone, 0f, 1f) * (ramp.Length - 1);
        int a = Math.Min((int)x, ramp.Length - 2);
        float t = x - a;
        SKColor p = ramp[a], q = ramp[a + 1];
        return new SKColor(
            (byte)Math.Clamp((int)MathF.Round(p.Red + (q.Red - p.Red) * t), 0, 255),
            (byte)Math.Clamp((int)MathF.Round(p.Green + (q.Green - p.Green) * t), 0, 255),
            (byte)Math.Clamp((int)MathF.Round(p.Blue + (q.Blue - p.Blue) * t), 0, 255));
    }
}
