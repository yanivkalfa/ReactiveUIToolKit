import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from '../../API/APIPage.style'

export const UitkxAPIPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      UITKX API Map
    </Typography>
    <Typography variant="body1" paragraph>
      For UITKX users, the important API split is: author in markup, use hooks in setup code, and
      understand the runtime types only when you need to mount, integrate, or debug.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Authoring surface
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>.uitkx</code> function-style components are the primary source format.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>useState</code>, <code>useEffect</code>, <code>useMemo</code>, and <code>useSignal</code> are the normal UITKX setup-code hooks, alongside router/context helpers.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Intrinsic tags map onto built-in ReactiveUITK/UI Toolkit elements." />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Runtime layer underneath
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>V</code> and <code>VirtualNode</code> still exist as the underlying runtime representation.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RootRenderer</code> and <code>EditorRootRendererUtility</code> are still how UITKX output is mounted.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Props classes and typed styles still matter for low-level integration, custom emit targets, and debugging generated output." />
        </ListItem>
      </List>
    </Box>
  </Box>
)
