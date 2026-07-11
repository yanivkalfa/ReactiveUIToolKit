#!/usr/bin/env node
// Copyright (c) 2026 Yaniv Kalfa. All Rights Reserved.
/**
 * The family-corpus mirror gate (A4 / TD-009) — Unity leg (leg 3). The scanner corpus is the FIRST
 * mirrored set across the three-engine family (Unreal .uetkx / Godot .guitkx / Unity .uitkx). The
 * markup + import grammar cases are BYTE-IDENTICAL family-wide once the diagnostic code prefix is
 * normalized (UETKX|GUITKX|UITKX -> TKX); this script hashes exactly those (the sections listed
 * under `_tiers.familyCore` in uitkx-scanner-cases.json) and compares against the committed
 * Plans~/family-corpus.hash. This is the SAME script + hash file adopted from the Unreal leg; a
 * release-time hash-match across all three repos is TD-009's resolution.
 *
 *   node scripts/corpus-hash.mjs            print the current family-core hash
 *   node scripts/corpus-hash.mjs --check    compare against Plans~/family-corpus.hash (CI gate)
 *   node scripts/corpus-hash.mjs --write     (re)write Plans~/family-corpus.hash
 */
import { readFileSync, writeFileSync, existsSync } from 'fs';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';
import { createHash } from 'crypto';

const REPO_ROOT = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const CORPUS_FILE = resolve(REPO_ROOT, 'ide-extensions~/lsp-server/test-fixtures/uitkx-scanner-cases.json');
const HASH_FILE = resolve(REPO_ROOT, 'Plans~/family-corpus.hash');

/** Fold the three per-engine diagnostic prefixes to a neutral token so the hash is engine-agnostic. */
function normalizeCodes(s) {
  return s.replace(/UETKX|GUITKX|UITKX/g, 'TKX');
}

/** The canonical, order-independent family-core case set → its sha256. */
export function computeFamilyCoreHash(corpus) {
  const tiers = corpus._tiers;
  if (!tiers || !Array.isArray(tiers.familyCore)) {
    throw new Error('uitkx-scanner-cases.json is missing `_tiers.familyCore` — cannot partition the corpus');
  }
  const familyCore = new Set(tiers.familyCore);
  const rows = [];
  for (const [section, value] of Object.entries(corpus)) {
    if (section.startsWith('_') || !Array.isArray(value)) continue;
    for (const c of value) {
      // A case's OWNING section is its own `section` field when present (the nonBmp router),
      // otherwise the JSON key it lives under. Only owner-in-familyCore cases are hashed.
      const owner = typeof c.section === 'string' ? c.section : section;
      if (!familyCore.has(owner)) continue;
      rows.push({
        owner,
        name: c.name ?? '',
        input: c.input ?? '',
        at: c.at ?? null,
        expect: c.expect ?? null,
        // any additional structural fields a fileScan case carries travel verbatim so the
        // family hash covers the FULL contract, not just the lexer quartet.
        extra: canonicalExtra(c),
      });
    }
  }
  // Order-independent: sort by (owner, name) — names are unique within a section by convention.
  rows.sort((a, b) => (a.owner + ' ' + a.name).localeCompare(b.owner + ' ' + b.name, 'en'));
  const canonical = normalizeCodes(JSON.stringify(rows));
  return createHash('sha256').update(canonical, 'utf8').digest('hex');
}

/** Every case field except the ones already captured, with keys sorted for determinism. */
function canonicalExtra(c) {
  const skip = new Set(['section', 'name', 'input', 'at', 'expect']);
  const out = {};
  for (const k of Object.keys(c).sort()) {
    if (!skip.has(k)) out[k] = c[k];
  }
  return out;
}

function main() {
  if (!existsSync(CORPUS_FILE)) {
    console.error(`✗ missing corpus: ${CORPUS_FILE}`);
    process.exit(1);
  }
  const corpus = JSON.parse(readFileSync(CORPUS_FILE, 'utf8'));
  const hash = computeFamilyCoreHash(corpus);
  const mode = process.argv[2];

  if (mode === '--write') {
    writeFileSync(HASH_FILE, hash + '\n', 'utf8');
    console.error(`✓ wrote family-core corpus hash: ${hash}`);
    return;
  }

  if (mode === '--check') {
    if (!existsSync(HASH_FILE)) {
      console.error(`✗ missing ${HASH_FILE} — run: node scripts/corpus-hash.mjs --write`);
      process.exit(1);
    }
    const expected = readFileSync(HASH_FILE, 'utf8').trim();
    if (expected !== hash) {
      console.error('✗ family-core corpus hash drifted.');
      console.error(`    committed: ${expected}`);
      console.error(`    computed:  ${hash}`);
      console.error('  If you intentionally changed a familyCore scanner case, re-pin it:');
      console.error('    node scripts/corpus-hash.mjs --write   (then mirror the case into sibling repos)');
      process.exit(1);
    }
    console.error(`✓ family-core corpus hash matches: ${hash}`);
    return;
  }

  // default: print the hash (stdout so it composes with shell)
  process.stdout.write(hash + '\n');
}

// Only run when invoked directly (allow importing computeFamilyCoreHash from tests).
if (resolve(process.argv[1]) === resolve(fileURLToPath(import.meta.url))) {
  main();
}
