# ProcureFlow Backend

The ProcureFlow backend is a .NET 8 ASP.NET Core Web API for authentication and the Purchase-to-Pay workflow. It uses PostgreSQL through Entity Framework Core and exposes role-protected REST endpoints consumed by the React frontend.

## Technology stack

- .NET 8 / ASP.NET Core Web API
- ASP.NET Core Identity
- JWT bearer authentication (HMAC SHA-256, eight-hour token lifetime)
- Entity Framework Core 8
- Npgsql PostgreSQL provider
- AutoMapper
- FluentValidation
- Swashbuckle/Swagger
- xUnit and Moq

## Solution architecture

```text
ProcureFlow.API
  Controllers, middleware, authentication/CORS pipeline, Swagger, DI
        |
        v
ProcureFlow.Application
  DTOs, validators, service interfaces, business services, mapping
        |
        v
ProcureFlow.Infrastructure
  EF Core DbContext, repositories, Identity, JWT creation, seed data
        |
        v
ProcureFlow.Domain
  Entities, base entity, workflow enums
```

Dependency direction:

- Domain has no project dependency on the other application layers.
- Application depends on Domain.
- Infrastructure depends on Application and Domain.
- API composes Application and Infrastructure.

Controllers remain thin. Application services enforce workflow and ownership rules. Repositories handle persistence and conditional/atomic status transitions.

## Prerequisites

- .NET 8 SDK
- PostgreSQL 16 or Docker Desktop
- Optional: `dotnet-ef` for explicit migration commands

## Local database

From the repository root:

```bash
docker compose up -d postgres
```

The checked-in local connection string matches Docker Compose:

```text
Host=localhost;Port=5432;Database=procureflow;Username=postgres;Password=postgres
```

Do not put production credentials in `appsettings.json`. Override configuration with user-secrets or environment variables.

Example using .NET user-secrets:

```bash
cd backend/src/ProcureFlow.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=procureflow;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-development-key"
```

Equivalent environment variable names include:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
AllowedOrigins__0
```

## Run the API

From the repository root:

```bash
dotnet restore backend/ProcureFlow.sln
dotnet run --project backend/src/ProcureFlow.API/ProcureFlow.API.csproj
```

Local URLs:

- API: `http://localhost:5080`
- Swagger UI: `http://localhost:5080/swagger`
- OpenAPI document: `http://localhost:5080/swagger/v1/swagger.json`

The Development launch profile is defined in `src/ProcureFlow.API/Properties/launchSettings.json`.

## Database migrations and seed data

An `InitialCreate` migration is checked in under `src/ProcureFlow.Infrastructure/Persistence/Migrations`.

At startup, `SeedData`:

1. Applies pending migrations with `MigrateAsync`.
2. Falls back to `EnsureCreatedAsync` only when no migrations are pending.
3. Creates Requester, Approver, and Admin roles.
4. Seeds departments, categories, and vendors when master data is empty.
5. Creates the three demo users if they do not exist.

Useful migration commands:

```bash
dotnet tool install --global dotnet-ef

dotnet ef migrations add <MigrationName> \
  --project backend/src/ProcureFlow.Infrastructure \
  --startup-project backend/src/ProcureFlow.API \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project backend/src/ProcureFlow.Infrastructure \
  --startup-project backend/src/ProcureFlow.API
```

Automatic migrations during application startup are convenient here, but production systems commonly run migrations as a separate deployment step.

## Authentication and authorization

Authenticate with `POST /api/auth/login`, then send the returned token as:

```http
Authorization: Bearer <token>
```

Demo accounts all use `Demo@123`:

| Email | Role |
|---|---|
| `requester@procureflow.demo` | Requester |
| `approver@procureflow.demo` | Approver |
| `admin@procureflow.demo` | Admin |

JWT claims include subject/user ID, name identifier, email, full name, and roles. Tokens expire after eight hours. There is currently no refresh-token endpoint.

## API conventions

### JSON naming

ASP.NET Core’s default web JSON settings emit camel-cased property names.

### Enum values

Enums are numeric in API JSON because `JsonStringEnumConverter` is not configured.

