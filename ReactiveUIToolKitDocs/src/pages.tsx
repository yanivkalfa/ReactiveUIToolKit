import type { ReactElement } from 'react'
import { IntroductionPage } from './pages/Introduction/IntroductionPage'
import { GettingStartedPage } from './pages/GettingStarted/GettingStartedPage'
import { ToolingOverviewPage } from './pages/Tooling/ToolingOverviewPage'
import { RouterPage } from './pages/Router/RouterPage'
import { SignalsPage } from './pages/Signals/SignalsPage'
import { ComponentsPage } from './pages/Components/ComponentsPage'
import { DifferencesPage } from './pages/Differences/DifferencesPage'
import { APIPage } from './pages/API/APIPage'

export type Page = {
  id: string
  title: string
  path: string
  keywords?: string[]
  element: () => ReactElement
}

export type Section = {
  id: string
  title: string
  pages: Page[]
}

export const pages: Section[] = [
  {
    id: 'intro',
    title: 'Introduction',
    pages: [
      {
        id: 'introduction',
        title: 'Introduction',
        path: '/',
        keywords: ['overview', 'unity 6.2', 'reactive', 'ui toolkit'],
        element: () => <IntroductionPage />,
      },
    ],
  },
  {
    id: 'getting-started',
    title: 'Getting Started',
    pages: [
      {
        id: 'install',
        title: 'Install & Setup',
        path: '/getting-started',
        keywords: ['install', 'setup', 'unity package manager', 'dist'],
        element: () => <GettingStartedPage />,
      },
    ],
  },
  {
    id: 'differences',
    title: 'Different from React',
    pages: [
      {
        id: 'different-from-react',
        title: 'Different from React',
        path: '/differences',
        keywords: ['react', 'usestate', 'signals', 'differences'],
        element: () => <DifferencesPage />,
      },
    ],
  },
  {
    id: 'tooling',
    title: 'Tooling',
    pages: [
      {
        id: 'tooling-index',
        title: 'Overview',
        path: '/tooling',
        keywords: ['router', 'signals', 'hooks', 'suspense'],
        element: () => <ToolingOverviewPage />,
      },
      {
        id: 'router',
        title: 'Router',
        path: '/tooling/router',
        keywords: ['navigation', 'routes'],
        element: () => <RouterPage />,
      },
      {
        id: 'signals',
        title: 'Signals',
        path: '/tooling/signals',
        keywords: ['state', 'observable'],
        element: () => <SignalsPage />,
      },
    ],
  },
  {
    id: 'components',
    title: 'Components',
    pages: [
      {
        id: 'components-index',
        title: 'Components',
        path: '/components',
        keywords: ['elements', 'adapters', 'props'],
        element: () => <ComponentsPage />,
      },
    ],
  },
  {
    id: 'api',
    title: 'API',
    pages: [
      {
        id: 'api-reference',
        title: 'API Reference',
        path: '/api',
        keywords: ['api', 'namespace', 'props', 'hooks', 'router', 'signals'],
        element: () => <APIPage />,
      },
    ],
  },
]

export const flat: Page[] = pages.flatMap((s) => s.pages)
