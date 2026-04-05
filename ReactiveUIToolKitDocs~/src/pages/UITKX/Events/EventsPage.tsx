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
import {
  EVENTS_CHANGE_EXAMPLE,
  EVENTS_CLICK_EXAMPLE,
  EVENTS_DRAG_EXAMPLE,
  EVENTS_FOCUS_EXAMPLE,
  EVENTS_GEOMETRY_EXAMPLE,
  EVENTS_KEYBOARD_EXAMPLE,
  EVENTS_POINTER_EXAMPLE,
  EVENTS_PROPAGATION_EXAMPLE,
} from './EventsPage.example'

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 2 },
  list: { pl: 2 },
} as const

/* ------------------------------------------------------------------ */
/*  Event handler table data                                          */
/* ------------------------------------------------------------------ */

type EventRow = { prop: string; delegate: string; eventArg: string; note?: string }

const pointerEvents: EventRow[] = [
  { prop: 'onClick', delegate: 'PointerEventHandler', eventArg: 'ReactivePointerEvent' },
  { prop: 'onPointerDown', delegate: 'PointerEventHandler', eventArg: 'ReactivePointerEvent' },
  { prop: 'onPointerUp', delegate: 'PointerEventHandler', eventArg: 'ReactivePointerEvent' },
  { prop: 'onPointerMove', delegate: 'PointerEventHandler', eventArg: 'ReactivePointerEvent' },
  { prop: 'onPointerEnter', delegate: 'PointerEventHandler', eventArg: 'ReactivePointerEvent' },
  { prop: 'onPointerLeave', delegate: 'PointerEventHandler', eventArg: 'ReactivePointerEvent' },
]

const scrollEvents: EventRow[] = [
  { prop: 'onWheel', delegate: 'WheelEventHandler', eventArg: 'ReactiveWheelEvent' },
  { prop: 'onScroll', delegate: 'WheelEventHandler', eventArg: 'ReactiveWheelEvent' },
]

const focusEvents: EventRow[] = [
  { prop: 'onFocus', delegate: 'FocusEventHandler', eventArg: 'ReactiveFocusEvent' },
  { prop: 'onBlur', delegate: 'FocusEventHandler', eventArg: 'ReactiveFocusEvent' },
  { prop: 'onFocusIn', delegate: 'FocusEventHandler', eventArg: 'ReactiveFocusEvent' },
  { prop: 'onFocusOut', delegate: 'FocusEventHandler', eventArg: 'ReactiveFocusEvent' },
]

const keyboardEvents: EventRow[] = [
  { prop: 'onKeyDown', delegate: 'KeyboardEventHandler', eventArg: 'ReactiveKeyboardEvent' },
  { prop: 'onKeyUp', delegate: 'KeyboardEventHandler', eventArg: 'ReactiveKeyboardEvent' },
]

const inputEvents: EventRow[] = [
  { prop: 'onChange', delegate: 'ChangeEventHandler<T>', eventArg: 'ChangeEvent<T>', note: 'T matches the control type (bool, float, string, etc.)' },
  { prop: 'onInput', delegate: 'InputEventHandler', eventArg: 'string', note: 'Receives new value directly' },
]

const lifecycleEvents: EventRow[] = [
  { prop: 'onGeometryChanged', delegate: 'GeometryChangedEventHandler', eventArg: 'ReactiveGeometryEvent' },
  { prop: 'onAttachToPanel', delegate: 'PanelLifecycleEventHandler', eventArg: 'ReactivePanelEvent' },
  { prop: 'onDetachFromPanel', delegate: 'PanelLifecycleEventHandler', eventArg: 'ReactivePanelEvent' },
]

