import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './RouterPage.style'
import {
  ROUTER_EDITOR_EXAMPLE,
  ROUTER_LINKS_AND_NAV_EXAMPLE,
  ROUTER_RUNTIME_EXAMPLE,
  ROUTER_SPLIT_LAYOUT_EXAMPLE,
} from './RouterPage.example'

export const RouterPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Router
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit includes a lightweight, in-memory router inspired by React Router. It routes
      based on the current path and lets you nest routes and links inside your <code>VirtualNode</code>{' '}
      tree.
    </Typography>
    <Box>
      <Typography variant="h5" component="h3" gutterBottom>
        Core concepts
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Use <code>V.Router(...)</code> at the root of a subtree to set up routing context
                and history.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Use <code>V.Route(path, exact, element, children)</code> to match the current path
                and decide what to render.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Use <code>V.Link</code> and <code>RouterHooks.UseNavigate(replace)</code> to
                perform navigation from code or UI.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Use <code>RouterHooks.UseLocation()</code>, <code>RouterHooks.UseParams()</code>, and{' '}
                <code>RouterHooks.UseQuery()</code> to access path, parameters, and query-string
                values.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Typography variant="h5" component="h3" gutterBottom>
      Basic example
    </Typography>
    <Typography variant="body1" paragraph>
      The example below shows the same router tree hosted in an editor window and in a runtime
      function component. Inside the matched routes you can use <code>RouterHooks.UseLocation()</code>{' '}
      and <code>RouterHooks.UseParams()</code> to read the active path and parameters.
    </Typography>
    <CodeBlock
      language="tsx"
      codeEditor={ROUTER_EDITOR_EXAMPLE}
      codeRuntime={ROUTER_RUNTIME_EXAMPLE}
    />

    <Typography variant="h5" component="h3" gutterBottom>
      Navigation and history
    </Typography>
    <Typography variant="body1" paragraph>
      By default <code>V.Router</code> uses an in-memory history implementation. You can provide a
      custom <code>IRouterHistory</code> instance if you want to control how locations are stored or
      synchronized. Inside components, use <code>RouterHooks.UseNavigate()</code> to push or replace
      locations, and <code>RouterHooks.UseGo()</code> / <code>RouterHooks.UseCanGo()</code> to
      implement back/forward UI. You can also use <code>RouterHooks.UseBlocker()</code> to prevent
      navigation while a confirmation dialog is open.
    </Typography>

    <Typography variant="h5" component="h3" gutterBottom>
      Links and route data
    </Typography>
    <Typography variant="body1" paragraph>
      Use <code>V.Link</code> to render navigation buttons bound to specific paths. Inside routed
      components, use <code>RouterHooks.UseLocationInfo()</code> for the full location payload,
      <code>RouterHooks.UseParams()</code> for path parameters, <code>RouterHooks.UseQuery()</code>{' '}
      for query-string values, and <code>RouterHooks.UseNavigationState()</code> for any state
      object passed when navigating.
    </Typography>

    <Typography variant="h5" component="h3" gutterBottom>
      Links, params, query, and state (example)
    </Typography>
    <Typography variant="body1" paragraph>
      The example below demonstrates how to combine <code>V.Link</code>,{' '}
      <code>RouterHooks.UseNavigate()</code>, <code>RouterHooks.UseGo()</code>,{' '}
      <code>RouterHooks.UseParams()</code>, <code>RouterHooks.UseQuery()</code>, and{' '}
      <code>RouterHooks.UseNavigationState()</code> to build a small navigation bar that can move
      back and forth and read route data.
    </Typography>
    <CodeBlock language="tsx" code={ROUTER_LINKS_AND_NAV_EXAMPLE} />

    <Typography variant="h5" component="h3" gutterBottom>
      Split layouts with nested routes
    </Typography>
    <Typography variant="body1" paragraph>
      You can keep a single router history while nesting routes to act like “outlets”. Child routes
      may use relative paths (for example “profile”), which we resolve against the parent match via
      <code>RouterPath.Combine</code>. When you use a relative route, we automatically prefix it
      with the parent route’s path before matching. That means patterns like{' '}
      <code>:id/edit</code> work the same way they do in React Router—no need to repeat the parent
      prefix.
    </Typography>
    <Typography variant="body1" paragraph>
      The example below matches <code>/mainMenu/*</code>, renders a sidebar, and nests additional{' '}
      <code>V.Route</code> elements so the right-hand panel switches content as the path changes. The
      sidebar buttons call <code>RouterHooks.UseNavigate()</code> with relative targets via{' '}
      <code>RouterPath.Combine</code>, so everything stays in sync without spinning up another router.
    </Typography>
    <CodeBlock language="tsx" code={ROUTER_SPLIT_LAYOUT_EXAMPLE} />
  </Box>
)
