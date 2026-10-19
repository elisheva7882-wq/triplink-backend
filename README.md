# TripLink

TripLink is a full-stack ride-sharing project built as a .NET graduation project. Drivers can publish rides, passengers can search and register for available rides, ride owners can manage their published rides and passengers, and administrators have dedicated management permissions.

The main focus of the project is the **ASP.NET Core Web API**, including clean layered architecture, EF Core Code First, JWT authentication, role-based authorization, validation, logging, automated tests, and safe handling of concurrent registration for a limited number of seats.

## Live Demo

- Frontend: https://triplink-frontend.vercel.app
- Backend API: https://triplink-api.onrender.com

> The API does not expose a page at `/`, so a `404` on the backend root URL is expected. Use the API routes below. Swagger is enabled in the Development environment.

## Repositories

- Backend: https://github.com/elisheva7882-wq/triplink-backend
- Frontend: https://github.com/elisheva7882-wq/triplink-frontend

## Main Features

- User registration and login with JWT authentication
- `User` and `Admin` roles with different permissions
- Publish, view, update, and delete rides
- Search rides by origin, destination, date, sorting, and pagination
- View the rides published by the currently logged-in user
- Register for a ride and cancel a registration
- Prevent duplicate registrations
- Prevent registration after departure
- Prevent overbooking when several users compete for the last available seat
- Ride owners can view registered passengers and remove a passenger from their ride
- Admin endpoints for viewing users/rides and managing rides
- Global error handling with consistent JSON responses
- Correlation ID support through `X-Correlation-ID`
- NLog-based application logging
- Unit and integration tests, including real optimistic-concurrency tests
- React frontend with local Israeli locality data and no runtime dependency on an external localities API

## Architecture

The backend follows a layered architecture:

```text
TripLink.Core
    ↑
    ├── TripLink.Data
    └── TripLink.Service
             ↑
          TripLink.API
```

Main projects:

```text
TripLink.Core/               Domain entities, DTOs, interfaces, enums and domain exceptions
TripLink.Data/               EF Core DbContext, entity configuration, repositories, migrations and persistence
TripLink.Service/            Business logic, authentication/security services and AutoMapper profiles
TripLink/                    ASP.NET Core Web API (TripLink.API.csproj)
TripLink.Tests/              Unit tests
TripLink.IntegrationTests/   Integration and concurrency tests
```

`Core` is independent of EF Core and HTTP. Controllers call service interfaces rather than accessing `DbContext` directly. Database access is performed through repositories and the Unit of Work abstraction.

## Database Model

The main entities are:

- `User` — account information, role, owned rides and ride registrations
- `Ride` — owner, origin, destination, departure time, capacity and booked seats
- `RideRegistration` — explicit many-to-many link between a passenger and a ride

Relationships include:

- One `User` can own many `Ride` records
- Many users can register for many rides through `RideRegistration`
- A composite key/unique relationship prevents the same passenger from registering twice for the same ride

The project uses **PostgreSQL** with **EF Core Code First**.

## Concurrency and the Limited Resource

The limited resource in TripLink is the number of available seats in a ride.

A normal check such as `BookedSeats < Capacity` is not enough by itself because two requests can read the same value at almost the same time. TripLink therefore combines several protections:

1. Registration runs inside a database transaction.
2. The ride is loaded as a tracked entity.
3. The service verifies that the ride exists, has not departed, has free capacity, and that the passenger is not already registered.
4. A `RideRegistration` is added and `Ride.BookedSeats` is updated in the same unit of work.
5. PostgreSQL optimistic concurrency is implemented using the row version mapped to PostgreSQL `xmin`.
6. If another request updated the same ride first, EF Core throws `DbUpdateConcurrencyException`.
7. The data layer converts the concurrency failure to `DomainConflictException`.
8. The API converts the conflict to HTTP **409 Conflict**.
9. The transaction is rolled back if the operation fails, so a partial registration is not left in the database.

This guarantees that when two users compete for one remaining seat, only one registration can succeed.

## Technology Stack

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 16
- AutoMapper
- JWT Bearer Authentication
- NLog
- xUnit
- Moq

### Frontend

- React
- Vite
- JavaScript
- Local Israeli locality dataset/package

### Cloud Deployment

- Frontend: Vercel
- Backend: Render
- Database: Neon PostgreSQL

## API Overview

Important routes include:

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Create a user account |
| `POST` | `/api/auth/login` | Login and receive a JWT |
| `GET` | `/api/rides` | Search and paginate rides |
| `GET` | `/api/rides/{id}` | Get ride details |
| `GET` | `/api/rides/mine` | Get rides published by the current user |
| `POST` | `/api/rides` | Publish a ride |
| `PUT` | `/api/rides/{id}` | Update an owned ride |
| `DELETE` | `/api/rides/{id}` | Delete an owned ride |
| `POST` | `/api/rides/{rideId}/registrations` | Register for a ride |
| `DELETE` | `/api/rides/{rideId}/registrations/me` | Cancel the current user's registration |
| `GET` | `/api/registrations/me` | Get the current user's registrations |
| `GET` | `/api/rides/{rideId}/registrations` | Ride owner: view registered passengers |
| `DELETE` | `/api/rides/{rideId}/registrations/{passengerId}` | Ride owner: remove a passenger |
| `GET` | `/api/admin/rides` | Admin: view rides |
| `GET` | `/api/admin/users` | Admin: view users |

The full API can be explored through Swagger while running the API in the Development environment.

## HTTP Status Codes

