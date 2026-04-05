using System.Collections.Immutable;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language.Lowering;

/// <summary>
/// Canonical lowering stage that normalizes function-style components.
///
/// Previously hoisted <c>FunctionSetupCode</c> into a synthetic
/// <c>CodeBlockNode</c>. Now that setup code is emitted directly by the
/// emitters from <see cref="DirectiveSet.FunctionSetupCode"/>, this pass
/// simply returns the parsed roots unchanged.
/// </summary>
public static class CanonicalLowering
{
    public static ImmutableArray<AstNode> LowerToRenderRoots(
        DirectiveSet directives,
        ImmutableArray<AstNode> parsedRoots,
        string filePath
    )
    {
        return parsedRoots;
    }
}
