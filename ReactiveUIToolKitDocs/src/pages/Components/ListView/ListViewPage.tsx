import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './ListViewPage.style'
import { LIST_VIEW_BASIC } from './ListViewPage.example'

export const ListViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ListView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ListView</code> wraps the UI Toolkit <code>ListView</code> control using{' '}
      <code>ListViewProps</code>. It can use either the standard <code>makeItem/bindItem</code>{' '}
      properties or the higher-level <code>Row</code> function that returns a <code>VirtualNode</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={LIST_VIEW_BASIC} />
    </Box>
  </Box>
)

