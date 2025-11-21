import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './FloatFieldPage.style'
import { FLOAT_FIELD_BASIC } from './FloatFieldPage.example'

export const FloatFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      FloatField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.FloatField</code> represents a single-precision numeric field, backed by{' '}
      <code>FloatFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={FLOAT_FIELD_BASIC} />
    </Box>
  </Box>
)

