import type { FC } from 'react'
import {
  Alert,
  Box,
  Chip,
  List,
  ListItem,
  ListItemText,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../Components/Button/ButtonPage.style'

const COMPONENT_SAMPLE = `component ButtonShowcase {
  var (enabled, setEnabled) = useState(true);

  return (
    <VisualElement>
      <Text text={$"Enabled: {enabled}"} />
      <Button
        text={enabled ? "Disable" : "Enable"}
        enabled={true}
        onClick={_ => setEnabled(previous => !previous)}
      />
      <Button
        text="Secondary action"
        enabled={enabled}
        onClick={_ => UnityEngine.Debug.Log("Clicked")}
      />
    </VisualElement>
  );
}`

/* ------------------------------------------------------------------ */
/*  Component catalog                                                  */
/* ------------------------------------------------------------------ */

type CompEntry = { name: string; desc: string; editor?: boolean }

const containers: CompEntry[] = [
  { name: 'VisualElement', desc: 'Universal container — the div of UI Toolkit' },
  { name: 'VisualElementSafe', desc: 'Container with safe-area insets for notched devices' },
  { name: 'Box', desc: 'Styled container (semantic wrapper)' },
  { name: 'ScrollView', desc: 'Scrollable container' },
  { name: 'Foldout', desc: 'Collapsible container with header' },
  { name: 'GroupBox', desc: 'Grouping container with label' },
  { name: 'TabView', desc: 'Tabbed container that switches between Tab children' },
  { name: 'Tab', desc: 'Single tab panel inside a TabView' },
  { name: 'TwoPaneSplitView', desc: 'Resizable two-pane split layout', editor: true },
  { name: 'TemplateContainer', desc: 'Host for UXML templates' },
  { name: 'IMGUIContainer', desc: 'Embed legacy IMGUI rendering', editor: true },
]

const display: CompEntry[] = [
  { name: 'Label', desc: 'Single-line text' },
  { name: 'TextElement', desc: 'Low-level text element' },
  { name: 'Image', desc: 'Displays a Texture, Sprite, or VectorImage' },
  { name: 'ProgressBar', desc: 'Determinate progress indicator' },
  { name: 'HelpBox', desc: 'Info / warning / error message box', editor: true },
]

const buttons: CompEntry[] = [
  { name: 'Button', desc: 'Standard clickable button' },
  { name: 'RepeatButton', desc: 'Button that fires repeatedly while held' },
  { name: 'Toggle', desc: 'Checkbox / boolean toggle' },
  { name: 'RadioButton', desc: 'Single radio choice' },
  { name: 'RadioButtonGroup', desc: 'Group of mutually exclusive radio buttons' },
  { name: 'ToggleButtonGroup', desc: 'Group of toggle buttons (single or multi-select)' },
]

const textInputs: CompEntry[] = [
  { name: 'TextField', desc: 'Single- or multi-line text input' },
  { name: 'IntegerField', desc: 'Integer input with validation' },
  { name: 'FloatField', desc: 'Float input with validation' },
  { name: 'DoubleField', desc: 'Double input with validation' },
  { name: 'LongField', desc: 'Long input with validation' },
  { name: 'UnsignedIntegerField', desc: 'Unsigned integer input' },
  { name: 'UnsignedLongField', desc: 'Unsigned long input' },
  { name: 'Hash128Field', desc: 'Hash128 input' },
]

const vectorFields: CompEntry[] = [
  { name: 'Vector2Field', desc: 'Two-component vector input' },
  { name: 'Vector2IntField', desc: 'Two-component integer vector input' },
  { name: 'Vector3Field', desc: 'Three-component vector input' },
  { name: 'Vector3IntField', desc: 'Three-component integer vector input' },
  { name: 'Vector4Field', desc: 'Four-component vector input' },
  { name: 'RectField', desc: 'Rectangle (x, y, width, height) input' },
  { name: 'RectIntField', desc: 'Integer rectangle input' },
  { name: 'BoundsField', desc: 'Bounds (center + size) input' },
  { name: 'BoundsIntField', desc: 'Integer bounds input' },
]

const pickers: CompEntry[] = [
  { name: 'Slider', desc: 'Float range slider' },
  { name: 'SliderInt', desc: 'Integer range slider' },
  { name: 'MinMaxSlider', desc: 'Two-thumb range slider' },
  { name: 'DropdownField', desc: 'Dropdown / popup selector' },
  { name: 'EnumField', desc: 'Enum value picker' },
  { name: 'EnumFlagsField', desc: 'Flags enum multi-picker' },
  { name: 'ColorField', desc: 'Color picker', editor: true },
  { name: 'ObjectField', desc: 'Unity Object reference picker', editor: true },
]

const dataViews: CompEntry[] = [
  { name: 'ListView', desc: 'Virtualised scrolling list' },
  { name: 'TreeView', desc: 'Hierarchical tree list' },
  { name: 'MultiColumnListView', desc: 'Multi-column sortable list' },
  { name: 'MultiColumnTreeView', desc: 'Multi-column hierarchical tree' },
  { name: 'Scroller', desc: 'Standalone scrollbar (used by ScrollView internally)' },
]

const editorToolbar: CompEntry[] = [
  { name: 'Toolbar', desc: 'Editor toolbar container', editor: true },
  { name: 'PropertyField & InspectorElement', desc: 'SerializedProperty binding', editor: true },
]

const framework: CompEntry[] = [
  { name: 'Animate', desc: 'Declarative animation wrapper (AnimateTrack)' },
  { name: 'ErrorBoundary', desc: 'Catches rendering exceptions in subtree' },
  { name: 'Portal', desc: 'Renders children into external VisualElement target' },
  { name: 'Suspense', desc: 'Shows fallback while async content loads' },
  { name: 'Fragment (<>…</>)', desc: 'Invisible grouping wrapper' },
]

const allCategories = [
  { label: 'Containers & Layout', rows: containers },
  { label: 'Display', rows: display },
  { label: 'Buttons & Toggles', rows: buttons },
  { label: 'Text Input', rows: textInputs },
  { label: 'Vector & Rect Fields', rows: vectorFields },
  { label: 'Pickers & Selectors', rows: pickers },
  { label: 'Data Views', rows: dataViews },
  { label: 'Editor Toolbar', rows: editorToolbar },
  { label: 'Framework Components', rows: framework },
]

/* ------------------------------------------------------------------ */
/*  BaseProps common properties                                        */
/* ------------------------------------------------------------------ */

type PropRow = { name: string; type: string; desc: string }

const baseProps: PropRow[] = [
  { name: 'name', type: 'string', desc: 'VisualElement name (for USS #name selectors)' },
  { name: 'className', type: 'string', desc: 'Space-separated USS class names (.class selectors)' },
  { name: 'style', type: 'Style', desc: 'Inline typed style object' },
  { name: 'ref', type: 'VisualElement', desc: 'Element ref (from useRef())' },
  { name: 'contentContainer', type: 'VisualElement', desc: 'Override content container' },
  { name: 'visible', type: 'bool', desc: 'Show / hide the element' },
  { name: 'enabled', type: 'bool', desc: 'Enable / disable interaction' },
  { name: 'pickingMode', type: 'PickingMode', desc: 'Picking mode (PickPosition, PickIgnore)' },
  { name: 'focusable', type: 'bool', desc: 'Whether the element can receive focus' },
  { name: 'tabIndex', type: 'int', desc: 'Tab order index' },
  { name: 'delegatesFocus', type: 'bool', desc: 'Delegate focus to child' },
  { name: 'tooltip', type: 'string', desc: 'Tooltip text on hover' },
  { name: 'viewDataKey', type: 'string', desc: 'Persistence key for view data' },
  { name: 'languageDirection', type: 'LanguageDirection', desc: 'LTR / RTL / Inherit' },
  { name: 'extraProps', type: 'Dictionary', desc: 'Escape hatch for non-standard properties' },
]

/* ------------------------------------------------------------------ */
/*  Page                                                               */
/* ------------------------------------------------------------------ */

export const UitkxComponentsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Components Overview
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit wraps every Unity UI Toolkit element as a declarative
      component you can use in <code>.uitkx</code> markup. Use intrinsic tag
      names for built-in elements and PascalCase names for your own custom
      components.
    </Typography>

    <CodeBlock language="jsx" code={COMPONENT_SAMPLE} />

    {/* ── Categorized component catalog ────────────────────────── */}
    {allCategories.map((cat) => (
      <Box key={cat.label} sx={{ mt: 2 }}>
        <Typography variant="h5" component="h2" gutterBottom>
          {cat.label}
        </Typography>
        <TableContainer component={Paper} variant="outlined">
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell><strong>Component</strong></TableCell>
                <TableCell><strong>Description</strong></TableCell>
                <TableCell><strong>Availability</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {cat.rows.map((c) => (
                <TableRow key={c.name}>
                  <TableCell><code>{`<${c.name.replace(/ .*/,'')}>`}</code></TableCell>
                  <TableCell>{c.desc}</TableCell>
                  <TableCell>
                    {c.editor
                      ? <Chip label="Editor-only" size="small" color="warning" variant="outlined" />
                      : <Chip label="Runtime + Editor" size="small" color="success" variant="outlined" />
                    }
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    ))}

    {/* ── BaseProps common properties ──────────────────────────── */}
    <Box sx={{ mt: 3 }}>
      <Typography variant="h5" component="h2" gutterBottom>
        Common props (BaseProps)
      </Typography>
      <Typography variant="body1" paragraph>
        Every component inherits these properties from <code>BaseProps</code>.
        They are available on all elements.
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Prop</strong></TableCell>
              <TableCell><strong>Type</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {baseProps.map((p) => (
              <TableRow key={p.name}>
                <TableCell><code>{p.name}</code></TableCell>
                <TableCell><code>{p.type}</code></TableCell>
                <TableCell>{p.desc}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      <Alert severity="info" sx={{ mt: 1 }}>
        In addition to these properties, every element also inherits 23+ event
        handlers (onClick, onPointerDown, onKeyDown, onFocus, etc.). See the{' '}
        <strong>Events &amp; Input Handling</strong> page for the complete list.
      </Alert>
    </Box>

    {/* ── Authoring guidelines ─────────────────────────────────── */}
    <Box sx={{ mt: 3 }}>
      <Typography variant="h5" component="h2" gutterBottom>
        Authoring guidelines
      </Typography>
      <List>
        <ListItem disablePadding>
          <ListItemText primary="Prefer direct tag props over hand-building props objects when authoring UITKX." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Keep setup code small and close to the returned markup tree." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Custom component names must be PascalCase and must not collide with built-in tag names." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Each .uitkx file contains exactly one component. The filename must match the component name." />
        </ListItem>
      </List>
    </Box>
  </Box>
)
