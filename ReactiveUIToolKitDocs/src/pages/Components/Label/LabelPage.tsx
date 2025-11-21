import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './LabelPage.style'
import { LABEL_BASIC } from './LabelPage.example'

export const LabelPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Label
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Label</code> wraps the UI Toolkit <code>Label</code> element via <code>LabelProps</code>.
      It is the primary way to render text in your component trees.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={LABEL_BASIC} />
    </Box>
  </Box>
)

