import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from './IntroductionPage.style'

export const IntroductionPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ReactiveUIToolKit
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit using a virtual
      node tree, typed props, and diff-based reconciliation.
    </Typography>
    <Typography variant="h5" component="h2" gutterBottom>
      Highlights
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="VirtualNode diffing and batched updates" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Typed props and adapters for UI Toolkit controls" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Hooks, Router, and Signals utilities" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Editor-only elements are UNITY_EDITOR guarded" />
      </ListItem>
    </List>
  </Box>
)

