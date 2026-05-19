# BookQuotes UI

This is the Angular 21 frontend for the BookQuotes application.

## Frontend Responsibilities

- Show the books landing page
- Show public quotes
- Provide register and login flows
- Protect book management and `My Quotes` routes
- Store the authenticated user locally
- Send JWT tokens with protected API requests
- Support responsive layouts for desktop, tablet, and mobile
- Support light and dark theme switching

## Tech Stack

- Angular 21
- Angular Router
- Angular Forms and Reactive Forms
- Bootstrap 5
- Font Awesome

## Local Run

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm start
```

Open:

```text
http://localhost:4200
```

## Development Proxy

During local development, `/api` requests are proxied to:

```text
https://localhost:7280
```

This is configured in `proxy.conf.json`.

## Main Routes

- `/books`: landing page and books list
- `/books/add`: add book, protected
- `/books/edit/:id`: edit book, protected
- `/quotes`: public quotes list
- `/quotes/add`: add quote, protected
- `/quotes/edit/:id`: edit quote, protected
- `/quotes/my`: personal quotes, protected
- `/login`: login page
- `/register`: register page

## Build

```bash
npm run build
```

## Docker

The repository includes a `Dockerfile` for the frontend and a `docker-compose.yml` to run the full stack (Postgres + API + frontend). To run the full stack:

```bash
docker compose up --build
```

The frontend will be served on `http://localhost:4200` when the compose stack is up.

## Continuous Integration

The project contains a GitHub Actions workflow at `.github/workflows/ci.yml` that builds the frontend and the API and runs EF migrations against a Postgres service. The workflow runs on pushes and pull requests to `main`.

## Test

```bash
npm test
```

## Frontend Notes

- The app uses the books page as the start page.
- Auth state is stored in local storage.
- Protected routes use an Angular route guard.
- The navbar includes a responsive mobile menu and a light/dark theme toggle.
- Duplicate prevention and friendly error messages are implemented in the book and quote forms.

## Deployment Notes

Before frontend deployment, verify:

- The production API URL is configured correctly
- CORS on the API allows the deployed frontend origin
- Authentication works with the production API
- Responsive layouts still behave correctly on mobile
