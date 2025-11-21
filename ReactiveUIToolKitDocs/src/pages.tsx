import type { ReactElement } from 'react'
import { IntroductionPage } from './pages/Introduction/IntroductionPage'
import { GettingStartedPage } from './pages/GettingStarted/GettingStartedPage'
import { RouterPage } from './pages/Router/RouterPage'
import { SignalsPage } from './pages/Signals/SignalsPage'
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
      {
        id: 'component-foldout',
        title: 'Foldout',
        path: '/components/foldout',
        keywords: ['foldout', 'toggle', 'collapsible'],
        element: () => <FoldoutPage />,
      },
      {
        id: 'component-group-box',
        title: 'GroupBox',
        path: '/components/group-box',
        keywords: ['group', 'groupbox'],
        element: () => <GroupBoxPage />,
      },
      {
        id: 'component-hash128-field',
        title: 'Hash128Field',
        path: '/components/hash128-field',
        keywords: ['hash128', 'field'],
        element: () => <Hash128FieldPage />,
      },
      {
        id: 'component-help-box',
        title: 'HelpBox',
        path: '/components/help-box',
        keywords: ['helpbox', 'message'],
        element: () => <HelpBoxPage />,
      },
      {
        id: 'component-imgui-container',
        title: 'IMGUIContainer',
        path: '/components/imgui-container',
        keywords: ['imgui', 'editor'],
        element: () => <IMGUIContainerPage />,
      },
      {
        id: 'component-image',
        title: 'Image',
        path: '/components/image',
        keywords: ['image', 'texture', 'sprite'],
        element: () => <ImagePage />,
      },
      {
        id: 'component-integer-field',
        title: 'IntegerField',
        path: '/components/integer-field',
        keywords: ['integer', 'field', 'int'],
        element: () => <IntegerFieldPage />,
      },
      {
        id: 'component-label',
        title: 'Label',
        path: '/components/label',
        keywords: ['label', 'text'],
        element: () => <LabelPage />,
      },
      {
        id: 'component-long-field',
        title: 'LongField',
        path: '/components/long-field',
        keywords: ['long', 'field', 'LongField'],
        element: () => <LongFieldPage />,
      },
      {
        id: 'component-progress-bar',
        title: 'ProgressBar',
        path: '/components/progress-bar',
        keywords: ['progress', 'bar'],
        element: () => <ProgressBarPage />,
      },
      {
        id: 'component-list-view',
        title: 'ListView',
        path: '/components/list-view',
        keywords: ['list', 'ListView'],
        element: () => <ListViewPage />,
      },
      {
        id: 'component-minmax-slider',
        title: 'MinMaxSlider',
        path: '/components/minmax-slider',
        keywords: ['minmax', 'slider'],
        element: () => <MinMaxSliderPage />,
      },
      {
        id: 'component-object-field',
        title: 'ObjectField',
        path: '/components/object-field',
        keywords: ['object', 'field'],
        element: () => <ObjectFieldPage />,
      },
      {
        id: 'component-radio-button',
        title: 'RadioButton',
        path: '/components/radio-button',
        keywords: ['radio', 'button'],
        element: () => <RadioButtonPage />,
      },
      {
        id: 'component-radio-button-group',
        title: 'RadioButtonGroup',
        path: '/components/radio-button-group',
        keywords: ['radio', 'group'],
        element: () => <RadioButtonGroupPage />,
      },
      {
        id: 'component-rect-field',
        title: 'RectField',
        path: '/components/rect-field',
        keywords: ['rect', 'field'],
        element: () => <RectFieldPage />,
      },
      {
        id: 'component-rect-int-field',
        title: 'RectIntField',
        path: '/components/rect-int-field',
        keywords: ['rectint', 'field'],
        element: () => <RectIntFieldPage />,
      },
      {
        id: 'component-repeat-button',
        title: 'RepeatButton',
        path: '/components/repeat-button',
        keywords: ['repeat', 'button'],
        element: () => <RepeatButtonPage />,
      },
      {
        id: 'component-scroll-view',
        title: 'ScrollView',
        path: '/components/scroll-view',
        keywords: ['scroll', 'view'],
        element: () => <ScrollViewPage />,
      },
      {
        id: 'component-slider',
        title: 'Slider',
        path: '/components/slider',
        keywords: ['slider', 'float'],
        element: () => <SliderPage />,
      },
      {
        id: 'component-slider-int',
        title: 'SliderInt',
        path: '/components/slider-int',
        keywords: ['slider', 'int'],
        element: () => <SliderIntPage />,
      },
      {
        id: 'component-toggle',
        title: 'Toggle',
        path: '/components/toggle',
        keywords: ['toggle', 'checkbox'],
        element: () => <TogglePage />,
      },
      {
        id: 'component-tree-view',
        title: 'TreeView',
        path: '/components/tree-view',
        keywords: ['tree', 'TreeView'],
        element: () => <TreeViewPage />,
      },
      {
        id: 'component-tab',
        title: 'Tab',
        path: '/components/tab',
        keywords: ['tab'],
        element: () => <TabPage />,
      },
      {
        id: 'component-tab-view',
        title: 'TabView',
        path: '/components/tab-view',
        keywords: ['tab', 'TabView'],
        element: () => <TabViewPage />,
      },
      {
        id: 'component-toggle-button-group',
        title: 'ToggleButtonGroup',
        path: '/components/toggle-button-group',
        keywords: ['toggle', 'buttons', 'group'],
        element: () => <ToggleButtonGroupPage />,
      },
      {
        id: 'component-text-field',
        title: 'TextField',
        path: '/components/text-field',
        keywords: ['text', 'field'],
        element: () => <TextFieldPage />,
      },
      {
        id: 'component-toolbar',
        title: 'Toolbar',
        path: '/components/toolbar',
        keywords: ['toolbar', 'editor'],
        element: () => <ToolbarPage />,
      },
      {
        id: 'component-template-container',
        title: 'TemplateContainer',
        path: '/components/template-container',
        keywords: ['template', 'container'],
        element: () => <TemplateContainerPage />,
      },
      {
        id: 'component-visual-element',
        title: 'VisualElement',
        path: '/components/visual-element',
        keywords: ['visualelement', 'container', 'safe'],
        element: () => <VisualElementPage />,
      },
      {
        id: 'component-visual-element-safe',
        title: 'VisualElementSafe',
        path: '/components/visual-element-safe',
        keywords: ['visualelementsafe', 'safe-area', 'container'],
        element: () => <VisualElementPage />,
      },
      {
        id: 'component-unsigned-integer-field',
        title: 'UnsignedIntegerField',
        path: '/components/unsigned-integer-field',
        keywords: ['uint', 'field'],
        element: () => <UnsignedIntegerFieldPage />,
      },
      {
        id: 'component-unsigned-long-field',
        title: 'UnsignedLongField',
        path: '/components/unsigned-long-field',
        keywords: ['ulong', 'field'],
        element: () => <UnsignedLongFieldPage />,
      },
      {
        id: 'component-vector2-field',
        title: 'Vector2Field',
        path: '/components/vector2-field',
        keywords: ['vector2', 'field'],
        element: () => <Vector2FieldPage />,
      },
      {
        id: 'component-vector2-int-field',
        title: 'Vector2IntField',
        path: '/components/vector2-int-field',
        keywords: ['vector2int', 'field'],
        element: () => <Vector2IntFieldPage />,
      },
      {
        id: 'component-vector3-field',
        title: 'Vector3Field',
        path: '/components/vector3-field',
        keywords: ['vector3', 'field'],
        element: () => <Vector3FieldPage />,
      },
      {
        id: 'component-vector3-int-field',
        title: 'Vector3IntField',
        path: '/components/vector3-int-field',
        keywords: ['vector3int', 'field'],
        element: () => <Vector3IntFieldPage />,
      },
      {
        id: 'component-vector4-field',
        title: 'Vector4Field',
        path: '/components/vector4-field',
        keywords: ['vector4', 'field'],
        element: () => <Vector4FieldPage />,
      },
      {
        id: 'component-animate',
        title: 'Animate',
        path: '/components/animate',
        keywords: ['animate', 'animation'],
        element: () => <AnimatePage />,
      },
      {
        id: 'component-error-boundary',
        title: 'ErrorBoundary',
        path: '/components/error-boundary',
        keywords: ['error', 'boundary'],
        element: () => <ErrorBoundaryPage />,
      },
      {
        id: 'component-multi-column-list-view',
        title: 'MultiColumnListView',
        path: '/components/multi-column-list-view',
        keywords: ['list', 'multi', 'columns'],
        element: () => <MultiColumnListViewPage />,
      },
      {
        id: 'component-multi-column-tree-view',
        title: 'MultiColumnTreeView',
        path: '/components/multi-column-tree-view',
        keywords: ['tree', 'multi', 'columns'],
        element: () => <MultiColumnTreeViewPage />,
      },
      {
        id: 'component-scroller',
        title: 'Scroller',
        path: '/components/scroller',
        keywords: ['scroller'],
        element: () => <ScrollerPage />,
      },
      {
        id: 'component-text-element',
        title: 'TextElement',
        path: '/components/text-element',
        keywords: ['text', 'TextElement'],
        element: () => <TextElementPage />,
      },
      {
        id: 'component-property-inspector',
        title: 'PropertyField & InspectorElement',
        path: '/components/property-inspector',
        keywords: ['propertyfield', 'inspectorelement', 'editor'],
        element: () => <PropertyInspectorPage />,
      },
      {
        id: 'component-two-pane-split-view',
        title: 'TwoPaneSplitView',
        path: '/components/two-pane-split-view',
        keywords: ['split', 'editor'],
        element: () => <TwoPaneSplitViewPage />,
      },
      {
        id: 'component-template-container',
        title: 'TemplateContainer',
        path: '/components/template-container',
        keywords: ['template', 'container'],
        element: () => <TemplateContainerPage />,
      },
      {
        id: 'component-unsigned-integer-field',
        title: 'UnsignedIntegerField',
        path: '/components/unsigned-integer-field',
        keywords: ['uint', 'field'],
        element: () => <UnsignedIntegerFieldPage />,
      },
      {
        id: 'component-unsigned-long-field',
        title: 'UnsignedLongField',
        path: '/components/unsigned-long-field',
        keywords: ['ulong', 'field'],
        element: () => <UnsignedLongFieldPage />,
      },
      {
        id: 'component-vector2-field',
        title: 'Vector2Field',
        path: '/components/vector2-field',
        keywords: ['vector2', 'field'],
        element: () => <Vector2FieldPage />,
      },
      {
        id: 'component-vector2-int-field',
        title: 'Vector2IntField',
        path: '/components/vector2-int-field',
        keywords: ['vector2int', 'field'],
        element: () => <Vector2IntFieldPage />,
      },
      {
        id: 'component-vector3-field',
        title: 'Vector3Field',
        path: '/components/vector3-field',
        keywords: ['vector3', 'field'],
        element: () => <Vector3FieldPage />,
      },
      {
        id: 'component-vector3-int-field',
        title: 'Vector3IntField',
        path: '/components/vector3-int-field',
        keywords: ['vector3int', 'field'],
        element: () => <Vector3IntFieldPage />,
      },
      {
        id: 'component-vector4-field',
        title: 'Vector4Field',
        path: '/components/vector4-field',
        keywords: ['vector4', 'field'],
        element: () => <Vector4FieldPage />,
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

export const flat: Page[] = pages.flatMap((s) => s.pages)
