import type { ReactElement } from 'react'
import { IntroductionPage } from './pages/Introduction/IntroductionPage'
import { GettingStartedPage } from './pages/GettingStarted/GettingStartedPage'
import { RouterPage } from './pages/Router/RouterPage'
import { SignalsPage } from './pages/Signals/SignalsPage'
import { ConceptsPage } from './pages/Concepts/ConceptsPage'
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
import { FoldoutPage } from './pages/Components/Foldout/FoldoutPage'
import { GroupBoxPage } from './pages/Components/GroupBox/GroupBoxPage'
import { Hash128FieldPage } from './pages/Components/Hash128Field/Hash128FieldPage'
import { HelpBoxPage } from './pages/Components/HelpBox/HelpBoxPage'
import { IMGUIContainerPage } from './pages/Components/IMGUIContainer/IMGUIContainerPage'
import { ImagePage } from './pages/Components/Image/ImagePage'
import { IntegerFieldPage } from './pages/Components/IntegerField/IntegerFieldPage'
import { LabelPage } from './pages/Components/Label/LabelPage'
import { LongFieldPage } from './pages/Components/LongField/LongFieldPage'
import { ProgressBarPage } from './pages/Components/ProgressBar/ProgressBarPage'
import { ListViewPage } from './pages/Components/ListView/ListViewPage'
import { MinMaxSliderPage } from './pages/Components/MinMaxSlider/MinMaxSliderPage'
import { ObjectFieldPage } from './pages/Components/ObjectField/ObjectFieldPage'
import { RadioButtonPage } from './pages/Components/RadioButton/RadioButtonPage'
import { RadioButtonGroupPage } from './pages/Components/RadioButtonGroup/RadioButtonGroupPage'
import { RepeatButtonPage } from './pages/Components/RepeatButton/RepeatButtonPage'
import { ScrollViewPage } from './pages/Components/ScrollView/ScrollViewPage'
import { SliderPage } from './pages/Components/Slider/SliderPage'
import { SliderIntPage } from './pages/Components/SliderInt/SliderIntPage'
import { TogglePage } from './pages/Components/Toggle/TogglePage'
import { TreeViewPage } from './pages/Components/TreeView/TreeViewPage'
import { TabPage } from './pages/Components/Tab/TabPage'
import { TabViewPage } from './pages/Components/TabView/TabViewPage'
import { ToggleButtonGroupPage } from './pages/Components/ToggleButtonGroup/ToggleButtonGroupPage'
import { TextFieldPage } from './pages/Components/TextField/TextFieldPage'
import { ToolbarPage } from './pages/Components/Toolbar/ToolbarPage'
import { RectFieldPage } from './pages/Components/RectField/RectFieldPage'
import { RectIntFieldPage } from './pages/Components/RectIntField/RectIntFieldPage'
import { UnsignedIntegerFieldPage } from './pages/Components/UnsignedIntegerField/UnsignedIntegerFieldPage'
import { UnsignedLongFieldPage } from './pages/Components/UnsignedLongField/UnsignedLongFieldPage'
import { Vector2FieldPage } from './pages/Components/Vector2Field/Vector2FieldPage'
import { Vector2IntFieldPage } from './pages/Components/Vector2IntField/Vector2IntFieldPage'
import { Vector3FieldPage } from './pages/Components/Vector3Field/Vector3FieldPage'
import { Vector3IntFieldPage } from './pages/Components/Vector3IntField/Vector3IntFieldPage'
import { Vector4FieldPage } from './pages/Components/Vector4Field/Vector4FieldPage'
import { TemplateContainerPage } from './pages/Components/TemplateContainer/TemplateContainerPage'
import { VisualElementPage } from './pages/Components/VisualElement/VisualElementPage'
import { VisualElementSafePage } from './pages/Components/VisualElementSafe/VisualElementSafePage'
import { AnimatePage } from './pages/Components/Animate/AnimatePage'
import { ErrorBoundaryPage } from './pages/Components/ErrorBoundary/ErrorBoundaryPage'
import { MultiColumnListViewPage } from './pages/Components/MultiColumnListView/MultiColumnListViewPage'
import { MultiColumnTreeViewPage } from './pages/Components/MultiColumnTreeView/MultiColumnTreeViewPage'
import { ScrollerPage } from './pages/Components/Scroller/ScrollerPage'
import { TextElementPage } from './pages/Components/TextElement/TextElementPage'
import { PropertyInspectorPage } from './pages/Components/PropertyInspector/PropertyInspectorPage'
import { TwoPaneSplitViewPage } from './pages/Components/TwoPaneSplitView/TwoPaneSplitViewPage'
import { KnownIssuesPage } from './pages/KnownIssues/KnownIssuesPage'
import { RoadmapPage } from './pages/Roadmap/RoadmapPage'
import { AnimationHooksPage } from './pages/SpecialHooks/AnimationHooksPage'
import { RouterHooksPage } from './pages/SpecialHooks/RouterHooksPage'
import { SignalsHooksPage } from './pages/SpecialHooks/SignalsHooksPage'
import { SafeAreaHooksPage } from './pages/SpecialHooks/SafeAreaHooksPage'

