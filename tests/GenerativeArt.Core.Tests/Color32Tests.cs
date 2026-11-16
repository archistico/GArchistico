using GenerativeArt.Core.Colors;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class Color32Tests
{
    [Theory]
    [InlineData(0x00000000u)]
    [InlineData(0x11223344u)]
    [InlineData(0x80FF0040u)]
    [InlineData(0xFFFFFFFFu)]
    public void ArgbRoundTrip(uint packed)
    {
        Assert.Equal(packed, Color32.FromArgb(packed).PackedArgb);
    }

    [Fact]
    public void AlphaZeroHasCanonicalPremultipliedBlack()
    {
        var straight = new Color32(0, 255, 90, 40);
        Assert.Equal(Color32.Transparent, straight.Premultiply().ToStraight());
    }

    [Fact]
    public void PremultiplicationRejectsInvalidRgbChannels() =>
        Assert.Throws<ArgumentException>(() => new PremultipliedColor32(20, 21, 0, 0));

    [Theory]
    [InlineData(255, 100, 130, 200)]
    [InlineData(128, 100, 130, 200)]
    [InlineData(64, 100, 130, 200)]
    public void PremultipliedRoundTripWithinQuantizationTolerance(int a, int r, int g, int b)
    {
        byte alpha = (byte)a;
        byte red = (byte)r;
        byte green = (byte)g;
        byte blue = (byte)b;
        var color = new Color32(alpha, red, green, blue);
        var back = color.Premultiply().ToStraight();
        Assert.Equal(alpha, back.A);
        // Quantizzazione RGB inevitabile con alpha molto basso: per A=64 errore <=2.
        Assert.InRange(Math.Abs(back.R - red), 0, 2);
        Assert.InRange(Math.Abs(back.G - green), 0, 2);
        Assert.InRange(Math.Abs(back.B - blue), 0, 2);
    }

    [Fact]
    public void DirectPremultipliedConversionPreservesAlphaAndExpectedRounding()
    {
        var premultiplied = new PremultipliedColor32(64, 0, 32, 64);
        Assert.Equal(new Color32(64, 0, 128, 255), premultiplied.ToStraight());
        Assert.Equal(Color32.Opaque(12, 34, 56),
            new PremultipliedColor32(255, 12, 34, 56).ToStraight());
    }

    [Fact]
    public void TransparentFrontDoesNotAlterOpaqueBack()
    {
        var back = Color32.Opaque(10, 20, 30);
        Assert.Equal(back, new Color32(0, 200, 200, 200).Over(back));
    }

    [Fact]
    public void HalfOpaqueRedOverBlueIsSourceOver()
    {
        var front = new Color32(128, 255, 0, 0);
        var back = Color32.Opaque(0, 0, 255);
        Assert.Equal(new Color32(255, 128, 0, 127), front.Over(back));
    }

    [Fact]
    public void TransparentOnTransparentIsTransparentBlack()
    {
        Assert.Equal(Color32.Transparent, new Color32(0, 255, 0, 0).Over(new Color32(0, 0, 0, 255)));
    }
}
