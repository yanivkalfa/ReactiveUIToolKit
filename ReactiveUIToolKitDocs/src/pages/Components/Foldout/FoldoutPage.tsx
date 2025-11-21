import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './FoldoutPage.style'
import { FOLDOUT_BASIC } from './FoldoutPage.example'

export const FoldoutPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Foldout
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Foldout</code> wraps the UI Toolkit <code>Foldout</code> element using{' '}
      <code>FoldoutProps</code>. It is useful for expandable sections of UI that reveal more content
      when open.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <Typography variant="body1" paragraph>
        Provide <code>Text</code>, an optional initial <code>Value</code>, and an{' '}
        <code>OnChange</code> handler. The example below also shows children rendered inside the
        foldout when it is expanded.
      </Typography>
      <CodeBlock language="tsx" code={FOLDOUT_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Children
      </Typography>
      <Typography variant="body1" paragraph>
        Children passed to <code>V.Foldout</code> are rendered inside the foldout&apos;s content
        area and are shown or hidden based on the current <code>Value</code>.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (header / contentContainer)
      </Typography>
      <Typography variant="body1" paragraph>
        Use <code>FoldoutProps.Header</code> and <code>FoldoutProps.ContentContainer</code> to style
        the header bar and inner content container. Both accept dictionaries; commonly a nested{' '}
        <code>Style</code> is provided under the <code>"style"</code> key.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Controlled value
      </Typography>
      <Typography variant="body1" paragraph>
        For controlled foldouts, track a <code>bool</code> with <code>Hooks.UseState</code> (or a
        signal) and update it in <code>OnChange</code>. The <code>Value</code> property will then
        always reflect your source of truth.
      </Typography>
    </Box>
  </Box>
)

