using GenerativeArt.Algorithms.Pattern;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Renderer Gray–Scott con simulazione a risoluzione di griglia e campionamento
/// bilineare del pigmento sul raster nativo, anche quando è richiesto fine-2x.
/// </summary>
internal static class ReactionDiffusionRenderer
{
    internal const long MaximumUpdates = 75_000_000;

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        int cell = ArtworkPaint.Int(recipe, "cell-size");
        int width = checked((recipe.Size.Width + cell - 1) / cell);
        int height = checked((recipe.Size.Height + cell - 1) / cell);
        long pixels = (long)width * height;
        if (width < 4 || height < 4 || pixels > 350_000)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Reazione–diffusione: griglia da 4×4 a 350.000 celle. Aumentare cell-size.");
        if (pixels * ArtworkPaint.Int(recipe, "iterations") > MaximumUpdates)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Reazione–diffusione: massimo 75 milioni di aggiornamenti. Ridurre iterazioni o aumentare cell-size.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        ValidateBudget(recipe);
        int cell = ArtworkPaint.Int(recipe, "cell-size");
        int width = (recipe.Size.Width + cell - 1) / cell;
        int height = (recipe.Size.Height + cell - 1) / cell;
        var field = new ReactionDiffusionField2D(width, height, recipe.Seed,
            ArtworkPaint.Int(recipe, "inoculations"), ArtworkPaint.Int(recipe, "inoculation-radius"));
        field.Evolve(ArtworkPaint.Int(recipe, "iterations"),
            ArtworkPaint.Number(recipe, "feed"), ArtworkPaint.Number(recipe, "kill"),
            ArtworkPaint.Number(recipe, "diffusion-u"), ArtworkPaint.Number(recipe, "diffusion-v"), ct);
        uint[] palette = recipe.Parameters["palette"] switch
        {
            "copper" => [0xFF1A1329, 0xFF7C384E, 0xFFE2A073, 0xFFFFF0BE],
            "lichen" => [0xFF121F25, 0xFF345D4C, 0xFF8BB26D, 0xFFF5E8BD],
            _ => [0xFF081A35, 0xFF29618C, 0xFF68C9C5, 0xFFF1F1C9]
        };
        Color32[] colors = PatternPalette.Resolve(recipe,
            ["gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4"], palette);
        string style = recipe.Parameters["style"];
        double exposure = ArtworkPaint.Number(recipe, "exposure");
        double bands = ArtworkPaint.Number(recipe, "bands");
        double contrast = ArtworkPaint.Number(recipe, "contrast");
        int scale = surface.RasterScale;
        var native = new CanvasSize(checked(recipe.Size.Width * scale), checked(recipe.Size.Height * scale));
        var pixels = new PixelBuffer(native);
        for (int y = 0; y < native.Height; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            double sy = (y + .5) / native.Height;
            for (int x = 0; x < native.Width; x++)
            {
                double sample = field.Sample((x + .5) / native.Width, sy);
                double tone = Math.Clamp(sample * exposure, 0, 1);
                tone = style switch
                {
                    "contours" => .5 + .5 * Math.Sin(tone * Math.Tau * bands),
                    "islands" => Math.Clamp((tone - .28) * 3.2, 0, 1),
                    _ => tone
                };
                tone = Math.Clamp(.5 + (tone - .5) * contrast, 0, 1);
                row[x] = PatternPalette.Gradient(colors, tone).PackedArgb;
            }
        }
        ct.ThrowIfCancellationRequested();
        surface.DrawPixels(pixels, nativeResolution: true);
    }
}
