using System.Collections.Immutable;

namespace ReactiveUITK.Language.Roslyn
{
    // ── Region kind ───────────────────────────────────────────────────────────

    /// <summary>
    /// Identifies the syntactic origin of a C# region inside a .uitkx file.
    /// Used both for diagnostic filtering (deciding which Roslyn errors are
    /// meaningful) and for selecting the right IDE feature (completions in
    /// code-block vs. expression context behave differently).
    /// </summary>
    public enum SourceRegionKind
    {
        /// <summary>An inline <c>@(expr)</c> expression in markup.</summary>
        InlineExpression,

        /// <summary>A <c>attr={expr}</c> C# attribute-value expression.</summary>
        AttributeExpression,

        /// <summary>The verbatim code body inside an <c>@code { ... }</c> block.</summary>
        CodeBlock,

        /// <summary>
        /// The setup-code section of a function-style <c>component Name { ... return(...) }</c>.
        /// Covers all statements before the <c>return</c>.
        /// </summary>
        FunctionSetup,

        /// <summary>
        /// The body of a <c>hook</c> declaration in a .uitkx file.
        /// Pure C# code — no markup.
        /// </summary>
        HookBody,

        /// <summary>
        /// The body of a <c>module</c> declaration in a .uitkx file.
        /// Pure C# code — no markup.
        /// </summary>
        ModuleBody,
    }

    // ── Source map entry ──────────────────────────────────────────────────────

    /// <summary>
    /// Represents a 1-to-1 character mapping between a contiguous span in the
    /// Roslyn virtual C# document and the corresponding span in the .uitkx source.
    ///
    /// The two spans always have equal length —
    /// <c>VirtualEnd - VirtualStart == UitkxEnd - UitkxStart</c> —
    /// because the C# text is copied verbatim (no transformation).
    /// Both spans are 0-based, exclusive-end byte offsets measured in UTF-16
    /// code units (i.e. standard C# <c>string</c> index arithmetic).
    /// </summary>
    public sealed record SourceMapEntry(
        /// <summary>Inclusive start offset inside the virtual C# document.</summary>
        int VirtualStart,
        /// <summary>Exclusive end offset inside the virtual C# document.</summary>
        int VirtualEnd,
        /// <summary>Inclusive start offset inside the .uitkx source file.</summary>
        int UitkxStart,
        /// <summary>Exclusive end offset inside the .uitkx source file.</summary>
        int UitkxEnd,
        /// <summary>The syntactic origin of this region.</summary>
        SourceRegionKind Kind,
        /// <summary>
        /// 1-based line number in the .uitkx file where this region starts.
        /// Matches the value written into the <c>#line</c> directive.
        /// </summary>
        int UitkxLine
    );

    // ── Source map ────────────────────────────────────────────────────────────

    /// <summary>
    /// An immutable, bidirectional position map between the Roslyn virtual C#
    /// document and the original .uitkx source.
    ///
    /// Lookup is O(n) in the number of mapped regions per translation call.
    /// For typical .uitkx files (≤ 50 C# regions) this is negligible.  If
    /// profiling ever flags this, replace the linear scan with a sorted array
    /// and binary search.
    /// </summary>
    public sealed class SourceMap
    {
        /// <summary>A SourceMap with no entries — returned when a file has no C# regions.</summary>
        public static readonly SourceMap Empty =
            new SourceMap(ImmutableArray<SourceMapEntry>.Empty);

        private readonly ImmutableArray<SourceMapEntry> _entries;

        public SourceMap(ImmutableArray<SourceMapEntry> entries) => _entries = entries;

        /// <summary>All mapped regions, in the order they were recorded.</summary>
        public ImmutableArray<SourceMapEntry> Entries => _entries;

        // ── Virtual → uitkx ─────────────────────────────────────────────────

        /// <summary>
        /// Translates a 0-based character offset in the virtual C# document to the
        /// corresponding offset in the .uitkx source file.
        ///
        /// Returns <c>null</c> when <paramref name="virtualOffset"/> lies inside
        /// generated scaffold (class header, <c>#pragma</c>, etc.) rather than in
        /// a region that traces back to user-authored .uitkx C# code.
        /// </summary>
        public (int UitkxOffset, SourceMapEntry Entry)? ToUitkxOffset(int virtualOffset)
        {
            foreach (var e in _entries)
            {
                if (virtualOffset >= e.VirtualStart && virtualOffset < e.VirtualEnd)
                    return (e.UitkxStart + (virtualOffset - e.VirtualStart), e);
            }
            return null;
        }

        /// <summary>
        /// Translates a 0-based character offset in the .uitkx source to the
        /// corresponding offset in the virtual C# document.
        ///
        /// Returns <c>null</c> when <paramref name="uitkxOffset"/> does not fall
        /// inside any tracked C# region (e.g. it points at markup text or
        /// a directive line).
        /// </summary>
        public (int VirtualOffset, SourceMapEntry Entry)? ToVirtualOffset(int uitkxOffset)
        {
            foreach (var e in _entries)
            {
                if (uitkxOffset >= e.UitkxStart && uitkxOffset <= e.UitkxEnd)
                    return (e.VirtualStart + (uitkxOffset - e.UitkxStart), e);
            }
            return null;
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="uitkxOffset"/> is inside any
        /// tracked C# region.  Cheaper than <see cref="ToVirtualOffset"/> when
        /// the caller only needs to know "is this C#?" without the actual offset.
        /// </summary>
        public bool IsInCSharpRegion(int uitkxOffset)
        {
            foreach (var e in _entries)
            {
                if (uitkxOffset >= e.UitkxStart && uitkxOffset < e.UitkxEnd)
                    return true;
            }
            return false;
        }
    }
}
