using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

/// <summary>
/// Sets 2-space indentation for .uitkx files when a text view is created.
/// </summary>
[Export(typeof(IWpfTextViewCreationListener))]
[ContentType("uitkx")]
[TextViewRole(PredefinedTextViewRoles.Editable)]
internal sealed class UitkxEditorSettings : IWpfTextViewCreationListener
{
    public void TextViewCreated(IWpfTextView textView)
    {
        textView.Options.SetOptionValue(DefaultOptions.TabSizeOptionId, 2);
        textView.Options.SetOptionValue(DefaultOptions.IndentSizeOptionId, 2);
        textView.Options.SetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId, true);
    }
}
