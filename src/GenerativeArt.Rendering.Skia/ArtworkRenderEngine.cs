using System.Globalization;
using System.Numerics;
using GenerativeArt.Algorithms.Animation;
using GenerativeArt.Algorithms.Flow;
using GenerativeArt.Algorithms.Noise;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Colors;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Core.Pixels;
using GenerativeArt.Core.Randomness;
using GenerativeArt.Core.Rendering;

namespace GenerativeArt.Rendering.Skia;

/// <summary>
/// Runtime raster condiviso da CLI e futura GUI. Un'istanza non mantiene stato di
/// composizione: ogni Render alloca e rilascia una superficie, anche in animazione.
/// Versione, schema ricetta e budget vengono verificati prima di scrivere nello stream.
/// </summary>
public sealed class ArtworkRenderEngine : IArtworkRasterRenderer
{
    private readonly ArtworkRegistry _catalog;

    public ArtworkRenderEngine(ArtworkRegistry? catalog = null) =>
        _catalog = catalog ?? BuiltInArtworkCatalog.Create();

    public void Validate(ArtworkRecipe recipe, int? frameIndex = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        recipe.ValidateAgainst(_catalog);
        if (recipe.Animation is null)
        {
            if (frameIndex is not null)
                throw new ArgumentException("Un'opera statica non accetta un indice fotogramma.", nameof(frameIndex));
        }
        else
        {
            if (frameIndex is null || frameIndex < 0 || frameIndex >= recipe.Animation.FrameCount)
                throw new ArgumentOutOfRangeException(nameof(frameIndex), "Indice fotogramma richiesto e valido per l'animazione.");
        }

        CanvasSize size = recipe.Size;
        if (size.Width > 4096 || size.Height > 4096 || size.PixelCount > 16_777_216)
            throw new ArgumentOutOfRangeException(nameof(recipe), "Budget statico: massimo 4096x4096 e 16.777.216 pixel.");
        RasterQualityPolicy.Validate(recipe);
        ArtisticFinishOptions finishing = ArtisticFinishOptions.FromRecipe(recipe);
        if (finishing.IsEnabled && size.PixelCount > ArtisticFinishOptions.MaximumFinishPixels)
            throw new ArgumentOutOfRangeException(nameof(recipe),
                $"Finitura artistica: massimo {ArtisticFinishOptions.MaximumFinishPixels:N0} pixel.");
        switch (recipe.ArtworkId)
        {
            case "guilloche":
            case "tonal-engraving":
                LineworkRenderer.ValidateBudget(recipe);
                break;
            case "domain-warp":
            case "lic-flow":
                FlowTextureRenderer.ValidateBudget(recipe);
                break;
            case "parametric-curves":
            case "noise-flow":
            case "spaced-streamlines":
                break;
            case "topographic-contours":
                TopographicContourRenderer.ValidateBudget(recipe);
                break;
            case "sdf-orbits":
                break;
            case "voronoi-mosaic":
                VoronoiMosaicRenderer.ValidateBudget(recipe);
                break;
            case "poisson-stippling":
                PoissonStippleRenderer.ValidateBudget(recipe);
                break;
            case "plasma-noise":
                // Il campo plasma crea un PixelBuffer addizionale, ma rimane
                // nel budget generale di 16 Mpixel già verificato sopra.
                break;
            case "looping-flow":
                RecipeAnimation anim = recipe.Animation ??
                    throw new ArgumentException("looping-flow richiede animation.", nameof(recipe));
                if (size.Width is < 64 or > 4096 || size.Height is < 64 or > 4096 ||
                    anim.FrameCount is < 2 or > 120 || anim.FramesPerSecond is < 1 or > 120 ||
                    size.PixelCount * anim.FrameCount > 40_000_000L)
                    throw new ArgumentOutOfRangeException(nameof(recipe),
                        "Loop: 64..4096 px per lato, 2..120 frame, 1..120 fps e massimo 40 milioni di pixel cumulativi.");
                break;
            default:
                throw new NotSupportedException($"Nessun renderer raster per '{recipe.ArtworkId}'.");
        }
    }

