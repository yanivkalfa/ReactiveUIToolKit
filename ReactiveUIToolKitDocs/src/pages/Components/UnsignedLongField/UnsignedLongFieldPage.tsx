import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './UnsignedLongFieldPage.style'
import { UNSIGNED_LONG_FIELD_BASIC } from './UnsignedLongFieldPage.example'

export const UnsignedLongFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      UnsignedLongField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.UnsignedLongField</code> represents a <code>ulong</code> numeric field using{' '}
      <code>UnsignedLongFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={UNSIGNED_LONG_FIELD_BASIC} />
    </Box>
  </Box>
)

