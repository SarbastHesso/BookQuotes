#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 9 ]]; then
  echo "Usage: $0 <repo> <staging-host> <staging-user> <staging-app-path> <staging-domain> <postgres-connection> <jwt-key> <ghcr-read-username> <ghcr-read-token> [ssh-port] [jwt-issuer] [jwt-audience] [jwt-expires-minutes]"
  exit 1
fi

REPO=$1
STAGING_HOST=$2
STAGING_USER=$3
STAGING_APP_PATH=$4
STAGING_DOMAIN=$5
STAGING_POSTGRES_CONNECTION=$6
STAGING_JWT_KEY=$7
GHCR_READ_USERNAME=$8
GHCR_READ_TOKEN=$9
STAGING_SSH_PORT=${10:-22}
STAGING_JWT_ISSUER=${11:-BookQuotesApi}
STAGING_JWT_AUDIENCE=${12:-BookQuotesClient}
STAGING_JWT_EXPIRES_IN_MINUTES=${13:-60}

if [[ ! -f "$HOME/.ssh/id_ed25519" && ! -f "$HOME/.ssh/id_rsa" ]]; then
  echo "No default SSH private key found in ~/.ssh. Provide STAGING_SSH_PRIVATE_KEY manually or create a deploy key first."
  exit 1
fi

SSH_KEY_PATH="$HOME/.ssh/id_ed25519"
if [[ ! -f "$SSH_KEY_PATH" ]]; then
  SSH_KEY_PATH="$HOME/.ssh/id_rsa"
fi

gh secret set STAGING_SSH_PRIVATE_KEY --repo "$REPO" < "$SSH_KEY_PATH"
gh secret set STAGING_HOST --body "$STAGING_HOST" --repo "$REPO"
gh secret set STAGING_USER --body "$STAGING_USER" --repo "$REPO"
gh secret set STAGING_APP_PATH --body "$STAGING_APP_PATH" --repo "$REPO"
gh secret set STAGING_DOMAIN --body "$STAGING_DOMAIN" --repo "$REPO"
gh secret set STAGING_POSTGRES_CONNECTION --body "$STAGING_POSTGRES_CONNECTION" --repo "$REPO"
gh secret set STAGING_JWT_KEY --body "$STAGING_JWT_KEY" --repo "$REPO"
gh secret set GHCR_READ_USERNAME --body "$GHCR_READ_USERNAME" --repo "$REPO"
gh secret set GHCR_READ_TOKEN --body "$GHCR_READ_TOKEN" --repo "$REPO"

gh variable set STAGING_SSH_PORT --body "$STAGING_SSH_PORT" --repo "$REPO"
gh variable set STAGING_JWT_ISSUER --body "$STAGING_JWT_ISSUER" --repo "$REPO"
gh variable set STAGING_JWT_AUDIENCE --body "$STAGING_JWT_AUDIENCE" --repo "$REPO"
gh variable set STAGING_JWT_EXPIRES_IN_MINUTES --body "$STAGING_JWT_EXPIRES_IN_MINUTES" --repo "$REPO"

echo "Staging GitHub secrets and variables configured for $REPO"
