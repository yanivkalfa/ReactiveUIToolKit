using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Roslyn in-process compilation (reflection handles)
// Uses Microsoft.CodeAnalysis.CSharp 4.3.1 netstandard2.0 loaded at runtime

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Compiles a .uitkx file in-process:
    ///   1. Parse via ReactiveUITK.Language.dll (loaded at runtime, Roslyn-free)
    ///   2. Emit C# via built-in HMR emitter
    ///   3. Compile via in-process Roslyn (fast) or external csc.dll (fallback)
    ///   4. Load via Assembly.Load
    /// </summary>
    internal sealed class UitkxHmrCompiler : IDisposable
    {
        // ── File-read retry policy ───────────────────────────────────────────
        // When a .uitkx (or .cs) save fires the FileSystemWatcher, the
        // originating editor often still holds the file with an exclusive
        // write lock for a few milliseconds. A naive File.ReadAllText then
        // throws IOException ("Sharing violation"), the compile is aborted
        // and the user's edit silently never reaches Roslyn — visible in
        // logs as repeated edits to a child component having zero effect
        // while edits to the parent (whose save happens to land after the
        // lock release) work fine.
        //
        // ReadTextWithRetry opens with FileShare.ReadWrite|Delete (so we
        // cooperate with the editor instead of fighting it) and retries
        // with a short exponential-ish backoff on IOException /
        // UnauthorizedAccessException. Total worst-case wait ~480ms which
        // is well under the FileWatcher debounce window.
        private const int FileReadMaxAttempts = 8;

        internal static string ReadTextWithRetry(string path)
        {
            int delayMs = 5;
            IOException lastIo = null;
            UnauthorizedAccessException lastUa = null;
            for (int attempt = 1; attempt <= FileReadMaxAttempts; attempt++)
            {
                try
                {
                    using (
                        var fs = new FileStream(
                            path,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.ReadWrite | FileShare.Delete,
                            bufferSize: 4096,
                            useAsync: false
                        )
                    )
                    using (var sr = new StreamReader(fs, detectEncodingFromByteOrderMarks: true))
                    {
                        var text = sr.ReadToEnd();
                        if (attempt > 1)
                        {
                            Debug.Log(
                                $"[HMR] ReadTextWithRetry: succeeded on attempt {attempt}/{FileReadMaxAttempts} for '{path}' (editor lock cleared)."
                            );
                        }
                        return text;
                    }
                }
                catch (IOException ex)
                {
                    lastIo = ex;
                }
                catch (UnauthorizedAccessException ex)
                {
                    lastUa = ex;
                }
                System.Threading.Thread.Sleep(delayMs);
                if (delayMs < 120)
                    delayMs *= 2;
            }
            // Out of retries — surface the original exception so the caller's
            // try/catch path (HandleCompileFailure → re-queue) still runs.
            Debug.LogWarning(
                $"[HMR] ReadTextWithRetry: gave up after {FileReadMaxAttempts} attempts for '{path}'. Editor lock did not release in time; compile will be re-queued."
            );
            if (lastIo != null)
                throw lastIo;
            if (lastUa != null)
                throw lastUa;
            // Defensive — both null shouldn't be reachable but keep the
            // contract: caller always gets a string or an exception.
            throw new IOException($"ReadTextWithRetry exhausted attempts for '{path}'.");
        }

        // ── Loaded pipeline assembly ──────────────────────────────────────────
        private Assembly _languageAsm;

        // ── Cached reflection handles ─────────────────────────────────────────
        private MethodInfo _directiveParse;
        private MethodInfo _uitkxParse;
        private MethodInfo _canonicalLower;
        private MethodInfo _parseFragment; // optional — see H-04
        private MethodInfo _findJsxBlockRanges;
        private MethodInfo _findBareJsxRanges;
        private MethodInfo _findLhsStartForLogicalAnd;
        private Type _parseDiagnosticType;

        // ── Reference cache (built once per session) ──────────────────────────
        private List<string> _referenceLocations;
        private int _swapCounter;
        private string _dotnetPath;
        private string _cscPath;
        private string _tempDir;

        // ── In-process Roslyn compilation ─────────────────────────────────────
        private bool _roslynLoaded;
        private Assembly _roslynCSharpAsm;
        private Assembly _roslynCommonAsm;

        // Cached reflection: CSharpSyntaxTree.ParseText(string, CSharpParseOptions)
        private MethodInfo _parseText;

        // Cached reflection: CSharpCompilation.Create(string, IEnumerable<SyntaxTree>, IEnumerable<MetadataReference>, CSharpCompilationOptions)
        private MethodInfo _compilationCreate;

        // Cached: CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, ...)
        private object _compilationOptions;

        // Cached: CSharpParseOptions(languageVersion: Latest)
        private object _parseOptions;

        // Cached: MetadataReference[] built from _referenceLocations + cross-refs
        private object[] _metadataReferences;

        // Cached: MethodInfo for MetadataReference.CreateFromFile(string)
        private MethodInfo _createFromFile;

        // Cached: MethodInfo for CSharpCompilation.Emit(Stream)
        private MethodInfo _emitToStream;

        // Cached: incremental compilation API handles
        private MethodInfo _compilationRemoveSyntaxTrees;
        private MethodInfo _compilationAddSyntaxTrees;
        private MethodInfo _compilationAddReferences;
        private MethodInfo _compilationWithAssemblyName;

        // ── Incremental compilation cache ─────────────────────────────────
        // Cached Compilation per component for incremental reuse
        private readonly Dictionary<string, object> _cachedCompilations = new(
            StringComparer.OrdinalIgnoreCase
        );

        // Cached SyntaxTree[] per component (to know what to remove)
        private readonly Dictionary<string, object[]> _cachedSyntaxTrees = new(
            StringComparer.OrdinalIgnoreCase
        );

        // Track genuinely-new component count to detect cross-ref changes
        private int _lastGenuineComponentCount;

        // ── Cross-ref MetadataReference cache ─────────────────────────────
        // Cached MetadataReference per cross-component DLL path
        private readonly Dictionary<string, object> _crossRefCache = new(
            StringComparer.OrdinalIgnoreCase
        );

        // ── Per-asmdef reference filtering ────────────────────────────────
        // _referenceLocations is built once from AppDomain.GetAssemblies() and
        // therefore contains EVERY loaded assembly. Handing that whole set to
        // Roslyn for an HMR compile of a file owned by asmdef X causes CS0433
        // duplicate-type errors whenever the project legitimately defines the
        // same type name in two non-cross-referencing asmdefs (e.g. a user
        // component `AppButton` in Assembly-CSharp and a sample `AppButton`
        // in ReactiveUITK.Samples — Unity's normal compile never sees both
        // because Assembly-CSharp.csproj does not reference Samples). The
        // fix mirrors Unity's per-asmdef reference closure via
        // CompilationPipeline.GetAssemblies(...).allReferences. Caches are
        // populated lazily on first use per asmdef and cleared in Reset().
        private readonly Dictionary<string, HashSet<string>> _allowedRefsByAsmdef = new(
            StringComparer.Ordinal
        );
        private readonly Dictionary<string, object[]> _filteredMetaRefsByAsmdef = new(
            StringComparer.Ordinal
        );
        private readonly Dictionary<string, List<string>> _filteredRefLocsByAsmdef = new(
            StringComparer.Ordinal
        );

        // Map of loaded Roslyn dependency DLLs for AssemblyResolve
        private readonly Dictionary<string, Assembly> _roslynDeps = new(
            StringComparer.OrdinalIgnoreCase
        );

        // ── HMR assembly registry (component → DLL path for cross-references) ─
        private readonly Dictionary<string, string> _hmrAssemblyPaths = new(
            StringComparer.OrdinalIgnoreCase
        );

        // Components that are genuinely new (not in any pre-existing assembly).
        // Only these need cross-references; existing components would cause CS0433.
        private readonly HashSet<string> _genuinelyNewComponents = new(
            StringComparer.OrdinalIgnoreCase
        );

        private bool _initialized;
        private string _initError;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Paths of all HMR-compiled assemblies currently on disk, keyed by component name.
        /// Used by the controller to know when new components become available.
        /// </summary>
        public IReadOnlyDictionary<string, string> HmrAssemblyPaths => _hmrAssemblyPaths;

        public bool TryInitialize(out string error)
        {
            if (_initialized)
            {
                error = _initError;
                return _initError == null;
            }
            _initialized = true;

            try
            {
                LoadLanguageDll();
                CacheReflectionHandles();
                FindCompilerPaths();
                BuildReferenceList();
                _tempDir = Path.Combine(Path.GetTempPath(), "UitkxHmr");
                Directory.CreateDirectory(_tempDir);

                // Clean up stale DLLs from previous sessions (may have been
                // locked by LoadFrom and couldn't be deleted on Dispose)
                CleanStaleTempFiles();

                // Try to load Roslyn for in-process compilation (non-fatal)
                TryLoadRoslyn();

                error = null;
                _initError = null;
                return true;
            }
            catch (Exception ex)
            {
                error = $"[HMR] Compiler init failed: {ex.Message}";
                _initError = error;
                Debug.LogError(error);
                return false;
            }
        }

        /// <summary>
        /// Compile a .uitkx file and return the loaded assembly + component name.
        /// </summary>
        public HmrCompileResult Compile(string uitkxPath, string[] companionCsFiles = null)
        {
            var result = new HmrCompileResult();
            var sw = Stopwatch.StartNew();

            try
            {
                string source = ReadTextWithRetry(uitkxPath);

                // ── 1. Parse directives ──────────────────────────────────────
                var stepSw = Stopwatch.StartNew();
                var diagList = CreateDiagnosticList();
                var directives = InvokeWithDefaults(
                    _directiveParse,
                    null,
                    source,
                    uitkxPath,
                    diagList,
                    true
                );

                if (directives == null)
                {
                    result.Error = "DirectiveParser returned null";
                    return result;
                }

                string componentName = (string)GetProp(directives, "ComponentName");
                string ns = (string)GetProp(directives, "Namespace");
                result.Namespace = ns ?? string.Empty;

                if (string.IsNullOrEmpty(componentName))
                {
                    // ── Hook/module file path ────────────────────────────────
                    return CompileHookModuleFile(directives, diagList, uitkxPath, sw, result);
                }

                result.ComponentName = componentName;

                // ── 2. Parse AST ─────────────────────────────────────────────
                // All 6 args explicit: source, filePath, directives, diagnostics,
                // validateSingleRoot=false, lineOffset=0. Passing 0 explicitly
                // (instead of letting InvokeWithDefaults pad it) silences the
                // drift warning while preserving the correct freestanding-file
                // semantics. If UitkxParser.Parse ever gains a 7th param, the
                // drift warning fires loud-and-clear so HMR can be updated.
                var astNodes = InvokeWithDefaults(
                    _uitkxParse,
                    null,
                    source,
                    uitkxPath,
                    directives,
                    diagList,
                    false,
                    0
                );
                stepSw.Stop();
                result.ParseMs = stepSw.Elapsed.TotalMilliseconds;

                // H-01: never emit from an error-recovered AST — see TryGetParseErrorMessage.
                if (TryGetParseErrorMessage(diagList, uitkxPath, out string parseErrorMsg))
                {
                    result.Error = parseErrorMsg;
                    return result;
                }

                // ── 3. Canonical lowering + Emit C# ──────────────────────────
                stepSw = Stopwatch.StartNew();
                var lowered = InvokeWithDefaults(
                    _canonicalLower,
                    null,
                    directives,
                    astNodes,
                    uitkxPath
                );

                // Build a delegate that can parse standalone JSX fragments.
                // Used by the emitter to splice embedded JSX in setup code.
                HmrCSharpEmitter.MarkupParseFunc parseMarkup = (jsxText, path, startLine) =>
                    ParseMarkupFragment(jsxText, path, startLine);

                // Phase 1: scanner delegates so the emitter can detect JSX
                // literals embedded inside arbitrary C# expressions
                // (ternaries, lambdas, attribute values, etc.). When the
                // language-lib build pre-dates Phase 1 these resolve to null
                // and the emitter falls back to non-splicing behavior.
                HmrCSharpEmitter.FindJsxRangesFunc findJsxBlockRanges =
                    _findJsxBlockRanges == null
                        ? null
                        : (src, s, e) =>
                            (System.Collections.IEnumerable)
                                _findJsxBlockRanges.Invoke(null, new object[] { src, s, e });
                HmrCSharpEmitter.FindJsxRangesFunc findBareJsxRanges =
                    _findBareJsxRanges == null
                        ? null
                        : (src, s, e) =>
                            (System.Collections.IEnumerable)
                                _findBareJsxRanges.Invoke(null, new object[] { src, s, e });

                // Phase 1.5: LHS walker for `cond && <Tag/>` desugar. Same
                // null-fallback pattern as the range scanners above.
                HmrCSharpEmitter.FindLhsStartFunc findLhsStartForLogicalAnd =
                    _findLhsStartForLogicalAnd == null
                        ? null
                        : (src, ss, ae) =>
                            (int)
                                _findLhsStartForLogicalAnd.Invoke(
                                    null,
                                    new object[] { src, ss, ae }
                                );

                string csharp = HmrCSharpEmitter.Emit(
                    directives,
                    lowered,
                    uitkxPath,
                    parseMarkup,
                    findJsxBlockRanges,
                    findBareJsxRanges,
                    findLhsStartForLogicalAnd
                );
                stepSw.Stop();
                result.EmitMs = stepSw.Elapsed.TotalMilliseconds;

                // ── 4. Compile ───────────────────────────────────────────────
                stepSw = Stopwatch.StartNew();
                var sources = new List<string> { csharp };

                // Include companion .uitkx files (.style, .hooks, .utils) as
                // partial-class sources so module/hook members are available
                // to the component in the same compilation unit.
                // Also collects hook container class names so we can inject
                // `using static Ns.XxxHooks;` into the component source.
                var hookContainerFqns = new List<string>();
                EmitCompanionUitkxSources(uitkxPath, componentName, ns, sources, hookContainerFqns);

                // Inject using-static for peer hook containers so the component
                // can call hook methods (e.g. useXxx()) without qualification —
                // mirrors SG's Stage 3d peer-hook-container injection.
                if (hookContainerFqns.Count > 0)
                {
                    var usingLines = new System.Text.StringBuilder();
                    foreach (var fqn in hookContainerFqns)
                        usingLines.AppendLine($"using static {fqn};");
                    sources[0] = usingLines.ToString() + sources[0];
                }

                if (companionCsFiles != null)
                {
                    foreach (var csFile in companionCsFiles)
                    {
                        if (File.Exists(csFile))
                            sources.Add(ReadTextWithRetry(csFile));
                    }
                }

                // ── Rank 2: pick up new .cs files anywhere in the asmdef ─────
                // When the user adds a fresh helper .cs (not in the same folder
                // as the .uitkx) and references it before Unity recompiles, the
                // helper is invisible to HMR by default. NewCsFileDiscovery
                // scans the asmdef for .cs files newer than the project DLL's
                // mtime whose primary type-name is not yet in AppDomain, and
                // adds them as additional source trees. Type-name dedupe avoids
                // CS0101 against the project DLL.
                TryIncludeNewAsmdefCsFiles(uitkxPath, companionCsFiles, sources);

                var asm = CompileSources(sources.ToArray(), componentName, uitkxPath, out string compileError);
                stepSw.Stop();
                result.CompileMs = stepSw.Elapsed.TotalMilliseconds;

                if (asm == null)
                {
                    result.Error = compileError;
                    return result;
                }

                result.LoadedAssembly = asm;
                result.Success = true;

                // Track whether this component is genuinely new (first-time check only).
                // Existing components (already in a project assembly) must NOT be added
                // as cross-references or they cause CS0433 duplicate-type errors.
                // FQN match (component-namespace + name) — see Issue 8 in the bug plan.
                if (!_genuinelyNewComponents.Contains(componentName))
                    CheckIfGenuinelyNew(componentName, ns);
            }
            catch (Exception ex)
            {
                result.IsInfrastructureError = IsInfrastructureException(ex);
                result.Error = ex is TargetInvocationException tie
                    ? $"{tie.InnerException?.GetType().Name}: {tie.InnerException?.Message ?? ex.Message}\n{tie.InnerException?.StackTrace}"
                    : $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            }
            finally
            {
                sw.Stop();
                result.TotalMs = sw.Elapsed.TotalMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// Per-file build artifacts collected by <see cref="BuildComponentArtifacts"/>.
        /// </summary>
        private sealed class ComponentBuildArtifacts
        {
            public string UitkxPath;
            public string ComponentName;
            public string Namespace;
            public string FullyQualifiedName;
            public string EmittedComponentSource;
            public List<string> CompanionUitkxSources = new();
            public List<string> CompanionCsSources = new();
            public List<string> HookContainerFqns = new();
            public HashSet<string> CompanionUitkxPathsConsumed = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );
            public double ParseMs;
            public double EmitMs;
            public string Error;
        }

        /// <summary>
        /// Parse + lower + emit a single component .uitkx file into a build
        /// artifact bundle. Mirrors the per-file emit work that <see cref="Compile"/>
        /// inlines, factored out so the union-compile path can reuse it.
        /// Returns null on parse failure with <c>error</c> populated.
        ///
        /// Hook/module-only files (no <c>component</c> keyword) are out of scope
        /// — callers must route them through <see cref="CompileHookModuleFile"/>.
        /// </summary>
        private ComponentBuildArtifacts BuildComponentArtifacts(
            string uitkxPath,
            string[] companionCsFiles,
            out string error
        )
        {
            error = null;
            var artifacts = new ComponentBuildArtifacts { UitkxPath = uitkxPath };

            try
            {
                string source = ReadTextWithRetry(uitkxPath);
                var stepSw = Stopwatch.StartNew();

                var diagList = CreateDiagnosticList();
                var directives = InvokeWithDefaults(
                    _directiveParse,
                    null,
                    source,
                    uitkxPath,
                    diagList,
                    true
                );
                if (directives == null)
                {
                    error = "DirectiveParser returned null";
                    return null;
                }

                string componentName = (string)GetProp(directives, "ComponentName");
                string ns = (string)GetProp(directives, "Namespace");

                if (string.IsNullOrEmpty(componentName))
                {
                    // Hook/module-only file — caller routes these separately.
                    error = "Hook/module file not eligible for union compile";
                    return null;
                }

                artifacts.ComponentName = componentName;
                artifacts.Namespace = ns ?? string.Empty;
                artifacts.FullyQualifiedName = string.IsNullOrEmpty(ns)
                    ? componentName
                    : ns + "." + componentName;

                var astNodes = InvokeWithDefaults(
                    _uitkxParse,
                    null,
                    source,
                    uitkxPath,
                    directives,
                    diagList,
                    false,
                    0
                );
                stepSw.Stop();
                artifacts.ParseMs = stepSw.Elapsed.TotalMilliseconds;

                // H-01: never emit from an error-recovered AST — see TryGetParseErrorMessage.
                if (TryGetParseErrorMessage(diagList, uitkxPath, out string parseErrorMsg))
                {
                    error = parseErrorMsg;
                    return null;
                }

                stepSw = Stopwatch.StartNew();
                var lowered = InvokeWithDefaults(
                    _canonicalLower,
                    null,
                    directives,
                    astNodes,
                    uitkxPath
                );

                // Build the parse delegates that the emitter needs for JSX
                // splice handling. Identical to the per-file path; the
                // delegates are pure functions over text + the language-lib
                // reflection handles, so sharing them across batch members is
                // safe (no per-file state inside).
                HmrCSharpEmitter.MarkupParseFunc parseMarkup = (jsxText, path, startLine) =>
                    ParseMarkupFragment(jsxText, path, startLine);
                HmrCSharpEmitter.FindJsxRangesFunc findJsxBlockRanges =
                    _findJsxBlockRanges == null
                        ? null
                        : (src, s, e) =>
                            (System.Collections.IEnumerable)
                                _findJsxBlockRanges.Invoke(null, new object[] { src, s, e });
                HmrCSharpEmitter.FindJsxRangesFunc findBareJsxRanges =
                    _findBareJsxRanges == null
                        ? null
                        : (src, s, e) =>
                            (System.Collections.IEnumerable)
                                _findBareJsxRanges.Invoke(null, new object[] { src, s, e });
                HmrCSharpEmitter.FindLhsStartFunc findLhsStartForLogicalAnd =
                    _findLhsStartForLogicalAnd == null
                        ? null
                        : (src, ss, ae) =>
                            (int)
                                _findLhsStartForLogicalAnd.Invoke(
                                    null,
                                    new object[] { src, ss, ae }
                                );

                string csharp = HmrCSharpEmitter.Emit(
                    directives,
                    lowered,
                    uitkxPath,
                    parseMarkup,
                    findJsxBlockRanges,
                    findBareJsxRanges,
                    findLhsStartForLogicalAnd
                );

                // Companion .uitkx pickup. EmitCompanionUitkxSources writes
                // into the (sources, fqns) lists; we tee into our per-file
                // bundle and remember which companion paths we consumed so
                // the batch can dedupe shared style/hook files across members.
                var companionInline = new List<string>();
                EmitCompanionUitkxSources(
                    uitkxPath,
                    componentName,
                    ns,
                    companionInline,
                    artifacts.HookContainerFqns
                );

                // Track which companion paths were inlined. The companion file
                // discovery is path-based (same dir, prefix match) so the same
                // path appears once per parent component file.
                var compDir = Path.GetDirectoryName(uitkxPath);
                if (compDir != null)
                {
                    string prefix = componentName + ".";
                    foreach (var file in Directory.GetFiles(compDir, prefix + "*.uitkx"))
                    {
                        if (!string.Equals(file, uitkxPath, StringComparison.OrdinalIgnoreCase))
                            artifacts.CompanionUitkxPathsConsumed.Add(Path.GetFullPath(file));
                    }
                }
                artifacts.CompanionUitkxSources = companionInline;

                // Inject the same `using static` lines that the per-file path
                // prepends, but as a separate header source so we can leave
                // the main C# string untouched. Empty when no peer hooks.
                if (artifacts.HookContainerFqns.Count > 0)
                {
                    var usingLines = new System.Text.StringBuilder();
                    foreach (var fqn in artifacts.HookContainerFqns)
                        usingLines.AppendLine($"using static {fqn};");
                    csharp = usingLines.ToString() + csharp;
                }

                artifacts.EmittedComponentSource = csharp;

                if (companionCsFiles != null)
                {
                    foreach (var csFile in companionCsFiles)
                    {
                        if (File.Exists(csFile))
                            artifacts.CompanionCsSources.Add(ReadTextWithRetry(csFile));
                    }
                }

                stepSw.Stop();
                artifacts.EmitMs = stepSw.Elapsed.TotalMilliseconds;
                return artifacts;
            }
            catch (Exception ex)
            {
                error = ex is TargetInvocationException tie
                    ? $"{tie.InnerException?.GetType().Name}: {tie.InnerException?.Message ?? ex.Message}"
                    : $"{ex.GetType().Name}: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Public entry point for the union compile. Caller is expected to
        /// provide already-resolved component .uitkx files (companion / hook
        /// files routed individually). When the batch is empty or contains a
        /// single eligible file, falls through to <see cref="Compile"/> to
        /// preserve the well-tested single-file path.
        ///
        /// Failure semantics: on guard or compile failure the returned result
        /// has <see cref="HmrBatchCompileResult.OverallSuccess"/> = false and
        /// <see cref="HmrBatchCompileResult.FallbackReason"/> populated. The
        /// controller MUST then invoke <see cref="Compile"/> per file so the
        /// user-facing error surface (CS0117 etc.) is not swallowed by the
        /// union path. See §5.2.1 of TECH_DEBT_20_21_22_RESOLUTION_PLAN.md.
        /// </summary>
        public HmrBatchCompileResult CompileBatch(
            IReadOnlyList<string> uitkxPaths,
            IReadOnlyDictionary<string, string[]> companionCsByPath = null
        )
        {
            var result = new HmrBatchCompileResult { BatchSize = uitkxPaths?.Count ?? 0 };

            if (uitkxPaths == null || uitkxPaths.Count == 0)
            {
                result.OverallError = "Empty batch";
                return result;
            }

            // Single-file batch → reuse the existing well-tested path. No
            // union compile is needed; caller still gets a HmrBatchCompileResult
            // back with one PerFileResults entry so the controller's swap loop
            // works uniformly.
            if (uitkxPaths.Count == 1)
            {
                string only = uitkxPaths[0];
                string[] companions =
                    companionCsByPath != null && companionCsByPath.TryGetValue(only, out var c)
                        ? c
                        : null;
                var single = Compile(only, companions);
                result.PerFileResults.Add(single);
                result.OverallSuccess = single.Success;
                result.OverallError = single.Error;
                result.UnionAssembly = single.LoadedAssembly;
                result.TotalMs = single.TotalMs;
                return result;
            }

            var sw = Stopwatch.StartNew();

            try
            {
                // ── 1. Per-file build (parse + lower + emit) ───────────────
                var artifactsByPath = new Dictionary<string, ComponentBuildArtifacts>(
                    StringComparer.OrdinalIgnoreCase
                );
                foreach (var path in uitkxPaths)
                {
                    var art = BuildComponentArtifacts(
                        path,
                        companionCsByPath != null && companionCsByPath.TryGetValue(path, out var c)
                            ? c
                            : null,
                        out string artErr
                    );
                    if (art == null)
                    {
                        // One file can't be union-compiled (parse error or
                        // hook/module-only). Bail to per-file. Don't try to
                        // partially batch — the cascade semantics expect all
                        // files to participate.
                        result.FallbackReason =
                            $"BuildComponentArtifacts failed for {Path.GetFileName(path)}: {artErr}";
                        result.OverallError = result.FallbackReason;
                        return result;
                    }
                    artifactsByPath[path] = art;
                }

                // ── 2. Pre-compile guard: unique (Namespace, ComponentName) ──
                string guardError = ValidateBatchUniqueness(artifactsByPath.Values);
                if (guardError != null)
                {
                    result.FallbackReason = $"Pre-compile guard failed: {guardError}";
                    result.OverallError = result.FallbackReason;
                    return result;
                }

                // ── 3. Aggregate sources (with companion dedupe) ───────────
                var allSources = new List<string>();
                var consumedCompanionPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var consumedCompanionCsTexts = new HashSet<string>(StringComparer.Ordinal);

                foreach (var art in artifactsByPath.Values)
                {
                    allSources.Add(art.EmittedComponentSource);

                    // Dedupe shared companion .uitkx emissions (e.g. a
                    // shared theme module across two components in the
                    // batch). We track BY PATH on the parent companion
                    // file rather than by emitted text because the emit
                    // is path-bound (file headers, line directives).
                    if (
                        art.CompanionUitkxSources.Count > 0
                        && art.CompanionUitkxPathsConsumed.Count > 0
                    )
                    {
                        var newOnes = new List<string>(art.CompanionUitkxSources.Count);
                        int idx = 0;
                        foreach (var compPath in art.CompanionUitkxPathsConsumed)
                        {
                            if (idx >= art.CompanionUitkxSources.Count)
                                break;
                            if (consumedCompanionPaths.Add(compPath))
                                newOnes.Add(art.CompanionUitkxSources[idx]);
                            idx++;
                        }
                        // Defensive fallback: if the path/source order
                        // assumption above is off, fall back to text dedupe.
                        if (
                            newOnes.Count == 0
                            && art.CompanionUitkxPathsConsumed.Count
                                < art.CompanionUitkxSources.Count
                        )
                        {
                            foreach (var s in art.CompanionUitkxSources)
                                if (consumedCompanionCsTexts.Add(s))
                                    allSources.Add(s);
                        }
                        else
                        {
                            allSources.AddRange(newOnes);
                        }
                    }

                    // Companion .cs sources — dedupe by raw text (file path
                    // isn't tracked through to here, but identical companion
                    // files would produce identical reads anyway).
                    foreach (var cs in art.CompanionCsSources)
                        if (consumedCompanionCsTexts.Add(cs))
                            allSources.Add(cs);
                }

                // ── 4. Rank 2 — new asmdef .cs pickup (once per batch) ────
                // The batch is asmdef-scoped by construction (controller
                // cascades within an asmdef). Run NewCsFileDiscovery once
                // against the first member's asmdef and add the union of new
                // .cs files. Same best-effort wrapping as single-file path.
                TryIncludeNewAsmdefCsFiles(
                    uitkxPaths[0],
                    /* alreadyIncluded */null,
                    allSources
                );

                // ── 5. Roslyn compile ──────────────────────────────────────
                // Use a deterministic batch key derived from the LAST file in
                // the batch (which is the "root" save per the cascade walker's
                // dependents-first / root-last ordering).
                string rootBase = Path.GetFileNameWithoutExtension(
                    uitkxPaths[uitkxPaths.Count - 1]
                );
                string batchKey = $"batch_{rootBase}_{uitkxPaths.Count}";

                // Invalidate cached compilations for every batch member —
                // the next per-file compile must rebuild from scratch since
                // the cached SyntaxTrees belong to a different compile shape.
                foreach (var art in artifactsByPath.Values)
                {
                    _cachedCompilations.Remove(art.ComponentName);
                    _cachedSyntaxTrees.Remove(art.ComponentName);
                }

                var compileSw = Stopwatch.StartNew();
                var asm = CompileSources(allSources.ToArray(), batchKey, uitkxPaths[0], out string compileError);
                compileSw.Stop();
                double compileMs = compileSw.Elapsed.TotalMilliseconds;

                if (asm == null)
                {
                    result.OverallError =
                        $"[HMR] Union compile failed ({uitkxPaths.Count} files): {compileError}";
                    result.FallbackReason =
                        "Roslyn errors in union compile — falling back to per-file";
                    return result;
                }

                // ── 6. Post-compile guard: assembly identity ───────────────
                string postGuardError = ValidateBatchAssemblyIdentity(asm, artifactsByPath.Values);
                if (postGuardError != null)
                {
                    result.UnionAssembly = asm;
                    result.OverallError = postGuardError;
                    result.FallbackReason =
                        $"Post-compile assembly-identity guard failed: {postGuardError}";
                    Debug.LogWarning(
                        "[HMR] union-compile sanity check failed — falling back. " + postGuardError
                    );
                    return result;
                }

                // ── 7. Build per-file results ──────────────────────────────
                foreach (var path in uitkxPaths)
                {
                    var art = artifactsByPath[path];
                    var perFile = new HmrCompileResult
                    {
                        Success = true,
                        ComponentName = art.ComponentName,
                        Namespace = art.Namespace ?? string.Empty,
                        LoadedAssembly = asm,
                        ParseMs = art.ParseMs,
                        EmitMs = art.EmitMs,
                        CompileMs = compileMs / uitkxPaths.Count,
                        TotalMs = (sw.ElapsedMilliseconds * 1.0) / uitkxPaths.Count,
                    };
                    result.PerFileResults.Add(perFile);

                    // Register the union DLL under every batch component's
                    // name so future single-file cross-refs see it.
                    if (
                        _hmrAssemblyPaths.TryGetValue(art.ComponentName, out var oldDll)
                        && !string.Equals(oldDll, asm.Location, StringComparison.OrdinalIgnoreCase)
                        && File.Exists(oldDll)
                    )
                    {
                        try
                        {
                            File.Delete(oldDll);
                        }
                        catch
                        { /* may be locked */
                        }
                    }
                    _hmrAssemblyPaths[art.ComponentName] = asm.Location;

                    if (!_genuinelyNewComponents.Contains(art.ComponentName))
                        CheckIfGenuinelyNew(art.ComponentName, art.Namespace);
                }

                result.UnionAssembly = asm;
                result.OverallSuccess = true;

                Debug.Log($"[HMR] union: {uitkxPaths.Count} files, {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                result.OverallError = ex is TargetInvocationException tie
                    ? $"{tie.InnerException?.GetType().Name}: {tie.InnerException?.Message ?? ex.Message}"
                    : $"{ex.GetType().Name}: {ex.Message}";
                result.FallbackReason = "Union compile threw — falling back to per-file";
            }
            finally
            {
                sw.Stop();
                result.TotalMs = sw.Elapsed.TotalMilliseconds;
            }

            return result;
        }

        // ── Rank 5 — pre/post-compile guards ─────────────────────────────────

        /// <summary>
        /// Pre-compile guard: every component in the batch must own a unique
        /// (Namespace, ComponentName) tuple. Two components colliding on the
        /// FQN inside one Roslyn compile would surface as <c>CS0260</c> or
        /// <c>CS0101</c> at emit time, but bailing here gives the controller a
        /// clean signal to fall back per-file with a useful diagnostic. Pure
        /// static method — unit-testable without Roslyn/Unity dependencies.
        /// </summary>
        internal static string ValidateBatchUniquenessImpl(
            IEnumerable<(string Namespace, string ComponentName, string FilePath)> components
        )
        {
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (ns, name, path) in components)
            {
                string fqn = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
                if (seen.TryGetValue(fqn, out var firstPath))
                {
                    return $"Two batch members emit type '{fqn}': "
                        + $"'{Path.GetFileName(firstPath)}' and "
                        + $"'{Path.GetFileName(path)}'. Cannot union-compile.";
                }
                seen[fqn] = path;
            }
            return null;
        }

        private static string ValidateBatchUniqueness(
            IEnumerable<ComponentBuildArtifacts> artifacts
        )
        {
            return ValidateBatchUniquenessImpl(
                artifacts.Select(a => (a.Namespace, a.ComponentName, a.UitkxPath))
            );
        }

        /// <summary>
        /// Post-compile guard: every batch component's expected type FQN must
        /// resolve to the union assembly we just loaded. If the type isn't
        /// found (emit-time culling), or worse — points at a DIFFERENT
        /// assembly (e.g. project DLL win race), the union swap would bind
        /// the wrong delegate. Bail and let per-file flow expose the issue.
        /// </summary>
        private static string ValidateBatchAssemblyIdentity(
            Assembly unionAssembly,
            IEnumerable<ComponentBuildArtifacts> artifacts
        )
        {
            foreach (var art in artifacts)
            {
                Type t = unionAssembly.GetType(art.FullyQualifiedName, throwOnError: false);
                if (t == null)
                    return $"Type '{art.FullyQualifiedName}' missing from union assembly.";
                if (t.Assembly != unionAssembly)
                    return $"Type '{art.FullyQualifiedName}' resolved to "
                        + $"'{t.Assembly.GetName().Name}' instead of union assembly.";
            }
            return null;
        }

        /// <summary>
        /// Compiles a hook or module .uitkx file (no component keyword).
        /// Uses <see cref="HmrHookEmitter"/> to generate the C# source and
        /// compiles it the same way as component files.
        /// </summary>
        private HmrCompileResult CompileHookModuleFile(
            object directives,
            object diagList,
            string uitkxPath,
            Stopwatch sw,
            HmrCompileResult result
        )
        {
            try
            {
                // H-01: same error-first gate as the component path — a malformed hook/
                // module signature must not silently emit from a recovered directive set.
                if (TryGetParseErrorMessage(diagList, uitkxPath, out string parseErrorMsg))
                {
                    result.Error = parseErrorMsg;
                    return result;
                }

                result.IsHookModuleFile = true;
                string containerClass = HmrHookEmitter.DeriveContainerClassName(uitkxPath);
                result.HookContainerClass = containerClass;
                result.ComponentName = containerClass;

                var stepSw = Stopwatch.StartNew();

                // ── Emit C# for hook bodies and/or module bodies ─────────
                var sources = new List<string>();

                string hookCSharp = HmrHookEmitter.Emit(directives, uitkxPath, containerClass);
                if (!string.IsNullOrEmpty(hookCSharp))
                    sources.Add(hookCSharp);
                // Records whether a hook container was actually emitted, so the
                // controller only runs SwapHooks for files that have hooks
                // (a module-only file has no container to find).
                result.HasHooks = !string.IsNullOrEmpty(hookCSharp);

                string moduleCSharp = HmrHookEmitter.EmitModules(directives, uitkxPath);
                if (!string.IsNullOrEmpty(moduleCSharp))
                    sources.Add(moduleCSharp);

                if (sources.Count == 0)
                {
                    result.Error = "No hook or module declarations found";
                    return result;
                }

                stepSw.Stop();
                result.EmitMs = stepSw.Elapsed.TotalMilliseconds;

                // ── Compile ──────────────────────────────────────────────
                stepSw = Stopwatch.StartNew();
                var asm = CompileSources(
                    sources.ToArray(),
                    containerClass,
                    uitkxPath,
                    out string compileError
                );
                stepSw.Stop();
                result.CompileMs = stepSw.Elapsed.TotalMilliseconds;

                if (asm == null)
                {
                    result.Error = compileError;
                    return result;
                }

                result.LoadedAssembly = asm;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsInfrastructureError = IsInfrastructureException(ex);
                result.Error = ex is System.Reflection.TargetInvocationException tie
                    ? $"{tie.InnerException?.GetType().Name}: {tie.InnerException?.Message ?? ex.Message}\n{tie.InnerException?.StackTrace}"
                    : $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            }
            finally
            {
                sw.Stop();
                result.TotalMs = sw.Elapsed.TotalMilliseconds;
            }

            return result;
        }

        /// <summary>
        /// Discovers companion .uitkx files (e.g. Foo.style.uitkx, Foo.hooks.uitkx)
        /// beside the component file, parses their directives, emits C# for their
        /// module/hook bodies, and appends the generated sources to the list so they
        /// are compiled together as partial-class fragments.
        /// Also collects fully-qualified hook container class names (e.g. "Ns.FooHooks")
        /// so the caller can inject <c>using static</c> directives into the component source.
        /// </summary>
        private void EmitCompanionUitkxSources(
            string uitkxPath,
            string componentName,
            string componentNs,
            List<string> sources,
            List<string> hookContainerFqns
        )
        {
            var dir = Path.GetDirectoryName(uitkxPath);
            if (dir == null)
                return;

            // Track which companion paths we emit inline so the registry pass
            // below doesn't add a duplicate `using static` for the same FQN.
            var inlinePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Pattern: ComponentName.*.uitkx  (e.g. TicTacToe.style.uitkx)
            string prefix = componentName + ".";
            foreach (var file in Directory.GetFiles(dir, prefix + "*.uitkx"))
            {
                // Skip the component file itself
                if (string.Equals(file, uitkxPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    string companionSource = ReadTextWithRetry(file);
                    var diagList = CreateDiagnosticList();
                    var companionDir = InvokeWithDefaults(
                        _directiveParse,
                        null,
                        companionSource,
                        file,
                        diagList,
                        true
                    );
                    if (companionDir == null)
                        continue;

                    // Emit module bodies (style constants, utility methods, etc.)
                    string moduleCSharp = HmrHookEmitter.EmitModules(companionDir, file);
                    if (!string.IsNullOrEmpty(moduleCSharp))
                    {
                        sources.Add(moduleCSharp);
                    }
                    else
                    {
                        // Defensive diagnostic: the file name matched the
                        // companion glob (ComponentName.*.uitkx) but emitted
                        // no module body. This can mask cascade-compile bugs
                        // where Container/Image/Label etc. fail to resolve in
                        // the component because the partial class fragment
                        // never made it into the compilation. Surface it so
                        // future copy-rename or new-file races are diagnosable
                        // from a single log line (see also: TexasOne
                        // creation race tracked in HMR_DETERMINISM_REPORT.md).
                        var moduleDecls = GetProp(companionDir, "ModuleDeclarations");
                        int moduleCount = 0;
                        if (moduleDecls is IEnumerable enumerable)
                            foreach (var _m in enumerable) moduleCount++;
                        if (moduleCount == 0)
                        {
                            Debug.LogWarning(
                                $"[HMR] Companion {Path.GetFileName(file)} matched the "
                                + $"'{componentName}.*.uitkx' glob but its directives "
                                + "contain no module declarations -- the parent's "
                                + "static member references (e.g. style identifiers "
                                + "like 'Container') will fail to resolve. This "
                                + "usually means the file was caught mid-write by "
                                + "the watcher; re-save the .uitkx to retry."
                            );
                        }
                    }

                    // Emit hook bodies if the companion also defines custom hooks
                    string containerClass = HmrHookEmitter.DeriveContainerClassName(file);
                    string hookCSharp = HmrHookEmitter.Emit(
                        companionDir,
                        file,
                        containerClass,
                        withTrampoline: true
                    );
                    if (!string.IsNullOrEmpty(hookCSharp))
                    {
                        sources.Add(hookCSharp);

                        // Collect FQN so caller can inject using-static
                        string hookNs = (string)GetProp(companionDir, "Namespace");
                        if (string.IsNullOrEmpty(hookNs))
                            hookNs = componentNs ?? "ReactiveUITK.Generated";
                        hookContainerFqns.Add($"{hookNs}.{containerClass}");

                        try
                        {
                            inlinePaths.Add(Path.GetFullPath(file));
                        }
                        catch
                        { /* ignore unfullable */
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[HMR] Failed to process companion {Path.GetFileName(file)}: {ex.Message}"
                    );
                }
            }

            // Cross-directory hook resolution. The same-folder pass above only
            // sees files matching `<ComponentName>.*.uitkx`; hooks declared in
            // a shared folder (e.g. Assets/UI/Hooks/UseUiDocumentSlot.hooks.uitkx
            // consumed from Assets/UI/Pages/...) live elsewhere. The registry
            // (seeded at HMR start) supplies their FQNs so the trampoline gets
            // the right `using static` directives. Their .cs is already in the
            // loaded assembly so we DO NOT add source — only the using line.
            //
            // Brief gate: if the seed scan hasn't completed yet (very first
            // recompile after HMR start), wait up to 100 ms. Subsequent
            // recompiles never block here.
            if (!HookContainerRegistry.TryWaitForSeed(100))
            {
                if (!_warnedSeedTimeout)
                {
                    _warnedSeedTimeout = true;
                    Debug.LogWarning(
                        "[HMR] HookContainerRegistry seed exceeded 100 ms; "
                            + "first recompile may miss cross-directory hooks. "
                            + "Subsequent recompiles will pick them up."
                    );
                }
            }

            string ownerAsmdef = AsmdefResolver.OwningAsmdefName(uitkxPath);
            var crossDir = HookContainerRegistry.GetForAsmdef(ownerAsmdef, inlinePaths);
            if (crossDir.Count > 0)
            {
                var seenFqn = new HashSet<string>(hookContainerFqns, StringComparer.Ordinal);
                foreach (var fqn in crossDir)
                    if (seenFqn.Add(fqn))
                        hookContainerFqns.Add(fqn);
            }
        }

        private bool _warnedSeedTimeout;

        /// <summary>
        /// Clears per-session caches without releasing the expensive Roslyn handles
        /// and MetadataReferences. Call this between HMR start/stop cycles.
        /// </summary>
        public void Reset()
        {
            _cachedCompilations.Clear();
            _cachedSyntaxTrees.Clear();
            _crossRefCache.Clear();
            _hmrAssemblyPaths.Clear();
            _genuinelyNewComponents.Clear();
            _allowedRefsByAsmdef.Clear();
            _filteredMetaRefsByAsmdef.Clear();
            _filteredRefLocsByAsmdef.Clear();
            _lastGenuineComponentCount = 0;
            _swapCounter = 0;

            // Clean up temp DLLs from this session
            if (_tempDir != null && Directory.Exists(_tempDir))
            {
                try
                {
                    foreach (var file in Directory.GetFiles(_tempDir))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        { /* locked by LoadFrom — will be cleaned next time */
                        }
                    }
                }
                catch { }
            }
        }

        public void Dispose()
        {
            Reset();

            _languageAsm = null;
            if (_roslynLoaded)
                AppDomain.CurrentDomain.AssemblyResolve -= RoslynAssemblyResolve;

            // Try to remove the temp directory itself (succeeds only if empty)
            if (_tempDir != null && Directory.Exists(_tempDir))
            {
                try
                {
                    Directory.Delete(_tempDir, false);
                }
                catch { }
            }
        }

        // ── Initialization helpers ────────────────────────────────────────────

        private void CleanStaleTempFiles()
        {
            try
            {
                foreach (var file in Directory.GetFiles(_tempDir))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    { /* still locked — leave for next time */
                    }
                }
            }
            catch { }
        }

        private void LoadLanguageDll()
        {
            string analyzersDir = FindAnalyzersDirectory();

            // System.Collections.Immutable is a dependency of Language.dll
            string immutablePath = Path.Combine(analyzersDir, "System.Collections.Immutable.dll");
            if (File.Exists(immutablePath))
            {
                // Only load if not already present
                bool alreadyLoaded = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .Any(a => a.GetName().Name == "System.Collections.Immutable");
                if (!alreadyLoaded)
                    Assembly.LoadFrom(immutablePath);
            }

            string langPath = Path.Combine(analyzersDir, "ReactiveUITK.Language.dll");
            if (!File.Exists(langPath))
                throw new FileNotFoundException(
                    $"ReactiveUITK.Language.dll not found at {langPath}"
                );

            _languageAsm = Assembly.LoadFrom(langPath);
        }

        private void CacheReflectionHandles()
        {
            // DirectiveParser
            var dpType = _languageAsm.GetType("ReactiveUITK.Language.Parser.DirectiveParser");
            if (dpType == null)
                throw new TypeLoadException("DirectiveParser not found");
            _directiveParse = dpType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
            if (_directiveParse == null)
                throw new MissingMethodException("DirectiveParser.Parse not found");

            // Phase 1 scanner methods (used to detect JSX literals embedded in
            // arbitrary C# expressions at HMR time, mirroring the SG path).
            // Optional — older Language.dll builds may lack these; HMR falls
            // back to pre-Phase-1 behavior (no expression splice) when missing.
            _findJsxBlockRanges = dpType.GetMethod(
                "FindJsxBlockRanges",
                BindingFlags.Public | BindingFlags.Static
            );
            _findBareJsxRanges = dpType.GetMethod(
                "FindBareJsxRanges",
                BindingFlags.Public | BindingFlags.Static
            );
            // Phase 1.5 LHS walker for `cond && <Tag/>` desugar. Same
            // optional-resolution pattern — when missing, the HMR splicer
            // falls back to emitting raw `&&` JSX (which the user's compiler
            // surfaces as CS0019 at the right line).
            _findLhsStartForLogicalAnd = dpType.GetMethod(
                "FindLhsStartForLogicalAnd",
                BindingFlags.Public | BindingFlags.Static
            );

            // ParseDiagnostic type (for creating List<ParseDiagnostic>)
            _parseDiagnosticType = _languageAsm.GetType("ReactiveUITK.Language.ParseDiagnostic");
            if (_parseDiagnosticType == null)
                throw new TypeLoadException("ParseDiagnostic type not found");

            // UitkxParser
            var upType = _languageAsm.GetType("ReactiveUITK.Language.Parser.UitkxParser");
            if (upType == null)
                throw new TypeLoadException("UitkxParser not found");
            _uitkxParse = upType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
            if (_uitkxParse == null)
                throw new MissingMethodException("UitkxParser.Parse not found");

            // CanonicalLowering
            var clType = _languageAsm.GetType("ReactiveUITK.Language.Lowering.CanonicalLowering");
            if (clType == null)
                throw new TypeLoadException("CanonicalLowering not found");
            _canonicalLower = clType.GetMethod(
                "LowerToRenderRoots",
                BindingFlags.Public | BindingFlags.Static
            );
            if (_canonicalLower == null)
                throw new MissingMethodException("CanonicalLowering.LowerToRenderRoots not found");

            // H-04: UitkxParser.ParseFragment (standalone JSX snippet parsing) replaces
            // the fragile synthetic-header-prepend trick previously used to parse
            // embedded JSX fragments. Optional — older Language.dll builds may lack it;
            // HMR falls back to the synthetic-header path when missing (see parseMarkup
            // delegates in Compile/BuildComponentArtifacts).
            _parseFragment = upType.GetMethod("ParseFragment", BindingFlags.Public | BindingFlags.Static);
        }

        private object CreateDiagnosticList()
        {
            var listType = typeof(List<>).MakeGenericType(_parseDiagnosticType);
            return Activator.CreateInstance(listType);
        }

        /// <summary>
        /// H-04: parses a standalone JSX fragment via <c>UitkxParser.ParseFragment</c>
        /// when the loaded language-lib has it; falls back to the old fragile
        /// synthetic-header-prepend trick for older committed DLLs that predate it.
        /// </summary>
        private IList ParseMarkupFragment(string jsxText, string path, int startLine)
        {
            if (_parseFragment != null)
            {
                var diags = CreateDiagnosticList();
                var nodes = InvokeWithDefaults(_parseFragment, null, jsxText, path, startLine, diags);
                return GetItems(nodes);
            }

            // ── Fallback: older Language.dll without ParseFragment ────────────
            string synthetic = "@namespace __Tmp\n@component __Tmp\n" + jsxText;
            var miniDiags = CreateDiagnosticList();
            var miniDir = InvokeWithDefaults(_directiveParse, null, synthetic, path, miniDiags, false);
            var legacyNodes = InvokeWithDefaults(
                _uitkxParse, null, synthetic, path, miniDir, miniDiags, false, 0);
            return GetItems(legacyNodes);
        }

        /// <summary>
        /// H-01: HMR previously parsed a file, filled <c>diagList</c> with whatever
        /// DirectiveParser/UitkxParser reported, and then emitted C# from the
        /// error-recovered AST regardless of severity — unlike the SourceGenerator
        /// pipeline (<c>UitkxPipeline.cs</c>), which converts every Error into a
        /// <c>#line</c>/<c>#error</c> and stops. A save with a syntax error would
        /// either produce a cryptic csc wall from the recovered AST's garbage output,
        /// or — worse — valid C# that hot-swapped the WRONG UI silently. This gate
        /// mirrors the SG pipeline's error-first policy for HMR.
        /// </summary>
        /// <returns>
        /// <c>true</c> and a formatted, human-readable message (one line per error,
        /// each "  CODE Lline: message") when <paramref name="diagList"/> contains at
        /// least one <c>ParseSeverity.Error</c> entry; otherwise <c>false</c>.
        /// </returns>
        private bool TryGetParseErrorMessage(object diagList, string uitkxPath, out string errorMessage)
        {
            errorMessage = null;
            if (diagList is not IEnumerable enumerable)
                return false;

            var lines = new List<string>();
            foreach (var diag in enumerable)
            {
                var severity = GetProp(diag, "Severity");
                if (severity == null)
                    continue;
                // ParseSeverity.Error == 0 (first enum member — see ParseDiagnostic.cs).
                if (Convert.ToInt32(severity) != 0)
                    continue;

                string code = (GetProp(diag, "Code") as string) ?? "";
                int line = Convert.ToInt32(GetProp(diag, "SourceLine") ?? 0);
                string message = (GetProp(diag, "Message") as string) ?? "";
                lines.Add($"  {code} L{line}: {message}");
            }

            if (lines.Count == 0)
                return false;

            errorMessage =
                $"[HMR] {uitkxPath} has {lines.Count} parse error(s):\n" + string.Join("\n", lines);
            return true;
        }

        private void BuildReferenceList()
        {
            _referenceLocations = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic || string.IsNullOrEmpty(asm.Location))
                    continue;
                if (!seen.Add(asm.Location))
                    continue;
                if (!File.Exists(asm.Location))
                    continue;
                _referenceLocations.Add(asm.Location);
            }
        }

        // ── Per-asmdef reference filtering ────────────────────────────────
        //
        // Returns the full set of DLL paths (case-insensitive, normalized)
        // that the target asmdef is allowed to reference, computed from
        // UnityEditor.Compilation.CompilationPipeline. Includes the asmdef's
        // own output DLL so types defined in that asmdef but not part of the
        // HMR compilation unit still resolve.
        //
        // Returns null when the asmdef is unknown to Unity or when the
        // pipeline API fails — caller must fall back to the unfiltered
        // reference list (preserving pre-fix behavior in degraded cases).
        private HashSet<string> GetAllowedRefsForAsmdef(string asmdefName)
        {
            if (string.IsNullOrEmpty(asmdefName))
                return null;
            if (_allowedRefsByAsmdef.TryGetValue(asmdefName, out var cached))
                return cached;

            HashSet<string> allowed = null;
            try
            {
                var asms = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(
                    UnityEditor.Compilation.AssembliesType.Editor
                );

                UnityEditor.Compilation.Assembly target = null;
                if (asms != null)
                {
                    for (int i = 0; i < asms.Length; i++)
                    {
                        if (
                            asms[i] != null
                            && string.Equals(asms[i].name, asmdefName, StringComparison.Ordinal)
                        )
                        {
                            target = asms[i];
                            break;
                        }
                    }
                }

                if (target != null)
                {
                    allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (!string.IsNullOrEmpty(target.outputPath))
                        allowed.Add(NormalizePath(target.outputPath));

                    if (target.allReferences != null)
                    {
                        for (int i = 0; i < target.allReferences.Length; i++)
                        {
                            var r = target.allReferences[i];
                            if (string.IsNullOrEmpty(r))
                                continue;
                            allowed.Add(NormalizePath(r));
                        }
                    }

                    // ── CS0433 shadow-rule ─────────────────────────────────
                    // Normal Unity compile of asmdef X has X's types as
                    // in-source (current compilation) and ref DLLs as
                    // references; Roslyn silently prefers current-compilation
                    // types over referenced ones, so duplicate FQNs across
                    // X.dll and a referenced DLL never raise CS0433 in normal
                    // compile.
                    //
                    // HMR compiles only the changed .uitkx into a fresh tiny
                    // assembly and references X.dll itself — so the duplicate
                    // suddenly lives across two referenced DLLs and CS0433
                    // fires. To preserve parity with normal compile we drop
                    // any non-owning ref whose public type FQN set intersects
                    // the owning DLL's. The owning asmdef wins, matching the
                    // same-assembly-shadows-referenced rule.
                    if (!string.IsNullOrEmpty(target.outputPath))
                    {
                        string ownPath = NormalizePath(target.outputPath);
                        var ownTypes = ReadPublicTypeFqns(ownPath);
                        if (ownTypes != null && ownTypes.Count > 0)
                        {
                            var toRemove = new List<string>();
                            foreach (var path in allowed)
                            {
                                if (string.Equals(path, ownPath, StringComparison.OrdinalIgnoreCase))
                                    continue;
                                var refTypes = ReadPublicTypeFqns(path);
                                if (refTypes == null || refTypes.Count == 0)
                                    continue;
                                bool collides = false;
                                foreach (var t in refTypes)
                                {
                                    if (ownTypes.Contains(t)) { collides = true; break; }
                                }
                                if (collides)
                                    toRemove.Add(path);
                            }
                            foreach (var r in toRemove)
                            {
                                allowed.Remove(r);
                                Debug.Log(
                                    $"[HMR] Excluding reference '{SafeFileName(r)}' from asmdef "
                                    + $"'{asmdefName}' compile — public type FQN conflict with "
                                    + $"owning DLL (mirrors Unity normal-compile shadowing)."
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[HMR] Could not compute allowed references for asmdef '{asmdefName}' "
                        + $"via CompilationPipeline; falling back to unfiltered references. "
                        + $"({ex.Message})"
                );
            }

            _allowedRefsByAsmdef[asmdefName] = allowed; // null => no filtering
            return allowed;
        }

        private static string NormalizePath(string p)
        {
            try
            {
                return Path.GetFullPath(p);
            }
            catch
            {
                return p;
            }
        }

        // Returns the per-asmdef MetadataReference[] for an HMR compilation
        // whose source file lives at <paramref name="uitkxPath"/>. Mirrors
        // exactly the reference set Unity itself uses to compile the owning
        // asmdef (CompilationPipeline.GetAssemblies(...).allReferences plus
        // the asmdef's own output DLL). Falls back to the unfiltered
        // _metadataReferences when the asmdef is unknown to Unity.
        //
        // CRITICAL: we materialize MetadataReferences directly from Unity's
        // allReferences paths rather than intersecting with
        // AppDomain.CurrentDomain.GetAssemblies() locations, because the
        // AppDomain location for core BCL DLLs (mscorlib at
        // MonoBleedingEdge/...) differs from the path Unity advertises in
        // allReferences (netstandard.dll under Data/NetStandard/...). A path
        // intersection silently drops every BCL ref and the compile fails
        // with CS0518 ("Predefined type 'System.Object' is not defined").
        private object[] GetFilteredMetaRefs(string uitkxPath)
        {
            string asmdef = AsmdefResolver.OwningAsmdefName(uitkxPath);
            if (string.IsNullOrEmpty(asmdef))
                return _metadataReferences;

            if (_filteredMetaRefsByAsmdef.TryGetValue(asmdef, out var cached))
                return cached;

            var allowed = GetAllowedRefsForAsmdef(asmdef);
            object[] result;
            if (allowed == null || allowed.Count == 0)
            {
                result = _metadataReferences;
            }
            else
            {
                var refs = new List<object>(allowed.Count);
                foreach (var loc in allowed)
                {
                    if (string.IsNullOrEmpty(loc) || !File.Exists(loc))
                        continue;
                    try
                    {
                        refs.Add(InvokeWithDefaults(_createFromFile, null, loc));
                    }
                    catch
                    {
                        // Skip assemblies Roslyn can't read (native, corrupt, etc.)
                    }
                }
                result = refs.ToArray();
            }
            _filteredMetaRefsByAsmdef[asmdef] = result;
            return result;
        }

        // Per-asmdef reference DLL paths for an external csc.dll compilation.
        // Same source-of-truth as GetFilteredMetaRefs: Unity's allReferences.
        private List<string> GetFilteredRefLocations(string uitkxPath)
        {
            string asmdef = AsmdefResolver.OwningAsmdefName(uitkxPath);
            if (string.IsNullOrEmpty(asmdef))
                return _referenceLocations;

            if (_filteredRefLocsByAsmdef.TryGetValue(asmdef, out var cached))
                return cached;

            var allowed = GetAllowedRefsForAsmdef(asmdef);
            List<string> result;
            if (allowed == null || allowed.Count == 0)
            {
                result = _referenceLocations;
            }
            else
            {
                result = new List<string>(allowed.Count);
                foreach (var loc in allowed)
                {
                    if (string.IsNullOrEmpty(loc) || !File.Exists(loc))
                        continue;
                    result.Add(loc);
                }
            }
            _filteredRefLocsByAsmdef[asmdef] = result;
            return result;
        }

        private static string SafeFileName(string p)
        {
            try { return Path.GetFileName(p); }
            catch { return p; }
        }

        // ── Public type FQN scanner (CS0433 shadow rule) ──────────────────
        // Returns the set of public top-level type FQNs exported by the
        // assembly at <paramref name="dllPath"/>. We resolve via the already-
        // loaded AppDomain assembly that matches the DLL's filename (simple
        // name) — every asmdef DLL Unity references for editor compilation
        // is already loaded into the editor's AppDomain by definition, so
        // this avoids needing System.Reflection.Metadata (which is not in
        // the editor asmdef reference closure) or Mono.Cecil.
        //
        // Returns null when the assembly cannot be located or enumerated;
        // callers MUST treat null as "could not classify" and skip filtering
        // (better to leave a ref in than wrongly drop one). Cache keyed by
        // path + assembly identity so a Unity rebuild (new Assembly instance)
        // invalidates naturally.
        private static readonly Dictionary<string, (Assembly asm, HashSet<string> fqns)> s_typeFqnCache =
            new Dictionary<string, (Assembly, HashSet<string>)>(StringComparer.OrdinalIgnoreCase);

        private static HashSet<string> ReadPublicTypeFqns(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath))
                return null;

            string simpleName;
            try
            {
                string fn = Path.GetFileName(dllPath);
                simpleName = fn.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    ? fn.Substring(0, fn.Length - 4)
                    : fn;
            }
            catch { return null; }
            if (string.IsNullOrEmpty(simpleName))
                return null;

            Assembly match = null;
            try
            {
                var loaded = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < loaded.Length; i++)
                {
                    var a = loaded[i];
                    if (a == null) continue;
                    var name = a.GetName().Name;
                    if (string.Equals(name, simpleName, StringComparison.OrdinalIgnoreCase))
                    {
                        match = a;
                        break;
                    }
                }
            }
            catch { /* fall through */ }

            if (match == null)
                return null;

            lock (s_typeFqnCache)
            {
                if (s_typeFqnCache.TryGetValue(dllPath, out var entry) && ReferenceEquals(entry.asm, match))
                    return entry.fqns;
            }

            HashSet<string> set = null;
            try
            {
                set = new HashSet<string>(StringComparer.Ordinal);
                // GetExportedTypes() returns public top-level + public-nested.
                // Filter to top-level only (nested types cannot collide as
                // standalone identifiers in CS0433 contexts).
                var types = match.GetExportedTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    var t = types[i];
                    if (t == null || t.IsNested) continue;
                    string fqn = t.FullName;
                    if (string.IsNullOrEmpty(fqn)) continue;
                    set.Add(fqn);
                }
            }
            catch
            {
                // Reflection-load issues — leave set null so caller skips.
                set = null;
            }

            lock (s_typeFqnCache)
            {
                s_typeFqnCache[dllPath] = (match, set);
            }
            return set;
        }

        private void CheckIfGenuinelyNew(string componentName, string expectedNamespace)
        {
            // Scan loaded assemblies (excluding HMR assemblies) for a type matching
            // the component's *fully-qualified* name. If none found, the component
            // is genuinely new and needs to be added as a cross-reference for
            // dependents.
            //
            // FQN match (rather than the bare-name match used pre-fix) prevents a
            // false "exists" hit when an unrelated assembly happens to declare a
            // public type literally named e.g. `App` or `Page` — which would
            // suppress cross-reference registration and break dependents at
            // compile time. See Plans~/PRETTY_UI_HMR_BUGS.md Issue 8.
            string expectedFqn = string.IsNullOrEmpty(expectedNamespace)
                ? componentName
                : expectedNamespace + "." + componentName;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.IsDynamic || string.IsNullOrEmpty(asm.Location))
                    continue;
                if (asm.GetName().Name.StartsWith("hmr_", StringComparison.OrdinalIgnoreCase))
                    continue;
                try
                {
                    foreach (var type in asm.GetExportedTypes())
                    {
                        // Match by FQN (case-sensitive — namespaces and type names
                        // are case-sensitive in C# regardless of source casing).
                        if (string.Equals(type.FullName, expectedFqn, StringComparison.Ordinal))
                            return; // exists in a pre-existing assembly — NOT new
                    }
                }
                catch { } // ReflectionTypeLoadException, etc.
            }
            _genuinelyNewComponents.Add(componentName);
        }

        // ── In-process Roslyn loading ─────────────────────────────────────────

        private void TryLoadRoslyn()
        {
            try
            {
                string nugetBase = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".nuget",
                    "packages"
                );

                // DLLs we need to load (order matters: deps before dependents)
                var dllSpecs = new[]
                {
                    (
                        "system.runtime.compilerservices.unsafe",
                        "6.0.0",
                        "System.Runtime.CompilerServices.Unsafe"
                    ),
                    ("system.collections.immutable", "6.0.0", "System.Collections.Immutable"),
                    ("system.reflection.metadata", "5.0.0", "System.Reflection.Metadata"),
                    ("system.text.encoding.codepages", "6.0.0", "System.Text.Encoding.CodePages"),
                };

                // Register AssemblyResolve handler for version redirects
                AppDomain.CurrentDomain.AssemblyResolve += RoslynAssemblyResolve;

                foreach (var (pkg, ver, name) in dllSpecs)
                {
                    string dllPath = Path.Combine(
                        nugetBase,
                        pkg,
                        ver,
                        "lib",
                        "netstandard2.0",
                        $"{name}.dll"
                    );
                    if (!File.Exists(dllPath))
                    {
                        Debug.Log(
                            $"[HMR] Roslyn dep not found: {dllPath} — falling back to external compiler"
                        );
                        return;
                    }

                    // Skip if a compatible version is already loaded
                    var loaded = AppDomain
                        .CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => !a.IsDynamic && a.GetName().Name == name);
                    if (loaded != null)
                    {
                        _roslynDeps[name] = loaded;
                        continue;
                    }

                    var asm = Assembly.LoadFrom(dllPath);
                    _roslynDeps[name] = asm;
                }

                // Load Roslyn Common then CSharp
                string commonPath = Path.Combine(
                    nugetBase,
                    "microsoft.codeanalysis.common",
                    "4.3.1",
                    "lib",
                    "netstandard2.0",
                    "Microsoft.CodeAnalysis.dll"
                );
                string csharpPath = Path.Combine(
                    nugetBase,
                    "microsoft.codeanalysis.csharp",
                    "4.3.1",
                    "lib",
                    "netstandard2.0",
                    "Microsoft.CodeAnalysis.CSharp.dll"
                );

                if (!File.Exists(commonPath) || !File.Exists(csharpPath))
                {
                    Debug.Log(
                        $"[HMR] Roslyn DLLs not found in NuGet cache — falling back to external compiler"
                    );
                    return;
                }

                _roslynCommonAsm = Assembly.LoadFrom(commonPath);
                _roslynDeps["Microsoft.CodeAnalysis"] = _roslynCommonAsm;

                _roslynCSharpAsm = Assembly.LoadFrom(csharpPath);
                _roslynDeps["Microsoft.CodeAnalysis.CSharp"] = _roslynCSharpAsm;

                // Cache reflection handles for the compilation API
                CacheRoslynHandles();
                BuildMetadataReferences();

                _roslynLoaded = true;
                Debug.Log("[HMR] In-process Roslyn compiler loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[HMR] Failed to load in-process Roslyn, will use external compiler: {ex.Message}"
                );
                _roslynLoaded = false;
            }
        }

        private Assembly RoslynAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedName = new AssemblyName(args.Name);
            if (_roslynDeps.TryGetValue(requestedName.Name, out var asm))
                return asm;
            return null;
        }

        private void CacheRoslynHandles()
        {
            // CSharpParseOptions — use Default (maps to latest stable C# for this Roslyn build)
            var parseOptionsType = _roslynCSharpAsm.GetType(
                "Microsoft.CodeAnalysis.CSharp.CSharpParseOptions"
            );
            var defaultProp = parseOptionsType.GetProperty(
                "Default",
                BindingFlags.Public | BindingFlags.Static
            );
            _parseOptions = defaultProp.GetValue(null);

            // ── Define UNITY_EDITOR (and Unity's full editor-define list when
            //    available) so companion .cs `#if UNITY_EDITOR` blocks compile
            //    with the same semantics as the project's Unity-Editor build.
            string[] preprocessorSymbols = ResolveEditorPreprocessorSymbols();
            var withSymbolsMethod = parseOptionsType.GetMethod(
                "WithPreprocessorSymbols",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(IEnumerable<string>) },
                null
            );
            if (withSymbolsMethod != null)
            {
                _parseOptions = withSymbolsMethod.Invoke(
                    _parseOptions,
                    new object[] { (IEnumerable<string>)preprocessorSymbols }
                );
            }
            else
            {
                Debug.LogWarning(
                    "[HMR] Roslyn CSharpParseOptions.WithPreprocessorSymbols(IEnumerable<string>) "
                        + "not found — UNITY_EDITOR will be undefined in HMR builds, breaking the "
                        + "trampoline on prior HMR DLLs (brand-new components will not hot-swap "
                        + "until a domain reload)."
                );
            }

            // CSharpSyntaxTree.ParseText(string text, CSharpParseOptions options, ...)
            // Roslyn ships multiple overloads of ParseText. The canonical one we want
            // has all-optional tail (so we can pass just the text + parse options) —
            // a non-deterministic First() pick can land on a (string,string,...)
            // overload that breaks our call shape. PickAllOptionalTailOverload makes
            // the discovery deterministic across Roslyn versions.
            var syntaxTreeType = _roslynCSharpAsm.GetType(
                "Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree"
            );
            _parseText = PickAllOptionalTailOverload(
                syntaxTreeType,
                "ParseText",
                typeof(string),
                BindingFlags.Public | BindingFlags.Static
            );
            RegisterSilentDrift(_parseText);

            // CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullable, optimize, ...)
            var outputKindType = _roslynCommonAsm.GetType("Microsoft.CodeAnalysis.OutputKind");
            object dllOutputKind = Enum.ToObject(outputKindType, 2); // DynamicallyLinkedLibrary = 2

            var compOptsType = _roslynCSharpAsm.GetType(
                "Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions"
            );
            // The constructor has ~20 optional parameters; find it by first param being OutputKind
            var compOptsCtor = compOptsType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .First(c =>
                    c.GetParameters().Length > 0
                    && c.GetParameters()[0].ParameterType == outputKindType
                );
            var ctorParams = compOptsCtor.GetParameters();
            var ctorArgs = new object[ctorParams.Length];
            ctorArgs[0] = dllOutputKind;
            for (int i = 1; i < ctorParams.Length; i++)
            {
                if (ctorParams[i].HasDefaultValue)
                {
                    var def = ctorParams[i].DefaultValue;
                    ctorArgs[i] = def is System.DBNull ? null : def;
                }
                else
                {
                    ctorArgs[i] = ctorParams[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(ctorParams[i].ParameterType)
                        : null;
                }
            }
            _compilationOptions = compOptsCtor.Invoke(ctorArgs);

            // Enable nullable: WithNullableContextOptions(NullableContextOptions.Enable)
            var nullableType = _roslynCommonAsm.GetType(
                "Microsoft.CodeAnalysis.NullableContextOptions"
            );
            if (nullableType != null)
            {
                object enableVal = Enum.ToObject(nullableType, 2); // Enable = 2
                var withNullable = compOptsType.GetMethod("WithNullableContextOptions");
                if (withNullable != null)
                    _compilationOptions = withNullable.Invoke(
                        _compilationOptions,
                        new[] { enableVal }
                    );
            }

            // Enable optimizations
            var optimizationLevel = _roslynCommonAsm.GetType(
                "Microsoft.CodeAnalysis.OptimizationLevel"
            );
            if (optimizationLevel != null)
            {
                object releaseVal = Enum.ToObject(optimizationLevel, 1); // Release = 1
                var withOpt = compOptsType.GetMethod("WithOptimizationLevel");
                if (withOpt != null)
                    _compilationOptions = withOpt.Invoke(_compilationOptions, new[] { releaseVal });
            }

            // CSharpCompilation.Create(string asmName, IEnumerable<SyntaxTree>?,
            //                          IEnumerable<MetadataReference>?, CSharpCompilationOptions?)
            // The 4-arg form is the canonical all-optional-tail overload today; using
            // the picker keeps us safe if Roslyn ever ships a sibling 4-arg shape.
            var compilationType = _roslynCSharpAsm.GetType(
                "Microsoft.CodeAnalysis.CSharp.CSharpCompilation"
            );
            _compilationCreate = PickAllOptionalTailOverload(
                compilationType,
                "Create",
                typeof(string),
                BindingFlags.Public | BindingFlags.Static
            );
            RegisterSilentDrift(_compilationCreate);

            // MetadataReference.CreateFromFile(string path, MetadataReferenceProperties, DocumentationProvider)
            var metaRefType = _roslynCommonAsm.GetType("Microsoft.CodeAnalysis.MetadataReference");
            _createFromFile = PickAllOptionalTailOverload(
                metaRefType,
                "CreateFromFile",
                typeof(string),
                BindingFlags.Public | BindingFlags.Static
            );
            RegisterSilentDrift(_createFromFile);

            // Compilation.Emit(Stream peStream, Stream pdbStream = null, ...) —
            // critical: Roslyn has multiple Emit overloads where param[1] is
            // a non-defaulted Stream (e.g. metadataPEStream variants). The old
            // First() pick was non-deterministic across runtimes; that's the
            // root cause of the Issue 3 "missing required argument 'pdbStream'"
            // failure. PickAllOptionalTailOverload guarantees we pick the
            // canonical overload where pdbStream and everything after it
            // have compile-time defaults.
            var baseCompilationType = _roslynCommonAsm.GetType(
                "Microsoft.CodeAnalysis.Compilation"
            );
            _emitToStream = PickAllOptionalTailOverload(
                baseCompilationType,
                "Emit",
                typeof(Stream),
                BindingFlags.Public | BindingFlags.Instance
            );
            RegisterSilentDrift(_emitToStream);

            // Incremental compilation handles:
            // Compilation.RemoveSyntaxTrees(params SyntaxTree[])
            _compilationRemoveSyntaxTrees = baseCompilationType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == "RemoveSyntaxTrees" && m.GetParameters().Length == 1
                );

            // Compilation.AddSyntaxTrees(params SyntaxTree[])
            _compilationAddSyntaxTrees = baseCompilationType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "AddSyntaxTrees" && m.GetParameters().Length == 1);

            // Compilation.AddReferences(params MetadataReference[])
            _compilationAddReferences = baseCompilationType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "AddReferences" && m.GetParameters().Length == 1);

            // Compilation.WithAssemblyName(string)
            _compilationWithAssemblyName = compilationType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == "WithAssemblyName"
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(string)
                );
        }

        private void BuildMetadataReferences()
        {
            var refs = new List<object>();
            foreach (var loc in _referenceLocations)
            {
                try
                {
                    refs.Add(InvokeWithDefaults(_createFromFile, null, loc));
                }
                catch
                {
                    // Skip assemblies Roslyn can't read (native, corrupt, etc.)
                }
            }
            _metadataReferences = refs.ToArray();
        }

        // ── Compiler discovery ────────────────────────────────────────────────

        private void FindCompilerPaths()
        {
            // Unity Editor path: EditorApplication.applicationPath → .../Unity.exe
            string editorDir = Path.GetDirectoryName(EditorApplication.applicationPath);
            string dataDir = Path.Combine(editorDir, "Data");

            _dotnetPath = Path.Combine(dataDir, "NetCoreRuntime", "dotnet.exe");
            if (!File.Exists(_dotnetPath))
            {
                // macOS/Linux fallback
                _dotnetPath = Path.Combine(dataDir, "NetCoreRuntime", "dotnet");
            }
            if (!File.Exists(_dotnetPath))
                throw new FileNotFoundException(
                    $"dotnet runtime not found at {Path.Combine(dataDir, "NetCoreRuntime")}"
                );

            _cscPath = Path.Combine(dataDir, "DotNetSdkRoslyn", "csc.dll");
            if (!File.Exists(_cscPath))
                throw new FileNotFoundException($"Roslyn csc.dll not found at {_cscPath}");
        }

        // ── Compilation ───────────────────────────────────────────────────────

        // Local polyfill for ModuleInitializerAttribute. Required because the
        // HMR DLL emits `[ModuleInitializer]` (see HmrCSharpEmitter companion
        // class emission) and references the project's main asmdef which only
        // exposes the SG-emitted polyfill as `internal` -- so the HMR DLL's
        // attribute reference fails with CS0122 "inaccessible due to its
        // protection level". Adding a local copy in the HMR compilation unit
        // makes Roslyn bind the attribute to the in-compilation type (same
        // FQN) before consulting referenced assemblies, sidestepping the
        // visibility issue without changing the main-asmdef polyfill's
        // accessibility (which would risk ambiguity once Unity moves to a
        // TFM that ships the real attribute).
        //
        // Guarded by #if !NET5_0_OR_GREATER so a future Unity TFM that ships
        // the real attribute does not produce a duplicate type error.
        private const string ModuleInitializerPolyfillSource =
            "// <auto-generated — HMR ModuleInitializerAttribute polyfill />\n"
            + "#if !NET5_0_OR_GREATER\n"
            + "namespace System.Runtime.CompilerServices\n"
            + "{\n"
            + "    [global::System.AttributeUsage(global::System.AttributeTargets.Method, Inherited = false)]\n"
            + "    internal sealed class ModuleInitializerAttribute : global::System.Attribute { }\n"
            + "}\n"
            + "#endif\n";

        private static string[] PrependPolyfills(string[] sources)
        {
            var combined = new string[sources.Length + 1];
            combined[0] = ModuleInitializerPolyfillSource;
            System.Array.Copy(sources, 0, combined, 1, sources.Length);
            return combined;
        }

        private Assembly CompileSources(
            string[] sources,
            string componentName,
            string ownerUitkxPath,
            out string error
        )
        {
            _swapCounter++;
            error = null;

            // Inject the ModuleInitializerAttribute polyfill into every HMR
            // compile so `[ModuleInitializer]` resolves against an in-compilation
            // type (visible from this assembly) instead of the referenced main
            // asmdef's `internal` copy. See ModuleInitializerPolyfillSource for
            // the full rationale.
            sources = PrependPolyfills(sources);

            // ── Fast path: in-process Roslyn ──────────────────────────────────
            if (_roslynLoaded)
            {
                var asm = InProcessCompile(sources, componentName, ownerUitkxPath, out error);
                if (asm != null || error == null)
                    return asm;
                // If in-process failed with an error, fall through to external
                Debug.LogWarning($"[HMR] In-process compile failed, trying external: {error}");
                error = null;
            }

            // ── Slow path: external dotnet csc.dll ────────────────────────────
            return ExternalCompile(sources, componentName, ownerUitkxPath, out error);
        }

        private Assembly InProcessCompile(
            string[] sources,
            string componentName,
            string ownerUitkxPath,
            out string error
        )
        {
            error = null;

            try
            {
                // Parse source texts into SyntaxTrees
                var treesList = new List<object>(sources.Length);
                foreach (var src in sources)
                {
                    var tree = InvokeWithDefaults(_parseText, null, src, _parseOptions);
                    treesList.Add(tree);
                }

                var newTrees = treesList.ToArray();

                // Build cross-component references using cache
                var crossRefs = BuildCrossRefs(componentName);

                string asmName = $"hmr_{componentName}_{_swapCounter}";

                // ── Try incremental compilation first ─────────────────────────
                object compilation = TryBuildIncremental(
                    componentName,
                    newTrees,
                    crossRefs,
                    asmName
                );

                // ── Fallback: fresh Compilation.Create ────────────────────────
                if (compilation == null)
                    compilation = BuildFreshCompilation(newTrees, crossRefs, asmName, ownerUitkxPath);

                // Emit to MemoryStream and write DLL to disk
                string outputDll = Path.Combine(
                    _tempDir,
                    $"hmr_{componentName}_{_swapCounter}.dll"
                );

                using (var ms = new MemoryStream())
                {
                    var emitResult = InvokeWithDefaults(_emitToStream, compilation, ms);

                    bool success = (bool)
                        emitResult
                            .GetType()
                            .GetProperty("Success", BindingFlags.Public | BindingFlags.Instance)
                            .GetValue(emitResult);

                    if (!success)
                    {
                        // If we used incremental and it failed, retry with a fresh build
                        if (_cachedCompilations.ContainsKey(componentName))
                        {
                            _cachedCompilations.Remove(componentName);
                            _cachedSyntaxTrees.Remove(componentName);
                            compilation = BuildFreshCompilation(newTrees, crossRefs, asmName, ownerUitkxPath);

                            ms.SetLength(0);
                            emitResult = InvokeWithDefaults(_emitToStream, compilation, ms);
                            success = (bool)
                                emitResult
                                    .GetType()
                                    .GetProperty(
                                        "Success",
                                        BindingFlags.Public | BindingFlags.Instance
                                    )
                                    .GetValue(emitResult);
                        }

                        if (!success)
                        {
                            // Extract diagnostics
                            var diagnostics = (IEnumerable)
                                emitResult
                                    .GetType()
                                    .GetProperty(
                                        "Diagnostics",
                                        BindingFlags.Public | BindingFlags.Instance
                                    )
                                    .GetValue(emitResult);

                            var errors = new List<string>();
                            foreach (var diag in diagnostics)
                            {
                                var severity = diag.GetType()
                                    .GetProperty(
                                        "Severity",
                                        BindingFlags.Public | BindingFlags.Instance
                                    )
                                    .GetValue(diag);
                                // DiagnosticSeverity.Error = 3
                                if (Convert.ToInt32(severity) == 3)
                                    errors.Add("  " + diag.ToString());
                            }

                            error =
                                errors.Count > 0
                                    ? $"[HMR] Compilation failed for {componentName}:\n{string.Join("\n", errors)}"
                                    : $"[HMR] Compilation failed for {componentName}: unknown error";
                            return null;
                        }
                    }

                    // Write directly from MemoryStream to disk (avoids byte[] copy)
                    using (var fs = new FileStream(outputDll, FileMode.Create, FileAccess.Write))
                    {
                        ms.Position = 0;
                        ms.CopyTo(fs);
                    }
                }

                // Cache the successful compilation and trees for incremental reuse
                _cachedCompilations[componentName] = compilation;
                _cachedSyntaxTrees[componentName] = newTrees;
                _lastGenuineComponentCount = _genuinelyNewComponents.Count;

                // Register on disk for cross-component references; replace previous version
                string oldDll = null;
                if (_hmrAssemblyPaths.TryGetValue(componentName, out oldDll) && oldDll != outputDll)
                {
                    // Invalidate cached cross-ref for the old DLL
                    _crossRefCache.Remove(componentName);
                    try
                    {
                        File.Delete(oldDll);
                    }
                    catch
                    { /* may be locked by LoadFrom — cleaned on next session */
                    }
                }
                _hmrAssemblyPaths[componentName] = outputDll;

                // Use LoadFrom (memory-mapped) instead of Load(byte[]) to avoid
                // copying the PE image into the managed heap
                var loadedAsm = Assembly.LoadFrom(outputDll);

                // Force every module's [ModuleInitializer] to run NOW. The CLR
                // only fires <Module>.cctor lazily on first member access from
                // the loaded module, and the downstream swap pipeline only
                // touches member-bearing /module/ types — never the synthetic
                // companion class that carries [ModuleInitializer]. Result
                // before this call: a freshly compiled component whose render
                // body never gets published to its Family (Register never
                // fires) so the parent renders the fallback placeholder and
                // the user sees nothing change on screen. See
                // ApplySuccessfulCompileResult's comment claiming
                // "the freshly compiled assembly's [ModuleInitializer] has
                // already run during the Roslyn-emit Assembly.Load above" --
                // that assumption is what this call now upholds.
                ForceRunModuleInitializers(loadedAsm);

                // Force GC to reclaim dead SyntaxTrees, EmitResult, MemoryStream etc.
                // before Mono's lazy GC decides to expand the heap
                GC.Collect(2, GCCollectionMode.Optimized);

                return loadedAsm;
            }
            catch (TargetInvocationException tie)
            {
                error =
                    $"[HMR] In-process Roslyn error: {tie.InnerException?.Message ?? tie.Message}";
                return null;
            }
            catch (Exception ex)
            {
                error = $"[HMR] In-process Roslyn error: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Build cross-component MetadataReferences using cache.
        /// Only creates new references when the underlying DLL path changes.
        /// </summary>
        private List<object> BuildCrossRefs(string componentName)
        {
            var crossRefs = new List<object>();
            foreach (var kvp in _hmrAssemblyPaths)
            {
                if (kvp.Key.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!_genuinelyNewComponents.Contains(kvp.Key))
                    continue;
                if (!File.Exists(kvp.Value))
                    continue;

                // Check cache: keyed by component name, invalidated when DLL path changes
                if (_crossRefCache.TryGetValue(kvp.Key, out var cached))
                {
                    crossRefs.Add(cached);
                }
                else
                {
                    try
                    {
                        var metaRef = InvokeWithDefaults(_createFromFile, null, kvp.Value);
                        _crossRefCache[kvp.Key] = metaRef;
                        crossRefs.Add(metaRef);
                    }
                    catch { }
                }
            }
            return crossRefs;
        }

        /// <summary>
        /// Try to reuse a cached Compilation for this component by swapping
        /// SyntaxTrees and updating references incrementally.
        /// Returns null if incremental is not possible.
        /// </summary>
        private object TryBuildIncremental(
            string componentName,
            object[] newTrees,
            List<object> crossRefs,
            string asmName
        )
        {
            // Incremental API handles must all be available
            if (
                _compilationRemoveSyntaxTrees == null
                || _compilationAddSyntaxTrees == null
                || _compilationWithAssemblyName == null
            )
                return null;

            // Must have a cached compilation for this component
            if (!_cachedCompilations.TryGetValue(componentName, out var cached))
                return null;

            // If new cross-refs appeared since last compile, invalidate all caches
            // because every compilation needs the new reference
            if (_genuinelyNewComponents.Count != _lastGenuineComponentCount)
            {
                _cachedCompilations.Clear();
                _cachedSyntaxTrees.Clear();
                return null;
            }

            try
            {
                // Remove old syntax trees
                if (
                    _cachedSyntaxTrees.TryGetValue(componentName, out var oldTrees)
                    && oldTrees.Length > 0
                )
                {
                    var syntaxTreeBaseType = _roslynCommonAsm.GetType(
                        "Microsoft.CodeAnalysis.SyntaxTree"
                    );
                    var oldArray = Array.CreateInstance(syntaxTreeBaseType, oldTrees.Length);
                    for (int i = 0; i < oldTrees.Length; i++)
                        oldArray.SetValue(oldTrees[i], i);
                    cached = _compilationRemoveSyntaxTrees.Invoke(
                        cached,
                        new object[] { oldArray }
                    );
                }

                // Add new syntax trees
                {
                    var syntaxTreeBaseType = _roslynCommonAsm.GetType(
                        "Microsoft.CodeAnalysis.SyntaxTree"
                    );
                    var newArray = Array.CreateInstance(syntaxTreeBaseType, newTrees.Length);
                    for (int i = 0; i < newTrees.Length; i++)
                        newArray.SetValue(newTrees[i], i);
                    cached = _compilationAddSyntaxTrees.Invoke(cached, new object[] { newArray });
                }

                // Update assembly name
                cached = _compilationWithAssemblyName.Invoke(cached, new object[] { asmName });

                return cached;
            }
            catch
            {
                // Incremental failed — caller will use fresh Create
                _cachedCompilations.Remove(componentName);
                _cachedSyntaxTrees.Remove(componentName);
                return null;
            }
        }

        /// <summary>
        /// Build a fresh CSharpCompilation from scratch (original behavior).
        /// </summary>
        private object BuildFreshCompilation(
            object[] newTrees,
            List<object> crossRefs,
            string asmName,
            string ownerUitkxPath
        )
        {
            var baseRefs = GetFilteredMetaRefs(ownerUitkxPath);
            var allRefs = new List<object>(baseRefs.Length + crossRefs.Count);
            for (int i = 0; i < baseRefs.Length; i++)
                allRefs.Add(baseRefs[i]);
            allRefs.AddRange(crossRefs);

            var syntaxTreeBaseType = _roslynCommonAsm.GetType("Microsoft.CodeAnalysis.SyntaxTree");
            var treesArray = Array.CreateInstance(syntaxTreeBaseType, newTrees.Length);
            for (int i = 0; i < newTrees.Length; i++)
                treesArray.SetValue(newTrees[i], i);

            var metaRefType = _roslynCommonAsm.GetType("Microsoft.CodeAnalysis.MetadataReference");
            var refsArray = Array.CreateInstance(metaRefType, allRefs.Count);
            for (int i = 0; i < allRefs.Count; i++)
                refsArray.SetValue(allRefs[i], i);

            return _compilationCreate.Invoke(
                null,
                new object[] { asmName, treesArray, refsArray, _compilationOptions }
            );
        }

        private Assembly ExternalCompile(
            string[] sources,
            string componentName,
            string ownerUitkxPath,
            out string error
        )
        {
            error = null;

            // Write sources to temp files
            var sourceFiles = new List<string>();
            for (int i = 0; i < sources.Length; i++)
            {
                string path = Path.Combine(_tempDir, $"hmr_{componentName}_{i}.cs");
                File.WriteAllText(path, sources[i]);
                sourceFiles.Add(path);
            }

            string outputDll = Path.Combine(_tempDir, $"hmr_{componentName}_{_swapCounter}.dll");

            // Build response file for csc
            string rspPath = Path.Combine(_tempDir, $"hmr_{componentName}.rsp");
            using (var rsp = new StreamWriter(rspPath, false))
            {
                rsp.WriteLine("-target:library");
                rsp.WriteLine($"-out:\"{outputDll}\"");
                rsp.WriteLine("-langversion:latest");
                rsp.WriteLine("-nowarn:0105,0436,8600,8601,8602,8603,8604");
                rsp.WriteLine("-nullable:enable");
                rsp.WriteLine("-deterministic");
                rsp.WriteLine("-optimize+");
                foreach (var loc in GetFilteredRefLocations(ownerUitkxPath))
                    rsp.WriteLine($"-reference:\"{loc}\"");
                // Add previously HMR-compiled assemblies for cross-component resolution (new components only)
                foreach (var kvp in _hmrAssemblyPaths)
                {
                    if (kvp.Key.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                        continue; // skip self — it will be replaced
                    // Only add cross-refs for genuinely new components;
                    // existing ones are already referenced and would cause CS0433
                    if (!_genuinelyNewComponents.Contains(kvp.Key))
                        continue;
                    if (File.Exists(kvp.Value))
                        rsp.WriteLine($"-reference:\"{kvp.Value}\"");
                }
                foreach (var sf in sourceFiles)
                    rsp.WriteLine($"\"{sf}\"");
            }

            // Invoke: dotnet csc.dll @response.rsp
            var psi = new ProcessStartInfo
            {
                FileName = _dotnetPath,
                Arguments = $"exec \"{_cscPath}\" \"@{rspPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _tempDir,
            };

            string stdout,
                stderr;
            int exitCode;

            using (var proc = Process.Start(psi))
            {
                stdout = proc.StandardOutput.ReadToEnd();
                stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

            if (exitCode != 0 || !File.Exists(outputDll))
            {
                // Parse errors from stdout (csc writes diagnostics to stdout)
                string output = string.IsNullOrEmpty(stdout) ? stderr : stdout;
                var errorLines = output
                    .Split('\n')
                    .Where(l => l.Contains(": error "))
                    .Select(l => "  " + l.Trim())
                    .ToArray();

                error =
                    errorLines.Length > 0
                        ? $"[HMR] Compilation failed for {componentName}:\n{string.Join("\n", errorLines)}"
                        : $"[HMR] Compilation failed for {componentName}:\n{output}";
                return null;
            }

            // Register on disk for cross-component references; replace previous version
            if (
                _hmrAssemblyPaths.TryGetValue(componentName, out string oldDll)
                && oldDll != outputDll
            )
            {
                _crossRefCache.Remove(componentName);
                try
                {
                    File.Delete(oldDll);
                }
                catch
                { /* may be locked by LoadFrom */
                }
            }
            _hmrAssemblyPaths[componentName] = outputDll;

            // Use LoadFrom (memory-mapped) instead of Load(byte[])
            var loadedAsm = Assembly.LoadFrom(outputDll);

            // Same rationale as the in-process path -- force module
            // initializers to run so per-component Register calls publish
            // the new render body to its Family before the swap pipeline
            // touches the assembly.
            ForceRunModuleInitializers(loadedAsm);

            // Force GC to reclaim dead allocations before Mono expands the heap
            GC.Collect(2, GCCollectionMode.Optimized);

            return loadedAsm;
        }

        // Explicitly fire <Module>.cctor for every module in the loaded HMR
        // assembly. RuntimeHelpers.RunModuleConstructor is the documented way
        // to deterministically execute a module's initializer; the CLR
        // de-duplicates internally so calling this when the cctor already ran
        // is a no-op. Without this call, Assembly.LoadFrom defers <Module>.cctor
        // until the first reflection access that touches a member of a type
        // in the module -- which never happens for synthetic companion types
        // that exist solely to carry [ModuleInitializer] for Family.Register.
        private static void ForceRunModuleInitializers(Assembly asm)
        {
            if (asm == null) return;
            Module[] modules;
            try { modules = asm.GetModules(); }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[HMR] Could not enumerate modules of HMR assembly to force "
                    + $"ModuleInitializer execution: {ex.Message}"
                );
                return;
            }
            foreach (var mod in modules)
            {
                if (mod == null) continue;
                try
                {
                    System.Runtime.CompilerServices.RuntimeHelpers
                        .RunModuleConstructor(mod.ModuleHandle);
                }
                catch (Exception ex)
                {
                    // A ModuleInitializer throwing should surface so the user
                    // can fix it; swallow only the meta-failure of "could not
                    // run cctor" itself, which is virtually never recoverable.
                    Debug.LogWarning(
                        $"[HMR] Failed to run ModuleInitializer for '{mod.Name}': "
                        + $"{ex.Message}"
                    );
                }
            }
        }

        // ── Utility ───────────────────────────────────────────────────────────

        private static string FindAnalyzersDirectory()
        {
            // Primary: use the package root via ScriptableObject asset path lookup
            // The Editor/HMR/ folder is inside the package — find the package root
            // by locating this script via AssetDatabase
            string[] guids = UnityEditor.AssetDatabase.FindAssets("UitkxHmrCompiler t:MonoScript");
            if (guids.Length > 0)
            {
                string scriptPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                // scriptPath is like "Assets/ReactiveUIToolKit/Editor/HMR/UitkxHmrCompiler.cs"
                // Walk up to package root then into Analyzers/
                string dir = Path.GetDirectoryName(scriptPath); // Editor/HMR
                dir = Path.GetDirectoryName(dir); // Editor
                dir = Path.GetDirectoryName(dir); // package root
                string analyzersDir = Path.Combine(
                    Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
                    dir,
                    "Analyzers"
                );
                if (Directory.Exists(analyzersDir))
                    return analyzersDir;
            }

            // Fallback: try the well-known path
            string packageRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "ReactiveUIToolKit")
            );
            string fallback = Path.Combine(packageRoot, "Analyzers");
            if (Directory.Exists(fallback))
                return fallback;

            // Fallback 2: search all Assets subfolders for ReactiveUIToolKit/Analyzers
            string assetsDir = Application.dataPath;
            foreach (
                var candidate in Directory.GetDirectories(
                    assetsDir,
                    "Analyzers",
                    SearchOption.AllDirectories
                )
            )
            {
                if (File.Exists(Path.Combine(candidate, "ReactiveUITK.Language.dll")))
                    return candidate;
            }

            throw new DirectoryNotFoundException(
                "Cannot find Analyzers/ directory containing ReactiveUITK.Language.dll"
            );
        }

        internal static object GetProp(object obj, string name)
        {
            if (obj == null)
                return null;
            return obj.GetType()
                .GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(obj);
        }

        internal static IList GetItems(object immutableArray)
        {
            if (immutableArray == null)
                return Array.Empty<object>();
            // ImmutableArray<T> implements IEnumerable<T>; cast to non-generic IEnumerable
            // and materialize to list for indexed access.
            var items = new List<object>();
            foreach (var item in (IEnumerable)immutableArray)
                items.Add(item);
            return items;
        }

        // ── Defensive reflection invoker ─────────────────────────────────────
        // Tracks per-MethodInfo whether we've already warned about silent
        // padding, so a single drift is reported once per session instead of
        // flooding the console on every .uitkx save.
        private static readonly HashSet<MethodInfo> _paddedMethodWarnings = new();
        private static readonly object _paddedMethodWarningsLock = new();

        // ── Drift-warning suppression set ────────────────────────────────────
        // Roslyn-targeted MethodInfos registered here are *intentionally*
        // called with a short positional arg list whose tail is filled from
        // compile-time defaults (the canonical "all-optional-tail" overload
        // shape returned by PickAllOptionalTailOverload). Drift warnings for
        // those calls are pure noise. Language-library calls are NOT in this
        // set so genuine API drift in the parser/lowering pipeline still
        // surfaces loud-and-clear on the very first save.
        private static readonly HashSet<MethodInfo> _silentDriftMethods = new();
        private static readonly object _silentDriftMethodsLock = new();

        private static void RegisterSilentDrift(MethodInfo m)
        {
            if (m == null)
                return;
            lock (_silentDriftMethodsLock)
                _silentDriftMethods.Add(m);
        }

        /// <summary>
        /// Locates the canonical "all-optional-tail" overload of a reflected
        /// method. Filters by <paramref name="name"/> and the type of parameter
        /// 0 (<paramref name="firstParamType"/>), then prefers the SHORTEST
        /// overload where every parameter from index 1 onward has
        /// <c>HasDefaultValue == true</c>. Falls back to the shortest matching
        /// overload regardless of optionality if no all-optional-tail candidate
        /// exists.
        /// <para>
        /// Critical for Roslyn discovery: <see cref="System.Linq.Enumerable.First{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, bool})"/>
        /// has no documented ordering guarantee across runtime versions, and
        /// <summary>
        /// Returns the preprocessor symbols Roslyn should define when
        /// compiling user .uitkx / companion .cs sources for HMR. Mirrors
        /// Unity's editor-side compile so the SG-emitted trampoline (and any
        /// user <c>#if UNITY_EDITOR</c> blocks) survive.
        /// <para>
        /// Strategy: ask <see cref="UnityEditor.Compilation.CompilationPipeline"/>
        /// for the editor-target Assembly-CSharp-Editor defines (Unity adds
        /// <c>UNITY_EDITOR</c>, version pragmas, scripting-backend pragmas
        /// etc. there). Fall back to a minimum viable set of just
        /// <c>UNITY_EDITOR</c> if the API is unavailable or returns nothing.
        /// </para>
        /// </summary>
        private static string[] ResolveEditorPreprocessorSymbols()
        {
            HashSet<string> symbols = new HashSet<string>(StringComparer.Ordinal)
            {
                "UNITY_EDITOR",
            };

            try
            {
                var asms = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(
                    UnityEditor.Compilation.AssembliesType.Editor
                );
                if (asms != null)
                {
                    foreach (var asm in asms)
                    {
                        if (asm?.defines == null)
                            continue;
                        for (int i = 0; i < asm.defines.Length; i++)
                            symbols.Add(asm.defines[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[HMR] Could not enumerate Unity editor defines via CompilationPipeline; "
                        + $"falling back to UNITY_EDITOR-only. ({ex.Message})"
                );
            }

            var result = new string[symbols.Count];
            symbols.CopyTo(result);
            return result;
        }

        /// <summary>
        /// Picks the overload whose parameters past <paramref name="firstParamType"/>
        /// are all optional. Used for resolving Roslyn surface methods in a
        /// version-tolerant way without locking to a particular API generation.
        /// <para>
        /// <c>Compilation.Emit</c> in particular ships multiple <c>(Stream, …)</c>
        /// overloads where parameter 1 is a non-defaulted <c>Stream</c>. A
        /// non-deterministic First() pick on those silently fails at invoke time.
        /// </para>
        /// </summary>
        private static MethodInfo PickAllOptionalTailOverload(
            Type declaringType,
            string name,
            Type firstParamType,
            BindingFlags flags
        )
        {
            if (declaringType == null)
                return null;

            MethodInfo allOptionalBest = null;
            int allOptionalBestLen = int.MaxValue;
            MethodInfo anyMatchBest = null;
            int anyMatchBestLen = int.MaxValue;

            foreach (var m in declaringType.GetMethods(flags))
            {
                if (m.Name != name)
                    continue;
                var ps = m.GetParameters();
                if (ps.Length == 0)
                    continue;
                if (ps[0].ParameterType != firstParamType)
                    continue;

                if (ps.Length < anyMatchBestLen)
                {
                    anyMatchBest = m;
                    anyMatchBestLen = ps.Length;
                }

                bool tailAllOptional = true;
                for (int i = 1; i < ps.Length; i++)
                {
                    if (!ps[i].HasDefaultValue)
                    {
                        tailAllOptional = false;
                        break;
                    }
                }
                if (!tailAllOptional)
                    continue;
                if (ps.Length < allOptionalBestLen)
                {
                    allOptionalBest = m;
                    allOptionalBestLen = ps.Length;
                }
            }

            return allOptionalBest ?? anyMatchBest;
        }

        /// <summary>
        /// Invokes a reflective method, padding short argument arrays with each
        /// parameter's compile-time default value when the language library has
        /// gained new optional parameters since this HMR build was shipped.
        /// Surfaces silent API drift via a one-time <c>LogWarning</c> per
        /// <see cref="MethodInfo"/>, and throws a clear <see cref="ArgumentException"/>
        /// when the mismatch is irrecoverable (too many args, or a non-optional
        /// parameter is missing).
        ///
        /// <para>The <paramref name="target"/> argument is the receiver for
        /// instance methods, or <c>null</c> for static methods. It is mandatory
        /// (rather than defaulted) so that overload resolution cannot silently
        /// shift a <c>string</c> argument into the receiver slot — a class of
        /// bug that previously hid behind two competing <c>params</c> overloads.</para>
        ///
        /// <para>Methods registered via <see cref="RegisterSilentDrift"/> skip
        /// the warning (see field comments for rationale).</para>
        /// </summary>
        private static object InvokeWithDefaults(
            MethodInfo method,
            object target,
            params object[] args
        )
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            args ??= Array.Empty<object>();

            var parameters = method.GetParameters();
            if (args.Length == parameters.Length)
                return method.Invoke(target, args);

            if (args.Length > parameters.Length)
            {
                throw new ArgumentException(
                    $"[HMR] {method.DeclaringType?.Name}.{method.Name}: HMR passed "
                        + $"{args.Length} args but the loaded language library declares "
                        + $"{parameters.Length} parameter(s). The HMR compiler must be updated."
                );
            }

            // args.Length < parameters.Length — pad missing tail with defaults.
            var padded = new object[parameters.Length];
            Array.Copy(args, padded, args.Length);
            for (int i = args.Length; i < parameters.Length; i++)
            {
                var p = parameters[i];
                if (!p.HasDefaultValue)
                {
                    throw new ArgumentException(
                        $"[HMR] {method.DeclaringType?.Name}.{method.Name}: missing "
                            + $"required argument '{p.Name}' (position {i}). The HMR compiler "
                            + $"is out of sync with the loaded language library."
                    );
                }
                padded[i] = p.DefaultValue;
            }

            bool firstWarning;
            lock (_paddedMethodWarningsLock)
                firstWarning = _paddedMethodWarnings.Add(method);
            bool isSilent;
            lock (_silentDriftMethodsLock)
                isSilent = _silentDriftMethods.Contains(method);
            if (firstWarning && !isSilent)
            {
                Debug.LogWarning(
                    $"[HMR] {method.DeclaringType?.Name}.{method.Name}: HMR passed "
                        + $"{args.Length} args but the loaded language library declares "
                        + $"{parameters.Length} parameter(s); padded missing tail with "
                        + $"compile-time defaults. Update the HMR compiler to pass the "
                        + $"new arguments explicitly."
                );
            }

            return method.Invoke(target, padded);
        }

        /// <summary>
        /// Classifies whether an exception caught inside the HMR compile pipeline
        /// represents an infrastructure failure (HMR plumbing is broken) as opposed
        /// to a user-authored .uitkx error. Infrastructure failures cause the
        /// controller to log one error and self-disable; user errors continue to
        /// follow the warn + retry cascade flow.
        /// </summary>
        internal static bool IsInfrastructureException(Exception ex)
        {
            if (ex == null)
                return false;
            // Unwrap reflection wrapper.
            if (ex is TargetInvocationException tie && tie.InnerException != null)
                ex = tie.InnerException;
            return ex is TargetParameterCountException
                || ex is MissingMethodException
                || ex is MissingFieldException
                || ex is TypeLoadException
                || ex is ReflectionTypeLoadException
                || ex is BadImageFormatException;
        }

        // ── Rank 2: new .cs pickup ───────────────────────────────────────────

        // Cache: asmdef name → last-known project DLL mtime. Used to filter
        // .cs files older than the DLL (already compiled into AppDomain).
        // Refreshed every Compile call cheaply via File.GetLastWriteTimeUtc.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<
            string,
            DateTime
        > s_asmdefDllMtimeCache = new();

        /// <summary>
        /// Rank 2 helper: appends fresh asmdef-scoped .cs files (whose primary
        /// type-name isn't yet in AppDomain) to <paramref name="sources"/>.
        /// Fully best-effort — any failure swallows quietly and the compile
        /// proceeds without the extra trees, matching pre-Rank-2 behavior.
        /// </summary>
        private void TryIncludeNewAsmdefCsFiles(
            string uitkxPath,
            string[] alreadyIncludedCs,
            List<string> sources
        )
        {
            try
            {
                string asmdef = AsmdefResolver.OwningAsmdefName(uitkxPath);
                if (string.IsNullOrEmpty(asmdef))
                    return;

                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

                // Resolve the project DLL mtime for this asmdef. If we can't
                // find it (e.g. first cold compile), use DateTime.MinValue so
                // every .cs is considered "newer" — type-name dedupe still
                // prevents CS0101 against any types Unity DID load.
                DateTime dllMtime = ResolveAsmdefDllMtime(projectRoot, asmdef);

                ICollection<string> already = alreadyIncludedCs ?? Array.Empty<string>();
                var extra = NewCsFileDiscovery.FindForAsmdef(
                    asmdef,
                    projectRoot,
                    dllMtime,
                    already
                );

                foreach (var path in extra)
                {
                    try
                    {
                        sources.Add(ReadTextWithRetry(path));
                    }
                    catch
                    { /* per-file IO error — skip silently */
                    }
                }
            }
            catch
            {
                // Never let the discovery pass fail the entire compile.
            }
        }

        private static DateTime ResolveAsmdefDllMtime(string projectRoot, string asmdef)
        {
            if (s_asmdefDllMtimeCache.TryGetValue(asmdef, out var cached))
            {
                // Refresh if the DLL has been updated since we cached.
                string dllPathCached = Path.Combine(
                    projectRoot,
                    "Library",
                    "ScriptAssemblies",
                    asmdef + ".dll"
                );
                try
                {
                    var fresh = File.GetLastWriteTimeUtc(dllPathCached);
                    if (fresh != cached)
                    {
                        s_asmdefDllMtimeCache[asmdef] = fresh;
                        return fresh;
                    }
                    return cached;
                }
                catch
                {
                    return cached;
                }
            }

            string dllPath = Path.Combine(
                projectRoot,
                "Library",
                "ScriptAssemblies",
                asmdef + ".dll"
            );
            DateTime mtime;
            try
            {
                mtime = File.GetLastWriteTimeUtc(dllPath);
            }
            catch
            {
                mtime = DateTime.MinValue;
            }

            s_asmdefDllMtimeCache[asmdef] = mtime;
            return mtime;
        }
    }

    // ── Result ────────────────────────────────────────────────────────────────

    internal sealed class HmrCompileResult
    {
        public bool Success;
        public string Error;
        public string ComponentName;

        /// <summary>
        /// Declared namespace from the .uitkx file's <c>@namespace</c> directive
        /// (empty string when omitted). Threaded from <see cref="ComponentBuildArtifacts.Namespace"/>
        /// so swappers can FQN-resolve types without a non-deterministic
        /// <c>GetTypes().FirstOrDefault().Namespace</c> probe (which picks Roslyn's
        /// embedded <c>Microsoft.CodeAnalysis.EmbeddedAttribute</c> based on metadata
        /// ordering).
        /// </summary>
        public string Namespace;

        public Assembly LoadedAssembly;

        /// <summary>
        /// True when this result is from a hook/module .uitkx file
        /// (as opposed to a component file). The controller uses this
        /// to route to the hook delegate swapper instead of the
        /// component fiber swapper.
        /// </summary>
        public bool IsHookModuleFile;

        /// <summary>
        /// True when this hook/module file actually declared one or more
        /// <c>hook</c>s (i.e. the hook emitter produced a container class).
        /// A module-ONLY file (only <c>module</c> bodies, no hooks) leaves this
        /// false, so the controller skips <see cref="UitkxHmrDelegateSwapper.SwapHooks"/>
        /// — which would otherwise log a spurious "Could not find hook container"
        /// warning and no-op — and instead fires the global re-render itself
        /// after the module-method swap.
        /// </summary>
        public bool HasHooks;

        /// <summary>
        /// Container class name for hook files (e.g. "CounterHooks").
        /// Used by the delegate swapper to find the static fields.
        /// </summary>
        public string HookContainerClass;

        // Per-step timing (milliseconds)
        public double ParseMs;
        public double EmitMs;
        public double CompileMs;
        public double SwapMs;
        public double TotalMs;

        public string TimingBreakdown =>
            $"Parse: {ParseMs:F1}ms | Emit: {EmitMs:F1}ms | Compile: {CompileMs:F1}ms | Swap: {SwapMs:F1}ms | Total: {TotalMs:F1}ms";

        /// <summary>
        /// True when the failure is caused by HMR infrastructure drift
        /// (reflection signature mismatch against the language library, missing
        /// type/method, or assembly load failure) rather than a user-authored
        /// .uitkx error. Triggers a one-time error log + self-disable in the
        /// controller — restart Unity or click Start in the HMR window after
        /// rebuilding the language library.
        /// </summary>
        public bool IsInfrastructureError;
    }

    // ── Batch (Rank 5 — per-SCC union) result ────────────────────────────────

    /// <summary>
    /// Result of a single union-compile call covering N <c>.uitkx</c> files.
    /// All files in the batch share <see cref="UnionAssembly"/>; per-file
    /// <see cref="HmrCompileResult"/> entries in <see cref="PerFileResults"/>
    /// each point at the same loaded assembly but carry per-file component
    /// metadata so the controller can run the existing swap pipeline once
    /// per file without re-emitting.
    ///
    /// On failure (pre-compile guard tripped, Roslyn errors, or post-compile
    /// guard tripped) <see cref="OverallSuccess"/> is <c>false</c>,
    /// <see cref="OverallError"/> carries the user-facing reason, and
    /// <see cref="FallbackReason"/> tells the controller why per-file compile
    /// must be used as the safety net (per §5.2.1 of the Tech Debt plan:
    /// "loud regression preferred over silent wrong-IL").
    /// </summary>
    internal sealed class HmrBatchCompileResult
    {
        public bool OverallSuccess;
        public string OverallError;
        public string FallbackReason;
        public Assembly UnionAssembly;
        public List<HmrCompileResult> PerFileResults = new();
        public double TotalMs;
        public int BatchSize;
    }
}