export type Page = {
  id: string
  title: string
  path: string
  keywords?: string[]
  group?: 'basic' | 'advanced'
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
    id: 'concepts',
    title: 'Concepts & Environment',
    pages: [
      {
        id: 'concepts-and-environment',
        title: 'Concepts & Environment',
        path: '/concepts',
        keywords: ['concepts', 'environment', 'defines', 'trace', 'react differences'],
        element: () => <ConceptsPage />,
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
        id: 'component-bounds-field',
        title: 'BoundsField',
        path: '/components/bounds-field',
        keywords: ['bounds', 'field', 'BoundsField'],
        group: 'advanced',
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
        group: 'basic',
        element: () => <BoxPage />,
      },
      {
        id: 'component-button',
        title: 'Button',
        path: '/components/button',
        keywords: ['button', 'click'],
        group: 'basic',
        element: () => <ButtonPage />,
      },
      {
        id: 'component-color-field',
        title: 'ColorField',
        path: '/components/color-field',
        keywords: ['color', 'field', 'ColorField'],
        group: 'advanced',
        element: () => <ColorFieldPage />,
      },
      {
        id: 'component-double-field',
        title: 'DoubleField',
        path: '/components/double-field',
        keywords: ['double', 'field', 'DoubleField'],
        group: 'advanced',
        element: () => <DoubleFieldPage />,
      },
      {
        id: 'component-dropdown-field',
        title: 'DropdownField',
        path: '/components/dropdown-field',
        keywords: ['dropdown', 'field', 'choices'],
        group: 'basic',
        element: () => <DropdownFieldPage />,
      },
      {
        id: 'component-enum-field',
        title: 'EnumField',
        path: '/components/enum-field',
        keywords: ['enum', 'field', 'EnumField'],
        group: 'basic',
        element: () => <EnumFieldPage />,
      },
      {
        id: 'component-enum-flags-field',
        title: 'EnumFlagsField',
        path: '/components/enum-flags-field',
        keywords: ['enum', 'flags', 'EnumFlagsField'],
        group: 'advanced',
        element: () => <EnumFlagsFieldPage />,
      },
      {
        id: 'component-float-field',
        title: 'FloatField',
        path: '/components/float-field',
        keywords: ['float', 'field', 'FloatField'],
        group: 'basic',
        element: () => <FloatFieldPage />,
      },
      {
        id: 'component-foldout',
        title: 'Foldout',
        path: '/components/foldout',
        keywords: ['foldout', 'toggle', 'collapsible'],
        group: 'basic',
        element: () => <FoldoutPage />,
      },
      {
        id: 'component-group-box',
        title: 'GroupBox',
        path: '/components/group-box',
        keywords: ['group', 'groupbox'],
        group: 'basic',
        element: () => <GroupBoxPage />,
      },
      {
        id: 'component-hash128-field',
        title: 'Hash128Field',
        path: '/components/hash128-field',
        keywords: ['hash128', 'field'],
        group: 'advanced',
        element: () => <Hash128FieldPage />,
      },
      {
        id: 'component-help-box',
        title: 'HelpBox',
        path: '/components/help-box',
        keywords: ['helpbox', 'message'],
        group: 'basic',
        element: () => <HelpBoxPage />,
      },
      {
        id: 'component-imgui-container',
        title: 'IMGUIContainer',
        path: '/components/imgui-container',
        keywords: ['imgui', 'editor'],
        group: 'advanced',
        element: () => <IMGUIContainerPage />,
      },
      {
        id: 'component-image',
        title: 'Image',
        path: '/components/image',
        keywords: ['image', 'texture', 'sprite'],
        group: 'basic',
        element: () => <ImagePage />,
      },
      {
        id: 'component-integer-field',
        title: 'IntegerField',
        path: '/components/integer-field',
        keywords: ['integer', 'field', 'int'],
        group: 'basic',
        element: () => <IntegerFieldPage />,
      },
      {
        id: 'component-label',
        title: 'Label',
        path: '/components/label',
        keywords: ['label', 'text'],
        group: 'basic',
        element: () => <LabelPage />,
      },
      {
        id: 'component-long-field',
        title: 'LongField',
        path: '/components/long-field',
        keywords: ['long', 'field', 'LongField'],
        group: 'advanced',
        element: () => <LongFieldPage />,
      },
      {
        id: 'component-progress-bar',
        title: 'ProgressBar',
        path: '/components/progress-bar',
        keywords: ['progress', 'bar'],
        group: 'basic',
        element: () => <ProgressBarPage />,
      },
      {
        id: 'component-list-view',
        title: 'ListView',
        path: '/components/list-view',
        keywords: ['list', 'ListView'],
        group: 'basic',
        element: () => <ListViewPage />,
      },
      {
        id: 'component-minmax-slider',
        title: 'MinMaxSlider',
        path: '/components/minmax-slider',
        keywords: ['minmax', 'slider'],
        group: 'advanced',
        element: () => <MinMaxSliderPage />,
      },
      {
        id: 'component-object-field',
        title: 'ObjectField',
        path: '/components/object-field',
        keywords: ['object', 'field'],
        group: 'advanced',
        element: () => <ObjectFieldPage />,
      },
      {
        id: 'component-radio-button',
        title: 'RadioButton',
        path: '/components/radio-button',
        keywords: ['radio', 'button'],
        group: 'basic',
        element: () => <RadioButtonPage />,
      },
      {
        id: 'component-radio-button-group',
        title: 'RadioButtonGroup',
        path: '/components/radio-button-group',
        keywords: ['radio', 'group'],
        group: 'basic',
        element: () => <RadioButtonGroupPage />,
      },
      {
        id: 'component-rect-field',
        title: 'RectField',
        path: '/components/rect-field',
        keywords: ['rect', 'field'],
        group: 'advanced',
        element: () => <RectFieldPage />,
      },
      {
        id: 'component-rect-int-field',
        title: 'RectIntField',
        path: '/components/rect-int-field',
        keywords: ['rectint', 'field'],
        group: 'advanced',
        element: () => <RectIntFieldPage />,
      },
      {
        id: 'component-repeat-button',
        title: 'RepeatButton',
        path: '/components/repeat-button',
        keywords: ['repeat', 'button'],
        group: 'basic',
        element: () => <RepeatButtonPage />,
      },
      {
        id: 'component-scroll-view',
        title: 'ScrollView',
        path: '/components/scroll-view',
        keywords: ['scroll', 'view'],
        group: 'basic',
        element: () => <ScrollViewPage />,
      },
      {
        id: 'component-slider',
        title: 'Slider',
        path: '/components/slider',
        keywords: ['slider', 'float'],
        group: 'basic',
        element: () => <SliderPage />,
      },
      {
        id: 'component-slider-int',
        title: 'SliderInt',
        path: '/components/slider-int',
        keywords: ['slider', 'int'],
        group: 'basic',
        element: () => <SliderIntPage />,
      },
      {
        id: 'component-toggle',
        title: 'Toggle',
        path: '/components/toggle',
        keywords: ['toggle', 'checkbox'],
        group: 'basic',
        element: () => <TogglePage />,
      },
      {
        id: 'component-tree-view',
        title: 'TreeView',
        path: '/components/tree-view',
        keywords: ['tree', 'TreeView'],
        group: 'basic',
        element: () => <TreeViewPage />,
      },
      {
        id: 'component-tab',
        title: 'Tab',
        path: '/components/tab',
        keywords: ['tab'],
        group: 'basic',
        element: () => <TabPage />,
      },
      {
        id: 'component-tab-view',
        title: 'TabView',
        path: '/components/tab-view',
        keywords: ['tab', 'TabView'],
        group: 'basic',
        element: () => <TabViewPage />,
      },
      {
        id: 'component-toggle-button-group',
        title: 'ToggleButtonGroup',
        path: '/components/toggle-button-group',
        keywords: ['toggle', 'buttons', 'group'],
        group: 'advanced',
        element: () => <ToggleButtonGroupPage />,
      },
      {
        id: 'component-text-field',
        title: 'TextField',
        path: '/components/text-field',
        keywords: ['text', 'field'],
        group: 'basic',
        element: () => <TextFieldPage />,
      },
      {
        id: 'component-toolbar',
        title: 'Toolbar',
        path: '/components/toolbar',
        keywords: ['toolbar', 'editor'],
        group: 'advanced',
        element: () => <ToolbarPage />,
      },
      {
        id: 'component-template-container',
        title: 'TemplateContainer',
        path: '/components/template-container',
        keywords: ['template', 'container'],
        group: 'advanced',
        element: () => <TemplateContainerPage />,
      },
      {
        id: 'component-visual-element',
        title: 'VisualElement',
        path: '/components/visual-element',
        keywords: ['visualelement', 'container', 'safe'],
        group: 'basic',
        element: () => <VisualElementPage />,
      },
      {
        id: 'component-visual-element-safe',
        title: 'VisualElementSafe',
        path: '/components/visual-element-safe',
        keywords: ['visualelementsafe', 'safe-area', 'container'],
        group: 'basic',
        element: () => <VisualElementSafePage />,
      },
      {
        id: 'component-unsigned-integer-field',
        title: 'UnsignedIntegerField',
        path: '/components/unsigned-integer-field',
        keywords: ['uint', 'field'],
        group: 'advanced',
        element: () => <UnsignedIntegerFieldPage />,
      },
      {
        id: 'component-unsigned-long-field',
        title: 'UnsignedLongField',
        path: '/components/unsigned-long-field',
        keywords: ['ulong', 'field'],
        group: 'advanced',
        element: () => <UnsignedLongFieldPage />,
      },
      {
        id: 'component-vector2-field',
        title: 'Vector2Field',
        path: '/components/vector2-field',
        keywords: ['vector2', 'field'],
        group: 'advanced',
        element: () => <Vector2FieldPage />,
      },
      {
        id: 'component-vector2-int-field',
        title: 'Vector2IntField',
        path: '/components/vector2-int-field',
        keywords: ['vector2int', 'field'],
        group: 'advanced',
        element: () => <Vector2IntFieldPage />,
      },
      {
        id: 'component-vector3-field',
        title: 'Vector3Field',
        path: '/components/vector3-field',
        keywords: ['vector3', 'field'],
        group: 'advanced',
        element: () => <Vector3FieldPage />,
      },
      {
        id: 'component-vector3-int-field',
        title: 'Vector3IntField',
        path: '/components/vector3-int-field',
        keywords: ['vector3int', 'field'],
        group: 'advanced',
        element: () => <Vector3IntFieldPage />,
      },
      {
        id: 'component-vector4-field',
        title: 'Vector4Field',
        path: '/components/vector4-field',
        keywords: ['vector4', 'field'],
        group: 'advanced',
        element: () => <Vector4FieldPage />,
      },
      {
        id: 'component-animate',
        title: 'Animate',
        path: '/components/animate',
        keywords: ['animate', 'animation'],
        group: 'basic',
        element: () => <AnimatePage />,
      },
      {
        id: 'component-error-boundary',
        title: 'ErrorBoundary',
        path: '/components/error-boundary',
        keywords: ['error', 'boundary'],
        group: 'advanced',
        element: () => <ErrorBoundaryPage />,
      },
      {
        id: 'component-multi-column-list-view',
        title: 'MultiColumnListView',
        path: '/components/multi-column-list-view',
        keywords: ['list', 'multi', 'columns'],
        group: 'basic',
        element: () => <MultiColumnListViewPage />,
      },
      {
        id: 'component-multi-column-tree-view',
        title: 'MultiColumnTreeView',
        path: '/components/multi-column-tree-view',
        keywords: ['tree', 'multi', 'columns'],
        group: 'basic',
        element: () => <MultiColumnTreeViewPage />,
      },
      {
        id: 'component-scroller',
        title: 'Scroller',
        path: '/components/scroller',
        keywords: ['scroller'],
        group: 'advanced',
        element: () => <ScrollerPage />,
      },
      {
        id: 'component-text-element',
        title: 'TextElement',
        path: '/components/text-element',
        keywords: ['text', 'TextElement'],
        group: 'advanced',
        element: () => <TextElementPage />,
      },
      {
        id: 'component-property-inspector',
        title: 'PropertyField & InspectorElement',
        path: '/components/property-inspector',
        keywords: ['propertyfield', 'inspectorelement', 'editor'],
        group: 'advanced',
        element: () => <PropertyInspectorPage />,
      },
      {
        id: 'component-two-pane-split-view',
        title: 'TwoPaneSplitView',
        path: '/components/two-pane-split-view',
        keywords: ['split', 'editor'],
        group: 'advanced',
        element: () => <TwoPaneSplitViewPage />,
      },
    ],
  },
  {
    id: 'special-hooks',
    title: 'Special Hooks',
    pages: [
      {
        id: 'special-hooks-animation',
        title: 'Animation hooks',
        path: '/special-hooks/animation',
        keywords: ['hooks', 'animation', 'UseAnimate', 'UseTweenFloat'],
        element: () => <AnimationHooksPage />,
      },
      {
        id: 'special-hooks-router',
        title: 'Router hooks',
        path: '/special-hooks/router',
        keywords: ['hooks', 'router', 'RouterHooks'],
        element: () => <RouterHooksPage />,
      },
      {
        id: 'special-hooks-signals',
        title: 'Signal hooks',
        path: '/special-hooks/signals',
        keywords: ['hooks', 'signals', 'UseSignal'],
        element: () => <SignalsHooksPage />,
      },
      {
        id: 'special-hooks-safe-area',
        title: 'Safe area hooks',
        path: '/special-hooks/safe-area',
        keywords: ['hooks', 'safe area', 'UseSafeArea', 'VisualElementSafe'],
        element: () => <SafeAreaHooksPage />,
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
  {
    id: 'known-issues',
    title: 'Known Issues',
    pages: [
      {
        id: 'known-issues-page',
        title: 'Known Issues',
        path: '/known-issues',
        keywords: ['issues', 'limitations', 'known issues'],
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
        title: 'Roadmap',
        path: '/roadmap',
        keywords: ['roadmap', 'future', 'plans'],
        element: () => <RoadmapPage />,
      },
    ],
  },
]

export const flat: Page[] = pages.flatMap((s) => {
  if (s.id === 'components') {
    const common = s.pages.filter((p) => p.group === 'basic')
    const uncommon = s.pages.filter((p) => p.group === 'advanced' || !p.group)
    return [...common, ...uncommon]
  }
  return s.pages
})
