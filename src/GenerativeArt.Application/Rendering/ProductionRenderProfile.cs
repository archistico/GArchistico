using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Application.Rendering;

/// <summary>Preparazione esplicita di una nuova ricetta 4K; nessuna modifica ai preset incorporati.</summary>
public static class ProductionRenderProfile
{
    public const long MaximumLogicalPixels = 4096L * 2160;
    public const long MaximumNativePixels = MaximumLogicalPixels * 4;
    public static bool IsProduction(ArtworkRecipe recipe) =>
        recipe.Parameters.TryGetValue("render-profile", out string? profile) && profile == "production";

    public static ArtworkRecipe PrepareFourK(ArtworkRecipe original, ArtworkRegistry catalog, bool maximumComplexity = true)
    {
        ArgumentNullException.ThrowIfNull(original); ArgumentNullException.ThrowIfNull(catalog);
        original.ValidateAgainst(catalog);
        var d = catalog.Get(original.ArtworkId);
        var p = new Dictionary<string,string>(original.Parameters)
        { ["render-profile"] = "production", ["render-quality"] = "fine-2x" };
        if (maximumComplexity)
        {
            foreach (string id in MaximumControls(d.Id))
                p[id] = d.Parameters.Single(s => s.Id == id).Maximum
                    ?? throw new InvalidOperationException($"Limite superiore mancante: {d.Id}/{id}.");
            foreach (string id in MinimumControls(d.Id))
                p[id] = d.Parameters.Single(s => s.Id == id).Minimum
                    ?? throw new InvalidOperationException($"Limite inferiore mancante: {d.Id}/{id}.");
            // I tratti mantengono lo spessore scelto: la distanza non può esserne inferiore.
            if (d.Id == "tonal-engraving" && double.Parse(p["spacing"], System.Globalization.CultureInfo.InvariantCulture)
                < double.Parse(p["line-width"], System.Globalization.CultureInfo.InvariantCulture)) p["spacing"] = p["line-width"];
        }
        bool portrait = original.Size.Height > original.Size.Width;
        return ArtworkRecipe.Create(d, new CanvasSize(portrait ? 2160 : 3840, portrait ? 3840 : 2160),
            original.Seed, p, original.Animation);
    }

    public static IReadOnlyList<string> MaximumControls(string id) => id switch
    {
        "alien-letters" => ["columns", "rows", "subdivisions"],
        "bokeh-depth" => ["particles", "planes"],
        "cincinnus-bristles" => ["vortices", "main-strokes", "placement-attempts", "patch-attempts", "ink-dots"],
        "cotton-circles" => ["columns", "rows"],
        "crinis-bristles" => ["bands", "samples", "vortex-centers", "ink-flecks", "fiber-density", "micro-detail"],
        "domain-warp" => ["octaves", "warp-layers"],
        "eclectic-trails" => ["trails", "steps", "whirls"],
        "fascia-ribbons" => ["bands", "steps", "whirls"],
        "flow-brush" => ["strokes", "steps", "bristles"],
        "guilloche" => ["samples", "layers", "waves"],
        "lic-flow" => ["length"],
        "looping-flow" => ["lines", "steps"],
        "noise-flow" => ["lines", "steps"],
        "ocellus-eyespots" => ["eyes", "min-depth"],
        "paper-cut" => ["layers", "samples"],
        "parametric-curves" => ["layers", "samples"],
        "phyllotaxis" => ["seeds"],
        "plasma-noise" => ["octaves"],
        "poisson-stippling" => ["max-points"],
        "reaction-diffusion" => ["iterations", "inoculations"],
        "rete-foam" => ["bubbles", "layers", "density"],
        "rigor-grid" => ["columns", "rows"],
        "sdf-orbits" => ["blobs", "layers"],
        "spaced-streamlines" => ["max-lines", "max-steps"],
        "spiral-flamebrush" => ["rings", "stamps", "particles"],
        "strata-landscape" => ["layers", "samples", "octaves"],
        "tonal-engraving" => ["hatch-layers", "octaves"],
        "topographic-contours" => ["contours"],
        "voronoi-mosaic" => ["sites", "relaxations", "depth-layers", "back-density"],
        "wave-interference" => ["sources"],
        _ => []
    };

    private static IReadOnlyList<string> MinimumControls(string id) => id switch
    {
        "cincinnus-bristles" => ["separation", "bristle-pitch"],
        "crinis-bristles" => ["bristle-pitch"],
        "poisson-stippling" => ["spacing"],
        "reaction-diffusion" or "topographic-contours" => ["cell-size"],
        "spaced-streamlines" => ["separation"],
        "tonal-engraving" => ["spacing"],
        _ => []
    };
}
