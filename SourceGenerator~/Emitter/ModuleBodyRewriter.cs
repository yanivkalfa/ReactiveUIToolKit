// ════════════════════════════════════════════════════════════════════════════
//  ModuleBodyRewriter — SG-side implementation
//
//  Transforms each top-level `static` method declared inside a `module { … }`
//  body into the HMR-friendly trampoline pattern (matches HookEmitter.cs):
//
//     #if UNITY_EDITOR
//     private delegate <ret> __<Name>_h<sig>_Delegate(<params>);
//     internal static __<Name>_h<sig>_Delegate __hmr_<Name>_h<sig> = __<Name>_body_h<sig>;
//     #endif
//
//     public static <ret> <Name>(<params>)
//     {
//     #if UNITY_EDITOR
//         if (HmrState.IsActive) { __hmr_<Name>_h<sig>(<args>); return; }
//     #endif
//         __<Name>_body_h<sig>(<args>);
//     }
//
//     private static <ret> __<Name>_body_h<sig>(<params>)
//     {
//         /* user body */
//     }
//
//  Why custom delegates and not Func/Action?
//     `ref`, `out`, `in`, and pointer parameters cannot be expressed by the
//     framework Func<>/Action<> generic types. ~30 % of static methods in
//     the bundled samples (DoomGame.GameLogic) use `ref` parameters. A custom
//     delegate type is the only correct way.
//
//  Why a hash suffix on every name?
//     Modules can have overloaded methods. A signature-based suffix gives
//     each overload a unique identifier and makes the swapper logic uniform
//     (one __hmr_<n>_h<x> field per method, never name-collision).
//
//  What is NOT rewritten:
//     • Non-static methods, partials, externs, abstracts, unsafe methods.
//     • Properties, operators, conversions, ctors/dtors.
//     • Fields (consts, static readonly, mutable static, instance).
//     • Nested types (class/struct/enum/record/interface).
//     All of the above are emitted verbatim — current behaviour preserved.
//
//  Failure mode:
//     If Roslyn cannot parse the module body, this rewriter returns
//     `RewriteResult.Verbatim` — the caller emits the original body
//     unchanged and reports UITKX0150 (Info-severity diagnostic).

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#nullable disable

namespace ReactiveUITK.SourceGenerator.Emitter
{
    internal readonly struct RewriteResult
    {
        /// <summary>The transformed module body. Always non-null; on failure equals <see cref="OriginalBody"/>.</summary>
        public readonly string TransformedBody;
        /// <summary>The original (verbatim) body. Returned when parsing failed.</summary>
        public readonly string OriginalBody;
        /// <summary>True if Roslyn parsing failed and we fell back to verbatim.</summary>
        public readonly bool ParseFailed;
        /// <summary>Description of the parse failure (only set when <see cref="ParseFailed"/> is true).</summary>
        public readonly string FailureReason;

        public RewriteResult(string transformed, string original, bool failed, string reason)
        {
            TransformedBody = transformed;
            OriginalBody = original;
            ParseFailed = failed;
            FailureReason = reason;
        }
    }

