import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './DoubleFieldPage.style'
import { DOUBLE_FIELD_BASIC } from './DoubleFieldPage.example'

export const DoubleFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      DoubleField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.DoubleField</code> exposes a double-precision numeric field via{' '}
      <code>DoubleFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={DOUBLE_FIELD_BASIC} />
    </Box>
  </Box>
)

