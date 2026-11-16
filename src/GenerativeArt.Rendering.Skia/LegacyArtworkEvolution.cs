using System.Numerics;
using GenerativeArt.Algorithms.Animation;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Core.Randomness;
using GenerativeArt.Core.Rendering;
using static GenerativeArt.Rendering.Skia.ArtworkPaint;

namespace GenerativeArt.Rendering.Skia;

internal static class LegacyArtworkEvolution
{
    internal static bool IsExtended(ArtworkRecipe recipe) =>
        LegacyArtworkControls.ForArtwork(recipe.ArtworkId).Any(p => recipe.Parameters[p.Id] != p.DefaultValue)
        || (recipe.ArtworkId == "noise-flow" && Int(recipe, "lines") > 500);

    internal static bool HasFlowGeometryChanges(ArtworkRecipe recipe) =>
        LegacyArtworkControls.ForArtwork(recipe.ArtworkId)
            .Where(p => p.Id is not ("opacity" or "line-width"))
            .Any(p => recipe.Parameters[p.Id] != p.DefaultValue)
        || (recipe.ArtworkId == "noise-flow" && Int(recipe, "lines") > 500);

    internal static void Validate(ArtworkRecipe recipe)
    {
        if (recipe.ArtworkId == "noise-flow" && HasFlowGeometryChanges(recipe))
        {
            double unit = Math.Min(recipe.Size.Width, recipe.Size.Height) / 1000d;
            double nodes = (Math.Ceiling(recipe.Size.Width / unit / 10) + 1)
                * (Math.Ceiling(recipe.Size.Height / unit / 10) + 1);
            if (nodes > VectorGrid2D.MaximumNodes)
                throw new ArgumentOutOfRangeException(nameof(recipe), "Rapporto del canvas eccessivo per il campo vettoriale.");
        }
        long cost = recipe.ArtworkId switch
        {
            "parametric-curves" => (long)Int(recipe, "layers") * (Int(recipe, "samples") + 1),
            "noise-flow" => (long)Int(recipe, "lines") * (Int(recipe, "steps") + 1),
            "spaced-streamlines" => (long)Int(recipe, "max-lines") * (2L * Int(recipe, "max-steps") + 1),
            "looping-flow" => (long)Math.Max(54, Int(recipe, "lines")) * (Int(recipe, "steps") + 1),
            _ => 0
        };
        if (cost > (recipe.ArtworkId == "spaced-streamlines" ? 8_000_000 : 4_000_000))
            throw new ArgumentOutOfRangeException(nameof(recipe), "Troppe tracce o campioni: ridurre densità o precisione.");
        if (recipe.ArtworkId == "looping-flow" && recipe.Animation is { } animation
            && cost * animation.FrameCount > 20_000_000)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Loop: massimo 20 milioni di vertici stimati nella sequenza.");
    }

    private static void Background(SkiaRasterSurface surface, ArtworkRecipe recipe, bool curves, CancellationToken ct)
    {
        if (!ArtworkInk.IsAuto(recipe, "background-color"))
        { surface.Clear(ArtworkInk.Resolve(recipe, "background-color", Color32.Transparent)); return; }
        var pixels = new PixelBuffer(recipe.Size);
        for (int y = 0; y < pixels.Height; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            for (int x = 0; x < pixels.Width; x++)
                row[x] = curves ? Color32.Opaque((byte)(14 + 36L * x / Math.Max(1, pixels.Width - 1)),
                    (byte)(21 + 47L * y / Math.Max(1, pixels.Height - 1)), 57).PackedArgb
                    : Color32.Opaque((byte)(12 + 17L * x / Math.Max(1, pixels.Width - 1)),
                    (byte)(16 + 16L * y / Math.Max(1, pixels.Height - 1)), 37).PackedArgb;
        }
        surface.DrawPixels(pixels);
    }

