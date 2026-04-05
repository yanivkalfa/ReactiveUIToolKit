import { useMemo, useState } from 'react'
import type { FC } from 'react'
import {
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Box,
  Chip,
  TextField,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  Link,
} from '@mui/material'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { SUPPORTED_VERSIONS, FLOOR_VERSION } from '../../../versionManifest'
import { useSelectedVersion } from '../../../contexts/VersionContext'
import { isAvailableIn } from '../../../versionManifest'
import Styles from '../../GettingStarted/GettingStartedPage.style'
import { STYLE_PROPERTY_CATALOG } from './stylePropertyCatalog'
import type { PropertyCard } from './stylePropertyCatalog'
import {
  EXAMPLE_IMPORT,
  EXAMPLE_BOTH_APIs,
  EXAMPLE_CONDITIONAL,
  EXAMPLE_INLINE,
  EXAMPLE_USS_BASIC,
  EXAMPLE_USS_FILE,
  EXAMPLE_USS_MULTIPLE,
  EXAMPLE_USS_COMBINED,
} from './StylingPage.example'

/** Build a version badge label like "6.3+" or "6.2" for floor. */
const versionLabel = (sinceUnity?: string): string => {
  if (!sinceUnity) return FLOOR_VERSION.label
  const info = SUPPORTED_VERSIONS.find((v) => v.version === sinceUnity)
  return info ? `${info.label}+` : `${sinceUnity}+`
}

/** Single collapsible property card */
const PropertyCardView: FC<{ card: PropertyCard }> = ({ card }) => {
  const isFloor = !card.sinceUnity
  const label = versionLabel(card.sinceUnity)
  return (
    <Accordion disableGutters variant="outlined" sx={{ mb: 1 }}>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="subtitle1" component="h3" sx={{ fontWeight: 600 }}>
            <code>{card.name}</code>
          </Typography>
          <Chip
            label={label}
            size="small"
            color={isFloor ? 'default' : 'info'}
            variant="outlined"
          />
          {card.shorthand && <Chip label="shorthand" size="small" variant="outlined" />}
        </Box>
      </AccordionSummary>
      <AccordionDetails>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {card.description}
        </Typography>
        <Typography variant="body2" sx={{ mb: 1 }}>
          Type: <code>{card.type}</code>
        </Typography>
        <CodeBlock language="jsx" code={`// Typed\nnew Style { ${card.typedExample} }\n\n// Untyped\nnew Style { ${card.untypedExample} }`} />
        {card.helpers && card.helpers.length > 0 && (
          <Typography variant="body2" sx={{ mt: 1 }}>
            CssHelpers:{' '}
            {card.helpers.map((h, i) => (
              <span key={h}>{i > 0 && ', '}<code>{h}</code></span>
            ))}
          </Typography>
        )}
      </AccordionDetails>
    </Accordion>
  )
}

