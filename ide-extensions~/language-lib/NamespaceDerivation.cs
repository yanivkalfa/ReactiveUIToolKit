using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveUITK.Language
{
    /// <summary>
    /// Path-derived default namespace for a .uitkx file (import/export grammar, leg 3, plan §4).
    /// The single source of truth in language-lib, consumed by the source generator and the LSP.
    /// Editor/HMR keeps a byte-for-byte mirror (it cannot reference language-lib) pinned by a
    /// parity contract test — if you change the algorithm here, change the mirror in lockstep.
    ///
    /// Rule: <c>EffectiveNamespace = @namespace if present, else "ReactiveUITK.Uitkx" +
    /// "." + Join('.', Sanitize(seg) for seg in dir-segments(fileDir relative to owning-asmdef dir))</c>.
    /// The file NAME is excluded (files in one folder share a namespace); a file directly beside the
    /// asmdef derives <c>ReactiveUITK.Uitkx</c> alone. No owning asmdef → caller emits UITKX2310.
    /// </summary>
    public static class NamespaceDerivation
    {
        /// <summary>The fixed root of every path-derived namespace.</summary>
        public const string Root = "ReactiveUITK.Uitkx";

        /// <summary>
        /// Derive the default namespace for <paramref name="fileAbsolutePath"/> given the directory
        /// of its owning .asmdef. Returns <c>null</c> when there is no owning asmdef (UITKX2310).
        /// Path comparison is ordinal-case-insensitive (Windows/macOS filesystem parity); segment
        /// casing is preserved verbatim in the output.
        /// </summary>
        public static string? Derive(string fileAbsolutePath, string? owningAsmdefDirAbsolutePath)
        {
            if (string.IsNullOrEmpty(owningAsmdefDirAbsolutePath))
                return null;

            var segments = RelativeDirSegments(fileAbsolutePath, owningAsmdefDirAbsolutePath!);
            if (segments.Count == 0)
                return Root;

            var sb = new StringBuilder(Root);
            foreach (var seg in segments)
                sb.Append('.').Append(Sanitize(seg));
            return sb.ToString();
        }

        /// <summary>
        /// The directory segments of <paramref name="fileAbs"/> relative to
        /// <paramref name="asmdefDir"/>, with the file name excluded. Empty when the file is
        /// directly in the asmdef directory (or, defensively, when it is not under it).
        /// </summary>
        private static List<string> RelativeDirSegments(string fileAbs, string asmdefDir)
        {
            static string Norm(string p) => p.Replace('\\', '/').TrimEnd('/');

            string file = Norm(fileAbs);
            string root = Norm(asmdefDir);

            int slash = file.LastIndexOf('/');
            string fileDir = slash >= 0 ? file.Substring(0, slash) : string.Empty;

            var result = new List<string>();
            if (fileDir.Length > root.Length &&
                fileDir.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
            {
                string rel = fileDir.Substring(root.Length + 1);
                foreach (var seg in rel.Split('/'))
                    if (seg.Length > 0)
                        result.Add(seg);
            }
            // fileDir == root  → beside the asmdef → no segments (result stays empty)
            // fileDir not under root → walk-up would not have returned this asmdef; empty is the safe default
            return result;
        }

        /// <summary>
        /// Sanitize one directory segment into a legal C# namespace identifier (plan §4, pinned
        /// EXACTLY, mirrored on the HMR + LSP sides): keep <c>[A-Za-z0-9_]</c>, map every other
        /// char to <c>_</c>; a leading digit gets an <c>_</c> prefix; an empty segment becomes
        /// <c>_</c>; an exact (case-sensitive) C# reserved keyword gets an <c>_</c> prefix; casing
        /// is otherwise preserved verbatim.
        /// </summary>
        public static string Sanitize(string segment)
        {
            if (string.IsNullOrEmpty(segment))
                return "_";

            var sb = new StringBuilder(segment.Length);
            foreach (char c in segment)
            {
                bool keep = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
                            || (c >= '0' && c <= '9') || c == '_';
                sb.Append(keep ? c : '_');
            }

            string s = sb.ToString();
            if (s.Length == 0)
                return "_";
            if (s[0] >= '0' && s[0] <= '9')
                s = "_" + s;
            if (s_reservedKeywords.Contains(s))
                s = "_" + s;
            return s;
        }

        // C# reserved keywords (case-sensitive; contextual keywords are legal identifiers and excluded).
        private static readonly HashSet<string> s_reservedKeywords = new(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while",
        };
    }
}
