using System.Collections.Immutable;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.SourceGenerator
{
    public sealed record PeerComponentInfo(
        string Name,
        string Namespace,
        bool EmitsGeneratedProps,
        ImmutableArray<FunctionParam> FunctionParams
    )
    {
        public string MetadataTypeName =>
            string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

        public string SourceQualifiedTypeName => $"global::{MetadataTypeName}";

        public string? SourceQualifiedPropsTypeName =>
            EmitsGeneratedProps ? $"{SourceQualifiedTypeName}.{Name}Props" : null;

        // ── Import/export grammar scaffolding (leg 3, M6b) ──────────────────────
        // Additive fields consumed by the strict resolver + scoped injection (M8).
        // Inert today: nothing reads them yet, so populating them is behavior-neutral.

        /// <summary>
        /// True when the declaration carried <c>export</c> (→ <c>public</c>). Defaults to
        /// <c>true</c> so pre-migration (bare) peers stay cross-file-visible until strictness
        /// is flipped on.
        /// </summary>
        public bool IsExported { get; init; } = true;

        /// <summary>Absolute path of the owning <c>.uitkx</c> file (import resolution + HMR invalidation).</summary>
        public string? SourceFilePath { get; init; }

        /// <summary>Path-derived default namespace for this peer (M5b), or <c>null</c> until wired.</summary>
        public string? DerivedNamespace { get; init; }
    }
}
