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
    ///   <item>UITKX0111 — Unused component parameter in function-style component</item>
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
            WalkNodeList(parseResult.RootNodes, insideForeach: false, projectElements, knownAttributes, diags,
                isFunctionStyle: d.IsFunctionStyle);

            // ── T2: Function-style unreachable-after-return ───────────────────
            if (d.IsFunctionStyle)
            {
                // Site A — dim code between the render return's `;` and the
                // component closing `}`, but never the `}` itself.
                if (d.FunctionReturnEndLine > 0 && d.FunctionBodyEndLine > 0)
                {
                    int unreachStart = d.FunctionReturnEndLine + 1;
                    int unreachEnd = d.FunctionBodyEndLine - 1; // exclude `}`
                    if (unreachEnd >= unreachStart)
                    {
                        diags.Add(new ParseDiagnostic
                        {
                            Code = DiagnosticCodes.UnreachableAfterReturn,
                            Severity = ParseSeverity.Hint,
                            Message = "Unreachable code after 'return'.",
                            SourceLine = unreachStart,
                            SourceColumn = 0,
                            EndLine = unreachEnd,
                            EndColumn = 9999,
                        });
                    }
                }

                // Render-return wrapper — if setup code has an early return,
                // the `return (` line that wraps the render root ISN'T an AST
                // node, so neither CheckUnreachableAfterReturn (code-block
                // internal) nor Site B (sibling ElementNodes) covers it.
                // Emit a one-line diagnostic for that gap.
                if (d.MarkupStartLine > 0)
                {
                    var setupCb = parseResult.RootNodes
                        .OfType<CodeBlockNode>().FirstOrDefault();
                    if (setupCb != null && HasTopLevelReturn(setupCb))
                    {
                        diags.Add(new ParseDiagnostic
                        {
                            Code = DiagnosticCodes.UnreachableAfterReturn,
                            Severity = ParseSeverity.Hint,
                            Message = "Unreachable code after 'return'.",
                            SourceLine = d.MarkupStartLine,
                            SourceColumn = 0,
                            EndLine = d.MarkupStartLine,
                            EndColumn = 9999,
                        });
                    }
                }
            }

            // ── T2: UITKX0111 — Unused component parameter ───────────────────
            if (d.IsFunctionStyle && !d.FunctionParams.IsDefaultOrEmpty)
            {
                CheckUnusedParameters(d, parseResult.RootNodes, diags);
            }

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

        /// <summary>
        /// Runs T2 element/attribute checks on an arbitrary set of AST nodes
        /// (e.g. markup embedded inside setup-code JSX blocks).
        /// </summary>
        public IReadOnlyList<ParseDiagnostic> AnalyzeNodes(
            ImmutableArray<AstNode> nodes,
            HashSet<string>? projectElements,
            IReadOnlyDictionary<string, IReadOnlyCollection<string>>? knownAttributes)
        {
            var diags = new List<ParseDiagnostic>();
            WalkNodeList(nodes, insideForeach: false, projectElements, knownAttributes, diags);
            return diags;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  AST WALKER
        // ═══════════════════════════════════════════════════════════════════════

        private static void WalkNodeList(
            ImmutableArray<AstNode> nodes,
            bool insideForeach,
            HashSet<string>? projectElements,
            IReadOnlyDictionary<string, IReadOnlyCollection<string>>? knownAttributes,
            List<ParseDiagnostic> diags,
            bool skipReturnCheck = false,
            bool isFunctionStyle = false
        )
        {
            // UITKX0104 — Duplicate literal key among siblings at this level.
            CheckDuplicateKeys(nodes, diags);

            for (int idx = 0; idx < nodes.Length; idx++)
            {
                var node = nodes[idx];
                WalkNode(node, insideForeach, projectElements, knownAttributes, diags,
                    skipReturnCheck: skipReturnCheck && node is CodeBlockNode,
                    isFunctionStyle: isFunctionStyle && node is CodeBlockNode);

                bool isScopeEnder = node is BreakNode || node is ContinueNode;
                bool isReturnEnder = !isScopeEnder && !skipReturnCheck
                    && node is CodeBlockNode cbn && HasTopLevelReturn(cbn);

                if (!isScopeEnder && !isReturnEnder)
                    continue;

                // Everything after this node in the sibling list is unreachable.
                // Emit a single multi-line diagnostic that spans the entire region.
                if (idx + 1 >= nodes.Length)
                    break; // nothing follows — no diagnostic needed

                int firstLine = nodes[idx + 1].SourceLine;

                // Compute the last line of the unreachable region by finding the
                // deepest descendant line among all remaining siblings.
                int lastLine = firstLine;
                for (int j = idx + 1; j < nodes.Length; j++)
                    lastLine = System.Math.Max(lastLine, GetLastDescendantLine(nodes[j]));

                bool fromReturn = isReturnEnder;
                diags.Add(new ParseDiagnostic
                {
                    Code = fromReturn
                        ? DiagnosticCodes.UnreachableAfterReturn
                        : DiagnosticCodes.UnreachableAfterBreakOrContinue,
                    Severity = ParseSeverity.Hint,
                    Message = fromReturn
                        ? "Unreachable code after 'return'."
                        : "This node is unreachable because a preceding '@break' or '@continue' exits the loop body.",
                    SourceLine = firstLine,
                    SourceColumn = 0,
                    EndLine = lastLine,
                    EndColumn = 9999,
                });

                break; // no need to continue — everything after is covered
            }
        }

        /// <summary>
        /// Returns the maximum 1-based source line occupied by any descendant
        /// of <paramref name="node"/>.  Used to compute the end of a multi-line
        /// unreachable region for <see cref="DiagnosticTag"/>
        /// <c>Unnecessary</c> fading.
        /// </summary>
        private static int GetLastDescendantLine(AstNode node)
        {
            int max = node.SourceLine;

            switch (node)
            {
                case ElementNode el:
                    if (el.CloseTagLine > 0)
                        max = System.Math.Max(max, el.CloseTagLine);
                    foreach (var child in el.Children)
                        max = System.Math.Max(max, GetLastDescendantLine(child));
                    break;

                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                    {
                        max = System.Math.Max(max, branch.SourceLine);
                        foreach (var child in branch.Body)
                            max = System.Math.Max(max, GetLastDescendantLine(child));
                    }
                    break;

                case ForeachNode fe:
                    foreach (var child in fe.Body)
                        max = System.Math.Max(max, GetLastDescendantLine(child));
                    break;

                case ForNode fo:
                    foreach (var child in fo.Body)
                        max = System.Math.Max(max, GetLastDescendantLine(child));
                    break;

                case WhileNode wh:
                    foreach (var child in wh.Body)
                        max = System.Math.Max(max, GetLastDescendantLine(child));
                    break;

                case SwitchNode sw:
                    foreach (var c in sw.Cases)
                    {
                        max = System.Math.Max(max, c.SourceLine);
                        foreach (var child in c.Body)
                            max = System.Math.Max(max, GetLastDescendantLine(child));
                    }
                    break;

                case CodeBlockNode cb:
                    foreach (var rm in cb.ReturnMarkups)
                        max = System.Math.Max(max, GetLastDescendantLine(rm.Element));
                    break;
            }

            // Add 1 to account for the closing brace/tag line that follows
            // the deepest child but isn't tracked in the AST.
            return max + 1;
        }

        private static void WalkNode(
            AstNode node,
            bool insideForeach,
            HashSet<string>? projectElements,
            IReadOnlyDictionary<string, IReadOnlyCollection<string>>? knownAttributes,
            List<ParseDiagnostic> diags,
            bool skipReturnCheck = false,
            bool isFunctionStyle = false
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
                    CheckUnreachableAfterReturn(cb, diags, isFunctionStyle: isFunctionStyle);
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

        /// <summary>
        /// Returns true when the code block contains an unconditional top-level
        /// <c>return</c> statement (depth 0), meaning all subsequent sibling
        /// AST nodes are unreachable.
        /// </summary>
        internal static bool HasTopLevelReturn(CodeBlockNode cb)
        {
            if (string.IsNullOrWhiteSpace(cb.Code))
                return false;

            var lines = cb.Code.Replace("\r\n", "\n").Split('\n');
            int depth = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmed = line.Trim();

                if (trimmed.Length == 0)
                {
                    depth += CountChar(line, '{') - CountChar(line, '}');
                    if (depth < 0) depth = 0;
                    continue;
                }

                if (depth == 0 && s_topLevelReturnRegex.IsMatch(line))
                    return true;

                depth += CountChar(line, '{') - CountChar(line, '}');
                if (depth < 0) depth = 0;
            }

            return false;
        }

        private static void CheckUnreachableAfterReturn(
            CodeBlockNode cb,
            List<ParseDiagnostic> diags,
            bool isFunctionStyle = false
        )
        {
            if (string.IsNullOrWhiteSpace(cb.Code))
                return;

            var lines = cb.Code.Replace("\r\n", "\n").Split('\n');
            int depth = 0;

            // For normal @code blocks, SourceLine = the @code line; code
            // starts one line below → +1.  For function-style, the synthetic
            // @code wrapper puts code on the *same* line → no +1.
            int lineBase = isFunctionStyle ? cb.SourceLine : cb.SourceLine + 1;

            // ── Unified unreachable-zone tracker ──────────────────────────
            // Works at ALL brace depths.  When a return statement completes
            // (single-line or multi-line + balanced parens + `;`), every
            // subsequent line at the same or deeper depth is unreachable
            // until a `}` drops us below the return's depth.
            //
            // `unreachDepth`  — the brace depth at which the return was
            //                   found, or −1 when not in an unreachable zone.
            // `parenTracking` — > 0 while we are inside a multi-line return's
            //                   `return ( … )` parenthesised expression.
            int unreachDepth   = -1;
            int rangeStart     = -1;
            int rangeEnd       = -1;
            int parenTracking  = -1; // -1 = not tracking; ≥ 0 = paren depth

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmed = line.Trim();
                int srcLine = lineBase + i;

                int opens  = CountChar(line, '{');
                int closes = CountChar(line, '}');
                int nextDepth = depth + opens - closes;
                if (nextDepth < 0) nextDepth = 0;

                if (trimmed.Length == 0)
                {
                    depth = nextDepth;
                    continue;
                }

                bool isComment = trimmed.StartsWith("//", System.StringComparison.Ordinal);

                // ── Multi-line return tracking ─────────────────────────────
                // We saw `return (` on a previous line.  Count parens across
                // lines until they balance, then check for `;`.
                if (parenTracking >= 0)
                {
                    for (int ci = 0; ci < line.Length; ci++)
                    {
                        char ch = line[ci];
                        if (ch == '(') parenTracking++;
                        else if (ch == ')')
                        {
                            parenTracking--;
                            if (parenTracking <= 0)
                            {
                                // Parens balanced.  Check the rest of the
                                // line (after `)`) for `;`.
                                string rest = line.Substring(ci + 1).Trim();
                                if (rest.StartsWith(";", System.StringComparison.Ordinal))
                                {
                                    // Return statement complete on this line.
                                    // Start unreachable zone from next line.
                                    unreachDepth = depth;
                                }
                                parenTracking = -1;
                                break;
                            }
                        }
                    }
                    depth = nextDepth;
                    continue;
                }

                // ── Flush unreachable zone when exiting scope ──────────────
                if (unreachDepth >= 0 && nextDepth < unreachDepth)
                {
                    if (rangeStart > 0 && rangeEnd >= rangeStart)
                    {
                        diags.Add(new ParseDiagnostic
                        {
                            Code       = DiagnosticCodes.UnreachableAfterReturn,
                            Severity   = ParseSeverity.Hint,
                            Message    = "Unreachable code after 'return'.",
                            SourceLine = rangeStart,
                            SourceColumn = 0,
                            EndLine    = rangeEnd,
                            EndColumn  = 9999,
                        });
                    }
                    unreachDepth = -1;
                    rangeStart   = -1;
                    rangeEnd     = -1;
                }

                // ── Accumulate unreachable lines ───────────────────────────
                if (unreachDepth >= 0 && depth >= unreachDepth && !isComment)
                {
                    if (rangeStart < 0) rangeStart = srcLine;
                    rangeEnd = srcLine;
                }

                // ── Detect new return ──────────────────────────────────────
                if (unreachDepth < 0 && !isComment
                    && s_topLevelReturnRegex.IsMatch(line))
                {

                    // Single-line return: `return <Tag/>;` or `return expr;`
                    if (trimmed.EndsWith(";", System.StringComparison.Ordinal))
                    {
                        unreachDepth = depth;
                    }
                    // Multi-line paren-wrapped: `return (`
                    else
                    {
                        string afterReturn = trimmed.Substring("return".Length).TrimStart();
                        if (afterReturn.StartsWith("(", System.StringComparison.Ordinal))
                        {
                            // Start tracking parens from this line.
                            parenTracking = 0;
                            for (int ci = 0; ci < line.Length; ci++)
                            {
                                if (line[ci] == '(') parenTracking++;
                                else if (line[ci] == ')') parenTracking--;
                            }
                            if (parenTracking <= 0)
                            {
                                // Balanced on same line — check for ;
                                if (trimmed.EndsWith(";", System.StringComparison.Ordinal))
                                {
                                    unreachDepth = depth;
                                }
                                parenTracking = -1;
                            }
                        }
                    }
                }

                depth = nextDepth;
            }

            // Flush any remaining unreachable range at end of code block.
            if (unreachDepth >= 0 && rangeStart > 0 && rangeEnd >= rangeStart)
            {
                diags.Add(new ParseDiagnostic
                {
                    Code       = DiagnosticCodes.UnreachableAfterReturn,
                    Severity   = ParseSeverity.Hint,
                    Message    = "Unreachable code after 'return'.",
                    SourceLine = rangeStart,
                    SourceColumn = 0,
                    EndLine    = rangeEnd,
                    EndColumn  = 9999,
                });
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
        //  UNUSED PARAMETER CHECK
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// UITKX0111 — For each function-style component parameter, check whether
        /// its name appears in the setup code (with scope-aware shadowing) or
        /// the markup AST (expressions, attribute bindings, conditions).
        /// </summary>
        private static void CheckUnusedParameters(
            DirectiveSet d,
            ImmutableArray<AstNode> rootNodes,
            List<ParseDiagnostic> diags)
        {
            // Collect C# text from markup only (expressions, attributes,
            // conditions).  Markup never has local declarations, so a
            // simple word-boundary check is safe here.
            var markupSb = new System.Text.StringBuilder();
            CollectCSharpText(rootNodes, markupSb);
            string markupText = markupSb.ToString();

            foreach (var p in d.FunctionParams)
            {
                var nameRx = new Regex(@"\b" + Regex.Escape(p.Name) + @"\b");

                // 1. Used in markup?  Simple text search is sufficient.
                if (nameRx.IsMatch(markupText))
                    continue;

                // 2. Used in setup code?  Scope-aware: skip usages inside
                //    scopes that declare their own local with the same name.
                if (IsUsedInSetupCode(d.FunctionSetupCode, p.Name, nameRx))
                    continue;

                int col    = p.NameColumn >= 0 ? p.NameColumn : 0;
                int endCol = col + p.Name.Length;
                int line   = p.SourceLine > 0 ? p.SourceLine : d.ComponentDeclarationLine;

                diags.Add(new ParseDiagnostic
                {
                    Code       = DiagnosticCodes.UnusedParameter,
                    Severity   = ParseSeverity.Error,
                    Message    = $"Parameter '{p.Name}' is declared but never used.",
                    SourceLine = line,
                    SourceColumn = col,
                    EndLine    = line,
                    EndColumn  = endCol,
                });
            }
        }

        /// <summary>
        /// Scope-aware check: does <paramref name="paramName"/> appear in
        /// <paramref name="setupCode"/> at a brace depth where it is NOT
        /// shadowed by a local declaration (<c>var name</c>, <c>Type name</c>,
        /// or a function parameter with the same name)?
        /// </summary>
        private static bool IsUsedInSetupCode(
            string? setupCode, string paramName, Regex nameRx)
        {
            if (string.IsNullOrEmpty(setupCode))
                return false;
            if (!nameRx.IsMatch(setupCode))
                return false; // fast bail

            // Matches a local variable declaration that introduces `paramName`
            // as a new local: "var name", "int name", "List<T>? name", etc.
            var declRx = new Regex(
                @"(?:var|int|uint|long|ulong|short|ushort|byte|sbyte|char" +
                @"|float|double|decimal|bool|string|object|dynamic|nint|nuint" +
                @"|[A-Z]\w*(?:<[^>]*>)?(?:\?|\[\])*)" +
                @"\s+" + Regex.Escape(paramName) + @"\b");

            var lines = setupCode.Replace("\r\n", "\n").Split('\n');
            int depth = 0;
            // shadowAt[d] == true  ⇒  a local with this name was declared at depth d.
            // Shadows apply at depth d and all deeper levels until the scope exits.
            var shadowAt = new bool[128];

            for (int li = 0; li < lines.Length; li++)
            {
                string line = lines[li];
                string trimmed = line.Trim();

                if (trimmed.Length == 0)
                {
                    // Still track braces on blank lines.
                    for (int c = 0; c < line.Length; c++)
                    {
                        if (line[c] == '{') { depth++; }
                        else if (line[c] == '}')
                        {
                            if (depth > 0 && depth < 128) shadowAt[depth] = false;
                            if (depth > 0) depth--;
                        }
                    }
                    continue;
                }

                // Check if this line contains a local declaration.
                if (declRx.IsMatch(line) && depth >= 0 && depth < 128)
                    shadowAt[depth] = true;

                // Check if the name is used on this line AND not shadowed.
                if (nameRx.IsMatch(line))
                {
                    bool shadowed = false;
                    for (int sd = 0; sd <= depth && sd < 128; sd++)
                        if (shadowAt[sd]) { shadowed = true; break; }

                    if (!shadowed)
                    {
                        // If the line also has a declaration, the name after
                        // "var/Type" is the declaration itself — remove it and
                        // re-check.  This way `var items = f(items)` still
                        // counts the RHS `items` as a parameter use.
                        string stripped = declRx.Replace(line, "");
                        if (nameRx.IsMatch(stripped))
                            return true;
                    }
                }

                // Update depth from braces on this line.
                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c] == '{') { depth++; }
                    else if (line[c] == '}')
                    {
                        if (depth > 0 && depth < 128) shadowAt[depth] = false;
                        if (depth > 0) depth--;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Recursively collects C# expression text from markup nodes only
        /// (element attributes, inline expressions, control-flow conditions).
        /// <see cref="CodeBlockNode.Code"/> is deliberately excluded — setup
        /// code is handled separately by <see cref="IsUsedInSetupCode"/> with
        /// scope-aware shadowing. Only <see cref="ReturnMarkupNode"/> elements
        /// inside code blocks are collected.
        /// </summary>
        private static void CollectCSharpText(ImmutableArray<AstNode> nodes, System.Text.StringBuilder sb)
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case ExpressionNode en:
                        sb.Append(' ').Append(en.Expression).Append(' ');
                        break;

                    case CodeBlockNode cb:
                        // Do NOT append cb.Code — it is scanned by IsUsedInSetupCode.
                        // Only collect from return-markup elements embedded in code.
                        foreach (var rm in cb.ReturnMarkups)
                            CollectCSharpTextFromElement(rm.Element, sb);
                        break;

                    case ElementNode el:
                        CollectCSharpTextFromElement(el, sb);
                        break;

                    case IfNode ifn:
                        foreach (var br in ifn.Branches)
                        {
                            if (br.Condition != null)
                                sb.Append(' ').Append(br.Condition).Append(' ');
                            CollectCSharpText(br.Body, sb);
                        }
                        break;

                    case ForeachNode fe:
                        sb.Append(' ').Append(fe.CollectionExpression).Append(' ');
                        CollectCSharpText(fe.Body, sb);
                        break;

                    case ForNode fn:
                        sb.Append(' ').Append(fn.ForExpression).Append(' ');
                        CollectCSharpText(fn.Body, sb);
                        break;

                    case WhileNode wh:
                        sb.Append(' ').Append(wh.Condition).Append(' ');
                        CollectCSharpText(wh.Body, sb);
                        break;

                    case SwitchNode sw:
                        sb.Append(' ').Append(sw.SwitchExpression).Append(' ');
                        foreach (var sc in sw.Cases)
                        {
                            if (sc.ValueExpression != null)
                                sb.Append(' ').Append(sc.ValueExpression).Append(' ');
                            CollectCSharpText(sc.Body, sb);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Collects C# text from an element's attributes and children.
        /// </summary>
        private static void CollectCSharpTextFromElement(ElementNode el, System.Text.StringBuilder sb)
        {
            foreach (var attr in el.Attributes)
            {
                switch (attr.Value)
                {
                    case CSharpExpressionValue cv:
                        sb.Append(' ').Append(cv.Expression).Append(' ');
                        break;
                    case JsxExpressionValue jv when jv.Element != null:
                        CollectCSharpTextFromElement(jv.Element, sb);
                        break;
                }
            }
            CollectCSharpText(el.Children, sb);
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
