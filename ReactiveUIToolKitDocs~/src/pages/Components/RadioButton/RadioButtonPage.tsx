import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './RadioButtonPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { RADIO_BUTTON_BASIC } from './RadioButtonPage.example'

export const RadioButtonPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      RadioButton
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.RadioButton</code> wraps the UI Toolkit <code>RadioButton</code> element using{' '}
      <code>RadioButtonProps</code>. It is usually used within a <code>RadioButtonGroup</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('RadioButtonProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={RADIO_BUTTON_BASIC} />
    </Box>
    <UnityDocsSection componentName="RadioButton" />
  </Box>
)

