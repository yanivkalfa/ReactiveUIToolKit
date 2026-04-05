import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './SliderPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { SLIDER_BASIC } from './SliderPage.example'

export const SliderPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Slider
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Slider</code> renders a float slider using <code>SliderProps</code>. In addition to
      basic value and range, you can optionally style the inner parts of the UI Toolkit slider
      through slot dictionaries (<code>Input</code>, <code>Track</code>, <code>DragContainer</code>,{' '}
      <code>Handle</code>, and <code>HandleBorder</code>), which map to the corresponding visual
      elements inside the control.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('SliderProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={SLIDER_BASIC} />
    </Box>
    <UnityDocsSection componentName="Slider" />
  </Box>
)

