using System.Numerics;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// G3: due linguaggi di flusso ispirati a Fascia ed Eclectic di Gȧrt.
/// Le geometrie e la ricerca delle collisioni sono in Algorithms; colori e
/// retinatura sono originali, senza portare librerie o runtime Kotlin.
/// </summary>
internal static class GartFlowRenderer
{
    private static readonly string[] InkRoles = ["ink-color-1", "ink-color-2", "ink-color-3", "ink-color-4"];
    private static readonly string[] TrailRoles = ["trail-color-1", "trail-color-2", "trail-color-3", "trail-color-4"];

    internal static void ValidateBudget(ArtworkRecipe recipe)
    {
        int steps = ArtworkPaint.Int(recipe, "steps");
        if (recipe.ArtworkId == "fascia-ribbons")
        {
            int bands = ArtworkPaint.Int(recipe, "bands");
            double width = Math.Min(recipe.Size.Width, recipe.Size.Height) *
                ArtworkPaint.Number(recipe, "band-width");
            double droplet = ArtworkPaint.Number(recipe, "droplet-size");
            int strips = Math.Clamp((int)Math.Ceiling(width / (droplet * 2)), 4, 24);
            if ((long)bands * steps * (strips + 1) > RenderWorkPolicy.Limit(recipe, 220_000, 2_000_000))
                throw new ArgumentOutOfRangeException(nameof(recipe),
                    "Fascia: il budget massimo è di 220.000 moduli di retinatura.");
        }
        else if (recipe.ArtworkId == "eclectic-trails")
        {
            if ((long)ArtworkPaint.Int(recipe, "trails") * steps > RenderWorkPolicy.Limit(recipe, 140_000, 400_000))
                throw new ArgumentOutOfRangeException(nameof(recipe),
                    "Eclectic: il budget massimo è di 140.000 integrazioni.");
        }
        else throw new ArgumentException("Generatore G3 sconosciuto.", nameof(recipe));
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        switch (recipe.ArtworkId)
        {
            case "fascia-ribbons": Fascia(surface, recipe, ct); break;
            case "eclectic-trails": Eclectic(surface, recipe, ct); break;
            default: throw new ArgumentException("Generatore G3 sconosciuto.", nameof(recipe));
        }
    }

    private static GartFlowWhirl[] Whirls(ArtworkRecipe recipe, SplitMix64Random random)
    {
        int count = ArtworkPaint.Int(recipe, "whirls");
        var values = new GartFlowWhirl[count];
        float min = Math.Min(recipe.Size.Width, recipe.Size.Height);
        for (int i = 0; i < count; i++)
        {
            var center = new Vector2((float)((-.15 + 1.3 * random.NextDouble()) * recipe.Size.Width),
                (float)((-.15 + 1.3 * random.NextDouble()) * recipe.Size.Height));
            float spin = (random.NextDouble() < .5 ? -1 : 1) *
                (float)(.7 + 1.1 * random.NextDouble());
            float reach = (float)(min * (.30 + .4 * random.NextDouble()));
            values[i] = new GartFlowWhirl(center, spin, reach);
        }
        return values;
    }

