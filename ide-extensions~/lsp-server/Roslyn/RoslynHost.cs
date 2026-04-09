using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ReactiveUITK.Language.Diagnostics;
using ReactiveUITK.Language.Parser;
using ReactiveUITK.Language.Roslyn;
using ReactiveUITK.Language.SemanticTokens;
using CSharpParseOptions = Microsoft.CodeAnalysis.CSharp.CSharpParseOptions;

namespace UitkxLanguageServer.Roslyn
{
    /// <summary>
    /// Manages a Roslyn <see cref="AdhocWorkspace"/> (one per open .uitkx file) and
    /// exposes async methods for type-system queries: diagnostics, completions,
    /// semantic tokens, and hover.
    ///
    /// <para><b>Lifetime:</b> Singleton.  Registered in DI, injected into
    /// <see cref="UitkxLanguageServer.DiagnosticsPublisher"/> and the handler classes
    /// that need Roslyn features.</para>
    ///
    /// <para><b>Threading model:</b>
    /// <list type="bullet">
    ///   <item>Each open file has its own <see cref="FileState"/> managed by a
    ///   <see cref="SemaphoreSlim"/>, guaranteeing that workspace rebuilds for the
    ///   same file are serialised.</item>
    ///   <item>Rebuilds are debounced (300 ms default) so a burst of rapid edits
    ///   triggers only one compilation.</item>
    ///   <item>The <see cref="ReferenceAssemblyLocator"/> caches references; its
    ///   cache is cleared when Unity recompiles (DLL file-change event).</item>
    /// </list></para>
    ///
    /// <para><b>language-lib constraint:</b> This class lives in <c>lsp-server</c>
    /// and is the ONLY layer that directly references the Roslyn NuGet packages.
    /// The <see cref="VirtualDocumentGenerator"/> and <see cref="SourceMap"/> live
    /// in <c>language-lib</c> and are Roslyn-free.</para>
    /// </summary>
    public sealed class RoslynHost : IDisposable
    {
        // ── Constants ─────────────────────────────────────────────────────────

        private const int DebounceMs = 300;

        private static readonly CSharpParseOptions s_parseOptions =
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

        private static readonly CSharpCompilationOptions s_compilationOptions =
            new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: false,
                nullableContextOptions: NullableContextOptions.Enable,
                reportSuppressedDiagnostics: false
            ).WithSpecificDiagnosticOptions(
                // Suppress nuisance diagnostics that are false-positives in the
                // virtual document scaffold.  User-configurable in the future.
                new Dictionary<string, ReportDiagnostic>
                {
                    // CS0246 removed: targeted #pragma warning disable CS0246 in the
                    // virtual document now covers scaffold-specific occurrences, so
                    // real user type-not-found errors can surface to the editor.
                    ["CS8019"] = ReportDiagnostic.Suppress, // unnecessary using directive
                    ["CS1591"] = ReportDiagnostic.Suppress, // missing XML comment
                    ["CS0649"] = ReportDiagnostic.Suppress, // field never assigned
                    ["CS0414"] = ReportDiagnostic.Suppress, // field assigned but never used
                    ["CS8618"] = ReportDiagnostic.Suppress, // non-nullable field uninitialized
                    ["CS0169"] = ReportDiagnostic.Suppress, // field never used
                    ["CS8632"] = ReportDiagnostic.Suppress, // nullable annotation outside #nullable context (auto-generated)
                    ["CS8974"] = ReportDiagnostic.Suppress, // converting method group to non-delegate type 'object'
                    // CS1977 fires when a lambda is passed as an argument to a dynamically-
                    // dispatched method call (e.g. `dm.AppendAction("X", _ => ...)` where
                    // `dm` is typed as `dynamic` in the block-body local function scaffold).
                    // This is a false-positive caused by the virtual document; real user code
                    // does compile correctly at runtime.  CS1977 cannot be suppressed via
                    // #pragma warning disable, so we suppress it at the compilation level.
                    ["CS1977"] = ReportDiagnostic.Suppress, // cannot use lambda as arg to dynamic dispatch (virtual-doc false positive)
                    // CS0436: source type conflicts with imported type — companion .cs files
                    // loaded alongside the virtual document shadow the same types in
                    // Assembly-CSharp.dll. This is intentional and harmless.
                    ["CS0436"] = ReportDiagnostic.Suppress, // source type shadows imported type (companion injection)
                    // CS0219: unused local variable — suppressed because UITKX0112
                    // (data-flow–based unused-variable detection) covers the same
                    // ground and also catches initialisers with side-effects that
                    // CS0219 ignores (e.g. `new Style { … }`).
                    ["CS0219"] = ReportDiagnostic.Suppress, // unused variable → handled by UITKX0112
                    // CS8321: unused local function — promoted to Error to match CS0219.
                    ["CS8321"] = ReportDiagnostic.Error, // unused local function → error
                }
            );

