import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './DifferencesPage.style'
import {
  STATE_COUNTER_EXAMPLE,
} from './DifferencesPage.example'

export const DifferencesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Different from React
    </Typography>

    <Typography variant="body1" paragraph>
      ReactiveUIToolKit feels familiar if you know React, but there are important differences in how
      state and updates behave when you are working in C# and Unity instead of JavaScript and the
      browser. This section focuses on the places where your mental model should be adjusted rather
      than re-explaining core concepts.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        State updates with UseState
      </Typography>
      <Typography variant="body1" paragraph>
        <code>Hooks.UseState</code> returns a value and a state setter delegate. The setter is a
        function that returns the new value and works together with{' '}
        <code>StateSetterExtensions.Set</code>. Instead of calling <code>set(value)</code> or{' '}
        <code>set(prev =&gt; next)</code> like in React, you call{' '}
        <code>set.Set(value)</code> or <code>set.Set(prev =&gt; next)</code>.
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>StateSetter&lt;T&gt;</code> instances are delegates (not methods on an object).
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                Use <code>StateSetterExtensions.Set(value)</code> or{' '}
                <code>Set(prev =&gt; next)</code> instead of calling <code>set(value)</code> /{' '}
                <code>set(prev =&gt; next)</code> like in React.
              </>
            }
          />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={STATE_COUNTER_EXAMPLE} />
    </Box>
  </Box>
)
