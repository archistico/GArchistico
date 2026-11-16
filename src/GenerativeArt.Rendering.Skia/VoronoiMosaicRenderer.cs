using System.Globalization;
using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Mosaico a celle Voronoi limitate con siti jittered, rilassamento di Lloyd,
/// palette seedate, fughe tra le tessere e contorni antialias. Nessun buffer
/// raster managed aggiuntivo: i poligoni vengono disegnati direttamente su Skia.
/// </summary>
internal static class VoronoiMosaicRenderer
{
    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        if (recipe.Size.Width < 64 || recipe.Size.Height < 64)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Voronoi: minimo 64×64 pixel.");
        // Il catalogo limita i siti a 196 e i rilassamenti a 3.
        // Il limite ridondante protegge il renderer anche da futuri cambi catalogo.
        int sites = Int(recipe, "sites");
        int relaxations = Int(recipe, "relaxations");
        if (sites is < 1 or > 196 || relaxations is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Budget Voronoi superato.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        CanvasSize size = recipe.Size;
        int siteCount = Int(recipe, "sites");
        float jitter = (float)Number(recipe, "jitter");
        float gutter = (float)Number(recipe, "gutter") * MathF.Min(size.Width, size.Height) / 800f;
        float borderWidth = MathF.Max(.4f, MathF.Min(size.Width, size.Height) / 800f);
        var palette = recipe.Parameters["palette"] switch
        {
            "glass" => (Background: Color32.Opaque(8, 20, 35), Edge: Color32.Opaque(18, 33, 49),
                Colors: new[] { Color32.Opaque(29, 103, 127), Color32.Opaque(67, 150, 158),
                    Color32.Opaque(133, 192, 171), Color32.Opaque(221, 171, 116),
                    Color32.Opaque(187, 101, 103) }, Accent: Color32.Opaque(244, 227, 182)),
            "copper" => (Background: Color32.Opaque(38, 24, 26), Edge: Color32.Opaque(66, 37, 39),
                Colors: new[] { Color32.Opaque(108, 55, 60), Color32.Opaque(167, 88, 72),
                    Color32.Opaque(206, 134, 83), Color32.Opaque(238, 181, 112),
                    Color32.Opaque(236, 213, 160) }, Accent: Color32.Opaque(255, 236, 197)),
            "lagoon" => (Background: Color32.Opaque(12, 26, 38), Edge: Color32.Opaque(21, 48, 58),
                Colors: new[] { Color32.Opaque(23, 71, 100), Color32.Opaque(24, 109, 128),
                    Color32.Opaque(58, 152, 147), Color32.Opaque(132, 194, 163),
                    Color32.Opaque(203, 222, 177) }, Accent: Color32.Opaque(233, 242, 198)),
            _ => throw new ArgumentException("Palette Voronoi sconosciuta.")
        };
        Color32 background = ArtworkInk.Resolve(recipe, "background-color", palette.Background);
        Color32 edgeColor = ArtworkInk.Resolve(recipe, "edge-color", palette.Edge);
        Color32 accentColor = ArtworkInk.Resolve(recipe, "accent-color", palette.Accent);
        Color32[] tileColors = new Color32[palette.Colors.Length];
        for (int i = 0; i < tileColors.Length; i++)
            tileColors[i] = ArtworkInk.Resolve(recipe, $"tile-color-{i + 1}", palette.Colors[i]);

        // Seminazione a griglia jittered: distribuzione uniforme con variazione
        // seeded. Anche su canvas molto allungati il numero di siti è esatto.
        var rng = new SplitMix64Random(recipe.Seed);
        int columns = Math.Max(1, Math.Min(siteCount,
            (int)Math.Round(Math.Sqrt(siteCount * (double)size.Width / size.Height))));
        int rows = (int)Math.Ceiling(siteCount / (double)columns);
        var sites = new Vector2[siteCount];
        for (int i = 0; i < siteCount; i++)
        {
            if ((i & 31) == 0) ct.ThrowIfCancellationRequested();
            int x = i % columns, y = i / columns;
            float fx = (float)((x + .5 + (rng.NextDouble() - .5) * jitter) * size.Width / columns);
            float fy = (float)((y + .5 + (rng.NextDouble() - .5) * jitter) * size.Height / rows);
            sites[i] = new Vector2(Math.Clamp(fx, 0, MathF.BitDecrement(size.Width)),
                Math.Clamp(fy, 0, MathF.BitDecrement(size.Height)));
        }

        IReadOnlyList<Vector2> relaxed = VoronoiTessellation.Relax(sites, size.Width, size.Height,
            Int(recipe, "relaxations"), ct);
        IReadOnlyList<VoronoiCell> cells = VoronoiTessellation.Build(relaxed, size.Width, size.Height, ct);
        var noise = new SeededSimplexNoise(recipe.Seed);
        surface.Clear(background);
        for (int i = 0; i < cells.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            VoronoiCell cell = cells[i];
            Vector2 center = cell.Centroid;
            var inset = new Vector2[cell.Vertices.Count];
            for (int v = 0; v < inset.Length; v++)
            {
                Vector2 corner = cell.Vertices[v];
                Vector2 inward = center - corner;
                float distance = inward.Length();
                inset[v] = distance > 0
                    ? corner + inward * (MathF.Min(gutter, distance * .25f) / distance)
                    : corner;
            }
            double field = noise.Sample2D(center.X * .0038 + 27.0, center.Y * .0038 - 51.0);
            // Piccola variazione seeded indipendente dalla disposizione dei poligoni.
            double tone = Math.Clamp(.5 + .48 * field + .13 * (rng.NextDouble() - .5), 0, .999);
            int index = Math.Min(tileColors.Length - 1, (int)(tone * tileColors.Length));
            surface.DrawPolygon(inset, tileColors[index], edgeColor, borderWidth);
            if ((i % 5) == 0)
                surface.DrawCircle(center.X, center.Y, MathF.Max(.5f, borderWidth * 1.35f),
                    accentColor, fill: true);
        }
    }

    private static int Int(ArtworkRecipe recipe, string key) =>
        int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);

    private static double Number(ArtworkRecipe recipe, string key) =>
        double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
