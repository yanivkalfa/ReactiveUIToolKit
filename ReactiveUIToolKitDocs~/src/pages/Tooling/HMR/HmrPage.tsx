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
          <ListItemText primary={<>C# is compiled in-process via Roslyn (<code>Microsoft.CodeAnalysis.CSharp</code> 4.3.1), with automatic fallback to external <code>csc.dll</code> if Roslyn DLLs aren't available.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>The compiled assembly is loaded via <code>Assembly.Load(byte[])</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>The new <code>Render</code> delegate is swapped into all active <code>RootRenderer</code> instances.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="A re-render is triggered — hooks run against preserved state." />
        </ListItem>
      </List>
      <Typography variant="body1" paragraph>
        Total time: typically <strong>25–100 ms</strong> compile + emit from save to visual update
        (first compile per session is ~1–1.5s due to Roslyn JIT warmup).
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
        Companion <code>.cs</code> files are <strong>optional</strong>. The source generator
        produces a complete class from the <code>.uitkx</code> file alone. However, you can add
        <code>.cs</code> files in the same directory to share styles, types, or utilities. When a{' '}
        <code>.uitkx</code> file changes, HMR automatically includes all <code>.cs</code> files in
        the same directory (excluding <code>.g.cs</code>) in the compilation:
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<>Style helpers (e.g. <code>MyComponent.styles.cs</code>)</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Type / prop definitions (e.g. <code>MyComponent.types.cs</code>)</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Shared utilities (e.g. <code>MyComponent.utils.cs</code>)</>} />
        </ListItem>
      </List>
      <Typography variant="body1" paragraph>
        Companion <code>.cs</code> file changes also trigger HMR — saving a{' '}
        <code>.styles.cs</code> or <code>.utils.cs</code> file automatically detects the
        associated <code>.uitkx</code> in the same directory, recompiles everything, and swaps the
        result in-place.
      </Typography>
      <Typography variant="body1" paragraph>
        <strong>Creating new companion files</strong> works too — simply create a <code>.cs</code>{' '}
        file in the same directory as your <code>.uitkx</code>. The file watcher detects new files
        and includes them in the next compilation.
      </Typography>
    </Section>

    <Section title="New Component Support">
      <Typography variant="body1" paragraph>
        HMR can compile and load <strong>new</strong> <code>.uitkx</code> files that don't exist in
        any pre-compiled assembly:
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="When a parent component references an unknown child, CS0103 errors are caught." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>HMR scans the project for matching <code>.uitkx</code> files and compiles them first.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="The parent is automatically retried after the dependency resolves." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Cross-component references are managed via an assembly registry." />
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
              <TableCell>First compile is slow</TableCell>
              <TableCell>
                ~1–1.5s on first HMR compile per session (Roslyn JIT warmup). Subsequent compiles
                are 25–100ms.
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Requires NuGet cache</TableCell>
              <TableCell>
                In-process Roslyn loads DLLs from <code>~/.nuget/packages/</code>. Falls back to
                external <code>csc.dll</code> if unavailable.
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
        <ListItem disablePadding>
          <ListItemText primary={<>Check for <code>[HMR] In-process Roslyn compiler loaded successfully</code> in Console.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>If Roslyn fails to load, verify that <code>~/.nuget/packages/microsoft.codeanalysis.csharp/4.3.1/</code> exists.</>} />
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
