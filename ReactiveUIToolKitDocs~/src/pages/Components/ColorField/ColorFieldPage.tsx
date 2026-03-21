import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ColorFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { COLOR_FIELD_BASIC } from './ColorFieldPage.example'

export const ColorFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ColorField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ColorField</code> wraps the UI Toolkit <code>ColorField</code> element using{' '}
      <code>ColorFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('ColorFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={COLOR_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        Use <code>ColorFieldProps.Label</code> to configure the label element, and{' '}
        <code>ColorFieldProps.VisualInput</code> to style the input container (for example, padding
        or background). Both properties accept dictionaries; in most cases you construct them from
        other typed props or by nesting a <code>Style</code> instance.
      </Typography>
    </Box>
    <UnityDocsSection componentName="ColorField" />
  </Box>
)

