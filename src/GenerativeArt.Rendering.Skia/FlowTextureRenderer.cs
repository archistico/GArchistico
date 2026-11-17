using System.Globalization;
using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;

namespace GenerativeArt.Rendering.Skia;

internal static class FlowTextureRenderer
{
    internal const long MaximumNoiseSamples = 400_000_000;

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        int scale = RasterQualityPolicy.Scale(recipe);
        if (recipe.ArtworkId == "lic-flow")
            LineIntegralConvolution.ValidateBudget(recipe.Size.Width * scale,
                recipe.Size.Height * scale, Int(recipe, "length"), RenderWorkPolicy.Production(recipe));
        else if (recipe.Size.PixelCount * scale * scale * Int(recipe, "octaves")
                 * (1 + 2L * Int(recipe, "warp-layers")) > RenderWorkPolicy.Limit(recipe, MaximumNoiseSamples, 2_000_000_000))
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Domain Warp: massimo 400 milioni di campioni noise. Ridurre ottave, livelli o risoluzione.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        int scale = surface.RasterScale;
        var size = new CanvasSize(recipe.Size.Width * scale, recipe.Size.Height * scale);
        Color32[] palette = Palette(recipe);
        var pixels = new PixelBuffer(size);
        if (recipe.ArtworkId == "domain-warp") RenderWarp(pixels, scale, recipe, palette, ct);
        else RenderLic(pixels, scale, recipe, palette, ct);
        surface.DrawPixels(pixels, nativeResolution: true);
    }

    private static void RenderWarp(PixelBuffer pixels, int scale, ArtworkRecipe recipe,
        Color32[] palette, CancellationToken ct)
    {
        var noise = new DomainWarpNoise2D(recipe.Seed, Int(recipe, "octaves"),
            Number(recipe, "warp-amount"), Int(recipe, "warp-layers"), Number(recipe, "lacunarity"),
            Number(recipe, "persistence"), Number(recipe, "warp-frequency"));
        double frequency = Number(recipe, "frequency"), bands = Number(recipe, "bands");
        string pattern = recipe.Parameters["pattern"];
        double rotation = Number(recipe, "rotation") * Math.PI / 180;
        double cos = Math.Cos(rotation), sin = Math.Sin(rotation);
        double stretchX = Number(recipe, "stretch-x"), stretchY = Number(recipe, "stretch-y");
        double offsetX = Number(recipe, "offset-x"), offsetY = Number(recipe, "offset-y");
        double contrast = Number(recipe, "contrast"), gamma = Number(recipe, "gamma");
        bool transform = rotation != 0 || stretchX != 1 || stretchY != 1 || offsetX != 0 || offsetY != 0;
        GenerativeArt.Core.Rendering.RasterRows.For(0, pixels.Height, RenderWorkPolicy.Production(recipe), ct, y =>
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            double py = ((y + .5) / scale - .5) * frequency;
            for (int x = 0; x < pixels.Width; x++)
            {
                double px = ((x + .5) / scale - .5) * frequency;
                double sx = px, sy = py;
                if (transform)
                {
                    sx = (px * cos - py * sin) * stretchX + offsetX;
                    sy = (px * sin + py * cos) * stretchY + offsetY;
                }
                double value = noise.Sample(sx, sy);
                double t = pattern switch
                {
                    "clouds" => .5 + .5 * value,
                    "ridges" => 1 - Math.Abs(value),
                    _ => .5 + .5 * Math.Sin(sx + value * bands * Math.PI)
                };
                if (contrast != 1) t = Math.Clamp(.5 + contrast * (t - .5), 0, 1);
                if (gamma != 1) t = Math.Pow(Math.Clamp(t, 0, 1), gamma);
                row[x] = Gradient(palette, t).PackedArgb;
            }
        });
    }

    private static void RenderLic(PixelBuffer pixels, int scale, ArtworkRecipe recipe,
        Color32[] palette, CancellationToken ct)
    {
        var bounds = new FlowBounds(0, 0, recipe.Size.Width, recipe.Size.Height);
        IVectorField2D original = recipe.Parameters["field"] == "vortex"
            ? VectorFields.Vortex(new Vector2(recipe.Size.Width * .5f, recipe.Size.Height * .5f))
            : new CurlNoiseField2D(recipe.Seed, (float)Number(recipe, "frequency"));
        var grid = VectorGrid2D.FromField(bounds,
            Math.Max(2, (int)Math.Ceiling(recipe.Size.Width / 8d) + 1),
            Math.Max(2, (int)Math.Ceiling(recipe.Size.Height / 8d) + 1), original);
        var field = VectorFields.FromFunction((x, y) =>
            grid.Sample((x + .5f) / scale - .5f, (y + .5f) / scale - .5f));
        double textureScale = Number(recipe, "texture-scale");
        var texture = new float[checked(pixels.Width * pixels.Height)];
        GenerativeArt.Core.Rendering.RasterRows.For(0, pixels.Height, RenderWorkPolicy.Production(recipe), ct, y =>
        {
            ct.ThrowIfCancellationRequested();
            for (int x = 0; x < pixels.Width; x++)
                texture[y * pixels.Width + x] = TextureAt(
                    ((x + .5) / scale - .5) / textureScale,
                    ((y + .5) / scale - .5) / textureScale, recipe.Seed);
        });
        float[] values = LineIntegralConvolution.Apply(pixels.Width, pixels.Height,
            texture, field, Int(recipe, "length"), scale, ct, RenderWorkPolicy.Production(recipe));
        double contrast = Number(recipe, "contrast");
        GenerativeArt.Core.Rendering.RasterRows.For(0, pixels.Height, RenderWorkPolicy.Production(recipe), ct, y =>
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            for (int x = 0; x < pixels.Width; x++)
                row[x] = Gradient(palette, .5 + contrast * (values[y * pixels.Width + x] - .5)).PackedArgb;
        });
    }

    // Rumore bianco su reticolo, interpolato per mantenere la scala logica in fine-2x.
    private static float TextureAt(double x, double y, ulong seed)
    {
        int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y);
        double tx = x - ix, ty = y - iy;
        float Hash(int a, int b)
        {
            unchecked
            {
                ulong bits = seed ^ (ulong)(long)a * 0x9E3779B97F4A7C15UL
                                  ^ (ulong)(long)b * 0xD1B54A32D192ED03UL;
                bits = (bits ^ (bits >> 30)) * 0xBF58476D1CE4E5B9UL;
                bits = (bits ^ (bits >> 27)) * 0x94D049BB133111EBUL;
                return (float)((bits ^ (bits >> 31)) >> 40) / 16_777_215;
            }
        }
        double top = Hash(ix, iy) * (1 - tx) + Hash(ix + 1, iy) * tx;
        double bottom = Hash(ix, iy + 1) * (1 - tx) + Hash(ix + 1, iy + 1) * tx;
        return (float)(top * (1 - ty) + bottom * ty);
    }

    private static Color32[] Palette(ArtworkRecipe recipe)
    {
        uint[] colors = recipe.Parameters["palette"] switch
        {
            "ocean" => [0xFF081C32, 0xFF15576C, 0xFF54B5AB, 0xFFF3E4B4],
            "ember" => [0xFF241327, 0xFF88384B, 0xFFDE8654, 0xFFFFE7AE],
            _ => [0xFF171E26, 0xFF4B5A66, 0xFFA4B2B9, 0xFFF3EDE0]
        };
        return colors.Select((color, i) => ArtworkInk.Resolve(recipe,
            $"gradient-color-{i + 1}", Color32.FromArgb(color))).ToArray();
    }

    private static Color32 Gradient(Color32[] palette, double t)
    {
        double position = Math.Clamp(t, 0, 1) * (palette.Length - 1);
        int index = Math.Min(palette.Length - 2, (int)position);
        double fraction = position - index;
        Color32 a = palette[index], b = palette[index + 1];
        byte Blend(byte left, byte right) => (byte)Math.Clamp(
            (int)Math.Round(left + (right - left) * fraction), 0, 255);
        return new Color32(Blend(a.A, b.A), Blend(a.R, b.R), Blend(a.G, b.G), Blend(a.B, b.B));
    }

    private static int Int(ArtworkRecipe recipe, string key) =>
        int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    private static double Number(ArtworkRecipe recipe, string key) =>
        double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
