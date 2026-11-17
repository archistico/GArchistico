using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
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
if (args.Length != 2 || args[0] is not ("picker" or "render" or "detail" or "studio" or "evolution" or "evolution-studio" or "compare" or "depth" or "depth-studio" or "depth-compare" or "depth-compat" or "sdf" or "sdf-studio" or "sdf-compare" or "sdf-aa" or "voronoi" or "voronoi-studio" or "voronoi-compare" or "voronoi-compat"))
{
    Console.Error.WriteLine("Uso: GenerativeArt.Smoke picker|render|detail|studio|evolution|evolution-studio|compare|depth|depth-studio|depth-compare|depth-compat|sdf|sdf-studio|sdf-compare|sdf-aa|voronoi|voronoi-studio|voronoi-compare|voronoi-compat <cartella solution>");
    Environment.ExitCode = 2;
    return;
}
string root = Path.GetFullPath(args[1]);
if (args[0].StartsWith("voronoi", StringComparison.Ordinal))
{
    VoronoiDepthSmoke.Run(args[0], root);
    return;
}
string output = Path.Combine(root, "artifacts", args[0].StartsWith("sdf", StringComparison.Ordinal) ? "m4d4-smoke"
    : args[0].StartsWith("depth", StringComparison.Ordinal) ? "m4d3-smoke"
    : args[0] is "evolution" or "evolution-studio" or "compare" ? "m4d2-smoke"
    : args[0] is "detail" or "studio" ? "m4d1-smoke" : "m4b1-smoke");
