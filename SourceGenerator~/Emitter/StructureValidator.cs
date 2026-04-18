using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Validates structural properties of the parsed UITKX AST:
    ///
    ///   UITKX0009 ΓÇö Element inside @foreach lacking a key attribute (reconciler stability)
    ///   UITKX0017 ΓÇö More than one root element (component must return a single root)
    ///   UITKX0018 ΓÇö <c>UseEffect</c> called with only a callback; dependency array is missing
    ///   UITKX0019 ΓÇö The loop iterator variable is used directly as the <c>key</c> attribute
    ///               inside a <c>@foreach</c> body (index-as-key anti-pattern)
    /// </summary>
    internal static class StructureValidator
    {
        public static void Validate(
            ImmutableArray<AstNode> rootNodes,
            string filePath,
            IList<Diagnostic> diagnostics,
            DirectiveSet directives = default
        )
        {
            CheckMultipleRoots(rootNodes, filePath, diagnostics);
            CheckUseEffectInSetupCode(directives, filePath, diagnostics);
            ScanControlBlockSetupCodes(rootNodes, filePath, diagnostics);
            WalkForeachForMissingKey(rootNodes, filePath, diagnostics);
            WalkForeachForIndexKey(rootNodes, filePath, diagnostics);
        }

        /// <summary>
        /// Runs the node-level structural checks (missing key, index key,
        /// control-block setup codes) on an arbitrary set of AST nodes — e.g.
        /// markup embedded in setup-code local functions.  Skips whole-file
        /// checks (multiple roots, UseEffect in setup code) that only apply
        /// to the main render root.
        /// </summary>
        public static void ValidateNodes(
            ImmutableArray<AstNode> nodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            ScanControlBlockSetupCodes(nodes, filePath, diagnostics);
            WalkForeachForMissingKey(nodes, filePath, diagnostics);
            WalkForeachForIndexKey(nodes, filePath, diagnostics);
        }

        // ΓöÇΓöÇ UITKX0017 ΓÇö multiple root elements ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private static void CheckMultipleRoots(
            ImmutableArray<AstNode> rootNodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            // Count top-level element nodes
            var elementRoots = rootNodes.OfType<ElementNode>().ToArray();
            if (elementRoots.Length <= 1)
                return;

            // Emit one diagnostic for every extra root beyond the first
            for (int i = 1; i < elementRoots.Length; i++)
            {
                var loc = MakeLoc(filePath, elementRoots[i].SourceLine);
                diagnostics.Add(
                    Diagnostic.Create(UitkxDiagnostics.MultipleRootElements, loc, filePath)
                );
            }
        }

        // ΓöÇΓöÇ UITKX0018 ΓÇö UseEffect missing dependency array ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private static void CheckUseEffectInSetupCode(
            DirectiveSet directives,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            if (!string.IsNullOrWhiteSpace(directives.FunctionSetupCode))
            {
                int setupLine = directives.FunctionSetupStartLine > 0
                    ? directives.FunctionSetupStartLine
                    : directives.ComponentDeclarationLine > 0
                        ? directives.ComponentDeclarationLine
                        : 1;
                ScanUseEffectMissingDeps(directives.FunctionSetupCode, setupLine, filePath, diagnostics);
            }
        }

        // ΓöÇΓöÇ UITKX0018 ΓÇö UseEffect missing deps in control-block SetupCode ΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Recursively walks the AST and scans every control-block's SetupCode
        /// for <c>UseEffect</c> calls missing a dependency array.
        /// </summary>
        private static void ScanControlBlockSetupCodes(
            ImmutableArray<AstNode> nodes,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case IfNode ifn:
                        foreach (var branch in ifn.Branches)
                        {
                            if (!string.IsNullOrWhiteSpace(branch.BodyCode))
                                ScanUseEffectMissingDeps(
                                    branch.BodyCode, branch.BodyCodeLine,
                                    filePath, diagnostics);
                            ScanControlBlockSetupCodes(branch.Body, filePath, diagnostics);
                        }
                        break;

                    case ForeachNode forn:
                        if (!string.IsNullOrWhiteSpace(forn.BodyCode))
                            ScanUseEffectMissingDeps(
                                forn.BodyCode, forn.BodyCodeLine,
                                filePath, diagnostics);
                        ScanControlBlockSetupCodes(forn.Body, filePath, diagnostics);
                        break;

                    case ForNode forNode:
                        if (!string.IsNullOrWhiteSpace(forNode.BodyCode))
                            ScanUseEffectMissingDeps(
                                forNode.BodyCode, forNode.BodyCodeLine,
                                filePath, diagnostics);
                        ScanControlBlockSetupCodes(forNode.Body, filePath, diagnostics);
                        break;

                    case WhileNode wn:
                        if (!string.IsNullOrWhiteSpace(wn.BodyCode))
                            ScanUseEffectMissingDeps(
                                wn.BodyCode, wn.BodyCodeLine,
                                filePath, diagnostics);
                        ScanControlBlockSetupCodes(wn.Body, filePath, diagnostics);
                        break;

                    case SwitchNode sw:
                        foreach (var c in sw.Cases)
                        {
                            if (!string.IsNullOrWhiteSpace(c.BodyCode))
                                ScanUseEffectMissingDeps(
                                    c.BodyCode, c.BodyCodeLine,
                                    filePath, diagnostics);
                            ScanControlBlockSetupCodes(c.Body, filePath, diagnostics);
                        }
                        break;

                    case ElementNode el:
                        ScanControlBlockSetupCodes(el.Children, filePath, diagnostics);
                        break;
                }
            }
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
                    var loc = MakeLoc(filePath, approxLine);
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

        // ΓöÇΓöÇ UITKX0009 ΓÇö @foreach child missing key ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Recursively walks every loop node (@foreach, @for, @while) and emits UITKX0009 for
        /// any direct element child that has no <c>key</c> attribute.
        /// </summary>
        private static void WalkForeachForMissingKey(
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
                        CheckDirectChildrenForMissingKey(forn.Body, forn.SourceLine, filePath, diagnostics);
                        WalkForeachForMissingKey(forn.Body, filePath, diagnostics);
                        break;
                    case ForNode fon:
                        CheckDirectChildrenForMissingKey(fon.Body, fon.SourceLine, filePath, diagnostics);
                        WalkForeachForMissingKey(fon.Body, filePath, diagnostics);
                        break;
                    case WhileNode whn:
                        CheckDirectChildrenForMissingKey(whn.Body, whn.SourceLine, filePath, diagnostics);
                        WalkForeachForMissingKey(whn.Body, filePath, diagnostics);
                        break;
                    case ElementNode el:
                        WalkForeachForMissingKey(el.Children, filePath, diagnostics);
                        break;
                    case IfNode ifn:
                        foreach (var branch in ifn.Branches)
                            WalkForeachForMissingKey(branch.Body, filePath, diagnostics);
                        break;
                    case SwitchNode sw:
                        foreach (var c in sw.Cases)
                            WalkForeachForMissingKey(c.Body, filePath, diagnostics);
                        break;
                }
            }
        }

        private static void CheckDirectChildrenForMissingKey(
            ImmutableArray<AstNode> body,
            int foreachLine,
            string filePath,
            IList<Diagnostic> diagnostics
        )
        {
            foreach (var node in body)
            {
                if (node is not ElementNode el)
                    continue;

                bool hasKey = el.Attributes.Any(a =>
                    string.Equals(a.Name, "key", StringComparison.OrdinalIgnoreCase));

                if (!hasKey)
                {
                    var loc = MakeLoc(filePath, el.SourceLine);
                    diagnostics.Add(
                        Diagnostic.Create(
                            UitkxDiagnostics.ForeachMissingKey,
                            loc,
                            el.TagName,
                            foreachLine,
                            filePath
                        )
                    );
                }
            }
        }

        // ΓöÇΓöÇ UITKX0019 ΓÇö loop iterator variable used as key ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

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
                            CheckDirectChildrenForIndexKey(forn.Body, loopVar, filePath, diagnostics);
                        WalkForeachForIndexKey(forn.Body, filePath, diagnostics);
                        break;
                    }
                    case ForNode fon:
                        WalkForeachForIndexKey(fon.Body, filePath, diagnostics);
                        break;
                    case WhileNode whn:
                        WalkForeachForIndexKey(whn.Body, filePath, diagnostics);
                        break;
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
                // the item identity ΓÇö using it directly as a key is correct and intentional.
                if (
                    keyAttr.Value is CSharpExpressionValue kv
                    && string.Equals(kv.Expression.Trim(), loopVar, StringComparison.Ordinal)
                    && IsPositionalIndexVar(loopVar)
                )
                {
                    var loc = MakeLoc(filePath, el.SourceLine);
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
        private static Location MakeLoc(string filePath, int sourceLine)
        {
            int line0 = Math.Max(0, sourceLine - 1);
            var pos = new LinePosition(line0, 0);
            return Location.Create(filePath, default, new LinePositionSpan(pos, pos));
        }

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
        /// Examples: <c>var item</c> ΓåÆ <c>item</c>, <c>int i</c> ΓåÆ <c>i</c>.
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
