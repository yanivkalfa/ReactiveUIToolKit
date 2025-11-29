import type { FC } from 'react'
import { Box, List, ListItem, ListItemText, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './MultiColumnListViewPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { MULTI_COLUMN_LIST_VIEW_BASIC } from './MultiColumnListViewPage.example'

export const MultiColumnListViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      MultiColumnListView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.MultiColumnListView</code> displays tabular data with columns configured via{' '}
      <code>MultiColumnListViewProps</code>. It is backed by Unity&apos;s{' '}
      <code>MultiColumnListView</code> control and supports large, virtualized data sets.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('MultiColumnListViewProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Concepts
      </Typography>
      <List sx={Styles.section}>
        <ListItem disablePadding>
          <ListItemText primary="Items are provided as an IList; rows are virtualized by the underlying control for performance." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="Columns are defined via MultiColumnListViewColumn objects, each with a name, width, and Cell callback." />
        </ListItem>
        <ListItem disablePadding>
          <ListItemText primary="The Cell callback receives the strongly-typed row item and index so you can render arbitrary content per column." />
        </ListItem>
      </List>
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={MULTI_COLUMN_LIST_VIEW_BASIC} />
    </Box>
    <UnityDocsSection componentName="MultiColumnListView" />
  </Box>
)
