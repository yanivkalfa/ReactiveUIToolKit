import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../Differences/DifferencesPage.style'
import { UITKX_STATE_COUNTER_EXAMPLE } from './UitkxDifferencesPage.example'

export const UitkxDifferencesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Different from React
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit borrows React's component-and-hooks mental model, but it runs on Unity UI
      Toolkit with a C# runtime. This section covers the places where your mental model should be
      adjusted rather than re-explaining core concepts.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        State updates
      </Typography>
      <Typography variant="body1" paragraph>
        <code>useState</code> matches React's mental model: you get a value and a setter, and you
        call the setter with either a value or an updater function (for example{' '}
        <code>set(value)</code> or <code>{'set(prev => next)'}</code>).
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary={<>The setter is a delegate (<code>{'StateSetter<T>'}</code>), not an instance method, but you call it like a normal function.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<>You can also use the optional extension helpers <code>StateSetterExtensions.Set(value)</code> / <code>{'StateSetterExtensions.Set(prev => next)'}</code> for a fluent style.</>} />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary={<><code>{'StateSetterExtensions.ToValueAction<T>()'}</code> converts a setter into an <code>{'Action<T>'}</code>, useful for binding directly to <code>onChange</code> events (e.g. <code>{'onChange={setName.ToValueAction()}'}</code>).</>} />
        </ListItem>
      </List>
      <CodeBlock language="jsx" code={UITKX_STATE_COUNTER_EXAMPLE} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Rendering model
      </Typography>
      <Typography variant="body1" paragraph>
        The Fiber reconciler currently runs in synchronous mode per Unity frame. There is no React
        18-style concurrent rendering: no <code>startTransition</code>, no transition priorities,
        and no cooperative time-slicing of large trees.
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="All updates scheduled in a frame are processed synchronously; there is no partial rendering or preemption between high- and low-priority updates." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="The scheduler can defer passive effects and slice render work when present, but it operates within Unity's runtime constraints — not as a full concurrent feature surface." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Interop with Unity controls, styles, and events is a first-class constraint, so some APIs deliberately differ from browser React conventions." />
        </ListItem>
      </List>
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        UseCallback returns Func&lt;T&gt;
      </Typography>
      <Typography variant="body1" paragraph>
        React's <code>useCallback</code> returns the same function type you pass in.
        In ReactiveUIToolKit, <code>{'UseCallback<T>'}</code> always returns{' '}
        <code>{'Func<T>'}</code> — a parameterless delegate that returns <code>T</code>.
        If you need a stable <code>Action</code> or <code>{'Action<T>'}</code>, use{' '}
        <code>UseStableCallback</code>, <code>{'UseStableAction<T>'}</code>, or{' '}
        <code>{'UseStableFunc<T>'}</code> instead.
      </Typography>
    </Box>
  </Box>
)
