import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import Styles from './RoadmapPage.style'

export const RoadmapPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Roadmap
    </Typography>
    <Typography variant="body1" paragraph>
      The roadmap will be documented here in a future update.
    </Typography>
  </Box>
)
