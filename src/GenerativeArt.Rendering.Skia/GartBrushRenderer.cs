using System.Numerics;
using GenerativeArt.Algorithms.Brushes;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// G2: due tecniche pittoriche distinte ricostruite dalle idee di Gȧrt:
/// Crinis (curve portanti e setole) e Flamebrush (timbri a spirale).
/// Non replica le coordinate del codice Kotlin né modifica i renderer storici.
/// </summary>
internal static class GartBrushRenderer
{
    private static readonly string[] FiberRoles = ["fiber-color-1", "fiber-color-2", "fiber-color-3"];
    private static readonly string[] ParticleRoles = ["particle-color-1", "particle-color-2", "particle-color-3"];

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        if (recipe.ArtworkId == "crinis-bristles")
        {
            int bands = ArtworkPaint.Int(recipe, "bands");
            int samples = ArtworkPaint.Int(recipe, "samples");
            double width = ArtworkPaint.Number(recipe, "band-width");
            double pitch = ArtworkPaint.Number(recipe, "bristle-pitch");
            double maxHalf = Math.Min(recipe.Size.Width, recipe.Size.Height) * width * .75;
            // Una ricetta istanziata da un catalogo storico resta in modalita classic.
            bool micro = recipe.Parameters.TryGetValue("detail-style", out string? detailStyle) &&
                detailStyle == "micro";
            double density = micro ? ArtworkPaint.Number(recipe, "fiber-density") : 1d;
            int bristles = Math.Clamp((int)Math.Ceiling(2 * maxHalf / pitch * density), 3, micro ? 360 : 140);
            long budget = micro ? 12_000_000L : 3_000_000L;
            if ((long)bands * samples * bristles > budget)
                throw new ArgumentOutOfRangeException(nameof(recipe),
                    $"Crinis supera il budget di {budget:N0} campioni delle setole.");
        }
        else if (recipe.ArtworkId == "spiral-flamebrush")
        {
            long dots = (long)ArtworkPaint.Int(recipe, "rings") *
                ArtworkPaint.Int(recipe, "stamps") * ArtworkPaint.Int(recipe, "particles");
            if (dots > 120_000L)
                throw new ArgumentOutOfRangeException(nameof(recipe),
                    "Flamebrush supera il budget di 120.000 particelle per rendering.");
        }
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        switch (recipe.ArtworkId)
        {
            case "crinis-bristles": RenderCrinis(surface, recipe, ct); break;
            case "spiral-flamebrush": RenderFlamebrush(surface, recipe, ct); break;
            default: throw new ArgumentException("Tecnica G2 non disponibile.", nameof(recipe));
        }
    }

    private static void RenderCrinis(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        if (r.Parameters.TryGetValue("detail-style", out string? detailStyle) && detailStyle == "micro")
        {
            CrinisMicroRenderer.Render(surface, r, ct);
            return;
        }
        (uint bg, uint[] fibers, uint shadow) = r.Parameters["palette"] switch
        {
            "copper" => (0xFF2C1521u, new uint[] { 0xFF733544, 0xFFCE8166, 0xFFF6D4A7 }, 0xFF190D1C),
            "forest" => (0xFF101E20u, new uint[] { 0xFF345A56, 0xFF91BAA2, 0xFFE0E8C2 }, 0xFF081518),
            "crimson" => (0xFF1A0308u, new uint[] { 0xFF4A0918, 0xFFD32C50, 0xFFF9C9B6 }, 0xFF110207),
            _ => (0xFF06132Cu, new uint[] { 0xFF25456C, 0xFF769CBE, 0xFFE1EFF7 }, 0xFF020A1B)
        };
        surface.Clear(ArtworkInk.Resolve(r, "background-color", Color32.FromArgb(bg)));
        Color32[] palette = PatternPalette.Resolve(r, FiberRoles, fibers);
        Color32 shadowInk = ArtworkInk.Resolve(r, "shadow-color", Color32.FromArgb(shadow));
        int bands = ArtworkPaint.Int(r, "bands"), samples = ArtworkPaint.Int(r, "samples");
        float shortSide = Math.Min(r.Size.Width, r.Size.Height);
        float half = shortSide * (float)ArtworkPaint.Number(r, "band-width") * .5f;
        float pitch = (float)ArtworkPaint.Number(r, "bristle-pitch");
        float hair = (float)ArtworkPaint.Number(r, "hair-width");
        double turn = ArtworkPaint.Number(r, "turn"), swirl = ArtworkPaint.Number(r, "swirl");
        double shadowStrength = ArtworkPaint.Number(r, "shadow-strength");
        double bodyOpacity = ArtworkPaint.Number(r, "body-opacity");
        double relief = ArtworkPaint.Number(r, "fiber-relief");
        double fray = ArtworkPaint.Number(r, "fiber-fray");
        double dryness = ArtworkPaint.Number(r, "dryness");
        var rnd = new SplitMix64Random(r.Seed);
        Vector2 eye = new(r.Size.Width * (float)(.35 + .3 * rnd.NextDouble()),
            r.Size.Height * (float)(.35 + .3 * rnd.NextDouble()));
        int spin = rnd.NextDouble() < .5 ? -1 : 1;
        int cellsX = 8, cellsY = 6;
        var coverage = new int[cellsX * cellsY];

        for (int band = 0; band < bands; band++)
        {
            ct.ThrowIfCancellationRequested();
            int minimum = coverage.Min();
            int[] candidates = Enumerable.Range(0, coverage.Length).Where(i => coverage[i] == minimum).ToArray();
            int cell = candidates[rnd.NextInt32Inclusive(0, candidates.Length - 1)];
            coverage[cell] += 1;
            Vector2 start = new((float)((cell % cellsX + .15 + .7 * rnd.NextDouble()) * r.Size.Width / cellsX),
                (float)((cell / cellsX + .15 + .7 * rnd.NextDouble()) * r.Size.Height / cellsY));
            double phase = rnd.NextDouble() * Math.Tau;
            float bandHalf = half * (float)(.72 + .5 * rnd.NextDouble());
            float step = Math.Max(.85f, shortSide / 220f);
            BristleSpinePoint[] spine = BristleSpine2D.Build(start, eye, samples, step,
                bandHalf, turn, swirl, phase, spin);
            if (spine.Length < 2) continue;
            var outline = new List<Vector2>(spine.Length * 2);
            foreach (var pnt in spine) outline.Add(pnt.Position + pnt.Normal * pnt.HalfWidth);
            for (int i = spine.Length - 1; i >= 0; i--)
                outline.Add(spine[i].Position - spine[i].Normal * spine[i].HalfWidth);

            // body-opacity=0 significa SOLO fibre: nessuna ombra poligonale resta visibile.
            if (shadowStrength > 0 && bodyOpacity > 0)
            {
                float offset = shortSide * .007f;
                var projected = outline.Select(v => v + new Vector2(offset, offset * 1.4f)).ToArray();
                surface.DrawPolygon(projected,
                    ArtworkPaint.Alpha(shadowInk, shadowStrength * .42 * bodyOpacity),
                    ArtworkPaint.Alpha(shadowInk, shadowStrength * .42 * bodyOpacity), .1f);
                projected = outline.Select(v => v + new Vector2(offset * .4f, offset * .55f)).ToArray();
                surface.DrawPolygon(projected,
                    ArtworkPaint.Alpha(shadowInk, shadowStrength * .6 * bodyOpacity),
                    ArtworkPaint.Alpha(shadowInk, shadowStrength * .6 * bodyOpacity), .1f);
            }
            Color32 baseColor = PatternPalette.Gradient(palette, Math.Clamp(.18 + .60 * band / Math.Max(1d, bands - 1), 0, 1));
            if (bodyOpacity > 0)
            {
                Color32 body = ArtworkPaint.Mix(baseColor, shadowInk, .32 + .18 * relief);
                surface.DrawPolygon(outline, ArtworkPaint.Alpha(body, bodyOpacity), ArtworkPaint.Alpha(body, bodyOpacity), .1f);
            }

            int hairs = Math.Clamp((int)(bandHalf * 2 / pitch), 3, 140);
            for (int j = 0; j < hairs; j++)
            {
                if ((j & 15) == 0) ct.ThrowIfCancellationRequested();
                double u = -1 + 2 * (j + .3 + .4 * rnd.NextDouble()) / hairs;
                int trim = (int)(samples * (.035 + .085 * u * u));
                Color32 color = PatternPalette.Gradient(palette,
                    Math.Clamp(.12 + .73 * (j / Math.Max(1d, hairs - 1)) +
                        .10 * (rnd.NextDouble() - .5), 0, 1));
                double depth = Math.Clamp((u + 1) * .5, 0, 1);
                color = SculptFiberColor(color, shadowInk, relief, depth);
                color = ArtworkPaint.Alpha(color, .60 + .25 * rnd.NextDouble());
                float hairWidth = Math.Max(.2f, hair * (float)(.86 + .28 * rnd.NextDouble()));
                double wiggle = rnd.NextDouble() * Math.Tau;
                var line = new List<Vector2>(samples);
                for (int k = trim; k < samples - trim; k++)
                {
                    double dryNoise = Math.Sin(k * .15 + j * 3.97 + wiggle) *
                        Math.Sin(k * .067 + phase + j * .73);
                    bool gap = dryness > 0 && dryNoise > .9 - dryness * .45;
                    if (gap)
                    {
                        DrawCrinisStroke(surface, line, color, shadowInk, hairWidth, relief, fray, wiggle + j * .31);
                        line.Clear();
                        continue;
                    }
                    var point = spine[k];
                    float transverse = (float)(u + .02 * Math.Sin(k * .035 + wiggle));
                    double edgeNoise = Math.Sin(k * .29 + wiggle * 1.7 + j * .11)
                        + .55 * Math.Sin(k * .97 + phase * .7 + j * .43)
                        + .22 * Math.Sin(k * 2.3 + wiggle * .45);
                    float roughOffset = (float)(fray * point.HalfWidth * .055 * edgeNoise);
                    line.Add(point.Position + point.Normal * (transverse * point.HalfWidth + roughOffset));
                }
                DrawCrinisStroke(surface, line, color, shadowInk, hairWidth, relief, fray, wiggle + j * .31);
            }
        }
    }

    private static void DrawCrinisStroke(SkiaRasterSurface surface, List<Vector2> line,
        Color32 fiberColor, Color32 shadowInk, float width, double relief, double fray, double phase)
    {
        if (line.Count < 2) return;
        if (relief > .001)
        {
            Color32 under = ArtworkPaint.Alpha(ArtworkPaint.Mix(shadowInk, fiberColor, .16), .12 + .18 * relief);
            surface.DrawPolyline(TexturedOffsetPath(line, width * (float)(.52 + .45 * relief), phase + .7, fray * .22f),
                1, 1, under, width * (1.15f + .18f * (float)relief));
        }

        surface.DrawPolyline(line, 1, 1, fiberColor, width);

        if (relief > .001)
        {
            Color32 highlightBase = ArtworkPaint.Mix(fiberColor, Color32.Opaque(248, 251, 255), .30 + .35 * relief);
            Color32 highlight = ArtworkPaint.Alpha(highlightBase, .12 + .30 * relief);
            surface.DrawPolyline(TexturedOffsetPath(line, -width * (float)(.28 + .32 * relief), phase + 1.9, fray * .18f),
                1, 1, highlight, Math.Max(.26f, width * (.42f + .08f * (float)relief)));
        }

        if (fray > .001)
        {
            Color32 fuzz = ArtworkPaint.Alpha(ArtworkPaint.Mix(fiberColor, Color32.Opaque(244, 247, 252), .18), .05 + .10 * fray);
            surface.DrawPolyline(TexturedOffsetPath(line, width * (float)(.66 + .52 * fray), phase + 2.8, fray),
                1, 1, fuzz, Math.Max(.22f, width * .26f));
            surface.DrawPolyline(TexturedOffsetPath(line, -width * (float)(.58 + .45 * fray), phase + 4.1, fray),
                1, 1, fuzz, Math.Max(.22f, width * .22f));
        }
    }

    private static Color32 SculptFiberColor(Color32 baseColor, Color32 shadowInk, double relief, double depth)
    {
        if (relief <= 0) return baseColor;
        Color32 shaded = ArtworkPaint.Mix(baseColor, shadowInk, relief * (.12 + .40 * depth));
        return ArtworkPaint.Mix(shaded, Color32.Opaque(246, 250, 255), relief * (.10 + .26 * (1 - depth)));
    }

    private static List<Vector2> TexturedOffsetPath(IReadOnlyList<Vector2> points, float offset, double phase, double texture)
    {
        var shifted = new List<Vector2>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 tangent = PathTangent(points, i);
            Vector2 normal = NormalizeOrUnit(new Vector2(-tangent.Y, tangent.X));
            double wave = Math.Sin(phase + i * .61)
                + .45 * Math.Sin(phase * 1.7 + i * 1.43)
                + .22 * Math.Sin(phase * .53 + i * 2.9);
            float jitter = (float)(offset * .22 * texture * wave);
            shifted.Add(points[i] + normal * (offset + jitter));
        }
        return shifted;
    }

    private static Vector2 PathTangent(IReadOnlyList<Vector2> points, int index)
    {
        if (points.Count == 1) return Vector2.UnitX;
        Vector2 tangent = index switch
        {
            0 => points[1] - points[0],
            _ when index == points.Count - 1 => points[index] - points[index - 1],
            _ => points[index + 1] - points[index - 1]
        };
        return NormalizeOrUnit(tangent);
    }

    private static Vector2 NormalizeOrUnit(Vector2 vector)
    {
        float length = vector.Length();
        return length > 1e-5f ? vector / length : Vector2.UnitX;
    }

    private static void RenderFlamebrush(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        (uint bg, uint[] colors) = r.Parameters["palette"] switch
        {
            "ember" => (0xFF130B1Du, new uint[] { 0xFFB33459, 0xFFFF9054, 0xFFFFDDA0 }),
            "orchid" => (0xFF16132Du, new uint[] { 0xFF643E9E, 0xFFE78CB9, 0xFF91DDE1 }),
            _ => (0xFF050914u, new uint[] { 0xFF28599D, 0xFF73C9E9, 0xFFD4ECF8 })
        };
        surface.Clear(ArtworkInk.Resolve(r, "background-color", Color32.FromArgb(bg)));
        Color32[] palette = PatternPalette.Resolve(r, ParticleRoles, colors);
        int rings = ArtworkPaint.Int(r, "rings"), stamps = ArtworkPaint.Int(r, "stamps");
        int particles = ArtworkPaint.Int(r, "particles");
        float size = Math.Min(r.Size.Width, r.Size.Height);
        float baseRadius = size * (float)ArtworkPaint.Number(r, "orbit-radius");
        float spacing = size * (float)ArtworkPaint.Number(r, "orbit-spacing");
        float brushRadius = Math.Max(1.3f, size * (float)ArtworkPaint.Number(r, "brush-radius") / 1080f);
        double turns = ArtworkPaint.Number(r, "spiral-turns");
        double phaseSpeed = ArtworkPaint.Number(r, "phase-speed");
        double scatter = ArtworkPaint.Number(r, "scatter");
        float dotSize = (float)ArtworkPaint.Number(r, "dot-size");
        double opacity = ArtworkPaint.Number(r, "opacity");
        var rnd = new SplitMix64Random(r.Seed);
        Vector2 center = new(r.Size.Width / 2f, r.Size.Height / 2f);

        for (int ring = 0; ring < rings; ring++)
        {
            ct.ThrowIfCancellationRequested();
            float radius = Math.Max(size * .03f, baseRadius - spacing * ring);
            double phase = rnd.NextDouble() * Math.Tau;
            for (int stamp = 0; stamp < stamps; stamp++)
            {
                if ((stamp & 15) == 0) ct.ThrowIfCancellationRequested();
                double a = Math.Tau * stamp / stamps + phase;
                Vector2 at = center + new Vector2(radius * (float)Math.Cos(a), radius * (float)Math.Sin(a));
                double rotation = a + stamp * phaseSpeed * .06 + ring * .8;
                float radiusAt = brushRadius * (float)(.72 + .55 * Math.Sin(Math.PI * stamp / stamps));
                Vector2[] spiral = SpiralStamp2D.Points(at, radiusAt, particles, rotation, turns);
                for (int i = 0; i < spiral.Length; i++)
                {
                    Vector2 dot = spiral[i];
                    float jitter = brushRadius * (float)(scatter * .35);
                    dot += new Vector2((float)(rnd.NextDouble() * 2 - 1) * jitter,
                        (float)(rnd.NextDouble() * 2 - 1) * jitter);
                    double tone = Math.Clamp(.12 + .8 * (ring + i / (double)particles) / Math.Max(1, rings), 0, 1);
                    Color32 pigment = PatternPalette.Gradient(palette, tone);
                    pigment = ArtworkPaint.Alpha(pigment, opacity * (.65 + .35 * rnd.NextDouble()));
                    float rDot = dotSize * (float)(.55 + .65 * rnd.NextDouble());
                    surface.DrawCircle(dot.X, dot.Y, rDot, pigment, fill: true);
                }
            }
        }
    }
}
