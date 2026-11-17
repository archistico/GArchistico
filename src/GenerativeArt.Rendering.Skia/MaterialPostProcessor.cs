using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Pixels;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Simulazione procedurale di supporti cartacei e pigmenti. Il valore in ogni
/// coordinata dipende solo da seed, posizione e opzioni: nessun Random condiviso
/// e nessuna variazione tra frame di un loop. I canali alpha restano intatti.
///
/// La modalità assorbimento legge i vicini dal raster ORIGINALE con tre righe
/// a scorrimento (O(larghezza) e non O(larghezza * altezza) memoria extra).
/// Si evita così che l'effetto dipenda dall'ordine di attraversamento.
/// </summary>
public static class MaterialPostProcessor
{
    private const ulong PaperSeed = 0xA0761D6478BD642FUL;
    private const ulong FiberSeed = 0xE7037ED1A0B428DBUL;
    private const ulong PigmentSeed = 0x8EBC6AF09C88C6E3UL;

    public static void Apply(PixelBuffer pixels, ArtisticFinishOptions options,
        ulong seed, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.HasMaterial) return;

        bool absorb = options.InkAbsorption > 0;
        uint[]? previous = null;
        uint[]? current = null;
        uint[]? next = null;
        if (absorb)
        {
            current = pixels.GetPackedArgbRow(0).ToArray();
            next = pixels.Height > 1 ? pixels.GetPackedArgbRow(1).ToArray() : null;
        }

        for (int y = 0; y < pixels.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Span<uint> target = pixels.GetWritablePackedArgbRow(y);
            ReadOnlySpan<uint> original = absorb ? current!.AsSpan() : target;
            ApplyMaterialRow(target, original, previous, next, options, seed, y);

            if (absorb)
            {
                // La riga successiva non è ancora stata modificata.
                uint[]? reused = previous;
                previous = current;
                current = next;
                next = y + 2 < pixels.Height
                    ? reused is not null && reused.Length == pixels.Width
                        ? CopyRow(pixels.GetPackedArgbRow(y + 2), reused)
                        : pixels.GetPackedArgbRow(y + 2).ToArray()
                    : null;
            }
        }
    }

    /// <summary>
    /// Applica carta/pigmento/inchiostro alla riga Y assoluta. I vicini, quando
    /// richiesti, provengono tutti dal raster originale, mai da una riga modificata.
    /// </summary>
    internal static void ApplyMaterialRow(Span<uint> target, ReadOnlySpan<uint> original,
        uint[]? previous, uint[]? next, ArtisticFinishOptions options, ulong seed, int y)
    {
        bool absorb = options.InkAbsorption > 0;
        for (int x = 0; x < target.Length; x++)
        {
            Color32 color = Color32.FromArgb(original[x]);
            if (color.A == 0) continue;

            // Il pattern viene valutato anche senza carta esplicita, se
            // serve modulare l'assorbimento; l'alpha non viene mai scritto.
            double paper = options.PaperTexture switch
            {
                "cotton" => Cotton(seed, x, y),
                "rough" => Rough(seed, x, y),
                _ => 0d
            };
            double red = color.R, green = color.G, blue = color.B;
            if (absorb)
            {
                Color32 dark = DarkestNeighbor(x, original, previous, next, color);
                double contrast = Luminance(color) - Luminance(dark);
                if (contrast > 16)
                {
                    double weight = options.InkAbsorption *
                        Math.Clamp((contrast - 16) / 120d, 0, 1) *
                        (0.78 - 0.22 * paper);
                    red += (dark.R - red) * weight;
                    green += (dark.G - green) * weight;
                    blue += (dark.B - blue) * weight;
                }
            }

            if (options.PaperTexture != "none" && options.PaperStrength > 0)
            {
                // La trama modifica il valore, non la geometria né alpha.
                double shift = options.PaperStrength * 48d * paper;
                red += shift; green += shift; blue += shift;
            }

            if (options.PigmentGranulation > 0)
            {
                double pigment = 0.64 * SmoothNoise(seed ^ PigmentSeed, x, y, 9, 9)
                    + 0.36 * SmoothNoise(seed ^ FiberSeed, x, y, 2, 3);
                // Il pigmento si aggrega soprattutto nelle campiture scure;
                // nelle zone chiare il deposito è più leggero.
                double darkness = 1d - Luminance(color) / 255d;
                double shift = options.PigmentGranulation * 76d * pigment *
                    (0.28 + 0.72 * darkness);
                red += shift; green += shift; blue += shift;
            }

            target[x] = new Color32(color.A, Channel(red), Channel(green), Channel(blue)).PackedArgb;
        }
    }

    private static uint[] CopyRow(ReadOnlySpan<uint> source, uint[] destination)
    {
        source.CopyTo(destination);
        return destination;
    }

    private static Color32 DarkestNeighbor(int x, ReadOnlySpan<uint> current,
        uint[]? previous, uint[]? next, Color32 center)
    {
        Color32 result = center;
        double darkest = Luminance(center);
        void Try(uint packed)
        {
            Color32 neighbor = Color32.FromArgb(packed);
            if (neighbor.A == 0) return;
            double value = Luminance(neighbor);
            if (value >= darkest) return;
            result = neighbor;
            darkest = value;
        }
        if (x > 0) Try(current[x - 1]);
        if (x + 1 < current.Length) Try(current[x + 1]);
        if (previous is not null) Try(previous[x]);
        if (next is not null) Try(next[x]);
        return result;
    }

    private static double Cotton(ulong seed, int x, int y) =>
        0.55 * SmoothNoise(seed ^ PaperSeed, x, y, 36, 36) +
        0.30 * SmoothNoise(seed ^ FiberSeed, x, y, 8, 8) +
        0.15 * SmoothNoise(seed ^ PaperSeed, x, y, 2, 24);

    private static double Rough(ulong seed, int x, int y) =>
        0.38 * SmoothNoise(seed ^ PaperSeed, x, y, 45, 18) +
        0.35 * SmoothNoise(seed ^ FiberSeed, x, y, 12, 4) +
        0.27 * SmoothNoise(seed ^ PaperSeed, x, y, 2, 23);

    private static double SmoothNoise(ulong seed, int x, int y, int scaleX, int scaleY)
    {
        int ix = x / scaleX, iy = y / scaleY;
        double tx = (x % scaleX) / (double)scaleX;
        double ty = (y % scaleY) / (double)scaleY;
        tx = tx * tx * (3 - 2 * tx);
        ty = ty * ty * (3 - 2 * ty);
        double a = Lerp(Lattice(seed, ix, iy), Lattice(seed, ix + 1, iy), tx);
        double b = Lerp(Lattice(seed, ix, iy + 1), Lattice(seed, ix + 1, iy + 1), tx);
        return Lerp(a, b, ty);
    }

    private static double Lattice(ulong seed, int x, int y)
    {
        ulong z = seed ^ ((ulong)(uint)x * 0x9E3779B97F4A7C15UL)
            ^ ((ulong)(uint)y * 0xD1B54A32D192ED03UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        z ^= z >> 31;
        return (z >> 32) / (double)uint.MaxValue * 2 - 1;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;
    private static double Luminance(Color32 c) => .2126 * c.R + .7152 * c.G + .0722 * c.B;
    private static byte Channel(double n) => (byte)Math.Clamp(
        (int)Math.Round(n, MidpointRounding.AwayFromZero), 0, 255);
}
