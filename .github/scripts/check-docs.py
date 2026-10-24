#!/usr/bin/env python3
"""Check relative Markdown links and required documentation files."""

from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parent.parent.parent
LOCAL = ROOT / ".local"
LINK = re.compile(r"\[[^\]]*\]\(([^)]+)\)")
REQUIRED = [
    "README.md",
    "AGENTS.md",
    "LICENSE",
    "docs/README.md",
    "docs/identity.md",
    "docs/product-requirements.md",
    "docs/architecture/security.md",
    "docs/risks-and-open-questions.md",
    "docs/roadmap.md",
    "docs/development/getting-started.md",
    "docs/development/testing.md",
    "docs/license-and-provenance.md",
    "docs/decisions/README.md",
]


def relative_links(path: Path):
    text = path.read_text(encoding="utf-8")
    for match in LINK.finditer(text):
        target = match.group(1).strip()
        if target.startswith(("http://", "https://", "mailto:", "urn:", "#")):
            continue
        if " " in target and not target.startswith("<"):
            target = target.split(" ", 1)[0]
        target = unquote(target.split("#", 1)[0])
        if target:
            yield match.group(0), target


def main() -> int:
    failures: list[str] = []
    for required in REQUIRED:
        if not (ROOT / required).exists():
            failures.append(f"missing required file: {required}")

    markdown_files = [
        path
        for path in sorted(ROOT.rglob("*.md"))
        if not any(part in {".git", ".local", "node_modules", "bin", "obj", ".nuget"} for part in path.parts)
    ]

    for path in markdown_files:
        for source, target in relative_links(path):
            resolved = (path.parent / target).resolve()
            if resolved.is_relative_to(LOCAL):
                rel = path.relative_to(ROOT)
                failures.append(f"{rel}: public document links into private workspace {source}")
            elif not resolved.exists():
                rel = path.relative_to(ROOT)
                failures.append(f"{rel}: broken link {source} -> {target}")

    if failures:
        print("Documentation checks failed:", file=sys.stderr)
        for failure in failures:
            print(f"  - {failure}", file=sys.stderr)
        return 1

    print(f"Documentation checks passed ({len(markdown_files)} Markdown files).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
