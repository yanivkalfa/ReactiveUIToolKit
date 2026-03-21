import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from './ConceptsPage.style'

export const ConceptsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Concepts & Environment
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit aims to feel familiar if you know React, while still fitting naturally into
      Unity&apos;s UI Toolkit and C# ecosystem. You build trees from <code>V.*</code> helpers and
      function components, use hooks to manage state, and let the reconciler diff and update the
      underlying <code>VisualElement</code> hierarchy for you.
    </Typography>
    <Typography variant="body1" paragraph>
      Where Unity or UI Toolkit impose different constraints (for example: layout system, event
      model, or platform concerns), the library deliberately diverges from React to provide a more
      idiomatic Unity experience. The routing, signals, and safe-area helpers are examples of
      features that don&apos;t exist in core React but are important here.
    </Typography>
    <Typography variant="body1" paragraph>
      The package also ships with a rich demo set under <code>Assets/ReactiveUIToolKit/Samples</code>{' '}
      (editor windows and runtime scenes) that you can import into your project. These demos show
      real-world usage of components, hooks, routing, signals, and more, and are a great way to see
      the concepts on this page in action.
    </Typography>

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
          <ListItemText
            primary={
              <>
                <code>ENV_DEV</code> — development environment. Enables dev-oriented defaults such
                as Basic trace level and compiles editor diagnostics helpers.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ENV_STAGING</code> — staging environment label (no implicit tracing changes).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ENV_PROD</code> — production environment label. This is the implied default if
                no <code>ENV_*</code> symbol is defined.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RUITK_TRACE_VERBOSE</code> — force reconciler trace level to{' '}
                <strong>Verbose</strong>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RUITK_TRACE_BASIC</code> — force reconciler trace level to{' '}
                <strong>Basic</strong>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>RUITK_DIFF_TRACING</code> — force{' '}
                <code>DiagnosticsConfig.EnableDiffTracing</code> to <code>true</code> for detailed
                Fiber diff diagnostics.
              </>
            }
          />
        </ListItem>
      </List>

      <Typography variant="body2" paragraph sx={Styles.section}>
        <strong>Behavior summary</strong>
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Environment is resolved to <code>development</code>, <code>staging</code>, or{' '}
                <code>production</code> via the <code>ENV_*</code> defines and is exposed at runtime
                as <code>HostContext.Environment[&quot;env&quot;]</code>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Trace level resolution priority:{' '}
                <code>RUITK_TRACE_VERBOSE</code> &gt; <code>RUITK_TRACE_BASIC</code> &gt;{' '}
                <code>ENV_DEV</code> (Basic) &gt; none.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary="Editor-only diagnostic utilities are compiled only when ENV_DEV is defined."
          />
        </ListItem>
      </List>
    </Box>
  </Box>
)
