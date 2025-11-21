import type { ReactElement } from 'react'
import { IntroductionPage } from './pages/Introduction/IntroductionPage'
import { GettingStartedPage } from './pages/GettingStarted/GettingStartedPage'
import { ToolingOverviewPage } from './pages/Tooling/ToolingOverviewPage'
import { RouterPage } from './pages/Router/RouterPage'
import { SignalsPage } from './pages/Signals/SignalsPage'
import { ComponentsPage } from './pages/Components/ComponentsPage'
import { DifferencesPage } from './pages/Differences/DifferencesPage'
import { APIPage } from './pages/API/APIPage'
import { BoundsFieldPage } from './pages/Components/BoundsField/BoundsFieldPage'
import { BoundsIntFieldPage } from './pages/Components/BoundsIntField/BoundsIntFieldPage'
import { BoxPage } from './pages/Components/Box/BoxPage'
import { ButtonPage } from './pages/Components/Button/ButtonPage'
import { ColorFieldPage } from './pages/Components/ColorField/ColorFieldPage'
import { DoubleFieldPage } from './pages/Components/DoubleField/DoubleFieldPage'
import { DropdownFieldPage } from './pages/Components/DropdownField/DropdownFieldPage'
import { EnumFieldPage } from './pages/Components/EnumField/EnumFieldPage'
import { EnumFlagsFieldPage } from './pages/Components/EnumFlagsField/EnumFlagsFieldPage'
import { FloatFieldPage } from './pages/Components/FloatField/FloatFieldPage'

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
      {
        id: 'component-bounds-field',
        title: 'BoundsField',
        path: '/components/bounds-field',
        keywords: ['bounds', 'field', 'BoundsField'],
        element: () => <BoundsFieldPage />,
      },
      {
        id: 'component-bounds-int-field',
        title: 'BoundsIntField',
        path: '/components/bounds-int-field',
        keywords: ['boundsint', 'field', 'BoundsIntField'],
        element: () => <BoundsIntFieldPage />,
      },
      {
        id: 'component-box',
        title: 'Box',
        path: '/components/box',
        keywords: ['box', 'container'],
        element: () => <BoxPage />,
      },
      {
        id: 'component-button',
        title: 'Button',
        path: '/components/button',
        keywords: ['button', 'click'],
        element: () => <ButtonPage />,
      },
      {
        id: 'component-color-field',
        title: 'ColorField',
        path: '/components/color-field',
        keywords: ['color', 'field', 'ColorField'],
        element: () => <ColorFieldPage />,
      },
      {
        id: 'component-double-field',
        title: 'DoubleField',
        path: '/components/double-field',
        keywords: ['double', 'field', 'DoubleField'],
        element: () => <DoubleFieldPage />,
      },
      {
        id: 'component-dropdown-field',
        title: 'DropdownField',
        path: '/components/dropdown-field',
        keywords: ['dropdown', 'field', 'choices'],
        element: () => <DropdownFieldPage />,
      },
      {
        id: 'component-enum-field',
        title: 'EnumField',
        path: '/components/enum-field',
        keywords: ['enum', 'field', 'EnumField'],
        element: () => <EnumFieldPage />,
      },
      {
        id: 'component-enum-flags-field',
        title: 'EnumFlagsField',
        path: '/components/enum-flags-field',
        keywords: ['enum', 'flags', 'EnumFlagsField'],
        element: () => <EnumFlagsFieldPage />,
      },
      {
        id: 'component-float-field',
        title: 'FloatField',
        path: '/components/float-field',
        keywords: ['float', 'field', 'FloatField'],
        element: () => <FloatFieldPage />,
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
