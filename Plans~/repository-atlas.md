# ReactiveUIToolKit Repository Atlas

This document is a persistent map of the authored repository surface.

It is not a changelog and it is not generated output. It is the working map for navigating the codebase as a product family:

- Unity runtime and editor package
- UITKX source generator
- shared language library
- LSP server and IDE clients
- samples and showcase hosts
- docs site
- diagnostics and benchmarking tools

## Scope

Authored source/product files counted here: `805`

Included:

- `*.cs`
- `*.uitkx`
- `*.ts`
- `*.tsx`
- config and project files that affect behavior
- docs source
- samples

Excluded as non-source-of-truth:

- `.meta`
- `dist~`
- `node_modules`
- `bin`
- `obj`
- `TestResults`
- compiled analyzer binaries
- generated preview output
- images and packaged extension artifacts

## Topology

Primary authored surface by top-level area:

- `Shared`: `189` files
- `Runtime`: `4` files
- `Editor`: `9` files
- `SourceGenerator~`: `24` files
- `ide-extensions~`: `110` files
- `Diagnostics`: `15` files
- `Samples`: `178` files
- `ReactiveUIToolKitDocs~`: `234` files
- `CICD`: `2` files
- `scripts`: `3` files

The center of gravity is `Shared`. The runtime/editor hosts are thin wrappers around it.

## Core Runtime

### Virtual tree and public construction API

- `Shared/Core/V.cs`: public `V.*` factory surface for host elements, function components, fragments, portals, router primitives, animation, suspense, error boundaries, memo, and host-root wrapping.
- `Shared/Core/VNode.cs`: immutable virtual node model with typed-props support, prop-type metadata, suspense/error-boundary payload, and child/props cloning.
- `Shared/Core/VNodeHostRenderer.cs`: normalizes `V.Host(...)` roots and hands the actual tree to the fiber renderer.

### Hook system

- `Shared/Core/Hooks.cs`: main hooks implementation.
  - local state queueing and state setters
  - memo, refs, layout/passive effects
  - safe area / stable callback helpers
  - context provide/consume and context change propagation
  - signals integration
  - suspense suspension and `FlushSync`
- `Shared/Core/StateSetterExtensions.cs`: ergonomic helpers around state setter usage.
- `Shared/Core/NodeMetadata.cs`: legacy metadata bridge plus `FunctionComponentState`.

### Fiber runtime

- `Shared/Core/Fiber/FiberNode.cs`: fiber data model, effect flags, tree links, committed/pending props, context payload, effect lists, and update flags.
- `Shared/Core/Fiber/FiberReconciler.cs`: render loop, scheduling, work-in-progress tree creation, commit phase, effect flushing, deletions, error boundary activation, and flag clearing.
- `Shared/Core/Fiber/FiberFunctionComponent.cs`: function-component render path, bailout rules, hook context setup, child reconciliation for single-root components, and effect tagging.
- `Shared/Core/Fiber/FiberChildReconciliation.cs`: keyed child reconciliation and clone paths.
- `Shared/Core/Fiber/FiberFactory.cs`: centralized new/clone fiber creation and flag propagation.
- `Shared/Core/Fiber/FiberIntrinsicComponents.cs`: internal support for suspense and related intrinsic behavior.
- `Shared/Core/Fiber/FiberHostConfig.cs`: host-side create/update/remove operations delegated through the element registry.
- `Shared/Core/Fiber/FiberRenderer.cs`: container-level mount/update/unmount wrapper.
- `Shared/Core/Fiber/FiberRoot.cs`: root container state.
- `Shared/Core/Fiber/FiberFragment.cs`: fragment update behavior.
- `Shared/Core/Fiber/FiberSuspenseSuspendException.cs`: internal render-abort exception for suspense.
- `Shared/Core/Fiber/FiberConfig.cs`: debug/config knobs.
- `Shared/Core/Fiber/FiberTest.cs`: runtime test/demo harness.

### Host and environment

