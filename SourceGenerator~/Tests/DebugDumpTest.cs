using System;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace ReactiveUITK.SourceGenerator.Tests;

public class DebugDumpTest
{
    private readonly ITestOutputHelper _out;
    public DebugDumpTest(ITestOutputHelper output) => _out = output;

    [Fact]
    public void DumpGameOverPageOutput()
    {
        const string uitkxSrc = """
            @namespace UI.App.Pages
            @using UnityEngine.UIElements

            component GameOverPage() {
                return (
                    <VisualElement style={Styles.TopContainerSafe}>
                        <Label text="Hello" />
                    </VisualElement>
                );
            }
            """;

        const string companionCs = """
            namespace UI.App.Pages
            {
                public partial class GameOverPage
                {
                    private static class Styles
                    {
                        public static readonly object TopContainerSafe = new object();
                    }
                }
            }
            """;

        // Run with the companion .cs included in the compilation
        var result = GeneratorTestHelper.RunWithExtraCSharp(
            uitkxSrc, companionCs, "GameOverPage.uitkx");

        // Show ALL generated sources
        _out.WriteLine($"Source was produced: {result.SourceWasProduced}");
        _out.WriteLine($"Generated source:\n{result.GeneratedSource ?? "(null)"}");
        _out.WriteLine("=== DIAGNOSTICS ===");
        foreach (var d in result.Diagnostics)
            _out.WriteLine($"  {d.Id} ({d.Severity}): {d.GetMessage()}");

        Assert.True(result.SourceWasProduced, "Expected source to be produced");
        Assert.True(result.SourceContains("partial class GameOverPage"),
            "Expected partial class GameOverPage in generated output");
    }
}
