import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './DropdownFieldPage.style'
import { DROPDOWN_FIELD_BASIC } from './DropdownFieldPage.example'

export const DropdownFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      DropdownField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.DropdownField</code> renders a text-based dropdown using <code>DropdownFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={DROPDOWN_FIELD_BASIC} />
    </Box>
  </Box>
)

