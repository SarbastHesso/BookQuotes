# BookQuotes

BookQuotes is a responsive CRUD web application built with Angular 21 for the frontend and .NET 9 for the backend API. It lets users browse books and public quotes, register and log in with cookie-backed JWT authentication, manage books, and maintain a personal list of favorite quotes.

## Tech Stack

- Frontend: Angular 21, Bootstrap 5, Font Awesome
- Backend: ASP.NET Core 9 Web API, Entity Framework Core, SQL Server LocalDB
- Authentication: JWT bearer tokens issued by the API and stored in an HTTP-only auth cookie
- Styling: Bootstrap with custom light/dark theme support

## Main Features

- Books landing page at `/books`
- Book CRUD for authenticated users
- Public quotes view
- Protected `My Quotes` view for each user
- Quote add, edit, and delete flows
- Maximum of 5 saved quotes per user
- Duplicate prevention for books and quotes
- JWT-based login and registration
- Responsive navigation with mobile menu
- Light and dark theme toggle

## Project Structure

- `bookquotes-ui/`: Angular frontend
- `BookQuotes.Api/`: ASP.NET Core API
- `BookQuotes.sln`: solution file for the backend project

## Local Development

### Prerequisites

- Node.js 20+
- npm 11+
- .NET SDK 9
- SQL Server LocalDB on Windows

### Local Test Setup

Before starting the app, create a local API development config:

1. Copy `BookQuotes.Api/appsettings.Development.example.json` to `BookQuotes.Api/appsettings.Development.json`
2. Keep the LocalDB connection string or replace it with your own local SQL Server connection string
3. Replace `Jwt:Key` with a long random development-only secret
4. Do not commit `BookQuotes.Api/appsettings.Development.json`

Then prepare and run the project:

1. From `BookQuotes.Api/`, run:

```bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

Cleanup: A helper script is available to remove local build artifacts and developer files before committing. Run from the repo root (PowerShell):

```powershell
pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass -File ./scripts/cleanup_repo.ps1
```

2. From `bookquotes-ui/`, run:

```bash
npm install
npm start
```

3. Open `http://localhost:4200`

### 1. Start the API

From `BookQuotes.Api/`:

```bash
dotnet restore
dotnet build
dotnet run
```

Default local URLs from launch settings:

- `https://localhost:7280`
- `http://localhost:5268`

The Angular app is configured to proxy `/api` to `https://localhost:7280` in development.

### 2. Start the frontend

From `bookquotes-ui/`:

```bash
npm install
npm start
```

Frontend development URL:

- `http://localhost:4200`

## Validation Commands

These are the main repo checks currently used:

### Backend

```bash
dotnet build BookQuotes.sln
```

## Cross-platform development (SQLite) and Docker

If you want a quick cross-platform local setup without installing SQL Server LocalDB, run the API using SQLite (already configured for Development):

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project BookQuotes.Api --startup-project BookQuotes.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run --project BookQuotes.Api
```

For production-like parity and easy onboarding, you can use Docker Compose which brings up Postgres + API + frontend:

```bash
docker compose up --build
```

API: http://localhost:5268
Frontend: http://localhost:4200

The `docker-compose.yml` uses Postgres and the API is configured to run migrations on startup.

## Continuous Integration

A GitHub Actions workflow is included in `.github/workflows/ci.yml`. It builds the backend, applies EF migrations against a Postgres service, runs tests, builds the frontend, and on pushes to `main` publishes staging container images to GHCR for the deploy workflow.

### Frontend

```bash
cd bookquotes-ui
npm run build
```

## Authentication Flow

- Users can register with a username and password.
- Users can log in and receive a JWT token from the API.
- The frontend stores the logged-in user in local storage.
- The auth interceptor sends the bearer token with protected API requests.
- Protected frontend routes use an Angular auth guard.

## Notes

- The app currently targets Angular 21 instead of Angular 20.
- The books page is used as the landing page and start page.
- The default development connection string uses SQL Server LocalDB.
- Local testing uses `BookQuotes.Api/appsettings.Development.json`, while deployment should use production-safe settings in `BookQuotes.Api/appsettings.json` examples plus environment variables or a secret store.

## Next Submission Steps

- Finish a full end-to-end requirement test pass
- Provision the staging host, domain, and secrets described in `DEPLOY_RUNBOOK.md`
- Run the `CI` workflow on `main` and then trigger `Deploy Staging`
- Validate `/health`, `/health/live`, login, CRUD, and logout on the deployed domain
- Submit the live link and GitHub repository links

## More Documentation

- Frontend details: `bookquotes-ui/README.md`
- API details: `BookQuotes.Api/README.md`
ci: trigger build
