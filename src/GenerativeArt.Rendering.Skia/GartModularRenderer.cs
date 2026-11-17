using System.Numerics;
using GenerativeArt.Algorithms.Pattern;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Tre alfabeti modulari parametrici, studiati dai sorgenti Gȧrt (AlienLetter V3,
/// CottonCircles e Rigor). Geometrie nuove, non copie delle immagini originali.
/// Solo geometria vettoriale su Skia: nessun PixelBuffer aggiuntivo.
/// </summary>
internal static class GartModularRenderer
{
    private static readonly string[] TileRoles = ["tile-color-1", "tile-color-2", "tile-color-3", "tile-color-4"];

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        switch (recipe.ArtworkId)
        {
            case "alien-letters": DrawAlien(surface, recipe, ct); break;
            case "cotton-circles": DrawCotton(surface, recipe, ct); break;
            case "rigor-grid": DrawRigor(surface, recipe, ct); break;
            default: throw new ArgumentException("Grammatica modulare non conosciuta.", nameof(recipe));
        }
    }

    private static void DrawAlien(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        uint[] standard = r.Parameters["palette"] switch
        {
            "neon" => [0xFFFF4B92, 0xFF4FE4C4, 0xFFFFD86E, 0xFF9796FA],
            "mineral" => [0xFF5B9EAA, 0xFFDBB988, 0xFFD67569, 0xFFB6C9BF],
            _ => [0xFFEAB46A, 0xFF88CFA9, 0xFFDF83B2, 0xFF90A9E8]
        };
        uint background = r.Parameters["palette"] switch
        {
            "neon" => 0xFF080D1F, "mineral" => 0xFF262C35, _ => 0xFF2D2926
        };
        surface.Clear(ArtworkInk.Resolve(r, "background-color", Color32.FromArgb(background)));
        var colors = PatternPalette.Resolve(r, TileRoles, standard);
        int cols = ArtworkPaint.Int(r, "columns"), rows = ArtworkPaint.Int(r, "rows");
        int divisions = ArtworkPaint.Int(r, "subdivisions");
        double scale = ArtworkPaint.Number(r, "glyph-scale"), gap = ArtworkPaint.Number(r, "grid-gap");
        double density = ArtworkPaint.Number(r, "glyph-density");
        double join = ArtworkPaint.Number(r, "join-probability");
        double round = ArtworkPaint.Number(r, "roundness");
        double cell = Math.Min(r.Size.Width * .86 / (cols + (cols - 1) * gap),
            r.Size.Height * .86 / (rows + (rows - 1) * gap)) * scale;
        double stride = cell * (1 + gap);
        double x0 = (r.Size.Width - (cols * cell + (cols - 1) * gap * cell)) * .5;
        double y0 = (r.Size.Height - (rows * cell + (rows - 1) * gap * cell)) * .5;
        var random = new SplitMix64Random(r.Seed);
        int slot = 0;
        for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                if ((slot++ & 7) == 0) ct.ThrowIfCancellationRequested();
                ulong glyphSeed = random.NextUInt64();
                var cells = ModularTileGrammar.CreateAlienGlyph(glyphSeed, divisions, density, join);
                double x = x0 + col * stride, y = y0 + row * stride;
                double sub = cell / (divisions + (divisions - 1) * .08);
                double start = (cell - (divisions * sub + (divisions - 1) * sub * .08)) * .5;
                foreach (var part in cells)
                {
                    double xx = x + start + part.Column * sub * 1.08;
                    double yy = y + start + part.Row * sub * 1.08;
                    double w = sub, h = sub;
                    if (part.Shape == 1) w = sub * 2.08;
                    if (part.Shape == 2) h = sub * 2.08;
                    // L'estensione rimane confinata nel glifo: barra e modulazione
                    // non invadono i glifi adiacenti.
                    w = Math.Min(w, x + cell - xx);
                    h = Math.Min(h, y + cell - yy);
                    if (w > 0 && h > 0)
                        RoundedRectangle(surface, xx, yy, w, h,
                            round * Math.Min(w, h), colors[part.Color]);
                }
            }
    }

    private static void DrawCotton(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        uint[] palette = r.Parameters["palette"] switch
        {
            "terra" => [0xFFB75039, 0xFFDF9B63, 0xFFBFC49E, 0xFF665967],
            "lagoon" => [0xFF164C69, 0xFF2B858D, 0xFF9DCAB3, 0xFFE2C993],
            _ => [0xFF3B5671, 0xFF83A6AF, 0xFFD9ACB9, 0xFFF0C879]
        };
        uint bg = r.Parameters["palette"] switch
        {
            "terra" => 0xFFF0E1C8, "lagoon" => 0xFF0D3041, _ => 0xFF17263A
        };
        var colors = PatternPalette.Resolve(r, TileRoles, palette);
        surface.Clear(ArtworkInk.Resolve(r, "background-color", Color32.FromArgb(bg)));
        int cols = ArtworkPaint.Int(r, "columns"), rows = ArtworkPaint.Int(r, "rows");
        double margin = ArtworkPaint.Number(r, "margin");
        double gap = ArtworkPaint.Number(r, "tile-gap");
        double arcScale = ArtworkPaint.Number(r, "arc-scale");
        string motif = r.Parameters["motif"];
        double tile = Math.Min(r.Size.Width * (1 - 2 * margin) / cols,
            r.Size.Height * (1 - 2 * margin) / rows);
        double x0 = (r.Size.Width - cols * tile) / 2;
        double y0 = (r.Size.Height - rows * tile) / 2;
        var cells = ModularTileGrammar.CreateCotton(r.Seed, cols, rows);
        foreach (var cell in cells)
        {
            if ((cell.Row * cols + cell.Column & 63) == 0) ct.ThrowIfCancellationRequested();
            double x = x0 + cell.Column * tile, y = y0 + cell.Row * tile;
            double padding = tile * gap * .5;
            double width = tile - 2 * padding;
            if (width <= 0) continue;
            Color32 baseColor = colors[cell.Color];
            Color32 accent = colors[(cell.Color + 1 + cell.Shape) % colors.Length];
            RoundedRectangle(surface, x + padding, y + padding, width, width, 0, baseColor);
            double cx = x + tile * .5, cy = y + tile * .5;
            double radius = width * .5 * arcScale;
            int direction = cell.Rotation;
            bool quarter = motif == "quarter" || (motif == "mixed" && cell.Shape == 1);
            double start = direction * Math.PI / 2;
            FilledSector(surface, cx, cy, radius, start, quarter ? Math.PI / 2 : Math.PI, accent);
            if (motif == "mixed" && cell.Shape == 2)
                FilledSector(surface, cx, cy, radius * .48, start + Math.PI, Math.PI, baseColor);
        }
    }

    private readonly record struct LetterSpot(int Col, int Row, char Glyph, double Tilt);

    private static void DrawRigor(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        (uint bg, uint ink, uint accent) = r.Parameters["palette"] switch
        {
            "night" => (0xFF111827u, 0xFFEDF2E4u, 0xFFEB8065u),
            "porcelain" => (0xFFE6E9E4u, 0xFF2D5661u, 0xFF9A6D8Eu),
            _ => (0xFFDBD1BFu, 0xFF242526u, 0xFFA8402Cu)
        };
        Color32 background = ArtworkInk.Resolve(r, "background-color", Color32.FromArgb(bg));
        Color32 text = ArtworkInk.Resolve(r, "ink-color", Color32.FromArgb(ink));
        Color32 rust = ArtworkInk.Resolve(r, "accent-color", Color32.FromArgb(accent));
        surface.Clear(background);
        int cols = ArtworkPaint.Int(r, "columns"), rows = ArtworkPaint.Int(r, "rows");
        int period = ArtworkPaint.Int(r, "period");
        double accentRate = ArtworkPaint.Number(r, "accent-rate");
        double lean = ArtworkPaint.Number(r, "letter-tilt");
        double reach = ArtworkPaint.Number(r, "reach");
        double neighborTilt = ArtworkPaint.Number(r, "neighbor-tilt");
        double push = ArtworkPaint.Number(r, "push");
        double scale = ArtworkPaint.Number(r, "tile-scale");
        double cell = Math.Min(r.Size.Width / (cols + 1d), r.Size.Height / (rows + 1d));
        double x0 = (r.Size.Width - cols * cell) / 2;
        double y0 = (r.Size.Height - rows * cell) / 2;
        var random = new SplitMix64Random(r.Seed);
        var block = ModularTileGrammar.CreateRigorBlock(random.NextUInt64(), period, accentRate);
        var letters = new List<LetterSpot>(4);
        if (r.Parameters["letters"] == "igor")
        {
            const string glyphs = "IGOR";
            for (int index = 0; index < glyphs.Length; index++)
            {
                int minRow = index * rows / 4;
                int maxRow = Math.Max(minRow, (index + 1) * rows / 4 - 1);
                int col = random.NextInt32Inclusive(0, cols - 1);
                int tries = 0;
                while (letters.Any(l => Math.Abs(l.Col - col) < 2) && tries++ < 32)
                    col = random.NextInt32Inclusive(0, cols - 1);
                letters.Add(new LetterSpot(col, random.NextInt32Inclusive(minRow, maxRow),
                    glyphs[index], random.NextDouble() < .5 ? -lean : lean));
            }
        }
        for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                if ((row * cols + col & 31) == 0) ct.ThrowIfCancellationRequested();
                var letter = letters.FirstOrDefault(l => l.Col == col && l.Row == row);
                bool hasLetter = letter.Glyph != default;
                double tilt = 0, ox = 0, oy = 0;
                if (!hasLetter)
                    foreach (var spot in letters)
                    {
                        double dx = col - spot.Col, dy = row - spot.Row;
                        double distance = Math.Sqrt(dx * dx + dy * dy);
                        if (distance < .001 || distance >= reach) continue;
                        double weight = 1 - distance / reach;
                        tilt += Math.Sign(spot.Tilt) * neighborTilt * weight;
                        ox += dx / distance * push * weight;
                        oy += dy / distance * push * weight;
                    }
                double cx = x0 + (col + .5 + ox) * cell;
                double cy = y0 + (row + .5 + oy) * cell;
                if (hasLetter)
                {
                    DrawGlyph(surface, letter.Glyph, cx, cy, cell * .82, letter.Tilt,
                        text);
                    continue;
                }
                var tile = block[(row % period) * 2 + (col % 2)];
                // Una singola decisione di colore per cella: non si altera il blocco
                // periodico, ma l'accento incide sul ritmo visivo.
                Color32 color = tile.Color == 1 ? rust : text;
                DrawRigorTile(surface, cx, cy, cell * scale, tile.Shape, tile.Rotation,
                    tilt, color);
            }
    }

    private static void DrawRigorTile(SkiaRasterSurface s, double cx, double cy, double size,
        int shape, int orient, double tilt, Color32 color)
    {
        double r = size * .46;
        double a = (orient * 90 + tilt) * Math.PI / 180;
        switch (shape)
        {
            case 0: FilledSector(s, cx, cy, r, a, Math.PI / 2, color); break;
            case 1: FilledSector(s, cx, cy, r, a, Math.PI, color); break;
            case 2: s.DrawCircle((float)cx, (float)cy, (float)(r * .84), color, fill: true); break;
            case 3:
                // Lente organica: due archi convergenti, ruotati come una singola tessera.
                var leaf = new List<Vector2>(49);
                for (int i = 0; i <= 24; i++)
                {
                    double t = i / 24d;
                    leaf.Add(Transform(cx, cy, (t * 2 - 1) * r,
                        -Math.Sin(Math.PI * t) * r * .6, a));
                }
                for (int i = 24; i >= 0; i--)
                {
                    double t = i / 24d;
                    leaf.Add(Transform(cx, cy, (t * 2 - 1) * r,
                        Math.Sin(Math.PI * t) * r * .6, a));
                }
                s.DrawPolygon(leaf, color, color, .1f);
                break;
            default:
                var triangle = new[] { Transform(cx, cy, -r, -r, a),
                    Transform(cx, cy, r, -r, a), Transform(cx, cy, r, r, a) };
                s.DrawPolygon(triangle, color, color, .1f);
                break;
        }
    }

    private static void DrawGlyph(SkiaRasterSurface s, char glyph, double cx, double cy,
        double size, double tilt, Color32 color)
    {
        double angle = tilt * Math.PI / 180;
        float stroke = (float)(size * .23);
        Vector2 Point(double x, double y) => Transform(cx, cy, x * size, y * size, angle);
        void Line(double x1, double y1, double x2, double y2)
        {
            var a = Point(x1, y1); var b = Point(x2, y2);
            s.DrawLine(a.X, a.Y, b.X, b.Y, color, stroke);
        }
        void Arc(double start, double sweep, double px = 0, double py = 0, double radius = .37)
        {
            var vertices = new List<Vector2>(33);
            for (int i = 0; i <= 32; i++)
            {
                double rad = (start + sweep * i / 32d) * Math.PI / 180;
                vertices.Add(Point(px + radius * Math.Cos(rad), py + radius * Math.Sin(rad)));
            }
            s.DrawPolyline(vertices, 1, 1, color, stroke);
        }
        switch (glyph)
        {
            case 'I': Line(0, -.38, 0, .38); break;
            case 'G': Arc(-75, 310); Line(.05, .08, .37, .08); break;
            case 'O': Arc(0, 360); break;
            case 'R':
                Line(-.25, -.38, -.25, .38);
                Arc(-90, 180, -.1, -.16, .22);
                Line(-.12, .04, .30, .40);
                break;
        }
    }

    private static void RoundedRectangle(SkiaRasterSurface surface, double x, double y,
        double w, double h, double radius, Color32 color)
    {
        radius = Math.Clamp(radius, 0, Math.Min(w, h) * .49);
        if (radius < .01)
        {
            surface.DrawPolygon([new Vector2((float)x, (float)y),
                new Vector2((float)(x + w), (float)y),
                new Vector2((float)(x + w), (float)(y + h)),
                new Vector2((float)x, (float)(y + h))], color, color, .1f);
            return;
        }
        var points = new List<Vector2>(28);
        foreach (var (centerX, centerY, start) in new[]
        {
            (x + w - radius, y + radius, -90d),
            (x + w - radius, y + h - radius, 0d),
            (x + radius, y + h - radius, 90d),
            (x + radius, y + radius, 180d)
        })
            for (int i = 0; i <= 6; i++)
            {
                double rad = (start + 90d * i / 6) * Math.PI / 180;
                points.Add(new Vector2((float)(centerX + radius * Math.Cos(rad)),
                    (float)(centerY + radius * Math.Sin(rad))));
            }
        surface.DrawPolygon(points, color, color, .1f);
    }

    private static void FilledSector(SkiaRasterSurface surface, double cx, double cy,
        double radius, double start, double sweep, Color32 color)
    {
        var vertices = new List<Vector2>(38) { new((float)cx, (float)cy) };
        int segments = Math.Max(12, (int)Math.Ceiling(Math.Abs(sweep) * radius / 7));
        segments = Math.Min(256, segments);
        for (int i = 0; i <= segments; i++)
        {
            double angle = start + sweep * i / segments;
            vertices.Add(new Vector2((float)(cx + Math.Cos(angle) * radius),
                (float)(cy + Math.Sin(angle) * radius)));
        }
        surface.DrawPolygon(vertices, color, color, .1f);
    }

    private static Vector2 Transform(double cx, double cy, double x, double y, double rotation) =>
        new((float)(cx + x * Math.Cos(rotation) - y * Math.Sin(rotation)),
            (float)(cy + x * Math.Sin(rotation) + y * Math.Cos(rotation)));
}