Directory.CreateDirectory(output);
if (args[0] is "studio" or "evolution-studio" or "depth-studio" or "sdf-studio")
{
    AppBuilder.Configure<StudioApp>().UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
    var window = new StudioWindow();
    window.Show();
    var registry = BuiltInArtworkCatalog.Create();
    var select = typeof(StudioWindow).GetMethod("SelectArtwork", BindingFlags.NonPublic | BindingFlags.Instance)!;
    var panel = (StackPanel)typeof(StudioWindow).GetField("_parameterItems", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window)!;
    string[] studioIds = args[0] == "sdf-studio" ? ["sdf-orbits", "paper-cut", "domain-warp"]
        : args[0] == "depth-studio" ? ["paper-cut", "bokeh-depth", "strata-landscape", "domain-warp"]
        : args[0] == "evolution-studio"
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
        if (LegacyArtworkControls.ForArtwork(id).Count > 0 || args[0] is "depth-studio" or "sdf-studio")
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
if (args[0] == "sdf-aa")
{
    var d = catalog.Get("sdf-orbits");
    var values = new Dictionary<string,string>(GenerativeArt.Application.Rendering.StudioRecipeDefaults.ParametersFor(d.Id))
    {
        ["background-color"]="#00000000", ["fill-color"]="#FFFFFF", ["ink-color"]="#FFFFFF",
        ["accent-color"]="#FFFFFF", ["pigment-color-4"]="#FFFFFF", ["layers"]="1", ["blobs"]="4",
        ["marbling"]="0", ["relief"]="0", ["highlight"]="0", ["shadow-opacity"]="0", ["depth-blur"]="0",
        ["outline"]="0", ["contour-opacity"]="0", ["opacity"]="1", ["warp-amount"]="0.09"
    };
    SKBitmap Make(int scale)
    {
        using var png = new MemoryStream();
        engine.Render(ArtworkRecipe.Create(d,new GenerativeArt.Core.Geometry.CanvasSize(128*scale,96*scale),42,values),png);
        return SKBitmap.Decode(png.ToArray()) ?? throw new Exception("Invalid SDF AA reference");
    }
    using var image=Make(1); using var reference=Make(4);
    double analyticError=0,binaryError=0; int edges=0, cx=64,cy=48;double nearest=double.MaxValue;
    var averaged=new double[128*96];
    for(int y=0;y<96;y++)for(int x=0;x<128;x++)
    {
        double alpha=0;
        for(int dy=0;dy<4;dy++)for(int dx=0;dx<4;dx++)alpha+=reference.GetPixel(x*4+dx,y*4+dy).Alpha/16d;
        averaged[y*128+x]=alpha;
        if(alpha is <=0 or >=255)continue;
        byte actual=image.GetPixel(x,y).Alpha;
        analyticError+=Math.Abs(actual-alpha);binaryError+=Math.Abs((actual>=128?255:0)-alpha);edges++;
        double distance=(x-64)*(x-64)+(y-48)*(y-48);
        if(actual is >20 and <235 && x>=8 && x<120 && y>=8 && y<88 && distance<nearest){nearest=distance;cx=x;cy=y;}
    }
    if(edges<50 || analyticError>=binaryError*.75 || nearest==double.MaxValue)throw new Exception("SDF AA regression");
    using var sheet=new SKBitmap(860,354);using var canvas=new SKCanvas(sheet);canvas.Clear(new SKColor(16,21,30));
    using var font=new SKFont{Size=18};using var ink=new SKPaint{Color=new SKColor(220,233,238),IsAntialias=true};
    string[] labels=["Soglia netta", "Copertura locale", "Riferimento 4×"];
    for(int side=0;side<3;side++)
    {
        using var zoom=new SKBitmap(256,256);
        for(int y=0;y<256;y++)for(int x=0;x<256;x++)
        {
            int sx=cx-8+x/16,sy=cy-8+y/16;byte alpha=image.GetPixel(sx,sy).Alpha;
            if(side==0)alpha=alpha>=128?(byte)255:(byte)0;
            if(side==2)alpha=(byte)Math.Round(averaged[sy*128+sx]);
            zoom.SetPixel(x,y,new SKColor(255,255,255,alpha));
        }
        canvas.DrawText(labels[side],side*280+20,30,SKTextAlign.Left,font,ink);
        canvas.DrawBitmap(zoom,side*280+20,50,new SKSamplingOptions(SKFilterMode.Nearest));
    }
    canvas.DrawText("Alpha del bordo · ingrandimento 16× · stessa geometria deformata",20,336,SKTextAlign.Left,font,ink);
    using var data=sheet.Encode(SKEncodedImageFormat.Png,100);
    using(var file=File.Create(Path.Combine(output,"sdf-antialiasing.png")))data.SaveTo(file);
    File.WriteAllText(Path.Combine(output,"antialiasing-report.json"),JsonSerializer.Serialize(new
    {
        seed=42,width=128,height=96,referenceScale=4,edgePixels=edges,cropCenterX=cx,cropCenterY=cy,
        analyticMeanAbsoluteAlphaError=analyticError/edges,binaryMeanAbsoluteAlphaError=binaryError/edges,
        improvementFraction=1-analyticError/binaryError,parameters=values
    },new JsonSerializerOptions{WriteIndented=true}));
    Console.WriteLine($"SDF AA: {edges} edge pixels; error reduction {1-analyticError/binaryError:P1} against binary coverage.");
    return;
}
if (args[0] == "sdf-compare")
{
    var descriptor = catalog.Get("sdf-orbits");
    var size = new GenerativeArt.Core.Geometry.CanvasSize(1920,1080);
    var fresh = new Dictionary<string,string>(GenerativeArt.Application.Rendering.StudioRecipeDefaults.ParametersFor("sdf-orbits"));
    var glass = new Dictionary<string,string>(fresh) { ["style"]="glass", ["palette"]="iris", ["highlight"]="0.85", ["relief"]="0.75" };
    ArtworkRecipe[] recipes=[ArtworkRecipe.Create(descriptor,size,42),ArtworkRecipe.Create(descriptor,size,42,fresh),ArtworkRecipe.Create(descriptor,size,42,glass)];
    string[] labels=["Classico · compatibilità", "Nuovo · pigmenti e fusioni", "Nuovo · vetro e riflessi"];
    using var sheet=new SKBitmap(1560,390);
    using var canvas=new SKCanvas(sheet);canvas.Clear(new SKColor(16,21,30));
    using var font=new SKFont { Size=20 };
    using var ink=new SKPaint { Color=new SKColor(220,233,238),IsAntialias=true };
    for(int i=0;i<recipes.Length;i++)
    {
        using var stream=new MemoryStream();engine.Render(recipes[i],stream);
        using var image=SKBitmap.Decode(stream.ToArray())??throw new Exception("Invalid SDF comparison");
        using var preview=MakePreview(image);
        canvas.DrawText(labels[i],i*520+20,35,SKTextAlign.Left,font,ink);
        canvas.DrawBitmap(preview,i*520+20,60,new SKSamplingOptions(SKFilterMode.Nearest));
        report.Add(new{label=labels[i],recipes[i].ArtworkId,recipes[i].Seed,width=1920,height=1080,parameters=recipes[i].Parameters});
        Console.WriteLine("SDF comparison: "+labels[i]);
    }
    canvas.DrawText("Seed 42 · 1920×1080 · tre stili, parametri conservati nel report",20,367,SKTextAlign.Left,font,ink);
    using var encoded=sheet.Encode(SKEncodedImageFormat.Png,100);
    using(var file=File.Create(Path.Combine(output,"sdf-comparison.png")))encoded.SaveTo(file);
    File.WriteAllText(Path.Combine(output,"comparison-parameters.json"),JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
    return;
}
if (args[0] == "depth-compat")
{
    using var baseline = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root,"docs","validation","M4D3_R1_compatibility.json")));
    foreach (var entry in baseline.RootElement.EnumerateArray())
    {
        string id = entry.GetProperty("ArtworkId").GetString()!;
        var d = catalog.Get(id);
        var r = ArtworkRecipe.Create(d,
            new GenerativeArt.Core.Geometry.CanvasSize(entry.GetProperty("width").GetInt32(), entry.GetProperty("height").GetInt32()),
            entry.GetProperty("Seed").GetUInt64(), animation:d.SupportsAnimation ? new RecipeAnimation(4,18) : null);
        using var png = new MemoryStream(); engine.Render(r,png,d.SupportsAnimation ? 0 : null);
        if (Convert.ToHexStringLower(SHA256.HashData(png.ToArray())) != entry.GetProperty("sha256").GetString())
            throw new Exception("Historical PNG changed: " + id);
        Console.WriteLine("Historical PNG identical: " + id);
    }
    return;
}
if (args[0] == "depth-compare")
{
    string[] names = ["paper-cut-canyon-vortex-1920", "bokeh-aurora-current-1920", "strata-glacier-atmosphere-1920"];
    using var sheet = new SKBitmap(1040, 1000);
    using var canvas = new SKCanvas(sheet);
    canvas.Clear(new SKColor(16, 21, 30));
    using var font = new SKFont { Size = 18 };
    using var ink = new SKPaint { Color = new SKColor(220, 233, 238), IsAntialias = true };
    canvas.DrawText("Stessa ricetta: effetti di profondità spenti", 20, 26, SKTextAlign.Left, font, ink);
    canvas.DrawText("Ombre gaussiane, piani e fuoco", 540, 26, SKTextAlign.Left, font, ink);
    for (int row = 0; row < names.Length; row++)
    {
        var after = ArtworkRecipeJson.Deserialize(File.ReadAllBytes(Path.Combine(root,"examples","recipes",names[row]+".json")), catalog);
        var values = new Dictionary<string,string>(after.Parameters);
        foreach (string key in new[] { "depth-blur", "shadow-opacity", "soft-shadow", "glow", "rim-light" })
            if (values.ContainsKey(key)) values[key] = "0";
        var before = ArtworkRecipe.Create(catalog.Get(after.ArtworkId), after.Size, after.Seed, values);
        for (int side = 0; side < 2; side++)
        {
            using var stream = new MemoryStream(); engine.Render(side == 0 ? before : after, stream);
            using var image = SKBitmap.Decode(stream.ToArray()) ?? throw new Exception("Invalid depth comparison PNG");
            using var preview = MakePreview(image);
            canvas.DrawText(catalog.Get(after.ArtworkId).Title, side * 520 + 20, row * 320 + 57, SKTextAlign.Left, font, ink);
            canvas.DrawBitmap(preview, side * 520 + 20, row * 320 + 70, new SKSamplingOptions(SKFilterMode.Nearest));
        }
        report.Add(new { recipe=names[row], after.ArtworkId, after.Seed, width=after.Size.Width, height=after.Size.Height,
            flatParameters=before.Parameters, depthParameters=after.Parameters });
        Console.WriteLine("Depth comparison: " + names[row]);
    }
    using var data = sheet.Encode(SKEncodedImageFormat.Png,100);
    using (var stream = File.Create(Path.Combine(output,"depth-comparison.png"))) data.SaveTo(stream);
    File.WriteAllText(Path.Combine(output,"comparison-parameters.json"), JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
    return;
}
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
if (args[0] == "depth") newRecipes = new HashSet<string>([
    "paper-cut-lagoon-portal-1920", "paper-cut-canyon-vortex-1920", "paper-cut-ivory-petal-1920", "paper-cut-nocturne-1920",
    "bokeh-aurora-current-1920", "bokeh-amber-spiral-1920", "bokeh-pearl-mist-1920", "bokeh-pearl-orbit-fine-1920",
    "strata-glacier-atmosphere-1920", "strata-desert-soft-light-1920"]);
if (args[0] == "sdf") newRecipes = new HashSet<string>([
    "sdf-sorbet-pigment-1920", "sdf-lagoon-river-1920", "sdf-iris-glass-1920", "sdf-nocturne-marble-1920",
    "sdf-ember-islands-1920", "sdf-sorbet-fused-fine-1920", "sdf-lagoon-ink-wash-1920", "sdf-nocturne-glass-fine-1920"]);
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
if (report.Count != newRecipes.Count) throw new Exception("Missing Full HD smoke recipes");
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
