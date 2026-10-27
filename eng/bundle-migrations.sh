#!/usr/bin/env bash
set -euo pipefail

output_path="${1:-artifacts/migrations/marsa-bundle}"
mkdir -p "$(dirname "$output_path")"

echo "Building migration bundle to $output_path..."
dotnet ef migrations bundle \
  --project src/Marsa.Infrastructure \
  --startup-project src/Marsa.Server \
  --output "$output_path" \
  --self-contained \
  --force

echo "Migration bundle created at $output_path"
