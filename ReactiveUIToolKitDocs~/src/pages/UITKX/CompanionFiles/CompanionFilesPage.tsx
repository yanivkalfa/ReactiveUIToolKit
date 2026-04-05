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
  EXAMPLE_UITKX,
  EXAMPLE_GENERATED_CLASS,
  EXAMPLE_DIRECTORY,
  EXAMPLE_STYLES,
  EXAMPLE_TYPES,
  EXAMPLE_UTILS,
  EXAMPLE_STANDALONE,
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
      The UITKX component
    </Typography>
    <Typography variant="body1" paragraph>
      Here is a component that uses styles, types, and utility functions defined in companion files:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_UITKX} />

    <Typography variant="h5" component="h2" gutterBottom>
      Generated namespace &amp; class name
    </Typography>
    <Typography variant="body1" paragraph>
      The source generator creates a C# class from the <code>.uitkx</code> file. Two things
      determine its identity:
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText
          primary={
            <>
              <strong>Namespace</strong> — comes from the <code>@namespace</code> directive at the
              top of the <code>.uitkx</code> file. If omitted, the generator looks for a companion{' '}
              <code>.cs</code> file with the same name (e.g. <code>PlayerCard.cs</code> next to{' '}
              <code>PlayerCard.uitkx</code>) and uses its namespace declaration. If neither exists,
              it falls back to <code>ReactiveUITK.FunctionStyle</code>. Declaring{' '}
              <code>@namespace</code> explicitly is recommended.
            </>
          }
        />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText
          primary={
            <>
              <strong>Class name</strong> — comes from the <code>component</code> name (the
              identifier after the <code>component</code> keyword).
            </>
          }
        />
      </ListItem>
    </List>
    <Typography variant="body1" paragraph>
      For the example above, the generator produces:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_GENERATED_CLASS} />
    <Typography variant="body1" paragraph>
      Companion <code>.cs</code> files that need to reference or extend the generated class must use
      the <strong>same namespace</strong> and <strong>same class name</strong>.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Directory layout
    </Typography>
    <Typography variant="body1" paragraph>
      Place companion files in the <strong>same directory</strong> as the <code>.uitkx</code> file:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_DIRECTORY} />
    <Typography variant="body2" paragraph>
      These names are conventions, not enforced rules. Any <code>.cs</code> file (except{' '}
      <code>.g.cs</code>) in the same directory is automatically picked up during compilation.
    </Typography>

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
          <TableRow>
            <TableCell><code>MyComponent.extra.cs</code></TableCell>
            <TableCell>Partial class extension (same namespace + class name)</TableCell>
            <TableCell>No</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>

    <Typography variant="h5" component="h2" gutterBottom>
      Example: style helpers
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_STYLES} />

    <Typography variant="h5" component="h2" gutterBottom>
      Example: type definitions
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_TYPES} />

    <Typography variant="h5" component="h2" gutterBottom>
      Example: utility functions
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_UTILS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Standalone classes
    </Typography>
    <Typography variant="body1" paragraph>
      Not everything has to go in the partial class. Standalone classes under the same namespace are
      useful for types shared across multiple components:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_STANDALONE} />

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
              Small helpers — for code that only the component uses, prefer
              setup code before <code>return()</code> inside the <code>.uitkx</code> file itself.
            </>
          }
        />
      </ListItem>
    </List>
  </Box>
)
