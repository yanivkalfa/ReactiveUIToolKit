import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './GroupBoxPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { GROUP_BOX_BASIC } from './GroupBoxPage.example'

export const GroupBoxPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      GroupBox
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.GroupBox</code> wraps the UI Toolkit <code>GroupBox</code> element using{' '}
      <code>GroupBoxProps</code>. It is useful for grouping related controls under a titled header.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('GroupBoxProps')} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <Typography variant="body1" paragraph>
        Provide <code>Text</code> for the group title, a <code>Style</code> for layout, and add
        children that will appear inside the group.
      </Typography>
      <CodeBlock language="jsx" code={GROUP_BOX_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Children
      </Typography>
      <Typography variant="body1" paragraph>
        Children passed to <code>V.GroupBox</code> are rendered inside the group&apos;s content
        container, below the labeled header.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / contentContainer)
      </Typography>
      <Typography variant="body1" paragraph>
        Use <code>GroupBoxProps.Label</code> and <code>GroupBoxProps.ContentContainer</code> to
        style the header label and the inner content container. Both properties accept dictionaries,
        often containing nested <code>Style</code> objects.
      </Typography>
    </Box>
    <UnityDocsSection componentName="GroupBox" />
  </Box>
)

