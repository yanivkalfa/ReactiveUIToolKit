using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Newtonsoft.Json.Linq;

namespace UitkxVsix;

/// <summary>
/// Quick-info (hover) source: calls textDocument/hover on the LSP server and
/// renders the markdown result as VS2022 rich text.
/// </summary>
internal sealed class UitkxQuickInfoSource : IAsyncQuickInfoSource
{
    private readonly ITextBuffer _buffer;

    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "uitkx-hover.log");

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] {msg}\n");
        }
        catch { }
    }

    public UitkxQuickInfoSource(ITextBuffer buffer) => _buffer = buffer;

    public async Task<QuickInfoItem?> GetQuickInfoItemAsync(
        IAsyncQuickInfoSession session,
        CancellationToken cancellationToken
    )
    {
        Log("GetQuickInfoItemAsync called");
        try
        {
            var rpc = UitkxLanguageClient.InternalRpc;
            if (rpc == null)
            {
                Log("InternalRpc null");
                return null;
            }

            // Resolve trigger point in the current snapshot.
            var snapshot = _buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(snapshot);
            if (triggerPoint == null)
            {
                Log("No trigger point");
                return null;
            }

            // Get ITextDocument for the file URI.
            if (!_buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
            {
                Log("No ITextDocument");
                return null;
            }

            var uri = new Uri(doc.FilePath).AbsoluteUri;
            var lineNo = snapshot.GetLineNumberFromPosition(triggerPoint.Value.Position);
            var lineStart = snapshot.GetLineFromLineNumber(lineNo).Start.Position;
            var charNo = triggerPoint.Value.Position - lineStart;

            // Sync current buffer contents to server before hovering.
            var currentText = snapshot.GetText();
            await rpc.NotifyWithParameterObjectAsync(
                    "textDocument/didChange",
                    new
                    {
                        textDocument = new { uri, version = 1 },
                        contentChanges = new[] { new { text = currentText } },
                    }
                )
                .ConfigureAwait(false);

            Log($"Calling textDocument/hover: {uri} {lineNo}:{charNo}");
            var result = await rpc.InvokeWithParameterObjectAsync<JToken?>(
                    "textDocument/hover",
                    new
                    {
                        textDocument = new { uri },
                        position = new { line = lineNo, character = charNo },
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);

            if (result == null || result.Type == JTokenType.Null)
            {
                Log("hover: null result");
                return null;
            }

            // Extract markdown string from contents.
            string? markdown = null;
            var contents = result["contents"];
            if (contents != null)
            {
                if (contents.Type == JTokenType.String)
                    markdown = contents.ToString();
                else if (contents["value"] != null)
                    markdown = contents["value"]!.ToString();
            }

            if (string.IsNullOrWhiteSpace(markdown))
            {
                Log("hover: empty markdown");
                return null;
            }

            Log($"hover: got {markdown.Length} chars of markdown");

            // Build a VS adornment: plain ClassifiedTextElement (no Markdown renderer needed).
            var element = BuildAdornment(markdown);

            // Determine the applicable span (word under cursor).
            var wordSpan = GetWordSpan(snapshot, triggerPoint.Value);
            var trackingSpan = snapshot.CreateTrackingSpan(
                wordSpan,
                SpanTrackingMode.EdgeInclusive
            );

            return new QuickInfoItem(trackingSpan, element);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Log($"hover error: {ex.Message}");
            return null;
        }
    }

    private static object BuildAdornment(string markdown)
    {
        // Strip markdown formatting for plain-text display.
        // VS2022 WPF quick-info shows ClassifiedTextRun lines nicely.
        var lines = markdown.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        var runs = new System.Collections.Generic.List<ClassifiedTextRun>();
        bool first = true;
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (!first)
                runs.Add(new ClassifiedTextRun("text", "\n"));
            first = false;

            if (line.StartsWith("## "))
                runs.Add(new ClassifiedTextRun("keyword", line.Substring(3).Replace("`", "")));
            else if (line.StartsWith("**") && line.EndsWith("**") && line.Length >= 4)
                runs.Add(new ClassifiedTextRun("keyword", line.Substring(2, line.Length - 4)));
            else
                runs.Add(new ClassifiedTextRun("text", line.Replace("`", "")));
        }

        return new ContainerElement(ContainerElementStyle.Wrapped, new ClassifiedTextElement(runs));
    }

    private static Span GetWordSpan(ITextSnapshot snapshot, SnapshotPoint point)
    {
        var pos = point.Position;
        var start = pos;
        var end = pos;

        while (start > 0 && IsWordChar(snapshot[start - 1]))
            start--;
        while (end < snapshot.Length && IsWordChar(snapshot[end]))
            end++;

        return Span.FromBounds(start, end);
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-';

    public void Dispose() { }
}
