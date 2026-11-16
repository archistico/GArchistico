using System.Globalization;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;

namespace GenerativeArt.Cli;

/// <summary>Comando compatibile: ora usa il runtime comune delle ricette.</summary>
internal static class FlowDemo
{
    public static int Run(string[] args)
    {
        if (args.Length is < 1 or > 5)
        {
            Console.Error.WriteLine("Uso: render-flow [file.png] [larghezza] [altezza] [seed]");
            return 2;
        }
        string path = args.Length >= 2 ? args[1] : "flow.png";
        int w = args.Length >= 3 ? ReadDimension(args[2]) : 800;
        int h = args.Length >= 4 ? ReadDimension(args[3]) : 600;
        ulong seed = args.Length >= 5 ? ReadSeed(args[4]) : 42UL;
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        int legacyLines = Math.Min(120, Math.Max(24, (int)((long)w * h / 6000)));
        var recipe = ArtworkRecipe.Create(catalog.Get("noise-flow"), new CanvasSize(w, h), seed,
            new Dictionary<string, string> { ["lines"] = legacyLines.ToString(CultureInfo.InvariantCulture) });
        RecipeOutputWriter.Write(recipe, path, new ArtworkRenderEngine(catalog));
        return 0;
    }

    private static int ReadDimension(string raw) =>
        int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out int n) && n is >= 1 and <= 4096
            ? n : throw new ArgumentOutOfRangeException(nameof(raw), "Dimensione ammessa: 1..4096.");

    private static ulong ReadSeed(string raw) =>
        ulong.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed)
            ? seed : throw new ArgumentException("Seed UInt64 non valido.", nameof(raw));
}
