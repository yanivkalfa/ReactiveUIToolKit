using Xunit;

namespace UitkxLanguageServer.Tests;

/// <summary>
/// Tests for the <see cref="UnityVersion"/> struct: parsing, comparison,
/// display, and edge cases.
/// </summary>
public sealed class UnityVersionTests
{
    // ── Parsing ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData("6000.3", 6000, 3)]
    [InlineData("6000.3.0f1", 6000, 3)]
    [InlineData("6000.3.12f1", 6000, 3)]
    [InlineData("2022.3", 2022, 3)]
    [InlineData("2022.3.12f1", 2022, 3)]
    [InlineData("6000.0", 6000, 0)]
    public void TryParse_ValidVersion_ReturnsTrue(string input, int expectedMajor, int expectedMinor)
    {
        Assert.True(UnityVersion.TryParse(input, out var v));
        Assert.Equal(expectedMajor, v.Major);
        Assert.Equal(expectedMinor, v.Minor);
        Assert.True(v.IsKnown);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("abc")]
    [InlineData("not.a.version")]
    public void TryParse_InvalidVersion_ReturnsFalse(string? input)
    {
        Assert.False(UnityVersion.TryParse(input, out var v));
        Assert.False(v.IsKnown);
        Assert.Equal(UnityVersion.Unknown, v);
    }

    // ── Comparison ────────────────────────────────────────────────────────

    [Fact]
    public void Comparison_HigherMinor_IsGreater()
    {
        var v62 = new UnityVersion(6000, 2);
        var v63 = new UnityVersion(6000, 3);

        Assert.True(v63 > v62);
        Assert.True(v62 < v63);
        Assert.True(v63 >= v62);
        Assert.True(v62 <= v63);
        Assert.False(v62 == v63);
        Assert.True(v62 != v63);
    }

    [Fact]
    public void Comparison_SameVersion_IsEqual()
    {
        var a = new UnityVersion(6000, 3);
        var b = new UnityVersion(6000, 3);

        Assert.True(a == b);
        Assert.True(a >= b);
        Assert.True(a <= b);
        Assert.False(a > b);
        Assert.False(a < b);
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void Comparison_DifferentMajor()
    {
        var v2022 = new UnityVersion(2022, 3);
        var v6000 = new UnityVersion(6000, 0);

        Assert.True(v6000 > v2022);
        Assert.True(v2022 < v6000);
    }

    [Fact]
    public void Unknown_IsNotKnown()
    {
        Assert.False(UnityVersion.Unknown.IsKnown);
        Assert.Equal(0, UnityVersion.Unknown.Major);
        Assert.Equal(0, UnityVersion.Unknown.Minor);
    }

    // ── Display ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(6000, 0, "Unity 6.0")]
    [InlineData(6000, 2, "Unity 6.2")]
    [InlineData(6000, 3, "Unity 6.3")]
    [InlineData(2022, 3, "Unity 2022.3")]
    public void ToDisplayString_FormatsCorrectly(int major, int minor, string expected)
    {
        var v = new UnityVersion(major, minor);
        Assert.Equal(expected, v.ToDisplayString());
    }

    [Fact]
    public void Unknown_ToDisplayString()
    {
        Assert.Equal("Unity (unknown)", UnityVersion.Unknown.ToDisplayString());
    }

    [Theory]
    [InlineData(6000, 3, "6000.3")]
    [InlineData(6000, 0, "6000.0")]
    public void ToString_ReturnsSchemaNotation(int major, int minor, string expected)
    {
        var v = new UnityVersion(major, minor);
        Assert.Equal(expected, v.ToString());
    }

    [Fact]
    public void Unknown_ToString()
    {
        Assert.Equal("unknown", UnityVersion.Unknown.ToString());
    }

    // ── Equality & HashCode ──────────────────────────────────────────────

    [Fact]
    public void Equals_WorksCorrectly()
    {
        var a = new UnityVersion(6000, 3);
        var b = new UnityVersion(6000, 3);
        var c = new UnityVersion(6000, 2);

        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
        Assert.False(a.Equals(c));
        Assert.False(a.Equals("not a version"));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}

/// <summary>
/// Tests for the schema version-awareness helpers: element/style version
/// queries and availability checks.
/// </summary>
public sealed class SchemaVersionTests
{
    private static UitkxSchema CreateSchema() => new UitkxSchema();

    [Fact]
    public void GetElementMinVersion_NoAnnotation_ReturnsUnknown()
    {
        var schema = CreateSchema();
        // Box has no sinceUnity annotation
        var v = schema.GetElementMinVersion("Box");
        Assert.False(v.IsKnown);
    }

    [Fact]
    public void IsElementAvailable_NoAnnotation_ReturnsTrue()
    {
        var schema = CreateSchema();
        var userVersion = new UnityVersion(6000, 2);
        Assert.True(schema.IsElementAvailable("Box", userVersion));
    }

    [Fact]
    public void IsElementAvailable_UnknownUserVersion_ReturnsTrue()
    {
        var schema = CreateSchema();
        // Even if an element had a version annotation, unknown user version → true (can't filter)
        Assert.True(schema.IsElementAvailable("Box", UnityVersion.Unknown));
    }

    [Fact]
    public void GetStyleMinVersion_NoAnnotation_ReturnsUnknown()
    {
        var schema = CreateSchema();
        var v = schema.GetStyleMinVersion("flexDirection");
        Assert.False(v.IsKnown);
    }

    [Fact]
    public void IsStyleAvailable_NoAnnotation_ReturnsTrue()
    {
        var schema = CreateSchema();
        var userVersion = new UnityVersion(6000, 2);
        Assert.True(schema.IsStyleAvailable("flexDirection", userVersion));
    }

    [Fact]
    public void StyleVersions_EmptyByDefault()
    {
        var schema = CreateSchema();
        Assert.Empty(schema.Root.StyleVersions);
    }

    [Fact]
    public void GetStyleVersionInfo_NoEntry_ReturnsNull()
    {
        var schema = CreateSchema();
        Assert.Null(schema.GetStyleVersionInfo("nonexistent"));
    }
}

/// <summary>
/// Tests for <see cref="Roslyn.ReferenceAssemblyLocator.DetectUnityVersion"/>.
/// </summary>
public sealed class VersionDetectionTests
{
    [Fact]
    public void DetectUnityVersion_NullRoot_ReturnsUnknown()
    {
        var version = Roslyn.ReferenceAssemblyLocator.DetectUnityVersion(null);
        Assert.Equal(UnityVersion.Unknown, version);
    }

    [Fact]
    public void DetectUnityVersion_EmptyRoot_ReturnsUnknown()
    {
        var version = Roslyn.ReferenceAssemblyLocator.DetectUnityVersion("");
        Assert.Equal(UnityVersion.Unknown, version);
    }

    [Fact]
    public void DetectUnityVersion_NonexistentPath_ReturnsUnknown()
    {
        var version = Roslyn.ReferenceAssemblyLocator.DetectUnityVersion(
            "C:/does/not/exist/anywhere");
        Assert.Equal(UnityVersion.Unknown, version);
    }
}
