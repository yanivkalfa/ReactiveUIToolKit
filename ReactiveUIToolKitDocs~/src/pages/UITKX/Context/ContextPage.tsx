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
  CONTEXT_BASIC_EXAMPLE,
  CONTEXT_DYNAMIC_EXAMPLE,
  CONTEXT_SHADOWING_EXAMPLE,
  CONTEXT_TYPED_EXAMPLE,
  CONTEXT_VS_SIGNALS,
} from './ContextPage.example'

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 2 },
  list: { pl: 2 },
} as const

export const ContextPage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Context API
    </Typography>
    <Typography variant="body1" paragraph>
      Context lets parent components provide data to any descendant without
      passing it through every intermediate component as props. It is the
      primary mechanism for dependency injection in ReactiveUIToolKit.
    </Typography>

    {/* ── API ──────────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        API
      </Typography>
      <CodeBlock language="jsx" code={`// Provide a value for all descendants
void provideContext<T>(string key, T value)
void provideContext(string key, object value)   // untyped overload

// Consume a value from the nearest provider above
T useContext<T>(string key)`} />
      <List sx={styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<>Context values are keyed by <code>string</code>. Use constant keys to prevent typos.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ProvideContext</code> attaches the value to the current component&apos;s fiber node.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>UseContext</code> walks up the fiber tree to find the nearest provider for that key.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>Returns <code>default(T)</code> if no provider is found.</>} />
        </ListItem>
      </List>
    </Box>

    {/* ── Basic example ────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic provider / consumer
      </Typography>
      <CodeBlock language="jsx" code={CONTEXT_BASIC_EXAMPLE} />
    </Box>

    {/* ── Provider shadowing ───────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Provider shadowing
      </Typography>
      <Typography variant="body1" paragraph>
        A nested provider for the same key <strong>shadows</strong> the outer
        provider. Each subtree sees its nearest ancestor&apos;s value.
      </Typography>
      <CodeBlock language="jsx" code={CONTEXT_SHADOWING_EXAMPLE} />
    </Box>

    {/* ── Dynamic context + re-renders ─────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Dynamic context values
      </Typography>
      <Typography variant="body1" paragraph>
        When the provided value changes (detected via <code>object.Equals</code>),
        all consumers in the subtree automatically schedule a re-render.
      </Typography>
      <CodeBlock language="jsx" code={CONTEXT_DYNAMIC_EXAMPLE} />
    </Box>

    {/* ── Type-safe keys ───────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Type-safe context keys
      </Typography>
      <Typography variant="body1" paragraph>
        Define context keys as string constants in a companion file or static
        class. This prevents typos and makes keys discoverable via IntelliSense.
      </Typography>
      <CodeBlock language="jsx" code={CONTEXT_TYPED_EXAMPLE} />
    </Box>

    {/* ── Context vs Signals ───────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Context vs Signals
      </Typography>
      <Typography variant="body1" paragraph>
        Both mechanisms share state across components, but they differ in scope
        and lifetime:
      </Typography>
      <CodeBlock language="jsx" code={CONTEXT_VS_SIGNALS} />
    </Box>

    {/* ── Predefined keys ──────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Built-in context keys
      </Typography>
      <Typography variant="body1" paragraph>
        The library provides predefined keys via{' '}
        <code>PortalContextKeys</code>:
      </Typography>
      <List sx={styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>PortalContextKeys.ModalRoot</code> — VisualElement target for modal portals.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>PortalContextKeys.TooltipRoot</code> — VisualElement target for tooltip portals.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>PortalContextKeys.OverlayRoot</code> — VisualElement target for overlay portals.</>} />
        </ListItem>
      </List>
    </Box>

    <Alert severity="info" sx={{ mt: 2 }}>
      <code>UseContext</code> does not consume a hook slot — it can technically
      be called conditionally. However, for consistency and readability, keep it
      in the setup code section alongside other hooks.
    </Alert>
  </Box>
)
