using System.Globalization;

namespace GenerativeArt.Application.Rendering;

/// <summary>
/// Valore per l'editor visivo: sRGB straight-alpha e HSV in coordinate standard.
/// Il formato serializzato resta #RRGGBB oppure #AARRGGBB (alpha per primo).
/// Nessuna conversione viene effettuata nel renderer, salvo l'override richiesto.
/// </summary>
public readonly record struct StudioColorValue(byte R, byte G, byte B, byte A = 255)
{
    public static StudioColorValue Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if ((text.Length != 7 && text.Length != 9) || text[0] != '#')
            throw new FormatException("Colore atteso nel formato #RRGGBB o #AARRGGBB.");
        for (int i = 1; i < text.Length; i++)
            if (!Uri.IsHexDigit(text[i]))
                throw new FormatException("Il colore contiene caratteri non esadecimali.");
        uint packed = uint.Parse(text.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        if (text.Length == 7) packed |= 0xFF000000;
        return new StudioColorValue((byte)(packed >> 16), (byte)(packed >> 8), (byte)packed, (byte)(packed >> 24));
    }

    public string ToHex() => A == 255
        ? $"#{R:X2}{G:X2}{B:X2}"
        : $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    public (double Hue, double Saturation, double Value) ToHsv()
    {
        double r = R / 255d, g = G / 255d, b = B / 255d;
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;
        double hue = 0d;
        if (delta > 0)
        {
            if (max == r) hue = 60d * (((g - b) / delta + 6) % 6);
            else if (max == g) hue = 60d * ((b - r) / delta + 2);
            else hue = 60d * ((r - g) / delta + 4);
        }
        return (hue, max == 0 ? 0 : delta / max, max);
    }

    public static StudioColorValue FromHsv(double hue, double saturation, double value, byte alpha = 255)
    {
        if (!double.IsFinite(hue) || !double.IsFinite(saturation) || !double.IsFinite(value)
            || hue < 0 || hue > 360 || saturation is < 0 or > 1 || value is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(hue), "HSV deve essere finito: H 0..360, S/V 0..1.");
        double h = hue == 360 ? 0 : hue;
        double c = value * saturation;
        double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
        double m = value - c;
        (double r, double g, double b) = h switch
        {
            < 60 => (c, x, 0d),
            < 120 => (x, c, 0d),
            < 180 => (0d, c, x),
            < 240 => (0d, x, c),
            < 300 => (x, 0d, c),
            _ => (c, 0d, x)
        };
        static byte Channel(double v) => (byte)Math.Clamp((int)Math.Round(v * 255d), 0, 255);
        return new StudioColorValue(Channel(r + m), Channel(g + m), Channel(b + m), alpha);
    }
}
