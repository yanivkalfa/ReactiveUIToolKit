import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './RadioButtonGroupPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { RADIO_BUTTON_GROUP_BASIC } from './RadioButtonGroupPage.example'

export const RadioButtonGroupPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      RadioButtonGroup
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.RadioButtonGroup</code> wraps UI Toolkit&apos;s <code>RadioButtonGroup</code> using{' '}
      <code>RadioButtonGroupProps</code>. It manages a set of mutually exclusive choices.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('RadioButtonGroupProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={RADIO_BUTTON_GROUP_BASIC} />
    </Box>
    <UnityDocsSection componentName="RadioButtonGroup" />
  </Box>
)