const dragEvents: EventRow[] = [
  { prop: 'onDragEnter', delegate: 'DragEventHandler', eventArg: 'ReactiveDragEvent', note: 'Editor-only' },
  { prop: 'onDragLeave', delegate: 'DragEventHandler', eventArg: 'ReactiveDragEvent', note: 'Editor-only' },
  { prop: 'onDragUpdated', delegate: 'DragEventHandler', eventArg: 'ReactiveDragEvent', note: 'Editor-only' },
  { prop: 'onDragPerform', delegate: 'DragEventHandler', eventArg: 'ReactiveDragEvent', note: 'Editor-only' },
  { prop: 'onDragExited', delegate: 'DragEventHandler', eventArg: 'ReactiveDragEvent', note: 'Editor-only' },
]

const allEvents = [
  { label: 'Pointer', rows: pointerEvents },
  { label: 'Scroll / Wheel', rows: scrollEvents },
  { label: 'Focus', rows: focusEvents },
  { label: 'Keyboard', rows: keyboardEvents },
  { label: 'Change & Input', rows: inputEvents },
  { label: 'Lifecycle', rows: lifecycleEvents },
  { label: 'Drag (Editor-only)', rows: dragEvents },
]

/* ------------------------------------------------------------------ */
/*  Event-data property tables                                        */
/* ------------------------------------------------------------------ */

type PropRow = { name: string; type: string; desc: string }

const reactiveEventProps: PropRow[] = [
  { name: 'Type', type: 'string', desc: 'UIElements event type name' },
  { name: 'Target', type: 'VisualElement', desc: 'Element that originally dispatched the event' },
  { name: 'CurrentTarget', type: 'VisualElement', desc: "Current handler's element" },
  { name: 'Timestamp', type: 'long', desc: 'Event timestamp' },
  { name: 'IsPropagationStopped', type: 'bool', desc: 'Whether propagation was stopped' },
  { name: 'IsDefaultPrevented', type: 'bool', desc: 'Whether the default action was prevented' },
  { name: 'NativeEvent', type: 'EventBase', desc: 'Underlying UIElements event' },
]

const pointerEventProps: PropRow[] = [
  { name: 'Position', type: 'Vector2', desc: 'Pointer position in panel space' },
  { name: 'DeltaPosition', type: 'Vector2', desc: 'Movement since last event' },
  { name: 'Button', type: 'int', desc: 'Mouse button index (0=left, 1=right, 2=middle)' },
  { name: 'ClickCount', type: 'int', desc: 'Number of rapid clicks (double-click = 2)' },
  { name: 'PointerId', type: 'int', desc: 'Pointer identifier for multi-touch' },
  { name: 'Pressure', type: 'float', desc: 'Pen/touch pressure (0–1)' },
  { name: 'Radius', type: 'Vector2', desc: 'Touch contact radius' },
  { name: 'AltKey', type: 'bool', desc: 'Alt key held' },
  { name: 'CtrlKey', type: 'bool', desc: 'Ctrl key held' },
  { name: 'ShiftKey', type: 'bool', desc: 'Shift key held' },
  { name: 'CommandKey', type: 'bool', desc: 'Command/Windows key held' },
]

const wheelEventProps: PropRow[] = [
  { name: 'Delta', type: 'Vector3', desc: 'Scroll delta (x = horizontal, y = vertical, z = depth)' },
]

const keyboardEventProps: PropRow[] = [
  { name: 'KeyCode', type: 'KeyCode', desc: 'Unity KeyCode of the pressed key' },
  { name: 'Character', type: 'char', desc: 'Character produced by the key press' },
  { name: 'AltKey', type: 'bool', desc: 'Alt key held' },
  { name: 'CtrlKey', type: 'bool', desc: 'Ctrl key held' },
  { name: 'ShiftKey', type: 'bool', desc: 'Shift key held' },
  { name: 'CommandKey', type: 'bool', desc: 'Command/Windows key held' },
]

const focusEventProps: PropRow[] = [
  { name: 'RelatedTarget', type: 'VisualElement', desc: 'Element losing or gaining focus' },
]

const geometryEventProps: PropRow[] = [
  { name: 'OldRect', type: 'Rect', desc: 'Previous bounding rectangle' },
  { name: 'NewRect', type: 'Rect', desc: 'New bounding rectangle' },
]

