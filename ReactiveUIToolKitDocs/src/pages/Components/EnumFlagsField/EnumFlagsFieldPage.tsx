import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
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
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('EnumFlagsFieldProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={ENUM_FLAGS_FIELD_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (label / visual input)
      </Typography>
      <Typography variant="body1" paragraph>
        <code>EnumFlagsFieldProps.Label</code> and <code>EnumFlagsFieldProps.VisualInput</code>
        behave the same as on <code>EnumFieldProps</code>, allowing you to style the label element
        and the embedded input area via dictionaries that can contain nested <code>Style</code>{' '}
        objects.
      </Typography>
    </Box>
  </Box>
)

