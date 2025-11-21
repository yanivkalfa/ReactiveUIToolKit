import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './EnumFieldPage.style'
import { ENUM_FIELD_BASIC } from './EnumFieldPage.example'

export const EnumFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      EnumField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.EnumField</code> binds to any enum type via <code>EnumFieldProps</code>. Provide the
      enum&apos;s assembly-qualified type name and an initial <code>Value</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={ENUM_FIELD_BASIC} />
    </Box>
  </Box>
)

