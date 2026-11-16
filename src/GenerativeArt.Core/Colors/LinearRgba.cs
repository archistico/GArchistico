namespace GenerativeArt.Core.Colors;

/// <summary>
/// RGB lineare, straight alpha, canali nel range [0,1].
/// Distinto da NormalizedRgba (sRGB codificato) e da PremultipliedColor32.
/// </summary>
public readonly record struct LinearRgba
{
    public double R { get; }
    public double G { get; }
    public double B { get; }
    public double A { get; }

    public LinearRgba(double r, double g, double b, double a = 1d)
    {
        R = NormalizedRgba.Unit(r, nameof(r));
        G = NormalizedRgba.Unit(g, nameof(g));
        B = NormalizedRgba.Unit(b, nameof(b));
        A = NormalizedRgba.Unit(a, nameof(a));
    }

    public static LinearRgba FromSrgb(NormalizedRgba srgb) => new(
        SrgbTransfer.ToLinear(srgb.R),
        SrgbTransfer.ToLinear(srgb.G),
        SrgbTransfer.ToLinear(srgb.B), srgb.A);

    public NormalizedRgba ToSrgb() => new(
        Math.Clamp(SrgbTransfer.ToSrgb(R), 0d, 1d),
        Math.Clamp(SrgbTransfer.ToSrgb(G), 0d, 1d),
        Math.Clamp(SrgbTransfer.ToSrgb(B), 0d, 1d), A);

    /// <summary>Interpolazione in RGB lineare; alpha straight interpolato separatamente.</summary>
    public LinearRgba Mix(LinearRgba other, double amount)
    {
        if (!double.IsFinite(amount) || amount < 0d || amount > 1d)
            throw new ArgumentOutOfRangeException(nameof(amount));
        return new LinearRgba(
            Math.Clamp(R + (other.R - R) * amount, 0d, 1d),
            Math.Clamp(G + (other.G - G) * amount, 0d, 1d),
            Math.Clamp(B + (other.B - B) * amount, 0d, 1d),
            Math.Clamp(A + (other.A - A) * amount, 0d, 1d));
    }
}
