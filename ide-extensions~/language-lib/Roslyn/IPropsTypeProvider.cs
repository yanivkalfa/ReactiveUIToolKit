namespace ReactiveUITK.Language.Roslyn
{
    /// <summary>
    /// Provides element → props-type mapping so the virtual document generator
    /// can emit typed property assignments instead of <c>object</c>-returning
    /// wrappers, enabling Roslyn to surface type-mismatch diagnostics.
    ///
    /// <para>Implementations live in the LSP server where schema and workspace
    /// index data are available.  This interface stays in <c>language-lib</c>
    /// (netstandard2.0) to avoid adding Roslyn or LSP dependencies.</para>
    /// </summary>
    public interface IPropsTypeProvider
    {
        /// <summary>
        /// Returns the fully-qualified or simple props class name for
        /// <paramref name="elementName"/> (e.g. <c>"LabelProps"</c> for
        /// <c>"Label"</c>), or <c>null</c> if the element is unknown.
        /// </summary>
        string? GetPropsType(string elementName);

        /// <summary>
        /// Returns the PascalCase C# property name on the props class for
        /// the given attribute (e.g. <c>"Text"</c> for attribute <c>"text"</c>
        /// on element <c>"Label"</c>), or <c>null</c> if the attribute is
        /// unknown or is a universal/event attribute that should not be
        /// type-checked against the props class.
        /// </summary>
        string? GetPropName(string elementName, string attributeName);

        /// <summary>
        /// Returns the C# type name of the property for the given attribute
        /// (e.g. <c>"string"</c> for attribute <c>"text"</c> on element
        /// <c>"Label"</c>, or <c>"float"</c> for attribute <c>"percent"</c>
        /// on element <c>"StatusBar"</c>).  Returns <c>null</c> if unknown.
        /// <para>Used for direct typed variable emission when the props class
        /// itself may not be resolvable in the Roslyn workspace (e.g.
        /// function-style components whose Props class is auto-generated).</para>
        /// </summary>
        string? GetPropType(string elementName, string attributeName);
    }
}
