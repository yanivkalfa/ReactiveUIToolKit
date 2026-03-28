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
            <TableCell>@foreach child missing key</TableCell>
            <TableCell>Add a <code>key</code> attribute with a stable unique identifier from the item.</TableCell>
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
            <TableCell>Element inside @foreach missing key</TableCell>
            <TableCell>Add a <code>key</code> attribute for reconciliation.</TableCell>
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
            <TableCell>Valid directives: <code>@if</code>, <code>@else</code>, <code>@for</code>, <code>@foreach</code>, <code>@while</code>, <code>@switch</code>, <code>@case</code>, <code>@default</code>, <code>@break</code>, <code>@continue</code>, <code>@code</code>.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><Chip label="UITKX0306" size="small" color="error" variant="outlined" /></TableCell>
            <TableCell><Chip label="Error" size="small" color="error" /></TableCell>
            <TableCell>@(expr) in setup code</TableCell>
            <TableCell>Inline expressions <code>@(...)</code> are only valid inside markup, not in <code>@code</code> blocks.</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
  </Box>
)
