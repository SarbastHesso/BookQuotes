#!/usr/bin/env bash
# Usage: ./store_secrets_gh.sh <repo> <jwt-key> <postgres-connection-string>
# Requires GitHub CLI (`gh`) and repo write permissions
set -euo pipefail
if [ "$#" -lt 3 ]; then
  echo "Usage: $0 <repo> <jwt-key> <postgres-connection-string>"
  echo "Example: $0 SarbastHesso/BookQuotes \"<JWT_KEY>\" \"Host=...;Password=...\""
  exit 1
fi
REPO=$1
JWT_KEY=$2
PG_CONN=$3

# Set repository secrets
gh secret set JWT_KEY --body "$JWT_KEY" --repo "$REPO"
gh secret set POSTGRES_CONNECTION --body "$PG_CONN" --repo "$REPO"

echo "GitHub Secrets set for $REPO"
