using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ReactiveUITK.SourceGenerator.Analyzers
{
    /// <summary>
    /// UITKX0210 — flags writes to fields decorated with
    /// <c>[ReactiveUITK.UitkxHmrSwap]</c> from anywhere other than the
    /// containing type's static constructor.
    ///
    /// The attribute marks generator-managed module statics whose
    /// <c>readonly</c> keyword was stripped so the HMR pipeline can refresh
    /// the slot at runtime. Writing to those fields from non-cctor code
    /// defeats hot-reload (the HMR swapper will overwrite the user's
    /// write on the next cycle) and is almost certainly a bug.
    ///
    /// Reflection-based writes are not detected by this analyzer — that is
    /// out of scope for static analysis.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UitkxHmrSwapWriteAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UITKX0210";

        private const string AttributeFullName = "ReactiveUITK.UitkxHmrSwapAttribute";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Write to [UitkxHmrSwap] field outside type initializer",
            messageFormat:
                "Field '{0}' is generator-managed for HMR re-initialization. "
                + "Writing to it from non-cctor code defeats hot-reload and is "
                + "almost certainly a bug. If this is intentional, suppress with "
                + "'#pragma warning disable UITKX0210'.",
            category: "ReactiveUITK.Hmr",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
                "The UITKX source generator emits the [UitkxHmrSwap] attribute "
                + "on every module-scope static field it produces (hoisted "
                + "styles, USS keys, user `static readonly` module fields). "
                + "These fields are conceptually immutable from user code; the "
                + "HMR pipeline rewrites them across edit-save cycles."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            if (context == null) return;
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext ctx)
        {
            var attrType = ctx.Compilation.GetTypeByMetadataName(AttributeFullName);
            if (attrType == null) return;

            ctx.RegisterOperationAction(
                opCtx => AnalyzeAssignment(opCtx, attrType),
                OperationKind.SimpleAssignment,
                OperationKind.CompoundAssignment,
                OperationKind.Increment,
                OperationKind.Decrement);
        }

        private static void AnalyzeAssignment(OperationAnalysisContext ctx, INamedTypeSymbol attrType)
        {
            IOperation target = null;
            switch (ctx.Operation)
            {
                case ISimpleAssignmentOperation simple:
                    target = simple.Target;
                    break;
                case ICompoundAssignmentOperation compound:
                    target = compound.Target;
                    break;
                case IIncrementOrDecrementOperation incdec:
                    target = incdec.Target;
                    break;
            }

            if (target is not IFieldReferenceOperation fieldRef) return;

            IFieldSymbol field = fieldRef.Field;
            if (field == null) return;

            bool hasAttr = field.GetAttributes()
                .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrType));
            if (!hasAttr) return;

            // Allow writes from the type's own static constructor (field
            // initializers are lowered into the cctor body).
            ISymbol containing = ctx.ContainingSymbol;
            while (containing != null)
            {
                if (containing is IMethodSymbol m && m.MethodKind == MethodKind.StaticConstructor)
                {
                    if (SymbolEqualityComparer.Default.Equals(m.ContainingType, field.ContainingType))
                        return;
                }
                containing = containing.ContainingSymbol;
            }

            ctx.ReportDiagnostic(Diagnostic.Create(s_rule, ctx.Operation.Syntax.GetLocation(), field.Name));
        }
    }
}
