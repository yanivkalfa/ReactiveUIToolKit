using System.Collections.Generic;

namespace ReactiveUITK.Language
{
    /// <summary>
    /// The namespaces every generated .uitkx C# file receives unconditionally from the runtime
    /// emitters (CSharpEmitter / HookEmitter / ModuleEmitter). Single source of truth for the
    /// namespace-import unification plan: a user-written <c>@using</c> / <c>import "@Ns"</c> that
    /// exactly matches one of these is provably redundant — the codemod's <c>--tidy</c> strips it and
    /// the editor flags it UITKX2317 (Hint). Kept in lockstep with the emitter preamble by
    /// <c>AutoInjectedUsingsParityTests</c>.
    /// </summary>
    public static class AutoInjectedUsings
    {
        /// <summary>
        /// Plain-namespace usings injected by every emitter (the <c>using X;</c> lines — NOT the
        /// <c>static</c>/alias forms, which no one hand-writes). Ordinal-exact; this is the set a
        /// hand-written <c>@using</c> can duplicate.
        /// </summary>
        public static readonly IReadOnlyList<string> Namespaces = new[]
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "ReactiveUITK",
            "ReactiveUITK.Core",
            "ReactiveUITK.Core.Animation",
            "ReactiveUITK.Props.Typed",
            "UnityEngine",
        };

        private static readonly HashSet<string> s_set =
            new HashSet<string>(Namespaces, System.StringComparer.Ordinal);

        /// <summary>True when <paramref name="payload"/> is a plain namespace already auto-injected — i.e. a redundant hand-written using.</summary>
        public static bool IsRedundant(string payload) =>
            payload != null && s_set.Contains(payload.Trim());
    }
}
