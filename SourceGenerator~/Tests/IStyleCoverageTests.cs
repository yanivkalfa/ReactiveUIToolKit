using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Coverage parity tests that lock down the wiring between
/// <c>UnityEngine.UIElements.IStyle</c> and the UITKX style pipeline.
///
/// <para>
/// These tests parse the source files directly (no Unity runtime needed) and
/// assert that every <c>IStyle</c> property is wired into:
///   • <see cref="ReactiveUITK.Props.PropsApplier"/> <c>styleSetters</c>,
///   • <see cref="ReactiveUITK.Props.PropsApplier"/> <c>styleResetters</c>,
///   • <see cref="ReactiveUITK.Props.Typed.StyleKeys"/> string constants,
///   • <see cref="ReactiveUITK.Props.Typed.Style"/> <c>SetByKey</c>,
///   • <see cref="ReactiveUITK.Props.Typed.Style"/> <c>BIT_*</c> constants
///     (via <c>KeyToBit</c> mapping).
/// </para>
///
/// <para>
/// The authoritative IStyle property list is hard-coded as a snapshot. When
/// Unity adds a new property, the dev who bumps the floor MUST update this
/// snapshot AND the wiring at the same commit — the test fails on mismatch
/// in either direction.
/// </para>
/// </summary>
public class IStyleCoverageTests
{
    /// <summary>
    /// Authoritative snapshot of <c>UnityEngine.UIElements.IStyle</c>'s public
    /// properties as of Unity <c>6000.2</c>. 6.3-only deltas are handled in
    /// <see cref="IStyleProperties_6_3_Only"/>.
    ///
    /// <para>
    /// To regenerate: see <c>Plans~/STYLE_COVERAGE_COMPLETION_PLAN.md</c>
    /// §verification probe.
    /// </para>
    /// </summary>
    public static readonly IReadOnlyCollection<string> IStyleProperties_6_2 = new[]
    {
        "alignContent", "alignItems", "alignSelf",
        "backgroundColor", "backgroundImage",
        "backgroundPositionX", "backgroundPositionY", "backgroundRepeat", "backgroundSize",
        "borderBottomColor", "borderBottomLeftRadius", "borderBottomRightRadius", "borderBottomWidth",
        "borderLeftColor", "borderLeftWidth",
        "borderRightColor", "borderRightWidth",
        "borderTopColor", "borderTopLeftRadius", "borderTopRightRadius", "borderTopWidth",
        "bottom", "color", "cursor", "display",
        "flexBasis", "flexDirection", "flexGrow", "flexShrink", "flexWrap",
        "fontSize", "height", "justifyContent",
        "left", "letterSpacing",
        "marginBottom", "marginLeft", "marginRight", "marginTop",
        "maxHeight", "maxWidth", "minHeight", "minWidth", "opacity", "overflow",
        "paddingBottom", "paddingLeft", "paddingRight", "paddingTop",
        "position", "right", "rotate", "scale",
        "textOverflow", "textShadow", "top",
        "transformOrigin",
        "transitionDelay", "transitionDuration", "transitionProperty", "transitionTimingFunction",
        "translate",
        "unityBackgroundImageTintColor", "unityBackgroundScaleMode",
        "unityEditorTextRenderingMode",
        "unityFont", "unityFontDefinition", "unityFontStyleAndWeight",
        "unityOverflowClipBox", "unityParagraphSpacing",
        "unitySliceBottom", "unitySliceLeft", "unitySliceRight",
        "unitySliceScale", "unitySliceTop", "unitySliceType",
        "unityTextAlign", "unityTextAutoSize", "unityTextGenerator",
        "unityTextOutlineColor", "unityTextOutlineWidth", "unityTextOverflowPosition",
        "visibility", "whiteSpace", "width", "wordSpacing",
    };

    /// <summary>Properties added in Unity 6.3 (gated by <c>UNITY_6000_3_OR_NEWER</c>).</summary>
    public static readonly IReadOnlyCollection<string> IStyleProperties_6_3_Only = new[]
    {
        "aspectRatio", "filter", "unityMaterial",
    };

    /// <summary>
    /// IStyle properties intentionally NOT wired by UITKX. Each entry must
    /// have a documented justification.
    /// </summary>
    private static readonly HashSet<string> s_intentionalOmissions = new()
    {
        // StyleCursor wraps Cursor (Texture2D + hotspot). No built-in cursor
        // constants in Unity; needs UX design before exposing a typed setter.
        // PropsApplier has an explicit no-op stub at "cursor".
        "cursor",
        // unityBackgroundScaleMode is the legacy ScaleMode-based path; Unity
        // splits it into backgroundPositionX/Y/Size/Repeat which we DO wire.
        "unityBackgroundScaleMode",
        // unityFont is wired under the alias "fontFamily" (StyleKeys.FontFamily).
        // Direct "unityFont" key is not exposed.
        "unityFont",
        // unityFontStyleAndWeight is wired under the alias "unityFontStyle".
        "unityFontStyleAndWeight",
        // unityTextAlign is wired under the alias "textAlign".
        "unityTextAlign",
        // unityBackgroundImageTintColor is wired under "backgroundImageTint".
        "unityBackgroundImageTintColor",
    };

