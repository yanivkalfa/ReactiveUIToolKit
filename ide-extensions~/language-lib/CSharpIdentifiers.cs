using System.Collections.Generic;

namespace Ruitk.Language
{
    /// <summary>
    /// C# identifier legality for names that are emitted VERBATIM into generated code
    /// (export names, and — after sanitization — namespace segments). The single source of
    /// truth for the reserved-keyword set: the parser (UITKX2114), the RUITK Builder's
    /// create/rename gestures and <see cref="NamespaceDerivation.Sanitize"/> all read it, so
    /// an author can never be told a name is legal by one layer and rejected by another.
    /// </summary>
    public static class CSharpIdentifiers
    {
        /// <summary>
        /// True when <paramref name="name"/> is exactly a C# reserved keyword. Comparison is
        /// ORDINAL (case-sensitive) because C# keywords are: <c>new</c> is reserved, <c>New</c>
        /// is a perfectly legal identifier. Contextual keywords (<c>value</c>, <c>var</c>,
        /// <c>record</c>, …) are legal identifiers and are deliberately absent.
        /// </summary>
        public static bool IsReservedKeyword(string? name) =>
            !string.IsNullOrEmpty(name) && s_reservedKeywords.Contains(name!);

        /// <summary>
        /// True when <paramref name="name"/> is a legal, non-reserved C# identifier: a letter or
        /// underscore followed by letters, digits or underscores. Verbatim (<c>@</c>-prefixed)
        /// identifiers are NOT accepted — a name that needs escaping to compile is a name the
        /// author should change.
        /// </summary>
        public static bool IsValidIdentifier(string? name)
        {
            if (string.IsNullOrEmpty(name) || IsReservedKeyword(name))
                return false;

            char first = name![0];
            if (!((first >= 'A' && first <= 'Z') || (first >= 'a' && first <= 'z') || first == '_'))
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                bool ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
                          || (c >= '0' && c <= '9') || c == '_';
                if (!ok)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Folds an arbitrary name (a .uxml file stem, say) into a legal PascalCase C#
        /// identifier: runs of non-alphanumeric characters become word breaks, each word is
        /// capitalized, a leading digit gets an <c>_</c> prefix. Returns
        /// <paramref name="fallback"/> when nothing usable survives. The PascalCase result can
        /// never be a reserved keyword, since every C# keyword is lowercase.
        /// </summary>
        public static string ToPascalIdentifier(string? name, string fallback = "Component")
        {
            if (string.IsNullOrEmpty(name))
                return fallback;

            var sb = new System.Text.StringBuilder(name!.Length);
            bool startWord = true;
            foreach (char c in name!)
            {
                bool alnum = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
                             || (c >= '0' && c <= '9');
                if (!alnum)
                {
                    startWord = true;
                    continue;
                }
                sb.Append(startWord ? char.ToUpperInvariant(c) : c);
                startWord = false;
            }

            if (sb.Length == 0)
                return fallback;
            if (sb[0] >= '0' && sb[0] <= '9')
                sb.Insert(0, '_');
            return sb.ToString();
        }

        private static readonly HashSet<string> s_reservedKeywords = new HashSet<string>(System.StringComparer.Ordinal)
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
