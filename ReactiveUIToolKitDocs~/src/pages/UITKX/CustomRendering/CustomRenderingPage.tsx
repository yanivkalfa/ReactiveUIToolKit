import type { FC } from 'react'
import {
  Alert,
  Box,
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
  CUSTOM_RENDERING_HELPERS_EXAMPLE,
  CUSTOM_RENDERING_PAINTER_EXAMPLE,
  CUSTOM_RENDERING_RAW_MESH_EXAMPLE,
  CUSTOM_RENDERING_REDRAW_KEY_EXAMPLE,
  CUSTOM_RENDERING_SIGNATURE_EXAMPLE,
} from './CustomRenderingPage.example'

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 2 },
} as const

/* ------------------------------------------------------------------ */
/*  Attribute reference                                               */
/* ------------------------------------------------------------------ */

type AttrRow = { name: string; type: string; desc: string }

const attributes: AttrRow[] = [
  {
    name: 'onGenerateVisualContent',
    type: 'Action<MeshGenerationContext>',
    desc: 'Custom draw callback. Receives a MeshGenerationContext; use ctx.painter2D for vector drawing or ctx.Allocate for raw vertex/index meshes. Available on every element (inherited from BaseProps).',
  },
  {
    name: 'redrawKey',
    type: 'int',
    desc: 'Bump this value to force a repaint without changing the callback reference. Pair it with a stable callback (useMemo / useStableCallback). Default 0 never forces a repaint on its own.',
  },
]

