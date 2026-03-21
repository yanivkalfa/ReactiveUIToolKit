namespace ReactiveUITK.Core
{
    /// <summary>
    /// Marker interface implemented by all typed component prop objects.
    ///
    /// Components declare their props by creating a class that implements <c>IProps</c>
    /// and overrides <see cref="object.Equals(object)"/> for correct bailout behavior.
    /// Generated props classes (from <c>@props</c> directives or function-style param lists)
    /// get structural equality emitted automatically.
    /// </summary>
    public interface IProps { }

    /// <summary>
    /// Singleton props object for components that declare no props.
    /// Passed by the framework when <c>V.Func</c> is called without a props argument.
    /// </summary>
    public sealed class EmptyProps : IProps
    {
        /// <summary>The shared singleton instance. Always reference-equal to itself.</summary>
        public static readonly EmptyProps Instance = new EmptyProps();

        private EmptyProps() { }

        public override bool Equals(object obj) => obj is EmptyProps;
        public override int GetHashCode() => 0;
    }

}
