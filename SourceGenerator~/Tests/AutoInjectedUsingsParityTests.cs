using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReactiveUITK.Language;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// Guards the single-source-of-truth <see cref="AutoInjectedUsings.Namespaces"/> against the
    /// actual emitter preamble (namespace-import unification plan). If a future edit adds/removes a
    /// plain <c>using X;</c> in CSharpEmitter's baseline block, this test fails until the shared list
    /// (which drives UITKX2317 + the codemod's <c>--tidy</c>) is updated in lockstep — the same
    /// drift-guard idiom as HmrCSharpEmitterAliasParityTests.
    /// </summary>
    public sealed class AutoInjectedUsingsParityTests
    {
        [Fact]
        public void SharedBaselineList_MatchesCSharpEmitterPreamble()
        {
            string emitter = File.ReadAllText(CSharpEmitterPath());

            // The emitter's baseline block is the run of `L("using X;");` lines before the first
            // `foreach (var u in _directives.Usings)`. Capture the plain (non-static, non-alias) ones.
            int cut = emitter.IndexOf("_directives.Usings", StringComparison.Ordinal);
            Assert.True(cut > 0, "Could not locate the user-usings loop in CSharpEmitter.");
            string preamble = emitter.Substring(0, cut);

            var emitted = Regex.Matches(preamble, @"L\(""using ([A-Za-z_][A-Za-z0-9_.]*);""\)")
                .Select(m => m.Groups[1].Value)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var ns in AutoInjectedUsings.Namespaces)
                Assert.True(emitted.Contains(ns),
                    $"AutoInjectedUsings lists `{ns}` but CSharpEmitter no longer injects it.");
            foreach (var ns in emitted)
                Assert.True(AutoInjectedUsings.Namespaces.Contains(ns),
                    $"CSharpEmitter injects `{ns}` but AutoInjectedUsings omits it (2317/--tidy would miss it).");
        }

        [Fact]
        public void IsRedundant_TrueForBaseline_FalseForOthers()
        {
            Assert.True(AutoInjectedUsings.IsRedundant("UnityEngine"));
            Assert.True(AutoInjectedUsings.IsRedundant("System"));
            Assert.True(AutoInjectedUsings.IsRedundant("  System.Linq  ")); // trimmed
            Assert.False(AutoInjectedUsings.IsRedundant("UnityEngine.UIElements")); // NOT auto-injected
            Assert.False(AutoInjectedUsings.IsRedundant("ReactiveUITK.Router"));
            Assert.False(AutoInjectedUsings.IsRedundant("static System.Math"));
        }

        private static string CSharpEmitterPath()
        {
            // From Tests/ → ../Emitter/CSharpEmitter.cs
            string testsDir = Path.GetDirectoryName(
                new Uri(typeof(AutoInjectedUsingsParityTests).Assembly.Location).LocalPath)!;
            // Walk up to the SourceGenerator~ root (contains Emitter/).
            string dir = testsDir;
            for (int i = 0; i < 8 && dir != null; i++)
            {
                string candidate = Path.Combine(dir, "Emitter", "CSharpEmitter.cs");
                if (File.Exists(candidate)) return candidate;
                candidate = Path.Combine(dir, "SourceGenerator~", "Emitter", "CSharpEmitter.cs");
                if (File.Exists(candidate)) return candidate;
                dir = Path.GetDirectoryName(dir);
            }
            throw new FileNotFoundException("Could not locate CSharpEmitter.cs from the test assembly.");
        }
    }
}