- `Shared/Core/HostContext.cs`: element registry plus contextual environment and provider frames.
- `Shared/Core/IScheduler.cs`: scheduling abstraction used by runtime/editor schedulers and `FlushSync`.
- `Shared/Core/PortalContextKeys.cs`: named environment slots for portal roots.
- `Shared/Core/IProps.cs`: typed props marker and `EmptyProps`.

### Reactive extras

- `Shared/Core/Signals/Signal.cs`: signal primitives, subscriptions, and dispatching.
- `Shared/Core/Signals/SignalsRuntime.cs`: global signal registry bootstrap and runtime host.
- `Shared/Core/Router/*`: in-memory router implementation, path parsing, matching, context keys, components, and hooks.
- `Shared/Core/Animation/AnimateFunc.cs`: animate component render behavior.
- `Shared/Core/Animation/Animator.cs`: track execution and handle lifecycle.
- `Shared/Core/Animation/Easing.cs`: easing evaluation.
- `Shared/Core/PropTypes.cs`: runtime prop-type definitions and validation.
- `Shared/Core/SyntheticEvents.cs`: deprecated synthetic event model retained for compatibility.
- `Shared/Core/ReactiveTypes.cs`: current event/delegate type model.
- `Shared/Core/RefUtility.cs`: ref assignment helpers.

### Utilities and diagnostics

- `Shared/Core/Util/SafeAreaUtility.cs`
- `Shared/Core/Util/ShallowCompare.cs`
- `Shared/Core/Util/SnapshotAssert.cs`
- `Shared/Core/Util/VNodeSnapshot.cs`
- `Shared/Diagnostics/WhyDidYouRender.cs`
- `Shared/Util/SortUtils.cs`

## Props Layer

Props are strongly patterned.

Base abstractions:

- `Shared/Props/Typed/BaseProps.cs`: shared visual-element prop contract. Identity, style, ref, focus, lifecycle, and event handlers live here.
- `Shared/Props/Typed/Style.cs`: fluent style dictionary wrapper.
- `Shared/Props/Typed/StyleKeys.cs`: string key constants for style emission.
- `Shared/Props/PropsApplier.cs`: main prop diff/apply engine, style conversion/reset, event wrapper registration/removal, and synthetic-event dispatch.
- `Shared/Props/PropsHelper.cs`: `INotifyPropertyChanged`-style binding helpers.
- `Shared/Props/StyleValue.cs`: style conversion support.

Typed prop pattern:

- almost every component props file in `Shared/Props/Typed` derives from `BaseProps`
- each concrete props type overrides `ToDictionary()`
- exceptions are intentional:
  - `BaseProps.cs`
  - `Style.cs`
  - `StyleKeys.cs`
  - `ErrorBoundaryProps.cs`
  - `AnimateProps.cs`

Representative typed props:

- host-level: `VisualElementProps.cs`, `BoxProps.cs`
- text/input: `LabelProps.cs`, `TextElementProps.cs`, `TextFieldProps.cs`
- collection widgets: `ListViewProps.cs`, `TreeViewProps.cs`, `MultiColumnListViewProps.cs`, `MultiColumnTreeViewProps.cs`
- editor-only controls: `ObjectFieldProps.cs`, `PropertyInspectorProps.cs`, `TwoPaneSplitViewProps.cs`, `ToolbarProps.cs`, `ColorFieldProps.cs`, `EnumFlagsFieldProps.cs`, `HelpBoxProps.cs`

## Elements Layer

Element adapters are also highly patterned.

Base abstractions:

- `Shared/Elements/IElementAdapter.cs`
- `Shared/Elements/BaseElementAdapter.cs`
- `Shared/Elements/StatefulElementAdapter.cs`
- `Shared/Elements/ElementRegistry.cs`
- `Shared/Elements/ElementRegistryProvider.cs`

General rule:

- most adapters implement `Create()`, `ApplyProperties()`, and `ApplyPropertiesDiff()`
- simple controls mostly delegate to `PropsApplier`
- stateful controls keep caches and trackers to preserve UI Toolkit internal state across diffs

Simple/mostly direct adapters:

