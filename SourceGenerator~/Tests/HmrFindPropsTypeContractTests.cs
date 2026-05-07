using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Contract tests for the props-type lookup performed by HMR's
/// <c>HmrCSharpEmitter.FindPropsType</c>. The HMR code lives in
/// <c>ReactiveUITK.Editor.asmdef</c> which depends on <c>UnityEditor</c> and
/// therefore cannot be loaded by this standalone .NET test runner — so the
/// algorithm under test is mirrored verbatim below as
/// <see cref="FindPropsTypeMirror"/>.
/// <para>
/// If <c>HmrCSharpEmitter.FindPropsType</c> changes, <see cref="FindPropsTypeMirror"/>
/// MUST change with it. The tests build tiny in-memory assemblies via Roslyn for
/// each of the three resolution paths plus the negative case, so any divergence
/// between the SG / HMR conventions and what these tests assert will fail CI.
/// </para>
/// </summary>
public class HmrFindPropsTypeContractTests
{
    // ── Mirror of HmrCSharpEmitter.FindPropsType (Editor/HMR/HmrCSharpEmitter.cs) ─
    // Keep this byte-for-byte semantically equivalent. The only intentional
    // difference is that assemblies are passed in (rather than reading
    // AppDomain.CurrentDomain.GetAssemblies()) so the test can scope the lookup
    // to a freshly-emitted assembly without leaking state.

    private static string FindPropsTypeMirror(string typeName, params Assembly[] assemblies)
    {
        foreach (var asm in assemblies)
        {
            if (asm.IsDynamic) continue;
            Type[] types;
            try { types = asm.GetTypes(); }
            catch { continue; }

            foreach (var type in types)
            {
                if (type.Name != typeName) continue;

                // Step 1 — sibling top-level {typeName}Props in same namespace
                string siblingName = typeName + "Props";
                string siblingFullName = string.IsNullOrEmpty(type.Namespace)
                    ? siblingName
                    : type.Namespace + "." + siblingName;
                var sibling = asm.GetType(siblingFullName, throwOnError: false);
                if (sibling != null
                    && sibling.GetInterface("ReactiveUITK.Core.IProps") != null)
                {
                    return "global::" + siblingFullName;
                }

                // Step 2 — nested {typeName}.{typeName}Props
                var nestedNamed = type.GetNestedType(siblingName);
                if (nestedNamed != null
                    && nestedNamed.GetInterface("ReactiveUITK.Core.IProps") != null)
                {
                    return $"{typeName}.{siblingName}";
                }

                // Step 3 — any nested IProps (legacy fallback)
                foreach (var nested in type.GetNestedTypes())
                {
                    if (nested.GetInterface("ReactiveUITK.Core.IProps") != null)
                        return $"{typeName}.{nested.Name}";
                }
            }
        }
        return $"{typeName}.{typeName}Props";
    }

    // ── Test fixture: build tiny in-memory assemblies via Roslyn ────────────────

    private const string IPropsStub = """
        namespace ReactiveUITK.Core
        {
            public interface IProps {}
        }
        """;

    private static Assembly CompileAndLoad(string source, string asmName)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var stub = CSharpSyntaxTree.ParseText(IPropsStub);

        var compilation = CSharpCompilation.Create(
            asmName,
            new[] { tree, stub },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var emit = compilation.Emit(ms);
        Assert.True(
            emit.Success,
            $"Test fixture failed to compile: {string.Join(", ", emit.Diagnostics)}");

        return Assembly.Load(ms.ToArray());
    }

    // ── Resolution path 1: sibling top-level (the RouterFunc / RouterFuncProps case) ─

    [Fact]
    public void FindPropsType_SiblingTopLevel_ReturnsGloballyQualifiedName()
    {
        // Mirrors ReactiveUITK.Router.{RouterFunc, RouterFuncProps} — both at
        // namespace scope, neither nested in the other.
        const string src = """
            using ReactiveUITK.Core;
            namespace MyApp.Routing
            {
                public static class RouterFunc {}
                public sealed class RouterFuncProps : IProps {}
            }
            """;

        var asm = CompileAndLoad(src, "Test_SiblingTopLevel");

        var result = FindPropsTypeMirror("RouterFunc", asm);

        Assert.Equal("global::MyApp.Routing.RouterFuncProps", result);
    }

    // ── Resolution path 2: nested {Type}.{Type}Props (SG-emitted convention) ─

    [Fact]
    public void FindPropsType_NestedSameNameProps_ReturnsBareNestedName()
    {
        const string src = """
            using ReactiveUITK.Core;
            namespace MyApp
            {
                public static class CompFunc
                {
                    public sealed class CompFuncProps : IProps {}
                }
            }
            """;

        var asm = CompileAndLoad(src, "Test_NestedNamed");

        var result = FindPropsTypeMirror("CompFunc", asm);

        Assert.Equal("CompFunc.CompFuncProps", result);
    }

    // ── Resolution path 3: any nested IProps (legacy ValuesBarFunc.Props) ─

    [Fact]
    public void FindPropsType_NestedDifferentlyNamedIProps_ReturnsNestedName()
    {
        const string src = """
            using ReactiveUITK.Core;
            namespace MyApp
            {
                public static class ValuesBarFunc
                {
                    public sealed class Props : IProps {}
                }
            }
            """;

        var asm = CompileAndLoad(src, "Test_NestedLegacy");

        var result = FindPropsTypeMirror("ValuesBarFunc", asm);

        Assert.Equal("ValuesBarFunc.Props", result);
    }

    // ── Priority: sibling top-level wins over a nested fallback ─────────────────

    [Fact]
    public void FindPropsType_SiblingPlusNestedLegacy_PrefersSiblingTopLevel()
    {
        const string src = """
            using ReactiveUITK.Core;
            namespace MyApp.Routing
            {
                public static class RouterFunc
                {
                    public sealed class SomeOtherProps : IProps {}
                }
                public sealed class RouterFuncProps : IProps {}
            }
            """;

        var asm = CompileAndLoad(src, "Test_SiblingWins");

        var result = FindPropsTypeMirror("RouterFunc", asm);

        Assert.Equal("global::MyApp.Routing.RouterFuncProps", result);
    }

    // ── Negative: nothing found — falls back to the convention string ──────────

    [Fact]
    public void FindPropsType_NoPropsAnywhere_FallsBackToConventionString()
    {
        const string src = """
            namespace MyApp
            {
                public static class BareFunc {}
            }
            """;

        var asm = CompileAndLoad(src, "Test_NoProps");

        var result = FindPropsTypeMirror("BareFunc", asm);

        // The fallback is intentionally a non-existent nested name so the
        // resulting CS error points at a recognizable location if the type
        // is genuinely missing in a consumer's project.
        Assert.Equal("BareFunc.BareFuncProps", result);
    }
}
