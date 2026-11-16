using System.Globalization;
using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Composizione di primitive SDF, CSG e linee equidistanti.
/// Un solo PixelBuffer per immagine, nessuna cache dei frame.
/// </summary>
internal static class SdfOrbitRenderer
{
    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        CanvasSize size = recipe.Size;
        float smoothness = (float)Number(recipe, "smoothness");
        float scale = (float)Number(recipe, "shape-scale");
        float edge = (float)Number(recipe, "edge-width");
        int bands = Int(recipe, "bands");
        (Color32 background, Color32 ink, Color32 fill, Color32 accent) = recipe.Parameters["palette"] switch
        {
            "mineral" => (Color32.Opaque(10, 22, 36), Color32.Opaque(159, 232, 228),
                          Color32.Opaque(34, 92, 108), Color32.Opaque(242, 219, 160)),
            "ember" => (Color32.Opaque(30, 13, 30), Color32.Opaque(255, 196, 141),
                        Color32.Opaque(132, 58, 73), Color32.Opaque(249, 225, 190)),
            "iris" => (Color32.Opaque(22, 18, 51), Color32.Opaque(215, 177, 251),
                       Color32.Opaque(76, 55, 151), Color32.Opaque(150, 240, 230)),
            _ => throw new ArgumentException("Palette SDF sconosciuta.")
        };
        background = ArtworkInk.Resolve(recipe, "background-color", background);
        ink = ArtworkInk.Resolve(recipe, "ink-color", ink);
        fill = ArtworkInk.Resolve(recipe, "fill-color", fill);
        accent = ArtworkInk.Resolve(recipe, "accent-color", accent);

        var rng = new SplitMix64Random(recipe.Seed);
        float angle = (float)(rng.NextDouble() * Math.PI * 2);
        Vector2 offset = new((float)((rng.NextDouble() - .5) * .12),
            (float)((rng.NextDouble() - .5) * .12));
        Vector2 centerA = SignedDistance2D.Rotate(new Vector2(-.15f, -.06f), angle) + offset;
        Vector2 centerB = SignedDistance2D.Rotate(new Vector2(.15f, .07f), angle) + offset;
        int samplingScale = surface.RasterScale;
        var rasterSize = new CanvasSize(size.Width * samplingScale, size.Height * samplingScale);
        var pixels = new PixelBuffer(rasterSize);
        float unit = Math.Max(1, Math.Min(rasterSize.Width, rasterSize.Height));
        float cx = (rasterSize.Width - 1) * .5f, cy = (rasterSize.Height - 1) * .5f;
        float step = 1f / (bands * 2.4f);
        for (int y = 0; y < rasterSize.Height; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            float py = (y - cy) / unit;
            for (int x = 0; x < rasterSize.Width; x++)
            {
                Vector2 p = new((x - cx) / unit, py);
                Vector2 q = p / scale;
                float a = SignedDistance2D.Circle(q - centerA, .245f);
                float b = SignedDistance2D.Circle(q - centerB, .23f);
                float flower = SignedDistance2D.SmoothUnion(a, b, smoothness);
                Vector2 rotated = SignedDistance2D.Rotate(q - offset, angle);
                float box = SignedDistance2D.RoundedBox(rotated, new Vector2(.27f, .095f), .055f);
                flower = SignedDistance2D.SmoothUnion(flower, box, smoothness * .75f);
                float hole = SignedDistance2D.Circle(q - offset, .07f);
                float body = SignedDistance2D.Difference(flower, hole);
                float ring = SignedDistance2D.Difference(
                    SignedDistance2D.Circle(q - offset, .465f),
                    SignedDistance2D.Circle(q - offset, .43f));
                float clipping = SignedDistance2D.Intersection(body,
                    SignedDistance2D.Circle(q - offset, .46f));
                float distance = SignedDistance2D.Union(clipping, ring) * scale;
                float stripe = MathF.Abs(distance / step - MathF.Round(distance / step)) * step;
                float boundary = MathF.Abs(distance);
                Color32 color = distance < 0 ? fill : background;
                if (stripe < edge * .45f) color = distance < 0 ? accent : ink;
                if (boundary < edge) color = ink;
                row[x] = color.PackedArgb;
            }
        }
        surface.DrawPixels(pixels, nativeResolution: true);
    }

    private static int Int(ArtworkRecipe recipe, string key) =>
        int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    private static double Number(ArtworkRecipe recipe, string key) =>
        double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
