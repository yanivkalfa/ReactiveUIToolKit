import type { FC } from 'react'
import { useState as useReactState } from 'react'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  List,
  ListItem,
  ListItemText,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsTable, type PropEntry } from '../../../propsDocs'
import { UNITY_DOC_LINKS, buildUnityDocUrl } from '../../../components/UnityDocsSection/unityDocLinks'
import { useSelectedVersion } from '../../../contexts/VersionContext'
import Styles from '../../Components/Button/ButtonPage.style'

export type UitkxComponentReferencePageProps = {
  title: string
}

const getPropsDocName = (title: string) => {
  if (title === 'VisualElementSafe') {
    return ''
  }

  return `${title}Props`
}

const getIntro = (title: string) => {
  if (title.endsWith('Field')) {
    return `Use <${title}> in UITKX for a controlled UI Toolkit ${title} input. Keep the current value in useState(...) or a signal and feed user edits back through onChange.`
  }

  switch (title) {
    case 'Button':
    case 'RepeatButton':
      return `Use <${title}> in UITKX for clickable actions. Pass text, event handlers, and styling directly on the tag.`
    case 'Toggle':
    case 'RadioButton':
      return `Use <${title}> in UITKX for boolean-style selection controls. The usual pattern is value + onChange, backed by local state or a signal.`
    case 'DropdownField':
    case 'EnumField':
    case 'EnumFlagsField':
    case 'RadioButtonGroup':
    case 'ToggleButtonGroup':
      return `Use <${title}> in UITKX when the user is choosing from a predefined set of options.`
    case 'Slider':
    case 'SliderInt':
    case 'Scroller':
    case 'MinMaxSlider':
      return `Use <${title}> in UITKX for range-style numeric input.`
    case 'Label':
    case 'TextElement':
      return `Use <${title}> in UITKX to render text content directly in markup.`
    case 'VisualElement':
    case 'VisualElementSafe':
    case 'Box':
    case 'GroupBox':
    case 'ScrollView':
    case 'TemplateContainer':
    case 'TwoPaneSplitView':
      return `Use <${title}> in UITKX as a structural layout primitive. It composes naturally with child tags and style props.`
    case 'HelpBox':
    case 'Image':
    case 'ProgressBar':
      return `Use <${title}> in UITKX as a presentational control inside your returned markup tree.`
    case 'ListView':
    case 'TreeView':
    case 'MultiColumnListView':
    case 'MultiColumnTreeView':
      return `Use <${title}> in UITKX for data-driven collection UIs. These components usually combine declarative markup with row, cell, or binding delegates configured through props.`
    case 'Tab':
    case 'TabView':
    case 'Toolbar':
      return `Use <${title}> in UITKX for higher-level navigation and editor-style composition.`
    case 'Animate':
    case 'ErrorBoundary':
    case 'IMGUIContainer':
    case 'PropertyInspector':
      return `Use <${title}> in UITKX when you need this higher-level ReactiveUITK runtime feature directly in markup.`
    default:
      return `Use <${title}> directly in UITKX markup. The runtime still exposes the underlying props type, but the normal authoring surface is the tag itself.`
  }
}

const getNotes = (title: string): string[] => {
  switch (title) {
    case 'VisualElementSafe':
      return [
        'VisualElementSafe is the safe-area-aware variant of VisualElement.',
        'Use it when your layout should automatically respect device insets.',
      ]
    case 'IMGUIContainer':
      return [
        'IMGUIContainer remains callback-driven even in UITKX.',
        'It is mainly useful for editor tooling or IMGUI interop.',
      ]
    case 'PropertyInspector':
      return [
        'PropertyInspector is especially useful in editor tooling and inspector-like UIs.',
        'It is backed by runtime props, so typed bindings work the same way as other components.',
      ]
    case 'ListView':
    case 'TreeView':
    case 'MultiColumnListView':
    case 'MultiColumnTreeView':
      return [
        'Collection components often rely on renderer delegates in addition to plain tag props.',
        'The props section below matters more than usual for these components.',
      ]
    default:
      return [
        'The props section below shows the underlying runtime API that UITKX lowers into.',
      ]
  }
}

