using ReactiveUITK.Language;
using Xunit;

namespace ReactiveUITK.SourceGenerator.Tests
{
    /// <summary>
    /// The shared payload classifier (namespace-import unification plan): both the SG (UITKX2316) and
    /// the LSP split a using payload into (kind, resolvable target, target-offset) identically.
    /// The offset is what keeps a diagnostic squiggle on the namespace/type token rather than the
    /// <c>static</c>/alias-name prefix.
    /// </summary>
    public sealed class UsingPayloadFactsTests
    {
        [Fact]
        public void PlainNamespace()
        {
            var (kind, target, off) = UsingPayloadFacts.Classify("System.Text");
            Assert.Equal(UsingPayloadFacts.PayloadKind.Namespace, kind);
            Assert.Equal("System.Text", target);
            Assert.Equal(0, off);
        }

        [Fact]
        public void StaticType_OffsetPastKeyword()
        {
            var (kind, target, off) = UsingPayloadFacts.Classify("static System.Math");
            Assert.Equal(UsingPayloadFacts.PayloadKind.StaticType, kind);
            Assert.Equal("System.Math", target);
            Assert.Equal(7, off); // "static " = 7 chars
        }

        [Fact]
        public void Alias_OffsetPastEquals()
        {
            var (kind, target, off) = UsingPayloadFacts.Classify("UColor = UnityEngine.Color");
            Assert.Equal(UsingPayloadFacts.PayloadKind.Alias, kind);
            Assert.Equal("UnityEngine.Color", target);
            Assert.Equal("UColor = ".Length, off);
        }

        [Fact]
        public void GlobalPrefix_Stripped()
        {
            var (kind, target, off) = UsingPayloadFacts.Classify("global::UnityEngine.UIElements");
            Assert.Equal(UsingPayloadFacts.PayloadKind.Namespace, kind);
            Assert.Equal("UnityEngine.UIElements", target);
            Assert.Equal("global::".Length, off);
        }

        [Fact]
        public void StaticLikePrefix_NotAKeyword_TreatedAsNamespace()
        {
            // "staticThing" is not the `static` keyword (no token boundary) → plain namespace.
            var (kind, target, _) = UsingPayloadFacts.Classify("staticThing.Sub");
            Assert.Equal(UsingPayloadFacts.PayloadKind.Namespace, kind);
            Assert.Equal("staticThing.Sub", target);
        }
    }
}
