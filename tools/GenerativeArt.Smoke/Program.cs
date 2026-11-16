using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Rendering.Skia;
using GenerativeArt.Studio;
using SkiaSharp;
if (args.Length != 2 || args[0] is not ("picker" or "render" or "detail" or "studio" or "evolution" or "evolution-studio" or "compare"))
{
    Console.Error.WriteLine("Uso: GenerativeArt.Smoke picker|render|detail|studio|evolution|evolution-studio|compare <cartella solution>");
    Environment.ExitCode = 2;
    return;
}
string root = Path.GetFullPath(args[1]);
string output = Path.Combine(root, "artifacts", args[0] is "evolution" or "evolution-studio" or "compare" ? "m4d2-smoke"
    : args[0] is "detail" or "studio" ? "m4d1-smoke" : "m4b1-smoke");
Directory.CreateDirectory(output);
if (args[0] is "studio" or "evolution-studio")
{
    AppBuilder.Configure<StudioApp>().UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
    var window = new StudioWindow();
    window.Show();
    var registry = BuiltInArtworkCatalog.Create();
    var select = typeof(StudioWindow).GetMethod("SelectArtwork", BindingFlags.NonPublic | BindingFlags.Instance)!;
    var panel = (StackPanel)typeof(StudioWindow).GetField("_parameterItems", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
    string[] studioIds = args[0] == "evolution-studio"
        ? ["flow-brush", "strata-landscape", "parametric-curves", "noise-flow", "plasma-noise", "spaced-streamlines", "looping-flow", "domain-warp"]
        : ["guilloche", "tonal-engraving", "domain-warp"];
    foreach (string id in studioIds)
    {
        var descriptor = registry.Get(id);
        select.Invoke(window, [descriptor, null]);
        Dispatcher.UIThread.RunJobs();
        int expected = descriptor.Parameters.Count(p => p.Kind is ParameterKind.Integer or ParameterKind.Number);
        int actual = panel.GetVisualDescendants().OfType<Slider>().Count();
        if (actual != expected) throw new Exception($"Missing numeric controls for {id}: {actual}/{expected}");
        if (LegacyArtworkControls.ForArtwork(id).Count > 0)
        {
            var values = (Dictionary<string,string>)typeof(StudioWindow).GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
            foreach (var pair in GenerativeArt.Application.Rendering.StudioRecipeDefaults.ParametersFor(id))
                if (values[pair.Key] != descriptor.Parameters.Single(p=>p.Id==pair.Key).ValidateValue(pair.Value))
                    throw new Exception("Fresh defaults not applied: " + id + "/" + pair.Key);
            var restored = ArtworkRecipe.Create(descriptor, new GenerativeArt.Core.Geometry.CanvasSize(1920,1080), 42,
                animation: descriptor.SupportsAnimation ? new RecipeAnimation(4,18) : null);
            select.Invoke(window, [descriptor, restored]);
            Dispatcher.UIThread.RunJobs();
            foreach (var pair in restored.Parameters)
                if (values[pair.Key] != pair.Value) throw new Exception("Imported recipe modified: " + id);
        }
        Console.WriteLine($"Studio {id}: {actual} numeric controls, color swatches and choices constructed.");
    }
    string caption = registry.Get("domain-warp").Parameters.Single(p => p.Id == "frequency").DisplayName;
    var frequencyGroup = panel.Children.OfType<StackPanel>()
        .Single(g => g.Children.OfType<TextBlock>().Any(t => t.Text == caption));
    var frequencyBox = frequencyGroup.GetVisualDescendants().OfType<TextBox>().Single();
    var frequencySlider = frequencyGroup.GetVisualDescendants().OfType<Slider>().Single();
    var parameters = (Dictionary<string,string>)typeof(StudioWindow)
        .GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
    frequencyBox.Text = "0,0008123456789";
    Dispatcher.UIThread.RunJobs();
    if (parameters["frequency"] != "0.0008123456789") throw new Exception("Exact input lost precision");
    if (frequencySlider.SmallChange > .00001) throw new Exception("Slider step too coarse");
    frequencySlider.Value = .00183;
    Dispatcher.UIThread.RunJobs();
    if (parameters["frequency"] != "0.00183") throw new Exception("Slider value was not committed correctly");
    frequencyBox.Text = "NaN";
    Dispatcher.UIThread.RunJobs();
    if (parameters["frequency"] != "0.00183") throw new Exception("Invalid input committed");
    Console.WriteLine("Studio frequency: exact decimal input preserved, fine step verified, invalid input rejected.");
    window.Close();
    return;
}
if (args[0] == "picker")
{
    AppBuilder.Configure<StudioApp>().UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
    Type type = typeof(StudioApp).Assembly.GetType("GenerativeArt.Studio.StudioColorPickerWindow")!;
    Window Make() => (Window)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic,
        null, ["Colore di prova", "#A04080C0"], null)!;
    foreach (int height in new[] { 280, 320, 480, 620 })
    {
        var picker = Make();
        picker.Width = 440; picker.Height = height; picker.Show();
        Dispatcher.UIThread.RunJobs();
        var buttons = picker.GetVisualDescendants().OfType<Button>()
            .Where(b => b.Content?.ToString() is "Conferma colore" or "Annulla").ToArray();
        foreach (Button button in buttons)
        {
            Point at = button.TranslatePoint(default, picker) ?? throw new Exception("No position");
            if (at.Y < 0 || at.Y + button.Bounds.Height > picker.ClientSize.Height + .1)
                throw new Exception($"Hidden button at height {height}: {button.Content}");
            Console.WriteLine($"Picker {height}: {button.Content}, y={at.Y}, height={button.Bounds.Height}, client={picker.ClientSize}");
        }
        if (buttons.Length != 2) throw new Exception("Missing buttons");
        var scroll = picker.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Content is StackPanel);
        scroll.Offset = new Vector(0, scroll.Extent.Height);
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        using var frame = picker.CaptureRenderedFrame();
        frame?.Save(Path.Combine(output, $"picker-{height}.png"), Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
        buttons.Single(b => b.Content?.ToString() == "Conferma colore").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if ((string?)type.GetProperty("SelectedHex")!.GetValue(picker) != "#A04080C0") throw new Exception("Confirm failed");
        var cancelled = Make(); cancelled.Show(); Dispatcher.UIThread.RunJobs();
        cancelled.GetVisualDescendants().OfType<Button>().Single(b => b.Content?.ToString() == "Annulla")
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (type.GetProperty("SelectedHex")!.GetValue(cancelled) is not null) throw new Exception("Cancel applied color");
    }
    return;
}
var catalog = BuiltInArtworkCatalog.Create();
var engine = new ArtworkRenderEngine(catalog);
var report = new List<object>();
if (args[0] == "compare")
{
    string[] ids = ["parametric-curves", "noise-flow", "spaced-streamlines", "plasma-noise", "looping-flow"];
    using var sheet = new SKBitmap(1040, 1640);
    using var canvas = new SKCanvas(sheet);
    canvas.Clear(new SKColor(16, 21, 30));
    using var font = new SKFont { Size = 18 };
    using var textPaint = new SKPaint { Color = new SKColor(220, 233, 238), IsAntialias = true };
    canvas.DrawText("Parametri storici", 20, 26, SKTextAlign.Left, font, textPaint);
    canvas.DrawText("Nuovo lavoro Studio", 540, 26, SKTextAlign.Left, font, textPaint);
    for (int row = 0; row < ids.Length; row++)
    {
        string id = ids[row];
        var descriptor = catalog.Get(id);
        var size = new GenerativeArt.Core.Geometry.CanvasSize(1920,1080);
        var animation = descriptor.SupportsAnimation ? new RecipeAnimation(12,18) : null;
        var before = ArtworkRecipe.Create(descriptor,size,42,animation:animation);
        var after = ArtworkRecipe.Create(descriptor,size,42,
            GenerativeArt.Application.Rendering.StudioRecipeDefaults.ParametersFor(id),animation);
        for (int side=0;side<2;side++)
        {
            using var stream = new MemoryStream();
            engine.Render(side == 0 ? before : after, stream, animation is null ? null : 0);
            using var bitmap = SKBitmap.Decode(stream.ToArray()) ?? throw new Exception("Invalid comparison PNG");
            using var thumbnail = MakePreview(bitmap);
            canvas.DrawText(descriptor.Title, side*520+20, row*320+57, SKTextAlign.Left, font, textPaint);
            canvas.DrawBitmap(thumbnail, side*520+20, row*320+70, new SKSamplingOptions(SKFilterMode.Nearest));
        }
        report.Add(new {artworkId=id,seed=42,width=1920,height=1080,frame=animation is null?(int?)null:0,
            oldParameters=before.Parameters,newParameters=after.Parameters});
        Console.WriteLine("Comparison: "+id);
    }
    using var encoded = sheet.Encode(SKEncodedImageFormat.Png,100);
    using (var file=File.Create(Path.Combine(output,"evolution-before-after.png"))) encoded.SaveTo(file);
    File.WriteAllText(Path.Combine(output,"comparison-parameters.json"),
        JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
    return;
}
var newRecipes = new HashSet<string>([
    "domain-warp-ocean-marble-1920", "domain-warp-ember-clouds-1920", "domain-warp-graphite-ridges-1920",
    "lic-curl-ocean-1920", "lic-vortex-graphite-1920", "spaced-streamlines-dense-1920", "spaced-streamlines-silk-1920"]);
if (args[0] == "detail") newRecipes = new HashSet<string>([
    "guilloche-banknote-1920", "guilloche-copper-orbits-1920", "guilloche-midnight-lace-1920",
    "engraving-terrain-banknote-1920", "engraving-copper-rings-1920", "engraving-midnight-diamond-1920",
    "domain-warp-silk-detail-1920", "domain-warp-copper-ribbons-1920"]);
if (args[0] == "evolution") newRecipes = new HashSet<string>([
    "flow-brush-lagoon-silk-1920", "flow-brush-ember-vortex-1920", "flow-brush-ink-dry-1920",
    "strata-desert-dunes-1920", "strata-dusk-ridges-1920", "strata-glacier-waves-1920",
    "parametric-woven-copper-1920", "parametric-harmonic-iris-1920",
    "noise-flow-curl-dense-1920", "noise-flow-braid-paper-1920", "plasma-ridged-moss-1920", "plasma-marble-ember-1920",
    "spaced-streamlines-vortex-1920", "looping-flow-aurora-dense-1920"]);
foreach (string file in Directory.GetFiles(Path.Combine(root, "examples", "recipes"), "*.json"))
{
    ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), catalog);
    engine.Validate(recipe, recipe.Animation is null ? null : 0);
    string name = Path.GetFileNameWithoutExtension(file);
    if (!newRecipes.Contains(name)) continue;
    string destination = Path.Combine(output, name + ".png");
    var timer = Stopwatch.StartNew();
    using (var stream = File.Create(destination)) engine.Render(recipe, stream, recipe.Animation is null ? null : 0);
    timer.Stop();
    using SKBitmap bitmap = SKBitmap.Decode(destination) ?? throw new Exception("Invalid PNG");
    if (bitmap.Width != 1920 || bitmap.Height != 1080) throw new Exception("Wrong dimensions");
    if (name.Contains("spaced-streamlines"))
    {
        int minimum = int.MaxValue;
        SKColor background = bitmap.GetPixel(0, 0);
        for (int by = 0; by < 8; by++)
            for (int bx = 0; bx < 8; bx++)
            {
                int covered = 0;
                for (int y = by * 135; y < (by + 1) * 135; y++)
                    for (int x = bx * 240; x < (bx + 1) * 240; x++)
                        if (bitmap.GetPixel(x, y) != background) covered++;
                minimum = Math.Min(minimum, covered);
            }
        if (minimum < 1296) throw new Exception($"Incomplete canvas {name}: {minimum}");
        Console.WriteLine($"Coverage {name}: minimum block {minimum / 324d:F1}%");
    }
    report.Add(new { recipe = name, width = bitmap.Width, height = bitmap.Height,
        seconds = timer.Elapsed.TotalSeconds, bytes = new FileInfo(destination).Length,
        processPeakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64 });
    using var thumbnail = MakePreview(bitmap);
    using var preview = thumbnail.Encode(SKEncodedImageFormat.Png, 100);
    using (var stream = File.Create(Path.Combine(output, name + "-preview.png"))) preview.SaveTo(stream);
    Console.WriteLine($"Rendered {name}: {timer.Elapsed.TotalSeconds:F2}s");
}
File.WriteAllText(Path.Combine(output, "render-report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("All recipes validated; Full HD smoke completed.");

static SKBitmap MakePreview(SKBitmap bitmap)
{
    var thumbnail = new SKBitmap(480,270);
    // Media d'area 4x4 in alpha premoltiplicato: conserva i tratti sottili riducendo il Full HD.
    for (int ty = 0; ty < 270; ty++)
        for (int tx = 0; tx < 480; tx++)
        {
            int a = 0, r = 0, g = 0, b = 0;
            for (int dy = 0; dy < 4; dy++)
                for (int dx = 0; dx < 4; dx++)
                {
                    SKColor c = bitmap.GetPixel(tx * 4 + dx, ty * 4 + dy);
                    a += c.Alpha; r += c.Red * c.Alpha; g += c.Green * c.Alpha; b += c.Blue * c.Alpha;
                }
            thumbnail.SetPixel(tx, ty, a == 0 ? SKColors.Transparent
                : new SKColor((byte)((r + a / 2) / a), (byte)((g + a / 2) / a),
                    (byte)((b + a / 2) / a), (byte)((a + 8) / 16)));
        }
    return thumbnail;
}
