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
  EXAMPLE_STYLES,
  EXAMPLE_TYPES,
  EXAMPLE_UTILS,
  EXAMPLE_UITKX,
  EXAMPLE_DIRECTORY,
} from './CompanionFilesPage.example'

export const CompanionFilesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Companion Files
    </Typography>
    <Typography variant="body1" paragraph>
      The source generator produces a <strong>complete C# class</strong> from every{' '}
      <code>.uitkx</code> file — namespace, partial class, <code>Render()</code> method, and
      everything else. You do <strong>not</strong> need to create any <code>.cs</code> file for a
      component to work.
    </Typography>
    <Typography variant="body1" paragraph>
      Companion files are <strong>optional</strong> <code>.cs</code> files that live next to a{' '}
      <code>.uitkx</code> file. Use them when you want to share styles, type definitions, or utility
      functions with your component.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Directory layout
    </Typography>
    <Typography variant="body1" paragraph>
      Place companion files in the <strong>same directory</strong> as the <code>.uitkx</code> file:
    </Typography>
    <CodeBlock language="text" code={EXAMPLE_DIRECTORY} />

    <Typography variant="h5" component="h2" gutterBottom>
      Naming conventions
    </Typography>
    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>File</strong></TableCell>
            <TableCell><strong>Purpose</strong></TableCell>
            <TableCell><strong>Required?</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>MyComponent.styles.cs</code></TableCell>
            <TableCell>Style constants, helper methods, colours, sizes</TableCell>
            <TableCell>No</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>MyComponent.types.cs</code></TableCell>
            <TableCell>Enums, structs, DTOs used by the component</TableCell>
            <TableCell>No</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>MyComponent.utils.cs</code></TableCell>
            <TableCell>Pure helper / formatting functions</TableCell>
            <TableCell>No</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <Typography variant="body2" paragraph>
      These names are conventions, not enforced rules. Any <code>.cs</code> file (except{' '}
      <code>.g.cs</code>) in the same directory is automatically picked up during compilation.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Example: style helpers
    </Typography>
    <CodeBlock language="csharp" code={EXAMPLE_STYLES} />

    <Typography variant="h5" component="h2" gutterBottom>
      Example: type definitions
    </Typography>
    <CodeBlock language="csharp" code={EXAMPLE_TYPES} />

    <Typography variant="h5" component="h2" gutterBottom>
      Example: utility functions
    </Typography>
    <CodeBlock language="csharp" code={EXAMPLE_UTILS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Using them in UITKX
    </Typography>
    <Typography variant="body1" paragraph>
      Reference companion types and methods directly in your <code>.uitkx</code> — they compile
      together:
    </Typography>
    <CodeBlock language="tsx" code={EXAMPLE_UITKX} />

    <Typography variant="h5" component="h2" gutterBottom>
      HMR support
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Editing a companion .cs file automatically triggers HMR for the associated .uitkx." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Creating a new companion file is detected instantly — the file watcher picks up new files." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="All .cs files in the directory (except .g.cs) are included in compilation." />
      </ListItem>
    </List>

    <Typography variant="h5" component="h2" gutterBottom>
      When not to use companion files
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Simple components — if a component has no shared styles or types, it doesn't need any companion files." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText
          primary={
            <>
              Small helpers — for code that only the component uses, prefer{' '}
              <code>@code</code> blocks inside the <code>.uitkx</code> file itself.
            </>
          }
        />
      </ListItem>
    </List>
  </Box>
)
