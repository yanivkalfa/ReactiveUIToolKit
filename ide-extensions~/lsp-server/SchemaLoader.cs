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
}
