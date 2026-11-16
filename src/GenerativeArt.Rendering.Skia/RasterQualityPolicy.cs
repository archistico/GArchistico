using GenerativeArt.Application.Recipes;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Il campionamento 2x impiega una superficie nativa temporanea e, per le opere
/// pixel-based, un PixelBuffer gestito delle stesse dimensioni. Il budget viene
/// applicato in Validate prima di creare superfici o scrivere nello stream.
/// </summary>
public static class RasterQualityPolicy
{
    public const long MaximumFineRasterPixels = 16_777_216;

    public static int Scale(ArtworkRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        // Compatibilità diretta anche con le ricette v1 serializzate senza questo parametro.
        if (!recipe.Parameters.TryGetValue("render-quality", out string? quality) || quality == "standard")
            return 1;
        if (quality == "fine-2x") return 2;
        throw new ArgumentException("Campionamento raster non supportato.", nameof(recipe));
    }

    public static void Validate(ArtworkRecipe recipe)
    {
        int scale = Scale(recipe);
        if (scale == 2 && recipe.Size.PixelCount * 4L > MaximumFineRasterPixels)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                $"Qualità fine-2x: massimo {MaximumFineRasterPixels:N0} pixel di superficie temporanea. " +
                "Ridurre il canvas oppure usare render-quality=standard.");
    }
}
