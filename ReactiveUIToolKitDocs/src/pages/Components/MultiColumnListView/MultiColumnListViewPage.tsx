import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './MultiColumnListViewPage.style'
import { MULTI_COLUMN_LIST_VIEW_BASIC } from './MultiColumnListViewPage.example'

export const MultiColumnListViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      MultiColumnListView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.MultiColumnListView</code> displays tabular data with columns configured via{' '}
      <code>MultiColumnListViewProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={MULTI_COLUMN_LIST_VIEW_BASIC} />
    </Box>
  </Box>
)

