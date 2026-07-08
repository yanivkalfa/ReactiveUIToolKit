using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Walks the AST and enforces ReactiveUITK's Rules of Hooks:
    ///
    ///   UITKX0013 - Hook called inside @if / @else branch
    ///   UITKX0014 - Hook called inside @foreach loop
    ///   UITKX0015 - Hook called inside @switch case
    ///
    /// Detection is text-based: any <see cref="ExpressionNode"/> whose
    /// content contains a call matching
    /// <c>Hooks.Use*</c> or <c>UseState(</c> / <c>UseEffect(</c> etc. is flagged.
    ///
    /// This catches the most common violation patterns without needing a full
    /// C# parser for the expression content.
    /// </summary>
    internal static class HooksValidator
    {
        public static void Validate(
            ImmutableArray<AstNode> rootNodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var node in rootNodes)
                WalkNode(node, HookContext.TopLevel, filePath, diagnostics);
        }

        // -- Context tracking --------------------------------------------------

        private enum HookContext
        {
            TopLevel,
            Conditional,
            Loop,
            Switch,
        }

        // -- Tree walker -------------------------------------------------------

        private static void WalkNode(
            AstNode node,
            HookContext ctx,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            switch (node)
            {
                case IfNode ifn:
                    foreach (var branch in ifn.Branches)
                    {
                        if (!string.IsNullOrWhiteSpace(branch.Payload.BodyCode))
                            ScanCodeForHooks(branch.Payload.BodyCode, branch.Payload.BodyCodeLine,
                                HookContext.Conditional, filePath, diagnostics,
                                GetPreambleEnd(branch.Payload.BodyCode, branch.Payload.BodyCodeOffset, branch.Payload.BodyMarkupRanges, branch.Payload.BodyBareJsxRanges));
                        WalkBody(branch.Payload.Body, HookContext.Conditional, filePath, diagnostics);
                    }
                    break;

                case ForeachNode forn:
                    if (!string.IsNullOrWhiteSpace(forn.Payload.BodyCode))
                        ScanCodeForHooks(forn.Payload.BodyCode, forn.Payload.BodyCodeLine,
                            HookContext.Loop, filePath, diagnostics,
                            GetPreambleEnd(forn.Payload.BodyCode, forn.Payload.BodyCodeOffset, forn.Payload.BodyMarkupRanges, forn.Payload.BodyBareJsxRanges));
                    WalkBody(forn.Payload.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case ForNode forNode:
                    if (!string.IsNullOrWhiteSpace(forNode.Payload.BodyCode))
                        ScanCodeForHooks(forNode.Payload.BodyCode, forNode.Payload.BodyCodeLine,
                            HookContext.Loop, filePath, diagnostics,
                            GetPreambleEnd(forNode.Payload.BodyCode, forNode.Payload.BodyCodeOffset, forNode.Payload.BodyMarkupRanges, forNode.Payload.BodyBareJsxRanges));
                    WalkBody(forNode.Payload.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case WhileNode wn:
                    if (!string.IsNullOrWhiteSpace(wn.Payload.BodyCode))
                        ScanCodeForHooks(wn.Payload.BodyCode, wn.Payload.BodyCodeLine,
                            HookContext.Loop, filePath, diagnostics,
                            GetPreambleEnd(wn.Payload.BodyCode, wn.Payload.BodyCodeOffset, wn.Payload.BodyMarkupRanges, wn.Payload.BodyBareJsxRanges));
                    WalkBody(wn.Payload.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case SwitchNode sw:
                    foreach (var c in sw.Cases)
                    {
                        if (!string.IsNullOrWhiteSpace(c.Payload.BodyCode))
                            ScanCodeForHooks(c.Payload.BodyCode, c.Payload.BodyCodeLine,
                                HookContext.Switch, filePath, diagnostics,
                                GetPreambleEnd(c.Payload.BodyCode, c.Payload.BodyCodeOffset, c.Payload.BodyMarkupRanges, c.Payload.BodyBareJsxRanges));
                        WalkBody(c.Payload.Body, HookContext.Switch, filePath, diagnostics);
                    }
                    break;

                case ElementNode el:
                    // Check attribute values for hook calls - always wrong, even at top-level
                    // (covers onClick={() => UseState(0)}, text={UseState(0).value}, etc.)
                    foreach (var attr in el.Attributes)
                    {
                        if (attr.Value is CSharpExpressionValue attrExpr)
                            CheckAttributeForHooks(
                                attrExpr.Expression,
                                attr.SourceLine,
                                filePath,
                                diagnostics
                            );
                    }
                    // Children inherit whatever context we're already in
                    WalkBody(el.Children, ctx, filePath, diagnostics);
                    break;

                case ExpressionNode ex:
                    if (ctx != HookContext.TopLevel)
                        CheckExpressionForHooks(
                            ex.Expression,
                            ex.SourceLine,
                            ctx,
                            filePath,
                            diagnostics
                        );
                    break;

                // TextNode - nothing to check
            }
        }

        private static void WalkBody(
            ImmutableArray<AstNode> body,
            HookContext ctx,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var n in body)
                WalkNode(n, ctx, filePath, diagnostics);
        }

        // -- Hook detection ----------------------------------------------------

        // Patterns that indicate a hook call. We match on the call site name only -
        // the leading `Hooks.` prefix is optional so hand-written using-aliases work too.
        //
        // Sourced from ReactiveUITK.Core.HookRegistry so this list cannot drift
        // from the source generator's alias table and the IDE diagnostics
        // analyzer's scanner.  This is also where useLayoutEffect coverage
        // arrives - pre-0.5.23 this table was missing it entirely.
        private static readonly string[] s_hookPatterns =
            global::ReactiveUITK.Core.HookRegistry.GetValidationPatterns();

        // -- UITKX0016 - hook inside attribute expression ---------------------

        private static void CheckAttributeForHooks(
            string code,
            int sourceLine,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var pattern in s_hookPatterns)
            {
                // U-10: raw IndexOf false-positives on comments/strings and
                // identifier/member-access boundaries (`_useState(`, `obj.useState(`).
                if (CSharpLexFacts.FindHookCall(code, pattern, 0, code.Length) < 0)
                    continue;

                string hookName = pattern.TrimEnd('(');
                var loc = MakeLoc(filePath, sourceLine);
                diagnostics.Add(
                    Diagnostic.Create(
                        UitkxDiagnostics.HookInEventHandler,
                        loc,
                        hookName,
                        sourceLine
                    )
                );
                break; // one diagnostic per attribute is enough
            }
        }

        // -- Hook in SetupCode (control-block body preamble) -----------------

        /// <summary>
        /// Returns the end offset within <paramref name="bodyCode"/> that is the
        /// start of the first JSX range (paren-wrapped or bare).  This is the
        /// "setup preamble" boundary - the point after which content belongs to
        /// the JSX expression and to nested directive bodies, which the tree walk
        /// will visit directly.  Limiting <see cref="ScanCodeForHooks"/> to this
        /// boundary prevents the same hook from being reported once per nesting level.
        /// </summary>
        private static int GetPreambleEnd(
            string bodyCode,
            int bodyCodeOffset,
            ImmutableArray<(int Start, int End, int Line)> markupRanges,
            ImmutableArray<(int Start, int End, int Line)> bareRanges
        )
        {
            int min = bodyCode.Length;
            foreach (var (absStart, _, _) in markupRanges)
            {
                int rel = absStart - bodyCodeOffset;
                if (rel >= 0 && rel < min) min = rel;
            }
            foreach (var (absStart, _, _) in bareRanges)
            {
                int rel = absStart - bodyCodeOffset;
                if (rel >= 0 && rel < min) min = rel;
            }
            return min;
        }

        /// <summary>
        /// Scans a control-block SetupCode string for hook calls.
        /// Uses the same hook patterns as expression/attribute scanning, but reports
        /// with approximate line numbers derived from the block's starting line.
        /// <paramref name="scanEnd"/> limits the scan to the setup preamble
        /// (before the first JSX range) so nested bodies are not double-counted.
        /// </summary>
        private static void ScanCodeForHooks(
            string code,
            int blockStartLine,
            HookContext ctx,
            string filePath,
            IList<Diagnostic> diagnostics,
            int scanEnd = int.MaxValue
        )
        {
            int limit = Math.Min(code.Length, scanEnd);
            foreach (var pattern in s_hookPatterns)
            {
                int idx = CSharpLexFacts.FindHookCall(code, pattern, 0, limit);
                if (idx < 0)
                    continue;

                string hookName = pattern.TrimEnd('(');

                var descriptor = ctx switch
                {
                    HookContext.Conditional => UitkxDiagnostics.HookInConditional,
                    HookContext.Loop => UitkxDiagnostics.HookInLoop,
                    HookContext.Switch => UitkxDiagnostics.HookInSwitch,
                    _ => null,
                };

                if (descriptor == null)
                    continue;

                // Approximate line by counting newlines before the match
                int approxLine = blockStartLine;
                for (int i = 0; i < idx && i < code.Length; i++)
                    if (code[i] == '\n')
                        approxLine++;

                var loc = MakeLoc(filePath, approxLine);
                diagnostics.Add(Diagnostic.Create(descriptor, loc, hookName, approxLine));
                break; // one diagnostic per SetupCode block is enough
            }
        }

        private static void CheckExpressionForHooks(
            string code,
            int sourceLine,
            HookContext ctx,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var pattern in s_hookPatterns)
            {
                int idx = CSharpLexFacts.FindHookCall(code, pattern, 0, code.Length);
                if (idx < 0)
                    continue;

                // Extract the hook name (up to the '(')
                int nameEnd = idx + pattern.Length - 1; // stops before '('
                string hookName = pattern.TrimEnd('(');

                var descriptor = ctx switch
                {
                    HookContext.Conditional => UitkxDiagnostics.HookInConditional,
                    HookContext.Loop => UitkxDiagnostics.HookInLoop,
                    HookContext.Switch => UitkxDiagnostics.HookInSwitch,
                    _ => null,
                };

                if (descriptor == null)
                    continue;

                var loc = MakeLoc(filePath, sourceLine);
                diagnostics.Add(Diagnostic.Create(descriptor, loc, hookName, sourceLine));

                // One diagnostic per pattern per expression is enough
                break;
            }
        }
        private static Location MakeLoc(string filePath, int sourceLine)
        {
            int line0 = Math.Max(0, sourceLine - 1);
            var pos = new LinePosition(line0, 0);
            return Location.Create(filePath, default, new LinePositionSpan(pos, pos));
        }
    }
}