Currency:

| Value | Meaning |
|---:|---|
| 1 | INR |
| 2 | USD |
| 3 | EUR |
| 4 | GBP |

Purchase request status:

| Value | Meaning |
|---:|---|
| 1 | Draft |
| 2 | Submitted |
| 3 | Approved |
| 4 | Rejected |
| 5 | POCreated |
| 6 | Delivered |
| 7 | Completed |

Purchase order and delivery status:

| Value | Meaning |
|---:|---|
| 1 | Created/Pending |
| 2 | Delivered |
| 3 | Completed |

EF Core stores these enum values as strings in PostgreSQL, while the HTTP contract currently uses numeric values.

### Pagination envelope

Paged endpoints return:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

Page numbers are one-based. Repository code clamps page size to the range 1–100.

### Error envelope

Handled application errors return:

```json
{
  "statusCode": 400,
  "message": "Validation failed.",
  "errors": ["Required Date cannot be earlier than 20 Sep 2026."]
}
```

Typical status codes:

- `400`: FluentValidation failure.
- `401`: missing or invalid bearer token, produced by authentication middleware.
- `403`: authorization/ownership failure.
- `404`: resource not found.
- `409`: invalid workflow transition or business-rule conflict.
- `500`: unexpected server error with details suppressed from the response.

## API reference

### Authentication

#### `POST /api/auth/login`

Anonymous.

Request:

```json
{
  "email": "requester@procureflow.demo",
  "password": "Demo@123"
}
```

Response:

```json
{
  "token": "<jwt>",
  "email": "requester@procureflow.demo",
  "fullName": "Requester User",
  "roles": ["Requester"]
}
```

#### `GET /api/auth/me`

Returns the authenticated email, name, and roles.

### Dashboard and master data

| Method | Route | Role | Result |
|---|---|---|---|
| GET | `/api/dashboard/summary` | Authenticated | Counts for all PR states and total POs |
| GET | `/api/departments` | Authenticated | `{id, name}` lookups |
| GET | `/api/categories` | Authenticated | `{id, name}` lookups |
| GET | `/api/vendors` | Authenticated | `{id, name}` lookups |

### Purchase requests

#### `GET /api/purchase-requests`

Query parameters:

| Parameter | Default | Notes |
|---|---:|---|
| `pageNumber` | 1 | One-based |
| `pageSize` | 10 | Clamped to 1–100 |
| `search` | — | PR number, description, or vendor |
| `status` | — | Enum name or numeric value accepted by ASP.NET model binding |
| `sortBy` | `createdAt` | Supported: `createdAt`, `amount` |
| `sortDirection` | `desc` | `asc` or `desc` |

Example:

```text
GET /api/purchase-requests?pageNumber=1&pageSize=10&search=laptop&status=Submitted&sortBy=amount&sortDirection=asc
```

Requesters receive only their own records. Other authenticated roles are not restricted by requester ID.

Each purchase-request list item includes:

- `createdAt`: original creation time.
- `updatedAt`: most recent general entity update when available.
- `lastActivityAt`: latest `StatusHistory.ChangedAt`, falling back to `updatedAt` and then `createdAt`. Dashboard recent activity should use this field instead of treating creation time as the time of the current status.

#### `GET /api/purchase-requests/{id}`

Returns base PR fields plus submission/approval/rejection data, status history, purchase-order summary, and delivery summary when present. Requesters can access only owned requests.

#### `POST /api/purchase-requests`

Requester only.

#### `PUT /api/purchase-requests/{id}`

Requester only; owned Draft requests only.

Create/update body:

```json
{
  "departmentId": "00000000-0000-0000-0000-000000000000",
  "vendorId": "00000000-0000-0000-0000-000000000000",
  "categoryId": "00000000-0000-0000-0000-000000000000",
  "description": "Developer laptops",
  "amount": 250000,
  "currency": 1,
  "requiredDate": "2026-10-31"
}
```

`requiredDate` must not be earlier than the server’s current UTC date.

#### `DELETE /api/purchase-requests/{id}`

Requester only; deletes an owned Draft request. Returns `204 No Content`.

