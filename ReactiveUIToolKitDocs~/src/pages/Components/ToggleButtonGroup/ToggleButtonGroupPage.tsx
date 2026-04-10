import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ToggleButtonGroupPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { TOGGLE_BUTTON_GROUP_BASIC } from './ToggleButtonGroupPage.example'

export const ToggleButtonGroupPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ToggleButtonGroup
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ToggleButtonGroup</code> wraps the UI Toolkit <code>ToggleButtonGroup</code> element
      using <code>ToggleButtonGroupProps</code>. Provide a zero-based <code>Value</code> index and
      add regular <code>V.Button</code> children, handling each button&apos;s <code>OnClick</code>
      to drive your own selection state.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('ToggleButtonGroupProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={TOGGLE_BUTTON_GROUP_BASIC} />
    </Box>
    <UnityDocsSection componentName="ToggleButtonGroup" />
  </Box>
)
