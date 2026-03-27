import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from '../../API/APIPage.style'

export const UitkxAPIPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      API Reference
    </Typography>
    <Typography variant="body1" paragraph>
      A high-level map of the main namespaces and types. Use it when you need to find where a
      particular class (for example <code>ButtonProps</code>) lives.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Core
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Core.V</code> — static factory for building <code>VirtualNode</code> trees (<code>V.VisualElement</code>, <code>V.Button</code>, <code>V.Label</code>, <code>V.Router</code>, etc.).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Core.Hooks</code> — hook functions: <code>UseState</code>, <code>UseReducer</code>, <code>UseEffect</code>, <code>UseMemo</code>, <code>UseSignal</code>, and context helpers.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Core.StateSetterExtensions</code> — fluent helpers for state setters (<code>set.Set(value)</code> / <code>{'set.Set(prev => next)'}</code>).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Core.RootRenderer</code> — runtime component that mounts a <code>VirtualNode</code> tree into a <code>UIDocument</code> root.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Core.RenderScheduler</code> — runtime scheduler used by the reconciler to batch updates per frame.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props & Styles
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Props.Typed</code> — typed props for UI Toolkit controls. Each control has a <code>*Props</code> class (<code>ButtonProps</code>, <code>LabelProps</code>, <code>ListViewProps</code>, <code>ScrollViewProps</code>, etc.).</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Props.Typed.Style</code> — strongly typed wrapper around a style dictionary used by many props.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Props.Typed.StyleKeys</code> — constants used as keys inside <code>Style</code> (<code>StyleKeys.MarginTop</code>, <code>StyleKeys.FlexDirection</code>, etc.).</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Router
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Router.RouterHooks</code> — hook helpers: <code>UseRouter()</code>, <code>UseLocation()</code>, <code>UseLocationInfo()</code>, <code>UseParams()</code>, <code>UseQuery()</code>, <code>UseNavigationState()</code>, <code>UseNavigate()</code>, <code>UseGo()</code>, <code>UseCanGo()</code>, <code>UseBlocker()</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Router.IRouterHistory</code>, <code>MemoryHistory</code> — the history abstraction. Supply a custom <code>IRouterHistory</code> to control how locations are stored.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Router.RouterLocation</code>, <code>RouterPath</code>, <code>RouteMatch</code> — types describing the current location, parsed path, and route matching result.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Signals
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Signals.Signals</code> — entry point: <code>{'Signals.Get<T>(key, initialValue)'}</code> and <code>{'Signals.TryGet<T>(key, out signal)'}</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'ReactiveUITK.Signals.Signal<T>'}</code> — concrete signal type with <code>Value</code>, <code>Subscribe(...)</code>, <code>Set(value)</code>, and <code>Dispatch(update)</code>.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Signals.SignalsRuntime</code> — bootstraps the runtime registry. Call <code>SignalsRuntime.EnsureInitialized()</code> at startup if using signals outside components.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Animation
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>Hooks.UseAnimate(tracks)</code> — starts <code>AnimateTrack</code> definitions on the component's VisualElement container. Plays on dependency change, cleans up on unmount.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>Hooks.UseTweenFloat(from, to, duration, ease, delay, onUpdate, onComplete)</code> — tweens a float value with easing. Integrates with component lifecycle and cancels on unmount.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Core.Animation.AnimateTrack</code> — helpers for creating animation tracks (property, size, color, etc.).</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Safe Area
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>Hooks.UseSafeArea(tolerance?)</code> — returns <code>SafeAreaInsets</code> (top, bottom, left, right) based on <code>Screen.safeArea</code>. Re-renders when insets change.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'<VisualElementSafe>'}</code> — a drop-in safe-area-aware container that automatically applies padding from <code>SafeAreaInsets</code>.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Editor Support
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.EditorSupport.EditorRootRendererUtility</code> — mounts a VirtualNode tree into an EditorWindow VisualElement.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.EditorSupport.EditorRenderScheduler</code> — scheduler for batched updates in the editor.</>} />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Elements & Registry
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Elements.ElementRegistry</code> — maps element names ("Button", "ListView") to concrete adapters used by the reconciler.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>ReactiveUITK.Elements.ElementRegistryProvider</code> — static helpers for obtaining the default registry.</>} />
        </ListItem>
      </List>
    </Box>
  </Box>
)
