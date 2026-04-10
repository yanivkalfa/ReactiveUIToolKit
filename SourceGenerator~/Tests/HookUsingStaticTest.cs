using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

public class HookUsingStaticTest
{
    [Fact]
    public void ComponentFile_GetsUsingStatic_ForPeerHookContainer()
    {
        // A hooks file and a component file in the same namespace
        var hooksContent = @"
@namespace MyApp.UI

hook useCounter() -> (int count, System.Action increment) {
  var (count, setCount) = useState(0);
  System.Action increment = () => setCount(c => c + 1);
  return (count, increment);
}
";
        var componentContent = @"
@namespace MyApp.UI

component TestComp {
  var (count, increment) = useCounter();
  return (
    <VisualElement />
  );
}
";

        string testDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "uitkx_tests");
        
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
            new InMemoryAdditionalText(System.IO.Path.Combine(testDir, "TestComp.hooks.uitkx"), hooksContent),
            new InMemoryAdditionalText(System.IO.Path.Combine(testDir, "TestComp.uitkx"), componentContent)
        );
        
        var driver = CSharpGeneratorDriver.Create(generator).AddAdditionalTexts(addlTexts);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var runResult = driver.GetRunResult();
        
        // Dump ALL generated sources
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Results count: {runResult.Results.Length}");
        foreach (var r in runResult.Results)
        {
            sb.AppendLine($"  Generator: {r.Generator.GetGeneratorType().Name}, Sources: {r.GeneratedSources.Length}, Diags: {r.Diagnostics.Length}");
            foreach (var src in r.GeneratedSources)
            {
                sb.AppendLine($"  --- HintName: {src.HintName} ---");
                var text = src.SourceText.ToString();
                var lines = text.Split('\n');
                for (int i = 0; i < System.Math.Min(40, lines.Length); i++)
                    sb.AppendLine($"    {lines[i]}");
                if (lines.Length > 40) sb.AppendLine($"    ... ({lines.Length - 40} more lines)");
            }
            foreach (var d in r.Diagnostics)
                sb.AppendLine($"  DIAG [{d.Severity}] {d.Id}: {d.GetMessage()}");
        }
        
        System.IO.File.WriteAllText(
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "hook_test_output.txt"),
            sb.ToString());
        
        // Find the component output specifically 
        string? componentSource = null;
        foreach (var r in runResult.Results)
            foreach (var src in r.GeneratedSources)
                if (src.HintName.Contains("TestComp_uitkx"))
                    componentSource = src.SourceText.ToString();
        
        Assert.NotNull(componentSource);
        Assert.Contains("using static MyApp.UI.TestCompHooks;", componentSource);
    }
}
