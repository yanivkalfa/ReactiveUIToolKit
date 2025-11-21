import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './MultiColumnTreeViewPage.style'
import { MULTI_COLUMN_TREE_VIEW_BASIC } from './MultiColumnTreeViewPage.example'

export const MultiColumnTreeViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      MultiColumnTreeView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.MultiColumnTreeView</code> renders hierarchical data across multiple columns via{' '}
      <code>MultiColumnTreeViewProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={MULTI_COLUMN_TREE_VIEW_BASIC} />
    </Box>
  </Box>
)

