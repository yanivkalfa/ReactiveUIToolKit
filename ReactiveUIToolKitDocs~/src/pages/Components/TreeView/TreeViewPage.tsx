import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './TreeViewPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { TREE_VIEW_BASIC } from './TreeViewPage.example'

export const TreeViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      TreeView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.TreeView</code> wraps the UI Toolkit <code>TreeView</code> control using{' '}
      <code>TreeViewProps</code>, allowing you to render hierarchical data with a{' '}
      <code>Row</code> function that returns <code>VirtualNode</code> instances.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('TreeViewProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={TREE_VIEW_BASIC} />
    </Box>
    <UnityDocsSection componentName="TreeView" />
  </Box>
)

