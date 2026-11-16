namespace GenerativeArt.Core.Colors.Spaces;

/// <summary>
/// OKLab basato sulla pipeline Gȧrt RGB(sRGB) -> XYZ D65/Bradford -> LMS -> OKLab.
/// I colori fuori gamut vengono limitati a [0,1] solo durante ToRgba().
/// I coefficienti sono quelli di ColorOKLAB.kt e xyz.kt nel repository sorgente.
/// </summary>
public readonly record struct OklabColor
{
    public double L { get; }
    public double A { get; }
    public double B { get; }
    public double Alpha { get; }

    public OklabColor(double lightness, double a, double b, double alpha = 1d)
    {
        if (!double.IsFinite(lightness))
            throw new ArgumentOutOfRangeException(nameof(lightness));
        if (!double.IsFinite(a)) throw new ArgumentOutOfRangeException(nameof(a));
        if (!double.IsFinite(b)) throw new ArgumentOutOfRangeException(nameof(b));
        L = lightness;
        A = a;
        B = b;
        Alpha = NormalizedRgba.Unit(alpha, nameof(alpha));
    }

    public static OklabColor FromRgba(NormalizedRgba color)
    {
        var (x, y, z) = GartsRgbXyz.ToXyz(color);
        // Gȧrt converte xyz in Float prima dell'applicazione delle matrici OKLab.
        double xf = (float)x, yf = (float)y, zf = (float)z;
        double l = Math.Cbrt(0.819022437996703 * xf + 0.3619062600528904 * yf - 0.1288737815209879 * zf);
        double m = Math.Cbrt(0.0329836539323885 * xf + 0.9292868615863434 * yf + 0.0361446663506424 * zf);
        double s = Math.Cbrt(0.0481771893596242 * xf + 0.2642395317527308 * yf + 0.6335478284694309 * zf);
        return new OklabColor(
            (float)(0.210454268309314 * l + 0.7936177747023054 * m - 0.0040720430116193 * s),
            (float)(1.9779985324311684 * l - 2.4285922420485799 * m + 0.450593709617411 * s),
            (float)(0.0259040424655478 * l + 0.7827717124575296 * m - 0.8086757549230774 * s), color.A);
    }

    public NormalizedRgba ToRgba()
    {
        double l = L + 0.3963377773761749 * A + 0.2158037573099136 * B;
        double m = L - 0.1055613458156586 * A - 0.0638541728258133 * B;
        double s = L - 0.0894841775298119 * A - 1.2914855480194092 * B;
        l = l * l * l;
        m = m * m * m;
        s = s * s * s;
        double x = (float)(1.2268798758459243 * l - 0.5578149944602171 * m + 0.2813910456659647 * s);
        double y = (float)(-0.0405757452148008 * l + 1.112286803280317 * m - 0.0717110580655164 * s);
        double z = (float)(-0.0763729366746601 * l - 0.4214933324022432 * m + 1.5869240198367816 * s);
        return GartsRgbXyz.FromXyz(x, y, z, Alpha);
    }

    public OklabColor Mix(OklabColor other, double amount)
    {
        if (!double.IsFinite(amount) || amount < 0d || amount > 1d)
            throw new ArgumentOutOfRangeException(nameof(amount));
        return new OklabColor(L + (other.L - L) * amount,
            A + (other.A - A) * amount, B + (other.B - B) * amount,
            Alpha + (other.Alpha - Alpha) * amount);
    }
}

/// <summary>Conversione con coefficenti di Gȧrt xyz.kt (compresi gli adattamenti Bradford).</summary>
internal static class GartsRgbXyz
{
    private const double WhiteX = 0.95047;
    private const double WhiteY = 1.0;
    private const double WhiteZ = 1.08883;

