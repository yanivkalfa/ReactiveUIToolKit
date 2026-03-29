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

type PropEntry = { name: string; type: string; inherited: boolean }
let propsTable: Record<string, PropEntry[]> = {}

// Parse BaseProps property names so we can mark inherited props
const basePropsPath = path.join(typedPropsDir, 'BaseProps.cs')
const basePropsNames = new Set<string>()
try {
  const baseRaw = fs.readFileSync(basePropsPath, 'utf-8').replace(/^\uFEFF/, '')
  const propRe = /public\s+(\S+(?:<[^>]+>)?(?:\?)?)\s+(\w+)\s*\{\s*get\s*;\s*set\s*;\s*\}/g
  let m
  while ((m = propRe.exec(baseRaw)) !== null) {
    basePropsNames.add(m[2])
  }
} catch { /* ok */ }

try {
  const entries = fs
    .readdirSync(typedPropsDir, { withFileTypes: true })
    .filter((e) => e.isFile() && e.name.endsWith('Props.cs'))

  for (const entry of entries) {
    const filePath = path.join(typedPropsDir, entry.name)
    const raw = fs.readFileSync(filePath, 'utf-8').replace(/^\uFEFF/, '')
    const key = path.basename(entry.name, '.cs')
    propsDocs[key] = stripDictionaryMethods(raw)

    // Extract structured props for table display
    const props: PropEntry[] = []
    const propRe = /public\s+(\S+(?:<[^>]+>)?(?:\?)?)\s+(\w+)\s*\{\s*get\s*;\s*set\s*;\s*\}/g
    let m
    while ((m = propRe.exec(raw)) !== null) {
      props.push({ name: m[2], type: m[1], inherited: basePropsNames.has(m[2]) })
    }
    if (props.length > 0) propsTable[key] = props
  }
} catch {
  propsDocs = {}
  propsTable = {}
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  define: {
    __PACKAGE_VERSION__: JSON.stringify(unityPackageJson.version),
    __UNITY_VERSION__: JSON.stringify(unityPackageJson.unity ?? '6000.2'),
    __PROPS_DOCS__: JSON.stringify(propsDocs),
    __PROPS_TABLE__: JSON.stringify(propsTable),
  },
  css: {
    postcss: './postcss.config.cjs',
  },
})
