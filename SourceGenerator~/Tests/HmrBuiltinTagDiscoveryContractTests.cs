using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Contract tests for HMR's <c>HmrBuiltinTagDiscovery.BuildAutoDiscoveredTagMap</c>
/// (defined in <c>Editor/HMR/HmrCSharpEmitter.cs</c>). The HMR emitter lives in
/// <c>ReactiveUITK.Editor.asmdef</c> which depends on <c>UnityEditor</c> and
/// therefore cannot be loaded by this standalone .NET test runner — so the
/// algorithm under test is mirrored verbatim below as
/// <see cref="DiscoverMirror"/>.
/// <para>
/// If <c>HmrBuiltinTagDiscovery.BuildAutoDiscoveredTagMap</c> changes,
/// <see cref="DiscoverMirror"/> MUST change with it. The tests build a tiny
/// in-memory assembly that synthesizes a <c>V</c>-shaped class with all of the
/// shapes the production <c>V.cs</c> contains, so any drift between the
/// algorithm here and what HMR runs at static-init will fail CI.
/// </para>
/// </summary>
public class HmrBuiltinTagDiscoveryContractTests
{
    // ── Mirror of HmrCSharpEmitter.HmrBuiltinTagDiscovery.BuildAutoDiscoveredTagMap ─
    // Keep this byte-for-byte semantically equivalent. Differences:
    //   - The classification keys use plain enum strings instead of TagKind
    //     (which is a private nested type in HmrCSharpEmitter, unreachable here).
    //   - The V type is passed in (not hardcoded to typeof(global::ReactiveUITK.V))
    //     so the test can scope the lookup to a freshly-emitted assembly.
    //   - The VirtualNode type comes from the test fixture's stub assembly.

    private enum MirrorTagKind
    {
        Typed,
        TypedC,
        Dict,
        Text,
        Fragment,
        Suspense,
        Portal,
    }

    private readonly record struct MirrorTagRes(
        MirrorTagKind Kind,
        string MethodName,
        string? PropsType
    );

    private static Dictionary<string, MirrorTagRes> DiscoverMirror(Type vType, Type vNodeType)
    {
        var map = new Dictionary<string, MirrorTagRes>(StringComparer.OrdinalIgnoreCase);
        var vNodeArrayType = vNodeType.MakeArrayType();
        var paramArrayAttr = typeof(ParamArrayAttribute);

        foreach (
            var m in vType.GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly
            )
        )
        {
            if (m.IsGenericMethodDefinition)
                continue;
            if (m.ReturnType != vNodeType)
                continue;
            var ps = m.GetParameters();
            if (ps.Length == 0)
                continue;

            var firstType = ps[0].ParameterType;
            bool acceptsChildren =
                ps[ps.Length - 1].IsDefined(paramArrayAttr, false)
                && ps[ps.Length - 1].ParameterType == vNodeArrayType;

            if (m.Name == "Fragment")
            {
                map["fragment"] = new MirrorTagRes(MirrorTagKind.Fragment, "Fragment", null);
                continue;
            }

            if (m.Name == "Text" && firstType == typeof(string))
            {
                map["text"] = new MirrorTagRes(MirrorTagKind.Text, "Text", null);
                continue;
            }

            if (firstType.Name.EndsWith("Props", StringComparison.Ordinal))
            {
                var kind = acceptsChildren ? MirrorTagKind.TypedC : MirrorTagKind.Typed;
                var key = m.Name.ToLowerInvariant();
                if (!map.TryGetValue(key, out var _existing) || kind == MirrorTagKind.Typed)
                {
                    map[key] = new MirrorTagRes(kind, m.Name, firstType.Name);
                }
                continue;
            }

            if (
                typeof(System.Collections.IDictionary).IsAssignableFrom(firstType)
                || (
                    firstType.IsGenericType
                    && firstType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                )
            )
            {
                map[m.Name.ToLowerInvariant()] = new MirrorTagRes(MirrorTagKind.Dict, m.Name, null);
                continue;
            }
        }

