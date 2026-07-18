using System.Collections.Immutable;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.SourceGenerator
{
    /// <summary>
    /// Per-file descriptor of a NEW-MODE (plain-declaration, ES-modules campaign) peer's export
    /// surface, discovered during the pre-scan (U-03). One entry per new-syntax file. Feeds:
    /// <list type="bullet">
    ///   <item><description>member-import lowering in <c>UitkxPipeline.ResolveInjectedUsings</c>
    ///   (<c>static {ns}.__Exports</c> / star / default payloads);</description></item>
    ///   <item><description>typed BRIDGE emission for rename-on-import / default member imports
    ///   (<c>ExportsEmitter</c>);</description></item>
    ///   <item><description>dotted-tag (<c>&lt;X.Comp/&gt;</c>) namespace resolution
    ///   (star-alias map, U-05).</description></item>
    /// </list>
    /// Legacy files never get an entry — their surface stays on the existing
    /// PeerComponentInfo/PeerHookContainerInfo/PeerModuleInfo tables.
    /// </summary>
    public sealed record PeerExportsInfo(
        /// <summary>Absolute path of the owning <c>.uitkx</c> file.</summary>
        string SourceFilePath,
        /// <summary>The file's EFFECTIVE (file-keyed) namespace.</summary>
        string Namespace,
        /// <summary>Every top-level member declaration (values/utils/hooks), exported or not.</summary>
        ImmutableArray<MemberDeclaration> Members,
        /// <summary>Names of the file's component declarations (exported only).</summary>
        ImmutableArray<string> ExportedComponentNames,
        /// <summary>The <c>export default</c> name, or <c>null</c>.</summary>
        string? DefaultExportName
    );
}
