import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './TabPage.style'
import { TAB_BASIC } from './TabPage.example'

export const TabPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Tab
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Tab</code> renders an individual tab using <code>TabProps</code>. In most cases you
      will use it indirectly via <code>TabView</code>, but you can also construct tab strips
      manually.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('TabProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={TAB_BASIC} />
    </Box>
  </Box>
)

