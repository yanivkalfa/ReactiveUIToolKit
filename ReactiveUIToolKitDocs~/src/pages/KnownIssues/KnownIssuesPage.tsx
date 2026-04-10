import type { FC } from 'react'
import { Alert, Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './KnownIssuesPage.style'

export const KnownIssuesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Known Issues
    </Typography>

    {/* ── Runtime ─────────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Runtime
    </Typography>
    <Typography variant="body1" paragraph>
      There is a known issue where <code>MultiColumnListView</code> can briefly
      jump or snap when scrolling large data sets; this will be addressed in a
      future update.
    </Typography>

    {/* ── Burst AOT ───────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Burst AOT &amp; Assembly Resolution
    </Typography>
    <Typography variant="body1" paragraph>
      If you encounter the error:
    </Typography>
    <CodeBlock
      language="jsx"
      code="Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: Assembly-CSharp-Editor"
    />
    <Typography variant="body1" paragraph>
      Go to <strong>Edit → Project Settings → Burst AOT Settings</strong> and
      add <code>Assembly-CSharp-Editor</code> to the exclusion list. This
      prevents Burst from trying to AOT-compile editor-only assemblies that
      reference UITKX types.
    </Typography>

    {/* ── HMR Limitations ─────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      HMR Limitations
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Old assemblies from HMR swaps cannot be unloaded by Mono. Each swap leaks approximately 10–30 KB. This is negligible for normal development sessions but accumulates over very long sessions." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="The first HMR compile is ~1–1.5 seconds due to Roslyn JIT warmup. Subsequent compiles are 25–100 ms." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Adding a brand-new .uitkx file while HMR is running requires HMR to detect and compile it. The file must be referenced by an existing component or HMR-watched folder to be auto-discovered." />
      </ListItem>
    </List>

    {/* ── Render Depth ────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Component Tree Depth
    </Typography>
    <Typography variant="body1" paragraph>
      The reconciler enforces a maximum render depth of <strong>25</strong>{' '}
      nested re-renders per component. If a component calls{' '}
      <code>setState</code> unconditionally during its setup code (creating an
      infinite render loop), the depth guard stops it and logs an error. This is
      not configurable — restructure your component to move state updates into
      event handlers or effects.
    </Typography>

    {/* ── Hooks ───────────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Hook Constraints
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Hooks must be called unconditionally at the top of the component's setup code. Calling hooks inside @if, @for, or other control blocks breaks hook ordering and causes runtime errors." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Thread safety: hooks are NOT thread-safe. All hook calls must happen on the main thread during the render cycle. Signal values can be read/written from any thread, but UseSignal() itself is a hook and follows hook rules." />
      </ListItem>
    </List>

    {/* ── Editor vs Runtime ───────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Editor vs Runtime Differences
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<>Editor uses <code>EditorRenderScheduler</code> (tied to <code>EditorApplication.update</code>), while runtime uses <code>RenderScheduler</code> (tied to <code>MonoBehaviour.Update</code>). Scheduling timing may differ slightly.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<>Drag events (<code>onDragEnter</code>, <code>onDragLeave</code>, <code>onDragUpdated</code>, <code>onDragPerform</code>, <code>onDragExited</code>) are editor-only and require <code>UNITY_EDITOR</code>.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<>Some components are editor-only: <code>PropertyField</code>, <code>InspectorElement</code>, <code>ObjectField</code>, <code>ColorField</code>, <code>Toolbar</code> and its children, <code>TwoPaneSplitView</code>, <code>HelpBox</code>, <code>IMGUIContainer</code>.</>} />
      </ListItem>
    </List>

    <Alert severity="info" sx={{ mt: 2 }}>
      For troubleshooting build or LSP issues, see the{' '}
      <strong>Debugging Guide</strong>.
    </Alert>
  </Box>
)
