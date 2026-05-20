#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="$ROOT_DIR/coverage"

mkdir -p "$OUTPUT_DIR"
cd "$ROOT_DIR"

mapfile -t test_projects < <(
  find "$ROOT_DIR" -name "*.csproj" -not -path "*/obj/*" -not -path "*/bin/*" |
    while IFS= read -r project; do
      if grep -qE '<IsTestProject>\s*true\s*</IsTestProject>' "$project"; then
        printf '%s\n' "$project"
      fi
    done
)

if [ ${#test_projects[@]} -eq 0 ]; then
  echo "No test projects found. Make sure there is at least one .csproj with <IsTestProject>true</IsTestProject>."
  exit 1
fi

for project in "${test_projects[@]}"; do
  project_name="$(basename "$project" .csproj)"
  results_dir="$OUTPUT_DIR/$project_name"
  mkdir -p "$results_dir"
  echo "Running coverage for: $project"
  dotnet test "$project" --no-restore --collect:"XPlat Code Coverage" --results-directory "$results_dir"
  echo "Coverage output saved to: $results_dir"
  echo
done

echo "Coverage reports generated under: $OUTPUT_DIR"