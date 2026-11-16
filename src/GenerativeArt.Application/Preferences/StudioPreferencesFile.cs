using System.Text.Json;
using GenerativeArt.Application.Files;

namespace GenerativeArt.Application.Preferences;

/// <summary>
/// Preferenze locali versionate e limitate a 64 KiB; pubblicazione atomica.
/// La perdita/corruzione delle preferenze non deve compromettere le ricette.
/// </summary>
public sealed class StudioPreferencesFile
{
    public const int SchemaVersion = 1;
    public const int MaximumBytes = 64 * 1024;
    private readonly string _path;

    public StudioPreferencesFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GenerativeArt", "studio-settings-v1.json");

    public StudioPreferences Load()
    {
        if (!File.Exists(_path)) return new StudioPreferences();
        using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length > MaximumBytes)
            throw new InvalidDataException("Preferenze Studio oltre il limite di 64 KiB.");
        byte[] bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        using var doc = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 8 });
        JsonElement root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Le preferenze devono essere un oggetto JSON.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in root.EnumerateObject())
            if (!seen.Add(property.Name) || property.Name is not
                ("schemaVersion" or "theme" or "favorites" or "recentRecipes"))
                throw new InvalidDataException("Proprietà preferenze duplicata o sconosciuta.");
        if (!root.TryGetProperty("schemaVersion", out var version) ||
            version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out int v) || v != SchemaVersion)
            throw new InvalidDataException("Versione del file preferenze non supportata.");
        if (!root.TryGetProperty("theme", out var theme) || theme.ValueKind != JsonValueKind.String ||
            theme.GetString() is not ("dark" or "light"))
            throw new InvalidDataException("Tema preferenze non valido.");
        string[] favorites = ReadStrings(root, "favorites", StudioPreferences.MaximumFavorites);
        string[] recents = ReadStrings(root, "recentRecipes", StudioPreferences.MaximumRecentRecipes);
        return new StudioPreferences(theme.GetString() == "light", favorites, recents);
    }

    public Task SaveAsync(StudioPreferences preferences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new
        {
            schemaVersion = SchemaVersion,
            theme = preferences.UseLightTheme ? "light" : "dark",
            favorites = preferences.FavoriteArtworkIds,
            recentRecipes = preferences.RecentRecipePaths
        }, new JsonSerializerOptions { WriteIndented = true });
        if (json.Length > MaximumBytes)
            throw new InvalidDataException("Preferenze Studio oltre il limite di 64 KiB.");
        return AtomicOutputFiles.WriteAsync(_path, json, overwrite: true, cancellationToken: cancellationToken);
    }

    private static string[] ReadStrings(JsonElement root, string name, int maximum)
    {
        if (!root.TryGetProperty(name, out var array) || array.ValueKind != JsonValueKind.Array ||
            array.GetArrayLength() > maximum)
            throw new InvalidDataException($"Elenco preferenze '{name}' non valido.");
        var values = new List<string>();
        foreach (JsonElement item in array.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String || item.GetString() is not string value)
                throw new InvalidDataException($"Elemento non valido in '{name}'.");
            values.Add(value);
        }
        return values.ToArray();
    }
}
