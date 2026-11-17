using System.Globalization;
using GenerativeArt.Algorithms.Interference;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;

namespace GenerativeArt.Rendering.Skia;

/// <summary>Renderer a righe per interferenze, caustiche e nastri moiré.</summary>
internal static class WaveInterferenceRenderer
{
    // Stima pre-allocazione: un contributo radiale per sorgente e pixel nativo.
    internal const long MaximumWaveSamples = 150_000_000;

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        long scale = RasterQualityPolicy.Scale(recipe);
        int sources = Int(recipe, "sources");
        if (recipe.Size.PixelCount * scale * scale * sources > MaximumWaveSamples)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Interferenze: massimo 150 milioni di contributi d'onda. Ridurre sorgenti, risoluzione o campionamento 2x.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        int scale = surface.RasterScale;
        var output = recipe.Size;
        var native = new CanvasSize(checked(output.Width * scale), checked(output.Height * scale));
        var pixels = new PixelBuffer(native);
        var field = new WaveInterferenceField2D(recipe.Seed,
            Int(recipe, "sources"), recipe.Parameters["layout"], Number(recipe, "spread"),
            Number(recipe, "frequency"), Number(recipe, "distortion"), Number(recipe, "falloff"),
            recipe.Parameters["mode"]);
        Color32[] colors = Palette(recipe);
        double contrast = Number(recipe, "contrast"), gamma = Number(recipe, "gamma");
        double minSide = Math.Min(output.Width, output.Height);
        for (int y = 0; y < native.Height; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            double py = ((y + 0.5) / scale - output.Height * 0.5) / minSide;
            for (int x = 0; x < native.Width; x++)
            {
                double px = ((x + 0.5) / scale - output.Width * 0.5) / minSide;
                double value = field.Sample(px, py);
                value = Math.Clamp(0.5 + contrast * (value - 0.5), 0, 1);
                value = Math.Pow(value, gamma);
                row[x] = Gradient(colors, value).PackedArgb;
            }
        }
        ct.ThrowIfCancellationRequested();
        surface.DrawPixels(pixels, nativeResolution: true);
    }

    private static Color32[] Palette(ArtworkRecipe recipe)
    {
        uint[] colors = recipe.Parameters["palette"] switch
        {
            "copper" => [0xFF150D24, 0xFF81455F, 0xFFDD9567, 0xFFFFE8B6],
            "nocturne" => [0xFF050B26, 0xFF304C94, 0xFF7CC5DF, 0xFFF0F4D7],
            _ => [0xFF061B2A, 0xFF146C79, 0xFF69C8BE, 0xFFF7E8C6]
        };
        return colors.Select((c, i) => ArtworkInk.Resolve(recipe,
            $"gradient-color-{i + 1}", Color32.FromArgb(c))).ToArray();
    }

    private static Color32 Gradient(Color32[] palette, double t)
    {
        double pos = Math.Clamp(t, 0, 1) * (palette.Length - 1);
        int index = Math.Min(palette.Length - 2, (int)pos);
        double fraction = pos - index;
        Color32 a = palette[index], b = palette[index + 1];
        byte Mix(byte left, byte right) => (byte)Math.Clamp(
            (int)Math.Round(left + (right - left) * fraction), 0, 255);
        return new Color32(Mix(a.A, b.A), Mix(a.R, b.R), Mix(a.G, b.G), Mix(a.B, b.B));
    }

    private static int Int(ArtworkRecipe recipe, string key) =>
        int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
    private static double Number(ArtworkRecipe recipe, string key) =>
        double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
