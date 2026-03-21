using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using ReactiveUITK.Language.Nodes;

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Validates structural properties of the parsed UITKX AST:
    ///
    ///   UITKX0017 — More than one root element (component must return a single root)
    ///   UITKX0018 — <c>UseEffect</c> called with only a callback; dependency array is missing
    ///   UITKX0019 — The loop iterator variable is used directly as the <c>key</c> attribute
    ///               inside a <c>@foreach</c> body (index-as-key anti-pattern)
    /// </summary>
    internal static class StructureValidator
    {
        public static void Validate(
            ImmutableArray<AstNode> rootNodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            CheckMultipleRoots(rootNodes, filePath, diagnostics);
            CheckUseEffectInCodeBlocks(rootNodes, filePath, diagnostics);
            WalkForeachForIndexKey(rootNodes, filePath, diagnostics);
        }

        // ── UITKX0017 — multiple root elements ───────────────────────────────

        private static void CheckMultipleRoots(
            ImmutableArray<AstNode> rootNodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            // Count top-level element nodes (CodeBlockNode is not a render output)
            var elementRoots = rootNodes.OfType<ElementNode>().ToArray();
            if (elementRoots.Length <= 1)
                return;

            // Emit one diagnostic for every extra root beyond the first
            for (int i = 1; i < elementRoots.Length; i++)
            {
                var loc = Location.Create(filePath, default, default);
                diagnostics.Add(
                    Diagnostic.Create(UitkxDiagnostics.MultipleRootElements, loc, filePath)
                );
            }
        }

        // ── UITKX0018 — UseEffect missing dependency array ───────────────────

        private static void CheckUseEffectInCodeBlocks(
            ImmutableArray<AstNode> rootNodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var node in rootNodes.OfType<CodeBlockNode>())
                ScanUseEffectMissingDeps(node.Code, node.SourceLine, filePath, diagnostics);
        }

        private static void ScanUseEffectMissingDeps(
            string code,
            int blockStartLine,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            // Find every "UseEffect(" occurrence (matches Hooks.UseEffect( and bare UseEffect()
            int pos = 0;
            while (pos < code.Length)
            {
                int idx = code.IndexOf("UseEffect(", pos, StringComparison.Ordinal);
                if (idx < 0)
                    break;

                int argStart = idx + "UseEffect(".Length;
                bool hasTopLevelComma = ScanForTopLevelComma(code, argStart);

                if (!hasTopLevelComma)
                {
                    // Approximate absolute line number by counting newlines before the match
                    int approxLine = blockStartLine + CountNewlines(code, 0, idx);
                    var loc = Location.Create(filePath, default, default);
                    diagnostics.Add(
                        Diagnostic.Create(
                            UitkxDiagnostics.UseEffectMissingDeps,
                            loc,
                            approxLine,
                            filePath
                        )
                    );
                }

                pos = idx + 1;
            }
        }

        /// <summary>
        /// Starting at <paramref name="start"/> (the character immediately after an opening
        /// parenthesis), scans forward and returns <c>true</c> if a comma exists at
        /// brace-depth 1 before the matching close-paren.
        /// </summary>
        private static bool ScanForTopLevelComma(string code, int start)
        {
            int depth = 1;
            for (int i = start; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case '(':
                    case '{':
                    case '[':
                        depth++;
                        break;

                    case ')':
                    case '}':
                    case ']':
                        depth--;
                        if (depth == 0)
                            return false; // reached closing paren with no comma
                        break;

                    case ',':
                        if (depth == 1)
                            return true;
                        break;
                }
            }
            return false;
        }

        private static int CountNewlines(string s, int from, int to)
        {
            int count = 0;
            for (int i = from; i < to && i < s.Length; i++)
                if (s[i] == '\n')
                    count++;
            return count;
        }

        // ── UITKX0019 — loop iterator variable used as key ───────────────────

        private static void WalkForeachForIndexKey(
            ImmutableArray<AstNode> nodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case ForeachNode forn:
                    {
                        string? loopVar = ExtractLoopVarName(forn.IteratorDeclaration);
                        if (loopVar != null)
                            CheckDirectChildrenForIndexKey(
                                forn.Body,
                                loopVar,
                                filePath,
                                diagnostics
                            );
                        // Recurse into the foreach body for nested constructs
                        WalkForeachForIndexKey(forn.Body, filePath, diagnostics);
                        break;
                    }
                    case ElementNode el:
                        WalkForeachForIndexKey(el.Children, filePath, diagnostics);
                        break;
                    case IfNode ifn:
                        foreach (var branch in ifn.Branches)
                            WalkForeachForIndexKey(branch.Body, filePath, diagnostics);
                        break;
                    case SwitchNode sw:
                        foreach (var c in sw.Cases)
                            WalkForeachForIndexKey(c.Body, filePath, diagnostics);
                        break;
                }
            }
        }

        private static void CheckDirectChildrenForIndexKey(
            ImmutableArray<AstNode> body,
            string loopVar,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var node in body)
            {
                if (node is not ElementNode el)
                    continue;

                var keyAttr = el.Attributes.FirstOrDefault(a => a.Name == "key");
                if (keyAttr == null)
                    continue;

                // Flag when the key expression is exactly the bare loop variable AND the
                // variable name looks like a positional index ("i", "j", "idx", ...).  
                // Descriptive names like 'entry' or 'item' indicate the loop variable IS
                // the item identity — using it directly as a key is correct and intentional.
                if (
                    keyAttr.Value is CSharpExpressionValue kv
                    && string.Equals(kv.Expression.Trim(), loopVar, StringComparison.Ordinal)
                    && IsPositionalIndexVar(loopVar)
                )
                {
                    var loc = Location.Create(filePath, default, default);
                    diagnostics.Add(
                        Diagnostic.Create(
                            UitkxDiagnostics.IndexAsKey,
                            loc,
                            el.TagName,
                            loopVar,
                            el.SourceLine,
                            filePath
                        )
                    );
                }
            }
        }

        /// <summary>
        /// Returns true when <paramref name="name"/> looks like a numeric loop index
        /// (one or two characters, or a well-known alias such as "index" or "idx").
        /// This guards UITKX0019 against false-positives on descriptive loop variables
        /// like <c>entry</c>, <c>item</c>, or <c>link</c>, where using the variable
        /// directly as a key is correct because it represents the item's identity.
        /// </summary>
        private static bool IsPositionalIndexVar(string name) =>
            name.Length <= 2
            || string.Equals(name, "index",    StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "idx",      StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "pos",      StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "position", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Extracts the loop variable name from an iterator declaration.
        /// Returns <c>null</c> for tuple-destructuring patterns (e.g. <c>(int i, string s)</c>)
        /// where simple detection isn't reliable.
        /// Examples: <c>var item</c> → <c>item</c>, <c>int i</c> → <c>i</c>.
        /// </summary>
        private static string? ExtractLoopVarName(string iteratorDecl)
        {
            if (string.IsNullOrWhiteSpace(iteratorDecl))
                return null;
            var trimmed = iteratorDecl.Trim();
            if (trimmed.StartsWith("(", StringComparison.Ordinal))
                return null; // tuple destructure
            var lastSpace = trimmed.LastIndexOf(' ');
            return lastSpace >= 0 ? trimmed.Substring(lastSpace + 1).Trim() : trimmed;
        }
    }
}
