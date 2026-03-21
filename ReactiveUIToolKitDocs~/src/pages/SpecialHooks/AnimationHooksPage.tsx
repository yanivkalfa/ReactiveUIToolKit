import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './AnimationHooksPage.style'

const ANIMATION_HOOKS_BASIC = `using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class AnimateWithHook
{
  // Function component – pass AnimateWithHook.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var tracks = new[]
    {
      AnimateTrack.Property(
        property: StyleKeys.BackgroundColor,
        from: Color.gray,
        to: Color.cyan,
        durationSeconds: 0.75f,
        easing: Easing.InOutQuad
      ),
    };

    Hooks.UseAnimate(tracks);

    return V.Box(
      new BoxProps
      {
        Style = new Style { (StyleKeys.Width, 120f), (StyleKeys.Height, 32f) },
      },
      V.Label(new LabelProps { Text = "Animated box (UseAnimate)" })
    );
  }
}`

const TWEEN_FLOAT_BASIC = `using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class TweenFloatExamples
{
  // Function component – pass TweenFloatExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    float current = 0f;

    Hooks.UseTweenFloat(
      from: 0f,
      to: 1f,
      duration: 1.0f,
      ease: Ease.InOutQuad,
      delay: 0f,
      onUpdate: value => current = value,
      onComplete: () => Debug.Log($"Tween finished at {current:0.00}")
    );

    return V.Label(new LabelProps { Text = $"Tween value: {current:0.00}" });
  }
}`

export const AnimationHooksPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Special animation hooks
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit exposes animation-specific hooks that do not exist in React&apos;s core API.
      These hooks are designed to drive UI Toolkit animations in a frame-accurate way while still
      fitting into the normal function component lifecycle.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        <code>Hooks.UseAnimate</code>
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Starts one or more AnimateTrack definitions on the component's VisualElement container." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Tracks are created with ReactiveUITK.Core.Animation.AnimateTrack helpers (for example, animating background color or size)." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Plays animations when dependencies change, and stops/cleans them up when the component unmounts or the effect is re-run." />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={ANIMATION_HOOKS_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        <code>Hooks.UseTweenFloat</code>
      </Typography>
      <List sx={Styles.list}>
        <ListItem disablePadding>
          <ListItemText primary="Tweens a single float value over time with easing and an optional delay." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Calls an onUpdate callback every frame with the eased value, and an onComplete callback when finished." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Uses UI Toolkit's scheduler and integrates with the component's lifecycle; cancelling on unmount." />
        </ListItem>
      </List>
      <CodeBlock language="tsx" code={TWEEN_FLOAT_BASIC} />
    </Box>

    <Typography variant="body2" sx={Styles.section}>
      For a higher-level API, see the <code>Animate</code> component documented under Components →
      Common/Uncommon Components. It builds on top of these hooks and the underlying{' '}
      <code>Animator</code> utilities.
    </Typography>
  </Box>
)
