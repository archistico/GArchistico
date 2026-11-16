using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Randomness;
using static GenerativeArt.Rendering.Skia.ArtworkPaint;

namespace GenerativeArt.Rendering.Skia;

internal static class PainterlyRenderer
{
    internal static void Validate(ArtworkRecipe recipe)
    {
        if (recipe.Size.Width < 64 || recipe.Size.Height < 64)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Pennelli e paesaggi richiedono almeno 64 pixel per lato.");
        long cost = recipe.ArtworkId == "flow-brush"
            ? (long)Int(recipe, "strokes") * Int(recipe, "bristles") * (2L * Int(recipe, "steps") + 1)
                * (Number(recipe, "shadow") > 0 ? 2 : 1)
            : (long)Int(recipe, "layers") * (Int(recipe, "samples") + 1);
        if (cost > (recipe.ArtworkId == "flow-brush" ? 4_000_000 : 250_000))
            throw new ArgumentOutOfRangeException(nameof(recipe), "Budget pittorico superato: ridurre setole, strati, tracce o campioni.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        Color32[] colors = PainterlyPalettes.Resolve(recipe);
        surface.Clear(colors[0]);
        if (recipe.ArtworkId == "flow-brush") Brush(surface, recipe, colors, ct);
        else Terrain(surface, recipe, colors, ct);
    }

    private static void Brush(SkiaRasterSurface surface, ArtworkRecipe recipe, Color32[] colors, CancellationToken ct)
    {
        double unit = Math.Min(recipe.Size.Width, recipe.Size.Height) / 1000d;
        var bounds = new FlowBounds(0, 0, (float)(recipe.Size.Width / unit), (float)(recipe.Size.Height / unit));
        var raw = ArtisticFlow.Field(recipe.Parameters["field"], recipe.Seed, bounds,
            Number(recipe, "frequency"), Number(recipe, "curvature"));
        var field = VectorGrid2D.FromField(bounds, (int)Math.Ceiling(bounds.Right / 8) + 1,
            (int)Math.Ceiling(bounds.Bottom / 8) + 1, raw);
        var random = new SplitMix64Random(recipe.Seed);
        int strokes = Int(recipe, "strokes"), steps = Int(recipe, "steps"), bristles = Int(recipe, "bristles");
        float width = (float)Number(recipe, "brush-width"), step = (float)Number(recipe, "step-size");
        double opacity = Number(recipe, "opacity"), shadow = Number(recipe, "shadow");
        for (int stroke = 0; stroke < strokes; stroke++)
        {
            ct.ThrowIfCancellationRequested();
            Vector2 start = ArtisticFlow.Start(stroke, strokes, recipe.Parameters["start-layout"], random, bounds);
            var back = FlowTracer.Trace(field, start, steps, -step, bounds: bounds, cancellationToken: ct);
            var front = FlowTracer.Trace(field, start, steps, step, bounds: bounds, cancellationToken: ct);
            if (back.Count + front.Count < 6) continue;
            var centerline = new Vector2[back.Count + front.Count - 1];
            int k = 0;
            for (int i = back.Count - 1; i > 0; i--) centerline[k++] = back[i];
            for (int i = 0; i < front.Count; i++) centerline[k++] = front[i];
            double colorT = strokes == 1 ? .5 : (double)stroke / (strokes - 1);
            Color32 ink = colorT < .5 ? Mix(colors[1], colors[2], colorT * 2)
                : Mix(colors[2], colors[3], (colorT - .5) * 2);
            for (int pass = shadow > 0 ? 0 : 1; pass <= 1; pass++)
                foreach (BristleStroke bristle in OrganicStroke.Build(centerline, bristles, width,
                    Number(recipe, "taper"), Number(recipe, "roughness"),
                    recipe.Seed ^ ((ulong)stroke * 0x9E3779B97F4A7C15UL), ct))
                {
                    if (pass == 0)
                        for (int i = 0; i < bristle.Points.Length; i++) bristle.Points[i] += new Vector2(2, 3);
                    Color32 paint = pass == 0 ? Alpha(Color32.Opaque(12, 20, 28), shadow * bristle.InkWeight)
                        : Alpha(ink, opacity * bristle.InkWeight);
                    surface.DrawPolyline(bristle.Points, (float)unit, (float)unit, paint,
                        (float)(Number(recipe, "bristle-width") * unit * (pass == 0 ? 1.5 : 1)));
                }
        }
    }

    private static void Terrain(SkiaRasterSurface surface, ArtworkRecipe recipe, Color32[] colors, CancellationToken ct)
    {
        int width = recipe.Size.Width, height = recipe.Size.Height;
        double unit = Math.Min(width, height) / 1000d;
        double sunRadius = Number(recipe, "sun-radius") * Math.Min(width, height);
        if (sunRadius > 0)
            surface.DrawCircle((float)(width * Number(recipe, "sun-x")), (float)(height * Number(recipe, "sun-y")),
                (float)sunRadius, colors[6], fill: true);
        var profile = new FractalProfile1D(recipe.Seed, Int(recipe, "octaves"),
            Number(recipe, "persistence"), Number(recipe, "warp"));
        int layers = Int(recipe, "layers"), samples = Int(recipe, "samples");
        double horizon = Number(recipe, "horizon"), amplitude = Number(recipe, "height"), tilt = Number(recipe, "tilt");
        double frequency = Number(recipe, "frequency"), drift = Number(recipe, "layer-drift");
        double atmosphere = Number(recipe, "atmosphere"), shadow = Number(recipe, "shadow");
        float edgeWidth = (float)(Number(recipe, "edge-width") * unit);
        var top = new Vector2[samples + 1];
        var polygon = new Vector2[samples + 3];
        var shadowPath = new Vector2[samples + 1];
        for (int layer = 0; layer < layers; layer++)
        {
            ct.ThrowIfCancellationRequested();
            double t = (double)layer / (layers - 1);
            double baseY = height * (horizon + (1 - horizon) * t);
            for (int i = 0; i <= samples; i++)
            {
                if ((i & 255) == 0) ct.ThrowIfCancellationRequested();
                double x = (double)i / samples;
                double n = profile.Sample(x * frequency, layer * drift);
                n = recipe.Parameters["profile"] switch
                {
                    "ridge" => 1 - 2 * Math.Abs(n),
                    "waves" => Math.Sin(x * frequency * Math.Tau + n * 2),
                    _ => n
                };
                top[i] = new Vector2((float)(x * width), (float)(baseY
                    + height * amplitude * (.3 + .7 * t) * n + (x - .5) * height * tilt));
                polygon[i] = top[i];
            }
            polygon[^2] = new Vector2(width, height + 1);
            polygon[^1] = new Vector2(0, height + 1);
            if (shadow > 0)
                for (int pass = 3; pass >= 1; pass--)
                {
                    float spread = (float)(Number(recipe, "shadow-width") * unit * pass / 3);
                    for (int i = 0; i < top.Length; i++) shadowPath[i] = top[i] - new Vector2(0, spread * .2f);
                    surface.DrawPolyline(shadowPath, 1, 1, Alpha(colors[5], shadow / 3), spread * 2);
                }
            double palettePosition = t * 3;
            int index = Math.Min(2, (int)palettePosition);
            Color32 fill = Mix(colors[1 + index], colors[2 + index], palettePosition - index);
            fill = Mix(fill, colors[0], atmosphere * (1 - t));
            surface.DrawPolygon(polygon, fill, edgeWidth > 0 ? Alpha(colors[5], .45) : Color32.Transparent,
                Math.Max(.01f, edgeWidth));
        }
    }
}
