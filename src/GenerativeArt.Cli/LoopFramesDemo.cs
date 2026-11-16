using System.Globalization;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Core.Geometry;
using GenerativeArt.Rendering.Skia;

namespace GenerativeArt.Cli;

/// <summary>Comando legacy dei loop: ora usa lo stesso runtime del JSON.</summary>
internal static class LoopFramesDemo
{
    public static int Run(string[] args)
    {
        if (args.Length is < 1 or > 7)
        {
            Console.Error.WriteLine("Uso: render-loop-frames [cartella] [larghezza] [altezza] [seed] [fotogrammi] [fps]");
            return 2;
        }
        string directory = args.Length >= 2 ? args[1] : "output/loop-frames";
        int w = args.Length >= 3 ? ReadInt(args[2], 64, 1024) : 512;
        int h = args.Length >= 4 ? ReadInt(args[3], 64, 1024) : 512;
        ulong seed = args.Length >= 5 ? ReadSeed(args[4]) : 42UL;
        int frames = args.Length >= 6 ? ReadInt(args[5], 2, 120) : 24;
        int fps = args.Length >= 7 ? ReadInt(args[6], 1, 120) : 24;
        ArtworkRegistry catalog = BuiltInArtworkCatalog.Create();
        ArtworkRecipe recipe = ArtworkRecipe.Create(catalog.Get("looping-flow"), new CanvasSize(w, h), seed,
            animation: new RecipeAnimation(frames, fps));
        RecipeOutputWriter.Write(recipe, directory, new ArtworkRenderEngine(catalog));
        return 0;
    }

    private static int ReadInt(string raw, int low, int high) =>
        int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out int n) && n >= low && n <= high
            ? n : throw new ArgumentOutOfRangeException(nameof(raw), $"Valore ammesso: {low}..{high}.");

    private static ulong ReadSeed(string raw) =>
        ulong.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed)
            ? seed : throw new ArgumentException("Seed UInt64 non valido.", nameof(raw));
}
