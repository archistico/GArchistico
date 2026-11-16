using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class FlowFieldsTests
{
    [Fact]
    public void ConstantSamplesAreStable()
    {
        var field = VectorFields.Constant(new Vector2(3, -4));
        Assert.Equal(new Vector2(3, -4), field.Sample(-100, 73));
        Assert.Equal(field.Sample(0, 0), field.Sample(123, 456));
    }

    [Fact]
    public void AngleZeroPointsRightAndQuarterTurnPointsDown()
    {
        var right = VectorFields.Angles((_, _) => 0f).Sample(0, 0);
        var down = VectorFields.Angles((_, _) => MathF.PI / 2).Sample(0, 0);
        Assert.InRange(Math.Abs(right.X - 1), 0f, 1e-6f);
        Assert.InRange(Math.Abs(right.Y), 0f, 1e-6f);
        Assert.InRange(Math.Abs(down.X), 0f, 1e-6f);
        Assert.InRange(Math.Abs(down.Y - 1), 0f, 1e-6f);
    }

    [Fact]
    public void PositiveSpinRotatesClockwiseInCanvasCoordinates()
    {
        var field = VectorFields.Vortex(new Vector2(4, 5));
        Assert.Equal(Vector2.Zero, field.Sample(4, 5));
        Vector2 east = field.Sample(5, 5);
        Assert.InRange(Math.Abs(east.X), 0f, 1e-6f);
        Assert.InRange(Math.Abs(east.Y - 1), 0f, 1e-6f);
    }

    [Fact]
    public void PositivePullPointsTowardVortexCenter()
    {
        Vector2 v = VectorFields.Vortex(Vector2.Zero, spin: 0, pull: 1).Sample(3, 4);
        Assert.InRange(Math.Abs(v.X + .6f), 0, 1e-6f);
        Assert.InRange(Math.Abs(v.Y + .8f), 0, 1e-6f);
    }

    [Fact]
    public void VortexReachAttenuatesFarSamples()
    {
        var near = VectorFields.Vortex(Vector2.Zero, reach: 2);
        Assert.True(near.Sample(4, 0).Length() <
                    VectorFields.Vortex(Vector2.Zero).Sample(4, 0).Length());
    }

    [Fact]
    public void ComposeAndScaleHaveConsistentResults()
    {
        var result = VectorFields.Scale(VectorFields.Add(
            VectorFields.Constant(new Vector2(2, 3)),
            VectorFields.Constant(new Vector2(1, -1))), 2);
        Assert.Equal(new Vector2(6, 4), result.Sample(12, 34));
    }

    [Fact]
    public void InvalidConstructionArgumentsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => VectorFields.Angles(null!));
        Assert.Throws<ArgumentNullException>(() => VectorFields.FromFunction(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => VectorFields.Constant(new Vector2(float.NaN, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => VectorFields.Vortex(Vector2.Zero, reach: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => VectorFields.Vortex(Vector2.Zero, spin: float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => VectorFields.Scale(VectorFields.Constant(Vector2.One), float.PositiveInfinity));
    }
}
