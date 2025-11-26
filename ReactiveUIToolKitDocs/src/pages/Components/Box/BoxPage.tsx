import type { FC } from 'react'
import { Box, Typography } from '@mui/material'
import { CodeBlock } from '../../../components/CodeBlock/CodeBlock'
import { getPropsDoc } from '../../../propsDocs'
import Styles from './BoxPage.style'
import { BOX_BASIC } from './BoxPage.example'

export const BoxPage: FC = () => (
  <Box sx={Styles.root}>
    <Typography variant="h4" component="h1" gutterBottom>
      Box
    </Typography>
    <Typography variant="body1" paragraph>
      <code>V.Box</code> renders a boxed container element with optional content. It is useful for
      grouping related controls with a background and padding.
    </Typography>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Props
      </Typography>
      <CodeBlock language="tsx" code={getPropsDoc('BoxProps')} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Basic usage
      </Typography>
      <Typography variant="body1" paragraph>
        Pass a <code>BoxProps</code> instance to <code>V.Box</code> and supply children as additional
        arguments.
      </Typography>
      <CodeBlock language="tsx" code={BOX_BASIC} />
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Children
      </Typography>
      <Typography variant="body1">
        Children are rendered inside the box&apos;s content container. Use this to create sections of
        your UI that share common styling.
      </Typography>
    </Box>

    <Box sx={Styles.section}>
      <Typography variant="h5" component="h2" gutterBottom>
        Slots (contentContainer)
      </Typography>
      <Typography variant="body1" paragraph>
        Use the <code>ContentContainer</code> property on <code>BoxProps</code> to style or configure
        the box&apos;s <code>contentContainer</code>. This property expects a dictionary, allowing you
        to pass a nested <code>Style</code> or additional props that should be applied to the content
        container element.
      </Typography>
    </Box>
  </Box>
)

