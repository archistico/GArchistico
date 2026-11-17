using GenerativeArt.Application.Recipes;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Campioni dei ruoli cromatici per l'editor quando il valore è Auto.
/// Gli sfondi graduati mostrano un campione centrale; Auto continua ad usare
/// l'algoritmo originale. Nessuna modifica ai renderer o alle ricette salvate.
/// </summary>
public static class ArtworkRolePalettePreview
{
    private static readonly IReadOnlyDictionary<string, string[]> Roles = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["paper-cut"] = ["background-color", "depth-color-1", "depth-color-2", "depth-color-3", "highlight-color", "shadow-color"],
        ["bokeh-depth"] = ["background-color", "depth-color-1", "depth-color-2", "depth-color-3", "highlight-color", "shadow-color"],
        ["guilloche"] = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3"],
        ["tonal-engraving"] = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3"],
        ["domain-warp"] = ["gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4"],
        ["lic-flow"] = ["gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4"],
        ["wave-interference"] = ["gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4"],
        ["reaction-diffusion"] = ["gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4"],
        ["phyllotaxis"] = ["background-color", "seed-color-1", "seed-color-2", "seed-color-3"],
        ["alien-letters"] = ["background-color", "tile-color-1", "tile-color-2", "tile-color-3", "tile-color-4"],
        ["cotton-circles"] = ["background-color", "tile-color-1", "tile-color-2", "tile-color-3", "tile-color-4"],
        ["rigor-grid"] = ["background-color", "ink-color", "accent-color"],
        ["crinis-bristles"] = ["background-color", "fiber-color-1", "fiber-color-2", "fiber-color-3", "shadow-color"],
        ["spiral-flamebrush"] = ["background-color", "particle-color-1", "particle-color-2", "particle-color-3"],
        ["fascia-ribbons"] = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3", "ink-color-4"],
        ["eclectic-trails"] = ["background-color", "trail-color-1", "trail-color-2", "trail-color-3", "trail-color-4"],
        ["ocellus-eyespots"] = ["background-color", "ring-color-1", "ring-color-2", "ring-color-3", "ring-color-4"],
        ["rete-foam"] = ["background-color", "wall-color", "film-color"],
        ["parametric-curves"] = ["background-color", "curve-color", "orbit-color"],
        ["noise-flow"] = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3"],
        ["spaced-streamlines"] = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3", "ink-color-4"],
        ["looping-flow"] = ["background-color", "ink-color-1", "ink-color-2", "ink-color-3", "ink-color-4"],
        ["plasma-noise"] = ["gradient-color-1", "gradient-color-2", "gradient-color-3", "gradient-color-4"],
        ["sdf-orbits"] = ["background-color", "ink-color", "fill-color", "accent-color", "pigment-color-4", "shadow-color"],
        ["poisson-stippling"] = ["background-color", "dot-color-1", "dot-color-2", "dot-color-3"],
        ["voronoi-mosaic"] = ["background-color", "edge-color", "accent-color", "tile-color-1", "tile-color-2", "tile-color-3", "tile-color-4", "tile-color-5", "shadow-color", "depth-color"],
        ["topographic-contours"] = ["line-color", "major-line-color", "band-color-1", "band-color-2", "band-color-3", "band-color-4", "band-color-5", "band-color-6"]
    };

    // Gli array sono nell'ordine di Roles. Alpha è sempre anteposto in AARRGGBB.
    private static readonly IReadOnlyDictionary<string, string[]> Presets = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["paper-cut|lagoon"] = ["#061C30", "#174B62", "#3F9B9F", "#DDE5C8", "#F8F0CB", "#071528"],
        ["paper-cut|canyon"] = ["#27142B", "#7A354A", "#CE805B", "#F2CF91", "#FFF0C5", "#26132C"],
        ["paper-cut|ivory"] = ["#424953", "#7F8C97", "#C4CAC7", "#F2EBDD", "#FFFFFF", "#26303C"],
        ["bokeh-depth|aurora"] = ["#081421", "#185A79", "#49ACAD", "#DDAD85", "#EAF9E5", "#040A18"],
        ["bokeh-depth|amber"] = ["#1C1222", "#6D3660", "#C66B56", "#EFC080", "#FFF1C9", "#130A1D"],
        ["bokeh-depth|pearl"] = ["#18243A", "#536B9F", "#9FADCB", "#E8DAD8", "#FFFFFF", "#0C1425"],
        ["guilloche|banknote"] = ["#F4F0DC", "#164F55", "#587B68", "#AD8050"],
        ["guilloche|copper"] = ["#F2DFCA", "#522C31", "#9C5141", "#CB8D52"],
        ["guilloche|midnight"] = ["#0B152B", "#63C8CA", "#A5BCE3", "#E6C28A"],
        ["tonal-engraving|banknote"] = ["#F4F0DC", "#164F55", "#587B68", "#AD8050"],
        ["tonal-engraving|copper"] = ["#F2DFCA", "#522C31", "#9C5141", "#CB8D52"],
        ["tonal-engraving|midnight"] = ["#0B152B", "#63C8CA", "#A5BCE3", "#E6C28A"],
        ["domain-warp|ocean"] = ["#081C32", "#15576C", "#54B5AB", "#F3E4B4"],
        ["domain-warp|ember"] = ["#241327", "#88384B", "#DE8654", "#FFE7AE"],
        ["domain-warp|graphite"] = ["#171E26", "#4B5A66", "#A4B2B9", "#F3EDE0"],
        ["lic-flow|ocean"] = ["#081C32", "#15576C", "#54B5AB", "#F3E4B4"],
        ["lic-flow|ember"] = ["#241327", "#88384B", "#DE8654", "#FFE7AE"],
        ["lic-flow|graphite"] = ["#171E26", "#4B5A66", "#A4B2B9", "#F3EDE0"],
        ["wave-interference|lagoon"] = ["#061B2A", "#146C79", "#69C8BE", "#F7E8C6"],
        ["wave-interference|copper"] = ["#150D24", "#81455F", "#DD9567", "#FFE8B6"],
        ["wave-interference|nocturne"] = ["#050B26", "#304C94", "#7CC5DF", "#F0F4D7"],
        ["reaction-diffusion|tidal"] = ["#081A35", "#29618C", "#68C9C5", "#F1F1C9"],
        ["reaction-diffusion|copper"] = ["#1A1329", "#7C384E", "#E2A073", "#FFF0BE"],
        ["reaction-diffusion|lichen"] = ["#121F25", "#345D4C", "#8BB26D", "#F5E8BD"],
        ["phyllotaxis|botanic"] = ["#16271D", "#4E6C42", "#9CB361", "#EFC967"],
        ["phyllotaxis|petals"] = ["#21152D", "#AD496D", "#EB94A9", "#F9D9A8"],
        ["phyllotaxis|luminous"] = ["#061728", "#377EC2", "#59D3CA", "#F4E7A3"],
        ["alien-letters|retro"] = ["#2D2926", "#EAB46A", "#88CFA9", "#DF83B2", "#90A9E8"],
        ["alien-letters|neon"] = ["#080D1F", "#FF4B92", "#4FE4C4", "#FFD86E", "#9796FA"],
        ["alien-letters|mineral"] = ["#262C35", "#5B9EAA", "#DBB988", "#D67569", "#B6C9BF"],
        ["cotton-circles|cotton"] = ["#17263A", "#3B5671", "#83A6AF", "#D9ACB9", "#F0C879"],
        ["cotton-circles|terra"] = ["#F0E1C8", "#B75039", "#DF9B63", "#BFC49E", "#665967"],
        ["cotton-circles|lagoon"] = ["#0D3041", "#164C69", "#2B858D", "#9DCAB3", "#E2C993"],
        ["rigor-grid|paper"] = ["#DBD1BF", "#242526", "#A8402C"],
        ["rigor-grid|night"] = ["#111827", "#EDF2E4", "#EB8065"],
        ["rigor-grid|porcelain"] = ["#E6E9E4", "#2D5661", "#9A6D8E"],
        ["crinis-bristles|indigo"] = ["#06132C", "#25456C", "#769CBE", "#E1EFF7", "#020A1B"],
        ["crinis-bristles|copper"] = ["#2C1521", "#733544", "#CE8166", "#F6D4A7", "#190D1C"],
        ["crinis-bristles|forest"] = ["#101E20", "#345A56", "#91BAA2", "#E0E8C2", "#081518"],
        ["crinis-bristles|crimson"] = ["#1A0308", "#4A0918", "#D32C50", "#F9C9B6", "#110207"],
        ["spiral-flamebrush|cool"] = ["#050914", "#28599D", "#73C9E9", "#D4ECF8"],
        ["spiral-flamebrush|ember"] = ["#130B1D", "#B33459", "#FF9054", "#FFDDA0"],
        ["spiral-flamebrush|orchid"] = ["#16132D", "#643E9E", "#E78CB9", "#91DDE1"],
        ["fascia-ribbons|ink"] = ["#F7EADB", "#171516", "#383953", "#9E5F55", "#31556C"],
        ["fascia-ribbons|copper"] = ["#F3E2CA", "#251B22", "#95553F", "#C3824F", "#385768"],
        ["fascia-ribbons|lagoon"] = ["#DBECE7", "#132F40", "#36817C", "#86AF9A", "#B06C66"],
        ["eclectic-trails|chalk"] = ["#161E29", "#E0DDD2", "#B8CDCF", "#E0A79C", "#8AA8B2"],
        ["eclectic-trails|neon"] = ["#0D1426", "#5FE4DE", "#F3A8DD", "#FFD074", "#B1A4FF"],
        ["eclectic-trails|earth"] = ["#F1E7D8", "#5A3534", "#9F6B50", "#6E8272", "#CC9D64"],
        ["ocellus-eyespots|lagoon"] = ["#F5EBC8", "#183E50", "#317E91", "#74B7B2", "#E6D9B1"],
        ["ocellus-eyespots|sunset"] = ["#F5EBC8", "#673C65", "#C86860", "#F2A65E", "#FCE0A5"],
        ["ocellus-eyespots|botanic"] = ["#F5EBC8", "#2D5347", "#5A947A", "#A8BA88", "#E6D9A1"],
        ["ocellus-eyespots|graphite"] = ["#F5EBC8", "#303847", "#636D7C", "#AFB5B2", "#F1EBDD"],
        ["ocellus-eyespots|coral"] = ["#F5EBC8", "#623B59", "#D05D71", "#F39B89", "#FFD1A4"],
        ["rete-foam|paper"] = ["#F5F3EB", "#17191E", "#FDFBF4"],
        ["rete-foam|night"] = ["#121A28", "#080E17", "#9BD7D1"],
        ["rete-foam|sepia"] = ["#F4E9D5", "#352623", "#F5C38A"],
        ["rete-foam|ice"] = ["#DDEAF1", "#34465C", "#F7FFFF"],
        ["parametric-curves|default"] = ["#202C39", "#AFFFDC88", "#70CAE7"],
        ["noise-flow|default"] = ["#141825", "#9E6ED9F0", "#91F9AB75", "#9EFC80A5"],
        ["spaced-streamlines|default"] = ["#0F1324", "#AF5DDEDD", "#AAF3BC77", "#A0F088AD", "#B48BACFF"],
        ["looping-flow|aurora"] = ["#0B1121", "#AF48D7E9", "#A098CFFF", "#9BF1B59A", "#9BFF9BBF"],
        ["looping-flow|ember"] = ["#0B1121", "#B4FF8048", "#A8FBBC60", "#9AEC647D", "#A0FCD992"],
        ["looping-flow|mist"] = ["#0B1121", "#A5B8D8F0", "#91A8C5D7", "#9BD8E0E7", "#AF8FAED3"],
        ["plasma-noise|deep"] = ["#081132", "#244580", "#39B1B6", "#FCE39F"],
        ["plasma-noise|ember"] = ["#190A1C", "#802140", "#F26437", "#FFE28C"],
        ["plasma-noise|moss"] = ["#08221D", "#1C5B45", "#7CAD69", "#EDE3A0"],
        ["sdf-orbits|mineral"] = ["#0A1624", "#9FE8E4", "#225C6C", "#F2DBA0", "#4F8EB1", "#030F1C"],
        ["sdf-orbits|ember"] = ["#1E0D1E", "#FFC48D", "#843A49", "#F9E1BE", "#CB615D", "#150A20"],
        ["sdf-orbits|iris"] = ["#161233", "#D7B1FB", "#4C3797", "#96F0E6", "#E295BA", "#100F28"],
        ["sdf-orbits|sorbet"] = ["#F3EDE2", "#F2B05D", "#E96E83", "#43B6AD", "#7B79BD", "#29314D"],
        ["sdf-orbits|lagoon"] = ["#E5E7D4", "#32B7AE", "#154F70", "#D4C47C", "#7B93BD", "#102E40"],
        ["sdf-orbits|nocturne"] = ["#0B122A", "#7B7CE2", "#206E98", "#E67FAD", "#6AD8C2", "#03091C"],
        ["poisson-stippling|paper"] = ["#F7EEDA", "#384857", "#A45643", "#2B8280"],
        ["poisson-stippling|night"] = ["#0D1323", "#6ACDDE", "#EEAB71", "#DDDDF5"],
        ["poisson-stippling|cobalt"] = ["#112B4C", "#F5E0A5", "#68ACE3", "#E0878E"],
        ["voronoi-mosaic|glass"] = ["#081423", "#122131", "#F4E3B6", "#1D677F", "#43969E", "#85C0AB", "#DDAB74", "#BB6567", "#030A15", "#536879"],
        ["voronoi-mosaic|copper"] = ["#26181A", "#422527", "#FFECC5", "#6C373C", "#A75848", "#CE8653", "#EEB570", "#ECD5A0", "#180B10", "#8D6A58"],
        ["voronoi-mosaic|lagoon"] = ["#0C1A26", "#15303A", "#E9F2C6", "#174764", "#186D80", "#3A9893", "#84C2A3", "#CBDEB1", "#03121E", "#486B6B"],
        ["topographic-contours|atlas"] = ["#B9E7EFD5", "#FDF6DC", "#0F2A3B", "#214F5B", "#4F8072", "#90AB84", "#CCC899", "#F1E1B6"],
        ["topographic-contours|nocturne"] = ["#B284DBE1", "#E9F9F8", "#0A1123", "#152442", "#283E64", "#3F5D80", "#5D8B99", "#91BFB8"],
        ["topographic-contours|terra"] = ["#BFFFD7A8", "#FFEFCC", "#371B24", "#672F38", "#944940", "#BC7150", "#DCA26F", "#FAD9A1"]
    };

    public static string SuggestHex(ArtworkRecipe recipe, string role)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        return SuggestHex(recipe.ArtworkId,
            recipe.Parameters.TryGetValue("palette", out string? palette) ? palette : "default", role);
    }

    public static string SuggestHex(string artworkId, string palette, string role)
    {
        ArgumentException.ThrowIfNullOrEmpty(artworkId);
        ArgumentException.ThrowIfNullOrEmpty(palette);
        ArgumentException.ThrowIfNullOrEmpty(role);
        if (artworkId is "flow-brush" or "strata-landscape")
            return PainterlyPalettes.SuggestHex(artworkId, palette, role);
        if (artworkId == "cincinnus-bristles")
            return CincinnusRenderer.SuggestHex(palette, role);
        if (!Roles.TryGetValue(artworkId, out string[]? roles))
            throw new ArgumentException("Artwork non presente nella mappa dei campioni.", nameof(artworkId));
        int index = Array.IndexOf(roles, role);
        if (index < 0) throw new ArgumentException("Ruolo colore non definito.", nameof(role));
        if (!Presets.TryGetValue($"{artworkId}|{palette}", out string[]? colors))
            throw new ArgumentException("Palette non prevista dalla mappa dei campioni.", nameof(palette));
        return colors[index];
    }
}
