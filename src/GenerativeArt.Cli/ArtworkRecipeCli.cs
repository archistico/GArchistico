using System.Globalization;
using System.Text;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;

namespace GenerativeArt.Cli;

/// <summary>Workflow delle ricette: nessuna reflection, nessun linguaggio esterno.</summary>
internal static class ArtworkRecipeCli
{
    public static int Run(string[] args)
    {
        try
        {
            return args[0] switch
            {
                "recipe-new" => New(args),
                "recipe-validate" => Validate(args),
                "recipe-show" => Show(args),
                "render-recipe" => Render(args),
                "prepare-4k" => PrepareFourK(args),
                _ => 2
            };
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException
            or InvalidOperationException or NotSupportedException or KeyNotFoundException
            or System.Text.Json.JsonException
            or OverflowException or DllNotFoundException or TypeInitializationException)
        {
            Console.Error.WriteLine($"Errore ricetta: {ex.Message}");
            return 1;
        }
    }

    private static int New(string[] args)
    {
        if (args.Length is < 3 or > 8)
        {
            Console.Error.WriteLine("Uso: recipe-new <id> <file.json> [larghezza] [altezza] [seed] [frameCount] [fps]");
            return 2;
        }
        ArtworkRegistry registry = BuiltInArtworkCatalog.Create();
        ArtworkDescriptor descriptor = registry.Get(args[1]);
        int width = args.Length >= 4 ? ReadInt(args[3], 1, 8192, "larghezza") : 512;
        int height = args.Length >= 5 ? ReadInt(args[4], 1, 8192, "altezza") : 512;
        ulong seed = args.Length >= 6 ? ReadSeed(args[5]) : 42UL;
        if (!descriptor.SupportsAnimation && args.Length >= 7)
            throw new ArgumentException("Non è possibile specificare frame/FPS per un'opera statica.");
        RecipeAnimation? animation = descriptor.SupportsAnimation
            ? new RecipeAnimation(args.Length >= 7 ? ReadInt(args[6], 2, 10000, "frameCount") : 24,
                args.Length >= 8 ? ReadInt(args[7], 1, 240, "fps") : 24)
            : null;
        ArtworkRecipe recipe = ArtworkRecipe.Create(descriptor, new CanvasSize(width, height), seed,
            animation: animation);
        ArtworkRecipeFiles.SaveNew(args[2], recipe, registry);
        Console.WriteLine($"Ricetta v{recipe.FormatVersion} creata: {Path.GetFullPath(args[2])}");
        return 0;
    }

    private static int Validate(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Uso: recipe-validate <file.json>");
            return 2;
        }
        ArtworkRecipe recipe = ArtworkRecipeFiles.Load(args[1], BuiltInArtworkCatalog.Create());
        Console.WriteLine($"Ricetta valida: {recipe.ArtworkId} v{recipe.AlgorithmVersion}, " +
            $"{recipe.Size.Width}x{recipe.Size.Height}, seed={recipe.Seed}.");
        return 0;
    }

    private static int Show(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Uso: recipe-show <file.json>");
            return 2;
        }
        ArtworkRegistry registry = BuiltInArtworkCatalog.Create();
        ArtworkRecipe recipe = ArtworkRecipeFiles.Load(args[1], registry);
        Console.WriteLine(Encoding.UTF8.GetString(ArtworkRecipeJson.Serialize(recipe, registry)));
        return 0;
    }

    private static int Render(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Uso: render-recipe <file.json> <file.png|cartella-frame>");
            return 2;
        }
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        ArtworkRecipe recipe = ArtworkRecipeFiles.Load(args[1], catalog);
        RecipeOutputWriter.Write(recipe, args[2], new ArtworkRenderEngine(catalog));
        return 0;
    }

    private static int PrepareFourK(string[] args)
    {
        if (args.Length is < 3 or > 4 || args.Length == 4 && args[3] != "--keep-complexity")
        {
            Console.Error.WriteLine("Uso: prepare-4k <origine.json> <nuova-ricetta.json> [--keep-complexity]");
            return 2;
        }
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        ArtworkRecipe original = ArtworkRecipeFiles.Load(args[1], catalog);
        ArtworkRecipe prepared = ProductionRenderProfile.PrepareFourK(original, catalog, args.Length == 3);
        new ArtworkRenderEngine(catalog).Validate(prepared, prepared.Animation is null ? null : 0);
        ArtworkRecipeFiles.SaveNew(args[2], prepared, catalog);
        Console.WriteLine($"Ricetta 4K {prepared.Size.Width}×{prepared.Size.Height}, supersampling 2×: {Path.GetFullPath(args[2])}");
        return 0;
    }

    private static int ReadInt(string text, int minimum, int maximum, string field)
    {
        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
            || value < minimum || value > maximum)
            throw new ArgumentException($"'{field}' deve essere compreso tra {minimum} e {maximum}.");
        return value;
    }

    private static ulong ReadSeed(string text)
    {
        if (text.Length == 0 || text.Any(c => c < '0' || c > '9') ||
            !ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out ulong value))
            throw new ArgumentException("Seed richiesto: UInt64 decimale non negativo.");
        return value;
    }
}