#### `POST /api/purchase-requests/{id}/submit`

Requester only; transitions Draft to Submitted. Returns `204 No Content`.

### Approvals

Approver only.

#### `GET /api/approvals`

Supports `pageNumber`, `pageSize`, and `search`. Returns Submitted purchase requests.

#### `POST /api/approvals/{id}/approve`

Transitions Submitted to Approved. Returns `204 No Content`.

#### `POST /api/approvals/{id}/reject`

Request:

```json
{
  "reason": "Budget approval is required."
}
```

Transitions Submitted to Rejected. Returns `204 No Content`.

### Purchase orders

#### `GET /api/purchase-orders`

Authenticated. Supports `pageNumber`, `pageSize`, `search`, and `status`. Search covers PO number, PR number, and vendor.

#### `GET /api/purchase-orders/{id}`

Authenticated. Returns one purchase order.

#### `POST /api/purchase-orders`

Admin only.

```json
{
  "purchaseRequestId": "00000000-0000-0000-0000-000000000000"
}
```

The linked request must be Approved and must not already have a PO.

### Deliveries

Admin only.

#### `POST /api/deliveries/{poId}/deliver`

```json
{
  "deliveryDate": "2026-10-25",
  "notes": "Received in good condition"
}
```

Transitions a Created PO and its PR to Delivered. Returns `204 No Content`.

#### `POST /api/deliveries/{poId}/complete`

Transitions a Delivered PO, delivery, and PR to Completed. Returns `204 No Content`.

## CORS

Allowed origins come from the `AllowedOrigins` configuration array. Local fallback is `http://localhost:5173`.

Azure App Service example:

```text
AllowedOrigins__0=https://<your-static-app>.azurestaticapps.net
```

Do not add a trailing slash. CORS is intentionally restrictive; do not replace it with `AllowAnyOrigin` when bearer credentials or other sensitive APIs are involved.

## Tests

Run all backend tests:

```bash
dotnet test backend/ProcureFlow.sln
```

The unit tests currently cover:

- Draft PR submission.
- Submitted PR edit rejection.
- Submitted PR approval.
- Draft PR approval rejection.
- PO creation from an Approved request.
- Duplicate PO rejection.
- Delivery followed by completion.
- Completion-before-delivery rejection.

The integration test project currently contains only a project-wiring smoke test and does not exercise HTTP endpoints or PostgreSQL.

## Key technical decisions

- Workflow mutations are explicit action endpoints.
- Conditional repository updates prevent stale concurrent transitions.
- Every PR status transition writes an immutable status-history row with `ChangedAt`; atomic transitions also maintain `UpdatedAt` and `UpdatedBy` on affected PR, PO, and delivery records.
- Unique indexes enforce PR number, PO number, one PO per PR, and one delivery per PO constraints.
- Amount columns use PostgreSQL precision `numeric(18,2)`.
- Entity and workflow timestamps are recorded in UTC. Tracked changes are stamped by the DbContext, while direct atomic updates set their audit fields explicitly.
- Requester ownership is enforced in the application service.
- Error details and stack traces are not exposed to clients.

## Known limitations

- PR/PO identifiers are count-based rather than sequence-based and should be replaced with database sequences for production concurrency.
- The integration suite is not a real integration suite yet.
- AutoMapper 13.0.1 currently produces NU1903 for a published high-severity advisory; it must be upgraded and regression-tested before production use.
- EF Core package references currently mix 8.0.8 and 8.0.31, which produces assembly conflict warnings in the integration-test build. Align the API, Infrastructure, Domain, and test dependencies to one supported patch version.
- The current build has minor compiler warnings for duplicate imports and unused constructor dependencies.
- Automatic startup migrations can cause contention with multiple application instances.
- Demo credentials and the example JWT key are not production-safe.
- No refresh tokens, revocation list, account lockout policy customization, or password-reset workflow is exposed.
- Swagger is currently enabled in every environment.
- Master data is seed-driven and has no CRUD API.
- There is no audit/event outbox, background processing, or distributed transaction strategy.
- Logging uses the standard ASP.NET Core pipeline without structured observability/tracing configuration.
