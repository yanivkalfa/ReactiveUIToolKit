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
        delegate (<code>StateSetter&lt;T&gt;</code>) that you can invoke directly, just like
        React&apos;s <code>setState</code> �?" for example <code>set(value)</code> or{' '}
        <code>set(prev =&gt; next)</code>. There is also an optional helper{' '}
        <code>StateSetterExtensions.Set</code> if you prefer method syntax (
        <code>set.Set(value)</code> / <code>set.Set(prev =&gt; next)</code>).
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                <code>StateSetter&lt;T&gt;</code> instances are delegates (function values), not
                instance methods, but you call them with the same syntax as a normal function.
              </>
            }
          />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText
            primary={
              <>
                You can either call <code>set(value)</code> / <code>set(prev =&gt; next)</code>{' '}
                (React-style) or use the extension helpers{' '}
                <code>StateSetterExtensions.Set(value)</code> /{' '}
                <code>StateSetterExtensions.Set(prev =&gt; next)</code> if you prefer a fluent style.
              </>
            }
          />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={STATE_COUNTER_EXAMPLE} />
    </Box>
  </Box>
)
