import type { FC } from 'react'
import {
  Box,
  List,
  ListItem,
  ListItemText,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material'
import Styles from './HmrPage.style'

const Section: FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
  <Box>
    <Typography variant="h5" component="h2" gutterBottom>
      {title}
    </Typography>
    {children}
  </Box>
)

export const HmrPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Hot Module Replacement
    </Typography>
    <Typography variant="body1" paragraph>
      Hot Module Replacement lets you edit <code>.uitkx</code> files and see changes instantly in
      the Unity Editor — without domain reload, without losing component state.
    </Typography>

    <Section title="Quick Start">
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<>Open <strong>ReactiveUITK → HMR Mode</strong> from the Unity menu bar.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Click <strong>Start HMR</strong>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Edit and save any <code>.uitkx</code> file.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="The component updates in-place — hook state (counters, refs, effects) is preserved." />
        </ListItem>
      </List>
    </Section>

    <Section title="How It Works">
      <Typography variant="body1" paragraph>
        When HMR is active:
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Assembly reloads are locked — no domain reload occurs on file saves." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>A <code>FileSystemWatcher</code> detects <code>.uitkx</code> changes under <code>Assets/</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>The file is parsed and emitted to C# using <code>ReactiveUITK.Language.dll</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>C# is compiled via Unity's built-in Roslyn compiler (<code>csc.dll</code>).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>The compiled assembly is loaded via <code>Assembly.Load(byte[])</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>The new <code>Render</code> delegate is swapped into all matching Fiber nodes.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="A re-render is triggered — hooks run against preserved state." />
        </ListItem>
      </List>
      <Typography variant="body1" paragraph>
        Total time: typically <strong>50–200 ms</strong> from save to visual update.
      </Typography>
    </Section>

    <Section title="State Preservation">
      <Typography variant="body1" paragraph>
        HMR preserves all hook state across swaps:
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>useState</code> — current values retained.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>useRef</code> — ref objects preserved.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>useEffect</code> — cleanup runs, effect re-runs with new closure.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>useMemo</code> / <code>useCallback</code> — recomputed with new function body.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>useContext</code> — context values preserved.</>} />
        </ListItem>
      </List>
      <Typography variant="body1" paragraph>
        If the number or order of hooks changes between edits, HMR detects the mismatch, resets
        state for that component, and logs a warning.
      </Typography>
    </Section>

    <Section title="Companion Files">
      <Typography variant="body1" paragraph>
        When a <code>.uitkx</code> file changes, HMR automatically includes all{' '}
        <code>.cs</code> files in the same directory (excluding <code>.g.cs</code> generated files)
        in the compilation. This covers:
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<>Partial class declarations (e.g. <code>MyComponent.cs</code>)</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Style files (e.g. <code>MyComponent.styles.cs</code>)</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Type definitions (e.g. <code>MyComponent.types.cs</code>)</>} />
        </ListItem>
      </List>
    </Section>

    <Section title="HMR Window">
      <Typography variant="body1" paragraph>
        The HMR window (<strong>ReactiveUITK → HMR Mode</strong>) shows:
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Start / Stop button with status indicator." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Stats: swap count, error count, last component name and timing." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Timing breakdown: Parse, Emit, Compile, and Swap durations per step." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Settings: auto-stop on play mode, swap notifications." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Keyboard Shortcuts: configurable bindings (see below)." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Recent Errors: last 10 compilation errors (scrollable, copyable)." />
        </ListItem>
      </List>
    </Section>

    <Section title="Keyboard Shortcuts">
      <Typography variant="body1" paragraph>
        Shortcuts are not bound by default — configure them in the HMR window to avoid conflicting
        with your existing keybindings.
      </Typography>
      <TableContainer component={Paper} variant="outlined" sx={Styles.table}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Action</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell>Toggle HMR</TableCell>
              <TableCell>Start or stop the HMR session</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Open / Close Window</TableCell>
              <TableCell>Show or hide the HMR window</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
      <Typography variant="body2" sx={{ mt: 1 }}>
        Requirements: at least one modifier key (Ctrl, Alt, or Shift) plus one regular key.
      </Typography>
    </Section>

    <Section title="Lifecycle">
      <TableContainer component={Paper} variant="outlined" sx={Styles.table}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Event</strong></TableCell>
              <TableCell><strong>Behavior</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell>Start HMR</TableCell>
              <TableCell>Assembly reload locked, file watcher started</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Stop HMR</TableCell>
              <TableCell>Assembly reload unlocked, pending changes compile normally</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Enter / Exit Play Mode</TableCell>
              <TableCell>Auto-stops HMR (configurable)</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Build (Player)</TableCell>
              <TableCell>Auto-stops HMR</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Editor quit</TableCell>
              <TableCell>Auto-stops HMR</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
      <Typography variant="body1" paragraph sx={{ mt: 1 }}>
        While HMR is active, <strong>all compilation is deferred</strong> — not just{' '}
        <code>.uitkx</code> changes. Any <code>.cs</code> edits accumulate and compile in one batch
        when HMR is stopped.
      </Typography>
    </Section>

    <Section title="Limitations">
      <TableContainer component={Paper} variant="outlined" sx={Styles.table}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Limitation</strong></TableCell>
              <TableCell><strong>Details</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell>Old assemblies stay in memory</TableCell>
              <TableCell>
                Mono cannot unload assemblies. ~10–30 KB per swap, cleared on domain reload.
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell>All compilation deferred</TableCell>
              <TableCell>
                Non-UITKX <code>.cs</code> changes don't take effect until HMR stops. UX warning
                shown.
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell>New components not hot-loaded</TableCell>
              <TableCell>
                A new <code>.uitkx</code> file compiles but has no active fibers to swap into yet.
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Static field changes ignored</TableCell>
              <TableCell>Statics live on the old assembly's type.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Cross-assembly props</TableCell>
              <TableCell>
                Props are read via reflection to handle type mismatches across assemblies.
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Section>

    <Section title="Troubleshooting">
      <Typography variant="h6" component="h3" gutterBottom>
        HMR doesn't start
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Check the Console for initialization errors." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Ensure <code>ReactiveUITK.Language.dll</code> exists in the <code>Analyzers/</code> folder.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Verify Unity's Roslyn compiler is present at <code>{'${EditorPath}'}/Data/DotNetSdkRoslyn/csc.dll</code>.</>} />
        </ListItem>
      </List>

      <Typography variant="h6" component="h3" gutterBottom sx={{ mt: 2 }}>
        Changes don't appear
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Confirm the file is saved (not just modified)." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Check the HMR window for compilation errors." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Verify the file is under <code>Assets/</code> (the watched directory).</>} />
        </ListItem>
      </List>

      <Typography variant="h6" component="h3" gutterBottom sx={{ mt: 2 }}>
        State is lost after edit
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Hook order or count may have changed — this triggers automatic state reset." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Check Console for "<code>[HMR] Hook mismatch</code>" messages.</>} />
        </ListItem>
      </List>
    </Section>
  </Box>
)