const panelEventProps: PropRow[] = [
  { name: 'Panel', type: 'VisualElement', desc: 'Panel being attached or detached' },
]

/* ------------------------------------------------------------------ */
/*  Helper: render a prop-table                                       */
/* ------------------------------------------------------------------ */

const PropTable: FC<{ rows: PropRow[] }> = ({ rows }) => (
  <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell><strong>Property</strong></TableCell>
          <TableCell><strong>Type</strong></TableCell>
          <TableCell><strong>Description</strong></TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.map((r) => (
          <TableRow key={r.name}>
            <TableCell><code>{r.name}</code></TableCell>
            <TableCell><code>{r.type}</code></TableCell>
            <TableCell>{r.desc}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </TableContainer>
)

/* ------------------------------------------------------------------ */
/*  Main page                                                         */
/* ------------------------------------------------------------------ */

export const EventsPage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Events &amp; Input Handling
    </Typography>
    <Typography variant="body1" paragraph>
      Every element in ReactiveUIToolKit inherits a common set of event handler
      props from <code>BaseProps</code>. Events use reactive wrappers around
      Unity UI Toolkit&apos;s native <code>EventBase</code> types, giving you
      typed access to pointer positions, key codes, modifier keys, and more.
    </Typography>

    {/* ── Complete event handler table ──────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Event handler reference
      </Typography>
      <Typography variant="body1" paragraph>
        All handlers below are available on every element (inherited from{' '}
        <code>BaseProps</code>).
      </Typography>

      {allEvents.map((group) => (
        <Box key={group.label} sx={{ mb: 2 }}>
          <Typography variant="h6" gutterBottom>
            {group.label}
          </Typography>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell><strong>Prop</strong></TableCell>
                  <TableCell><strong>Delegate</strong></TableCell>
                  <TableCell><strong>Event argument</strong></TableCell>
                  <TableCell><strong>Notes</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {group.rows.map((r) => (
                  <TableRow key={r.prop}>
                    <TableCell><code>{r.prop}</code></TableCell>
                    <TableCell><code>{r.delegate}</code></TableCell>
                    <TableCell><code>{r.eventArg}</code></TableCell>
                    <TableCell>
                      {r.note && <Chip label={r.note} size="small" color={r.note === 'Editor-only' ? 'warning' : 'default'} />}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      ))}
    </Box>

    {/* ── Event data classes ───────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Event data classes
      </Typography>

      <Typography variant="h6" gutterBottom>
        ReactiveEvent (base)
      </Typography>
      <Typography variant="body2" paragraph>
        All event arguments extend <code>ReactiveEvent</code>. Call{' '}
        <code>StopPropagation()</code> to stop the event from bubbling, or{' '}
        <code>PreventDefault()</code> to cancel the default behaviour.
      </Typography>
      <PropTable rows={reactiveEventProps} />

      <Typography variant="h6" gutterBottom>
        ReactivePointerEvent
      </Typography>
      <Typography variant="body2" paragraph>
        Used by all pointer handlers: <code>onClick</code>,{' '}
        <code>onPointerDown</code>, <code>onPointerUp</code>,{' '}
        <code>onPointerMove</code>, <code>onPointerEnter</code>,{' '}
        <code>onPointerLeave</code>.
      </Typography>
      <PropTable rows={pointerEventProps} />

      <Typography variant="h6" gutterBottom>
        ReactiveWheelEvent
      </Typography>
      <Typography variant="body2" paragraph>
        Extends <code>ReactivePointerEvent</code> with scroll delta. Used by{' '}
        <code>onWheel</code> and <code>onScroll</code>.
      </Typography>
      <PropTable rows={wheelEventProps} />

      <Typography variant="h6" gutterBottom>
        ReactiveKeyboardEvent
      </Typography>
      <PropTable rows={keyboardEventProps} />

      <Typography variant="h6" gutterBottom>
        ReactiveFocusEvent
      </Typography>
      <PropTable rows={focusEventProps} />

      <Typography variant="h6" gutterBottom>
        ReactiveGeometryEvent
      </Typography>
      <PropTable rows={geometryEventProps} />

      <Typography variant="h6" gutterBottom>
        ReactivePanelEvent
      </Typography>
      <PropTable rows={panelEventProps} />
    </Box>

    {/* ── ChangeEvent<T> ───────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'ChangeEvent<T> & onInput'}
      </Typography>
      <Typography variant="body1" paragraph>
        Input controls fire typed change events. The type parameter matches the
        control: <code>{'ChangeEvent<bool>'}</code> for <code>Toggle</code>,{' '}
        <code>{'ChangeEvent<float>'}</code> for <code>Slider</code>,{' '}
        <code>{'ChangeEvent<string>'}</code> for <code>TextField</code>, etc.
      </Typography>
      <Typography variant="body1" paragraph>
        For text-based controls, <code>onInput</code> provides the new value
        directly as a <code>string</code> (no event wrapper).
      </Typography>
      <CodeBlock language="jsx" code={EVENTS_CHANGE_EXAMPLE} />
    </Box>

    {/* ── Examples ─────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Examples
      </Typography>

      <Typography variant="h6" gutterBottom>
        Click handling
      </Typography>
      <CodeBlock language="jsx" code={EVENTS_CLICK_EXAMPLE} />

      <Typography variant="h6" gutterBottom>
        Pointer tracking
      </Typography>
      <CodeBlock language="jsx" code={EVENTS_POINTER_EXAMPLE} />

      <Typography variant="h6" gutterBottom>
        Keyboard input
      </Typography>
      <CodeBlock language="jsx" code={EVENTS_KEYBOARD_EXAMPLE} />

      <Typography variant="h6" gutterBottom>
        Focus tracking
      </Typography>
      <CodeBlock language="jsx" code={EVENTS_FOCUS_EXAMPLE} />

      <Typography variant="h6" gutterBottom>
        Geometry changes
      </Typography>
      <CodeBlock language="jsx" code={EVENTS_GEOMETRY_EXAMPLE} />
    </Box>

    {/* ── Propagation ──────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Event propagation
      </Typography>
      <Typography variant="body1" paragraph>
        Events bubble from the target element up through its ancestors, matching
        Unity UI Toolkit&apos;s native propagation model. Call{' '}
        <code>e.StopPropagation()</code> to stop bubbling.
      </Typography>
      <CodeBlock language="jsx" code={EVENTS_PROPAGATION_EXAMPLE} />
    </Box>

    {/* ── Editor-only drag events ──────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Editor-only drag events
      </Typography>
      <Alert severity="warning" sx={{ mb: 2 }}>
        Drag events (<code>onDragEnter</code>, <code>onDragLeave</code>,{' '}
        <code>onDragUpdated</code>, <code>onDragPerform</code>,{' '}
        <code>onDragExited</code>) are only available in the Unity Editor. They
        require the <code>UNITY_EDITOR</code> scripting define.
      </Alert>
      <CodeBlock language="jsx" code={EVENTS_DRAG_EXAMPLE} />
    </Box>

    {/* ── Delegate signatures ──────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Delegate signatures
      </Typography>
      <CodeBlock
        language="jsx"
        code={`public delegate void PointerEventHandler(ReactivePointerEvent e);
public delegate void WheelEventHandler(ReactiveWheelEvent e);
public delegate void KeyboardEventHandler(ReactiveKeyboardEvent e);
public delegate void FocusEventHandler(ReactiveFocusEvent e);
public delegate void DragEventHandler(ReactiveDragEvent e);      // Editor-only
public delegate void GeometryChangedEventHandler(ReactiveGeometryEvent e);
public delegate void PanelLifecycleEventHandler(ReactivePanelEvent e);
public delegate void ChangeEventHandler<T>(ChangeEvent<T> e);
public delegate void InputEventHandler(string newValue);
public delegate void ErrorEventHandler(Exception error);
public delegate void MenuBuilderHandler(DropdownMenu menu);`}
      />
    </Box>
  </Box>
)
