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
  HOOKS_CONTEXT_EXAMPLE,
  HOOKS_DEFERRED_EXAMPLE,
  HOOKS_DEPENDENCY_RULES,
  HOOKS_IMPERATIVE_EXAMPLE,
  HOOKS_USECALLBACK_EXAMPLE,
  HOOKS_USEEFFECT_EXAMPLE,
  HOOKS_USELAYOUTEFFECT_EXAMPLE,
  HOOKS_USEMEMO_EXAMPLE,
  HOOKS_USEREDUCER_EXAMPLE,
  HOOKS_USEREF_EXAMPLE,
  HOOKS_USESTATE_EXAMPLE,
  HOOKS_STABLE_EXAMPLE,
} from './HooksGuidePage.example'

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 2 },
  list: { pl: 2 },
} as const

export const HooksGuidePage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Hooks Guide
    </Typography>
    <Typography variant="body1" paragraph>
      Hooks let you use state, effects, context, and other framework features
      inside function-style <code>.uitkx</code> components. This page covers
      every hook in depth, with patterns and examples.
    </Typography>

    <Alert severity="info" sx={{ mt: 1 }}>
      Hook calls must be <strong>unconditional</strong> and at the{' '}
      <strong>top level</strong> of your component&apos;s setup code (before{' '}
      <code>return</code>). Never call hooks inside <code>@if</code>,{' '}
      <code>@for</code>, or other control blocks.
    </Alert>

    {/* ── UseState ─────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseState<T>'}
      </Typography>
      <CodeBlock language="jsx" code={`(T value, StateSetter<T> set) useState<T>(T initial = default)`} />
      <Typography variant="body1" paragraph>
        Returns the current value and a setter delegate. The setter accepts
        either a direct value or a functional updater{' '}
        <code>{'Func<T, T>'}</code> (via the implicit{' '}
        <code>{'StateUpdate<T>'}</code> conversion). Functional updaters are
        safer when batching multiple updates because they always read the latest
        state.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_USESTATE_EXAMPLE} />
    </Box>

    {/* ── UseReducer ───────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseReducer<TState, TAction>'}
      </Typography>
      <CodeBlock language="jsx" code={`(TState state, Action<TAction> dispatch) useReducer<TState, TAction>(
    Func<TState, TAction, TState> reducer,
    TState initialState
)`} />
      <Typography variant="body1" paragraph>
        Preferred over <code>UseState</code> when state transitions depend on
        the previous state and an action. The reducer is a pure function:{' '}
        <code>(state, action) =&gt; newState</code>.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_USEREDUCER_EXAMPLE} />
    </Box>

    {/* ── UseEffect ────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        UseEffect
      </Typography>
      <CodeBlock language="jsx" code={`void useEffect(Func<Action> effectFactory, params object[] dependencies)`} />
      <Typography variant="body1" paragraph>
        Runs an effect after the component renders. The factory returns an
        optional cleanup <code>Action</code> that runs before the next effect
        or on unmount. Return <code>null</code> if no cleanup is needed.
      </Typography>
      <Typography variant="h6" gutterBottom>
        Dependency rules
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_DEPENDENCY_RULES} />
      <CodeBlock language="jsx" code={HOOKS_USEEFFECT_EXAMPLE} />
    </Box>

    {/* ── UseLayoutEffect ──────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        UseLayoutEffect
      </Typography>
      <CodeBlock language="jsx" code={`void useLayoutEffect(Func<Action> effectFactory, params object[] dependencies)`} />
      <Typography variant="body1" paragraph>
        Identical to <code>UseEffect</code> but fires <strong>synchronously</strong>{' '}
        before the frame paints. Use it when you need to read layout
        measurements or mutate the DOM before the user sees the frame.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_USELAYOUTEFFECT_EXAMPLE} />
    </Box>

    {/* ── UseMemo ──────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseMemo<T>'}
      </Typography>
      <CodeBlock language="jsx" code={`T useMemo<T>(Func<T> factory, params object[] dependencies)`} />
      <Typography variant="body1" paragraph>
        Returns a memoised value. The factory re-runs only when a dependency
        changes. Use it to avoid recomputing expensive derived data on every
        render.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_USEMEMO_EXAMPLE} />
    </Box>

    {/* ── UseCallback ──────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseCallback<T>'}
      </Typography>
      <CodeBlock language="jsx" code={`Func<T> useCallback<T>(Func<T> callback, params object[] dependencies)`} />
      <Alert severity="warning" sx={{ mb: 1 }}>
        Unlike React&apos;s <code>useCallback</code>, this hook returns{' '}
        <code>{'Func<T>'}</code> (not the same delegate type you passed in).
        If you need a stable <code>Action</code> or <code>{'Action<T>'}</code>,
        use <code>UseStableCallback</code> / <code>UseStableAction</code>{' '}
        instead.
      </Alert>
      <Typography variant="body1" paragraph>
        Returns a stable <code>{'Func<T>'}</code> whose identity only changes
        when a dependency changes. Useful for passing callbacks to child
        components without triggering unnecessary re-renders.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_USECALLBACK_EXAMPLE} />
    </Box>

    {/* ── UseRef ───────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseRef<T> & element ref'}
      </Typography>
      <CodeBlock language="jsx" code={`Ref<T> useRef<T>(T initial = default)   // mutable value container
VisualElement useRef()                   // element ref`} />
      <List sx={styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'Ref<T>'}</code> — a mutable container with a <code>.Current</code> property. Persists across renders. Changing <code>.Current</code> does <strong>not</strong> trigger a re-render.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>The parameterless <code>useRef()</code> overload returns a <code>VisualElement</code> reference. Attach it via the <code>ref</code> prop to read the underlying element.</>} />
        </ListItem>
      </List>
      <CodeBlock language="jsx" code={HOOKS_USEREF_EXAMPLE} />
    </Box>

    {/* ── Context ──────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseContext<T> & ProvideContext'}
      </Typography>
      <CodeBlock language="jsx" code={`T useContext<T>(string key)
void provideContext<T>(string key, T value)
void provideContext(string key, object value)   // untyped overload`} />
      <Typography variant="body1" paragraph>
        Context lets you pass data down the component tree without threading
        props through every level. <code>ProvideContext</code> makes a value
        available to all descendants; <code>UseContext</code> reads it.
      </Typography>
      <List sx={styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Context values are keyed by string. Use descriptive keys to avoid collisions." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Nested providers shadow outer providers for the same key." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="When a provided value changes, all consumers in the subtree automatically re-render." />
        </ListItem>
      </List>
      <CodeBlock language="jsx" code={HOOKS_CONTEXT_EXAMPLE} />
      <Alert severity="info" sx={{ mt: 1 }}>
        See the dedicated <strong>Context API</strong> page for advanced
        patterns (shadowing, performance, when to prefer Signals).
      </Alert>
    </Box>

    {/* ── UseDeferredValue ─────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseDeferredValue<T>'}
      </Typography>
      <CodeBlock language="jsx" code={`T useDeferredValue<T>(T value, params object[] dependencies)`} />
      <Typography variant="body1" paragraph>
        Returns a copy of <code>value</code> that may lag behind the latest
        render. This lets urgent updates (like typing) render immediately while
        expensive derived work (like filtering a large list) runs at lower
        priority.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_DEFERRED_EXAMPLE} />
    </Box>

    {/* ── UseImperativeHandle ──────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        {'UseImperativeHandle<THandle>'}
      </Typography>
      <CodeBlock language="jsx" code={`THandle useImperativeHandle<THandle>(
    Func<THandle> factory,
    params object[] dependencies
) where THandle : class`} />
      <Typography variant="body1" paragraph>
        Exposes an imperative API from a child component to its parent via the
        ref system. The factory creates the handle object; it recalculates when
        dependencies change.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_IMPERATIVE_EXAMPLE} />
    </Box>

    {/* ── Stable function helpers ──────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Stable function helpers
      </Typography>
      <CodeBlock language="jsx" code={`Func<T> useStableFunc<T>(Func<T> function)
Action<T> useStableAction<T>(Action<T> action)
Action useStableCallback(Action callback)`} />
      <Typography variant="body1" paragraph>
        These hooks return a wrapper whose identity <strong>never changes</strong>{' '}
        across renders. The wrapped delegate always calls through to the latest
        closure. Use them for event handlers passed to child components or
        native UI Toolkit callbacks.
      </Typography>
      <CodeBlock language="jsx" code={HOOKS_STABLE_EXAMPLE} />
    </Box>

    {/* ── Configuration properties ─────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Hook configuration
      </Typography>
      <Typography variant="body1" paragraph>
        The <code>Hooks</code> class exposes static properties that control
        runtime validation:
      </Typography>
      <List sx={styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={<><code>Hooks.EnableHookValidation</code> — validates that hooks are called in the same order every render (<code>true</code> by default in Editor).</>}
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={<><code>Hooks.EnableStrictDiagnostics</code> — enables additional runtime checks and warnings.</>}
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={<><code>Hooks.EnableHookAutoRealign</code> — attempts to auto-correct misaligned hook indices (useful during HMR).</>}
          />
        </ListItem>
      </List>
    </Box>
  </Box>
)
