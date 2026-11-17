using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Rendering.Skia;
using GenerativeArt.Studio;
using SkiaSharp;

internal static class VoronoiDepthSmoke
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    internal static void Run(string mode, string root)
    {
        string output = Path.Combine(root, "artifacts", "voronoi-depth-smoke");
        Directory.CreateDirectory(output);
        var catalog = BuiltInArtworkCatalog.Create();
        var engine = new ArtworkRenderEngine(catalog);
        if (mode == "voronoi-studio") { Studio(root, catalog); return; }
        if (mode == "voronoi-compare") { Compare(root, output, catalog, engine); return; }
        if (mode == "voronoi-compat") { Compatibility(root, catalog, engine); return; }
        int validated = 0;
        foreach (string path in Directory.GetFiles(Path.Combine(root, "examples", "recipes"), "*.json", SearchOption.AllDirectories))
        {
            var recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(path), catalog);
            engine.Validate(recipe, recipe.Animation is null ? null : 0); validated++;
        }
        var report = new List<object>();
        foreach (string path in NewRecipes(root))
        {
            var recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(path), catalog);
            string name = Path.GetFileNameWithoutExtension(path);
            string png = Path.Combine(output, name + ".png");
            var timer = Stopwatch.StartNew();
            using (var stream = File.Create(png)) engine.Render(recipe, stream);
            timer.Stop();
            using var image = SKBitmap.Decode(png) ?? throw new Exception("Invalid Voronoi PNG");
            if (image.Width != 1920 || image.Height != 1080 || image.Pixels.Distinct().Count() < 100)
                throw new Exception("Wrong Voronoi dimensions or content");
            using var thumbnail = Thumbnail(image);
            using var data = thumbnail.Encode(SKEncodedImageFormat.Png, 100);
            using (var stream = File.Create(Path.Combine(output, name + "-preview.png"))) data.SaveTo(stream);
            report.Add(new { recipe = name, width = image.Width, height = image.Height,
                quality = recipe.Parameters["render-quality"], seconds = timer.Elapsed.TotalSeconds,
                bytes = new FileInfo(png).Length, processPeakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64 });
            Console.WriteLine($"Voronoi {name}: {timer.Elapsed.TotalSeconds:F2}s");
        }
        if (report.Count != 6) throw new Exception("Missing Voronoi recipes");
        File.WriteAllText(Path.Combine(output, "render-report.json"),
            JsonSerializer.Serialize(new { validatedRecipes = validated, renders = report }, JsonOptions));
        Console.WriteLine($"Validated {validated} recipes; decoded six Full HD Voronoi PNGs.");
    }

    private static string[] NewRecipes(string root) => Directory.GetFiles(
        Path.Combine(root, "examples", "recipes", "voronoi-depth-v1"), "*.json").OrderBy(p => p, StringComparer.Ordinal).ToArray();

    private static void Studio(string root, ArtworkRegistry catalog)
    {
        AppBuilder.Configure<StudioApp>().UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
        var window = new StudioWindow(); window.Show();
        Type type = typeof(StudioWindow);
        var select = type.GetMethod("SelectArtwork", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var parameters = (Dictionary<string,string>)type.GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
        var editors = (Dictionary<string,(Control Editor, TextBlock Note)>)type.GetField("_applicabilityEditors", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
        var panel = (StackPanel)type.GetField("_parameterItems", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
        var descriptor = catalog.Get("voronoi-mosaic");
        select.Invoke(window, [descriptor, null]); Dispatcher.UIThread.RunJobs();
        if (parameters["depth-style"] != "flat" || parameters["sites"] != "81" || editors["depth-layers"].Editor.IsEnabled)
            throw new Exception("Personal preset or disabled editor changed");
        var style = panel.GetVisualDescendants().OfType<ComboBox>().First();
        style.SelectedItem = "layered"; Dispatcher.UIThread.RunJobs();
        if (parameters["depth-style"] != "layered" || !editors["depth-layers"].Editor.IsEnabled)
            throw new Exception("Layered editors did not activate");
        foreach (string path in NewRecipes(root))
        {
            var recipe = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(path), catalog);
            select.Invoke(window, [descriptor, recipe]); Dispatcher.UIThread.RunJobs();
            foreach (var pair in recipe.Parameters)
                if (parameters[pair.Key] != pair.Value) throw new Exception("Imported recipe modified: " + pair.Key);
            int expected = descriptor.Parameters.Count(p => p.Kind is ParameterKind.Integer or ParameterKind.Number);
            if (panel.GetVisualDescendants().OfType<Slider>().Count() != expected)
                throw new Exception("Missing numeric Voronoi controls");
            var swatches = (Dictionary<string,Action>)type.GetField("_swatchRefresh", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
            if (swatches.Count != 10) throw new Exception("Missing Voronoi color swatches");
            foreach (var refresh in swatches.Values) refresh();
            Console.WriteLine($"Studio {Path.GetFileName(path)}: {expected} numeric editors, 10 colors; imported values intact.");
        }
        window.Close();
    }

    private static void Compatibility(string root, ArtworkRegistry catalog, ArtworkRenderEngine engine)
    {
        using var data = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "docs", "validation", "VORONOI_V1_R1_compatibility.json")));
        foreach (var entry in data.RootElement.GetProperty("renders").EnumerateArray())
        {
            var recipe = ArtworkRecipeJson.Deserialize(entry.GetProperty("originalRecipe").GetRawText(), catalog);
            using var stream = new MemoryStream(); engine.Render(recipe, stream);
            if (Convert.ToHexStringLower(SHA256.HashData(stream.ToArray())) != entry.GetProperty("sha256").GetString())
                throw new Exception("Historical Voronoi changed");
            Console.WriteLine("Historical PNG identical: " + entry.GetProperty("name").GetString());
        }
        foreach (var entry in data.RootElement.GetProperty("presets").EnumerateArray())
        {
            string path = Path.Combine(root, "src", "GenerativeArt.Application", "Defaults", entry.GetProperty("file").GetString()!);
            if (Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))) != entry.GetProperty("sha256").GetString())
                throw new Exception("Embedded preset changed: " + path);
        }
        Console.WriteLine("All 30 embedded preset files unchanged.");
    }

    private static void Compare(string root, string output, ArtworkRegistry catalog, ArtworkRenderEngine engine)
    {
        string[] names = ["voronoi-depth-glass-windows-1920", "voronoi-depth-copper-cavities-1920"];
        string[] labels = ["Piatto (storico)", "Aperture e piani", "Ombre, rilievo e distanza"];
        using var sheet = new SKBitmap(1560, 700); using var canvas = new SKCanvas(sheet);
        canvas.Clear(new SKColor(16, 21, 30));
        using var font = new SKFont { Size = 18 };
        using var ink = new SKPaint { Color = new SKColor(220, 233, 238), IsAntialias = true };
        var report = new List<object>();
        for (int column = 0; column < 3; column++)
            canvas.DrawText(labels[column], column * 520 + 20, 25, SKTextAlign.Left, font, ink);
        for (int row = 0; row < names.Length; row++)
        {
            var after = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(Path.Combine(root, "examples", "recipes", "voronoi-depth-v1", names[row] + ".json")), catalog);
            for (int column = 0; column < 3; column++)
            {
                var values = new Dictionary<string,string>(after.Parameters);
                if (column == 0) values["depth-style"] = "flat";
                if (column == 1)
                    foreach (string key in new[] { "shadow-opacity", "tile-relief", "depth-blur", "distance-dimming", "atmosphere" }) values[key] = "0";
                var recipe = ArtworkRecipe.Create(catalog.Get(after.ArtworkId), after.Size, after.Seed, values);
                using var stream = new MemoryStream(); engine.Render(recipe, stream);
                using var image = SKBitmap.Decode(stream.ToArray()) ?? throw new Exception("Invalid Voronoi comparison");
                using var thumbnail = Thumbnail(image);
                canvas.DrawText(names[row].Replace("voronoi-depth-", "").Replace("-1920", ""), column * 520 + 20, row * 320 + 55, SKTextAlign.Left, font, ink);
                canvas.DrawBitmap(thumbnail, column * 520 + 20, row * 320 + 70, new SKSamplingOptions(SKFilterMode.Nearest));
                report.Add(new { recipe = names[row], view = labels[column], after.Seed, width = 1920, height = 1080, parameters = recipe.Parameters });
            }
            Console.WriteLine("Voronoi comparison: " + names[row]);
        }
        using var encoded = sheet.Encode(SKEncodedImageFormat.Png, 100);
        using (var stream = File.Create(Path.Combine(output, "voronoi-depth-comparison.png"))) encoded.SaveTo(stream);
        File.WriteAllText(Path.Combine(output, "comparison-parameters.json"), JsonSerializer.Serialize(report, JsonOptions));
    }

    private static SKBitmap Thumbnail(SKBitmap image)
    {
        var result = new SKBitmap(480, 270);
        for (int y = 0; y < 270; y++) for (int x = 0; x < 480; x++)
        {
            int a = 0, r = 0, g = 0, b = 0;
            for (int dy = 0; dy < 4; dy++) for (int dx = 0; dx < 4; dx++)
            {
                SKColor c = image.GetPixel(x * 4 + dx, y * 4 + dy);
                a += c.Alpha; r += c.Red * c.Alpha; g += c.Green * c.Alpha; b += c.Blue * c.Alpha;
            }
            result.SetPixel(x, y, a == 0 ? SKColors.Transparent : new SKColor(
                (byte)((r + a / 2) / a), (byte)((g + a / 2) / a), (byte)((b + a / 2) / a), (byte)((a + 8) / 16)));
        }
        return result;
    }
}
