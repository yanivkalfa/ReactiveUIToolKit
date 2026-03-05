// Polyfill: 'record' and 'init' setters (C# 9) rely on
// System.Runtime.CompilerServices.IsExternalInit, which exists in
// .NET 5+ BCL but not in netstandard2.0.
// Adding this empty marker class re-enables both features when targeting
// netstandard2.0, which is required by the Roslyn analyzer/generator SDK.
// See: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/init
//
// ModuleInitializerAttribute (C# 9 / .NET 5+) polyfill is included here as well
// so that LanguageLibResolver can use [ModuleInitializer] on netstandard2.0.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ModuleInitializerAttribute : Attribute { }
}