    /// <summary>
    /// Properties that exist as setters/resetters under a UITKX-friendly alias
    /// instead of the literal IStyle name. Tests treat these as covered.
    /// </summary>
    private static readonly Dictionary<string, string> s_aliases = new()
    {
        ["unityFont"] = "fontFamily",
        ["unityFontStyleAndWeight"] = "unityFontStyle",
        ["unityTextAlign"] = "textAlign",
        ["unityBackgroundImageTintColor"] = "backgroundImageTint",
    };

    private static IEnumerable<string> AllIStyleProperties =>
        IStyleProperties_6_2.Concat(IStyleProperties_6_3_Only);

    // ─── Source paths ──────────────────────────────────────────────────────

    private static readonly string s_repoRoot = ResolveRepoRoot();

    private static string ResolveRepoRoot()
    {
        // Walk up from the test assembly directory until we find
        // package.json + Shared/Props/PropsApplier.cs (the package root).
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "package.json"))
                && File.Exists(Path.Combine(dir.FullName, "Shared", "Props", "PropsApplier.cs")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            "Could not locate repo root containing Shared/Props/PropsApplier.cs.");
    }

    private static string ReadSource(params string[] relParts)
    {
        var path = Path.Combine(new[] { s_repoRoot }.Concat(relParts).ToArray());
        return File.ReadAllText(path);
    }

    // ─── Extractors ────────────────────────────────────────────────────────

    private static HashSet<string> ExtractStyleSetterKeys()
    {
        var src = ReadSource("Shared", "Props", "PropsApplier.cs");
        var rx = new Regex(@"styleSetters\[""([^""]+)""\]\s*=", RegexOptions.Compiled);
        return rx.Matches(src).Select(m => m.Groups[1].Value).ToHashSet();
    }

    private static HashSet<string> ExtractStyleResetterKeys()
    {
        var src = ReadSource("Shared", "Props", "PropsApplier.cs");
        var rx = new Regex(@"styleResetters\[""([^""]+)""\]\s*=", RegexOptions.Compiled);
        return rx.Matches(src).Select(m => m.Groups[1].Value).ToHashSet();
    }

    private static HashSet<string> ExtractStyleKeysConstants()
    {
        var src = ReadSource("Shared", "Props", "Typed", "StyleKeys.cs");
        var rx = new Regex(
            @"public\s+const\s+string\s+\w+\s*=\s*""([^""]+)""\s*;",
            RegexOptions.Compiled);
        return rx.Matches(src).Select(m => m.Groups[1].Value).ToHashSet();
    }

    /// <summary>Keys that appear as case labels inside <c>Style.SetByKey</c>.</summary>
    private static HashSet<string> ExtractStyleSetByKeyCases()
    {
        var src = ReadSource("Shared", "Props", "Typed", "Style.cs");
        // SetByKey body is the first internal SetByKey method.
        var bodyMatch = Regex.Match(
            src,
            @"internal\s+void\s+SetByKey\s*\(string\s+key,\s*object\s+value\)\s*\{(?<body>.*?)^\s{8}\}",
            RegexOptions.Singleline | RegexOptions.Multiline);
        Assert.True(bodyMatch.Success, "Could not locate Style.SetByKey body.");
        var body = bodyMatch.Groups["body"].Value;
        var rx = new Regex(@"case\s+""([^""]+)""\s*:", RegexOptions.Compiled);
        return rx.Matches(body).Select(m => m.Groups[1].Value).ToHashSet();
    }

    /// <summary>Keys that appear as case labels inside <c>Style.KeyToBit</c>.</summary>
    private static HashSet<string> ExtractKeyToBitCases()
    {
        var src = ReadSource("Shared", "Props", "Typed", "Style.cs");
        var bodyMatch = Regex.Match(
            src,
            @"internal\s+static\s+int\s+KeyToBit\s*\(string\s+key\)\s*\{(?<body>.*?)^\s{8}\}",
            RegexOptions.Singleline | RegexOptions.Multiline);
        Assert.True(bodyMatch.Success, "Could not locate Style.KeyToBit body.");
        var body = bodyMatch.Groups["body"].Value;
        var rx = new Regex(@"case\s+""([^""]+)""\s*:", RegexOptions.Compiled);
        return rx.Matches(body).Select(m => m.Groups[1].Value).ToHashSet();
    }

    private static string ResolveAlias(string istyleName) =>
        s_aliases.TryGetValue(istyleName, out var alias) ? alias : istyleName;

    // ─── Tests ────────────────────────────────────────────────────────────

    [Fact]
    public void EveryIStylePropertyHasAStyleSetter()
    {
        var setters = ExtractStyleSetterKeys();
        var missing = AllIStyleProperties
            .Where(p => !s_intentionalOmissions.Contains(p))
            .Where(p => !setters.Contains(ResolveAlias(p)))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"PropsApplier.styleSetters is missing entries for: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryIStylePropertyHasAStyleResetter()
    {
        var resetters = ExtractStyleResetterKeys();
        var missing = AllIStyleProperties
            .Where(p => !s_intentionalOmissions.Contains(p))
            .Where(p => !resetters.Contains(ResolveAlias(p)))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"PropsApplier.styleResetters is missing entries for: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryIStylePropertyHasAStyleKeysConstant()
    {
        var keys = ExtractStyleKeysConstants();
        var missing = AllIStyleProperties
            .Where(p => !s_intentionalOmissions.Contains(p))
            .Where(p => !keys.Contains(ResolveAlias(p)))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"StyleKeys is missing constants for: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryStyleKeysConstantHasAStyleSetter()
    {
        // Catches typos: a StyleKeys constant whose value isn't recognised
        // by styleSetters silently no-ops at runtime.
        var setters = ExtractStyleSetterKeys();
        var keys = ExtractStyleKeysConstants();

        // Permitted shorthands not present in IStyle but accepted by setters.
        var shorthands = new HashSet<string>
        {
            "margin", "padding", "borderWidth", "borderColor", "borderRadius",
        };

        var orphans = keys
            .Where(k => !setters.Contains(k))
            .Where(k => !shorthands.Contains(k))
            .OrderBy(k => k)
            .ToList();

        Assert.True(
            orphans.Count == 0,
            $"StyleKeys constants with no matching styleSetters entry: {string.Join(", ", orphans)}");
    }

    [Fact]
    public void EveryIStylePropertyHasASetByKeyCase()
    {
        var cases = ExtractStyleSetByKeyCases();
        var missing = AllIStyleProperties
            .Where(p => !s_intentionalOmissions.Contains(p))
            .Where(p => !cases.Contains(ResolveAlias(p)))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"Style.SetByKey switch is missing cases for: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EveryIStylePropertyHasABitMapping()
    {
        var cases = ExtractKeyToBitCases();
        var missing = AllIStyleProperties
            .Where(p => !s_intentionalOmissions.Contains(p))
            .Where(p => !cases.Contains(ResolveAlias(p)))
            .OrderBy(p => p)
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"Style.KeyToBit switch is missing cases for: {string.Join(", ", missing)}");
    }

    [Fact]
    public void StyleSettersAndResettersAgreeOnKeySet()
    {
        // An asymmetric dictionary is the original tech-debt root cause.
        // Setters and resetters MUST cover the same keys (after subtracting
        // explicitly intentional asymmetries: shorthand setters that decompose
        // to per-side resetters, and string-only legacy "transition" /
        // "backgroundPosition" / "cursor" no-op stubs).
        var setters = ExtractStyleSetterKeys();
        var resetters = ExtractStyleResetterKeys();

        // Setters that intentionally have no resetter (no-op or shorthand
        // that decomposes into per-side resets handled elsewhere).
        var setterOnlyAllow = new HashSet<string>
        {
            "margin", "padding", "borderWidth", "borderColor", "borderRadius",
            "flex",                  // shorthand
            "transition",            // CSS shorthand stub
            "cursor",                // intentional no-op stub
            "backgroundPosition",    // legacy no-op
            // These shorthand expanders are stylesetters but reset is per-side.
        };
        var settersMissingResetter = setters
            .Where(s => !resetters.Contains(s))
            .Where(s => !setterOnlyAllow.Contains(s))
            .OrderBy(s => s)
            .ToList();

        Assert.True(
            settersMissingResetter.Count == 0,
            $"Setters with no matching resetter: {string.Join(", ", settersMissingResetter)}");

        // Resetters that intentionally have no setter (none expected).
        var resetterOnlyAllow = new HashSet<string>();
        var resettersMissingSetter = resetters
            .Where(r => !setters.Contains(r))
            .Where(r => !resetterOnlyAllow.Contains(r))
            .OrderBy(r => r)
            .ToList();

        Assert.True(
            resettersMissingSetter.Count == 0,
            $"Resetters with no matching setter: {string.Join(", ", resettersMissingSetter)}");
    }
}
