import type { ReactElement } from 'react'
import type { Page as LegacyPage } from './pages'
import { pages as legacySections } from './pages'
import { PAGE_VERSIONS, isAvailableIn, compareVersions } from './versionManifest'
import { KnownIssuesPage } from './pages/KnownIssues/KnownIssuesPage'
import { RoadmapPage } from './pages/Roadmap/RoadmapPage'
import { UitkxAPIPage } from './pages/UITKX/API/UitkxAPIPage'
import { UitkxComponentReferencePage } from './pages/UITKX/Components/UitkxComponentReferencePage'
import { UitkxComponentsPage } from './pages/UITKX/Components/UitkxComponentsPage'
import { CompanionFilesPage } from './pages/UITKX/CompanionFiles/CompanionFilesPage'
import { UitkxConceptsPage } from './pages/UITKX/Concepts/UitkxConceptsPage'
import { UitkxConfigPage } from './pages/UITKX/Config/UitkxConfigPage'
import { UitkxDebuggingPage } from './pages/UITKX/Debugging/UitkxDebuggingPage'
import { UitkxDiagnosticsPage } from './pages/UITKX/Diagnostics/UitkxDiagnosticsPage'
import { UitkxDifferencesPage } from './pages/UITKX/Differences/UitkxDifferencesPage'
import { UitkxGettingStartedPage } from './pages/UITKX/GettingStarted/UitkxGettingStartedPage'
import { UitkxIntroductionPage } from './pages/UITKX/Introduction/UitkxIntroductionPage'
import { UitkxReferencePage } from './pages/UITKX/Reference/UitkxReferencePage'
import { UitkxRouterPage } from './pages/UITKX/Router/UitkxRouterPage'
import { UitkxSignalsPage } from './pages/UITKX/Signals/UitkxSignalsPage'
import { UitkxPortalPage } from './pages/UITKX/Portal/UitkxPortalPage'
import { UitkxSuspensePage } from './pages/UITKX/Suspense/UitkxSuspensePage'
import { HmrPage } from './pages/Tooling/HMR/HmrPage'
import { FAQPage } from './pages/FAQ/FAQPage'
import { StylingPage } from './pages/UITKX/Styling/StylingPage'
import { AssetsPage } from './pages/UITKX/Assets/AssetsPage'
import { EventsPage } from './pages/UITKX/Events/EventsPage'
import { HooksGuidePage } from './pages/UITKX/Hooks/HooksGuidePage'
import { ContextPage } from './pages/UITKX/Context/ContextPage'
import { HooksAPIPage } from './pages/UITKX/HooksAPI/HooksAPIPage'
import { CssHelpersReferencePage } from './pages/UITKX/CssHelpersRef/CssHelpersReferencePage'
import { RefGuidePage } from './pages/UITKX/Guides/RefGuidePage'
import { KeyGuidePage } from './pages/UITKX/Guides/KeyGuidePage'
import { AdvancedAPIPage } from './pages/UITKX/AdvancedAPI/AdvancedAPIPage'

export type DocPage = {
  id: string
  canonicalId: string
  title: string
  path: string
  keywords?: string[]
  searchContent?: string
  group?: 'basic' | 'advanced'
  sinceUnity?: string
  element: () => ReactElement
}

export type DocSection = {
  id: string
  title: string
  pages: DocPage[]
}

const componentPages = legacySections.find((section) => section.id === 'components')?.pages ?? []

