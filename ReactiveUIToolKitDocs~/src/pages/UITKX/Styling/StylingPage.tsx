import type { FC } from 'react'
import {
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
} from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../GettingStarted/GettingStartedPage.style'
import {
  EXAMPLE_OLD_STYLE,
  EXAMPLE_NEW_STYLE,
  EXAMPLE_CSS_HELPERS,
  EXAMPLE_IMPORT,
  EXAMPLE_LAYOUT,
  EXAMPLE_POSITIONING,
  EXAMPLE_COLORS,
  EXAMPLE_BORDERS,
  EXAMPLE_TEXT,
  EXAMPLE_BACKGROUND,
  EXAMPLE_TRANSFORMS,
  EXAMPLE_CONDITIONAL,
  EXAMPLE_INLINE,
  EXAMPLE_BOTH_APIs,
  EXAMPLE_ENUM_TABLE,
  EXAMPLE_LENGTH_HELPERS,
} from './StylingPage.example'

export const StylingPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Styling
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit provides a <strong>typed <code>Style</code> class</strong> with compile-time
      checked properties that map directly to Unity UI Toolkit&apos;s inline style system. A
      companion <strong><code>CssHelpers</code></strong> static class provides terse shortcuts for
      lengths, colors, and enum values.
    </Typography>

    <Alert severity="info" sx={{ mb: 3 }}>
      Both <code>Style</code> and <code>CssHelpers</code> live in the{' '}
      <code>ReactiveUITK.Props.Typed</code> namespace.
    </Alert>

    {/* ── Setup ─────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      Setup
    </Typography>
    <Typography variant="body1" paragraph>
      Add these directives at the top of your <code>.uitkx</code> file or companion <code>.cs</code>:
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_IMPORT} />

    {/* ── Before / After ────────────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      Before &amp; After
    </Typography>
    <Typography variant="body1" paragraph>
      The old tuple-based syntax is still supported but the new typed properties give you
      IntelliSense, compile errors on typos, and type checking on values:
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_OLD_STYLE} />
    <CodeBlock language="tsx" code={EXAMPLE_NEW_STYLE} />
    <CodeBlock language="tsx" code={EXAMPLE_CSS_HELPERS} />

    {/* ── Type categories ───────────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      Style property types
    </Typography>
    <Typography variant="body1" paragraph>
      Each property on <code>Style</code> accepts a specific Unity type. The compiler rejects
      mismatches immediately:
    </Typography>
    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>Category</strong></TableCell>
            <TableCell><strong>Type</strong></TableCell>
            <TableCell><strong>Examples</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell>Layout &amp; spacing</TableCell>
            <TableCell><code>StyleLength</code></TableCell>
            <TableCell>Width, Height, Margin, Padding, FlexBasis, FontSize, BorderRadius</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Flex &amp; opacity</TableCell>
            <TableCell><code>StyleFloat</code></TableCell>
            <TableCell>FlexGrow, FlexShrink, Opacity, BorderWidth</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Colors</TableCell>
            <TableCell><code>Color</code></TableCell>
            <TableCell>TextColor, BackgroundColor, BorderColor (9 total)</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Enum styles</TableCell>
            <TableCell>Unity enums</TableCell>
            <TableCell>FlexDirection, JustifyContent, Position, Display, TextAlign (15 total)</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Background</TableCell>
            <TableCell>Compound structs</TableCell>
            <TableCell>BackgroundRepeat, BackgroundPositionX/Y, BackgroundSize</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Transforms</TableCell>
            <TableCell><code>float</code> / struct</TableCell>
            <TableCell>Rotate, Scale, Translate, TransformOrigin</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Assets</TableCell>
            <TableCell><code>Texture2D</code> / <code>Font</code></TableCell>
            <TableCell>BackgroundImage, FontFamily</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <Typography variant="body2" paragraph>
      <code>StyleLength</code> has implicit conversions from <code>float</code>, <code>int</code>,{' '}
      <code>Length</code>, and <code>StyleKeyword</code> — so <code>Width = 100f</code> and{' '}
      <code>Width = Pct(50)</code> and <code>Width = Auto</code> all work.
    </Typography>

    {/* ── Examples by category ──────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      Layout &amp; flexbox
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_LAYOUT} />

    <Typography variant="h5" component="h2" gutterBottom>
      Positioning
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_POSITIONING} />

    <Typography variant="h5" component="h2" gutterBottom>
      Colors
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_COLORS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Borders
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_BORDERS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Text
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_TEXT} />

    <Typography variant="h5" component="h2" gutterBottom>
      Background (advanced)
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_BACKGROUND} />

    <Typography variant="h5" component="h2" gutterBottom>
      Transforms
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_TRANSFORMS} />

    {/* ── Patterns ──────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      Conditional styles
    </Typography>
    <Typography variant="body1" paragraph>
      <code>Style</code> is a plain C# object — use ternaries, if/else, or any expression:
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_CONDITIONAL} />

    <Typography variant="h5" component="h2" gutterBottom>
      Inline styles
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_INLINE} />

    {/* ── CssHelpers reference ──────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      CssHelpers reference
    </Typography>
    <Typography variant="body1" paragraph>
      Import via <code>using static ReactiveUITK.Props.Typed.CssHelpers;</code> to use these
      directly without qualification:
    </Typography>
    <CodeBlock language="text" code={EXAMPLE_LENGTH_HELPERS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Enum shortcuts
    </Typography>
    <Typography variant="body1" paragraph>
      All enum values are available as static properties with concise names:
    </Typography>
    <CodeBlock language="text" code={EXAMPLE_ENUM_TABLE} />

    {/* ── Dual API ──────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      Dual API — typed &amp; untyped
    </Typography>
    <Typography variant="body1" paragraph>
      The typed properties are the recommended path. The old tuple syntax{' '}
      <code>(StyleKeys.Key, value)</code> remains available as an escape hatch for edge cases
      (e.g. keys not yet exposed as typed properties):
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_BOTH_APIs} />
  </Box>
)
