using System.Collections.ObjectModel;
using System.Globalization;

namespace GenerativeArt.Application.Catalog;

/// <summary>
/// Parametro con tipo dichiarato e valori serializzabili in cultura invariante.
/// Il formato canonico è testuale per la CLI; le ricette JSON tipizzate arriveranno in M3C.2.
/// </summary>
public sealed class ArtworkParameterSpec
{
    private readonly ReadOnlyCollection<string> _choices;

    public string Id { get; }
    public string DisplayName { get; }
    public string Group { get; }
    public ParameterKind Kind { get; }
    public string DefaultValue { get; }
    public string? Minimum { get; }
    public string? Maximum { get; }
    public IReadOnlyList<string> Choices => _choices;
    /// <summary>I parametri aggiunti dopo le ricette v1 possono mancare nei JSON storici.</summary>
    public bool OptionalInJson { get; }
    /// <summary>Una sovrascrittura cromatica accetta "auto" per usare la palette scelta.</summary>
    public bool AllowAutoColor { get; }

    private ArtworkParameterSpec(string id, string displayName, string group, ParameterKind kind,
        string defaultValue, string? minimum, string? maximum, IEnumerable<string>? choices,
        bool optionalInJson = false, bool allowAutoColor = false)
    {
        ArtworkIds.Validate(id, nameof(id));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Nome parametro obbligatorio.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(group)) throw new ArgumentException("Gruppo obbligatorio.", nameof(group));
        Id = id;
        DisplayName = displayName;
        Group = group;
        Kind = kind;
        DefaultValue = defaultValue;
        Minimum = minimum;
        Maximum = maximum;
        OptionalInJson = optionalInJson;
        AllowAutoColor = allowAutoColor;
        string[] values = choices?.ToArray() ?? [];
        if (values.Any(string.IsNullOrWhiteSpace) || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException("Scelte vuote o duplicate.", nameof(choices));
        _choices = Array.AsReadOnly(values);
        ValidateValue(defaultValue);
    }

    public static ArtworkParameterSpec Integer(string id, string displayName, int defaultValue,
        int minimum, int maximum, string group = "Generale", bool optionalInJson = false)
    {
        if (minimum > maximum || defaultValue < minimum || defaultValue > maximum)
            throw new ArgumentOutOfRangeException(nameof(defaultValue), "Default intero fuori intervallo.");
        return new(id, displayName, group, ParameterKind.Integer,
            defaultValue.ToString(CultureInfo.InvariantCulture),
            minimum.ToString(CultureInfo.InvariantCulture),
            maximum.ToString(CultureInfo.InvariantCulture), null, optionalInJson);
    }

    public static ArtworkParameterSpec Number(string id, string displayName, double defaultValue,
        double minimum, double maximum, string group = "Generale", bool optionalInJson = false)
    {
        if (!double.IsFinite(defaultValue) || !double.IsFinite(minimum) || !double.IsFinite(maximum)
            || minimum > maximum || defaultValue < minimum || defaultValue > maximum)
            throw new ArgumentOutOfRangeException(nameof(defaultValue), "Default numerico o limiti non validi.");
        return new(id, displayName, group, ParameterKind.Number,
            defaultValue.ToString("R", CultureInfo.InvariantCulture),
            minimum.ToString("R", CultureInfo.InvariantCulture),
            maximum.ToString("R", CultureInfo.InvariantCulture), null, optionalInJson);
    }

    public static ArtworkParameterSpec Boolean(string id, string displayName, bool defaultValue,
        string group = "Generale") => new(id, displayName, group, ParameterKind.Boolean,
            defaultValue ? "true" : "false", null, null, null);

    public static ArtworkParameterSpec Color(string id, string displayName, string defaultValue,
        string group = "Colori") => new(id, displayName, group, ParameterKind.Color,
            defaultValue, null, null, null);

    public static ArtworkParameterSpec ColorOverride(string id, string displayName,
        string group = "Colori") => new(id, displayName, group, ParameterKind.Color,
            "auto", null, null, null, optionalInJson: true, allowAutoColor: true);

    public static ArtworkParameterSpec Choice(string id, string displayName, string defaultValue,
        IEnumerable<string> options, string group = "Generale", bool optionalInJson = false) => new(id, displayName, group,
        ParameterKind.Choice, defaultValue, null, null, options, optionalInJson);

    /// <summary>Verifica il tipo e l'intervallo; restituisce una rappresentazione canonica senza cultura locale.</summary>
    public string ValidateValue(string? rawValue)
    {
        if (rawValue is null || rawValue.Length > 128)
            throw new ArgumentException($"Valore mancante o troppo lungo per '{Id}'.", nameof(rawValue));
        switch (Kind)
        {
            case ParameterKind.Integer:
                if (!int.TryParse(rawValue, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int i)
                    || i < int.Parse(Minimum!, CultureInfo.InvariantCulture)
                    || i > int.Parse(Maximum!, CultureInfo.InvariantCulture))
                    throw new ArgumentException($"Intero fuori intervallo per '{Id}'.", nameof(rawValue));
                return i.ToString(CultureInfo.InvariantCulture);
            case ParameterKind.Number:
                if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double n)
                    || !double.IsFinite(n)
                    || n < double.Parse(Minimum!, CultureInfo.InvariantCulture)
                    || n > double.Parse(Maximum!, CultureInfo.InvariantCulture))
                    throw new ArgumentException($"Numero non valido per '{Id}'.", nameof(rawValue));
                return n.ToString("R", CultureInfo.InvariantCulture);
            case ParameterKind.Boolean:
                if (!bool.TryParse(rawValue, out bool b))
                    throw new ArgumentException($"Booleano non valido per '{Id}'.", nameof(rawValue));
                return b ? "true" : "false";
            case ParameterKind.Color:
                if (AllowAutoColor && string.Equals(rawValue, "auto", StringComparison.OrdinalIgnoreCase))
                    return "auto";
                if ((rawValue.Length != 7 && rawValue.Length != 9) || rawValue[0] != '#')
                    throw new ArgumentException($"Colore non valido per '{Id}', atteso #RRGGBB o #AARRGGBB.", nameof(rawValue));
                for (int k = 1; k < rawValue.Length; k++)
                    if (!Uri.IsHexDigit(rawValue[k]))
                        throw new ArgumentException($"Colore non valido per '{Id}'.", nameof(rawValue));
                return rawValue.ToUpperInvariant();
            case ParameterKind.Choice:
                if (!_choices.Contains(rawValue, StringComparer.Ordinal))
                    throw new ArgumentException($"Scelta non disponibile per '{Id}'.", nameof(rawValue));
                return rawValue;
            default:
                throw new InvalidOperationException("Tipo parametro sconosciuto.");
        }
    }
}
