# BookQuotes API

This is the ASP.NET Core 9 Web API for the BookQuotes application.

## Responsibilities

- Register and authenticate users
- Issue JWT tokens after successful login
- Validate bearer tokens for protected endpoints
- Provide CRUD endpoints for books
- Provide CRUD endpoints for user-owned quotes
- Enforce quote and book business rules

## Tech Stack

- ASP.NET Core 9 Web API
- Entity Framework Core 9
- SQL Server / LocalDB
- JWT bearer authentication

## Local Run

Before running the API locally:

1. Copy `appsettings.Development.example.json` to `appsettings.Development.json`
2. Keep the LocalDB connection string or replace it with your own local SQL Server connection string
3. Replace `Jwt:Key` with a long random development-only secret

From this folder:

```bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

### Cross-platform quick start (SQLite)

The API supports SQLite for quick cross-platform development. To use SQLite locally (Development environment):

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project BookQuotes.Api --startup-project BookQuotes.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

The app will create `bookquotes.db` in the API folder when migrations run.

Note: If you're using the SQLite provider for local development, prefer running the app with `dotnet run` (Development environment). `Program.cs` uses `EnsureCreated()` for SQLite which creates the database schema on first run. Running `dotnet ef database update` against an existing `bookquotes.db` that was created by a previous run may cause "table already exists" errors if migrations and the existing schema are out of sync.

If you run into migration conflicts with SQLite, remove the local DB and re-create it using the helper scripts in `BookQuotes.Api/scripts/` (examples below).

### Docker (Postgres) dev

You can run a development stack using Docker Compose (Postgres + API + frontend). From the repo root:

```bash
docker compose up --build
```

The API container applies EF migrations on startup and is configured to use Postgres in the compose file.

Default development URLs:

- `https://localhost:7280`
- `http://localhost:5268`

## Database

For local development, the connection string in `appsettings.Development.json` uses:

```text
Server=(localdb)\MSSQLLocalDB;Database=BookQuotesDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

The project already contains Entity Framework migrations.

If you need to apply them manually, use:

```bash
dotnet ef database update
```

## JWT Configuration

For local development, the JWT key is stored in `appsettings.Development.json`.

Production JWT settings are configured in `appsettings.json`:

- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiresInMinutes`

Replace these values with production-safe secrets before deployment.

## Main API Endpoints

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

### Books

- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books` protected
- `PUT /api/books/{id}` protected
- `DELETE /api/books/{id}` protected

### Quotes

- `GET /api/quotes`
- `GET /api/quotes/{id}`
- `GET /api/quotes/my` protected
- `POST /api/quotes` protected
- `PUT /api/quotes/{id}` protected
- `DELETE /api/quotes/{id}` protected

## Business Rules

- Usernames must be unique
- Books are unique by title + author
- Quote text is trimmed before save
- Each user can save at most 5 quotes
- A user cannot save duplicate quote text
- Quote edit and delete operations are restricted to the owner

## Development Notes

- CORS currently allows `http://localhost:4200`
- OpenAPI is enabled in development
- JSON responses use camelCase naming

## Deployment Notes

Before deploying the API:

- Move secrets out of `appsettings.json`
- Replace the LocalDB connection string with a production database
- Update CORS for the deployed frontend origin
- Verify HTTPS-only JWT and API access

## Secrets & environment (best practices)

- **Environment variables:** Use environment variables for production secrets. For nested configuration use double-underscore naming (for example `ConnectionStrings__DefaultConnection`).
- **Local development:** Keep local secrets in `appsettings.Development.json` only and do not commit it. Use `appsettings.Development.example.json` as the tracked example.
- **.env.example:** A small `.env.example` is provided (`BookQuotes.Api/.env.example`) showing the common env keys for local cross-platform runs.
- **DataProtection keys:** Persist DataProtection keys in production (use an external key store, Redis, or a mounted volume). Do not rely on ephemeral container storage for key rings.
- **Certificates:** Keep certificates and `.pfx` files out of the repo; use secret stores or CI/CD protected variables to deploy them.

### Quick cross-platform env examples

Linux / macOS (bash):

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Data Source=bookquotes.db"
export Jwt__Key="ReplaceWithDevSecret"
dotnet ef database update --project BookQuotes.Api --startup-project BookQuotes.Api
dotnet run --project BookQuotes.Api
```

Windows PowerShell (session):

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\\MSSQLLocalDB;Database=BookQuotesDb;Trusted_Connection=True;'
dotnet ef database update --project BookQuotes.Api --startup-project BookQuotes.Api
dotnet run --project BookQuotes.Api
```
