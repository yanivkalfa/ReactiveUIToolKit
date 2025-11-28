import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './DifferencesPage.style'
import { STATE_COUNTER_EXAMPLE } from './DifferencesPage.example'

export const DifferencesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Different from React
    </Typography>

    <Typography variant="body1" paragraph>
      ReactiveUIToolKit feels familiar if you know React, but there are important differences in how
      rendering and scheduling behave when you are working in C# and Unity instead of JavaScript and
      the browser. This section focuses on the places where your mental model should be adjusted
      rather than re-explaining core concepts.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        State updates with UseState (parity)
      </Typography>
      <Typography variant="body1" paragraph>
        <code>Hooks.UseState</code> matches React&apos;s mental model: you get a value and a setter,
        and you can call the setter with either a value or a function of the previous value (for
        example <code>set(value)</code> or <code>set(prev =&gt; next)</code>).
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                The setter is a delegate (<code>StateSetter&lt;T&gt;</code>), not an instance
                method, but you call it just like a normal function.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                You can either call <code>set(value)</code> / <code>set(prev =&gt; next)</code>{' '}
                (React-style) or use the optional extension helpers{' '}
                <code>StateSetterExtensions.Set(value)</code> /{' '}
                <code>StateSetterExtensions.Set(prev =&gt; next)</code> if you prefer a fluent style.
              </>
            }
          />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={STATE_COUNTER_EXAMPLE} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Sync rendering vs React concurrent mode
      </Typography>
      <Typography variant="body1" paragraph>
        ReactiveUIToolKit&apos;s Fiber reconciler currently runs in a single, synchronous mode per
        Unity frame. There is no React 18-style concurrent rendering yet: no{' '}
        <code>startTransition</code>, no transition priorities, and no cooperative time-slicing of
        large trees.
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                All updates scheduled in a frame are processed synchronously; there is no partial
                rendering or preemption between high- and low-priority updates.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                This behaves like legacy React (pre-18) &quot;sync mode&quot;: your components and
                hooks logic are the same, but you should not expect concurrent features such as
                transitions or suspenseful background rendering.
              </>
            }
          />
        </ListItem>
      </List>
    </Box>
  </Box>
)
