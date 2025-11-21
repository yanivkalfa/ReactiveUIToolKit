import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './ToolbarPage.style'
import { TOOLBAR_BASIC } from './ToolbarPage.example'

export const ToolbarPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Toolbar
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Toolbar</code> and related helpers (<code>V.ToolbarButton</code>,{' '}
      <code>V.ToolbarToggle</code>, <code>V.ToolbarMenu</code>, etc.) wrap the UI Toolkit editor
      toolbar elements using the <code>ToolbarProps</code> family.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage (Editor)
      </Typography>
      <CodeBlock language="tsx" code={TOOLBAR_BASIC} />
    </Box>
  </Box>
)

