using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using static GenerativeArt.Rendering.Skia.ArtworkPaint;

namespace GenerativeArt.Rendering.Skia;

internal static class DepthArtworkRenderer
{
    internal static void Validate(ArtworkRecipe recipe)
    {
        bool paper = recipe.ArtworkId == "paper-cut";
        if (paper && Number(recipe, "inner-radius") >= Number(recipe, "outer-radius"))
            throw new ArgumentException("Il raggio interno deve essere minore del raggio esterno.", nameof(recipe));
        int planes = Int(recipe, paper ? "layers" : "planes");
        if (paper && (long)planes * (Int(recipe, "samples") + 1) > 150_000)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Budget contorni: massimo 150.000 vertici.");
        bool glow = !paper && Number(recipe, "glow") > 0;
        double sigma = Number(recipe, "depth-blur") + Number(recipe, "shadow-softness");
        if (glow) sigma += Number(recipe, "radius") * .55;
        DepthLayerPolicy.Validate(recipe, planes * (glow ? 2 : 1), sigma);
        // Limita anche la copertura in casi con migliaia di dischi molto grandi.
        if (!paper && Int(recipe, "particles") * Math.Pow(Number(recipe, "radius") * Number(recipe, "depth-scale"), 2) > 160_000_000)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Budget dischi superato: ridurre particelle, raggio o ingrandimento.");
    }

    private static Color32[] Colors(ArtworkRecipe recipe) => LegacyColors(recipe,
        "background-color", "depth-color-1", "depth-color-2", "depth-color-3", "highlight-color", "shadow-color");

    private static Color32 Ramp(Color32[] colors, double t) => t < .5
        ? Mix(colors[1], colors[2], t * 2) : Mix(colors[2], colors[3], (t - .5) * 2);

    private static LayerEffects Effects(ArtworkRecipe r, double t, double unit, Color32 shadow)
    {
        double angle = Number(r, "light-angle") * Math.PI / 180;
        double distance = Number(r, "shadow-distance") * unit;
        return new LayerEffects(Number(r, "depth-blur") * unit * DepthLayerPolicy.Defocus(t, Number(r, "focus"), Number(r, "focus-range")),
            Number(r, "shadow-opacity"), -Math.Cos(angle) * distance, -Math.Sin(angle) * distance,
            Number(r, "shadow-softness") * unit, shadow);
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        Color32[] colors = Colors(recipe);
        surface.Clear(colors[0]);
        if (recipe.ArtworkId == "paper-cut") Paper(surface, recipe, colors, ct);
        else Bokeh(surface, recipe, colors, ct);
    }

    private static void Paper(SkiaRasterSurface surface, ArtworkRecipe r, Color32[] colors, CancellationToken ct)
    {
        int layers = Int(r, "layers"), samples = Int(r, "samples");
        float width = r.Size.Width, height = r.Size.Height, minimum = Math.Min(width, height);
        double unit = minimum / 1000d;
        var contour = new OrganicAperture(r.Seed);
        double rotation = Number(r, "rotation") * Math.PI / 180;
        double twist = Number(r, "twist") * Math.PI / 180;
        double inner = Number(r, "inner-radius"), outer = Number(r, "outer-radius");
        double drift = Number(r, "drift") * minimum;
        for (int layer = 0; layer < layers; layer++)
        {
            ct.ThrowIfCancellationRequested();
            double t = (double)layer / (layers - 1), progression = Math.Pow(t, Number(r, "layer-bias"));
            float radius = (float)(minimum * (inner + (outer - inner) * progression));
            var center = new Vector2((float)(width * Number(r, "center-x") + drift * (1 - t) * Math.Cos(rotation + t * 2)),
                (float)(height * Number(r, "center-y") + drift * (1 - t) * Math.Sin(rotation + t * 2)));
            Vector2[] hole = contour.Build(center, radius, samples, Int(r, "lobes"), Number(r, "ripple"),
                Number(r, "roughness"), rotation + twist * t, Number(r, "stretch"), t, ct);
            Color32 fill = Ramp(colors, t);
            var effects = Effects(r, t, unit, colors[5]);
            surface.DrawLayer(s =>
            {
                s.DrawAperture(hole, fill);
                if (Number(r, "rim-light") > 0)
                    s.DrawPolyline(hole, 1, 1, Alpha(colors[4], Number(r, "rim-light")),
                        (float)(Number(r, "rim-width") * unit));
            }, effects, ct);
        }
    }

    private static void Bokeh(SkiaRasterSurface surface, ArtworkRecipe r, Color32[] colors, CancellationToken ct)
    {
        var layout = r.Parameters["layout"] switch
        {
            "spiral" => DepthParticleLayout.Spiral,
            "flow" => DepthParticleLayout.Flow,
            _ => DepthParticleLayout.Cloud
        };
        DepthParticle[] particles = DepthParticleField.Build(r.Seed, Int(r, "particles"), layout,
            Number(r, "spread"), Number(r, "turns"), ct);
        int planes = Int(r, "planes"), start = 0;
        double unit = Math.Min(r.Size.Width, r.Size.Height) / 1000d;
        double radius = Number(r, "radius") * unit;
        double angle = Number(r, "light-angle") * Math.PI / 180;
        for (int plane = 0; plane < planes; plane++)
        {
            ct.ThrowIfCancellationRequested();
            int end = start;
            while (end < particles.Length && particles[end].Depth <= (double)(plane + 1) / planes) end++;
            if (end == start) continue;
            double t = (plane + .5) / planes;
            LayerEffects effects = Effects(r, t, unit, colors[5]);
            void Draw(SkiaRasterSurface s, bool halo)
            {
                for (int i = start; i < end; i++)
                {
                    if ((i & 31) == 0) ct.ThrowIfCancellationRequested();
                    var p = particles[i];
                    float x = p.Position.X * r.Size.Width, y = p.Position.Y * r.Size.Height;
                    float size = (float)(radius * p.Size * (.35 + p.Depth * (Number(r, "depth-scale") - .35)));
                    Color32 color = Alpha(Ramp(colors, p.Depth), Number(r, "opacity") * p.Brightness);
                    if (halo) s.DrawCircle(x, y, size * 1.18f, Alpha(color, Number(r, "glow")), fill: true);
                    else
                    {
                        s.DrawLitDisc(x, y, size, color, angle, Number(r, "light-strength"));
                        double ring = Number(r, "aperture-ring");
                        if (ring > 0) s.DrawCircle(x, y, size * .94f,
                            Alpha(colors[4], ring * Number(r, "opacity") * p.Brightness),
                            strokeWidth: (float)Math.Max(.15, unit * (1 + p.Depth * 1.8)));
                    }
                }
            }
            if (Number(r, "glow") > 0)
                surface.DrawLayer(s => Draw(s, true), new LayerEffects(Blur: effects.Blur + radius * .55), ct);
            surface.DrawLayer(s => Draw(s, false), effects, ct);
            start = end;
        }
    }
}
