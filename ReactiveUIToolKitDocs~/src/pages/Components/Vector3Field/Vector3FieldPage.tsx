import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './Vector3FieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { VECTOR3_FIELD_BASIC } from './Vector3FieldPage.example'

export const Vector3FieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Vector3Field
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Vector3Field</code> wraps the UI Toolkit <code>Vector3Field</code> control using{' '}
      <code>Vector3FieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('Vector3FieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={VECTOR3_FIELD_BASIC} />
    </Box>
    <UnityDocsSection componentName="Vector3Field" />
  </Box>
)

