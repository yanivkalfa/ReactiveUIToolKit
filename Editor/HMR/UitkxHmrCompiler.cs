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
                var directives = _directiveParse.Invoke(
                    null,
                    new object[] { source, uitkxPath, diagList, true }
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
                    result.Error = "No @component directive found";
                    return result;
                }

                result.ComponentName = componentName;

                // ── 2. Parse AST ─────────────────────────────────────────────
                var astNodes = _uitkxParse.Invoke(
                    null,
                    new object[] { source, uitkxPath, directives, diagList }
                );
                stepSw.Stop();
                result.ParseMs = stepSw.Elapsed.TotalMilliseconds;

                // ── 3. Canonical lowering + Emit C# ──────────────────────────
                stepSw = Stopwatch.StartNew();
                var lowered = _canonicalLower.Invoke(
                    null,
                    new object[] { directives, astNodes, uitkxPath }
                );
                string csharp = HmrCSharpEmitter.Emit(directives, lowered, uitkxPath);
                stepSw.Stop();
                result.EmitMs = stepSw.Elapsed.TotalMilliseconds;

                // ── 4. Compile ───────────────────────────────────────────────
                stepSw = Stopwatch.StartNew();
                var sources = new List<string> { csharp };
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
                if (!_genuinelyNewComponents.Contains(componentName))
                    CheckIfGenuinelyNew(componentName);
            }
            catch (Exception ex)
            {
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

        public void Dispose()
        {
            _languageAsm = null;
            if (_roslynLoaded)
                AppDomain.CurrentDomain.AssemblyResolve -= RoslynAssemblyResolve;
            // Clean up temp directory
            if (_tempDir != null && Directory.Exists(_tempDir))
            {
                try
                {
                    Directory.Delete(_tempDir, true);
                }
                catch { }
            }
        }

        // ── Initialization helpers ────────────────────────────────────────────

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

        private void CheckIfGenuinelyNew(string componentName)
        {
            // Scan loaded assemblies (excluding HMR assemblies) for a type matching
            // the component name. If none found, the component is genuinely new and
            // needs to be added as a cross-reference for dependents.
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
                        if (type.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase))
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
            var syntaxTreeType = _roslynCSharpAsm.GetType(
                "Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree"
            );
            _parseText = syntaxTreeType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m =>
                    m.Name == "ParseText"
                    && m.GetParameters().Length >= 1
                    && m.GetParameters()[0].ParameterType == typeof(string)
                );

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

            // CSharpCompilation.Create(string, IEnumerable<SyntaxTree>, IEnumerable<MetadataReference>, CSharpCompilationOptions)
            var compilationType = _roslynCSharpAsm.GetType(
                "Microsoft.CodeAnalysis.CSharp.CSharpCompilation"
            );
            _compilationCreate = compilationType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Create" && m.GetParameters().Length == 4);

            // MetadataReference.CreateFromFile(string, MetadataReferenceProperties, DocumentationProvider)
            var metaRefType = _roslynCommonAsm.GetType("Microsoft.CodeAnalysis.MetadataReference");
            _createFromFile = metaRefType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m =>
                    m.Name == "CreateFromFile"
                    && m.GetParameters().Length > 0
                    && m.GetParameters()[0].ParameterType == typeof(string)
                );

            // Compilation.Emit(Stream peStream, ...) — many optional params, first is Stream
            var baseCompilationType = _roslynCommonAsm.GetType(
                "Microsoft.CodeAnalysis.Compilation"
            );
            _emitToStream = baseCompilationType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m =>
                    m.Name == "Emit"
                    && m.GetParameters().Length > 0
                    && m.GetParameters()[0].ParameterType == typeof(Stream)
                );
        }

        /// <summary>
        /// Invoke a method filling in default values for any optional parameters
        /// beyond those explicitly provided.
        /// </summary>
        private static object InvokeWithDefaults(
            MethodInfo method,
            object target,
            params object[] explicitArgs
        )
        {
            var parms = method.GetParameters();
            var allArgs = new object[parms.Length];
            for (int i = 0; i < parms.Length; i++)
            {
                if (i < explicitArgs.Length)
                {
                    allArgs[i] = explicitArgs[i];
                }
                else if (parms[i].HasDefaultValue)
                {
                    var def = parms[i].DefaultValue;
                    allArgs[i] = def is System.DBNull ? null : def;
                }
                else
                {
                    allArgs[i] = parms[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(parms[i].ParameterType)
                        : null;
                }
            }
            return method.Invoke(target, allArgs);
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

                // Build references: base refs + cross-component HMR refs (new components only)
                var allRefs = new List<object>(_metadataReferences);
                foreach (var kvp in _hmrAssemblyPaths)
                {
                    if (kvp.Key.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    // Only add cross-refs for genuinely new components;
                    // existing ones are already in _metadataReferences and would cause CS0433
                    if (!_genuinelyNewComponents.Contains(kvp.Key))
                        continue;
                    if (File.Exists(kvp.Value))
                    {
                        try
                        {
                            allRefs.Add(InvokeWithDefaults(_createFromFile, null, kvp.Value));
                        }
                        catch { }
                    }
                }

                // Create typed arrays via reflection (Roslyn expects IEnumerable<SyntaxTree> / IEnumerable<MetadataReference>)
                var syntaxTreeBaseType = _roslynCommonAsm.GetType(
                    "Microsoft.CodeAnalysis.SyntaxTree"
                );
                var treesArray = Array.CreateInstance(syntaxTreeBaseType, treesList.Count);
                for (int i = 0; i < treesList.Count; i++)
                    treesArray.SetValue(treesList[i], i);

                var metaRefType = _roslynCommonAsm.GetType(
                    "Microsoft.CodeAnalysis.MetadataReference"
                );
                var refsArray = Array.CreateInstance(metaRefType, allRefs.Count);
                for (int i = 0; i < allRefs.Count; i++)
                    refsArray.SetValue(allRefs[i], i);

                string asmName = $"hmr_{componentName}_{_swapCounter}";

                // CSharpCompilation.Create(assemblyName, syntaxTrees, references, options)
                var compilation = _compilationCreate.Invoke(
                    null,
                    new object[] { asmName, treesArray, refsArray, _compilationOptions }
                );

                // Emit to MemoryStream
                byte[] dllBytes;
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

                    dllBytes = ms.ToArray();
                }

                // Write DLL to disk for cross-component references
                string outputDll = Path.Combine(
                    _tempDir,
                    $"hmr_{componentName}_{_swapCounter}.dll"
                );
                File.WriteAllBytes(outputDll, dllBytes);

                // Register on disk for cross-component references; replace previous version
                if (
                    _hmrAssemblyPaths.TryGetValue(componentName, out string oldDll)
                    && oldDll != outputDll
                )
                    try
                    {
                        File.Delete(oldDll);
                    }
                    catch { }
                _hmrAssemblyPaths[componentName] = outputDll;

                return Assembly.Load(dllBytes);
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
                try
                {
                    File.Delete(oldDll);
                }
                catch { }
            _hmrAssemblyPaths[componentName] = outputDll;

            // Load from bytes (avoids file lock on the reference DLL)
            byte[] dllBytes = File.ReadAllBytes(outputDll);
            return Assembly.Load(dllBytes);
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
    }

    // ── Result ────────────────────────────────────────────────────────────────

    internal sealed class HmrCompileResult
    {
        public bool Success;
        public string Error;
        public string ComponentName;
        public Assembly LoadedAssembly;

        // Per-step timing (milliseconds)
        public double ParseMs;
        public double EmitMs;
        public double CompileMs;
        public double SwapMs;
        public double TotalMs;

        public string TimingBreakdown =>
            $"Parse: {ParseMs:F1}ms | Emit: {EmitMs:F1}ms | Compile: {CompileMs:F1}ms | Swap: {SwapMs:F1}ms | Total: {TotalMs:F1}ms";
    }
}
