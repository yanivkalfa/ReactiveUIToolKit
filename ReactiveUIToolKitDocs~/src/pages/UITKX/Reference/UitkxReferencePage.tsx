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
    return (<Label text="Welcome back!" />);
  } @else {
    return (<Button text="Log in" onClick={_ => login()} />);
  }

  @foreach (var item in items) {
    // Setup code — C# statements before return
    var label = item.Name.ToUpper();
    var bg = item.IsActive ? Color.green : Color.gray;

    return (
      <Label key={item.Id} text={label}
        style={new Style { (StyleKeys.Color, bg) }} />
    );
  }

  @for (int i = 0; i < count; i++) {
    if (i % 2 != 0) return null; // skip odd — renders nothing

    return (<Label key={i} text={$"Even: {i}"} />);
  }

  @switch (mode) {
    @case "dark":
      return (<Label text="Dark mode" />);
    @default:
      return (<Label text="Light mode" />);
  }
</VisualElement>`

const EXPRESSION_EXAMPLE = `<Label text={$"Count: {count}"} />
<Button onClick={_ => setCount(count + 1)} />
<VisualElement>
  {myCustomNode}
  {cond ? <A/> : <B/>}
  {items.Select(x => <Item key={x.Id} text={x.Name} />)}
  // This is a line comment
  /* This is a block comment */
</VisualElement>

// C# switch expression in child position
{status switch {
    "ok"  => <Label text="All good" />,
    "err" => <Label text="Something went wrong" />,
    _     => <Label text="Unknown" />
}}`

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
    <CodeBlock language="jsx" code={DIRECTIVE_HEADER_EXAMPLE} />

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
    <CodeBlock language="jsx" code={FUNCTION_STYLE_EXAMPLE} />

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

        </TableBody>
      </Table>
    </TableContainer>
    <CodeBlock language="jsx" code={CONTROL_FLOW_EXAMPLE} />
    <Typography variant="body2" paragraph sx={{ mt: 2 }}>
      Each directive body is a <strong>C# function body</strong>: arbitrary statements
      (variable declarations, conditionals, LINQ) followed by{' '}
      <code>return (&lt;JSX /&gt;);</code>. Use <code>return null;</code> to skip
      rendering for a particular iteration or branch. JSX can also be assigned to
      variables in setup code: <code>var x = (&lt;Label text="hi" /&gt;);</code> and
      rendered inline with <code>{'{x}'}</code>.
    </Typography>

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
            <TableCell><code>{'{expr}'}</code></TableCell>
            <TableCell><code>{'<Box>{myNode}</Box>'}</code></TableCell>
            <TableCell>
              C# expression in <strong>markup-child</strong> position. The expression
              may evaluate to a <code>VirtualNode</code>, an{' '}
              <code>{'IEnumerable<VirtualNode>'}</code> (rendered as siblings), a
              string (rendered as a label), or <code>null</code> (renders nothing).
              JSX literals are allowed inside the expression
              (e.g. <code>{'{cond ? <A/> : <B/>}'}</code>).
            </TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>{'attr={expr}'}</code></TableCell>
            <TableCell><code>text={'{$"Count: {count}"}'}</code></TableCell>
            <TableCell>C# expression as attribute value</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>"literal"</code></TableCell>
            <TableCell><code>text="hello"</code></TableCell>
            <TableCell>Plain string attribute</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>// comment</code></TableCell>
            <TableCell><code>// TODO</code></TableCell>
            <TableCell>Line comment (to end of line)</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>/* comment */</code></TableCell>
            <TableCell><code>/* TODO */</code></TableCell>
            <TableCell>Block comment (multi-line)</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>{'<>...</>'}</code></TableCell>
            <TableCell><code>{'<>'}{'<Label /><Label />'}{'</>'}</code></TableCell>
            <TableCell>Fragment — invisible wrapper for multiple elements without a parent node</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>{'{expr switch { ... }}'}</code></TableCell>
            <TableCell><code>{'{status switch { "ok" => <Label text="OK" />, _ => <Label text="?" /> }}'}</code></TableCell>
            <TableCell>C# switch expression in child position — returns markup per branch</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <CodeBlock language="jsx" code={EXPRESSION_EXAMPLE} />

    {/* ── Migration from @(expr) ─────────────────────────────────────────── */}
    <Box sx={{ my: 3, p: 2, borderLeft: '4px solid', borderColor: 'warning.main', bgcolor: 'action.hover' }}>
      <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
        Migration: <code>@(expr)</code> → <code>{'{expr}'}</code>
      </Typography>
      <Typography variant="body2" paragraph sx={{ mb: 1 }}>
        Earlier versions of UITKX accepted <code>@(expr)</code> as a markup-child
        embed (Razor-style). It has been removed. The canonical and only embed
        form for arbitrary C# expressions inside markup is now <code>{'{expr}'}</code>{' '}
        (matching JSX/Babel/React). Files containing legacy <code>@(expr)</code>{' '}
        in markup raise a hard parse error <strong>UITKX0306</strong>.
      </Typography>
      <Typography variant="body2" paragraph sx={{ mb: 1 }}>
        The <code>@</code> prefix continues to mark <strong>directives only</strong>:{' '}
        <code>@if</code>, <code>@else</code>, <code>@for</code>, <code>@foreach</code>,{' '}
        <code>@while</code>, <code>@switch</code>, <code>@case</code>,{' '}
        <code>@default</code>, <code>@using</code>, <code>@namespace</code>,{' '}
        <code>@component</code>, <code>@props</code>, <code>@key</code>,{' '}
        <code>@inject</code>, <code>@uss</code>.
      </Typography>
      <Typography variant="body2" sx={{ mb: 1 }}>
        Migration is mechanical:
      </Typography>
      <CodeBlock language="jsx" code={`// before
