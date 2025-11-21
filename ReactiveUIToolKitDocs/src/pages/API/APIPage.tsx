import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from './APIPage.style'

export const APIPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      API Reference
    </Typography>
    <Typography variant="body1" paragraph>
      This section gives a high-level map of the main namespaces and types you will use when working
      with ReactiveUIToolKit. Use it as a guide when you are looking for where a particular class
      (for example <code>ButtonProps</code>) lives.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Core
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Core.V</code> – static factory for building{' '}
                <code>VirtualNode</code> trees (for example <code>V.VisualElement</code>,{' '}
                <code>V.Label</code>, <code>V.Button</code>, <code>V.Router</code>,{' '}
                <code>V.TabView</code>).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Core.Hooks</code> – hook functions for function components, such
                as <code>UseState</code>, <code>UseReducer</code>, <code>UseEffect</code>,{' '}
                <code>UseMemo</code>, <code>UseSignal</code>, and context helpers.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Core.StateSetterExtensions</code> – helpers for working with
                state setters (for example <code>set.Set(value)</code> /{' '}
                <code>set.Set(prev =&gt; next)</code>).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Core.RootRenderer</code> – runtime component that mounts a{' '}
                <code>VirtualNode</code> tree into a <code>UIDocument</code> root.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Core.RenderScheduler</code> – runtime scheduler used by the
                reconciler to batch updates per frame.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props &amp; Styles
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Props.Typed</code> – typed props for UI Toolkit controls. Each
                control has a corresponding <code>*Props</code> class (for example{' '}
                <code>ButtonProps</code>, <code>LabelProps</code>, <code>ListViewProps</code>,{' '}
                <code>ScrollViewProps</code>).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Props.Typed.Style</code> – strongly typed wrapper around a style
                dictionary used by many props (<code>Style</code> is often passed as{' '}
                <code>props.Style</code>).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Props.Typed.StyleKeys</code> – constants used as keys inside{' '}
                <code>Style</code> (for example <code>StyleKeys.MarginTop</code>,{' '}
                <code>StyleKeys.FlexDirection</code>).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Most field and layout controls follow the same pattern:
                <code>V.FloatField(new FloatFieldProps &#123; ... &#125;)</code>,{' '}
                <code>V.ListView(new ListViewProps &#123; ... &#125;)</code>, and so on.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Router
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Router.RouterHooks</code> – hook helpers for routing:{' '}
                <code>UseRouter()</code>, <code>UseLocation()</code>, <code>UseLocationInfo()</code>
                , <code>UseParams()</code>, <code>UseQuery()</code>,{' '}
                <code>UseNavigationState()</code>, <code>UseNavigate()</code>, <code>UseGo()</code>
                , <code>UseCanGo()</code>, <code>UseBlocker()</code>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Router.IRouterHistory</code>, <code>MemoryHistory</code> – the
                history abstraction used by <code>V.Router</code>. You can supply your own history
                implementation by passing an <code>IRouterHistory</code> instance to{' '}
                <code>V.Router</code>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Router.RouterLocation</code>, <code>RouterPath</code>,{' '}
                <code>RouteMatch</code> – types that describe the current location, parsed path, and
                the result of route matching.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Signals
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Signals.Signals</code> – entry point for working with signals via{' '}
                <code>Signals.Get&lt;T&gt;(key, initialValue)</code> and{' '}
                <code>Signals.TryGet&lt;T&gt;(key, out signal)</code>.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Signals.Signal&lt;T&gt;</code> – concrete signal type with{' '}
                <code>Value</code>, <code>Subscribe(...)</code>, <code>Set(value)</code>, and{' '}
                <code>Dispatch(update)</code> methods.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Signals.SignalsRuntime</code> – bootstraps the runtime registry
                and hidden host GameObject. Call <code>SignalsRuntime.EnsureInitialized()</code> at
                startup if you are using signals outside of components.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Editor support
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.EditorSupport.EditorRootRendererUtility</code> – helper for
                mounting a <code>VirtualNode</code> tree into an EditorWindow{' '}
                <code>VisualElement</code>. Used from editor samples and your own tools.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.EditorSupport.EditorRenderScheduler</code> – scheduler used in
                the editor for batched updates.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Elements &amp; registry
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Elements.ElementRegistry</code> – maps element names (for example{' '}
                <code>"Button"</code>, <code>"ListView"</code>) to concrete adapters and is used by
                the reconciler when creating and updating UI Toolkit elements.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>ReactiveUITK.Elements.ElementRegistryProvider</code> – static helpers for
                obtaining the default registry used by both runtime and editor hosts.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>
  </Box>
)

