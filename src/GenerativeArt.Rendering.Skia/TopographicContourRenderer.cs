using System.Globalization;
using GenerativeArt.Algorithms.Contours;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// M4A: terrain scalar field, gradini cromatici e isolinee collegate.
/// Usa una griglia con passo spaziale controllato: il campo viene valutato una
/// sola volta; la stessa griglia alimenta sia il colore sia Marching Squares.
/// </summary>
internal static class TopographicContourRenderer
{
    internal const long MaximumPixelCount = 16_777_216L;
    internal const long MaximumCellLevelBudget = 40_000_000L;

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        if (recipe.Size.Width < 16 || recipe.Size.Height < 16)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                "Curve di livello: dimensione minima 16x16 pixel.");
        if (recipe.Size.PixelCount > MaximumPixelCount)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                $"Curve di livello: massimo {MaximumPixelCount:N0} pixel per immagine.");
        int cell = Int(recipe, "cell-size");
        int levels = Int(recipe, "contours");
        int nx = Math.Max(1, (recipe.Size.Width - 1 + cell - 1) / cell);
        int ny = Math.Max(1, (recipe.Size.Height - 1 + cell - 1) / cell);
        if ((long)nx * ny * levels > MaximumCellLevelBudget)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                $"Curve di livello: budget massimo {MaximumCellLevelBudget:N0} celle x livelli.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken cancellationToken)
    {
        CanvasSize size = recipe.Size;
        int cell = Int(recipe, "cell-size");
        int contours = Int(recipe, "contours");
        double frequency = Double(recipe, "frequency");
        float lineWidth = (float)Double(recipe, "line-width");
        int cellsX = Math.Max(1, (size.Width - 1 + cell - 1) / cell);
        int cellsY = Math.Max(1, (size.Height - 1 + cell - 1) / cell);
        int columns = cellsX + 1, rows = cellsY + 1;
        var samples = new float[columns * rows];
        var noise = new SeededSimplexNoise(recipe.Seed);

        for (int gy = 0; gy < rows; gy++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double y = gy * ((double)(size.Height - 1) / cellsY);
            for (int gx = 0; gx < columns; gx++)
            {
                double x = gx * ((double)(size.Width - 1) / cellsX);
                double amplitude = 1, total = 0, sum = 0, f = frequency;
                for (int octave = 0; octave < 4; octave++)
                {
                    sum += amplitude * noise.Sample2D((x + 142.5) * f, (y - 81.25) * f);
                    total += amplitude;
                    amplitude *= .52;
                    f *= 2;
                }
                samples[gy * columns + gx] = (float)Math.Clamp(.5 + .5 * sum / total, 0, 1);
            }
        }

        Color32[] palette = recipe.Parameters["palette"] switch
        {
            "atlas" => [Color32.Opaque(15, 42, 59), Color32.Opaque(33, 79, 91),
                        Color32.Opaque(79, 128, 114), Color32.Opaque(144, 171, 132),
                        Color32.Opaque(204, 200, 153), Color32.Opaque(241, 225, 182)],
            "nocturne" => [Color32.Opaque(10, 17, 35), Color32.Opaque(21, 36, 66),
                            Color32.Opaque(40, 62, 100), Color32.Opaque(63, 93, 128),
                            Color32.Opaque(93, 139, 153), Color32.Opaque(145, 191, 184)],
            "terra" => [Color32.Opaque(55, 27, 36), Color32.Opaque(103, 47, 56),
                         Color32.Opaque(148, 73, 64), Color32.Opaque(188, 113, 80),
                         Color32.Opaque(220, 162, 111), Color32.Opaque(250, 217, 161)],
            _ => throw new ArgumentException("Palette topografica non supportata.")
        };
        Color32 thin = recipe.Parameters["palette"] switch
        {
            "atlas" => new Color32(185, 231, 239, 213),
            "nocturne" => new Color32(178, 132, 219, 225),
            _ => new Color32(191, 255, 215, 168)
        };
        Color32 major = recipe.Parameters["palette"] switch
        {
            "atlas" => Color32.Opaque(253, 246, 220),
            "nocturne" => Color32.Opaque(233, 249, 248),
            _ => Color32.Opaque(255, 239, 204)
        };
        thin = ArtworkInk.Resolve(recipe, "line-color", thin);
        major = ArtworkInk.Resolve(recipe, "major-line-color", major);
        for (int i = 0; i < palette.Length; i++)
            palette[i] = ArtworkInk.Resolve(recipe, $"band-color-{i + 1}", palette[i]);

        // Il raster usa la stessa griglia scalare delle isolinee, senza ricampionare
        // nuovamente il noise per ogni pixel. Interpolazione bilineare tra celle.
        var raster = new PixelBuffer(size);
        for (int y = 0; y < size.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double gy = (double)y * cellsY / Math.Max(1, size.Height - 1);
            int iy = Math.Min((int)gy, cellsY - 1);
            double ty = Math.Clamp(gy - iy, 0, 1);
            Span<uint> row = raster.GetWritablePackedArgbRow(y);
            for (int x = 0; x < size.Width; x++)
            {
                double gx = (double)x * cellsX / Math.Max(1, size.Width - 1);
                int ix = Math.Min((int)gx, cellsX - 1);
                double tx = Math.Clamp(gx - ix, 0, 1);
                int top = iy * columns + ix, bottom = top + columns;
                double a = samples[top] + (samples[top + 1] - samples[top]) * tx;
                double b = samples[bottom] + (samples[bottom + 1] - samples[bottom]) * tx;
                double height = Math.Clamp(a + (b - a) * ty, 0, 1);
                int band = Math.Min(palette.Length - 1, (int)(height * palette.Length));
                row[x] = palette[band].PackedArgb;
            }
        }
        surface.DrawPixels(raster);

        float scaleX = (float)(size.Width - 1) / cellsX;
        float scaleY = (float)(size.Height - 1) / cellsY;
        string strokeStyle = recipe.Parameters["line-style"];
        float roughness = (float)Double(recipe, "stroke-roughness");
        for (int i = 0; i < contours; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float level = (float)(.13 + .74 * (i + 1) / (contours + 1));
            bool prominent = i % 4 == 0;
            IReadOnlyList<ContourPolyline> lines = MarchingSquares.Trace(samples,
                columns, rows, level, cancellationToken);
            foreach (ContourPolyline line in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IReadOnlyList<System.Numerics.Vector2> stroke = ContourStrokeVariation.Apply(
                    line.Points, line.Closed, strokeStyle, roughness, scaleX, scaleY, recipe.Seed);
                surface.DrawPolyline(stroke, scaleX, scaleY,
                    prominent ? major : thin, prominent ? lineWidth * 1.45f : lineWidth);
            }
        }
    }

    private static int Int(ArtworkRecipe recipe, string key) =>
        int.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);

    private static double Double(ArtworkRecipe recipe, string key) =>
        double.Parse(recipe.Parameters[key], CultureInfo.InvariantCulture);
}
