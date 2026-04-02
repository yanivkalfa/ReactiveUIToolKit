import type { FC } from 'react'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './UitkxReferencePage.style'

const DIRECTIVE_HEADER_EXAMPLE = `@namespace My.Game.UI
@using System.Collections.Generic
@component MyButton
@props MyButtonProps
@key "root-key"
@inject ILogger logger`

const FUNCTION_STYLE_EXAMPLE = `@using UnityEngine

component Counter(string label = "Count") {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Label text={$"{label}: {count}"} />
      <Button text="+" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`

const CONTROL_FLOW_EXAMPLE = `<VisualElement>
  @if (isLoggedIn) {
    <Label text="Welcome back!" />
  } @else {
    <Button text="Log in" onClick={_ => login()} />
  }

  @foreach (var item in items) {
    <Label key={item.Id} text={item.Name} />
  }

  @switch (mode) {
    @case "dark":
      <Label text="Dark mode" />
    @default:
      <Label text="Light mode" />
  }
</VisualElement>`

const EXPRESSION_EXAMPLE = `<Label text={$"Count: {count}"} />
<Button onClick={_ => setCount(count + 1)} />
<VisualElement>
  @(MyCustomComponent)
  {/* This is a JSX comment */}
</VisualElement>`

export const UitkxReferencePage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      UITKX Language Reference
    </Typography>
    <Typography variant="body1" paragraph>
      Complete reference for the UITKX markup language — directives, syntax,
      control flow, and expressions.
    </Typography>

    {/* ── Header Directives ─────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Header Directives
    </Typography>
    <Typography variant="body2" paragraph>
      Header directives appear at the top of a <code>.uitkx</code> file, before
      any markup. They configure the generated C# class.
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Directive</TableCell>
            <TableCell>Syntax</TableCell>
            <TableCell>Description</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>@namespace</code></TableCell>
            <TableCell><code>@namespace My.Game.UI</code></TableCell>
            <TableCell>C# namespace for the generated class</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@component</code></TableCell>
            <TableCell><code>@component MyButton</code></TableCell>
            <TableCell>Component class name (must match filename)</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@using</code></TableCell>
            <TableCell><code>@using System.Collections.Generic</code></TableCell>
            <TableCell>Adds a using directive to the generated file. Note: <code>StyleKeys</code> and <code>CssHelpers</code> are auto-imported.</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@props</code></TableCell>
            <TableCell><code>@props MyButtonProps</code></TableCell>
            <TableCell>Props type consumed by the component</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@key</code></TableCell>
            <TableCell><code>@key "root-key"</code></TableCell>
            <TableCell>Static key on the root element</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@inject</code></TableCell>
            <TableCell><code>@inject ILogger logger</code></TableCell>
            <TableCell>Dependency-injected field</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <CodeBlock language="tsx" code={DIRECTIVE_HEADER_EXAMPLE} />

    {/* ── Function-Style Components ──────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Function-Style Components
    </Typography>
    <Typography variant="body2" paragraph>
      Function-style components use a <code>component Name {'{ ... }'}</code>{' '}
      syntax with optional typed parameters. They replace the directive-header
      form for most use cases.
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Feature</TableCell>
            <TableCell>Syntax</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell>Declaration</TableCell>
            <TableCell><code>component Name {'{ ... }'}</code></TableCell>
          </TableRow>
          <TableRow>
            <TableCell>With parameters</TableCell>
            <TableCell><code>component Name(string text = "default") {'{ ... }'}</code></TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Preamble <code>@using</code></TableCell>
            <TableCell>Before the <code>component</code> keyword</TableCell>
          </TableRow>
          <TableRow>
            <TableCell>Preamble <code>@namespace</code></TableCell>
            <TableCell>Optional explicit namespace override</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <CodeBlock language="tsx" code={FUNCTION_STYLE_EXAMPLE} />

    {/* ── Markup Control Flow ────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Markup Control Flow
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Directive</TableCell>
            <TableCell>Syntax</TableCell>
            <TableCell>Notes</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>@if / @else if / @else</code></TableCell>
            <TableCell><code>@if (cond) {'{ ... }'} @else {'{ ... }'}</code></TableCell>
            <TableCell>Conditional rendering</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@foreach</code></TableCell>
            <TableCell><code>@foreach (var item in list) {'{ ... }'}</code></TableCell>
            <TableCell>Loop — direct children must have <code>key</code></TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@for</code></TableCell>
            <TableCell><code>@for (int i = 0; i &lt; n; i++) {'{ ... }'}</code></TableCell>
            <TableCell>C-style for loop</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@while</code></TableCell>
            <TableCell><code>@while (cond) {'{ ... }'}</code></TableCell>
            <TableCell>While loop</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@switch / @case / @default</code></TableCell>
            <TableCell><code>@switch (val) {'{ @case "a": ... @default: ... }'}</code></TableCell>
            <TableCell>Switch expression</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@break</code></TableCell>
            <TableCell><code>@break;</code></TableCell>
            <TableCell>Exit a <code>@for</code> or <code>@while</code> loop</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>@continue</code></TableCell>
            <TableCell><code>@continue;</code></TableCell>
            <TableCell>Skip to the next iteration</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <CodeBlock language="tsx" code={CONTROL_FLOW_EXAMPLE} />

    {/* ── Expressions & Values ───────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Expressions & Values
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Syntax</TableCell>
            <TableCell>Example</TableCell>
            <TableCell>Description</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>@(expr)</code></TableCell>
            <TableCell><code>@(MyCustomComponent)</code></TableCell>
            <TableCell>Render a component or expression inline in markup children</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>{'{expr}'}</code></TableCell>
            <TableCell><code>text={'{$"Count: {count}"}'}</code></TableCell>
            <TableCell>C# expression as attribute value</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>"literal"</code></TableCell>
            <TableCell><code>text="hello"</code></TableCell>
            <TableCell>Plain string attribute</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>{'{/* comment */}'}</code></TableCell>
            <TableCell><code>{'{/* TODO */}'}</code></TableCell>
            <TableCell>JSX-style block comment</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <CodeBlock language="tsx" code={EXPRESSION_EXAMPLE} />

    {/* ── Rules & Gotchas ────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Rules & Gotchas
    </Typography>
    <Typography component="ul" variant="body2">
      <li><code>@namespace</code> must appear before <code>@component</code> in directive-header form.</li>
      <li>Hook calls must be unconditional at component top level — not inside <code>@if</code>, <code>@foreach</code>, etc.</li>
      <li><code>@break</code> / <code>@continue</code> are only valid inside <code>@for</code> and <code>@while</code>.</li>
      <li>Direct children of <code>@foreach</code> need a <code>key</code> attribute for stable reconciliation.</li>
      <li>Components must have a single root element.</li>
      <li>Component names must match the filename (e.g. <code>MyButton.uitkx</code> defines <code>component MyButton</code>).</li>
    </Typography>
  </Box>
)
