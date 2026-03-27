#!/usr/bin/env node
/**
 * Centralized changelog management for UITKX IDE extensions.
 *
 * Source of truth: ide-extensions~/changelog.json
 * Per-IDE changelogs are generated from this file by CI.
 *
 * Commands:
 *   add              — Append a changelog entry
 *   extract          — Generate per-IDE CHANGELOG.md (stdout or --out)
 *   extract-overview — Generate VS Marketplace overview.md (stdout or --out)
 *   import           — Import entries from an existing markdown changelog
 *
 * Run `node scripts/changelog.mjs` with no args for full usage.
 */
import { readFileSync, writeFileSync, existsSync } from 'fs';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = resolve(__dirname, '..');
const CHANGELOG_PATH = resolve(REPO_ROOT, 'ide-extensions~', 'changelog.json');
const KNOWN_IDES = ['vscode', 'vs2022', 'rider'];

// ── helpers ──────────────────────────────────────────────────────────────────

function readChangelog() {
  try {
    return JSON.parse(readFileSync(CHANGELOG_PATH, 'utf8'));
  } catch {
    return { entries: [] };
  }
}

function writeChangelog(data) {
  writeFileSync(CHANGELOG_PATH, JSON.stringify(data, null, 2) + '\n', 'utf8');
}

function today() {
  return new Date().toISOString().slice(0, 10);
}

function parseArgs(argv) {
  const args = {};
  for (let i = 0; i < argv.length; i++) {
    if (argv[i].startsWith('--')) {
      const key = argv[i].slice(2);
      const next = argv[i + 1];
      if (next && !next.startsWith('--')) {
        args[key] = next;
        i++;
      } else {
        args[key] = true;
      }
    }
  }
  return args;
}

function output(text, args) {
  if (args.out) {
    writeFileSync(resolve(args.out), text, 'utf8');
    console.error(`Wrote ${resolve(args.out)}`);
  } else {
    process.stdout.write(text);
  }
}

// ── add ──────────────────────────────────────────────────────────────────────

function cmdAdd(args) {
  const scope = args.scope;
  const message = args.message;

  if (!scope || !message) {
    console.error(
      'Usage: add --scope <shared|vscode|vs2022|rider> --message "text" ' +
      '[--vscode X.Y.Z] [--vs2022 X.Y.Z] [--rider X.Y.Z] [--date YYYY-MM-DD]'
    );
    process.exit(1);
  }

  if (scope !== 'shared' && !KNOWN_IDES.includes(scope)) {
    console.error(`Unknown scope: ${scope}. Use: shared, ${KNOWN_IDES.join(', ')}`);
    process.exit(1);
  }

  const versions = {};
  for (const ide of KNOWN_IDES) {
    if (args[ide]) versions[ide] = args[ide];
  }
  if (Object.keys(versions).length === 0) {
    console.error('Provide at least one IDE version: --vscode X.Y.Z, --vs2022 X.Y.Z, --rider X.Y.Z');
    process.exit(1);
  }

  const date = args.date || today();
  const data = readChangelog();

  // Find existing entry with matching versions and date
  let entry = data.entries.find(e =>
    e.date === date &&
    Object.entries(versions).every(([k, v]) => e.versions[k] === v)
  );

  if (!entry) {
    entry = { date, versions };
    data.entries.unshift(entry); // newest first
  }

  if (!entry[scope]) entry[scope] = [];
  entry[scope].push(message);

  writeChangelog(data);
  const verStr = Object.entries(versions).map(([k, v]) => `${k} ${v}`).join(', ');
  console.error(`Added to ${scope} in ${date} entry (${verStr})`);
}

// ── extract ──────────────────────────────────────────────────────────────────

function cmdExtract(args) {
  const ide = args.ide;
  if (!ide) {
    console.error('Usage: extract --ide <vscode|vs2022|rider> [--version X.Y.Z] [--out file]');
    process.exit(1);
  }

  const data = readChangelog();
  const relevant = data.entries.filter(e => e.versions && e.versions[ide]);
  const filtered = args.version
    ? relevant.filter(e => e.versions[ide] === args.version)
    : relevant;

  const lines = ['# Changelog', ''];
  for (const entry of filtered) {
    const ver = entry.versions[ide];
    lines.push(`## [${ver}] - ${entry.date}`);

    const messages = [...(entry.shared || []), ...(entry[ide] || [])];
    for (const msg of messages) {
      lines.push(`- ${msg}`);
    }
    lines.push('');
  }

  output(lines.join('\n'), args);
}

// ── extract-overview ─────────────────────────────────────────────────────────

