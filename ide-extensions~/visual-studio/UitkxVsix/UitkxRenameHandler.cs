using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

/* ── KeyProcessor: intercepts F2 at the editor input pipeline level ───── */

[Export(typeof(IKeyProcessorProvider))]
[ContentType("uitkx")]
[TextViewRole(PredefinedTextViewRoles.Editable)]
[Name("UitkxRenameKeyProcessor")]
[Order(Before = "default")]
internal sealed class UitkxRenameKeyProcessorProvider : IKeyProcessorProvider
{
    public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
    {
        return wpfTextView.Properties.GetOrCreateSingletonProperty(
            typeof(UitkxRenameKeyProcessor),
            () => new UitkxRenameKeyProcessor(wpfTextView));
    }
}

/* ── Command filter: suppresses click-triggered RENAME dispatches ─────── */

[Export(typeof(IWpfTextViewCreationListener))]
[ContentType("uitkx")]
[TextViewRole(PredefinedTextViewRoles.Editable)]
internal sealed class UitkxRenameCommandFilterProvider : IWpfTextViewCreationListener
{
    [Import]
    internal IVsEditorAdaptersFactoryService AdapterService { get; set; } = null!;

    public void TextViewCreated(IWpfTextView textView)
    {
        if (textView.Properties.ContainsProperty(typeof(UitkxRenameCommandFilter)))
            return;

        var vsTextView = AdapterService.GetViewAdapter(textView);
        if (vsTextView == null) return;

        var filter = new UitkxRenameCommandFilter();
        vsTextView.AddCommandFilter(filter, out var next);
        filter.NextTarget = next;
        textView.Properties.AddProperty(typeof(UitkxRenameCommandFilter), filter);
        RenameLog.Write("Command filter attached");
    }
}

/* ── KeyProcessor for .uitkx files ────────────────────────────────────── */

internal sealed class UitkxRenameKeyProcessor : KeyProcessor
{
    private readonly IWpfTextView _textView;

    public UitkxRenameKeyProcessor(IWpfTextView textView)
    {
        _textView = textView;
        RenameLog.Write("KeyProcessor created for uitkx view");
    }

    public override void PreviewKeyDown(KeyEventArgs args)
    {
        if (args.Key == Key.F2)
        {
            args.Handled = true;
            RenameLog.Write("F2 intercepted by uitkx KeyProcessor");
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(
                () => RenameFlow.ExecuteAsync(_textView));
        }
    }
}

/* ── KeyProcessor for companion .cs files (e.g. .style.cs) ───────────── */

[Export(typeof(IKeyProcessorProvider))]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Editable)]
[Name("UitkxCompanionRenameKeyProcessor")]
[Order(Before = "default")]
internal sealed class UitkxCompanionRenameKeyProcessorProvider : IKeyProcessorProvider
{
    public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
    {
        return wpfTextView.Properties.GetOrCreateSingletonProperty(
            typeof(UitkxCompanionRenameKeyProcessor),
            () => new UitkxCompanionRenameKeyProcessor(wpfTextView));
    }
}

internal sealed class UitkxCompanionRenameKeyProcessor : KeyProcessor
{
    private readonly IWpfTextView _textView;
    private readonly bool _hasUitkxSibling;

    public UitkxCompanionRenameKeyProcessor(IWpfTextView textView)
    {
        _textView = textView;

        // Check once at creation if this .cs file has a .uitkx sibling
        _hasUitkxSibling = false;
        if (textView.TextBuffer.Properties.TryGetProperty(
                typeof(ITextDocument), out ITextDocument doc))
        {
            var dir = Path.GetDirectoryName(doc.FilePath);
            if (dir != null)
                _hasUitkxSibling = Directory.EnumerateFiles(dir, "*.uitkx").Any();
        }

        RenameLog.Write($"CSharp KeyProcessor created, hasUitkxSibling={_hasUitkxSibling}");
    }

