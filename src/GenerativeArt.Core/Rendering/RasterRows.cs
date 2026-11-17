namespace GenerativeArt.Core.Rendering;

/// <summary>Righe disgiunte, aritmetica locale invariata e parallelismo limitato; nessun disegno Skia concorrente.</summary>
public static class RasterRows
{
    public static void For(int start, int end, bool parallel, CancellationToken cancellationToken, Action<int> renderRow)
    {
        ArgumentNullException.ThrowIfNull(renderRow);
        if (start < 0 || end < start) throw new ArgumentOutOfRangeException(nameof(start));
        cancellationToken.ThrowIfCancellationRequested();
        if (!parallel || Environment.ProcessorCount == 1 || end - start < 8)
        {
            for (int y = start; y < end; y++) { cancellationToken.ThrowIfCancellationRequested(); renderRow(y); }
            return;
        }
        Parallel.For(start, end, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Clamp(Environment.ProcessorCount - 1, 1, 16)
        }, renderRow);
    }
}
