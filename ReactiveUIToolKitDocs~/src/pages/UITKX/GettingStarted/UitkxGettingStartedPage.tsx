import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../GettingStarted/GettingStartedPage.style'
import {
  UITKX_HELLO_WORLD_BOOTSTRAP,
  UITKX_HELLO_WORLD_COMPONENT,
  UITKX_HELLO_WORLD_PARTIAL,
  UITKX_INSTALL_URL,
} from './UitkxGettingStartedPage.example'

export const UitkxGettingStartedPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      UITKX Getting Started
    </Typography>
    <Typography variant="body1" paragraph>
      UITKX is the primary authoring model for ReactiveUIToolKit. You write function-style{' '}
      <code>.uitkx</code> components, keep a tiny companion partial class for the generated output,
      and mount the result through the normal ReactiveUITK renderer.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Install via Unity Package Manager
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Open Package Manager in Unity." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Add package from Git URL:" />
      </ListItem>
    </List>
    <CodeBlock language="tsx" code={UITKX_INSTALL_URL} />

    <Typography variant="h5" component="h2" gutterBottom>
      1. Create a UITKX component
    </Typography>
    <Typography variant="body1" paragraph>
      A function-style UITKX component contains setup code at the top and returns markup. This is
      the default shape new users should learn.
    </Typography>
    <CodeBlock language="tsx" code={UITKX_HELLO_WORLD_COMPONENT} />

    <Typography variant="h5" component="h2" gutterBottom>
      2. Add the companion partial
    </Typography>
    <Typography variant="body1" paragraph>
      The generator emits <code>Render()</code> into the matching partial class on the next Unity
      compile.
    </Typography>
    <CodeBlock language="tsx" code={UITKX_HELLO_WORLD_PARTIAL} />

    <Typography variant="h5" component="h2" gutterBottom>
      3. Mount it
    </Typography>
    <Typography variant="body1" paragraph>
      Runtime mounting still uses <code>RootRenderer</code> and <code>V.Func(...)</code>, but the
      authored UI stays in UITKX.
    </Typography>
    <CodeBlock language="tsx" code={UITKX_HELLO_WORLD_BOOTSTRAP} />
  </Box>
)
