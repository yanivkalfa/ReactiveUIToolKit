import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './PropertyInspectorPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { PROPERTY_INSPECTOR_BASIC } from './PropertyInspectorPage.example'

export const PropertyInspectorPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      PropertyField &amp; InspectorElement
    </Typography>
    <Typography variant="body1" paragraph>
      Editor-only helpers that wrap Unity&apos;s <code>PropertyField</code> and <code>InspectorElement</code>{' '}
      via <code>PropertyFieldProps</code> and <code>InspectorElementProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('PropertyInspectorProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={PROPERTY_INSPECTOR_BASIC} />
    </Box>
    <UnityDocsSection componentName="PropertyInspector" />
  </Box>
)

