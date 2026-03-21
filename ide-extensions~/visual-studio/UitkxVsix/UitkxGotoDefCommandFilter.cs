using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

/// <summary>
/// Intercepts F12 (Go To Definition) and Ctrl+Click at the raw WPF keyboard
/// level via <see cref="KeyProcessor"/>.
///
/// With <c>BaseDefinition("text")</c>, VS2022 does not route the standard
/// Go-To-Definition command (cmdID 935) to the text view's IOleCommandTarget
/// chain for custom content types. The F12 key press is consumed by the VS
/// shell before reaching any command filter we install.
///
/// A KeyProcessor fires before VS2022's command routing, giving us guaranteed
/// access to the raw keystroke.
/// </summary>
[Export(typeof(IKeyProcessorProvider))]
[ContentType("uitkx")]
[TextViewRole(PredefinedTextViewRoles.Document)]
[Name("UitkxGotoDefKeyProcessor")]
[Order(Before = "default")]
internal sealed class UitkxGotoDefKeyProcessorProvider : IKeyProcessorProvider
{
    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "uitkx-gotodef.log");

    public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
    {
        try
        {
            File.AppendAllText(
                LogPath,
                $"[{DateTime.UtcNow:O}] KeyProcessor created for text view\n"
            );
        }
        catch { }
        return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            new GotoDefKeyProcessor(wpfTextView)
        );
    }
}

internal sealed class GotoDefKeyProcessor : KeyProcessor
{
    private readonly IWpfTextView _textView;

    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "uitkx-gotodef.log");

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    public GotoDefKeyProcessor(IWpfTextView textView) => _textView = textView;

    public override void KeyDown(KeyEventArgs args)
    {
        // F12 = Go To Definition
        if (args.Key == Key.F12)
        {
            Log("F12 intercepted by KeyProcessor");
            args.Handled = true;
            GoToDefinitionAtCaret();
            return;
        }

        base.KeyDown(args);
    }

    public override void PreviewKeyDown(KeyEventArgs args)
    {
        // Also try PreviewKeyDown in case KeyDown is too late
        if (args.Key == Key.F12)
        {
            Log("F12 intercepted by PreviewKeyDown");
            args.Handled = true;
            GoToDefinitionAtCaret();
            return;
        }

        UpdateCtrlUnderline(args);
        base.PreviewKeyDown(args);
    }

    public override void PreviewKeyUp(KeyEventArgs args)
    {
        UpdateCtrlUnderline(args);
        base.PreviewKeyUp(args);
    }

    private void UpdateCtrlUnderline(KeyEventArgs args)
    {
        bool ctrlDown =
            (args.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0
            && (args.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0;
        if (!ctrlDown)
        {
            // Ctrl released — clear the underline
            var classifier = CtrlClickUnderlineClassifierProvider.GetClassifierForView(_textView);
            if (classifier?.CurrentUnderlineSpan != null)
            {
                classifier.SetUnderlineSpan(null);
                Mouse.OverrideCursor = null;
            }
        }
    }

    private void GoToDefinitionAtCaret()
    {
        GoToDefinitionAt(_textView, _textView.Caret.Position.BufferPosition);
    }

    /// <summary>Shared: triggers go-to-definition at a given buffer position.</summary>
    internal static void GoToDefinitionAt(ITextView textView, SnapshotPoint point)
    {
        var rpc = UitkxLanguageClient.InternalRpc;
        if (rpc == null)
        {
            Log("InternalRpc is null — LSP not connected");
            return;
        }

        var buffer = textView.TextBuffer;
        if (!buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
        {
            Log("No ITextDocument on buffer");
            return;
        }

        var snapshot = buffer.CurrentSnapshot;
        var lineNo = snapshot.GetLineNumberFromPosition(point.Position);
        var lineStart = snapshot.GetLineFromLineNumber(lineNo).Start.Position;
        var charNo = point.Position - lineStart;
        var uri = new Uri(doc.FilePath).AbsoluteUri;
        var text = snapshot.GetText();

        Log($"Calling definition: {uri} {lineNo}:{charNo}");

        // Use RunAsync to avoid blocking the UI thread — JoinableTaskFactory.Run
        // can deadlock when the RPC pipeline hasn't warmed up yet (first call).
        Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            // Timeout prevents infinite hang when the server hasn't finished initializing.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                await UitkxGoToDefinitionHandler.GoToDefinitionCoreAsync(
                    rpc,
                    uri,
                    text,
                    lineNo,
                    charNo,
                    cts.Token
                );
            }
            catch (OperationCanceledException)
            {
                Log("F12 timed out or cancelled");
            }
            catch (Exception ex)
            {
                Log($"F12 error: {ex.Message}");
            }
        });
    }
}

// ── Ctrl+Click mouse handler ────────────────────────────────────────────────
// Based on Noah Richards' GoToDef pattern (the canonical Ctrl+Click extension).
// Uses IMouseProcessorProvider only — NO AdornmentLayerDefinition (which causes
// MEF CompositionFailedException with BaseDefinition("text") content types).

