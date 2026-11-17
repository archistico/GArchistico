using GenerativeArt.Algorithms.Pattern;
using Xunit;

namespace GenerativeArt.Algorithms.Tests;

public sealed class ModularTileGrammarTests
{
    [Fact]
    public void AlienGlyphIsDeterministicAndJoinProbabilityChangesGeometry()
    {
        var first = ModularTileGrammar.CreateAlienGlyph(42UL, 7, .8, .4);
        var second = ModularTileGrammar.CreateAlienGlyph(42UL, 7, .8, .4);
        Assert.True(first.SequenceEqual(second));
        Assert.NotEmpty(first);
        Assert.All(first, cell =>
        {
            Assert.InRange(cell.Column, 0, 6);
            Assert.InRange(cell.Row, 0, 6);
            Assert.InRange(cell.Shape, 0, 2);
            Assert.InRange(cell.Color, 0, 3);
        });
        Assert.False(first.SequenceEqual(ModularTileGrammar.CreateAlienGlyph(42UL, 7, .8, 1)));
        Assert.False(first.SequenceEqual(ModularTileGrammar.CreateAlienGlyph(43UL, 7, .8, .4)));
    }

    [Fact]
    public void CottonCellsCoverTheGridExactlyOnce()
    {
        var tiles = ModularTileGrammar.CreateCotton(2718UL, 14, 9);
        Assert.Equal(126, tiles.Count);
        Assert.Equal(126, tiles.Select(t => (t.Column, t.Row)).Distinct().Count());
        Assert.All(tiles, tile => Assert.InRange(tile.Rotation, 0, 3));
        Assert.True(tiles.SequenceEqual(ModularTileGrammar.CreateCotton(2718UL, 14, 9)));
        Assert.False(tiles.SequenceEqual(ModularTileGrammar.CreateCotton(2719UL, 14, 9)));
    }

    [Fact]
    public void RigorBlockHasExactlyTwoColumnsAndRequestedPeriod()
    {
        var block = ModularTileGrammar.CreateRigorBlock(14, 5);
        Assert.Equal(10, block.Count);
        Assert.All(block, tile => Assert.InRange(tile.Shape, 0, 4));
        Assert.True(block.SequenceEqual(ModularTileGrammar.CreateRigorBlock(14, 5)));
        Assert.False(block.SequenceEqual(ModularTileGrammar.CreateRigorBlock(15, 5)));
        Assert.All(ModularTileGrammar.CreateRigorBlock(14, 5, 0), tile => Assert.Equal(0, tile.Color));
        Assert.All(ModularTileGrammar.CreateRigorBlock(14, 5, 1), tile => Assert.Equal(1, tile.Color));
    }

    [Fact]
    public void OutOfBudgetGeometryIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ModularTileGrammar.CreateAlienGlyph(0, 11, .5, .5));
        Assert.Throws<ArgumentOutOfRangeException>(() => ModularTileGrammar.CreateAlienGlyph(0, 5, -1, .5));
        Assert.Throws<ArgumentOutOfRangeException>(() => ModularTileGrammar.CreateAlienGlyph(0, 5, .5, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => ModularTileGrammar.CreateCotton(0, 65, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => ModularTileGrammar.CreateRigorBlock(0, 9));
    }
}
