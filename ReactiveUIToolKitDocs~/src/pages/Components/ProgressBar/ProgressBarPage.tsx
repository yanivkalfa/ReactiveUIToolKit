import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './ProgressBarPage.style'
import { PROGRESS_BAR_BASIC } from './ProgressBarPage.example'

export const ProgressBarPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      ProgressBar
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.ProgressBar</code> renders a UI Toolkit <code>ProgressBar</code> using{' '}
      <code>ProgressBarProps</code>. It is typically driven by state changes elsewhere in your UI.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('ProgressBarProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={PROGRESS_BAR_BASIC} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Styling track and fill
      </Typography>
      <Typography variant="body1" paragraph>
        The{' '}
        <a
          href="https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-uxml-element-ProgressBar.html"
          target="_blank"
          rel="noreferrer"
        >
          Unity ProgressBar documentation
        </a>{' '}
        highlights that the root element is the visible track, while the inner{' '}
        <code>.unity-progress-bar__progress</code> child renders the filled portion.
      </Typography>
      <Typography variant="body1" paragraph>
        Assign styles to the track via <code>ProgressBarProps.Style</code> (for border, unfilled
        background, size, etc.) and target the fill through the <code>Progress</code> slot. You can
        also style the caption by populating <code>TitleElement</code>. The example above uses this
        pattern to create a progress bar with a dark green track, a lighter fill, and centered text.
      </Typography>
    </Box>
  </Box>
)
