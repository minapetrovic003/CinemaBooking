# CinemaBooking

A full-stack web application for managing cinema bookings. The backend is built with ASP.NET Core 10 following Clean Architecture principles, and the frontend is a React 19 single-page application.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Technology Stack](#technology-stack)
- [Domain Models](#domain-models)
- [Authentication and Authorization](#authentication-and-authorization)
- [API Endpoints](#api-endpoints)
- [Seat Locking System](#seat-locking-system)
- [Payment Flow](#payment-flow)
- [Booking Lifecycle](#booking-lifecycle)
- [Middleware](#middleware)
- [Getting Started](#getting-started)
- [Configuration](#configuration)

---

## Architecture Overview

The backend follows Clean Architecture, divided into four layers:

- **Domain** — core models, enums, and DTOs with no external dependencies
- **Application** — business logic via CQRS handlers, repository interfaces, and FluentValidation validators
- **Infrastructure** — Entity Framework Core implementations, ASP.NET Core Identity, and repository concretions
- **API** — controllers, middleware, background services, and dependency injection setup

Communication between layers flows inward: the API layer depends on Application, Application depends on Domain, and Infrastructure implements the interfaces defined in Application.

---

## Project Structure

```
CinemaBooking.Domain
  Models/             Core entities (Booking, Movie, Hall, Showtime, Seat, SeatLock, Payment)
  DTOs/               Request and response objects grouped by feature
  
CinemaBooking.Application
  CQRS/               Commands and queries organized by feature
    Bookings/
    Movies/
    Halls/
    Showtimes/
    SeatLocks/
    Payments/
  Repositories/       Repository and UnitOfWork interfaces
  Behaviors/          MediatR pipeline behaviors (validation)

CinemaBooking.Infrastructure
  Identity/           ApplicationUser and UserRepository
  Repositories/       EF Core repository implementations
  CinemaBookingContext.cs

CinemaBooking.API
  Controllers/        REST endpoints
  Middlewares/        Exception handling, idempotency, request logging
  Services/           JwtTokenService
  BackgroundServices/ SeatLockCleanupService
  Extensions/         IdentitySeedExtensions
  Autentification/    JwtOptions

CinemaBookingFrontend
  CinemaFrontendApp/  React 19 + Vite frontend
```

---

## Technology Stack

**Backend**
- .NET 10, ASP.NET Core 10
- Entity Framework Core with SQL Server
- ASP.NET Core Identity
- MediatR (CQRS)
- FluentValidation
- JWT Bearer authentication (Microsoft.IdentityModel)
- Scalar (OpenAPI documentation)

**Frontend**
- React 19
- Vite 8

---

## Domain Models

### Booking

A booking links a user to a showtime and a set of seats. It progresses through statuses managed by domain methods:

| Method | Transition | Condition |
|---|---|---|
| `Confirm()` | Pending -> Confirmed | Status must be Pending |
| `Cancel()` | Pending -> Canceled | Status must be Pending |
| `CancelAfterRefund()` | Confirmed/CheckedIn -> Canceled | Status must be Confirmed or CheckedIn |
| `CheckIn()` | Confirmed -> CheckedIn | Status must be Confirmed |

BookingStatus values: `Pending`, `Confirmed`, `Canceled`, `Expired`, `CheckedIn`

### Showtime

Each showtime belongs to a movie and a hall. It carries a `RowVersion` byte array annotated with `[Timestamp]`, enabling optimistic concurrency control. When two users attempt to book the same seat simultaneously, EF Core throws `DbUpdateConcurrencyException` if the row was updated between the read and the write, guaranteeing that only one booking succeeds.

### SeatLock

A temporary reservation of a seat for a specific user and showtime. Locks expire after a configurable number of minutes (default 10, maximum 15). The model exposes `IsActive()` to check expiry against `DateTime.UtcNow` and `OwnedBy(userId)` to verify ownership.

### Payment

PaymentStatus values: `Pending`, `Completed`, `Failed`, `Refunded`

PaymentMethod values are stored as an enum on the Payment entity.

---

## Authentication and Authorization

The application uses ASP.NET Core Identity for user management and JWT Bearer tokens for stateless authentication.

### User Model

`ApplicationUser` extends `IdentityUser` with `FirstName` and `LastName` fields. The username is always set equal to the email address.

### Password Policy

- Minimum length: 6 characters
- Must contain: uppercase letter, lowercase letter, digit, non-alphanumeric character
- Minimum unique characters: 2
- Email must be unique per user

### Roles

Two roles exist in the system: `Admin` and `User`. On application startup, both roles are seeded automatically. A default admin account (`admin@cinema.com` / `Admin!123`) is also created if it does not already exist.

### Registration — POST /auth/register

Accepts `FirstName`, `LastName`, `Email`, `Password`, and an optional `Role` (defaults to `"User"`). Only `"Admin"` and `"User"` are accepted as valid roles; any other value silently falls back to `"User"`. Returns 409 if the email is already registered. Does not return a token — the user must log in separately.

### Login — POST /auth/login

Accepts `Email` and `Password`. Returns a `LoginResult` containing the JWT token string and its UTC expiry time. Both "email not found" and "wrong password" cases return 401 with the same generic message to prevent user enumeration.

### JWT Token Contents

The token is signed with HMAC-SHA256 using a symmetric key from configuration. It contains the following claims:

| Claim | Value |
|---|---|
| `NameIdentifier` | User GUID |
| `Email` | User email |
| `Name` | User email |
| `FullName` | First and last name |
| `Jti` | Unique token identifier (new GUID per token) |
| `Role` | One or more role names |

### Token Validation

Every incoming request passes through the JWT Bearer middleware, which validates the issuer signing key, the issuer, and the audience. If validation passes, `HttpContext.User` is populated with the claims. Token lifetime defaults to 60 minutes and is configurable via `appsettings.json`.

### Endpoint Protection

- `[Authorize]` — requires a valid token
- `[AllowAnonymous]` — accessible without a token
- `[Authorize(Roles = "Admin")]` — restricted to Admin role

The check-in endpoint (`PATCH /bookings/{id}/checkin`) is `[AllowAnonymous]` by design, because users access it by scanning a QR code sent to their email and are not expected to be logged in at that point.

---

## API Endpoints

### Auth

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | /auth/register | Anonymous | Register a new user |
| POST | /auth/login | Anonymous | Log in and receive a JWT token |

### Bookings

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | /bookings | Required | List bookings with optional filters |
| GET | /bookings/{id} | Required | Get a single booking by ID |
| GET | /bookings/{id}/verify | Anonymous | Public booking verification (QR code use case) |
| POST | /bookings | Required | Create a new booking |
| PATCH | /bookings/{id}/cancel | Required | Cancel a pending booking |
| PATCH | /bookings/{id}/checkin | Anonymous | Check in a confirmed booking |

### Seat Locks

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | /seat-locks/lock | Required | Lock seats for the current user |
| DELETE | /seat-locks/release | Required | Release the current user's locks |
| GET | /seat-locks/availability/{showtimeId} | Anonymous | Get seat availability map |

### Movies

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | /movies | Anonymous | List movies with optional filters |
| GET | /movies/{id} | Anonymous | Get movie by ID |
| POST | /movies | Admin | Create a movie |
| PUT | /movies/{id} | Admin | Update a movie |
| DELETE | /movies/{id} | Admin | Delete a movie |

### Halls

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | /halls | Anonymous | List halls |
| GET | /halls/{id} | Anonymous | Get hall by ID |
| POST | /halls | Admin | Create a hall |
| DELETE | /halls/{id} | Admin | Delete a hall |
| PATCH | /halls/{id}/seat-type | Admin | Update seat type for a seat |

### Showtimes

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | /showtimes | Anonymous | List showtimes |
| POST | /showtimes | Admin | Create a showtime |
| DELETE | /showtimes/{id} | Admin | Delete a showtime |

### Payments

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | /payments | Required | List payments |
| GET | /payments/{id} | Required | Get payment by ID |
| POST | /payments | Required | Create a payment |
| POST | /payments/{id}/refund | Required | Refund a payment |

---

## Seat Locking System

The seat locking system prevents two users from booking the same seat simultaneously. It operates as a temporary reservation layer between seat selection and payment.

### Locking Seats — POST /seat-locks/lock

When a user selects seats, the following checks are performed in order:

1. The target showtime must exist.
2. The showtime must not have already started.
3. All requested seat IDs must exist within the hall.
4. The seats must not be permanently booked.
5. The seats must not be locked by another user.
6. Any existing locks held by the same user for the same showtime are released.
7. New `SeatLock` records are created with `ExpiresAt = UtcNow + lockMinutes`.

The lock duration is clamped server-side to a maximum of 15 minutes regardless of the value requested by the client.

### Releasing Locks — DELETE /seat-locks/release

Explicitly called when the user navigates away from the checkout flow. Deletes all active locks for the authenticated user and the specified showtime.

### Automatic Expiry — SeatLockCleanupService

A hosted background service runs every 60 seconds and deletes all `SeatLock` records where `ExpiresAt <= DateTime.UtcNow`. Because `IHostedService` is a singleton and `DbContext` is scoped, the service uses `IServiceScopeFactory` to create a new scope per cleanup cycle.

### Availability Map — GET /seat-locks/availability/{showtimeId}

Returns every seat in the hall with one of four statuses:

| Status | Meaning |
|---|---|
| Available | Free to select |
| Booked | Permanently reserved, cannot be selected |
| Locked | Temporarily held by another user |
| MyLock | Held by the currently authenticated user |

The frontend polls this endpoint every 5 seconds to keep the seat map current. The endpoint is anonymous, meaning unauthenticated users see the map but cannot distinguish their own locks from others.

---

## Payment Flow

1. The user creates a booking (`POST /bookings`), which starts in `Pending` status.
2. The user submits payment (`POST /payments`). On success, the booking transitions to `Confirmed` and a confirmation email with a PDF ticket is sent.
3. If a refund is requested (`POST /payments/{id}/refund`), the payment status becomes `Refunded` and the booking transitions to `Canceled` via `CancelAfterRefund()`.

---

## Booking Lifecycle

```
Pending
  |-- payment succeeds --> Confirmed
  |-- user cancels     --> Canceled
  |-- time expires     --> Expired

Confirmed
  |-- check-in         --> CheckedIn
  |-- refund issued    --> Canceled

CheckedIn
  |-- refund issued    --> Canceled
```

---

## Middleware

Three custom middleware components are registered in the pipeline before authentication:

### GlobalExceptionMiddleware

Catches all unhandled exceptions and maps them to structured JSON responses:

| Exception Type | HTTP Status |
|---|---|
| `ValidationException` | 400 Bad Request |
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `InvalidOperationException` | 409 Conflict |
| All others | 500 Internal Server Error |

All error responses include `StatusCode`, `Message`, and `Timestamp` fields. Validation errors additionally include a list of field-level error objects.

### IdempotencyMiddleware

Applies to all POST requests except `/auth/login` and `/auth/register`. Requires an `Idempotency-Key` header. If a request with a given key has already been processed successfully (2xx response), the cached response is returned immediately with an `Idempotency-Cache: HIT` header. This prevents duplicate bookings or payments caused by network retries.

### RequestLoggingMiddleware

Logs the HTTP method, path, response status code, and elapsed time for every request.

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server or SQL Server LocalDB
- Node.js 20+ (for the frontend)

### Backend Setup

```bash
cd ".Net projects"

# Restore packages
dotnet restore

# Apply database migrations (runs automatically on startup via SeedIdentityAsync)
# Or apply manually:
dotnet ef database update --project CinemaBooking.Infrastructure --startup-project CinemaBooking.API

# Run the API
dotnet run --project CinemaBooking.API
```

The API will be available at `https://localhost:5001`. The Scalar API reference is available at `/scalar` in development.

### Frontend Setup

```bash
cd ".Net projects/CinemaBookingFrontend/CinemaFrontendApp"
npm install
npm run dev
```

The frontend development server starts at `http://localhost:5173`.

---

## Configuration

All configuration lives in `CinemaBooking.API/appsettings.json`.

### Database

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CinemaBookingDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### JWT

```json
"Jwt": {
  "Key": "your-secret-key-at-least-32-characters",
  "Issuer": "CinemaBooking.API",
  "Audience": "CinemaBooking.Client",
  "ExpiresInMinutes": 60
}
```

The application throws `InvalidOperationException` at startup if the JWT key is missing or empty.

### CORS

```json
"AllowedOrigins": [ "http://localhost:5173" ]
```

### SMTP

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UserName": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "CinemaBooking",
  "UseSsl": true
}
```

SMTP is used to send booking confirmation emails with attached PDF tickets after a successful payment.
