namespace ReactiveUITK.SourceGenerator
{
    /// <summary>
    /// Lightweight descriptor for a hook container class discovered during
    /// the pre-scan phase. Used to inject <c>using static</c> directives
    /// into component files so that hook methods (e.g. <c>useCounter()</c>)
    /// are directly callable without qualifying the container class name.
    /// </summary>
    public sealed record PeerHookContainerInfo(
        /// <summary>Namespace declared in the hook file (<c>@namespace</c>).</summary>
        string Namespace,
        /// <summary>
        /// Generated container class name (e.g. <c>TicTacToeHooks</c>).
        /// Derived from the filename by <see cref="Emitter.HookEmitter.DeriveContainerClassName"/>.
        /// </summary>
        string ClassName
    );
}
