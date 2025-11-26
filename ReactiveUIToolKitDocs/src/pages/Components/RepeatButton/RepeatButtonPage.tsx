import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './RepeatButtonPage.style'
import { REPEAT_BUTTON_BASIC } from './RepeatButtonPage.example'

export const RepeatButtonPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      RepeatButton
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.RepeatButton</code> wraps UI Toolkit&apos;s <code>RepeatButton</code>, invoking{' '}
      <code>OnClick</code> repeatedly while the button is held.
    </Typography>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('RepeatButtonProps')} />
    </Box>
    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <CodeBlock language="tsx" code={REPEAT_BUTTON_BASIC} />
    </Box>
  </Box>
)

