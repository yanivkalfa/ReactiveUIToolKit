using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using ReactiveUITK.Language.Parser;

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

        /// <summary>
        /// Simple names of component types that will be generated from other .uitkx files
        /// in the same source-generator run.  Used to suppress UITKX0008 for peer
        /// components that cannot be found in the compilation (because they are generated
        /// in the same pass and therefore not yet compiled into the snapshot).
        /// </summary>
        private readonly ImmutableHashSet<string> _peerComponentTypeNames;

        /// <summary>
        /// Subset of <see cref="_peerComponentTypeNames"/> for peers that declare
        /// function-style params (and therefore have a generated <c>XxxProps</c> nested
        /// class).  Only these peers get the typed <c>V.Func&lt;T&gt;</c> code path.
        /// </summary>
        private readonly ImmutableHashSet<string> _peerPropsComponentTypeNames;

        /// <summary>
        /// The parsed <see cref="FunctionParam"/> list for each peer-UITKX component that
        /// uses the function-style declaration syntax.  Keyed by component simple name.
        /// Used by <see cref="TryGetRefParamPropName"/> to locate the <c>Hooks.MutableRef&lt;T&gt;</c>
        /// parameter so that a bare <c>ref={x}</c> attribute can be routed to the correct prop.
        /// </summary>
        private readonly ImmutableDictionary<
            string,
            ImmutableArray<FunctionParam>
        > _peerFunctionParams;

        /// <summary>Lowercase tag → TagResolution for every V.* built-in.</summary>
        private readonly Dictionary<string, TagResolution> _builtinMap;

        // ── Roslyn type-name constants ────────────────────────────────────────
        private const string VTypeName = "ReactiveUITK.V";
        private const string VirtualNodeName = "VirtualNode";

        // ── Well-known component tag aliases ────────────────────────────────────
        // Maps short markup tag names to their implementing C# class names.
        // This lets users write <Router>, <Route>, <Link> in .uitkx markup
        // without knowing the Func-suffixed class names that back them.
        private static readonly Dictionary<string, string> s_componentTagAliases = new Dictionary<
            string,
            string
        >(StringComparer.Ordinal)
        {
            ["Router"] = "RouterFunc",
            ["Route"] = "RouteFunc",
            ["Link"] = "LinkFunc",
        };

        // ── Fallback hard-coded map (used when V type not resolvable) ─────────
        private static readonly IReadOnlyDictionary<string, TagResolution> s_fallbackMap =
            BuildFallbackMap();

        public PropsResolver(
            Compilation compilation,
            ImmutableHashSet<string>? peerComponentTypeNames = null,
            ImmutableHashSet<string>? peerPropsComponentTypeNames = null,
            ImmutableDictionary<string, ImmutableArray<FunctionParam>>? peerFunctionParams = null
        )
        {
            _compilation = compilation;
            _peerComponentTypeNames = peerComponentTypeNames ?? ImmutableHashSet<string>.Empty;
            _peerPropsComponentTypeNames =
                peerPropsComponentTypeNames ?? ImmutableHashSet<string>.Empty;
            _peerFunctionParams =
                peerFunctionParams
                ?? ImmutableDictionary<string, ImmutableArray<FunctionParam>>.Empty;
            _builtinMap = BuildBuiltinMapFromCompilation(compilation);
        }

        // ── Public API ────────────────────────────────────────────────────────

        // ── ref-as-prop routing ───────────────────────────────────────────────

        /// <summary>
        /// Outcome of a <see cref="TryGetRefParamPropName"/> look-up.
        /// </summary>
        internal enum RefParamLookupResult
        {
            /// <summary>No <c>Hooks.MutableRef&lt;T&gt;</c> parameter was found.</summary>
            None,

            /// <summary>Exactly one param found; <c>refPropName</c> is set.</summary>
            Found,

            /// <summary>Multiple params found; routing is ambiguous.</summary>
            Ambiguous,
        }

        /// <summary>
        /// Attempts to find the single unambiguous <c>Hooks.MutableRef&lt;T&gt;</c> parameter
        /// for <paramref name="componentTypeName"/> so that a bare <c>ref={x}</c> attribute
        /// can be routed to the correct Props property name.
        ///
        /// Two resolution paths are tried in order:
        /// <list type="number">
        ///   <item>Peer-UITKX path — scans <see cref="_peerFunctionParams"/> for directly parsed
        ///     <see cref="FunctionParam"/> entries whose type string indicates <c>MutableRef&lt;T&gt;</c>.</item>
        ///   <item>Roslyn path — inspects the compiled Props type (<paramref name="propsTypeName"/>)
        ///     for public settable properties whose Roslyn type is <c>Hooks.MutableRef&lt;T&gt;</c>.</item>
        /// </list>
        /// </summary>
        /// <param name="componentTypeName">Simple C# type name of the component (e.g. "RefChild").</param>
        /// <param name="propsTypeName">Qualified or simple props type name, or <c>null</c> when the
        ///   component has no props class.</param>
        /// <param name="refPropName">PascalCase property name to emit (e.g. "InputRef").</param>
        /// <returns>The lookup outcome.</returns>
        internal RefParamLookupResult TryGetRefParamPropName(
            string componentTypeName,
            string? propsTypeName,
            ImmutableArray<string> searchNamespaces,
            out string? refPropName
        )
        {
            refPropName = null;

            // ── Path A: peer-UITKX component (same code-gen pass) ─────────────
            if (_peerFunctionParams.TryGetValue(componentTypeName, out var fpList))
            {
                var mutableRefParams = fpList.Where(fp => IsMutableRefTypeName(fp.Type)).ToList();

                if (mutableRefParams.Count == 0)
                    return RefParamLookupResult.None;

                if (mutableRefParams.Count > 1)
                    return RefParamLookupResult.Ambiguous;

                // Exactly one — convert camelCase param name to PascalCase prop name.
                refPropName = ToPropName(mutableRefParams[0].Name);
                return RefParamLookupResult.Found;
            }

            // ── Path B: C# component — inspect compiled Props type ────────────
            if (propsTypeName == null)
                return RefParamLookupResult.None;

            var propNames = GetMutableRefPropertyNames(propsTypeName, searchNamespaces);
            if (propNames.Count == 0)
                return RefParamLookupResult.None;

            if (propNames.Count > 1)
                return RefParamLookupResult.Ambiguous;

            refPropName = propNames[0];
            return RefParamLookupResult.Found;
        }

        // ── ref-detection helpers ─────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when <paramref name="typeName"/> (as it appears in a .uitkx
        /// function-style parameter list) denotes a <c>Ref&lt;T&gt;</c> or the deprecated
        /// <c>Hooks.MutableRef&lt;T&gt;</c> type.
        /// Handles optional nullable suffix and fully-qualified form.
        /// </summary>
        private static bool IsMutableRefTypeName(string typeName)
        {
            string stripped = typeName.TrimEnd('?').Trim();
            return stripped.StartsWith("Ref<", StringComparison.Ordinal)
                || stripped.StartsWith("ReactiveUITK.Core.Ref<", StringComparison.Ordinal)
                || stripped.StartsWith("Hooks.MutableRef<", StringComparison.Ordinal) // [Obsolete] compat
                || stripped.StartsWith(
                    "ReactiveUITK.Core.Hooks.MutableRef<",
                    StringComparison.Ordinal
                );
        }

        /// <summary>
        /// Converts a camelCase identifier to PascalCase — e.g. "inputRef" → "InputRef".
        /// No op if the first character is already uppercase.
        /// </summary>
        private static string ToPropName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            return char.IsUpper(name[0])
                ? name
                : char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        /// <summary>
        /// Collects the names of all public settable properties on the given
        /// <paramref name="propsTypeName"/> that are typed as <c>Ref&lt;T&gt;</c> or the
        /// deprecated <c>Hooks.MutableRef&lt;T&gt;</c>, walking the inheritance chain.
        /// Returns an empty list when the type cannot be resolved by Roslyn.
        /// </summary>
        private List<string> GetMutableRefPropertyNames(
            string propsTypeName,
            ImmutableArray<string> searchNamespaces
        )
        {
            INamedTypeSymbol? typeSymbol = null;

            // Try the propsTypeName as-is (may be fully qualified via TryGetFuncComponentPropsTypeName).
            typeSymbol = _compilation.GetTypeByMetadataName(propsTypeName);

            // Try each @using namespace as a qualifier.
            if (typeSymbol == null)
            {
                foreach (var ns in searchNamespaces)
                {
                    typeSymbol = _compilation.GetTypeByMetadataName($"{ns}.{propsTypeName}");
                    if (typeSymbol != null)
                        break;

                    // Nested type variant (e.g. RouterFunc+Props)
                    typeSymbol = _compilation.GetTypeByMetadataName(
                        $"{ns}.{propsTypeName.Replace('.', '+')}"
                    );
                    if (typeSymbol != null)
                        break;
                }
            }

            if (typeSymbol == null)
                return new List<string>();

            var result = new List<string>();
            var current = typeSymbol;
            while (current != null && current.SpecialType != SpecialType.System_Object)
            {
                foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (
                        member.DeclaredAccessibility == Accessibility.Public
                        && member.SetMethod != null
                        && IsRoslynMutableRefType(member.Type)
                    )
                    {
                        result.Add(member.Name);
                    }
                }
                current = current.BaseType;
            }
            return result;
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="typeSymbol"/> is the top-level
        /// <c>Ref&lt;T&gt;</c> from <c>ReactiveUITK.Core</c>, or the deprecated
        /// <c>Hooks.MutableRef&lt;T&gt;</c> type.
        /// Also matches nullable wrappers by unwrapping via Nullable.
        /// </summary>
        private static bool IsRoslynMutableRefType(ITypeSymbol typeSymbol)
        {
            // Unwrap Nullable<T> → T
            if (
                typeSymbol
                    is INamedTypeSymbol
                    {
                        ConstructedFrom: { SpecialType: SpecialType.System_Nullable_T }
                    }
                && typeSymbol is INamedTypeSymbol nullable
            )
            {
                typeSymbol = nullable.TypeArguments[0];
            }

            if (typeSymbol is not INamedTypeSymbol named || !named.IsGenericType)
                return false;

            var def = named.ConstructedFrom;

            // Match top-level Ref<T> in ReactiveUITK.Core namespace (not nested)
            if (
                string.Equals(def.Name, "Ref", StringComparison.Ordinal)
                && def.ContainingType == null
                && string.Equals(
                    def.ContainingNamespace?.ToDisplayString(),
                    "ReactiveUITK.Core",
                    StringComparison.Ordinal
                )
            )
                return true;

            // Match Hooks.MutableRef<T> — [Obsolete] backward compat
            return string.Equals(def.Name, "MutableRef", StringComparison.Ordinal)
                && string.Equals(def.ContainingType?.Name, "Hooks", StringComparison.Ordinal)
                && string.Equals(
                    def.ContainingNamespace?.ToDisplayString(),
                    "ReactiveUITK.Core",
                    StringComparison.Ordinal
                );
        }

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
            // Apply well-known component tag aliases first so that short markup
            // names like <Router>, <Route>, <Link> transparently resolve to their
            // Func-suffixed implementation classes.
            string lookupTypeName = s_componentTagAliases.TryGetValue(tagName, out var aliased)
                ? aliased
                : tagName;

            bool typeFound = TryFindFuncComponentType(lookupTypeName, usingNamespaces);
            if (!typeFound)
            {
                unknownDiagnostic = UitkxDiagnostics.UnknownComponent;
                // Still emit V.Func(LookupTypeName.Render, ...) — let C# compiler report missing type
            }

            string? funcPropsTypeName = TryGetFuncComponentPropsTypeName(
                lookupTypeName,
                usingNamespaces
            );

            return new TagResolution(
                TagResolutionKind.FuncComponent,
                "Func",
                null,
                AcceptsChildren: true,
                FuncTypeName: lookupTypeName,
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
                if (sym != null)
                    return sym.Name;
            }

            // Fall back to convention for peer-generated components that have props.
            // Same-pass generated types are absent from the Roslyn snapshot, so
            // GetTypeByMetadataName returns null even though {Name}Props WILL exist
            // after code generation.  The Props class is emitted as a nested type
            // inside the component's partial class (e.g. UnstableChild.UnstableChildProps),
            // so qualify the name with the declaring type to avoid CS0246.
            // Only do this for peers that actually declare function-style params;
            // peers with no params never emit a Props class and must take the no-props path.
            //
            // IMPORTANT: this check runs before the nested-Props scan below.
            // Both a C# legacy class (e.g. ShowcaseTopBar.cs with nested Props) and
            // its UITKX peer counterpart (ShowcaseTopBar.uitkx with ShowcaseTopBarProps)
            // may be present in the same compilation.  Peer components must always win
            // so the UITKX-generated typed V.Func<X.XProps> call is used, not the C#
            // legacy V.Func<X.Props> call which would target the wrong namespace.
            if (_peerPropsComponentTypeNames.Contains(componentTypeName))
                return $"{componentTypeName}.{candidate}";

            // Also check for a nested Props class (convention: TypeName.Props).
            // In Roslyn metadata names, nested classes use '+': TypeName+Props.
            // If found, return "TypeName.Props" so the emitter emits
            //   V.Func<TypeName.Props>(TypeName.Render, new TypeName.Props { ... })
            // This supports legacy C# static classes that follow the ValuesBarFunc
            // pattern (nested Props class rather than a sibling {TypeName}Props class).
            if (_compilation.GetTypeByMetadataName($"{componentTypeName}+Props") != null)
                return $"{componentTypeName}.Props";

            foreach (var ns in usingNamespaces)
            {
                var sym = _compilation.GetTypeByMetadataName($"{ns}.{componentTypeName}+Props");
                if (sym != null)
                    return $"{componentTypeName}.Props";
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

            // Fall back to peer-generated component names.
            // Types generated from other .uitkx files in the same pass are not yet
            // compiled into the Roslyn snapshot, so GetTypeByMetadataName returns null
            // for them.  If the simple type name matches a peer component we suppress
            // UITKX0008 — the C# compiler will catch any real mismatch.
            if (_peerComponentTypeNames.Contains(typeName))
                return true;

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
                    // Func, ForwardRef, Router, etc. — skip
                    // (Portal and Suspense are registered manually below)
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

            // Suspense is a first-class built-in: V.Suspense() arguments are emitted
            // by a dedicated code path (EmitSuspense) that maps well-known attribute
            // names (isReady, pendingTask, fallback) to the correct overload.
            // V.Suspense's first param is Func<bool> — the Roslyn scanner skips it,
            // so we register it manually here.
            if (!map.ContainsKey("suspense"))
                map["suspense"] = new TagResolution(
                    TagResolutionKind.BuiltinSuspense,
                    "Suspense",
                    null,
                    AcceptsChildren: true
                );

            // Portal: V.Portal(VisualElement, string key, params VirtualNode[]).
            // First param is VisualElement — the scanner skips it, so register manually.
            if (!map.ContainsKey("portal"))
                map["portal"] = new TagResolution(
                    TagResolutionKind.BuiltinPortal,
                    "Portal",
                    null,
                    AcceptsChildren: true
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
                ["suspense"] = new TagResolution(
                    TagResolutionKind.BuiltinSuspense,
                    "Suspense",
                    null,
                    AcceptsChildren: true
                ),
                ["portal"] = new TagResolution(
                    TagResolutionKind.BuiltinPortal,
                    "Portal",
                    null,
                    AcceptsChildren: true
                ),
                ["errorboundary"] = Typed("ErrorBoundary", "ErrorBoundaryProps", children: true),
            };
        }
    }
}
