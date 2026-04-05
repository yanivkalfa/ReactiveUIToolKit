import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './VisualElementSafePage.style'
import { VISUAL_ELEMENT_SAFE, VISUAL_ELEMENT_SAFE_SIGNATURE } from './VisualElementSafePage.example'

export const VisualElementSafePage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      VisualElementSafe
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.VisualElementSafe</code> is a safe-area-aware variant of <code>V.VisualElement</code>{' '}
      that merges its padding with safe-area insets from <code>SafeAreaUtility</code>. Use it as a
      top-level container on devices with notches or system UI overlays.
    </Typography>
    <Typography variant="body1" paragraph>
      Pass either a <code>Style</code> or the same props dictionary you would send to{' '}
      <code>V.VisualElement</code> (e.g., <code>pickingMode</code>, <code>name</code>, refs, event
      handlers). The helper clones those props, replaces/merges the <code>style</code> entry, and
      leaves everything else untouched.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Signature
      </Typography>
      <CodeBlock language="jsx" code={VISUAL_ELEMENT_SAFE_SIGNATURE} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Safe-area aware container
      </Typography>
      <CodeBlock language="jsx" code={VISUAL_ELEMENT_SAFE} />
    </Box>
  </Box>
)
