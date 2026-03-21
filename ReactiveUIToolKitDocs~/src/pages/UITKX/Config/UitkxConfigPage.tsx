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

export const UitkxConfigPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Configuration Reference
    </Typography>
    <Typography variant="body1" paragraph>
      All configuration options for the UITKX editor extensions and formatter.
    </Typography>

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
