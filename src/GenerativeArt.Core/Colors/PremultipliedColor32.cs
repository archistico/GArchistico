namespace GenerativeArt.Core.Colors;

/// <summary>
/// ARGB premoltiplicato. Per canali validi R,G,B non possono superare A.
/// Niente interpretazione implicita come Color32 straight.
/// </summary>
public readonly struct PremultipliedColor32
{
    public byte A { get; }
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public PremultipliedColor32(byte alpha, byte red, byte green, byte blue)
    {
        if (red > alpha || green > alpha || blue > alpha)
            throw new ArgumentException("I canali premoltiplicati non possono superare alpha.");
        A = alpha;
        R = red;
        G = green;
        B = blue;
    }

    public static PremultipliedColor32 FromStraight(Color32 color)
    {
        byte Scale(byte component) => (byte)((component * color.A + 127) / 255);
        return new PremultipliedColor32(color.A, Scale(color.R), Scale(color.G), Scale(color.B));
    }

    public Color32 ToStraight()
    {
        if (A == 0) return Color32.Transparent;
        byte alpha = A;
        return new Color32(alpha, Unscale(R, alpha), Unscale(G, alpha), Unscale(B, alpha));
    }

    // Helper statico: evita la cattura implicita di 'this' in una readonly struct (CS1673).
    private static byte Unscale(byte component, byte alpha) =>
        (byte)Math.Min(255, (component * 255 + alpha / 2) / alpha);
}
