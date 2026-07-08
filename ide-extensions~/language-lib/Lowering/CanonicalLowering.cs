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
///
/// U-43 (2026-07-06): confirmed still a pure no-op pass-through, called from three
/// sites (DiagnosticsPublisher, the HMR compiler, and UitkxPipeline). Deliberately kept
/// rather than deleted — a lowering pass is a natural seam if/when a future canonical
/// transform is needed (e.g. desugaring), and removing + re-adding a stage later is more
/// churn than leaving this identity pass in place. If this comment is still accurate a
/// year on with no lowering work planned, delete the stage and its three call sites.
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
