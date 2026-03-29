import type { FC } from 'react'
import {
  Alert,
  Box,
  Chip,
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
import {
  EXAMPLE_BASIC,
  EXAMPLE_RELATIVE,
  EXAMPLE_SHORTHAND,
  EXAMPLE_INLINE,
  EXAMPLE_USS,
} from './AssetsPage.example'

const section = { mt: 4 }

export const AssetsPage: FC = () => (
  <Box>
    <Typography variant="h4" component="h1" gutterBottom>
      Assets &amp; Stylesheets
    </Typography>
    <Typography variant="body1" paragraph>
      UITKX provides built-in helpers for loading Unity assets (textures, sprites,
      audio clips, fonts, materials, and more) directly from your <code>.uitkx</code> files.
      Paths are resolved at compile time, validated by diagnostics, and served from
      a lightweight asset registry at runtime.
    </Typography>

    {/* ── Asset<T> ─────────────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="asset-t">
        Asset&lt;T&gt;()
      </Typography>
      <Typography variant="body1" paragraph>
        Use <code>Asset&lt;T&gt;("path")</code> to load any Unity asset by path.
        The source generator resolves relative paths (starting with <code>./</code> or{' '}
        <code>../</code>) relative to the <code>.uitkx</code> file's location and
        registers them in the asset registry automatically.
      </Typography>
      <CodeBlock language="tsx" code={EXAMPLE_BASIC} />
    </Box>

    {/* ── Relative paths ───────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="relative-paths">
        Relative Paths
      </Typography>
      <Typography variant="body1" paragraph>
        Paths starting with <code>./</code> or <code>../</code> are resolved
        relative to the <code>.uitkx</code> file. This keeps asset references
        co-located with components — move the folder and everything still works.
      </Typography>
      <CodeBlock language="tsx" code={EXAMPLE_RELATIVE} />
    </Box>

    {/* ── Ast<T> shorthand ─────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="shorthand">
        Ast&lt;T&gt;() Shorthand
      </Typography>
      <Typography variant="body1" paragraph>
        <code>Ast&lt;T&gt;()</code> is a shorter alias for <code>Asset&lt;T&gt;()</code>.
        They are identical — use whichever reads better in your code.
      </Typography>
      <CodeBlock language="tsx" code={EXAMPLE_SHORTHAND} />
    </Box>

    {/* ── Inline usage ─────────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="inline">
        Inline Usage
      </Typography>
      <Typography variant="body1" paragraph>
        You can call <code>Asset&lt;T&gt;()</code> directly inside attribute
        expressions — no setup variable needed.
      </Typography>
      <CodeBlock language="tsx" code={EXAMPLE_INLINE} />
    </Box>

    {/* ── @uss directive ───────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="uss-directive">
        @uss Directive
      </Typography>
      <Typography variant="body1" paragraph>
        Use <code>@uss "path"</code> in the preamble to attach a USS (Unity Style
        Sheet) to your component. The stylesheet is loaded from the asset registry
        and applied to the root element at render time. Relative paths work the same
        way as <code>Asset&lt;T&gt;()</code>.
      </Typography>
      <CodeBlock language="tsx" code={EXAMPLE_USS} />
      <Typography variant="body2" paragraph sx={{ mt: 1, opacity: 0.7 }}>
        Multiple <code>@uss</code> directives are supported — each stylesheet is
        applied in order.
      </Typography>
    </Box>

    {/* ── Auto-import ──────────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="auto-import">
        Automatic Texture Import
      </Typography>
      <Typography variant="body1" paragraph>
        When the editor sync encounters an image reference, it automatically configures
        the TextureImporter — no manual import settings needed.
      </Typography>
      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Reference</strong></TableCell>
              <TableCell><strong>Import Mode</strong></TableCell>
              <TableCell><strong>Use Case</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><code>Asset&lt;Sprite&gt;("./icon.png")</code></TableCell>
              <TableCell>Sprite (2D and UI)</TableCell>
              <TableCell>UI icons, sprites, atlas entries</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>Asset&lt;Texture2D&gt;("./bg.png")</code></TableCell>
              <TableCell>Default</TableCell>
              <TableCell>Background images, textures</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
      <Alert severity="info" sx={{ mb: 1 }}>
        The import mode is set the first time the asset is encountered during editor sync.
        Changing the type in your <code>.uitkx</code> file will update the import settings on the next save.
      </Alert>
    </Box>

    {/* ── Diagnostics ──────────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="diagnostics">
        Diagnostics
      </Typography>
      <Typography variant="body1" paragraph>
        The source generator and LSP server validate asset references at compile
        time and in the IDE. You get immediate feedback for missing files and
        type mismatches.
      </Typography>
      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Code</strong></TableCell>
              <TableCell><strong>Source</strong></TableCell>
              <TableCell><strong>Severity</strong></TableCell>
              <TableCell><strong>Description</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><Chip label="UITKX0022" size="small" color="error" variant="outlined" /></TableCell>
              <TableCell>Source Generator</TableCell>
              <TableCell>Error</TableCell>
              <TableCell>File not found — the referenced file does not exist on disk</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><Chip label="UITKX0023" size="small" color="error" variant="outlined" /></TableCell>
              <TableCell>Source Generator</TableCell>
              <TableCell>Error</TableCell>
              <TableCell>Type mismatch — file extension is incompatible with the requested type</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><Chip label="UITKX0120" size="small" color="warning" variant="outlined" /></TableCell>
              <TableCell>LSP (IDE)</TableCell>
              <TableCell>Error</TableCell>
              <TableCell>File not found — real-time squiggle in the editor</TableCell>
            </TableRow>
            <TableRow>
              <TableCell><Chip label="UITKX0121" size="small" color="warning" variant="outlined" /></TableCell>
              <TableCell>LSP (IDE)</TableCell>
              <TableCell>Error</TableCell>
              <TableCell>Type mismatch — real-time squiggle in the editor</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
      <Typography variant="h6" component="h3" gutterBottom sx={{ mt: 2 }}>
        Examples
      </Typography>
      <CodeBlock language="tsx" code={`Asset<Texture2D>("./missing.png")   // UITKX0022 — file does not exist\nAsset<AudioClip>("./bg.png")        // UITKX0023 — AudioClip vs .png`} />
    </Box>

    {/* ── Supported types ──────────────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="supported-types">
        Supported File Types
      </Typography>
      <Typography variant="body1" paragraph>
        The type validation system knows which asset types are valid for each file
        extension. Using an incompatible combination triggers{' '}
        <Chip label="UITKX0023" size="small" variant="outlined" /> /{' '}
        <Chip label="UITKX0121" size="small" variant="outlined" />.
      </Typography>
      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Extensions</strong></TableCell>
              <TableCell><strong>Valid Types</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><code>.png .jpg .jpeg .bmp .tga .psd .gif .tif .tiff .exr .hdr</code></TableCell>
              <TableCell><code>Sprite</code>, <code>Texture2D</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.wav .mp3 .ogg .aiff .flac</code></TableCell>
              <TableCell><code>AudioClip</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.ttf .otf</code></TableCell>
              <TableCell><code>Font</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.mat</code></TableCell>
              <TableCell><code>Material</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.prefab</code></TableCell>
              <TableCell><code>GameObject</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.asset</code></TableCell>
              <TableCell><code>ScriptableObject</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.uss</code></TableCell>
              <TableCell><code>StyleSheet</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.anim</code></TableCell>
              <TableCell><code>AnimationClip</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.controller</code></TableCell>
              <TableCell><code>RuntimeAnimatorController</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.mesh</code></TableCell>
              <TableCell><code>Mesh</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.physicMaterial</code></TableCell>
              <TableCell><code>PhysicMaterial</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.shader</code></TableCell>
              <TableCell><code>Shader</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.compute</code></TableCell>
              <TableCell><code>ComputeShader</code></TableCell>
            </TableRow>
            <TableRow>
              <TableCell><code>.cubemap</code></TableCell>
              <TableCell><code>Cubemap</code></TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Box>

    {/* ── How the registry works ───────────────────────────── */}
    <Box sx={section}>
      <Typography variant="h5" component="h2" gutterBottom id="registry">
        How the Asset Registry Works
      </Typography>
      <Typography variant="body1" paragraph>
        Under the hood, asset paths are stored in a <code>UitkxAssetRegistry</code>{' '}
        ScriptableObject. At runtime, <code>Asset&lt;T&gt;()</code> reads directly
        from a static cache — no <code>Resources.Load</code> or Addressables overhead.
      </Typography>

      <Typography variant="h6" component="h3" gutterBottom>
        Sync triggers
      </Typography>
      <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell><strong>Trigger</strong></TableCell>
              <TableCell><strong>When</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell><code>.uitkx</code> file save</TableCell>
              <TableCell>Immediately after saving in the editor</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Domain reload</TableCell>
              <TableCell>On script compilation / entering Play mode</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Full project rescan</TableCell>
              <TableCell>On import or via the Fiber menu</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>HMR compilation</TableCell>
              <TableCell>On every hot-reload cycle</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>

      <Alert severity="success">
        The registry is fully automatic — you never need to manually register or
        update asset entries.
      </Alert>
    </Box>
  </Box>
)
