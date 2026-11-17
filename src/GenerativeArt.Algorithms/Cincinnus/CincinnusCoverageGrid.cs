using System.Numerics;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Cincinnus;

/// <summary>
/// Griglia di copertura raster-independent. Un tratto incrementa ogni cella
/// al più una volta, rispettando secchezza del pigmento, testata e assottigliamento
/// della coda, sul modello della classe Cover di Cincinnus.kt.
/// </summary>
public sealed class CincinnusCoverageGrid
{
    private readonly int[] _counts;
    private readonly int[] _seen;
    private int _stamp;
    private int _bare;
    private readonly float _cell;
    private readonly int _widthCells;
    private readonly int _heightCells;
    private readonly int _width;
    private readonly int _height;

    public CincinnusCoverageGrid(int width, int height, float cell, int maximumCells = 150_000)
    {
        if (width is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(width));
        if (height is < 1 or > 32_768) throw new ArgumentOutOfRangeException(nameof(height));
        if (!float.IsFinite(cell) || cell <= 0) throw new ArgumentOutOfRangeException(nameof(cell));
        if (maximumCells is < 1 or > 1_000_000) throw new ArgumentOutOfRangeException(nameof(maximumCells));
        long columns = (long)Math.Floor((double)width / cell) + 1;
        long rows = (long)Math.Floor((double)height / cell) + 1;
        if (columns * rows > maximumCells) throw new ArgumentOutOfRangeException(nameof(cell));
        _width = width; _height = height; _cell = cell;
        _widthCells = (int)columns; _heightCells = (int)rows;
        _counts = new int[(int)(columns * rows)];
        _seen = new int[_counts.Length];
        _bare = _counts.Length;
    }

    public int CellCount => _counts.Length;
    public int UncoveredCellCount => _bare;
    public int GetCount(int x, int y) => _counts[checked(y * _widthCells + x)];

    /// <summary>Seleziona uniformemente una delle celle con conteggio minimo, senza allocare un elenco.</summary>
    public Vector2 Pick(IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);
        int min = _counts.Min();
        int tied = 0;
        int choice = 0;
        for (int i = 0; i < _counts.Length; i++)
        {
            if (_counts[i] != min) continue;
            tied++;
            // Reservoir sampling esatto: ogni cella minima ha probabilità 1/tied.
            if (random.NextInt32Inclusive(1, tied) == 1) choice = i;
        }
        float x = (choice % _widthCells + random.NextFloat() - .5f) * _cell;
        float y = (choice / _widthCells + random.NextFloat() - .5f) * _cell;
        return new Vector2(Math.Clamp(x, 0f, _width), Math.Clamp(y, 0f, _height));
    }

    public void Mark(Vector2 point)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) throw new ArgumentOutOfRangeException(nameof(point));
        int col = Math.Clamp((int)MathF.Round(point.X / _cell), 0, _widthCells - 1);
        int row = Math.Clamp((int)MathF.Round(point.Y / _cell), 0, _heightCells - 1);
        Increment(row * _widthCells + col);
    }

    public void Add(CincinnusBrushStroke stroke, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stroke);
        cancellationToken.ThrowIfCancellationRequested();
        if (_stamp == int.MaxValue) { Array.Clear(_seen); _stamp = 0; }
        _stamp++;
        IReadOnlyList<CincinnusSpineSample> spine = stroke.Spine;
        float length = stroke.Length;
        for (int k = 0; k < spine.Count; k += 3)
        {
            if ((k & 63) == 0) cancellationToken.ThrowIfCancellationRequested();
            CincinnusSpineSample sample = spine[k];
            float from = sample.Arc;
            float end = Math.Clamp((length - from - 50f) / 150f, 0f, 1f);
            float gone = 1f - stroke.Load(from / Math.Max(length, 1f));
            float dry = SmoothStep(.6f, .2f, gone);
            float radius = sample.HalfWidth * MathF.Sqrt(Math.Max(0f, end * dry));
            if (radius < 1f) continue;
            float unit = sample.HalfWidth * MathF.Sqrt(Math.Max(0f, dry));
            if (unit <= 1e-6f) continue;
            Vector2 p = sample.Position;
            Vector2 normal = sample.Normal;
            float along = 1.5f * Math.Max(1f, k == 0 && spine.Count > 1 ? spine[1].Arc :
                k > 0 ? sample.Arc - spine[k - 1].Arc : 1f) + 1f;
            float extent = Math.Max(radius, along);
            int col0 = Math.Max(0, (int)Math.Ceiling((p.X - extent) / _cell));
            int col1 = Math.Min(_widthCells - 1, (int)Math.Floor((p.X + extent) / _cell));
            int row0 = Math.Max(0, (int)Math.Ceiling((p.Y - extent) / _cell));
            int row1 = Math.Min(_heightCells - 1, (int)Math.Floor((p.Y + extent) / _cell));
            for (int y = row0; y <= row1; y++)
                for (int x = col0; x <= col1; x++)
                {
                    float dx = x * _cell - p.X;
                    float dy = y * _cell - p.Y;
                    float off = dx * normal.X + dy * normal.Y;
                    if (MathF.Abs(off) > radius || MathF.Abs(dx * normal.Y - dy * normal.X) > along ||
                        stroke.LandingOffset(off / unit) > from) continue;
                    int index = y * _widthCells + x;
                    if (_seen[index] == _stamp) continue;
                    _seen[index] = _stamp;
                    Increment(index);
                }
        }
    }

    private void Increment(int index)
    {
        if (_counts[index] == 0) _bare--;
        if (_counts[index] != int.MaxValue) _counts[index]++;
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}
