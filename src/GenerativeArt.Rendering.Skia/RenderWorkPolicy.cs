using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;

namespace GenerativeArt.Rendering.Skia;

internal static class RenderWorkPolicy
{
    internal static bool Production(ArtworkRecipe recipe) => ProductionRenderProfile.IsProduction(recipe);
    internal static long Limit(ArtworkRecipe recipe, long balanced, long production) => Production(recipe) ? production : balanced;
    internal static void Validate(ArtworkRecipe recipe)
    {
        if (Production(recipe) && (recipe.Size.Width > 4096 || recipe.Size.Height > 4096 ||
            recipe.Size.PixelCount > ProductionRenderProfile.MaximumLogicalPixels))
            throw new ArgumentOutOfRangeException(nameof(recipe), "Produzione 4K: massimo 4096 px per lato e 8.847.360 pixel (4K UHD/DCI o verticale).");
    }
}
