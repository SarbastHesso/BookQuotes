# BookQuotes

BookQuotes is a responsive CRUD web application built with Angular 21 for the frontend and .NET 9 for the backend API. It lets users browse books and public quotes, register and log in with JWT authentication, manage books, and maintain a personal list of favorite quotes.

## Tech Stack

- Frontend: Angular 21, Bootstrap 5, Font Awesome
- Backend: ASP.NET Core 9 Web API, Entity Framework Core, SQL Server LocalDB
- Authentication: JWT bearer tokens
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

Then prepare and run the project:

1. From `BookQuotes.Api/`, run:

```bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run
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

A GitHub Actions workflow is included in `.github/workflows/ci.yml`. It builds the backend, applies EF migrations against a Postgres service, and builds the frontend. The workflow runs on pushes and pull requests to `main`.

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
- Local testing uses `BookQuotes.Api/appsettings.Development.json`, while deployment should use production-safe settings in `BookQuotes.Api/appsettings.json` or environment variables.

## Secrets & environment (best practices)

- **Do not commit secrets.** Keep any secrets out of source control (JWT keys, production DB connection strings, certificates). Use environment variables or a secret store (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) in production.
- **Local development:** copy `BookQuotes.Api/appsettings.Development.example.json` to `BookQuotes.Api/appsettings.Development.json` and keep secrets there only for local use. `appsettings.Development.json` is ignored by `.gitignore`.
- **Use `.env.example`.** Add an `.env.example` showing required environment variables without values (see `BookQuotes.Api/.env.example`). Do not commit a populated `.env` file.
- **DataProtection keys:** Persist ASP.NET Core DataProtection keys in production (file share, Redis, or Key Vault). For Docker Compose add a mounted volume for the key ring and configure `DataProtection` to use that folder.
- **Windows vs cross-platform env vars:** On Windows use PowerShell to set env for a session: `setx ASPNETCORE_ENVIRONMENT Development` or `$env:ASPNETCORE_ENVIRONMENT = 'Development'`. Cross-platform examples are in the API README.
- **Migrations:** Prefer running `dotnet ef database update --project BookQuotes.Api --startup-project BookQuotes.Api` so the correct startup configuration is used.

## Next Submission Steps

- Finish a full end-to-end requirement test pass
- Deploy the frontend to a free hosting service
- Deploy the API to a free hosting service
- Update production API URLs and CORS/proxy settings
- Submit the live link and GitHub repository links

## More Documentation

- Frontend details: `bookquotes-ui/README.md`
- API details: `BookQuotes.Api/README.md`
