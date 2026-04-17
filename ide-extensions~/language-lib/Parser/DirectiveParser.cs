using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;

namespace ReactiveUITK.Language.Parser
{
    /// <summary>
    /// Parses the function-style component declaration from a .uitkx source file.
    ///
    /// A function-style component has the form:
    /// <code>
    ///   component PlayerHUD(string title = "HUD") {
    ///       var (count, setCount) = useState(0);
    ///       return (
    ///           &lt;Label text={title} /&gt;
    ///       )
    ///   }
    /// </code>
    ///
    /// The returned <see cref="DirectiveSet"/> carries the character index and
    /// 1-based line number where the markup begins, which the parser reads from.
    /// </summary>
    public static class DirectiveParser
    {
        private const string FunctionStyleDefaultNamespace = "ReactiveUITK.FunctionStyle";
        private static readonly Regex s_namespaceRegex = new Regex(
            @"^\s*namespace\s+([A-Za-z_][A-Za-z0-9_\.]*)",
            RegexOptions.Multiline | RegexOptions.CultureInvariant
        );

        private static readonly HashSet<string> s_topLevelKeywords = new HashSet<string>(
            StringComparer.Ordinal
        )
        {
            "namespace",
            "component",
            "hook",
            "module",
            "using",
            "uss",
            "props",
            "key",
            "inject",
        };

        // ΓöÇΓöÇ Public API ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Parses all top-level directives from <paramref name="source"/> and
        /// appends any validation diagnostics to <paramref name="diagnosticBag"/>.
        /// </summary>
        public static DirectiveSet Parse(
            string source,
            string filePath,
            List<ParseDiagnostic> diagnosticBag,
            bool useLastReturn = true
        )
        {
            if (source.Length > 0 && source[0] == '\uFEFF')
                source = source.Substring(1);

            if (TryParseFunctionStyle(source, filePath, diagnosticBag, out var functionStyleSet, useLastReturn))
                return functionStyleSet;

            if (LooksLikeFunctionStyleComponent(source, 0))
            {
                int fsI = 0;
                int fsLine = 1;
                SkipLeadingFunctionStyleTrivia(source, ref fsI, ref fsLine);

                string fallbackComponent = Path.GetFileNameWithoutExtension(filePath);
                if (TryReadKeyword(source, ref fsI, "component"))
                {
                    SkipSpaces(source, ref fsI);
                    if (TryReadIdentifier(source, ref fsI, out string parsedComponentName))
                        fallbackComponent = parsedComponentName;
                }

                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2105",
                    Severity = ParseSeverity.Error,
                    SourceLine = fsLine,
                    Message = "Invalid function-style component declaration. Expected 'component PascalCaseName { ... return (...) ... }'.",
                });

                return new DirectiveSet(
                    Namespace: InferFunctionStyleNamespace(filePath),
                    ComponentName: fallbackComponent,
                    PropsTypeName: null,
                    DefaultKey: null,
                    Usings: ImmutableArray<string>.Empty,
                    UssFiles: ImmutableArray<string>.Empty,
                    Injects: ImmutableArray<(string Type, string Name)>.Empty,
                    MarkupStartLine: fsLine,
                    MarkupStartIndex: source.Length,
                    MarkupEndIndex: source.Length,
                    IsFunctionStyle: true,
                    HasExplicitNamespace: false,
                    FunctionSetupCode: string.Empty,
                    FunctionSetupStartLine: fsLine
                );
            }

            // File does not contain a valid function-style component declaration.
            string fallbackName = Path.GetFileNameWithoutExtension(filePath);
            diagnosticBag.Add(new ParseDiagnostic
            {
                Code = "UITKX2105",
                Severity = ParseSeverity.Error,
                SourceLine = 1,
                Message = $"'{fallbackName}.uitkx' does not contain a valid function-style component declaration. Expected 'component PascalCaseName {{ ... return (...) ... }}'.",
            });

