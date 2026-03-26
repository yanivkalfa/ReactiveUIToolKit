export interface UnityDocLinkInfo {
  /** Unity element name in the docs URL (e.g. 'BoundsField', 'InspectorElement'). */
  unityElement: string
  label?: string
  note?: string
}

/** Build a Unity manual URL for a given element and docs version. */
export const buildUnityDocUrl = (unityElement: string, version: string): string =>
  `https://docs.unity3d.com/${version}/Documentation/Manual/UIE-uxml-element-${unityElement}.html`

export const UNITY_DOC_LINKS: Record<string, UnityDocLinkInfo> = {
  BoundsField: {
    unityElement: 'BoundsField',
  },
  BoundsIntField: { unityElement: 'BoundsIntField' },
  Box: { unityElement: 'Box' },
  Button: { unityElement: 'Button' },
  ColorField: { unityElement: 'ColorField' },
  DoubleField: { unityElement: 'DoubleField' },
  DropdownField: { unityElement: 'DropdownField' },
  EnumField: { unityElement: 'EnumField' },
  EnumFlagsField: { unityElement: 'EnumFlagsField' },
  FloatField: { unityElement: 'FloatField' },
  Foldout: { unityElement: 'Foldout' },
  GroupBox: { unityElement: 'GroupBox' },
  Hash128Field: { unityElement: 'Hash128Field' },
  HelpBox: { unityElement: 'HelpBox' },
  IMGUIContainer: { unityElement: 'IMGUIContainer' },
  Image: { unityElement: 'Image' },
  IntegerField: { unityElement: 'IntegerField' },
  Label: { unityElement: 'Label' },
  ListView: { unityElement: 'ListView' },
  LongField: { unityElement: 'LongField' },
  MinMaxSlider: { unityElement: 'MinMaxSlider' },
  MultiColumnListView: { unityElement: 'MultiColumnListView' },
  MultiColumnTreeView: { unityElement: 'MultiColumnTreeView' },
  ObjectField: { unityElement: 'ObjectField' },
  ProgressBar: { unityElement: 'ProgressBar' },
  PropertyInspector: {
    unityElement: 'InspectorElement',
    label: 'InspectorElement entry',
    note: 'ReactiveUITK.PropertyInspector wraps Unity\u2019s InspectorElement to embed serialized-object inspectors.',
  },
  RadioButton: { unityElement: 'RadioButton' },
  RadioButtonGroup: { unityElement: 'RadioButtonGroup' },
  RectField: { unityElement: 'RectField' },
  RectIntField: { unityElement: 'RectIntField' },
  RepeatButton: { unityElement: 'RepeatButton' },
  ScrollView: { unityElement: 'ScrollView' },
  Scroller: { unityElement: 'Scroller' },
  Slider: { unityElement: 'Slider' },
  SliderInt: { unityElement: 'SliderInt' },
  Tab: { unityElement: 'Tab' },
  TabView: { unityElement: 'TabView' },
  TemplateContainer: { unityElement: 'TemplateContainer' },
  TextElement: { unityElement: 'TextElement' },
  TextField: { unityElement: 'TextField' },
  Toggle: { unityElement: 'Toggle' },
  ToggleButtonGroup: { unityElement: 'ToggleButtonGroup' },
  Toolbar: { unityElement: 'Toolbar' },
  TreeView: { unityElement: 'TreeView' },
  TwoPaneSplitView: { unityElement: 'TwoPaneSplitView' },
  UnsignedIntegerField: { unityElement: 'UnsignedIntegerField' },
  UnsignedLongField: { unityElement: 'UnsignedLongField' },
  Vector2Field: { unityElement: 'Vector2Field' },
  Vector2IntField: { unityElement: 'Vector2IntField' },
  Vector3Field: { unityElement: 'Vector3Field' },
  Vector3IntField: { unityElement: 'Vector3IntField' },
  Vector4Field: { unityElement: 'Vector4Field' },
  VisualElement: { unityElement: 'VisualElement' },
}

export type UnityComponentName = keyof typeof UNITY_DOC_LINKS
