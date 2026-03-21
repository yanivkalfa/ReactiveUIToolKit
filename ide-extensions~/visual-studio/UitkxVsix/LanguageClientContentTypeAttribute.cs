using System;
using System.ComponentModel.Composition;

namespace UitkxVsix;

/// <summary>
/// MEF metadata attribute that sets the "ContentTypes" key as string[]
/// on an ILanguageClient export — matching exactly what the VS LSP broker expects.
/// VS MEF requires ContentTypes to be a string array; the built-in [ContentType]
/// attribute only sets the singular "ContentType" key and is for other MEF contracts.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class LanguageClientContentTypeAttribute : Attribute
{
    public LanguageClientContentTypeAttribute(params string[] contentTypes)
    {
        ContentTypes = contentTypes;
    }

    public string[] ContentTypes { get; }
}
