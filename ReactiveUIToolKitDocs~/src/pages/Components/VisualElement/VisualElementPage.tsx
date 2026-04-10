import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './VisualElementPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { VISUAL_ELEMENT_BASIC, VISUAL_ELEMENT_SIGNATURE } from './VisualElementPage.example'

export const VisualElementPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      VisualElement
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.VisualElement</code> creates a generic container element styled via a <code>Style</code>{' '}
      instance, and is often used as the top-level layout node for your component trees.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Signature
      </Typography>
      <CodeBlock language="jsx" code={VISUAL_ELEMENT_SIGNATURE} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic container
      </Typography>
      <CodeBlock language="jsx" code={VISUAL_ELEMENT_BASIC} />
    </Box>
    <UnityDocsSection componentName="VisualElement" />
  </Box>
)

