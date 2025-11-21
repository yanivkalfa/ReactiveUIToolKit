import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './ScrollerPage.style'
import { SCROLLER_BASIC } from './ScrollerPage.example'

export const ScrollerPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Scroller
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Scroller</code> wraps the low-level UI Toolkit <code>Scroller</code> element using{' '}
      <code>ScrollerProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={SCROLLER_BASIC} />
    </Box>
  </Box>
)

