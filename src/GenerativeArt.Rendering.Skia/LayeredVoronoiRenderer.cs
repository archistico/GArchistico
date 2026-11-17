using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Randomness;
using static GenerativeArt.Rendering.Skia.ArtworkPaint;

namespace GenerativeArt.Rendering.Skia;

internal static class LayeredVoronoiRenderer
{
    internal static int SitesAtDepth(ArtworkRecipe r, int depth) => Math.Clamp(
        (int)Math.Round(Int(r, "sites") * Math.Pow(Number(r, "back-density"), depth / (double)(Int(r, "depth-layers") - 1))),
        16, VoronoiTessellation.MaximumSites);

    internal static void ValidateBudget(ArtworkRecipe r)
    {
        int layers = Int(r, "depth-layers");
        long geometry = 0, tiles = 0;
        for (int depth = 0; depth < layers; depth++)
        {
            int count = SitesAtDepth(r, depth);
            tiles += count; geometry += (long)count * count * (Int(r, "relaxations") + 1);
        }
        if (tiles > RenderWorkPolicy.Limit(r, 1400, 2048) || geometry > RenderWorkPolicy.Limit(r, 1_600_000, 2_500_000))
            throw new ArgumentOutOfRangeException(nameof(r), "Budget Voronoi stratificato: massimo 1.400 tessere e 1.600.000 unità geometriche. Ridurre piani, siti o rilassamenti.");
        DepthLayerPolicy.Validate(r, layers, Number(r, "depth-blur") + Number(r, "shadow-softness"));
    }

    private static Color32[] Colors(ArtworkRecipe r, string palette)
    {
        Color32 Resolve(string role) => ArtworkInk.Resolve(r, role,
            FromHex(ArtworkRolePalettePreview.SuggestHex(r.ArtworkId, palette, role)));
        return [Resolve("background-color"), Resolve("edge-color"), Resolve("accent-color"),
            Resolve("tile-color-1"), Resolve("tile-color-2"), Resolve("tile-color-3"), Resolve("tile-color-4"), Resolve("tile-color-5"),
            Resolve("shadow-color"), Resolve("depth-color")];
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        int width = r.Size.Width, height = r.Size.Height, layers = Int(r, "depth-layers");
        double unit = Math.Min(width, height) / 1000d;
        float gutter = (float)(Number(r, "gutter") * Math.Min(width, height) / 800d);
        float edgeWidth = (float)Math.Max(.3, unit * .8);
        double angle = Number(r, "light-angle") * Math.PI / 180;
        Color32[] front = Colors(r, r.Parameters["palette"]);
        string backPalette = r.Parameters["back-palette"] == "same" ? r.Parameters["palette"] : r.Parameters["back-palette"];
        Color32[] back = Colors(r, backPalette);
        surface.Clear(front[0]);
        for (int depth = layers - 1; depth >= 0; depth--)
        {
            ct.ThrowIfCancellationRequested();
            double t = depth / (double)(layers - 1);
            ulong seed = r.Seed ^ unchecked((ulong)depth * 0x9E3779B97F4A7C15UL);
            var plan = VoronoiLayerPlan.Build(seed, SitesAtDepth(r, depth), width, height,
                (float)Number(r, "jitter"), Int(r, "relaxations"),
                depth == layers - 1 ? 0 : Number(r, "missing-tiles"),
                r.Parameters["hole-pattern"] == "random" ? VoronoiHolePattern.Random : VoronoiHolePattern.Clustered,
                new Vector2((float)(depth * unit * Number(r, "parallax-x")), (float)(depth * unit * Number(r, "parallax-y"))), ct);
            Color32[] colors = depth == 0 ? front : back;
            var random = new SplitMix64Random(seed).Fork("voronoi/depth/colors/v1");
            var noise = new SeededSimplexNoise(seed);
            double distance = Number(r, "shadow-distance") * unit;
            var effects = new LayerEffects(Number(r, "depth-blur") * unit * t,
                depth == layers - 1 ? 0 : Number(r, "shadow-opacity") * (1 - .35 * t),
                -Math.Cos(angle) * distance, -Math.Sin(angle) * distance,
                Number(r, "shadow-softness") * unit, front[8]);
            surface.DrawLayer(s =>
            {
                for (int i = 0; i < plan.Cells.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var cell = plan.Cells[i];
                    Vector2 center = cell.Centroid;
                    double field = noise.Sample2D(center.X / unit * .0038 + 27, center.Y / unit * .0038 - 51);
                    double tone = Math.Clamp(.5 + .48 * field + Number(r, "tile-variation") * (random.NextDouble() - .5), 0, .999);
                    if (plan.Missing[i]) continue;
                    int index = Math.Min(3, (int)(tone * 4));
                    Color32 fill = Mix(colors[3 + index], colors[4 + index], tone * 4 - index);
                    fill = Mix(fill, new Color32(fill.A, 0, 0, 0), Number(r, "distance-dimming") * t);
                    fill = Mix(fill, front[9].WithAlpha(fill.A), Number(r, "atmosphere") * t * front[9].A / 255d);
                    var inset = new Vector2[cell.Vertices.Count];
                    for (int v = 0; v < inset.Length; v++)
                    {
                        Vector2 corner = cell.Vertices[v], inward = center - corner;
                        float length = inward.Length();
                        inset[v] = length > 0 ? corner + inward * (Math.Min(gutter, length * .25f) / length) : corner;
                    }
                    s.DrawBeveledConvexPolygon(inset, fill, Alpha(colors[1], Number(r, "edge-opacity")), edgeWidth,
                        (float)(Number(r, "bevel-width") * unit), Number(r, "tile-relief"), angle);
                    if (i % 5 == 0 && Number(r, "accent-strength") > 0)
                        s.DrawLitDisc(center.X, center.Y, (float)Math.Max(.3, unit * 1.2),
                            Alpha(colors[2], Number(r, "accent-strength") * (1 - .35 * t)), angle, .8);
                }
            }, effects, ct);
        }
    }
}
