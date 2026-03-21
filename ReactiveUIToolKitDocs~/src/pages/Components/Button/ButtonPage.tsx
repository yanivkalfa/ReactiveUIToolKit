import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ButtonPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { BUTTON_BASIC } from './ButtonPage.example'

export const ButtonPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Button
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Button</code> wraps the UI Toolkit <code>Button</code> element with{' '}
      <code>ButtonProps</code>. Use it for clickable actions.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('ButtonProps')} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <Typography variant="body1" paragraph>
        Provide <code>Text</code>, optional <code>Style</code>, and an <code>OnClick</code> handler
        in <code>ButtonProps</code>. Combine with <code>Hooks.UseState</code> to build controlled
        buttons.
      </Typography>
      <CodeBlock language="tsx" code={BUTTON_BASIC} />
    </Box>
    <UnityDocsSection componentName="Button" />
  </Box>
)

