import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './Vector3IntFieldPage.style'
import { VECTOR3_INT_FIELD_BASIC } from './Vector3IntFieldPage.example'

export const Vector3IntFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Vector3IntField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Vector3IntField</code> wraps the UI Toolkit <code>Vector3IntField</code> control using{' '}
      <code>Vector3IntFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={VECTOR3_INT_FIELD_BASIC} />
    </Box>
  </Box>
)

