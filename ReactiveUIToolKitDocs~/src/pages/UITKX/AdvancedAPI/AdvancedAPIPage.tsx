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
  DEPTH_GUARD_EXAMPLE,
  ELEMENT_REGISTRY_EXAMPLE,
  ERROR_PATTERNS_EXAMPLE,
  FLUSHSYNC_EXAMPLE,
  HOSTCONTEXT_EXAMPLE,
  PROPTYPES_EXAMPLE,
  SCHEDULER_EXAMPLE,
  SNAPSHOT_EXAMPLE,
  VIRTUALNODE_EXAMPLE,
} from './AdvancedAPIPage.example'

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 3 },
  list: { pl: 2 },
} as const

/* ------------------------------------------------------------------ */
/*  PropTypes definitions table                                        */
/* ------------------------------------------------------------------ */

type PropTypeRow = { method: string; params: string; desc: string }

const propTypeMethods: PropTypeRow[] = [
  { method: 'String(name, required)', params: 'string, bool', desc: 'Validates prop is a string' },
  { method: 'Number(name, required)', params: 'string, bool', desc: 'Validates prop is numeric' },
  { method: 'Boolean(name, required)', params: 'string, bool', desc: 'Validates prop is bool' },
  { method: 'Enum(name, allowedValues, required)', params: 'string, IEnumerable<string>, bool', desc: 'Validates prop is one of the allowed values' },
  { method: 'InstanceOf<T>(name, required)', params: 'string, bool', desc: 'Validates prop is an instance of T' },
  { method: 'Custom(name, validator, description, required)', params: 'string, Func<object,bool>, string, bool', desc: 'Custom validation function' },
]

/* ------------------------------------------------------------------ */
/*  IScheduler Priority table                                          */
/* ------------------------------------------------------------------ */

type PriorityRow = { name: string; value: string; desc: string }

const priorities: PriorityRow[] = [
  { name: 'High', value: '0', desc: 'Critical updates — user input responses' },
  { name: 'Normal', value: '1', desc: 'Standard state updates (default)' },
  { name: 'Low', value: '2', desc: 'Deferred work — background computation' },
  { name: 'Idle', value: '3', desc: 'Lowest priority — analytics, logging' },
]

/* ------------------------------------------------------------------ */
/*  VirtualNodeType table                                              */
/* ------------------------------------------------------------------ */

type NodeTypeRow = { name: string; desc: string }

const nodeTypes: NodeTypeRow[] = [
  { name: 'Element', desc: 'Intrinsic UI Toolkit element (Button, Label, etc.)' },
  { name: 'Text', desc: 'Text content node' },
  { name: 'FunctionComponent', desc: 'User-defined function component' },
  { name: 'Fragment', desc: 'Invisible grouping wrapper (<>…</>)' },
  { name: 'Portal', desc: 'Renders children into external target' },
  { name: 'Suspense', desc: 'Shows fallback while loading' },
  { name: 'ErrorBoundary', desc: 'Catches child exceptions' },
  { name: 'Host', desc: 'Subtree mount point with own root' },
]

/* ------------------------------------------------------------------ */
/*  Page                                                               */
/* ------------------------------------------------------------------ */

