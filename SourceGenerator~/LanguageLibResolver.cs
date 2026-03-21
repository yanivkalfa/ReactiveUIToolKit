using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ReactiveUITK.SourceGenerator
{
    /// <summary>
    /// Registers an <see cref="AppDomain.AssemblyResolve"/> handler so that
    /// ReactiveUITK.Language.dll — and any of its transitive dependencies
    /// (e.g. System.Collections.Immutable) — can be located when the source
    /// generator DLL is loaded in an isolated context (Roslyn's analyzer host
    /// or Unity's managed plugin loader).
    ///
    /// Registration is idempotent and thread-safe.  It is triggered in three ways
    /// so that it fires regardless of the runtime:
    ///   1. [ModuleInitializer]       — fires on .NET 5+ / CoreCLR at module load
    ///   2. Static constructor        — fires when UitkxGenerator type is first used
    ///   3. EnsureRegistered() call   — explicit belt-and-suspenders call in Initialize()
    /// </summary>
    internal static class LanguageLibResolver
    {
        private static int _registered;

        // ModuleInitializer runs exactly once when this assembly is loaded,
        // before any user code in the assembly executes (on .NET 5+/CoreCLR).
        [ModuleInitializer]
        internal static void Register() => EnsureRegistered();

        /// <summary>
        /// Idempotent registration — safe to call multiple times from different
        /// entry points (ModuleInitializer, static ctor, Initialize()).
        /// </summary>
        internal static void EnsureRegistered()
        {
            if (Interlocked.CompareExchange(ref _registered, 1, 0) != 0)
                return;

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            string? requestedName = new AssemblyName(args.Name).Name;
            if (string.IsNullOrEmpty(requestedName))
                return null;

            // Look in the same directory as this assembly (Analyzers/ in Unity,
            // or the build output folder during test / IDE scenarios).
            string? generatorDir = Path.GetDirectoryName(
                typeof(LanguageLibResolver).Assembly.Location);

            if (generatorDir is null)
                return null;

            string candidate = Path.Combine(generatorDir, requestedName + ".dll");
            if (File.Exists(candidate))
                return Assembly.LoadFrom(candidate);

            return null;
        }
    }
}
