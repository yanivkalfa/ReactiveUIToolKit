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
      <CodeBlock language="jsx" code={getPropsDoc('ErrorBoundaryProps')} />
      <Typography component="ul" variant="body2" sx={{ mt: 1 }}>
        <li><code>Fallback</code> — a <code>VirtualNode</code> rendered when an error is caught.</li>
        <li>
          <code>OnError</code> — an <code>{'ErrorEventHandler (Action<Exception>)'}</code> callback
          invoked when an exception is caught. Use it for logging or analytics.
        </li>
        <li>
          <code>ResetKey</code> — a string value that, when changed, clears the error state and
          re-renders the children. Useful for "try again" patterns.
        </li>
      </Typography>
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={ERROR_BOUNDARY_BASIC} />
    </Box>
  </Box>
)