    internal static class ModuleBodyRewriter
    {
        /// <summary>
        /// Rewrites the body of a <c>module {…}</c> declaration so that every
        /// top-level <c>static</c> method becomes a public trampoline +
        /// delegate field + private body method (the HMR-swappable shape).
        /// All non-method members are emitted verbatim.
        /// </summary>
        /// <param name="originalBody">Raw user body, between (but not including) the outer braces.</param>
        /// <param name="moduleName">Name of the enclosing module — used in #line commentary only.</param>
        /// <param name="bodyStartLine">1-based line number in the .uitkx file where the body begins.</param>
        /// <param name="linePath">Forward-slashed absolute path used in #line directives.</param>
        public static RewriteResult Rewrite(
            string originalBody,
            string moduleName,
            int bodyStartLine,
            string linePath)
        {
            if (string.IsNullOrEmpty(originalBody))
                return new RewriteResult(originalBody ?? string.Empty, originalBody ?? string.Empty, false, null);

            // Wrap in a synthetic class so Roslyn can parse class members (free-floating
            // members are not parseable as a CompilationUnitSyntax). We keep track of how
            // many lines the wrapper added so #line directives can be back-translated to
            // the user's original .uitkx coordinates.
            const string WrapperHead = "namespace __W { class __C {\n";
            const string WrapperTail = "\n} }";
            int wrapperLineOffset = 1; // "namespace __W { class __C {" is 1 line above user code

            string wrapped = WrapperHead + originalBody + WrapperTail;

            SyntaxTree tree;
            try
            {
                tree = CSharpSyntaxTree.ParseText(wrapped);
            }
            catch (Exception ex)
            {
                return new RewriteResult(originalBody, originalBody, true, ex.Message);
            }

            // Surface only ERROR-level parse diagnostics; warnings about unused symbols
            // etc. are unrelated to whether we can safely rewrite the body.
            var parseErrors = tree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();
            if (parseErrors.Count > 0)
            {
                return new RewriteResult(
                    originalBody,
                    originalBody,
                    true,
                    parseErrors[0].GetMessage());
            }

            var classNode = tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();
            if (classNode == null)
                return new RewriteResult(originalBody, originalBody, true, "no class node in wrapped tree");

            var sb = new StringBuilder(originalBody.Length + 512);
            foreach (var member in classNode.Members)
            {
                EmitMember(sb, member, wrapperLineOffset, bodyStartLine, linePath);
            }

            return new RewriteResult(sb.ToString(), originalBody, false, null);
        }

        // ── Member dispatch ────────────────────────────────────────────────────

        private static void EmitMember(
            StringBuilder sb,
            MemberDeclarationSyntax member,
            int wrapperLineOffset,
            int bodyStartLine,
            string linePath)
        {
            if (member is MethodDeclarationSyntax method && IsHotSwappable(method))
            {
                EmitTrampoline(sb, method, wrapperLineOffset, bodyStartLine, linePath);
                return;
            }

            // Verbatim emit with #line mapped back to the user's file. Use the
            // member's leading-trivia start so attached XML doc comments / attribute
            // lists keep their position.
            int wrappedLine = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1; // 1-based
            int userLine = wrappedLine - wrapperLineOffset + bodyStartLine - 1;

            sb.Append("#line ").Append(userLine).Append(" \"").Append(linePath).AppendLine("\"");
            sb.AppendLine(member.ToFullString().TrimEnd());
            sb.AppendLine("#line hidden");
        }

        // ── Eligibility ────────────────────────────────────────────────────────

        private static bool IsHotSwappable(MethodDeclarationSyntax method)
        {
            // No body (interface/abstract/extern/partial-without-impl) → can't swap.
            if (method.Body == null && method.ExpressionBody == null) return false;

            bool hasStatic = false;
            foreach (var mod in method.Modifiers)
            {
                switch (mod.ValueText)
                {
                    case "static":   hasStatic = true; break;
                    case "partial":  return false;
                    case "extern":   return false;
                    case "abstract": return false;
                    case "unsafe":   return false; // would require unsafe delegate type — out of scope
                }
            }
            if (!hasStatic) return false;

            // Reserve our own naming prefix.
            if (method.Identifier.ValueText.StartsWith("__", StringComparison.Ordinal))
                return false;

            return true;
        }

        /// <summary>
        /// Extracts the C# accessibility keyword(s) from a method's modifier
        /// list, joining <c>protected internal</c> / <c>private protected</c>
        /// pairs back into a single token. Returns <c>"private"</c> when no
        /// visibility modifier is declared (the C# default for class
        /// members), so the synthesized trampoline / delegate / HMR field
        /// trio retain matching accessibility and never trip CS0050 / CS0051
        /// / CS0052 / CS0058 / CS0059 against the original method's
        /// parameter or return types.
        /// </summary>
        private static string ExtractVisibility(SyntaxTokenList modifiers)
        {
            bool hasPublic = false, hasInternal = false, hasProtected = false, hasPrivate = false;
            foreach (var mod in modifiers)
            {
                switch (mod.ValueText)
                {
                    case "public":    hasPublic = true; break;
                    case "internal":  hasInternal = true; break;
                    case "protected": hasProtected = true; break;
                    case "private":   hasPrivate = true; break;
                }
            }

            if (hasPublic) return "public";
            if (hasProtected && hasInternal) return "protected internal";
            if (hasPrivate && hasProtected)  return "private protected";
            if (hasInternal) return "internal";
            if (hasProtected) return "protected";
            if (hasPrivate) return "private";
            return "private"; // C# default for class members
        }

