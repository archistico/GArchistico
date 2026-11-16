using System.Globalization;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;

namespace GenerativeArt.Rendering.Skia;

/// <summary>Applica il singolo colore opzionale sopra la palette, senza alterare i preset.</summary>
internal static class ArtworkInk
{
    internal static bool IsAuto(ArtworkRecipe recipe, string key) =>
        !recipe.Parameters.TryGetValue(key, out string? value) || value == "auto";

    internal static Color32 Resolve(ArtworkRecipe recipe, string key, Color32 paletteColor)
    {
        if (!recipe.Parameters.TryGetValue(key, out string? value) || value == "auto")
            return paletteColor;
        uint packed = uint.Parse(value.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        if (value.Length == 7) packed |= 0xFF000000;
        return Color32.FromArgb(packed);
    }
}
