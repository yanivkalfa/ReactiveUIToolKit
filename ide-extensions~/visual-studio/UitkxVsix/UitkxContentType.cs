using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

// ── Content-type definition ─────────────────────────────────────────────────

/// <summary>Defines the "uitkx" content type that inherits from "code".</summary>
internal static class UitkxContentType
{
#pragma warning disable CS0649
    [Export, Name("uitkx"), DisplayName("UITKX"), BaseDefinition("text")]
    internal static ContentTypeDefinition? UitkxContentTypeDef;

    [Export, FileExtension(".uitkx"), ContentType("uitkx")]
    internal static FileExtensionToContentTypeDefinition? UitkxExtensionDef;
#pragma warning restore CS0649
}
