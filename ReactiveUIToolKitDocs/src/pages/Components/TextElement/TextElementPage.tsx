import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './TextElementPage.style'
import { TEXT_ELEMENT_BASIC } from './TextElementPage.example'

export const TextElementPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      TextElement
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.TextElement</code> is a low-level text node wrapper using <code>TextElementProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={TEXT_ELEMENT_BASIC} />
    </Box>
  </Box>
)

