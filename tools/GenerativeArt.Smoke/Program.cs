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
if (args.Length != 2 || args[0] is not ("picker" or "render" or "detail" or "studio"))
{
    Console.Error.WriteLine("Uso: GenerativeArt.Smoke picker|render|detail|studio <cartella solution>");
    Environment.ExitCode = 2;
    return;
}
string root = Path.GetFullPath(args[1]);
string output = Path.Combine(root, "artifacts", args[0] is "detail" or "studio" ? "m4d1-smoke" : "m4b1-smoke");
Directory.CreateDirectory(output);
if (args[0] == "studio")
{
    AppBuilder.Configure<StudioApp>().UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
    var window = new StudioWindow();
    window.Show();
    var registry = BuiltInArtworkCatalog.Create();
    var select = typeof(StudioWindow).GetMethod("SelectArtwork", BindingFlags.NonPublic | BindingFlags.Instance)!;
    var panel = (StackPanel)typeof(StudioWindow).GetField("_parameterItems", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
    foreach (string id in new[] { "guilloche", "tonal-engraving", "domain-warp" })
    {
        var descriptor = registry.Get(id);
        select.Invoke(window, [descriptor, null]);
        Dispatcher.UIThread.RunJobs();
        int expected = descriptor.Parameters.Count(p => p.Kind is ParameterKind.Integer or ParameterKind.Number);
        int actual = panel.GetVisualDescendants().OfType<Slider>().Count();
        if (actual != expected) throw new Exception($"Missing numeric controls for {id}: {actual}/{expected}");
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
var newRecipes = new HashSet<string>([
    "domain-warp-ocean-marble-1920", "domain-warp-ember-clouds-1920", "domain-warp-graphite-ridges-1920",
    "lic-curl-ocean-1920", "lic-vortex-graphite-1920", "spaced-streamlines-dense-1920", "spaced-streamlines-silk-1920"]);
if (args[0] == "detail") newRecipes = new HashSet<string>([
    "guilloche-banknote-1920", "guilloche-copper-orbits-1920", "guilloche-midnight-lace-1920",
    "engraving-terrain-banknote-1920", "engraving-copper-rings-1920", "engraving-midnight-diamond-1920",
    "domain-warp-silk-detail-1920", "domain-warp-copper-ribbons-1920"]);
foreach (string file in Directory.GetFiles(Path.Combine(root, "examples", "recipes"), "*.json"))
{
    ArtworkRecipe recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(file), catalog);
    engine.Validate(recipe, recipe.Animation is null ? null : 0);
    string name = Path.GetFileNameWithoutExtension(file);
    if (!newRecipes.Contains(name)) continue;
    string destination = Path.Combine(output, name + ".png");
    var timer = Stopwatch.StartNew();
    using (var stream = File.Create(destination)) engine.Render(recipe, stream);
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
    using var thumbnail = new SKBitmap(480, 270);
    using (var canvas = new SKCanvas(thumbnail))
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, 480, 270), new SKSamplingOptions(SKFilterMode.Linear));
    using var preview = thumbnail.Encode(SKEncodedImageFormat.Png, 100);
    using (var stream = File.Create(Path.Combine(output, name + "-preview.png"))) preview.SaveTo(stream);
    Console.WriteLine($"Rendered {name}: {timer.Elapsed.TotalSeconds:F2}s");
}
File.WriteAllText(Path.Combine(output, "render-report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("All recipes validated; Full HD smoke completed.");
