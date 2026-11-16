using System.Numerics;
using GenerativeArt.Algorithms.Geometry;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using static GenerativeArt.Rendering.Skia.ArtworkPaint;

namespace GenerativeArt.Rendering.Skia;

internal static class SdfPigmentRenderer
{
    private static readonly byte[] Srgb = Enumerable.Range(0, 8193)
        .Select(i => (byte)Math.Round(SrgbTransfer.ToSrgb(i / 8192d) * 255)).ToArray();

    internal static void Validate(ArtworkRecipe r)
    {
        if (r.Parameters["style"] == "classic") return;
        int layers = Int(r, "layers"), scale = RasterQualityPolicy.Scale(r);
        // Include due righe esterne e due colonne per le derivate a schermo.
        long samples = (r.Size.Width * (long)scale + 2) * (r.Size.Height * (long)scale + 2);
        if (samples * layers * Int(r, "blobs") > 260_000_000)
            throw new ArgumentOutOfRangeException(nameof(r), "Budget SDF organiche: massimo 260 milioni di valutazioni di macchie. Ridurre piani, macchie o risoluzione.");
        DepthLayerPolicy.Validate(r, layers, Number(r, "depth-blur") + Number(r, "shadow-softness"));
    }

    internal static void Render(SkiaRasterSurface surface, ArtworkRecipe r, CancellationToken ct)
    {
        Color32[] colors = LegacyColors(r, "background-color", "fill-color", "ink-color", "accent-color", "pigment-color-4", "shadow-color");
        surface.Clear(colors[0]);
        var palette = new PigmentPalette(colors[1..5], r.Parameters["color-mode"] == "pigment");
        int sampling = surface.RasterScale, width = r.Size.Width * sampling, height = r.Size.Height * sampling;
        float unit = Math.Min(width, height);
        double logicalUnit = Math.Min(r.Size.Width, r.Size.Height) / 1000d;
        int layers = Int(r, "layers");
        // Un buffer raster riutilizzato; solo tre righe di campioni, mai un campo float a pieno canvas.
        var pixels = new PixelBuffer(new CanvasSize(width, height));
        var above = new PigmentSdfSample[width + 2];
        var current = new PigmentSdfSample[width + 2];
        var below = new PigmentSdfSample[width + 2];
        float rotation = (float)(Number(r, "rotation") * Math.PI / 180);
        float cos = MathF.Cos(rotation), sin = MathF.Sin(rotation);
        var stretch = new Vector2((float)Number(r, "stretch-x"), (float)Number(r, "stretch-y"));
        float shapeScale = (float)Number(r, "shape-scale");
        float centerX = (float)Number(r, "center-x") * width, centerY = (float)Number(r, "center-y") * height;
        float marbleAmount = (float)Number(r, "marbling"), marbleFrequency = (float)Number(r, "marble-frequency");
        float opacity = (float)Number(r, "opacity");
        bool glass = r.Parameters["style"] == "glass";
        if (glass) opacity *= .72f;
        float relief = (float)Number(r, "relief"), highlight = (float)Number(r, "highlight");
        float contourOpacity = (float)Number(r, "contour-opacity"), outline = (float)Number(r, "outline");
        float edge = (float)Number(r, "edge-width"), bandStep = 1f / (Int(r, "bands") * 2.4f);
        float radius = (float)Number(r, "blob-radius");
        double lightAngle = Number(r, "light-angle") * Math.PI / 180;
        var light = new Vector2((float)Math.Cos(lightAngle), (float)Math.Sin(lightAngle));
        for (int layer = 0; layer < layers; layer++)
        {
            ct.ThrowIfCancellationRequested();
            double t = layers == 1 ? 1 : (double)layer / (layers - 1);
            ulong seed = r.Seed ^ unchecked((ulong)layer * 0x9E3779B97F4A7C15UL);
            var field = new PigmentSdfField(seed, new Vector2(width / unit, height / unit), new PigmentSdfOptions
            {
                Blobs = Int(r, "blobs"), Radius = radius, RadiusVariation = (float)Number(r, "radius-variation"),
                Spread = (float)Number(r, "spread"), Elongation = (float)Number(r, "elongation"),
                Fusion = (float)Number(r, "smoothness"), ColorMix = (float)Number(r, "color-mix"),
                Warp = (float)Number(r, "warp-amount"), WarpFrequency = (float)Number(r, "warp-frequency"),
                Layout = r.Parameters["layout"] switch { "orbital" => PigmentSdfLayout.Orbital, "river" => PigmentSdfLayout.River, _ => PigmentSdfLayout.Cluster }
            });
            var marbleNoise = new SeededSimplexNoise(seed ^ 0xF13A9UL);
            float planeScale = shapeScale * (float)(.8 + .2 * t);
            Vector2 Transform(float x, float y)
            {
                var p = new Vector2((x + .5f - centerX) / unit, (y + .5f - centerY) / unit);
                return new Vector2(p.X * cos + p.Y * sin, -p.X * sin + p.Y * cos) / stretch / planeScale;
            }
            Vector2 stepX = new(cos / unit / stretch.X / planeScale, -sin / unit / stretch.Y / planeScale);
            void SampleRow(PigmentSdfSample[] row, int y)
            {
                Vector2 start = Transform(-1, y);
                for (int i = 0; i < row.Length; i++)
                {
                    if ((i & 127) == 0) ct.ThrowIfCancellationRequested();
                    row[i] = field.Sample(start + stepX * i);
                }
            }
            SampleRow(above, -1); SampleRow(current, 0);
            for (int y = 0; y < height; y++)
            {
                ct.ThrowIfCancellationRequested();
                SampleRow(below, y + 1);
                Span<uint> output = pixels.GetWritablePackedArgbRow(y);
                Vector2 rowStart = Transform(0, y);
                for (int x = 0; x < width; x++)
                {
                    if ((x & 127) == 0) ct.ThrowIfCancellationRequested();
                    var sample = current[x + 1];
                    float dx = (current[x + 2].Distance - current[x].Distance) * .5f;
                    float dy = (below[x + 1].Distance - above[x + 1].Distance) * .5f;
                    float footprint = Math.Max(1e-7f, Math.Abs(dx) + Math.Abs(dy));
                    float coverage = SdfPixelCoverage.Fill(sample.Distance, footprint);
                    if (coverage <= 0) { output[x] = 0; continue; }
                    Vector4 weights = sample.ColorWeights;
                    if (marbleAmount > 0)
                    {
                        Vector2 q = rowStart + stepX * x;
                        float n = (float)marbleNoise.Sample2D(q.X * 3 + 71, q.Y * 3 + 19);
                        float wave = .5f + .5f * MathF.Sin((q.X + q.Y * .4f) * marbleFrequency * MathF.Tau + n * 4);
                        weights = Vector4.Lerp(weights, new Vector4(weights.Y, weights.Z, weights.W, weights.X), marbleAmount * wave * .65f);
                    }
                    Vector3 rgb = palette.Mix(weights, out float alpha);
                    float length = MathF.Sqrt(dx * dx + dy * dy);
                    float facing = length > 1e-9f ? (dx * light.X + dy * light.Y) / length : 0;
                    float shoulder = MathF.Exp(-Math.Max(0, -sample.Distance) / (radius * .32f));
                    rgb *= 1 - relief * .16f + facing * shoulder * relief * .22f;
                    float specular = MathF.Pow(Math.Max(0, facing), glass ? 10 : 3) * shoulder * highlight * (glass ? .8f : .28f);
                    rgb = Vector3.Lerp(rgb, Vector3.One, specular);
                    if (contourOpacity > 0)
                    {
                        float bandDistance = sample.Distance - MathF.Round(sample.Distance / bandStep) * bandStep;
                        float stripe = edge * .45f >= bandStep ? 1 : SdfPixelCoverage.Stroke(bandDistance, edge * .45f, footprint);
                        rgb = Vector3.Lerp(rgb, rgb * .35f, stripe * contourOpacity);
                    }
                    if (outline > 0)
                        rgb = Vector3.Lerp(rgb, rgb * .25f, SdfPixelCoverage.Stroke(sample.Distance, edge, footprint) * outline);
                    output[x] = palette.Encode(rgb, alpha * coverage * opacity);
                }
                (above, current, below) = (current, below, above);
            }
            double distance = Number(r, "shadow-distance") * logicalUnit;
            var effects = new LayerEffects(Number(r, "depth-blur") * logicalUnit * Math.Pow(1 - t, 1.4),
                Number(r, "shadow-opacity"), -light.X * distance, -light.Y * distance,
                Number(r, "shadow-softness") * logicalUnit, colors[5]);
            surface.DrawLayer(s => s.DrawPixels(pixels, nativeResolution: true), effects, ct);
        }
    }

