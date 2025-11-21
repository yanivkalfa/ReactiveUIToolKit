import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './ProgressBarPage.style'
import { PROGRESS_BAR_BASIC } from './ProgressBarPage.example'

export const ProgressBarPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ProgressBar
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ProgressBar</code> renders a UI Toolkit <code>ProgressBar</code> using{' '}
      <code>ProgressBarProps</code>. It is typically driven by state changes elsewhere in your UI.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={PROGRESS_BAR_BASIC} />
    </Box>
  </Box>
)

