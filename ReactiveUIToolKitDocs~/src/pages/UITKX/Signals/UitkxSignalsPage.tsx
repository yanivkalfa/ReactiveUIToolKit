import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../Signals/SignalsPage.style'
import {
  UITKX_SIGNALS_COMPONENT_EXAMPLE,
  UITKX_SIGNALS_RUNTIME_EXAMPLE,
} from './UitkxSignalsPage.example'

export const UitkxSignalsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Signals
    </Typography>
    <Typography variant="body1" paragraph>
      Signals remain the shared-state primitive underneath the UITKX authoring model. Outside
      components you work with the signal registry directly; inside UITKX components you typically
      read them with <code>useSignal(...)</code> and dispatch updates from event handlers.
    </Typography>

    <Box>
      <Typography variant="h5" component="h3" gutterBottom>
        Runtime access
      </Typography>
      <CodeBlock language="tsx" code={UITKX_SIGNALS_RUNTIME_EXAMPLE} />
    </Box>

    <Box>
      <Typography variant="h5" component="h3" gutterBottom>
        Using signals inside UITKX
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Use a signal factory or registry lookup in setup code." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Read the current value with useSignal(...)." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Dispatch updates directly from UITKX event handlers." />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={UITKX_SIGNALS_COMPONENT_EXAMPLE} />
    </Box>
  </Box>
)
