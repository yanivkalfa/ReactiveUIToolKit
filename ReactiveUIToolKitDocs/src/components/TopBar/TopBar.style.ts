import type { SxProps } from '@mui/material'

const appBar: SxProps = { borderBottom: 1, borderColor: 'divider' }
const toolbar: SxProps = { display: 'flex', alignItems: 'center', gap: 2 }

const left: SxProps = { display: 'flex', alignItems: 'center', gap: 1 }
const title: SxProps = { fontWeight: 600, letterSpacing: 0.3 }

const center: SxProps = { flex: 1, display: 'flex', justifyContent: 'center' }
const searchPaper: SxProps = { p: '2px 8px', display: 'flex', alignItems: 'center', gap: 1, width: 360, cursor: 'text' }
const inputFlex: SxProps = { flex: 1 }

const right: SxProps = { ml: 1 }

const Styles = { appBar, toolbar, left, title, center, searchPaper, inputFlex, right }

export default Styles
