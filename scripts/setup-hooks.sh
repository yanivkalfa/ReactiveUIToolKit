#!/usr/bin/env bash
set -euo pipefail

set -e

# Determine package root from this script's directory
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PKG_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
HOOKS_PATH_ABS="$PKG_ROOT/.githooks"

echo "Configuring local git hooks path to $HOOKS_PATH_ABS ..."
git config core.hooksPath "$HOOKS_PATH_ABS"

echo "Ensuring working copy has visible Samples folder ..."
if [ -d "$PKG_ROOT/Samples~" ]; then
  mv -f "$PKG_ROOT/Samples~" "$PKG_ROOT/Samples" || true
fi

echo "Done. Hooks will rename Samples -> Samples~ for commits, and restore after."