    public override void PreviewKeyDown(KeyEventArgs args)
    {
        if (args.Key == Key.F2 && _hasUitkxSibling)
        {
            args.Handled = true;
            RenameLog.Write("F2 intercepted by CSharp companion KeyProcessor");
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var handled = await RenameFlow.ExecuteAsync(_textView, fallbackOnNotRenameable: true);
                if (!handled)
                {
                    // Our LSP didn't handle it — fall back to Roslyn rename
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    RenameLog.Write("Falling back to standard Roslyn rename");
                    var shell = (Microsoft.VisualStudio.Shell.Interop.IVsUIShell)
                        Package.GetGlobalService(
                            typeof(Microsoft.VisualStudio.Shell.Interop.SVsUIShell));
                    if (shell != null)
                    {
                        var cmdGroup = VSConstants.VSStd2K;
                        shell.PostExecCommand(
                            ref cmdGroup, 150 /*RENAME*/, 0, null);
                    }
                }
            });
        }
    }
}

/* ── Command filter: eats RENAME from clicks ──────────────────────────── */

internal sealed class UitkxRenameCommandFilter : IOleCommandTarget
{
    private const uint CmdIdRename = 150;
    internal IOleCommandTarget? NextTarget;

    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return NextTarget?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText)
               ?? (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
    }

    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == CmdIdRename)
        {
            RenameLog.Write("RENAME command suppressed (click)");
            return VSConstants.S_OK;
        }
        return NextTarget?.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
               ?? VSConstants.E_FAIL;
    }
}

/* ── Logging ──────────────────────────────────────────────────────────── */

internal static class RenameLog
{
    private static readonly string FilePath = Path.Combine(
        Path.GetTempPath(), "uitkx-rename.log");

    internal static void Write(string msg)
    {
        try { File.AppendAllText(FilePath, $"[{DateTime.UtcNow:O}] {msg}\n"); }
        catch { }
    }
}

/* ── Rename flow ──────────────────────────────────────────────────────── */

internal static class RenameFlow
{
    /// <summary>
    /// Returns true if rename was handled (or user cancelled), false if the
    /// symbol is not renameable by our LSP and the caller should fall back.
    /// </summary>
    internal static async System.Threading.Tasks.Task<bool> ExecuteAsync(
        IWpfTextView textView, bool fallbackOnNotRenameable = false)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var buffer = textView.TextBuffer;
        if (!buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
        {
            RenameLog.Write("No ITextDocument on buffer");
            return true;
        }

        var rpc = UitkxLanguageClient.InternalRpc;
        if (rpc == null)
        {
            RenameLog.Write("InternalRpc is null — LSP not connected");
            return !fallbackOnNotRenameable;
        }

        var uri = new Uri(doc.FilePath).AbsoluteUri;
        var snapshot = buffer.CurrentSnapshot;
        var caretPos = textView.Caret.Position.BufferPosition;
        var lineNo = snapshot.GetLineNumberFromPosition(caretPos.Position);
        var lineStart = snapshot.GetLineFromLineNumber(lineNo).Start.Position;
        var charNo = caretPos.Position - lineStart;

        try
        {
            await System.Threading.Tasks.TaskScheduler.Default;

            // Step 1: Sync buffer
            await BufferSyncService.SyncIfChangedAsync(rpc, uri, snapshot.GetText());

            // Step 2: prepareRename
            var prepare = await rpc.InvokeWithParameterObjectAsync<JToken?>(
                "textDocument/prepareRename",
                new
                {
                    textDocument = new { uri },
                    position = new { line = lineNo, character = charNo },
                });

            if (prepare == null || prepare.Type == JTokenType.Null)
            {
                RenameLog.Write("Symbol is not renameable at this position");
                return !fallbackOnNotRenameable;
            }

            var placeholder = prepare["placeholder"]?.ToString();
            if (string.IsNullOrEmpty(placeholder))
            {
                var range = prepare["range"] ?? prepare;
                var sl = range["start"]?["line"]?.Value<int>() ?? 0;
                var sc = range["start"]?["character"]?.Value<int>() ?? 0;
                var el = range["end"]?["line"]?.Value<int>() ?? 0;
                var ec = range["end"]?["character"]?.Value<int>() ?? 0;

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var sLine = snapshot.GetLineFromLineNumber(sl);
                var eLine = snapshot.GetLineFromLineNumber(el);
                var sPos = sLine.Start.Position + sc;
                var ePos = eLine.Start.Position + ec;
                if (ePos > sPos && ePos <= snapshot.Length)
                    placeholder = snapshot.GetText(sPos, ePos - sPos);
                await System.Threading.Tasks.TaskScheduler.Default;
            }

            if (string.IsNullOrEmpty(placeholder))
            {
                RenameLog.Write("Could not determine current symbol name");
                return true;
            }

            RenameLog.Write($"PrepareRename ok: '{placeholder}'");

            // Step 3: Show rename dialog on UI thread
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var newName = ShowRenameDialog(placeholder);
            if (newName == null)
            {
                RenameLog.Write("Rename cancelled by user");
                return true;
            }

            // Step 4: Send rename request on background thread
            await System.Threading.Tasks.TaskScheduler.Default;
            var workspaceEdit = await rpc.InvokeWithParameterObjectAsync<JToken?>(
                "textDocument/rename",
                new
                {
                    textDocument = new { uri },
                    position = new { line = lineNo, character = charNo },
                    newName,
                });

            if (workspaceEdit == null || workspaceEdit.Type == JTokenType.Null)
            {
                RenameLog.Write("Rename returned no edits");
                return true;
            }

            // Step 5: Apply edits on UI thread
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ApplyWorkspaceEdit(workspaceEdit, textView);
            RenameLog.Write($"Rename applied: '{placeholder}' → '{newName}'");
        }
        catch (OperationCanceledException)
        {
            RenameLog.Write("Rename cancelled");
        }
        catch (Exception ex)
        {
            RenameLog.Write($"Rename error: {ex.Message}");
        }

        return true;
    }

