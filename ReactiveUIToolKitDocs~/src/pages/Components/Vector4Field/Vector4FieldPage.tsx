import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './Vector4FieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { VECTOR4_FIELD_BASIC } from './Vector4FieldPage.example'

export const Vector4FieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Vector4Field
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Vector4Field</code> wraps the UI Toolkit <code>Vector4Field</code> control using{' '}
      <code>Vector4FieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('Vector4FieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={VECTOR4_FIELD_BASIC} />
    </Box>
    <UnityDocsSection componentName="Vector4Field" />
  </Box>
)

