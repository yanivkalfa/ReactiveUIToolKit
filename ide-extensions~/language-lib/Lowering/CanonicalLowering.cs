using System.Collections.Immutable;
using System.Linq;
using ReactiveUITK.Language.Nodes;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.Language.Lowering;

/// <summary>
/// Canonical lowering stage that normalizes authoring forms into one renderable
/// AST shape consumed by downstream validators and emitters.
///
/// Current normalization:
/// - Function-style source (<c>component Name { ... return (...) ... }</c>)
///   is lowered to legacy-compatible roots by hoisting setup code as a synthetic
///   <see cref="CodeBlockNode"/> before parsed markup roots.
/// - Directive-header source is forwarded unchanged.
/// </summary>
public static class CanonicalLowering
{
    public static ImmutableArray<AstNode> LowerToRenderRoots(
        DirectiveSet directives,
        ImmutableArray<AstNode> parsedRoots,
        string filePath
    )
    {
        if (!directives.IsFunctionStyle || string.IsNullOrWhiteSpace(directives.FunctionSetupCode))
            return parsedRoots;

        string setupCode = directives.FunctionSetupCode ?? string.Empty;
        int setupLine = directives.FunctionSetupStartLine > 0
            ? directives.FunctionSetupStartLine
            : directives.ComponentDeclarationLine > 0
                ? directives.ComponentDeclarationLine
                : directives.MarkupStartLine;

        var lowered = ImmutableArray.CreateBuilder<AstNode>(parsedRoots.Length + 1);
        lowered.Add(ParseFunctionSetupAsCodeBlock(setupCode, setupLine, filePath));
        lowered.AddRange(parsedRoots);
        return lowered.ToImmutable();
    }

    private static CodeBlockNode ParseFunctionSetupAsCodeBlock(
        string setupCode,
        int setupLine,
        string filePath
    )
    {
        var leadingLines = setupLine > 1 ? new string('\n', setupLine - 1) : string.Empty;
        string syntheticSource = leadingLines + "@code {" + setupCode + "\n}";

        var parseDiags = new System.Collections.Generic.List<ParseDiagnostic>();
        var directives = DirectiveParser.Parse(syntheticSource, filePath, parseDiags);
        var nodes = UitkxParser.Parse(syntheticSource, filePath, directives, parseDiags);

        var parsedCode = nodes.OfType<CodeBlockNode>().FirstOrDefault();
        if (parsedCode is null)
            return new CodeBlockNode(setupCode, setupLine, filePath);

        return parsedCode with
        {
            SourceLine = setupLine,
            SourceFile = filePath,
        };
    }
}
