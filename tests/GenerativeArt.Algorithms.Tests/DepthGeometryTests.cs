using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class DepthGeometryTests
{
    [Fact]
    public void ApertureIsClosedSeededAndHasNoSeamJump()
    {
        Vector2[] a = new OrganicAperture(42).Build(new Vector2(30, 40), 50, 1000);
        Assert.Equal(a[0], a[^1]);
        Assert.Equal(a, new OrganicAperture(42).Build(new Vector2(30, 40), 50, 1000));
        Assert.NotEqual(a[32], new OrganicAperture(43).Build(new Vector2(30, 40), 50, 1000)[32]);
        Vector2 before = a[^1] - a[^2], after = a[1] - a[0];
        Assert.True(Vector2.Dot(Vector2.Normalize(before), Vector2.Normalize(after)) > .99f);
        Assert.All(a, p => Assert.InRange(Vector2.Distance(p, new Vector2(30, 40)), 35, 65));
    }

    [Fact]
    public void NeutralApertureIsAnExactEllipseAndRotationPreservesRadius()
    {
        var generator = new OrganicAperture(1);
        Vector2[] ellipse = generator.Build(Vector2.Zero, 20, 64, ripple: 0, roughness: 0, stretch: 2);
        Assert.Equal(new Vector2(40, 0), ellipse[0]);
        Assert.InRange(ellipse[16].Y, 19.999f, 20.001f);
        Vector2[] circle = generator.Build(Vector2.Zero, 20, 64, ripple: 0, roughness: 0, rotation: 1.7);
        Assert.All(circle, p => Assert.InRange(p.Length(), 19.999f, 20.001f));
    }

    [Theory]
    [InlineData(DepthParticleLayout.Cloud)]
    [InlineData(DepthParticleLayout.Spiral)]
    [InlineData(DepthParticleLayout.Flow)]
    public void ParticlesAreSortedBoundedAndReproducible(DepthParticleLayout layout)
    {
        var field = DepthParticleField.Build(42, 500, layout);
        Assert.Equal(field, DepthParticleField.Build(42, 500, layout));
        Assert.NotEqual(field[0], DepthParticleField.Build(43, 500, layout)[0]);
        Assert.Equal(field.Select(p => p.Depth).OrderBy(d => d), field.Select(p => p.Depth));
        Assert.All(field, p =>
        {
            Assert.InRange(p.Depth, 0, 1); Assert.InRange(p.Size, .45f, 1.35f);
            Assert.InRange(p.Brightness, .65f, 1);
            Assert.True(float.IsFinite(p.Position.X) && float.IsFinite(p.Position.Y));
        });
    }

    [Fact]
    public void GeometryRejectsInvalidInputsAndObservesCancellation()
    {
        var contour = new OrganicAperture(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => contour.Build(Vector2.Zero, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => contour.Build(Vector2.Zero, 10, samples: 5000));
        Assert.Throws<ArgumentOutOfRangeException>(() => contour.Build(Vector2.Zero, 10, roughness: double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => contour.Build(Vector2.Zero, 10, depth: 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => DepthParticleField.Build(1, 4001));
        Assert.Throws<ArgumentOutOfRangeException>(() => DepthParticleField.Build(1, layout: (DepthParticleLayout)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => DepthParticleField.Build(1, spread: double.PositiveInfinity));
        using var cts = new CancellationTokenSource(); cts.Cancel();
        Assert.Throws<OperationCanceledException>(() => contour.Build(Vector2.Zero, 10, cancellationToken: cts.Token));
        Assert.Throws<OperationCanceledException>(() => DepthParticleField.Build(1, cancellationToken: cts.Token));
    }
}
