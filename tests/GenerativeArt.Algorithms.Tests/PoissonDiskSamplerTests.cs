using System.Numerics;
using GenerativeArt.Algorithms.Sampling;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class PoissonDiskSamplerTests
{
    [Fact]
    public void SamplesAreDeterministicBoundedAndRespectMinimumDistance()
    {
        const float width = 256, height = 192, minimumDistance = 15;
        IReadOnlyList<Vector2> first = PoissonDiskSampler.Sample(width, height, minimumDistance, 42, 220);
        IReadOnlyList<Vector2> again = PoissonDiskSampler.Sample(width, height, minimumDistance, 42, 220);
        IReadOnlyList<Vector2> otherSeed = PoissonDiskSampler.Sample(width, height, minimumDistance, 43, 220);
        Assert.Equal(first.ToArray(), again.ToArray());
        Assert.NotEmpty(first);
        Assert.True(first.Count <= 220);
        Assert.False(first.SequenceEqual(otherSeed));
        foreach (Vector2 point in first)
        {
            Assert.InRange(point.X, 0f, width);
            Assert.InRange(point.Y, 0f, height);
            Assert.True(point.X < width && point.Y < height);
        }
        for (int i = 0; i < first.Count; i++)
            for (int j = i + 1; j < first.Count; j++)
                Assert.True(Vector2.DistanceSquared(first[i], first[j]) >= minimumDistance * minimumDistance - .001f,
                    $"I punti {i} e {j} sono troppo vicini.");
    }

    [Fact]
    public void MaximumPointsAndCancellationAreHonored()
    {
        Assert.Single(PoissonDiskSampler.Sample(300, 250, 10, 5, maximumPoints: 1));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            PoissonDiskSampler.Sample(300, 250, 10, 5, cancellationToken: cts.Token));
    }

    [Fact]
    public void InvalidInputAndOversizedGridAreRejectedBeforeAllocation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PoissonDiskSampler.Validate(0, 80, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => PoissonDiskSampler.Validate(100, 80, float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => PoissonDiskSampler.Validate(100, 80, 0.001f));
        Assert.Throws<ArgumentOutOfRangeException>(() => PoissonDiskSampler.Validate(100, 80, 4, 50_001));
        Assert.Throws<ArgumentOutOfRangeException>(() => PoissonDiskSampler.Validate(100, 80, 4, candidatesPerActive: 0));
        PoissonDiskSampler.Validate(1920, 1080, 1.08f, maximumPoints: 50_000);
    }
}
