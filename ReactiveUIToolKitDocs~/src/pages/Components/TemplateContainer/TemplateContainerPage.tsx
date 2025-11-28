import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './TemplateContainerPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { TEMPLATE_CONTAINER_BASIC } from './TemplateContainerPage.example'

export const TemplateContainerPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      TemplateContainer
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.TemplateContainer</code> wraps UI Toolkit <code>TemplateContainer</code> and exposes a{' '}
      <code>ContentContainer</code> slot through <code>TemplateContainerProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('TemplateContainerProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={TEMPLATE_CONTAINER_BASIC} />
    </Box>
    <UnityDocsSection componentName="TemplateContainer" />
  </Box>
)