        // ── Trampoline emission ────────────────────────────────────────────────

        private static void EmitTrampoline(
            StringBuilder sb,
            MethodDeclarationSyntax method,
            int wrapperLineOffset,
            int bodyStartLine,
            string linePath)
        {
            string name = method.Identifier.ValueText;
            string returnType = method.ReturnType.ToString();
            bool isVoid = returnType == "void";
            bool isGeneric = method.TypeParameterList != null
                && method.TypeParameterList.Parameters.Count > 0;

            // Original parameter list rendered verbatim (preserves modifiers,
            // default values, attributes). Used for the public trampoline AND
            // the body method AND the delegate type.
            string paramListSig = RenderParamList(method.ParameterList, includeDefaults: true);
            string paramListSigNoDefaults = RenderParamList(method.ParameterList, includeDefaults: false);

            // Argument list for forwarding the call, with proper ref/out/in keywords.
            string argList = RenderArgList(method.ParameterList);

            // Generic suffix and constraints.
            string genericSuffix = isGeneric ? method.TypeParameterList.ToString() : string.Empty;
            string constraints = method.ConstraintClauses.Count == 0
                ? string.Empty
                : " " + string.Join(" ", method.ConstraintClauses.Select(c => c.ToString()));

            // Stable, deterministic hash of the canonical signature. Distinguishes
            // overloads, never collides with user-named identifiers because of the
            // __hmr_/__name_body_ prefixes.
            string sigHash = ComputeSignatureHash(method);

            string hmrFieldName = $"__hmr_{name}_h{sigHash}";
            string bodyMethodName = $"__{name}_body_h{sigHash}";
            string delegateTypeName = $"__{name}_h{sigHash}_Delegate";

            // Original method attributes (e.g. [Obsolete], [Conditional]) propagate
            // to the trampoline so external behaviour is unchanged.
            string attributes = string.Join(string.Empty, method.AttributeLists.Select(a => a.ToFullString()));

            // Preserve the original method's visibility on the trampoline, the
            // synthesized delegate type, and the HMR field. Otherwise a `private
            // static` method using a `private` nested type would generate a
            // `public static` trampoline / `internal delegate` whose return or
            // parameter types are less accessible than the synthesized declaration
            // (CS0050 / CS0051 / CS0052 / CS0058 / CS0059). Default to `internal`
            // when no visibility is declared (matches C# default-method-visibility
            // for class members in user-authored module bodies, which expect a
            // working public surface where `public` was intended).
            string visibility = ExtractVisibility(method.Modifiers);

            // 1-based line of the user-visible body start.
            int wrappedBodyLine = (method.Body ?? (SyntaxNode)method.ExpressionBody)
                .GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            int userBodyLine = wrappedBodyLine - wrapperLineOffset + bodyStartLine - 1;

            // ── Custom delegate + HMR field (Editor-only) ──────────────────
            sb.AppendLine("#if UNITY_EDITOR");
            if (isGeneric)
            {
                // Generic delegate mirrors the method's generic arity & constraints.
                // Visibility tracks the trampoline so a `private` method with
                // `private` nested-type parameters does not trip CS0058/CS0059.
                sb.Append("        ").Append(visibility).Append(" delegate ").Append(returnType).Append(' ')
                  .Append(delegateTypeName).Append(genericSuffix).Append('(')
                  .Append(paramListSigNoDefaults).Append(')').Append(constraints).AppendLine(";");

                sb.AppendLine("        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                sb.Append("        ").Append(visibility).Append(" static global::System.Reflection.MethodInfo ")
                  .Append(hmrFieldName).AppendLine(" = null;");
                sb.AppendLine("        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                sb.Append("        ").Append(visibility).Append(" static readonly global::System.Collections.Concurrent.ConcurrentDictionary<global::System.Type, global::System.Delegate> ")
                  .Append(hmrFieldName).AppendLine("_cache = new();");
            }
            else
            {
                // Visibility tracks the trampoline (CS0052 + nested-type accessibility).
                sb.Append("        ").Append(visibility).Append(" delegate ").Append(returnType).Append(' ')
                  .Append(delegateTypeName).Append('(')
                  .Append(paramListSigNoDefaults).AppendLine(");");
                sb.AppendLine("        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                sb.Append("        ").Append(visibility).Append(" static ").Append(delegateTypeName).Append(' ')
                  .Append(hmrFieldName).Append(" = ").Append(bodyMethodName).AppendLine(";");
            }
            sb.AppendLine("#endif");

            // ── Trampoline (preserves original visibility) ──────────────
            sb.Append(attributes); // attribute lists already include trailing newline
            sb.Append("        ").Append(visibility).Append(" static ").Append(returnType).Append(' ').Append(name)
              .Append(genericSuffix).Append('(').Append(paramListSig).Append(')')
              .Append(constraints).AppendLine();
            sb.AppendLine("        {");

            sb.AppendLine("#if UNITY_EDITOR");
            if (isGeneric)
            {
                sb.Append("            if (global::ReactiveUITK.Core.HmrState.IsActive && ")
                  .Append(hmrFieldName).AppendLine(" != null)");
                sb.AppendLine("            {");

                // Build typeof argument list from the generic type parameter names.
                string typeofArgs = BuildTypeofArgs(method.TypeParameterList);
                sb.Append("                var __del = (").Append(delegateTypeName).Append(genericSuffix).Append(')')
                  .Append(hmrFieldName).AppendLine("_cache");
                sb.Append("                    .GetOrAdd(typeof(").Append(typeofArgs).AppendLine("), __t =>");
                sb.AppendLine("                    {");
                sb.Append("                        var __closed = ").Append(hmrFieldName)
                  .Append(".MakeGenericMethod(");
                AppendTypeArgs(sb, method.TypeParameterList);
                sb.AppendLine(");");
                sb.Append("                        return global::System.Delegate.CreateDelegate(typeof(")
                  .Append(delegateTypeName).Append(genericSuffix).AppendLine("), __closed);");
                sb.AppendLine("                    });");

                if (isVoid)
                {
                    sb.Append("                __del(").Append(argList).AppendLine("); return;");
                }
                else
                {
                    sb.Append("                return __del(").Append(argList).AppendLine(");");
                }
                sb.AppendLine("            }");
            }
            else
            {
                sb.AppendLine("            if (global::ReactiveUITK.Core.HmrState.IsActive)");
                if (isVoid)
                {
                    sb.Append("            { ").Append(hmrFieldName).Append('(').Append(argList).AppendLine("); return; }");
                }
                else
                {
                    sb.Append("                return ").Append(hmrFieldName).Append('(').Append(argList).AppendLine(");");
                }
            }
            sb.AppendLine("#endif");

            if (isVoid)
            {
                sb.Append("            ").Append(bodyMethodName).Append(genericSuffix)
                  .Append('(').Append(argList).AppendLine(");");
            }
            else
            {
                sb.Append("            return ").Append(bodyMethodName).Append(genericSuffix)
                  .Append('(').Append(argList).AppendLine(");");
            }
            sb.AppendLine("        }");

            // ── Body method ────────────────────────────────────────────────
            sb.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
            sb.AppendLine("        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            sb.Append("        private static ").Append(returnType).Append(' ').Append(bodyMethodName)
              .Append(genericSuffix).Append('(').Append(paramListSigNoDefaults).Append(')')
              .Append(constraints).AppendLine();

            // Body itself with #line mapping back to the .uitkx file.
            if (method.Body != null)
            {
                sb.Append("#line ").Append(userBodyLine).Append(" \"").Append(linePath).AppendLine("\"");
                sb.AppendLine(method.Body.ToFullString().TrimEnd());
                sb.AppendLine("#line hidden");
            }
            else if (method.ExpressionBody != null)
            {
                sb.AppendLine("        {");
                sb.Append("#line ").Append(userBodyLine).Append(" \"").Append(linePath).AppendLine("\"");
                if (isVoid)
                {
                    sb.Append("            ").Append(method.ExpressionBody.Expression.ToString()).AppendLine(";");
                }
                else
                {
                    sb.Append("            return ").Append(method.ExpressionBody.Expression.ToString()).AppendLine(";");
                }
                sb.AppendLine("#line hidden");
                sb.AppendLine("        }");
            }

            sb.AppendLine();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string RenderParamList(ParameterListSyntax paramList, bool includeDefaults)
        {
            if (paramList == null || paramList.Parameters.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < paramList.Parameters.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var p = paramList.Parameters[i];

                // Modifiers (ref/out/in/params/this).
                foreach (var mod in p.Modifiers)
                {
                    sb.Append(mod.ValueText).Append(' ');
                }

                if (p.Type != null)
                    sb.Append(p.Type.ToString()).Append(' ');
                sb.Append(p.Identifier.ValueText);

                if (includeDefaults && p.Default != null)
                    sb.Append(' ').Append(p.Default.ToString());
            }
            return sb.ToString();
        }

        private static string RenderArgList(ParameterListSyntax paramList)
        {
            if (paramList == null || paramList.Parameters.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < paramList.Parameters.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var p = paramList.Parameters[i];

                foreach (var mod in p.Modifiers)
                {
                    // `params` and `this` are call-site sugar — don't forward them.
                    var v = mod.ValueText;
                    if (v == "params" || v == "this") continue;
                    sb.Append(v).Append(' ');
                }
                sb.Append(p.Identifier.ValueText);
            }
            return sb.ToString();
        }

        private static string BuildTypeofArgs(TypeParameterListSyntax tpl)
        {
            // For typeof() — single type-arg uses the name directly,
            // multiple type-args wrap in a value-tuple typeof which gives a
            // distinct cache key per concrete combination.
            var ps = tpl.Parameters;
            if (ps.Count == 1) return ps[0].Identifier.ValueText;

            var sb = new StringBuilder("(");
            for (int i = 0; i < ps.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(ps[i].Identifier.ValueText);
            }
            sb.Append(')');
            return sb.ToString();
        }

        private static void AppendTypeArgs(StringBuilder sb, TypeParameterListSyntax tpl)
        {
            var ps = tpl.Parameters;
            for (int i = 0; i < ps.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(ps[i].Identifier.ValueText);
            }
        }

        /// <summary>
        /// Computes a deterministic 8-hex FNV-1a 32-bit hash of the method's
        /// canonical signature. Distinguishes overloads; never seen by the
        /// user (only appears inside synthesized identifiers).
        /// </summary>
        private static string ComputeSignatureHash(MethodDeclarationSyntax method)
        {
            var sb = new StringBuilder();
            // Include arity so `Foo<T>(int)` and `Foo(int)` get distinct hashes.
            int arity = method.TypeParameterList?.Parameters.Count ?? 0;
            sb.Append('A').Append(arity).Append('|');

            if (method.ParameterList != null)
            {
                bool first = true;
                foreach (var p in method.ParameterList.Parameters)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    foreach (var mod in p.Modifiers)
                    {
                        var v = mod.ValueText;
                        // `params` is call-site sugar but DOES affect overload identity,
                        // so include it in the hash. `this` is also significant for
                        // extension-method detection though we don't support those.
                        if (v != "this") sb.Append(v).Append(' ');
                    }
                    sb.Append(p.Type?.ToString() ?? "?");
                }
            }

            return Fnv1a32Hex(sb.ToString());
        }

        private static string Fnv1a32Hex(string s)
        {
            const uint OffsetBasis = 2166136261;
            const uint Prime = 16777619;
            uint hash = OffsetBasis;
            for (int i = 0; i < s.Length; i++)
            {
                hash ^= s[i];
                hash *= Prime;
            }
            return hash.ToString("x8");
        }
    }
}