function cmdExtractOverview(args) {
  const ide = args.ide;
  const template = args.template;

  if (!ide || !template) {
    console.error(
      'Usage: extract-overview --ide <vs2022> --template <path-to-overview-template.md> [--out file]'
    );
    process.exit(1);
  }

  const templatePath = resolve(template);
  if (!existsSync(templatePath)) {
    console.error(`Template not found: ${templatePath}`);
    process.exit(1);
  }

  const data = readChangelog();
  const relevant = data.entries.filter(e => e.versions && e.versions[ide]);

  const changelogLines = ['## Changelog', ''];
  for (const entry of relevant) {
    const ver = entry.versions[ide];
    changelogLines.push(`### [${ver}] - ${entry.date}`);

    const messages = [...(entry.shared || []), ...(entry[ide] || [])];
    for (const msg of messages) {
      changelogLines.push(`- ${msg}`);
    }
    changelogLines.push('');
  }

  const templateContent = readFileSync(templatePath, 'utf8').trimEnd();
  const result = templateContent + '\n\n' + changelogLines.join('\n');
  output(result, args);
}

// ── import ───────────────────────────────────────────────────────────────────

function cmdImport(args) {
  const ide = args.ide;
  const file = args.file;

  if (!ide || !file) {
    console.error('Usage: import --ide <vscode|vs2022> --file <path-to-CHANGELOG.md>');
    process.exit(1);
  }

  const filePath = resolve(file);
  if (!existsSync(filePath)) {
    console.error(`File not found: ${filePath}`);
    process.exit(1);
  }

  const content = readFileSync(filePath, 'utf8');
  const data = readChangelog();

  // Skip lines that are just noise (re-publish, skip, etc.)
  const SKIP = new Set([
    'skip', 'skip-no-new-entry', 'already set', 're-publish',
    'no-op reinstall', 'fix IsSuppressed filter',
  ]);

  // Parse: ## [VERSION] - DATE  or  ### [VERSION] - DATE
  const versionRe = /^#{2,3}\s*\[([^\]]+)\]\s*-?\s*(\d{4}-\d{2}-\d{2})?/;
  let curVer = null;
  let curDate = null;

  for (const line of content.split('\n')) {
    const m = line.match(versionRe);
    if (m) {
      curVer = m[1];
      curDate = m[2] || 'unknown';
      continue;
    }

    if (curVer && line.startsWith('- ')) {
      const msg = line.slice(2).trim();
      if (!msg || SKIP.has(msg)) continue;

      // Find or create entry for this IDE + version
      let entry = data.entries.find(e => e.versions && e.versions[ide] === curVer);
      if (!entry) {
        entry = { date: curDate, versions: { [ide]: curVer } };
        data.entries.push(entry);
      }

      if (!entry[ide]) entry[ide] = [];
      if (!entry[ide].includes(msg)) {
        entry[ide].push(msg);
      }
    }
  }

  // Sort newest first (date desc, then version desc). "unknown" dates sort last.
  data.entries.sort((a, b) => {
    const da = a.date === 'unknown' ? '0000-00-00' : a.date;
    const db = b.date === 'unknown' ? '0000-00-00' : b.date;
    if (da !== db) return da > db ? -1 : 1;
    const aMax = Math.max(...Object.values(a.versions).map(v => parseFloat(v.split('.').pop()) || 0));
    const bMax = Math.max(...Object.values(b.versions).map(v => parseFloat(v.split('.').pop()) || 0));
    return bMax - aMax;
  });

  writeChangelog(data);
  const count = data.entries.filter(e => e.versions && e.versions[ide]).length;
  console.error(`Imported ${count} ${ide} entries from ${file}`);
}

// ── main ─────────────────────────────────────────────────────────────────────

const [command, ...rest] = process.argv.slice(2);
const args = parseArgs(rest);

switch (command) {
  case 'add':              cmdAdd(args); break;
  case 'extract':          cmdExtract(args); break;
  case 'extract-overview': cmdExtractOverview(args); break;
  case 'import':           cmdImport(args); break;
  default:
    console.log(
`Usage: node scripts/changelog.mjs <command> [options]

Commands:
  add              Add a changelog entry
  extract          Generate per-IDE CHANGELOG.md
  extract-overview Generate overview.md with changelog section
  import           Import entries from existing markdown changelog

Examples:
  add --scope shared --message "Fix: server crash" --vscode 1.0.292 --vs2022 1.0.69
  add --scope vscode --message "Fix: debounce" --vscode 1.0.292
  extract --ide vscode --out ide-extensions~/vscode/CHANGELOG.md
  extract --ide vscode --version 1.0.292
  extract-overview --ide vs2022 --template ide-extensions~/visual-studio/UitkxVsix/overview-template.md --out overview.md
  import --ide vscode --file ide-extensions~/vscode/CHANGELOG.md`
    );
}
