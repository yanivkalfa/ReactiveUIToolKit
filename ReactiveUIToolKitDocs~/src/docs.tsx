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
import { HmrPage } from './pages/Tooling/HMR/HmrPage'
import { FAQPage } from './pages/FAQ/FAQPage'
import { StylingPage } from './pages/UITKX/Styling/StylingPage'

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
        searchContent: 'getting started reactiveuitoolkit function-style .uitkx components source generator produces complete class no boilerplate install via unity package manager open package manager add package from git url create a uitkx component setup code returned markup generator emits render mount rootrenderer @namespace MyGame.UI component HelloWorld var count setCount useState return VisualElement Text Hello ReactiveUITK Button Increment onClick setCount count + 1 companion files optional styles types utils',
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
        searchContent: 'companion files optional .cs file styles types utils naming conventions directory layout source generator produces complete class no boilerplate needed MyComponent.styles.cs style constants helpers colours sizes MyComponent.types.cs enums structs DTOs MyComponent.utils.cs pure helper formatting functions hmr support editing companion triggers hmr creating new file detected instantly @code blocks when not to use simple components small helpers',
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
        searchContent: 'styling typed style class compile-time checked properties inline style system CssHelpers static helpers Pct Px Auto None Initial length units color helpers Hex Rgba enum shortcuts FlexDirection Row Column JustifyContent JustifyCenter AlignItems AlignCenter Stretch SpaceBetween SpaceAround Position Absolute Relative Display Flex Visibility Hidden Overflow WhiteSpace TextOverflow TextAnchor FontStyle StyleLength StyleFloat StyleKeyword Width Height Margin Padding BorderRadius BackgroundColor TextColor BorderColor FlexGrow FlexShrink Opacity FontSize LetterSpacing BackgroundRepeat BackgroundSize BackgroundPosition TransformOrigin Rotate Scale Translate typed properties tuple syntax escape hatch StyleKeys backward compatible property reference',
        element: () => <StylingPage />,
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
        searchContent: 'components intrinsic tags visualelement button text router tags custom components pascalcase names native element consumers custom component name authoring guidelines prefer direct tag props hand-building props objects keep setup code small close to returned markup tree custom component names must not collide with native tag name',
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
        searchContent: 'concepts and environment react-like component model unity ui toolkit components hooks markup reconciliation scheduling authoring rules intrinsic tag names reserved custom components distinct names function-style components setup code first single returned markup tree state setters called directly as functions setcount(count + 1) companion partial classes environment defines compile-time scripting define symbols env_dev env_staging env_prod environment labeling ruitk_trace_verbose ruitk_trace_basic ruitk_diff_tracing runtime diagnostics editor-only diagnostic helpers development symbols behavior summary trace level resolution priority hostcontext',
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
        searchContent: 'different from react component-and-hooks mental model unity ui toolkit c# runtime visualelement system scheduling model state updates usestate setter value updater function statesetter delegate statesetterextensions fluent rendering model fiber reconciler synchronous mode per frame no starttransition no concurrent rendering scheduler defer passive effects slice render work unity runtime constraints interop controls styles events apis differ from browser react conventions',
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
        searchContent: 'router lightweight in-memory router inspired by react router routing authored directly in markup Router Route links routed child components Router establishes routing context subtree Route matches paths render elements RouterHooks setup code imperative navigation history IRouterHistory MemoryHistory custom history UseNavigate pushes replaces locations UseGo UseCanGo back forward UseLocationInfo UseParams UseQuery UseNavigationState expose active routed data UseBlocker intercept transitions unsaved guarded state nested routes relative paths outlets parent match declarative route composition imperative helpers RouterNavLink',
        element: () => <UitkxRouterPage />,
      },
      {
        id: 'signals-page',
        canonicalId: 'signals',
        title: 'Signals',
        path: '/tooling/signals',
        keywords: ['signals', 'shared state', 'reactive'],
        searchContent: 'signals lightweight named reactive values process-wide registry observable store single source of truth global registry keyed by string Signals.Get SignalFactory.Get Signal Subscribe useSignal Dispatch updates event handlers SignalsRuntime.EnsureInitialized selector overloads useSignal signal selector comparer project slice custom equality useMemo SignalCounterDemo counterSignal count Increment Reset Style StyleKeys.FlexDirection row',
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
        searchContent: 'api reference map namespaces types core V VirtualNode Hooks UseState UseReducer UseEffect UseMemo UseSignal StateSetterExtensions RootRenderer RenderScheduler props typed ButtonProps LabelProps ListViewProps ScrollViewProps Style StyleKeys Router RouterHooks UseRouter UseLocation UseLocationInfo UseParams UseQuery UseNavigationState UseNavigate UseGo UseCanGo UseBlocker IRouterHistory MemoryHistory RouterLocation RouterPath RouteMatch Signals Signal Subscribe Set Dispatch SignalsRuntime animation UseAnimate UseTweenFloat AnimateTrack safe area UseSafeArea SafeAreaInsets VisualElementSafe editor EditorRootRendererUtility EditorRenderScheduler elements ElementRegistry ElementRegistryProvider',
        element: () => <UitkxAPIPage />,
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
        searchContent: 'uitkx language reference directives syntax control flow expressions header directives @namespace My.Game.UI c# namespace generated class @component MyButton component class name must match filename @using System.Collections.Generic adds using directive generated file @props MyButtonProps props type consumed by the component @key root-key static key root element @inject ILogger logger dependency-injected field function-style components component keyword preamble declaration parameters typed optional default @using UnityEngine component Counter string label Count var count setCount useState return VisualElement Label text Button onClick setCount conditional rendering @if @else @foreach @switch @case @for @while @break @continue @(expr) render component expression inline markup children {expr} c# expression attribute value literal plain string attribute {/* comment */} jsx-style block comment rules gotchas hook calls must be unconditional component top level single root element component names must match filename reconciliation',
        element: () => <UitkxReferencePage />,
      },
      {
        id: 'diagnostics',
        canonicalId: 'diagnostics',
        title: 'Diagnostics',
        path: '/diagnostics',
        keywords: ['diagnostics', 'errors', 'warnings', 'codes'],
        searchContent: 'diagnostics reference diagnostic code source generator language server severity meaning fix compile time roslyn processing .uitkx files uitkx0001 uitkx0002 uitkx0005 uitkx0006 uitkx0008 uitkx0009 uitkx0010 uitkx0012 uitkx0013 uitkx0014 uitkx0015 uitkx0016 uitkx0017 uitkx0018 uitkx0019 uitkx0020 uitkx0021 structural diagnostics language server real time squiggly underlines editor uitkx0101 uitkx0102 uitkx0103 uitkx0104 uitkx0105 uitkx0106 uitkx0107 uitkx0108 uitkx0109 uitkx0111 parser diagnostics uitkx0300 uitkx0301 uitkx0302 uitkx0303 uitkx0304 uitkx0305 uitkx0306',
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
        searchContent: 'debugging guide diagnose fix common issues inspecting generated code .uitkx .uitkx.g.cs roslyn source generator vs code definition f12 generatedfiles analyzers #line directives lsp server logs trace level uitkx.trace.server verbose output panel json-rpc missing completions stale diagnostics crashes formatter issues format-on-save reporting bugs',
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
        searchContent: 'known issues runtime multicolumnlistview briefly jump snap scrolling large data sets burst aot assembly resolution mono.cecil.assemblyresolutionexception failed resolve assembly assembly-csharp-editor project settings burst aot exclusion list',
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
        searchContent: 'roadmap documented future update planned features',
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
