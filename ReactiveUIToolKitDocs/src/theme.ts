import { createTheme } from '@mui/material'

export const theme = createTheme({
  palette: {
    mode: 'dark',
    background: {
      default: '#151821',
      paper: '#1b1f29',
    },
    divider: '#303749',
    primary: { main: '#4cc2ff' },
  },
  shape: { borderRadius: 8 },
  typography: {
    fontSize: 14,
    body1: { lineHeight: 1.4 },
    body2: { lineHeight: 1.4 },
  },
})