const getExample = (title: string) => {
  switch (title) {
    case 'Button':
      return `component ButtonExample {
  var (count, setCount) = useState(0);

  return (
    <Button
      text={$"Click me ({count})"}
      onClick={_ => setCount(previous => previous + 1)}
    />
  );
}`
    case 'RepeatButton':
      return `component RepeatButtonExample {
  var (count, setCount) = useState(0);

  return (
    <RepeatButton
      text={$"Hold to increment ({count})"}
      onClick={_ => setCount(previous => previous + 1)}
    />
  );
}`
    case 'Toggle':
      return `component ToggleExample {
  var (value, setValue) = useState(true);

  return (
    <Toggle
      text="Enabled"
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'RadioButton':
      return `component RadioButtonExample {
  var (value, setValue) = useState(false);

  return (
    <RadioButton
      text="Option"
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'DropdownField':
      return `component DropdownFieldExample {
  var choices = new[] { "Red", "Green", "Blue" };
  var (selectedIndex, setSelectedIndex) = useState(0);

  return (
    <DropdownField
      choices={choices}
      selectedIndex={selectedIndex}
      onChange={evt => setSelectedIndex(Array.IndexOf(choices, evt.newValue))}
    />
  );
}`
    case 'TextField':
      return `component TextFieldExample {
  var (value, setValue) = useState("Hello");

  return (
    <TextField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'IntegerField':
      return `component IntegerFieldExample {
  var (value, setValue) = useState(42);

  return (
    <IntegerField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'FloatField':
      return `component FloatFieldExample {
  var (value, setValue) = useState(1.23f);

  return (
    <FloatField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'DoubleField':
      return `component DoubleFieldExample {
  var (value, setValue) = useState(3.14159);

  return (
    <DoubleField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'LongField':
      return `component LongFieldExample {
  var (value, setValue) = useState(123456789L);

  return (
    <LongField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'UnsignedIntegerField':
      return `component UnsignedIntegerFieldExample {
  var (value, setValue) = useState<uint>(0u);

  return (
    <UnsignedIntegerField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'UnsignedLongField':
      return `component UnsignedLongFieldExample {
  var (value, setValue) = useState<ulong>(0ul);

  return (
    <UnsignedLongField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Slider':
      return `component SliderExample {
  var (value, setValue) = useState(0.5f);

  return (
    <Slider
      lowValue={0f}
      highValue={1f}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'SliderInt':
      return `component SliderIntExample {
  var (value, setValue) = useState(5);

  return (
    <SliderInt
      lowValue={0}
      highValue={10}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Scroller':
      return `component ScrollerExample {
  var (value, setValue) = useState(0f);

  return (
    <Scroller
      lowValue={0f}
      highValue={100f}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'MinMaxSlider':
      return `component MinMaxSliderExample {
  var (range, setRange) = useState((min: 20f, max: 80f));

  return (
    <MinMaxSlider
      minValue={0f}
      maxValue={100f}
      value={range}
      onChange={evt => setRange(evt.newValue)}
    />
  );
}`
    case 'BoundsField':
      return `component BoundsFieldExample {
  var (value, setValue) = useState(new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f)));

  return (
    <BoundsField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'BoundsIntField':
      return `component BoundsIntFieldExample {
  var (value, setValue) = useState(new BoundsInt(1, 2, 3, 4, 5, 6));

  return (
    <BoundsIntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'ColorField':
      return `component ColorFieldExample {
  var (value, setValue) = useState(new Color(0.2f, 0.6f, 0.9f, 1f));

  return (
    <ColorField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'EnumField':
      return `component EnumFieldExample {
  var (value, setValue) = useState(ExampleEnum.B);

  return (
    <EnumField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'EnumFlagsField':
      return `component EnumFlagsFieldExample {
  var (value, setValue) = useState(ExampleFlags.A | ExampleFlags.C);

  return (
    <EnumFlagsField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Hash128Field':
      return `component Hash128FieldExample {
  var (value, setValue) = useState(new Hash128(1, 2, 3, 4));

  return (
    <Hash128Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'ObjectField':
      return `component ObjectFieldExample {
  var (value, setValue) = useState<Object>(null);

  return (
    <ObjectField
      objectType={typeof(Texture2D)}
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'RectField':
      return `component RectFieldExample {
  var (value, setValue) = useState(new Rect(0f, 0f, 128f, 64f));

  return (
    <RectField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'RectIntField':
      return `component RectIntFieldExample {
  var (value, setValue) = useState(new RectInt(0, 0, 16, 16));

  return (
    <RectIntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Vector2Field':
      return `component Vector2FieldExample {
  var (value, setValue) = useState(new Vector2(1f, 2f));

  return (
    <Vector2Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Vector2IntField':
      return `component Vector2IntFieldExample {
  var (value, setValue) = useState(new Vector2Int(1, 2));

  return (
    <Vector2IntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Vector3Field':
      return `component Vector3FieldExample {
  var (value, setValue) = useState(new Vector3(1f, 2f, 3f));

  return (
    <Vector3Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Vector3IntField':
      return `component Vector3IntFieldExample {
  var (value, setValue) = useState(new Vector3Int(1, 2, 3));

  return (
    <Vector3IntField
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Vector4Field':
      return `component Vector4FieldExample {
  var (value, setValue) = useState(new Vector4(1f, 2f, 3f, 4f));

  return (
    <Vector4Field
      value={value}
      onChange={evt => setValue(evt.newValue)}
    />
  );
}`
    case 'Label':
      return `component LabelExample {
  return (
    <Label text="Hello from UITKX" />
  );
}`
    case 'TextElement':
      return `component TextElementExample {
  return (
    <TextElement text="TextElement authored directly in UITKX" />
  );
}`
    case 'HelpBox':
      return `component HelpBoxExample {
  return (
    <HelpBox
      text="Remember to save before entering play mode."
      messageType="warning"
    />
  );
}`
    case 'ProgressBar':
      return `component ProgressBarExample {
  return (
    <ProgressBar
      value={65f}
      title="Build progress"
    />
  );
}`
    case 'Image':
      return `component ImageExample(Texture2D texture) {
  return (
    <Image texture={texture} />
  );
}`
    case 'VisualElement':
      return `component VisualElementExample {
  return (
    <VisualElement>
      <Text text="Child content inside a VisualElement" />
    </VisualElement>
  );
}`
    case 'VisualElementSafe':
      return `component VisualElementSafeExample {
  return (
    <VisualElementSafe>
      <Text text="Safe-area aware content" />
    </VisualElementSafe>
  );
}`
    case 'Box':
      return `component BoxExample {
  return (
    <Box>
      <Text text="Inside Box" />
    </Box>
  );
}`
    case 'GroupBox':
      return `component GroupBoxExample {
  return (
    <GroupBox text="Example group">
      <Text text="Content item 1" />
      <Text text="Content item 2" />
    </GroupBox>
  );
}`
    case 'ScrollView':
      return `component ScrollViewExample {
  return (
    <ScrollView mode="vertical">
      <Text text="Row 1" />
      <Text text="Row 2" />
      <Text text="Row 3" />
    </ScrollView>
  );
}`
    case 'TwoPaneSplitView':
      return `component TwoPaneSplitViewExample {
  return (
    <TwoPaneSplitView
      orientation="horizontal"
      fixedPaneIndex={0}
      fixedPaneInitialDimension={220f}
    >
      <VisualElement>
        <Text text="Pane 1" />
      </VisualElement>
      <VisualElement>
        <Text text="Pane 2" />
      </VisualElement>
    </TwoPaneSplitView>
  );
}`
    case 'Toolbar':
      return `component ToolbarExample {
  return (
    <Toolbar>
      <ToolbarButton text="Ping" onClick={_ => UnityEngine.Debug.Log("Ping")} />
      <ToolbarToggle text="Toggle" value={false} />
      <ToolbarSearchField />
    </Toolbar>
  );
}`
    case 'Foldout':
      return `component FoldoutExample {
  var (open, setOpen) = useState(true);

  return (
    <Foldout
      text="Advanced options"
      value={open}
      onChange={evt => setOpen(evt.newValue)}
    >
      <Text text="Nested content" />
    </Foldout>
  );
}`
    case 'Tab':
      return `component TabExample {
  return (
    <Tab text="General" />
  );
}`
    case 'TabView':
      return `component TabViewExample {
  return (
    <TabView>
      <Tab text="General">
        <Text text="General settings" />
      </Tab>
      <Tab text="Audio">
        <Text text="Audio settings" />
      </Tab>
    </TabView>
  );
}`
    case 'ListView':
      return `component ListViewExample {
  var items = new[] { "One", "Two", "Three" };

  return (
    <ListView
      items={items}
      fixedItemHeight={20f}
      row={(index, item) => <Label text={$"{index}: {item}"} />}
    />
  );
}`
    case 'TreeView':
      return `component TreeViewExample {
  return (
    <TreeView
      items={treeItems}
      row={(index, item) => <Label text={item.ToString()} />}
    />
  );
}`
    case 'MultiColumnListView':
      return `component MultiColumnListViewExample {
  return (
    <MultiColumnListView
      items={rows}
      columns={columns}
    />
  );
}`
    case 'MultiColumnTreeView':
      return `component MultiColumnTreeViewExample {
  return (
    <MultiColumnTreeView
      items={treeItems}
      columns={columns}
    />
  );
}`
    case 'Animate':
      return `component AnimateExample {
  return (
    <Animate>
      <Box>
        <Text text="Animated box" />
      </Box>
    </Animate>
  );
}`
    case 'ErrorBoundary':
      return `component ErrorBoundaryExample {
  return (
    <ErrorBoundary fallback={<Text text="Something went wrong." />}>
      <UnstableChild />
    </ErrorBoundary>
  );
}`
    case 'IMGUIContainer':
      return `component IMGUIContainerExample {
  void DrawGui()
  {
    GUILayout.Label("Hello from IMGUI");
  }

  return (
    <IMGUIContainer onGUI={DrawGui} />
  );
}`
    case 'PropertyInspector':
      return `component PropertyInspectorExample {
  var (target, setTarget) = useState<Object>(null);

  return (
    <PropertyInspector target={target} />
  );
}`
    default:
      return `component ${title}Example {
  return (
    <${title} />
  );
}`
  }
}

const toCamelCase = (name: string) => name.charAt(0).toLowerCase() + name.slice(1)

const PropsTable: FC<{ entries: PropEntry[]; caption?: string }> = ({
  entries,
  caption,
}) => (
  <TableContainer>
    {caption && (
      <Typography variant="subtitle2" sx={{ mb: 1, opacity: 0.7 }}>
        {caption}
      </Typography>
    )}
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell sx={{ fontWeight: 700 }}>Prop</TableCell>
          <TableCell sx={{ fontWeight: 700 }}>Type</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {entries.map((e) => (
          <TableRow key={e.name}>
            <TableCell>
              <code>{toCamelCase(e.name)}</code>
            </TableCell>
            <TableCell>
              <code>{e.type}</code>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </TableContainer>
)

export const UitkxComponentReferencePage: FC<UitkxComponentReferencePageProps> = ({
  title,
}) => {
  const allProps = getPropsTable(getPropsDocName(title))
  const componentProps = allProps.filter((p) => !p.inherited)
  const inheritedProps = allProps.filter((p) => p.inherited)
  const notes = getNotes(title)
  const [baseOpen, setBaseOpen] = useReactState(false)
  const { selectedVersion } = useSelectedVersion()
  const unityInfo = UNITY_DOC_LINKS[title]

  return (
    <Box sx={Styles.root}>
      <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1.5 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          {title}
        </Typography>
        {unityInfo && (
          <Typography
            component="a"
            href={buildUnityDocUrl(unityInfo.unityElement, selectedVersion)}
            target="_blank"
            rel="noreferrer"
            variant="body2"
            sx={{ color: 'primary.main', textDecoration: 'none', whiteSpace: 'nowrap', '&:hover': { textDecoration: 'underline' } }}
          >
            Unity docs &darr;
          </Typography>
        )}
      </Box>
      <Typography variant="body1" paragraph>
        {getIntro(title)}
      </Typography>

      <Box sx={Styles.section}>
        <Typography variant="h5" component="h2" gutterBottom>
          UITKX Example
        </Typography>
        <CodeBlock language="tsx" code={getExample(title)} />
      </Box>

      <Box sx={Styles.section}>
        <Typography variant="h5" component="h2" gutterBottom>
          Notes
        </Typography>
        <List>
          {notes.map((note) => (
            <ListItem key={note} disablePadding>
              <ListItemText primary={note} />
            </ListItem>
          ))}
        </List>
      </Box>

      {allProps.length > 0 && (
        <Box sx={Styles.section}>
          <Typography variant="h5" component="h2" gutterBottom>
            Props
          </Typography>
          <Typography variant="body2" paragraph sx={{ opacity: 0.7 }}>
            Attribute names in UITKX use camelCase (e.g. <code>lowValue</code>,{' '}
            <code>onChange</code>).
          </Typography>

          {componentProps.length > 0 && (
            <PropsTable entries={componentProps} />
          )}

          {inheritedProps.length > 0 && (
            <Accordion
              expanded={baseOpen}
              onChange={() => setBaseOpen(!baseOpen)}
              disableGutters
              sx={{ mt: 2, boxShadow: 'none', '&:before': { display: 'none' } }}
            >
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="subtitle2">
                  Common props (inherited from BaseProps)
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <PropsTable entries={inheritedProps} />
              </AccordionDetails>
            </Accordion>
          )}
        </Box>
      )}
    </Box>
  )
}
