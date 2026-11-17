using System.Globalization;
using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

internal static class LineworkRenderer
{
    private const long MaximumCurveVertices = 4_000_000;
    private const long MaximumNoiseSamples = 400_000_000;

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        if (recipe.Size.Width < 64 || recipe.Size.Height < 64)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Incisione e Guilloché richiedono almeno 64 pixel per lato.");
        if (recipe.ArtworkId == "guilloche")
        {
            GuillocheCurve curve = Curve(recipe);
            if ((long)(Math.Max(Int(recipe, "samples"), curve.MinimumSamples) + 1)
                    * Int(recipe, "layers") > RenderWorkPolicy.Limit(recipe, MaximumCurveVertices, 50_000_000))
                throw new ArgumentOutOfRangeException(nameof(recipe),
                    "Guilloché: massimo 4 milioni di vertici complessivi. Ridurre curve o campioni.");
        }
        else
        {
            if (Number(recipe, "line-width") > Number(recipe, "spacing"))
                throw new ArgumentOutOfRangeException(nameof(recipe), "Lo spessore del tratteggio non può superare la distanza fra i tratti.");
            int scale = RasterQualityPolicy.Scale(recipe);
            if (recipe.Size.PixelCount * scale * scale * Int(recipe, "octaves") * 5 > RenderWorkPolicy.Limit(recipe, MaximumNoiseSamples, 2_000_000_000))
                throw new ArgumentOutOfRangeException(nameof(recipe), "Incisione: massimo 400 milioni di campioni noise.");
        }
    }

    private static GuillocheCurve Curve(ArtworkRecipe recipe)
    {
        var random = new SplitMix64Random(recipe.Seed);
        return new GuillocheCurve(Int(recipe, "fixed-teeth"), Int(recipe, "moving-teeth"),
            Number(recipe, "pen-radius"), recipe.Parameters["curve"] == "outside",
            Int(recipe, "waves"), Number(recipe, "wave-amount"), random.NextDouble() * Math.Tau);
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        Color32[] colors = Palette(recipe);
        if (recipe.ArtworkId == "guilloche") RenderGuilloche(surface, recipe, colors, ct);
        else RenderEngraving(surface, recipe, colors, ct);
    }

    private static void RenderGuilloche(SkiaRasterSurface surface, ArtworkRecipe recipe,
        Color32[] colors, CancellationToken ct)
    {
        GuillocheCurve curve = Curve(recipe);
        Vector2[] original = curve.CreatePolyline(Int(recipe, "samples"), ct);
        var transformed = new Vector2[original.Length];
        int layers = Int(recipe, "layers");
        double unit = Math.Min(recipe.Size.Width, recipe.Size.Height) / 1000d;
        double radius = 500 * unit * Number(recipe, "scale") / curve.EnvelopeRadius;
        double inner = Number(recipe, "inner-scale"), bias = Number(recipe, "layer-bias");
        double twist = Number(recipe, "twist"), rotation = Number(recipe, "rotation");
        double opacity = Number(recipe, "opacity");
        var random = new SplitMix64Random(recipe.Seed);
        rotation += random.NextDouble() * 360;
        double centerX = recipe.Size.Width * Number(recipe, "center-x");
        double centerY = recipe.Size.Height * Number(recipe, "center-y");
        float width = (float)(Number(recipe, "line-width") * unit);
        surface.Clear(colors[0]);
        for (int layer = 0; layer < layers; layer++)
        {
            ct.ThrowIfCancellationRequested();
            double t = layers == 1 ? 1 : (double)layer / (layers - 1);
            double size = radius * (inner + (1 - inner) * Math.Pow(t, bias));
            double angle = (rotation + twist * (t - .5)) * Math.PI / 180;
            double cos = Math.Cos(angle) * size, sin = Math.Sin(angle) * size;
            for (int i = 0; i < original.Length; i++)
            {
                if ((i & 1023) == 0) ct.ThrowIfCancellationRequested();
                Vector2 p = original[i];
                transformed[i] = new Vector2((float)(centerX + p.X * cos - p.Y * sin),
                    (float)(centerY + p.X * sin + p.Y * cos));
            }
            Color32 ink = t < .5 ? Mix(colors[1], colors[2], t * 2)
                : Mix(colors[2], colors[3], (t - .5) * 2);
            surface.DrawPolyline(transformed, 1, 1, Alpha(ink, opacity), width);
        }
    }

    private static void RenderEngraving(SkiaRasterSurface surface, ArtworkRecipe recipe,
        Color32[] colors, CancellationToken ct)
    {
        int scale = surface.RasterScale;
        var pixels = new PixelBuffer(new CanvasSize(recipe.Size.Width * scale, recipe.Size.Height * scale));
        double unit = Math.Min(recipe.Size.Width, recipe.Size.Height) / 1000d;
        double centerX = recipe.Size.Width * Number(recipe, "center-x");
        double centerY = recipe.Size.Height * Number(recipe, "center-y");
        double frequency = Number(recipe, "frequency"), bands = Int(recipe, "bands");
        double bend = Number(recipe, "bend"), contrast = Number(recipe, "contrast");
        double offset = Number(recipe, "tone-offset"), gamma = Number(recipe, "gamma");
        double wash = Number(recipe, "wash");
        string field = recipe.Parameters["field"], mask = recipe.Parameters["mask"];
        bool inverted = recipe.Parameters["tone-direction"] == "inverted";
        double maskRadius = Number(recipe, "mask-radius"), feather = Number(recipe, "mask-feather");
        var noise = new DomainWarpNoise2D(recipe.Seed, Int(recipe, "octaves"), Number(recipe, "warp-amount"));
        var hatching = new TonalHatching(Number(recipe, "spacing"), Number(recipe, "line-width"),
            Number(recipe, "angle"), Number(recipe, "cross-angle"), Int(recipe, "hatch-layers"));
        GenerativeArt.Core.Rendering.RasterRows.For(0, pixels.Height, RenderWorkPolicy.Production(recipe), ct, y =>
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            double py = (((y + .5) / scale - .5) - centerY) / unit;
            for (int x = 0; x < pixels.Width; x++)
            {
                double px = (((x + .5) / scale - .5) - centerX) / unit;
                double maskValue = Mask(mask, px / 500, py / 500, maskRadius, feather);
                if (maskValue == 0) { row[x] = colors[0].PackedArgb; continue; }
                double n = noise.Sample(px * frequency, py * frequency);
                double tone = field switch
                {
                    "rings" => .5 + .5 * Math.Cos(Math.Sqrt(px * px + py * py) * bands * .015 + n * 2),
                    "marble" => .5 + .5 * Math.Sin(px * .006 + n * bands * Math.PI),
                    _ => .5 + .5 * n
                };
                double darkness = Math.Pow(Math.Clamp(.5 + contrast * (tone - .5) + offset, 0, 1), gamma);
                if (inverted) darkness = 1 - darkness;
                HatchCoverage hatch = hatching.Sample(px + bend * n, py + bend * n * .5,
                    darkness, 1 / (scale * unit));
                Color32 color = Alpha(colors[3], wash * darkness * maskValue).Over(colors[0]);
                color = Alpha(colors[1], hatch.Primary * maskValue).Over(color);
                color = Alpha(colors[2], hatch.Cross * maskValue).Over(color);
                row[x] = color.PackedArgb;
            }
        });
        surface.DrawPixels(pixels, nativeResolution: true);
    }

    private static double Mask(string shape, double x, double y, double radius, double feather)
    {
        if (shape == "none") return 1;
        double distance = (shape == "circle" ? Math.Sqrt(x * x + y * y) : Math.Abs(x) + Math.Abs(y)) - radius;
        if (feather == 0) return distance <= 0 ? 1 : 0;
        double t = Math.Clamp((distance + feather) / (2 * feather), 0, 1);
        return 1 - t * t * (3 - 2 * t);
    }

    private static Color32[] Palette(ArtworkRecipe recipe)
    {
        uint[] values = recipe.Parameters["palette"] switch
        {
            "banknote" => [0xFFF4F0DC, 0xFF164F55, 0xFF587B68, 0xFFAD8050],
            "copper" => [0xFFF2DFCA, 0xFF522C31, 0xFF9C5141, 0xFFCB8D52],
            _ => [0xFF0B152B, 0xFF63C8CA, 0xFFA5BCE3, 0xFFE6C28A]
        };
        string[] roles = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3"];
        return values.Select((value, i) => ArtworkInk.Resolve(recipe, roles[i], Color32.FromArgb(value))).ToArray();
    }

    private static Color32 Alpha(Color32 color, double opacity) =>
        color.WithAlpha((byte)Math.Clamp((int)Math.Round(color.A * Math.Clamp(opacity, 0, 1)), 0, 255));

    private static Color32 Mix(Color32 a, Color32 b, double t)
    {
        byte Blend(byte left, byte right) => (byte)Math.Clamp((int)Math.Round(left + (right - left) * t), 0, 255);
        return new Color32(Blend(a.A, b.A), Blend(a.R, b.R), Blend(a.G, b.G), Blend(a.B, b.B));
    }

    private static int Int(ArtworkRecipe recipe, string key) => int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    private static double Number(ArtworkRecipe recipe, string key) => double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
