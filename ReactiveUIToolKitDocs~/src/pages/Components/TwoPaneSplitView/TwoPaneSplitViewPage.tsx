import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './TwoPaneSplitViewPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { TWO_PANE_SPLIT_VIEW_BASIC } from './TwoPaneSplitViewPage.example'

export const TwoPaneSplitViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      TwoPaneSplitView
    </Typography>
    <Typography variant="body1" paragraph>
      Editor-only splitter layout wrapping Unity&apos;s <code>TwoPaneSplitView</code> via{' '}
      <code>TwoPaneSplitViewProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('TwoPaneSplitViewProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={TWO_PANE_SPLIT_VIEW_BASIC} />
    </Box>
    <UnityDocsSection componentName="TwoPaneSplitView" />
  </Box>
)

