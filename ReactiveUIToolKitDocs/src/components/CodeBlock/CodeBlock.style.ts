import type { SxProps, Theme } from '@mui/material'

const wrapper: SxProps = {
  position: 'relative',
  p: 1.5,
  borderRadius: 1,
  overflow: 'hidden',
  border: 1,
  borderColor: 'divider',
}

const copyBtnWrap: SxProps = {
  position: 'absolute',
  right: 0,
  top: 0,
  zIndex: 1,
}

const copyBtn: SxProps<Theme> = {
  color: 'text.secondary',
}

export default { wrapper, copyBtnWrap, copyBtn };