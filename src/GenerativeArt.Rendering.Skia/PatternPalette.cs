using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;

namespace GenerativeArt.Rendering.Skia;

/// <summary>Palette e interpolazione ARGB comuni alle nuove famiglie di pattern.</summary>
internal static class PatternPalette
{
    internal static Color32[] Resolve(ArtworkRecipe recipe, string[] roles, uint[] defaults)
    {
        if (roles.Length != defaults.Length) throw new ArgumentException("Ruoli e campioni devono corrispondere.");
        var colors = new Color32[roles.Length];
        for (int i = 0; i < roles.Length; i++)
            colors[i] = ArtworkInk.Resolve(recipe, roles[i], Color32.FromArgb(defaults[i]));
        return colors;
    }

    internal static Color32 Gradient(Color32[] colors, double t)
    {
        double position = Math.Clamp(t, 0, 1) * (colors.Length - 1);
        int index = Math.Min(colors.Length - 2, (int)position);
        return ArtworkPaint.Mix(colors[index], colors[index + 1], position - index);
    }
}
