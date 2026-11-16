using System.Numerics;

namespace GenerativeArt.Algorithms.Contours;

/// <summary>
/// Curve di livello da una griglia scalare row-major, con ambiguità 5/10 risolta
/// tramite il decider bilineare. Le intersezioni su archi condivisi vengono
/// calcolate nello stesso orientamento e raccordate senza coordinate arrotondate.
/// La funzione è deterministica, cancellabile e non conserva stato globale.
/// </summary>
public static class MarchingSquares
{
    private readonly record struct Segment(Vector2 A, Vector2 B);

    /// <param name="samples">Valori row-major, da sinistra a destra, dall'alto in basso.</param>
    /// <param name="columns">Numero di campioni per riga, almeno due.</param>
    /// <param name="rows">Numero di righe di campioni, almeno due.</param>
    /// <param name="level">Isovalore finito; i punti esattamente al livello sono considerati interni.</param>
    public static IReadOnlyList<ContourPolyline> Trace(ReadOnlySpan<float> samples,
        int columns, int rows, float level, CancellationToken cancellationToken = default)
    {
        if (columns < 2 || rows < 2 || (long)columns * rows > 4_194_304L)
            throw new ArgumentOutOfRangeException(nameof(columns), "Griglia: almeno 2x2 e massimo 4.194.304 campioni.");
        if (samples.Length != (long)columns * rows)
            throw new ArgumentException("La dimensione dei campioni non corrisponde alla griglia.", nameof(samples));
        if (!float.IsFinite(level))
            throw new ArgumentOutOfRangeException(nameof(level), "Isovalore non finito.");
        cancellationToken.ThrowIfCancellationRequested();
        foreach (float value in samples)
            if (!float.IsFinite(value))
                throw new ArgumentException("Il campo scalare contiene valori non finiti.", nameof(samples));

        var segments = new List<Segment>();
        for (int y = 0; y < rows - 1; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (int x = 0; x < columns - 1; x++)
            {
                // Angoli in senso orario: TL, TR, BR, BL.
                float tl = samples[y * columns + x];
                float tr = samples[y * columns + x + 1];
                float br = samples[(y + 1) * columns + x + 1];
                float bl = samples[(y + 1) * columns + x];
                int mask = (tl >= level ? 1 : 0) | (tr >= level ? 2 : 0) |
                           (br >= level ? 4 : 0) | (bl >= level ? 8 : 0);
                if (mask is 0 or 15) continue;

                // Un bordo viene sempre interpolato da sinistra a destra oppure
                // dall'alto in basso, per produrre esattamente lo stesso punto
                // dalle due celle adiacenti.
                Vector2 Edge(int edge) => edge switch
                {
                    0 => new Vector2(x + Fraction(tl, tr, level), y),
                    1 => new Vector2(x + 1, y + Fraction(tr, br, level)),
                    2 => new Vector2(x + Fraction(bl, br, level), y + 1),
                    3 => new Vector2(x, y + Fraction(tl, bl, level)),
                    _ => throw new ArgumentOutOfRangeException(nameof(edge))
                };
                void Add(int first, int second)
                {
                    Vector2 a = Edge(first), b = Edge(second);
                    // Attraversamenti degeneri di un vertice esattamente sul livello.
                    if (a != b) segments.Add(new Segment(a, b));
                }

                switch (mask)
                {
                    case 1: case 14: Add(3, 0); break;
                    case 2: case 13: Add(0, 1); break;
                    case 3: case 12: Add(3, 1); break;
                    case 4: case 11: Add(1, 2); break;
                    case 6: case 9: Add(0, 2); break;
                    case 7: case 8: Add(3, 2); break;
                    case 5: case 10:
                    {
                        // Segno del determinante del bilineare: risolve la sella
                        // senza introdurre croci al centro della cella.
                        double determinant = ((double)tl - level) * (br - level) -
                                             ((double)tr - level) * (bl - level);
                        bool highConnected = mask == 5 ? determinant > 0 : determinant < 0;
                        if (determinant == 0)
                            highConnected = (tl + (double)tr + br + bl) * .25 >= level;
                        if (highConnected)
                        {
                            if (mask == 5) { Add(0, 1); Add(2, 3); }
                            else { Add(3, 0); Add(1, 2); }
                        }
                        else
                        {
                            if (mask == 5) { Add(3, 0); Add(1, 2); }
                            else { Add(0, 1); Add(2, 3); }
                        }
                        break;
                    }
                }
            }
        }
        return Stitch(segments, cancellationToken);
    }

    private static float Fraction(float start, float end, float level)
    {
        double distance = (double)end - start;
        if (distance == 0) return .5f;
        return (float)Math.Clamp(((double)level - start) / distance, 0, 1);
    }

    private static IReadOnlyList<ContourPolyline> Stitch(List<Segment> segments,
        CancellationToken cancellationToken)
    {
        var edgesAt = new Dictionary<Vector2, List<int>>();
        for (int i = 0; i < segments.Count; i++)
        {
            Segment s = segments[i];
            Append(s.A, i);
            Append(s.B, i);
        }
        void Append(Vector2 point, int segment)
        {
            if (!edgesAt.TryGetValue(point, out List<int>? edges))
            {
                edges = new List<int>(2);
                edgesAt.Add(point, edges);
            }
            edges.Add(segment);
        }

        var used = new bool[segments.Count];
        var lines = new List<ContourPolyline>();
        void Follow(int firstSegment, Vector2 start)
        {
            var points = new List<Vector2> { start };
            int currentSegment = firstSegment;
            Vector2 current = start;
            while (currentSegment >= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (used[currentSegment]) break;
                used[currentSegment] = true;
                Segment s = segments[currentSegment];
                Vector2 next = s.A == current ? s.B : s.A;
                points.Add(next);
                current = next;
                currentSegment = -1;
                foreach (int candidate in edgesAt[current])
                    if (!used[candidate]) { currentSegment = candidate; break; }
            }
            if (points.Count >= 2)
                lines.Add(new ContourPolyline(points.ToArray(), current == start));
        }

        // Prima traccia le curve aperte, poi i cicli: raccordo stabile anche
        // in presenza di giunzioni degeneri con più di due archi.
        for (int i = 0; i < segments.Count; i++)
        {
            if ((i & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
            Segment s = segments[i];
            if (!used[i] && (edgesAt[s.A].Count != 2 || edgesAt[s.B].Count != 2))
                Follow(i, edgesAt[s.A].Count != 2 ? s.A : s.B);
        }
        for (int i = 0; i < segments.Count; i++)
        {
            if ((i & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (!used[i]) Follow(i, segments[i].A);
        }
        return lines;
    }
}
