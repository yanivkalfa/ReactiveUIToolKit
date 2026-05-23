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
  EXAMPLE_HOOKS,
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
      everything else. A component can work with just a single <code>.uitkx</code> file.
    </Typography>
    <Typography variant="body1" paragraph>
      Companion files are <strong>optional</strong> <code>.uitkx</code> files that live next to a
      component using <code>hook</code> and <code>module</code> keywords. Use them to extract
      reusable state logic, styles, type definitions, or utility functions.
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
              top of the <code>.uitkx</code> file.
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

    <Typography variant="h5" component="h2" gutterBottom>
      Directory layout
    </Typography>
    <Typography variant="body1" paragraph>
      Place companion files in the <strong>same directory</strong> as the <code>.uitkx</code>{' '}
      component:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_DIRECTORY} />

    <Typography variant="h5" component="h2" gutterBottom>
      File types
    </Typography>
    <TableContainer component={Paper} variant="outlined" sx={{ mb: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell><strong>File</strong></TableCell>
            <TableCell><strong>Keyword</strong></TableCell>
            <TableCell><strong>Purpose</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow>
            <TableCell><code>MyComponent.uitkx</code></TableCell>
            <TableCell><code>component</code></TableCell>
            <TableCell>UI markup + setup code</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>MyComponent.hooks.uitkx</code></TableCell>
            <TableCell><code>hook</code></TableCell>
            <TableCell>Custom hooks — reusable state logic</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>MyComponent.style.uitkx</code></TableCell>
            <TableCell><code>module</code></TableCell>
            <TableCell>Style constants, helpers, colours, sizes</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>MyComponent.types.uitkx</code></TableCell>
            <TableCell><code>module</code></TableCell>
            <TableCell>Enums, structs, DTOs used by the component</TableCell>
          </TableRow>
          <TableRow>
            <TableCell><code>MyComponent.utils.uitkx</code></TableCell>
            <TableCell><code>module</code></TableCell>
            <TableCell>Pure helper / formatting functions</TableCell>
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
    <Typography variant="body2" paragraph>
      All companion files end in <code>.uitkx</code>. The naming conventions (
      <code>.hooks.</code>, <code>.style.</code>, <code>.utils.</code>) are recommendations, not
      enforced rules.
    </Typography>

    <Typography variant="h5" component="h2" gutterBottom>
      Hooks — reusable state logic
    </Typography>
    <Typography variant="body1" paragraph>
      Use the <code>hook</code> keyword to extract stateful logic into reusable functions. Hook
      bodies are pure C# — they can call <code>useState</code>, <code>useEffect</code>,{' '}
      <code>useMemo</code>, and any other built-in hooks. Use <code>{'-> ReturnType'}</code> to
      declare the return type:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_HOOKS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Modules — styles
    </Typography>
    <Typography variant="body1" paragraph>
      Use the <code>module</code> keyword to add styles, constants, and helpers to your component.
      When the module name matches the component name, it extends the component's partial class:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_STYLES} />

    <Typography variant="h5" component="h2" gutterBottom>
      Modules — type definitions
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_TYPES} />

    <Typography variant="h5" component="h2" gutterBottom>
      Modules — utility functions
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_UTILS} />

    <Typography variant="h5" component="h2" gutterBottom>
      Standalone modules
    </Typography>
    <Typography variant="body1" paragraph>
      Not everything has to be tied to a component. Standalone modules with a unique name are useful
      for types shared across multiple components:
    </Typography>
    <CodeBlock language="jsx" code={EXAMPLE_STANDALONE} />

    <Typography variant="h5" component="h2" gutterBottom>
      HMR support
    </Typography>
    <List>
      <ListItem disablePadding>
        <ListItemText primary="Editing a hook file triggers HMR — the hook delegate is swapped in-place and all components re-render with the new logic." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Hook state is preserved across swaps (useState, useRef, useEffect, etc.)." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Module changes (styles, utilities) hot-reload in-place: static readonly field initializers are re-evaluated and the new values are copied into the live module type, and static method bodies are swapped via per-method delegate trampolines. No domain reload required." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="Adding a new field or method to a module mid-session is a CLR rude edit — the project type's metadata cannot grow at runtime. HMR auto-recovers by scheduling a domain reload (configurable via the HMR window's 'Auto-reload on rude edit' toggle, default on; or EditorPref UITKX_HMR_AutoReloadOnRudeEdit). A once-per-session warning is logged either way." />
      </ListItem>
      <ListItem disablePadding>
        <ListItemText primary="For HMR-able module values prefer fields (public static readonly Style Root = …) over static auto-properties — the C# compiler lowers get-only auto-properties to a private static readonly backing field that the source generator cannot rewrite, so the JIT inlines the cold value and HMR cannot refresh it." />
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
