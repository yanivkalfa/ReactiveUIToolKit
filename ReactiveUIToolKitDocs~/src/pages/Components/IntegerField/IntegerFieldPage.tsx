import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './IntegerFieldPage.style'
import { UnityDocsSection } from '../../../components/UnityDocsSection/UnityDocsSection'
import { INTEGER_FIELD_BASIC } from './IntegerFieldPage.example'

export const IntegerFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      IntegerField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.IntegerField</code> represents an integer numeric field using <code>IntegerFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="jsx" code={getPropsDoc('IntegerFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="jsx" code={INTEGER_FIELD_BASIC} />
    </Box>
    <UnityDocsSection componentName="IntegerField" />
  </Box>
)