        /// <summary>
        /// Descriptor for UITKX0112 — variable declared but never read.
        /// Created once and reused for every synthetic diagnostic instance.
        /// </summary>
        private static readonly DiagnosticDescriptor s_unusedVariableDescriptor =
            new DiagnosticDescriptor(
                id: DiagnosticCodes.UnusedVariable,
                title: "Unused variable",
                messageFormat: "The variable '{0}' is declared but never used",
                category: "UITKX",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            );

        // ── Inner types ───────────────────────────────────────────────────────

        /// <summary>Per-file compilation state.</summary>
        private sealed class FileState : IDisposable
        {
            /// <summary>Serialises workspace updates for this file.</summary>
            public readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);

            public AdhocWorkspace? Workspace;
            public ProjectId? ProjectId;
            public DocumentId? DocumentId;

            /// <summary>Document IDs for companion .cs files loaded from the same directory.</summary>
            public List<DocumentId> CompanionDocIds = new List<DocumentId>();
            public VirtualDocument? VirtualDoc;

            /// <summary>Hash of the source text used for the last virtual-doc build.
            /// <see cref="EnsureReadyAsync"/> uses this to skip redundant rebuilds.</summary>
            public string LastBuiltSource = "";

            /// <summary>Debounce timer for rebuild requests.</summary>
            public Timer? DebounceTimer;
            public CancellationTokenSource RebuildCts = new CancellationTokenSource();

