import type { FC } from 'react'
import {
  Box,
  List,
  ListItem,
  ListItemText,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './AudioPage.style'
import { AUDIO_3D, AUDIO_BASIC, USE_SFX_BASIC, USE_SFX_MIXER } from './AudioPage.example'

export const AudioPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Audio
    </Typography>
    <Typography variant="body1" paragraph>
      <code>&lt;Audio&gt;</code> is a side-effect-only Func-Component — it renders nothing
      visible. On mount it rents a pooled <code>AudioSource</code> from the shared{' '}
      <code>MediaHost</code>, applies the props, and returns the source on unmount. Use it for
      music beds, ambient loops, or any positional 3D source whose lifetime should follow a
      component.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('AudioProps')} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Concepts
      </Typography>
      <List sx={Styles.section}>
        <ListItem disablePadding>
          <ListItemText primary="MediaHost owns a HideAndDontSave GameObject that pools AudioSources. Sources are reference-counted and survive domain reloads." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Renders nothing — does not create a VisualElement. Use it as a sibling inside any container." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Setting WorldPosition reparents the source to the host root and places it in world space so 3D attenuation (RolloffMode / MinDistance / MaxDistance) works." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="On unmount the source is stopped and returned to the pool, and the AudioController flips IsAttached to false." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="For one-shot, fire-and-forget sounds prefer the useSfx() hook below — it does not allocate a per-instance source." />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage — looping music
      </Typography>
      <CodeBlock language="jsx" code={AUDIO_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        3D positional audio
      </Typography>
      <CodeBlock language="jsx" code={AUDIO_3D} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Imperative control via <code>AudioController</code>
      </Typography>
      <Typography variant="body2" paragraph>
        Pass a <code>Ref&lt;AudioController&gt;</code> through the <code>Controller</code> prop
        for imperative access. After unmount <code>IsAttached</code> turns false and further
        calls become no-ops.
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Member</strong></TableCell>
              <TableCell><strong>Type</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><code>IsAttached</code></TableCell>
              <TableCell><code>bool</code></TableCell>
              <TableCell>True while bound to a live AudioSource.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>IsPlaying</code></TableCell>
              <TableCell><code>bool</code></TableCell>
              <TableCell>True while audio is currently playing.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Length</code></TableCell>
              <TableCell><code>float</code></TableCell>
              <TableCell>Clip length in seconds, 0 if no clip.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Time</code></TableCell>
              <TableCell><code>float</code></TableCell>
              <TableCell>Read/write playback position in seconds.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Volume</code></TableCell>
              <TableCell><code>float</code></TableCell>
              <TableCell>Volume in [0, 1].</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Pitch</code></TableCell>
              <TableCell><code>float</code></TableCell>
              <TableCell>Pitch multiplier (negative = reverse).</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Mute</code></TableCell>
              <TableCell><code>bool</code></TableCell>
              <TableCell>Read/write mute toggle.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Play()</code></TableCell>
              <TableCell><code>void</code></TableCell>
              <TableCell>Begin or resume playback.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Pause()</code></TableCell>
              <TableCell><code>void</code></TableCell>
              <TableCell>Pause without rewinding.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Stop()</code></TableCell>
              <TableCell><code>void</code></TableCell>
              <TableCell>Stop and rewind to the start.</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        <code>useSfx()</code> — one-shot sounds
      </Typography>
      <Typography variant="body1" paragraph>
        <code>useSfx()</code> returns a stable, allocation-free{' '}
        <code>{'Action<AudioClip, float>'}</code> backed by{' '}
        <code>MediaHost.SfxSource.PlayOneShot</code>. Use it for buttons, hover sounds, hit
        markers, and any other fire-and-forget effect — the host owns one shared{' '}
        <code>AudioSource</code> for all one-shots, so there is no per-call allocation and no
        component lifetime to manage.
      </Typography>
      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Signature</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><code>{'useSfx(AudioMixerGroup mixer = null)'}</code></TableCell>
              <TableCell>
                Returns a stable <code>{'Action<AudioClip, float>'}</code>. Pass{' '}
                <code>(clip, volumeScale)</code> to play a one-shot. The optional{' '}
                <code>mixer</code> routes through a specific mixer group; pass <code>null</code>{' '}
                to use the MediaHost default.
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
      <CodeBlock language="jsx" code={USE_SFX_BASIC} />
      <Box sx={{ mt: 2 }}>
        <CodeBlock language="jsx" code={USE_SFX_MIXER} />
      </Box>
      <Typography variant="body2" paragraph sx={{ mt: 2, opacity: 0.8 }}>
        The returned delegate has the same identity across renders, so it is safe to put inside
        a <code>useEffect</code> dependency array without re-running the effect on every render.
      </Typography>
    </Box>
  </Box>
)
