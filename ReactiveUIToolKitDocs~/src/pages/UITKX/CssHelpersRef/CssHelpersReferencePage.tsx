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

const styles = {
  root: { display: 'flex', flexDirection: 'column', gap: 2 },
  section: { mt: 2 },
} as const

type HelperRow = { name: string; returnType: string; value: string }

/* ------------------------------------------------------------------ */
/*  Data tables — mirrored from CssHelpers.cs                         */
/* ------------------------------------------------------------------ */

const lengthUnits: HelperRow[] = [
  { name: 'Pct(float|int)', returnType: 'Length', value: 'Percentage length' },
  { name: 'Px(float|int)', returnType: 'Length', value: 'Pixel length' },
]

const keywords: HelperRow[] = [
  { name: 'StyleAuto', returnType: 'StyleKeyword', value: 'Auto' },
  { name: 'StyleNone', returnType: 'StyleKeyword', value: 'None' },
  { name: 'StyleInitial', returnType: 'StyleKeyword', value: 'Initial' },
]

const flexDirection: HelperRow[] = [
  { name: 'FlexRow', returnType: 'FlexDirection', value: 'Row' },
  { name: 'FlexColumn', returnType: 'FlexDirection', value: 'Column' },
  { name: 'FlexRowReverse', returnType: 'FlexDirection', value: 'RowReverse' },
  { name: 'FlexColumnReverse', returnType: 'FlexDirection', value: 'ColumnReverse' },
]

const justify: HelperRow[] = [
  { name: 'JustifyStart', returnType: 'Justify', value: 'FlexStart' },
  { name: 'JustifyEnd', returnType: 'Justify', value: 'FlexEnd' },
  { name: 'JustifyCenter', returnType: 'Justify', value: 'Center' },
  { name: 'JustifySpaceBetween', returnType: 'Justify', value: 'SpaceBetween' },
  { name: 'JustifySpaceAround', returnType: 'Justify', value: 'SpaceAround' },
  { name: 'JustifySpaceEvenly', returnType: 'Justify', value: 'SpaceEvenly' },
]

const align: HelperRow[] = [
  { name: 'AlignStart', returnType: 'Align', value: 'FlexStart' },
  { name: 'AlignEnd', returnType: 'Align', value: 'FlexEnd' },
  { name: 'AlignCenter', returnType: 'Align', value: 'Center' },
  { name: 'AlignStretch', returnType: 'Align', value: 'Stretch' },
  { name: 'AlignAuto', returnType: 'Align', value: 'Auto' },
]

const wrap: HelperRow[] = [
  { name: 'WrapOn', returnType: 'Wrap', value: 'Wrap' },
  { name: 'WrapOff', returnType: 'Wrap', value: 'NoWrap' },
  { name: 'WrapReverse', returnType: 'Wrap', value: 'WrapReverse' },
]

const position: HelperRow[] = [
  { name: 'PosRelative', returnType: 'Position', value: 'Relative' },
  { name: 'PosAbsolute', returnType: 'Position', value: 'Absolute' },
]

const display: HelperRow[] = [
  { name: 'DisplayFlex', returnType: 'DisplayStyle', value: 'Flex' },
  { name: 'DisplayNone', returnType: 'DisplayStyle', value: 'None' },
]

const visibility: HelperRow[] = [
  { name: 'VisVisible', returnType: 'Visibility', value: 'Visible' },
  { name: 'VisHidden', returnType: 'Visibility', value: 'Hidden' },
]

const overflow: HelperRow[] = [
  { name: 'OverflowVisible', returnType: 'Overflow', value: 'Visible' },
  { name: 'OverflowHidden', returnType: 'Overflow', value: 'Hidden' },
]

const whiteSpace: HelperRow[] = [
  { name: 'WsNormal', returnType: 'WhiteSpace', value: 'Normal' },
  { name: 'WsNowrap', returnType: 'WhiteSpace', value: 'NoWrap' },
  { name: 'WsPre', returnType: 'WhiteSpace', value: 'Pre' },
  { name: 'WsPreWrap', returnType: 'WhiteSpace', value: 'PreWrap' },
]

