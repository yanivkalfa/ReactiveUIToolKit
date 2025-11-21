import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './VisualElementPage.style'
import { VISUAL_ELEMENT_BASIC, VISUAL_ELEMENT_SAFE } from './VisualElementPage.example'

export const VisualElementPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      VisualElement &amp; VisualElementSafe
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.VisualElement</code> creates a generic container element styled via a <code>Style</code>{' '}
      instance, and is often used as the top-level layout node. <code>V.VisualElementSafe</code>{' '}
      behaves the same but merges its padding with safe-area insets from <code>SafeAreaUtility</code>.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic container
      </Typography>
      <CodeBlock language="tsx" code={VISUAL_ELEMENT_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Safe-area aware container
      </Typography>
      <CodeBlock language="tsx" code={VISUAL_ELEMENT_SAFE} />
    </Box>
  </Box>
)

