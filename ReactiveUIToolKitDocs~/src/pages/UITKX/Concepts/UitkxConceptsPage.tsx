import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from '../../Concepts/ConceptsPage.style'

export const UitkxConceptsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Concepts & Environment
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit. You write
      components, use hooks to manage state, and the reconciler diffs and updates the{' '}
      <code>VisualElement</code> hierarchy for you.
    </Typography>
    <Typography variant="body1" paragraph>
      Where Unity or UI Toolkit impose different constraints (layout system, event model, or platform
      concerns), the library deliberately diverges from React to provide a more idiomatic Unity
      experience. Routing, signals, and safe-area helpers are examples of features that don't exist
      in core React but are important here.
    </Typography>
    <Typography variant="body1" paragraph>
      The package ships with a demo set under <code>Assets/ReactiveUIToolKit/Samples</code> (editor
      windows and runtime scenes). Import them into your project to see real-world usage of
      components, hooks, routing, signals, and more.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Core authoring rules
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Intrinsic UITKX/native tag names are reserved; custom components should use distinct names." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Function-style components are the default form: setup code first, then a single returned markup tree." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="State setters are called directly like functions, for example setCount(count + 1)." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Companion .cs files are optional — use them to share styles, types, or utilities. The source generator produces the full class from the .uitkx file alone." />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Common props (BaseProps)
      </Typography>
      <Typography variant="body1" paragraph>
        Every element inherits these properties from <code>BaseProps</code>. They
        are available on <code>{'<Button>'}</code>, <code>{'<Label>'}</code>,{' '}
        <code>{'<VisualElement>'}</code>, and all other elements:
      </Typography>
      <Typography component="ul" variant="body2">
        <li><code>name</code>, <code>className</code>, <code>style</code> — identity and styling</li>
        <li><code>ref</code>, <code>contentContainer</code> — element references</li>
        <li><code>visible</code>, <code>enabled</code> — visibility and interactivity</li>
        <li><code>pickingMode</code>, <code>focusable</code>, <code>tabIndex</code>, <code>delegatesFocus</code> — input focus</li>
        <li><code>tooltip</code>, <code>viewDataKey</code>, <code>languageDirection</code> — miscellaneous</li>
        <li><code>extraProps</code> — escape hatch for additional properties</li>
        <li>Event handlers: <code>onClick</code>, <code>onPointerDown</code>, <code>onPointerUp</code>, <code>onPointerMove</code>, <code>onPointerEnter</code>, <code>onPointerLeave</code>, <code>onKeyDown</code>, <code>onKeyUp</code>, <code>onFocusIn</code>, <code>onFocusOut</code>, <code>onWheel</code>, <code>onGeometryChanged</code>, <code>onAttachToPanel</code>, <code>onDetachFromPanel</code>, and more</li>
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Scripting define symbols (environment & tracing)
      </Typography>
      <Typography variant="body2" paragraph>
        Set these in <strong>Project Settings → Player → Scripting Define Symbols</strong>. They
        control environment labels and diagnostics at compile time.
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ENV_DEV</code> — development environment. Enables dev-oriented defaults such as Basic trace level and compiles editor diagnostics helpers.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ENV_STAGING</code> — staging environment label (no implicit tracing changes).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ENV_PROD</code> — production environment label. This is the implied default if no <code>ENV_*</code> symbol is defined.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RUITK_TRACE_VERBOSE</code> — force reconciler trace level to <strong>Verbose</strong>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RUITK_TRACE_BASIC</code> — force reconciler trace level to <strong>Basic</strong>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RUITK_DIFF_TRACING</code> — force <code>DiagnosticsConfig.EnableDiffTracing</code> to <code>true</code> for detailed Fiber diff diagnostics.</>} />
        </ListItem>
      </List>
      <Typography variant="body2" paragraph sx={Styles.section}>
        <strong>Behavior summary</strong>
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<>Environment is resolved to <code>development</code>, <code>staging</code>, or <code>production</code> via the <code>ENV_*</code> defines and is exposed at runtime as <code>HostContext.Environment["env"]</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Trace level resolution priority: <code>RUITK_TRACE_VERBOSE</code> &gt; <code>RUITK_TRACE_BASIC</code> &gt; <code>ENV_DEV</code> (Basic) &gt; none.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Editor-only diagnostic utilities compile only when ENV_DEV is defined." />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Rendering pipeline
      </Typography>
      <Typography variant="body1" paragraph>
        Understanding the pipeline helps you read error messages and diagnose performance:
      </Typography>
      <Typography component="ol" variant="body2">
        <li>
          <strong>Author</strong> — You write <code>.uitkx</code> markup with setup code and a{' '}
          <code>return (...)</code> statement.
        </li>
        <li>
          <strong>Generate</strong> — The Roslyn source generator compiles each{' '}
          <code>.uitkx</code> file into a C# class with a{' '}
          <code>Render(IProps, children)</code> method that returns a{' '}
          <code>VirtualNode</code> tree.
        </li>
        <li>
          <strong>Mount</strong> — <code>V.Func(Component.Render)</code> wraps the generated method
          as a <code>VirtualNode</code>. The <code>RootRenderer</code> (or{' '}
          <code>EditorRootRendererUtility</code>) mounts it into a{' '}
          <code>VisualElement</code> root.
        </li>
        <li>
          <strong>Reconcile</strong> — On each render, the Fiber reconciler diffs the old and new{' '}
          <code>VirtualNode</code> trees, computing the minimal set of patches.
        </li>
        <li>
          <strong>Commit</strong> — Patches are applied to the live{' '}
          <code>VisualElement</code> tree: elements are created, removed, or updated.
        </li>
        <li>
          <strong>Effects</strong> — After commit, cleanup functions from the previous render run,
          then new <code>UseEffect</code> / <code>UseLayoutEffect</code> callbacks fire.
        </li>
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Component lifecycle
      </Typography>
      <Typography component="ul" variant="body2">
        <li>
          <strong>Mount</strong> — construct <code>VirtualNode</code> → create{' '}
          <code>VisualElement</code> → run effects.
        </li>
        <li>
          <strong>Update</strong> — re-render → diff → patch → run cleanup → run new effects.
        </li>
        <li>
          <strong>Unmount</strong> — run all cleanup functions → remove{' '}
          <code>VisualElement</code> from the tree.
        </li>
      </Typography>
    </Box>
  </Box>
)
