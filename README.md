# ProcureFlow

ProcureFlow is a technical-assessment implementation of an enterprise-style Purchase-to-Pay workflow:

**Purchase Request -> Approval -> Purchase Order -> Delivery -> Completion**

## Technology

### Frontend
React, TypeScript, Vite, Material UI, Redux Toolkit, RTK Query, React Router, React Hook Form and Zod.

### Backend
.NET 8 ASP.NET Core Web API, Controller -> Service -> Repository architecture, Entity Framework Core Code First, PostgreSQL, ASP.NET Core Identity + JWT, AutoMapper, FluentValidation and Swagger.

## Repository layout

```text
ProcureFlow/
  frontend/
  backend/
    ProcureFlow.sln
    src/
      ProcureFlow.API/
      ProcureFlow.Application/
      ProcureFlow.Domain/
      ProcureFlow.Infrastructure/
    tests/
  docs/
  docker-compose.yml
```

## Demo users

All demo accounts use password: `Demo@123`

- `requester@procureflow.demo` - Requester
- `approver@procureflow.demo` - Approver
- `admin@procureflow.demo` - Admin

These credentials are strictly for local assessment/demo use.

## 1. Start PostgreSQL

The simplest option is Docker:

```bash
docker compose up -d postgres
```

Default local database settings are already present in `backend/src/ProcureFlow.API/appsettings.json`.

## 2. Backend setup in Visual Studio 2022

Requirements:
- Visual Studio 2022 with ASP.NET/web workload
- .NET 8 SDK
- PostgreSQL 16 or Docker Desktop

Open:

```text
backend/ProcureFlow.sln
```

Set `ProcureFlow.API` as startup project.

### Recommended: create the initial EF migration

From a terminal at repository root:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project backend/src/ProcureFlow.Infrastructure --startup-project backend/src/ProcureFlow.API --output-dir Persistence/Migrations
dotnet ef database update --project backend/src/ProcureFlow.Infrastructure --startup-project backend/src/ProcureFlow.API
```

For convenience, if there are no migrations yet the included startup seeder uses `EnsureCreated` so the app can still be run immediately. Once you create `InitialCreate`, EF migrations become the normal Code First path.

Run the API. With the supplied launch settings it uses:

```text
http://localhost:5080
```

Swagger (Development):

```text
http://localhost:5080/swagger
```

## 3. Frontend setup in VS Code

```bash
cd frontend
npm install
cp .env.example .env
npm run dev
```

Open:

```text
http://localhost:5173
```

`VITE_API_URL` defaults to `http://localhost:5080/api` if no environment file is supplied.

## Business workflow

### Requester
1. Login as Requester.
2. Create a Purchase Request in Draft.
3. Edit/delete while Draft.
4. Submit for approval.

### Approver
1. Login as Approver.
2. Open Pending Approvals.
3. Approve or reject a Submitted PR.

### Admin
1. Login as Admin.
2. Create a PO from an Approved PR.
3. Mark the PO Delivered.
4. Complete the transaction.

Backend services enforce all workflow transitions and role rules.

## Statuses

Purchase Request:
- Draft
- Submitted
- Approved
- Rejected
- POCreated
- Delivered
- Completed

Purchase Order:
- Created
- Delivered
- Completed

Delivery:
- Pending
- Delivered
- Completed

## Main APIs

```text
POST /api/auth/login
GET  /api/auth/me

GET    /api/purchase-requests
GET    /api/purchase-requests/{id}
POST   /api/purchase-requests
PUT    /api/purchase-requests/{id}
DELETE /api/purchase-requests/{id}
POST   /api/purchase-requests/{id}/submit

GET  /api/approvals
POST /api/approvals/{id}/approve
POST /api/approvals/{id}/reject

GET  /api/purchase-orders
GET  /api/purchase-orders/{id}
POST /api/purchase-orders

POST /api/deliveries/{poId}/deliver
POST /api/deliveries/{poId}/complete

GET /api/dashboard/summary
GET /api/departments
GET /api/categories
GET /api/vendors
```

## Search and pagination

Purchase Request list supports parameters including:

```text
?pageNumber=1&pageSize=10&search=laptop&status=Submitted&sortBy=createdAt&sortDirection=desc
```

## Important implementation notes

- Individual repositories are used rather than a generic repository.
- Business rules are kept in services, not controllers or repositories.
- Workflow status changes are action endpoints rather than arbitrary status updates.
- PR -> PO and PO -> Delivery relationships are one-to-zero/one and protected by unique database indexes.
- UTC timestamps are used on the server.
- JWT role claims drive backend authorization and role-aware frontend navigation.

## Testing

Test projects are included under `backend/tests`. They are intentionally lightweight starting points. For the assessment, add focused tests around invalid workflow transitions and authorization, especially:

- Draft can submit.
- Submitted cannot be edited.
- Draft cannot be approved.
- Submitted can approve/reject.
- Rejected cannot create PO.
- Approved can create exactly one PO.
- Delivery requires an existing Created PO.
- Completion requires Delivered status.

## AI-assisted development disclosure

This project is structured for AI-assisted development. ChatGPT/Codex/GitHub Copilot may be used for scaffolding, implementation assistance, debugging, testing and documentation. Architecture and implementation decisions should be reviewed and understood by the candidate before submission.

## Current scaffold scope

This ZIP provides the full agreed architecture and a runnable core feature implementation for authentication, Purchase Requests, approvals, Purchase Orders, delivery/completion, dashboard summary, master data, role-aware React navigation, search and basic pagination. It is intended to be opened and continued in Visual Studio 2022 and VS Code for assessment polish, expanded tests and deployment work.
