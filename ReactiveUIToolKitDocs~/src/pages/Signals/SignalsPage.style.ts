import type { SxProps, Theme } from '@mui/material'

const root: SxProps<Theme> = {
  display: 'flex',
  flexDirection: 'column',
  gap: 2,
}

const list: SxProps<Theme> = {
  pl: 2,
}

const Styles = { root, list }

export default Styles
