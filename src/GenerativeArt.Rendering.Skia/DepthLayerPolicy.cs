using GenerativeArt.Application.Recipes;

namespace GenerativeArt.Rendering.Skia;

internal static class DepthLayerPolicy
{
    // Modello conservativo di lavoro raster, non una stima della RAM del processo.
    internal static void Validate(ArtworkRecipe recipe, int passes, double maximumSigma)
    {
        if (recipe.Size.Width < 64 || recipe.Size.Height < 64)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Composizioni di profondità: minimo 64 pixel per lato.");
        int scale = RasterQualityPolicy.Scale(recipe);
        long pixels = recipe.Size.PixelCount * scale * scale;
        double sigma = maximumSigma * Math.Min(recipe.Size.Width, recipe.Size.Height) / 1000d * scale;
        if (pixels > 8_388_608 || pixels * (double)passes * (1 + sigma / 12) > 800_000_000)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Budget livelli filtrati superato: massimo 8.388.608 pixel nativi e 800 milioni di unità raster. Ridurre risoluzione, piani o sfocatura.");
    }

    internal static double Defocus(double depth, double focus, double range) =>
        Math.Pow(Math.Max(0, Math.Abs(depth - focus) - range / 2), 1.4);
}
