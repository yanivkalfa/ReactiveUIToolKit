using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Every element registered in <c>ElementRegistryProvider</c> must have an entry in
    /// <c>uitkx-schema.json</c>. The schema drives the EDITOR's completion, hover, and the
    /// UITKX0109 unknown-attribute check: a registered-but-unschema'd element falls into the
    /// user-component branch (declared-params-only), so every BaseProps attribute on it —
    /// <c>onClick</c>, <c>style</c>, … — false-flags as UITKX0109 in the IDE while the build
    /// is clean (the SG resolves BaseProps semantically). 30 elements had drifted this way
    /// (the whole Toolbar family, MultiColumn views, ObjectField, TwoPaneSplitView, …) before
    /// this gate existed.
    /// </summary>
    public sealed class SchemaRegistryParityTests
    {
        private static string WorkspaceRoot([CallerFilePath] string thisFile = "")
        {
            var dir = Path.GetDirectoryName(thisFile)!; // Tests/
            return Path.GetFullPath(Path.Combine(dir, "../..")); // workspace root
        }

        [Fact]
        public void EveryRegisteredElement_HasASchemaEntry()
        {
            string root = WorkspaceRoot();
            string registrySource = File.ReadAllText(
                Path.Combine(root, "Shared", "Elements", "ElementRegistryProvider.cs"));
            string schemaJson = File.ReadAllText(
                Path.Combine(root, "ide-extensions~", "grammar", "uitkx-schema.json"));

            var registered = Regex
                .Matches(registrySource, "RegisterIfAllowed\\(registry, \"([A-Za-z]+)\"")
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();
            Assert.True(registered.Count > 0, "regex found no registrations — registry file shape changed?");

            // Element names are the keys of the schema's `elements` object. A simple
            // containment probe on the raw JSON ("  \"Name\": {") is exact enough and avoids
            // a JSON-parser dependency; the quoted-key shape is stable (2-space formatting).
            var missing = registered
                .Where(name => !schemaJson.Contains($"\"{name}\": {{")
                            && !schemaJson.Contains($"\"{name}\":{{"))
                .ToList();

            Assert.True(
                missing.Count == 0,
                "Registered elements missing from uitkx-schema.json (editor will false-flag " +
                "UITKX0109 on their BaseProps attributes): " + string.Join(", ", missing) +
                ". Add entries (propsType/description/acceptsChildren/attributes) — see " +
                "the add-unity-version skill's new-element checklist.");
        }
    }
}
