using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

// ── Classification type + format for Ctrl+hover underline ───────────────────
// Uses the safe IViewTaggerProvider + ClassificationTag pattern (no AdornmentLayerDefinition).
// Based on Noah Richards' GoToDef Classifier.cs.

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = "uitkx.ctrl-click-underline")]
[Name("uitkx.ctrl-click-underline.format")]
[UserVisible(false)]
[Order(After = Priority.High)]
internal sealed class CtrlClickUnderlineFormat : ClassificationFormatDefinition
{
    public CtrlClickUnderlineFormat()
    {
        DisplayName = "UITKX Ctrl+Click Underline";
        TextDecorations = System.Windows.TextDecorations.Underline;
        ForegroundColor = Color.FromRgb(0x56, 0x9C, 0xD6); // VS2022 blue link color
    }
}

internal static class CtrlClickClassificationType
{
#pragma warning disable CS0649
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("uitkx.ctrl-click-underline")]
    internal static ClassificationTypeDefinition? Definition;
#pragma warning restore CS0649
}

// ── Tagger provider ─────────────────────────────────────────────────────────

[Export(typeof(IViewTaggerProvider))]
[ContentType("uitkx")]
[TagType(typeof(ClassificationTag))]
internal sealed class CtrlClickUnderlineClassifierProvider : IViewTaggerProvider
{
    [Import]
    internal IClassificationTypeRegistryService ClassificationRegistry = null!;

    private static IClassificationType? _classificationType;

    public static CtrlClickUnderlineClassifier? GetClassifierForView(ITextView view)
    {
        if (_classificationType == null)
            return null;
        return view.Properties.GetOrCreateSingletonProperty(
            typeof(CtrlClickUnderlineClassifier),
            () => new CtrlClickUnderlineClassifier(view, _classificationType)
        );
    }

    public ITagger<T>? CreateTagger<T>(ITextView textView, ITextBuffer buffer)
        where T : ITag
    {
        _classificationType ??= ClassificationRegistry.GetClassificationType(
            "uitkx.ctrl-click-underline"
        );

        if (textView.TextBuffer != buffer)
            return null;

        return GetClassifierForView(textView) as ITagger<T>;
    }
}

// ── The classifier (ITagger<ClassificationTag>) ─────────────────────────────

internal sealed class CtrlClickUnderlineClassifier : ITagger<ClassificationTag>
{
    private readonly IClassificationType _classificationType;
    private readonly ITextView _textView;
    private SnapshotSpan? _underlineSpan;

    public CtrlClickUnderlineClassifier(ITextView textView, IClassificationType classificationType)
    {
        _textView = textView;
        _classificationType = classificationType;
    }

    public SnapshotSpan? CurrentUnderlineSpan => _underlineSpan;

    public void SetUnderlineSpan(SnapshotSpan? span)
    {
        var oldSpan = _underlineSpan;
        _underlineSpan = span;

        if (!oldSpan.HasValue && !_underlineSpan.HasValue)
            return;
        if (oldSpan.HasValue && _underlineSpan.HasValue && oldSpan == _underlineSpan)
            return;

        if (!_underlineSpan.HasValue)
        {
            SendEvent(oldSpan!.Value);
        }
        else
        {
            var updateSpan = _underlineSpan.Value;
            if (oldSpan.HasValue)
                updateSpan = new SnapshotSpan(
                    updateSpan.Snapshot,
                    Span.FromBounds(
                        Math.Min(updateSpan.Start, oldSpan.Value.Start),
                        Math.Max(updateSpan.End, oldSpan.Value.End)
                    )
                );
            SendEvent(updateSpan);
        }
    }

    public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (!_underlineSpan.HasValue || spans.Count == 0)
            yield break;

        var request = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End);
        var underline = _underlineSpan.Value.TranslateTo(
            request.Snapshot,
            SpanTrackingMode.EdgeInclusive
        );
        if (underline.IntersectsWith(request))
            yield return new TagSpan<ClassificationTag>(
                underline,
                new ClassificationTag(_classificationType)
            );
    }

    public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

    private void SendEvent(SnapshotSpan span)
    {
        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
    }
}
