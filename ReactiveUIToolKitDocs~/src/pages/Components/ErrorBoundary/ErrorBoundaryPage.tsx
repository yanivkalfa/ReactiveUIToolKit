import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ErrorBoundaryPage.style'
import { ERROR_BOUNDARY_BASIC } from './ErrorBoundaryPage.example'

export const ErrorBoundaryPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ErrorBoundary
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ErrorBoundary</code> catches exceptions from its descendants and renders the{' '}
      <code>Fallback</code> <code>VirtualNode</code> from <code>ErrorBoundaryProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('ErrorBoundaryProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={ERROR_BOUNDARY_BASIC} />
    </Box>
  </Box>
)
