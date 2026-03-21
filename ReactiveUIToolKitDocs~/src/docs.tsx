import type { ReactElement } from 'react'
import type { Page as LegacyPage, Section as LegacySection } from './pages'
import { pages as legacySections } from './pages'
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

export type DocTrack = 'uitkx' | 'csharp'

export type DocPage = {
  id: string
  canonicalId: string
  title: string
  path: string
  keywords?: string[]
  searchContent?: string
  group?: 'basic' | 'advanced'
  track: DocTrack
  element: () => ReactElement
}

export type DocSection = {
  id: string
  title: string
  track: DocTrack
  pages: DocPage[]
}

const legacyComponentPages = legacySections.find((section) => section.id === 'components')?.pages ?? []

const prefixPath = (prefix: string, path: string) => (path === '/' ? prefix : `${prefix}${path}`)

const withTrackPrefix = (track: DocTrack, sections: LegacySection[], prefix: string): DocSection[] =>
  sections.map((section) => ({
    id: `${track}-${section.id}`,
    title: section.title,
    track,
    pages: section.pages.map((page: LegacyPage) => ({
      id: `${track}-${page.id}`,
      canonicalId: page.id,
      title: page.title,
      path: prefixPath(prefix, page.path),
      keywords: page.keywords,
      searchContent: page.searchContent,
      group: page.group,
      track,
      element: page.element,
    })),
  }))

