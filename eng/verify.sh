#!/usr/bin/env bash
set -euo pipefail

dotnet restore --locked-mode
dotnet build Marsa.sln -warnaserror
dotnet test Marsa.sln --no-build
python3 .github/scripts/check-docs.py
