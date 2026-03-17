import type { FC } from 'react'
import { useMemo, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { Dialog, DialogContent, Paper, InputBase, List, ListItemButton, ListItemText, Typography, Box, IconButton } from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import CloseIcon from '@mui/icons-material/Close'
import { getFlatForTrack, getTrackFromPath } from '../../docs'
import Styles from './SearchModal.style'

export type SearchModalProps = { open: boolean; onClose: () => void }

export const SearchModal: FC<SearchModalProps> = ({ open, onClose }) => {
  const nav = useNavigate()
  const { pathname } = useLocation()
  const track = getTrackFromPath(pathname)
  const flat = useMemo(() => getFlatForTrack(track), [track])
  const [q, setQ] = useState('')
  const handleClose = () => {
    setQ('')
    onClose()
  }
  const results = useMemo(() => {
    const needle = q.trim().toLowerCase()
    if (!needle) return [] as typeof flat
    return flat.filter((p) => p.title.toLowerCase().includes(needle) || (p.keywords || []).some((k) => k.toLowerCase().includes(needle)))
  }, [flat, q])
  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="md">
      <DialogContent sx={Styles.content}>
        <Box sx={Styles.header}>
          <Paper sx={Styles.inputPaper} variant="outlined">
            <SearchIcon />
            <InputBase
              autoFocus
              placeholder={`Search ${track === 'uitkx' ? 'UITKX' : 'C#'} docs…`}
              value={q}
              onChange={(e) => setQ(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Escape') handleClose()
                if (e.key === 'Enter' && results[0]) { handleClose(); nav(results[0].path) }
              }}
              sx={{ flex: 1 }}
            />
          </Paper>
          <IconButton onClick={handleClose} aria-label="Close search"><CloseIcon /></IconButton>
        </Box>
        <List>
          {results.map((r) => (
            <ListItemButton key={r.id} onClick={() => { handleClose(); nav(r.path) }}>
              <ListItemText primary={r.title} secondary={(r.keywords || []).join(', ')} />
            </ListItemButton>
          ))}
          {q && results.length === 0 && <Typography sx={Styles.noResults} color="text.secondary">No results</Typography>}
        </List>
      </DialogContent>
    </Dialog>
  )
}
