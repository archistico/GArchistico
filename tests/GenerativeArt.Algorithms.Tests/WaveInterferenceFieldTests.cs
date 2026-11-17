using GenerativeArt.Algorithms.Interference;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class WaveInterferenceFieldTests
{
    [Theory]
    [InlineData("ring", "interference")]
    [InlineData("line", "caustics")]
    [InlineData("scatter", "ribbons")]
    public void SamplingIsBoundedDeterministicAndSensitiveToSeed(string layout, string mode)
    {
        var field = new WaveInterferenceField2D(42, sourceCount: 6, layout: layout,
            spread: 1.1, frequency: 13, distortion: .3, falloff: .7, mode: mode);
        var same = new WaveInterferenceField2D(42, sourceCount: 6, layout: layout,
            spread: 1.1, frequency: 13, distortion: .3, falloff: .7, mode: mode);
        var different = new WaveInterferenceField2D(43, sourceCount: 6, layout: layout,
            spread: 1.1, frequency: 13, distortion: .3, falloff: .7, mode: mode);
        Assert.Equal(6, field.SourceCount);
        bool seedChangesOutput = false;
        for (int iy = 0; iy < 11; iy++)
            for (int ix = 0; ix < 13; ix++)
            {
                double x = (ix - 6) / 11d, y = (iy - 5) / 11d;
                double value = field.Sample(x, y);
                Assert.InRange(value, 0, 1);
                Assert.Equal(value, same.Sample(x, y));
                if (Math.Abs(value - different.Sample(x, y)) > 1e-7)
                    seedChangesOutput = true;
            }
        Assert.True(seedChangesOutput);
    }

    [Fact]
    public void FrequencyAndGeometryAffectTheField()
    {
        var original = new WaveInterferenceField2D(42);
        var faster = new WaveInterferenceField2D(42, frequency: 20);
        var wider = new WaveInterferenceField2D(42, spread: 2);
        Assert.NotEqual(original.Sample(.17, -.12), faster.Sample(.17, -.12));
        Assert.NotEqual(original.Sample(.17, -.12), wider.Sample(.17, -.12));
    }

    [Fact]
    public void InvalidConfigurationsAndCoordinatesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaveInterferenceField2D(1, sourceCount: 13));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaveInterferenceField2D(1, frequency: double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaveInterferenceField2D(1, distortion: -1));
        Assert.Throws<ArgumentException>(() => new WaveInterferenceField2D(1, layout: "unknown"));
        Assert.Throws<ArgumentException>(() => new WaveInterferenceField2D(1, mode: "unknown"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaveInterferenceField2D(1).Sample(double.PositiveInfinity, 0));
    }
}
