import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../Introduction/IntroductionPage.style'

const QUICK_SAMPLE = `component CounterCard {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text={$"Count: {count}"} />
      <Button text="+" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`

export const UitkxIntroductionPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ReactiveUIToolKit
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit is a React-like UI Toolkit runtime for Unity, and UITKX is its primary
      authoring language. You write function-style components in <code>.uitkx</code>, use hooks for
      state and effects, and let the toolkit reconcile the resulting tree onto Unity
      <code>VisualElement</code>s.
    </Typography>
    <Typography variant="body1" paragraph>
      The authored experience should feel markup-first. The underlying runtime is still C#, but the
      normal way to build UI is UITKX, not hand-built <code>V.*</code> trees.
    </Typography>

    <CodeBlock language="tsx" code={QUICK_SAMPLE} />

    <Typography variant="h5" component="h2" gutterBottom>
      Highlights
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Function-style UITKX components with hooks and typed props" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Reactive diffing and batched updates on top of UI Toolkit" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Router and Signals utilities that work naturally inside UITKX" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Generated C# output for production builds with no runtime codegen" />
      </ListItem>
    </List>
  </Box>
)
