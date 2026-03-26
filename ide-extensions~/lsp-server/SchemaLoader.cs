using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UitkxLanguageServer;

/// <summary>
/// Loads and exposes the uitkx-schema.json embedded resource.
/// </summary>
public sealed class UitkxSchema
{
    // ── Deserialization types ────────────────────────────────────────────────

    public sealed class AttributeInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("sinceUnity")]
        public string? SinceUnity { get; set; }

        [JsonPropertyName("deprecatedIn")]
        public string? DeprecatedIn { get; set; }

        [JsonPropertyName("removedIn")]
        public string? RemovedIn { get; set; }
    }

    public sealed class ElementInfo
    {
        [JsonPropertyName("propsType")]
        public string PropsType { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("acceptsChildren")]
        public bool AcceptsChildren { get; set; }

        [JsonPropertyName("attributes")]
        public List<AttributeInfo> Attributes { get; set; } = new();

        [JsonPropertyName("sinceUnity")]
        public string? SinceUnity { get; set; }

        [JsonPropertyName("deprecatedIn")]
        public string? DeprecatedIn { get; set; }

        [JsonPropertyName("removedIn")]
        public string? RemovedIn { get; set; }
    }

    public sealed class DirectiveInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }

    public sealed class SchemaRoot
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("universalAttributes")]
        public List<AttributeInfo> UniversalAttributes { get; set; } = new();

        [JsonPropertyName("directives")]
        public List<DirectiveInfo> Directives { get; set; } = new();

        [JsonPropertyName("controlFlow")]
        public List<DirectiveInfo> ControlFlow { get; set; } = new();

        [JsonPropertyName("elements")]
        public Dictionary<string, ElementInfo> Elements { get; set; } = new();

        [JsonPropertyName("styleKeyValues")]
        public Dictionary<string, List<string>> StyleKeyValues { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Optional per-style-property version annotations.
        /// Maps camelCase style key names (e.g. "filter", "aspectRatio") to version metadata.
        /// Entries absent from this dictionary are assumed to be available since the floor version.
        /// </summary>
        [JsonPropertyName("styleVersions")]
        public Dictionary<string, VersionInfo> StyleVersions { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Version lifecycle metadata for a schema entry (element, attribute, or style property).
    /// All fields are optional — <c>null</c> means "available since the floor version" or
    /// "not deprecated / not removed".
    /// </summary>
    public sealed class VersionInfo
    {
        /// <summary>First Unity version where this feature is available (e.g. "6000.3").</summary>
        [JsonPropertyName("sinceUnity")]
        public string? SinceUnity { get; set; }

        /// <summary>Unity version where this feature was deprecated (still compiles, but warned).</summary>
        [JsonPropertyName("deprecatedIn")]
        public string? DeprecatedIn { get; set; }

        /// <summary>Unity version where this feature was removed (won't compile).</summary>
        [JsonPropertyName("removedIn")]
        public string? RemovedIn { get; set; }

        /// <summary>Suggested replacement when deprecated (e.g. "filter" replaces "unityBackgroundImageTintColor").</summary>
        [JsonPropertyName("replacedBy")]
        public string? ReplacedBy { get; set; }
    }

    // ── Public surface ───────────────────────────────────────────────────────

    public SchemaRoot Root { get; }

    public UitkxSchema()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream =
            asm.GetManifestResourceStream("uitkx-schema.json")
            ?? throw new InvalidOperationException(
                "Embedded resource uitkx-schema.json not found."
            );

        Root =
            JsonSerializer.Deserialize<SchemaRoot>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? throw new InvalidOperationException("Failed to deserialize uitkx-schema.json.");

        // Rebuild Elements with case-insensitive keys so <Button> and <button> both resolve
        Root.Elements = new Dictionary<string, ElementInfo>(
            Root.Elements,
            StringComparer.OrdinalIgnoreCase
        );
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    public ElementInfo? TryGetElement(string tagName) =>
        Root.Elements.TryGetValue(tagName, out var el) ? el : null;

    public IEnumerable<AttributeInfo> GetAttributesForElement(string tagName)
    {
        var el = TryGetElement(tagName);
        if (el is null)
            return Enumerable.Empty<AttributeInfo>();
        return el.Attributes.Concat(Root.UniversalAttributes);
    }

    // ── Version-awareness helpers ────────────────────────────────────────────

    /// <summary>
    /// Returns the minimum Unity version required for a schema element,
    /// or <see cref="UnityVersion.Unknown"/> if no annotation exists (floor version).
    /// </summary>
    public UnityVersion GetElementMinVersion(string tagName)
    {
        var el = TryGetElement(tagName);
        if (el?.SinceUnity is null)
            return UnityVersion.Unknown;
        return UnityVersion.TryParse(el.SinceUnity, out var v) ? v : UnityVersion.Unknown;
    }

    /// <summary>
    /// Returns the minimum Unity version required for a style property,
    /// or <see cref="UnityVersion.Unknown"/> if no annotation exists (floor version).
    /// </summary>
    public UnityVersion GetStyleMinVersion(string camelCaseKey)
    {
        if (!Root.StyleVersions.TryGetValue(camelCaseKey, out var info))
            return UnityVersion.Unknown;
        if (info.SinceUnity is null)
            return UnityVersion.Unknown;
        return UnityVersion.TryParse(info.SinceUnity, out var v) ? v : UnityVersion.Unknown;
    }

    /// <summary>
    /// Returns version metadata for a style property, or <c>null</c> if none exists.
    /// </summary>
    public VersionInfo? GetStyleVersionInfo(string camelCaseKey) =>
        Root.StyleVersions.TryGetValue(camelCaseKey, out var info) ? info : null;

    /// <summary>
    /// Checks whether an element is available for the given Unity version.
    /// Returns <c>true</c> if no version annotation exists (assumed floor).
    /// </summary>
    public bool IsElementAvailable(string tagName, UnityVersion userVersion)
    {
        if (!userVersion.IsKnown)
            return true; // can't filter if we don't know the user's version
        var minVersion = GetElementMinVersion(tagName);
        return !minVersion.IsKnown || userVersion >= minVersion;
    }

    /// <summary>
    /// Checks whether a style property is available for the given Unity version.
    /// Returns <c>true</c> if no version annotation exists (assumed floor).
    /// </summary>
    public bool IsStyleAvailable(string camelCaseKey, UnityVersion userVersion)
    {
        if (!userVersion.IsKnown)
            return true;
        var minVersion = GetStyleMinVersion(camelCaseKey);
        return !minVersion.IsKnown || userVersion >= minVersion;
    }
}
