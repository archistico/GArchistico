using System.Globalization;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Algorithms.Sampling;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Stippling seeded con Poisson disk: nessuna collisione tra centri, palette
/// guidata da campo Simplex, rendering diretto sulla superficie Skia.
/// </summary>
internal static class PoissonStippleRenderer
{
    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        if (recipe.Size.Width < 64 || recipe.Size.Height < 64)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Poisson: dimensione minima 64x64.");
        if (!RenderWorkPolicy.Production(recipe) && Int(recipe, "max-points") > PoissonDiskSampler.MaximumPoints)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Poisson: oltre 50.000 punti selezionare Profilo di lavorazione → production (massimo 500.000, formato 4K).");
        float r = Distance(recipe);
        PoissonDiskSampler.Validate(recipe.Size.Width, recipe.Size.Height, r,
            Int(recipe, "max-points"), production: RenderWorkPolicy.Production(recipe));
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        CanvasSize size = recipe.Size;
        var palette = recipe.Parameters["palette"] switch
        {
            "paper" => (Background: Color32.Opaque(247, 238, 218),
                Colors: new[] { Color32.Opaque(56, 72, 87), Color32.Opaque(164, 86, 67), Color32.Opaque(43, 130, 128) }),
            "night" => (Background: Color32.Opaque(13, 19, 35),
                Colors: new[] { Color32.Opaque(106, 205, 222), Color32.Opaque(238, 171, 113), Color32.Opaque(221, 221, 245) }),
            "cobalt" => (Background: Color32.Opaque(17, 43, 76),
                Colors: new[] { Color32.Opaque(245, 224, 165), Color32.Opaque(104, 172, 227), Color32.Opaque(224, 135, 142) }),
            _ => throw new ArgumentException("Palette Poisson sconosciuta.")
        };
        Color32 background = ArtworkInk.Resolve(recipe, "background-color", palette.Background);
        Color32[] colors = new Color32[palette.Colors.Length];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = ArtworkInk.Resolve(recipe, $"dot-color-{i + 1}", palette.Colors[i]);
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<System.Numerics.Vector2> points = PoissonDiskSampler.Sample(
            size.Width, size.Height, Distance(recipe), recipe.Seed, Int(recipe, "max-points"),
            cancellationToken: ct, production: RenderWorkPolicy.Production(recipe));
        var noise = new SeededSimplexNoise(recipe.Seed);
        float radius = (float)Number(recipe, "dot-size") *
            MathF.Max(.5f, MathF.Min(size.Width, size.Height) / 800f);
        float variation = (float)Number(recipe, "radius-variation");
        double frequency = Number(recipe, "pigment-frequency");
        string dotStyle = recipe.Parameters["dot-style"];
        surface.Clear(background);
        for (int i = 0; i < points.Count; i++)
        {
            if ((i & 31) == 0) ct.ThrowIfCancellationRequested();
            System.Numerics.Vector2 p = points[i];
            double field = noise.Sample2D(p.X * frequency + 13, p.Y * frequency - 27);
            int colorIndex = field < -.16 ? 0 : field > .18 ? 2 : 1;
            // La variazione zero produce tutti punti dello stesso raggio. A 0.36
            // conserva il carattere delle vecchie ricette, ma il controllo resta libero.
            float factor = 1f - variation * (7f / 9f) + variation * MathF.Abs((float)field);
            float pointRadius = MathF.Max(.025f, radius * factor);
            bool ring = dotStyle == "ring" || (dotStyle == "mixed" && (i % 5) == 0);
            surface.DrawCircle(p.X, p.Y, pointRadius, colors[colorIndex], fill: !ring,
                strokeWidth: MathF.Max(.35f, pointRadius * .24f));
        }
    }

    private static float Distance(ArtworkRecipe recipe) =>
        (float)(Math.Min(recipe.Size.Width, recipe.Size.Height) * Number(recipe, "spacing"));

    private static int Int(ArtworkRecipe recipe, string key) =>
        int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    private static double Number(ArtworkRecipe recipe, string key) =>
        double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