[Export(typeof(IMouseProcessorProvider))]
[ContentType("uitkx")]
[TextViewRole(PredefinedTextViewRoles.Document)]
[Name("UitkxGotoDefMouseProcessor")]
[Order(Before = "WordSelection")]
internal sealed class UitkxGoToDefMouseHandlerProvider : IMouseProcessorProvider
{
    [Import]
    internal ITextStructureNavigatorSelectorService NavigatorService = null!;

    public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
    {
        return wpfTextView.Properties.GetOrCreateSingletonProperty(
            typeof(GoToDefMouseHandler),
            () =>
                new GoToDefMouseHandler(
                    wpfTextView,
                    NavigatorService.GetTextStructureNavigator(wpfTextView.TextBuffer)
                )
        );
    }
}

internal sealed class GoToDefMouseHandler : MouseProcessorBase
{
    private readonly IWpfTextView _view;
    private readonly ITextStructureNavigator _navigator;
    private Point? _mouseDownAnchorPoint;

    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "uitkx-gotodef.log");

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    public GoToDefMouseHandler(IWpfTextView view, ITextStructureNavigator navigator)
    {
        _view = view;
        _navigator = navigator;

        _view.LostAggregateFocus += (_, _) => ClearHighlight();
        _view.VisualElement.MouseLeave += (_, _) => ClearHighlight();
    }

    public override void PostprocessMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (
            (Keyboard.Modifiers & ModifierKeys.Control) != 0
            && (Keyboard.Modifiers & ModifierKeys.Shift) == 0
        )
        {
            _mouseDownAnchorPoint = RelativeToView(e.GetPosition(_view.VisualElement));
        }
    }

    public override void PreprocessMouseMove(MouseEventArgs e)
    {
        if (
            !_mouseDownAnchorPoint.HasValue
            && (Keyboard.Modifiers & ModifierKeys.Control) != 0
            && (Keyboard.Modifiers & ModifierKeys.Shift) == 0
            && e.LeftButton == MouseButtonState.Released
        )
        {
            TryHighlightWord(RelativeToView(e.GetPosition(_view.VisualElement)));
        }
        else if (_mouseDownAnchorPoint.HasValue)
        {
            // If dragging, clear highlight
            var currentPos = RelativeToView(e.GetPosition(_view.VisualElement));
            if (InDragOperation(_mouseDownAnchorPoint.Value, currentPos))
            {
                _mouseDownAnchorPoint = null;
                ClearHighlight();
            }
        }
    }

    public override void PreprocessMouseUp(MouseButtonEventArgs e)
    {
        if (
            _mouseDownAnchorPoint.HasValue
            && (Keyboard.Modifiers & ModifierKeys.Control) != 0
            && (Keyboard.Modifiers & ModifierKeys.Shift) == 0
        )
        {
            var currentPos = RelativeToView(e.GetPosition(_view.VisualElement));
            if (!InDragOperation(_mouseDownAnchorPoint.Value, currentPos))
            {
                var bufferPos = GetBufferPosition(currentPos);
                if (bufferPos.HasValue)
                {
                    Log("Ctrl+Click intercepted by MouseProcessor");
                    e.Handled = true;
                    ClearHighlight();
                    _view.Selection.Clear();
                    GotoDefKeyProcessor.GoToDefinitionAt(_view, bufferPos.Value);
                }
            }
        }
        _mouseDownAnchorPoint = null;
    }

    public override void PreprocessMouseLeave(MouseEventArgs e)
    {
        _mouseDownAnchorPoint = null;
        ClearHighlight();
    }

    private Point RelativeToView(Point position)
    {
        return new Point(position.X + _view.ViewportLeft, position.Y + _view.ViewportTop);
    }

    private SnapshotPoint? GetBufferPosition(Point viewRelativePos)
    {
        var line = _view.TextViewLines.GetTextViewLineContainingYCoordinate(viewRelativePos.Y);
        return line?.GetBufferPositionFromXCoordinate(viewRelativePos.X);
    }

    private void TryHighlightWord(Point viewRelativePos)
    {
        var classifier = CtrlClickUnderlineClassifierProvider.GetClassifierForView(_view);
        if (classifier == null)
            return;

        try
        {
            var bufferPos = GetBufferPosition(viewRelativePos);
            if (!bufferPos.HasValue)
            {
                ClearHighlight();
                return;
            }

            // Check if already highlighting this exact span
            var currentSpan = classifier.CurrentUnderlineSpan;
            if (currentSpan.HasValue && currentSpan.Value.Contains(bufferPos.Value))
                return;

            var extent = _navigator.GetExtentOfWord(bufferPos.Value);
            if (!extent.IsSignificant)
            {
                ClearHighlight();
                return;
            }

            classifier.SetUnderlineSpan(extent.Span);
            Mouse.OverrideCursor = Cursors.Hand;
        }
        catch
        {
            ClearHighlight();
        }
    }

    private void ClearHighlight()
    {
        var classifier = CtrlClickUnderlineClassifierProvider.GetClassifierForView(_view);
        if (classifier?.CurrentUnderlineSpan != null)
        {
            classifier.SetUnderlineSpan(null);
            Mouse.OverrideCursor = null;
        }
    }

    private static bool InDragOperation(Point anchor, Point current)
    {
        return Math.Abs(anchor.X - current.X) >= 4 || Math.Abs(anchor.Y - current.Y) >= 4;
    }
}
