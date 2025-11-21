import type { FC } from 'react'
import { useLocation, Link as RouterLink } from 'react-router-dom'
import { Box, List, ListItemButton, ListItemText, Collapse, Divider } from '@mui/material'
import ExpandLess from '@mui/icons-material/ExpandLess'
import ExpandMore from '@mui/icons-material/ExpandMore'
import { useState } from 'react'
import { pages as sections } from '../../pages'
import Styles from './Sidebar.style'

export const Sidebar: FC = () => {
  const location = useLocation()
  const [open, setOpen] = useState<Record<string, boolean>>(() => {
    const init: Record<string, boolean> = {}
    sections.forEach((sec, idx) => (init[sec.id] = idx === 0))
    return init
  })

  return (
    <Box sx={Styles.root}>
      <List disablePadding>
        {sections.map((sec) => (
          <Box key={sec.id}>
            <ListItemButton onClick={() => setOpen({ ...open, [sec.id]: !open[sec.id] })}>
              <ListItemText primaryTypographyProps={{ fontWeight: 700 }} primary={sec.title} />
              {open[sec.id] ? <ExpandLess /> : <ExpandMore />}
            </ListItemButton>
            <Collapse in={!!open[sec.id]} timeout="auto" unmountOnExit>
              <List disablePadding>
                {sec.pages.map((p) => (
                  <ListItemButton key={p.id} component={RouterLink} to={p.path} selected={location.pathname === p.path} sx={Styles.childItem}>
                    <ListItemText primary={p.title} />
                  </ListItemButton>
                ))}
              </List>
            </Collapse>
            <Divider />
          </Box>
        ))}
      </List>
    </Box>
  )
}