const textOverflow: HelperRow[] = [
  { name: 'TextClip', returnType: 'TextOverflow', value: 'Clip' },
  { name: 'TextEllipsis', returnType: 'TextOverflow', value: 'Ellipsis' },
]

const textAlign: HelperRow[] = [
  { name: 'TextUpperLeft', returnType: 'TextAnchor', value: 'UpperLeft' },
  { name: 'TextUpperCenter', returnType: 'TextAnchor', value: 'UpperCenter' },
  { name: 'TextUpperRight', returnType: 'TextAnchor', value: 'UpperRight' },
  { name: 'TextMiddleLeft', returnType: 'TextAnchor', value: 'MiddleLeft' },
  { name: 'TextMiddleCenter', returnType: 'TextAnchor', value: 'MiddleCenter' },
  { name: 'TextMiddleRight', returnType: 'TextAnchor', value: 'MiddleRight' },
  { name: 'TextLowerLeft', returnType: 'TextAnchor', value: 'LowerLeft' },
  { name: 'TextLowerCenter', returnType: 'TextAnchor', value: 'LowerCenter' },
  { name: 'TextLowerRight', returnType: 'TextAnchor', value: 'LowerRight' },
]

const textOverflowPos: HelperRow[] = [
  { name: 'TextOverflowStart', returnType: 'TextOverflowPosition', value: 'Start' },
  { name: 'TextOverflowMiddle', returnType: 'TextOverflowPosition', value: 'Middle' },
  { name: 'TextOverflowEnd', returnType: 'TextOverflowPosition', value: 'End' },
]

const textAutoSize: HelperRow[] = [
  { name: 'AutoSizeNone', returnType: 'TextAutoSizeMode', value: 'None' },
  { name: 'AutoSizeBestFit', returnType: 'TextAutoSizeMode', value: 'BestFit' },
]

const fontStyle: HelperRow[] = [
  { name: 'FontBold', returnType: 'FontStyle', value: 'Bold' },
  { name: 'FontItalic', returnType: 'FontStyle', value: 'Italic' },
  { name: 'FontBoldItalic', returnType: 'FontStyle', value: 'BoldAndItalic' },
  { name: 'FontNormal', returnType: 'FontStyle', value: 'Normal' },
]

const pickingMode: HelperRow[] = [
  { name: 'PickPosition', returnType: 'PickingMode', value: 'Position' },
  { name: 'PickIgnore', returnType: 'PickingMode', value: 'Ignore' },
]

const selectionType: HelperRow[] = [
  { name: 'SelectNone', returnType: 'SelectionType', value: 'None' },
  { name: 'SelectSingle', returnType: 'SelectionType', value: 'Single' },
  { name: 'SelectMultiple', returnType: 'SelectionType', value: 'Multiple' },
]

const scrollerVis: HelperRow[] = [
  { name: 'ScrollerAuto', returnType: 'ScrollerVisibility', value: 'Auto' },
  { name: 'ScrollerVisible', returnType: 'ScrollerVisibility', value: 'AlwaysVisible' },
  { name: 'ScrollerHidden', returnType: 'ScrollerVisibility', value: 'Hidden' },
]

const langDirection: HelperRow[] = [
  { name: 'DirInherit', returnType: 'LanguageDirection', value: 'Inherit' },
  { name: 'DirLTR', returnType: 'LanguageDirection', value: 'LTR' },
  { name: 'DirRTL', returnType: 'LanguageDirection', value: 'RTL' },
]

const sliderDir: HelperRow[] = [
  { name: 'SliderHorizontal', returnType: 'string', value: '"horizontal"' },
  { name: 'SliderVertical', returnType: 'string', value: '"vertical"' },
]

