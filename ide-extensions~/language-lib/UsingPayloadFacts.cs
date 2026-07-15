namespace ReactiveUITK.Language
{
    /// <summary>
    /// Roslyn-free classifier for a <c>@using</c> / <c>import "@Ns"</c> payload (namespace-import
    /// unification plan). Both the source generator (build-time UITKX2316) and the LSP (editor-time
    /// UITKX2316/2317) share this so the payload → (kind, target token) split behaves identically;
    /// each side then does its own symbol lookup against its own compilation.
    /// </summary>
    public static class UsingPayloadFacts
    {
        public enum PayloadKind
        {
            /// <summary><c>using Ns;</c> — the target must be a namespace.</summary>
            Namespace,
            /// <summary><c>using static Type;</c> — the target must be a type.</summary>
            StaticType,
            /// <summary><c>using Alias = Target;</c> — the target may be a type or namespace.</summary>
            Alias,
        }

        /// <summary>
        /// Splits a using payload into its kind and the resolvable target token, plus the target's
        /// 0-based char offset WITHIN the payload (so a diagnostic can anchor at
        /// <c>PayloadColumn + TargetOffset</c>).
        /// <list type="bullet">
        ///   <item><c>"System.Text"</c> → (Namespace, "System.Text", 0)</item>
        ///   <item><c>"static System.Math"</c> → (StaticType, "System.Math", 7)</item>
        ///   <item><c>"UColor = UnityEngine.Color"</c> → (Alias, "UnityEngine.Color", 9)</item>
        /// </list>
        /// A <c>global::</c> prefix on the target is stripped (offset advanced past it).
        /// </summary>
        public static (PayloadKind Kind, string Target, int TargetOffset) Classify(string payload)
        {
            string p = payload ?? string.Empty;

            // Alias: `Name = Target` (the FIRST '=' at top level). Precedes the static check because
            // `static` is not a legal alias name, so an '=' unambiguously marks an alias.
            int eq = p.IndexOf('=');
            if (eq >= 0)
            {
                int t = eq + 1;
                while (t < p.Length && (p[t] == ' ' || p[t] == '\t')) t++;
                var (target, off) = StripGlobal(p, t);
                return (PayloadKind.Alias, target, off);
            }

            // Static: `static Target`.
            if (StartsWithWord(p, "static", out int afterStatic))
            {
                int t = afterStatic;
                while (t < p.Length && (p[t] == ' ' || p[t] == '\t')) t++;
                var (target, off) = StripGlobal(p, t);
                return (PayloadKind.StaticType, target, off);
            }

            // Plain namespace.
            {
                var (target, off) = StripGlobal(p, 0);
                return (PayloadKind.Namespace, target, off);
            }
        }

        private static (string Target, int Offset) StripGlobal(string p, int start)
        {
            const string g = "global::";
            if (start + g.Length <= p.Length && p.Substring(start, g.Length) == g)
                start += g.Length;
            return (p.Substring(start).Trim(), start);
        }

        private static bool StartsWithWord(string p, string word, out int after)
        {
            after = 0;
            int i = 0;
            while (i < p.Length && (p[i] == ' ' || p[i] == '\t')) i++;
            if (i + word.Length > p.Length) return false;
            if (p.Substring(i, word.Length) != word) return false;
            int j = i + word.Length;
            // Must be followed by whitespace (a token boundary), else it's e.g. `staticThing`.
            if (j >= p.Length || (p[j] != ' ' && p[j] != '\t')) return false;
            after = j;
            return true;
        }
    }
}
