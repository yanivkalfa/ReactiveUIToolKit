import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs'
import path from 'node:path'

const unityPackageJsonPath = path.resolve(process.cwd(), '..', 'package.json')
const unityPackageJsonRaw = fs.readFileSync(unityPackageJsonPath, 'utf-8')
const unityPackageJson = JSON.parse(unityPackageJsonRaw.replace(/^\uFEFF/, ''))

const typedPropsDir = path.resolve(process.cwd(), '..', 'Shared', 'Props', 'Typed')

const stripDictionaryMethods = (source: string): string => {
  let text = source
  const marker = 'public Dictionary<string, object> ToDictionary('

  while (true) {
    const idx = text.indexOf(marker)
    if (idx === -1) break

    // Start from the beginning of the line that declares the method
    let start = idx
    while (start > 0 && text[start - 1] !== '\n') start--

    const braceIndex = text.indexOf('{', idx)
    if (braceIndex === -1) break

    let depth = 0
    let end = braceIndex
    for (let i = braceIndex; i < text.length; i++) {
      const ch = text[i]
      if (ch === '{') depth++
      else if (ch === '}') {
        depth--
        if (depth === 0) {
          end = i + 1
          break
        }
      }
    }

    text = text.slice(0, start) + text.slice(end)
  }

  return text.trim()
}

let propsDocs: Record<string, string> = {}

try {
  const entries = fs
    .readdirSync(typedPropsDir, { withFileTypes: true })
    .filter((e) => e.isFile() && e.name.endsWith('Props.cs'))

  for (const entry of entries) {
    const filePath = path.join(typedPropsDir, entry.name)
    const raw = fs.readFileSync(filePath, 'utf-8').replace(/^\uFEFF/, '')
    const key = path.basename(entry.name, '.cs')
    propsDocs[key] = stripDictionaryMethods(raw)
  }
} catch {
  propsDocs = {}
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  define: {
    __PACKAGE_VERSION__: JSON.stringify(unityPackageJson.version),
    __PROPS_DOCS__: JSON.stringify(propsDocs),
  },
  css: {
    postcss: './postcss.config.cjs',
  },
})
