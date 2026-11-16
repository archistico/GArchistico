using System.Globalization;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;

namespace GenerativeArt.Rendering.Skia;

internal static class ArtworkPaint
{
    internal static int Int(ArtworkRecipe recipe, string key) => int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    internal static double Number(ArtworkRecipe recipe, string key) => double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    internal static Color32 Alpha(Color32 color, double value) => color.WithAlpha(
        (byte)Math.Clamp((int)Math.Round(color.A * value), 0, 255));
    internal static Color32 Mix(Color32 a, Color32 b, double t)
    {
        t = Math.Clamp(t, 0, 1);
        byte Channel(byte x, byte y) => (byte)Math.Clamp((int)Math.Round(x + (y - x) * t), 0, 255);
        return new Color32(Channel(a.A, b.A), Channel(a.R, b.R), Channel(a.G, b.G), Channel(a.B, b.B));
    }
    internal static Color32 FromHex(string hex)
    {
        uint value = uint.Parse(hex.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        return Color32.FromArgb(hex.Length == 7 ? value | 0xFF000000 : value);
    }
    internal static Color32[] LegacyColors(ArtworkRecipe recipe, params string[] roles) => roles.Select(role =>
        ArtworkInk.Resolve(recipe, role, FromHex(ArtworkRolePalettePreview.SuggestHex(recipe, role)))).ToArray();
}
