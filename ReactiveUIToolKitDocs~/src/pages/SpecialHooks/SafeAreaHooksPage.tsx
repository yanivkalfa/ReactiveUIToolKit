import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './SafeAreaHooksPage.style'

const SAFE_AREA_HOOKS_BASIC = `using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Core.Util;
using UnityEngine;

public static class SafeAreaHooksDemoFunc
{
  // Function component – pass SafeAreaHooksDemoFunc.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    // Read current insets (top, bottom, left, right)
    SafeAreaInsets insets = Hooks.UseSafeArea();

    var style = new Style
    {
      (StyleKeys.BackgroundColor, new Color(0.15f, 0.15f, 0.15f, 1f)),
    };

    return V.VisualElementSafe(
      style,
      key: null,
      V.Label(new LabelProps { Text = $"Safe area: top={insets.Top:0}, bottom={insets.Bottom:0}" })
    );
  }
}`

export const SafeAreaHooksPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Safe area hooks
    </Typography>
    <Typography variant="body1" paragraph>
      When targeting mobile or platforms with notches and system insets, the{' '}
      <code>Hooks.UseSafeArea</code> hook and <code>V.VisualElementSafe</code> helper work together
      to keep your layout inside the safe region.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        <code>Hooks.UseSafeArea</code>
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Returns SafeAreaInsets (top, bottom, left, right) based on Unity's Screen.safeArea." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Records a hook usage so that changes to the safe area can trigger re-rendering." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Accepts an optional tolerance parameter to avoid flicker when the reported insets change only slightly." />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={SAFE_AREA_HOOKS_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        <code>V.VisualElementSafe</code> helper
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="V.VisualElementSafe(style, key, children) – wraps a VisualElement and automatically applies padding based on SafeAreaInsets." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Merges your own padding with the safe-area padding so you keep control over layout while staying visible on all devices." />
        </ListItem>
      </List>
    </Box>

    <Typography variant="body2" sx={Styles.section}>
      Combine <code>Hooks.UseSafeArea</code> when you need direct access to inset values with{' '}
      <code>V.VisualElementSafe</code> when you want a drop-in, safe-area-aware container.
    </Typography>
  </Box>
)
