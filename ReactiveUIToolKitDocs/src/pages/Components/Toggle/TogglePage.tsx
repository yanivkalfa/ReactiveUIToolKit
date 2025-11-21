import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './TogglePage.style'
import { TOGGLE_BASIC } from './TogglePage.example'

export const TogglePage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Toggle
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Toggle</code> wraps the UI Toolkit <code>Toggle</code> control using <code>ToggleProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={TOGGLE_BASIC} />
    </Box>
  </Box>
)