const AttrTable: FC = () => (
  <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell><strong>Attribute</strong></TableCell>
          <TableCell><strong>Type</strong></TableCell>
          <TableCell><strong>Description</strong></TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {attributes.map((a) => (
          <TableRow key={a.name}>
            <TableCell><code>{a.name}</code></TableCell>
            <TableCell><code>{a.type}</code></TableCell>
            <TableCell>{a.desc}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </TableContainer>
)

/* ------------------------------------------------------------------ */
/*  Main page                                                         */
/* ------------------------------------------------------------------ */

export const CustomRenderingPage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Custom Rendering
    </Typography>
    <Typography variant="body1" paragraph>
      Every element accepts an <code>onGenerateVisualContent</code> attribute - a
      declarative binding for Unity UI Toolkit&apos;s{' '}
      <code>VisualElement.generateVisualContent</code> delegate. Use it to draw
      your own vector shapes, charts, gauges, or raw meshes directly into an
      element, while keeping the rest of your UI fully reactive. The callback
      receives a <code>MeshGenerationContext</code> and is inherited from{' '}
      <code>BaseProps</code>, so it works on <code>VisualElement</code>,{' '}
      <code>Button</code>, and every other built-in element.
    </Typography>

    {/* ── Attribute reference ──────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Attributes
      </Typography>
      <AttrTable />
      <CodeBlock language="jsx" code={CUSTOM_RENDERING_SIGNATURE_EXAMPLE} />
    </Box>

    {/* ── How repaints work ────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        How repainting works
      </Typography>
      <Typography variant="body1" paragraph>
        Unity only re-runs <code>generateVisualContent</code> when the element is
        marked dirty. ReactiveUIToolKit handles that for you: the element
        repaints automatically whenever the callback{' '}
        <strong>reference changes</strong> between renders, or whenever{' '}
        <code>redrawKey</code> changes. A fresh inline lambda is a new reference
        every render, so by default the canvas redraws each time its owner
        re-renders - the same reactive model as a normal element.
      </Typography>
      <Alert severity="info" sx={{ mb: 2 }}>
        Treat the element as <strong>read-only</strong> inside the callback. It
        runs during Unity&apos;s paint phase, not during your render, so do not
        mutate state, set styles, or add children from inside it - read{' '}
        <code>ctx.visualElement.contentRect</code> and draw.
      </Alert>
    </Box>

    {/* ── Painter2D ────────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Vector drawing with Painter2D
      </Typography>
      <Typography variant="body1" paragraph>
        <code>ctx.painter2D</code> is a retained vector API: build a path with{' '}
        <code>BeginPath</code> / <code>MoveTo</code> / <code>LineTo</code> /{' '}
        <code>Arc</code> / <code>BezierCurveTo</code>, set{' '}
        <code>strokeColor</code>, <code>fillColor</code>, and{' '}
        <code>lineWidth</code>, then call <code>Stroke()</code> or{' '}
        <code>Fill()</code>. Drive it from component state and the drawing
        updates reactively.
      </Typography>
      <CodeBlock language="jsx" code={CUSTOM_RENDERING_PAINTER_EXAMPLE} />
    </Box>

    {/* ── Companion file best practice ─────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Keep draw bodies in a companion file
      </Typography>
      <Typography variant="body1" paragraph>
        The example above calls <code>DrawHelpers.Polygon</code> rather than
        inlining a multi-statement lambda. Keeping draw bodies in a plain-C#
        companion class is the recommended pattern: the markup stays a simple
        single-expression lambda, the draw code gets full IntelliSense, and the{' '}
        <code>.uitkx</code> file formats cleanly. Each method just needs to match{' '}
        <code>Action&lt;MeshGenerationContext&gt;</code>.
      </Typography>
      <CodeBlock language="jsx" code={CUSTOM_RENDERING_HELPERS_EXAMPLE} />
    </Box>

    {/* ── Raw meshes ───────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Raw meshes with ctx.Allocate
      </Typography>
      <Typography variant="body1" paragraph>
        For full control over geometry, call{' '}
        <code>ctx.Allocate(vertexCount, indexCount)</code> to get a{' '}
        <code>MeshWriteData</code>, then fill it with{' '}
        <code>SetAllVertices</code> / <code>SetAllIndices</code>. Set each{' '}
        <code>Vertex.position</code> (use <code>Vertex.nearZ</code> for the z
        component) and <code>Vertex.tint</code>.
      </Typography>
      <CodeBlock language="jsx" code={CUSTOM_RENDERING_RAW_MESH_EXAMPLE} />
    </Box>

    {/* ── redrawKey + stable callbacks ─────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Controlling repaints with redrawKey
      </Typography>
      <Typography variant="body1" paragraph>
        When drawing is expensive, stabilise the callback so it is{' '}
        <strong>not</strong> reallocated each render -{' '}
        <code>useMemo</code> with empty dependencies, or{' '}
        <code>useStableCallback</code>. With a stable reference the element no
        longer repaints on every render; instead, bump <code>redrawKey</code>{' '}
        exactly when you want a fresh frame.
      </Typography>
      <CodeBlock language="jsx" code={CUSTOM_RENDERING_REDRAW_KEY_EXAMPLE} />
      <Alert severity="info" sx={{ mb: 2 }}>
        For a continuous animation that repaints without re-rendering, capture
        the element with a <code>ref</code> and call{' '}
        <code>MarkDirtyRepaint()</code> from a ticker, or pair it with the
        built-in animation hooks. <code>redrawKey</code> is for discrete,
        on-demand repaints.
      </Alert>
    </Box>

    {/* ── Player-safe ──────────────────────────────────────────── */}
    <Box sx={styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Runtime and player builds
      </Typography>
      <Typography variant="body1" paragraph>
        <code>MeshGenerationContext</code>, <code>Painter2D</code>, and{' '}
        <code>Vertex</code> all live in <code>UnityEngine.UIElements</code> -
        runtime types, not editor-only. <code>onGenerateVisualContent</code> and{' '}
        <code>redrawKey</code> ship in player builds with no scripting-define
        gating, so custom drawing behaves identically in the Editor and in a
        built game.
      </Typography>
      <Alert severity="info" sx={{ mb: 2 }}>
        The shipped sample <code>CustomDrawDemoFunc</code> demonstrates all three
        techniques. Open it from the Unity menu under{' '}
        <strong>ReactiveUITK &rarr; Demos &rarr; Custom Drawing</strong>.
      </Alert>
    </Box>
  </Box>
)
