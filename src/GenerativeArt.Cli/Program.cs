using System.Globalization;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;

namespace GenerativeArt.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] is "list" or "describe")
            return ArtworkCatalogCli.Run(args);
        if (args.Length > 0 && args[0] is "recipe-new" or "recipe-validate" or "recipe-show" or "render-recipe")
            return ArtworkRecipeCli.Run(args);
        try
        {
            if (args.Length > 0 && args[0] == "render-loop-frames")
                return LoopFramesDemo.Run(args);
            if (args.Length > 0 && args[0] == "render-flow-spaced")
                return EvenFlowDemo.Run(args);
            if (args.Length > 0 && args[0] == "render-flow")
                return FlowDemo.Run(args);
            if (args.Length is < 1 or > 4 || args[0] != "render-demo")
            {
                PrintHelp();
                return 2;
            }
            string path = args.Length >= 2 ? args[1] : "demo.png";
            int w = args.Length >= 3 ? ReadDimension(args[2]) : 800;
            int h = args.Length >= 4 ? ReadDimension(args[3]) : 600;
            ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
            ArtworkRecipe recipe = ArtworkRecipe.Create(catalog.Get("parametric-curves"),
                new CanvasSize(w, h), 42UL);
            RecipeOutputWriter.Write(recipe, path, new ArtworkRenderEngine(catalog));
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException
            or InvalidOperationException or NotSupportedException or KeyNotFoundException
            or DllNotFoundException or TypeInitializationException or OverflowException)
        {
            Console.Error.WriteLine($"Errore rendering: {ex.Message}");
            return 1;
        }
    }

    private static int ReadDimension(string raw) =>
        int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out int n) && n is >= 1 and <= 4096
            ? n : throw new ArgumentOutOfRangeException(nameof(raw), "Dimensione ammessa: 1..4096 pixel.");

    private static void PrintHelp() =>
        Console.WriteLine("Uso: dotnet run --project src/GenerativeArt.Cli -c Release -- list | describe <id> | render-demo|render-flow|render-flow-spaced [file.png] [larghezza] [altezza] [seed] | render-loop-frames [cartella] [larghezza] [altezza] [seed] [fotogrammi] [fps] | recipe-new <id> <file.json> [w] [h] [seed] [frames] [fps] | recipe-validate <file.json> | recipe-show <file.json> | render-recipe <file.json> <destinazione>");
}
