using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

// ── Content-type definition ─────────────────────────────────────────────────

/// <summary>Defines the "uitkx" content type.</summary>
/// <remarks>
/// BaseDefinition is <c>CodeRemoteContentTypeName</c> so VS2022 natively routes
/// LSP features (F12, Shift+F12, F2, hover, completion, etc.) through the
/// <see cref="ILanguageClient"/> framework and middleware.
/// This requires the server to report capabilities statically in InitializeResult
/// (forced via OnInitialize callback that disables dynamic registration).
/// LSP semantic-token registration is stripped by <c>CapabilityPatchStream</c>
/// to preserve our custom <c>IClassifier</c> coloring.
/// </remarks>
internal static class UitkxContentType
{
#pragma warning disable CS0649
    [Export]
    [Name("uitkx")]
    [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
    internal static ContentTypeDefinition? UitkxContentTypeDef;

    [Export, FileExtension(".uitkx"), ContentType("uitkx")]
    internal static FileExtensionToContentTypeDefinition? UitkxExtensionDef;
#pragma warning restore CS0649
}
