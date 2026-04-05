import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './HelpBoxPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { HELP_BOX_BASIC } from './HelpBoxPage.example'

export const HelpBoxPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      HelpBox
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.HelpBox</code> wraps the standard UI Toolkit <code>HelpBox</code> for displaying
      informational, warning, or error messages.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('HelpBoxProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={HELP_BOX_BASIC} />
    </Box>
    <UnityDocsSection componentName="HelpBox" />
  </Box>
)

