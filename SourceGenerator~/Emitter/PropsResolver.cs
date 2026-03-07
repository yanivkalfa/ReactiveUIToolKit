using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Phase 3 — maps markup tag names to the V.* call pattern that the
    /// <see cref="CSharpEmitter"/> will generate.
    ///
    /// Strategy B (Roslyn symbol inspection):
    ///   1. Scans the <c>ReactiveUITK.V</c> class at resolver-construction time to
    ///      build a lowercase tag-name → <see cref="TagResolution"/> map for all
    ///      built-in elements.
    ///   2. For PascalCase tags (function components), checks whether a type with that
    ///      name and an appropriate <c>Render</c> static method exists in the
    ///      compilation.  Emits <c>UITKX0008</c> if the type cannot be found.
    ///   3. Falls back to a hardcoded built-in map when the V class cannot be located
    ///      in the compilation (e.g. the shared assembly is not yet compiled).
    /// </summary>
    public sealed class PropsResolver
    {
        private readonly Compilation _compilation;

        /// <summary>Lowercase tag → TagResolution for every V.* built-in.</summary>
        private readonly Dictionary<string, TagResolution> _builtinMap;

        // ── Roslyn type-name constants ────────────────────────────────────────
        private const string VTypeName = "ReactiveUITK.V";
        private const string VirtualNodeName = "VirtualNode";

        // ── Fallback hard-coded map (used when V type not resolvable) ─────────
        private static readonly IReadOnlyDictionary<string, TagResolution> s_fallbackMap =
            BuildFallbackMap();

        public PropsResolver(Compilation compilation)
        {
            _compilation = compilation;
            _builtinMap = BuildBuiltinMapFromCompilation(compilation);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves <paramref name="tagName"/> (as it appears in markup) to a
        /// <see cref="TagResolution"/> describing how to emit the V.* call.
        /// </summary>
        /// <param name="tagName">Raw tag name from the .uitkx file.</param>
        /// <param name="usingNamespaces">
        ///   @using namespaces from the DirectiveSet, used when searching for
        ///   function-component types.
        /// </param>
        /// <param name="unknownDiagnostic">
        ///   When the tag cannot be verified, set to a <see cref="DiagnosticDescriptor"/>
        ///   (caller should create the <see cref="Diagnostic"/> with source location).
        /// </param>
        public TagResolution Resolve(
            string tagName,
            ImmutableArray<string> usingNamespaces,
            out DiagnosticDescriptor? unknownDiagnostic
        )
        {
            unknownDiagnostic = null;

            // Empty tag name = <></> fragment shorthand
            if (string.IsNullOrEmpty(tagName))
                return new TagResolution(
                    TagResolutionKind.Fragment,
                    "Fragment",
                    null,
                    AcceptsChildren: true
                );

            // "Fragment" tag = explicit fragment
            if (string.Equals(tagName, "Fragment", StringComparison.OrdinalIgnoreCase))
                return new TagResolution(
                    TagResolutionKind.Fragment,
                    "Fragment",
                    null,
                    AcceptsChildren: true
                );

            // ── Built-in lookup (lowercase match) ────────────────────────────
            if (_builtinMap.TryGetValue(tagName, out var builtinResolution))
                return builtinResolution;

            // ── Lowercase unknown → defer to C# compiler, emit UITKX0001 ─────
            if (!char.IsUpper(tagName[0]))
            {
                unknownDiagnostic = UitkxDiagnostics.UnknownElement;
                return new TagResolution(TagResolutionKind.Unknown, tagName, null, false);
            }

            // ── PascalCase → function component ──────────────────────────────
            bool typeFound = TryFindFuncComponentType(tagName, usingNamespaces);
            if (!typeFound)
            {
                unknownDiagnostic = UitkxDiagnostics.UnknownComponent;
                // Still emit V.Func(TagName.Render, ...) — let C# compiler report missing type
            }

            string? funcPropsTypeName = TryGetFuncComponentPropsTypeName(tagName, usingNamespaces);

            return new TagResolution(
                TagResolutionKind.FuncComponent,
                "Func",
                null,
                AcceptsChildren: true,
                FuncTypeName: tagName,
                FuncPropsTypeName: funcPropsTypeName
            );
        }

        // GetPublicPropertyNames — used by CSharpEmitter for UITKX0002 attribute validation

        private static readonly string[] s_propsNamespaces = { "ReactiveUITK.Props.Typed" };

        private static HashSet<string> CollectPropertyNames(INamedTypeSymbol type)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // Walk up the inheritance chain so that properties declared in BaseProps
            // (and any other ancestor) are visible to attribute validation.
            var current = type;
            while (current != null && current.SpecialType != SpecialType.System_Object)
            {
                foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (
                        member.DeclaredAccessibility == Accessibility.Public
                        && member.SetMethod != null
                    )
                        result.Add(member.Name);
                }
                current = current.BaseType;
            }
            return result;
        }

        /// <summary>
        /// Returns the PascalCase public settable property names for
        /// <paramref name="propsTypeName"/> (e.g. "LabelProps" → {"Text","Style","Name",…}).
        /// Used to validate attribute names and produce did-you-mean hints.
        /// Returns an empty set when the type cannot be found.
        /// </summary>
        public HashSet<string> GetPublicPropertyNames(string propsTypeName)
        {
            foreach (var ns in s_propsNamespaces)
            {
                var type = _compilation.GetTypeByMetadataName($"{ns}.{propsTypeName}");
                if (type != null)
                    return CollectPropertyNames(type);
            }
            return new HashSet<string>();
        }

        /// <summary>
        /// Returns the simple type name of the companion props class for a PascalCase
        /// function component (convention: "{ComponentName}Props"), or <c>null</c> when
        /// no such class is found in the compilation.
        ///
        /// When a props class is found the emitter uses the typed <c>V.Func&lt;TProps&gt;</c>
        /// overload; otherwise it falls back to the no-props <c>V.Func(TypeName.Render)</c> call.
        /// </summary>
        public string? TryGetFuncComponentPropsTypeName(
            string componentTypeName,
            ImmutableArray<string> usingNamespaces
        )
        {
            string candidate = $"{componentTypeName}Props";

            // Try unqualified (global namespace)
            if (_compilation.GetTypeByMetadataName(candidate) != null)
                return candidate;

            // Try each @using namespace
            foreach (var ns in usingNamespaces)
            {
                var sym = _compilation.GetTypeByMetadataName($"{ns}.{candidate}");
                if (sym != null) return sym.Name;
            }

            return null;
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private bool TryFindFuncComponentType(
            string typeName,
            ImmutableArray<string> usingNamespaces
        )
        {
            // Try unqualified first (rare but works in the global namespace)
            if (TryGetFuncType(typeName))
                return true;

            // Try each @using namespace
            foreach (var ns in usingNamespaces)
            {
                if (TryGetFuncType($"{ns}.{typeName}"))
                    return true;
            }

            return false;
        }

        private bool TryGetFuncType(string fullyQualified)
        {
            var type = _compilation.GetTypeByMetadataName(fullyQualified);
            if (type == null)
                return false;

            // Check: has a static Render method that returns VirtualNode
            return type.GetMembers("Render")
                .OfType<IMethodSymbol>()
                .Any(m =>
                    m.IsStatic
                    && m.DeclaredAccessibility == Accessibility.Public
                    && m.ReturnType.Name == VirtualNodeName
                );
        }

        // ── Build built-in map from Roslyn V type ─────────────────────────────

        private static Dictionary<string, TagResolution> BuildBuiltinMapFromCompilation(
            Compilation compilation
        )
        {
            var vType = compilation.GetTypeByMetadataName(VTypeName);
            if (vType == null)
            {
                // Compilation doesn't yet contain the shared assembly (first load).
                // Fall back to hardcoded map.
                return new Dictionary<string, TagResolution>(
                    s_fallbackMap.ToDictionary(kv => kv.Key, kv => kv.Value),
                    StringComparer.OrdinalIgnoreCase
                );
            }

            var map = new Dictionary<string, TagResolution>(StringComparer.OrdinalIgnoreCase);

            foreach (var member in vType.GetMembers().OfType<IMethodSymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Public)
                    continue;
                if (!member.IsStatic)
                    continue;
                if (member.ReturnType.Name != VirtualNodeName)
                    continue;
                if (member.Parameters.IsEmpty)
                    continue;

                var methodName = member.Name;
                var parameters = member.Parameters;
                var firstParam = parameters[0];
                var firstType = firstParam.Type;

                // Detect AcceptsChildren: last param is params VirtualNode[]
                bool acceptsChildren = false;
                var lastParam = parameters[parameters.Length - 1];
                if (
                    lastParam.IsParams
                    && lastParam.Type is IArrayTypeSymbol arr
                    && arr.ElementType.Name == VirtualNodeName
                )
                {
                    acceptsChildren = true;
                }

                // Classify
                TagResolutionKind kind;
                string? propsTypeName = null;

                if (string.Equals(methodName, "Fragment", StringComparison.Ordinal))
                {
                    kind = TagResolutionKind.Fragment;
                }
                else if (
                    string.Equals(methodName, "Text", StringComparison.Ordinal)
                    && firstType.SpecialType == SpecialType.System_String
                )
                {
                    kind = TagResolutionKind.BuiltinText;
                }
                else if (firstType.Name.EndsWith("Props", StringComparison.Ordinal))
                {
                    kind = TagResolutionKind.BuiltinTyped;
                    propsTypeName = firstType.Name;
                }
                else if (
                    firstType.Name == "IReadOnlyDictionary"
                    || firstType.Name == "Dictionary"
                    || (
                        firstParam.Type is INamedTypeSymbol namedFirst
                        && namedFirst.ConstructedFrom?.Name == "IReadOnlyDictionary"
                    )
                )
                {
                    kind = TagResolutionKind.BuiltinDictionary;
                }
                else
                {
                    // Func, ForwardRef, Portal, Router, etc. — skip
                    continue;
                }

                var key = methodName.ToLowerInvariant();

                // Prefer the typed overload when multiple overloads exist for the same tag
                if (
                    !map.TryGetValue(key, out var existing)
                    || kind == TagResolutionKind.BuiltinTyped
                )
                {
                    map[key] = new TagResolution(kind, methodName, propsTypeName, acceptsChildren);
                }
            }

            // Ensure "fragment" is always present
            if (!map.ContainsKey("fragment"))
                map["fragment"] = new TagResolution(
                    TagResolutionKind.Fragment,
                    "Fragment",
                    null,
                    true
                );

            return map;
        }

        // ── Fallback hardcoded map ────────────────────────────────────────────

        private static IReadOnlyDictionary<string, TagResolution> BuildFallbackMap()
        {
            static TagResolution Typed(string name, string props, bool children = false) =>
                new TagResolution(TagResolutionKind.BuiltinTyped, name, props, children);

            static TagResolution Dict(string name) =>
                new TagResolution(TagResolutionKind.BuiltinDictionary, name, null, true);

            return new Dictionary<string, TagResolution>(StringComparer.OrdinalIgnoreCase)
            {
                ["label"] = Typed("Label", "LabelProps"),
                ["button"] = Typed("Button", "ButtonProps"),
                ["textfield"] = Typed("TextField", "TextFieldProps"),
                ["toggle"] = Typed("Toggle", "ToggleProps"),
                ["slider"] = Typed("Slider", "SliderProps"),
                ["sliderint"] = Typed("SliderInt", "SliderIntProps"),
                ["image"] = Typed("Image", "ImageProps"),
                ["box"] = Typed("Box", "BoxProps", children: true),
                ["scrollview"] = Typed("ScrollView", "ScrollViewProps", children: true),
                ["listview"] = Typed("ListView", "ListViewProps"),
                ["treeview"] = Typed("TreeView", "TreeViewProps"),
                ["foldout"] = Typed("Foldout", "FoldoutProps", children: true),
                ["groupbox"] = Typed("GroupBox", "GroupBoxProps", children: true),
                ["helpbox"] = Typed("HelpBox", "HelpBoxProps"),
                ["progressbar"] = Typed("ProgressBar", "ProgressBarProps"),
                ["tab"] = Typed("Tab", "TabProps"),
                ["tabview"] = Typed("TabView", "TabViewProps"),
                ["textelement"] = Typed("TextElement", "TextElementProps"),
                ["radiobutton"] = Typed("RadioButton", "RadioButtonProps"),
                ["radiobuttongroup"] = Typed("RadioButtonGroup", "RadioButtonGroupProps"),
                ["dropdownfield"] = Typed("DropdownField", "DropdownFieldProps"),
                ["enumfield"] = Typed("EnumField", "EnumFieldProps"),
                ["integerfield"] = Typed("IntegerField", "IntegerFieldProps"),
                ["floatfield"] = Typed("FloatField", "FloatFieldProps"),

                ["visualelement"] = Dict("VisualElement"),

                ["text"] = new TagResolution(TagResolutionKind.BuiltinText, "Text", null, false),
                ["fragment"] = new TagResolution(
                    TagResolutionKind.Fragment,
                    "Fragment",
                    null,
                    true
                ),
            };
        }
    }
}
