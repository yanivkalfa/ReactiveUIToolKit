using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReactiveUITK.SourceGenerator.Nodes;

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Walks the AST and enforces ReactiveUITK's Rules of Hooks:
    ///
    ///   UITKX0013 — Hook called inside @if / @else branch
    ///   UITKX0014 — Hook called inside @foreach loop
    ///   UITKX0015 — Hook called inside @switch case
    ///
    /// Detection is text-based: any <see cref="ExpressionNode"/> or
    /// <see cref="CodeBlockNode"/> whose content contains a call matching
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
                        WalkBody(branch.Body, HookContext.Conditional, filePath, diagnostics);
                    break;

                case ForeachNode forn:
                    WalkBody(forn.Body, HookContext.Loop, filePath, diagnostics);
                    break;

                case SwitchNode sw:
                    foreach (var c in sw.Cases)
                        WalkBody(c.Body, HookContext.Switch, filePath, diagnostics);
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

                case CodeBlockNode cb:
                    // @code blocks embedded inside markup (rare — normally hoisted)
                    if (ctx != HookContext.TopLevel)
                        CheckExpressionForHooks(cb.Code, cb.SourceLine, ctx, filePath, diagnostics);
                    break;

                // TextNode, CodeBlockNode at top level — nothing to check
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
