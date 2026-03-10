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
    ///   <item>UITKX0107 — Unreachable code after top-level <c>return</c> in <c>@code</c></item>
    ///   <item>UITKX0108 — Component has more than one root render node</item>
    ///   <item>UITKX0109 — Unknown attribute on a known element (when attribute map available)</item>
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
        /// <param name="knownAttributes">
        /// Map of element name → set of valid attribute names for that element.
        /// Pass <c>null</c> to skip the unknown-attribute check.
        /// </param>
        public IReadOnlyList<ParseDiagnostic> Analyze(
            ParseResult parseResult,
            string? filePath,
            HashSet<string>? projectElements = null,
            IReadOnlyDictionary<string, IReadOnlyCollection<string>>? knownAttributes = null
        )
        {
            var diags = new List<ParseDiagnostic>();
            var d = parseResult.Directives;

            // ── T2: UITKX0101 — Missing @namespace ───────────────────────────
            if (!d.IsFunctionStyle && string.IsNullOrEmpty(d.Namespace))
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
            if (!d.IsFunctionStyle && string.IsNullOrEmpty(d.ComponentName))
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
                    // Use the exact line where `component Name {` / `@component` was declared
                    // when available; fall back to the line above the markup root otherwise.
                    int errLine = d.ComponentDeclarationLine > 0
                        ? d.ComponentDeclarationLine
                        : (d.MarkupStartLine > 1 ? d.MarkupStartLine - 1 : 1);
                    // Aim the squiggle at the NAME token, not the `component` keyword.
                    int nameCol    = d.ComponentNameColumn >= 0 ? d.ComponentNameColumn : 0;
                    int nameEndCol = nameCol > 0 && d.ComponentName != null
                        ? nameCol + d.ComponentName.Length
                        : 0;
                    diags.Add(
                        MakeDiag(
                            DiagnosticCodes.FilenameMismatch,
                            ParseSeverity.Error,
                            $"@component name '{d.ComponentName}' does not match filename '{stem}.uitkx'.",
                            line:      errLine,
                            column:    nameCol,
                            endColumn: nameEndCol
                        )
                    );
                }
            }

            // ── T2: UITKX0108 — Multiple render roots ────────────────────────
            CheckSingleRenderRoot(parseResult.RootNodes, diags);

            // ── T2: AST walks ─────────────────────────────────────────────────
            WalkNodeList(parseResult.RootNodes, insideForeach: false, projectElements, knownAttributes, diags);

            return diags;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  RENDER-ROOT CHECK
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// UITKX0108 — A component must have exactly one render root.
        ///
        /// "Render root" is any node that contributes to the rendered output:
        /// <see cref="ElementNode"/>, <see cref="IfNode"/>, <see cref="ForeachNode"/>,
        /// <see cref="ForNode"/>, <see cref="WhileNode"/>, <see cref="SwitchNode"/>,
        /// <see cref="ExpressionNode"/>, or a non-whitespace <see cref="TextNode"/>.
        ///
        /// Excluded from the count (they do not produce rendered output):
        /// <list type="bullet">
        ///   <item><see cref="CodeBlockNode"/> — setup code or <c>@code</c> class body.</item>
        ///   <item><see cref="JsxCommentNode"/> — JSX comment, not rendered.</item>
        ///   <item>Whitespace-only <see cref="TextNode"/>.</item>
        /// </list>
        ///
        /// An error is reported on every render-root node beyond the first,
        /// pointing authors to the exact offending node.
        /// </summary>
        private static void CheckSingleRenderRoot(
            ImmutableArray<AstNode> rootNodes,
            List<ParseDiagnostic> diags
        )
        {
            // Collect all nodes that actually contribute to the rendered output.
            var renderRoots = new List<AstNode>(rootNodes.Length);
            foreach (var node in rootNodes)
            {
                switch (node)
                {
                    // Non-rendering nodes — excluded from the count.
                    case CodeBlockNode:
                    case JsxCommentNode:
                        continue;
                    case TextNode tn when string.IsNullOrWhiteSpace(tn.Content):
                        continue;

                    // Everything else contributes a render tree node.
                    default:
                        renderRoots.Add(node);
                        break;
                }
            }

            // The first render root is the valid single root.  Every subsequent
            // one is an error.  Report each individually so the user sees a squiggle
            // on the exact offending node.
            for (int i = 1; i < renderRoots.Count; i++)
            {
                var extra = renderRoots[i];
                string label = DescribeRenderNode(extra);
                int endCol = extra is ElementNode el
                    ? extra.SourceColumn + 1 + el.TagName.Length
                    : extra.SourceColumn;

                diags.Add(
                    MakeDiag(
                        DiagnosticCodes.MultipleRenderRoots,
                        ParseSeverity.Error,
                        $"A component must have a single root element. '{label}' is an extra root — wrap all root nodes in a single container element.",
                        extra.SourceLine,
                        extra.SourceColumn,
                        endCol
                    )
                );
            }
        }

        /// <summary>
        /// Returns a concise human-readable label for a render-contributing node,
        /// used in diagnostic messages.
        /// </summary>
        private static string DescribeRenderNode(AstNode node) =>
            node switch
            {
                ElementNode el   => $"<{el.TagName}>",
                IfNode           => "@if",
                ForeachNode      => "@foreach",
                ForNode          => "@for",
                WhileNode        => "@while",
                SwitchNode       => "@switch",
                ExpressionNode en => $"@({en.Expression})",
                TextNode tn      => $"text \"{ tn.Content.Trim() }\"",
                BreakNode        => "@break",
                ContinueNode     => "@continue",
                _                => node.GetType().Name,
            };

        // ═══════════════════════════════════════════════════════════════════════
        //  AST WALKER
        // ═══════════════════════════════════════════════════════════════════════

        private static void WalkNodeList(
            ImmutableArray<AstNode> nodes,
            bool insideForeach,
            HashSet<string>? projectElements,
            IReadOnlyDictionary<string, IReadOnlyCollection<string>>? knownAttributes,
            List<ParseDiagnostic> diags
        )
        {
            // UITKX0104 — Duplicate literal key among siblings at this level.
            CheckDuplicateKeys(nodes, diags);

            foreach (var node in nodes)
            {
                WalkNode(node, insideForeach, projectElements, knownAttributes, diags);
            }
        }

        private static void WalkNode(
            AstNode node,
            bool insideForeach,
            HashSet<string>? projectElements,
            IReadOnlyDictionary<string, IReadOnlyCollection<string>>? knownAttributes,
            List<ParseDiagnostic> diags
        )
        {
            switch (node)
            {
                case ElementNode el:
                    CheckElement(el, insideForeach, projectElements, knownAttributes, diags);
                    WalkNodeList(el.Children, insideForeach: false, projectElements, knownAttributes, diags);
                    break;

                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                        WalkNodeList(branch.Body, insideForeach, projectElements, knownAttributes, diags);
                    break;

                case ForeachNode fe:
                    WalkNodeList(fe.Body, insideForeach: true, projectElements, knownAttributes, diags);
                    break;

                case ForNode fn:
                    WalkNodeList(fn.Body, insideForeach, projectElements, knownAttributes, diags);
                    break;

                case WhileNode wh:
                    WalkNodeList(wh.Body, insideForeach, projectElements, knownAttributes, diags);
                    break;

                case SwitchNode sw:
                    foreach (var sc in sw.Cases)
                        WalkNodeList(sc.Body, insideForeach, projectElements, knownAttributes, diags);
                    break;

                case CodeBlockNode cb:
                    CheckUnreachableAfterReturn(cb, diags);
                    foreach (var rm in cb.ReturnMarkups)
                    {
                        CheckElement(rm.Element, insideForeach, projectElements, knownAttributes, diags);
                        WalkNodeList(
                            rm.Element.Children,
                            insideForeach: false,
                            projectElements,
                            knownAttributes,
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
            HashSet<string>? projectElements,
            IReadOnlyDictionary<string, IReadOnlyCollection<string>>? knownAttributes,
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

            // UITKX0105 — Unknown PascalCase element (custom component not in index).
            bool isPascalCase = el.TagName.Length > 0 && char.IsUpper(el.TagName[0]);
            bool elementKnown = true; // assume known unless we have an index and it's missing
            if (isPascalCase && projectElements != null && !projectElements.Contains(el.TagName))
            {
                elementKnown = false;
                diags.Add(
                    MakeDiag(
                        DiagnosticCodes.UnknownElement,
                        ParseSeverity.Error,
                        $"Unknown element '<{el.TagName}>'. No component with this name was found in the workspace.",
                        el.SourceLine,
                        el.SourceColumn + 1,  // +1 to point at the name, past '<'
                        el.SourceColumn + 1 + el.TagName.Length
                    )
                );
            }

            // UITKX0109 — Unknown attribute on a known element.
            // Only check when the element is known (no double-error on unknown elements)
            // and we have attribute data for it.
            if (elementKnown
                && knownAttributes != null
                && knownAttributes.TryGetValue(el.TagName, out var validAttrs))
            {
                foreach (var attr in el.Attributes)
                {
                    if (!validAttrs.Contains(attr.Name))
                    {
                        diags.Add(
                            MakeDiag(
                                DiagnosticCodes.UnknownAttribute,
                                ParseSeverity.Error,
                                $"Unknown attribute '{attr.Name}' on <{el.TagName}>.",
                                attr.SourceLine,
                                attr.SourceColumn,
                                attr.NameEndColumn > 0
                                    ? attr.NameEndColumn
                                    : attr.SourceColumn + attr.Name.Length
                            )
                        );
                    }
                }
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
