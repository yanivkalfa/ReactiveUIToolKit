import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../GettingStarted/GettingStartedPage.style'
import {
  UITKX_HELLO_WORLD_BOOTSTRAP,
  UITKX_HELLO_WORLD_COMPONENT,
  UITKX_INSTALL_URL,
} from './UitkxGettingStartedPage.example'

export const UitkxGettingStartedPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      UITKX Getting Started
    </Typography>
    <Typography variant="body1" paragraph>
      You write function-style <code>.uitkx</code> components and the source generator produces a
      complete C# class automatically — no boilerplate needed. Supported Unity versions:{' '}
      <strong>Unity 6.2+</strong>.
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
      A UITKX component contains setup code at the top and returns markup.
    </Typography>
    <CodeBlock language="tsx" code={UITKX_HELLO_WORLD_COMPONENT} />
    <Typography variant="body1" paragraph>
      On the next Unity compile the source generator emits a complete C# class
      (<code>HelloWorld.uitkx.g.cs</code>) with <code>namespace</code>,{' '}
      <code>public partial class</code>, and a full <code>Render()</code> method. You don't need to
      create any companion file for this to work.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      2. Mount it
    </Typography>
    <Typography variant="body1" paragraph>
      Mount the component at runtime with <code>RootRenderer</code> or in an editor window with{' '}
      <code>EditorRootRendererUtility</code>.
    </Typography>
    <CodeBlock language="tsx" code={UITKX_HELLO_WORLD_BOOTSTRAP} />

    <Typography variant="h5" component="h2" gutterBottom>
      Companion files (optional)
    </Typography>
    <Typography variant="body1" paragraph>
      The generator produces everything needed, but you can optionally add <code>.cs</code> files
      next to your <code>.uitkx</code> to share styles, types, or utilities across components. See
      the <strong>Companion Files</strong> page for naming conventions and examples.
    </Typography>
  </Box>
)
