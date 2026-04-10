import type { FC } from 'react'
import {
  Alert,
  Box,
  List,
  ListItem,
  ListItemText,
  Typography,
} from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import {
  REF_BASIC_EXAMPLE,
  REF_FOCUS_EXAMPLE,
  REF_IMPERATIVE_EXAMPLE,
  REF_MUTABLE_EXAMPLE,
} from './RefAndKeyGuide.example'

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 2 },
  list: { pl: 2 },
} as const

export const RefGuidePage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Refs Guide
    </Typography>
    <Typography variant="body1" paragraph>
      Refs give you direct access to values and elements that persist across
      renders without triggering re-renders when they change.
    </Typography>

    {/* ── Element refs ─────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Element refs
      </Typography>
      <Typography variant="body1" paragraph>
        Call <code>useRef()</code> (no type parameter) to get a{' '}
        <code>VisualElement</code> reference. Attach it via the{' '}
        <code>ref</code> prop. After the first render the ref points to the
        underlying UI Toolkit element.
      </Typography>
      <CodeBlock language="jsx" code={REF_BASIC_EXAMPLE} />
    </Box>

    {/* ── Mutable value refs ───────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Mutable value refs
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'useRef<T>(initial)'}</code> returns a <code>{'Ref<T>'}</code>{' '}
        with a <code>.Current</code> property. Unlike state, mutating{' '}
        <code>.Current</code> does <strong>not</strong> trigger a re-render.
      </Typography>
      <List sx={styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Use for render counters, previous-value tracking, interval IDs, or any mutable data that shouldn't re-render." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="The ref value persists across renders and survives HMR." />
        </ListItem>
      </List>
      <CodeBlock language="jsx" code={REF_MUTABLE_EXAMPLE} />
    </Box>

    {/* ── Focus example ────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Auto-focus pattern
      </Typography>
      <CodeBlock language="jsx" code={REF_FOCUS_EXAMPLE} />
    </Box>

    {/* ── Imperative handles ───────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Imperative handles
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'useImperativeHandle<T>'}</code> lets a child component expose a
        custom API object (e.g., Focus, Clear) to its parent instead of
        exposing the raw <code>VisualElement</code>.
      </Typography>
      <CodeBlock language="jsx" code={REF_IMPERATIVE_EXAMPLE} />
    </Box>

    <Alert severity="info" sx={{ mt: 2 }}>
      Avoid using refs to read values that could be expressed as state. Refs
      are an escape hatch — prefer <code>useState</code> when you need the UI
      to react to changes.
    </Alert>
  </Box>
)
