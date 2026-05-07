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
          <ListItemText primary={<><code>{'<Router>'}</code> establishes routing context and history for the subtree. Optional <code>basename</code> attribute prefixes every URL.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<Routes>'}</code> ranks its child <code>{'<Route>'}</code>s and renders the single best match (RR-v6 behavior, first-match-wins by score).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<Route>'}</code> matches the current path and decides what to render. Supports <code>index</code>, <code>caseSensitive</code>, and layout-route composition with child <code>{'<Route>'}</code>s + <code>{'<Outlet/>'}</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<Outlet/>'}</code> is the render-slot inside a layout route — descendants render here.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<NavLink>'}</code> (and the legacy <code>{'<RouterNavLink>'}</code>) renders a navigation link with active-state styling.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<Navigate to=...>'}</code> performs a declarative redirect (defaults to <code>replace=true</code>).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks</code> expose imperative navigation, location data, search params, blockers, and breadcrumbs from setup code.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic example
      </Typography>
      <Typography variant="body1" paragraph>
        The example below shows the full RR-v6-parity surface: <code>{'<Router basename>'}</code>,
        ranked <code>{'<Routes>'}</code>, an <code>index</code> route, a layout route with{' '}
        <code>{'<Outlet/>'}</code>, a declarative redirect, and the new search-params /
        breadcrumb hooks.
      </Typography>
      <CodeBlock language="jsx" code={UITKX_ROUTER_EXAMPLE} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Routes — ranked first-match-wins
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'<Routes>'}</code> is the deterministic selector. It walks its child{' '}
        <code>{'<Route>'}</code> declarations, scores each one with the same algorithm React
        Router uses (<code>staticSegmentValue=10</code>, <code>dynamicSegmentValue=3</code>,{' '}
        <code>splatPenalty=-2</code>, <code>indexRouteValue=2</code>,{' '}
        <code>emptySegmentValue=1</code>), and renders only the highest-ranked match. Ties break
        by declaration order. Use <code>{'<Routes>'}</code> whenever you have more than one route
        that could conceivably match the same path — it eliminates the "two routes both matched"
        foot-gun of bare <code>{'<Route>'}</code>s.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Layout routes and Outlet
      </Typography>
      <Typography variant="body1" paragraph>
        A <code>{'<Route>'}</code> with both an <code>element</code> and child{' '}
        <code>{'<Route>'}</code>s becomes a <em>layout route</em>. Its <code>element</code>{' '}
        renders as a wrapper, and the matched child renders wherever you place an{' '}
        <code>{'<Outlet/>'}</code> inside that element. This mirrors React Router v6 exactly —
        the wrapper sees the matched child via context, and there's no prop-drilling.
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'<Outlet context={value}>'}</code> exposes a typed value to descendants via{' '}
        <code>{'RouterHooks.UseOutletContext<T>()'}</code>, the same way RR's{' '}
        <code>useOutletContext</code> works.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Index and case-sensitive routes
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>index={'{true}'}</code> — the route matches the parent path exactly (no extra segment). Setting both <code>index</code> and <code>path</code> on the same Route throws an actionable <code>InvalidOperationException</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>caseSensitive={'{true}'}</code> — opt-in to case-sensitive segment matching for that Route. The default remains case-insensitive for back-compat.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        NavLink — active-state links
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'<NavLink to="/about" label="About">'}</code> behaves like a regular link but
        applies <code>activeStyle</code> when its target matches the current location. The match
        rules mirror RR's <code>NavLink</code> exactly — including the special case where{' '}
        <code>to="/"</code> is only "active" when the current path is exactly <code>"/"</code>{' '}
        (otherwise every page would highlight Home). Use <code>end={'{true}'}</code> to require an
        exact match for non-root paths and <code>caseSensitive={'{true}'}</code> for case-sensitive
        comparison.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Navigate — declarative redirects
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'<Navigate to="/welcome">'}</code> performs a redirect at render time. It defaults
        to <code>replace=true</code> so redirects don't grow the history stack — perfect for{' '}
        <code>{'<Route path="/" element={<Navigate to="/dashboard"/>}/>'}</code> patterns. Pass{' '}
        <code>state</code> to forward navigation state along.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        basename
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'<Router basename="/app">'}</code> tells the router that <code>/app</code> is the
        application root. Inbound locations have the prefix stripped before matching, and outbound
        navigations (<code>UseNavigate</code>, <code>UseResolvedPath</code>) re-attach it. Useful
        when an app is mounted under a path segment.
      </Typography>
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
        <code>RouterHooks.UseBlocker()</code> (or the convenience wrapper{' '}
        <code>RouterHooks.UsePrompt(when, message)</code>) to prevent navigation while a
        confirmation dialog is open.
      </Typography>
      <Typography variant="body1" paragraph>
        Nesting two <code>{'<Router>'}</code> elements inside the same tree is a hard error
        (<code>InvalidOperationException</code>) — mirrors RR's{' '}
        <code>invariant(!useInRouterContext())</code>. Use a single root <code>{'<Router>'}</code>{' '}
        and compose <code>{'<Route>'}</code>s underneath it.
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
          <ListItemText primary={<><code>RouterHooks.UseQuery()</code> — parsed query-string key/value pairs (read-only).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseSearchParams()</code> — <code>(query, set)</code> tuple. The setter preserves the path component and replaces only the query (RR's <code>useSearchParams</code> equivalent).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseNavigationState()</code> — arbitrary state object passed during navigation.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseRouteMatch()</code> — the current <code>RouteMatch</code> object with the matched path, pattern, and resolved parameters.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseMatches()</code> — the ordered chain of <code>RouteMatch</code> from root → current. Useful for breadcrumbs, debug overlays, and analytics.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseNavigationBase()</code> — the base path for resolving relative navigations in nested route trees.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseResolvedPath(to)</code> — pure path resolver against the current navigation base. Same algorithm <code>UseNavigate</code> uses internally.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'RouterHooks.UseOutletContext<T>()'}</code> — typed accessor for the value passed via <code>{'<Outlet context=...>'}</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>RouterHooks.UseNavigate(NavigateOptions)</code> — overload returning a path-only navigator pre-bound to <code>Replace</code>/<code>State</code>. The original <code>UseNavigate(bool replace = false)</code> remains for back-compat.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Link vs NavLink
      </Typography>
      <Typography variant="body1" paragraph>
        <code>{'<NavLink>'}</code> is the markup element for in-app navigation with active-state
        styling. The legacy <code>{'<RouterNavLink>'}</code> alias remains as a synonym.{' '}
        <code>V.Link(to, label, replace, style, key, state)</code> is the equivalent runtime C#
        factory for plain links. Use <code>{'<Navigate>'}</code> for declarative redirects (no
        click required).
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Nested routes
      </Typography>
      <Typography variant="body1" paragraph>
        Layout routes are the recommended way to nest. A parent <code>{'<Route>'}</code> with both{' '}
        <code>element</code> and child <code>{'<Route>'}</code>s renders its element as a wrapper
        and projects the matched child into the descendant <code>{'<Outlet/>'}</code>. Child
        routes may use relative paths (for example <code>"profile"</code> or{' '}
        <code>":id/edit"</code>) and are automatically resolved against the parent match — no
        need to repeat the parent prefix. This matches React Router v6 exactly.
      </Typography>
    </Box>
  </Box>
)
