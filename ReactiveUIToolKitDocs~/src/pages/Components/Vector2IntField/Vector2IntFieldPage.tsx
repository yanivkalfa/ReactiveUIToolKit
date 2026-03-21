import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './Vector2IntFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { VECTOR2_INT_FIELD_BASIC } from './Vector2IntFieldPage.example'

export const Vector2IntFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Vector2IntField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Vector2IntField</code> wraps the UI Toolkit <code>Vector2IntField</code> control using{' '}
      <code>Vector2IntFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('Vector2IntFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={VECTOR2_INT_FIELD_BASIC} />
    </Box>
    <UnityDocsSection componentName="Vector2IntField" />
  </Box>
)