    public void Render(ArtworkRecipe recipe, Stream destination, int? frameIndex = null,
        CancellationToken cancellationToken = default)
    {
        Validate(recipe, frameIndex);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Lo stream PNG deve essere scrivibile.", nameof(destination));
        cancellationToken.ThrowIfCancellationRequested();

        using var surface = new SkiaRasterSurface(recipe.Size, RasterQualityPolicy.Scale(recipe));
        switch (recipe.ArtworkId)
        {
            case "guilloche":
            case "tonal-engraving": LineworkRenderer.Render(surface, recipe, cancellationToken); break;
            case "domain-warp":
            case "lic-flow": FlowTextureRenderer.Render(surface, recipe, cancellationToken); break;
            case "parametric-curves": RenderParametric(surface, recipe, cancellationToken); break;
            case "noise-flow": RenderNoiseFlow(surface, recipe, cancellationToken); break;
            case "spaced-streamlines": RenderSpacedFlow(surface, recipe, cancellationToken); break;
            case "plasma-noise": RenderPlasma(surface, recipe, cancellationToken); break;
            case "topographic-contours": TopographicContourRenderer.Render(surface, recipe, cancellationToken); break;
            case "voronoi-mosaic": VoronoiMosaicRenderer.Render(surface, recipe, cancellationToken); break;
            case "sdf-orbits": SdfOrbitRenderer.Render(surface, recipe, cancellationToken); break;
            case "poisson-stippling": PoissonStippleRenderer.Render(surface, recipe, cancellationToken); break;
            case "looping-flow": RenderLoop(surface, recipe, frameIndex!.Value, cancellationToken); break;
            default: throw new NotSupportedException($"Nessun renderer per '{recipe.ArtworkId}'.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        surface.WritePng(destination, ArtisticFinishOptions.FromRecipe(recipe), recipe.Seed, cancellationToken);
    }

    private static void RenderParametric(SkiaRasterSurface renderer, ArtworkRecipe recipe, CancellationToken ct)
    {
        CanvasSize size = recipe.Size;
        if (ArtworkInk.IsAuto(recipe, "background-color"))
        {
            var bg = new PixelBuffer(size);
            for (int y = 0; y < size.Height; y++)
            {
                ct.ThrowIfCancellationRequested();
                Span<uint> row = bg.GetWritablePackedArgbRow(y);
                for (int x = 0; x < size.Width; x++)
                {
                    byte r = (byte)(14 + 36L * x / Math.Max(1, size.Width - 1));
                    byte g = (byte)(21 + 47L * y / Math.Max(1, size.Height - 1));
                    row[x] = Color32.Opaque(r, g, 57).PackedArgb;
                }
            }
            renderer.DrawPixels(bg);
        }
        else renderer.Clear(ArtworkInk.Resolve(recipe, "background-color", Color32.Opaque(14, 21, 57)));
        Color32 stroke = ArtworkInk.Resolve(recipe, "curve-color", new Color32(175, 255, 220, 136));
        Color32 orbit = ArtworkInk.Resolve(recipe, "orbit-color", Color32.Opaque(112, 202, 231));
        float cx = size.CenterX, cy = size.CenterY;
        float radius = MathF.Min(size.Width, size.Height) * (float)Number(recipe, "scale");
        const int steps = 960;
        for (int i = 1; i <= steps; i++)
        {
            if ((i & 63) == 0) ct.ThrowIfCancellationRequested();
            double a = (i - 1) * 2d * Math.PI / steps;
            double b = i * 2d * Math.PI / steps;
            float x1 = cx + radius * (float)Math.Sin(a * 5);
            float y1 = cy + radius * (float)Math.Sin(a * 4 + 0.4);
            float x2 = cx + radius * (float)Math.Sin(b * 5);
            float y2 = cy + radius * (float)Math.Sin(b * 4 + 0.4);
            renderer.DrawLine(x1, y1, x2, y2, stroke, 1.8f);
        }
        renderer.DrawCircle(cx, cy, radius * 1.07f, orbit, strokeWidth: 1.5f);
    }

    private static void RenderNoiseFlow(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        CanvasSize size = recipe.Size;
        if (ArtworkInk.IsAuto(recipe, "background-color"))
        {
            var bg = new PixelBuffer(size);
            for (int y = 0; y < size.Height; y++)
            {
                ct.ThrowIfCancellationRequested();
                Span<uint> row = bg.GetWritablePackedArgbRow(y);
                for (int x = 0; x < size.Width; x++)
                {
                    int r = 12 + (int)(17L * x / Math.Max(1, size.Width - 1));
                    int g = 16 + (int)(16L * y / Math.Max(1, size.Height - 1));
                    row[x] = Color32.Opaque((byte)r, (byte)g, 37).PackedArgb;
                }
            }
            surface.DrawPixels(bg);
        }
        else surface.Clear(ArtworkInk.Resolve(recipe, "background-color", Color32.Opaque(12, 16, 37)));
        var noise = new SeededSimplexNoise(recipe.Seed);
        IVectorField2D field = VectorFields.Angles((x, y) =>
            (float)(noise.Sample2D(x * .005, y * .005) * Math.PI * 3.7 + .4 * Math.Sin(x * .006)));
        var random = new SplitMix64Random(recipe.Seed);
        var bounds = new FlowBounds(0, 0, size.Width, size.Height);
        int lines = Integer(recipe, "lines");
        for (int i = 0; i < lines; i++)
        {
            ct.ThrowIfCancellationRequested();
            var start = new Vector2(random.NextFloat() * (size.Width - 1),
                random.NextFloat() * (size.Height - 1));
            IReadOnlyList<Vector2> trace = FlowTracer.Trace(field, start, steps: 320, step: 2f, bounds: bounds);
            Color32 ink = (i % 3) switch
            {
                0 => new Color32(158, 110, 217, 240),
                1 => new Color32(145, 249, 171, 117),
                _ => new Color32(158, 252, 128, 165)
            };
            ink = ArtworkInk.Resolve(recipe, $"ink-color-{i % 3 + 1}", ink);
            for (int j = 1; j < trace.Count; j++)
                surface.DrawLine(trace[j - 1].X, trace[j - 1].Y, trace[j].X, trace[j].Y, ink, 1.2f);
        }
    }

    private static void RenderSpacedFlow(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        CanvasSize size = recipe.Size;
        int w = size.Width, h = size.Height;
        var bounds = new FlowBounds(0, 0, w, h);
        var curl = new CurlNoiseField2D(recipe.Seed, frequency: .0065f, derivativeStep: .5f);
        int cols = Math.Max(2, (int)Math.Ceiling(w / 12d) + 1);
        int rows = Math.Max(2, (int)Math.Ceiling(h / 12d) + 1);
        var field = VectorGrid2D.FromField(bounds, cols, rows, curl);
        ct.ThrowIfCancellationRequested();
        float minDimension = MathF.Min(w, h);
        float separation = MathF.Max(.5f, MathF.Min((float)Number(recipe, "separation"), minDimension * .2f));
        int maxLines = Integer(recipe, "max-lines");
        float lineWidth = (float)Number(recipe, "line-width");
        int candidates = maxLines <= 500 && separation == MathF.Min(13f, minDimension * .2f)
            ? 8_000 : Math.Clamp(maxLines * 20, 8_000, 200_000);
        var paths = EvenStreamlineTracer.Trace(field, new EvenStreamlineOptions
        {
            Bounds = bounds,
            Separation = separation,
            StepSize = MathF.Min(2.4f, separation * .25f),
            Margin = MathF.Min(6f, minDimension * .1f),
            MaxStepsPerDirection = 190,
            MaxLines = maxLines,
            MaxCandidates = candidates,
            DistributeSeedBudget = maxLines > 500 || separation != MathF.Min(13f, minDimension * .2f)
        }, ct);
        ct.ThrowIfCancellationRequested();
        surface.Clear(ArtworkInk.Resolve(recipe, "background-color", Color32.Opaque(15, 19, 36)));
        for (int i = 0; i < paths.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            IReadOnlyList<Vector2> path = paths[i];
            Color32 color = (i % 4) switch
            {
                0 => new Color32(175, 93, 222, 221),
                1 => new Color32(170, 243, 188, 119),
                2 => new Color32(160, 240, 136, 173),
                _ => new Color32(180, 139, 172, 255)
            };
            color = ArtworkInk.Resolve(recipe, $"ink-color-{i % 4 + 1}", color);
            for (int j = 1; j < path.Count; j++)
                surface.DrawLine(path[j - 1].X, path[j - 1].Y, path[j].X, path[j].Y, color,
                    lineWidth);
        }
    }

    private static void RenderLoop(SkiaRasterSurface surface, ArtworkRecipe recipe, int frameIndex, CancellationToken ct)
    {
        var size = recipe.Size;
        var bounds = new FlowBounds(0, 0, size.Width, size.Height);
        LoopFrame time = LoopFrame.At(frameIndex, recipe.Animation!.FrameCount, recipe.Animation.FramesPerSecond);
        var field = new LoopNoiseField2D(recipe.Seed, time.Phase, frequency: .006,
            loopRadius: Number(recipe, "loop-radius"));
        Color32[] palette = recipe.Parameters["palette"] switch
        {
            "aurora" =>
            [
                new Color32(175, 72, 215, 233), new Color32(160, 152, 207, 255),
                new Color32(155, 241, 181, 154), new Color32(155, 255, 155, 191)
            ],
            "ember" =>
            [
                new Color32(180, 255, 128, 72), new Color32(168, 251, 188, 96),
                new Color32(154, 236, 100, 125), new Color32(160, 252, 217, 146)
            ],
            "mist" =>
            [
                new Color32(165, 184, 216, 240), new Color32(145, 168, 197, 215),
                new Color32(155, 216, 224, 231), new Color32(175, 143, 174, 211)
            ],
            _ => throw new ArgumentException("Palette non supportata.")
        };
        for (int i = 0; i < palette.Length; i++)
            palette[i] = ArtworkInk.Resolve(recipe, $"ink-color-{i + 1}", palette[i]);
        const int maxSteps = 95;
        int lines = Math.Min(54, Math.Max(18, size.Width * size.Height / 12000));
        var random = new SplitMix64Random(recipe.Seed);
        surface.Clear(ArtworkInk.Resolve(recipe, "background-color", Color32.Opaque(11, 17, 33)));
        for (int i = 0; i < lines; i++)
        {
            ct.ThrowIfCancellationRequested();
            var start = new Vector2(4 + random.NextFloat() * (size.Width - 8),
                                    4 + random.NextFloat() * (size.Height - 8));
            IReadOnlyList<Vector2> path = FlowTracer.Trace(field, start,
                steps: maxSteps, step: 2.25f, bounds: bounds);
            Color32 ink = palette[i % palette.Length];
            for (int j = 1; j < path.Count; j++)
                surface.DrawLine(path[j - 1].X, path[j - 1].Y,
                    path[j].X, path[j].Y, ink, 1.35f);
        }
    }

    private static void RenderPlasma(SkiaRasterSurface surface, ArtworkRecipe recipe, CancellationToken ct)
    {
        CanvasSize size = recipe.Size;
        double frequency = Number(recipe, "frequency");
        int octaves = Integer(recipe, "octaves");
        Color32[] palette = recipe.Parameters["palette"] switch
        {
            "deep" => [Color32.Opaque(8, 17, 50), Color32.Opaque(36, 69, 128),
                       Color32.Opaque(57, 177, 182), Color32.Opaque(252, 227, 159)],
            "ember" => [Color32.Opaque(25, 10, 28), Color32.Opaque(128, 33, 64),
                        Color32.Opaque(242, 100, 55), Color32.Opaque(255, 226, 140)],
            "moss" => [Color32.Opaque(8, 34, 29), Color32.Opaque(28, 91, 69),
                       Color32.Opaque(124, 173, 105), Color32.Opaque(237, 227, 160)],
            _ => throw new ArgumentException("Palette plasma non supportata.")
        };
        for (int i = 0; i < palette.Length; i++)
            palette[i] = ArtworkInk.Resolve(recipe, $"gradient-color-{i + 1}", palette[i]);
        var noise = new SeededSimplexNoise(recipe.Seed);
        int samplingScale = surface.RasterScale;
        var rasterSize = new CanvasSize(size.Width * samplingScale, size.Height * samplingScale);
        var pixels = new PixelBuffer(rasterSize);
        for (int y = 0; y < rasterSize.Height; y++)
        {
            ct.ThrowIfCancellationRequested();
            Span<uint> row = pixels.GetWritablePackedArgbRow(y);
            for (int x = 0; x < rasterSize.Width; x++)
            {
                // Stesso campo artistico in coordinate LOGICHE, non noise raddoppiato.
                double logicalX = ((x + .5) / samplingScale) - .5;
                double logicalY = ((y + .5) / samplingScale) - .5;
                double freq = frequency, weight = 1, total = 0, sum = 0;
                for (int octave = 0; octave < octaves; octave++)
                {
                    sum += weight * noise.Sample2D((logicalX + 13.5) * freq, (logicalY - 7.25) * freq);
                    total += weight;
                    weight *= .5;
                    freq *= 2;
                }
                double normalized = Math.Clamp(.5 + .5 * sum / total, 0, 1);
                double segment = Math.Min(2.999999999, normalized * 3);
                int index = (int)segment;
                double t = segment - index;
                Color32 left = palette[index], right = palette[index + 1];
                row[x] = new Color32(Blend(left.A, right.A, t),
                    Blend(left.R, right.R, t), Blend(left.G, right.G, t),
                    Blend(left.B, right.B, t)).PackedArgb;
            }
        }
        surface.DrawPixels(pixels, nativeResolution: true);
    }

    private static byte Blend(byte a, byte b, double t) =>
        (byte)Math.Clamp((int)Math.Round(a + (b - a) * t), 0, 255);

    private static int Integer(ArtworkRecipe recipe, string name) =>
        int.Parse(recipe.Parameters[name], CultureInfo.InvariantCulture);

    private static double Number(ArtworkRecipe recipe, string name) =>
        double.Parse(recipe.Parameters[name], CultureInfo.InvariantCulture);
}
