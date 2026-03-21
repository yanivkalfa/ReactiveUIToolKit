using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

[Export(typeof(IAsyncQuickInfoSourceProvider))]
[ContentType("uitkx")]
[Name("UitkxLspQuickInfo")]
internal sealed class UitkxQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
{
    public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) =>
        new UitkxQuickInfoSource(textBuffer);
}
