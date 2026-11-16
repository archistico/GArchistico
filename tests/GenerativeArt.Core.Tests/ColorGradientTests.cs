using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Colors.Gradients;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class ColorGradientTests
{
    private static readonly NormalizedRgba Black = new(0, 0, 0);
    private static readonly NormalizedRgba White = new(1, 1, 1);

    [Fact]
    public void MidpointDiffersByInterpolationSpace()
    {
        var srgb = new ColorGradient(GradientInterpolationSpace.Srgb,
            new ColorStop(0, Black), new ColorStop(1, White));
        var linear = new ColorGradient(GradientInterpolationSpace.LinearSrgb,
            new ColorStop(0, Black), new ColorStop(1, White));

        Assert.Equal(.5d, srgb.Sample(.5d).R);
        Assert.InRange(Math.Abs(linear.Sample(.5d).R - .7353569830524495d), 0d, 1e-12d);
    }

    [Fact]
    public void ExactStopsAndOutOfRangeSamplesAreClamped()
    {
        var red = new NormalizedRgba(1d, 0d, 0d);
        var green = new NormalizedRgba(0d, 1d, 0d);
        var blue = new NormalizedRgba(0d, 0d, 1d);
        var gradient = new ColorGradient(GradientInterpolationSpace.LinearSrgb,
            new ColorStop(.2d, red), new ColorStop(.6d, green), new ColorStop(.9d, blue));
        Assert.Equal(red, gradient.Sample(-100));
        Assert.Equal(red, gradient.Sample(.2d));
        Assert.Equal(green, gradient.Sample(.6d));
        Assert.Equal(blue, gradient.Sample(.9d));
        Assert.Equal(blue, gradient.Sample(3d));
    }

    [Fact]
    public void MultipleSegmentsUseTheirLocalNormalizedParameter()
    {
        var gradient = new ColorGradient(GradientInterpolationSpace.Srgb,
            new ColorStop(0d, Black),
            new ColorStop(.25d, White),
            new ColorStop(1d, Black));
        Assert.Equal(new NormalizedRgba(.5d, .5d, .5d), gradient.Sample(.125d));
        Assert.Equal(new NormalizedRgba(.5d, .5d, .5d), gradient.Sample(.625d));
    }

    [Fact]
    public void AlphaUsesStraightInterpolationInBothSpaces()
    {
        var transparent = new NormalizedRgba(1d, 0d, 0d, 0d);
        var opaque = new NormalizedRgba(0d, 0d, 1d, 1d);
        foreach (GradientInterpolationSpace space in new[]
                 { GradientInterpolationSpace.Srgb, GradientInterpolationSpace.LinearSrgb })
        {
            var gradient = new ColorGradient(space,
                new ColorStop(0d, transparent), new ColorStop(1d, opaque));
            Assert.Equal(.5d, gradient.Sample(.5d).A);
        }
    }

    [Fact]
    public void ConstructorDefensivelyCopiesStops()
    {
        ColorStop[] stops = [new(0d, Black), new(1d, White)];
        var gradient = new ColorGradient(GradientInterpolationSpace.Srgb, stops);
        stops[0] = new ColorStop(0d, White);
        Assert.Equal(Black, gradient.Sample(0d));
        Assert.Equal(2, gradient.Stops.Count);
        Assert.Throws<NotSupportedException>(() => ((IList<ColorStop>)gradient.Stops)[0] = new ColorStop(0d, White));
    }

    [Fact]
    public void RejectsDuplicateUnsortedOrMissingStops()
    {
        Assert.Throws<ArgumentException>(() => new ColorGradient(GradientInterpolationSpace.Srgb));
        Assert.Throws<ArgumentException>(() => new ColorGradient(GradientInterpolationSpace.Srgb, new ColorStop(0d, Black)));
        Assert.Throws<ArgumentException>(() => new ColorGradient(GradientInterpolationSpace.Srgb,
            new ColorStop(.4d, Black), new ColorStop(.4d, White)));
        Assert.Throws<ArgumentException>(() => new ColorGradient(GradientInterpolationSpace.Srgb,
            new ColorStop(.8d, Black), new ColorStop(.2d, White)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ColorStop(double.NaN, Black));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ColorGradient((GradientInterpolationSpace)100,
            new ColorStop(0d, Black), new ColorStop(1d, White)));
    }

    [Fact]
    public void RejectsNonFiniteSamplingPosition()
    {
        var gradient = new ColorGradient(GradientInterpolationSpace.Srgb,
            new ColorStop(0, Black), new ColorStop(1, White));
        Assert.Throws<ArgumentOutOfRangeException>(() => gradient.Sample(double.PositiveInfinity));
    }
}
