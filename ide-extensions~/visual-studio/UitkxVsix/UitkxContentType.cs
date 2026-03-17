using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

// ── Content-type definition ─────────────────────────────────────────────────

/// <summary>Defines the "uitkx" content type.</summary>
/// <remarks>
/// BaseDefinition is "text" so our custom IClassifier provides all coloring.
/// Do NOT use CodeRemoteContentDefinition.CodeRemoteContentTypeName — it
/// activates VS2022's built-in LSP classification which overrides IClassifier.
/// Go-to-definition works via our IOleCommandTarget (UitkxGotoDefCommandFilter)
/// which calls the LSP server directly through InternalRpc, bypassing VS2022's
/// built-in LSP routing. Hover, completion, and formatting are similarly
/// handled through the ILanguageClient middleware.
/// </remarks>
internal static class UitkxContentType
{
#pragma warning disable CS0649
    [Export]
    [Name("uitkx")]
    [BaseDefinition("text")]
    internal static ContentTypeDefinition? UitkxContentTypeDef;

    [Export, FileExtension(".uitkx"), ContentType("uitkx")]
    internal static FileExtensionToContentTypeDefinition? UitkxExtensionDef;
#pragma warning restore CS0649
}
