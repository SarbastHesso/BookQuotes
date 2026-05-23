# Deploy Runbook

This runbook covers the current deployment path for BookQuotes.

## What is in place

- Production-ready staging image publishing to GHCR from `.github/workflows/ci.yml` on pushes to `main`
- Manual staging deployment workflow in `.github/workflows/deploy-staging.yml`
- HTTPS reverse proxy with Caddy in `deploy/Caddyfile`
- Production container topology in `docker-compose.production.yml`
- Cookie-based auth with same-origin API routing through Caddy
- Readiness endpoint: `/health`
- Liveness endpoint: `/health/live`

## Required secrets and variables

GitHub repository secrets:

- `STAGING_SSH_PRIVATE_KEY`
- `STAGING_HOST`
- `STAGING_USER`
- `STAGING_APP_PATH`
- `STAGING_DOMAIN`
- `STAGING_POSTGRES_CONNECTION`
- `STAGING_JWT_KEY`
- `GHCR_READ_USERNAME`
- `GHCR_READ_TOKEN`

Optional GitHub repository/environment variables:

- `STAGING_SSH_PORT`
- `STAGING_JWT_ISSUER`
- `STAGING_JWT_AUDIENCE`
- `STAGING_JWT_EXPIRES_IN_MINUTES`

Host-side environment shape:

- See `deploy/.env.staging.example`

## Exact GitHub setup

Authenticate GitHub CLI first:

```bash
gh auth login
gh auth status
```

Generate a staging JWT key:

```bash
./scripts/generate_jwt_key.sh
```

Windows PowerShell alternative:

```powershell
.\scripts\generate_jwt_key.ps1
```

Set the minimum required staging secrets manually:

```bash
gh secret set STAGING_SSH_PRIVATE_KEY --repo <owner/repo> < ./id_ed25519
gh secret set STAGING_HOST --body "staging.example.com" --repo <owner/repo>
gh secret set STAGING_USER --body "deploy" --repo <owner/repo>
gh secret set STAGING_APP_PATH --body "/opt/bookquotes" --repo <owner/repo>
gh secret set STAGING_DOMAIN --body "staging.bookquotes.example" --repo <owner/repo>
gh secret set STAGING_POSTGRES_CONNECTION --body "Host=<host>;Database=<db>;Username=<user>;Password=<password>" --repo <owner/repo>
gh secret set STAGING_JWT_KEY --body "<generated-jwt-key>" --repo <owner/repo>
gh secret set GHCR_READ_USERNAME --body "<github-username>" --repo <owner/repo>
gh secret set GHCR_READ_TOKEN --body "<ghcr-read-token>" --repo <owner/repo>
```

Optional variables:

```bash
gh variable set STAGING_SSH_PORT --body "22" --repo <owner/repo>
gh variable set STAGING_JWT_ISSUER --body "BookQuotesApi" --repo <owner/repo>
gh variable set STAGING_JWT_AUDIENCE --body "BookQuotesClient" --repo <owner/repo>
gh variable set STAGING_JWT_EXPIRES_IN_MINUTES --body "60" --repo <owner/repo>
```

Helper script:

```bash
./scripts/store_staging_secrets_gh.sh \
	<owner/repo> \
	staging.example.com \
	deploy \
	/opt/bookquotes \
	staging.bookquotes.example \
	"Host=<host>;Database=<db>;Username=<user>;Password=<password>" \
	"<generated-jwt-key>" \
	<github-username> \
	"<ghcr-read-token>"
```

## One-time staging host setup

1. Install Docker Engine and Docker Compose plugin on the staging host.
2. Open ports `80` and `443` to the internet.
3. Point DNS for the staging domain to the host.
4. Ensure the SSH user has permission to run Docker.
5. Ensure the host has outbound access to `ghcr.io` and Let's Encrypt.

Ubuntu 24.04 / 22.04 reference commands:

```bash
ssh <user>@<staging-host>
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg
echo \
	"deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
	$(. /etc/os-release && echo \"$VERSION_CODENAME\") stable" | \
	sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo usermod -aG docker $USER
mkdir -p /opt/bookquotes/deploy
docker --version
docker compose version
```

If you want an idempotent helper on the host itself:

```bash
curl -fsSL https://raw.githubusercontent.com/<owner>/<repo>/main/deploy/bootstrap-staging-host.sh -o bootstrap-staging-host.sh
chmod +x bootstrap-staging-host.sh
./bootstrap-staging-host.sh /opt/bookquotes
```

## Staging deploy flow

1. Push changes to `main`.
2. Wait for `CI` to pass and publish `bookquotes-api:staging` and `bookquotes-frontend:staging`.
3. Run the `Deploy Staging` workflow manually from GitHub Actions.
4. The workflow uploads `docker-compose.production.yml` and `deploy/Caddyfile`, writes the host `.env`, pulls images, and starts the stack.

Manual host-side fallback deploy:

```bash
cd /opt/bookquotes
cat > .env <<'EOF'
BOOKQUOTES_DOMAIN=staging.bookquotes.example
POSTGRES_CONNECTION=Host=<host>;Database=<db>;Username=<user>;Password=<password>
JWT_KEY=<generated-jwt-key>
JWT_ISSUER=BookQuotesApi
JWT_AUDIENCE=BookQuotesClient
JWT_EXPIRES_IN_MINUTES=60
API_IMAGE=ghcr.io/<owner>/bookquotes-api:staging
FRONTEND_IMAGE=ghcr.io/<owner>/bookquotes-frontend:staging
EOF
docker login ghcr.io -u <github-username>
docker compose --env-file .env -f docker-compose.production.yml pull
docker compose --env-file .env -f docker-compose.production.yml up -d
docker compose --env-file .env -f docker-compose.production.yml ps
```

## Post-deploy validation

Run these checks against the staging domain:

```bash
curl -I https://<staging-domain>/
curl -I https://<staging-domain>/health
curl -I https://<staging-domain>/health/live
curl -I https://<staging-domain>/api/books
```

Expected results:

- HTTPS responds successfully
- `/health` returns `200`
- `/health/live` returns `200`
- API routes are reachable through the same origin

Manual browser checks:

1. Load the landing page.
2. Register a fresh test user.
3. Log in and verify protected routes load.
4. Create and delete a book.
5. Create and delete a quote.
6. Refresh the page and verify the session persists.
7. Log out and verify protected routes redirect back to login.

## Production promotion checklist

1. Replace staging domain and database secrets with production values.
2. Generate and store a fresh production JWT key.
3. Confirm CORS origins and cookie behavior match the final domain.
4. Confirm DataProtection keys are persisted on durable storage.
5. Run the same post-deploy validation checks.
6. Rotate any temporary staging/test secrets that were reused.

## API versioning decision

- The deployed API currently stays on the existing unversioned route shape under `/api/*`.
- Recommended future approach: adopt path-based versioning at the first breaking API change, starting with `/api/v1/*`.
- Migration rule: keep the Angular client on one version at a time and avoid partial mixed routing unless a deliberate deprecation window is required.
- This is a product and compatibility decision rather than a deployment blocker, so it was evaluated and intentionally not implemented during the current deployment hardening pass.

## Current blockers to actual deployment

- No confirmed staging host is configured in this workspace.
- Required GitHub staging secrets are not populated here.
- Production domain, certificate ownership, and database host are not yet provisioned.

Until those external prerequisites are in place, the repository is prepared for deployment but cannot be fully deployed from this environment alone.
