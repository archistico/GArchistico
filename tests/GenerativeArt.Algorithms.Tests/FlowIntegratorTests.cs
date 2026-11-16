using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class FlowIntegratorTests
{
    [Theory]
    [InlineData(FlowIntegrationMethod.Euler)]
    [InlineData(FlowIntegrationMethod.Rk2)]
    [InlineData(FlowIntegrationMethod.Rk4)]
    public void NormalizedDirectionAlwaysUsesStepLength(FlowIntegrationMethod method)
    {
        var field = VectorFields.Constant(new Vector2(6, 8));
        Assert.True(FlowIntegrator.TryAdvance(field, new Vector2(1, 2), 5f,
            method, out Vector2 result));
        Assert.InRange(Vector2.Distance(result, new Vector2(4, 6)), 0f, 1e-5f);
    }

    [Theory]
    [InlineData(FlowIntegrationMethod.Euler)]
    [InlineData(FlowIntegrationMethod.Rk2)]
    [InlineData(FlowIntegrationMethod.Rk4)]
    public void NegativeStepWalksAgainstField(FlowIntegrationMethod method)
    {
        Assert.True(FlowIntegrator.TryAdvance(VectorFields.Constant(Vector2.UnitX),
            new Vector2(5, 2), -2, method, out Vector2 result));
        Assert.Equal(new Vector2(3, 2), result);
    }

    [Theory]
    [InlineData(FlowIntegrationMethod.Euler, 1.5)]
    [InlineData(FlowIntegrationMethod.Rk2, 1.625)]
    [InlineData(FlowIntegrationMethod.Rk4, 1.6484375)]
    public void VelocityIntegrationHasExpectedTaylorSteps(FlowIntegrationMethod method, double expected)
    {
        var field = VectorFields.FromFunction((x, _) => new Vector2(x, 0));
        Assert.True(FlowIntegrator.TryAdvance(field, Vector2.UnitX, .5f,
            method, out Vector2 result, velocity: true));
        Assert.InRange(Math.Abs(result.X - expected), 0d, 1e-6d);
    }

    [Fact]
    public void EulerUsesOneSampleRk2TwoRk4Four()
    {
        foreach (var item in new[] { (FlowIntegrationMethod.Euler, 1),
                      (FlowIntegrationMethod.Rk2, 2), (FlowIntegrationMethod.Rk4, 4) })
        {
            int calls = 0;
            var f = VectorFields.FromFunction((_, _) => { calls++; return Vector2.UnitX; });
            Assert.True(FlowIntegrator.TryAdvance(f, Vector2.Zero, 1, item.Item1, out _));
            Assert.Equal(item.Item2, calls);
        }
    }

    [Fact]
    public void NullZeroAndNonFiniteFieldsStopWithoutChangingResult()
    {
        foreach (Vector2 direction in new[] { Vector2.Zero, new Vector2(float.NaN, 2),
                      new Vector2(1, float.PositiveInfinity), new Vector2(1e-8f, 0) })
        {
            var field = VectorFields.FromFunction((_, _) => direction);
            Assert.False(FlowIntegrator.TryAdvance(field, new Vector2(10, 20), 1,
                FlowIntegrationMethod.Rk4, out Vector2 next));
            Assert.Equal(new Vector2(10, 20), next);
        }
    }

    [Fact]
    public void OverflowingVelocityStepStopsRatherThanReturningInfinity()
    {
        var field = VectorFields.Constant(new Vector2(float.MaxValue, 0));
        Assert.False(FlowIntegrator.TryAdvance(field, Vector2.Zero, float.MaxValue,
            FlowIntegrationMethod.Rk4, out _, velocity: true));
    }

    [Fact]
    public void InvalidInputsAreRejected()
    {
        var field = VectorFields.Constant(Vector2.UnitX);
        Assert.Throws<ArgumentNullException>(() => FlowIntegrator.TryAdvance(null!, Vector2.Zero, 1,
            FlowIntegrationMethod.Euler, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => FlowIntegrator.TryAdvance(field, Vector2.Zero, 0,
            FlowIntegrationMethod.Euler, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => FlowIntegrator.TryAdvance(field, Vector2.Zero, 1,
            (FlowIntegrationMethod)99, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => FlowIntegrator.TryAdvance(field, new Vector2(float.NaN, 0), 1,
            FlowIntegrationMethod.Euler, out _));
    }
}
