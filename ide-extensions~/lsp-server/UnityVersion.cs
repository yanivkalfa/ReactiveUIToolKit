using System;
using System.Text.RegularExpressions;

namespace UitkxLanguageServer;

/// <summary>
/// Represents a Unity Editor version as a comparable (major, minor) pair.
/// <para>
/// Parsing accepts any of these formats:
/// <list type="bullet">
///   <item><c>6000.3</c> (schema/annotation style)</item>
///   <item><c>6000.3.0f1</c> (ProjectVersion.txt format)</item>
///   <item><c>6000.3.12f1</c> (ProjectVersion.txt format with patch)</item>
/// </list>
/// Only the major and minor components are significant for API-level
/// compatibility checks (patch releases don't add/remove IStyle properties).
/// </para>
/// </summary>
public readonly struct UnityVersion : IComparable<UnityVersion>, IEquatable<UnityVersion>
{
    /// <summary>Major version number (e.g. 6000 for Unity 6.x).</summary>
    public int Major { get; }

    /// <summary>Minor version number (e.g. 3 for Unity 6.3).</summary>
    public int Minor { get; }

    /// <summary>A sentinel representing an unknown or undetected version.</summary>
    public static readonly UnityVersion Unknown = default;

    /// <summary><c>true</c> when this instance was successfully parsed (Major > 0).</summary>
    public bool IsKnown => Major > 0;

    public UnityVersion(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }

    // ── Parsing ───────────────────────────────────────────────────────────

    // Matches:  6000.3   |  6000.3.0f1  |  2022.3.12f1  |  6000.3.0
    private static readonly Regex s_versionPattern = new Regex(
        @"^(\d+)\.(\d+)",
        RegexOptions.Compiled);

    /// <summary>
    /// Attempts to parse a version string into a <see cref="UnityVersion"/>.
    /// Returns <c>true</c> on success.
    /// </summary>
    public static bool TryParse(string? input, out UnityVersion version)
    {
        version = Unknown;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var match = s_versionPattern.Match(input.Trim());
        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups[1].Value, out int major)
            || !int.TryParse(match.Groups[2].Value, out int minor))
            return false;

        if (major <= 0)
            return false;

        version = new UnityVersion(major, minor);
        return true;
    }

    // ── Comparison ────────────────────────────────────────────────────────

    public int CompareTo(UnityVersion other)
    {
        int cmp = Major.CompareTo(other.Major);
        return cmp != 0 ? cmp : Minor.CompareTo(other.Minor);
    }

    public bool Equals(UnityVersion other) => Major == other.Major && Minor == other.Minor;
    public override bool Equals(object? obj) => obj is UnityVersion v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(Major, Minor);

    public static bool operator ==(UnityVersion a, UnityVersion b) => a.Equals(b);
    public static bool operator !=(UnityVersion a, UnityVersion b) => !a.Equals(b);
    public static bool operator <(UnityVersion a, UnityVersion b) => a.CompareTo(b) < 0;
    public static bool operator >(UnityVersion a, UnityVersion b) => a.CompareTo(b) > 0;
    public static bool operator <=(UnityVersion a, UnityVersion b) => a.CompareTo(b) <= 0;
    public static bool operator >=(UnityVersion a, UnityVersion b) => a.CompareTo(b) >= 0;

    // ── Display ───────────────────────────────────────────────────────────

    /// <summary>Returns the version in schema notation, e.g. <c>"6000.3"</c>.</summary>
    public override string ToString() => IsKnown ? $"{Major}.{Minor}" : "unknown";

    /// <summary>Returns a human-readable display string, e.g. <c>"Unity 6.3"</c>.</summary>
    public string ToDisplayString()
    {
        if (!IsKnown)
            return "Unity (unknown)";

        // Unity 6000.x → "Unity 6.x"
        if (Major >= 6000)
            return $"Unity {Major - 6000 + 6}.{Minor}";

        // Legacy: 2022.3.12f1 → "Unity 2022.3"
        return $"Unity {Major}.{Minor}";
    }
}
