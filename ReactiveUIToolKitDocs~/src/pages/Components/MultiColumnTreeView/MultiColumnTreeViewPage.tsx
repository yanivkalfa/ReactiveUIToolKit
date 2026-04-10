import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './MultiColumnTreeViewPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { MULTI_COLUMN_TREE_VIEW_BASIC } from './MultiColumnTreeViewPage.example'

export const MultiColumnTreeViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      MultiColumnTreeView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.MultiColumnTreeView</code> renders hierarchical data across multiple columns via{' '}
      <code>MultiColumnTreeViewProps</code>. It is backed by Unity&apos;s{' '}
      <code>MultiColumnTreeView</code> control and is suitable for project browser–style views.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('MultiColumnTreeViewProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Concepts
      </Typography>
      <List sx={Styles.section}>
        <ListItem disablePadding>
          <ListItemText primary="Items are provided as a tree of nodes; the adapter flattens and expands them based on TreeView state." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Columns are defined via MultiColumnTreeViewColumn objects, just like MultiColumnListView." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Each Cell callback receives the node item and index so you can render per-column content (labels, badges, icons)." />
        </ListItem>
      </List>
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={MULTI_COLUMN_TREE_VIEW_BASIC} />
    </Box>
    <UnityDocsSection componentName="MultiColumnTreeView" />
  </Box>
)