- `VisualElementAdapter.cs`
- `ButtonElementAdapter.cs`
- `LabelElementAdapter.cs`
- `ToggleElementAdapter.cs`
- `RadioButtonElementAdapter.cs`
- `SliderElementAdapter.cs`
- `IntegerFieldElementAdapter.cs`
- `Vector*FieldElementAdapter.cs`

Stateful or complex adapters:

- `TextFieldElementAdapter.cs`: placeholder/password/read-only/text element slot handling and reflection fallback for internal UIToolkit pieces
- `ListViewElementAdapter.cs`
- `TreeViewElementAdapter.cs`
- `MultiColumnListViewElementAdapter.cs`
- `MultiColumnTreeViewElementAdapter.cs`
- `TabViewElementAdapter.cs`

Editor-only adapter group:

- `Shared/Elements/Editor/PropertyInspectorElementAdapters.cs`
- `Shared/Elements/Editor/ToolbarElementAdapters.cs`
- `Shared/Elements/Editor/TwoPaneSplitViewElementAdapter.cs`

Tracker support:

- `Shared/Elements/Trackers/*` handles selection, scroll, expansion, layout, and sorting persistence for stateful tree/list/multicolumn controls

## Runtime and Editor Hosts

Runtime:

- `Runtime/Core/RootRenderer.cs`: `MonoBehaviour` root bootstrap around `VNodeHostRenderer`
- `Runtime/Core/RenderScheduler.cs`: batched frame scheduler for async render/effect timing
- `Runtime/Core/UitkxElementAttribute.cs`: component annotation used by generated code / tooling

Editor:

- `Editor/EditorRootRendererUtility.cs`: editor-time mount/render/unmount helper
- `Editor/EditorRenderScheduler.cs`: editor scheduler implementation
- `Editor/FiberMenu.cs`: editor diagnostics menu entry points
- `Editor/UITKX_GeneratorTrigger.cs`: generator-related editor integration
- `Editor/UitkxChangeWatcher.cs`: watches `.uitkx` changes
- `Editor/UitkxConsoleNavigation.cs`: console hyperlink/navigation integration
- `Editor/UitkxCsprojPostprocessor.cs`: injects `.uitkx` files as Roslyn additional files in Unity-generated csproj output
- `Editor/UitkxTestRunnerWindow.cs`: editor UI for tests

## Source Generator

Entry and orchestration:

- `SourceGenerator~/UitkxGenerator.cs`: incremental generator entry point, project-root discovery, additional-file ingestion, disk fallback scan, per-file pipeline execution
- `SourceGenerator~/UitkxPipeline.cs`: directive parse -> AST parse -> lowering -> validation -> props resolution -> emission
- `SourceGenerator~/UitkxPipelineResult.cs`

Emitter and validation:

- `SourceGenerator~/Emitter/CSharpEmitter.cs`: generated partial class source emission, helper methods, function-style props emission, line directives
- `SourceGenerator~/Emitter/PropsResolver.cs`: maps UITKX tags to runtime constructors and props types
- `SourceGenerator~/Emitter/HooksValidator.cs`: rules-of-hooks validation
- `SourceGenerator~/Emitter/StructureValidator.cs`: root-count, effect dependency, and key-related validation
- `SourceGenerator~/Emitter/TagResolution.cs`

Diagnostics and runtime loading:

- `SourceGenerator~/Diagnostics/UitkxDiagnostics.cs`
- `SourceGenerator~/LanguageLibResolver.cs`
- `SourceGenerator~/AssemblyInfo.cs`
- `SourceGenerator~/IsExternalInit.cs`

Tests:

- `SourceGenerator~/Tests/DiagnosticTests.cs`
- `SourceGenerator~/Tests/EmitterTests.cs`
- `SourceGenerator~/Tests/FormatterTests.cs`
- `SourceGenerator~/Tests/FormatterSnapshotTests.cs`
- `SourceGenerator~/Tests/LoweringTests.cs`
- `SourceGenerator~/Tests/ParserTests.cs`
- `SourceGenerator~/Tests/DebugDumpTest.cs`
- `SourceGenerator~/Tests/Helpers/GeneratorTestHelper.cs`

