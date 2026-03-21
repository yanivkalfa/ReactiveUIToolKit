import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './KnownIssuesPage.style'

export const KnownIssuesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Known Issues
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
