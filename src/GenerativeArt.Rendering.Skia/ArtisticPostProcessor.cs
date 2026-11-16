using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Pixels;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Grana monocromatica seeded e dithering ordinato spaziale. Opera sul raster
/// finale in ARGB straight preservando l'alpha. La finitura materia facoltativa
/// usa soltanto un ring buffer di tre righe per l’assorbimento del colore.
/// L'algoritmo non usa Random.Shared, non ha stato e non dipende dal frame:
/// texture coerente anche durante un loop senza accumulare fotogrammi.
/// </summary>
public static class ArtisticPostProcessor
{
    private static ReadOnlySpan<byte> Bayer4 =>
    [
         0,  8,  2, 10,
        12,  4, 14,  6,
         3, 11,  1,  9,
        15,  7, 13,  5
    ];

    private static ReadOnlySpan<byte> Bayer8 =>
    [
         0, 48, 12, 60,  3, 51, 15, 63,
        32, 16, 44, 28, 35, 19, 47, 31,
         8, 56,  4, 52, 11, 59,  7, 55,
        40, 24, 36, 20, 43, 27, 39, 23,
         2, 50, 14, 62,  1, 49, 13, 61,
        34, 18, 46, 30, 33, 17, 45, 29,
        10, 58,  6, 54,  9, 57,  5, 53,
        42, 26, 38, 22, 41, 25, 37, 21
    ];

    public static void Apply(PixelBuffer pixels, ArtisticFinishOptions options,
        ulong seed, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.IsEnabled) return;
        if (options.HasMaterial)
            MaterialPostProcessor.Apply(pixels, options, seed, cancellationToken);
        if (options.GrainAmount == 0 && options.DitherMode == "none") return;

        bool grain = options.GrainAmount > 0;
        int matrixSide = options.DitherMode switch { "bayer4" => 4, "bayer8" => 8, _ => 0 };
        for (int y = 0; y < pixels.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            for (int x = 0; x < row.Length; x++)
            {
                Color32 color = Color32.FromArgb(row[x]);
                // Nessun colore nascosto viene generato nei pixel completamente trasparenti.
                if (color.A == 0) continue;
                int delta = grain ? GrainOffset(seed, x / options.GrainSize,
                    y / options.GrainSize, options.GrainAmount) : 0;
                int r = Math.Clamp(color.R + delta, 0, 255);
                int g = Math.Clamp(color.G + delta, 0, 255);
                int b = Math.Clamp(color.B + delta, 0, 255);
                if (matrixSide != 0)
                {
                    ReadOnlySpan<byte> matrix = matrixSide == 4 ? Bayer4 : Bayer8;
                    int rank = matrix[(y % matrixSide) * matrixSide + x % matrixSide];
                    // Soglia ordinata [0,1): bilanciata nel tempo e nello spazio.
                    double threshold = (rank + 0.5) / (matrixSide * matrixSide);
                    r = Quantize(r, options.DitherLevels, threshold);
                    g = Quantize(g, options.DitherLevels, threshold);
                    b = Quantize(b, options.DitherLevels, threshold);
                }
                row[x] = new Color32(color.A, (byte)r, (byte)g, (byte)b).PackedArgb;
            }
        }
    }

    private static int Quantize(int value, int levels, double threshold)
    {
        int bucket = Math.Clamp((int)Math.Floor(value * (levels - 1d) / 255d + threshold),
            0, levels - 1);
        return (int)Math.Round(bucket * 255d / (levels - 1), MidpointRounding.AwayFromZero);
    }

    private static int GrainOffset(ulong seed, int cellX, int cellY, double amount)
    {
        // Finalizer SplitMix64 su coordinate di cella: nessuno stato mutabile.
        ulong z = seed ^ ((ulong)(uint)cellX * 0x9E3779B97F4A7C15UL) ^
            ((ulong)(uint)cellY * 0xD1B54A32D192ED03UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        z ^= z >> 31;
        int signed = (int)(z >> 56) - 128;
        return (int)Math.Round(signed * (255d / 128d) * amount, MidpointRounding.AwayFromZero);
    }
}
