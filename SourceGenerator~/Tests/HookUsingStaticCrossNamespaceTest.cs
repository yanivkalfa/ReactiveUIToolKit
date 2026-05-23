using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

// Stage 3d cross-namespace coverage. Issue #18 fix: the generator must inject
// `using static <HookNs>.<HookContainer>;` even when the hook file lives in
// a different namespace from the consumer, as long as both belong to the
// same Unity assembly. The asmdef boundary is enforced one layer up in
// UitkxGenerator's pre-scan via IsOwnedByCompilation; this test pins the
// pipeline-level contract.
public class HookUsingStaticCrossNamespaceTest
{
    [Fact]
    public void ComponentInOtherNamespace_GetsUsingStatic_ForPeerHookContainer()
    {
        var hooksContent = @"
@namespace MyApp.Hooks

hook useCounter() -> (int count, System.Action increment) {
  var (count, setCount) = useState(0);
  System.Action increment = () => setCount(c => c + 1);
  return (count, increment);
}
";
        var componentContent = @"
@namespace MyApp.UI

component ConsumerComp {
  var (count, increment) = useCounter();
  return (
    <VisualElement />
  );
}
";

        string testDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "uitkx_tests_xns");
        var stubTree = CSharpSyntaxTree.ParseText(
            "namespace ReactiveUITK.Core { public abstract class VirtualNode { } }",
            path: System.IO.Path.Combine(testDir, "_Stubs.g.cs"));

        var compilation = CSharpCompilation.Create(
            assemblyName: "Assembly-CSharp",
            syntaxTrees: new[] { stubTree },
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new UitkxGenerator();
        var addlTexts = ImmutableArray.Create<AdditionalText>(
            new InMemoryAdditionalText(System.IO.Path.Combine(testDir, "UseCounter.hooks.uitkx"), hooksContent),
            new InMemoryAdditionalText(System.IO.Path.Combine(testDir, "ConsumerComp.uitkx"), componentContent)
        );

        var driver = CSharpGeneratorDriver.Create(generator).AddAdditionalTexts(addlTexts);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var runResult = driver.GetRunResult();

        string? componentSource = null;
        foreach (var r in runResult.Results)
            foreach (var src in r.GeneratedSources)
                if (src.HintName.Contains("ConsumerComp.uitkx"))
                    componentSource = src.SourceText.ToString();

        Assert.NotNull(componentSource);
        // The using-static FQN must reference the HOOK file's namespace, not
        // the consumer's. The container class name is derived from the hook
        // filename minus its first dot-suffix, PascalCased + "Hooks".
        Assert.Contains("using static MyApp.Hooks.UseCounterHooks;", componentSource);
    }

    [Fact]
    public void EmptyComponentNamespace_StillGetsUsingStatic_ForPeerHookContainer()
    {
        // Regression: previously the strict-equality check `phc.Namespace ==
        // directives.Namespace` plus the `!string.IsNullOrEmpty(directives.Namespace)`
        // gate dropped this entire path when the consumer had no @namespace.
        var hooksContent = @"
@namespace MyApp.Hooks

hook useFlag() -> bool {
  var (v, _) = useState(false);
  return v;
}
";
        var componentContent = @"
component NoNsComp {
  var v = useFlag();
  return (
    <VisualElement />
  );
}
";

        string testDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "uitkx_tests_no_ns");
        var stubTree = CSharpSyntaxTree.ParseText(
            "namespace ReactiveUITK.Core { public abstract class VirtualNode { } }",
            path: System.IO.Path.Combine(testDir, "_Stubs.g.cs"));

        var compilation = CSharpCompilation.Create(
            assemblyName: "Assembly-CSharp",
            syntaxTrees: new[] { stubTree },
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new UitkxGenerator();
        var addlTexts = ImmutableArray.Create<AdditionalText>(
            new InMemoryAdditionalText(System.IO.Path.Combine(testDir, "UseFlag.hooks.uitkx"), hooksContent),
            new InMemoryAdditionalText(System.IO.Path.Combine(testDir, "NoNsComp.uitkx"), componentContent)
        );

        var driver = CSharpGeneratorDriver.Create(generator).AddAdditionalTexts(addlTexts);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var runResult = driver.GetRunResult();

        string? componentSource = null;
        foreach (var r in runResult.Results)
            foreach (var src in r.GeneratedSources)
                if (src.HintName.Contains("NoNsComp.uitkx"))
                    componentSource = src.SourceText.ToString();

        Assert.NotNull(componentSource);
        Assert.Contains("using static MyApp.Hooks.UseFlagHooks;", componentSource);
    }
}
