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

From this folder:

```bash
dotnet restore
dotnet build
dotnet run
```

Default development URLs:

- `https://localhost:7280`
- `http://localhost:5268`

## Database

The default connection string in `appsettings.json` uses:

```text
Server=(localdb)\MSSQLLocalDB;Database=BookQuotesDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

The project already contains Entity Framework migrations.

If you need to apply them manually, use:

```bash
dotnet ef database update
```

## JWT Configuration

JWT settings are stored in `appsettings.json`:

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