<Box>@(items.Count)</Box>
<Box>@(myNode)</Box>
<Box>@(KeyDot(label))</Box>

// after
<Box>{items.Count}</Box>
<Box>{myNode}</Box>
<Box>{KeyDot(label)}</Box>`} />
    </Box>

    {/* ── User-component strict attribute validation (0.5.4) ───────────────── */}
    <Box sx={{ my: 3, p: 2, borderLeft: '4px solid', borderColor: 'warning.main', bgcolor: 'action.hover' }}>
      <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
        Migration: User-component attribute strictness (<code>0.5.4</code>)
      </Typography>
      <Typography variant="body2" paragraph sx={{ mb: 1 }}>
        Earlier versions silently allowed any <code>BaseProps</code> attribute
        (<code>style</code>, <code>name</code>, <code>className</code>,{' '}
        <code>onClick</code>, <code>extraProps</code>, …) on every tag — including
        user-defined function components — because the schema lumped them with{' '}
        <code>key</code> / <code>ref</code> under one <em>universal</em> list.
        That produced <strong>CS0117</strong> at C# compile time when the user
        component's generated <code>*Props</code> class didn't actually have
        the property.
      </Typography>
      <Typography variant="body2" paragraph sx={{ mb: 1 }}>
        From <code>0.5.4</code> onward, user components only accept their{' '}
        <strong>declared parameters</strong> plus the two truly universal
        attributes: <code>key</code> (VirtualNode reconciliation slot) and{' '}
        <code>ref</code> (auto-routed to the unique{' '}
        <code>{'Hooks.MutableRef<T>'}</code> parameter via{' '}
        <code>forwardRef</code>-style semantics). Anything else raises{' '}
        <strong>UITKX0109</strong> (Error). Built-in tags (<code>Box</code>,{' '}
        <code>Button</code>, <code>Label</code>, …) are unchanged — they still
        accept the full <code>BaseProps</code> intrinsic surface.
      </Typography>
      <Typography variant="body2" sx={{ mb: 1 }}>
        Migration: declare the attribute as a parameter and forward it.
      </Typography>
      <CodeBlock language="jsx" code={`// before — silent slip-through, then CS0117 on AppButtonProps.Style
component AppButton(string text = "") {
    return (<Button text={text}/>);
}
<AppButton text="Save" style={btnStyle}/>   // UITKX0109 in 0.5.4+

// after — declare style as a parameter and forward it explicitly
component AppButton(string text = "", IStyle? style = null) {
    return (<Button text={text} style={style}/>);
}
<AppButton text="Save" style={btnStyle}/>   // OK`} />
    </Box>

    {/* ── JSX in Setup Code ──────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      JSX in Setup Code
    </Typography>
    <Typography variant="body2" paragraph>
      JSX elements can be used in setup code (before <code>return</code>) in
      several ways. Bare JSX works in assignments, ternaries, and arrow
      expressions. For collection initializers (arrays, lists, dictionaries),
      wrap each element in parentheses <code>()</code>.
    </Typography>
    <CodeBlock language="jsx" code={`// ── Bare JSX — works in assignments and ternaries ──
var header = <Label text="Title" />;
var icon = isActive ? <Box style={activeStyle} /> : <Box style={inactiveStyle} />;

// ── Paren-wrapped JSX — works everywhere, including collections ──
var arr  = new VirtualNode[] { (<Label text="hi" />), (<Box />) };
var list = new List<VirtualNode> { (<A/>), (<B/>) };
var dict = new Dictionary<string, VirtualNode> { { "header", (<Label text="Title" />) } };`} />

    {/* ── Rules & Gotchas ────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Rules & Gotchas
    </Typography>
    <Typography component="ul" variant="body2">
      <li><code>@namespace</code> must appear before <code>@component</code> in directive-header form.</li>
      <li>Hook calls must be unconditional at component top level — not inside <code>@if</code>, <code>@foreach</code>, etc.</li>
      <li>Each control block body must wrap its markup in <code>return (...);</code>. Setup code (variable declarations, computations) goes before <code>return</code>. Use <code>return null;</code> to skip rendering.</li>
      <li>Direct children of <code>@foreach</code> need a <code>key</code> attribute for stable reconciliation.</li>
      <li>Components must have a single root element.</li>
      <li>Component names must match the filename (e.g. <code>MyButton.uitkx</code> defines <code>component MyButton</code>).</li>
    </Typography>
  </Box>
)
