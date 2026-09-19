# ProcureFlow

ProcureFlow is a role-based Purchase-to-Pay application covering the workflow from purchase request through approval, purchase order, delivery, and completion.

```text
Draft -> Submitted -> Approved -> PO Created -> Delivered -> Completed
                     
                     -> Rejected
```

This is a monorepo containing a React single-page application, a .NET Web API, PostgreSQL persistence, automated backend tests, and Azure deployment workflows.

## Components

| Component | Purpose | Primary technology | Documentation |
|---|---|---|---|
| Frontend | Role-aware web interface | React, TypeScript, Vite, Material UI, Redux Toolkit | [frontend/README.md](frontend/README.md) |
| Backend | Authentication, API, and workflow rules | .NET 8, ASP.NET Core, EF Core, Identity, JWT | [backend/README.md](backend/README.md) |
| Database | Transactional persistence | PostgreSQL 16 | [backend/README.md](backend/README.md#local-database) |
| Deployment | Static frontend and hosted API | Azure Static Web Apps and Azure App Service | [Frontend](frontend/README.md#deployment) / [Backend](backend/README.md#cors) |

## Product capabilities

- JWT authentication for Requester, Approver, and Admin roles.
- Purchase-request creation, editing, deletion, submission, details, and status history.
- Approval and rejection workflows.
- Purchase-order creation from approved requests.
- Delivery recording and completion.
- Dashboard workflow totals.
- Server-side search, filtering, sorting, and pagination.
- Responsive light/dark user interface with notifications and loading states.
- Backend-enforced authorization, ownership, validation, and workflow transitions.

## Architecture overview

```text
React + RTK Query
       |
       | HTTPS / JSON / JWT
       v
ASP.NET Core Controllers
       v
Application Services
       v
Repositories + EF Core
       v
PostgreSQL
```

The frontend and backend are built and deployed independently. Frontend route guards improve the user experience, while the API remains the security boundary.

## Repository layout

```text
ProcureFlow/
  .github/workflows/       Azure deployment workflows
  backend/
    ProcureFlow.sln
    README.md              Backend setup and API reference
    src/
    tests/
  frontend/
    README.md              Frontend setup and implementation guide
    src/
  docs/
  docker-compose.yml
```

## Quick start

Requirements: .NET 8 SDK, Node.js 20 or a supported LTS release, npm, and Docker Desktop or PostgreSQL 16.

Start PostgreSQL from the repository root:

```bash
docker compose up -d postgres
```

Start the API:

```bash
dotnet run --project backend/src/ProcureFlow.API/ProcureFlow.API.csproj
```

Start the frontend in another terminal:

```bash
cd frontend
npm install
cp .env.example .env
npm run dev
```

PowerShell users can replace the copy command with:

```powershell
Copy-Item .env.example .env
```

Open:

- Frontend: `http://localhost:5173`
- Swagger: `http://localhost:5080/swagger`

For database configuration, migrations, CORS, authentication, and API contracts, use the [backend guide](backend/README.md). For frontend environment variables, routes, state, and production builds, use the [frontend guide](frontend/README.md).

## Demo workflow

Local seed accounts use the password `Demo@123`:

| Email | Role |
|---|---|
| `requester@procureflow.demo` | Requester |
| `approver@procureflow.demo` | Approver |
| `admin@procureflow.demo` | Admin |

Typical flow:

1. Requester creates and submits a Draft purchase request.
2. Approver approves or rejects the Submitted request.
3. Admin creates a PO from an Approved request.
4. Admin records delivery and completes the PO.

These credentials are for local/demo use only.

## Build and test

Backend:

```bash
dotnet build backend/ProcureFlow.sln
dotnet test backend/ProcureFlow.sln
```

Frontend:

```bash
cd frontend
npm install
npm run build
```

The current backend suite contains eight workflow unit tests and one integration-project smoke test. Frontend automated tests are not yet configured. Component-specific warnings and limitations are documented in the respective READMEs.

## Deployment overview

- `.github/workflows/azure-static-web-apps-white-field-0fd17a610.yml` builds and deploys the frontend.
- `.github/workflows/main_procureflow-api.yml` publishes and deploys the API.
- `VITE_API_URL` is embedded into the frontend during its production build.
- Backend connection strings, JWT settings, and allowed frontend origins must be configured through Azure App Service settings or another secret store.

See [frontend deployment](frontend/README.md#deployment) and [backend configuration/CORS](backend/README.md#cors) for the component-specific requirements.

## Documentation ownership

To avoid duplicated and conflicting instructions:

- This root README contains only the product overview, repository entry point, and cross-component workflow.
- `backend/README.md` owns backend architecture, database setup, migrations, authentication, API reference, tests, backend decisions, and backend limitations.
- `frontend/README.md` owns frontend setup, environment variables, routes, state, forms, grids, build/deployment behavior, frontend decisions, and frontend limitations.

## AI-assisted development disclosure

ChatGPT, Codex, or GitHub Copilot may have been used for scaffolding, implementation assistance, debugging, testing, and documentation. Maintainers should review and understand architecture, security, and implementation decisions before production use.
