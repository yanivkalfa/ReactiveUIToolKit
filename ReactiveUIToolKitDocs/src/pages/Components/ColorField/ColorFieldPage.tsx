import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './ColorFieldPage.style'
import { COLOR_FIELD_BASIC } from './ColorFieldPage.example'

export const ColorFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ColorField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ColorField</code> wraps the UI Toolkit <code>ColorField</code> element using{' '}
      <code>ColorFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={COLOR_FIELD_BASIC} />
    </Box>
  </Box>
)

