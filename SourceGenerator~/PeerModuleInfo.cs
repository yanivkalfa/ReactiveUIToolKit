namespace ReactiveUITK.SourceGenerator
{
    /// <summary>
    /// Lightweight descriptor for a <c>module</c> declaration discovered during the pre-scan
    /// phase (import/export grammar, leg 3, M6b). Module edges historically resolved via ambient
    /// C# visibility only — there was no peer table for them. The strict resolver + module-alias
    /// injection (§6: <c>using {X} = {EffectiveNamespace}.{X};</c>) consume this in M8.
    ///
    /// Inert today: no consumer reads it yet, so building it is behavior-neutral.
    /// </summary>
    public sealed record PeerModuleInfo(
        /// <summary>Module (static class) name, e.g. <c>CounterStyles</c>.</summary>
        string Name,
        /// <summary>Effective namespace the module type is emitted into.</summary>
        string Namespace,
        /// <summary>True when the declaration carried <c>export</c> (→ <c>public</c>).</summary>
        bool IsExported
    )
    {
        /// <summary>Absolute path of the owning <c>.uitkx</c> file (import resolution + HMR invalidation).</summary>
        public string? SourceFilePath { get; init; }

        /// <summary>Path-derived default namespace for this module (M5b), or <c>null</c> until wired.</summary>
        public string? DerivedNamespace { get; init; }

        public string MetadataTypeName =>
            string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

        public string SourceQualifiedTypeName => $"global::{MetadataTypeName}";
    };
}