Observation:

- the formatter snapshot suite is the single largest authored file in the repository and acts as a regression net for UITKX formatting edge cases

## Language Library and Tooling

### Shared language library

- `ide-extensions~/language-lib/Parser/DirectiveParser.cs`
- `ide-extensions~/language-lib/Parser/UitkxParser.cs`
- `ide-extensions~/language-lib/Parser/MarkupTokenizer.cs`
- `ide-extensions~/language-lib/Parser/ExpressionExtractor.cs`
- `ide-extensions~/language-lib/Parser/ParseResult.cs`
- `ide-extensions~/language-lib/Lowering/CanonicalLowering.cs`
- `ide-extensions~/language-lib/Nodes/AstNode.cs`
- `ide-extensions~/language-lib/Formatter/AstFormatter.cs`
- `ide-extensions~/language-lib/Formatter/ConfigLoader.cs`
- `ide-extensions~/language-lib/Formatter/FormatterOptions.cs`
- `ide-extensions~/language-lib/Formatter/ICSharpFormatterDelegate.cs`
- `ide-extensions~/language-lib/Roslyn/VirtualDocumentGenerator.cs`
- `ide-extensions~/language-lib/Roslyn/SourceMap.cs`
- `ide-extensions~/language-lib/SemanticTokens/*`
- `ide-extensions~/language-lib/Diagnostics/*`

This library is intentionally Roslyn-free except for virtual-document concepts used by higher layers.

### LSP server

- `ide-extensions~/lsp-server/Program.cs`: server composition and handler registration
- `ide-extensions~/lsp-server/DocumentStore.cs`
- `ide-extensions~/lsp-server/WorkspaceIndex.cs`
- `ide-extensions~/lsp-server/DiagnosticsPublisher.cs`
- `ide-extensions~/lsp-server/TextSyncHandler.cs`
- `ide-extensions~/lsp-server/CompletionHandler.cs`
- `ide-extensions~/lsp-server/HoverHandler.cs`
- `ide-extensions~/lsp-server/FormattingHandler.cs`
- `ide-extensions~/lsp-server/SemanticTokensHandler.cs`
- `ide-extensions~/lsp-server/DefinitionHandler.cs`
- `ide-extensions~/lsp-server/SignatureHelpHandler.cs`
- `ide-extensions~/lsp-server/WatchedFilesHandler.cs`
- `ide-extensions~/lsp-server/SchemaLoader.cs`
- `ide-extensions~/lsp-server/CapabilityPatchStream.cs`
- `ide-extensions~/lsp-server/ServerLog.cs`
- `ide-extensions~/lsp-server/StartupLogger.cs`

Roslyn-specific backing:

- `ide-extensions~/lsp-server/Roslyn/RoslynHost.cs`
- `ide-extensions~/lsp-server/Roslyn/RoslynHostStartup.cs`
- `ide-extensions~/lsp-server/Roslyn/ReferenceAssemblyLocator.cs`
- `ide-extensions~/lsp-server/Roslyn/RoslynCompletionProvider.cs`
- `ide-extensions~/lsp-server/Roslyn/RoslynSemanticTokensProvider.cs`
- `ide-extensions~/lsp-server/Roslyn/RoslynDiagnosticMapper.cs`
- `ide-extensions~/lsp-server/Roslyn/RoslynCSharpFormatter.cs`

### IDE clients

VS Code:

- `ide-extensions~/vscode/src/extension.ts`: extension entry point, language client startup, local formatting/range heuristics

Visual Studio:

- `ide-extensions~/visual-studio/UitkxVsix/*`: classifier, completion, quick info, diagnostics, go-to-definition, client bridge

Rider:

- `ide-extensions~/rider/src/main/kotlin/com/reactiveuitk/uitkx/UitkxPlugin.kt`

## Samples and Showcase

The samples act as behavioral spec, not just demos.

### Hand-written C# samples

`Samples/Components/*` contains functional component examples for:

