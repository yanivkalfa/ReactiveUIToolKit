import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './Vector2FieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { VECTOR2_FIELD_BASIC } from './Vector2FieldPage.example'

export const Vector2FieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Vector2Field
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Vector2Field</code> wraps the UI Toolkit <code>Vector2Field</code> control using{' '}
      <code>Vector2FieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('Vector2FieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={VECTOR2_FIELD_BASIC} />
    </Box>
    <UnityDocsSection componentName="Vector2Field" />
  </Box>
)