export const sections: DocSection[] = [
  {
    id: 'intro',
    title: 'Introduction',
    pages: [
      {
        id: 'introduction',
        canonicalId: 'introduction',
        title: 'Introduction',
        path: '/',
        keywords: ['introduction', 'markup', 'unity ui toolkit'],
        searchContent: 'reactiveuitoolkit react-like ui toolkit runtime unity uitkx authoring language function-style components .uitkx hooks state effects reconcile visualelement c# component countercard usestate return text button onclick highlights reactive diffing batched updates router signals utilities generated c# output production builds no runtime codegen var count setcount',
        element: () => <UitkxIntroductionPage />,
      },
    ],
  },
  {
    id: 'getting-started',
    title: 'Getting Started',
    pages: [
      {
        id: 'getting-started-page',
        canonicalId: 'install',
        title: 'Install & Setup',
        path: '/getting-started',
        keywords: ['install', 'setup', 'component', 'partial'],
        searchContent: 'getting started reactiveuitoolkit function-style .uitkx components source generator produces complete class no boilerplate install via unity package manager open package manager add package from git url create a uitkx component setup code returned markup generator emits render mount rootrenderer V.Func EditorRootRendererUtility.Mount editor window one component per file filename must match component name auto-discovers assets directory @namespace MyGame.UI component HelloWorld var count setCount useState return VisualElement Text Hello ReactiveUITK Button Increment onClick setCount count + 1 companion files optional styles types utils',
        element: () => <UitkxGettingStartedPage />,
      },
    ],
  },
  {
    id: 'companion-files',
    title: 'Companion Files',
    pages: [
      {
        id: 'companion-files-page',
        canonicalId: 'companion-files',
        title: 'Companion Files',
        path: '/companion-files',
        keywords: ['companion', 'styles', 'types', 'utils', 'partial class'],
        searchContent: 'companion files optional .cs file styles types utils naming conventions directory layout source generator produces complete class no boilerplate needed MyComponent.styles.cs style constants helpers colours sizes MyComponent.types.cs enums structs DTOs MyComponent.utils.cs pure helper formatting functions hmr support editing companion triggers hmr creating new file detected instantly setup code when not to use simple components small helpers',
        element: () => <CompanionFilesPage />,
      },
    ],
  },
  {
    id: 'styling',
    title: 'Styling',
    pages: [
      {
        id: 'styling-page',
        canonicalId: 'styling',
        title: 'Styling',
        path: '/styling',
        keywords: ['style', 'css', 'typed', 'CssHelpers', 'StyleKeys', 'layout', 'colors', 'flexbox'],
        searchContent: 'styling typed style class compile-time checked properties inline style system CssHelpers static helpers Pct Px StyleAuto StyleNone StyleInitial length units color helpers Hex Rgba enum shortcuts FlexDirection FlexRow FlexColumn JustifyContent JustifyCenter AlignItems AlignCenter AlignStretch JustifySpaceBetween JustifySpaceAround JustifySpaceEvenly Position PosAbsolute PosRelative Display DisplayFlex DisplayNone Visibility VisHidden VisVisible Overflow OverflowHidden WhiteSpace WsNowrap WsNormal TextOverflow TextClip TextEllipsis TextAnchor FontStyle FontBold FontNormal StyleLength StyleFloat StyleKeyword Width Height Margin Padding BorderRadius BackgroundColor Color BorderColor FlexGrow FlexShrink Opacity FontSize LetterSpacing BackgroundRepeat BgRepeatNone BgRepeatBoth BackgroundSize BgSizeCover BgSizeContain BackgroundPosition BgPosCenter TransformOrigin OriginCenter Origin Rotate Scale Translate Xlate EasingFunction Ease EaseInOut typed properties tuple syntax escape hatch StyleKeys backward compatible property reference compound struct factories',
        element: () => <StylingPage />,
      },
    ],
  },
  {
    id: 'assets',
    title: 'Assets & Stylesheets',
    pages: [
      {
        id: 'assets-page',
        canonicalId: 'assets',
        title: 'Assets & Stylesheets',
        path: '/assets',
        keywords: ['asset', 'texture', 'sprite', 'uss', 'stylesheet', 'image', 'audio', 'font', 'material'],
        searchContent: 'asset loading Asset<T> Ast<T> shorthand texture sprite audioclip font material stylesheet uss @uss directive relative path resolve compile time diagnostics UITKX0022 UITKX0023 UITKX0120 UITKX0121 file not found type mismatch auto-import texture importer asset registry scriptableobject editor sync hmr hot-reload supported file types png jpg wav mp3 ttf otf mat prefab',
        element: () => <AssetsPage />,
      },
    ],
  },
  {
    id: 'components-overview',
    title: 'Components Overview',
    pages: [
      {
        id: 'components-overview-page',
        canonicalId: 'uitkx-components-overview',
        title: 'Components Overview',
        path: '/components',
        keywords: ['components', 'intrinsic tags', 'custom components'],
        searchContent: 'components overview categorized catalog intrinsic tags visualelement button text router tags custom components pascalcase names native element consumers containers layout display buttons toggles text input vector fields pickers selectors data views editor toolbar framework Animate ErrorBoundary Portal Suspense Fragment BaseProps common props name className style ref visible enabled pickingMode focusable tabIndex tooltip viewDataKey languageDirection extraProps editor-only runtime authoring guidelines one component per file',
        element: () => <UitkxComponentsPage />,
      },
    ],
  },
  {
    id: 'component-reference',
    title: 'Components',
    pages: componentPages.map((page: LegacyPage) => ({
      id: page.id,
      canonicalId: page.id,
      title: page.title,
      path: page.path,
      keywords: page.keywords,
      group: page.group,
      sinceUnity: page.sinceUnity,
      element: () => (
        <UitkxComponentReferencePage
          title={page.title}
        />
      ),
    })),
  },
  {
    id: 'concepts',
    title: 'Concepts & Environment',
    pages: [
      {
        id: 'concepts-page',
        canonicalId: 'concepts-and-environment',
        title: 'Concepts & Environment',
        path: '/concepts',
        keywords: ['concepts', 'environment', 'defines'],
        searchContent: 'concepts and environment react-like component model unity ui toolkit components hooks markup reconciliation scheduling authoring rules intrinsic tag names reserved custom components distinct names function-style components setup code first single returned markup tree state setters called directly as functions setcount(count + 1) companion partial classes environment defines compile-time scripting define symbols env_dev env_staging env_prod environment labeling ruitk_trace_verbose ruitk_trace_basic ruitk_diff_tracing runtime diagnostics editor-only diagnostic helpers development symbols behavior summary trace level resolution priority hostcontext rendering pipeline component lifecycle mount update unmount BaseProps common props event handlers onClick onPointerDown onKeyDown visible enabled className ref',
        element: () => <UitkxConceptsPage />,
      },
    ],
  },
  {
    id: 'differences',
    title: 'Different from React',
    pages: [
      {
        id: 'differences-page',
        canonicalId: 'different-from-react',
        title: 'Different from React',
        path: '/differences',
        keywords: ['react', 'hooks', 'rendering', 'state'],
        searchContent: 'different from react component-and-hooks mental model unity ui toolkit c# runtime visualelement system scheduling model state updates usestate setter value updater function statesetter delegate statesetterextensions fluent ToValueAction rendering model fiber reconciler synchronous mode per frame no starttransition no concurrent rendering scheduler defer passive effects slice render work unity runtime constraints interop controls styles events apis differ from browser react conventions UseCallback Func UseStableCallback UseStableAction UseStableFunc',
        element: () => <UitkxDifferencesPage />,
      },
    ],
  },
  {
    id: 'tooling',
    title: 'Tooling',
    pages: [
      {
        id: 'router-page',
        canonicalId: 'router',
        title: 'Router',
        path: '/tooling/router',
        keywords: ['router', 'routes', 'navigation'],
        searchContent: 'router lightweight in-memory router inspired by react router routing authored directly in markup Router Route links routed child components Router establishes routing context subtree Route matches paths render elements RouterHooks setup code imperative navigation history IRouterHistory MemoryHistory custom history UseNavigate pushes replaces locations UseGo UseCanGo back forward UseLocationInfo UseParams UseQuery UseNavigationState UseRouteMatch UseNavigationBase expose active routed data UseBlocker intercept transitions unsaved guarded state nested routes relative paths outlets parent match declarative route composition imperative helpers RouterNavLink Link vs RouterNavLink',
        element: () => <UitkxRouterPage />,
      },
      {
        id: 'signals-page',
        canonicalId: 'signals',
        title: 'Signals',
        path: '/tooling/signals',
        keywords: ['signals', 'shared state', 'reactive'],
        searchContent: 'signals lightweight named reactive values process-wide registry observable store single source of truth global registry keyed by string SignalFactory.Get Signal Subscribe useSignal Dispatch updates event handlers SignalsRuntime.EnsureInitialized selector overloads useSignal signal selector comparer project slice custom equality useMemo SignalCounterDemo counterSignal count Increment Reset Style StyleKeys.FlexDirection row thread safety lock-based synchronization',
        element: () => <UitkxSignalsPage />,
      },
      {
        id: 'hmr-page',
        canonicalId: 'hmr',
        title: 'Hot Module Replacement',
        path: '/tooling/hmr',
        keywords: ['hmr', 'hot reload', 'live editing', 'instant preview'],
        searchContent: 'hot module replacement hmr edit .uitkx files changes instantly unity editor without domain reload component state quick start open reactiveuitk hmr mode start edit save updates in-place hook state counters refs effects preserved assembly reloads filesystemwatcher detects parsed emitted compiled in-process roslyn microsoft.codeanalysis.csharp csc.dll fallback assembly.load render delegate swapped rootrenderer instances re-render hooks state preservation usestate useref useeffect usememo usecallback usecontext companion files .cs partial class style types create new companion auto-detected new component support cs0103 dependency auto-discovery cross-component assembly registry hmr window stats swap count error timing parse emit compile keyboard shortcuts toggle start stop lifecycle limitations old assemblies memory mono unload 10-30 kb per swap first compile jit warmup nuget cache troubleshooting console errors',
        element: () => <HmrPage />,
      },
      {
        id: 'portal-page',
        canonicalId: 'portal',
        title: 'Portal',
        path: '/tooling/portal',
        keywords: ['portal', 'modal', 'overlay', 'tooltip'],
        searchContent: 'portal render children different visualelement target outside component hierarchy modals tooltips overlays clipping stacking context V.Portal portalTargetElement PortalContextKeys ModalRoot TooltipRoot OverlayRoot provideContext useContext',
        element: () => <UitkxPortalPage />,
      },
      {
        id: 'suspense-page',
        canonicalId: 'suspense',
        title: 'Suspense',
        path: '/tooling/suspense',
        keywords: ['suspense', 'loading', 'async', 'fallback'],
        searchContent: 'suspense loading fallback async await task isReady V.Suspense fallbackNode pendingTask SuspendUntil callback mode task mode loading state pattern',
        element: () => <UitkxSuspensePage />,
      },
    ],
  },
  {
    id: 'guides',
    title: 'Guides',
    pages: [
      {
        id: 'events-page',
        canonicalId: 'events',
        title: 'Events & Input Handling',
        path: '/guides/events',
        keywords: ['events', 'input', 'click', 'pointer', 'keyboard', 'focus', 'drag'],
        searchContent: 'events input handling onClick onPointerDown onPointerUp onPointerMove onPointerEnter onPointerLeave onWheel onScroll onFocus onBlur onFocusIn onFocusOut onKeyDown onKeyUp onInput onGeometryChanged onAttachToPanel onDetachFromPanel onDragEnter onDragLeave onDragUpdated onDragPerform onDragExited ReactivePointerEvent ReactiveWheelEvent ReactiveKeyboardEvent ReactiveFocusEvent ReactiveDragEvent ReactiveGeometryEvent ReactivePanelEvent PointerEventHandler WheelEventHandler KeyboardEventHandler FocusEventHandler DragEventHandler GeometryChangedEventHandler PanelLifecycleEventHandler ChangeEventHandler InputEventHandler StopPropagation PreventDefault Position DeltaPosition Button ClickCount KeyCode Character modifier keys AltKey CtrlKey ShiftKey CommandKey Pressure Radius Delta RelatedTarget OldRect NewRect event bubbling propagation editor-only drag BaseProps delegate signatures',
        element: () => <EventsPage />,
      },
      {
        id: 'hooks-guide-page',
        canonicalId: 'hooks-guide',
        title: 'Hooks Guide',
        path: '/guides/hooks',
        keywords: ['hooks', 'useState', 'useEffect', 'useRef', 'useMemo', 'useReducer'],
        searchContent: 'hooks guide useState useReducer useEffect useLayoutEffect useMemo useCallback useRef useContext provideContext useDeferredValue useImperativeHandle useStableFunc useStableAction useStableCallback state setter functional updater StateUpdate reducer dispatch dependency array cleanup mount unmount synchronous before paint memoization stable callback identity mutable ref container element ref context provider consumer shadowing deferred value imperative handle hook configuration EnableHookValidation EnableStrictDiagnostics EnableHookAutoRealign hook rules unconditional top level',
        element: () => <HooksGuidePage />,
      },
      {
        id: 'context-page',
        canonicalId: 'context',
        title: 'Context API',
        path: '/guides/context',
        keywords: ['context', 'provider', 'consumer', 'useContext', 'provideContext'],
        searchContent: 'context api useContext provideContext provider consumer string key type-safe generics nested provider shadowing subtree data dependency injection PortalContextKeys ModalRoot TooltipRoot OverlayRoot context vs signals scope lifetime dynamic context value re-render when value changes object.Equals',
        element: () => <ContextPage />,
      },
      {
        id: 'ref-guide-page',
        canonicalId: 'ref-guide',
        title: 'Refs Guide',
        path: '/guides/refs',
        keywords: ['ref', 'useRef', 'useImperativeHandle', 'element ref'],
        searchContent: 'refs guide useRef element ref mutable value container Ref Current persists across renders no re-render on change VisualElement ref focus auto-focus pattern useImperativeHandle imperative handle expose custom API render counter previous value tracking',
        element: () => <RefGuidePage />,
      },
      {
        id: 'key-guide-page',
        canonicalId: 'key-guide',
        title: 'Keys Guide',
        path: '/guides/keys',
        keywords: ['key', 'list', 'reconciler', 'reorder', 'identity'],
        searchContent: 'keys guide key prop reconciler identity dynamic list foreach for loop stable unique identifier reorder move elements index antipattern reset state unmount remount siblings performance correctness preserve component state hooks refs',
        element: () => <KeyGuidePage />,
      },
    ],
  },
  {
    id: 'api',
    title: 'API',
    pages: [
      {
        id: 'api-page',
        canonicalId: 'api-reference',
        title: 'API Reference',
        path: '/api',
        keywords: ['api', 'hooks', 'runtime', 'namespaces'],
        searchContent: 'api reference map namespaces types core V V.Func VirtualNode Hooks UseState UseReducer UseEffect UseLayoutEffect UseMemo UseCallback UseRef UseContext ProvideContext UseDeferredValue UseImperativeHandle UseStableFunc UseStableAction UseStableCallback UseSignal StateSetterExtensions ToValueAction RootRenderer RenderScheduler EnableHookValidation EnableStrictDiagnostics EnableHookAutoRealign props typed ButtonProps LabelProps ListViewProps ScrollViewProps Style StyleKeys Router RouterHooks UseRouter UseLocation UseLocationInfo UseParams UseQuery UseNavigationState UseNavigate UseGo UseCanGo UseBlocker IRouterHistory MemoryHistory RouterLocation RouterPath RouteMatch SignalFactory Signal Subscribe Set Dispatch SignalsRuntime animation UseAnimate UseTweenFloat AnimateTrack safe area UseSafeArea SafeAreaInsets VisualElementSafe editor EditorRootRendererUtility EditorRenderScheduler elements ElementRegistry ElementRegistryProvider',
        element: () => <UitkxAPIPage />,
      },
      {
        id: 'hooks-api-page',
        canonicalId: 'hooks-api',
        title: 'Hooks API Reference',
        path: '/api/hooks',
        keywords: ['hooks', 'api', 'signatures', 'StateSetter', 'StateUpdate', 'Ref'],
        searchContent: 'hooks api reference exact signatures useState useReducer useEffect useLayoutEffect useMemo useCallback useDeferredValue useRef useImperativeHandle useContext provideContext useStableFunc useStableAction useStableCallback useSignal useAnimate useTweenFloat useSafeArea StateSetter StateUpdate Ref SafeAreaInsets EnableHookValidation EnableStrictDiagnostics EnableHookAutoRealign StateSetterExtensions ToValueAction Set functional updater params object dependencies',
        element: () => <HooksAPIPage />,
      },
      {
        id: 'csshelpers-ref-page',
        canonicalId: 'csshelpers-reference',
        title: 'CssHelpers Reference',
        path: '/api/csshelpers',
        keywords: ['CssHelpers', 'Pct', 'Px', 'Hex', 'Rgba', 'FlexRow', 'easing'],
        searchContent: 'csshelpers reference static shortcuts Pct Px StyleAuto StyleNone StyleInitial FlexRow FlexColumn FlexRowReverse FlexColumnReverse JustifyStart JustifyEnd JustifyCenter JustifySpaceBetween JustifySpaceAround JustifySpaceEvenly AlignStart AlignEnd AlignCenter AlignStretch WrapOn WrapOff WrapReverse PosRelative PosAbsolute DisplayFlex DisplayNone VisVisible VisHidden OverflowVisible OverflowHidden WsNormal WsNowrap WsPre WsPreWrap TextClip TextEllipsis TextUpperLeft TextMiddleCenter TextLowerRight TextOverflowStart TextOverflowMiddle TextOverflowEnd AutoSizeNone AutoSizeBestFit FontBold FontItalic FontNormal PickPosition PickIgnore SelectNone SelectSingle SelectMultiple ScrollerAuto ScrollerVisible ScrollerHidden DirInherit DirLTR DirRTL SliderHorizontal SliderVertical ColorTransparent ColorWhite ColorBlack ColorRed ColorGreen ColorBlue Hex Rgba BgRepeatNone BgRepeatBoth BgPosCenter BgSizeCover BgSizeContain Origin OriginCenter Xlate EaseDefault EaseLinear EaseIn EaseOut EaseInOut EaseInSine EaseOutSine EaseInCubic EaseOutCubic EaseInOutCubic EaseInCirc EaseOutCirc EaseInElastic EaseOutElastic EaseInBack EaseOutBack EaseInBounce EaseOutBounce EaseInOutBounce FilterBlur FilterGrayscale FilterContrast FilterHueRotate FilterInvert FilterOpacity FilterSepia FilterTint',
        element: () => <CssHelpersReferencePage />,
      },
      {
        id: 'advanced-api-page',
        canonicalId: 'advanced-api',
        title: 'Advanced API Reference',
        path: '/api/advanced',
        keywords: ['PropTypes', 'HostContext', 'IScheduler', 'FlushSync', 'SnapshotAssert', 'ElementRegistry', 'VirtualNode'],
        searchContent: 'advanced api reference PropTypes PropTypeDefinition PropTypeValidator WithPropTypes String Int Float Bool Object Class factory methods validation HostContext SetContextValue ResolveContext Environment initialization IScheduler Priority High Normal Low Idle Enqueue BeginBatch EndBatch PumpNow scheduling FlushSync synchronous flush state batching SnapshotAssert Compare AssertEqual testing snapshot diff ElementRegistry Register Resolve GetDefaultRegistry CreateFilteredRegistry element adapters VirtualNode VirtualNodeType Intrinsic Component Fragment Portal Text ErrorBoundary NodeType Properties Children error handling patterns ErrorBoundary fallback render depth guard MaxRenderDepth 25 infinite loop detection',
        element: () => <AdvancedAPIPage />,
      },
    ],
  },
  {
    id: 'reference-guides',
    title: 'Reference & Guides',
    pages: [
      {
        id: 'language-reference',
        canonicalId: 'language-reference',
        title: 'Language Reference',
        path: '/reference',
        keywords: ['directives', 'syntax', 'control flow', 'expressions'],
        searchContent: 'uitkx language reference directives syntax control flow expressions header directives @namespace My.Game.UI c# namespace generated class @component MyButton component class name must match filename @using System.Collections.Generic adds using directive generated file @props MyButtonProps props type consumed by the component @key root-key static key root element @inject ILogger logger dependency-injected field function-style components component keyword preamble declaration parameters typed optional default @using UnityEngine component Counter string label Count var count setCount useState return VisualElement Label text Button onClick setCount conditional rendering @if @else @foreach @switch @case @for @while @(expr) render component expression inline markup children {expr} c# expression attribute value literal plain string attribute // line comment /* block comment */ standard c-style comments fragment <> </> invisible wrapper rules gotchas hook calls must be unconditional component top level single root element component names must match filename reconciliation setup code return() switch expression jsx in setup code bare jsx assignment ternary paren-wrapped collection initializer array list dictionary new VirtualNode[]',
        element: () => <UitkxReferencePage />,
      },
      {
        id: 'diagnostics',
        canonicalId: 'diagnostics',
        title: 'Diagnostics',
        path: '/diagnostics',
        keywords: ['diagnostics', 'errors', 'warnings', 'codes'],
        searchContent: 'diagnostics reference diagnostic code source generator language server severity meaning fix compile time roslyn processing .uitkx files uitkx0001 uitkx0002 uitkx0005 uitkx0006 uitkx0008 uitkx0009 uitkx0010 uitkx0012 uitkx0013 uitkx0014 uitkx0015 uitkx0016 uitkx0017 uitkx0018 uitkx0019 uitkx0020 uitkx0021 uitkx0022 uitkx0023 uitkx0024 structural diagnostics language server real time squiggly underlines editor uitkx0101 uitkx0102 uitkx0103 uitkx0104 uitkx0105 uitkx0106 uitkx0107 uitkx0108 uitkx0109 uitkx0111 uitkx0112 uitkx0120 uitkx0121 uitkx0200 parser diagnostics uitkx0300 uitkx0301 uitkx0302 uitkx0303 uitkx0304 uitkx0305 uitkx0306 function-style component diagnostics uitkx2100 uitkx2101 uitkx2102 uitkx2104 uitkx2105 uitkx2106',
        element: () => <UitkxDiagnosticsPage />,
      },
      {
        id: 'config',
        canonicalId: 'configuration',
        title: 'Configuration',
        path: '/config',
        keywords: ['config', 'settings', 'vscode', 'extension'],
        searchContent: 'configuration reference options editor extensions formatter vs code extension settings uitkx.server.path uitkx.server.dotnetpath uitkx.trace.server editor defaults editor.defaultformatter reactiveuitk.uitkx editor.formatonsave editor.tabsize editor.insertspaces editor.bracketpaircolorization semantic tokens',
        element: () => <UitkxConfigPage />,
      },
      {
        id: 'debugging',
        canonicalId: 'debugging',
        title: 'Debugging Guide',
        path: '/debugging',
        keywords: ['debugging', 'troubleshooting', 'logs', 'generated code'],
        searchContent: 'debugging guide diagnose fix common issues inspecting generated code .uitkx .uitkx.g.cs roslyn source generator vs code definition f12 generatedfiles analyzers #line directives breakpoint stack trace ui toolkit debugger lsp server logs trace level uitkx.trace.server verbose output panel json-rpc missing completions stale diagnostics crashes formatter issues format-on-save reporting bugs',
        element: () => <UitkxDebuggingPage />,
      },
    ],
  },
  {
    id: 'faq',
    title: 'FAQ',
    pages: [
      {
        id: 'faq-page',
        canonicalId: 'faq-page',
        title: 'FAQ',
        path: '/faq',
        keywords: ['faq', 'frequently asked questions', 'help'],
        searchContent: 'frequently asked questions what is uitkx markup language authoring unity ui toolkit components react-like model .uitkx jsx-style hooks control flow roslyn source generator which unity versions supported unity 6.2 does uitkx work with existing ui toolkit code visualelement runtime overhead reconciliation scheduler per-frame cost aot-compatible production builds plain c# which editors supported vs code visual studio 2022 jetbrains rider .net version language server .net 8 dotnet directive-header form function-style components hmr hot module replacement hooks top level unconditional burst assembly-csharp-editor completions hover stopped working debugging guide',
        element: () => <FAQPage />,
      },
    ],
  },
  {
    id: 'known-issues',
    title: 'Known Issues',
    pages: [
      {
        id: 'known-issues-page',
        canonicalId: 'known-issues-page',
        title: 'Known Issues',
        path: '/known-issues',
        keywords: ['issues', 'limitations', 'known issues'],
        searchContent: 'known issues runtime multicolumnlistview briefly jump snap scrolling large data sets burst aot assembly resolution mono.cecil.assemblyresolutionexception failed resolve assembly assembly-csharp-editor project settings burst aot exclusion list HMR limitations memory leak assembly unloading mono domain reload JIT warmup roslyn first compile new file detection render depth guard MaxRenderDepth 25 infinite loop detection component tree depth hook constraints unconditional ordering thread safety main thread editor vs runtime differences EditorRenderScheduler RuntimeRenderScheduler drag events editor-only components PropertyInspector IMGUIContainer',
        element: () => <KnownIssuesPage />,
      },
    ],
  },
  {
    id: 'roadmap',
    title: 'Roadmap',
    pages: [
      {
        id: 'roadmap-page',
        canonicalId: 'roadmap-page',
        title: 'Roadmap',
        path: '/roadmap',
        keywords: ['roadmap', 'future', 'plans'],
        searchContent: 'roadmap planned features v0.3.0 function-style components control block bodies switch expressions CssHelpers expansion portal suspense fragment vs code visual studio rider extensions performance profiling testing utilities refactoring actions animation transitions PropTypes element adapter registration',
        element: () => <RoadmapPage />,
      },
    ],
  },
]

