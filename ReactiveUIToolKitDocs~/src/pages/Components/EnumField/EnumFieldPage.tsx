import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './EnumFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { ENUM_FIELD_BASIC } from './EnumFieldPage.example'

export const EnumFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      EnumField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.EnumField</code> binds to any enum type via <code>EnumFieldProps</code>. Provide the
      enum&apos;s assembly-qualified type name and an initial <code>Value</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('EnumFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={ENUM_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        <code>EnumFieldProps.Label</code> and <code>EnumFieldProps.VisualInput</code> configure the
        label and input slots respectively. As with other fields, both expect dictionaries; label
        dictionaries are often created from <code>LabelProps.ToDictionary()</code>, while visual
        input dictionaries typically wrap a <code>Style</code> instance.
      </Typography>
    </Box>
    <UnityDocsSection componentName="EnumField" />
  </Box>
)

