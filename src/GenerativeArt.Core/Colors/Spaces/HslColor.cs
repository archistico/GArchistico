namespace GenerativeArt.Core.Colors.Spaces;

/// <summary>HSL: H in gradi [0,360), saturazione/luminosità/alpha in [0,1].</summary>
public readonly record struct HslColor
{
    public double H { get; }
    public double S { get; }
    public double L { get; }
    public double A { get; }

    public HslColor(double hueDegrees, double saturation, double lightness, double alpha = 1d)
    {
        H = Hue.Normalize(hueDegrees);
        S = NormalizedRgba.Unit(saturation, nameof(saturation));
        L = NormalizedRgba.Unit(lightness, nameof(lightness));
        A = NormalizedRgba.Unit(alpha, nameof(alpha));
    }

    public static HslColor FromRgba(NormalizedRgba color)
    {
        double max = Math.Max(color.R, Math.Max(color.G, color.B));
        double min = Math.Min(color.R, Math.Min(color.G, color.B));
        double delta = max - min;
        double lightness = (max + min) / 2d;
        if (delta == 0d) return new HslColor(0d, 0d, lightness, color.A);
        double saturation = delta / (1d - Math.Abs(2d * lightness - 1d));
        double hue = max == color.R ? 60d * ((color.G - color.B) / delta)
            : max == color.G ? 60d * (2d + (color.B - color.R) / delta)
            : 60d * (4d + (color.R - color.G) / delta);
        return new HslColor(hue, Math.Clamp(saturation, 0d, 1d), lightness, color.A);
    }

    public NormalizedRgba ToRgba()
    {
        double chroma = (1d - Math.Abs(2d * L - 1d)) * S;
        double x = chroma * (1d - Math.Abs(H / 60d % 2d - 1d));
        double m = L - chroma / 2d;
        (double r, double g, double b) = H switch
        {
            < 60d => (chroma, x, 0d),
            < 120d => (x, chroma, 0d),
            < 180d => (0d, chroma, x),
            < 240d => (0d, x, chroma),
            < 300d => (x, 0d, chroma),
            _ => (chroma, 0d, x)
        };
        return new NormalizedRgba(Math.Clamp(r + m, 0d, 1d),
            Math.Clamp(g + m, 0d, 1d), Math.Clamp(b + m, 0d, 1d), A);
    }

    public HslColor Mix(HslColor other, double amount)
    {
        if (!double.IsFinite(amount) || amount < 0d || amount > 1d)
            throw new ArgumentOutOfRangeException(nameof(amount));
        return new HslColor(Hue.Mix(H, other.H, amount),
            S + (other.S - S) * amount, L + (other.L - L) * amount,
            A + (other.A - A) * amount);
    }
}
