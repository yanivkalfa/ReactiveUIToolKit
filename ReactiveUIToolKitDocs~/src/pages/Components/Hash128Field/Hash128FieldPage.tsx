import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './Hash128FieldPage.style'
import { HASH128_FIELD_BASIC } from './Hash128FieldPage.example'

export const Hash128FieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Hash128Field
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Hash128Field</code> wraps the UI Toolkit <code>Hash128Field</code> for editing{' '}
      <code>Hash128</code> values.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('Hash128FieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={HASH128_FIELD_BASIC} />
    </Box>
  </Box>
)

