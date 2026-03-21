#!/usr/bin/env python3

import json
import os
import re
from collections import Counter


ROOTS = [
    "Shared",
    "Runtime",
    "Editor",
    "SourceGenerator~",
    "ide-extensions~",
    "Diagnostics",
    "Samples",
    "ReactiveUIToolKitDocs~",
    "CICD",
    "scripts",
]

EXCLUDED_DIR_NAMES = {
    ".git",
    ".vs",
    "node_modules",
    "dist~",
    "bin",
    "obj",
    "TestResults",
}

EXCLUDED_PATH_PREFIXES = [
    "ReactiveUIToolKitDocs~/dist/",
    "ide-extensions~/vscode/out/",
    "ide-extensions~/vscode/server/",
    "ide-extensions~/visual-studio/UitkxVsix/server/",
    "GeneratedPreview~/",
]

EXCLUDED_EXTENSIONS = {
    ".meta",
    ".dll",
    ".pdb",
    ".deps.json",
    ".trx",
    ".png",
    ".svg",
    ".ico",
    ".vsix",
    ".exe",
    ".map",
}

INCLUDED_EXTENSIONS = {
    ".cs",
    ".ts",
    ".tsx",
    ".uitkx",
    ".json",
    ".md",
    ".asmdef",
    ".csproj",
    ".ps1",
    ".js",
    ".yml",
    ".html",
    ".kts",
    ".properties",
    ".kt",
    ".xml",
    ".txt",
    ".pkgdef",
    ".cjs",
    ".asset",
    ".tss",
    "",
}

TYPE_PATTERN = re.compile(r"\b(class|struct|interface|enum|record)\s+([A-Za-z_][A-Za-z0-9_]*)")
NAMESPACE_PATTERN = re.compile(r"\bnamespace\s+([A-Za-z0-9_\.]+)")
CS_METHOD_PATTERN = re.compile(
    r"\b(?:public|private|internal|protected|static|sealed|override|virtual|async|partial|readonly|extern|unsafe|new)\s+"
    r"[A-Za-z0-9_<>,\.\[\]\?\s:]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\("
)
TS_FUNCTION_PATTERN = re.compile(
    r"\b(?:export\s+)?(?:async\s+)?function\s+([A-Za-z_][A-Za-z0-9_]*)\s*\("
)


def should_skip_path(rel_path: str, ext: str) -> bool:
    if ext in EXCLUDED_EXTENSIONS:
        return True
    if ext not in INCLUDED_EXTENSIONS:
        return True
    normalized = rel_path.replace("\\", "/")
    for prefix in EXCLUDED_PATH_PREFIXES:
        if normalized.startswith(prefix):
            return True
    return False


def detect_tags(text: str, rel_path: str):
    tags = []
    lower_path = rel_path.lower()
    checks = [
        ("state", ["UseState", "useState", "UseReducer"]),
        ("effects", ["UseEffect", "useEffect", "UseLayoutEffect"]),
        ("context", ["UseContext", "ProvideContext"]),
        ("signals", ["Signal<", "UseSignal", "useSignal", "SignalFactory"]),
        ("router", ["Router", "Route", "Link", "Navigate"]),
        ("uitoolkit", ["VisualElement", "UIElements", "UIDocument"]),
        ("diagnostics", ["Diagnostic", "Diagnostics", "LogWarning", "LogError"]),
        ("roslyn", ["Roslyn", "Microsoft.CodeAnalysis", "AdhocWorkspace"]),
        ("formatter", ["Formatter", "Format("]),
        ("completion", ["Completion", "Complete", "Completions"]),
        ("hover", ["Hover", "QuickInfo"]),
        ("uitkx", [".uitkx", "component ", "@component", "Uitkx"]),
    ]
    for tag, needles in checks:
        if any(needle in text for needle in needles):
            tags.append(tag)
    if rel_path.endswith(".uitkx") and "uitkx" not in tags:
        tags.append("uitkx")
    if "showcase" in lower_path and "samples" not in tags:
        tags.append("samples")
    return tags


def extract_methods(ext: str, text: str):
    methods = []
    patterns = []
    if ext == ".cs":
        patterns.append(CS_METHOD_PATTERN)
    elif ext in {".ts", ".tsx", ".js", ".kt"}:
        patterns.append(TS_FUNCTION_PATTERN)
    for pattern in patterns:
        for match in pattern.finditer(text):
            name = match.group(1)
            if name not in methods:
                methods.append(name)
            if len(methods) >= 25:
                return methods
    return methods


def build_index():
    entries = []
    files_by_root = Counter()
    ext_counts = Counter()

    for root in ROOTS:
        if not os.path.exists(root):
            continue
        for dirpath, dirnames, filenames in os.walk(root):
            dirnames[:] = [d for d in dirnames if d not in EXCLUDED_DIR_NAMES]
            for filename in sorted(filenames):
                ext = os.path.splitext(filename)[1]
                rel_path = os.path.join(dirpath, filename).replace("\\", "/")
                if should_skip_path(rel_path, ext):
                    continue
                try:
                    with open(rel_path, "r", encoding="utf-8", errors="ignore") as handle:
                        text = handle.read()
                except OSError:
                    continue

                lines = text.count("\n") + 1 if text else 0
                namespace_match = NAMESPACE_PATTERN.search(text)
                namespace = namespace_match.group(1) if namespace_match else None
                types = [name for _, name in TYPE_PATTERN.findall(text)[:25]]
                methods = extract_methods(ext, text)
                tags = detect_tags(text, rel_path)

                entries.append(
                    {
                        "path": rel_path,
                        "root": root,
                        "ext": ext or "<noext>",
                        "lines": lines,
                        "namespace": namespace,
                        "types": types,
                        "methods": methods,
                        "tags": tags,
                    }
                )
                files_by_root[root] += 1
                ext_counts[ext or "<noext>"] += 1

    entries.sort(key=lambda entry: entry["path"])
    return {
        "authored_file_count": len(entries),
        "roots": dict(sorted(files_by_root.items())),
        "extensions": dict(sorted(ext_counts.items())),
        "excluded_dir_names": sorted(EXCLUDED_DIR_NAMES),
        "excluded_path_prefixes": EXCLUDED_PATH_PREFIXES,
        "entries": entries,
    }


def main():
    index = build_index()
    print(json.dumps(index, indent=2))


if __name__ == "__main__":
    main()
