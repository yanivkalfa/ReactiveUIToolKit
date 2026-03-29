#!/usr/bin/env node
/**
 * apply-diff-to-schema.mjs
 *
 * Reads a unity-api-diff JSON report and patches uitkx-schema.json
 * with sinceUnity / removedIn annotations on elements and attributes.
 *
 * Usage:
 *   node automation~/apply-diff-to-schema.mjs <diff-report.json> <target-version>
 *
 * Example:
 *   # First generate the diff:
 *   pwsh automation~/unity-api-diff.ps1 -From 6000.2 -To 6000.3 -OutFile automation~/diff-reports/6000.2-to-6000.3.json
 *
 *   # Then apply it to the schema:
 *   node automation~/apply-diff-to-schema.mjs automation~/diff-reports/6000.2-to-6000.3.json 6000.3
 *
 * What it does:
 *   - For each element added in the diff → sets sinceUnity on that element in the schema
 *   - For each element removed in the diff → sets removedIn on that element in the schema
 *   - For each IStyle property added → adds to styleVersions with sinceUnity
 *   - For each IStyle property removed → adds removedIn to styleVersions
 *
 * Attribute-level annotations require per-element attribute diffs (not yet in
 * unity-api-diff.ps1 output). When that data is available, this script will
 * also annotate individual attributes.
 */

import { readFileSync, writeFileSync } from 'fs'
import { resolve, dirname } from 'path'
import { fileURLToPath } from 'url'

const __dirname = dirname(fileURLToPath(import.meta.url))

// ── Args ────────────────────────────────────────────────────────────────────

const [diffPath, targetVersion] = process.argv.slice(2)
if (!diffPath || !targetVersion) {
  console.error('Usage: node apply-diff-to-schema.mjs <diff-report.json> <target-version>')
  console.error('  e.g. node apply-diff-to-schema.mjs diff-reports/6000.2-to-6000.3.json 6000.3')
  process.exit(1)
}

// ── Load files ──────────────────────────────────────────────────────────────

const diffFile = resolve(diffPath)
const schemaFile = resolve(__dirname, '..', 'ide-extensions~', 'grammar', 'uitkx-schema.json')
const versionManifestFile = resolve(__dirname, '..', 'ReactiveUIToolKitDocs~', 'src', 'versionManifest.ts')

const diff = JSON.parse(readFileSync(diffFile, 'utf8'))
const schema = JSON.parse(readFileSync(schemaFile, 'utf8'))

let changes = 0

// ── Elements ────────────────────────────────────────────────────────────────

if (diff.elements) {
  for (const name of diff.elements.added ?? []) {
    if (schema.elements?.[name]) {
      if (!schema.elements[name].sinceUnity) {
        schema.elements[name].sinceUnity = targetVersion
        console.log(`  + Element ${name}: sinceUnity = ${targetVersion}`)
        changes++
      }
    } else {
      console.log(`  ? Element ${name} added in diff but not in schema (add manually)`)
    }
  }

  for (const name of diff.elements.removed ?? []) {
    if (schema.elements?.[name]) {
      if (!schema.elements[name].removedIn) {
        schema.elements[name].removedIn = targetVersion
        console.log(`  - Element ${name}: removedIn = ${targetVersion}`)
        changes++
      }
    }
  }
}

// ── IStyle properties → styleVersions ───────────────────────────────────────

if (!schema.styleVersions) schema.styleVersions = {}

if (diff.istyle) {
  for (const item of diff.istyle.added ?? []) {
    const name = typeof item === 'string' ? item : item.name ?? item.key
    if (!name) continue
    if (!schema.styleVersions[name]) {
      schema.styleVersions[name] = { sinceUnity: targetVersion }
      console.log(`  + Style ${name}: sinceUnity = ${targetVersion}`)
      changes++
    }
  }

  for (const item of diff.istyle.removed ?? []) {
    const name = typeof item === 'string' ? item : item.name ?? item.key
    if (!name) continue
    if (!schema.styleVersions[name]) {
      schema.styleVersions[name] = { removedIn: targetVersion }
    } else if (!schema.styleVersions[name].removedIn) {
      schema.styleVersions[name].removedIn = targetVersion
    }
    console.log(`  - Style ${name}: removedIn = ${targetVersion}`)
    changes++
  }
}

// ── Write schema ────────────────────────────────────────────────────────────

if (changes === 0) {
  console.log('\nNo version annotations to apply.')
} else {
  writeFileSync(schemaFile, JSON.stringify(schema, null, 2) + '\n', 'utf8')
  console.log(`\n✓ Applied ${changes} annotation(s) to ${schemaFile}`)
}

// ── Print reminder ──────────────────────────────────────────────────────────

console.log(`
Next steps:
  1. Review the changes in uitkx-schema.json
  2. Update versionManifest.ts if new versions were added:
       ${versionManifestFile}
  3. Rebuild LSP server: dotnet clean -c Release && dotnet publish -c Release -o ../vscode/server --self-contained false
  4. Run tests: cd SourceGenerator~/Tests && dotnet test
`)
