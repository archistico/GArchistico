namespace GenerativeArt.Core.Colors;

/// <summary>Colore ARGB a 8 bit per canale, alpha non premoltiplicato (straight).</summary>
public readonly record struct Color32(byte A, byte R, byte G, byte B)
{
    public static Color32 Transparent => new(0, 0, 0, 0);
    public static Color32 Opaque(byte red, byte green, byte blue) => new(255, red, green, blue);
    public static Color32 FromArgb(uint argb) => new(
        (byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    public uint PackedArgb => ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;
    public Color32 WithAlpha(byte alpha) => new(alpha, R, G, B);

    public PremultipliedColor32 Premultiply() => PremultipliedColor32.FromStraight(this);

    /// <summary>Composizione Porter-Duff SrcOver con arrotondamento al più vicino.</summary>
    public Color32 Over(Color32 back)
    {
        int frontA = A;
        int backA = back.A;
        int outA = frontA + (backA * (255 - frontA) + 127) / 255;
        if (outA == 0) return Transparent;

        byte Composite(byte front, byte background)
        {
            long numerator = (long)front * frontA * 255L
                + (long)background * backA * (255 - frontA);
            long denominator = (long)outA * 255L;
            return (byte)Math.Clamp((numerator + denominator / 2L) / denominator, 0L, 255L);
        }

        return new Color32((byte)outA,
            Composite(R, back.R), Composite(G, back.G), Composite(B, back.B));
    }
}
