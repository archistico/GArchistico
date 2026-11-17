using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Budget del canvas finale, separato dai budget del supersampling, delle
/// finiture e dei singoli algoritmi. La prima abilitazione 8K riguarda
/// soltanto renderer che disegnano direttamente su Skia senza creare una
/// seconda bitmap della dimensione completa dell'opera.
/// </summary>
public static class RasterCanvasBudgetPolicy
{
    public const int LegacyMaximumDimension = 4096;
    public const long LegacyMaximumPixels = 16_777_216L;
    public const int EightKMaximumDimension = 8192;
    public const long EightKMaximumPixels = 7680L * 4320L;

    /// <summary>
    /// Supporto sperimentale 8K UHD standard per le primitive geometriche.
    /// Non includere i renderer che allocano PixelBuffer a pieno formato,
    /// layer filtrati o un secondo campo raster di dimensioni equivalenti.
    /// </summary>
    public static bool SupportsEightK(string artworkId) => artworkId is
        "guilloche" or "spaced-streamlines" or "voronoi-mosaic" or "poisson-stippling";

    public static void Validate(ArtworkRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        CanvasSize size = recipe.Size;
        if (size.Width <= LegacyMaximumDimension &&
            size.Height <= LegacyMaximumDimension &&
            size.PixelCount <= LegacyMaximumPixels)
            return;

        if (recipe.Animation is not null)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Canvas oltre 4K non ancora disponibile per le animazioni.");
        if (!SupportsEightK(recipe.ArtworkId))
            throw new ArgumentOutOfRangeException(nameof(recipe),
                $"Il generatore '{recipe.ArtworkId}' richiede ancora un canvas fino a " +
                "4096×4096: il supporto 8K arriverà dopo l'ottimizzazione dei buffer.");
        if (size.Width > EightKMaximumDimension || size.Height > EightKMaximumDimension ||
            size.PixelCount > EightKMaximumPixels)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Budget raster 8K UHD: massimo 8192 pixel per lato e " +
                "33.177.600 pixel complessivi (7680×4320 oppure verticale). " +
                "Il formato quadrato 8192×8192 richiederà una pipeline dedicata.");
    }

    /// <summary>Difesa anche per chi usa SkiaRasterSurface direttamente.</summary>
    internal static void ValidateEightKDimensions(CanvasSize size)
    {
        ArgumentNullException.ThrowIfNull(size);
        if (size.Width > EightKMaximumDimension || size.Height > EightKMaximumDimension ||
            size.PixelCount > EightKMaximumPixels)
            throw new ArgumentOutOfRangeException(nameof(size),
                "Budget raster 8K UHD: massimo 8192 pixel per lato e " +
                "33.177.600 pixel complessivi (7680×4320 oppure verticale). " +
                "Il formato quadrato 8192×8192 richiederà una pipeline dedicata.");
    }
}
