using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class LineIntegralConvolutionTests
{
    [Fact]
    public void ConstantTextureIsPreservedIncludingTruncatedBorders()
    {
        float[] texture = Enumerable.Repeat(.37f, 31 * 23).ToArray();
        var field = VectorFields.Vortex(new Vector2(15, 11));
        Assert.All(LineIntegralConvolution.Apply(31, 23, texture, field), v => Assert.Equal(.37f, v, 6));
    }

    [Fact]
    public void HorizontalFieldAveragesOnlyWithinRowsAndIsSignSymmetric()
    {
        float[] texture = [0, 0, 1, 0, 0, 1, 1, 0, 1, 1];
        float[] before = texture.ToArray();
        var forward = LineIntegralConvolution.Apply(5, 2, texture, VectorFields.Constant(Vector2.UnitX), 1);
        var backward = LineIntegralConvolution.Apply(5, 2, texture, VectorFields.Constant(-Vector2.UnitX), 1);
        Assert.Equal(.5f, forward[2], 6);
        Assert.Equal(.5f, forward[7], 6);
        Assert.Equal(0, forward[0]);
        Assert.Equal(1, forward[5]);
        Assert.Equal(forward, backward);
        Assert.Equal(before, texture);
    }

    [Fact]
    public void ZeroFieldAndZeroStepsReturnOriginalTexture()
    {
        float[] texture = [0, .2f, .7f, 1];
        Assert.Equal(texture, LineIntegralConvolution.Apply(2, 2, texture, VectorFields.Constant(Vector2.Zero)));
        Assert.Equal(texture, LineIntegralConvolution.Apply(2, 2, texture, VectorFields.Constant(Vector2.One), 0));
    }

    [Fact]
    public void BudgetAndInputsAreCheckedBeforeSamplingAndCancellationIsHonored()
    {
        var field = VectorFields.Constant(Vector2.UnitX);
        Assert.Throws<ArgumentOutOfRangeException>(() => LineIntegralConvolution.ValidateBudget(4096, 4096, 48));
        LineIntegralConvolution.ValidateBudget(1920, 1080, 48);
        Assert.Throws<ArgumentException>(() => LineIntegralConvolution.Apply(2, 2, [0, 1], field));
        Assert.Throws<ArgumentException>(() => LineIntegralConvolution.Apply(2, 2, [0, 1, float.NaN, 0], field));
        Assert.Throws<ArgumentOutOfRangeException>(() => LineIntegralConvolution.Apply(1, 1, [0], field, stepSize: 0));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            LineIntegralConvolution.Apply(1, 1, [0], field, cancellationToken: cancellation.Token));
    }
}
