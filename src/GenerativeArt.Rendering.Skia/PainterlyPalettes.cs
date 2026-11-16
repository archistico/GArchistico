using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;

namespace GenerativeArt.Rendering.Skia;

internal static class PainterlyPalettes
{
    private static readonly string[] BrushRoles = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3"];
    private static readonly string[] TerrainRoles = ["background-color", "layer-color-1", "layer-color-2", "layer-color-3", "layer-color-4", "edge-color", "sun-color"];
    private static readonly IReadOnlyDictionary<string, string[]> Presets = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["flow-brush|lagoon"] = ["#EAE6D5", "#164B59", "#338F96", "#ACBE9E"],
        ["flow-brush|ember"] = ["#231B2A", "#CB6558", "#ECA873", "#F4D7A7"],
        ["flow-brush|ink"] = ["#EEE9DD", "#243544", "#576F7F", "#879A9E"],
        ["strata-landscape|dusk"] = ["#D4CFD9", "#9899B6", "#656C94", "#465977", "#243C59", "#273248", "#F4D6AD"],
        ["strata-landscape|desert"] = ["#F0DFC7", "#E0B082", "#C78462", "#A95D50", "#663E48", "#58333D", "#FFF1CE"],
        ["strata-landscape|glacier"] = ["#DDEAE9", "#A6CDD0", "#6DA1B1", "#3E768F", "#254B67", "#203D57", "#FFF1C9"]
    };

    internal static string SuggestHex(string artworkId, string palette, string role)
    {
        var roles = artworkId == "flow-brush" ? BrushRoles : TerrainRoles;
        int index = Array.IndexOf(roles, role);
        if (index < 0 || !Presets.TryGetValue(artworkId + "|" + palette, out var colors))
            throw new ArgumentException("Ruolo colore o palette non previsto.", nameof(role));
        return colors[index];
    }

    internal static Color32[] Resolve(ArtworkRecipe recipe)
    {
        var roles = recipe.ArtworkId == "flow-brush" ? BrushRoles : TerrainRoles;
        return roles.Select(role => ArtworkInk.Resolve(recipe, role,
            ArtworkPaint.FromHex(SuggestHex(recipe.ArtworkId, recipe.Parameters["palette"], role)))).ToArray();
    }
}
