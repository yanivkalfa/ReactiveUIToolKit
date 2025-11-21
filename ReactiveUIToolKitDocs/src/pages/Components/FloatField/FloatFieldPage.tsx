import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import Styles from './FloatFieldPage.style'
import { FLOAT_FIELD_BASIC } from './FloatFieldPage.example'

export const FloatFieldPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      FloatField
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.FloatField</code> represents a single-precision numeric field, backed by{' '}
      <code>FloatFieldProps</code>.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={FLOAT_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        <code>FloatFieldProps.Label</code> and <code>FloatFieldProps.VisualInput</code> let you
        customize the label element and the inner input container. Both accept dictionaries: build a
        label via <code>LabelProps.ToDictionary()</code> and pass a dictionary with a nested{' '}
        <code>Style</code> object to <code>VisualInput</code> to style the input.
      </Typography>
    </Box>
  </Box>
)

