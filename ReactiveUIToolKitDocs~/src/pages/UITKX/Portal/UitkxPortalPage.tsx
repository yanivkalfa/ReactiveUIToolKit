import type { FC } from 'react'
import { Alert, Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../GettingStarted/GettingStartedPage.style'
import { PORTAL_BASIC, PORTAL_CONTEXT_KEYS } from './UitkxPortalPage.example'

export const UitkxPortalPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Portal
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Portal</code> renders its children into a different{' '}
      <code>VisualElement</code> target outside the normal component hierarchy.
      This is useful for modals, tooltips, and overlays that need to visually
      escape their parent&apos;s clipping or stacking context.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Signature
    </Typography>
    <CodeBlock language="jsx" code={`VirtualNode V.Portal(
    VisualElement portalTargetElement,
    string key = null,
    params VirtualNode[] children
)`} />

    <Typography variant="h5" component="h2" gutterBottom>
      Basic usage
    </Typography>
    <Typography variant="body1" paragraph>
      Provide a <code>VisualElement</code> reference as the portal target.
      Children are rendered into that element instead of the component&apos;s
      own container.
    </Typography>
    <CodeBlock language="jsx" code={PORTAL_BASIC} />

    <Typography variant="h5" component="h2" gutterBottom>
      PortalContextKeys
    </Typography>
    <Typography variant="body1" paragraph>
      The library provides predefined context keys for well-known portal slots.
      Use <code>ProvideContext</code> to register a <code>VisualElement</code>{' '}
      target, then resolve it with <code>UseContext</code> in child components:
    </Typography>
    <CodeBlock language="jsx" code={PORTAL_CONTEXT_KEYS} />

    <List>
      <ListItem disablePadding>
        <ListItemText primary={<><code>PortalContextKeys.ModalRoot</code> — for modal dialogs and full-screen overlays.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><code>PortalContextKeys.TooltipRoot</code> — for floating tooltips.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><code>PortalContextKeys.OverlayRoot</code> — for generic overlay content.</>} />
      </ListItem>
    </List>

    <Alert severity="info" sx={{ mt: 2 }}>
      Portal children participate in the normal React-like lifecycle (hooks,
      effects, context) even though they render into a different part of the
      VisualElement tree.
    </Alert>
  </Box>
)
