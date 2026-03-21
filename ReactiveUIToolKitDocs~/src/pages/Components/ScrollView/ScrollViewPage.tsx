import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ScrollViewPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { SCROLL_VIEW_BASIC } from './ScrollViewPage.example'

export const ScrollViewPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ScrollView
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ScrollView</code> wraps the UI Toolkit <code>ScrollView</code> element using{' '}
      <code>ScrollViewProps</code>. It is the primary way to add scrolling regions to your layouts.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('ScrollViewProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={SCROLL_VIEW_BASIC} />
    </Box>
    <UnityDocsSection componentName="ScrollView" />
  </Box>
)