    private static void Fascia(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        (uint background, uint[] colors) = r.Parameters["palette"] switch
        {
            "copper" => (0xFFF3E2CAu, new uint[] { 0xFF251B22, 0xFF95553F, 0xFFC3824F, 0xFF385768 }),
            "lagoon" => (0xFFDBECE7u, new uint[] { 0xFF132F40, 0xFF36817C, 0xFF86AF9A, 0xFFB06C66 }),
            _ => (0xFFF7EADBu, new uint[] { 0xFF171516, 0xFF383953, 0xFF9E5F55, 0xFF31556C })
        };
        Color32 paper = ArtworkInk.Resolve(r, "background-color", Color32.FromArgb(background));
        Color32[] inks = PatternPalette.Resolve(r, InkRoles, colors);
        surface.Clear(paper);
        var rnd = new SplitMix64Random(r.Seed);
        GartFlowWhirl[] whirls = Whirls(r, rnd);
        int bands = ArtworkPaint.Int(r, "bands"), steps = ArtworkPaint.Int(r, "steps");
        float stepSize = (float)ArtworkPaint.Number(r, "step-size");
        float width = Math.Min(r.Size.Width, r.Size.Height) *
            (float)ArtworkPaint.Number(r, "band-width");
        float taper = (float)ArtworkPaint.Number(r, "taper");
        float droplet = (float)ArtworkPaint.Number(r, "droplet-size");
        float density = (float)ArtworkPaint.Number(r, "drop-density");
        double fade = ArtworkPaint.Number(r, "fade");
        float frequency = (float)ArtworkPaint.Number(r, "frequency");
        float drift = (float)ArtworkPaint.Number(r, "drift");
        float waviness = (float)ArtworkPaint.Number(r, "waviness");
        int columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(bands * (double)r.Size.Width / r.Size.Height)));
        int rows = (int)Math.Ceiling(bands / (double)columns);
        for (int band = 0; band < bands; band++)
        {
            ct.ThrowIfCancellationRequested();
            int col = band % columns, row = band / columns;
            var origin = new Vector2((float)((col + .1 + .8 * rnd.NextDouble()) * r.Size.Width / columns),
                (float)((row + .1 + .8 * rnd.NextDouble()) * r.Size.Height / rows));
            List<Vector2> path = GartFlowGeometry2D.Trace(origin, steps, stepSize,
                r.Size.Width, r.Size.Height,
                p => GartFlowGeometry2D.Direction(p, frequency, drift, whirls, waviness), ct);
            if (path.Count < 2) continue;
            Color32 ink = inks[band % inks.Length];
            Color32 target = band % 3 == 0 ? paper : inks[(band + 1) % inks.Length];
            double fadeStart = 1 - fade * (.65 + .3 * rnd.NextDouble());
            int strips = Math.Clamp((int)Math.Ceiling(width / (2 * droplet)), 4, 24);
            for (int i = 0; i < path.Count - 1; i++)
            {
                if ((i & 31) == 0) ct.ThrowIfCancellationRequested();
                Vector2 a = path[i], b = path[i + 1];
                Vector2 delta = b - a;
                float length = delta.Length();
                if (length < .0001f) continue;
                Vector2 normal = new(-delta.Y / length, delta.X / length);
                double u = (i + .5) / (path.Count - 1d);
                float halfWidth = .5f * width * (float)(1 - taper * Math.Pow(Math.Abs(2 * u - 1), 1.7));
                halfWidth = Math.Max(.2f, halfWidth);
                double f = fade <= 0 ? 0 : Math.Clamp((u - fadeStart) / Math.Max(.0001, 1 - fadeStart), 0, 1);
                if (f <= 0)
                {
                    DrawStrip(surface, a, b, normal, -halfWidth, halfWidth, ink);
                }
                else if (f >= 1)
                {
                    DrawStrip(surface, a, b, normal, -halfWidth, halfWidth, target);
                }
                else
                {
                    // Colore discreto per ciascuna microcella: non interpolazione RGB.
                    // La posizione del pixel ancora la texture al foglio e al seed.
                    for (int k = 0; k < strips; k++)
                    {
                        float lo = halfWidth * (2f * k / strips - 1f);
                        float hi = halfWidth * (2f * (k + 1) / strips - 1f);
                        Vector2 mid = (a + b) * .5f + normal * ((lo + hi) * .5f);
                        int gx = (int)MathF.Floor(mid.X / Math.Max(.5f, droplet));
                        int gy = (int)MathF.Floor(mid.Y / Math.Max(.5f, droplet));
                        double cellular = Hash01(gx, gy, band, r.Seed);
                        double mist = Hash01(gx * 7 + i, gy * 5 + k, 11, r.Seed);
                        double threshold = density * cellular + (1 - density) * mist;
                        DrawStrip(surface, a, b, normal, lo, hi, f > threshold ? target : ink);
                    }
                }
            }
        }
    }

    private static void DrawStrip(SkiaRasterSurface surface, Vector2 a, Vector2 b,
        Vector2 normal, float low, float high, Color32 color)
    {
        surface.DrawPolygon([a + normal * low, a + normal * high,
            b + normal * high, b + normal * low], color, color, .1f);
    }

    // Hash non soggetto alla randomizzazione di string.GetHashCode(), riproducibile.
    private static double Hash01(int x, int y, int layer, ulong seed)
    {
        ulong value = seed ^ unchecked((ulong)(uint)x * 0x9E3779B97F4A7C15UL) ^
            unchecked((ulong)(uint)y * 0xBF58476D1CE4E5B9UL) ^
            unchecked((ulong)(uint)layer * 0x94D049BB133111EBUL);
        value = unchecked((value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL);
        value = unchecked((value ^ (value >> 27)) * 0x94D049BB133111EBUL);
        value ^= value >> 31;
        return (value >> 11) * (1d / 9007199254740992d);
    }

    private static void Eclectic(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        (uint background, uint[] colors) = r.Parameters["palette"] switch
        {
            "neon" => (0xFF0D1426u, new uint[] { 0xFF5FE4DE, 0xFFF3A8DD, 0xFFFFD074, 0xFFB1A4FF }),
            "earth" => (0xFFF1E7D8u, new uint[] { 0xFF5A3534, 0xFF9F6B50, 0xFF6E8272, 0xFFCC9D64 }),
            _ => (0xFF161E29u, new uint[] { 0xFFE0DDD2, 0xFFB8CDCF, 0xFFE0A79C, 0xFF8AA8B2 })
        };
        surface.Clear(ArtworkInk.Resolve(r, "background-color", Color32.FromArgb(background)));
        Color32[] inks = PatternPalette.Resolve(r, TrailRoles, colors);
        var rnd = new SplitMix64Random(r.Seed);
        GartFlowWhirl[] whirls = Whirls(r, rnd);
        int attempts = ArtworkPaint.Int(r, "trails"), steps = ArtworkPaint.Int(r, "steps");
        float stepSize = (float)ArtworkPaint.Number(r, "step-size");
        float separation = (float)ArtworkPaint.Number(r, "separation");
        float minLength = (float)ArtworkPaint.Number(r, "min-length");
        float baseWidth = (float)ArtworkPaint.Number(r, "stroke-width");
        float variation = (float)ArtworkPaint.Number(r, "width-variation");
        float frequency = (float)ArtworkPaint.Number(r, "frequency");
        float drift = (float)ArtworkPaint.Number(r, "drift");
        float waviness = (float)ArtworkPaint.Number(r, "waviness");
        double opacity = ArtworkPaint.Number(r, "opacity");
        var occupancy = new GartTrailOccupancy2D(separation);
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            Vector2 p = new((float)(rnd.NextDouble() * r.Size.Width),
                (float)(rnd.NextDouble() * r.Size.Height));
            if (occupancy.IsOccupied(p, separation)) continue;
            var trail = new List<Vector2>(steps + 1) { p };
            for (int i = 0; i < steps; i++)
            {
                if ((i & 31) == 0) ct.ThrowIfCancellationRequested();
                Vector2 d = GartFlowGeometry2D.Direction(p, frequency, drift, whirls, waviness);
                Vector2 middle = p + .5f * stepSize * d;
                Vector2 next = p + stepSize * GartFlowGeometry2D.Direction(
                    middle, frequency, drift, whirls, waviness);
                if (next.X < 0 || next.Y < 0 || next.X >= r.Size.Width || next.Y >= r.Size.Height ||
                    occupancy.IsOccupied(next, separation)) break;
                trail.Add(next);
                p = next;
            }
            if ((trail.Count - 1) * stepSize < minLength) continue;
            occupancy.Deposit(trail);
            Color32 ink = ArtworkPaint.Alpha(inks[attempt % inks.Length], opacity);
            float strokeWidth = Math.Max(.3f, baseWidth * (float)(1 + variation * (2 * rnd.NextDouble() - 1)));
            surface.DrawPolyline(trail, 1, 1, ink, strokeWidth);
        }
    }
}
