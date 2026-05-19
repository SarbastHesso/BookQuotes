#!/usr/bin/env bash
# Remove local SQLite DB files used for development.
set -euo pipefail
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
ROOT_DIR="$SCRIPT_DIR/.."
DB_FILE="$ROOT_DIR/bookquotes.db"
if [ -f "$DB_FILE" ]; then
  echo "Removing $DB_FILE"
  rm -f "$DB_FILE"
  echo "Removed. You can now run: ASPNETCORE_ENVIRONMENT=Development dotnet run --project $ROOT_DIR/BookQuotes.Api"
else
  echo "$DB_FILE not found. Nothing to remove."
fi