            public void Dispose()
            {
                DebounceTimer?.Dispose();
                RebuildCts.Dispose();
                Workspace?.Dispose();
                Gate.Dispose();
            }
        }

        // ── State ─────────────────────────────────────────────────────────────

        private readonly ConcurrentDictionary<string, FileState> _files = new ConcurrentDictionary<
            string,
            FileState
        >(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Overlay of companion .cs text set after a rename so that the next
        /// <see cref="UpdateWorkspace"/> call reads the post-rename content
        /// instead of (potentially stale) disk content.
        /// Keyed by full path (case-insensitive).
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _companionOverlay =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly ReferenceAssemblyLocator _refLocator;
        private readonly VirtualDocumentGenerator _docGenerator = new VirtualDocumentGenerator();
        private readonly RoslynSemanticTokensProvider _roslynTokensProvider = new RoslynSemanticTokensProvider();
        private readonly ILanguageServerFacade _server;
        private readonly IPropsTypeProvider _propsTypes;

        private string? _workspaceRoot;

        private FileSystemWatcher? _dllWatcher;
        private bool _disposed;

        // ── Construction ──────────────────────────────────────────────────────

        public RoslynHost(
            ILanguageServerFacade server,
            UitkxSchema schema,
            WorkspaceIndex workspaceIndex
        )
        {
            _server = server;
            _refLocator = new ReferenceAssemblyLocator();
            _propsTypes = new PropsTypeAdapter(schema, workspaceIndex);
        }

        // ── Workspace root (set once on server start) ─────────────────────────

        /// <summary>Returns the workspace root path, or <c>null</c> if not yet set.</summary>
        public string? WorkspaceRoot => _workspaceRoot;

        /// <summary>Returns paths of all .uitkx files that have a tracked Roslyn workspace.</summary>
        public IReadOnlyCollection<string> GetAllTrackedPaths() => _files.Keys.ToArray();

        /// <summary>
        /// The Unity Editor version detected from the project's <c>ProjectVersion.txt</c>.
        /// Returns <see cref="UnityVersion.Unknown"/> if not yet resolved or not a Unity project.
        /// </summary>
        public UnityVersion DetectedUnityVersion => _refLocator.DetectedVersion;

        /// <summary>
        /// Informs the host of the workspace root so it can discover Unity
        /// assemblies and set up a DLL-change watcher.  Call this from
        /// <see cref="OmniSharp.Extensions.LanguageServer.Protocol.Server.IOnLanguageServerStarted.OnStarted"/>.
        /// </summary>
        public void SetWorkspaceRoot(string? root)
        {
            _workspaceRoot = root;
            SetupDllWatcher(root);
        }

        // ── Document management ───────────────────────────────────────────────

        /// <summary>
        /// Schedules a debounced rebuild of the Roslyn workspace for
        /// <paramref name="uitkxFilePath"/> using the latest source text and
        /// parse result.
        ///
        /// The rebuild runs on a background thread — this method returns immediately.
        /// When diagnostics are ready they are pushed to the LSP client via
        /// <see cref="DiagnosticsPublisher"/>.
        /// </summary>
        public void EnqueueRebuild(
            string uitkxFilePath,
            string source,
            ParseResult parseResult,
            DiagnosticsPublisher publisher
        )
        {
            if (string.IsNullOrEmpty(uitkxFilePath))
                return;

            var state = _files.GetOrAdd(uitkxFilePath, _ => new FileState());

            // Cancel any previously-scheduled rebuild for this file
            state.DebounceTimer?.Dispose();
            state.DebounceTimer = new Timer(
                callback: _ =>
                {
                    // Fire-and-forget async rebuild
                    _ = RebuildAsync(uitkxFilePath, source, parseResult, publisher, state);
                },
                state: null,
                dueTime: DebounceMs,
                period: Timeout.Infinite
            );
        }

        /// <summary>Removes a file from the host (called on <c>textDocument/didClose</c>).</summary>
        public void CloseDocument(string uitkxFilePath)
        {
            if (_files.TryRemove(uitkxFilePath, out var state))
                state.Dispose();
        }

        // ── Async Roslyn operations ───────────────────────────────────────────

        /// <summary>
        /// Returns a snapshot of the latest Roslyn <see cref="Diagnostic"/> objects
        /// for <paramref name="uitkxFilePath"/>, or an empty array if the workspace
        /// has not yet been built.
        ///
        /// Diagnostics are already filtered to remove scaffold-induced false-positives.
        /// </summary>
        public IReadOnlyList<(
            Diagnostic Diagnostic,
            SourceMapEntry? MapEntry
        )> GetLatestDiagnostics(string uitkxFilePath)
        {
            if (
                !_files.TryGetValue(uitkxFilePath, out var state)
                || state.Workspace == null
                || state.DocumentId == null
                || state.VirtualDoc == null
            )
                return Array.Empty<(Diagnostic, SourceMapEntry?)>();

            try
            {
                var doc = state.Workspace.CurrentSolution.GetDocument(state.DocumentId);
                if (doc == null)
                    return Array.Empty<(Diagnostic, SourceMapEntry?)>();

                var semantic = doc.GetSemanticModelAsync().GetAwaiter().GetResult();
                if (semantic == null)
                    return Array.Empty<(Diagnostic, SourceMapEntry?)>();

                var map = state.VirtualDoc.Map;
                var result = new List<(Diagnostic, SourceMapEntry?)>();

                foreach (var diag in semantic.GetDiagnostics())
                {
                    if (diag.Severity == DiagnosticSeverity.Hidden)
                        continue;

                    // #pragma warning disable in the virtual document marks a
                    // diagnostic as suppressed (IsSuppressed = true).  Don't
                    // surface those to the user.
                    if (diag.IsSuppressed)
                        continue;

                    // CS1026/CS1513 are Roslyn error-recovery cascade codes that
                    // fire inside multi-line collection-initializer lambdas when a
                    // containing type is unresolved.  CS0411 is lambda delegate-type
                    // inference noise from expression-check scaffolding.  All three
                    // are artefacts of the virtual document — never real user errors.
                    // Suppress them via explicit id check as a backstop in case
                    // #pragma warning disable does not mark them IsSuppressed.
                    if (diag.Id == "CS1026" || diag.Id == "CS1513" || diag.Id == "CS0411")
                        continue;

                    // Map back to uitkx coordinates via source-map or #line info
                    var mapped = TryMapDiagnostic(diag, map);
                    result.Add((diag, mapped));
                }

                // ── Data-flow analysis: unused-variable detection ─────────────
                // Roslyn's CS0219 only fires for constant-value assignments.
                // Variables initialised with `new Foo { … }` (object / collection
                // initialisers) are invisible to CS0219 because the constructor
                // and Add() calls count as potential side-effects.
                //
                // AnalyzeDataFlow on the __uitkx_render() method body captures
                // *all* local reads/writes and lets us report the broader set as
                // UITKX0112.  Scaffold locals (__uitkx_*) and variables whose
                // declaration doesn't map back to .uitkx source are skipped.
                try
                {
                    var root = semantic.SyntaxTree.GetRoot();
                    MethodDeclarationSyntax? renderMethod = null;
                    foreach (var node in root.DescendantNodes())
                    {
                        if (
                            node is MethodDeclarationSyntax mds
                            && mds.Identifier.Text == "__uitkx_render"
                        )
                        {
                            renderMethod = mds;
                            break;
                        }
                    }

                    if (renderMethod?.Body != null)
                    {
                        var dataFlow = semantic.AnalyzeDataFlow(renderMethod.Body);
                        if (dataFlow != null && dataFlow.Succeeded)
                        {
                            var readSet = new HashSet<ISymbol>(
                                dataFlow.ReadInside,
                                SymbolEqualityComparer.Default
                            );

                            foreach (var local in dataFlow.VariablesDeclared)
                            {
                                // Skip scaffold variables (all prefixed __uitkx_)
                                if (local.Name.StartsWith("__uitkx_"))
                                    continue;

                                // Skip discard-convention names: _ or _prefixed
                                // (standard C# convention for intentionally unused)
                                if (local.Name.StartsWith("_"))
                                    continue;

                                // Skip if the variable is read anywhere in the method
                                if (readSet.Contains(local))
                                    continue;

                                // Get declaration location in the virtual document
                                if (local.Locations.IsEmpty || !local.Locations[0].IsInSource)
                                    continue;
                                var loc = local.Locations[0];

                                // Only report variables whose position maps back to
                                // user-authored .uitkx source (filters out scaffold)
                                var mapResult = map.ToUitkxOffset(loc.SourceSpan.Start);
                                if (!mapResult.HasValue)
                                    continue;

                                // Only flag variables declared in setup code or
                                // @code blocks — NOT lambda parameters inside
                                // expression checks (AttributeExpression /
                                // InlineExpression regions).
                                var regionKind = mapResult.Value.Entry.Kind;
                                if (
                                    regionKind != SourceRegionKind.FunctionSetup
                                    && regionKind != SourceRegionKind.CodeBlock
                                )
                                    continue;

                                var synthDiag = Diagnostic.Create(
                                    s_unusedVariableDescriptor,
                                    loc,
                                    local.Name
                                );
                                result.Add((synthDiag, mapResult.Value.Entry));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Data-flow failure is non-fatal — regular diagnostics still
                    // surface; just log and continue.
                    ServerLog.Log(
                        $"[RoslynHost] AnalyzeDataFlow error: {ex.Message}"
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynHost] GetLatestDiagnostics error: {ex.Message}");
                return Array.Empty<(Diagnostic, SourceMapEntry?)>();
            }
        }

        /// <summary>
        /// Returns the current <see cref="VirtualDocument"/> for
        /// <paramref name="uitkxFilePath"/>, or <c>null</c> if not yet built.
        /// Used by completions, hover, and semantic-token handlers.
        /// </summary>
        public VirtualDocument? GetVirtualDocument(string uitkxFilePath)
        {
            if (_files.TryGetValue(uitkxFilePath, out var state))
                return state.VirtualDoc;
            return null;
        }

        /// <summary>
        /// Returns the Roslyn <see cref="Document"/> from the current solution for
        /// <paramref name="uitkxFilePath"/>, or <c>null</c>.
        /// Callers that need <see cref="SemanticModel"/> or completion services
        /// should use this entry point.
        /// </summary>
        public Document? GetRoslynDocument(string uitkxFilePath)
        {
            if (
                !_files.TryGetValue(uitkxFilePath, out var state)
                || state.Workspace == null
                || state.DocumentId == null
            )
                return null;

            return state.Workspace.CurrentSolution.GetDocument(state.DocumentId);
        }

        /// <summary>
        /// Searches all tracked .uitkx workspaces for a companion .cs document
        /// whose file-name matches <paramref name="csFilePath"/>.
        /// Returns the companion <see cref="Document"/>, the owning .uitkx path,
        /// and the main virtual-document <see cref="DocumentId"/>, or <c>null</c>.
        /// </summary>
        public (Document CompanionDoc, string UitkxPath, DocumentId MainDocId, VirtualDocument? VDoc)?
            FindCompanionDocument(string csFilePath)
        {
            var csFileName = System.IO.Path.GetFileName(csFilePath);
            var csDir = System.IO.Path.GetDirectoryName(csFilePath);

            foreach (var kv in _files)
            {
                var uitkxDir = System.IO.Path.GetDirectoryName(kv.Key);
                if (!string.Equals(uitkxDir, csDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                var state = kv.Value;
                if (state.Workspace == null || state.DocumentId == null)
                    continue;

                var solution = state.Workspace.CurrentSolution;
                foreach (var companionDocId in state.CompanionDocIds)
                {
                    var doc = solution.GetDocument(companionDocId);
                    if (doc != null && string.Equals(doc.Name, csFileName, StringComparison.OrdinalIgnoreCase))
                        return (doc, kv.Key, state.DocumentId, state.VirtualDoc);
                }
            }

            return null;
        }

        /// <summary>
        /// Updates a companion document's text in the Roslyn workspace so that
        /// subsequent symbol resolution uses the latest content (e.g. after
        /// a rename applied edits to the .cs file from the .uitkx side).
        /// Returns the refreshed <see cref="Document"/>, or <c>null</c>.
        /// </summary>
        public Document? RefreshCompanionDocument(string csFilePath, string currentText)
        {
            // Also update the overlay so a concurrent UpdateWorkspace doesn't
            // overwrite with stale disk content.
            _companionOverlay[csFilePath] = currentText;

            var csFileName = System.IO.Path.GetFileName(csFilePath);
            var csDir = System.IO.Path.GetDirectoryName(csFilePath);

            foreach (var kv in _files)
            {
                var uitkxDir = System.IO.Path.GetDirectoryName(kv.Key);
                if (!string.Equals(uitkxDir, csDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                var state = kv.Value;
                if (state.Workspace == null || state.DocumentId == null)
                    continue;

                foreach (var companionDocId in state.CompanionDocIds)
                {
                    var doc = state.Workspace.CurrentSolution.GetDocument(companionDocId);
                    if (doc == null || !string.Equals(doc.Name, csFileName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Replace the document text in the workspace
                    var newSourceText = Microsoft.CodeAnalysis.Text.SourceText.From(currentText);
                    var newSolution = state.Workspace.CurrentSolution.WithDocumentText(companionDocId, newSourceText);
                    state.Workspace.TryApplyChanges(newSolution);
                    return state.Workspace.CurrentSolution.GetDocument(companionDocId);
                }
            }

            return null;
        }

        /// <summary>
        /// Stores post-rename companion .cs text so the next workspace rebuild
        /// uses this content instead of (potentially stale) disk content.
        /// </summary>
        public void SetCompanionOverlay(string csPath, string text)
        {
            _companionOverlay[csPath] = text;
        }

        /// <summary>
        /// Returns all tracked .uitkx file paths in the same directory as
        /// <paramref name="csFilePath"/>. Used to trigger diagnostic
        /// re-evaluation when a companion .cs file changes.
        /// </summary>
        public IReadOnlyList<string> FindUitkxFilesForCompanion(string csFilePath)
        {
            var csDir = System.IO.Path.GetDirectoryName(csFilePath);
            if (csDir == null) return Array.Empty<string>();

            var result = new List<string>();
            foreach (var kv in _files)
            {
                var uitkxDir = System.IO.Path.GetDirectoryName(kv.Key);
                if (string.Equals(uitkxDir, csDir, StringComparison.OrdinalIgnoreCase))
                    result.Add(kv.Key);
            }
            return result;
        }

        /// <summary>
        /// Ensures the Roslyn workspace for <paramref name="uitkxFilePath"/> is
        /// built and ready for IDE queries (completions, hover, semantic tokens).
        ///
        /// <para>If the workspace already has a document the method returns
        /// immediately.  Otherwise it performs an immediate one-shot rebuild —
        /// bypassing the debounce delay — so the caller does not have to wait for
        /// the next <see cref="EnqueueRebuild"/> cycle to fire.</para>
        ///
        /// <para>This method does <em>not</em> push diagnostics to the client.</para>
        /// </summary>
        public async Task EnsureReadyAsync(
            string uitkxFilePath,
            string source,
            ParseResult parseResult,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrEmpty(uitkxFilePath))
                return;

            var state = _files.GetOrAdd(uitkxFilePath, _ => new FileState());

            // Fast path: workspace exists and source hasn't changed since last build.
            if (
                state.Workspace != null
                && state.DocumentId != null
                && state.LastBuiltSource == source
            )
                return;

            await state.Gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // Double-checked after acquiring the gate — must recheck LastBuiltSource too,
                // otherwise a concurrent EnsureReadyAsync that raced here first would have
                // already rebuilt with the new source, and we can skip.
                if (
                    state.Workspace != null
                    && state.DocumentId != null
                    && state.LastBuiltSource == source
                )
                    return;

                var virtualDoc = _docGenerator.Generate(parseResult, source, uitkxFilePath, _propsTypes);
                UpdateWorkspace(state, uitkxFilePath, virtualDoc, ct);
                state.VirtualDoc = virtualDoc;
                state.LastBuiltSource = source;

                ServerLog.Log(
                    $"[RoslynHost] EnsureReadyAsync: immediate build for {System.IO.Path.GetFileName(uitkxFilePath)}"
                );
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynHost] EnsureReadyAsync error: {ex.Message}");
            }
            finally
            {
                state.Gate.Release();
            }
        }

        // ── Rebuild logic ─────────────────────────────────────────────────────

        private async Task RebuildAsync(
            string uitkxFilePath,
            string source,
            ParseResult parseResult,
            DiagnosticsPublisher publisher,
            FileState state
        )
        {
            // Acquire the per-file gate to serialise rebuilds
            await state.Gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var cts = new CancellationTokenSource();
                var old = Interlocked.Exchange(ref state.RebuildCts, cts);
                old.Cancel();
                old.Dispose();

                var ct = cts.Token;

                // 1. Generate virtual document
                var virtualDoc = _docGenerator.Generate(parseResult, source, uitkxFilePath, _propsTypes);

                // 2. Update (or create) the AdhocWorkspace for this file
                UpdateWorkspace(state, uitkxFilePath, virtualDoc, ct);

                // 3. Store the new virtual document on state
                state.VirtualDoc = virtualDoc;
                state.LastBuiltSource = source;

                if (ct.IsCancellationRequested)
                    return;

                // 4. Push T3 diagnostics through the publisher
                publisher.PushTier3(uitkxFilePath, GetLatestDiagnostics(uitkxFilePath), source);

                // 5. Push classification overrides to VS2022 (custom notification).
                //    VSCode uses LSP semantic tokens for delegate coloring; VS2022
                //    cannot consume semantic tokens, so we push overrides via a
                //    custom uitkx/classificationOverrides notification instead.
                if (CapabilityPatchStream.IsVisualStudio && !ct.IsCancellationRequested)
                {
                    ServerLog.Log($"[RoslynHost] IsVisualStudio gate passed, calling PushClassificationOverridesAsync for '{System.IO.Path.GetFileName(uitkxFilePath)}'");
                    try
                    {
                        await PushClassificationOverridesAsync(
                            uitkxFilePath, source, state, ct
                        ).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { /* next rebuild will send */ }
                    catch (Exception ex)
                    {
                        ServerLog.Log($"[RoslynHost] PushClassificationOverrides error: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal: a newer rebuild was requested before this one completed
            }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynHost] RebuildAsync error for {uitkxFilePath}: {ex}");
            }
            finally
            {
                state.Gate.Release();
            }
        }

        /// <summary>
        /// Sends a <c>uitkx/classificationOverrides</c> notification to VS2022
        /// containing delegate-typed identifiers that need to be reclassified
        /// from <c>identifier</c> to <c>method</c> (function call colour).
        /// </summary>
        private async Task PushClassificationOverridesAsync(
            string uitkxFilePath,
            string source,
            FileState state,
            CancellationToken ct
        )
        {
            var roslynDoc = GetRoslynDocument(uitkxFilePath);
            var vDoc = state.VirtualDoc;
            if (roslynDoc == null || vDoc == null)
            {
                ServerLog.Log(
                    $"[RoslynHost] PushClassificationOverrides skipped — "
                    + $"roslynDoc={roslynDoc != null}, vDoc={vDoc != null} "
                    + $"for '{System.IO.Path.GetFileName(uitkxFilePath)}'"
                );
                return;
            }

            var overrides = await _roslynTokensProvider
                .GetDelegateOverridesAsync(roslynDoc, vDoc.Map, source, ct)
                .ConfigureAwait(false);

            // Always send — an empty array clears stale overrides from a prior edit.
            _server.SendNotification("uitkx/classificationOverrides", new
            {
                uri = DocumentUri.File(uitkxFilePath).ToString(),
                overrides = overrides,
            });

            ServerLog.Log(
                $"[RoslynHost] Pushed {overrides.Length} classification overrides for "
                + $"'{System.IO.Path.GetFileName(uitkxFilePath)}'"
            );
        }

        private void UpdateWorkspace(
            FileState state,
            string uitkxFilePath,
            VirtualDocument virtualDoc,
            CancellationToken ct
        )
        {
            var refs = _refLocator.GetReferences(_workspaceRoot);

            if (state.Workspace == null)
            {
                // ── First open: create workspace + project + document ────────
                // MefHostServices.DefaultHost loads C# MEF composition so that
                // CompletionService.GetService(document) returns non-null.
                var ws = new AdhocWorkspace(MefHostServices.DefaultHost);

                var projectInfo = ProjectInfo.Create(
                    id: ProjectId.CreateNewId(debugName: uitkxFilePath),
                    version: VersionStamp.Create(),
                    name: System.IO.Path.GetFileNameWithoutExtension(uitkxFilePath),
                    assemblyName: "UitkxVirtual",
                    language: LanguageNames.CSharp,
                    parseOptions: s_parseOptions,
                    compilationOptions: s_compilationOptions,
                    metadataReferences: refs
                );

                var project = ws.AddProject(projectInfo);

                var docInfo = DocumentInfo.Create(
                    id: DocumentId.CreateNewId(project.Id, debugName: uitkxFilePath),
                    name: System.IO.Path.GetFileName(uitkxFilePath) + ".g.cs",
                    sourceCodeKind: SourceCodeKind.Regular,
                    loader: TextLoader.From(
                        TextAndVersion.Create(
                            Microsoft.CodeAnalysis.Text.SourceText.From(virtualDoc.Text),
                            VersionStamp.Create(),
                            uitkxFilePath
                        )
                    )
                );

                var doc = ws.AddDocument(docInfo);

                state.Workspace = ws;
                state.ProjectId = project.Id;
                state.DocumentId = doc.Id;

                // ── Load companion .cs files from the same directory ─────────
                AddCompanionDocuments(ws, project.Id, state, uitkxFilePath);
            }
            else
            {
                // ── Subsequent update: replace document text ──────────────────
                ct.ThrowIfCancellationRequested();

                var currentSolution = state.Workspace.CurrentSolution;
                var doc = currentSolution.GetDocument(state.DocumentId!);

                if (doc == null)
                    return; // workspace was disposed — bail

                var newSolution = currentSolution.WithDocumentText(
                    state.DocumentId!,
                    Microsoft.CodeAnalysis.Text.SourceText.From(virtualDoc.Text)
                );

                // Also update metadata references in case Unity recompiled
                newSolution = newSolution.WithProjectMetadataReferences(state.ProjectId!, refs);

                // ── Refresh companion .cs files ──────────────────────────────
                // Remove old companions and re-add with fresh content so that
                // edits to .style.cs / .util.cs / etc. are picked up.
                foreach (var oldId in state.CompanionDocIds)
                    newSolution = newSolution.RemoveDocument(oldId);
                state.CompanionDocIds.Clear();

                var companions = FindCompanionFiles(uitkxFilePath);
                foreach (var companionPath in companions)
                {
                    try
                    {
                        // Prefer overlay (set after a rename) over disk to
                        // avoid reading stale content from an unsaved file.
                        if (!_companionOverlay.TryRemove(companionPath, out var companionText))
                            companionText = System.IO.File.ReadAllText(companionPath);
                        var newDocId = DocumentId.CreateNewId(
                            state.ProjectId!,
                            debugName: companionPath
                        );
                        var companionDocInfo = DocumentInfo.Create(
                            id: newDocId,
                            name: System.IO.Path.GetFileName(companionPath),
                            sourceCodeKind: SourceCodeKind.Regular,
                            loader: TextLoader.From(
                                TextAndVersion.Create(
                                    Microsoft.CodeAnalysis.Text.SourceText.From(companionText),
                                    VersionStamp.Create(),
                                    companionPath
                                )
                            )
                        );
                        newSolution = newSolution.AddDocument(companionDocInfo);
                        state.CompanionDocIds.Add(newDocId);
                    }
                    catch
                    { /* file may have been deleted between discovery and read */
                    }
                }

                state.Workspace.TryApplyChanges(newSolution);
            }
        }

        // ── Companion file discovery ──────────────────────────────────────────

        /// <summary>
        /// Returns all .cs files in the same directory as the .uitkx file.
        /// These are loaded as additional source documents so that partial-class
        /// members (Styles, utils, types) defined in companion files are visible
        /// to Roslyn's semantic analysis.
        /// </summary>
        private static IReadOnlyList<string> FindCompanionFiles(string uitkxFilePath)
        {
            var dir = System.IO.Path.GetDirectoryName(uitkxFilePath);
            if (dir == null || !System.IO.Directory.Exists(dir))
                return Array.Empty<string>();

            var result = new List<string>();
            foreach (var file in System.IO.Directory.EnumerateFiles(dir, "*.cs"))
                result.Add(file);
            return result;
        }

        /// <summary>
        /// Adds companion .cs files from the .uitkx directory to the workspace.
        /// Called during first-open workspace creation.
        /// </summary>
        private static void AddCompanionDocuments(
            AdhocWorkspace ws,
            ProjectId projectId,
            FileState state,
            string uitkxFilePath
        )
        {
            state.CompanionDocIds.Clear();
            var companions = FindCompanionFiles(uitkxFilePath);
            foreach (var companionPath in companions)
            {
                try
                {
                    var companionText = System.IO.File.ReadAllText(companionPath);
                    var companionDocInfo = DocumentInfo.Create(
                        id: DocumentId.CreateNewId(projectId, debugName: companionPath),
                        name: System.IO.Path.GetFileName(companionPath),
                        sourceCodeKind: SourceCodeKind.Regular,
                        loader: TextLoader.From(
                            TextAndVersion.Create(
                                Microsoft.CodeAnalysis.Text.SourceText.From(companionText),
                                VersionStamp.Create(),
                                companionPath
                            )
                        )
                    );
                    var companionDoc = ws.AddDocument(companionDocInfo);
                    state.CompanionDocIds.Add(companionDoc.Id);
                }
                catch (Exception ex)
                {
                    ServerLog.Log(
                        $"[RoslynHost] Could not load companion {companionPath}: {ex.Message}"
                    );
                }
            }

            if (state.CompanionDocIds.Count > 0)
                ServerLog.Log(
                    $"[RoslynHost] Loaded {state.CompanionDocIds.Count} companion file(s) for {System.IO.Path.GetFileName(uitkxFilePath)}"
                );
        }

        // ── Source-map-aware diagnostic translation ───────────────────────────

        private static SourceMapEntry? TryMapDiagnostic(Diagnostic diag, SourceMap map)
        {
            // Roslyn's diagnostic span is in the virtual document.
            // Try the source map first (character-precise).
            var span = diag.Location.SourceSpan;
            var mapped = map.ToUitkxOffset(span.Start);

            if (mapped.HasValue)
                return mapped.Value.Entry;

            // Fall back to #line info: Roslyn rewrites the location using #line
            // directives so mappedLineSpan.Path and .StartLinePosition already
            // reflect the .uitkx file.  Return null to signal "use #line info".
            return null;
        }

        // ── DLL hot-reload watcher ────────────────────────────────────────────

        private void SetupDllWatcher(string? workspaceRoot)
        {
            _dllWatcher?.Dispose();
            _dllWatcher = null;

            if (string.IsNullOrEmpty(workspaceRoot))
                return;

            string scriptAssembliesDir = System.IO.Path.Combine(
                workspaceRoot,
                "Library",
                "ScriptAssemblies"
            );

            if (!System.IO.Directory.Exists(scriptAssembliesDir))
                return;

            try
            {
                _dllWatcher = new FileSystemWatcher(scriptAssembliesDir, "*.dll")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true,
                };

                // Debounced handler — a Unity recompile touches many DLLs in quick
                // succession; we only need to invalidate once.
                Timer? invalidateTimer = null;
                void OnDllChanged(object _, FileSystemEventArgs __)
                {
                    invalidateTimer?.Dispose();
                    invalidateTimer = new Timer(
                        _ =>
                        {
                            _refLocator.Invalidate();
                            ServerLog.Log(
                                "[RoslynHost] Unity recompile detected — reference cache cleared."
                            );
                        },
                        null,
                        dueTime: 1500,
                        period: Timeout.Infinite
                    );
                }

                _dllWatcher.Changed += OnDllChanged;
                _dllWatcher.Created += OnDllChanged;

                ServerLog.Log($"[RoslynHost] DLL watcher active: {scriptAssembliesDir}");
            }
            catch (Exception ex)
            {
                ServerLog.Log($"[RoslynHost] Could not set up DLL watcher: {ex.Message}");
            }
        }

        // ── IDisposable ───────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _dllWatcher?.Dispose();

            foreach (var kv in _files)
                kv.Value.Dispose();

            _files.Clear();
        }
    }
}