    internal static void Parametric(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        Background(surface, recipe, true, ct);
        int layers = Int(recipe, "layers"), samples = Int(recipe, "samples");
        int fx = Int(recipe, "frequency-x"), fy = Int(recipe, "frequency-y");
        double phase = Number(recipe, "phase"), inner = Number(recipe, "layer-scale");
        double stretchX = Number(recipe, "stretch-x"), stretchY = Number(recipe, "stretch-y");
        double rotation = Number(recipe, "rotation"), twist = Number(recipe, "twist");
        string style = recipe.Parameters["style"];
        var random = new SplitMix64Random(recipe.Seed);
        double seedPhase = style == "legacy" ? 0 : random.NextDouble() * Math.Tau;
        double radius = Math.Min(recipe.Size.Width, recipe.Size.Height) * Number(recipe, "scale");
        Color32 stroke = ArtworkInk.Resolve(recipe, "curve-color", new Color32(175, 255, 220, 136));
        Color32 orbit = ArtworkInk.Resolve(recipe, "orbit-color", Color32.Opaque(112, 202, 231));
        var path = new Vector2[samples + 1];
        for (int layer = 0; layer < layers; layer++)
        {
            ct.ThrowIfCancellationRequested();
            double t = layers == 1 ? 1 : (double)layer / (layers - 1);
            double size = radius * (inner + (1 - inner) * t);
            double angle = (rotation + twist * (t - .5)) * Math.PI / 180;
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            double shift = style == "woven" ? t * Math.Tau * .35 : 0;
            for (int i = 0; i < samples; i++)
            {
                if ((i & 255) == 0) ct.ThrowIfCancellationRequested();
                double a = i * Math.Tau / samples;
                double x = size * stretchX * Math.Sin(a * fx + shift);
                double y = size * stretchY * Math.Sin(a * fy + phase + seedPhase + shift);
                path[i] = new Vector2((float)(recipe.Size.CenterX + x * cos - y * sin),
                    (float)(recipe.Size.CenterY + x * sin + y * cos));
            }
            path[samples] = path[0];
            surface.DrawPolyline(path, 1, 1, style == "legacy" ? stroke : Mix(stroke, orbit, t * .75),
                (float)Number(recipe, "line-width"));
        }
        if (Number(recipe, "orbit-width") > 0)
            surface.DrawCircle(recipe.Size.CenterX, recipe.Size.CenterY, (float)(radius * 1.07), orbit,
                strokeWidth: (float)Number(recipe, "orbit-width"));
    }

    internal static void NoiseFlow(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        Background(surface, recipe, false, ct);
        double unit = Math.Min(recipe.Size.Width, recipe.Size.Height) / 1000d;
        var bounds = new FlowBounds(0, 0, (float)(recipe.Size.Width / unit), (float)(recipe.Size.Height / unit));
        var raw = ArtisticFlow.Field(recipe.Parameters["field"], recipe.Seed, bounds,
            Number(recipe, "frequency"), Number(recipe, "curvature"));
        var field = VectorGrid2D.FromField(bounds, (int)Math.Ceiling(bounds.Right / 10) + 1,
            (int)Math.Ceiling(bounds.Bottom / 10) + 1, raw);
        var random = new SplitMix64Random(recipe.Seed);
        int lines = Int(recipe, "lines"), steps = Int(recipe, "steps");
        Color32[] colors = LegacyColors(recipe, "ink-color-1", "ink-color-2", "ink-color-3");
        for (int i = 0; i < lines; i++)
        {
            ct.ThrowIfCancellationRequested();
            Vector2 start = ArtisticFlow.Start(i, lines, recipe.Parameters["start-layout"], random, bounds);
            var path = FlowTracer.Trace(field, start, steps, (float)Number(recipe, "step-size"),
                bounds: bounds, cancellationToken: ct);
            surface.DrawPolyline(path, (float)unit, (float)unit, Alpha(colors[i % colors.Length], Number(recipe, "opacity")),
                (float)(Number(recipe, "line-width") * unit));
        }
    }