    private static void ApplyWorkspaceEdit(JToken workspaceEdit, IWpfTextView textView)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var changes = workspaceEdit["changes"] as JObject;
        if (changes == null) return;

        // Get the current view's file path for matching
        string? currentFilePath = null;
        if (textView.TextBuffer.Properties.TryGetProperty(
                typeof(ITextDocument), out ITextDocument currentDoc))
            currentFilePath = currentDoc.FilePath;

        foreach (var prop in changes.Properties())
        {
            var edits = prop.Value as JArray;
            if (edits == null || edits.Count == 0) continue;

            string filePath;
            try { filePath = new Uri(prop.Name).LocalPath; }
            catch { continue; }

            RenameLog.Write($"Applying {edits.Count} edit(s) to {filePath}");

            // Use current textView buffer if this is the same file
            ITextBuffer? buffer = null;
            if (currentFilePath != null &&
                string.Equals(filePath, currentFilePath, StringComparison.OrdinalIgnoreCase))
            {
                buffer = textView.TextBuffer;
                RenameLog.Write("  Using current textView buffer");
            }
            else
            {
                buffer = FindBuffer(filePath);
            }

            if (buffer != null)
            {
                ApplyEditsToBuffer(buffer, edits);
            }
            else
            {
                RenameLog.Write($"  Buffer not found — applying edits to file on disk");
                ApplyEditsToDisk(filePath, edits);
            }
        }
    }

    private static void ApplyEditsToBuffer(ITextBuffer buffer, JArray edits)
    {
        var snapshot = buffer.CurrentSnapshot;
        using var textEdit = buffer.CreateEdit();

        var sorted = edits.OrderByDescending(e =>
            (e["range"]?["start"]?["line"]?.Value<int>() ?? 0) * 100000
            + (e["range"]?["start"]?["character"]?.Value<int>() ?? 0));

        foreach (var edit in sorted)
        {
            var range = edit["range"];
            if (range == null) continue;
            var sl = range["start"]?["line"]?.Value<int>() ?? 0;
            var sc = range["start"]?["character"]?.Value<int>() ?? 0;
            var el = range["end"]?["line"]?.Value<int>() ?? 0;
            var ec = range["end"]?["character"]?.Value<int>() ?? 0;
            var newText = edit["newText"]?.ToString() ?? "";

            var startLine = snapshot.GetLineFromLineNumber(sl);
            var endLine = snapshot.GetLineFromLineNumber(el);
            var startPos = startLine.Start.Position + sc;
            var endPos = endLine.Start.Position + ec;

            textEdit.Replace(startPos, endPos - startPos, newText);
        }

        textEdit.Apply();
    }

    private static void ApplyEditsToDisk(string filePath, JArray edits)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                RenameLog.Write($"  File does not exist: {filePath}");
                return;
            }

            var lines = File.ReadAllLines(filePath).ToList();

            // Apply edits bottom-up to preserve positions
            var sorted = edits.OrderByDescending(e =>
                (e["range"]?["start"]?["line"]?.Value<int>() ?? 0) * 100000
                + (e["range"]?["start"]?["character"]?.Value<int>() ?? 0));

            foreach (var edit in sorted)
            {
                var range = edit["range"];
                if (range == null) continue;
                var sl = range["start"]?["line"]?.Value<int>() ?? 0;
                var sc = range["start"]?["character"]?.Value<int>() ?? 0;
                var el = range["end"]?["line"]?.Value<int>() ?? 0;
                var ec = range["end"]?["character"]?.Value<int>() ?? 0;
                var newText = edit["newText"]?.ToString() ?? "";

                if (sl >= lines.Count) continue;
                if (el >= lines.Count) el = lines.Count - 1;

                // Build the text before the edit start and after the edit end
                var prefix = sc <= lines[sl].Length ? lines[sl].Substring(0, sc) : lines[sl];
                var suffix = ec <= lines[el].Length ? lines[el].Substring(ec) : "";

                // Remove the affected lines
                var removeCount = el - sl + 1;
                lines.RemoveRange(sl, removeCount);

                // Insert the replaced text
                var replacedText = prefix + newText + suffix;
                var newLines = replacedText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                lines.InsertRange(sl, newLines);
            }

            File.WriteAllLines(filePath, lines);
            RenameLog.Write($"  Disk write ok: {filePath}");
        }
        catch (Exception ex)
        {
            RenameLog.Write($"  Disk write failed: {ex.Message}");
        }
    }

    private static ITextBuffer? FindBuffer(string filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var rdt = (Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable)
            Package.GetGlobalService(
                typeof(Microsoft.VisualStudio.Shell.Interop.SVsRunningDocumentTable));
        if (rdt == null) return null;

        if (rdt.FindAndLockDocument(
                (uint)Microsoft.VisualStudio.Shell.Interop._VSRDTFLAGS.RDT_NoLock,
                filePath, out _, out _, out var docData, out _) == 0
            && docData != IntPtr.Zero)
        {
            var obj = Marshal.GetObjectForIUnknown(docData);
            if (obj is ITextBuffer buf) return buf;
            if (obj is IVsTextBuffer vsBuffer)
            {
                var editorAdapters = (IVsEditorAdaptersFactoryService)
                    Package.GetGlobalService(typeof(IVsEditorAdaptersFactoryService));
                return editorAdapters?.GetDocumentBuffer(vsBuffer);
            }
        }

        // File not open — open it via DTE and retry
        var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
        dte?.ItemOperations.OpenFile(filePath);

        if (rdt.FindAndLockDocument(
                (uint)Microsoft.VisualStudio.Shell.Interop._VSRDTFLAGS.RDT_NoLock,
                filePath, out _, out _, out var docData2, out _) == 0
            && docData2 != IntPtr.Zero)
        {
            var obj = Marshal.GetObjectForIUnknown(docData2);
            if (obj is ITextBuffer buf) return buf;
        }

        return null;
    }

    private static string? ShowRenameDialog(string currentName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = currentName,
            Margin = new System.Windows.Thickness(12, 8, 12, 4),
            FontSize = 14,
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
        };
        var okButton = new System.Windows.Controls.Button
        {
            Content = "Rename", Width = 80, Height = 26,
            Margin = new System.Windows.Thickness(4), IsDefault = true,
        };
        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Cancel", Width = 80, Height = 26,
            Margin = new System.Windows.Thickness(4), IsCancel = true,
        };
        var buttonPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new System.Windows.Thickness(8, 4, 8, 8),
        };
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        var mainPanel = new System.Windows.Controls.StackPanel();
        mainPanel.Children.Add(new System.Windows.Controls.Label
        {
            Content = "New name:",
            Margin = new System.Windows.Thickness(8, 8, 8, 0),
        });
        mainPanel.Children.Add(textBox);
        mainPanel.Children.Add(buttonPanel);

        var dialog = new System.Windows.Window
        {
            Title = "Rename Symbol",
            Content = mainPanel,
            Width = 350,
            SizeToContent = System.Windows.SizeToContent.Height,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
            ResizeMode = System.Windows.ResizeMode.NoResize,
            ShowInTaskbar = false,
        };

        try
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(dialog);
            helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        }
        catch { }

        okButton.Click += (s, e) => dialog.DialogResult = true;
        textBox.SelectAll();
        textBox.Focus();

        if (dialog.ShowDialog() == true)
        {
            var newName = textBox.Text?.Trim();
            if (!string.IsNullOrEmpty(newName) && newName != currentName)
                return newName;
        }

        return null;
    }
}
