namespace GenerativeArt.Core.Colors;

/// <summary>
/// Colore RGBA sRGB straight, con canali normalizzati nel range [0,1].
/// Non è alpha premoltiplicato e non è RGB lineare.
/// </summary>
public readonly record struct NormalizedRgba
{
    public double R { get; }
    public double G { get; }
    public double B { get; }
    public double A { get; }

    public NormalizedRgba(double r, double g, double b, double a = 1d)
    {
        R = Unit(r, nameof(r));
        G = Unit(g, nameof(g));
        B = Unit(b, nameof(b));
        A = Unit(a, nameof(a));
    }

    internal static double Unit(double value, string name)
    {
        if (!double.IsFinite(value) || value < 0d || value > 1d)
            throw new ArgumentOutOfRangeException(name, "Il canale deve essere finito e compreso tra 0 e 1.");
        return value;
    }

    public static NormalizedRgba FromColor32(Color32 color) =>
        new(color.R / 255d, color.G / 255d, color.B / 255d, color.A / 255d);

    public Color32 ToColor32() => new(
        Quantize(A), Quantize(R), Quantize(G), Quantize(B));

    private static byte Quantize(double channel) =>
        (byte)Math.Round(channel * 255d, MidpointRounding.AwayFromZero);
}
