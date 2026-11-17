using GenerativeArt.Algorithms.Pattern;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;

namespace GenerativeArt.Rendering.Skia;

/// <summary>Fillotassi a dischi, anelli o pennellate radiali; primitive Skia AA.</summary>
internal static class PhyllotaxisRenderer
{
    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        int count = ArtworkPaint.Int(recipe, "seeds");
        double spread = ArtworkPaint.Number(recipe, "spread");
        double size = ArtworkPaint.Number(recipe, "point-size");
        double sizeVariation = ArtworkPaint.Number(recipe, "size-variation");
        double opacity = ArtworkPaint.Number(recipe, "opacity");
        double minSide = Math.Min(recipe.Size.Width, recipe.Size.Height);
        double originX = ArtworkPaint.Number(recipe, "center-x") * recipe.Size.Width;
        double originY = ArtworkPaint.Number(recipe, "center-y") * recipe.Size.Height;
        string[] roles = ["seed-color-1", "seed-color-2", "seed-color-3"];
        uint[] palette = recipe.Parameters["palette"] switch
        {
            "petals" => [0xFFAD496D, 0xFFEB94A9, 0xFFF9D9A8],
            "luminous" => [0xFF377EC2, 0xFF59D3CA, 0xFFF4E7A3],
            _ => [0xFF4E6C42, 0xFF9CB361, 0xFFEFC967]
        };
        var colors = PatternPalette.Resolve(recipe, roles, palette);
        uint defaultBackground = recipe.Parameters["palette"] switch
        {
            "petals" => 0xFF21152D,
            "luminous" => 0xFF061728,
            _ => 0xFF16271D
        };
        surface.Clear(ArtworkInk.Resolve(recipe, "background-color", Color32.FromArgb(defaultBackground)));
        string style = recipe.Parameters["point-style"];
        int current = 0;
        foreach (var point in PhyllotaxisPattern.Generate(recipe.Seed, count,
            ArtworkPaint.Number(recipe, "angle"), ArtworkPaint.Number(recipe, "jitter"),
            ArtworkPaint.Int(recipe, "arms"), recipe.Parameters["shape"]))
        {
            if ((current++ & 255) == 0) ct.ThrowIfCancellationRequested();
            float x = (float)(originX + point.X * minSide * .46 * spread);
            float y = (float)(originY + point.Y * minSide * .46 * spread);
            float radius = (float)Math.Max(.08, size * minSide / 1000 *
                (1 + sizeVariation * (1 - 2 * point.Progress)));
            double gradient = Math.Clamp(.14 + .86 * point.Progress +
                .13 * Math.Sin(point.Radius * Math.Tau * 3), 0, 1);
            Color32 ink = ArtworkPaint.Alpha(PatternPalette.Gradient(colors, gradient), opacity);
            switch (style)
            {
                case "ring":
                    surface.DrawCircle(x, y, radius, ink, strokeWidth: Math.Max(.12f, radius * .28f));
                    break;
                case "petal":
                    double angle = Math.Atan2(point.Y, point.X);
                    float dx = (float)Math.Cos(angle) * radius * 2.2f;
                    float dy = (float)Math.Sin(angle) * radius * 2.2f;
                    surface.DrawLine(x - dx * .3f, y - dy * .3f, x + dx, y + dy, ink,
                        Math.Max(.2f, radius * .8f));
                    break;
                default:
                    surface.DrawCircle(x, y, radius, ink, fill: true);
                    break;
            }
        }
    }
}