export const uitkxSections: DocSection[] = [
  {
    id: 'uitkx-intro',
    title: 'Introduction',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-introduction',
        canonicalId: 'introduction',
        title: 'Introduction',
        path: '/',
        keywords: ['uitkx', 'introduction', 'markup', 'unity ui toolkit'],
        searchContent: 'reactiveuitoolkit react-like ui toolkit runtime unity uitkx primary authoring language function-style components .uitkx hooks state effects reconcile visualelement markup-first c# v.* trees component countercard usestate return text button onclick highlights reactive diffing batched updates router signals utilities generated c# output production builds no runtime codegen var count setcount',
        track: 'uitkx',
        element: () => <UitkxIntroductionPage />,
      },
    ],
  },
  {
    id: 'uitkx-getting-started',
    title: 'Getting Started',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-getting-started-page',
        canonicalId: 'install',
        title: 'Install & Setup',
        path: '/getting-started',
        keywords: ['uitkx', 'install', 'setup', 'component', 'partial'],
        searchContent: 'uitkx getting started primary authoring model reactiveuitoolkit function-style .uitkx components source generator produces complete class no boilerplate install via unity package manager open package manager add package from git url create a uitkx component setup code returned markup generator emits render mount rootrenderer v.func @namespace MyGame.UI component HelloWorld var count setCount useState return VisualElement Text Hello ReactiveUITK Button Increment onClick setCount count + 1 companion files optional styles types utils',
        track: 'uitkx',
        element: () => <UitkxGettingStartedPage />,
      },
    ],
  },
  {
    id: 'uitkx-companion-files',
    title: 'Companion Files',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-companion-files-page',
        canonicalId: 'companion-files',
        title: 'Companion Files',
        path: '/companion-files',
        keywords: ['uitkx', 'companion', 'styles', 'types', 'utils', 'partial class'],
        searchContent: 'companion files optional .cs file styles types utils naming conventions directory layout source generator produces complete class no boilerplate needed MyComponent.styles.cs style constants helpers colours sizes MyComponent.types.cs enums structs DTOs MyComponent.utils.cs pure helper formatting functions hmr support editing companion triggers hmr creating new file detected instantly @code blocks when not to use simple components small helpers',
        track: 'uitkx',
        element: () => <CompanionFilesPage />,
      },
    ],
  },
  {
    id: 'uitkx-components',
    title: 'Components Overview',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-components-overview',
        canonicalId: 'uitkx-components-overview',
        title: 'Components Overview',
        path: '/components',
        keywords: ['uitkx', 'components', 'intrinsic tags', 'custom components'],
        searchContent: 'components in uitkx intrinsic tags visualelement button text router tags custom components pascalcase names native element consumers custom component name authoring guidelines prefer direct tag props hand-building props objects keep setup code small close to returned markup tree custom component names must not collide with native tag name',
        track: 'uitkx',
        element: () => <UitkxComponentsPage />,
      },
    ],
  },
  {
    id: 'uitkx-component-reference',
    title: 'Components',
    track: 'uitkx',
    pages: legacyComponentPages.map((page: LegacyPage) => ({
      id: `uitkx-${page.id}`,
      canonicalId: page.id,
      title: page.title,
      path: page.path,
      keywords: ['uitkx', ...(page.keywords ?? [])],
      group: page.group,
      track: 'uitkx',
      element: () => (
        <UitkxComponentReferencePage
          title={page.title}
        />
      ),
    })),
  },
  {
    id: 'uitkx-concepts',
    title: 'Concepts & Environment',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-concepts-page',
        canonicalId: 'concepts-and-environment',
        title: 'Concepts & Environment',
        path: '/concepts',
        keywords: ['uitkx', 'concepts', 'environment', 'defines'],
        searchContent: 'concepts and environment uitkx authoring layer reactiveuitk runtime layer components intrinsic tags hooks markup structure reconciliation scheduling adapter application mental model write ui uitkx setup code local component generator runtime bridge unity ui toolkit core authoring rules intrinsic uitkx native tag names reserved custom components distinct names function-style components default form setup code first single returned markup tree state setters called directly as functions setcount(count + 1) companion partial classes host generated output environment defines compile-time environment tracing symbols env_dev env_staging env_prod environment labeling ruitk_trace_verbose ruitk_trace_basic runtime diagnostics editor-only diagnostic helpers development symbols',
        track: 'uitkx',
        element: () => <UitkxConceptsPage />,
      },
    ],
  },
  {
    id: 'uitkx-differences',
    title: 'Different from React',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-differences-page',
        canonicalId: 'different-from-react',
        title: 'Different from React',
        path: '/differences',
        keywords: ['uitkx', 'react', 'hooks', 'rendering'],
        searchContent: 'different from react uitkx borrows react component-and-hooks mental model unity ui toolkit c# runtime markup-first visualelement system scheduling model c# semantics state updates usestate behaves like react usestate setter directly value updater function uitkx lowers runtime hook implementation component rendering model reactiveuitk fiber schedule work asynchronously scheduler sliced render work deferred passive effects concurrent feature surface scheduler unity runtime constraints jsx-like syntax runtime representation browser dom model interop unity controls styles events first-class constraint apis differ from browser react conventions',
        track: 'uitkx',
        element: () => <UitkxDifferencesPage />,
      },
    ],
  },
  {
    id: 'uitkx-tooling',
    title: 'Tooling',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-router-page',
        canonicalId: 'router',
        title: 'Router',
        path: '/tooling/router',
        keywords: ['uitkx', 'router', 'routes', 'navigation'],
        searchContent: 'router uitkx routing authored directly in markup Router Route links routed child components returned UI tree Router establishes routing context subtree Route matches paths render elements RouterHooks setup code imperative navigation history control params query values navigation state RouterHooks.UseNavigate() pushes replaces locations UseGo() UseCanGo() back forward RouterHooks.UseLocationInfo() UseParams() UseQuery() UseNavigationState() expose active routed data RouterHooks.UseBlocker() intercept transitions unsaved guarded state declarative route composition imperative helpers @using ReactiveUITK.Router component RouterDemo var navigate RouterHooks.UseNavigate var parameters RouterHooks.UseParams var query RouterHooks.UseQuery RouterNavLink path label exact Route path /users/:id VisualElement Text User Not found',
        track: 'uitkx',
        element: () => <UitkxRouterPage />,
      },
      {
        id: 'uitkx-signals-page',
        canonicalId: 'signals',
        title: 'Signals',
        path: '/tooling/signals',
        keywords: ['uitkx', 'signals', 'shared state'],
        searchContent: 'signals shared-state primitive uitkx authoring model signal registry useSignal dispatch updates event handlers SignalsRuntime.EnsureInitialized Signals.Get SignalFactory.Get useMemo @using ReactiveUITK.Signals System component SignalCounterDemo var counterSignal SignalFactory.Get int demo.counter var count useSignal counterSignal Text Signal Counter Count Button Increment onClick counterSignal.Dispatch v + 1 Reset Dispatch 0 Style StyleKeys.FlexDirection row',
        track: 'uitkx',
        element: () => <UitkxSignalsPage />,
      },
      {
        id: 'uitkx-hmr-page',
        canonicalId: 'hmr',
        title: 'Hot Module Replacement',
        path: '/tooling/hmr',
        keywords: ['uitkx', 'hmr', 'hot reload', 'live editing', 'instant preview'],
        searchContent: 'hot module replacement hmr edit .uitkx files changes instantly unity editor without domain reload component state quick start open reactiveuitk hmr mode start edit save updates in-place hook state counters refs effects preserved assembly reloads filesystemwatcher detects parsed emitted compiled in-process roslyn microsoft.codeanalysis.csharp csc.dll fallback assembly.load render delegate swapped rootrenderer instances re-render hooks state preservation usestate useref useeffect usememo usecallback usecontext companion files .cs partial class style types create new companion auto-detected new component support cs0103 dependency auto-discovery cross-component assembly registry hmr window stats swap count error timing parse emit compile keyboard shortcuts toggle start stop lifecycle limitations old assemblies memory mono unload 10-30 kb per swap first compile jit warmup nuget cache troubleshooting console errors',
        track: 'uitkx',
        element: () => <HmrPage />,
      },
    ],
  },
  {
    id: 'uitkx-api',
    title: 'API',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-api-page',
        canonicalId: 'api-reference',
        title: 'API Map',
        path: '/api',
        keywords: ['uitkx', 'api', 'hooks', 'runtime'],
        searchContent: 'uitkx api map author markup hooks setup code runtime types mount integrate debug authoring surface .uitkx function-style components usestate useeffect usememo usesignal router context helpers intrinsic tags built-in reactiveuitk ui toolkit elements runtime layer virtualnode rootrenderer editorrootrendererutility props classes typed styles',
        track: 'uitkx',
        element: () => <UitkxAPIPage />,
      },
    ],
  },
  {
    id: 'uitkx-reference-guides',
    title: 'Reference & Guides',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-language-reference',
        canonicalId: 'language-reference',
        title: 'Language Reference',
        path: '/reference',
        keywords: ['uitkx', 'directives', 'syntax', 'control flow', 'expressions'],
        searchContent: 'uitkx language reference directives syntax control flow expressions header directives @namespace My.Game.UI c# namespace generated class @component MyButton component class name must match filename @using System.Collections.Generic adds using directive generated file @props MyButtonProps props type consumed by the component @key root-key static key root element @inject ILogger logger dependency-injected field function-style components component keyword preamble declaration parameters typed optional default @using UnityEngine component Counter string label Count var count setCount useState return VisualElement Label text Button onClick setCount conditional rendering @if (isLoggedIn) Label Welcome back @else Button Log in login @foreach (var item in items) Label key item.Id text item.Name @switch (mode) @case dark Label Dark mode @default Label Light mode @for c-style for loop @while while loop @break exit loop @continue skip next iteration @(expr) @(MyCustomComponent) render component expression inline markup children {expr} c# expression attribute value literal plain string attribute {/* comment */} jsx-style block comment rules gotchas hook calls must be unconditional component top level single root element component names must match filename reconciliation',
        track: 'uitkx',
        element: () => <UitkxReferencePage />,
      },
      {
        id: 'uitkx-diagnostics',
        canonicalId: 'diagnostics',
        title: 'Diagnostics',
        path: '/diagnostics',
        keywords: ['uitkx', 'diagnostics', 'errors', 'warnings', 'codes'],
        searchContent: 'diagnostics reference diagnostic code uitkx source generator language server severity meaning fix source generator diagnostics compile time roslyn processing .uitkx files uitkx0001 warning unknown built-in element tag name pascalcase button label uitkx0002 warning unknown attribute element attribute name property props type uitkx0005 error missing required directive @namespace @component uitkx0006 warning @component name mismatch rename file uitkx0008 warning unknown function component type exists public static render method uitkx0009 warning @foreach child missing key stable unique identifier uitkx0010 warning duplicate sibling key unique key value uitkx0012 error directive order error @namespace above @component uitkx0013 error hook in conditional hook call must be at component top level not inside @if branch uitkx0014 error hook in loop must be at component top level not inside @foreach loop uitkx0015 error hook in switch case must be at component top level uitkx0016 error hook in event handler must be at component top level uitkx0017 error multiple root elements wrap in single container element visualelement uitkx0018 warning useeffect missing dependency array explicit dependency array.empty uitkx0019 warning loop variable used as key stable unique identifier not loop index uitkx0020 error ref on component without ref param parameter remove ref attribute uitkx0021 error ref ambiguous multiple ref params explicit prop name structural diagnostics language server real time squiggly underlines editor uitkx0101 uitkx0102 uitkx0103 uitkx0104 uitkx0105 uitkx0106 uitkx0107 hint unreachable code uitkx0108 uitkx0109 uitkx0111 unused parameter parser diagnostics malformed syntax uitkx0300 unexpected token uitkx0301 unclosed tag uitkx0302 mismatched closing tag uitkx0303 unexpected end of file uitkx0304 unclosed expression or block uitkx0305 unknown markup directive uitkx0306 @(expr) in setup code inline expressions only valid inside markup',
        track: 'uitkx',
        element: () => <UitkxDiagnosticsPage />,
      },
      {
        id: 'uitkx-config',
        canonicalId: 'configuration',
        title: 'Configuration',
        path: '/config',
        keywords: ['uitkx', 'config', 'settings', 'vscode', 'extension'],
        searchContent: 'configuration reference configuration options uitkx editor extensions formatter vs code extension settings uitkx.server.path string absolute path to custom uitkxlanguageserver.dll leave empty to use bundled server uitkx.server.dotnetpath string path to dotnet executable .net 8+ sdk non-standard location uitkx.trace.server enum off controls lsp trace output messages verbose json-rpc traffic output panel uitkx language server channel editor defaults extension automatically configures editor settings for .uitkx files editor.defaultformatter reactiveuitk.uitkx uitkx formatter editor.formatonsave true auto-format on save recommended editor.tabsize 2 uitkx 2-space indentation editor.insertspaces true spaces not tabs editor.bracketpaircolorization false disabled conflicting colors uitkx semantic tokens',
        track: 'uitkx',
        element: () => <UitkxConfigPage />,
      },
      {
        id: 'uitkx-debugging',
        canonicalId: 'debugging',
        title: 'Debugging Guide',
        path: '/debugging',
        keywords: ['uitkx', 'debugging', 'troubleshooting', 'logs', 'generated code'],
        searchContent: 'debugging guide diagnose fix common issues uitkx inspecting generated code .uitkx .uitkx.g.cs roslyn source generator vs code definition f12 generated symbol generatedfiles folder analyzers output directory #line directives map errors original .uitkx file line number library packagecache com.reactiveuitk understanding #line directives c# compiler error generated code lsp server logs detailed lsp communication trace level vs code settings uitkx.trace.server verbose output panel ctrl+shift+u uitkx language server channel json-rpc requests responses missing completions stale diagnostics textdocument/publishdiagnostics server crashes formatter issues formatting unexpected results syntax errors red squiggles format-on-save editor.defaultformatter reactiveuitk.uitkx shift+alt+f reporting bugs minimal .uitkx file reproduces problem exact error message diagnostic code editor extension version lsp trace output',
        track: 'uitkx',
        element: () => <UitkxDebuggingPage />,
      },
    ],
  },
  {
    id: 'uitkx-faq',
    title: 'FAQ',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-faq-page',
        canonicalId: 'faq-page',
        title: 'FAQ',
        path: '/faq',
        keywords: ['faq', 'frequently asked questions', 'help'],
        searchContent: 'frequently asked questions what is uitkx markup language authoring unity ui toolkit components react-like model .uitkx jsx-style hooks control flow roslyn source generator which unity versions supported unity 6.2 does uitkx work with existing ui toolkit code visualelement does uitkx add runtime overhead reconciliation scheduler per-frame cost aot-compatible production builds plain c# no interpreter no runtime codegen which editors supported vs code visual studio 2022 full extensions syntax highlighting completions hover diagnostics formatting jetbrains rider stub not officially supported v1 do i need the vs code extension source generator runs inside unity wrong colours briefly textmate grammar lsp semantic tokens 200ms what .net version language server .net 8 dotnet directive-header form function-style components @namespace @component @props setup code c# @using hmr hot module replacement build times bypasses unity compilation roslyn assembly.load 50-200ms hooks top level unconditional @if @foreach reconciler burst assembly-csharp-editor project settings burst aot exclusion list completions hover stopped working uitkx.trace.server verbose output panel debugging guide red squiggles saved @namespace before @component',
        track: 'uitkx',
        element: () => <FAQPage />,
      },
    ],
  },
  {
    id: 'uitkx-known-issues',
    title: 'Known Issues',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-known-issues-page',
        canonicalId: 'known-issues-page',
        title: 'Known Issues',
        path: '/known-issues',
        keywords: ['issues', 'limitations', 'known issues'],
        searchContent: 'known issues runtime multicolumnlistview briefly jump snap scrolling large data sets burst aot assembly resolution mono.cecil.assemblyresolutionexception failed resolve assembly assembly-csharp-editor project settings burst aot exclusion list editor-only assemblies uitkx types',
        track: 'uitkx',
        element: () => <KnownIssuesPage />,
      },
    ],
  },
  {
    id: 'uitkx-roadmap',
    title: 'Roadmap',
    track: 'uitkx',
    pages: [
      {
        id: 'uitkx-roadmap-page',
        canonicalId: 'roadmap-page',
        title: 'Roadmap',
        path: '/roadmap',
        keywords: ['roadmap', 'future', 'plans'],
        searchContent: 'roadmap documented future update planned features',
        track: 'uitkx',
        element: () => <RoadmapPage />,
      },
    ],
  },
]

