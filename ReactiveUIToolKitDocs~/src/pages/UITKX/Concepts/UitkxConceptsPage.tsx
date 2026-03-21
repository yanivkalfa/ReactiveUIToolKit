import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from '../../Concepts/ConceptsPage.style'

export const UitkxConceptsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Concepts & Environment
    </Typography>
    <Typography variant="body1" paragraph>
      UITKX is the authoring layer. ReactiveUITK is the runtime layer underneath it. In practice,
      that means you think in terms of components, intrinsic tags, hooks, and markup structure, while
      the runtime handles reconciliation, scheduling, and adapter application.
    </Typography>
    <Typography variant="body1" paragraph>
      The key mental model is: write UI as UITKX, keep your setup code local to the component, and
      let the generator and runtime bridge that into Unity UI Toolkit.
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
          <ListItemText primary="Use companion partial classes only to host generated output, not as the main place where UI is authored." />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Environment defines
      </Typography>
      <Typography variant="body2" paragraph>
        Compile-time environment and tracing symbols still work the same way in UITKX projects,
        because the generated output runs on the same ReactiveUITK runtime.
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ENV_DEV</code>, <code>ENV_STAGING</code>, <code>ENV_PROD</code> control environment labeling.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RUITK_TRACE_VERBOSE</code> and <code>RUITK_TRACE_BASIC</code> control runtime diagnostics.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Editor-only diagnostic helpers still compile behind the same development symbols." />
        </ListItem>
      </List>
    </Box>
  </Box>
)
