import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper } from '@mui/material'
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
      <CodeBlock language="jsx" code={getPropsDoc('AnimateProps')} />
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
      <CodeBlock language="jsx" code={ANIMATE_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        AnimateTrack properties
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Property</strong></TableCell>
              <TableCell><strong>Type</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow><TableCell><code>Property</code></TableCell><TableCell><code>string</code></TableCell><TableCell>The style property to animate (e.g. <code>&quot;opacity&quot;</code>, <code>&quot;background-color&quot;</code>).</TableCell></TableRow>
            <TableRow><TableCell><code>From</code></TableCell><TableCell><code>object</code></TableCell><TableCell>Start value.</TableCell></TableRow>
            <TableRow><TableCell><code>To</code></TableCell><TableCell><code>object</code></TableCell><TableCell>End value.</TableCell></TableRow>
            <TableRow><TableCell><code>Duration</code></TableCell><TableCell><code>float</code></TableCell><TableCell>Duration in seconds.</TableCell></TableRow>
            <TableRow><TableCell><code>Delay</code></TableCell><TableCell><code>float</code></TableCell><TableCell>Delay before playback starts.</TableCell></TableRow>
            <TableRow><TableCell><code>Ease</code></TableCell><TableCell><code>Ease</code></TableCell><TableCell>Easing curve (default: <code>EaseInOutCubic</code>).</TableCell></TableRow>
            <TableRow><TableCell><code>Repeat</code></TableCell><TableCell><code>int</code></TableCell><TableCell>Number of repetitions (0 = once).</TableCell></TableRow>
            <TableRow><TableCell><code>Loop</code></TableCell><TableCell><code>bool</code></TableCell><TableCell>Loop indefinitely.</TableCell></TableRow>
            <TableRow><TableCell><code>Yoyo</code></TableCell><TableCell><code>bool</code></TableCell><TableCell>Reverse direction on each repeat.</TableCell></TableRow>
            <TableRow><TableCell><code>TimeScale</code></TableCell><TableCell><code>float</code></TableCell><TableCell>Playback speed multiplier (default: 1).</TableCell></TableRow>
            <TableRow><TableCell><code>OnUpdate</code></TableCell><TableCell><code>{'Action<float>'}</code></TableCell><TableCell>Called each frame with the normalized progress (0–1).</TableCell></TableRow>
            <TableRow><TableCell><code>OnComplete</code></TableCell><TableCell><code>Action</code></TableCell><TableCell>Called when the animation finishes.</TableCell></TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Ease enum values
      </Typography>
      <Typography variant="body2" paragraph>
        All easing presets from <code>CssHelpers</code> (e.g. <code>EaseInSine</code>) map to
        these <code>Ease</code> values:
      </Typography>
      <Typography component="ul" variant="body2">
        <li><code>Linear</code></li>
        <li><code>EaseInSine</code>, <code>EaseOutSine</code>, <code>EaseInOutSine</code></li>
        <li><code>EaseInQuad</code>, <code>EaseOutQuad</code>, <code>EaseInOutQuad</code></li>
        <li><code>EaseInCubic</code>, <code>EaseOutCubic</code>, <code>EaseInOutCubic</code></li>
        <li><code>EaseInQuint</code>, <code>EaseOutQuint</code>, <code>EaseInOutQuint</code></li>
        <li><code>EaseInExpo</code>, <code>EaseOutExpo</code>, <code>EaseInOutExpo</code></li>
        <li><code>EaseInBack</code>, <code>EaseOutBack</code>, <code>EaseInOutBack</code></li>
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Hook alternatives
      </Typography>
      <Typography variant="body1" paragraph>
        You can use animation hooks directly in setup code instead of the{' '}
        <code>{'<Animate>'}</code> element:
      </Typography>
      <Typography component="ul" variant="body2">
        <li>
          <code>{'Hooks.UseAnimate(tracks, autoplay?, deps)'}</code> — starts{' '}
          <code>AnimateTrack</code> definitions on the component&apos;s container.
          Plays on dependency change, cleans up on unmount.
        </li>
        <li>
          <code>{'Hooks.UseTweenFloat(from, to, duration, ease, delay, onUpdate, onComplete, deps)'}</code>{' '}
          — tweens a single float value with easing. Useful for custom procedural
          animations that don&apos;t target a specific style property.
        </li>
      </Typography>
    </Box>
  </Box>
)
