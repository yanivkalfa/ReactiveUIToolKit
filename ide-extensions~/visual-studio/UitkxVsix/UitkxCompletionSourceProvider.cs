using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

/// <summary>
/// Provides the LSP-backed async completion source for .uitkx files.
/// VS2022 does not auto-activate its RemoteCompletionSource for custom content
/// types in Open Folder mode, so we register one explicitly here.
/// </summary>
[Export(typeof(IAsyncCompletionSourceProvider))]
[ContentType("uitkx")]
[Name("UitkxLspCompletionSource")]
[Order(Before = "default")]
internal sealed class UitkxCompletionSourceProvider : IAsyncCompletionSourceProvider
{
    // Single instance — completion source is stateless.
    private static readonly UitkxCompletionSource _source = new();

    public IAsyncCompletionSource? GetOrCreate(ITextView textView) => _source;
}
