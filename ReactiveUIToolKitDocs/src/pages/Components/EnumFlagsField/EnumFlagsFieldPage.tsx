import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './EnumFlagsFieldPage.style'
import { ENUM_FLAGS_FIELD_BASIC } from './EnumFlagsFieldPage.example'

export const EnumFlagsFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      EnumFlagsField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.EnumFlagsField</code> is similar to <code>V.EnumField</code> but supports{' '}
      <code>[Flags]</code> enums.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={ENUM_FLAGS_FIELD_BASIC} />
    </Box>
  </Box>
)

