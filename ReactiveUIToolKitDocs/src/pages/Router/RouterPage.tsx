import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../components/CodeBlock/CodeBlock'
import Styles from './RouterPage.style'
import { ROUTER_USAGE } from './RouterPage.example'

export const RouterPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Router
    </Typography>
    <Typography variant="body1" paragraph>
      Client-side routing for VirtualNode trees.
    </Typography>
    <Typography variant="h6" component="h3" gutterBottom>
      Usage
    </Typography>
    <CodeBlock language="tsx" codeRuntime={ROUTER_USAGE} />
  </Box>
)
