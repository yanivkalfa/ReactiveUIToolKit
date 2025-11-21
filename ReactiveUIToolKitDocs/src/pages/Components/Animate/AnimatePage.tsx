import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './AnimatePage.style'
import { ANIMATE_BASIC } from './AnimatePage.example'

export const AnimatePage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Animate
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Animate</code> wraps a child subtree with tracks controlled by <code>AnimateProps</code> and{' '}
      <code>Hooks.UseAnimate</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={ANIMATE_BASIC} />
    </Box>
  </Box>
)