export const AdvancedAPIPage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Advanced API Reference
    </Typography>
    <Typography variant="body1" paragraph>
      This page covers internal and advanced APIs that most users won&apos;t
      need for everyday <code>.uitkx</code> development. They are useful for
      custom renderers, testing, debugging, and advanced integration scenarios.
    </Typography>

    {/* ── PropTypes ────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        PropTypes &amp; PropTypeValidator
      </Typography>
      <Typography variant="body1" paragraph>
        A React-style prop validation system for development-time type checking.
        Attach <code>PropTypeDefinition</code> arrays to VirtualNodes via the{' '}
        <code>WithPropTypes()</code> extension method.
      </Typography>

      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Factory method</strong></TableCell>
              <TableCell><strong>Parameters</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {propTypeMethods.map((r) => (
              <TableRow key={r.method}>
                <TableCell><code>PropTypes.{r.method}</code></TableCell>
                <TableCell><code>{r.params}</code></TableCell>
                <TableCell>{r.desc}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <CodeBlock language="jsx" code={PROPTYPES_EXAMPLE} />

      <Alert severity="info" sx={{ mt: 1 }}>
        PropType validation runs only when{' '}
        <code>PropTypeValidator.Enabled</code> is <code>true</code> (default in
        Editor). Disable it in production builds for zero overhead.
      </Alert>
    </Box>

    {/* ── HostContext ──────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        HostContext &amp; Environment
      </Typography>
      <Typography variant="body1" paragraph>
        <code>HostContext</code> manages the component tree&apos;s environment
        dictionary and context provider stack. It is created internally by{' '}
        <code>RootRenderer</code> and exposed via the initialization callback.
      </Typography>

      <CodeBlock language="jsx" code={`public sealed class HostContext
{
    ElementRegistry ElementRegistry { get; }
    Dictionary<string, object> Environment { get; }

    void SetContextValue(string key, object value)
    object ResolveContext(string key)
}`} />

      <CodeBlock language="jsx" code={HOSTCONTEXT_EXAMPLE} />

      <List sx={styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>Environment</code> — a dictionary for scheduler, portal targets, flags, and other global values.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>SetContextValue</code> — sets a context value accessible to all descendants via <code>UseContext</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ResolveContext</code> — walks the provider stack, then falls back to the Environment dictionary.</>} />
        </ListItem>
      </List>
    </Box>

    {/* ── IScheduler ──────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        IScheduler &amp; Priorities
      </Typography>
      <Typography variant="body1" paragraph>
        <code>IScheduler</code> controls how render work is dispatched. The
        default <code>RenderScheduler</code> processes work once per Unity
        frame. Custom schedulers can be used for testing or headless rendering.
      </Typography>

      <CodeBlock language="jsx" code={`public interface IScheduler
{
    enum Priority { High = 0, Normal = 1, Low = 2, Idle = 3 }

    void Enqueue(Action action, Priority priority = Priority.Normal);
    void EnqueueBatchedEffect(Action effect);
    void BeginBatch();
    void EndBatch();
    void PumpNow();   // synchronously drain pending work
}`} />

      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Priority</strong></TableCell>
              <TableCell><strong>Value</strong></TableCell>
              <TableCell><strong>Use case</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {priorities.map((r) => (
              <TableRow key={r.name}>
                <TableCell><code>{r.name}</code></TableCell>
                <TableCell>{r.value}</TableCell>
                <TableCell>{r.desc}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <CodeBlock language="jsx" code={SCHEDULER_EXAMPLE} />
    </Box>

    {/* ── FlushSync ───────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        FlushSync
      </Typography>
      <Typography variant="body1" paragraph>
        <code>Hooks.FlushSync</code> forces all pending state updates to be
        processed synchronously, bypassing the scheduler&apos;s frame-based
        batching. Use sparingly — it blocks until the render is complete.
      </Typography>

      <CodeBlock language="jsx" code={`// Batch and flush state updates synchronously
public static void Hooks.FlushSync(Action action)

// Drain pending scheduler work without new updates
public static void Hooks.FlushSync()`} />

      <CodeBlock language="jsx" code={FLUSHSYNC_EXAMPLE} />

      <Alert severity="warning" sx={{ mt: 1 }}>
        Avoid calling <code>FlushSync</code> inside effects or render functions.
        It is designed for imperative code paths like event handlers or external
        API callbacks where you need the UI to update before continuing.
      </Alert>
    </Box>

    {/* ── Error Handling Patterns ──────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Error handling patterns
      </Typography>
      <Typography variant="body1" paragraph>
        <code>ErrorBoundary</code> catches exceptions thrown during rendering by
        any descendant component. Combine multiple boundaries for granular
        recovery, and use <code>ResetKey</code> to retry after errors.
      </Typography>
      <CodeBlock language="jsx" code={ERROR_PATTERNS_EXAMPLE} />
    </Box>

    {/* ── Render Depth Guard ──────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Render depth guard
      </Typography>
      <Typography variant="body1" paragraph>
        The reconciler tracks nesting depth during render. If a component
        triggers more than <strong>25</strong> nested re-renders (usually caused
        by calling <code>setState</code> unconditionally in setup code), the
        guard stops the infinite loop and logs an error.
      </Typography>
      <CodeBlock language="jsx" code={DEPTH_GUARD_EXAMPLE} />
    </Box>

    {/* ── SnapshotAssert ──────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        SnapshotAssert (testing)
      </Typography>
      <Typography variant="body1" paragraph>
        <code>SnapshotAssert</code> compares two <code>VirtualNode</code> trees
        structure and produces a diff when they differ. Useful for unit-testing
        component render output.
      </Typography>

      <CodeBlock language="jsx" code={`public static class SnapshotAssert
{
    public struct Result { bool Pass; string Diff; string Expected; string Actual; }

    static Result Compare(VirtualNode expected, VirtualNode actual)
    static void AssertEqual(VirtualNode expected, VirtualNode actual, Action<string> logAction = null)
}`} />

      <CodeBlock language="jsx" code={SNAPSHOT_EXAMPLE} />
    </Box>

    {/* ── ElementRegistry ─────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        ElementRegistry
      </Typography>
      <Typography variant="body1" paragraph>
        <code>ElementRegistry</code> maps element type names (like{' '}
        <code>&quot;Button&quot;</code>) to <code>IElementAdapter</code>{' '}
        implementations that know how to create and patch the corresponding{' '}
        <code>VisualElement</code>. The default registry includes{' '}
        <strong>61</strong> built-in elements.
      </Typography>

      <CodeBlock language="jsx" code={`public sealed class ElementRegistry
{
    void Register(string elementTypeName, IElementAdapter adapter)
    IElementAdapter Resolve(string elementTypeName)
}

public static class ElementRegistryProvider
{
    static ElementRegistry GetDefaultRegistry()         // 61 built-in elements
    static ElementRegistry CreateFilteredRegistry(IEnumerable<string> allowed)
}`} />

      <CodeBlock language="jsx" code={ELEMENT_REGISTRY_EXAMPLE} />

      <Alert severity="info" sx={{ mt: 1 }}>
        <code>CreateFilteredRegistry</code> is useful for sandboxed environments
        where you want to restrict which elements are available (e.g., user-generated
        UI that should only use safe container elements).
      </Alert>
    </Box>

    {/* ── VirtualNode ─────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        VirtualNode
      </Typography>
      <Typography variant="body1" paragraph>
        <code>VirtualNode</code> is the immutable node type representing the
        virtual DOM tree. In <code>.uitkx</code> files, the source generator
        creates VirtualNodes automatically from your markup. If you use the C#
        runtime API directly, you build trees with <code>V.*</code> factory
        methods.
      </Typography>

      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Node type</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {nodeTypes.map((r) => (
              <TableRow key={r.name}>
                <TableCell><code>{r.name}</code></TableCell>
                <TableCell>{r.desc}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <CodeBlock language="jsx" code={`public sealed class VirtualNode
{
    // Identity & type
    VirtualNodeType NodeType { get; }
    string ElementTypeName { get; }
    string Key { get; }
    string TextContent { get; }

    // Children & props
    IReadOnlyDictionary<string, object> Properties { get; }
    IReadOnlyList<VirtualNode> Children { get; }

    // Special nodes
    VisualElement PortalTarget { get; }      // Portal
    VirtualNode Fallback { get; }            // Suspense / ErrorBoundary
    Func<bool> SuspenseReady { get; }        // Suspense callback
    Task SuspenseReadyTask { get; }          // Suspense async
    ErrorEventHandler ErrorHandler { get; }  // ErrorBoundary
    string ErrorResetToken { get; }          // ErrorBoundary reset

    // Function components
    Func<IProps, IReadOnlyList<VirtualNode>, VirtualNode> TypedFunctionRender { get; }
    IProps TypedProps { get; }
}`} />

      <CodeBlock language="jsx" code={VIRTUALNODE_EXAMPLE} />
    </Box>
  </Box>
)
