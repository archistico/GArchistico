using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Pattern;

/// <summary>
/// Primitive deterministiche per le grammatiche di tasselli G1.
/// Gli indici e i sottostream sono indipendenti dalla GUI e dalla risoluzione raster.
/// </summary>
public static class ModularTileGrammar
{
    public readonly record struct Cell(int Column, int Row, int Shape, int Rotation, int Color,
        double OffsetX = 0, double OffsetY = 0);

    public static IReadOnlyList<Cell> CreateCotton(ulong seed, int columns, int rows, int paletteSize = 4)
    {
        ValidateGrid(columns, rows);
        if (paletteSize is < 2 or > 16) throw new ArgumentOutOfRangeException(nameof(paletteSize));
        var random = new SplitMix64Random(seed);
        var cells = new List<Cell>(columns * rows);
        for (int row = 0; row < rows; row++)
            for (int col = 0; col < columns; col++)
                cells.Add(new Cell(col, row, random.NextInt32Inclusive(0, 2),
                    random.NextInt32Inclusive(0, 3), random.NextInt32Inclusive(0, paletteSize - 1)));
        return cells;
    }

    /// <summary>Blocchetto periodico a due colonne; caratteri e distorsioni rimangono nel renderer.</summary>
    public static IReadOnlyList<Cell> CreateRigorBlock(ulong seed, int period, double accentRate = .12)
    {
        if (period is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(period));
        if (!double.IsFinite(accentRate) || accentRate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(accentRate));
        var random = new SplitMix64Random(seed);
        var cells = new List<Cell>(period * 2);
        for (int row = 0; row < period; row++)
            for (int col = 0; col < 2; col++)
                cells.Add(new Cell(col, row, random.NextInt32Inclusive(0, 4),
                    random.NextInt32Inclusive(0, 3), random.NextDouble() < accentRate ? 1 : 0));
        return cells;
    }

    /// <summary>
    /// Glifi astratti costruiti su una sottogriglia. Gli indici 1 e 2 estendono
    /// rettangoli orizzontali/verticali, mantenendo l'occupazione deterministica.
    /// </summary>
    public static IReadOnlyList<Cell> CreateAlienGlyph(ulong seed, int divisions, double density, double joinProbability)
    {
        if (divisions is < 2 or > 10) throw new ArgumentOutOfRangeException(nameof(divisions));
        if (!double.IsFinite(density) || density is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(density));
        if (!double.IsFinite(joinProbability) || joinProbability is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(joinProbability));
        var random = new SplitMix64Random(seed);
        var cells = new List<Cell>(divisions * divisions);
        for (int row = 0; row < divisions; row++)
            for (int col = 0; col < divisions; col++)
            {
                if (random.NextDouble() >= density) continue;
                int shape = random.NextDouble() < joinProbability
                    ? random.NextInt32Inclusive(1, 2) : 0;
                cells.Add(new Cell(col, row, shape, 0, random.NextInt32Inclusive(0, 3)));
            }
        return cells;
    }

    private static void ValidateGrid(int columns, int rows)
    {
        if (columns is < 1 or > 64) throw new ArgumentOutOfRangeException(nameof(columns));
        if (rows is < 1 or > 64) throw new ArgumentOutOfRangeException(nameof(rows));
    }
}
