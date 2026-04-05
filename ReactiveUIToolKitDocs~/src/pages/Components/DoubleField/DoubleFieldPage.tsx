import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './DoubleFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { DOUBLE_FIELD_BASIC } from './DoubleFieldPage.example'

export const DoubleFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      DoubleField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.DoubleField</code> exposes a double-precision numeric field via{' '}
      <code>DoubleFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('DoubleFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={DOUBLE_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        <code>DoubleFieldProps.Label</code> and <code>DoubleFieldProps.VisualInput</code> follow the
        same pattern as other numeric fields. Use a label dictionary (often built from{' '}
        <code>LabelProps</code>) and a visual input dictionary that can contain a nested{' '}
        <code>Style</code> for the inner input container.
      </Typography>
    </Box>
    <UnityDocsSection componentName="DoubleField" />
  </Box>
)

