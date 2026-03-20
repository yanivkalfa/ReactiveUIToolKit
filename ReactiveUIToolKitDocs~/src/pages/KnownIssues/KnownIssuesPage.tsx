import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './KnownIssuesPage.style'

export const KnownIssuesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Known Issues & Editor Limitations
    </Typography>

    {/* ── Runtime ─────────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Runtime
    </Typography>
    <Typography variant="body1" paragraph>
      There is a known issue where <code>MultiColumnListView</code> can briefly
      jump or snap when scrolling large data sets; this will be addressed in a
      future update.
    </Typography>

    {/* ── Editor-specific ─────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Editor-Specific Limitations
    </Typography>

    <Typography variant="h6" component="h3" sx={Styles.section}>
      VS Code
    </Typography>
    <Typography component="ul" variant="body2">
      <li>
        <strong>Brief colour flash on file open</strong> — TextMate grammar
        colours appear first (e.g. PascalCase names briefly show as green
        "type" tokens), then the LSP semantic tokens override them within
        ~200ms. This is inherent to how VS Code layers TM grammar and semantic
        tokens.
      </li>
      <li>
        Full syntax highlighting, formatting, completions, hover, and
        diagnostics are supported.
      </li>
    </Typography>

    <Typography variant="h6" component="h3" sx={Styles.section}>
      Visual Studio 2022
    </Typography>
    <Typography component="ul" variant="body2">
      <li>
        Full support including dimming of unreachable code and JSX comment
        colouring.
      </li>
      <li>
        The UITKX Visual Studio extension uses MEF-based classification and
        the shared language library.
      </li>
    </Typography>

    <Typography variant="h6" component="h3" sx={Styles.section}>
      JetBrains Rider
    </Typography>
    <Typography component="ul" variant="body2">
      <li>
        Stub implementation only — the extent of working features has not been
        fully verified. Source generation and <code>#line</code> mapping work
        via standard Roslyn support.
      </li>
    </Typography>

    {/* ── Burst AOT ───────────────────────────────────────────────────────── */}
    <Typography variant="h5" component="h2" sx={Styles.section}>
      Burst AOT & Assembly Resolution
    </Typography>
    <Typography variant="body1" paragraph>
      If you encounter the error:
    </Typography>
    <CodeBlock
      language="text"
      code="Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: Assembly-CSharp-Editor"
    />
    <Typography variant="body1" paragraph>
      Go to <strong>Edit → Project Settings → Burst AOT Settings</strong> and
      add <code>Assembly-CSharp-Editor</code> to the exclusion list. This
      prevents Burst from trying to AOT-compile editor-only assemblies that
      reference UITKX types.
    </Typography>
  </Box>
)