- context and context bailout
- deferred effects
- effect cleanup order
- event batching
- exception flow and error boundaries
- flush sync
- render depth guard
- router
- signals
- prop types
- portal event scope
- ref forwarding
- keyed diffing
- synthetic events
- text field and basic counters

### Shared sample pages

`Samples/Shared/*` contains reusable page composition and richer stateful demos:

- tree view and multicolumn tree view stateful examples
- shared showcase page composition
- list view stateful demo
- values/top bar/fields/extras/tree tabs panels

### UITKX samples

`Samples/UITKX/Components/*` mirrors the runtime concepts in `.uitkx` form.

Notable multi-file component groups:

- `RouterDemoFunc`
- `MainMenuRouterDemoFunc`
- `ShowcaseDemoPage`
- `ContextBailoutDemoFunc`
- `EffectCleanupOrderDemoFunc`
- `RefForwardingDemoFunc`

### Showcase hosts

Editor showcase launchers live in:

- `Samples/Showcase/Editor/*`
- `Samples/UITKX/Showcase/Editor/*`

Pattern:

- nearly every demo has an editor window launcher in both the C# and UITKX showcase trees
- runtime bootstraps for a smaller subset live under `Samples/Showcase/Runtime/*`

## Docs Site

The docs site is large but structurally repetitive.

Shell:

- `ReactiveUIToolKitDocs~/src/App.tsx`
- `ReactiveUIToolKitDocs~/src/App.style.ts`
- `ReactiveUIToolKitDocs~/src/main.tsx`
- `ReactiveUIToolKitDocs~/src/pages.tsx`
- `ReactiveUIToolKitDocs~/src/theme.ts`
- `ReactiveUIToolKitDocs~/src/propsDocs.ts`
- `ReactiveUIToolKitDocs~/src/version.ts`

Shared UI:

- `components/TopBar/*`
- `components/Sidebar/*`
- `components/SearchModal/*`
- `components/Pager/*`
- `components/CodeBlock/*`
- `components/UnityDocsSection/*`

Page pattern:

- most component pages follow `Page.tsx` + `Page.style.ts` + `Page.example.ts`
- `Components` is by far the largest docs subtree with `168` files
- other doc sections are concept/tooling/router/signals/getting-started pages with the same style/content separation

## Diagnostics and Benchmarking

- `Diagnostics/Logs/ReactiveLogCapture.cs`: editor log capture support
- `Diagnostics/Benchmark/BenchmarkSetup.cs`: benchmark orchestration
- `Diagnostics/Benchmark/BenchScenarios.cs`: benchmark scenario definitions
- `Diagnostics/Benchmark/BenchRuntimeHost.cs`
- `Diagnostics/Benchmark/BenchEditorHost.cs`
- `Diagnostics/Benchmark/BenchSharedHost.cs`
- `Diagnostics/Benchmark/BenchMetrics.cs`
- `Diagnostics/Benchmark/BenchConfig.cs`
- `Diagnostics/Benchmark/BenchLogging/*`
- `Diagnostics/Benchmark/Editor/BenchResultsViewer.cs`

## CI, Packaging, and Scripts

- `CICD/Editor/PublishUtility.cs`: publishing automation
- `scripts/build-generator.ps1`
- `scripts/publish-extension.ps1`
- `scripts/publish-vsix.ps1`

## Repo-Level Conventions

Patterns that repeat across the project:

- runtime behavior is centralized; wrappers around it are thin
- typed props are serialized through `ToDictionary()`
- adapters are responsible for UIToolkit-specific quirks, not the core reconciler
- `.uitkx` functionality is mirrored across generator, formatter, parser, and tooling layers
- samples often exist in both hand-written C# and UITKX forms
- docs pages are mostly triads of style/example/content files

## Deliberate Exclusions

This atlas does not attempt to treat these as authored knowledge targets:

- generated `.g.cs` output
- `node_modules`
- copied `dist~` package mirror
- compiled extension/analyzer binaries
- Unity `.meta` files
- transient test result artifacts

Those are consumable artifacts, not the design surface.
