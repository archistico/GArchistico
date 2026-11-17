using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Pixels;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// G4: due campi procedurali autonomi (Ocellus e Rete). Render per pixel
/// esatto rispetto alla ricetta; nessuna riduzione nascosta della preview.
/// Il disegno non riutilizza immagini dell'autore originale.
/// </summary>
internal static class GartCellularRenderer
{
    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        if (recipe.ArtworkId == "ocellus-eyespots")
        {
            long work = recipe.Size.PixelCount * OcellusField.MaximumLevels * ArtworkPaint.Int(recipe, "eyes");
            if (work > 900_000_000L)
                throw new ArgumentOutOfRangeException(nameof(recipe), "Ocellus: lavoro eccessivo per il numero di occhi e pixel.");
            return;
        }
        if (recipe.ArtworkId != "rete-foam") throw new ArgumentException("Opera cellulare sconosciuta.");
        if (ArtworkPaint.Number(recipe, "small-radius") >= ArtworkPaint.Number(recipe, "big-radius"))
            throw new ArgumentException("Rete: il raggio minimo deve essere inferiore a quello massimo.", nameof(recipe));
        long pixels = recipe.Size.PixelCount;
        if (pixels > 4_194_304L ||
            pixels * ArtworkPaint.Int(recipe, "layers") * ArtworkPaint.Int(recipe, "bubbles") > 3_000_000_000L)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Rete: combinazione pixel/livelli/celle fuori budget.");
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        switch (recipe.ArtworkId)
        {
            case "ocellus-eyespots": RenderOcellus(surface, recipe, ct); break;
            case "rete-foam": RenderRete(surface, recipe, ct); break;
            default: throw new ArgumentException("Generatore cellulare sconosciuto.", nameof(recipe));
        }
    }

    private static void RenderOcellus(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        int w = recipe.Size.Width, h = recipe.Size.Height;
        var eyes = OcellusField.Generate(recipe.Seed, w, h, ArtworkPaint.Int(recipe, "eyes"),
            ArtworkPaint.Int(recipe, "min-depth"), ArtworkPaint.Number(recipe, "depth-bias"),
            ArtworkPaint.Number(recipe, "core-radius"), ArtworkPaint.Number(recipe, "room"),
            ArtworkPaint.Number(recipe, "margin"), ct);
        double side = Math.Min(w, h);
        double band = ArtworkPaint.Number(recipe, "band");
        double grow = ArtworkPaint.Number(recipe, "growth");
        double fuse = ArtworkPaint.Number(recipe, "fuse");
        double pool = ArtworkPaint.Number(recipe, "pool");
        double poreStrength = ArtworkPaint.Number(recipe, "pore-strength");
        double poreRadius = ArtworkPaint.Number(recipe, "pore-radius");
        double rimLight = ArtworkPaint.Number(recipe, "rim-light");
        double rimWidth = ArtworkPaint.Number(recipe, "rim-width");
        double[,] radii = OcellusField.Radii(eyes, (int)side,
            OcellusField.MaximumLevels, band, grow);

        uint[] defaults = recipe.Parameters["palette"] switch
        {
            "sunset" => [0xFF673C65, 0xFFC86860, 0xFFF2A65E, 0xFFFCE0A5],
            "botanic" => [0xFF2D5347, 0xFF5A947A, 0xFFA8BA88, 0xFFE6D9A1],
            "graphite" => [0xFF303847, 0xFF636D7C, 0xFFAFB5B2, 0xFFF1EBDD],
            "coral" => [0xFF623B59, 0xFFD05D71, 0xFFF39B89, 0xFFFFD1A4],
            _ => [0xFF183E50, 0xFF317E91, 0xFF74B7B2, 0xFFE6D9B1]
        };
        Color32 background = ArtworkInk.Resolve(recipe, "background-color", Color32.FromArgb(0xFFF5EBC8));
        Color32[] ramp = PatternPalette.Resolve(recipe,
            ["ring-color-1", "ring-color-2", "ring-color-3", "ring-color-4"], defaults);
        var colors = new Color32[OcellusField.MaximumLevels + 1];
        for (int k = 1; k < colors.Length; k++)
            colors[k] = PatternPalette.Gradient(ramp, (k - 1d) / (OcellusField.MaximumLevels - 1));
        var melt = new double[OcellusField.MaximumLevels + 1];
        for (int k = 1; k < melt.Length; k++)
        {
            double reach = 0;
            for (int j = k; j <= OcellusField.MaximumLevels; j++)
            {
                double t = (OcellusField.MaximumLevels - j) / 11d;
                reach += side * band * (j == 1 ? .6 : 1 + grow * t * t * t);
            }
            melt[k] = Math.Max(.01, fuse * (reach + side * band) * (k == 1 ? pool : 1));
        }
        var pixels = new PixelBuffer(recipe.Size);
        var distances = new double[eyes.Length];
        for (int y = 0; y < h; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            for (int x = 0; x < w; x++)
            {
                for (int i = 0; i < eyes.Length; i++)
                    distances[i] = GeometryDistance.Hypotenuse(x + .5 - eyes[i].X, y + .5 - eyes[i].Y);
                Color32 color = background;
                for (int k = 1; k <= OcellusField.MaximumLevels; k++)
                {
                    double signed = OcellusField.SignedDistance(eyes, radii, distances, k, melt[k]);
                    double coverage = Math.Clamp(.5 - signed, 0, 1);
                    if (coverage <= 0) break;
                    Color32 layerInk = colors[k];
                    if (rimLight > 0)
                    {
                        // Riflesso appena all'interno del bordo: segue la geometria
                        // reale dell'unione SDF, non i cerchi isolati.
                        double t = (signed + .6 * rimWidth) / Math.Max(.25, .65 * rimWidth);
                        double highlight = rimLight * Math.Exp(-t * t);
                        layerInk = ArtworkPaint.Mix(layerInk, new Color32(layerInk.A, 255, 255, 255),
                            .66 * highlight);
                    }
                    color = ArtworkPaint.Alpha(layerInk, coverage).Over(color);
                }
                if (poreStrength > 0 && eyes.Length >= 3)
                {
                    double pore = OcellusField.JunctionPore(eyes, radii, distances, poreRadius);
                    if (pore > 0) color = ArtworkPaint.Mix(color, background, poreStrength * pore);
                }
                row[x] = color.PackedArgb;
            }
        }
        surface.DrawPixels(pixels);
    }

    private static void RenderRete(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        int w = recipe.Size.Width, h = recipe.Size.Height;
        int length = checked(w * h);
        int layers = ArtworkPaint.Int(recipe, "layers");
        var composite = new float[length];
        var coverage = new float[length];
        var scratch = new float[length];
        var film = new float[length];
        double junctionStrength = ArtworkPaint.Number(recipe, "junction-strength");
        double junctionRadius = ArtworkPaint.Number(recipe, "junction-radius");
        double nodeGlow = ArtworkPaint.Number(recipe, "node-glow");
        double membraneOpacity = ArtworkPaint.Number(recipe, "membrane-opacity");
        // Evita un buffer aggiuntivo se il riflesso dei nodi è disattivato.
        float[]? nodes = nodeGlow > 0 ? new float[length] : null;
        double wall = ArtworkPaint.Number(recipe, "wall");
        double knot = ArtworkPaint.Number(recipe, "knot");
        double filmStrength = ArtworkPaint.Number(recipe, "film");
        double blur = ArtworkPaint.Number(recipe, "rear-blur");
        double rearOpacity = ArtworkPaint.Number(recipe, "rear-opacity");
        double fade = ArtworkPaint.Number(recipe, "fade");
        double zoom = ArtworkPaint.Number(recipe, "zoom");
        double drain = ArtworkPaint.Number(recipe, "drain");
        double side = Math.Min(w, h);
        for (int layer = layers - 1; layer >= 0; layer--)
        {
            ct.ThrowIfCancellationRequested();
            double scale = Math.Pow(zoom, layer);
            ulong layerSeed = recipe.Seed ^ unchecked(0x9E3779B97F4A7C15UL * (ulong)(layer + 1));
            var field = ReteFoamField.Create(layerSeed, w, h, ArtworkPaint.Int(recipe, "bubbles"),
                Math.Min(.39, ArtworkPaint.Number(recipe, "big-radius") * scale),
                Math.Min(.049, ArtworkPaint.Number(recipe, "small-radius") * scale),
                ArtworkPaint.Number(recipe, "tier-ratio"),
                ArtworkPaint.Number(recipe, "radius-variation"),
                ArtworkPaint.Number(recipe, "density"), ArtworkPaint.Number(recipe, "fusion"),
                ArtworkPaint.Number(recipe, "froth"), layer == 0 ? drain : 0,
                ArtworkPaint.Number(recipe, "sink"), ct);
            for (int y = 0; y < h; y++)
            {
                ct.ThrowIfCancellationRequested();
                int start = y * w;
                for (int x = 0; x < w; x++)
                {
                    var near = field.Probe(x + .5, y + .5);
                    double boundary = Math.Max(0, (near.Second - near.First) * .5);
                    double thin = Math.Clamp(Math.Sqrt(near.FirstRadius / Math.Max(1, side * .14)), .38, 1);
                    double wet = layer == 0 && drain > 0
                        ? Math.Clamp(1 - GeometryDistance.Hypotenuse(x - w * 1.02, y - h * 1.05) / (drain * side + .14 * side), 0, 1)
                        : 0;
                    double width = wall * thin * (.5 + 2.5 * wet * wet);
                    double junction = junctionStrength > 0 || nodes is not null
                        ? ReteFoamField.JunctionIntensity(near, junctionRadius) : 0;
                    width *= 1 + junctionStrength * 1.35 * junction;
                    double thirdBoundary = Math.Max(0, (near.Third - near.First) * .5);
                    double soft = OcellusField.SmoothMin(boundary, thirdBoundary, knot * width * 2.6);
                    double wallCoverage = Math.Clamp(.5 + (width * .5 - soft), 0, 1);
                    coverage[start + x] = (float)wallCoverage;
                    if (layer == 0)
                    {
                        double whiteFilm = Math.Exp(-Math.Pow(boundary / Math.Max(.25, width * .16), 2)) *
                            filmStrength * (.25 + .75 * wet) * wallCoverage;
                        if (membraneOpacity > 0)
                        {
                            // La membrana tenue ora appartiene all'interno delle celle,
                            // e non soltanto alla linea di separazione tra due celle.
                            double relativeSurface = Math.Abs(near.First) / Math.Max(1, near.FirstRadius);
                            double interior = Math.Clamp(1 - relativeSurface, 0, 1);
                            double rim = Math.Clamp(1 - boundary / Math.Max(1, near.FirstRadius * .7), 0, 1);
                            double ripple = .5 + .5 * Math.Cos((x + y) * .028 +
                                near.FirstRadius * .071 + Math.Sin(x * .013 - y * .017));
                            double membrane = membraneOpacity * (.16 + .6 * interior) *
                                (.38 + .62 * rim) * (.55 + .45 * ripple) * (1 - wallCoverage);
                            whiteFilm = 1 - (1 - Math.Clamp(whiteFilm, 0, 1)) * (1 - membrane);
                        }
                        film[start + x] = (float)Math.Clamp(whiteFilm, 0, 1);
                        if (nodes is not null)
                            nodes[start + x] = (float)Math.Clamp(nodeGlow * junction * wallCoverage, 0, 1);
                    }
                }
            }
            int blurRadius = Math.Clamp((int)Math.Round(blur * layer), 0, 25);
            if (blurRadius > 0) BoxBlur(coverage, scratch, w, h, blurRadius, ct);
            float opacity = layer == 0 ? 1f : (float)(rearOpacity * Math.Pow(fade, layer - 1));
            for (int i = 0; i < length; i++)
                composite[i] = 1 - (1 - composite[i]) * (1 - opacity * coverage[i]);
        }

        (uint ground, uint wallArgb, uint light) = recipe.Parameters["palette"] switch
        {
            "night" => (0xFF121A28, 0xFF080E17, 0xFF9BD7D1),
            "sepia" => (0xFFF4E9D5, 0xFF352623, 0xFFF5C38A),
            "ice" => (0xFFDDEAF1, 0xFF34465C, 0xFFF7FFFF),
            _ => (0xFFF5F3EB, 0xFF17191E, 0xFFFDFBF4)
        };
        Color32 bg = ArtworkInk.Resolve(recipe, "background-color", Color32.FromArgb(ground));
        Color32 dark = ArtworkInk.Resolve(recipe, "wall-color", Color32.FromArgb(wallArgb));
        Color32 pale = ArtworkInk.Resolve(recipe, "film-color", Color32.FromArgb(light));
        var pixels = new PixelBuffer(recipe.Size);
        for (int y = 0; y < h; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                Color32 c = ArtworkPaint.Alpha(dark, composite[i]).Over(bg);
                if (film[i] > 0) c = ArtworkPaint.Alpha(pale, film[i]).Over(c);
                if (nodes is not null && nodes[i] > 0)
                    c = ArtworkPaint.Alpha(pale, nodes[i]).Over(c);
                row[x] = c.PackedArgb;
            }
        }
        surface.DrawPixels(pixels);
    }

    /// <summary>Filtro separabile a media mobile, senza allocazioni nascoste per ogni pixel.</summary>
    private static void BoxBlur(float[] data, float[] tmp, int w, int h, int radius, CancellationToken ct)
    {
        int diameter = radius * 2 + 1;
        for (int y = 0; y < h; y++)
        {
            ct.ThrowIfCancellationRequested();
            int start = y * w;
            float sum = 0;
            for (int k = -radius; k <= radius; k++) sum += data[start + Math.Clamp(k, 0, w - 1)];
            for (int x = 0; x < w; x++)
            {
                tmp[start + x] = sum / diameter;
                sum += data[start + Math.Min(w - 1, x + radius + 1)] -
                       data[start + Math.Max(0, x - radius)];
            }
        }
        for (int x = 0; x < w; x++)
        {
            ct.ThrowIfCancellationRequested();
            float sum = 0;
            for (int k = -radius; k <= radius; k++) sum += tmp[Math.Clamp(k, 0, h - 1) * w + x];
            for (int y = 0; y < h; y++)
            {
                data[y * w + x] = sum / diameter;
                sum += tmp[Math.Min(h - 1, y + radius + 1) * w + x] -
                       tmp[Math.Max(0, y - radius) * w + x];
            }
        }
    }
}
