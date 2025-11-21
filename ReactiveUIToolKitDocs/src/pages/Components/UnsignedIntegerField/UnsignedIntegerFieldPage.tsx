import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './UnsignedIntegerFieldPage.style'
import { UNSIGNED_INTEGER_FIELD_BASIC } from './UnsignedIntegerFieldPage.example'

export const UnsignedIntegerFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      UnsignedIntegerField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.UnsignedIntegerField</code> represents a <code>uint</code> numeric field using{' '}
      <code>UnsignedIntegerFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={UNSIGNED_INTEGER_FIELD_BASIC} />
    </Box>
  </Box>
)