The API intentionally uses REST-appropriate status codes, including:

- `200 OK`
- `201 Created`
- `204 No Content`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`

Errors are returned in a consistent JSON format and include a correlation ID when applicable.

## Local Requirements

Before running the backend locally, install:

- Visual Studio 2022 or the .NET 8 SDK
- PostgreSQL 16 (or another compatible PostgreSQL 16 environment)
- Git

For the React client, Node.js/npm is also required.

## Local Backend Setup

Clone the backend repository:

```bash
git clone https://github.com/elisheva7882-wq/triplink-backend.git
cd triplink-backend
```

Restore and build:

```bash
dotnet restore
dotnet build
```

### Configure User Secrets

Secrets are intentionally not stored in Git.

From the repository root, configure the API project:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=triplink;Username=postgres;Password=YOUR_PASSWORD" --project TripLink/TripLink.API.csproj

dotnet user-secrets set "Jwt:Key" "YOUR_BASE64_JWT_KEY" --project TripLink/TripLink.API.csproj
dotnet user-secrets set "Jwt:Issuer" "TripLink.API" --project TripLink/TripLink.API.csproj
dotnet user-secrets set "Jwt:Audience" "TripLink.Client" --project TripLink/TripLink.API.csproj

dotnet user-secrets set "Demo:Password" "YOUR_DEMO_PASSWORD" --project TripLink/TripLink.API.csproj
```

Do not commit passwords, JWT keys, access tokens, production connection strings, `.env` files, or other secrets.

### Database and Migrations

TripLink uses EF Core migrations. On application startup the API calls `Database.MigrateAsync()`, so pending migrations are applied automatically to the configured database.

The schema is managed by EF Core; there is no need to create or edit tables manually.

### Run the API

```bash
dotnet run --project TripLink/TripLink.API.csproj
```

When running in the Development environment, open Swagger using the local URL displayed by ASP.NET Core, followed by `/swagger`.

## Demo Users

Development seed data includes two demo roles:

| Role | Email | Password |
| --- | --- | --- |
| User | `driver@triplink.example` | The value configured in the `Demo:Password` user secret |
| Admin | `admin@triplink.example` | The value configured in the `Demo:Password` user secret |

Demo users are seeded only in the **Development** environment. A password is not hard-coded in the repository.

## Authentication

After login, the API returns a JWT. Protected routes expect:

```http
Authorization: Bearer <token>
```

The token contains the user identifier and role claims used by the API for authentication and authorization.

Authorization is enforced on the server. The React UI is not relied on as a security boundary.

## Validation and Error Handling

TripLink uses multiple validation layers:

- Request/model validation for HTTP input
- Business validation in the Service layer
- Database constraints for persistence-level integrity
- Optimistic concurrency handling for competing updates

Custom middleware maps known exceptions to HTTP responses and prevents internal stack traces from being exposed to clients.

## Logging and Correlation IDs

The API uses NLog and a custom correlation ID middleware.

Requests use the header:

```text
X-Correlation-ID
```

If the client does not provide one, the API creates one. The value is included in the logging scope so messages belonging to the same request can be traced together.

Sensitive information such as passwords and JWT tokens must not be written to logs.

## Tests

Run all tests from the repository root:

```bash
dotnet test
```

The test suite includes service-level unit tests and PostgreSQL integration tests.

Important concurrency scenarios include:

- Two separate `DbContext` instances load the same ride; after the first saves an update, the second save fails with `DbUpdateConcurrencyException`.
- Two different passengers concurrently compete for the final seat in a ride; only one registration succeeds.
- Duplicate registration is rejected.
- Failed transactional operations do not leave inconsistent `RideRegistration` or `BookedSeats` state.

## Frontend

The React client is maintained in a separate repository:

https://github.com/elisheva7882-wq/triplink-frontend

For local development, configure its environment variable to point to the local API:

```env
VITE_API_BASE_URL=https://localhost:<API_PORT>
```

The production frontend uses:

```text
https://triplink-frontend.vercel.app
```

The API CORS policy allows the local Vite frontend and the production Vercel frontend.

## Production Configuration

The deployed backend uses environment variables rather than committed secrets. Relevant configuration keys include:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=...
Jwt__Key=...
Jwt__Issuer=TripLink.API
Jwt__Audience=TripLink.Client
```

The production database is hosted on Neon PostgreSQL. The backend is deployed on Render and the frontend is deployed on Vercel.

## Security Notes

- Passwords are stored as password hashes, never as plain text.
- `PasswordHash` is never exposed in API response DTOs.
- JWT secrets and database credentials are kept outside source control.
- Authorization is enforced by the API.
- Admin operations require the `Admin` role.
- Ownership checks prevent one regular user from modifying another user's ride.
- `.gitignore` excludes build artifacts, logs, local environment files and secret files.

## Project Completion Checklist

The project demonstrates the required course topics:

- Layered ASP.NET Core Web API architecture
- REST endpoints and Swagger
- Dependency Injection
- EF Core Code First and migrations
- One-to-many and many-to-many relationships
- LINQ filtering, sorting and database-side pagination
- DTOs and AutoMapper
- Async operations with `CancellationToken`
- JWT authentication and two roles
- Global error handling middleware
- Correlation IDs and NLog
- Unit tests with xUnit/Moq
- Integration tests against PostgreSQL
- Optimistic concurrency with a real concurrency token
- HTTP `409 Conflict` for resource contention
- Separate backend and React GitHub repositories
- React client demonstrating the API end to end

## Author

TripLink was developed as a .NET Web API graduation project.