const scrollMode: HelperRow[] = [
  { name: 'ScrollVertical', returnType: 'string', value: '"vertical"' },
  { name: 'ScrollHorizontal', returnType: 'string', value: '"horizontal"' },
  { name: 'ScrollBoth', returnType: 'string', value: '"verticalandhorizontal"' },
]

const imageScale: HelperRow[] = [
  { name: 'ScaleStretch', returnType: 'string', value: '"stretchfill"' },
  { name: 'ScaleFit', returnType: 'string', value: '"scaletofit"' },
  { name: 'ScaleCrop', returnType: 'string', value: '"scalefill"' },
]

const splitOrient: HelperRow[] = [
  { name: 'OrientHorizontal', returnType: 'string', value: '"horizontal"' },
  { name: 'OrientVertical', returnType: 'string', value: '"vertical"' },
]

const columnSort: HelperRow[] = [
  { name: 'SortNone', returnType: 'string', value: '"None"' },
  { name: 'SortDefault', returnType: 'string', value: '"Default"' },
  { name: 'SortCustom', returnType: 'string', value: '"Custom"' },
]

const colors: HelperRow[] = [
  { name: 'ColorTransparent', returnType: 'Color', value: 'Color.clear' },
  { name: 'ColorWhite', returnType: 'Color', value: 'Color.white' },
  { name: 'ColorBlack', returnType: 'Color', value: 'Color.black' },
  { name: 'ColorRed', returnType: 'Color', value: 'Color.red' },
  { name: 'ColorGreen', returnType: 'Color', value: 'Color.green' },
  { name: 'ColorBlue', returnType: 'Color', value: 'Color.blue' },
  { name: 'ColorYellow', returnType: 'Color', value: 'Color.yellow' },
  { name: 'ColorCyan', returnType: 'Color', value: 'Color.cyan' },
  { name: 'ColorMagenta', returnType: 'Color', value: 'Color.magenta' },
  { name: 'ColorGrey / ColorGray', returnType: 'Color', value: 'Color.grey' },
  { name: 'Hex(string)', returnType: 'Color', value: 'Parse "#RRGGBB" or "#RGB"' },
  { name: 'Rgba(byte r, g, b, a=255)', returnType: 'Color', value: '0–255 colour' },
  { name: 'Rgba(float r, g, b, a=1f)', returnType: 'Color', value: '0–1 colour' },
]

const bgRepeat: HelperRow[] = [
  { name: 'BgRepeat(x, y)', returnType: 'BackgroundRepeat', value: 'Custom repeat' },
  { name: 'BgRepeatNone', returnType: 'BackgroundRepeat', value: 'NoRepeat, NoRepeat' },
  { name: 'BgRepeatBoth', returnType: 'BackgroundRepeat', value: 'Repeat, Repeat' },
  { name: 'BgRepeatX', returnType: 'BackgroundRepeat', value: 'Repeat, NoRepeat' },
  { name: 'BgRepeatY', returnType: 'BackgroundRepeat', value: 'NoRepeat, Repeat' },
  { name: 'BgRepeatSpace', returnType: 'BackgroundRepeat', value: 'Space, Space' },
  { name: 'BgRepeatRound', returnType: 'BackgroundRepeat', value: 'Round, Round' },
]

const bgPosition: HelperRow[] = [
  { name: 'BgPos(keyword)', returnType: 'BackgroundPosition', value: 'By keyword' },
  { name: 'BgPos(keyword, offset)', returnType: 'BackgroundPosition', value: 'Keyword + offset' },
  { name: 'BgPosCenter', returnType: 'BackgroundPosition', value: 'Center' },
  { name: 'BgPosTop', returnType: 'BackgroundPosition', value: 'Top' },
  { name: 'BgPosBottom', returnType: 'BackgroundPosition', value: 'Bottom' },
  { name: 'BgPosLeft', returnType: 'BackgroundPosition', value: 'Left' },
  { name: 'BgPosRight', returnType: 'BackgroundPosition', value: 'Right' },
]

