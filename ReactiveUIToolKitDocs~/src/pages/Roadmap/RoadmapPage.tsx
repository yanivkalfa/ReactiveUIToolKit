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
      Completed (v0.3.0)
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Function-style components with setup code + return() syntax</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Control block bodies with return() and setup code</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Switch expression support in markup</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />CssHelpers expansion (easing, filters, whitespace, text auto-size)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />Portal, Suspense, and Fragment components</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Done" size="small" color="success" sx={{ mr: 1 }} />VS Code, Visual Studio 2022, and JetBrains Rider extensions</>} />
      </ListItem>
    </List>

    <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 2 }}>
      Planned
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Planned" size="small" color="info" sx={{ mr: 1 }} />Performance profiling tools and render-count diagnostics</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Planned" size="small" color="info" sx={{ mr: 1 }} />Component testing utilities (snapshot assertions)</>} />
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
    </List>

    <Typography variant="h5" component="h2" gutterBottom sx={{ mt: 2 }}>
      Under consideration
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Exploring" size="small" color="warning" sx={{ mr: 1 }} />PropTypes runtime validation (public API surface review)</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Exploring" size="small" color="warning" sx={{ mr: 1 }} />Custom element adapter registration</>} />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary={<><Chip label="Exploring" size="small" color="warning" sx={{ mr: 1 }} />Server-driven UI patterns</>} />
      </ListItem>
    </List>
  </Box>
)
