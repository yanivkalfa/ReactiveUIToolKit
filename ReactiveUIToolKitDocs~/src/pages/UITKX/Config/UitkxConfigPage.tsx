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
import Styles from '../Reference/UitkxReferencePage.style'

const VSCODE_SETTINGS = `{
  // Path to a custom UitkxLanguageServer.dll (leave empty for bundled server)
  "uitkx.server.path": "",

  // Path to the dotnet executable used to run the LSP server
  "uitkx.server.dotnetPath": "dotnet",

  // Trace LSP communication (off | messages | verbose)
  "uitkx.trace.server": "off"
}`

const UITKX_CONFIG_JSON = `// uitkx.config.json — place it at your UI source root (or a subfolder).
{
  // The project-relative folder that the '~/' import/asset alias resolves against.
  // Default: "Assets".
  "root": "Assets/UI",

  // The root of every path-derived namespace (files with no @namespace).
  // Lets a whole project carry its own namespace root — no per-file @namespace.
  // Default: falls back to the owning .asmdef's rootNamespace, else "ReactiveUITK.Uitkx".
  "namespacePrefix": "MyGame.UI"
}`

export const UitkxConfigPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Configuration Reference
    </Typography>
    <Typography variant="body1" paragraph>
      Two layers of configuration: the per-project <code>uitkx.config.json</code> (read by the
      source generator, analyzer, and language server) and the per-editor extension settings.
    </Typography>

    {/* ── uitkx.config.json ─────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Project config &mdash; <code>uitkx.config.json</code>
    </Typography>
    <Typography variant="body1" paragraph>
      An optional <code>uitkx.config.json</code> configures import/asset resolution for a project.
      It is read by everything that resolves paths &mdash; the Roslyn source generator, the Unity
      analyzer, and the LSP &mdash; so the behavior is identical in a Unity build and in every
      editor. It is <strong>not</strong> required; with no config, defaults apply.
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Key</TableCell>
            <TableCell>Type</TableCell>
            <TableCell>Default</TableCell>
            <TableCell>Description</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>root</code></TableCell>
            <TableCell>string</TableCell>
            <TableCell><code>"Assets"</code></TableCell>
            <TableCell>
              The project-relative folder that the <code>~/</code> alias resolves against, in
              both <code>import</code> specifiers (<code>import {'{ X }'} from "~/Shared/X"</code>)
              and asset paths (<code>Asset&lt;T&gt;("~/Textures/icon")</code>, <code>@uss "~/…"</code>).
              A <code>~/</code> path that escapes this root raises <code>UITKX2314</code>.
            </TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>namespacePrefix</code></TableCell>
            <TableCell>string</TableCell>
            <TableCell><code>(see below)</code></TableCell>
            <TableCell>
              The root of every <strong>path-derived</strong> namespace (a file that has no explicit{' '}
              <code>@namespace</code>). Setting it lets a whole project carry its own namespace root
              with <strong>no per-file <code>@namespace</code></strong> — the generated type for{' '}
              <code>UI/App/pages/GameOverPage/GameOverPage.uitkx</code> becomes{' '}
              <code>MyPrefix.App.Pages.GameOverPage</code>. Each dotted segment is sanitized to a
              legal C# identifier.
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <Typography variant="body2" paragraph>
      <strong>Namespace-prefix precedence</strong> (most-specific wins): a per-file{' '}
      <code>@namespace</code> &rarr; the config <code>"namespacePrefix"</code> &rarr; the owning{' '}
      <code>.asmdef</code>&rsquo;s <code>rootNamespace</code> (Unity&rsquo;s own field) &rarr; the
      built-in <code>ReactiveUITK.Uitkx</code> default. Every step is opt-in, so a project that sets
      neither a prefix nor an asmdef <code>rootNamespace</code> keeps deriving under{' '}
      <code>ReactiveUITK.Uitkx</code>. The asmdef <em>name</em> is deliberately not used — it would
      silently re-root every project the moment it named an assembly.
    </Typography>
    <Typography variant="body2" paragraph>
      <strong>Discovery:</strong> resolution walks up from the <code>.uitkx</code> file to the
      nearest <code>uitkx.config.json</code> and uses its <code>"root"</code> outright &mdash;
      nearest wins, no merging up the tree. A file with no config on the way up falls back to the
      default <code>Assets</code> root.
    </Typography>
    <CodeBlock language="json" code={UITKX_CONFIG_JSON} />

    {/* ── VS Code Settings ──────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      VS Code Extension Settings
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Setting</TableCell>
            <TableCell>Type</TableCell>
            <TableCell>Default</TableCell>
            <TableCell>Description</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>uitkx.server.path</code></TableCell>
            <TableCell>string</TableCell>
            <TableCell><code>""</code></TableCell>
            <TableCell>
              Absolute path to a custom <code>UitkxLanguageServer.dll</code>.
              Leave empty to use the server bundled with the extension.
            </TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>uitkx.server.dotnetPath</code></TableCell>
            <TableCell>string</TableCell>
            <TableCell><code>"dotnet"</code></TableCell>
            <TableCell>
              Path to the <code>dotnet</code> executable. Override this if your
              .NET 8+ SDK is installed in a non-standard location.
            </TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>uitkx.trace.server</code></TableCell>
            <TableCell>enum</TableCell>
            <TableCell><code>"off"</code></TableCell>
            <TableCell>
              Controls LSP trace output. Set to <code>"messages"</code> or{' '}
              <code>"verbose"</code> to see JSON-RPC traffic in the Output panel
              (select "UITKX Language Server" channel).
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <CodeBlock language="json" code={VSCODE_SETTINGS} />

    {/* ── Editor Defaults ────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Editor Defaults
    </Typography>
    <Typography variant="body2" paragraph>
      The extension automatically configures these editor settings for{' '}
      <code>.uitkx</code> files:
    </Typography>
    <TableContainer>
      <Table size="small" sx={Styles.table}>
        <TableHead>
          <TableRow>
            <TableCell>Setting</TableCell>
            <TableCell>Value</TableCell>
            <TableCell>Reason</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>editor.defaultFormatter</code></TableCell>
            <TableCell><code>ReactiveUITK.uitkx</code></TableCell>
            <TableCell>Uses the UITKX formatter for <code>.uitkx</code> files</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>editor.formatOnSave</code></TableCell>
            <TableCell><code>true</code></TableCell>
            <TableCell>Auto-format on save (recommended)</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>editor.tabSize</code></TableCell>
            <TableCell><code>2</code></TableCell>
            <TableCell>UITKX uses 2-space indentation</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>editor.insertSpaces</code></TableCell>
            <TableCell><code>true</code></TableCell>
            <TableCell>Spaces, not tabs</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>editor.bracketPairColorization</code></TableCell>
            <TableCell><code>false</code></TableCell>
            <TableCell>Disabled — conflicting colors with UITKX semantic tokens</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
  </Box>
)
