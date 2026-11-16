namespace GenerativeArt.Core.Colors;

/// <summary>
/// Funzioni di trasferimento standard sRGB IEC 61966-2-1, su canali unitari.
/// Non effettuano premoltiplicazione alpha, conversioni HDR o gamut mapping.
/// </summary>
public static class SrgbTransfer
{
    public static double ToLinear(double srgb)
    {
        ValidateUnit(srgb, nameof(srgb));
        return srgb <= 0.04045d
            ? srgb / 12.92d
            : Math.Pow((srgb + 0.055d) / 1.055d, 2.4d);
    }

    public static double ToSrgb(double linear)
    {
        ValidateUnit(linear, nameof(linear));
        return linear <= 0.0031308d
            ? 12.92d * linear
            : 1.055d * Math.Pow(linear, 1d / 2.4d) - 0.055d;
    }

    private static void ValidateUnit(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0d || value > 1d)
            throw new ArgumentOutOfRangeException(parameterName, "Il canale deve essere finito e compreso tra 0 e 1.");
    }
}
