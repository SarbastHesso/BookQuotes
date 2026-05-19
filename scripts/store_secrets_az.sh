#!/usr/bin/env bash
# Usage: ./store_secrets_az.sh <key-vault-name> <jwt-key> <postgres-connection-string>
set -euo pipefail
if [ "$#" -lt 3 ]; then
  echo "Usage: $0 <key-vault-name> <jwt-key> <postgres-connection-string>"
  exit 1
fi
KV_NAME=$1
JWT_KEY=$2
PG_CONN=$3

# Store secrets in Azure Key Vault
az keyvault secret set --vault-name "$KV_NAME" --name "Jwt--Key" --value "$JWT_KEY"
az keyvault secret set --vault-name "$KV_NAME" --name "ConnectionStrings--PostgresConnection" --value "$PG_CONN"

echo "Secrets stored in Key Vault '$KV_NAME'. Set AZURE_KEY_VAULT_URI to https://$KV_NAME.vault.azure.net/ in your deployment environment or GitHub Actions secrets." 
