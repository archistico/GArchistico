using GenerativeArt.Core.Randomness;
using Xunit;

namespace GenerativeArt.Core.Tests;

public sealed class RandomTests
{
    [Theory]
    [InlineData(0UL, 0xE220A8397B1DCDAFUL)]
    [InlineData(1UL, 0x910A2DEC89025CC1UL)]
    [InlineData(42UL, 0xBDD732262FEB6E95UL)]
    [InlineData(ulong.MaxValue, 0xE4D971771B652C20UL)]
    public void FirstValueIsStable(ulong seed, ulong first)
    {
        Assert.Equal(SplitMix64Random.AlgorithmId, "splitmix64-v1");
        Assert.Equal(first, new SplitMix64Random(seed).NextUInt64());
    }

    [Fact]
    public void FiveValuesOfSeed42AreGolden()
    {
        ulong[] expected =
        [
            0xBDD732262FEB6E95UL, 0x28EFE333B266F103UL,
            0x47526757130F9F52UL, 0x581CE1FF0E4AE394UL,
            0x09BC585A244823F2UL
        ];
        var rng = new SplitMix64Random(42);
        foreach (ulong value in expected) Assert.Equal(value, rng.NextUInt64());
    }

    [Fact]
    public void FullInt32DomainNeverOverflowsAndIsReproducible()
    {
        var left = new SplitMix64Random(777);
        var right = new SplitMix64Random(777);
        for (int i = 0; i < 4_096; i++)
            Assert.Equal(left.NextInt32Inclusive(int.MinValue, int.MaxValue),
                right.NextInt32Inclusive(int.MinValue, int.MaxValue));
    }

    [Fact]
    public void DegenerateRangeConsumesNoRandomData()
    {
        var a = new SplitMix64Random(12);
        var b = new SplitMix64Random(12);
        Assert.Equal(10, a.NextInt32Inclusive(10, 10));
        Assert.Equal(b.NextUInt64(), a.NextUInt64());
    }

    [Fact]
    public void InvalidRangeIsRejected()
    {
        var random = new SplitMix64Random(12);
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt32Inclusive(10, 9));
    }

    [Fact]
    public void InclusiveRangeIncludesBothEnds()
    {
        var random = new SplitMix64Random(3);
        var seen = new HashSet<int>();
        for (int i = 0; i < 2_000; i++) seen.Add(random.NextInt32Inclusive(3, 6));
        Assert.True(seen.SetEquals([3, 4, 5, 6]));
    }

    [Fact]
    public void ForkDoesNotDependOnParentConsumption()
    {
        var a = new SplitMix64Random(99);
        var b = new SplitMix64Random(99);
        for (int i = 0; i < 1_000; i++) a.NextUInt64();
        var aChild = a.Fork("tracciato-1");
        var bChild = b.Fork("tracciato-1");
        for (int i = 0; i < 32; i++) Assert.Equal(aChild.NextUInt64(), bChild.NextUInt64());
    }

    [Fact]
    public void DistinctAndUnicodeStreamIdsGetDifferentDraws()
    {
        var rng = new SplitMix64Random(88);
        Assert.NotEqual(rng.Fork("onda").NextUInt64(), rng.Fork("ónda").NextUInt64());
        Assert.NotEqual(rng.Fork("onda").NextUInt64(), rng.Fork("onda2").NextUInt64());
        Assert.Throws<ArgumentException>(() => rng.Fork(""));
    }

    [Fact]
    public void NormalizedRandomValuesAreInHalfOpenUnitInterval()
    {
        var random = new SplitMix64Random(123);
        for (int i = 0; i < 8_000; i++)
        {
            Assert.InRange(random.NextDouble(), 0d, 0.9999999999999999d);
            Assert.InRange(random.NextFloat(), 0f, 0.99999994f);
        }
    }
}
