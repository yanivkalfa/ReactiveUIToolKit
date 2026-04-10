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
      ReactiveUIToolKit includes a lightweight, in-memory router inspired by React Router. Routing
      is authored directly in markup — you compose <code>{'<Router>'}</code>,{' '}
      <code>{'<Route>'}</code>, links, and routed child components as part of the returned UI tree.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Core concepts
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<Router>'}</code> establishes routing context and history for the subtree.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<Route>'}</code> matches the current path and decides what to render.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<RouterNavLink>'}</code> and <code>RouterHooks.UseNavigate()</code> perform navigation from markup or setup code.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseLocation()</code>, <code>RouterHooks.UseParams()</code>, and <code>RouterHooks.UseQuery()</code> access path, parameters, and query-string values.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic example
      </Typography>
      <Typography variant="body1" paragraph>
        The example below shows declarative route composition in markup together with imperative
        setup-code helpers through <code>RouterHooks</code>.
      </Typography>
      <CodeBlock language="jsx" code={UITKX_ROUTER_EXAMPLE} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Navigation and history
      </Typography>
      <Typography variant="body1" paragraph>
        By default <code>{'<Router>'}</code> uses an in-memory history implementation. You can
        provide a custom <code>IRouterHistory</code> instance if you want to control how locations
        are stored or synchronized. Inside components, use <code>RouterHooks.UseNavigate()</code>{' '}
        to push or replace locations, and <code>RouterHooks.UseGo()</code> /{' '}
        <code>RouterHooks.UseCanGo()</code> to implement back/forward UI. Use{' '}
        <code>RouterHooks.UseBlocker()</code> to prevent navigation while a confirmation dialog
        is open.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Reading route data
      </Typography>
      <Typography variant="body1" paragraph>
        Inside routed components, use these hooks to access the current routing state:
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseLocationInfo()</code> — full location payload (path, query, state).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseParams()</code> — path parameters extracted from the route template.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseQuery()</code> — parsed query-string key/value pairs.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseNavigationState()</code> — arbitrary state object passed during navigation.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseRouteMatch()</code> — the current <code>RouteMatch</code> object with the matched path, pattern, and resolved parameters.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseNavigationBase()</code> — the base path for resolving relative navigations in nested route trees.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Link vs RouterNavLink
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'<RouterNavLink>'}</code> is the markup element for in-app navigation. It renders a
        clickable label that calls <code>UseNavigate()</code> under the hood.{' '}
        <code>V.Link(to, label, replace, style, key, state)</code> is the equivalent runtime C# factory.
        Both produce the same element — use whichever matches your syntax preference.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Nested routes
      </Typography>
      <Typography variant="body1" paragraph>
        You can keep a single router history while nesting routes to act like outlets. Child routes
        may use relative paths (for example <code>"profile"</code>), and they are automatically
        resolved against the parent match. Patterns like <code>:id/edit</code> work the same way
        they do in React Router — no need to repeat the parent prefix.
      </Typography>
    </Box>
  </Box>
)
