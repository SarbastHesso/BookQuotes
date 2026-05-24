# BookQuotes Deployment Update Runbook

This file describes the exact steps to follow each time you change code and want the deployed version updated.

Important:

- This file documents the current deployment paths and operational steps. It may contain host-specific notes and examples; it MUST NOT contain plaintext secrets (database passwords, private keys, tokens).

- The examples include guidance to set secrets with `gh` and `az`. Prefer storing secrets in a secure store (GitHub repository secrets, GitHub environment secrets, or Azure Key Vault) rather than embedding them in files checked into source control.

- The runbook can be committed to the repository for convenience, but any environment-specific secret values should be omitted or redacted.

## Current deployment model

The project currently deploys through two separate paths.

### Backend path

1. Push backend code to `main`.
2. GitHub Actions `CI` on `main` builds and pushes:
   - `ghcr.io/sarbasthesso/bookquotes-api:staging`
3. Azure App Service pulls that `:staging` image.
4. Because the App Service is using a container image tag, you should restart the App Service after the new backend image is available.

### Frontend path

1. Push frontend code to `main`.
2. GitHub Actions `Azure Static Web Apps CI/CD` on `main` deploys the Angular app.

## Quick decision guide

### If you changed only backend code

Follow the backend-only steps.

### If you changed only frontend code

Follow the frontend-only steps.

### If you changed both backend and frontend

You must update both branches and then verify both deployments.

## Backend-only update steps

Use this when you changed files in the API, database, auth, controllers, services, backend config, or anything that affects the backend container.

1. Make your code changes locally.
2. Test locally if possible.
3. Commit the backend change locally.
4. Push the backend change to `main`.
5. Open GitHub Actions.
6. Wait for `CI` on `main` to finish successfully.
7. Open Azure Portal.
8. Open App Service: `bookquotes-webapp-staging-sarb`.
9. Click `Restart`.
10. Wait about 1 minute.
11. Check health:

```text
https://bookquotes-webapp-staging-sarb-h5cydwd3fwcqfjdx.westeurope-01.azurewebsites.net/health
```

12. Confirm it returns:

```json
{"status":"Healthy"}
```

## Frontend-only update steps

Use this when you changed Angular pages, components, styles, routing, or frontend-only behavior.

1. Make your frontend changes locally.
2. Test locally if possible.
3. Commit the frontend change locally.
4. Push the frontend change to `main`.
5. Open GitHub Actions.
6. Wait for `Azure Static Web Apps CI/CD` on `master` to finish successfully.
7. Open the Azure Static Web App site.
8. Hard refresh the browser.
9. Confirm the frontend change is visible.

## Backend + frontend update steps

Use this when one change touches both the backend and the frontend.

1. Make your code changes locally.
2. Test locally if possible.
3. Commit your changes locally.
4. Push the change to `main` first.
5. Wait for `CI` on `main` to pass.
6. Restart the App Service in Azure Portal.
7. Confirm `/health` works.
8. Push the frontend part to `main` as well.
9. Wait for `Azure Static Web Apps CI/CD` on `main` to pass.
10. Open the deployed frontend site and retest the affected feature.

## How to decide what goes to which branch

### Push to `main` when:

- You changed backend code.
- You changed Docker-related backend deployment behavior.
- You changed auth behavior that affects the API.
- You changed database or API configuration.

### Push to `main` when (frontend):

- You changed frontend code that must appear on the Azure Static Web App.
- You changed Angular environment behavior used by the deployed frontend.

### If one fix affects both sides

Do both:

- push to `main` for the backend image
- push to `master` for the frontend deployment

## Recommended simple workflow each time

If you want a practical routine without overthinking branches, use this:

### For backend work

1. Finish the change.
2. Push to `main`.
3. Wait for `CI` on `main` to pass.
4. Restart App Service.
5. Check `/health`.

### For frontend work

1. Finish the change.
2. Push to `main`.
3. Wait for `Azure Static Web Apps CI/CD` to pass.
4. Open the site and verify.

### For full-stack work

1. Push backend changes to `main`.
2. Wait for `CI` on `main` to pass.
3. Restart App Service.
4. Verify backend health.
5. Push frontend changes to `main`.
6. Wait for `Azure Static Web Apps CI/CD` to pass.
7. Verify the full feature in the browser.

## What to check after every deployment

Always verify these:

1. Backend health page works.
2. Frontend site opens.
3. Register works.
4. Login works.
5. Refresh does not log you out unexpectedly.
6. Protected pages load.
7. Books and quotes actions still work.

## URLs to keep handy

### Backend health

```text
https://bookquotes-webapp-staging-sarb-h5cydwd3fwcqfjdx.westeurope-01.azurewebsites.net/health
```

### Frontend site

```text
https://lemon-grass-069153203.7.azurestaticapps.net
```

## If backend deployment looks stuck

Check these in order:

1. `CI` on `main` passed.
2. Azure App Service image is still:

```text
ghcr.io/sarbasthesso/bookquotes-api:staging
```

3. App Service was restarted after the image update.
4. `/health` responds successfully.
5. CORS app setting is still:

```text
Cors__AllowedOrigins=https://lemon-grass-069153203.7.azurestaticapps.net
```

## If frontend deployment looks stuck

Check these in order:

1. `Azure Static Web Apps CI/CD` on `main` passed.
2. You opened the correct Azure Static Web App URL.
3. You did a hard refresh.
4. The code really was pushed to `main`.

## Current limitation to remember

The current setup is split:

- backend deployment follows `main`
- frontend deployment follows `main`

That means full-stack changes are a two-part deployment until you later simplify the branch strategy.