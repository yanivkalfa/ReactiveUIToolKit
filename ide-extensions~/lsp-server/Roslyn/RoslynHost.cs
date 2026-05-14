using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ReactiveUITK.Language;
using ReactiveUITK.Language.Diagnostics;
using ReactiveUITK.Language.Nodes;
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
        /// Shared polyfill source added once per Roslyn project.  Contains
        /// lightweight stubs for ReactiveUITK.Core event types and delegate
        /// handlers that may not exist in the loaded metadata references
        /// (e.g. when Unity ScriptAssemblies are unavailable).
        /// <para>
        /// This document is added to each project exactly once — never
        /// duplicated across peer virtual documents — so there are no
        /// CS0101 "type already defined" conflicts.
        /// </para>
        /// </summary>
        private const string PolyfillSource =
            "// <auto-generated: UITKX polyfill — event types & delegate stubs>\n"
            + "#pragma warning disable CS0436\n"
            + "namespace ReactiveUITK.Core\n{\n"
            + "    public sealed class Ref<T>\n"
            + "    {\n"
            + "        public T Current { get; set; } = default!;\n"
            + "        public T Value { get => Current; set => Current = value; }\n"
            + "    }\n"
            + "    public class ReactiveEvent { public bool StopPropagation; public bool PreventDefault; }\n"
            + "    public class ReactivePointerEvent : ReactiveEvent { }\n"
            + "    public sealed class ReactiveWheelEvent : ReactivePointerEvent { }\n"
            + "    public sealed class ReactiveKeyboardEvent : ReactiveEvent { }\n"
            + "    public sealed class ReactiveFocusEvent : ReactiveEvent { }\n"
            + "    public sealed class ReactiveDragEvent : ReactiveEvent { }\n"
            + "    public sealed class ReactiveGeometryEvent : ReactiveEvent { }\n"
            + "    public sealed class ReactivePanelEvent : ReactiveEvent { }\n"
            + "    public delegate void UIEventHandler<E>(E e);\n"
            + "    public delegate void PointerEventHandler(ReactivePointerEvent e);\n"
            + "    public delegate void WheelEventHandler(ReactiveWheelEvent e);\n"
            + "    public delegate void KeyboardEventHandler(ReactiveKeyboardEvent e);\n"
            + "    public delegate void FocusEventHandler(ReactiveFocusEvent e);\n"
            + "    public delegate void DragEventHandler(ReactiveDragEvent e);\n"
            + "    public delegate void GeometryChangedEventHandler(ReactiveGeometryEvent e);\n"
            + "    public delegate void PanelLifecycleEventHandler(ReactivePanelEvent e);\n"
            + "    public delegate void InputEventHandler(string newValue);\n"
            + "    public delegate void ErrorEventHandler(System.Exception error);\n"
            + "}\n"
            + "#pragma warning restore CS0436\n";

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

            /// <summary>Document IDs for peer .uitkx virtual documents (hooks, modules, styles).</summary>
            public List<DocumentId> PeerDocIds = new List<DocumentId>();

            /// <summary>Document ID for the shared polyfill document containing
            /// event type stubs and delegate definitions.</summary>
            public DocumentId? PolyfillDocId;

            /// <summary>
            /// Maps each peer Document ID to its original .uitkx path and generated
            /// <see cref="VirtualDocument"/> (including the <see cref="SourceMap"/>).
            /// Used by go-to-definition and rename to map Roslyn positions in peer
            /// virtual documents back to their .uitkx source.
            /// </summary>
            public Dictionary<DocumentId, (string PeerPath, VirtualDocument PeerVDoc)> PeerVDocs =
                new Dictionary<DocumentId, (string, VirtualDocument)>();

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
        private readonly ConcurrentDictionary<string, string> _companionOverlay = new(
            StringComparer.OrdinalIgnoreCase
        );

        private readonly ReferenceAssemblyLocator _refLocator;
        private readonly VirtualDocumentGenerator _docGenerator = new VirtualDocumentGenerator();
        private readonly RoslynSemanticTokensProvider _roslynTokensProvider =
            new RoslynSemanticTokensProvider();
        private readonly ILanguageServerFacade _server;
        private readonly IPropsTypeProvider _propsTypes;
        private readonly DocumentStore? _documentStore;

        private string? _workspaceRoot;

        private readonly WorkspaceIndex _workspaceIndex;

        private FileSystemWatcher? _dllWatcher;
        private bool _disposed;

        // ── Construction ──────────────────────────────────────────────────────

        public RoslynHost(
            ILanguageServerFacade server,
            UitkxSchema schema,
            WorkspaceIndex workspaceIndex,
            DocumentStore? documentStore = null
        )
        {
            _server = server;
            _refLocator = new ReferenceAssemblyLocator();
            _workspaceIndex = workspaceIndex;
            _propsTypes = new PropsTypeAdapter(schema, workspaceIndex);
            _documentStore = documentStore;
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

        // ── Peer source resolution ───────────────────────────────────────────

        /// <summary>
        /// Returns the content of a peer .uitkx file, preferring the in-memory
        /// editor buffer (via <see cref="DocumentStore"/>) over disk.
        /// This ensures that unsaved edits in an open peer document are picked
        /// up immediately by dependent workspaces, eliminating cross-file
        /// diagnostic staleness.
        /// </summary>
        private string ReadPeerSource(string peerPath)
        {
            if (_documentStore != null && _documentStore.TryGetByPath(peerPath, out var editorText))
                return editorText;
            return System.IO.File.ReadAllText(peerPath);
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
                            // Read set must include BOTH direct reads
                            // (dataFlow.ReadInside) AND locals captured by any
                            // nested lambda / local function. A capture is, by
                            // definition, a future use — flagging it as "never
                            // used" is a false positive. Common case: state
                            // hook deconstructions where the setter is only
                            // referenced inside an event-handler lambda
                            // (e.g. `onChange={(e) => setText(e.newValue)}`)
                            // and the value is only referenced inside a JSX
                            // fragment that the emitter lowers into a nested
                            // render lambda.
                            var readSet = new HashSet<ISymbol>(
                                dataFlow.ReadInside,
                                SymbolEqualityComparer.Default
                            );
                            foreach (var captured in dataFlow.Captured)
                                readSet.Add(captured);

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
                    ServerLog.Log($"[RoslynHost] AnalyzeDataFlow error: {ex.Message}");
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
        /// Looks up a peer virtual document by its Roslyn <see cref="DocumentId"/>.
        /// Returns the peer's original .uitkx path and <see cref="VirtualDocument"/>
        /// (including its <see cref="SourceMap"/>), or <c>null</c> if not found.
        /// Used by go-to-definition and rename to map positions in peer virtual
        /// documents back to their .uitkx source.
        /// </summary>
        public (string PeerPath, VirtualDocument PeerVDoc)? TryGetPeerVirtualDocument(
            string uitkxFilePath,
            DocumentId peerDocId
        )
        {
            if (!_files.TryGetValue(uitkxFilePath, out var state))
                return null;
            if (state.PeerVDocs.TryGetValue(peerDocId, out var entry))
                return entry;
            return null;
        }

        /// <summary>
        /// Returns a snapshot of all peer virtual documents for the given .uitkx file.
        /// Used by the rename handler to map Roslyn edits in peer virtual documents
        /// back to their .uitkx source.
        /// </summary>
        public Dictionary<
            DocumentId,
            (string PeerPath, VirtualDocument PeerVDoc)
        >? GetPeerVirtualDocuments(string uitkxFilePath)
        {
            if (!_files.TryGetValue(uitkxFilePath, out var state))
                return null;
            if (state.PeerVDocs.Count == 0)
                return null;
            return new Dictionary<DocumentId, (string, VirtualDocument)>(state.PeerVDocs);
        }

        /// <summary>
        /// Invalidates the workspace of every tracked .uitkx file that has
        /// <paramref name="changedPeerPath"/> as a peer document.  The next
        /// <see cref="EnsureReadyAsync"/> call for those files will trigger a
        /// full workspace rebuild that picks up the changed peer content.
        /// </summary>
        /// <returns>List of file paths whose workspaces were invalidated.</returns>
        public List<string> InvalidatePeerDependents(string changedPeerPath)
        {
            var fullPath = System.IO.Path.GetFullPath(changedPeerPath);
            var invalidated = new List<string>();
            foreach (var kv in _files)
            {
                // Skip the changed file itself
                if (
                    string.Equals(
                        System.IO.Path.GetFullPath(kv.Key),
                        fullPath,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    continue;

                var state = kv.Value;
                // Check if this file has the changed path as a peer
                foreach (var (_, (peerPath, _)) in state.PeerVDocs)
                {
                    if (
                        string.Equals(
                            System.IO.Path.GetFullPath(peerPath),
                            fullPath,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        // Force next EnsureReadyAsync to rebuild
                        state.LastBuiltSource = "";
                        invalidated.Add(kv.Key);
                        break;
                    }
                }
            }
            if (invalidated.Count > 0)
                ServerLog.Log(
                    $"[RoslynHost] InvalidatePeerDependents: invalidated {invalidated.Count} workspace(s) depending on {System.IO.Path.GetFileName(changedPeerPath)}"
                );
            return invalidated;
        }

        /// <summary>
        /// Searches all tracked .uitkx workspaces for a companion .cs document
        /// whose file-name matches <paramref name="csFilePath"/>.
        /// Returns the companion <see cref="Document"/>, the owning .uitkx path,
        /// and the main virtual-document <see cref="DocumentId"/>, or <c>null</c>.
        /// </summary>
        public (
            Document CompanionDoc,
            string UitkxPath,
            DocumentId MainDocId,
            VirtualDocument? VDoc
        )? FindCompanionDocument(string csFilePath)
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
                    if (
                        doc != null
                        && string.Equals(doc.Name, csFileName, StringComparison.OrdinalIgnoreCase)
                    )
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
                    if (
                        doc == null
                        || !string.Equals(doc.Name, csFileName, StringComparison.OrdinalIgnoreCase)
                    )
                        continue;

                    // Replace the document text in the workspace
                    var newSourceText = Microsoft.CodeAnalysis.Text.SourceText.From(currentText);
                    var newSolution = state.Workspace.CurrentSolution.WithDocumentText(
                        companionDocId,
                        newSourceText
                    );
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
            if (csDir == null)
                return Array.Empty<string>();

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

                var enriched = EnrichWithPeerHookUsings(parseResult, uitkxFilePath);
                var virtualDoc = _docGenerator.Generate(
                    enriched,
                    source,
                    uitkxFilePath,
                    _propsTypes
                );
                UpdateWorkspace(state, uitkxFilePath, virtualDoc, ct);
                state.VirtualDoc = virtualDoc;
                state.LastBuiltSource = source;
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
                // Use CancelAsync so any registered cancellation callbacks run on the thread
                // pool instead of inline, and we don't hold the per-file gate while they execute.
                await old.CancelAsync().ConfigureAwait(false);
                old.Dispose();

                var ct = cts.Token;

                // 1. Generate virtual document (enriched with peer hook usings)
                var enriched = EnrichWithPeerHookUsings(parseResult, uitkxFilePath);
                var virtualDoc = _docGenerator.Generate(
                    enriched,
                    source,
                    uitkxFilePath,
                    _propsTypes
                );

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
                    try
                    {
                        await PushClassificationOverridesAsync(uitkxFilePath, source, state, ct)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    { /* next rebuild will send */
                    }
                    catch (Exception ex)
                    {
                        ServerLog.Log(
                            $"[RoslynHost] PushClassificationOverrides error: {ex.Message}"
                        );
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
                return;

            var overrides = await _roslynTokensProvider
                .GetDelegateOverridesAsync(roslynDoc, vDoc.Map, source, ct)
                .ConfigureAwait(false);

            // Always send — an empty array clears stale overrides from a prior edit.
            _server.SendNotification(
                "uitkx/classificationOverrides",
                new { uri = DocumentUri.File(uitkxFilePath).ToString(), overrides = overrides }
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

            // Polyfill stubs are only needed when the real ReactiveUITK runtime
            // assemblies are NOT among the metadata references (e.g. Unity
            // project not yet compiled).  When the real DLL is loaded, event
            // types and delegates come from metadata — both delegate coloring
            // and lambda binding work correctly.  Having BOTH source stubs and
            // the real DLL in the same compilation causes CS0436 conflicts that
            // break nested lambda binding.
            //
            // The check matches only RUNTIME assemblies (Shared/Runtime/Core),
            // never the LSP-internal `ReactiveUITK.Language` parser DLL — that
            // DLL doesn't contain the runtime types (Ref<T>, ReactiveEvent,
            // delegate handlers) that the polyfill provides.
            bool needsPolyfill = !Array.Exists(
                refs,
                r =>
                {
                    if (r.Display == null)
                        return false;
                    var name = System.IO.Path.GetFileNameWithoutExtension(r.Display);
                    if (string.IsNullOrEmpty(name))
                        return false;
                    return name.Equals("ReactiveUITK.Shared", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("ReactiveUITK.Runtime", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("ReactiveUITK.Core", StringComparison.OrdinalIgnoreCase);
                }
            );

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

                // ── Conditionally add polyfill document (event types, delegates) ─
                if (needsPolyfill)
                {
                    var polyfillDocInfo = DocumentInfo.Create(
                        id: DocumentId.CreateNewId(project.Id, debugName: "__polyfill__"),
                        name: "__UitkxPolyfill__.g.cs",
                        sourceCodeKind: SourceCodeKind.Regular,
                        loader: TextLoader.From(
                            TextAndVersion.Create(
                                Microsoft.CodeAnalysis.Text.SourceText.From(PolyfillSource),
                                VersionStamp.Create(),
                                "__polyfill__"
                            )
                        )
                    );
                    var polyfillDoc = ws.AddDocument(polyfillDocInfo);
                    state.PolyfillDocId = polyfillDoc.Id;
                }

                // ── Load companion .cs files from the same directory ─────────
                AddCompanionDocuments(ws, project.Id, state, uitkxFilePath);

                // ── Load peer .uitkx virtual documents (hooks, modules) ──────
                AddPeerUitkxDocuments(ws, project.Id, state, uitkxFilePath);
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

                // ── Sync polyfill document with current refs ─────────────────
                // If the real ReactiveUITK assembly appeared/disappeared (Unity
                // recompiled), add or remove the polyfill accordingly.
                if (needsPolyfill && state.PolyfillDocId == null)
                {
                    var newPolyfillId = DocumentId.CreateNewId(
                        state.ProjectId!,
                        debugName: "__polyfill__"
                    );
                    newSolution = newSolution.AddDocument(
                        DocumentInfo.Create(
                            id: newPolyfillId,
                            name: "__UitkxPolyfill__.g.cs",
                            sourceCodeKind: SourceCodeKind.Regular,
                            loader: TextLoader.From(
                                TextAndVersion.Create(
                                    Microsoft.CodeAnalysis.Text.SourceText.From(PolyfillSource),
                                    VersionStamp.Create(),
                                    "__polyfill__"
                                )
                            )
                        )
                    );
                    state.PolyfillDocId = newPolyfillId;
                }
                else if (!needsPolyfill && state.PolyfillDocId != null)
                {
                    newSolution = newSolution.RemoveDocument(state.PolyfillDocId);
                    state.PolyfillDocId = null;
                }

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

                // ── Refresh peer .uitkx virtual documents ─────────────────
                foreach (var oldId in state.PeerDocIds)
                    newSolution = newSolution.RemoveDocument(oldId);
                state.PeerDocIds.Clear();
                state.PeerVDocs.Clear();

                newSolution = AddPeerUitkxDocumentsToSolution(
                    newSolution,
                    state.ProjectId!,
                    state,
                    uitkxFilePath
                );

                state.Workspace.TryApplyChanges(newSolution);
            }
        }

        // ── Companion file discovery ──────────────────────────────────────────

        /// <summary>
        /// Returns all .cs files that should be loaded as companions for the
        /// given .uitkx file. The set includes:
        ///   - every .cs in the same directory (legacy contract)
        ///   - every .cs anywhere in the workspace whose nearest *.asmdef
        ///     resolves to the same owner asmdef as the .uitkx (added 0.5.12)
        /// The asmdef boundary is identical to the SG's IsOwnedByCompilation
        /// contract, mirrored via <see cref="AsmdefResolver"/>. Projects with
        /// no .asmdef anywhere fall back to <c>Assembly-CSharp</c> /
        /// <c>Assembly-CSharp-Editor</c> by Editor/-segment convention.
        /// </summary>
        private IReadOnlyList<string> FindCompanionFiles(string uitkxFilePath)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var dir = System.IO.Path.GetDirectoryName(uitkxFilePath);
            if (dir != null && System.IO.Directory.Exists(dir))
            {
                foreach (var file in System.IO.Directory.EnumerateFiles(dir, "*.cs"))
                    result.Add(file);
            }

            if (_workspaceIndex != null)
            {
                string ownerAsmdef = AsmdefResolver.OwningAsmdefName(uitkxFilePath);
                var allCs = _workspaceIndex.GetAllCsFiles();

                // Soft telemetry breadcrumb. Documented in LATENCY_TARGETS.md
                // — the .uitkx-open companion scan cost scales with the .cs
                // count in the owning asmdef. Asmdef splitting is the
                // recommended fix for monolith Assembly-CSharp projects.
                if (allCs.Count > 500)
                    ServerLog.Log(
                        $"[RoslynHost] FindCompanionFiles: workspace has {allCs.Count} "
                        + $".cs files; companion union for '{System.IO.Path.GetFileName(uitkxFilePath)}' "
                        + $"is asmdef-scoped to '{ownerAsmdef}'.");

                foreach (var cs in allCs)
                {
                    if (string.Equals(
                            AsmdefResolver.OwningAsmdefName(cs),
                            ownerAsmdef,
                            StringComparison.Ordinal))
                        result.Add(cs);
                }
            }

            return result.Count == 0
                ? Array.Empty<string>()
                : result.ToArray();
        }

        /// <summary>
        /// Adds companion .cs files from the .uitkx directory to the workspace.
        /// Called during first-open workspace creation.
        /// </summary>
        private void AddCompanionDocuments(
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

        // ── Peer .uitkx file discovery & loading ─────────────────────────────

        /// <summary>
        /// Returns peer .uitkx files whose generated code must be visible in
        /// the Roslyn workspace for cross-file references to resolve. Includes
        /// every file in the same directory as <paramref name="uitkxFilePath"/>
        /// (covers component<->hook/style/types pairs and `*Func` siblings) plus
        /// every workspace-wide .uitkx file that declares a top-level <c>module</c>
        /// or <c>hook</c>, sourced from <see cref="WorkspaceIndex"/>. The latter
        /// is what makes a reference like <c>Theme.SidebarWidth</c> resolve when
        /// <c>Theme.uitkx</c> lives in a parent or sibling directory.
        /// The file itself is always excluded; results are case-insensitively
        /// deduplicated.
        /// </summary>
        private IReadOnlyList<string> FindPeerUitkxFiles(string uitkxFilePath)
        {
            var self = System.IO.Path.GetFullPath(uitkxFilePath);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { self };
            var result = new List<string>();

            var dir = System.IO.Path.GetDirectoryName(uitkxFilePath);
            if (dir != null && System.IO.Directory.Exists(dir))
            {
                foreach (var file in System.IO.Directory.EnumerateFiles(dir, "*.uitkx"))
                {
                    var full = System.IO.Path.GetFullPath(file);
                    if (seen.Add(full))
                        result.Add(file);
                }
            }

            foreach (var file in _workspaceIndex.GetModuleAndHookFiles())
            {
                string full;
                try { full = System.IO.Path.GetFullPath(file); }
                catch { continue; }
                if (seen.Add(full))
                    result.Add(file);
            }

            return result;
        }

        /// <summary>
        /// Enriches a <see cref="ParseResult"/> by injecting <c>using static Ns.XxxHooks;</c>
        /// entries for every peer hook container that belongs to the same asmdef as
        /// the consumer .uitkx file, regardless of whether the hook file lives in the
        /// same namespace or directory. This is the LSP-side mirror of the SG's
        /// Stage 3d in <c>UitkxPipeline.cs</c>; both inject the same set of
        /// using-static directives so the IDE and the build agree.
        /// </summary>
        private ParseResult EnrichWithPeerHookUsings(ParseResult parseResult, string uitkxFilePath)
        {
            var d = parseResult.Directives;
            // Only enrich component files (hooks/modules don't call peer hooks).
            if (d.ComponentName == null)
                return parseResult;

            var peers = FindPeerUitkxFiles(uitkxFilePath);
            if (peers.Count == 0)
                return parseResult;

            string consumerAsmdef = AsmdefResolver.OwningAsmdefName(uitkxFilePath);

            var extraUsings = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (!d.Usings.IsDefault)
                foreach (var u in d.Usings)
                    seen.Add(u);

            foreach (var peerPath in peers)
            {
                try
                {
                    var peerSource = System.IO.File.ReadAllText(peerPath);
                    var peerDiags = new List<ParseDiagnostic>();
                    var peerDirectives = DirectiveParser.Parse(peerSource, peerPath, peerDiags);

                    if (peerDirectives.HookDeclarations.IsDefaultOrEmpty)
                        continue;
                    if (string.IsNullOrEmpty(peerDirectives.Namespace))
                        continue;
                    if (!string.Equals(
                            AsmdefResolver.OwningAsmdefName(peerPath),
                            consumerAsmdef,
                            StringComparison.Ordinal))
                        continue;

                    string containerClass = DerivePeerHookContainerClass(peerPath);
                    string fqn = $"static {peerDirectives.Namespace}.{containerClass}";
                    if (seen.Add(fqn))
                        extraUsings.Add(fqn);
                }
                catch
                { /* skip unreadable peers */
                }
            }

            if (extraUsings.Count == 0)
                return parseResult;

            var currentUsings = d.Usings.IsDefault ? ImmutableArray<string>.Empty : d.Usings;
            var newUsings = currentUsings.AddRange(extraUsings);
            var enrichedDirectives = d with { Usings = newUsings };
            return new ParseResult(
                enrichedDirectives,
                parseResult.RootNodes,
                parseResult.Diagnostics
            );
        }

        // Derives the static container class name for a peer hook file.
        // Mirrors HookEmitter.DeriveContainerClassName (SG) and
        // GenerateHookDocument (VDG): take the part before the first dot in the
        // filename (so .hooks / .style middle segments are ignored), PascalCase
        // the first letter, and append "Hooks".
        private static string DerivePeerHookContainerClass(string peerPath)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(peerPath);
            int dot = fileName.IndexOf('.');
            if (dot > 0)
                fileName = fileName.Substring(0, dot);
            if (fileName.Length > 0 && char.IsLower(fileName[0]))
                fileName = char.ToUpper(fileName[0]) + fileName.Substring(1);
            return fileName + "Hooks";
        }

        /// <summary>
        /// First-open path: adds peer .uitkx virtual documents to the workspace
        /// via <see cref="AdhocWorkspace.AddDocument"/>.
        /// </summary>
        private void AddPeerUitkxDocuments(
            AdhocWorkspace ws,
            ProjectId projectId,
            FileState state,
            string uitkxFilePath
        )
        {
            state.PeerDocIds.Clear();
            state.PeerVDocs.Clear();
            var peers = FindPeerUitkxFiles(uitkxFilePath);
            foreach (var peerPath in peers)
            {
                try
                {
                    var peerSource = ReadPeerSource(peerPath);
                    var peerDiags = new List<ParseDiagnostic>();
                    var peerDirectives = DirectiveParser.Parse(peerSource, peerPath, peerDiags);

                    // Only include files that have hooks or modules — regular components
                    // are handled by their own workspace.
                    if (
                        peerDirectives.HookDeclarations.IsDefaultOrEmpty
                        && peerDirectives.ModuleDeclarations.IsDefaultOrEmpty
                    )
                        continue;

                    var peerParseResult = new ParseResult(
                        peerDirectives,
                        ImmutableArray<AstNode>.Empty,
                        ImmutableArray.CreateRange<ParseDiagnostic>(peerDiags)
                    );
                    var peerVDoc = _docGenerator.Generate(
                        peerParseResult,
                        peerSource,
                        peerPath,
                        _propsTypes
                    );

                    var docInfo = DocumentInfo.Create(
                        id: DocumentId.CreateNewId(projectId, debugName: peerPath + ".peer"),
                        name: System.IO.Path.GetFileName(peerPath) + ".g.cs",
                        sourceCodeKind: SourceCodeKind.Regular,
                        loader: TextLoader.From(
                            TextAndVersion.Create(
                                Microsoft.CodeAnalysis.Text.SourceText.From(peerVDoc.Text),
                                VersionStamp.Create(),
                                peerPath
                            )
                        )
                    );
                    var doc = ws.AddDocument(docInfo);
                    state.PeerDocIds.Add(doc.Id);
                    state.PeerVDocs[doc.Id] = (peerPath, peerVDoc);
                }
                catch (Exception ex)
                {
                    ServerLog.Log($"[RoslynHost] Could not load peer {peerPath}: {ex.Message}");
                }
            }

            if (state.PeerDocIds.Count > 0)
                ServerLog.Log(
                    $"[RoslynHost] Loaded {state.PeerDocIds.Count} peer .uitkx document(s) for {System.IO.Path.GetFileName(uitkxFilePath)}"
                );
        }

        /// <summary>
        /// Update path: adds peer .uitkx virtual documents to an existing solution.
        /// Returns the modified solution.
        /// </summary>
        private Solution AddPeerUitkxDocumentsToSolution(
            Solution solution,
            ProjectId projectId,
            FileState state,
            string uitkxFilePath
        )
        {
            var peers = FindPeerUitkxFiles(uitkxFilePath);
            foreach (var peerPath in peers)
            {
                try
                {
                    var peerSource = ReadPeerSource(peerPath);
                    var peerDiags = new List<ParseDiagnostic>();
                    var peerDirectives = DirectiveParser.Parse(peerSource, peerPath, peerDiags);

                    if (
                        peerDirectives.HookDeclarations.IsDefaultOrEmpty
                        && peerDirectives.ModuleDeclarations.IsDefaultOrEmpty
                    )
                        continue;

                    var peerParseResult = new ParseResult(
                        peerDirectives,
                        ImmutableArray<AstNode>.Empty,
                        ImmutableArray.CreateRange<ParseDiagnostic>(peerDiags)
                    );
                    var peerVDoc = _docGenerator.Generate(
                        peerParseResult,
                        peerSource,
                        peerPath,
                        _propsTypes
                    );

                    var newDocId = DocumentId.CreateNewId(projectId, debugName: peerPath + ".peer");
                    var docInfo = DocumentInfo.Create(
                        id: newDocId,
                        name: System.IO.Path.GetFileName(peerPath) + ".g.cs",
                        sourceCodeKind: SourceCodeKind.Regular,
                        loader: TextLoader.From(
                            TextAndVersion.Create(
                                Microsoft.CodeAnalysis.Text.SourceText.From(peerVDoc.Text),
                                VersionStamp.Create(),
                                peerPath
                            )
                        )
                    );
                    solution = solution.AddDocument(docInfo);
                    state.PeerDocIds.Add(newDocId);
                    state.PeerVDocs[newDocId] = (peerPath, peerVDoc);
                }
                catch (Exception ex)
                {
                    ServerLog.Log($"[RoslynHost] Could not load peer {peerPath}: {ex.Message}");
                }
            }

            return solution;
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
