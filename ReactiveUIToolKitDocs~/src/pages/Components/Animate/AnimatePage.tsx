import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './AnimatePage.style'
import { ANIMATE_BASIC } from './AnimatePage.example'

export const AnimatePage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Animate
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Animate</code> wraps a child subtree and drives one or more animation tracks on its
      root <code>VisualElement</code>. It is a thin, declarative wrapper around{' '}
      <code>Hooks.UseAnimate</code> and the underlying <code>Animator</code> helpers.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('AnimateProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Concepts
      </Typography>
      <List sx={Styles.section}>
        <ListItem disablePadding>
          <ListItemText primary="Tracks are defined via AnimateTrack helpers and target individual style properties (for example, backgroundColor, opacity, size)." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Each track specifies from/to values, duration, easing, and optional delay." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="When the Animate node mounts or its dependencies change, tracks are played; they are stopped and cleaned up automatically when unmounting." />
        </ListItem>
      </List>
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={ANIMATE_BASIC} />
    </Box>
  </Box>
)
