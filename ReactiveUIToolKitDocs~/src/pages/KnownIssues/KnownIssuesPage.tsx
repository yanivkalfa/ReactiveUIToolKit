import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import Styles from './KnownIssuesPage.style'

export const KnownIssuesPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Known Issues
    </Typography>
    <Typography variant="body1" paragraph>
      There is a known issue where <code>MultiColumnListView</code> can briefly jump or snap when
      scrolling large data sets; this will be addressed in a future update.
    </Typography>
  </Box>
)