    private sealed class PigmentPalette
    {
        private readonly Vector3[] _colors;
        private readonly Vector4 _alpha;
        private readonly bool _pigment;
        internal PigmentPalette(Color32[] colors, bool pigment)
        {
            _pigment = pigment;
            _colors = colors.Select(c => new Vector3((float)SrgbTransfer.ToLinear(c.R / 255d),
                (float)SrgbTransfer.ToLinear(c.G / 255d), (float)SrgbTransfer.ToLinear(c.B / 255d))).ToArray();
            if (pigment)
                for (int i = 0; i < _colors.Length; i++)
                    _colors[i] = new Vector3(MathF.Log(Math.Max(.0001f, _colors[i].X)),
                        MathF.Log(Math.Max(.0001f, _colors[i].Y)), MathF.Log(Math.Max(.0001f, _colors[i].Z)));
            _alpha = new Vector4(colors[0].A, colors[1].A, colors[2].A, colors[3].A) / 255;
        }
        internal Vector3 Mix(Vector4 weights, out float alpha)
        {
            weights *= _alpha;
            alpha = weights.X + weights.Y + weights.Z + weights.W;
            if (alpha <= 1e-8f) return Vector3.Zero;
            weights /= alpha;
            Vector3 rgb = _colors[0] * weights.X + _colors[1] * weights.Y + _colors[2] * weights.Z + _colors[3] * weights.W;
            return _pigment ? new Vector3(MathF.Exp(rgb.X), MathF.Exp(rgb.Y), MathF.Exp(rgb.Z)) : rgb;
        }
        internal uint Encode(Vector3 rgb, float alpha)
        {
            uint a = (uint)Math.Clamp((int)MathF.Round(alpha * 255), 0, 255);
            if (a == 0) return 0;
            byte Channel(float value) => Srgb[(int)MathF.Round(Math.Clamp(value, 0, 1) * 8192)];
            return a << 24 | (uint)Channel(rgb.X) << 16 | (uint)Channel(rgb.Y) << 8 | Channel(rgb.Z);
        }
    }
}
