using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Rendering;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioParameterInputTests
{
    [Fact]
    public void TinyFrequencyRangesHaveUsefulFineStepsAndExactInputKeepsMorePrecision()
    {
        var spec = BuiltInArtworkCatalog.Create().Get("domain-warp").Parameters.Single(p => p.Id == "frequency");
        Assert.InRange(StudioParameterInput.NumericStep(spec), .0000001, .00001);
        Assert.True(StudioParameterInput.TryNormalize(spec, "0,0008123456789", out string value));
        Assert.Equal("0.0008123456789", value);
    }

    [Fact]
    public void IntegerStepsRemainWholeAndConstantRangesAreFinite()
    {
        Assert.Equal(1, StudioParameterInput.NumericStep(ArtworkParameterSpec.Integer("lines", "Linee", 500, 1, 10000)));
        Assert.True(double.IsFinite(StudioParameterInput.NumericStep(ArtworkParameterSpec.Number("fixed", "Fisso", .5, .5, .5))));
    }

    [Theory]
    [InlineData("0.0123456789012345", "0.0123456789012345")]
    [InlineData("0,0123456789012345", "0.0123456789012345")]
    [InlineData(" 0,25 ", "0.25")]
    public void AcceptsDecimalSeparatorWithoutDestroyingPrecision(string input, string expected)
    {
        var spec = ArtworkParameterSpec.Number("frequency", "Frequenza", 0.02, 0.001, 1);
        Assert.True(StudioParameterInput.TryNormalize(spec, input, out string canonical));
        Assert.Equal(expected, canonical);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("non valido")]
    [InlineData("0,1,1")]
    [InlineData("2")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void InvalidInputIsNeverCommitted(string input)
    {
        var spec = ArtworkParameterSpec.Number("frequency", "Frequenza", 0.02, 0.001, 1);
        Assert.False(StudioParameterInput.TryNormalize(spec, input, out string canonical));
        Assert.Equal(string.Empty, canonical);
    }

    [Fact]
    public void IntegersDoNotAcceptDecimalCommas()
    {
        var spec = ArtworkParameterSpec.Integer("octaves", "Ottave", 3, 1, 8);
        Assert.False(StudioParameterInput.TryNormalize(spec, "3,5", out _));
        Assert.True(StudioParameterInput.TryNormalize(spec, " 7 ", out string canonical));
        Assert.Equal("7", canonical);
    }
}
