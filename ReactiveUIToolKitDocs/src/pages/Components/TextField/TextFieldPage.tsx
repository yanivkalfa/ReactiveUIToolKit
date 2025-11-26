import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './TextFieldPage.style'
import { TEXT_FIELD_BASIC } from './TextFieldPage.example'

export const TextFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      TextField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.TextField</code> wraps the UI Toolkit <code>TextField</code> using{' '}
      <code>TextFieldProps</code>, with support for slots like <code>Label</code>,{' '}
      <code>Input</code>, and <code>TextElement</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('TextFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={TEXT_FIELD_BASIC} />
    </Box>
  </Box>
)

