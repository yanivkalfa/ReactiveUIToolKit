using ReactiveUITK.SourceGenerator.Tests.Helpers;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests;

/// <summary>
/// Integration tests that verify each diagnostic ID fires for the expected
/// input pattern.  Every test runs the full <see cref="UitkxGenerator"/>
/// pipeline so the diagnostics are produced exactly as Unity would see them.
/// </summary>
public class DiagnosticTests
{
    /// Wraps markup in a minimal function-style component for test convenience.
    private static string Wrap(string markup) =>
        "component MyComp {\n  return (\n" + markup + "\n  );\n}";

    /// Wraps code + markup in a function-style component (code runs before return).
    private static string WrapWithCode(string code, string markup) =>
        "component MyComp {\n  " + code + "\n  return (\n" + markup + "\n  );\n}";

    // ── Rules of Hooks violations ─────────────────────────────────────────────

    [Fact]
    public void UITKX0013_HookInConditional()
    {
        var src = Wrap("""
            @if (true) {
                return (
                    @(Hooks.UseState(0))
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0013"),
            "Expected UITKX0013 for hook call inside @if branch"
        );
    }

    [Fact]
    public void UITKX0014_HookInLoop()
    {
        var src = Wrap("""
            @foreach (var i in items) {
                return (
                    @(Hooks.UseState(i))
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0014"),
            "Expected UITKX0014 for hook call inside @foreach"
        );
    }

    [Fact]
    public void UITKX0015_HookInSwitch()
    {
        var src = Wrap("""
            @switch (mode) {
                @case 0: return (@(Hooks.UseState(42)));
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0015"),
            "Expected UITKX0015 for hook call inside @switch case"
        );
    }

    [Fact]
    public void UITKX0016_HookInAttribute()
    {
        // Hook call inside an event-handler attribute expression
        var src = Wrap("""<button onClick={() => Hooks.UseState(0)}/>""");
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0016"),
            "Expected UITKX0016 for hook inside attribute expression"
        );
    }

    // ── Hooks in SetupCode (control-block body preamble) ─────────────────────

    [Fact]
    public void UITKX0013_HookInIfSetupCode()
    {
        var src = Wrap("""
            @if (true) {
                var s = Hooks.UseState(0);
                return (
                    <box />
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0013"),
            "Expected UITKX0013 for hook in @if branch SetupCode"
        );
    }

    [Fact]
    public void UITKX0014_HookInForeachSetupCode()
    {
        var src = Wrap("""
            @foreach (var item in items) {
                var s = Hooks.UseState(0);
                return (
                    <label key={item} text={item}/>
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0014"),
            "Expected UITKX0014 for hook in @foreach SetupCode"
        );
    }

    [Fact]
    public void UITKX0014_HookInForSetupCode()
    {
        var src = Wrap("""
            @for (var i = 0; i < 10; i++) {
                var s = Hooks.UseState(0);
                return (
                    <box key={i.ToString()}/>
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0014"),
            "Expected UITKX0014 for hook in @for SetupCode"
        );
    }

    [Fact]
    public void UITKX0014_HookInWhileSetupCode()
    {
        var src = Wrap("""
            @while (true) {
                var s = Hooks.UseState(0);
                return (
                    <box />
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0014"),
            "Expected UITKX0014 for hook in @while SetupCode"
        );
    }

    [Fact]
    public void UITKX0015_HookInSwitchCaseSetupCode()
    {
        var src = Wrap("""
            @switch (mode) {
                @case 0:
                    var s = Hooks.UseState(42);
                    return (
                        <box />
                    );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0015"),
            "Expected UITKX0015 for hook in @switch case SetupCode"
        );
    }

    // ── Structural violations (UITKX0017–0019) ───────────────────────────────

    [Fact]
    public void UITKX0017_MultipleRootElements()
    {
        var src = Wrap("<box/>\n<label text=\"oops\"/>");
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0017"),
            "Expected UITKX0017 when component has more than one root element"
        );
    }

    [Fact]
    public void UITKX0018_UseEffectMissingDeps()
    {
        // UseEffect with only a callback argument — no dependency array
        var src = WrapWithCode("Hooks.UseEffect(() => { });", "<box/>");
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0018"),
            "Expected UITKX0018 when UseEffect has no dependency array"
        );
    }

    [Fact]
    public void UITKX0018_NotFiredWhenDepsProvided()
    {
        // UseEffect with two arguments — should NOT fire
        var src = WrapWithCode("Hooks.UseEffect(() => { }, new object[] { count });", "<box/>");
        var result = GeneratorTestHelper.Run(src);
        Assert.False(
            result.HasDiagnostic("UITKX0018"),
            "UITKX0018 should not fire when dependency array is provided"
        );
    }

    [Fact]
    public void UITKX0018_UseEffectMissingDepsInIfSetupCode()
    {
        var src = Wrap("""
            @if (true) {
                Hooks.UseEffect(() => { });
                return (
                    <box />
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0018"),
            "Expected UITKX0018 for UseEffect missing deps in @if SetupCode"
        );
    }

    [Fact]
    public void UITKX0018_UseEffectMissingDepsInForeachSetupCode()
    {
        var src = Wrap("""
            @foreach (var item in items) {
                Hooks.UseEffect(() => { });
                return (
                    <label key={item} text={item}/>
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0018"),
            "Expected UITKX0018 for UseEffect missing deps in @foreach SetupCode"
        );
    }

    [Fact]
    public void UITKX0018_UseEffectWithDepsInSetupCode_NotFired()
    {
        var src = Wrap("""
            @if (true) {
                Hooks.UseEffect(() => { }, new object[] { });
                return (
                    <box />
                );
            }
            <box/>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.False(
            result.HasDiagnostic("UITKX0018"),
            "UITKX0018 should not fire when deps are provided in SetupCode"
        );
    }

    [Fact]
    public void UITKX0019_IndexAsKey()
    {
        // Loop iterator variable used directly as key
        var src = Wrap("""
            @foreach (var i in items) {
                return (
                    <label key={i} text={i.ToString()}/>
                );
            }
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0019"),
            "Expected UITKX0019 when loop variable is used directly as key"
        );
    }

    [Fact]
    public void UITKX0019_NotFiredForStableKey()
    {
        // key uses a property of the loop item — should NOT fire
        var src = Wrap("""
            @foreach (var item in items) {
                return (
                    <label key={item.Id} text={item.Name}/>
                );
            }
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.False(
            result.HasDiagnostic("UITKX0019"),
            "UITKX0019 should not fire when key uses a stable property"
        );
    }

    // ── Key/reconciler warnings (UITKX0009, UITKX0010) ──────────────────────

    [Fact]
    public void UITKX0009_ForeachMissingKey()
    {
        // Element inside @foreach has no key attribute
        var src = Wrap("""
            @foreach (var item in items) {
                return (
                    <label text={item}/>
                );
            }
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0009"),
            "Expected UITKX0009 when @foreach element lacks a key attribute"
        );
    }

    [Fact]
    public void UITKX0010_DuplicateSiblingKey()
    {
        var src = Wrap("""
            <box>
                <label key="same" text="A"/>
                <label key="same" text="B"/>
            </box>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.True(
            result.HasDiagnostic("UITKX0010"),
            "Expected UITKX0010 when two siblings share the same string key"
        );
    }

    [Fact]
    public void UITKX0010_NotFiredForUniqueKeys()
    {
        var src = Wrap("""
            <box>
                <label key="a" text="A"/>
                <label key="b" text="B"/>
            </box>
            """);
        var result = GeneratorTestHelper.Run(src);
        Assert.False(
            result.HasDiagnostic("UITKX0010"),
            "UITKX0010 should not fire when sibling keys are unique"
        );
    }

    // ── Clean file: no false positives ───────────────────────────────────────

    [Fact]
    public void CleanFile_ProducesNoDiagnostics()
    {
        var src = WrapWithCode(
            "Hooks.UseEffect(() => { }, new object[] { });",
            """
            <box>
                <label text="Hello"/>
                <button text="Click" onClick={handler}/>
            </box>
            """);
        var result = GeneratorTestHelper.Run(src);
        // No semantic-error diagnostics — only possibly UITKX0001/0008 warnings for
        // unknown tags (no V.* type in our stub), which is acceptable.
        Assert.DoesNotContain(
            result.Diagnostics,
            d =>
                d.Id
                    is "UITKX0013"
                        or "UITKX0014"
                        or "UITKX0015"
                        or "UITKX0016"
                        or "UITKX0017"
                        or "UITKX0018"
                        or "UITKX0019"
                        or "UITKX0009"
                        or "UITKX0010"
        );
    }

    // ── UITKX0306: @(expr) in setup code ─────────────────────────────────────

    [Fact]
    public void UITKX0306_AtExprInRawSetupCode_Fires()
    {
        var src = """
            @namespace Test.NS
            component C {
              var x = @(123);
              return (
                <box />
              );
            }
            """;
        var result = GeneratorTestHelper.Run(src);
        Assert.True(result.HasDiagnostic("UITKX0306"),
            "Expected UITKX0306 for @(expr) in raw C# setup code");
    }

    [Fact]
    public void UITKX0306_AtExprInsideJsxInSetupCode_DoesNotFire()
    {
        var src = """
            @namespace Test.NS
            component C {
              var childNode = 42;
              var el = (
                <box>@(childNode)</box>
              );
              return (
                <box />
              );
            }
            """;
        var result = GeneratorTestHelper.Run(src);
        Assert.False(result.HasDiagnostic("UITKX0306"),
            "Should NOT fire UITKX0306 for @(expr) inside embedded JSX in setup code");
    }

    [Fact]
    public void UITKX0306_AtExprInsideBareJsxReturn_DoesNotFire()
    {
        var src = """
            @namespace Test.NS
            component C {
              var childNode = 42;
              var el = () => <box>@(childNode)</box>;
              return (
                <box />
              );
            }
            """;
        var result = GeneratorTestHelper.Run(src);
        Assert.False(result.HasDiagnostic("UITKX0306"),
            "Should NOT fire UITKX0306 for @(expr) inside bare arrow JSX in setup code");
    }
}
