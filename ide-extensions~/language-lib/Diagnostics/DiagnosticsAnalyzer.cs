using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language.Diagnostics
{
    /// <summary>
    /// Performs Tier-2 (structural) diagnostics on a parsed UITKX file.
    ///
    /// Tier 2 checks:
    /// <list type="bullet">
    ///   <item>UITKX0101 — Missing <c>@namespace</c> directive</item>
    ///   <item>UITKX0102 — Missing <c>@component</c> directive</item>
    ///   <item>UITKX0103 — <c>@component</c> name does not match filename</item>
    ///   <item>UITKX0104 — Duplicate literal <c>key="…"</c> among siblings</item>
    ///   <item>UITKX0105 — Unknown PascalCase element (when index available)</item>
    ///   <item>UITKX0106 — Element inside <c>@foreach</c> body has no <c>key</c> (warning)</item>
    /// </list>
    ///
    /// Tier-1 (parser syntax) errors are already present in
    /// <see cref="ParseResult.Diagnostics"/> and should be passed through directly
    /// by the caller; this class never re-emits them.
    /// </summary>
    public sealed class DiagnosticsAnalyzer
    {
        /// <summary>
        /// Analyse the parse result and return all T2 structural diagnostics.
        /// </summary>
        /// <param name="parseResult">The complete parse result (directives + AST + T1 errors).</param>
        /// <param name="filePath">
        /// Absolute or relative path of the source file, used for the filename-mismatch check.
        /// May be empty or null — the check is simply skipped.
        /// </param>
        /// <param name="projectElements">
        /// Set of component names known in the project (suffix "Props" stripped).
        /// Pass <c>null</c> to skip the unknown-element check. Pass an empty set to
        /// report every unrecognised PascalCase element as a warning.
        /// </param>
        public IReadOnlyList<ParseDiagnostic> Analyze(
            ParseResult parseResult,
            string? filePath,
            HashSet<string>? projectElements = null
        )
        {
            var diags = new List<ParseDiagnostic>();
            var d = parseResult.Directives;

            // ── T2: UITKX0101 — Missing @namespace ───────────────────────────
            if (string.IsNullOrEmpty(d.Namespace))
            {
                diags.Add(
                    MakeDiag(
                        DiagnosticCodes.MissingNamespace,
                        ParseSeverity.Error,
                        "Missing required '@namespace' directive.",
                        line: 1
                    )
                );
            }

            // ── T2: UITKX0102 — Missing @component ───────────────────────────
            if (string.IsNullOrEmpty(d.ComponentName))
            {
                diags.Add(
                    MakeDiag(
                        DiagnosticCodes.MissingComponent,
                        ParseSeverity.Error,
                        "Missing required '@component' directive.",
                        line: 1
                    )
                );
            }

            // ── T2: UITKX0103 — Filename / component-name mismatch ───────────
            if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(d.ComponentName))
            {
                var stem = Path.GetFileNameWithoutExtension(filePath);
                if (!string.Equals(stem, d.ComponentName, System.StringComparison.Ordinal))
                {
                    diags.Add(
                        MakeDiag(
                            DiagnosticCodes.FilenameMismatch,
                            ParseSeverity.Error,
                            $"@component name '{d.ComponentName}' does not match filename '{stem}.uitkx'.",
                            line: d.MarkupStartLine > 1 ? d.MarkupStartLine - 1 : 1
                        )
                    );
                }
            }

            // ── T2: AST walks ─────────────────────────────────────────────────
            WalkNodeList(parseResult.RootNodes, insideForeach: false, projectElements, diags);

            return diags;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  AST WALKER
        // ═══════════════════════════════════════════════════════════════════════

        private static void WalkNodeList(
            ImmutableArray<AstNode> nodes,
            bool insideForeach,
            HashSet<string>? projectElements,
            List<ParseDiagnostic> diags
        )
        {
            // UITKX0104 — Duplicate literal key among siblings at this level.
            CheckDuplicateKeys(nodes, diags);

            foreach (var node in nodes)
            {
                WalkNode(node, insideForeach, projectElements, diags);
            }
        }

        private static void WalkNode(
            AstNode node,
            bool insideForeach,
            HashSet<string>? projectElements,
            List<ParseDiagnostic> diags
        )
        {
            switch (node)
            {
                case ElementNode el:
                    CheckElement(el, insideForeach, diags);
                    WalkNodeList(el.Children, insideForeach: false, projectElements, diags);
                    break;

                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                        WalkNodeList(branch.Body, insideForeach, projectElements, diags);
                    break;

                case ForeachNode fe:
                    WalkNodeList(fe.Body, insideForeach: true, projectElements, diags);
                    break;

                case ForNode fn:
                    WalkNodeList(fn.Body, insideForeach, projectElements, diags);
                    break;

                case WhileNode wh:
                    WalkNodeList(wh.Body, insideForeach, projectElements, diags);
                    break;

                case SwitchNode sw:
                    foreach (var sc in sw.Cases)
                        WalkNodeList(sc.Body, insideForeach, projectElements, diags);
                    break;

                case CodeBlockNode cb:
                    CheckUnreachableAfterReturn(cb, diags);
                    foreach (var rm in cb.ReturnMarkups)
                    {
                        CheckElement(rm.Element, insideForeach, diags);
                        WalkNodeList(
                            rm.Element.Children,
                            insideForeach: false,
                            projectElements,
                            diags
                        );
                    }
                    break;

                // TextNode, ExpressionNode — nothing to check.
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  ELEMENT CHECKS
        // ═══════════════════════════════════════════════════════════════════════

        private static void CheckElement(
            ElementNode el,
            bool insideForeach,
            List<ParseDiagnostic> diags
        )
        {
            // UITKX0106 — Missing key inside @foreach.
            if (insideForeach && !HasKeyAttribute(el))
            {
                diags.Add(
                    MakeDiag(
                        DiagnosticCodes.MissingKey,
                        ParseSeverity.Error,
                        $"Element <{el.TagName}> inside @foreach should have a 'key' attribute to help with reconciliation.",
                        el.SourceLine,
                        el.SourceColumn,
                        el.SourceColumn + 1 + el.TagName.Length
                    )
                );
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DUPLICATE-KEY CHECK
        // ═══════════════════════════════════════════════════════════════════════

        private static void CheckDuplicateKeys(
            ImmutableArray<AstNode> siblings,
            List<ParseDiagnostic> diags
        )
        {
            // Collect elements that have a literal string key attribute.
            // Map: key-value → first element that used it.
            var seen = new Dictionary<string, ElementNode>(System.StringComparer.Ordinal);

            foreach (var node in siblings)
            {
                if (node is not ElementNode el)
                    continue;

                var keyAttr = el.Attributes.FirstOrDefault(a =>
                    string.Equals(a.Name, "key", System.StringComparison.Ordinal)
                    && a.Value is StringLiteralValue
                );

                if (keyAttr is null)
                    continue;

                var keyVal = ((StringLiteralValue)keyAttr.Value).Value;

                if (seen.TryGetValue(keyVal, out var first))
                {
                    // Report on the duplicate (current element).
                    diags.Add(
                        MakeDiag(
                            DiagnosticCodes.DuplicateKey,
                            ParseSeverity.Error,
                            $"Duplicate sibling key \"{keyVal}\". Keys must be unique among sibling elements.",
                            el.SourceLine,
                            el.SourceColumn
                        )
                    );

                    // Also report on the first occurrence if not already done.
                    if (
                        !diags.Any(d =>
                            d.Code == DiagnosticCodes.DuplicateKey
                            && d.SourceLine == first.SourceLine
                        )
                    )
                    {
                        diags.Add(
                            MakeDiag(
                                DiagnosticCodes.DuplicateKey,
                                ParseSeverity.Error,
                                $"Duplicate sibling key \"{keyVal}\". Keys must be unique among sibling elements.",
                                first.SourceLine,
                                first.SourceColumn
                            )
                        );
                    }
                }
                else
                {
                    seen[keyVal] = el;
                }
            }
        }

        private static readonly Regex s_topLevelReturnRegex = new Regex(
            @"^\s*return\b",
            RegexOptions.Compiled
        );

        private static void CheckUnreachableAfterReturn(
            CodeBlockNode cb,
            List<ParseDiagnostic> diags
        )
        {
            if (string.IsNullOrWhiteSpace(cb.Code))
                return;

            var lines = cb.Code.Replace("\r\n", "\n").Split('\n');
            bool seenTopLevelReturn = false;
            int depth = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmed = line.Trim();

                if (trimmed.Length == 0)
                {
                    depth += CountChar(line, '{') - CountChar(line, '}');
                    if (depth < 0)
                        depth = 0;
                    continue;
                }

                if (
                    seenTopLevelReturn && !trimmed.StartsWith("//", System.StringComparison.Ordinal)
                )
                {
                    int leading = line.Length - line.TrimStart().Length;
                    diags.Add(
                        MakeDiag(
                            DiagnosticCodes.UnreachableAfterReturn,
                            ParseSeverity.Hint,
                            "Unreachable code after 'return'.",
                            cb.SourceLine + 1 + i,
                            leading,
                            line.Length
                        )
                    );
                }

                if (!seenTopLevelReturn && depth == 0 && s_topLevelReturnRegex.IsMatch(line))
                {
                    seenTopLevelReturn = true;
                }

                depth += CountChar(line, '{') - CountChar(line, '}');
                if (depth < 0)
                    depth = 0;
            }
        }

        private static int CountChar(string text, char ch)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
                if (text[i] == ch)
                    count++;
            return count;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private static bool HasKeyAttribute(ElementNode el) =>
            el.Attributes.Any(a =>
                string.Equals(a.Name, "key", System.StringComparison.OrdinalIgnoreCase)
            );

        private static ParseDiagnostic MakeDiag(
            string code,
            ParseSeverity severity,
            string message,
            int line,
            int column = 0,
            int endColumn = 0
        ) =>
            new ParseDiagnostic
            {
                Code = code,
                Severity = severity,
                Message = message,
                SourceLine = line,
                SourceColumn = column,
                EndLine = line,
                EndColumn = endColumn > 0 ? endColumn : column,
            };
    }
}
