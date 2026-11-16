using System.Globalization;
using System.Text.Json;
using GenerativeArt.Application.Recipes;
using GenerativeArt.Application.Rendering;

namespace GenerativeArt.Cli;

/// <summary>
/// Scrittura senza sovrascrivere file esistenti. Manifest scritto per ultimo:
/// la sua presenza segnala che tutte le immagini sono state prodotte correttamente.
/// Su errore vengono rimossi solo i file creati da questa invocazione.
/// </summary>
internal static class RecipeOutputWriter
{
    public static void Write(ArtworkRecipe recipe, string target, IArtworkRasterRenderer renderer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentException.ThrowIfNullOrWhiteSpace(target);
        // Budget verificato prima di creare qualunque output.
        renderer.Validate(recipe, recipe.Animation is null ? null : 0);
        if (recipe.Animation is null)
            WriteSingle(recipe, target, renderer, cancellationToken);
        else
            WriteFrames(recipe, target, renderer, cancellationToken);
    }

    private static void WriteSingle(ArtworkRecipe recipe, string target,
        IArtworkRasterRenderer renderer, CancellationToken ct)
    {
        string full = Path.GetFullPath(target);
        if (Directory.Exists(full))
            throw new IOException("La destinazione PNG è una directory.");
        string? parent = Path.GetDirectoryName(full);
        if (parent is not null) Directory.CreateDirectory(parent);
        bool owned = false;
        try
        {
            using (var destination = new FileStream(full, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                owned = true;
                renderer.Render(recipe, destination, cancellationToken: ct);
            }
        }
        catch
        {
            if (owned) File.Delete(full);
            throw;
        }
        Console.WriteLine($"PNG generato: {full} ({recipe.Size.Width}x{recipe.Size.Height}, seed={recipe.Seed})");
    }

    private static void WriteFrames(ArtworkRecipe recipe, string target,
        IArtworkRasterRenderer renderer, CancellationToken ct)
    {
        string full = Path.GetFullPath(target);
        if (File.Exists(full)) throw new IOException("La destinazione frame è un file.");
        if (Directory.Exists(full) && Directory.EnumerateFileSystemEntries(full).Any())
            throw new IOException("La directory dei frame deve essere nuova o vuota.");
        bool createdDirectory = !Directory.Exists(full);
        Directory.CreateDirectory(full);
        RecipeAnimation animation = recipe.Animation ?? throw new ArgumentException("Animazione richiesta.", nameof(recipe));
        var createdFiles = new List<string>();
        try
        {
            for (int i = 0; i < animation.FrameCount; i++)
            {
                ct.ThrowIfCancellationRequested();
                string path = Path.Combine(full, $"frame_{i:D4}.png");
                using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    createdFiles.Add(path);
                    renderer.Render(recipe, stream, frameIndex: i, cancellationToken: ct);
                }
            }

            string manifestPath = Path.Combine(full, "manifest.json");
            using (var stream = new FileStream(manifestPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                createdFiles.Add(manifestPath);
                var manifest = new
                {
                    Format = "generativeart-loop-frames-v1",
                    ArtworkId = recipe.ArtworkId,
                    AlgorithmVersion = recipe.AlgorithmVersion,
                    Seed = recipe.Seed.ToString(CultureInfo.InvariantCulture),
                    Width = recipe.Size.Width,
                    Height = recipe.Size.Height,
                    FrameCount = animation.FrameCount,
                    FramesPerSecond = animation.FramesPerSecond,
                    LoopRadius = double.Parse(recipe.Parameters["loop-radius"], CultureInfo.InvariantCulture),
                    Palette = recipe.Parameters["palette"],
                    PhaseFormula = "index / frameCount (first frame not duplicated)"
                };
                JsonSerializer.Serialize(stream, manifest, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch
        {
            foreach (string path in createdFiles)
                if (File.Exists(path)) File.Delete(path);
            if (createdDirectory && !Directory.EnumerateFileSystemEntries(full).Any())
                Directory.Delete(full);
            throw;
        }
        Console.WriteLine($"Loop periodico: {animation.FrameCount} PNG in {full} " +
            $"({recipe.Size.Width}x{recipe.Size.Height}, {animation.FramesPerSecond} fps, seed={recipe.Seed}).");
    }
}
