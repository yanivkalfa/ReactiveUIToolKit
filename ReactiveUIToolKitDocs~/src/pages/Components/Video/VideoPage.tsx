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
import Styles from './VideoPage.style'
import { VIDEO_BASIC, VIDEO_CONTROLLER, VIDEO_URL } from './VideoPage.example'

export const VideoPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Video
    </Typography>
    <Typography variant="body1" paragraph>
      <code>&lt;Video&gt;</code> renders a positionable <code>VisualElement</code> that displays a
      pooled <code>RenderTexture</code> driven by a pooled <code>VideoPlayer</code>. Playback,
      pooling, audio routing and editor-mode pumping are all handled by the shared{' '}
      <code>MediaHost</code> peer.
    </Typography>
    <Typography variant="body1" paragraph>
      Provide either <code>Clip</code> (an asset-imported <code>VideoClip</code>) or{' '}
      <code>Url</code> (HTTP/RTMP/file URL for streaming). When both are set, <code>Url</code>{' '}
      wins.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('VideoProps')} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Concepts
      </Typography>
      <List sx={Styles.section}>
        <ListItem disablePadding>
          <ListItemText primary="MediaHost owns a HideAndDontSave GameObject that pools VideoPlayers, AudioSources, and RenderTextures keyed by (width, height, depth). Pools survive domain reloads." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="The host VisualElement uses Image.image = renderTexture (not style.backgroundImage), so the GPU surface is sampled fresh every frame instead of cached." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="VideoPlayer.frameReady drives MarkDirtyRepaint(); in the editor an extra QueuePlayerLoopUpdate() pump advances the player even when Unity is not ticking." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="On unmount the player + render-texture are returned to the pool. Reference counts make sure shared resources are torn down only when no element still holds them." />
        </ListItem>
      </List>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={VIDEO_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Streaming URL & lifecycle callbacks
      </Typography>
      <CodeBlock language="jsx" code={VIDEO_URL} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Imperative control via <code>VideoController</code>
      </Typography>
      <Typography variant="body2" paragraph>
        Pass a <code>Ref&lt;VideoController&gt;</code> through the <code>Controller</code> prop to
        drive playback imperatively. The controller is detached automatically on unmount —{' '}
        <code>IsAttached</code> turns false and further calls become no-ops.
      </Typography>
      <CodeBlock language="jsx" code={VIDEO_CONTROLLER} />
      <TableContainer component={Paper} variant="outlined" sx={{ mt: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>
                <strong>Member</strong>
              </TableCell>
              <TableCell>
                <strong>Type</strong>
              </TableCell>
              <TableCell>
                <strong>Description</strong>
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><code>IsAttached</code></TableCell>
              <TableCell><code>bool</code></TableCell>
              <TableCell>True while the controller is bound to a live VideoPlayer.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>IsPlaying</code></TableCell>
              <TableCell><code>bool</code></TableCell>
              <TableCell>True while the underlying player is currently playing.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>IsPrepared</code></TableCell>
              <TableCell><code>bool</code></TableCell>
              <TableCell>True after the source has finished preparing.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Duration</code></TableCell>
              <TableCell><code>double</code></TableCell>
              <TableCell>Clip length in seconds, 0 if not yet known.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Time</code></TableCell>
              <TableCell><code>double</code></TableCell>
              <TableCell>Read/write playback position in seconds.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Frame</code></TableCell>
              <TableCell><code>long</code></TableCell>
              <TableCell>Read/write playback position in frames.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Volume</code></TableCell>
              <TableCell><code>float</code></TableCell>
              <TableCell>Audio volume in [0, 1].</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Muted</code></TableCell>
              <TableCell><code>bool</code></TableCell>
              <TableCell>Read/write mute toggle.</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>PlaybackSpeed</code></TableCell>
              <TableCell><code>float</code></TableCell>
              <TableCell>Speed multiplier (1 = normal).</TableCell>
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
            <TableRow>
              <TableCell><code>Seek(double seconds)</code></TableCell>
              <TableCell><code>void</code></TableCell>
              <TableCell>Seek to a position; <code>OnSeekCompleted</code> fires when ready.</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        ScaleMode values
      </Typography>
      <Typography component="ul" variant="body2">
        <li><code>scaleToFit</code> — fit inside, preserve aspect (default).</li>
        <li><code>scaleAndCrop</code> — fill, preserve aspect, crop overflow.</li>
        <li><code>stretchToFill</code> — fill, ignore aspect.</li>
      </Typography>
    </Box>
  </Box>
)
