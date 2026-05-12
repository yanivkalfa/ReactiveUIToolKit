// ════════════════════════════════════════════════════════════════════════════
//  StaticReadonlyStripper — Roslyn helper used by the source-generator's
//  ModuleBodyRewriter to rewrite user-authored module field declarations so:
//
//      public static readonly Style Wrapper = new Style { ... };
//
//  becomes:
//
//      [global::ReactiveUITK.UitkxHmrSwap]
//      public static Style Wrapper = new Style { ... };
//
//  Why: Mono's JIT inlines the object reference for `ldsfld` against an
//  initonly (== `readonly`) static field. Once a Render method is JIT-compiled,
//  reflection-based slot updates from the HMR pipeline (FieldInfo.SetValue)
//  are no longer observed by that compiled code. Removing the initonly flag
//  forces every read to go through the slot. The attribute discriminates
//  generator-managed fields from user-managed mutable statics so the HMR
//  swapper only touches the former.
//
//  Edge-case policy:
//    • const             → untouched (IsStripCandidate returns false)
//    • mutable static    → untouched (IsStripCandidate returns false)
//    • static readonly   → strip readonly + add [UitkxHmrSwap]
//    • nested types      → ModuleBodyRewriter does not descend into nested
//                          types; user-nested types opt out of HMR re-init
//    • static auto-property → backing field is initonly but generator-invisible;
//                            documented limitation — users should declare fields
//                            (not properties) for HMR-swappable module statics
//    • multi-declarator  → single attribute on the declaration covers all variables
//    • attributes / XML doc / trivia → preserved by token-level rewrite
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#nullable disable

namespace ReactiveUITK.SourceGenerator.Emitter
{
    /// <summary>
    /// Stateless helper that rewrites a single <c>static readonly</c> field
    /// declaration into a plain <c>static</c> field decorated with
    /// <c>[global::ReactiveUITK.UitkxHmrSwap]</c>.
    /// </summary>
    internal static class StaticReadonlyStripper
    {
        /// <summary>
        /// The fully-qualified attribute name emitted on every stripped
        /// field. Kept in one place so SG, HMR and tests stay in lockstep.
        /// </summary>
        public const string AttributeQualifiedName = "global::ReactiveUITK.UitkxHmrSwap";

        /// <summary>
        /// Returns <c>true</c> if <paramref name="field"/> is a candidate for
        /// rewriting — i.e. has both <c>static</c> and <c>readonly</c>
        /// modifiers and no <c>const</c> modifier.
        /// </summary>
        public static bool IsStripCandidate(FieldDeclarationSyntax field)
        {
            if (field == null) return false;

            bool hasStatic = false;
            bool hasReadonly = false;
            foreach (var mod in field.Modifiers)
            {
                switch (mod.ValueText)
                {
                    case "static":   hasStatic = true; break;
                    case "readonly": hasReadonly = true; break;
                    case "const":    return false;
                }
            }

            return hasStatic && hasReadonly;
        }

        /// <summary>
        /// Returns a new <see cref="FieldDeclarationSyntax"/> with the
        /// <c>readonly</c> modifier removed and a <c>[UitkxHmrSwap]</c>
        /// attribute prepended. Preserves the original trivia, XML doc,
        /// existing attribute lists, visibility, generic types and
        /// all variable declarators.
        ///
        /// Caller is expected to have verified <see cref="IsStripCandidate"/>
        /// returns <c>true</c>.
        /// </summary>
        public static FieldDeclarationSyntax Strip(FieldDeclarationSyntax field)
        {
            var modifiers = field.Modifiers;
            int readonlyIdx = -1;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].IsKind(SyntaxKind.ReadOnlyKeyword))
                {
                    readonlyIdx = i;
                    break;
                }
            }
            if (readonlyIdx < 0)
                return field;

            var readonlyToken = modifiers[readonlyIdx];
            var leadingTrivia = readonlyToken.LeadingTrivia;
            var trailingTrivia = readonlyToken.TrailingTrivia;
            var newModifiers = modifiers.RemoveAt(readonlyIdx);

            if (readonlyIdx < newModifiers.Count)
            {
                var nextToken = newModifiers[readonlyIdx];
                var merged = nextToken
                    .WithLeadingTrivia(leadingTrivia.AddRange(nextToken.LeadingTrivia))
                    .WithTrailingTrivia(trailingTrivia.AddRange(nextToken.TrailingTrivia));
                newModifiers = newModifiers.Replace(nextToken, merged);
            }
            else
            {
                var declType = field.Declaration.Type;
                var mergedType = declType
                    .WithLeadingTrivia(leadingTrivia.AddRange(declType.GetLeadingTrivia()))
                    .WithTrailingTrivia(declType.GetTrailingTrivia());
                field = field.WithDeclaration(field.Declaration.WithType(mergedType));
            }

            var attrName = SyntaxFactory.ParseName(AttributeQualifiedName);
            var attribute = SyntaxFactory.Attribute(attrName);
            var attributeList = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(attribute));

            var origLeading = field.GetLeadingTrivia();
            attributeList = attributeList
                .WithLeadingTrivia(origLeading)
                .WithTrailingTrivia(
                    SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed));

            var newAttributeLists = field.AttributeLists.Insert(0, attributeList);
            var withoutOldLeading = field.WithoutLeadingTrivia();
            var rewritten = withoutOldLeading
                .WithAttributeLists(newAttributeLists)
                .WithModifiers(newModifiers);

            return rewritten;
        }
    }
}