            return new DirectiveSet(
                Namespace: InferFunctionStyleNamespace(filePath),
                ComponentName: fallbackName,
                PropsTypeName: null,
                DefaultKey: null,
                Usings: ImmutableArray<string>.Empty,
                UssFiles: ImmutableArray<string>.Empty,
                Injects: ImmutableArray<(string Type, string Name)>.Empty,
                MarkupStartLine: 1,
                MarkupStartIndex: 0,
                MarkupEndIndex: source.Length,
                IsFunctionStyle: true,
                HasExplicitNamespace: false,
                FunctionSetupCode: string.Empty,
                FunctionSetupStartLine: 1
            );
        }

        // ΓöÇΓöÇ Helpers ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        private static bool TryParseFunctionStyle(
            string source,
            string filePath,
            List<ParseDiagnostic> diagnosticBag,
            out DirectiveSet directiveSet,
            bool useLastReturn = true
        )
        {
            directiveSet = default!;

            if (source.Length > 0 && source[0] == '\uFEFF')
                source = source.Substring(1);

            int i = 0;
            int line = 1;
            SkipLeadingFunctionStyleTrivia(source, ref i, ref line);

            // Parse optional leading `using X.Y.Z;` lines, `@uss "path"` lines,
            // AND an optional `@namespace X.Y` directive before the component keyword,
            // in any order.
            var usings = new List<string>();
            var ussFiles = new List<string>();
            string? inlineNamespace = null;
            bool parsedPreambleLine;
            do
            {
                parsedPreambleLine = false;
                if (TryReadFunctionStyleUsing(source, ref i, ref line, usings))
                {
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    parsedPreambleLine = true;
                }
                if (TryReadFunctionStyleUss(source, ref i, ref line, ussFiles))
                {
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    parsedPreambleLine = true;
                }
                if (inlineNamespace == null
                    && TryReadFunctionStyleNamespaceDirective(source, ref i, ref line, out string? parsedNs))
                {
                    inlineNamespace = parsedNs;
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    parsedPreambleLine = true;
                }
            } while (parsedPreambleLine);

            // ΓöÇΓöÇ Keyword dispatch: component / hook / module ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            if (TryReadKeywordAt(source, i, "hook") || TryReadKeywordAt(source, i, "module"))
            {
                return TryParseHookModuleFile(
                    source, filePath, diagnosticBag, ref directiveSet,
                    ref i, ref line,
                    usings, ussFiles, inlineNamespace
                );
            }

            if (!TryReadKeyword(source, ref i, "component"))
                return false;

            int componentLine = line;

            SkipSpaces(source, ref i);
            int nameStartI = i; // column anchor ΓÇö position of first char of component name
            if (!TryReadIdentifier(source, ref i, out string componentName))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2105",
                    Severity = ParseSeverity.Error,
                    SourceLine = componentLine,
                    Message = "Expected PascalCase component name after 'component'.",
                });
                componentName = Path.GetFileNameWithoutExtension(filePath);
            }

            if (!IsPascalCase(componentName))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2100",
                    Severity = ParseSeverity.Error,
                    SourceLine = componentLine,
                    Message = $"Function-style component name '{componentName}' must be PascalCase.",
                });
            }

            string functionNamespace = inlineNamespace ?? InferFunctionStyleNamespace(filePath);
            int componentNameCol = ColAtPos(source, nameStartI);

            // ΓöÇΓöÇ Optional typed-props parameter list ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            // Supports: component Name(Type param = default, ...)
            SkipSpaces(source, ref i);
            var functionParams = ImmutableArray<FunctionParam>.Empty;
            string? functionPropsTypeName = null;
            if (i < source.Length && source[i] == '(')
            {
                functionParams = ParseFunctionParamList(source, ref i, ref line, componentLine, diagnosticBag);
                if (!functionParams.IsEmpty)
                    functionPropsTypeName = componentName + "Props";
            }

            SkipWhitespaceAndNewlines(source, ref i, ref line);
            if (i >= source.Length || source[i] != '{')
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2105",
                    Severity = ParseSeverity.Error,
                    SourceLine = componentLine,
                    Message = "Expected '{' after function-style component declaration.",
                });

                directiveSet = new DirectiveSet(
                    Namespace: functionNamespace,
                    ComponentName: componentName,
                    PropsTypeName: functionPropsTypeName,
                    DefaultKey: null,
                    Usings: usings.ToImmutableArray(),
                    UssFiles: ussFiles.ToImmutableArray(),
                    Injects: ImmutableArray<(string Type, string Name)>.Empty,
                    MarkupStartLine: componentLine,
                    MarkupStartIndex: source.Length,
                    MarkupEndIndex: source.Length,
                    IsFunctionStyle: true,
                    HasExplicitNamespace: inlineNamespace != null,
                    FunctionSetupCode: string.Empty,
                    FunctionSetupStartLine: componentLine,
                    FunctionParams: functionParams,
                    ComponentDeclarationLine: componentLine,
                    ComponentNameColumn: componentNameCol
                );
                return true;
            }

            int bodyOpen = i;
            if (!TryReadBalancedBlock(source, bodyOpen, out int bodyCloseExclusive))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2105",
                    Severity = ParseSeverity.Error,
                    SourceLine = componentLine,
                    Message = "Unclosed function-style component body. Missing '}'.",
                });

                directiveSet = new DirectiveSet(
                    Namespace: functionNamespace,
                    ComponentName: componentName,
                    PropsTypeName: functionPropsTypeName,
                    DefaultKey: null,
                    Usings: usings.ToImmutableArray(),
                    UssFiles: ussFiles.ToImmutableArray(),
                    Injects: ImmutableArray<(string Type, string Name)>.Empty,
                    MarkupStartLine: componentLine,
                    MarkupStartIndex: source.Length,
                    MarkupEndIndex: source.Length,
                    IsFunctionStyle: true,
                    HasExplicitNamespace: inlineNamespace != null,
                    FunctionSetupCode: string.Empty,
                    FunctionSetupStartLine: componentLine,
                    FunctionParams: functionParams,
                    ComponentDeclarationLine: componentLine,
                    ComponentNameColumn: componentNameCol
                );
                return true;
            }

            int bodyStart = bodyOpen + 1;
            int bodyEndExclusive = bodyCloseExclusive - 1;

            if (
                !TryFindTopLevelReturn(
                    source,
                    bodyStart,
                    bodyEndExclusive,
                    out int returnStart,
                    out int returnOpenParen,
                    out int returnCloseParen,
                    out int returnStmtEndExclusive,
                    useLastReturn
                )
            )
            {
                int malformedReturnPos = FindTopLevelReturnAfter(source, bodyStart, bodyEndExclusive);
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = malformedReturnPos >= 0 ? "UITKX2102" : "UITKX2101",
                    Severity = ParseSeverity.Error,
                    SourceLine = malformedReturnPos >= 0 ? LineAtPos(source, malformedReturnPos) : componentLine,
                    // For UITKX2101 point the squiggle at the component name, matching UITKX0103.
                    SourceColumn = malformedReturnPos >= 0 ? 0 : componentNameCol,
                    EndColumn    = malformedReturnPos >= 0 ? 0 : componentNameCol + componentName.Length,
                    Message = malformedReturnPos >= 0
                        ? "'return' must return UITKX markup using 'return (...)'."
                        : "Function-style component must contain exactly one top-level 'return (...)' statement.",
                });

                int fseTrimStart1 = FirstNonWhitespaceAt(source, bodyStart);
                var setupMarkupRanges1 = FindJsxBlockRanges(source, bodyStart, bodyEndExclusive);
                var bareJsxRanges1 = FindBareJsxRanges(source, bodyStart, bodyEndExclusive);
                ScanAtExprInSetupCode(source, bodyStart, bodyEndExclusive, diagnosticBag, setupMarkupRanges1, bareJsxRanges1);
                CheckMissingSemicolonAfterJsxParenBlocks(source, setupMarkupRanges1, diagnosticBag);
                directiveSet = new DirectiveSet(
                    Namespace: functionNamespace,
                    ComponentName: componentName,
                    PropsTypeName: functionPropsTypeName,
                    DefaultKey: null,
                    Usings: usings.ToImmutableArray(),
                    UssFiles: ussFiles.ToImmutableArray(),
                    Injects: ImmutableArray<(string Type, string Name)>.Empty,
                    MarkupStartLine: componentLine,
                    MarkupStartIndex: source.Length,
                    MarkupEndIndex: source.Length,
                    IsFunctionStyle: true,
                    FunctionSetupCode: source.Substring(bodyStart, Math.Max(0, bodyEndExclusive - bodyStart)).Trim(),
                    FunctionSetupStartLine: LineAtPos(source, fseTrimStart1),
                    FunctionSetupStartOffset: fseTrimStart1,
                    FunctionParams: functionParams,
                    ComponentDeclarationLine: componentLine,
                    ComponentNameColumn: componentNameCol,
                    HasExplicitNamespace: inlineNamespace != null,
                    SetupCodeMarkupRanges: setupMarkupRanges1,
                    SetupCodeBareJsxRanges: bareJsxRanges1
                );
                return true;
            }

            int markupStart = returnOpenParen + 1;
            int markupEnd = returnCloseParen;
            int markupLine = LineAtPos(source, markupStart);

            if (!LooksLikeMarkupRoot(source, markupStart, markupEnd))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2102",
                    Severity = ParseSeverity.Error,
                    SourceLine = markupLine,
                    Message = "'return' must return UITKX markup.",
                });

                markupStart = markupEnd;
            }

            string setupCode =
                source.Substring(bodyStart, Math.Max(0, returnStart - bodyStart))
                + source.Substring(
                    returnStmtEndExclusive,
                    Math.Max(0, bodyEndExclusive - returnStmtEndExclusive)
                );

            int fseTrimStart2 = FirstNonWhitespaceAt(source, bodyStart);
            int setupGapOffset = returnStart - fseTrimStart2;
            int setupGapLength = returnStmtEndExclusive - returnStart;
            var setupMarkupRanges2 = FindJsxBlockRanges(
                    source,
                    bodyStart,       returnStart,
                    returnStmtEndExclusive, bodyEndExclusive);
            var bareJsxRanges2 = FindBareJsxRanges(
                    source,
                    bodyStart,       returnStart,
                    returnStmtEndExclusive, bodyEndExclusive);

            // Scan setup code ranges for @(expr) ΓÇö emit UITKX0306 per occurrence,
            // but skip @( inside embedded JSX markup where it is valid syntax.
            ScanAtExprInSetupCode(source, bodyStart, returnStart, diagnosticBag, setupMarkupRanges2, bareJsxRanges2);
            ScanAtExprInSetupCode(source, returnStmtEndExclusive, bodyEndExclusive, diagnosticBag, setupMarkupRanges2, bareJsxRanges2);

            CheckMissingSemicolonAfterJsxParenBlocks(source, setupMarkupRanges2, diagnosticBag);
            directiveSet = new DirectiveSet(
                Namespace: functionNamespace,
                ComponentName: componentName,
                PropsTypeName: functionPropsTypeName,
                DefaultKey: null,
                Usings: usings.ToImmutableArray(),
                UssFiles: ussFiles.ToImmutableArray(),
                Injects: ImmutableArray<(string Type, string Name)>.Empty,
                MarkupStartLine: markupLine,
                MarkupStartIndex: markupStart,
                MarkupEndIndex: markupEnd,
                IsFunctionStyle: true,
                FunctionSetupCode: setupCode.Trim(),
                FunctionSetupStartLine: LineAtPos(source, fseTrimStart2),
                FunctionSetupStartOffset: fseTrimStart2,
                FunctionParams: functionParams,
                ComponentDeclarationLine: componentLine,
                ComponentNameColumn: componentNameCol,
                HasExplicitNamespace: inlineNamespace != null,
                FunctionReturnEndLine: LineAtPos(source, returnStmtEndExclusive - 1),
                FunctionBodyEndLine: LineAtPos(source, bodyEndExclusive),
                SetupCodeMarkupRanges: setupMarkupRanges2,
                SetupCodeBareJsxRanges: bareJsxRanges2,
                FunctionSetupGapOffset: setupGapOffset,
                FunctionSetupGapLength: setupGapLength
            );

            if (TryFindNextNonWhitespace(source, bodyCloseExclusive, out int trailingPos))
            {
                if (IsDirectiveHeaderAt(source, trailingPos))
                {
                    diagnosticBag.Add(new ParseDiagnostic
                    {
                        Code = "UITKX2104",
                        Severity = ParseSeverity.Error,
                        SourceLine = LineAtPos(source, trailingPos),
                        Message = "Function-style form cannot be mixed with directive header form.",
                    });
                }
                else
                {
                    diagnosticBag.Add(new ParseDiagnostic
                    {
                        Code = "UITKX2105",
                        Severity = ParseSeverity.Error,
                        SourceLine = LineAtPos(source, trailingPos),
                        Message = "Invalid top-level statement after function-style component declaration.",
                    });
                }
            }

            return true;
        }

        // ΓöÇΓöÇ Hook / Module file parser ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Parses a .uitkx file containing one or more <c>hook</c> and/or
        /// <c>module</c> declarations (no component).
        /// Called from <see cref="TryParseFunctionStyle"/> when the first
        /// top-level keyword after the preamble is <c>hook</c> or <c>module</c>.
        /// </summary>
        private static bool TryParseHookModuleFile(
            string source,
            string filePath,
            List<ParseDiagnostic> diagnosticBag,
            ref DirectiveSet directiveSet,
            ref int i,
            ref int line,
            List<string> usings,
            List<string> ussFiles,
            string? inlineNamespace
        )
        {
            string functionNamespace = inlineNamespace ?? InferFunctionStyleNamespace(filePath);

            // Validate preamble: @uss is not allowed in hook/module files
            if (ussFiles.Count > 0)
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2210",
                    Severity = ParseSeverity.Error,
                    SourceLine = line,
                    Message = "@uss is not allowed in hook/module files. Stylesheets can only be attached to component files.",
                });
            }

            var hooks = new List<HookDeclaration>();
            var modules = new List<ModuleDeclaration>();

            // Parse multiple declarations in sequence
            while (i < source.Length)
            {
                SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                if (i >= source.Length) break;

                if (TryReadKeyword(source, ref i, "hook"))
                {
                    ParseSingleHook(source, filePath, diagnosticBag, hooks, ref i, ref line);
                }
                else if (TryReadKeyword(source, ref i, "module"))
                {
                    ParseSingleModule(source, filePath, diagnosticBag, modules, ref i, ref line);
                }
                else
                {
                    break; // unknown content after declarations
                }
            }

            if (hooks.Count == 0 && modules.Count == 0)
                return false;

            // Check for trailing content after declarations
            if (TryFindNextNonWhitespace(source, i, out int trailingPos))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2105",
                    Severity = ParseSeverity.Error,
                    SourceLine = LineAtPos(source, trailingPos),
                    Message = "Invalid top-level statement after hook/module declarations.",
                });
            }

            int firstDeclLine = hooks.Count > 0
                ? hooks[0].DeclarationLine
                : modules[0].DeclarationLine;

            directiveSet = new DirectiveSet(
                Namespace: functionNamespace,
                ComponentName: null,
                PropsTypeName: null,
                DefaultKey: null,
                Usings: usings.ToImmutableArray(),
                UssFiles: ImmutableArray<string>.Empty,
                Injects: ImmutableArray<(string Type, string Name)>.Empty,
                MarkupStartLine: firstDeclLine,
                MarkupStartIndex: source.Length,
                MarkupEndIndex: source.Length,
                IsFunctionStyle: true,
                HasExplicitNamespace: inlineNamespace != null,
                FunctionSetupCode: string.Empty,
                FunctionSetupStartLine: firstDeclLine,
                HookDeclarations: hooks.ToImmutableArray(),
                ModuleDeclarations: modules.ToImmutableArray()
            );
            return true;
        }

        /// <summary>
        /// Parses a single hook declaration starting after the <c>hook</c> keyword
        /// has already been consumed.
        /// </summary>
        private static void ParseSingleHook(
            string source,
            string filePath,
            List<ParseDiagnostic> diagnosticBag,
            List<HookDeclaration> hooks,
            ref int i,
            ref int line
        )
        {
            int hookLine = line;
            SkipSpaces(source, ref i);

            // Hook name (camelCase)
            if (!TryReadIdentifier(source, ref i, out string hookName))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2200",
                    Severity = ParseSeverity.Error,
                    SourceLine = hookLine,
                    Message = "Expected hook name after 'hook' keyword.",
                });
                return;
            }

            // Warning if not starting with "use"
            if (!hookName.StartsWith("use", StringComparison.Ordinal))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2203",
                    Severity = ParseSeverity.Warning,
                    SourceLine = hookLine,
                    Message = $"Hook name '{hookName}' should start with 'use' (e.g. 'use{char.ToUpper(hookName[0])}{hookName.Substring(1)}').",
                });
            }

            // Optional generic parameters: <T> or <T, U>
            string? genericParams = null;
            if (i < source.Length && source[i] == '<')
            {
                genericParams = ReadGenericParams(source, ref i);
            }

            // Parameter list
            SkipSpaces(source, ref i);
            var hookParams = ImmutableArray<FunctionParam>.Empty;
            if (i < source.Length && source[i] == '(')
            {
                hookParams = ParseFunctionParamList(source, ref i, ref line, hookLine, diagnosticBag);
            }
            else
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2201",
                    Severity = ParseSeverity.Error,
                    SourceLine = hookLine,
                    Message = $"Hook '{hookName}' is missing a parameter list. Add '()' after the name.",
                });
            }

            // Optional -> ReturnType
            SkipWhitespaceAndNewlines(source, ref i, ref line);
            string? returnType = TryReadArrowReturnType(source, ref i, ref line);

            // Body { ... }
            SkipWhitespaceAndNewlines(source, ref i, ref line);
            if (i >= source.Length || source[i] != '{')
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2202",
                    Severity = ParseSeverity.Error,
                    SourceLine = hookLine,
                    Message = $"Hook '{hookName}' is missing a body. Expected '{{' after declaration.",
                });
                return;
            }

            int bodyOpen = i;
            if (!TryReadBalancedBlock(source, bodyOpen, out int bodyCloseExclusive))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2202",
                    Severity = ParseSeverity.Error,
                    SourceLine = hookLine,
                    Message = $"Unclosed hook body for '{hookName}'. Missing '}}'.",
                });
                return;
            }

            int bodyStart = bodyOpen + 1;
            int bodyEnd = bodyCloseExclusive - 1;
            string rawBody = source.Substring(bodyStart, Math.Max(0, bodyEnd - bodyStart));
            string body = rawBody.Trim();

            // BodyStartOffset must point to where the trimmed body text
            // actually starts (first non-whitespace char), NOT to bodyStart
            // which includes leading \r\n and indentation stripped by Trim().
            // This offset is used by VDG to build SourceMap entries, so a
            // mismatch here shifts ALL mapped positions (rename, go-to-def, etc.).
            int trimmedLeading = rawBody.Length - rawBody.TrimStart().Length;
            int actualBodyStart = bodyStart + trimmedLeading;

            int bodyStartLine = LineAtPos(source, FirstNonWhitespaceAt(source, bodyStart));

            hooks.Add(new HookDeclaration(
                Name: hookName,
                GenericParams: genericParams,
                Params: hookParams,
                ReturnType: returnType,
                Body: body,
                DeclarationLine: hookLine,
                BodyStartLine: bodyStartLine,
                BodyStartOffset: actualBodyStart,
                BodyEndOffset: bodyEnd
            ));

            i = bodyCloseExclusive; // advance past '}'
        }

        /// <summary>
        /// Parses a single module declaration starting after the <c>module</c>
        /// keyword has already been consumed.
        /// </summary>
        private static void ParseSingleModule(
            string source,
            string filePath,
            List<ParseDiagnostic> diagnosticBag,
            List<ModuleDeclaration> modules,
            ref int i,
            ref int line
        )
        {
            int moduleLine = line;
            SkipSpaces(source, ref i);

            // Module name (PascalCase)
            if (!TryReadIdentifier(source, ref i, out string moduleName))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2204",
                    Severity = ParseSeverity.Error,
                    SourceLine = moduleLine,
                    Message = "Expected module name after 'module' keyword.",
                });
                return;
            }

            // Body { ... }
            SkipWhitespaceAndNewlines(source, ref i, ref line);
            if (i >= source.Length || source[i] != '{')
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2205",
                    Severity = ParseSeverity.Error,
                    SourceLine = moduleLine,
                    Message = $"Module '{moduleName}' is missing a body. Expected '{{' after name.",
                });
                return;
            }

            int bodyOpen = i;
            if (!TryReadBalancedBlock(source, bodyOpen, out int bodyCloseExclusive))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2205",
                    Severity = ParseSeverity.Error,
                    SourceLine = moduleLine,
                    Message = $"Unclosed module body for '{moduleName}'. Missing '}}'.",
                });
                return;
            }

            int bodyStart = bodyOpen + 1;
            int bodyEnd = bodyCloseExclusive - 1;
            string rawBody = source.Substring(bodyStart, Math.Max(0, bodyEnd - bodyStart));
            string body = rawBody.Trim();

            // BodyStartOffset must point to where the trimmed body text
            // actually starts (first non-whitespace char), NOT to bodyStart
            // which includes leading \r\n and indentation stripped by Trim().
            int trimmedLeading = rawBody.Length - rawBody.TrimStart().Length;
            int actualBodyStart = bodyStart + trimmedLeading;

            int bodyStartLine = LineAtPos(source, FirstNonWhitespaceAt(source, bodyStart));

            modules.Add(new ModuleDeclaration(
                Name: moduleName,
                Body: body,
                DeclarationLine: moduleLine,
                BodyStartLine: bodyStartLine,
                BodyStartOffset: actualBodyStart,
                BodyEndOffset: bodyEnd
            ));

            i = bodyCloseExclusive; // advance past '}'
        }

        // ΓöÇΓöÇ Arrow return type reader ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Reads an optional <c>-&gt; ReturnType</c> after a hook parameter list.
        /// Handles tuples <c>(int, Action)</c>, generics <c>List&lt;int&gt;</c>,
        /// arrays, and nullable types by scanning until <c>{</c> at depth zero.
        /// Returns <c>null</c> if no arrow is present (void hook).
        /// </summary>
        private static string? TryReadArrowReturnType(string source, ref int i, ref int line)
        {
            if (i + 1 >= source.Length || source[i] != '-' || source[i + 1] != '>')
                return null;

            i += 2; // consume '->'
            SkipWhitespaceAndNewlines(source, ref i, ref line);

            int start = i;
            int parenDepth = 0;
            int angleDepth = 0;

            while (i < source.Length)
            {
                char c = source[i];

                if (c == '(') { parenDepth++; i++; continue; }
                if (c == ')') { parenDepth--; i++; continue; }
                if (c == '<') { angleDepth++; i++; continue; }
                if (c == '>') { angleDepth--; i++; continue; }

                // Stop at '{' only when all parens and angles are balanced
                if (c == '{' && parenDepth == 0 && angleDepth == 0)
                    break;

                // Track newlines
                if (IsNewline(c))
                {
                    ConsumeNewline(source, ref i, ref line);
                    continue;
                }

                i++;
            }

            string returnType = source.Substring(start, i - start).Trim();
            return returnType.Length > 0 ? returnType : null;
        }

        // ΓöÇΓöÇ Generic params reader ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Reads generic type parameters including angle brackets from the current
        /// position, e.g. <c>&lt;T&gt;</c> or <c>&lt;TKey, TValue&gt;</c>.
        /// Returns the full string including <c>&lt;</c> and <c>&gt;</c>.
        /// </summary>
        private static string ReadGenericParams(string source, ref int i)
        {
            if (i >= source.Length || source[i] != '<')
                return string.Empty;

            int start = i;
            int depth = 0;

            while (i < source.Length)
            {
                if (source[i] == '<') { depth++; i++; }
                else if (source[i] == '>') { depth--; i++; if (depth == 0) break; }
                else i++;
            }

            return source.Substring(start, i - start);
        }

        // ΓöÇΓöÇ Function param-list parser ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Parses a comma-separated parameter list that follows a function-style
        /// component name: <c>component Foo(int X = 0, string Label = "hi")</c>.
        ///
        /// The opening <c>(</c> must be the character at <paramref name="i"/> on entry;
        /// on exit <paramref name="i"/> points past the closing <c>)</c>.
        /// </summary>
        private static ImmutableArray<FunctionParam> ParseFunctionParamList(
            string source,
            ref int i,
            ref int line,
            int componentLine,
            List<ParseDiagnostic> diagnosticBag
        )
        {
            // Consume '('
            i++;

            var result = ImmutableArray.CreateBuilder<FunctionParam>();

            while (i < source.Length)
            {
                SkipWhitespaceAndNewlines(source, ref i, ref line);

                if (i >= source.Length)
                    break;

                if (source[i] == ')')
                {
                    i++; // consume ')'
                    break;
                }

                // Parse type name (may include generics: List<int>, Dictionary<string,int>)
                if (!TryReadTypeName(source, ref i, ref line, out string typeName))
                {
                    // Skip to next comma or closing paren
                    while (i < source.Length && source[i] != ',' && source[i] != ')')
                        i++;
                    if (i < source.Length && source[i] == ',')
                        i++;
                    continue;
                }

                SkipSpaces(source, ref i);

                // Capture position before reading the identifier so we can
                // attach squiggles to unused-parameter diagnostics later.
                int paramNameLine = line;
                int paramNameCol  = ComputeColumn(source, i);

                // Parse parameter name
                if (!TryReadIdentifier(source, ref i, out string paramName))
                {
                    diagnosticBag.Add(new ParseDiagnostic
                    {
                        Code = "UITKX2106",
                        Severity = ParseSeverity.Warning,
                        SourceLine = componentLine,
                        Message = $"Expected parameter name after type '{typeName}' in component parameter list.",
                    });
                    // Skip to next comma or closing paren
                    while (i < source.Length && source[i] != ',' && source[i] != ')')
                        i++;
                    if (i < source.Length && source[i] == ',')
                        i++;
                    continue;
                }

                SkipSpaces(source, ref i);

                string? defaultValue = null;
                if (i < source.Length && source[i] == '=')
                {
                    i++; // consume '='
                    SkipSpaces(source, ref i);
                    defaultValue = ReadDefaultValue(source, ref i);
                }

                result.Add(new FunctionParam(typeName, paramName, defaultValue)
                {
                    SourceLine = paramNameLine,
                    NameColumn = paramNameCol,
                });

                SkipSpaces(source, ref i);
                if (i < source.Length && source[i] == ',')
                    i++; // consume comma, loop continues
            }

            return result.ToImmutable();
        }

        /// <summary>
        /// Reads a C# type name, including optional generic type arguments
        /// (balanced &lt; ΓÇª &gt; pairs), arrays (<c>[]</c>), and nullable markers
        /// (<c>?</c>).  Does NOT handle tuple types or complex pointer types.
        /// </summary>
        private static bool TryReadTypeName(
            string source,
            ref int i,
            ref int line,
            out string typeName
        )
        {
            typeName = string.Empty;
            int start = i;

            // Leading identifier (required)
            if (i >= source.Length || !(char.IsLetter(source[i]) || source[i] == '_'))
                return false;

            while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_'))
                i++;

            // Dotted qualifier: System.Collections.Generic.List
            while (i < source.Length && source[i] == '.')
            {
                i++; // consume '.'
                while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_'))
                    i++;
            }

            // Generic type arguments: <T1, T2>
            if (i < source.Length && source[i] == '<')
            {
                int depth = 1;
                i++; // consume '<'
                while (i < source.Length && depth > 0)
                {
                    if (source[i] == '<') { depth++; i++; }
                    else if (source[i] == '>') { depth--; i++; }
                    else i++;
                }
            }

            // Array suffix: [], [,], etc.
            while (i + 1 < source.Length && source[i] == '[')
            {
                i++;
                while (i < source.Length && source[i] != ']')
                    i++;
                if (i < source.Length)
                    i++; // consume ']'
            }

            // Nullable marker
            if (i < source.Length && source[i] == '?')
                i++;

            typeName = source.Substring(start, i - start);
            return typeName.Length > 0;
        }

        /// <summary>
        /// Reads the default-value expression for a parameter, stopping at the
        /// first unbalanced <c>,</c> or <c>)</c> (i.e., the next parameter or the
        /// closing paren of the list).
        /// Handles nested parentheses, brackets, braces, string literals, and
        /// char literals correctly so that commas inside them are not mistaken for
        /// parameter separators.
        /// </summary>
        private static string ReadDefaultValue(string source, ref int i)
        {
            int start = i;
            int parenDepth = 0;
            int braceDepth = 0;
            int bracketDepth = 0;

            while (i < source.Length)
            {
                // Skip string / char literals so commas inside them are ignored
                if (TrySkipNonCodeSpan(source, ref i, source.Length))
                    continue;

                char c = source[i];

                if (c == '(') { parenDepth++; i++; continue; }
                if (c == ')') { if (parenDepth > 0) { parenDepth--; i++; continue; } break; }
                if (c == '{') { braceDepth++; i++; continue; }
                if (c == '}') { if (braceDepth > 0) { braceDepth--; i++; continue; } break; }
                if (c == '[') { bracketDepth++; i++; continue; }
                if (c == ']') { if (bracketDepth > 0) { bracketDepth--; i++; continue; } break; }
                if (c == ',' && parenDepth == 0 && braceDepth == 0 && bracketDepth == 0) break;

                i++;
            }

            return source.Substring(start, i - start).Trim();
        }

        private static string InferFunctionStyleNamespace(string filePath)
        {
            try
            {
                var companionCsPath = Path.ChangeExtension(filePath, ".cs");
                if (string.IsNullOrWhiteSpace(companionCsPath) || !File.Exists(companionCsPath))
                    return FunctionStyleDefaultNamespace;

                var csText = File.ReadAllText(companionCsPath);
                var m = s_namespaceRegex.Match(csText);
                if (m.Success)
                {
                    var ns = m.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(ns))
                        return ns;
                }
            }
            catch
            {
            }

            return FunctionStyleDefaultNamespace;
        }

        private static bool LooksLikeMarkupRoot(string source, int start, int endExclusive)
        {
            int i = start;
            while (i < endExclusive)
            {
                if (TrySkipNonCodeSpan(source, ref i, endExclusive))
                    continue;

                if (char.IsWhiteSpace(source[i]))
                {
                    i++;
                    continue;
                }

                return source[i] == '<' || source[i] == '@'
                    || (source[i] == '{' && i + 2 < endExclusive
                        && source[i + 1] == '/' && source[i + 2] == '*');
            }

            return false;
        }

        private static bool LooksLikeFunctionStyleComponent(string source, int start)
        {
            int i = start;
            int line = 1;
            SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
            // Skip any leading `using X.Y.Z;`, `@uss "path"`, and `@namespace X.Y` lines, in any order.
            var dummy = new List<string>();
            bool skippedSomething;
            do
            {
                skippedSomething = false;
                if (TryReadFunctionStyleUsing(source, ref i, ref line, dummy))
                {
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    skippedSomething = true;
                }
                if (TryReadFunctionStyleUss(source, ref i, ref line, dummy))
                {
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    skippedSomething = true;
                }
                if (TryReadFunctionStyleNamespaceDirective(source, ref i, ref line, out _))
                {
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    skippedSomething = true;
                }
            } while (skippedSomething);
            return TryReadKeywordAt(source, i, "component")
                || TryReadKeywordAt(source, i, "hook")
                || TryReadKeywordAt(source, i, "module");
        }

        /// <summary>
        /// Tries to read a single <c>@namespace X.Y</c> line at the current position.
        /// On success advances <paramref name="i"/> past the line terminator and sets
        /// <paramref name="namespaceName"/>. On failure restores <paramref name="i"/>
        /// and returns false.
        /// </summary>
        private static bool TryReadFunctionStyleNamespaceDirective(
            string source,
            ref int i,
            ref int line,
            out string? namespaceName
        )
        {
            namespaceName = null;
            int savedI = i;
            int savedLine = line;

            // Allow leading spaces/tabs.
            while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                i++;

            // Must start with '@'.
            if (i >= source.Length || source[i] != '@')
            {
                i = savedI;
                line = savedLine;
                return false;
            }

            i++; // consume '@'

            if (!TryReadKeyword(source, ref i, "namespace"))
            {
                i = savedI;
                line = savedLine;
                return false;
            }

            SkipSpaces(source, ref i);

            // Read namespace name up to ';' or end-of-line.
            int nameStart = i;
            while (i < source.Length && source[i] != ';' && !IsNewline(source[i]))
                i++;

            string ns = source.Substring(nameStart, i - nameStart).Trim();

            // Consume optional ';'.
            if (i < source.Length && source[i] == ';')
                i++;

            // Skip remainder of line and the newline.
            while (i < source.Length && !IsNewline(source[i]))
                i++;
            if (i < source.Length && IsNewline(source[i]))
                ConsumeNewline(source, ref i, ref line);

            if (string.IsNullOrWhiteSpace(ns))
            {
                i = savedI;
                line = savedLine;
                return false;
            }

            namespaceName = ns;
            return true;
        }

        /// <summary>
        /// Tries to read a single <c>using Namespace.Name;</c> line at the current
        /// position. On success advances <paramref name="i"/> past the line terminator
        /// and appends the namespace string to <paramref name="usings"/>.
        /// On failure restores <paramref name="i"/> and returns false.
        /// </summary>
        private static bool TryReadFunctionStyleUsing(
            string source,
            ref int i,
            ref int line,
            List<string> usings
        )
        {
            int savedI = i;
            int savedLine = line;

            // Allow leading spaces/tabs ΓÇö newlines are already consumed by trivia before each call.
            while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                i++;

            // Accept both `using X.Y.Z;` (C# form) and `@using X.Y.Z` (directive form).
            if (i < source.Length && source[i] == '@')
                i++;

            if (!TryReadKeyword(source, ref i, "using"))
            {
                i = savedI;
                line = savedLine;
                return false;
            }

            SkipSpaces(source, ref i);

            // Read namespace name up to ';' or end-of-line.
            int nameStart = i;
            while (i < source.Length && source[i] != ';' && !IsNewline(source[i]))
                i++;

            string namespaceName = source.Substring(nameStart, i - nameStart).Trim();

            // Consume optional ';'.
            if (i < source.Length && source[i] == ';')
                i++;

            // Skip rest of line and the newline.
            while (i < source.Length && !IsNewline(source[i]))
                i++;
            if (i < source.Length && IsNewline(source[i]))
                ConsumeNewline(source, ref i, ref line);

            if (!string.IsNullOrWhiteSpace(namespaceName))
                usings.Add(namespaceName);

            return true;
        }

        /// <summary>
        /// Attempts to read an <c>@uss "path"</c> directive.
        /// On failure restores <paramref name="i"/> and returns false.
        /// </summary>
        private static bool TryReadFunctionStyleUss(
            string source,
            ref int i,
            ref int line,
            List<string> ussFiles
        )
        {
            int savedI = i;
            int savedLine = line;

            while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                i++;

            // @uss is always directive-form (requires '@')
            if (i >= source.Length || source[i] != '@')
            {
                i = savedI;
                line = savedLine;
                return false;
            }
            i++;

            if (!TryReadKeyword(source, ref i, "uss"))
            {
                i = savedI;
                line = savedLine;
                return false;
            }

            SkipSpaces(source, ref i);

            // Expect a quoted path: "..." or '...'
            if (i >= source.Length || (source[i] != '"' && source[i] != '\''))
            {
                i = savedI;
                line = savedLine;
                return false;
            }

            char quote = source[i];
            i++; // skip opening quote
            int pathStart = i;
            while (i < source.Length && source[i] != quote && !IsNewline(source[i]))
                i++;

            string path = source.Substring(pathStart, i - pathStart).Trim();

            // Consume closing quote
            if (i < source.Length && source[i] == quote)
                i++;

            // Consume optional ';'
            if (i < source.Length && source[i] == ';')
                i++;

            // Skip rest of line and the newline.
            while (i < source.Length && !IsNewline(source[i]))
                i++;
            if (i < source.Length && IsNewline(source[i]))
                ConsumeNewline(source, ref i, ref line);

            if (!string.IsNullOrWhiteSpace(path))
                ussFiles.Add(path);

            return true;
        }

        private static void SkipLeadingFunctionStyleTrivia(string source, ref int i, ref int line)
        {
            while (i < source.Length)
            {
                if (source[i] == ' ' || source[i] == '\t')
                {
                    i++;
                    continue;
                }

                if (IsNewline(source[i]))
                {
                    ConsumeNewline(source, ref i, ref line);
                    continue;
                }

                // // line comment
                if (
                    source[i] == '/'
                    && i + 1 < source.Length
                    && source[i + 1] == '/'
                )
                {
                    i += 2;
                    while (i < source.Length && !IsNewline(source[i]))
                        i++;
                    continue;
                }

                // /* block comment */
                if (
                    source[i] == '/'
                    && i + 1 < source.Length
                    && source[i + 1] == '*'
                )
                {
                    i += 2;
                    while (i < source.Length)
                    {
                        if (IsNewline(source[i]))
                        {
                            ConsumeNewline(source, ref i, ref line);
                            continue;
                        }

                        if (
                            source[i] == '*'
                            && i + 1 < source.Length
                            && source[i + 1] == '/'
                        )
                        {
                            i += 2;
                            break;
                        }

                        i++;
                    }
                    continue;
                }

                // <!-- html comment -->
                if (
                    source[i] == '<'
                    && i + 3 < source.Length
                    && source[i + 1] == '!'
                    && source[i + 2] == '-'
                    && source[i + 3] == '-'
                )
                {
                    i += 4;
                    while (i < source.Length)
                    {
                        if (IsNewline(source[i]))
                        {
                            ConsumeNewline(source, ref i, ref line);
                            continue;
                        }

                        if (
                            source[i] == '-'
                            && i + 2 < source.Length
                            && source[i + 1] == '-'
                            && source[i + 2] == '>'
                        )
                        {
                            i += 3;
                            break;
                        }

                        i++;
                    }
                    continue;
                }

                break;
            }
        }

        private static bool TryFindNextNonWhitespace(string source, int start, out int pos)
        {
            pos = start;
            while (pos < source.Length && char.IsWhiteSpace(source[pos]))
                pos++;

            return pos < source.Length;
        }

        private static bool IsDirectiveHeaderAt(string source, int atPos)
        {
            if (atPos < 0 || atPos >= source.Length || source[atPos] != '@')
                return false;

            int i = atPos + 1;
            int start = i;
            while (i < source.Length && char.IsLetter(source[i]))
                i++;

            if (i <= start)
                return false;

            string keyword = source.Substring(start, i - start).ToLowerInvariant();
            return s_topLevelKeywords.Contains(keyword);
        }

        private static bool TryFindTopLevelReturn(
            string source,
            int start,
            int endExclusive,
            out int returnStart,
            out int openParen,
            out int closeParen,
            out int stmtEndExclusive,
            bool useLastReturn = true
        )
        {
            returnStart = -1;
            openParen = -1;
            closeParen = -1;
            stmtEndExclusive = -1;

            int i = start;
            int braceDepth = 0;
            int parenDepth = 0;
            int bracketDepth = 0;

            while (i < endExclusive)
            {
                if (TrySkipNonCodeSpan(source, ref i, endExclusive))
                    continue;

                char c = source[i];

                if (c == '{')
                {
                    braceDepth++;
                    i++;
                    continue;
                }
                if (c == '}')
                {
                    if (braceDepth > 0)
                        braceDepth--;
                    i++;
                    continue;
                }
                if (c == '(')
                {
                    parenDepth++;
                    i++;
                    continue;
                }
                if (c == ')')
                {
                    if (parenDepth > 0)
                        parenDepth--;
                    i++;
                    continue;
                }
                if (c == '[')
                {
                    bracketDepth++;
                    i++;
                    continue;
                }
                if (c == ']')
                {
                    if (bracketDepth > 0)
                        bracketDepth--;
                    i++;
                    continue;
                }

                if (braceDepth == 0 && parenDepth == 0 && bracketDepth == 0)
                {
                    if (TryReadKeywordAt(source, i, "return"))
                    {
                        int candidateStart = i;
                        int j = i + "return".Length;
                        SkipWhitespace(source, ref j);

                        if (j < endExclusive && source[j] == '(')
                        {
                            int candidateOpenParen = j;
                            if (TryReadBalancedParen(source, candidateOpenParen, endExclusive, out int closeParenExclusive))
                            {
                                int candidateCloseParen = closeParenExclusive - 1;
                                j = closeParenExclusive;
                                SkipWhitespace(source, ref j);
                                if (j < endExclusive && source[j] == ';')
                                {
                                    returnStart = candidateStart;
                                    openParen = candidateOpenParen;
                                    closeParen = candidateCloseParen;
                                    stmtEndExclusive = j + 1;

                                    if (!useLastReturn)
                                        return true;

                                    // Continue scanning to find the last match
                                    i = stmtEndExclusive;
                                    continue;
                                }
                            }
                        }

                        if (!useLastReturn)
                            return false;
                    }
                }

                i++;
            }

            return returnStart >= 0;
        }

        private static int FindTopLevelReturnAfter(string source, int start, int endExclusive)
        {
            int i = start;
            int braceDepth = 0;
            int parenDepth = 0;
            int bracketDepth = 0;

            while (i < endExclusive)
            {
                if (TrySkipNonCodeSpan(source, ref i, endExclusive))
                    continue;

                char c = source[i];
                if (c == '{')
                {
                    braceDepth++;
                    i++;
                    continue;
                }
                if (c == '}')
                {
                    if (braceDepth > 0)
                        braceDepth--;
                    i++;
                    continue;
                }
                if (c == '(')
                {
                    parenDepth++;
                    i++;
                    continue;
                }
                if (c == ')')
                {
                    if (parenDepth > 0)
                        parenDepth--;
                    i++;
                    continue;
                }
                if (c == '[')
                {
                    bracketDepth++;
                    i++;
                    continue;
                }
                if (c == ']')
                {
                    if (bracketDepth > 0)
                        bracketDepth--;
                    i++;
                    continue;
                }

                if (braceDepth == 0 && parenDepth == 0 && bracketDepth == 0)
                {
                    if (TryReadKeywordAt(source, i, "return"))
                        return i;
                }

                i++;
            }

            return -1;
        }

        private static bool TryReadBalancedBlock(string source, int openBracePos, out int closeExclusive)
        {
            closeExclusive = -1;
            if (openBracePos < 0 || openBracePos >= source.Length || source[openBracePos] != '{')
                return false;

            int i = openBracePos + 1;
            int depth = 1;
            while (i < source.Length)
            {
                if (TrySkipNonCodeSpan(source, ref i, source.Length))
                    continue;

                if (source[i] == '{')
                {
                    depth++;
                    i++;
                    continue;
                }

                if (source[i] == '}')
                {
                    depth--;
                    i++;
                    if (depth == 0)
                    {
                        closeExclusive = i;
                        return true;
                    }
                    continue;
                }

                i++;
            }

            return false;
        }

        private static bool TryReadBalancedParen(
            string source,
            int openParenPos,
            int endExclusive,
            out int closeExclusive
        )
        {
            closeExclusive = -1;
            if (openParenPos < 0 || openParenPos >= source.Length || source[openParenPos] != '(')
                return false;

            int i = openParenPos + 1;
            int depth = 1;
            while (i < endExclusive)
            {
                if (TrySkipNonCodeSpan(source, ref i, endExclusive))
                    continue;

                if (source[i] == '(')
                {
                    depth++;
                    i++;
                    continue;
                }

                if (source[i] == ')')
                {
                    depth--;
                    i++;
                    if (depth == 0)
                    {
                        closeExclusive = i;
                        return true;
                    }
                    continue;
                }

                i++;
            }

            return false;
        }

        private static bool TrySkipNonCodeSpan(string source, ref int i, int limit)
        {
            if (i >= limit)
                return false;

            if (source[i] == '/' && i + 1 < limit)
            {
                if (source[i + 1] == '/')
                {
                    i += 2;
                    while (i < limit && source[i] != '\n')
                        i++;
                    return true;
                }

                if (source[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < limit && !(source[i] == '*' && source[i + 1] == '/'))
                        i++;
                    i = i + 1 < limit ? i + 2 : limit;
                    return true;
                }
            }

            if (source[i] == '\'')
            {
                i++;
                while (i < limit)
                {
                    if (source[i] == '\\')
                    {
                        i += 2;
                        continue;
                    }
                    if (source[i] == '\'')
                    {
                        i++;
                        break;
                    }
                    i++;
                }
                return true;
            }

            int quotePos = -1;
            bool verbatim = false;

            if (source[i] == '"')
            {
                quotePos = i;
            }
            else if ((source[i] == '@' || source[i] == '$') && i + 1 < limit && source[i + 1] == '"')
            {
                quotePos = i + 1;
                verbatim = source[i] == '@';
            }
            else if (
                (source[i] == '@' || source[i] == '$')
                && i + 2 < limit
                && (source[i + 1] == '@' || source[i + 1] == '$')
                && source[i + 2] == '"'
            )
            {
                quotePos = i + 2;
                verbatim = source[i] == '@' || source[i + 1] == '@';
            }

            if (quotePos >= 0)
            {
                i = quotePos + 1;
                while (i < limit)
                {
                    if (verbatim)
                    {
                        if (source[i] == '"')
                        {
                            if (i + 1 < limit && source[i + 1] == '"')
                            {
                                i += 2;
                                continue;
                            }
                            i++;
                            break;
                        }
                        i++;
                        continue;
                    }

                    if (source[i] == '\\')
                    {
                        i += 2;
                        continue;
                    }

                    if (source[i] == '"')
                    {
                        i++;
                        break;
                    }

                    i++;
                }

                return true;
            }

            return false;
        }

        private static bool TryReadKeyword(string source, ref int i, string keyword)
        {
            if (!TryReadKeywordAt(source, i, keyword))
                return false;
            i += keyword.Length;
            return true;
        }

        private static bool TryReadKeywordAt(string source, int i, string keyword)
        {
            if (i < 0 || i + keyword.Length > source.Length)
                return false;

            if (!string.Equals(source.Substring(i, keyword.Length), keyword, StringComparison.Ordinal))
                return false;

            char prev = i > 0 ? source[i - 1] : '\0';
            char next = i + keyword.Length < source.Length ? source[i + keyword.Length] : '\0';
            bool prevOk = prev == '\0' || !(char.IsLetterOrDigit(prev) || prev == '_');
            bool nextOk = next == '\0' || !(char.IsLetterOrDigit(next) || next == '_');
            return prevOk && nextOk;
        }

        private static bool TryReadIdentifier(string source, ref int i, out string ident)
        {
            ident = string.Empty;
            if (i >= source.Length || !(char.IsLetter(source[i]) || source[i] == '_'))
                return false;

            int start = i;
            i++;
            while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_'))
                i++;

            ident = source.Substring(start, i - start);
            return true;
        }

        private static bool IsPascalCase(string ident)
        {
            if (string.IsNullOrEmpty(ident) || !char.IsUpper(ident[0]))
                return false;

            for (int i = 1; i < ident.Length; i++)
                if (!char.IsLetterOrDigit(ident[i]))
                    return false;

            return true;
        }

        private static void SkipSpaces(string source, ref int i)
        {
            while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                i++;
        }

        /// <summary>
        /// Returns the 0-based column of position <paramref name="pos"/> in
        /// <paramref name="source"/> by scanning backwards to the nearest newline.
        /// </summary>
        private static int ComputeColumn(string source, int pos)
        {
            int lineStart = source.LastIndexOf('\n', pos > 0 ? pos - 1 : 0);
            return lineStart < 0 ? pos : pos - lineStart - 1;
        }

        private static void SkipWhitespace(string source, ref int i)
        {
            while (i < source.Length && char.IsWhiteSpace(source[i]))
                i++;
        }

        private static void SkipWhitespaceAndNewlines(string source, ref int i, ref int line)
        {
            while (i < source.Length)
            {
                if (source[i] == ' ' || source[i] == '\t')
                {
                    i++;
                    continue;
                }
                if (IsNewline(source[i]))
                {
                    ConsumeNewline(source, ref i, ref line);
                    continue;
                }
                break;
            }
        }

        /// <summary>
        /// Returns the first position in <paramref name="source"/> at or after
        /// <paramref name="start"/> that is not ASCII whitespace.  Returns
        /// <paramref name="start"/> if the character there is already non-whitespace
        /// or if <paramref name="start"/> is at/past the end of the string.
        /// </summary>
        private static int FirstNonWhitespaceAt(string source, int start)
        {
            while (start < source.Length && (source[start] == ' ' || source[start] == '\t'
                                          || source[start] == '\r' || source[start] == '\n'))
                start++;
            return start;
        }

        // ΓöÇΓöÇ @(expr) scanner ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// Scans <paramref name="source"/> between <paramref name="rangeStart"/> and
        /// <paramref name="rangeEnd"/> for <c>@(</c> tokens that are outside strings,
        /// comments, and embedded JSX markup ranges, emitting
        /// <see cref="DiagnosticCodes.AtExprInSetupCode"/> for each occurrence.
        /// </summary>
        private static void ScanAtExprInSetupCode(
            string source,
            int rangeStart,
            int rangeEnd,
            List<ParseDiagnostic> diagnosticBag,
            ImmutableArray<(int Start, int End, int Line)> jsxRanges = default,
            ImmutableArray<(int Start, int End, int Line)> bareJsxRanges = default)
        {
            int i = rangeStart;
            while (i < rangeEnd)
            {
                char ch = source[i];

                // Skip verbatim strings @"..."
                if (ch == '@' && i + 1 < rangeEnd && source[i + 1] == '"')
                {
                    i += 2;
                    while (i < rangeEnd)
                    {
                        if (source[i] == '"')
                        {
                            if (i + 1 < rangeEnd && source[i + 1] == '"')
                                i += 2; // escaped ""
                            else { i++; break; }
                        }
                        else i++;
                    }
                    continue;
                }

                // Skip regular strings "..."
                if (ch == '"')
                {
                    i++;
                    while (i < rangeEnd && source[i] != '"')
                    {
                        if (source[i] == '\\') i++;
                        i++;
                    }
                    if (i < rangeEnd) i++;
                    continue;
                }

                // Skip character literals '.'
                if (ch == '\'')
                {
                    i++;
                    if (i < rangeEnd && source[i] == '\\') i++;
                    if (i < rangeEnd) i++; // the char
                    if (i < rangeEnd && source[i] == '\'') i++;
                    continue;
                }

                // Skip single-line comments //
                if (ch == '/' && i + 1 < rangeEnd && source[i + 1] == '/')
                {
                    i += 2;
                    while (i < rangeEnd && source[i] != '\n') i++;
                    continue;
                }

                // Skip block comments /* ... */
                if (ch == '/' && i + 1 < rangeEnd && source[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < rangeEnd && !(source[i] == '*' && source[i + 1] == '/'))
                        i++;
                    if (i + 1 < rangeEnd) i += 2;
                    continue;
                }

                // Detect @(
                if (ch == '@' && i + 1 < rangeEnd && source[i + 1] == '(')
                {
                    // @(expr) is valid inside embedded JSX markup ΓÇö skip those.
                    if (!IsInsideJsxRange(i, jsxRanges) && !IsInsideJsxRange(i, bareJsxRanges))
                    {
                        diagnosticBag.Add(new ParseDiagnostic
                        {
                            Code = Diagnostics.DiagnosticCodes.AtExprInSetupCode,
                            Severity = ParseSeverity.Error,
                            SourceLine = LineAtPos(source, i),
                            SourceColumn = ColAtPos(source, i),
                            EndColumn = ColAtPos(source, i) + 2,
                            Message = "'@(...)' syntax is not supported in setup code. " +
                                      "Use a local variable instead: var x = (...); then reference x.",
                        });
                    }
                }

                i++;
            }
        }

        private static bool IsInsideJsxRange(int pos, ImmutableArray<(int Start, int End, int Line)> ranges)
        {
            if (ranges.IsDefaultOrEmpty) return false;
            foreach (var (s, e, _) in ranges)
                if (pos >= s && pos < e) return true;
            return false;
        }

        private static int LineAtPos(string source, int pos)
        {
            int line = 1;
            for (int i = 0; i < pos && i < source.Length; i++)
                if (source[i] == '\n')
                    line++;
            return line;
        }

        /// <summary>
        /// Returns the 0-based column of <paramref name="pos"/> within its line
        /// (number of characters after the last '\n' before <paramref name="pos"/>).
        /// </summary>
        private static int ColAtPos(string source, int pos)
        {
            int col = 0;
            while (pos > 0 && source[pos - 1] != '\n')
            {
                pos--;
                col++;
            }
            return col;
        }

        private static bool IsNewline(char c) => c == '\r' || c == '\n';

        // ΓöÇΓöÇ JSX block range finder ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

        /// <summary>
        /// For each paren-wrapped JSX block, checks whether a semicolon follows the
        /// closing <c>)</c>.  If not, emits a <c>CS1002</c> diagnostic.
        /// </summary>
        private static void CheckMissingSemicolonAfterJsxParenBlocks(
            string source,
            ImmutableArray<(int Start, int End, int Line)> ranges,
            List<ParseDiagnostic> diagnosticBag)
        {
            if (ranges.IsDefaultOrEmpty) return;
            foreach (var (start, end, _) in ranges)
            {
                // Only check paren-wrapped blocks ΓÇö the char before Start is '('.
                if (start <= 0 || source[start - 1] != '(') continue;

                // 'end' is position of ')'. Scan past whitespace and comments.
                int pos = SkipWhitespaceAndComments(source, end + 1);

                if (pos >= source.Length)
                {
                    AddSemicolonDiagnostic(source, end, diagnosticBag);
                    continue;
                }

                char next = source[pos];
                // Valid continuations after ')' ΓÇö operators, ternary, comma, braces, etc.
                if (next == ';' || next == ':' || next == ',' || next == ')' ||
                    next == '.' || next == '?' || next == '!' || next == '[' ||
                    next == '}' || next == '{' ||
                    next == '+' || next == '-' || next == '*' || next == '/' ||
                    next == '%' || next == '&' || next == '|' || next == '^' ||
                    next == '<' || next == '>' || next == '=' || next == '~')
                    continue;

                AddSemicolonDiagnostic(source, end, diagnosticBag);
            }
        }

        /// <summary>
        /// Advances past whitespace, <c>// line comments</c>, and <c>/* block comments */</c>.
        /// </summary>
        private static int SkipWhitespaceAndComments(string source, int pos)
        {
            while (pos < source.Length)
            {
                char c = source[pos];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    pos++;
                    continue;
                }
                if (c == '/' && pos + 1 < source.Length)
                {
                    if (source[pos + 1] == '/')
                    {
                        // Line comment ΓÇö skip to end of line
                        pos += 2;
                        while (pos < source.Length && source[pos] != '\n')
                            pos++;
                        continue;
                    }
                    if (source[pos + 1] == '*')
                    {
                        // Block comment ΓÇö skip to */
                        pos += 2;
                        while (pos + 1 < source.Length &&
                               !(source[pos] == '*' && source[pos + 1] == '/'))
                            pos++;
                        pos += 2; // skip */
                        continue;
                    }
                }
                break;
            }
            return pos;
        }

        private static void AddSemicolonDiagnostic(
            string source, int parenPos, List<ParseDiagnostic> diagnosticBag)
        {
            diagnosticBag.Add(new ParseDiagnostic
            {
                Code     = "CS1002",
                Severity = ParseSeverity.Error,
                SourceLine   = LineAtPos(source, parenPos),
                SourceColumn = ColAtPos(source, parenPos) + 1,
                Message  = "; expected",
            });
        }

        /// <summary>
        /// Scans <paramref name="source"/> between <paramref name="rangeStart"/> and
        /// <paramref name="rangeEnd"/> for UITKX JSX paren blocks of the form
        /// <c>(&lt;Element&gt;...&lt;/Element&gt;)</c> and returns a list of
        /// <c>(Start, End, Line)</c> tuples for each one found.
        /// <para>
        /// <c>Start</c> = char index just inside the opening <c>(</c>;<br/>
        /// <c>End</c>   = exclusive index at the closing <c>)</c>;<br/>
        /// <c>Line</c>  = 1-based source line of <c>Start</c>.
        /// </para>
        /// </summary>
        internal static ImmutableArray<(int Start, int End, int Line)> FindJsxBlockRanges(
            string source, int rangeStart, int rangeEnd)
        {
            var result = ImmutableArray.CreateBuilder<(int, int, int)>();
            int i = rangeStart;
            while (i < rangeEnd)
            {
                // Skip // line comments
                if (source[i] == '/' && i + 1 < rangeEnd && source[i + 1] == '/')
                {
                    while (i < rangeEnd && source[i] != '\n') i++;
                    continue;
                }

                // Skip string and char literals
                if (TrySkipStringOrCharLiteral(source, rangeEnd, ref i))
                    continue;

                // ΓöÇΓöÇ Bare arrow: => <Tag ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (source[i] == '=' && i + 1 < rangeEnd && source[i + 1] == '>')
                {
                    int peek = i + 2;
                    while (peek < rangeEnd &&
                           (source[peek] == ' '  || source[peek] == '\t' ||
                            source[peek] == '\r' || source[peek] == '\n'))
                        peek++;

                    if (peek < rangeEnd && source[peek] == '<'
                        && peek + 1 < rangeEnd && char.IsLetter(source[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(source, peek, rangeEnd);
                        if (jsxEnd > peek)
                        {
                            int blockLine = LineAtPos(source, peek);
                            result.Add((peek, jsxEnd, blockLine));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                // ΓöÇΓöÇ Paren-wrapped: ( <Tag ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (source[i] != '(')
                {
                    i++;
                    continue;
                }

                // Peek past whitespace / newlines to see if the next token is '<' or '@directive'
                int peek2 = i + 1;
                while (peek2 < rangeEnd &&
                       (source[peek2] == ' '  || source[peek2] == '\t' ||
                        source[peek2] == '\r' || source[peek2] == '\n'))
                    peek2++;

                bool isJsx = peek2 < rangeEnd && source[peek2] == '<';
                bool isDirective = !isJsx && peek2 < rangeEnd && source[peek2] == '@'
                    && StartsWithDirectiveKeyword(source, peek2 + 1, rangeEnd);

                if (!isJsx && !isDirective)
                {
                    i++;
                    continue;
                }

                // Balance parens to find the matching ')'
                int depth = 1;
                int j     = i + 1;
                while (j < rangeEnd && depth > 0)
                {
                    if (TrySkipStringOrCharLiteral(source, rangeEnd, ref j))
                        continue;
                    if      (source[j] == '(') depth++;
                    else if (source[j] == ')') depth--;
                    j++;
                }

                if (depth == 0)
                {
                    int blockStart = i + 1;   // content starts after '('
                    int blockEnd   = j - 1;   // exclusive ΓÇö at the ')'
                    int blockLine  = LineAtPos(source, blockStart);
                    result.Add((blockStart, blockEnd, blockLine));
                    i = j; // hop past the entire block
                }
                else
                {
                    i++; // unbalanced ΓÇö skip
                }
            }
            return result.ToImmutable();
        }

        /// <summary>
        /// Returns true if <paramref name="source"/> at <paramref name="pos"/>
        /// starts with a UITKX directive keyword (if, foreach, for, while, switch)
        /// followed by a non-identifier character.
        /// </summary>
        private static bool StartsWithDirectiveKeyword(string source, int pos, int end)
        {
            // Ordered longest-first to avoid "for" matching "foreach"
            ReadOnlySpan<char> text = source.AsSpan();
            return MatchKeyword(text, pos, end, "switch")
                || MatchKeyword(text, pos, end, "foreach")
                || MatchKeyword(text, pos, end, "while")
                || MatchKeyword(text, pos, end, "for")
                || MatchKeyword(text, pos, end, "if");
        }

        private static bool MatchKeyword(ReadOnlySpan<char> source, int pos, int end, string keyword)
        {
            if (pos + keyword.Length > end)
                return false;
            for (int k = 0; k < keyword.Length; k++)
                if (source[pos + k] != keyword[k])
                    return false;
            // Must be followed by a non-identifier char (or EOF) to avoid partial matches
            return pos + keyword.Length >= end
                || !char.IsLetterOrDigit(source[pos + keyword.Length]);
        }

        /// <summary>
        /// Scans two disjoint (start, end) ranges and returns the combined list.
        /// Useful when setup code spans a gap where <c>return (...)</c> was removed.
        /// </summary>
        private static ImmutableArray<(int Start, int End, int Line)> FindJsxBlockRanges(
            string source,
            int range1Start, int range1End,
            int range2Start, int range2End)
        {
            var r1 = FindJsxBlockRanges(source, range1Start, range1End);
            var r2 = FindJsxBlockRanges(source, range2Start, range2End);
            if (r2.IsDefaultOrEmpty) return r1;
            if (r1.IsDefaultOrEmpty) return r2;
            return r1.AddRange(r2);
        }

        /// <summary>
        /// Finds bare (non-paren-wrapped) JSX ranges in setup code:
        /// <c>return &lt;Tag/&gt;</c>, <c>? &lt;Tag/&gt;</c>,
        /// <c>: &lt;Tag/&gt;</c>, <c>= &lt;Tag/&gt;</c>.
        /// These are NOT detected by <see cref="FindJsxBlockRanges"/> and are
        /// stored separately to avoid breaking the formatter's block-index
        /// alignment.
        /// </summary>
        internal static ImmutableArray<(int Start, int End, int Line)> FindBareJsxRanges(
            string source, int rangeStart, int rangeEnd)
        {
            var result = ImmutableArray.CreateBuilder<(int, int, int)>();
            int i = rangeStart;
            while (i < rangeEnd)
            {
                // Skip // line comments
                if (source[i] == '/' && i + 1 < rangeEnd && source[i + 1] == '/')
                {
                    while (i < rangeEnd && source[i] != '\n') i++;
                    continue;
                }

                // Skip /* ... */ block comments
                if (source[i] == '/' && i + 1 < rangeEnd && source[i + 1] == '*')
                {
                    int end = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = end >= 0 ? end + 2 : rangeEnd;
                    continue;
                }

                // Skip string and char literals
                if (TrySkipStringOrCharLiteral(source, rangeEnd, ref i))
                    continue;

                // ΓöÇΓöÇ Bare return: return <Tag ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (source[i] == 'r' && i + 5 < rangeEnd
                    && source.Substring(i, 6) == "return"
                    && (i == 0 || !(char.IsLetterOrDigit(source[i - 1]) || source[i - 1] == '_'))
                    && (i + 6 >= rangeEnd || !(char.IsLetterOrDigit(source[i + 6]) || source[i + 6] == '_')))
                {
                    int peek = i + 6;
                    while (peek < rangeEnd &&
                           (source[peek] == ' '  || source[peek] == '\t' ||
                            source[peek] == '\r' || source[peek] == '\n'))
                        peek++;

                    if (peek < rangeEnd && source[peek] == '<'
                        && peek + 1 < rangeEnd && char.IsLetter(source[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(source, peek, rangeEnd);
                        if (jsxEnd > peek)
                        {
                            int blockLine = LineAtPos(source, peek);
                            result.Add((peek, jsxEnd, blockLine));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                // ΓöÇΓöÇ Ternary true branch: ? <Tag  (but NOT ?. or ??) ΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (source[i] == '?' && i + 1 < rangeEnd
                    && source[i + 1] != '.' && source[i + 1] != '?')
                {
                    int peek = i + 1;
                    while (peek < rangeEnd &&
                           (source[peek] == ' '  || source[peek] == '\t' ||
                            source[peek] == '\r' || source[peek] == '\n'))
                        peek++;

                    if (peek < rangeEnd && source[peek] == '<'
                        && peek + 1 < rangeEnd && char.IsLetter(source[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(source, peek, rangeEnd);
                        if (jsxEnd > peek)
                        {
                            int blockLine = LineAtPos(source, peek);
                            result.Add((peek, jsxEnd, blockLine));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                // ΓöÇΓöÇ Ternary false branch: : <Tag  (but NOT ::) ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
                if (source[i] == ':' && i + 1 < rangeEnd
                    && source[i + 1] != ':')
                {
                    int peek = i + 1;
                    while (peek < rangeEnd &&
                           (source[peek] == ' '  || source[peek] == '\t' ||
                            source[peek] == '\r' || source[peek] == '\n'))
                        peek++;

                    if (peek < rangeEnd && source[peek] == '<'
                        && peek + 1 < rangeEnd && char.IsLetter(source[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(source, peek, rangeEnd);
                        if (jsxEnd > peek)
                        {
                            int blockLine = LineAtPos(source, peek);
                            result.Add((peek, jsxEnd, blockLine));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                // ΓöÇΓöÇ Bare assignment: = <Tag  (but NOT ==, =>, !=, <=, >=)
                if (source[i] == '=' && i + 1 < rangeEnd
                    && source[i + 1] != '=' && source[i + 1] != '>'
                    && (i == 0 || (source[i - 1] != '!' && source[i - 1] != '<' && source[i - 1] != '>')))
                {
                    int peek = i + 1;
                    while (peek < rangeEnd &&
                           (source[peek] == ' '  || source[peek] == '\t' ||
                            source[peek] == '\r' || source[peek] == '\n'))
                        peek++;

                    if (peek < rangeEnd && source[peek] == '<'
                        && peek + 1 < rangeEnd && char.IsLetter(source[peek + 1]))
                    {
                        int jsxEnd = FindJsxElementEnd(source, peek, rangeEnd);
                        if (jsxEnd > peek)
                        {
                            int blockLine = LineAtPos(source, peek);
                            result.Add((peek, jsxEnd, blockLine));
                            i = jsxEnd;
                            continue;
                        }
                    }
                }

                i++;
            }
            return result.ToImmutable();
        }

        /// <summary>Two-range overload for <see cref="FindBareJsxRanges"/>.</summary>
        private static ImmutableArray<(int Start, int End, int Line)> FindBareJsxRanges(
            string source,
            int range1Start, int range1End,
            int range2Start, int range2End)
        {
            var r1 = FindBareJsxRanges(source, range1Start, range1End);
            var r2 = FindBareJsxRanges(source, range2Start, range2End);
            if (r2.IsDefaultOrEmpty) return r1;
            if (r1.IsDefaultOrEmpty) return r2;
            return r1.AddRange(r2);
        }

        /// <summary>
        /// Finds the end position (exclusive) of a JSX element starting at
        /// <paramref name="start"/> (which must point to <c>&lt;</c>).
        /// </summary>
        private static int FindJsxElementEnd(string text, int start, int limit)
            => ReturnFinder.FindJsxElementEnd(text, start, limit);

        private static void ConsumeNewline(string source, ref int i, ref int line)
        {
            if (i >= source.Length)
                return;
            if (source[i] == '\r')
                i++;
            if (i < source.Length && source[i] == '\n')
                i++;
            line++;
        }

        /// <summary>
        /// If <c>source[i]</c> starts a C# string or char literal, advances
        /// <paramref name="i"/> past its closing delimiter and returns <c>true</c>.
        /// Handles regular <c>"..."</c>, verbatim <c>@"..."</c>,
        /// interpolated <c>$"..."</c>, combined <c>$@"..."</c> / <c>@$"..."</c>,
        /// and char <c>'...'</c> literals.  Inside interpolated strings the
        /// method tracks brace depth and recursively skips nested string literals
        /// within interpolation holes.
        /// </summary>
        internal static bool TrySkipStringOrCharLiteral(string source, int rangeEnd, ref int i)
        {
            if (i >= rangeEnd) return false;
            char c0 = source[i];

            // ΓöÇΓöÇ Char literal '...' ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            if (c0 == '\'')
            {
                int j = i + 1;
                while (j < rangeEnd)
                {
                    if (source[j] == '\\') { j += 2; continue; }
                    if (source[j] == '\'') { i = j + 1; return true; }
                    j++;
                }
                i = rangeEnd;
                return true;
            }

            // ΓöÇΓöÇ Detect string kind ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            bool isVerbatim = false;
            bool isInterpolated = false;
            int quotePos = -1;

            if (c0 == '"')
            {
                quotePos = i;
            }
            else if (c0 == '$' && i + 1 < rangeEnd)
            {
                if (source[i + 1] == '"')
                {
                    isInterpolated = true;
                    quotePos = i + 1;
                }
                else if (source[i + 1] == '@' && i + 2 < rangeEnd && source[i + 2] == '"')
                {
                    isInterpolated = true;
                    isVerbatim = true;
                    quotePos = i + 2;
                }
            }
            else if (c0 == '@' && i + 1 < rangeEnd)
            {
                if (source[i + 1] == '"')
                {
                    isVerbatim = true;
                    quotePos = i + 1;
                }
                else if (source[i + 1] == '$' && i + 2 < rangeEnd && source[i + 2] == '"')
                {
                    isInterpolated = true;
                    isVerbatim = true;
                    quotePos = i + 2;
                }
            }

            if (quotePos < 0) return false;

            // ΓöÇΓöÇ Scan to end of string ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
            int k = quotePos + 1;
            int braceDepth = 0;

            while (k < rangeEnd)
            {
                char ch = source[k];

                // Inside an interpolation hole ΓÇö track braces, skip nested strings
                if (isInterpolated && braceDepth > 0)
                {
                    if (ch == '{') { braceDepth++; k++; continue; }
                    if (ch == '}') { braceDepth--; k++; continue; }
                    // Nested string or char literal inside interpolation
                    if (ch == '"' || ch == '\'' || ch == '$' || ch == '@')
                    {
                        if (TrySkipStringOrCharLiteral(source, rangeEnd, ref k))
                            continue;
                    }
                    // Skip // and /* */ inside interpolation
                    if (ch == '/' && k + 1 < rangeEnd)
                    {
                        if (source[k + 1] == '/') { while (k < rangeEnd && source[k] != '\n') k++; continue; }
                        if (source[k + 1] == '*')
                        {
                            int ce = source.IndexOf("*/", k + 2, StringComparison.Ordinal);
                            k = ce >= 0 ? ce + 2 : rangeEnd;
                            continue;
                        }
                    }
                    k++;
                    continue;
                }

                // Inside string text (braceDepth == 0)
                if (isVerbatim)
                {
                    if (ch == '"')
                    {
                        if (k + 1 < rangeEnd && source[k + 1] == '"')
                        { k += 2; continue; } // escaped ""
                        i = k + 1; return true; // end of string
                    }
                    if (isInterpolated && ch == '{')
                    {
                        if (k + 1 < rangeEnd && source[k + 1] == '{')
                        { k += 2; continue; } // escaped {{
                        braceDepth++;
                    }
                    if (isInterpolated && ch == '}')
                    {
                        if (k + 1 < rangeEnd && source[k + 1] == '}')
                        { k += 2; continue; } // escaped }}
                    }
                    k++;
                    continue;
                }

                // Regular or interpolated non-verbatim
                if (ch == '\\') { k += 2; continue; }
                if (ch == '"') { i = k + 1; return true; }
                if (isInterpolated && ch == '{')
                {
                    if (k + 1 < rangeEnd && source[k + 1] == '{')
                    { k += 2; continue; } // escaped {{
                    braceDepth++;
                }
                if (isInterpolated && ch == '}')
                {
                    if (k + 1 < rangeEnd && source[k + 1] == '}')
                    { k += 2; continue; } // escaped }}
                }
                k++;
            }

            // Unterminated ΓÇö advance to end
            i = rangeEnd;
            return true;
        }
    }
}
