using GenerativeArt.Application.Catalog;

namespace GenerativeArt.Application.Recipes;

/// <summary>Persistenza con limiti di lettura e creazione senza sovrascrittura implicita.</summary>
public static class ArtworkRecipeFiles
{
    public static ArtworkRecipe Load(string path, ArtworkRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[4096];
        int read;
        while ((read = stream.Read(chunk, 0, chunk.Length)) != 0)
        {
            if (buffer.Length + read > ArtworkRecipeJson.MaximumUtf8Bytes)
                throw new InvalidDataException("File ricetta troppo grande (max 64 KiB).");
            buffer.Write(chunk, 0, read);
        }
        return ArtworkRecipeJson.Deserialize(buffer.ToArray(), registry);
    }

    public static void SaveNew(string path, ArtworkRecipe recipe, ArtworkRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        byte[] content = ArtworkRecipeJson.Serialize(recipe, registry);
        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (directory is not null) Directory.CreateDirectory(directory);
        bool created = false;
        try
        {
            using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            created = true;
            stream.Write(content);
        }
        catch
        {
            if (created) File.Delete(fullPath);
            throw;
        }
    }
}