export const StylingPage: FC = () => {
  const { selectedVersion } = useSelectedVersion()
  const [search, setSearch] = useState('')

  // Filter to properties available for the selected Unity version,
  // flat sorted by version then alphabetically, then filtered by search.
  const cards = useMemo(() => {
    const q = search.toLowerCase().trim()
    return STYLE_PROPERTY_CATALOG
      .filter((c) => isAvailableIn(c.sinceUnity ? { sinceUnity: c.sinceUnity } : undefined, selectedVersion))
      .sort((a, b) => {
        const va = a.sinceUnity ?? ''
        const vb = b.sinceUnity ?? ''
        if (va !== vb) return va.localeCompare(vb)
        return a.key.localeCompare(b.key)
      })
      .filter((c) => !q || c.name.toLowerCase().includes(q) || c.key.toLowerCase().includes(q) || c.description.toLowerCase().includes(q))
  }, [selectedVersion, search])

  return (
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
      <code>StyleKeys</code> and <code>CssHelpers</code> are auto-imported in{' '}
      <code>.uitkx</code> files. For companion <code>.cs</code> files, add:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_IMPORT} />

    {/* ── Two approaches ────────────────────────────────────── */}
    <Typography variant="h5" component="h2" gutterBottom>
      Two approaches
    </Typography>
    <Typography variant="body1" paragraph>
      Every property can be set in two ways. Both are valid and can be mixed in the same style object:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_BOTH_APIs} />

    {/* ── Jump links ────────────────────────────────────────── */}
    <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2, mt: 4 }}>
      <Chip label="Patterns" component="a" href="#patterns" clickable size="small" />
      <Chip label="Type reference" component="a" href="#type-reference" clickable size="small" />
      <Chip label="CssHelpers reference" component="a" href="#csshelpers-reference" clickable size="small" />
      <Chip label="Enum shortcuts" component="a" href="#enum-shortcuts" clickable size="small" />
      <Chip label="Compound helpers" component="a" href="#compound-helpers" clickable size="small" />
    </Box>

    {/* ── Property reference ────────────────────────────────── */}
    <Typography variant="h4" component="h2" gutterBottom>
      Property reference
    </Typography>
    <Typography variant="body1" paragraph>
      Every style property available for Unity {SUPPORTED_VERSIONS.find(v => v.version === selectedVersion)?.label ?? selectedVersion}.
      Click a card to see typed and untyped syntax.
    </Typography>

    <TextField
      size="small"
      placeholder="Filter properties…"
      value={search}
      onChange={(e) => setSearch(e.target.value)}
      sx={{ mb: 2, maxWidth: 360 }}
      fullWidth
    />

    {cards.map((card) => (
      <PropertyCardView key={card.key} card={card} />
    ))}

    {cards.length === 0 && (
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        No properties match &quot;{search}&quot;.
      </Typography>
    )}

    {/* ── Patterns ──────────────────────────────────────────── */}
    <Typography id="patterns" variant="h4" component="h2" gutterBottom sx={{ mt: 4 }}>
      Patterns
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Conditional styles
    </Typography>
    <Typography variant="body1" paragraph>
      <code>Style</code> is a plain C# object — use ternaries, if/else, or any expression:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_CONDITIONAL} />

    <Typography variant="h5" component="h2" gutterBottom>
      Inline styles
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_INLINE} />

    {/* ── Type reference ────────────────────────────────────── */}
    <Typography id="type-reference" variant="h4" component="h2" gutterBottom sx={{ mt: 4 }}>
      Type reference
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
            <TableCell>Color, BackgroundColor, BorderColor (9 total)</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Enum styles</TableCell>
            <TableCell>Unity enums</TableCell>
            <TableCell>FlexDirection, JustifyContent, Position, Display, TextAlign</TableCell>
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
          <TableRow>
            <TableCell>Transitions</TableCell>
            <TableCell><code>StyleList&lt;T&gt;</code></TableCell>
            <TableCell>TransitionProperty, TransitionDuration, TransitionDelay, TransitionTimingFunction</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <Typography variant="body2" paragraph>
      <code>StyleLength</code> has implicit conversions from <code>float</code>, <code>int</code>,{' '}
      <code>Length</code>, and <code>StyleKeyword</code> — so <code>Width = 100f</code> and{' '}
      <code>Width = Pct(50)</code> and <code>Width = Auto</code> all work.
    </Typography>

    {/* ── CssHelpers reference ──────────────────────────────── */}
    <Typography id="csshelpers-reference" variant="h4" component="h2" gutterBottom sx={{ mt: 4 }}>
      CssHelpers reference
    </Typography>
    <Typography variant="body1" paragraph>
      <code>CssHelpers</code> is <strong>auto-imported</strong> in <code>.uitkx</code> files —
      all shortcuts are available without any <code>@using</code> directive.
      In companion <code>.cs</code> files, add{' '}
      <code>using static ReactiveUITK.Props.Typed.CssHelpers;</code>.
    </Typography>

    <Alert severity="warning" sx={{ mb: 3 }}>
      <strong>Do not</strong> add <code>@using UnityEngine.UIElements</code> to{' '}
      <code>.uitkx</code> files — it causes naming conflicts with{' '}
      <code>using static StyleKeys</code> constants like{' '}
      <code>FlexDirection</code> and <code>Position</code>. The SG already
      imports the UIElements types it needs via targeted aliases.
    </Alert>

    <Typography variant="h6" gutterBottom>Length helpers</Typography>
    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>Helper</strong></TableCell>
            <TableCell><strong>Result</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow><TableCell><code>Pct(50)</code></TableCell><TableCell>50% (<code>Length.Percent</code>)</TableCell></TableRow>
          <TableRow><TableCell><code>Px(100)</code></TableCell><TableCell>100px (<code>Length.Pixel</code>)</TableCell></TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    <Typography variant="h6" gutterBottom>Style keywords</Typography>
    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>Shortcut</strong></TableCell>
            <TableCell><strong>Maps to</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow><TableCell><code>StyleAuto</code></TableCell><TableCell><code>StyleKeyword.Auto</code></TableCell></TableRow>
          <TableRow><TableCell><code>StyleNone</code></TableCell><TableCell><code>StyleKeyword.None</code></TableCell></TableRow>
          <TableRow><TableCell><code>StyleInitial</code></TableCell><TableCell><code>StyleKeyword.Initial</code></TableCell></TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    <Typography variant="h6" gutterBottom>Color helpers</Typography>
    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>Helper</strong></TableCell>
            <TableCell><strong>Description</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow><TableCell><code>ColorWhite</code>, <code>ColorBlack</code>, <code>ColorRed</code>, <code>ColorGreen</code>, <code>ColorBlue</code>, <code>ColorYellow</code>, <code>ColorCyan</code>, <code>ColorMagenta</code>, <code>ColorGrey</code>, <code>ColorTransparent</code></TableCell><TableCell>Named color constants</TableCell></TableRow>
          <TableRow><TableCell><code>Hex(&quot;#FF0000&quot;)</code></TableCell><TableCell>Color from hex string</TableCell></TableRow>
          <TableRow><TableCell><code>Rgba(255, 0, 0)</code></TableCell><TableCell>Color from 0–255 byte values</TableCell></TableRow>
          <TableRow><TableCell><code>Rgba(1f, 0f, 0f, 0.5f)</code></TableCell><TableCell>Color from 0–1 float values</TableCell></TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    <Typography id="enum-shortcuts" variant="h5" component="h2" gutterBottom>
      Enum shortcuts
    </Typography>
    <Typography variant="body1" paragraph>
      All enum values are available as static properties with concise names:
    </Typography>

    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>Enum</strong></TableCell>
            <TableCell><strong>Shortcuts</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow><TableCell><code>FlexDirection</code></TableCell><TableCell><code>FlexRow</code>, <code>FlexColumn</code>, <code>FlexRowReverse</code>, <code>FlexColumnReverse</code></TableCell></TableRow>
          <TableRow><TableCell><code>Justify</code></TableCell><TableCell><code>JustifyStart</code>, <code>JustifyEnd</code>, <code>JustifyCenter</code>, <code>JustifySpaceBetween</code>, <code>JustifySpaceAround</code>, <code>JustifySpaceEvenly</code></TableCell></TableRow>
          <TableRow><TableCell><code>Align</code></TableCell><TableCell><code>AlignStart</code>, <code>AlignEnd</code>, <code>AlignCenter</code>, <code>AlignStretch</code>, <code>AlignAuto</code></TableCell></TableRow>
          <TableRow><TableCell><code>Wrap</code></TableCell><TableCell><code>WrapOn</code>, <code>WrapOff</code>, <code>WrapReverse</code></TableCell></TableRow>
          <TableRow><TableCell><code>Position</code></TableCell><TableCell><code>PosRelative</code>, <code>PosAbsolute</code></TableCell></TableRow>
          <TableRow><TableCell><code>DisplayStyle</code></TableCell><TableCell><code>DisplayFlex</code>, <code>DisplayNone</code></TableCell></TableRow>
          <TableRow><TableCell><code>Visibility</code></TableCell><TableCell><code>VisVisible</code>, <code>VisHidden</code></TableCell></TableRow>
          <TableRow><TableCell><code>Overflow</code></TableCell><TableCell><code>OverflowVisible</code>, <code>OverflowHidden</code></TableCell></TableRow>
          <TableRow><TableCell><code>WhiteSpace</code></TableCell><TableCell><code>WsNormal</code>, <code>WsNowrap</code>, <code>WsPre</code>, <code>WsPreWrap</code></TableCell></TableRow>
          <TableRow><TableCell><code>TextOverflow</code></TableCell><TableCell><code>TextClip</code>, <code>TextEllipsis</code></TableCell></TableRow>
          <TableRow><TableCell><code>TextAnchor</code></TableCell><TableCell><code>TextUpperLeft</code>, <code>TextUpperCenter</code>, <code>TextUpperRight</code>, <code>TextMiddleLeft</code>, <code>TextMiddleCenter</code>, <code>TextMiddleRight</code>, <code>TextLowerLeft</code>, <code>TextLowerCenter</code>, <code>TextLowerRight</code></TableCell></TableRow>
          <TableRow><TableCell><code>FontStyle</code></TableCell><TableCell><code>FontNormal</code>, <code>FontBold</code>, <code>FontItalic</code>, <code>FontBoldItalic</code></TableCell></TableRow>
          <TableRow><TableCell><code>TextOverflowPosition</code></TableCell><TableCell><code>TextOverflowStart</code>, <code>TextOverflowMiddle</code>, <code>TextOverflowEnd</code></TableCell></TableRow>
          <TableRow><TableCell><code>TextAutoSizeMode</code></TableCell><TableCell><code>AutoSizeNone</code>, <code>AutoSizeBestFit</code></TableCell></TableRow>
          <TableRow><TableCell><code>PickingMode</code></TableCell><TableCell><code>PickPosition</code>, <code>PickIgnore</code></TableCell></TableRow>
          <TableRow><TableCell><code>SelectionType</code></TableCell><TableCell><code>SelectNone</code>, <code>SelectSingle</code>, <code>SelectMultiple</code></TableCell></TableRow>
          <TableRow><TableCell><code>ScrollerVisibility</code></TableCell><TableCell><code>ScrollerAuto</code>, <code>ScrollerVisible</code>, <code>ScrollerHidden</code></TableCell></TableRow>
          <TableRow><TableCell><code>LanguageDirection</code></TableCell><TableCell><code>DirInherit</code>, <code>DirLTR</code>, <code>DirRTL</code></TableCell></TableRow>
          <TableRow><TableCell><code>SliderDirection</code></TableCell><TableCell><code>SliderHorizontal</code>, <code>SliderVertical</code></TableCell></TableRow>
          <TableRow><TableCell><code>ScrollViewMode</code></TableCell><TableCell><code>ScrollVertical</code>, <code>ScrollHorizontal</code>, <code>ScrollBoth</code></TableCell></TableRow>
          <TableRow><TableCell><code>ScaleMode</code></TableCell><TableCell><code>ScaleStretch</code>, <code>ScaleFit</code>, <code>ScaleCrop</code></TableCell></TableRow>
          <TableRow><TableCell><code>TwoPaneSplitViewOrientation</code></TableCell><TableCell><code>OrientHorizontal</code>, <code>OrientVertical</code></TableCell></TableRow>
          <TableRow><TableCell><code>ColumnSortingMode</code></TableCell><TableCell><code>SortNone</code>, <code>SortDefault</code>, <code>SortCustom</code></TableCell></TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    <Typography id="compound-helpers" variant="h5" component="h2" gutterBottom sx={{ mt: 3 }}>
      Compound struct helpers
    </Typography>
    <Typography variant="body1" paragraph>
      CssHelpers provides factory methods and presets for compound struct types,
      so you don&apos;t need verbose constructor calls:
    </Typography>

    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>Type</strong></TableCell>
            <TableCell><strong>Helpers</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow><TableCell><code>BackgroundRepeat</code></TableCell><TableCell><code>BgRepeat(x, y)</code>, <code>BgRepeatNone</code>, <code>BgRepeatBoth</code>, <code>BgRepeatX</code>, <code>BgRepeatY</code>, <code>BgRepeatSpace</code>, <code>BgRepeatRound</code></TableCell></TableRow>
          <TableRow><TableCell><code>BackgroundPosition</code></TableCell><TableCell><code>BgPos(keyword)</code>, <code>BgPos(keyword, offset)</code>, <code>BgPosCenter</code>, <code>BgPosTop</code>, <code>BgPosBottom</code>, <code>BgPosLeft</code>, <code>BgPosRight</code></TableCell></TableRow>
          <TableRow><TableCell><code>BackgroundSize</code></TableCell><TableCell><code>BgSize(x, y)</code>, <code>BgSizeCover</code>, <code>BgSizeContain</code></TableCell></TableRow>
          <TableRow><TableCell><code>TransformOrigin</code></TableCell><TableCell><code>Origin(x, y)</code>, <code>OriginCenter</code></TableCell></TableRow>
          <TableRow><TableCell><code>Translate</code></TableCell><TableCell><code>Xlate(x, y)</code></TableCell></TableRow>
          <TableRow><TableCell><code>EasingFunction</code></TableCell><TableCell><code>Easing(mode)</code>, <code>EaseDefault</code>, <code>EaseLinear</code>, <code>EaseIn</code>, <code>EaseOut</code>, <code>EaseInOut</code>, + sine/cubic/circ/elastic/back/bounce variants</TableCell></TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    {/* ── USS Stylesheets ───────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={{ mt: 6 }} gutterBottom id="uss-stylesheets">
      USS Stylesheets
    </Typography>
    <Typography variant="body1" paragraph>
      For static, class-based styling you can use standard Unity Style Sheets (.uss files)
      via the <code>@uss</code> directive. This is the same USS format Unity UI Toolkit
      uses — type selectors, class selectors, pseudo-classes, and all standard USS properties.
    </Typography>

    <Typography variant="h6" component="h3" sx={{ mt: 3 }} gutterBottom>
      Basic usage
    </Typography>
    <Typography variant="body1" paragraph>
      Add <code>@uss "path"</code> to the preamble (before the <code>component</code> keyword).
      Relative paths are resolved from the <code>.uitkx</code> file's location.
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_USS_BASIC} />

    <Typography variant="h6" component="h3" sx={{ mt: 3 }} gutterBottom>
      Example .uss file
    </Typography>
    <CodeBlock language="css" code={EXAMPLE_USS_FILE} />

    <Typography variant="h6" component="h3" sx={{ mt: 3 }} gutterBottom>
      Multiple stylesheets
    </Typography>
    <Typography variant="body1" paragraph>
      You can import multiple <code>@uss</code> files — they are applied in order,
      so later sheets can override earlier ones.
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_USS_MULTIPLE} />

    <Typography variant="h6" component="h3" sx={{ mt: 3 }} gutterBottom>
      Combining USS + typed Style
    </Typography>
    <Typography variant="body1" paragraph>
      USS is great for static layout and theming. The typed <code>Style</code> class
      is great for dynamic, state-driven values. Use both together —
      USS handles the baseline, <code>Style</code> handles the runtime overrides.
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_USS_COMBINED} />

    <Alert severity="info" sx={{ mt: 2 }}>
      <strong>HMR support:</strong> Saving a <code>.uss</code> file triggers
      hot-reload of all components that reference it via <code>@uss</code> —
      no domain reload needed.
    </Alert>

    <Alert severity="info" sx={{ mt: 2 }}>
      <strong>Specificity:</strong> Inline <code>style={'{...}'}</code> always wins over USS
      rules — matching standard CSS behavior. Use USS for static layout and theming, and
      inline <code>Style</code> for dynamic, state-driven overrides.
    </Alert>

    <Alert severity="info" sx={{ mt: 2 }}>
      <strong>Multiple classes:</strong> <code>className</code> accepts space-separated class names
      (e.g. <code>className=&quot;card dark-theme&quot;</code>). Each name is added
      via <code>AddToClassList</code>.
    </Alert>

    {/* ── Table of contents ─────────────────────────────────── */}
    <Paper variant="outlined" sx={{ p: 2, mt: 6 }}>
      <Typography variant="h6" gutterBottom>
        Table of contents
      </Typography>
      <Box component="ul" sx={{ m: 0, pl: 2 }}>
        <li><Link href="#patterns">Patterns</Link></li>
        <li><Link href="#type-reference">Type reference</Link></li>
        <li><Link href="#csshelpers-reference">CssHelpers reference</Link></li>
        <li><Link href="#enum-shortcuts">Enum shortcuts</Link></li>
        <li><Link href="#compound-helpers">Compound struct helpers</Link></li>
        <li><Link href="#uss-stylesheets">USS Stylesheets</Link></li>
      </Box>
    </Paper>
  </Box>
  )
}