export const csharpSections: DocSection[] = withTrackPrefix('csharp', legacySections, '/csharp')

export const docsByTrack: Record<DocTrack, DocSection[]> = {
  uitkx: uitkxSections,
  csharp: csharpSections,
}

export const allSections: DocSection[] = [...uitkxSections, ...csharpSections]

export const getTrackFromPath = (pathname: string): DocTrack =>
  pathname === '/csharp' || pathname.startsWith('/csharp/') ? 'csharp' : 'uitkx'

export const getSectionsForTrack = (track: DocTrack): DocSection[] => docsByTrack[track]

export const getFlatForTrack = (track: DocTrack): DocPage[] =>
  docsByTrack[track].flatMap((section) => {
    if (section.title === 'Components') {
      const common = section.pages.filter((page) => page.group === 'basic')
      const uncommon = section.pages.filter((page) => page.group === 'advanced' || !page.group)
      return [...common, ...uncommon]
    }
    return section.pages
  })

export const allFlat: DocPage[] = [...getFlatForTrack('uitkx'), ...getFlatForTrack('csharp')]

export const getTrackHome = (track: DocTrack) => (track === 'uitkx' ? '/' : '/csharp')

export const getMatchingPathInTrack = (track: DocTrack, canonicalId: string) =>
  allFlat.find((page) => page.track === track && page.canonicalId === canonicalId)?.path ?? getTrackHome(track)

export const legacyRedirects = legacySections
  .flatMap((section) => section.pages)
  .filter((page) => page.path !== '/')
  .map((page) => ({
    from: page.path,
    to: prefixPath('/csharp', page.path),
  }))
