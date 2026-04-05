using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReactiveUITK.Language.Nodes;

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Walks the AST and enforces ReactiveUITK's Rules of Hooks:
    ///
    ///   UITKX0013 — Hook called inside @if / @else branch
    ///   UITKX0014 — Hook called inside @foreach loop
    ///   UITKX0015 — Hook called inside @switch case
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

        // ── Context tracking ──────────────────────────────────────────────────

        private enum HookContext
        {
            TopLevel,
            Conditional,
            Loop,
            Switch,
        }

        // ── Tree walker ───────────────────────────────────────────────────────

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
                        if (!string.IsNullOrWhiteSpace(branch.SetupCode))
                            ScanCodeForHooks(branch.SetupCode, branch.SetupCodeLine,
                                HookContext.Conditional, filePath, diagnostics);
                        WalkBody(branch.Body, HookContext.Conditional, filePath, diagnostics);
                    }
                    break;

                case ForeachNode forn:
                    if (!string.IsNullOrWhiteSpace(forn.SetupCode))
                        ScanCodeForHooks(forn.SetupCode, forn.SetupCodeLine,
                            HookContext.Loop, filePath, diagnostics);
                    WalkBody(forn.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case ForNode forNode:
                    if (!string.IsNullOrWhiteSpace(forNode.SetupCode))
                        ScanCodeForHooks(forNode.SetupCode, forNode.SetupCodeLine,
                            HookContext.Loop, filePath, diagnostics);
                    WalkBody(forNode.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case WhileNode wn:
                    if (!string.IsNullOrWhiteSpace(wn.SetupCode))
                        ScanCodeForHooks(wn.SetupCode, wn.SetupCodeLine,
                            HookContext.Loop, filePath, diagnostics);
                    WalkBody(wn.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case SwitchNode sw:
                    foreach (var c in sw.Cases)
                    {
                        if (!string.IsNullOrWhiteSpace(c.SetupCode))
                            ScanCodeForHooks(c.SetupCode, c.SetupCodeLine,
                                HookContext.Switch, filePath, diagnostics);
                        WalkBody(c.Body, HookContext.Switch, filePath, diagnostics);
                    }
                    break;

                case ElementNode el:
                    // Check attribute values for hook calls — always wrong, even at top-level
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

                // TextNode — nothing to check
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

        // ── Hook detection ────────────────────────────────────────────────────

        // Patterns that indicate a hook call. We match on the call site name only —
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

        // ── UITKX0016 — hook inside attribute expression ─────────────────────

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
                var loc = Location.Create(filePath, default, default);
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

        // ── Hook in SetupCode (control-block body preamble) ─────────────────

        /// <summary>
        /// Scans a control-block SetupCode string for hook calls.
        /// Uses the same hook patterns as expression/attribute scanning, but reports
        /// with approximate line numbers derived from the block's starting line.
        /// </summary>
        private static void ScanCodeForHooks(
            string code,
            int blockStartLine,
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

                var loc = Location.Create(filePath, default, default);
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

                var loc = Location.Create(filePath, default, default);
                diagnostics.Add(Diagnostic.Create(descriptor, loc, hookName, sourceLine));

                // One diagnostic per pattern per expression is enough
                break;
            }
        }
    }
}
