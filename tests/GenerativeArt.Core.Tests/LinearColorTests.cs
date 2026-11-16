using GenerativeArt.Core.Colors;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class LinearColorTests
{
    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(0.04045d, 0.0031308049535603713d)]
    [InlineData(0.5d, 0.21404114048223255d)]
    [InlineData(1d, 1d)]
    public void SrgbLinearizationHasKnownStandardSamples(double srgb, double expected)
    {
        Assert.InRange(Math.Abs(SrgbTransfer.ToLinear(srgb) - expected), 0d, 1e-12d);
    }

    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(0.5d, 0.7353569830524495d)]
    [InlineData(1d, 1d)]
    public void CompandingHasKnownStandardSamples(double linear, double expected)
    {
        Assert.InRange(Math.Abs(SrgbTransfer.ToSrgb(linear) - expected), 0d, 1e-12d);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(0.003d)]
    [InlineData(0.040449d)]
    [InlineData(0.0404501d)]
    [InlineData(0.25d)]
    [InlineData(0.8d)]
    [InlineData(1d)]
    public void TransferFunctionsRoundTrip(double value)
    {
        Assert.InRange(Math.Abs(SrgbTransfer.ToSrgb(SrgbTransfer.ToLinear(value)) - value), 0d, 1e-12d);
    }

    [Fact]
    public void RoundedSrgbBreakpointsHaveDocumentedDiscontinuity()
    {
        // IEC 61966-2-1 uses rounded cutoffs: 0.04045 (sRGB) and 0.0031308 (linear).
        // These are not an exact inverse pair at the breakpoint; do not weaken other roundtrip tests.
        const double srgbBreakpoint = 0.04045d;
        double linear = SrgbTransfer.ToLinear(srgbBreakpoint);
        double encoded = SrgbTransfer.ToSrgb(linear);

        Assert.InRange(Math.Abs(linear - 0.0031308049535603713d), 0d, 1e-12d);
        Assert.InRange(Math.Abs(encoded - 0.040449970408122003d), 0d, 1e-12d);
        Assert.InRange(Math.Abs(SrgbTransfer.ToSrgb(0.0031308d) - 0.040449936d), 0d, 1e-12d);
    }

    [Fact]
    public void RgbAndAlphaRemainSeparate()
    {
        var encoded = new NormalizedRgba(.5d, .25d, .75d, .4d);
        LinearRgba linear = LinearRgba.FromSrgb(encoded);
        Assert.Equal(.4d, linear.A);
        NormalizedRgba back = linear.ToSrgb();
        Assert.Equal(.4d, back.A);
        Assert.InRange(Math.Abs(back.R - encoded.R), 0d, 1e-12d);
        Assert.InRange(Math.Abs(back.G - encoded.G), 0d, 1e-12d);
        Assert.InRange(Math.Abs(back.B - encoded.B), 0d, 1e-12d);
    }

    [Fact]
    public void LinearMixInterpolatesAlphaSeparately()
    {
        var a = new LinearRgba(0d, 0d, 0d, .2d);
        var b = new LinearRgba(1d, 1d, 1d, .8d);
        LinearRgba result = a.Mix(b, .5d);
        Assert.Equal(.5d, result.R);
        Assert.InRange(Math.Abs(result.A - .5d), 0d, 1e-12d);
    }

    [Fact]
    public void NonFiniteOrOutOfRangeColorsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SrgbTransfer.ToLinear(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => SrgbTransfer.ToLinear(-0.01d));
        Assert.Throws<ArgumentOutOfRangeException>(() => SrgbTransfer.ToSrgb(1.001d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LinearRgba(0d, double.PositiveInfinity, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LinearRgba(0d, 0d, 0d).Mix(new LinearRgba(1d, 1d, 1d), double.NaN));
    }
}