const bgSize: HelperRow[] = [
  { name: 'BgSize(x, y)', returnType: 'BackgroundSize', value: 'Custom size' },
  { name: 'BgSizeCover', returnType: 'BackgroundSize', value: 'Cover' },
  { name: 'BgSizeContain', returnType: 'BackgroundSize', value: 'Contain' },
]

const transform: HelperRow[] = [
  { name: 'Origin(x, y)', returnType: 'TransformOrigin', value: 'Custom origin' },
  { name: 'OriginCenter', returnType: 'TransformOrigin', value: 'Pct(50), Pct(50)' },
  { name: 'Xlate(x, y)', returnType: 'Translate', value: 'Translation' },
]

const easing: HelperRow[] = [
  { name: 'Easing(mode)', returnType: 'EasingFunction', value: 'Custom easing' },
  { name: 'EaseDefault', returnType: 'EasingFunction', value: 'Ease' },
  { name: 'EaseLinear', returnType: 'EasingFunction', value: 'Linear' },
  { name: 'EaseIn', returnType: 'EasingFunction', value: 'EaseIn' },
  { name: 'EaseOut', returnType: 'EasingFunction', value: 'EaseOut' },
  { name: 'EaseInOut', returnType: 'EasingFunction', value: 'EaseInOut' },
  { name: 'EaseInSine', returnType: 'EasingFunction', value: 'EaseInSine' },
  { name: 'EaseOutSine', returnType: 'EasingFunction', value: 'EaseOutSine' },
  { name: 'EaseInOutSine', returnType: 'EasingFunction', value: 'EaseInOutSine' },
  { name: 'EaseInCubic', returnType: 'EasingFunction', value: 'EaseInCubic' },
  { name: 'EaseOutCubic', returnType: 'EasingFunction', value: 'EaseOutCubic' },
  { name: 'EaseInOutCubic', returnType: 'EasingFunction', value: 'EaseInOutCubic' },
  { name: 'EaseInCirc', returnType: 'EasingFunction', value: 'EaseInCirc' },
  { name: 'EaseOutCirc', returnType: 'EasingFunction', value: 'EaseOutCirc' },
  { name: 'EaseInOutCirc', returnType: 'EasingFunction', value: 'EaseInOutCirc' },
  { name: 'EaseInElastic', returnType: 'EasingFunction', value: 'EaseInElastic' },
  { name: 'EaseOutElastic', returnType: 'EasingFunction', value: 'EaseOutElastic' },
  { name: 'EaseInOutElastic', returnType: 'EasingFunction', value: 'EaseInOutElastic' },
  { name: 'EaseInBack', returnType: 'EasingFunction', value: 'EaseInBack' },
  { name: 'EaseOutBack', returnType: 'EasingFunction', value: 'EaseOutBack' },
  { name: 'EaseInOutBack', returnType: 'EasingFunction', value: 'EaseInOutBack' },
  { name: 'EaseInBounce', returnType: 'EasingFunction', value: 'EaseInBounce' },
  { name: 'EaseOutBounce', returnType: 'EasingFunction', value: 'EaseOutBounce' },
  { name: 'EaseInOutBounce', returnType: 'EasingFunction', value: 'EaseInOutBounce' },
]

const filters: HelperRow[] = [
  { name: 'FilterBlur(radiusPx)', returnType: 'FilterFunction', value: 'Blur filter' },
  { name: 'FilterGrayscale(amount)', returnType: 'FilterFunction', value: 'Grayscale filter' },
  { name: 'FilterContrast(amount)', returnType: 'FilterFunction', value: 'Contrast adjustment' },
  { name: 'FilterHueRotate(degrees)', returnType: 'FilterFunction', value: 'Hue rotation' },
  { name: 'FilterInvert(amount)', returnType: 'FilterFunction', value: 'Color inversion' },
  { name: 'FilterOpacity(amount)', returnType: 'FilterFunction', value: 'Opacity filter' },
  { name: 'FilterSepia(amount)', returnType: 'FilterFunction', value: 'Sepia filter' },
  { name: 'FilterTint(color)', returnType: 'FilterFunction', value: 'Color tint' },
]

