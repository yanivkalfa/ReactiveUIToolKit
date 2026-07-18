import type { FC } from 'react'
import { Box, Chip, List, ListItem, ListItemText, Typography } from '@mui/material'
import Styles from './RoadmapPage.style'

export const RoadmapPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Roadmap
    </Typography>
    <Typography variant="body1" paragraph>
      ReactiveUIToolKit is under active development. Below is a high-level view
      of planned areas. Priorities may shift based on community feedback.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 2 }}>
      Completed (through 0.9.x)
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Function-style components, control blocks, switch expressions, mixed-declaration files</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Imports &amp; exports: explicit cross-file references, path-derived namespaces, migration codemod (0.7.0)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />ES modules: a file IS a module — plain export declarations, file-keyed namespaces, full import surface (as / * as / default / export lists), namespace imports (0.9.0)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Hot Module Replacement / Fast Refresh: edit-save hot-swap with state preservation, no domain reload</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Typed style system + CssHelpers (easing, filters, whitespace, text auto-size, 9-slice, shadows)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Router, Signals, Portal, Suspense, Fragment, ErrorBoundary; Audio/Video elements; custom rendering (onGenerateVisualContent)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Snapshot testing utilities (SnapshotAssert / VNodeSnapshot)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Custom element adapter registration (ElementRegistry.Register)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />VS Code + Visual Studio 2022 extensions (LSP: diagnostics, completion, go-to-definition, formatting, rename); Rider plugin via the shared language server</>} />
      </ListItem>
    </List>

    <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 2 }}>
      Planned
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Planned" size="small" color="info" sx={{ mr: 1 }} />Deeper performance profiling: render-count diagnostics beyond the current bench metrics + diff tracing</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Planned" size="small" color="info" sx={{ mr: 1 }} />Additional IDE refactoring actions</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Planned" size="small" color="info" sx={{ mr: 1 }} />Expanded animation API and transition helpers</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Planned" size="small" color="info" sx={{ mr: 1 }} />Extended documentation: component testing guide, advanced patterns</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Planned" size="small" color="info" sx={{ mr: 1 }} />Unity 6.3 additions (new IStyle properties, FilterFunction helpers)</>} />
      </ListItem>
    </List>

    <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 2 }}>
      Under consideration
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Exploring" size="small" color="warning" sx={{ mr: 1 }} />PropTypes runtime validation (public API surface review)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Exploring" size="small" color="warning" sx={{ mr: 1 }} />Runtime-only package variant (no editor tooling)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Exploring" size="small" color="warning" sx={{ mr: 1 }} />Server-driven UI patterns</>} />
      </ListItem>
    </List>
  </Box>
)
