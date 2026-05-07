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
        // ── Loaded pipeline assembly ──────────────────────────────────────────
        private Assembly _languageAsm;

        // ── Cached reflection handles ─────────────────────────────────────────
        private MethodInfo _directiveParse;
        private MethodInfo _uitkxParse;
        private MethodInfo _canonicalLower;
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
                string source = File.ReadAllText(uitkxPath);

                // ── 1. Parse directives ──────────────────────────────────────
                var stepSw = Stopwatch.StartNew();
                var diagList = CreateDiagnosticList();
                var directives = InvokeWithDefaults(
                    _directiveParse,
                    null,
                    source, uitkxPath, diagList, true
                );

                if (directives == null)
                {
                    result.Error = "DirectiveParser returned null";
                    return result;
                }

                string componentName = (string)GetProp(directives, "ComponentName");
                string ns = (string)GetProp(directives, "Namespace");

                if (string.IsNullOrEmpty(componentName))
                {
                    // ── Hook/module file path ────────────────────────────────
                    return CompileHookModuleFile(directives, uitkxPath, sw, result);
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
                    source, uitkxPath, directives, diagList, false, 0
                );
                stepSw.Stop();
                result.ParseMs = stepSw.Elapsed.TotalMilliseconds;

                // ── 3. Canonical lowering + Emit C# ──────────────────────────
                stepSw = Stopwatch.StartNew();
                var lowered = InvokeWithDefaults(
                    _canonicalLower,
                    null,
                    directives, astNodes, uitkxPath
                );

                // Build a delegate that can parse standalone JSX fragments.
                // Used by the emitter to splice embedded JSX in setup code.
                HmrCSharpEmitter.MarkupParseFunc parseMarkup = (jsxText, path, startLine) =>
                {
                    string synthetic = "@namespace __Tmp\n@component __Tmp\n" + jsxText;
                    var miniDiags = CreateDiagnosticList();
                    var miniDir = InvokeWithDefaults(
                        _directiveParse, null, synthetic, path, miniDiags, false);
                    var nodes = InvokeWithDefaults(
                        _uitkxParse, null, synthetic, path, miniDir, miniDiags, false, 0);
                    return GetItems(nodes);
                };

                string csharp = HmrCSharpEmitter.Emit(directives, lowered, uitkxPath, parseMarkup);
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
                            sources.Add(File.ReadAllText(csFile));
                    }
                }

                var asm = CompileSources(sources.ToArray(), componentName, out string compileError);
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
        /// Compiles a hook or module .uitkx file (no component keyword).
        /// Uses <see cref="HmrHookEmitter"/> to generate the C# source and
        /// compiles it the same way as component files.
        /// </summary>
        private HmrCompileResult CompileHookModuleFile(
            object directives, string uitkxPath, Stopwatch sw, HmrCompileResult result)
        {
            try
            {
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
                var asm = CompileSources(sources.ToArray(), containerClass, out string compileError);
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
            string uitkxPath, string componentName, string componentNs,
            List<string> sources, List<string> hookContainerFqns)
        {
            var dir = Path.GetDirectoryName(uitkxPath);
            if (dir == null) return;

            // Pattern: ComponentName.*.uitkx  (e.g. TicTacToe.style.uitkx)
            string prefix = componentName + ".";
            foreach (var file in Directory.GetFiles(dir, prefix + "*.uitkx"))
            {
                // Skip the component file itself
                if (string.Equals(file, uitkxPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    string companionSource = File.ReadAllText(file);
                    var diagList = CreateDiagnosticList();
                    var companionDir = InvokeWithDefaults(
                        _directiveParse, null, companionSource, file, diagList, true);
                    if (companionDir == null) continue;

                    // Emit module bodies (style constants, utility methods, etc.)
                    string moduleCSharp = HmrHookEmitter.EmitModules(companionDir, file);
                    if (!string.IsNullOrEmpty(moduleCSharp))
                        sources.Add(moduleCSharp);

                    // Emit hook bodies if the companion also defines custom hooks
                    string containerClass = HmrHookEmitter.DeriveContainerClassName(file);
                    string hookCSharp = HmrHookEmitter.Emit(
                        companionDir, file, containerClass, withTrampoline: true);
                    if (!string.IsNullOrEmpty(hookCSharp))
                    {
                        sources.Add(hookCSharp);

                        // Collect FQN so caller can inject using-static
                        string hookNs = (string)GetProp(companionDir, "Namespace");
                        if (string.IsNullOrEmpty(hookNs))
                            hookNs = componentNs ?? "ReactiveUITK.Generated";
                        hookContainerFqns.Add($"{hookNs}.{containerClass}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[HMR] Failed to process companion {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }

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
            _lastGenuineComponentCount = 0;
            _swapCounter = 0;

            // Clean up temp DLLs from this session
            if (_tempDir != null && Directory.Exists(_tempDir))
            {
                try
                {
                    foreach (var file in Directory.GetFiles(_tempDir))
                    {
                        try { File.Delete(file); }
                        catch { /* locked by LoadFrom — will be cleaned next time */ }
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
                try { Directory.Delete(_tempDir, false); }
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
                    try { File.Delete(file); }
                    catch { /* still locked — leave for next time */ }
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
        }

        private object CreateDiagnosticList()
        {
            var listType = typeof(List<>).MakeGenericType(_parseDiagnosticType);
            return Activator.CreateInstance(listType);
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
                .FirstOrDefault(m => m.Name == "RemoveSyntaxTrees" && m.GetParameters().Length == 1);

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

        private Assembly CompileSources(string[] sources, string componentName, out string error)
        {
            _swapCounter++;
            error = null;

            // ── Fast path: in-process Roslyn ──────────────────────────────────
            if (_roslynLoaded)
            {
                var asm = InProcessCompile(sources, componentName, out error);
                if (asm != null || error == null)
                    return asm;
                // If in-process failed with an error, fall through to external
                Debug.LogWarning($"[HMR] In-process compile failed, trying external: {error}");
                error = null;
            }

            // ── Slow path: external dotnet csc.dll ────────────────────────────
            return ExternalCompile(sources, componentName, out error);
        }

        private Assembly InProcessCompile(string[] sources, string componentName, out string error)
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
                object compilation = TryBuildIncremental(componentName, newTrees, crossRefs, asmName);

                // ── Fallback: fresh Compilation.Create ────────────────────────
                if (compilation == null)
                    compilation = BuildFreshCompilation(newTrees, crossRefs, asmName);

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
                            compilation = BuildFreshCompilation(newTrees, crossRefs, asmName);

                            ms.SetLength(0);
                            emitResult = InvokeWithDefaults(_emitToStream, compilation, ms);
                            success = (bool)
                                emitResult
                                    .GetType()
                                    .GetProperty("Success", BindingFlags.Public | BindingFlags.Instance)
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
                if (
                    _hmrAssemblyPaths.TryGetValue(componentName, out oldDll)
                    && oldDll != outputDll
                )
                {
                    // Invalidate cached cross-ref for the old DLL
                    _crossRefCache.Remove(componentName);
                    try { File.Delete(oldDll); }
                    catch { /* may be locked by LoadFrom — cleaned on next session */ }
                }
                _hmrAssemblyPaths[componentName] = outputDll;

                // Use LoadFrom (memory-mapped) instead of Load(byte[]) to avoid
                // copying the PE image into the managed heap
                var loadedAsm = Assembly.LoadFrom(outputDll);

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
            string asmName)
        {
            // Incremental API handles must all be available
            if (_compilationRemoveSyntaxTrees == null
                || _compilationAddSyntaxTrees == null
                || _compilationWithAssemblyName == null)
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
                if (_cachedSyntaxTrees.TryGetValue(componentName, out var oldTrees) && oldTrees.Length > 0)
                {
                    var syntaxTreeBaseType = _roslynCommonAsm.GetType(
                        "Microsoft.CodeAnalysis.SyntaxTree"
                    );
                    var oldArray = Array.CreateInstance(syntaxTreeBaseType, oldTrees.Length);
                    for (int i = 0; i < oldTrees.Length; i++)
                        oldArray.SetValue(oldTrees[i], i);
                    cached = _compilationRemoveSyntaxTrees.Invoke(cached, new object[] { oldArray });
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
            string asmName)
        {
            var allRefs = new List<object>(_metadataReferences);
            allRefs.AddRange(crossRefs);

            var syntaxTreeBaseType = _roslynCommonAsm.GetType(
                "Microsoft.CodeAnalysis.SyntaxTree"
            );
            var treesArray = Array.CreateInstance(syntaxTreeBaseType, newTrees.Length);
            for (int i = 0; i < newTrees.Length; i++)
                treesArray.SetValue(newTrees[i], i);

            var metaRefType = _roslynCommonAsm.GetType(
                "Microsoft.CodeAnalysis.MetadataReference"
            );
            var refsArray = Array.CreateInstance(metaRefType, allRefs.Count);
            for (int i = 0; i < allRefs.Count; i++)
                refsArray.SetValue(allRefs[i], i);

            return _compilationCreate.Invoke(
                null,
                new object[] { asmName, treesArray, refsArray, _compilationOptions }
            );
        }

        private Assembly ExternalCompile(string[] sources, string componentName, out string error)
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
                foreach (var loc in _referenceLocations)
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
                try { File.Delete(oldDll); }
                catch { /* may be locked by LoadFrom */ }
            }
            _hmrAssemblyPaths[componentName] = outputDll;

            // Use LoadFrom (memory-mapped) instead of Load(byte[])
            var loadedAsm = Assembly.LoadFrom(outputDll);

            // Force GC to reclaim dead allocations before Mono expands the heap
            GC.Collect(2, GCCollectionMode.Optimized);

            return loadedAsm;
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
            if (m == null) return;
            lock (_silentDriftMethodsLock) _silentDriftMethods.Add(m);
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
            if (declaringType == null) return null;

            MethodInfo allOptionalBest = null;
            int allOptionalBestLen = int.MaxValue;
            MethodInfo anyMatchBest = null;
            int anyMatchBestLen = int.MaxValue;

            foreach (var m in declaringType.GetMethods(flags))
            {
                if (m.Name != name) continue;
                var ps = m.GetParameters();
                if (ps.Length == 0) continue;
                if (ps[0].ParameterType != firstParamType) continue;

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
                if (!tailAllOptional) continue;
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
            params object[] args)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            args ??= Array.Empty<object>();

            var parameters = method.GetParameters();
            if (args.Length == parameters.Length)
                return method.Invoke(target, args);

            if (args.Length > parameters.Length)
            {
                throw new ArgumentException(
                    $"[HMR] {method.DeclaringType?.Name}.{method.Name}: HMR passed " +
                    $"{args.Length} args but the loaded language library declares " +
                    $"{parameters.Length} parameter(s). The HMR compiler must be updated.");
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
                        $"[HMR] {method.DeclaringType?.Name}.{method.Name}: missing " +
                        $"required argument '{p.Name}' (position {i}). The HMR compiler " +
                        $"is out of sync with the loaded language library.");
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
                    $"[HMR] {method.DeclaringType?.Name}.{method.Name}: HMR passed " +
                    $"{args.Length} args but the loaded language library declares " +
                    $"{parameters.Length} parameter(s); padded missing tail with " +
                    $"compile-time defaults. Update the HMR compiler to pass the " +
                    $"new arguments explicitly.");
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
            if (ex == null) return false;
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
    }

    // ── Result ────────────────────────────────────────────────────────────────

    internal sealed class HmrCompileResult
    {
        public bool Success;
        public string Error;
        public string ComponentName;
        public Assembly LoadedAssembly;

        /// <summary>
        /// True when this result is from a hook/module .uitkx file
        /// (as opposed to a component file). The controller uses this
        /// to route to the hook delegate swapper instead of the
        /// component fiber swapper.
        /// </summary>
        public bool IsHookModuleFile;

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
}
