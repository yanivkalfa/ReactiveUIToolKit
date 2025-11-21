import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './MinMaxSliderPage.style'
import { MIN_MAX_SLIDER_BASIC } from './MinMaxSliderPage.example'

export const MinMaxSliderPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      MinMaxSlider
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.MinMaxSlider</code> wraps the UI Toolkit <code>MinMaxSlider</code> element using{' '}
      <code>MinMaxSliderProps</code> for selecting a range between two limits.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={MIN_MAX_SLIDER_BASIC} />
    </Box>
  </Box>
)

