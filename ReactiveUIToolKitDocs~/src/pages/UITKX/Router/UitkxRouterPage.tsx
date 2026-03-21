import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../Router/RouterPage.style'
import { UITKX_ROUTER_EXAMPLE } from './UitkxRouterPage.example'

export const UitkxRouterPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Router
    </Typography>
    <Typography variant="body1" paragraph>
      In UITKX, routing is authored directly in markup. You compose <code>&lt;Router&gt;</code>,
      <code>&lt;Route&gt;</code>, links, and routed child components as part of the same returned UI
      tree.
    </Typography>

    <List sx={Styles.list}>
      <ListItem disablePadding>
        <ListItemText primary={<><code>&lt;Router&gt;</code> establishes routing context for the subtree.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><code>&lt;Route&gt;</code> matches paths and can render elements or child component trees.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><code>RouterHooks</code> stay in setup code for imperative navigation, history control, params, query values, and navigation state.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><code>RouterHooks.UseNavigate()</code> pushes or replaces locations, while <code>UseGo()</code> and <code>UseCanGo()</code> drive back/forward UI.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><code>RouterHooks.UseLocationInfo()</code>, <code>UseParams()</code>, <code>UseQuery()</code>, and <code>UseNavigationState()</code> expose the active routed data.</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><code>RouterHooks.UseBlocker()</code> lets you intercept transitions when a screen has unsaved or guarded state.</>} />
      </ListItem>
    </List>

    <Typography variant="body1" paragraph>
      The example below shows both styles together: declarative route composition in markup, and
      imperative setup-code helpers through <code>RouterHooks</code> for navigation and route data.
    </Typography>

    <CodeBlock language="tsx" code={UITKX_ROUTER_EXAMPLE} />
  </Box>
)
