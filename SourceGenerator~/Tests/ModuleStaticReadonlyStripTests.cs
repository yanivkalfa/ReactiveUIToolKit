using System.Linq;
using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// End-to-end coverage for B28 — the SG strips `readonly` from every
/// top-level <c>static readonly</c> module field and decorates the result
/// with <c>[global::ReactiveUITK.UitkxHmrSwap]</c>.
/// </summary>
public class ModuleStaticReadonlyStripTests
{
    [Fact]
    public void Module_StaticReadonlyField_StripsAndAddsAttribute()
    {
        const string src = """
            @namespace Test
            module Sidebar {
                public static readonly int X = 16;
            }
            """;
        var result = GeneratorTestHelper.Run(src, "Sidebar.uitkx");

        Assert.True(result.SourceWasProduced,
            $"No source produced. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.GetMessage()))}");
        Assert.Contains("[global::ReactiveUITK.UitkxHmrSwap]", result.GeneratedSource);
        Assert.Contains("public static int X = 16", result.GeneratedSource);
        Assert.DoesNotContain("public static readonly int X", result.GeneratedSource);
    }

    [Fact]
    public void Module_MultipleStaticReadonlyFields_AllStripped()
    {
        const string src = """
            @namespace Test
            module Theme {
                public static readonly string A = "a";
                public static readonly string B = "b";
            }
            """;
        var result = GeneratorTestHelper.Run(src, "Theme.uitkx");

        Assert.True(result.SourceWasProduced);
        // Both fields must be decorated.
        int attrCount = CountOccurrences(result.GeneratedSource, "[global::ReactiveUITK.UitkxHmrSwap]");
        Assert.True(attrCount >= 2,
            $"Expected at least 2 [UitkxHmrSwap] attributes, found {attrCount}. Got:\n{result.GeneratedSource}");
        Assert.DoesNotContain("static readonly", result.GeneratedSource);
    }

    [Fact]
    public void Module_ConstField_Untouched()
    {
        const string src = """
            @namespace Test
            module Cfg {
                public const int VERSION = 7;
            }
            """;
        var result = GeneratorTestHelper.Run(src, "Cfg.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.Contains("public const int VERSION = 7", result.GeneratedSource);
        Assert.DoesNotContain("[global::ReactiveUITK.UitkxHmrSwap]", result.GeneratedSource);
    }

    [Fact]
    public void Module_MutableStatic_Untouched()
    {
        const string src = """
            @namespace Test
            module State {
                public static int Counter;
            }
            """;
        var result = GeneratorTestHelper.Run(src, "State.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.Contains("public static int Counter", result.GeneratedSource);
        Assert.DoesNotContain("[global::ReactiveUITK.UitkxHmrSwap]", result.GeneratedSource);
    }

    [Fact]
    public void Module_StaticReadonly_PreservesExistingAttribute()
    {
        const string src = """
            @namespace Test
            @using System

            module M {
                [System.Obsolete] public static readonly int X = 1;
            }
            """;
        var result = GeneratorTestHelper.Run(src, "M.uitkx");

        Assert.True(result.SourceWasProduced);
        Assert.Contains("[System.Obsolete]", result.GeneratedSource);
        Assert.Contains("[global::ReactiveUITK.UitkxHmrSwap]", result.GeneratedSource);
        Assert.DoesNotContain("static readonly", result.GeneratedSource);
    }

    private static int CountOccurrences(string source, string needle)
    {
        int count = 0;
        int idx = 0;
        while ((idx = source.IndexOf(needle, idx, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }
}
