using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Rendering.Skia;
using GenerativeArt.Studio;

internal static class FourKStudioSmoke
{
    internal static void Run(string root)
    {
        AppBuilder.Configure<StudioApp>().UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
        var catalog = BuiltInArtworkCatalog.Create();
        var engine = new ArtworkRenderEngine(catalog);
        var select = typeof(StudioWindow).GetMethod("SelectArtwork", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var build = typeof(StudioWindow).GetMethod("BuildRecipe", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var checkedIds = new List<string>();
        var window = new StudioWindow();
        window.Show();
        try
        {
            Button button = window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PrepareFourK");
            foreach (var descriptor in catalog.All)
            {
                ArtworkRecipe original = StudioRecipeDefaults.RecipeFor(descriptor.Id);
                select.Invoke(window, [descriptor, original]);
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                ArtworkRecipe actual = (ArtworkRecipe)build.Invoke(window, null)!;
                ArtworkRecipe expected = ProductionRenderProfile.PrepareFourK(original, catalog);
                if (!ArtworkRecipeJson.Serialize(actual, catalog).AsSpan().SequenceEqual(ArtworkRecipeJson.Serialize(expected, catalog)))
                    throw new InvalidOperationException("Studio 4K preparation differs from CLI: " + descriptor.Id);
                engine.Validate(actual, actual.Animation is null ? null : 0);
                checkedIds.Add(descriptor.Id);
            }
        }
        finally { window.Close(); }
        string folder = Path.Combine(root, "artifacts", "four-k-studio");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "report.json"), JsonSerializer.Serialize(new
        {
            checkedIds, action = "4K · qualità massima", check = "UI button builds the same recipe as CLI; pending preview cancelled on close",
            interactiveDesktopTest = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Studio 4K: {checkedIds.Count} generatori, ricette identiche alla CLI; nessun render ridotto.");
    }
}
