using System.Globalization;
using GenerativeArt.Algorithms.Cincinnus;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;

// Strumento autonomo C3/C4: NON registra un artwork nel catalogo Studio.
// Uso: dotnet run --project tools/GenerativeArt.CincinnusPreview -c Release -- [path.png] [seed] [width] [height] [simple|adaptive]
try
{
    string file = args.Length > 0 ? args[0] : "cincinnus-c4-preview.png";
    long seed = args.Length > 1 ? long.Parse(args[1], CultureInfo.InvariantCulture) : 3;
    int width = args.Length > 2 ? int.Parse(args[2], CultureInfo.InvariantCulture) : 768;
    int height = args.Length > 3 ? int.Parse(args[3], CultureInfo.InvariantCulture) : width;
    string mode = args.Length > 4 ? args[4].ToLowerInvariant() : "simple";
    if (args.Length > 5 || width is < 256 or > 2048 || height is < 256 or > 2048)
        throw new ArgumentOutOfRangeException(nameof(args), "Dimensioni ammesse per la preview: 256-2048 px.");
    if (mode is not ("simple" or "adaptive"))
        throw new ArgumentException("La modalità deve essere 'simple' oppure 'adaptive'.", nameof(args));

    float scale = Math.Min(width, height) / 2000f;
    IReadOnlyList<CincinnusBrushStroke> brushStrokes;
    bool limits;
    string description;
    if (mode == "adaptive")
    {
        var adaptiveOptions = new CincinnusAdaptiveOptions
        {
            CoverageCell = 20f * scale,
            StepSize = Math.Max(.75f, 3f * scale),
            MaximumTraceLength = 9000f * scale,
            Margin = 500f * scale,
            MinimumRadius = 25f * scale,
            MinimumAverageHalfWidth = 30f * scale,
            MaximumHalfWidth = 120f * scale,
            MaximumStrokeLength = 1600f * scale,
            MaximumSpinePoints = 350_000,
            MaximumCoverageCells = 150_000,
            MaximumTotalStrokes = 1_400
        };
        CincinnusAdaptiveLayout layout = CincinnusAdaptivePlacer.Generate(width, height, seed, adaptiveOptions);
        brushStrokes = layout.StrokesInPaintOrder;
        limits = layout.SafetyLimitReached;
        description = $"main={layout.Main.Count}, filler={layout.Filler.Count}, patch={layout.Patches.Count}, " +
            $"tentativi={layout.PlacementAttempts}+{layout.PatchAttempts}, " +
            $"celle scoperte={layout.UncoveredCellsBeforePatches}->{layout.UncoveredCellsAfterPatches}";
    }
    else
    {
        float gap = 110f * scale;
        var placement = new CincinnusSimpleOptions
        {
            Separation = gap,
            StepSize = Math.Clamp(3f * scale, 1.25f, 3f),
            MinimumLength = Math.Max(30f, 300f * scale),
            MaximumLength = Math.Max(800f, 9000f * scale),
            Margin = 3.75f * gap,
            MaximumLines = 2_048,
            MaximumPoints = 250_000,
            MaximumPendingSeeds = 500_000,
            MaximumCoverCells = 150_000
        };
        CincinnusSimpleLayout lines = CincinnusSimplePlacer.Generate(width, height, seed, placement);
        var strokeOptions = new CincinnusStrokeOptions
        {
            FirstMinimumLength = Math.Max(35f, 200f * scale),
            SubsequentMinimumLength = Math.Max(60f, 600f * scale),
            MaximumStrokeLength = Math.Max(125f, 1600f * scale),
            MinimumOverlap = Math.Max(15f, 150f * scale),
            MaximumOverlap = Math.Max(30f, 350f * scale)
        };
        CincinnusStrokePlan strokes = CincinnusStrokePlanner.Build(lines, seed, strokeOptions);
        brushStrokes = strokes.Strokes;
        limits = lines.SafetyLimitReached || strokes.SafetyLimitReached;
        description = $"linee={lines.Lines.Count}, pennellate={strokes.Strokes.Count}";
    }
    if (brushStrokes.Count == 0)
        throw new InvalidOperationException("Nessuna pennellata prodotta; scegliere un altro seed o altri parametri.");
    using SKSurface surface = SKSurface.Create(new SKImageInfo(width, height,
        SKColorType.Bgra8888, SKAlphaType.Premul))
        ?? throw new InvalidOperationException("Creazione della superficie Skia non riuscita.");
    var paintOptions = new CincinnusPaintOptions
    {
        BristlePitch = Math.Clamp(4.6f * scale, .8f, 4.6f),
        WideShadowBlur = 60f * scale,
        TightShadowBlur = 10f * scale,
        DotCount = (int)Math.Round(4000d * width * height / 4_000_000d)
    };
    CincinnusBristlePainter.Paint(surface.Canvas, brushStrokes, width, height, seed, paintOptions);
    using SKImage image = surface.Snapshot();
    using SKData data = image.Encode(SKEncodedImageFormat.Png, 100)
        ?? throw new InvalidOperationException("Codifica PNG non riuscita.");
    string fullPath = Path.GetFullPath(file);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    using (FileStream output = File.Create(fullPath)) data.SaveTo(output);
    Console.WriteLine($"Cincinnus {mode}: {width}x{height}, seed={seed}, {description}, PNG={fullPath}");
    if (limits) Console.Error.WriteLine("ATTENZIONE: budget di sicurezza raggiunto; composizione incompleta.");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"Cincinnus preview: {error.Message}");
    return 1;
}
