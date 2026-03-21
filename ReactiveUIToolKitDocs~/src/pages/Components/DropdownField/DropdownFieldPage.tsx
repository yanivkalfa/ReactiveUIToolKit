import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './DropdownFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { DROPDOWN_FIELD_BASIC } from './DropdownFieldPage.example'

export const DropdownFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      DropdownField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.DropdownField</code> renders a text-based dropdown using <code>DropdownFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('DropdownFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={DROPDOWN_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        <code>DropdownFieldProps.Label</code> and <code>DropdownFieldProps.VisualInput</code> mirror
        the slots on the underlying UI Toolkit control. Use <code>Label</code> to configure the label
        element, and <code>VisualInput</code> to style the internal input area via a dictionary that
        can contain a nested <code>Style</code>.
      </Typography>
    </Box>
    <UnityDocsSection componentName="DropdownField" />
  </Box>
)

