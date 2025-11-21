import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './ComponentsPage.style'
import { COMPONENTS_BUTTON } from './ComponentsPage.example'

export const ComponentsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Components
    </Typography>
    <Typography variant="body1" paragraph>
      UI Toolkit controls wrapped with typed props and adapters. Use <code>V.*</code> helpers to
      create virtual nodes.
    </Typography>
    <CodeBlock language="tsx" codeRuntime={COMPONENTS_BUTTON} />
  </Box>
)
