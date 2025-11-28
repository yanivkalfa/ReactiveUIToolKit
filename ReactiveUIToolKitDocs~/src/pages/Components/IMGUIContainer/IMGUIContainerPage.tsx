import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './IMGUIContainerPage.style'
import { IMGUI_CONTAINER_BASIC } from './IMGUIContainerPage.example'

export const IMGUIContainerPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      IMGUIContainer
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.IMGUIContainer</code> lets you embed IMGUI content inside a UI Toolkit layout by
      providing an <code>OnGUI</code> callback in <code>IMGUIContainerProps</code>. This is primarily
      an editor-only pattern.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('IMGUIContainerProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage (Editor)
      </Typography>
      <CodeBlock language="tsx" code={IMGUI_CONTAINER_BASIC} />
    </Box>
  </Box>
)

