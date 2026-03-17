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
      UITKX borrows React’s component-and-hooks mental model, but it runs on Unity UI Toolkit and a
      C# runtime. The biggest difference is that your authored code is markup-first, while the
      underlying runtime is still constrained by Unity’s VisualElement system, scheduling model,
      and C# semantics.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        State updates
      </Typography>
      <Typography variant="body1" paragraph>
        <code>useState</code> behaves like React’s <code>useState</code>. You call the setter
        directly with either a value or an updater function, and UITKX lowers that into the runtime
        hook implementation for you.
      </Typography>
      <CodeBlock language="tsx" code={UITKX_STATE_COUNTER_EXAMPLE} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Rendering model
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="ReactiveUITK’s fiber can schedule work asynchronously when a scheduler is present, including sliced render work and deferred passive effects." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="That does not mean UITKX promises a one-to-one clone of React’s full concurrent feature surface; the scheduler still operates inside Unity’s runtime constraints." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="The authored syntax is JSX-like, but it lowers into ReactiveUITK’s own runtime representation instead of a browser DOM model." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Interop with Unity controls, styles, and events is a first-class constraint, so some APIs deliberately differ from browser React conventions." />
        </ListItem>
      </List>
    </Box>
  </Box>
)
