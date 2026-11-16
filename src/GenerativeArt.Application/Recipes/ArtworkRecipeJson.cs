using System.Globalization;
using System.Text;
using System.Text.Json;
using GenerativeArt.Application.Catalog;
using GenerativeArt.Core.Geometry;

namespace GenerativeArt.Application.Recipes;

/// <summary>
/// Codec JSON v1 rigoroso: niente proprietà ignote/duplicate, valori con tipi JSON reali,
/// seed UInt64 decimale come stringa, chiavi dei parametri ordinate e output canonico.
/// </summary>
public static class ArtworkRecipeJson
{
    public const int MaximumUtf8Bytes = 65_536;

    private static readonly string[] RootNames =
        ["formatVersion", "artworkId", "algorithmVersion", "seed", "width", "height", "parameters", "animation"];
    private static readonly string[] AnimationNames = ["frameCount", "framesPerSecond"];

    public static byte[] Serialize(ArtworkRecipe recipe, ArtworkRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        recipe.ValidateAgainst(registry);
        ArtworkDescriptor descriptor = registry.Get(recipe.ArtworkId);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("formatVersion", recipe.FormatVersion);
            writer.WriteString("artworkId", recipe.ArtworkId);
            writer.WriteNumber("algorithmVersion", recipe.AlgorithmVersion);
            writer.WriteString("seed", recipe.Seed.ToString(CultureInfo.InvariantCulture));
            writer.WriteNumber("width", recipe.Size.Width);
            writer.WriteNumber("height", recipe.Size.Height);
            writer.WritePropertyName("parameters");
            writer.WriteStartObject();
            foreach (KeyValuePair<string, string> parameter in recipe.Parameters)
            {
                ArtworkParameterSpec spec = descriptor.Parameters.Single(p => p.Id == parameter.Key);
                switch (spec.Kind)
                {
                    case ParameterKind.Integer:
                        writer.WriteNumber(parameter.Key, int.Parse(parameter.Value, CultureInfo.InvariantCulture));
                        break;
                    case ParameterKind.Number:
                        writer.WriteNumber(parameter.Key, double.Parse(parameter.Value, CultureInfo.InvariantCulture));
                        break;
                    case ParameterKind.Boolean:
                        writer.WriteBoolean(parameter.Key, bool.Parse(parameter.Value));
                        break;
                    case ParameterKind.Color:
                    case ParameterKind.Choice:
                        writer.WriteString(parameter.Key, parameter.Value);
                        break;
                    default:
                        throw new InvalidOperationException("Tipo parametro sconosciuto.");
                }
            }
            writer.WriteEndObject();
            if (recipe.Animation is not null)
            {
                writer.WritePropertyName("animation");
                writer.WriteStartObject();
                writer.WriteNumber("frameCount", recipe.Animation.FrameCount);
                writer.WriteNumber("framesPerSecond", recipe.Animation.FramesPerSecond);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
        byte[] bytes = stream.ToArray();
        if (bytes.Length > MaximumUtf8Bytes)
            throw new InvalidOperationException("Ricetta superiore al limite massimo consentito.");
        return bytes;
    }

    public static ArtworkRecipe Deserialize(ReadOnlyMemory<byte> utf8, ArtworkRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (utf8.Length == 0 || utf8.Length > MaximumUtf8Bytes)
            throw new ArgumentException("Ricetta JSON vuota o troppo grande.", nameof(utf8));
        using JsonDocument document = JsonDocument.Parse(utf8, new JsonDocumentOptions
        {
            AllowTrailingCommas = false, CommentHandling = JsonCommentHandling.Disallow, MaxDepth = 12
        });
        Dictionary<string, JsonElement> root = StrictProperties(document.RootElement, RootNames, "radice");
        int format = ReadInt(Required(root, "formatVersion"), "formatVersion");
        if (format != ArtworkRecipe.CurrentFormatVersion)
            throw new NotSupportedException($"Formato ricetta {format} non supportato: atteso v1.");
        string id = ReadString(Required(root, "artworkId"), "artworkId");
        if (!registry.TryGet(id, out ArtworkDescriptor? descriptor) || descriptor is null)
            throw new ArgumentException($"Opera non registrata: '{id}'.");
        int algorithmVersion = ReadInt(Required(root, "algorithmVersion"), "algorithmVersion");
        if (algorithmVersion != descriptor.AlgorithmVersion)
            throw new NotSupportedException($"Versione algoritmo di '{id}' non supportata: " +
                $"v{algorithmVersion} anziché v{descriptor.AlgorithmVersion}.");
        string seedText = ReadString(Required(root, "seed"), "seed");
        if (seedText.Length == 0 || (seedText.Length > 1 && seedText[0] == '0') ||
            seedText.Any(c => c < '0' || c > '9') ||
            !ulong.TryParse(seedText, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed))
            throw new ArgumentException("Seed non valido: richiesto UInt64 decimale canonico.");
        var size = new CanvasSize(ReadInt(Required(root, "width"), "width"),
            ReadInt(Required(root, "height"), "height"));
        Dictionary<string, JsonElement> parameterProperties = StrictProperties(
            Required(root, "parameters"), descriptor.Parameters.Select(p => p.Id), "parameters");
        var supplied = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (ArtworkParameterSpec parameter in descriptor.Parameters)
        {
            if (!parameterProperties.TryGetValue(parameter.Id, out JsonElement value))
            {
                // Ricette v1 precedenti all'introduzione dei controlli avanzati:
                // soltanto i nuovi parametri marcati opzionali ricevono il default.
                if (parameter.OptionalInJson) continue;
                throw new ArgumentException($"Parametro richiesto mancante: '{parameter.Id}'.");
            }
            supplied.Add(parameter.Id, ReadParameter(value, parameter));
        }
        RecipeAnimation? animation = null;
        if (root.TryGetValue("animation", out JsonElement animationJson))
        {
            Dictionary<string, JsonElement> fields = StrictProperties(animationJson, AnimationNames, "animation");
            animation = new RecipeAnimation(
                ReadInt(Required(fields, "frameCount"), "frameCount"),
                ReadInt(Required(fields, "framesPerSecond"), "framesPerSecond"));
        }
        return ArtworkRecipe.Create(descriptor, size, seed, supplied, animation);
    }

    public static ArtworkRecipe Deserialize(string json, ArtworkRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(json);
        // Il numero di byte UTF-8 non può essere minore del numero di code unit UTF-16:
        // rifiutiamo stringhe manifestamente troppo grandi PRIMA dell'allocazione UTF-8.
        if (json.Length > MaximumUtf8Bytes)
            throw new ArgumentException("Ricetta JSON troppo grande.", nameof(json));
        return Deserialize(Encoding.UTF8.GetBytes(json), registry);
    }

    private static string ReadParameter(JsonElement value, ArtworkParameterSpec parameter)
    {
        return parameter.Kind switch
        {
            ParameterKind.Integer or ParameterKind.Number when value.ValueKind == JsonValueKind.Number =>
                parameter.ValidateValue(value.GetRawText()),
            ParameterKind.Boolean when value.ValueKind is JsonValueKind.True or JsonValueKind.False =>
                parameter.ValidateValue(value.GetBoolean() ? "true" : "false"),
            ParameterKind.Color or ParameterKind.Choice when value.ValueKind == JsonValueKind.String =>
                parameter.ValidateValue(ReadString(value, parameter.Id)),
            _ => throw new ArgumentException($"Tipo JSON non valido per '{parameter.Id}'.")
        };
    }

    private static Dictionary<string, JsonElement> StrictProperties(JsonElement element,
        IEnumerable<string> acceptedNames, string section)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException($"La sezione '{section}' deve essere un oggetto JSON.");
        var accepted = new HashSet<string>(acceptedNames, StringComparer.Ordinal);
        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!accepted.Contains(property.Name))
                throw new ArgumentException($"Proprietà sconosciuta in '{section}': '{property.Name}'.");
            if (!result.TryAdd(property.Name, property.Value))
                throw new ArgumentException($"Proprietà duplicata in '{section}': '{property.Name}'.");
        }
        return result;
    }

    private static JsonElement Required(Dictionary<string, JsonElement> values, string name) =>
        values.TryGetValue(name, out JsonElement value)
            ? value : throw new ArgumentException($"Proprietà obbligatoria mancante: '{name}'.");

    private static int ReadInt(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int number))
            throw new ArgumentException($"'{name}' deve essere un intero JSON a 32 bit.");
        return number;
    }

    private static string ReadString(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.String)
            throw new ArgumentException($"'{name}' deve essere una stringa JSON.");
        string? text = value.GetString();
        if (text is null || text.Length > 128)
            throw new ArgumentException($"Stringa non valida o troppo lunga per '{name}'.");
        return text;
    }
}
