import type { FC } from 'react'
import {
  Box,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import Styles from '../Reference/UitkxReferencePage.style'

export const UitkxDiagnosticsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Diagnostics Reference
    </Typography>
    <Typography variant="body1" paragraph>
      Every diagnostic code emitted by the UITKX source generator and language
      server, with severity, meaning, and how to fix it.
    </Typography>

    {/* ── Generator Diagnostics (UITKX0001–0021) ────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Source Generator Diagnostics
    </Typography>
    <Typography variant="body2" paragraph>
      Emitted at compile time by the Roslyn source generator when processing
      <code>.uitkx</code> files.
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Code</TableCell>
            <TableCell>Severity</TableCell>
            <TableCell>Title</TableCell>
            <TableCell>How to fix</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><Chip label="UITKX0001" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Unknown built-in element</TableCell>
            <TableCell>Check the tag name — built-in elements use PascalCase (e.g. <code>&lt;Button&gt;</code>, <code>&lt;Label&gt;</code>).</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0002" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Unknown attribute on element</TableCell>
            <TableCell>Verify the attribute name matches a property on the element's props type.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0005" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Missing required directive</TableCell>
            <TableCell>Add the missing <code>@namespace</code> or <code>@component</code> directive.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0006" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>@component name mismatch</TableCell>
            <TableCell>Rename <code>@component</code> to match the file name, or rename the file.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0008" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Unknown function component</TableCell>
            <TableCell>Ensure the component type exists and has a public static <code>Render</code> method.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0009" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Loop element missing key</TableCell>
            <TableCell>Add a <code>key</code> attribute with a stable unique identifier. Applies to all loop types: <code>@foreach</code>, <code>@for</code>, <code>@while</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0010" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Duplicate sibling key</TableCell>
            <TableCell>Ensure each sibling element has a unique <code>key</code> value.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0012" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Directive order error</TableCell>
            <TableCell>Move <code>@namespace</code> above <code>@component</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0013" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Hook in conditional</TableCell>
            <TableCell>Move the hook call to the component top level, outside any <code>@if</code> branch.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0014" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Hook in loop</TableCell>
            <TableCell>Move the hook call to the component top level, outside any <code>@foreach</code> loop.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0015" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Hook in switch case</TableCell>
            <TableCell>Move the hook call to the component top level, outside the <code>@switch</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0016" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Hook in event handler</TableCell>
            <TableCell>Move the hook call to the component top level — hooks cannot be called inside attribute expressions.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0017" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Multiple root elements</TableCell>
            <TableCell>Wrap all root elements in a single container element (e.g. <code>&lt;VisualElement&gt;</code>).</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0018" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>UseEffect missing dependency array</TableCell>
            <TableCell>Pass an explicit dependency array as the second argument, or <code>Array.Empty&lt;object&gt;()</code> for run-once.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0019" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Loop variable used as key</TableCell>
            <TableCell>Use a stable unique identifier from the item instead of the loop index.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0020" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>ref on component without Ref&lt;T&gt; param</TableCell>
            <TableCell>Add a <code>Ref&lt;T&gt;?</code> parameter to the component, or remove the <code>ref</code> attribute.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0021" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>ref ambiguous — multiple Ref&lt;T&gt; params</TableCell>
            <TableCell>Use an explicit prop name (e.g. <code>inputRef={'{x}'}</code>) instead of <code>ref</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0022" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Asset file not found</TableCell>
            <TableCell>Check the asset path — it must be relative to the <code>.uitkx</code> file or an absolute project path.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0023" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Asset type mismatch</TableCell>
            <TableCell>The asset at the given path is a different type than the requested <code>Asset&lt;T&gt;</code>. Verify the generic type parameter.</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    {/* ── Structural Diagnostics (UITKX0101–0111) ────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Structural Diagnostics (Language Server)
    </Typography>
    <Typography variant="body2" paragraph>
      Emitted in real time by the language server as you type. These appear as
      squiggly underlines in your editor.
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Code</TableCell>
            <TableCell>Severity</TableCell>
            <TableCell>Message</TableCell>
            <TableCell>How to fix</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><Chip label="UITKX0101" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Missing required <code>@namespace</code> directive</TableCell>
            <TableCell>Add <code>@namespace Your.Namespace</code> at the top of the file.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0102" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Missing required <code>@component</code> directive</TableCell>
            <TableCell>Add <code>@component YourComponentName</code> or use function-style syntax.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0103" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>@component name does not match filename</TableCell>
            <TableCell>Rename <code>@component</code> to match the file name.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0104" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Duplicate sibling key</TableCell>
            <TableCell>Ensure each sibling has a unique <code>key</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0105" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unknown element — no component found</TableCell>
            <TableCell>Check the tag name or add the missing component to your project.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0106" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Loop element missing key</TableCell>
            <TableCell>Add a <code>key</code> attribute for reconciliation. Applies to <code>@foreach</code>, <code>@for</code>, <code>@while</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0107" size="small" color="info" variant="outlined" /></TableCell>
            <TableCell><Chip label="Hint" size="small" color="info" /></TableCell>
            <TableCell>Unreachable code after return / @break / @continue</TableCell>
            <TableCell>Remove the unreachable code, or restructure control flow.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0108" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Multiple root elements</TableCell>
            <TableCell>Wrap all root elements in a single container.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0109" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unknown attribute on element</TableCell>
            <TableCell>Check the attribute name against the element's props type.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0111" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unused component parameter</TableCell>
            <TableCell>Remove the unused parameter or use it in the component body.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0112" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unused variable in setup code</TableCell>
            <TableCell>Remove the unused variable or use it in the component.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0120" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Asset path not found (language server)</TableCell>
            <TableCell>Fix the asset path in <code>Asset&lt;T&gt;("path")</code> — the language server validates paths in real time.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0121" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Asset type validation (language server)</TableCell>
            <TableCell>The asset type does not match the <code>Asset&lt;T&gt;</code> generic parameter.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0200" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Attribute requires newer Unity version</TableCell>
            <TableCell>The attribute is only available in a newer Unity version, or was removed in the current version.</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    {/* ── Parser Diagnostics (UITKX0300–0306) ────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Parser Diagnostics
    </Typography>
    <Typography variant="body2" paragraph>
      Emitted when the parser encounters malformed syntax.
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Code</TableCell>
            <TableCell>Severity</TableCell>
            <TableCell>Title</TableCell>
            <TableCell>How to fix</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><Chip label="UITKX0300" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unexpected token</TableCell>
            <TableCell>Check for typos or misplaced syntax near the reported line.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0301" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unclosed tag</TableCell>
            <TableCell>Add a matching closing tag or use self-closing syntax (<code>/&gt;</code>).</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0302" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Mismatched closing tag</TableCell>
            <TableCell>Ensure the closing tag matches the opening tag name.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0303" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unexpected end of file</TableCell>
            <TableCell>Close any open tags, braces, or expressions.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0304" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Unclosed expression or block</TableCell>
            <TableCell>Close the unclosed brace or parenthesis.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0305" size="small" color="warning" variant="outlined" /></TableCell>
            <TableCell><Chip label="Warning" size="small" color="warning" /></TableCell>
            <TableCell>Unknown markup directive</TableCell>
            <TableCell>Valid directives: <code>@if</code>, <code>@else</code>, <code>@for</code>, <code>@foreach</code>, <code>@while</code>, <code>@switch</code>, <code>@case</code>, <code>@default</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0306" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell><code>@(expr)</code> markup syntax is no longer supported</TableCell>
            <TableCell>
              The <code>@(expr)</code> markup-child embed has been removed. Use{' '}
              <code>{'{expr}'}</code> instead (matching JSX/Babel/React). Migration
              is mechanical: replace every <code>@(</code> with <code>{'{'}</code>{' '}
              and the matching <code>)</code> with <code>{'}'}</code>. The{' '}
              <code>@</code> prefix still marks <strong>directives</strong>{' '}
              (<code>@if</code>, <code>@for</code>, <code>@foreach</code>,{' '}
              <code>@while</code>, <code>@switch</code>, <code>@case</code>,{' '}
              <code>@default</code>, <code>@using</code>, <code>@namespace</code>,{' '}
              <code>@component</code>, <code>@props</code>, <code>@key</code>,{' '}
              <code>@inject</code>, <code>@uss</code>).
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    {/* ── Function-Style Component Diagnostics (UITKX2100–2106) ─────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Function-Style Component Diagnostics
    </Typography>
    <Typography variant="body2" paragraph>
      Emitted by the language server when validating function-style component
      declarations.
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Code</TableCell>
            <TableCell>Severity</TableCell>
            <TableCell>Message</TableCell>
            <TableCell>How to fix</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><Chip label="UITKX2100" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Non-PascalCase component name</TableCell>
            <TableCell>Component names must start with an uppercase letter (e.g. <code>MyButton</code>, not <code>myButton</code>).</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX2101" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Missing return statement</TableCell>
            <TableCell>Function-style components must end with <code>return (...);</code> containing markup.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX2102" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Malformed return statement</TableCell>
            <TableCell>The <code>return</code> must be followed by <code>(...)</code> containing UITKX markup.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX2104" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Mixed directive and function-style syntax</TableCell>
            <TableCell>Use either directive headers (<code>@namespace</code> / <code>@component</code>) or function-style (<code>component Name {'{ }'}</code>), not both.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX2105" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Invalid function-style declaration</TableCell>
            <TableCell>Ensure the <code>component</code> keyword is followed by a valid name and optional parameter list.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX2106" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>Missing parameter name</TableCell>
            <TableCell>Each parameter in the component signature must have a name: <code>component Name(string text)</code>.</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
  </Box>
)
