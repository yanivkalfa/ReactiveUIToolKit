import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ImagePage.style'
import { IMAGE_BASIC } from './ImagePage.example'

export const ImagePage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Image
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Image</code> renders a UI Toolkit <code>Image</code> using <code>ImageProps</code>. It
      supports both <code>Texture2D</code> and <code>Sprite</code> sources.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('ImageProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={IMAGE_BASIC} />
    </Box>
  </Box>
)

