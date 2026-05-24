# Tester Quick Start

This file explains the minimal steps a tester needs to run BookQuotes locally.

## Prerequisites
- .NET 9 SDK
- Node.js 20+ and npm
- SQL Server LocalDB (Windows) or Docker (optional)
- Git

## 1 — Clone the repo

```bash
git clone <repo-url>
cd BookQuotes
```

## 2 — Configuration (required)

 - Create a local development config by copying the example and filling values, or request a sanitized config from the project owner via a secure channel.

  - Copy the example:

    - `cp BookQuotes.Api/appsettings.Development.example.json BookQuotes.Api/appsettings.Development.json`

  - Edit `BookQuotes.Api/appsettings.Development.json` and set the database provider, connection string, and `Jwt:Key` as needed.

  - Important: do NOT commit `BookQuotes.Api/appsettings.Development.json`. It may contain development secrets.

- Confirm the DB provider in the file:
  - On Windows: `"Database": { "Provider": "SqlServer" }`
  - On macOS/Linux: `"Database": { "Provider": "Sqlite" }` (optional)

## 3 — Start the backend (API)

Open a terminal in `BookQuotes.Api` and run the app. In Development the API applies migrations on startup.

Recommended flow (Windows):

1. `dotnet restore`
2. `dotnet build`
3. `dotnet run`

Verify readiness: open `http://localhost:5268/health` — expected: HTTP `200 OK`
Verify liveness: open `http://localhost:5268/health/live` — expected: HTTP `200 OK`

## 4 — Start the frontend (UI)

Open a new terminal in `bookquotes-ui`:

1. `npm ci` (only once or when dependencies change)
2. `npm start`

Open `http://localhost:4200`

### Mobile testing (recommended)

If you're testing from a mobile device on the same network, cookies with `SameSite=None` require HTTPS to be accepted by modern browsers. Use an HTTPS tunnel to expose your local API and frontend, for example with `ngrok`:

```bash
# Install ngrok and run an HTTPS tunnel to the API port (example: 7280)
ngrok http 7280

# Optionally run a tunnel for the frontend port 4200 as well
ngrok http 4200
```

Update the frontend `apiBaseUrl` to use the ngrok host (the `https://...` URL) before testing on mobile so the auth cookie is set with `Secure` and `SameSite=None` and will persist across refreshes.

## 5 — Smoke tests for the tester

- Load landing page (books list).
- Register a user, login, and confirm protected routes (`/quotes/my`, `/books/add`) work.
- Create a book and a quote; refresh and confirm persistence.

## Safer alternatives

- Instead of sending a config file, the tester can set environment variables according to `BookQuotes.Api/.env.example`.
- Or run the full stack with Docker: `docker compose up --build` (uses Postgres + API + frontend).
- For hosted staging or production review, use the deployment notes in `README.md` and `DEPLOY_RUNBOOK.md`.

## Security note

- Never commit `appsettings.Development.json` to the repo. If you must share a real key, use a secure channel and provide a short-lived test key.

---
If you want I can also add a short email template for the tester to include with the file. 
