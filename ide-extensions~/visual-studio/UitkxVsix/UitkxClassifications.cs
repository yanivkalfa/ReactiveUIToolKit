using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace UitkxVsix;

internal static class UitkxClassificationNames
{
    public const string DirectivePunctuation = "uitkx.directive.punctuation";
    public const string DirectiveKeyword = "uitkx.directive.keyword";
    public const string ControlKeyword = "uitkx.control.keyword";
    public const string TypeName = "uitkx.type.name";
    public const string TagName = "uitkx.tag.name";
    public const string TagDelimiter = "uitkx.tag.delimiter";
    public const string AttributeName = "uitkx.attribute.name";
    public const string String = "uitkx.string";
    public const string Number = "uitkx.number";
    public const string Function = "uitkx.function";
    public const string Identifier = "uitkx.identifier";
    public const string Operator = "uitkx.operator";
    public const string Comment = "uitkx.comment";
}

internal static class UitkxClassificationTypes
{
#pragma warning disable CS0649
    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.DirectivePunctuation)]
    internal static ClassificationTypeDefinition? DirectivePunctuation;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.DirectiveKeyword)]
    internal static ClassificationTypeDefinition? DirectiveKeyword;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.ControlKeyword)]
    internal static ClassificationTypeDefinition? ControlKeyword;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.TypeName)]
    internal static ClassificationTypeDefinition? TypeName;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.TagName)]
    internal static ClassificationTypeDefinition? TagName;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.TagDelimiter)]
    internal static ClassificationTypeDefinition? TagDelimiter;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.AttributeName)]
    internal static ClassificationTypeDefinition? AttributeName;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.String)]
    internal static ClassificationTypeDefinition? String;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.Number)]
    internal static ClassificationTypeDefinition? Number;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.Function)]
    internal static ClassificationTypeDefinition? Function;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.Identifier)]
    internal static ClassificationTypeDefinition? Identifier;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.Operator)]
    internal static ClassificationTypeDefinition? Operator;

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(UitkxClassificationNames.Comment)]
    internal static ClassificationTypeDefinition? Comment;
#pragma warning restore CS0649
}

internal abstract class UitkxColorFormatBase : ClassificationFormatDefinition
{
    protected UitkxColorFormatBase(string displayName, byte r, byte g, byte b)
    {
        DisplayName = displayName;
        ForegroundColor = Color.FromRgb(r, g, b);
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.DirectivePunctuation)]
[Name(UitkxClassificationNames.DirectivePunctuation)]
internal sealed class UitkxDirectivePunctuationFormat : UitkxColorFormatBase
{
    public UitkxDirectivePunctuationFormat()
        : base("UITKX Directive Punctuation", 0xCC, 0xCC, 0xCC)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.DirectiveKeyword)]
[Name(UitkxClassificationNames.DirectiveKeyword)]
internal sealed class UitkxDirectiveKeywordFormat : UitkxColorFormatBase
{
    public UitkxDirectiveKeywordFormat()
        : base("UITKX Directive Keyword", 0x56, 0x9C, 0xD6)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.ControlKeyword)]
[Name(UitkxClassificationNames.ControlKeyword)]
internal sealed class UitkxControlKeywordFormat : UitkxColorFormatBase
{
    public UitkxControlKeywordFormat()
        : base("UITKX Control Keyword", 0xC5, 0x86, 0xC0)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.TypeName)]
[Name(UitkxClassificationNames.TypeName)]
internal sealed class UitkxTypeNameFormat : UitkxColorFormatBase
{
    public UitkxTypeNameFormat()
        : base("UITKX Type Name", 0x4E, 0xC9, 0xB0)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.TagName)]
[Name(UitkxClassificationNames.TagName)]
internal sealed class UitkxTagNameFormat : UitkxColorFormatBase
{
    public UitkxTagNameFormat()
        : base("UITKX Tag Name", 0x56, 0x9C, 0xD6)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.TagDelimiter)]
[Name(UitkxClassificationNames.TagDelimiter)]
internal sealed class UitkxTagDelimiterFormat : UitkxColorFormatBase
{
    public UitkxTagDelimiterFormat()
        : base("UITKX Tag Delimiter", 0x80, 0x80, 0x80)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.AttributeName)]
[Name(UitkxClassificationNames.AttributeName)]
internal sealed class UitkxAttributeNameFormat : UitkxColorFormatBase
{
    public UitkxAttributeNameFormat()
        : base("UITKX Attribute Name", 0x9C, 0xDC, 0xFE)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.String)]
[Name(UitkxClassificationNames.String)]
internal sealed class UitkxStringFormat : UitkxColorFormatBase
{
    public UitkxStringFormat()
        : base("UITKX String", 0xCE, 0x91, 0x78)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.Number)]
[Name(UitkxClassificationNames.Number)]
internal sealed class UitkxNumberFormat : UitkxColorFormatBase
{
    public UitkxNumberFormat()
        : base("UITKX Number", 0xB5, 0xCE, 0xA8)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.Function)]
[Name(UitkxClassificationNames.Function)]
internal sealed class UitkxFunctionFormat : UitkxColorFormatBase
{
    public UitkxFunctionFormat()
        : base("UITKX Function", 0xDC, 0xDC, 0xAA)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.Identifier)]
[Name(UitkxClassificationNames.Identifier)]
internal sealed class UitkxIdentifierFormat : UitkxColorFormatBase
{
    public UitkxIdentifierFormat()
        : base("UITKX Identifier", 0x9C, 0xDC, 0xFE)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.Operator)]
[Name(UitkxClassificationNames.Operator)]
internal sealed class UitkxOperatorFormat : UitkxColorFormatBase
{
    public UitkxOperatorFormat()
        : base("UITKX Operator/Punctuation", 0xD4, 0xD4, 0xD4)
    {
    }
}

[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = UitkxClassificationNames.Comment)]
[Name(UitkxClassificationNames.Comment)]
internal sealed class UitkxCommentFormat : UitkxColorFormatBase
{
    public UitkxCommentFormat()
        : base("UITKX Comment", 0x6A, 0x99, 0x55)
    {
    }
}
