import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './SignalsPage.style'
import {
  SIGNAL_EDITOR_COMPONENT_EXAMPLE,
  SIGNAL_RUNTIME_EXAMPLE,
} from './SignalsPage.example'

export const SignalsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Signals
    </Typography>
    <Typography variant="body1" paragraph>
      <code>Signals</code> are lightweight, named reactive values that live in a process-wide
      registry. They behave like a small observable store with a simple API and are ideal whenever
      you want a single source of truth with a single point of entry for reading and updating state
      (for example: selection, filters, or global preferences).
    </Typography>
    <Box>
      <Typography variant="h5" component="h3" gutterBottom>
        Concepts
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>Signals</code> live in a global registry keyed by <code>string</code>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Call <code>Signals.Get&lt;T&gt;(key, initialValue)</code> to create or return a{' '}
                <code>Signal&lt;T&gt;</code> instance.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Call <code>signal.Subscribe(...)</code> to watch changes outside of components; use{' '}
                <code>Hooks.UseSignal(...)</code> inside function components.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Use <code>Dispatch(prev =&gt; next)</code> or <code>Dispatch(value)</code> to update
                the value and notify listeners.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Typography variant="h5" component="h3" gutterBottom>
      Runtime usage
    </Typography>
    <CodeBlock language="tsx" code={SIGNAL_RUNTIME_EXAMPLE} />

    <Typography variant="h5" component="h3" gutterBottom>
      Using signals from components
    </Typography>
    <Typography variant="body1" paragraph>
      Inside function components, use <code>Hooks.UseSignal</code> or the selector overload{' '}
      <code>Hooks.UseSignal&lt;T, TSlice&gt;(...)</code> to read a signal and re-render when it
      changes. The example below shows a simple counter bound to the global <code>demo-counter</code>{' '}
      signal, but you can also project a slice of a more complex signal value and compare with a
      custom equality comparer for performance.
    </Typography>
    <CodeBlock language="tsx" code={SIGNAL_EDITOR_COMPONENT_EXAMPLE} />
  </Box>
)
