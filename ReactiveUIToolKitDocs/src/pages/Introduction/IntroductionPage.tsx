import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from './IntroductionPage.style'

export const IntroductionPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ReactiveUIToolKit
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit brings a React-like component model to Unity UI Toolkit using a virtual node
      tree, typed props, and reconciliation logic that runs in C#. You build your UI from{' '}
      <code>V.*</code> helpers and function components, and the reconciler updates the underlying
      <code>VisualElement</code> hierarchy for you.
    </Typography>
    <Typography variant="body1" paragraph>
      The toolkit is designed to work both in the Unity Editor and at runtime, and to feel familiar
      if you have used React, while still fitting naturally into Unity&apos;s component model and UI
      Toolkit controls.
    </Typography>
    <Typography variant="body2" paragraph>
      <strong>P.S.</strong> ReactiveUIToolKit runs entirely in C# on top of Unity UI Toolkit. There
      is no JavaScript engine or bridge layer involved.
    </Typography>
    <Typography variant="h5" component="h2" gutterBottom>
      Highlights
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="VirtualNode diffing and batched updates for UI Toolkit trees" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Typed props and adapters for most built-in UI Toolkit controls" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Router and Signals utilities for navigation and shared state" />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Editor-only elements are UNITY_EDITOR guarded" />
      </ListItem>
    </List>
  </Box>
)
