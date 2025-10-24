#!/usr/bin/env bash
set -euo pipefail

echo "Configuring local git hooks path to .githooks ..."
git config core.hooksPath .githooks

echo "Ensuring working copy has visible Samples folder ..."
PKG_ROOT="Assets/ReactiveUIToolKit"
if [ -d "$PKG_ROOT/Samples~" ]; then
  mv -f "$PKG_ROOT/Samples~" "$PKG_ROOT/Samples" || true
fi

echo "Done. Hooks will rename Samples -> Samples~ for commits, and restore after."

