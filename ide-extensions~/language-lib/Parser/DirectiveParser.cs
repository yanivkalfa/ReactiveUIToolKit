using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;

namespace ReactiveUITK.Language.Parser
{
    /// <summary>
    /// Extracts the top-level <c>@directive value</c> lines from the beginning of a
    /// .uitkx source file and validates that the required directives are present.
    ///
    /// The directive block consists of consecutive lines of the form:
    /// <code>
    ///   @namespace  MyGame.UI
    ///   @component  PlayerHUD
    ///   @using      MyGame.Models
    ///   @using      System.Collections.Generic
    ///   @props      PlayerHUDProps
    ///   @key        "hud-root"
    /// </code>
    ///
    /// The block ends at the first line that is not blank and does not start with
    /// one of the recognised top-level directive keywords.  Any <c>@</c> word that
    /// is a markup/control-flow keyword (<c>@if</c>, <c>@foreach</c>, etc.) is
    /// treated as the start of markup and therefore ends the directive block.
    ///
    /// The returned <see cref="DirectiveSet"/> also carries the character index and
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
            "using",
            "props",
            "key",
            "inject",
        };

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Parses all top-level directives from <paramref name="source"/> and
        /// appends any validation diagnostics to <paramref name="diagnosticBag"/>.
        /// </summary>
        public static DirectiveSet Parse(
            string source,
            string filePath,
            List<ParseDiagnostic> diagnosticBag,
            bool useLastReturn = false
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

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string? ns = null;
            string? component = null;
            string? props = null;
            string? key = null;
            var usings = new List<string>();
            var injects = new List<(string Type, string Name)>();
            int nsLine = -1;
            int componentLine = -1;
            int componentNameColumn = -1;

            int i = 0;
            int line = 1;
            int markupStart = 0;
            int markupLine = 1;

            while (i < source.Length)
            {
                // ── Skip leading whitespace on the line (spaces/tabs only) ────
                int lineStart = i;
                while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                    i++;

                // ── Skip blank lines ─────────────────────────────────────────
                if (i < source.Length && IsNewline(source[i]))
                {
                    ConsumeNewline(source, ref i, ref line);
                    continue;
                }

                if (i >= source.Length)
                {
                    markupStart = i;
                    markupLine = line;
                    break;
                }

                // ── Must start with '@' to be a directive ────────────────────
                if (source[i] != '@')
                {
                    markupStart = lineStart;
                    markupLine = line;
                    break;
                }

                i++; // consume '@'

                // Read the keyword after '@'
                int keywordStart = i;
                while (i < source.Length && char.IsLetter(source[i]))
                    i++;
                string keyword = source
                    .Substring(keywordStart, i - keywordStart)
                    .ToLowerInvariant();

                // If this is NOT a recognised top-level directive keyword, treat
                // this line (starting at '@') as the beginning of the markup.
                if (!s_topLevelKeywords.Contains(keyword))
                {
                    markupStart = lineStart;
                    markupLine = line;
                    break;
                }

                // ── Skip whitespace between keyword and value ─────────────────
                while (i < source.Length && (source[i] == ' ' || source[i] == '\t'))
                    i++;

                // ── Read the value until end of line ──────────────────────────
                int valueStart = i;
                while (i < source.Length && !IsNewline(source[i]))
                    i++;
                string value = source.Substring(valueStart, i - valueStart).Trim();

                // ── Consume the newline ───────────────────────────────────────
                ConsumeNewline(source, ref i, ref line);

                // ── Store ─────────────────────────────────────────────────────
                switch (keyword)
                {
                    case "namespace":
                        ns = value;
                        nsLine = line;
                        break;
                    case "component":
                        component = value;
                        componentLine = line;
                        componentNameColumn = ColAtPos(source, valueStart);
                        break;
                    case "props":
                        props = value;
                        break;
                    case "key":
                        key = value.Trim('"');
                        break;
                    case "using":
                        if (!string.IsNullOrEmpty(value))
                            usings.Add(value);
                        break;
                    case "inject":
                        // Value is "TypeName fieldName" — split on the last whitespace
                        // so generic types like "IService<Foo> _svc" are handled.
                        if (!string.IsNullOrEmpty(value))
                        {
                            int lastSpace = value.LastIndexOfAny(new[] { ' ', '\t' });
                            if (lastSpace > 0)
                            {
                                string injType = value.Substring(0, lastSpace).Trim();
                                string injName = value.Substring(lastSpace + 1).Trim();
                                if (!string.IsNullOrEmpty(injType) && !string.IsNullOrEmpty(injName))
                                    injects.Add((injType, injName));
                            }
                        }
                        break;
                }
            }

            // If we consumed the entire file without finding markup
            if (i >= source.Length && markupStart == 0 && markupLine == 1)
            {
                markupStart = source.Length;
                markupLine = line;
            }

            if (LooksLikeFunctionStyleComponent(source, markupStart))
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2104",
                    Severity = ParseSeverity.Error,
                    SourceLine = markupLine,
                    Message = "Function-style form cannot be mixed with directive header form.",
                });
            }

            // ── Validate required directives ──────────────────────────────────
            string shortName = Path.GetFileName(filePath);

            if (ns == null)
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX0005",
                    Severity = ParseSeverity.Error,
                    SourceLine = 1,
                    Message = $"'{shortName}' is missing a required '@namespace' directive",
                });

            if (component == null)
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX0005",
                    Severity = ParseSeverity.Error,
                    SourceLine = 1,
                    Message = $"'{shortName}' is missing a required '@component' directive",
                });

            if (
                component != null
                && fileName != null
                && !string.Equals(component, fileName, StringComparison.Ordinal)
            )
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX0006",
                    Severity = ParseSeverity.Warning,
                    SourceLine = componentLine > 0 ? componentLine : 1,
                    Message =
                        $"@component '{component}' does not match the file name '{fileName}'. "
                        + $"The generated class will use '{component}'.",
                });
            }

            // UITKX0012 — @namespace must be declared before @component
            if (ns != null && component != null && nsLine > componentLine)
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX0012",
                    Severity = ParseSeverity.Error,
                    SourceLine = nsLine > 0 ? nsLine : 1,
                    Message = $"'@namespace' must be declared before '@component' in '{shortName}'",
                });
            }

            return new DirectiveSet(
                Namespace: ns,
                ComponentName: component,
                PropsTypeName: props,
                DefaultKey: key,
                Usings: usings.ToImmutableArray(),
                Injects: injects.ToImmutableArray(),
                MarkupStartLine: markupLine,
                MarkupStartIndex: markupStart,
                MarkupEndIndex: -1,
                IsFunctionStyle: false,
                FunctionSetupCode: null,
                FunctionSetupStartLine: -1,
                ComponentDeclarationLine: componentLine > 0 ? componentLine : 1,
                ComponentNameColumn: componentNameColumn
            );
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool TryParseFunctionStyle(
            string source,
            string filePath,
            List<ParseDiagnostic> diagnosticBag,
            out DirectiveSet directiveSet,
            bool useLastReturn = false
        )
        {
            directiveSet = default!;

            if (source.Length > 0 && source[0] == '\uFEFF')
                source = source.Substring(1);

            int i = 0;
            int line = 1;
            SkipLeadingFunctionStyleTrivia(source, ref i, ref line);

            // Parse optional leading `using X.Y.Z;` lines AND an optional
            // `@namespace X.Y` directive before the component keyword, in any order.
            var usings = new List<string>();
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
                if (inlineNamespace == null
                    && TryReadFunctionStyleNamespaceDirective(source, ref i, ref line, out string? parsedNs))
                {
                    inlineNamespace = parsedNs;
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    parsedPreambleLine = true;
                }
            } while (parsedPreambleLine);

            if (!TryReadKeyword(source, ref i, "component"))
                return false;

            int componentLine = line;

            SkipSpaces(source, ref i);
            int nameStartI = i; // column anchor — position of first char of component name
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

            // ── Optional typed-props parameter list ───────────────────────────
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
                directiveSet = new DirectiveSet(
                    Namespace: functionNamespace,
                    ComponentName: componentName,
                    PropsTypeName: functionPropsTypeName,
                    DefaultKey: null,
                    Usings: usings.ToImmutableArray(),
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
                    SetupCodeMarkupRanges: FindJsxBlockRanges(source, bodyStart, bodyEndExclusive)
                );
                return true;
            }

            int secondReturn = FindTopLevelReturnAfter(
                source,
                returnStmtEndExclusive,
                bodyEndExclusive
            );
            if (secondReturn >= 0)
            {
                diagnosticBag.Add(new ParseDiagnostic
                {
                    Code = "UITKX2103",
                    Severity = ParseSeverity.Warning,
                    SourceLine = LineAtPos(source, secondReturn),
                    Message = "Multiple top-level returns are not allowed in function-style components.",
                });
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
            directiveSet = new DirectiveSet(
                Namespace: functionNamespace,
                ComponentName: componentName,
                PropsTypeName: functionPropsTypeName,
                DefaultKey: null,
                Usings: usings.ToImmutableArray(),
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
                SetupCodeMarkupRanges: FindJsxBlockRanges(
                    source,
                    bodyStart,       returnStart,
                    returnStmtEndExclusive, bodyEndExclusive),
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

        // ── Function param-list parser ────────────────────────────────────────

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

                result.Add(new FunctionParam(typeName, paramName, defaultValue));

                SkipSpaces(source, ref i);
                if (i < source.Length && source[i] == ',')
                    i++; // consume comma, loop continues
            }

            return result.ToImmutable();
        }

        /// <summary>
        /// Reads a C# type name, including optional generic type arguments
        /// (balanced &lt; … &gt; pairs), arrays (<c>[]</c>), and nullable markers
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

                return source[i] == '<' || source[i] == '@';
            }

            return false;
        }

        private static bool LooksLikeFunctionStyleComponent(string source, int start)
        {
            int i = start;
            int line = 1;
            SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
            // Skip any leading `using X.Y.Z;` and `@namespace X.Y` lines, in any order.
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
                if (TryReadFunctionStyleNamespaceDirective(source, ref i, ref line, out _))
                {
                    SkipLeadingFunctionStyleTrivia(source, ref i, ref line);
                    skippedSomething = true;
                }
            } while (skippedSomething);
            return TryReadKeywordAt(source, i, "component");
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

            // Allow leading spaces/tabs — newlines are already consumed by trivia before each call.
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
            bool useLastReturn = false
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

        // ── JSX block range finder ────────────────────────────────────────────

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
        private static ImmutableArray<(int Start, int End, int Line)> FindJsxBlockRanges(
            string source, int rangeStart, int rangeEnd)
        {
            var result = ImmutableArray.CreateBuilder<(int, int, int)>();
            int i = rangeStart;
            while (i < rangeEnd)
            {
                if (source[i] != '(')
                {
                    i++;
                    continue;
                }

                // Peek past whitespace / newlines to see if the next token is '<'
                int peek = i + 1;
                while (peek < rangeEnd &&
                       (source[peek] == ' '  || source[peek] == '\t' ||
                        source[peek] == '\r' || source[peek] == '\n'))
                    peek++;

                if (peek >= rangeEnd || source[peek] != '<')
                {
                    i++;
                    continue;
                }

                // Balance parens to find the matching ')'
                int depth = 1;
                int j     = i + 1;
                while (j < rangeEnd && depth > 0)
                {
                    if      (source[j] == '(') depth++;
                    else if (source[j] == ')') depth--;
                    j++;
                }

                if (depth == 0)
                {
                    int blockStart = i + 1;   // content starts after '('
                    int blockEnd   = j - 1;   // exclusive — at the ')'
                    int blockLine  = LineAtPos(source, blockStart);
                    result.Add((blockStart, blockEnd, blockLine));
                    i = j; // hop past the entire block
                }
                else
                {
                    i++; // unbalanced — skip
                }
            }
            return result.ToImmutable();
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
    }
}
