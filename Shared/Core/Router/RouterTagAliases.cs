using System;
using System.Collections.Generic;

namespace ReactiveUITK.Router
{
    /// <summary>
    /// Canonical map of short markup tag names to their implementing C# class
    /// names for the router primitives.  Both the source generator
    /// (<c>SourceGenerator~/Emitter/PropsResolver.cs</c>) and the HMR emitter
    /// (<c>Editor/HMR/HmrCSharpEmitter.cs</c>) consult this map so that users
    /// can write <c>&lt;Router&gt;</c>, <c>&lt;Route&gt;</c>, <c>&lt;Outlet&gt;</c>,
    /// etc. without knowing the <c>*Func</c>-suffixed type names.
    ///
    /// The source generator csproj links this file via
    /// <c>&lt;Compile Include=".."&gt;</c> so both the analyzer DLL and the
    /// runtime DLL share the exact same alias set.  Editing this list in one
    /// place is sufficient to teach every layer about a new router primitive.
    /// </summary>
    public static class RouterTagAliases
    {
        /// <summary>
        /// Markup-tag-name → implementing-type-name (case-sensitive, ordinal).
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> Map =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Router"] = "RouterFunc",
                ["Routes"] = "RoutesFunc",
                ["Route"] = "RouteFunc",
                ["Outlet"] = "OutletFunc",
                ["Link"] = "LinkFunc",
                ["NavLink"] = "NavLinkFunc",
                ["Navigate"] = "NavigateFunc",
            };

        /// <summary>
        /// Returns the implementing type name for a markup tag, or the input
        /// tag name unchanged when no alias exists.
        /// </summary>
        public static string Resolve(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return tagName;
            }
            return Map.TryGetValue(tagName, out var aliased) ? aliased : tagName;
        }
    }
}
