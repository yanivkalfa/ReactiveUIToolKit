import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './RectIntFieldPage.style'
import { RECT_INT_FIELD_BASIC } from './RectIntFieldPage.example'

export const RectIntFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      RectIntField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.RectIntField</code> wraps the UI Toolkit <code>RectIntField</code> control using{' '}
      <code>RectIntFieldProps</code>. It is available in both runtime and editor UIs.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={RECT_INT_FIELD_BASIC} />
    </Box>
  </Box>
)

