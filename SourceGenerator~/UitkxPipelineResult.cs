using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ReactiveUITK.SourceGenerator
{
    /// <summary>
    /// Immutable result produced by <see cref="UitkxPipeline.Run"/> for a single
    /// .uitkx file.
    ///
    /// Must be a value-equality type (record) so the incremental generator
    /// infrastructure can determine whether the output of a pipeline step has
    /// actually changed — avoiding unnecessary downstream work.
    /// </summary>
    public sealed record UitkxPipelineResult(
        /// <summary>
        /// The hint name passed to <c>SourceProductionContext.AddSource</c>.
        /// Convention: <c>{filename}.uitkx.g.cs</c> — unique within a compilation,
        /// clearly identifies the provenance file.
        /// </summary>
        string HintName,
        /// <summary>
        /// The generated C# source code, or <c>null</c> when the pipeline
        /// encountered a fatal error and no code should be emitted for this file.
        /// </summary>
        string? Source,
        /// <summary>
        /// Zero or more diagnostics produced during parsing, resolution, or
        /// emission. Errors map back to the .uitkx file via Location objects
        /// constructed from the file path and source line number.
        /// </summary>
        ImmutableArray<Diagnostic> Diagnostics,
        /// <summary>
        /// Additional (HintName, Source) compilation units produced by a
        /// mixed-decl / multi-component file. Each entry is a COMPLETE, standalone
        /// C# compilation unit (its own <c>using</c> preamble + <c>namespace</c>
        /// block) that merges with <see cref="Source"/> as a <c>partial</c> across
        /// compilation units — exactly like sibling .uitkx files already do. This
        /// exists because a single .uitkx that declares several
        /// components/hooks/modules must NOT be emitted as one concatenated file
        /// (concatenating self-contained units puts later <c>using</c> directives
        /// after a namespace → CS1529). Default/empty for the common single-unit
        /// case, so every 3-argument construction stays byte-identical.
        /// </summary>
        ImmutableArray<(string HintName, string Source)> ExtraSources = default
    );
}
