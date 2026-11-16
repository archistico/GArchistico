using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Colors.Spaces;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class ColorSpacesTests
{
    // Campioni ottenuti eseguendo le conversioni Kotlin originali con stub Color4f.
    [Theory]
    [InlineData(1d, 0d, 0d, 0d, 1d, 1d, 0d, 1d, 0.5d)]
    [InlineData(0d, 1d, 0d, 120d, 1d, 1d, 120d, 1d, 0.5d)]
    [InlineData(0d, 0d, 1d, 240d, 1d, 1d, 240d, 1d, 0.5d)]
    [InlineData(1d, 1d, 0d, 60d, 1d, 1d, 60d, 1d, 0.5d)]
    [InlineData(0.1d, 0.5d, 0.9d, 210d, 0.88888884d, 0.9d, 210d, 0.79999995d, 0.5d)]
    public void HsvAndHslMatchKotlinGolden(double r, double g, double b,
        double hsvH, double hsvS, double hsvV, double hslH, double hslS, double hslL)
    {
        var rgb = new NormalizedRgba(r, g, b, 0.75d);
        HsvColor hsv = HsvColor.FromRgba(rgb);
        HslColor hsl = HslColor.FromRgba(rgb);
        Close(hsvH, hsv.H, 1e-5d);
        Close(hsvS, hsv.S, 1e-5d);
        Close(hsvV, hsv.V, 1e-5d);
        Close(hslH, hsl.H, 1e-5d);
        Close(hslS, hsl.S, 1e-5d);
        Close(hslL, hsl.L, 1e-5d);
        AssertRgbClose(rgb, hsv.ToRgba());
        AssertRgbClose(rgb, hsl.ToRgba());
    }

    [Theory]
    [InlineData(1d, 0d, 0d, 0.6279887d, 0.22487502d, 0.12585296d)]
    [InlineData(0d, 1d, 0d, 0.86643296d, -0.23388578d, 0.17949706d)]
    [InlineData(0d, 0d, 1d, 0.45197758d, -0.032454364d, -0.31150326d)]
    [InlineData(1d, 1d, 0d, 0.9679917d, -0.07134627d, 0.19857132d)]
    [InlineData(0.1d, 0.5d, 0.9d, 0.5986687d, -0.04975632d, -0.17012894d)]
    [InlineData(0.5d, 0.5d, 0.5d, 0.5981813d, 0.0000069753983d, 0.000022158503d)]
    [InlineData(1d, 1d, 1d, 1.000001d, 0.000011653032d, 0.000037040983d)]
    [InlineData(0d, 0d, 0d, 0d, 0d, 0d)]
    public void OklabMatchesKotlinGolden(double r, double g, double b,
        double expectedL, double expectedA, double expectedB)
    {
        var rgb = new NormalizedRgba(r, g, b, 0.75d);
        var lab = OklabColor.FromRgba(rgb);
        Close(expectedL, lab.L, 2e-5d);
        Close(expectedA, lab.A, 2e-5d);
        Close(expectedB, lab.B, 2e-5d);
        Close(.75d, lab.Alpha, 1e-12d);
        AssertRgbClose(rgb, lab.ToRgba(), 2e-4d);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(0.5d)]
    [InlineData(1d)]
    public void HsvGreysHaveDefinedHueInsteadOfOriginalNaN(double value)
    {
        var hsv = HsvColor.FromRgba(new NormalizedRgba(value, value, value));
        Assert.Equal(0d, hsv.H);
        Assert.Equal(0d, hsv.S);
        Close(value, hsv.ToRgba().R);
    }

    [Fact]
    public void HueNormalizationAndShortestInterpolationCrossSeam()
    {
        Assert.Equal(350d, new HsvColor(-10d, 1d, 1d).H);
        Assert.Equal(0d, new HsvColor(350d, 1d, 1d).Mix(new HsvColor(10d, 1d, 1d), .5d).H);
        Assert.Equal(0d, new HslColor(350d, 1d, .5d).Mix(new HslColor(10d, 1d, .5d), .5d).H);
    }

    [Fact]
    public void AlphaIsPreservedAcrossColorSpaces()
    {
        var color = new NormalizedRgba(.1d, .2d, .3d, .42d);
        Close(.42d, HsvColor.FromRgba(color).ToRgba().A);
        Close(.42d, HslColor.FromRgba(color).ToRgba().A);
        Close(.42d, OklabColor.FromRgba(color).ToRgba().A);
    }

    [Fact]
    public void Color32UsesExplicitRoundToNearest()
    {
        var color = new Color32(128, 255, 0, 64);
        Assert.Equal(color, NormalizedRgba.FromColor32(color).ToColor32());
    }

    [Fact]
    public void InvalidInputsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NormalizedRgba(double.NaN, 0d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NormalizedRgba(1.001d, 0d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HsvColor(double.PositiveInfinity, 1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HslColor(0d, -1d, .5d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OklabColor(double.NaN, 0d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HsvColor(20d, 1d, 1d).Mix(new HsvColor(0d, 1d, 1d), 2d));
    }

    private static void Close(double expected, double actual, double tolerance = 1e-7d) =>
        Assert.InRange(Math.Abs(actual - expected), 0d, tolerance);

    private static void AssertRgbClose(NormalizedRgba expected, NormalizedRgba actual, double tolerance = 1e-6d)
    {
        Close(expected.R, actual.R, tolerance);
        Close(expected.G, actual.G, tolerance);
        Close(expected.B, actual.B, tolerance);
        Close(expected.A, actual.A, tolerance);
    }
}
