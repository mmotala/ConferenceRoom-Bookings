# Meeting Room Manager

A full-stack meeting room booking application built with a .NET API and a Vue client. The app supports room discovery, booking creation, booking updates, cancellations, recurring bookings, admin room/user management, validation feedback, and a calendar view.

## Tech Stack

- Backend: .NET 9 Minimal API, EF Core InMemory, FluentValidation, Swagger/OpenAPI
- Frontend: Vue 3, TypeScript, Vite, FullCalendar, Vitest, Cypress
- Architecture: Clean Architecture-inspired separation across API, Application, Domain, Infrastructure, and Client

## Solution Structure

```text
Server/
  ConferenceRoom.API/              Minimal API endpoints, middleware, Swagger
  ConferenceRoom.Application/      Feature handlers, validation, result/error contracts
  ConferenceRoom.Domain/           Entities, enums, domain events
  ConferenceRoom.Infrastructure/   EF Core DbContext, seed data, notification stub
  ConferenceRoom.Tests/            Handler-level business rule tests

Client/
  src/features/bookings/           Booking UI, API wrappers, types, tests
  src/features/rooms/              Room UI, API wrappers, types, tests
  src/features/users/              Dummy login/admin users, store, types, tests
  src/shared/                      Shared API client, components, styles, utilities
  cypress/                         E2E workflow tests
```

## Backend Design

The backend follows a feature-handler style. Endpoints stay thin and delegate business work to Application handlers such as `CreateBooking.Handler`, `UpdateBooking.Handler`, `CancelBooking.Handler`, and `CreateRecurringBooking.Handler`.

Validation is handled with FluentValidation before business logic executes. Application operations return `Result` / `Result<T>` objects with structured `Error` values, which are translated into appropriate HTTP responses and problem-style validation errors.

The API also includes global exception handling middleware, request logging middleware, Swagger/OpenAPI in development, seeded demo data, dummy current-user handling through headers, and JSON string enum support for API-friendly values such as `"Weekly"`.

## Frontend Design

The Vue client is organized by feature rather than by generic file type. Forms provide inline validation for exact field messages, while toast messages give a short summary such as:

```text
Please fix the highlighted fields.
```

The shared API client attaches the selected dummy user as headers:

```text
X-User-Id
X-User-Role
```

This keeps authentication intentionally simple for the assessment while still allowing ownership/admin rules to be exercised.

### Start the API

```bash
cd Server
dotnet restore
dotnet run --project ConferenceRoom.API/ConferenceRoom.API.csproj
```

Default API URLs:

```text
http://localhost:5096
https://localhost:7187
```

Swagger:

```text
http://localhost:5096/swagger
```

### Start the Vue Client

```bash
cd Client
npm install
```

Run the client:

```bash
npm run dev
```

Default client URL:

```text
http://localhost:5173
```

The Vite dev server is configured to use port `5173` and open the browser automatically. The client defaults to the local API at `http://localhost:5096`, so an `.env.local` file is not required for normal local development. If needed, the API URL can still be overridden with `VITE_API_BASE_URL`.

## Running Tests

Backend:

```bash
cd Server
dotnet test
```

Frontend:

```bash
cd Client
npm run type-check
npx vue-tsc --build tsconfig.vitest.json
npm run test:unit -- --run
```

Optional E2E tests:

```bash
npm run test:e2e
```

## CI

GitHub Actions includes separate backend and client workflows.

Client CI runs dependency install, app type-check, unit-test type-check, Vitest unit tests, and a production build. Backend CI runs restore, build, and tests.

## Assumptions and Trade-offs

- Authentication is intentionally implemented as dummy login for assessment scope.
- EF Core InMemory is used for simple local persistence and fast setup.
- A production version should use a relational database such as PostgreSQL. PostgreSQL would allow stronger concurrency guarantees for booking overlap prevention, especially with transactions, isolation levels, constraints, or exclusion constraints.
- SignalR was not implemented. It would be ideal for live UI updates when bookings are created, updated, or cancelled by another user.
- Email notifications are represented by a dummy notification service rather than a real provider.

## Demo Users

Seed data logs the demo users when the API starts:

```text
admin@demo.com
user@demo.com
second@demo.com
```

Use the demo login panel in the client to switch between users.
