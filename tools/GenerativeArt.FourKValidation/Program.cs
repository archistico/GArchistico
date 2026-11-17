using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Rendering.Skia;
using SkiaSharp;

// Run from the solution directory. Each render should run in a separate process
// so its peak working set measures that recipe rather than the entire matrix.
try
{
    if (args.Length < 2) throw new ArgumentException("Usage: prepare <output-folder> | render <recipe.json> <output-folder> | sheet <render-folder> <sheet.png> | details <render-folder> <details.png>");
    var catalog = BuiltInArtworkCatalog.Create();
    var engine = new ArtworkRenderEngine(catalog);
    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    switch (args[0])
    {
        case "prepare":
        {
            string folder = Path.GetFullPath(args[1]);
            Directory.CreateDirectory(Path.Combine(folder, "recipes"));
            foreach (var descriptor in catalog.All)
            {
                ArtworkRecipe recipe = ProductionRenderProfile.PrepareFourK(StudioRecipeDefaults.RecipeFor(descriptor.Id), catalog);
                engine.Validate(recipe, recipe.Animation is null ? null : 0);
                File.WriteAllBytes(Path.Combine(folder, "recipes", descriptor.Id + ".json"), ArtworkRecipeJson.Serialize(recipe, catalog));
            }
            int examples = 0;
            var errors = new List<object>();
            foreach (string path in Directory.GetFiles("examples/recipes", "*.json", SearchOption.AllDirectories).Order())
            {
                examples++;
                try
                {
                    ArtworkRecipe prepared = ProductionRenderProfile.PrepareFourK(ArtworkRecipeFiles.Load(path, catalog), catalog);
                    engine.Validate(prepared, prepared.Animation is null ? null : 0);
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    errors.Add(new { path, error = ex.Message });
                }
            }
            File.WriteAllText(Path.Combine(folder, "validation.json"), JsonSerializer.Serialize(new { generators = catalog.All.Count, examples, errors }, jsonOptions));
            Console.WriteLine($"4K preflight: {catalog.All.Count} generators, {examples} examples, {errors.Count} failures.");
            if (errors.Count != 0) Environment.ExitCode = 1;
            break;
        }
        case "render" when args.Length == 3:
        {
            ArtworkRecipe recipe = ArtworkRecipeFiles.Load(args[1], catalog);
            string folder = Path.GetFullPath(args[2]);
            Directory.CreateDirectory(folder);
            string name = Path.GetFileNameWithoutExtension(args[1]);
            string destination = Path.Combine(folder, name + ".png");
            var watch = Stopwatch.StartNew();
            using (var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
                engine.Render(recipe, stream, recipe.Animation is null ? null : 0);
            watch.Stop();
            using SKBitmap bitmap = SKBitmap.Decode(destination) ?? throw new InvalidOperationException("Invalid PNG.");
            if (bitmap.Width != recipe.Size.Width || bitmap.Height != recipe.Size.Height)
                throw new InvalidOperationException("PNG dimensions differ from recipe.");
            var colors = new HashSet<SKColor>();
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
                colors.Add(bitmap.GetPixel((x * bitmap.Width + bitmap.Width / 2) / 16, (y * bitmap.Height + bitmap.Height / 2) / 16));
            var info = new
            {
                recipe.ArtworkId, recipe.Seed, width = bitmap.Width, height = bitmap.Height,
                profile = recipe.Parameters["render-profile"], quality = recipe.Parameters["render-quality"],
                frame = recipe.Animation is null ? (int?)null : 0,
                seconds = watch.Elapsed.TotalSeconds, bytes = new FileInfo(destination).Length,
                peakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64, sampleColorCount = colors.Count,
                sha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(destination))), parameters = recipe.Parameters
            };
            File.WriteAllText(Path.Combine(folder, name + ".report.json"), JsonSerializer.Serialize(info, jsonOptions));
            Console.WriteLine($"{name}: {info.seconds:F2}s, {info.peakWorkingSetBytes / 1048576d:F0}MiB, {colors.Count} sampled colors.");
            break;
        }
        case "sheet" when args.Length == 3:
        {
            string[] files = Directory.GetFiles(args[1], "*.report.json").Order().ToArray();
            if (files.Length == 0) throw new ArgumentException("No render reports found.");
            const int columns = 5, cellWidth = 384, cellHeight = 256;
            using var sheet = new SKBitmap(columns * cellWidth, ((files.Length + columns - 1) / columns) * cellHeight);
            using var canvas = new SKCanvas(sheet);
            canvas.Clear(new SKColor(16, 21, 30));
            using var font = new SKFont { Size = 14 };
            using var paint = new SKPaint { Color = new SKColor(224, 235, 239), IsAntialias = true };
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]).Replace(".report.json", "", StringComparison.Ordinal);
                using SKBitmap bitmap = SKBitmap.Decode(Path.Combine(args[1], name + ".png"));
                using SKBitmap thumbnail = bitmap.Resize(new SKImageInfo(368, 207), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                int x = i % columns * cellWidth + 8, y = i / columns * cellHeight;
                canvas.DrawText(name, x, y + 22, SKTextAlign.Left, font, paint);
                canvas.DrawBitmap(thumbnail, x, y + 32, new SKSamplingOptions(SKFilterMode.Nearest));
            }
            using SKData encoded = sheet.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new FileStream(args[2], FileMode.CreateNew, FileAccess.Write);
            encoded.SaveTo(stream);
            break;
        }
        case "details" when args.Length == 3:
        {
            string[] names = ["crinis-bristles", "cincinnus-adaptive", "voronoi-layered", "sdf-orbits", "flow-brush", "spaced-streamlines"];
            using var sheet = new SKBitmap(1536, 1088);
            using var canvas = new SKCanvas(sheet);
            canvas.Clear(new SKColor(16, 21, 30));
            using var font = new SKFont { Size = 16 };
            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            for (int i = 0; i < names.Length; i++)
            {
                using SKBitmap bitmap = SKBitmap.Decode(Path.Combine(args[1], names[i] + ".png"));
                int x = (bitmap.Width - 512) / 2, y = (bitmap.Height - 512) / 2;
                using var crop = new SKBitmap();
                if (!bitmap.ExtractSubset(crop, new SKRectI(x, y, x + 512, y + 512)))
                    throw new InvalidOperationException("Cannot extract native crop.");
                int left = i % 3 * 512, top = i / 3 * 544;
                canvas.DrawText(names[i] + " · crop 100%", left + 8, top + 22, SKTextAlign.Left, font, paint);
                canvas.DrawBitmap(crop, left, top + 32, new SKSamplingOptions(SKFilterMode.Nearest));
            }
            using SKData encoded = sheet.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new FileStream(args[2], FileMode.CreateNew, FileAccess.Write);
            encoded.SaveTo(stream);
            break;
        }
        default: throw new ArgumentException("Unknown mode or incorrect arguments.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}
