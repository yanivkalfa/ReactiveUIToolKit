import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './TabViewPage.style'
import { TAB_VIEW_BASIC } from './TabViewPage.example'

export const TabViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      TabView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.TabView</code> renders a tab strip and tab content using <code>TabViewProps</code>.
      Each tab is defined by a <code>TabViewProps.TabDef</code>, which can provide either static
      content or a factory function.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={TAB_VIEW_BASIC} />
    </Box>
  </Box>
)