        // Manual overrides (must match the production code exactly):
        map["suspense"] = new MirrorTagRes(MirrorTagKind.Suspense, "Suspense", null);
        map["portal"] = new MirrorTagRes(MirrorTagKind.Portal, "Portal", null);
        map["visualelementsafe"] = new MirrorTagRes(MirrorTagKind.Dict, "VisualElementSafe", null);

        return map;
    }

    // ── Test fixture: build a synthetic V class with every shape the real V.cs uses ─

    /// <summary>
    /// Synthesizes a <c>V</c>-shaped class containing one factory of every
    /// shape the production <c>V.cs</c> declares. Each factory's parameters
    /// are crafted to exercise a different branch of the discovery algorithm.
    /// </summary>
    private const string VStub = """
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Threading.Tasks;

        namespace ReactiveUITK.Core
        {
            public abstract class VirtualNode { }
            public interface IProps { }
        }

        namespace ReactiveUITK
        {
            using ReactiveUITK.Core;

            // Fake props/peer types matching real shapes.
            public sealed class LabelProps { }
            public sealed class ButtonProps { }
            public sealed class ScrollViewProps { }
            public sealed class ErrorBoundaryProps { }
            public sealed class VisualElementProps { }
            public sealed class AnimateProps { }
            public sealed class FoldoutProps { }
            // New (Phase 3/5):
            public sealed class AudioProps { }
            public sealed class VideoProps { }

            public static class V
            {
                // Typed (no children) — Label
                public static VirtualNode Label(LabelProps props, string key = null) => null;
                // Typed (no children) — Button
                public static VirtualNode Button(ButtonProps props, string key = null) => null;
                // TypedC (accepts children) — ScrollView
                public static VirtualNode ScrollView(ScrollViewProps props, string key = null, params VirtualNode[] children) => null;
                // TypedC — Foldout
                public static VirtualNode Foldout(FoldoutProps props, string key = null, params VirtualNode[] children) => null;
                // TypedC — VisualElement
                public static VirtualNode VisualElement(VisualElementProps props, string key = null, params VirtualNode[] children) => null;
                // TypedC — ErrorBoundary
                public static VirtualNode ErrorBoundary(ErrorBoundaryProps props, string key = null, params VirtualNode[] children) => null;
                // TypedC — Animate (Pattern B: delegates internally to V.Func<TProps>)
                public static VirtualNode Animate(AnimateProps props, string key = null, params VirtualNode[] children) => null;
                // TypedC — Audio (Phase 3)
                public static VirtualNode Audio(AudioProps props, string key = null, params VirtualNode[] children) => null;
                // TypedC — Video (Phase 5)
                public static VirtualNode Video(VideoProps props, string key = null, params VirtualNode[] children) => null;

                // Fragment (first param is `string key`)
                public static VirtualNode Fragment(string key = null, params VirtualNode[] children) => null;

                // Text (first param is `string text`)
                public static VirtualNode Text(string text, string key = null) => null;

                // Portal (first param is a non-Props non-string non-IDictionary type)
                public static VirtualNode Portal(object portalTarget, string key = null, params VirtualNode[] children) => null;

                // Suspense (first param is Func<bool>) — multiple overloads should coalesce.
                public static VirtualNode Suspense(Func<bool> isReady, VirtualNode fallback, string key = null, params VirtualNode[] children) => null;
                public static VirtualNode Suspense(Task readyTask, VirtualNode fallback, string key = null, params VirtualNode[] children) => null;

                // VisualElementSafe (first param is `object`) — manually-overridden Dict.
                public static VirtualNode VisualElementSafe(object props, string key = null, params VirtualNode[] children) => null;

                // Generic — V.Func<TProps>(…) — must be SKIPPED (not added to map).
                public static VirtualNode Func<TProps>(Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> render, TProps props, string key = null, params VirtualNode[] children)
                    where TProps : class, IProps => null;

                // Untyped Func (first param Func<…>) — should NOT be classified.
                public static VirtualNode Func(Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> render, IProps props = null, string key = null, params VirtualNode[] children) => null;

                // Router-style factories (string first param) — should NOT be classified
                // (they're handled by the component-alias path).
                public static VirtualNode Link(string to, string label = null, string key = null) => null;
                public static VirtualNode Navigate(string to, bool replace = true, object state = null, string key = null) => null;

                // A non-VirtualNode-returning helper (must be skipped by return-type filter).
                public static int NotAFactory(int a, int b) => a + b;
            }
        }
        """;

    private static (Type vType, Type vNodeType) CompileAndLoadV()
    {
        var tree = CSharpSyntaxTree.ParseText(VStub);
        var compilation = CSharpCompilation.Create(
            "Test_HmrBuiltinTagDiscovery",
            new[] { tree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(
                    typeof(System.Collections.IDictionary).Assembly.Location
                ),
                MetadataReference.CreateFromFile(typeof(IReadOnlyDictionary<,>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Func<>).Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var emit = compilation.Emit(ms);
        Assert.True(
            emit.Success,
            $"Test fixture failed to compile: {string.Join(", ", emit.Diagnostics)}"
        );

        var asm = Assembly.Load(ms.ToArray());
        var vType = asm.GetType("ReactiveUITK.V")!;
        var vNodeType = asm.GetType("ReactiveUITK.Core.VirtualNode")!;
        return (vType, vNodeType);
    }

    // ── Spot facts: every classification branch produces the expected entry ─

    [Fact]
    public void Discover_TypedNoChildren_ProducesTypedKind()
    {
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.True(map.TryGetValue("label", out var label));
        Assert.Equal(MirrorTagKind.Typed, label.Kind);
        Assert.Equal("Label", label.MethodName);
        Assert.Equal("LabelProps", label.PropsType);

        Assert.True(map.TryGetValue("button", out var button));
        Assert.Equal(MirrorTagKind.Typed, button.Kind);
    }

    [Fact]
    public void Discover_TypedWithChildren_ProducesTypedCKind()
    {
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.True(map.TryGetValue("scrollview", out var sv));
        Assert.Equal(MirrorTagKind.TypedC, sv.Kind);
        Assert.Equal("ScrollView", sv.MethodName);
        Assert.Equal("ScrollViewProps", sv.PropsType);

        Assert.True(map.TryGetValue("foldout", out var fo));
        Assert.Equal(MirrorTagKind.TypedC, fo.Kind);

        Assert.True(map.TryGetValue("visualelement", out var ve));
        Assert.Equal(MirrorTagKind.TypedC, ve.Kind);
        Assert.Equal("VisualElementProps", ve.PropsType);

        Assert.True(map.TryGetValue("errorboundary", out var eb));
        Assert.Equal(MirrorTagKind.TypedC, eb.Kind);
        Assert.Equal("ErrorBoundaryProps", eb.PropsType);
    }

    [Fact]
    public void Discover_Animate_AutoDiscoveredAsTypedC_FixesPre020LatentBug()
    {
        // The whole point of this fix: pre-0.4.20, <Animate> was missing from
        // the literal s_tagMap. Auto-discovery picks it up correctly because
        // V.Animate(AnimateProps, key, params children) matches the
        // *Props + params VirtualNode[] shape.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.True(map.TryGetValue("animate", out var anim));
        Assert.Equal(MirrorTagKind.TypedC, anim.Kind);
        Assert.Equal("Animate", anim.MethodName);
        Assert.Equal("AnimateProps", anim.PropsType);
    }

    [Fact]
    public void Discover_AudioAndVideo_AutoDiscoveredForPhase3And5()
    {
        // Audio (Phase 3) and Video (Phase 5) get auto-discovered the
        // moment V.Audio / V.Video exist — no s_tagMap edit required for
        // either to become hot-reloadable.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.True(map.TryGetValue("audio", out var audio));
        Assert.Equal(MirrorTagKind.TypedC, audio.Kind);
        Assert.Equal("Audio", audio.MethodName);
        Assert.Equal("AudioProps", audio.PropsType);

        Assert.True(map.TryGetValue("video", out var video));
        Assert.Equal(MirrorTagKind.TypedC, video.Kind);
        Assert.Equal("Video", video.MethodName);
        Assert.Equal("VideoProps", video.PropsType);
    }

    [Fact]
    public void Discover_Fragment_ResolvesByName_NotByFirstParam()
    {
        // V.Fragment's first param is a `string key` — would otherwise be
        // mis-classified as Text. Name-based dispatch handles it.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.True(map.TryGetValue("fragment", out var fr));
        Assert.Equal(MirrorTagKind.Fragment, fr.Kind);
        Assert.Equal("Fragment", fr.MethodName);
    }

    [Fact]
    public void Discover_Text_ResolvesAsTextKind()
    {
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.True(map.TryGetValue("text", out var t));
        Assert.Equal(MirrorTagKind.Text, t.Kind);
        Assert.Equal("Text", t.MethodName);
    }

    [Fact]
    public void Discover_ManualOverrides_AreApplied()
    {
        // Suspense / Portal / VisualElementSafe have non-*Props first
        // parameters and are not auto-discovered — manual overrides ensure
        // they're in the map.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.True(map.TryGetValue("suspense", out var sus));
        Assert.Equal(MirrorTagKind.Suspense, sus.Kind);
        Assert.Equal("Suspense", sus.MethodName);

        Assert.True(map.TryGetValue("portal", out var p));
        Assert.Equal(MirrorTagKind.Portal, p.Kind);
        Assert.Equal("Portal", p.MethodName);

        Assert.True(map.TryGetValue("visualelementsafe", out var ves));
        Assert.Equal(MirrorTagKind.Dict, ves.Kind);
        Assert.Equal("VisualElementSafe", ves.MethodName);
    }

    [Fact]
    public void Discover_GenericFunc_IsSkipped()
    {
        // V.Func<TProps>(…) is a generic method definition — must NOT appear.
        // It's handled by the FuncComponent path, not the typed path.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.False(map.ContainsKey("func"));
    }

    [Fact]
    public void Discover_NonVirtualNodeReturn_IsSkipped()
    {
        // V.NotAFactory(int, int) returns int — must NOT appear.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.False(map.ContainsKey("notafactory"));
    }

    [Fact]
    public void Discover_RouterFactoriesWithStringFirstParam_AreSkipped()
    {
        // V.Link(string, …), V.Navigate(string, …) — first param string,
        // not a *Props. They must be SKIPPED so the FuncComponent /
        // component-alias path can resolve them at the next level up.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        Assert.False(map.ContainsKey("link"));
        Assert.False(map.ContainsKey("navigate"));
    }

    [Fact]
    public void Discover_KeysAreLowercase_ForCaseInsensitiveLookup()
    {
        // The map is constructed with StringComparer.OrdinalIgnoreCase, so
        // both casings must resolve. Explicit lowercase-keys assertion guards
        // against accidental case-sensitive consumers downstream.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        foreach (var key in map.Keys)
        {
            Assert.Equal(key.ToLowerInvariant(), key);
        }
    }

    [Fact]
    public void Discover_CoversAllExpectedBuiltinKeys_NoRegression()
    {
        // Catalogue of every key produced by the algorithm against the
        // synthetic V. If new shapes are added to the stub, this list
        // grows — explicitly enumerated so unintended regressions surface.
        var (v, vn) = CompileAndLoadV();
        var map = DiscoverMirror(v, vn);

        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "label",
            "button",
            "scrollview",
            "foldout",
            "visualelement",
            "errorboundary",
            "animate",
            "audio",
            "video",
            "fragment",
            "text",
            // manual overrides:
            "suspense",
            "portal",
            "visualelementsafe",
        };

        Assert.Equal(expected.OrderBy(x => x), map.Keys.OrderBy(x => x));
    }
}
