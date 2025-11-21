import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './GettingStartedPage.style'
import { INSTALL_URL, HELLO_WORLD_EDITOR } from './GettingStartedPage.example'

export const GettingStartedPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Getting Started
    </Typography>
    <Typography variant="body1" paragraph>
      Supported Unity versions: <strong>Unity 6.2+</strong>
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
    <CodeBlock language="tsx" code={INSTALL_URL} />
    <Typography variant="h5" component="h2" gutterBottom>
      Hello World (Editor)
    </Typography>
    <CodeBlock language="tsx" code={HELLO_WORLD_EDITOR} />
  </Box>
)

