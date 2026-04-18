using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReactiveUITK.Language.Nodes;

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Walks the AST and enforces ReactiveUITK's Rules of Hooks:
    ///
    ///   UITKX0013 ΓÇö Hook called inside @if / @else branch
    ///   UITKX0014 ΓÇö Hook called inside @foreach loop
    ///   UITKX0015 ΓÇö Hook called inside @switch case
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

        // ΓöÇΓöÇ Context tracking ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private enum HookContext
        {
            TopLevel,
            Conditional,
            Loop,
            Switch,
        }

        // ΓöÇΓöÇ Tree walker ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
                        if (!string.IsNullOrWhiteSpace(branch.BodyCode))
                            ScanCodeForHooks(branch.BodyCode, branch.BodyCodeLine,
                                HookContext.Conditional, filePath, diagnostics,
                                GetPreambleEnd(branch.BodyCode, branch.BodyCodeOffset, branch.BodyMarkupRanges, branch.BodyBareJsxRanges));
                        WalkBody(branch.Body, HookContext.Conditional, filePath, diagnostics);
                    }
                    break;

                case ForeachNode forn:
                    if (!string.IsNullOrWhiteSpace(forn.BodyCode))
                        ScanCodeForHooks(forn.BodyCode, forn.BodyCodeLine,
                            HookContext.Loop, filePath, diagnostics,
                            GetPreambleEnd(forn.BodyCode, forn.BodyCodeOffset, forn.BodyMarkupRanges, forn.BodyBareJsxRanges));
                    WalkBody(forn.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case ForNode forNode:
                    if (!string.IsNullOrWhiteSpace(forNode.BodyCode))
                        ScanCodeForHooks(forNode.BodyCode, forNode.BodyCodeLine,
                            HookContext.Loop, filePath, diagnostics,
                            GetPreambleEnd(forNode.BodyCode, forNode.BodyCodeOffset, forNode.BodyMarkupRanges, forNode.BodyBareJsxRanges));
                    WalkBody(forNode.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case WhileNode wn:
                    if (!string.IsNullOrWhiteSpace(wn.BodyCode))
                        ScanCodeForHooks(wn.BodyCode, wn.BodyCodeLine,
                            HookContext.Loop, filePath, diagnostics,
                            GetPreambleEnd(wn.BodyCode, wn.BodyCodeOffset, wn.BodyMarkupRanges, wn.BodyBareJsxRanges));
                    WalkBody(wn.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case SwitchNode sw:
                    foreach (var c in sw.Cases)
                    {
                        if (!string.IsNullOrWhiteSpace(c.BodyCode))
                            ScanCodeForHooks(c.BodyCode, c.BodyCodeLine,
                                HookContext.Switch, filePath, diagnostics,
                                GetPreambleEnd(c.BodyCode, c.BodyCodeOffset, c.BodyMarkupRanges, c.BodyBareJsxRanges));
                        WalkBody(c.Body, HookContext.Switch, filePath, diagnostics);
                    }
                    break;

                case ElementNode el:
                    // Check attribute values for hook calls ΓÇö always wrong, even at top-level
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

                // TextNode ΓÇö nothing to check
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

        // ΓöÇΓöÇ Hook detection ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        // Patterns that indicate a hook call. We match on the call site name only ΓÇö
        // the leading `Hooks.` prefix is optional so hand-written using-aliases work too.
        private static readonly string[] s_hookPatterns =
        {
            "Hooks.UseState(",
            "Hooks.UseEffect(",
            "Hooks.UseRef(",
            "Hooks.UseCallback(",
            "Hooks.UseMemo(",
            "Hooks.UseContext(",
            "Hooks.UseReducer(",
            "Hooks.UseSignal(",
            "Hooks.UseDeferredValue(",
            "Hooks.UseTransition(",
            // Also catch bare names (when imported via `using static ReactiveUITK.Core.Hooks`)
            "UseState(",
            "UseEffect(",
            "UseRef(",
            "UseCallback(",
            "UseMemo(",
            "UseContext(",
            "UseReducer(",
            "UseSignal(",
            "UseDeferredValue(",
            "UseTransition(",
            // React-style camelCase shorthand aliases supported by the emitter
            "useState(",
            "useEffect(",
            "useRef(",
            "useCallback(",
            "useMemo(",
            "useContext(",
            "useReducer(",
            "useSignal(",
            "useDeferredValue(",
            "useTransition(",
        };

        // ΓöÇΓöÇ UITKX0016 ΓÇö hook inside attribute expression ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private static void CheckAttributeForHooks(
            string code,
            int sourceLine,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var pattern in s_hookPatterns)
            {
                if (code.IndexOf(pattern, StringComparison.Ordinal) < 0)
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

        // ΓöÇΓöÇ Hook in SetupCode (control-block body preamble) ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Returns the end offset within <paramref name="bodyCode"/> that is the
        /// start of the first JSX range (paren-wrapped or bare).  This is the
        /// "setup preamble" boundary — the point after which content belongs to
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
                int idx = code.IndexOf(pattern, 0, limit, StringComparison.Ordinal);
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
                int idx = code.IndexOf(pattern, StringComparison.Ordinal);
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
