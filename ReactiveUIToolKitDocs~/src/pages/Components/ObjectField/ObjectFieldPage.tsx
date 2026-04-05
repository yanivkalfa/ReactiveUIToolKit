import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ObjectFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { OBJECT_FIELD_BASIC } from './ObjectFieldPage.example'

export const ObjectFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ObjectField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ObjectField</code> wraps the editor-only UI Toolkit <code>ObjectField</code> element
      using <code>ObjectFieldProps</code>. It is typically used in custom inspectors and tools.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('ObjectFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage (Editor)
      </Typography>
      <CodeBlock language="jsx" code={OBJECT_FIELD_BASIC} />
    </Box>
    <UnityDocsSection componentName="ObjectField" />
  </Box>
)

