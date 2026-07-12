using System.Collections.Immutable;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.SourceGenerator;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// The hook-container injection seam (plan §6.2, <see cref="UitkxPipeline.ResolveInjectedUsings"/>).
    /// Flag OFF exposes every container in the asmdef (legacy, CS0121-prone); flag ON exposes only the
    /// container(s) a file actually <c>import</c>s. Both modes are asserted here directly so the
    /// strict path is verified before the global flag flips (plan §12 step 6 later supersedes the
    /// three <c>HookUsingStatic*</c> tests with this contract).
    /// </summary>
    public sealed class ImportScopedHookInjectionTests
    {
        private const string ScreenPath = "C:/proj/Assets/UI/Screen.uitkx";

        private static ImmutableArray<PeerHookContainerInfo> TwoContainers() => ImmutableArray.Create(
            new PeerHookContainerInfo("NsA", "CounterHooks") { SourceFilePath = "C:/proj/Assets/UI/Counter.hooks.uitkx" },
            new PeerHookContainerInfo("NsB", "OtherHooks") { SourceFilePath = "C:/proj/Assets/UI/Other.hooks.uitkx" });

        private static DirectiveSet ScreenImportingCounter()
        {
            var ds = new DirectiveSet(
                Namespace: "ReactiveUITK.FunctionStyle",
                ComponentName: "Screen",
                PropsTypeName: null,
                DefaultKey: null,
                Usings: ImmutableArray<string>.Empty,
                UssFiles: ImmutableArray<string>.Empty,
                Injects: ImmutableArray<(string Type, string Name)>.Empty,
                MarkupStartLine: 1,
                MarkupStartIndex: 0);
            return ds with
            {
                Imports = ImmutableArray.Create(new ImportDeclaration(
                    ImmutableArray.Create("useCounter"),
                    "./Counter.hooks",
                    1, 0,
                    ImmutableArray.Create(8))),
            };
        }

        [Fact]
        public void FlagOff_ExposesEveryContainer()
        {
            var usings = UitkxPipeline.ResolveInjectedUsings(
                ScreenImportingCounter(), TwoContainers(), ScreenPath, strict: false);

            Assert.Contains("static NsA.CounterHooks", usings);
            Assert.Contains("static NsB.OtherHooks", usings); // legacy: unimported container still injected
        }

        [Fact]
        public void FlagOn_ExposesOnlyImportedContainer()
        {
            var usings = UitkxPipeline.ResolveInjectedUsings(
                ScreenImportingCounter(), TwoContainers(), ScreenPath, strict: true);

            Assert.Contains("static NsA.CounterHooks", usings);
            Assert.DoesNotContain("static NsB.OtherHooks", usings); // strict: unimported container is NOT injected
        }

        [Fact]
        public void FlagOn_CrossNamespaceModule_Aliased_SameNamespace_Not()
        {
            var ds = new DirectiveSet(
                Namespace: "My.Screen",
                ComponentName: "Screen", PropsTypeName: null, DefaultKey: null,
                Usings: ImmutableArray<string>.Empty, UssFiles: ImmutableArray<string>.Empty,
                Injects: ImmutableArray<(string Type, string Name)>.Empty,
                MarkupStartLine: 1, MarkupStartIndex: 0)
            {
                Imports = ImmutableArray.Create(
                    new ImportDeclaration(ImmutableArray.Create("Palette"), "./Palette", 1, 0, ImmutableArray<int>.Empty),
                    new ImportDeclaration(ImmutableArray.Create("Local"), "./Local", 2, 0, ImmutableArray<int>.Empty)),
            };
            var modules = ImmutableArray.Create(
                new PeerModuleInfo("Palette", "Other.Ns", true) { SourceFilePath = "C:/proj/Assets/UI/Palette.uitkx" },
                new PeerModuleInfo("Local", "My.Screen", true) { SourceFilePath = "C:/proj/Assets/UI/Local.uitkx" });

            var usings = UitkxPipeline.ResolveInjectedUsings(ds, null, ScreenPath, strict: true, modules);

            Assert.Contains("Palette = Other.Ns.Palette", usings);          // cross-namespace → aliased
            Assert.DoesNotContain(usings, u => u.StartsWith("Local =", System.StringComparison.Ordinal)); // same-ns → no alias
        }

        [Fact]
        public void FlagOn_NoImports_InjectsNothing()
        {
            var ds = new DirectiveSet(
                Namespace: "ReactiveUITK.FunctionStyle",
                ComponentName: "Screen",
                PropsTypeName: null,
                DefaultKey: null,
                Usings: ImmutableArray<string>.Empty,
                UssFiles: ImmutableArray<string>.Empty,
                Injects: ImmutableArray<(string Type, string Name)>.Empty,
                MarkupStartLine: 1,
                MarkupStartIndex: 0);

            var usings = UitkxPipeline.ResolveInjectedUsings(ds, TwoContainers(), ScreenPath, strict: true);

            Assert.DoesNotContain("static NsA.CounterHooks", usings);
            Assert.DoesNotContain("static NsB.OtherHooks", usings);
        }
    }
}
