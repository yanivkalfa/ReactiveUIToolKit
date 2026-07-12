import type { FC } from 'react'
import {
  Box,
  List,
  ListItem,
  ListItemText,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from '../../GettingStarted/GettingStartedPage.style'
import {
  EXAMPLE_GRAMMAR,
  EXAMPLE_SPECIFIERS,
  EXAMPLE_MIXED,
  EXAMPLE_STRICT,
  EXAMPLE_CODEMOD,
  EXAMPLE_NAMESPACE,
} from './UitkxImportsPage.example'

const DIAGS: [string, string][] = [
  ['UITKX2300', 'unknown import specifier — no file at that path (also engine-native specifiers, which never resolve)'],
  ['UITKX2301', '`X` is not exported by that file — add `export` to its declaration'],
  ['UITKX2303', 'duplicate import of `X` (already imported from another specifier)'],
  ['UITKX2304', 'unused import `X` (warning)'],
  ['UITKX2305', '`X` is defined in a peer file but not imported — the message names the exact import line to add'],
  ['UITKX2307', '`X` is used like a component/hook but no file exports it'],
  ['UITKX2308', 'import crosses a module/root boundary — imports are asmdef-scoped in v1'],
  ['UITKX2309', 'import must appear in the preamble, before the first declaration'],
]

export const UitkxImportsPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Imports &amp; Exports
    </Typography>
    <Typography variant="body1" paragraph>
      Cross-file references in <code>.uitkx</code> are <strong>explicit</strong>. A file
      declares what it exposes with <code>export</code> and pulls in what it needs with{' '}
      <code>import</code> — ESM-style, the same grammar shared across the ReactiveUI family
      (Unity <code>.uitkx</code>, Unreal <code>.uetkx</code>, Godot <code>.guitkx</code>).
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Grammar
    </Typography>
    <Typography variant="body1" paragraph>
      A file is a preamble of <code>import</code> lines followed by a sequence of
      declarations, each optionally <code>export</code>-prefixed. Multiple components, hooks,
      and modules may live in one file, in any order.
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_GRAMMAR} />
    <List>
      <ListItem>
        <ListItemText primary="export" secondary="Makes a declaration visible to other files. No export = file-private (internal + strict-invisible)." />
      </ListItem>
      <ListItem>
        <ListItemText primary="import { A, B } from '…'" secondary="Named imports only — no default, no namespace import. Preamble only (before the first declaration)." />
      </ListItem>
    </List>

    <Typography variant="h5" component="h2" gutterBottom>
      Specifiers
    </Typography>
    <Typography variant="body1" paragraph>
      Specifiers are relative (<code>./</code>, <code>../</code>) or the root alias{' '}
      <code>~/</code> (the UI source root — <code>Assets/</code> by default, overridable via{' '}
      <code>uitkx.config.json</code>). They are extensionless; <code>.uitkx</code> is implied.
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_SPECIFIERS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Mixed declarations
    </Typography>
    <Typography variant="body1" paragraph>
      The old &ldquo;one component per file&rdquo; and companion-file naming rules are now
      lint-tier conventions — a file can declare any mix of components, hooks, and modules.
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_MIXED} />

    <Typography variant="h5" component="h2" gutterBottom>
      Strict resolution
    </Typography>
    <Typography variant="body1" paragraph>
      Referencing a peer-exported name without importing it is an error, and the diagnostic
      names the exact import line to add. Built-in elements (<code>&lt;Box&gt;</code>,{' '}
      <code>&lt;Label&gt;</code>…) and built-in hooks (<code>useState</code>,{' '}
      <code>useEffect</code>…) never need importing, and hand-written C# is ambient (never
      flagged).
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_STRICT} />

    <Typography variant="h6" component="h3" gutterBottom>
      Diagnostics (UITKX2300–2315)
    </Typography>
    <TableContainer component={Paper} sx={{ my: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>Code</strong></TableCell>
            <TableCell><strong>Meaning</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {DIAGS.map(([code, meaning]) => (
            <TableRow key={code}>
              <TableCell><code>{code}</code></TableCell>
              <TableCell>{meaning}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>

    <Typography variant="h5" component="h2" gutterBottom>
      Namespaces &amp; privacy
    </Typography>
    <Typography variant="body1" paragraph>
      A file&rsquo;s default namespace is derived from its path relative to the owning{' '}
      <code>.asmdef</code>. <code>@namespace</code> is now an optional interop override.
      Privacy is a compile-time fence: non-exported declarations are <code>internal</code> and
      invisible to strict resolution.
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_NAMESPACE} />

    <Typography variant="h5" component="h2" gutterBottom>
      Migrating an existing project
    </Typography>
    <Typography variant="body1" paragraph>
      New syntax is additive — an un-migrated project keeps compiling. When you&rsquo;re
      ready, the bundled codemod rewrites every <code>.uitkx</code> in place: it adds{' '}
      <code>export</code> to all declarations, inserts the imports each file needs, and stamps
      each file&rsquo;s current <code>@namespace</code> so identity is unchanged. It is
      idempotent and formatter-stable, so you can re-run it safely.
    </Typography>
    <CodeBlock language="bash" code={EXAMPLE_CODEMOD} />
  </Box>
)
