namespace GenerativeArt.Core.Colors.Spaces;

/// <summary>HSV: H in gradi [0,360), saturazione/valore/alpha in [0,1].</summary>
public readonly record struct HsvColor
{
    public double H { get; }
    public double S { get; }
    public double V { get; }
    public double A { get; }

    public HsvColor(double hueDegrees, double saturation, double value, double alpha = 1d)
    {
        H = Hue.Normalize(hueDegrees);
        S = NormalizedRgba.Unit(saturation, nameof(saturation));
        V = NormalizedRgba.Unit(value, nameof(value));
        A = NormalizedRgba.Unit(alpha, nameof(alpha));
    }

    public static HsvColor FromRgba(NormalizedRgba color)
    {
        double max = Math.Max(color.R, Math.Max(color.G, color.B));
        double min = Math.Min(color.R, Math.Min(color.G, color.B));
        double delta = max - min;
        if (delta == 0d) return new HsvColor(0d, 0d, max, color.A);

        double hue = max == color.R ? 60d * ((color.G - color.B) / delta)
            : max == color.G ? 60d * (2d + (color.B - color.R) / delta)
            : 60d * (4d + (color.R - color.G) / delta);
        return new HsvColor(hue, max == 0d ? 0d : delta / max, max, color.A);
    }

    public NormalizedRgba ToRgba()
    {
        double chroma = V * S;
        double x = chroma * (1d - Math.Abs(H / 60d % 2d - 1d));
        double m = V - chroma;
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

    public HsvColor Mix(HsvColor other, double amount)
    {
        if (!double.IsFinite(amount) || amount < 0d || amount > 1d)
            throw new ArgumentOutOfRangeException(nameof(amount));
        return new HsvColor(Hue.Mix(H, other.H, amount),
            S + (other.S - S) * amount, V + (other.V - V) * amount,
            A + (other.A - A) * amount);
    }
}