    internal static void Loop(SkiaRasterSurface surface, ArtworkRecipe recipe, int frame, CancellationToken ct)
    {
        double unit = Math.Min(recipe.Size.Width, recipe.Size.Height) / 1000d;
        var bounds = new FlowBounds(0, 0, (float)(recipe.Size.Width / unit), (float)(recipe.Size.Height / unit));
        LoopFrame time = LoopFrame.At(frame, recipe.Animation!.FrameCount, recipe.Animation.FramesPerSecond);
        var field = new LoopNoiseField2D(recipe.Seed, time.Phase, Number(recipe, "frequency"), Number(recipe, "loop-radius"));
        Color32[] colors = LegacyColors(recipe, "background-color", "ink-color-1", "ink-color-2", "ink-color-3", "ink-color-4");
        surface.Clear(colors[0]);
        int lines = Int(recipe, "lines");
        if (lines == 0) lines = Math.Min(54, Math.Max(18, recipe.Size.Width * recipe.Size.Height / 12000));
        var random = new SplitMix64Random(recipe.Seed);
        for (int i = 0; i < lines; i++)
        {
            ct.ThrowIfCancellationRequested();
            Vector2 start = ArtisticFlow.Start(i, lines, "grid", random, bounds);
            var path = FlowTracer.Trace(field, start, Int(recipe, "steps"), (float)Number(recipe, "step-size"),
                bounds: bounds, cancellationToken: ct);
            surface.DrawPolyline(path, (float)unit, (float)unit, Alpha(colors[1 + i % 4], Number(recipe, "opacity")),
                (float)(Number(recipe, "line-width") * unit));
        }
    }

    internal static void Plasma(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        var noise = new SeededSimplexNoise(recipe.Seed);
        Color32[] colors = LegacyColors(recipe, "gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4");
        int scale = surface.RasterScale, octaves = Int(recipe, "octaves"), bands = Int(recipe, "bands");
        var pixels = new PixelBuffer(new CanvasSize(recipe.Size.Width * scale, recipe.Size.Height * scale));
        double frequency = Number(recipe, "frequency"), persistence = Number(recipe, "persistence");
        double lacunarity = Number(recipe, "lacunarity"), contrast = Number(recipe, "contrast"), gamma = Number(recipe, "gamma");
        double angle = Number(recipe, "rotation") * Math.PI / 180, cos = Math.Cos(angle), sin = Math.Sin(angle);
        double stretchX = Number(recipe, "stretch-x"), stretchY = Number(recipe, "stretch-y");
        double contour = Number(recipe, "contour-strength");
        string pattern = recipe.Parameters["pattern"];
        for (int y = 0; y < pixels.Height; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            for (int x = 0; x < pixels.Width; x++)
            {
                double px = (x + .5) / scale - .5, py = (y + .5) / scale - .5;
                double sx = (px * cos - py * sin) * stretchX + 13.5;
                double sy = (px * sin + py * cos) * stretchY - 7.25;
                double f = frequency, weight = 1, sum = 0, total = 0;
                for (int octave = 0; octave < octaves; octave++)
                { sum += weight * noise.Sample2D(sx * f, sy * f); total += weight; f *= lacunarity; weight *= persistence; }
                double value = sum / total;
                double t = pattern switch
                {
                    "turbulence" => Math.Abs(value), "ridged" => 1 - Math.Abs(value),
                    "marble" => .5 + .5 * Math.Sin(sx * frequency + value * bands * Math.PI),
                    _ => .5 + .5 * value
                };
                t = Math.Pow(Math.Clamp(.5 + (t - .5) * contrast, 0, 1), gamma);
                double position = t * 3;
                int index = Math.Min(2, (int)position);
                Color32 color = Mix(colors[index], colors[index + 1], position - index);
                if (contour > 0)
                {
                    double distance = Math.Abs(t * bands - Math.Round(t * bands));
                    color = Mix(color, colors[0], contour * Math.Clamp(1 - distance / .045, 0, 1));
                }
                row[x] = color.PackedArgb;
            }
        }
        surface.DrawPixels(pixels, nativeResolution: true);
    }
}
