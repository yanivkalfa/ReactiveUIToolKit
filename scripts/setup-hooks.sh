#!/usr/bin/env bash
set -euo pipefail

set -e

# Resolve repo root and configure hooks path to the package-local .githooks folder
REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || true)
if [ -z "$REPO_ROOT" ]; then
  echo "Error: not inside a git repository. Run this from within your repo." >&2
  exit 1
fi

HOOKS_PATH_REL="Assets/ReactiveUIToolKit/.githooks"
HOOKS_PATH_ABS="$REPO_ROOT/$HOOKS_PATH_REL"

echo "Configuring local git hooks path to $HOOKS_PATH_REL ..."
git config core.hooksPath "$HOOKS_PATH_REL"

echo "Ensuring working copy has visible Samples folder ..."
PKG_ROOT="Assets/ReactiveUIToolKit"
if [ -d "$PKG_ROOT/Samples~" ]; then
  mv -f "$PKG_ROOT/Samples~" "$PKG_ROOT/Samples" || true
fi

echo "Done. Hooks will rename Samples -> Samples~ for commits, and restore after."

