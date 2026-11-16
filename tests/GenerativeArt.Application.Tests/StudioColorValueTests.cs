using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioColorValueTests
{
    [Theory]
    [InlineData("#123456", 18, 52, 86, 255, "#123456")]
    [InlineData("#80123456", 18, 52, 86, 128, "#80123456")]
    [InlineData("#00010203", 1, 2, 3, 0, "#00010203")]
    [InlineData("#ffffffff", 255, 255, 255, 255, "#FFFFFF")]
    public void ArgbAndRgbHexAreParsedWithoutLosingAlpha(string hex, byte r, byte g,
        byte b, byte alpha, string canonical)
    {
        StudioColorValue c = StudioColorValue.Parse(hex);
        Assert.Equal(new StudioColorValue(r, g, b, alpha), c);
        Assert.Equal(canonical, c.ToHex());
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("#12345")]
    [InlineData("#1122334455")]
    [InlineData("#GG0011")]
    [InlineData("#12345_")]
    public void RejectInvalidHex(string text) =>
        Assert.Throws<FormatException>(() => StudioColorValue.Parse(text));

    [Theory]
    [InlineData(0, 1, 1, 255, 0, 0)]
    [InlineData(120, 1, 1, 0, 255, 0)]
    [InlineData(240, 1, 1, 0, 0, 255)]
    [InlineData(360, 1, 1, 255, 0, 0)]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(240, 0, 1, 255, 255, 255)]
    public void HsvPrimariesAndGrayscale(double h, double s, double v, byte r, byte g, byte b)
    {
        StudioColorValue color = StudioColorValue.FromHsv(h, s, v, alpha: 77);
        Assert.Equal((r, g, b, (byte)77), (color.R, color.G, color.B, color.A));
    }

    // Copertura di tutti e sei i rami dello switch HSV: le tuple devono
    // conservare componenti double anche quando un canale vale zero.
    [Theory]
    [InlineData(30, 255, 128, 0)]
    [InlineData(90, 128, 255, 0)]
    [InlineData(150, 0, 255, 128)]
    [InlineData(210, 0, 128, 255)]
    [InlineData(270, 128, 0, 255)]
    [InlineData(330, 255, 0, 128)]
    public void HsvIntermediateSectorsPreserveColorAndAlpha(double hue, byte r, byte g, byte b)
    {
        StudioColorValue color = StudioColorValue.FromHsv(hue, 1d, 1d, alpha: 200);
        Assert.Equal(new StudioColorValue(r, g, b, 200), color);
    }

    [Fact]
    public void ConvertingRgbToHsvAndBackKeepsEveryChannelWithinOneStep()
    {
        for (int r = 0; r <= 255; r += 17)
            for (int g = 0; g <= 255; g += 17)
                for (int b = 0; b <= 255; b += 17)
                {
                    var original = new StudioColorValue((byte)r, (byte)g, (byte)b, 63);
                    (double hue, double saturation, double value) = original.ToHsv();
                    var result = StudioColorValue.FromHsv(hue, saturation, value, original.A);
                    Assert.InRange(Math.Abs(result.R - r), 0, 1);
                    Assert.InRange(Math.Abs(result.G - g), 0, 1);
                    Assert.InRange(Math.Abs(result.B - b), 0, 1);
                    Assert.Equal((byte)63, result.A);
                }
    }

    [Theory]
    [InlineData(-1, 1, 1)]
    [InlineData(361, 1, 1)]
    [InlineData(60, -0.01, 1)]
    [InlineData(60, 1, 1.01)]
    [InlineData(double.NaN, 0, 0)]
    [InlineData(double.PositiveInfinity, 0, 0)]
    public void RejectInvalidHsv(double h, double s, double v) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => StudioColorValue.FromHsv(h, s, v));
}
