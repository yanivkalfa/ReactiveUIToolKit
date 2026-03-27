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
  </Box>
)
