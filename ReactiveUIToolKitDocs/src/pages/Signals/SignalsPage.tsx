import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './SignalsPage.style'
import { SIGNALS_EXAMPLE } from './SignalsPage.example'

export const SignalsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Signals
    </Typography>
    <Typography variant="body1" paragraph>
      Lightweight reactive values for cross-component communication.
    </Typography>
    <CodeBlock language="tsx" code={SIGNALS_EXAMPLE} />
  </Box>
)