const allGroups = [
  { label: 'Length units', rows: lengthUnits },
  { label: 'Style keywords', rows: keywords },
  { label: 'Flex direction', rows: flexDirection },
  { label: 'Justify content', rows: justify },
  { label: 'Align', rows: align },
  { label: 'Wrap', rows: wrap },
  { label: 'Position', rows: position },
  { label: 'Display', rows: display },
  { label: 'Visibility', rows: visibility },
  { label: 'Overflow', rows: overflow },
  { label: 'White-space', rows: whiteSpace },
  { label: 'Text overflow', rows: textOverflow },
  { label: 'Text align', rows: textAlign },
  { label: 'Text overflow position', rows: textOverflowPos },
  { label: 'Text auto size', rows: textAutoSize },
  { label: 'Font style', rows: fontStyle },
  { label: 'Picking mode', rows: pickingMode },
  { label: 'Selection type', rows: selectionType },
  { label: 'Scroller visibility', rows: scrollerVis },
  { label: 'Language direction', rows: langDirection },
  { label: 'Slider direction', rows: sliderDir },
  { label: 'ScrollView mode', rows: scrollMode },
  { label: 'Image scale mode', rows: imageScale },
  { label: 'TwoPaneSplitView orientation', rows: splitOrient },
  { label: 'Column sorting mode', rows: columnSort },
  { label: 'Colors', rows: colors },
  { label: 'Background repeat', rows: bgRepeat },
  { label: 'Background position', rows: bgPosition },
  { label: 'Background size', rows: bgSize },
  { label: 'Transform', rows: transform },
  { label: 'Easing functions', rows: easing },
  { label: 'Filter functions (Unity 6.3+)', rows: filters },
]

const totalCount = allGroups.reduce((acc, g) => acc + g.rows.length, 0)

/* ------------------------------------------------------------------ */
/*  Component                                                         */
/* ------------------------------------------------------------------ */

const HelperTable: FC<{ rows: HelperRow[] }> = ({ rows }) => (
  <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell><strong>Helper</strong></TableCell>
          <TableCell><strong>Return type</strong></TableCell>
          <TableCell><strong>Value</strong></TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.map((r) => (
          <TableRow key={r.name}>
            <TableCell><code>{r.name}</code></TableCell>
            <TableCell><code>{r.returnType}</code></TableCell>
            <TableCell>{r.value}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </TableContainer>
)

export const CssHelpersReferencePage: FC = () => (
  <Box sx={styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      CssHelpers Reference
    </Typography>
    <Typography variant="body1" paragraph>
      <code>CssHelpers</code> provides <strong>{totalCount}</strong> static
      shortcuts for Unity UI Toolkit style values. Import them with:
    </Typography>
    <CodeBlock language="jsx" code={`using static ReactiveUITK.Props.Typed.CssHelpers;

// Then use directly in styles:
new Style { Width = Pct(50), FlexDirection = FlexRow, BackgroundColor = Hex("#1e1e1e") }`} />

    {allGroups.map((group) => (
      <Box key={group.label} sx={styles.section}>
        <Typography variant="h6" gutterBottom>
          {group.label}
          {group.label.includes('Unity 6.3') && (
            <Chip label="Unity 6.3+" size="small" color="warning" sx={{ ml: 1 }} />
          )}
        </Typography>
        <HelperTable rows={group.rows} />
      </Box>
    ))}

    <Alert severity="info" sx={{ mt: 2 }}>
      All helpers are compile-time evaluated static properties or pure
      functions. They introduce zero runtime overhead.
    </Alert>
  </Box>
)
