import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './RectFieldPage.style'
import { RECT_FIELD_BASIC } from './RectFieldPage.example'

export const RectFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      RectField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.RectField</code> wraps the UI Toolkit <code>RectField</code> control using{' '}
      <code>RectFieldProps</code>. It is available in both runtime and editor UIs.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('RectFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={RECT_FIELD_BASIC} />
    </Box>
  </Box>
)

