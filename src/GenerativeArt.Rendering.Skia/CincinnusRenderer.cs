using System.Globalization;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using SkiaSharp;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// C5 sperimentale: collega gli algoritmi C1-C4 e il pittore C3 al contratto
/// ricetta/Studio/CLI. Tutte le misure artistiche sono scalate dal riferimento
/// 2000px al lato minore del canvas, come nello strumento preview C4.
/// </summary>
internal static class CincinnusRenderer
{
    internal const int MinimumSide = 256;
    internal const int MaximumSide = 2048;
    internal const long MaximumPixels = 2_097_152;

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        int width = recipe.Size.Width, height = recipe.Size.Height;
        if (width is < MinimumSide or > MaximumSide ||
            height is < MinimumSide or > MaximumSide || recipe.Size.PixelCount > MaximumPixels)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Cincinnus sperimentale: ogni lato deve essere 256..2048 px e il canvas non deve superare 2.097.152 pixel.");
        if (recipe.Parameters.TryGetValue("render-quality", out string? quality) && quality == "fine-2x" &&
            recipe.Size.PixelCount > 524_288)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Cincinnus fine-2x: massimo 524.288 pixel logici, in attesa dei benchmark memoria.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        ValidateBudget(recipe);
        ct.ThrowIfCancellationRequested();
        int width = recipe.Size.Width, height = recipe.Size.Height;
        float scale = Math.Min(width, height) / 2000f;
        long seed = unchecked((long)recipe.Seed);
        var field = CincinnusFlowField.Create(width, height, seed, Integer(recipe, "vortices"),
            cancellationToken: ct);
        IReadOnlyList<CincinnusBrushStroke> strokes;
        bool incomplete;
        if (recipe.Parameters["mode"] == "adaptive")
        {
            var placement = new CincinnusAdaptiveOptions
            {
                MainStrokeCount = Integer(recipe, "main-strokes"),
                MaximumPlacementAttempts = Integer(recipe, "placement-attempts"),
                MaximumPatchAttempts = Integer(recipe, "patch-attempts"),
                CoverageCell = Number(recipe, "coverage-cell") * scale,
                StepSize = Math.Max(.75f, 3f * scale),
                MaximumTraceLength = 9000f * scale,
                Margin = 500f * scale,
                MinimumRadius = 25f * scale,
                MinimumAverageHalfWidth = 30f * scale,
                MaximumHalfWidth = 120f * scale,
                HalfWidthFactor = Number(recipe, "adaptive-width"),
                MaximumStrokeLength = 1600f * scale,
                MaximumSpinePoints = 350_000,
                MaximumCoverageCells = 150_000,
                MaximumTotalStrokes = 1_400
            };
            CincinnusAdaptiveLayout layout = CincinnusAdaptivePlacer.Place(field, width, height, seed, placement, ct);
            strokes = layout.StrokesInPaintOrder;
            incomplete = layout.SafetyLimitReached;
        }
        else
        {
            float separation = Number(recipe, "separation") * scale;
            var placement = new CincinnusSimpleOptions
            {
                Separation = separation,
                StepSize = Math.Clamp(3f * scale, 1.25f, 3f),
                MinimumLength = Math.Max(30f, 300f * scale),
                MaximumLength = Math.Max(800f, 9000f * scale),
                Margin = 3.75f * separation,
                MaximumLines = 2048,
                MaximumPoints = 250_000,
                MaximumPendingSeeds = 500_000,
                MaximumCoverCells = 150_000
            };
            CincinnusSimpleLayout lines = CincinnusSimplePlacer.Place(field, width, height, seed, placement, ct);
            var options = new CincinnusStrokeOptions
            {
                HalfWidthFactor = Number(recipe, "half-width-factor"),
                FirstMinimumLength = Math.Max(35f, 200f * scale),
                SubsequentMinimumLength = Math.Max(60f, 600f * scale),
                MaximumStrokeLength = Math.Max(125f, 1600f * scale),
                MinimumOverlap = Math.Max(15f, 150f * scale),
                MaximumOverlap = Math.Max(30f, 350f * scale)
            };
            CincinnusStrokePlan plan = CincinnusStrokePlanner.Build(lines, seed, options, ct);
            strokes = plan.Strokes;
            incomplete = lines.SafetyLimitReached || plan.SafetyLimitReached;
        }
        // Il renderer pubblico non produce silenziosamente opere tronche.
        if (incomplete && recipe.Parameters["require-complete"] == "true") throw new InvalidOperationException(
            "Cincinnus ha raggiunto un limite di sicurezza: ridurre densità, dimensioni o tentativi.");
        if (strokes.Count == 0) throw new InvalidOperationException(
            "Cincinnus non ha prodotto pennellate: modificare seed o parametri geometrici.");
        var ramp = Palette(recipe.Parameters["palette"]);
        for (int i = 0; i < ramp.Length; i++)
            ramp[i] = Color(recipe, $"pigment-{i + 1}", ramp[i]);
        SKColor? background = ArtworkInk.IsAuto(recipe, "background-color")
            ? null : Color(recipe, "background-color", ramp[0]);
        var paint = new CincinnusPaintOptions
        {
            Palette = Array.AsReadOnly(ramp),
            BackgroundColor = background,
            BristlePitch = Math.Max(.8f, Number(recipe, "bristle-pitch") * scale),
            HairWidthRatio = Number(recipe, "hair-width-ratio"),
            BodyShade = Number(recipe, "body-shade"),
            WideShadowBlur = Number(recipe, "wide-shadow") * scale,
            TightShadowBlur = Number(recipe, "tight-shadow") * scale,
            WideShadowOpacity = Number(recipe, "wide-opacity"),
            TightShadowOpacity = Number(recipe, "tight-opacity"),
            DepthTone = Number(recipe, "depth-tone"),
            TopToBottomTone = Number(recipe, "vertical-tone"),
            DotCount = Math.Clamp((int)Math.Round(Integer(recipe, "ink-dots") *
                (double)width * height / 4_000_000d), 0, 20_000)
        };
        surface.PaintCincinnus(strokes, seed, paint, ct);
    }

    internal static string SuggestHex(string palette, string role)
    {
        SKColor[] ramp = Palette(palette);
        SKColor color;
        if (role == "background-color") color = CincinnusBristlePainter.Sample(ramp, .02f);
        else
        {
            int index = role.Length == 9 && role.StartsWith("pigment-", StringComparison.Ordinal)
                && role[8] is >= '1' and <= '8' ? role[8] - '1' : -1;
            if (index < 0) throw new ArgumentException("Ruolo colore Cincinnus non definito.", nameof(role));
            color = ramp[index];
        }
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }

    private static SKColor[] Palette(string id) => id switch
    {
        "crimson" =>
        [new(0x1A, 0x03, 0x08), new(0x3D, 0x07, 0x13), new(0x6E, 0x0F, 0x22),
         new(0xA3, 0x14, 0x2F), new(0xCB, 0x1B, 0x45), new(0xE0, 0x48, 0x5A),
         new(0xF0, 0x8A, 0x80), new(0xF9, 0xC9, 0xB6)],
        "ocean" =>
        [new(0x07, 0x14, 0x20), new(0x0D, 0x2B, 0x3A), new(0x15, 0x48, 0x5D),
         new(0x20, 0x71, 0x80), new(0x3A, 0x93, 0xA0), new(0x6E, 0xB7, 0xB8),
         new(0xAF, 0xD6, 0xCC), new(0xEB, 0xF2, 0xDC)],
        "moss" =>
        [new(0x11, 0x1D, 0x14), new(0x23, 0x32, 0x23), new(0x3D, 0x52, 0x32),
         new(0x5E, 0x72, 0x41), new(0x83, 0x91, 0x54), new(0xA8, 0xB1, 0x78),
         new(0xCE, 0xCA, 0x9C), new(0xF0, 0xE9, 0xC9)],
        _ => throw new ArgumentException("Palette Cincinnus non supportata.", nameof(id))
    };

    private static SKColor Color(ArtworkRecipe recipe, string key, SKColor fallback)
    {
        Color32 value = ArtworkInk.Resolve(recipe, key,
            new Color32(fallback.Alpha, fallback.Red, fallback.Green, fallback.Blue));
        return new SKColor(value.R, value.G, value.B, value.A);
    }

    private static int Integer(ArtworkRecipe recipe, string key) =>
        int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    private static float Number(ArtworkRecipe recipe, string key) =>
        float.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
