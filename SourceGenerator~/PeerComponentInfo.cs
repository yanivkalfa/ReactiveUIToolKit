using System.Collections.Immutable;
using ReactiveUITK.Language.Parser;

namespace ReactiveUITK.SourceGenerator
{
    public sealed record PeerComponentInfo(
        string Name,
        string Namespace,
        bool EmitsGeneratedProps,
        ImmutableArray<FunctionParam> FunctionParams
    )
    {
        public string MetadataTypeName =>
            string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

        public string SourceQualifiedTypeName => $"global::{MetadataTypeName}";

        public string? SourceQualifiedPropsTypeName =>
            EmitsGeneratedProps ? $"{SourceQualifiedTypeName}.{Name}Props" : null;
    }
}