    // Row-index = index 0..2, col-index = index 0,3,6 (layout col-major usato in xyz.kt).
    private static readonly double[] RgbToXyz =
    [
        0.4124564390896922, 0.21267285140562253, 0.0193338955823293,
        0.357576077643909, 0.715152155287818, 0.11919202588130297,
        0.18043748326639894, 0.07217499330655958, 0.9503040785363679
    ];
    private static readonly double[] XyzToRgb =
    [
        3.2404541621141045, -0.9692660305051868, 0.055643430959114726,
        -1.5371385127977166, 1.8760108454466942, -0.2040259135167538,
        -0.498531409556016, 0.041556017530349834, 1.0572251882231791
    ];
    private static readonly double[] Bradford =
    [
        0.8951, -0.7502, 0.0389,
        0.2664, 1.7135, -0.0685,
        -0.1614, 0.0367, 1.0296
    ];
    private static readonly double[] InverseBradford =
    [
        0.9869929054667123, 0.43230526972339456, -0.008528664575177328,
        -0.14705425642099013, 0.5183602715367776, 0.04004282165408487,
        0.15996265166373125, 0.0492912282128556, 0.9684866957875502
    ];

    private static double Row(double[] matrix, int row, double x, double y, double z) =>
        x * matrix[row] + y * matrix[row + 3] + z * matrix[row + 6];

    internal static (double X, double Y, double Z) ToXyz(NormalizedRgba color)
    {
        double r = Linearize(color.R), g = Linearize(color.G), b = Linearize(color.B);
        double x = Row(RgbToXyz, 0, r, g, b);
        double y = Row(RgbToXyz, 1, r, g, b);
        double z = Row(RgbToXyz, 2, r, g, b);

        double ad = Row(Bradford, 0, WhiteX, WhiteY, WhiteZ);
        double bd = Row(Bradford, 1, WhiteX, WhiteY, WhiteZ);
        double cd = Row(Bradford, 2, WhiteX, WhiteY, WhiteZ);
        double xx = Row(Bradford, 0, x, y, z) * ad / 0.9414285350000001;
        double yy = Row(Bradford, 1, x, y, z) * bd / 1.040417467;
        double zz = Row(Bradford, 2, x, y, z) * cd / 1.089532651;
        return (Row(InverseBradford, 0, xx, yy, zz),
            Row(InverseBradford, 1, xx, yy, zz),
            Row(InverseBradford, 2, xx, yy, zz));
    }

    internal static NormalizedRgba FromXyz(double x, double y, double z, double alpha)
    {
        // In xyz.kt la ri-adattazione usa lo stesso D65 per fonte e destinazione:
        // il rapporto dei bianchi e' esattamente 1; manteniamo le due matrici.
        double x1 = Row(Bradford, 0, x, y, z);
        double y1 = Row(Bradford, 1, x, y, z);
        double z1 = Row(Bradford, 2, x, y, z);
        double x2 = Row(InverseBradford, 0, x1, y1, z1);
        double y2 = Row(InverseBradford, 1, x1, y1, z1);
        double z2 = Row(InverseBradford, 2, x1, y1, z1);
        double r = Compand(Row(XyzToRgb, 0, x2, y2, z2));
        double g = Compand(Row(XyzToRgb, 1, x2, y2, z2));
        double b = Compand(Row(XyzToRgb, 2, x2, y2, z2));
        return new NormalizedRgba(Math.Clamp((double)(float)r, 0d, 1d),
            Math.Clamp((double)(float)g, 0d, 1d), Math.Clamp((double)(float)b, 0d, 1d), alpha);
    }

    private static double Linearize(double value) => value <= 0.04045d
        ? value / 12.92d
        : Math.Pow((value + 0.055d) / 1.055d, 2.4d);

    private static double Compand(double value)
    {
        double sign = Math.Sign(value);
        double abs = Math.Abs(value);
        return sign * (abs <= 0.0031308d ? abs * 12.92d : 1.055d * Math.Pow(abs, 1d / 2.4d) - 0.055d);
    }
}
