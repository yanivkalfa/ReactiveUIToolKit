import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './SliderIntPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { SLIDER_INT_BASIC } from './SliderIntPage.example'

export const SliderIntPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      SliderInt
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.SliderInt</code> renders an integer slider using <code>SliderIntProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('SliderIntProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={SLIDER_INT_BASIC} />
    </Box>
    <UnityDocsSection componentName="SliderInt" />
  </Box>
)

