import type { ReactElement } from 'react'
import type { Page as LegacyPage, Section as LegacySection } from './pages'
import { pages as legacySections } from './pages'
import { KnownIssuesPage } from './pages/KnownIssues/KnownIssuesPage'
import { RoadmapPage } from './pages/Roadmap/RoadmapPage'
import { UitkxAPIPage } from './pages/UITKX/API/UitkxAPIPage'
import { UitkxComponentReferencePage } from './pages/UITKX/Components/UitkxComponentReferencePage'
import { UitkxComponentsPage } from './pages/UITKX/Components/UitkxComponentsPage'
import { UitkxConceptsPage } from './pages/UITKX/Concepts/UitkxConceptsPage'
import { UitkxDifferencesPage } from './pages/UITKX/Differences/UitkxDifferencesPage'
import { UitkxGettingStartedPage } from './pages/UITKX/GettingStarted/UitkxGettingStartedPage'
import { UitkxIntroductionPage } from './pages/UITKX/Introduction/UitkxIntroductionPage'
import { UitkxRouterPage } from './pages/UITKX/Router/UitkxRouterPage'
import { UitkxSignalsPage } from './pages/UITKX/Signals/UitkxSignalsPage'
import { HmrPage } from './pages/Tooling/HMR/HmrPage'

export type DocTrack = 'uitkx' | 'csharp'

export type DocPage = {
  id: string
  canonicalId: string
  title: string
  path: string
  keywords?: string[]
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
        track: 'uitkx',
        element: () => <UitkxGettingStartedPage />,
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
        track: 'uitkx',
        element: () => <UitkxRouterPage />,
      },
      {
        id: 'uitkx-signals-page',
        canonicalId: 'signals',
        title: 'Signals',
        path: '/tooling/signals',
        keywords: ['uitkx', 'signals', 'shared state'],
        track: 'uitkx',
        element: () => <UitkxSignalsPage />,
      },
      {
        id: 'uitkx-hmr-page',
        canonicalId: 'hmr',
        title: 'Hot Module Replacement',
        path: '/tooling/hmr',
        keywords: ['uitkx', 'hmr', 'hot reload', 'live editing', 'instant preview'],
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
        track: 'uitkx',
        element: () => <UitkxAPIPage />,
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
