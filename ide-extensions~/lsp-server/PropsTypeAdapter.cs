using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUITK.Language.Roslyn;

namespace UitkxLanguageServer;

/// <summary>
/// Implements <see cref="IPropsTypeProvider"/> by combining the static
/// <see cref="UitkxSchema"/> (built-in elements) with the dynamic
/// <see cref="WorkspaceIndex"/> (user-defined components) to provide
/// element → props-type and attribute → property-name mappings.
/// </summary>
public sealed class PropsTypeAdapter : IPropsTypeProvider
{
    private readonly UitkxSchema _schema;
    private readonly WorkspaceIndex _index;

    /// <summary>
    /// Universal attribute names that exist on every element but are NOT
    /// declared as typed C# properties on the props classes (they are
    /// handled by the framework at a lower level).  Trying to emit
    /// <c>__p.Key = ...</c> would produce a false CS1061 error.
    /// </summary>
    private static readonly HashSet<string> s_universalSkip =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "key",
            "ref",
            "className",
        };

    public PropsTypeAdapter(UitkxSchema schema, WorkspaceIndex index)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _index = index ?? throw new ArgumentNullException(nameof(index));
    }

    /// <inheritdoc/>
    public string? GetPropsType(string elementName)
    {
        // 1. Try the static schema first (built-in elements)
        var schemaEl = _schema.TryGetElement(elementName);
        if (schemaEl != null && !string.IsNullOrEmpty(schemaEl.PropsType))
            return schemaEl.PropsType;

        // 2. Try the workspace index (user components: StatusBar → StatusBarProps)
        var indexEl = _index.TryGetElementInfo(elementName);
        if (indexEl != null)
            return elementName + "Props";

        return null;
    }

    /// <inheritdoc/>
    public string? GetPropName(string elementName, string attributeName)
    {
        // Skip universal attributes that aren't typed props properties
        if (s_universalSkip.Contains(attributeName))
            return null;

        // Skip event callback attributes — they use Action/delegate types
        // and are already handled well by the lambda emission path.
        if (attributeName.StartsWith("on", StringComparison.OrdinalIgnoreCase)
            && attributeName.Length > 2
            && char.IsUpper(attributeName[2]))
            return null;

        // 1. Try the static schema
        var schemaEl = _schema.TryGetElement(elementName);
        if (schemaEl != null && !string.IsNullOrEmpty(schemaEl.PropsType))
        {
            var attr = schemaEl.Attributes
                .FirstOrDefault(a => string.Equals(a.Name, attributeName, StringComparison.OrdinalIgnoreCase));
            if (attr != null)
                return ToPropName(attr.Name);
            // Attribute not in schema element-specific list — might be universal or unknown.
            return null;
        }

        // 2. Try the workspace index
        var props = _index.GetProps(elementName);
        if (props != null && props.Count > 0)
        {
            var prop = props
                .FirstOrDefault(p => string.Equals(p.Name, attributeName, StringComparison.OrdinalIgnoreCase));
            if (prop != null)
                return ToPropName(prop.Name);
            return null;
        }

        return null;
    }

    /// <inheritdoc/>
    public string? GetPropType(string elementName, string attributeName)
    {
        // Skip universal attributes and event callbacks (same rules as GetPropName)
        if (s_universalSkip.Contains(attributeName))
            return null;
        if (attributeName.StartsWith("on", StringComparison.OrdinalIgnoreCase)
            && attributeName.Length > 2
            && char.IsUpper(attributeName[2]))
            return null;

        // 1. Try the static schema (built-in elements)
        var schemaEl = _schema.TryGetElement(elementName);
        if (schemaEl != null && !string.IsNullOrEmpty(schemaEl.PropsType))
        {
            var attr = schemaEl.Attributes
                .FirstOrDefault(a => string.Equals(a.Name, attributeName, StringComparison.OrdinalIgnoreCase));
            if (attr != null && !string.IsNullOrEmpty(attr.Type))
                return attr.Type;
            return null;
        }

        // 2. Try the workspace index (function-style components)
        var props = _index.GetProps(elementName);
        if (props != null && props.Count > 0)
        {
            var prop = props
                .FirstOrDefault(p => string.Equals(p.Name, attributeName, StringComparison.OrdinalIgnoreCase));
            if (prop != null && !string.IsNullOrEmpty(prop.Type))
                return prop.Type;
            return null;
        }

        return null;
    }

    /// <summary>
    /// Converts an attribute name (camelCase) to a C# property name (PascalCase)
    /// by uppercasing the first character.
    /// </summary>
    private static string ToPropName(string attrName)
    {
        if (string.IsNullOrEmpty(attrName))
            return attrName;
        return char.ToUpperInvariant(attrName[0]) + attrName.Substring(1);
    }
}