// ---------------------------------------------------------------------------
// Flat page lists
// ---------------------------------------------------------------------------

export const allFlat: DocPage[] = sections.flatMap((section) => {
  if (section.title === 'Components') {
    const common = section.pages.filter((page) => page.group === 'basic')
    const uncommon = section.pages.filter((page) => page.group === 'advanced' || !page.group)
    return [...common, ...uncommon]
  }
  return section.pages
})

// ---------------------------------------------------------------------------
// Version-aware filtering
// ---------------------------------------------------------------------------

/** Check if a page is available for the given Unity version. */
const isPageAvailable = (page: DocPage, selectedVersion: string): boolean => {
  if (page.sinceUnity) {
    return compareVersions(page.sinceUnity, selectedVersion) <= 0
  }
  const pv = PAGE_VERSIONS[page.canonicalId]
  return isAvailableIn(pv, selectedVersion)
}

/** Filter sections to only include pages available in the selected version. */
export const getFilteredSections = (selectedVersion: string): DocSection[] =>
  sections
    .map((section) => ({
      ...section,
      pages: section.pages.filter((page) => isPageAvailable(page, selectedVersion)),
    }))
    .filter((section) => section.pages.length > 0)

/** Flat page list filtered by version — for search and sidebar. */
export const getFilteredFlat = (selectedVersion: string): DocPage[] =>
  getFilteredSections(selectedVersion).flatMap((section) => {
    if (section.title === 'Components') {
      const common = section.pages.filter((page) => page.group === 'basic')
      const uncommon = section.pages.filter((page) => page.group === 'advanced' || !page.group)
      return [...common, ...uncommon]
    }
    return section.pages
  })
