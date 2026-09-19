# ProcureFlow Frontend

The ProcureFlow frontend is a responsive React and TypeScript single-page application for Requester, Approver, and Admin purchase workflows.

## Technology stack

- React 18
- TypeScript 5
- Vite 5
- Material UI 6
- MUI X Data Grid 7 Community
- Redux Toolkit and React Redux
- RTK Query
- React Router 6
- React Hook Form and Zod

## Main capabilities

- JWT login and persisted client session.
- Role-aware navigation and protected routes.
- Responsive/collapsible navigation shell.
- Light and dark themes persisted in browser storage.
- Dashboard workflow metrics.
- Server-side purchase-request pagination, search, status filtering, and amount/created-date sorting.
- Purchase-request create/edit dialogs, Draft-only delete, submission, details, history, PO summary, and delivery summary.
- Pending approval search, approval, and validated rejection dialog.
- Purchase-order search, status filter, pagination, and creation.
- Delivery search, delivery-date/notes dialog, and completion confirmation.
- Shared Snackbars, friendly API error parsing, loading buttons, status chips, confirmation dialogs, and viewport-filling grids.

## Source structure

```text
src/
  api/
    baseApi.ts                 RTK Query endpoints and bearer-token headers
  app/
    store.ts                   Redux store and authentication slice
  components/
    Layout.tsx                 AppBar, sidebar, responsive content shell
    ProtectedRoute.tsx         Authentication/role route guard
    ColorModeProvider.tsx      Theme and persisted color mode
    NotificationProvider.tsx   Global Snackbar API
    PurchaseRequestForm.tsx    Shared create/edit form
    EditPurchaseRequestDialog.tsx
    common.tsx                 Shared headers, loaders, buttons, dialogs, grids
  pages/
    LoginPage.tsx
    DashboardPage.tsx
    PurchaseRequestsPage.tsx
    PurchaseRequestDetailsPage.tsx
    NewPurchaseRequestPage.tsx
    EditPurchaseRequestPage.tsx
    ApprovalsPage.tsx
    PurchaseOrdersPage.tsx
    DeliveriesPage.tsx
  types/
  utils/
    apiError.ts
```

## Prerequisites

- Node.js 20 or a currently supported Node.js LTS release
- npm
- ProcureFlow API running locally or deployed

## Local setup

```bash
cd frontend
npm install
```

Create `.env` from the example:

```bash
cp .env.example .env
```

PowerShell:

```powershell
Copy-Item .env.example .env
```

The local value is:

```dotenv
VITE_API_URL=http://localhost:5080/api
```

Start Vite:

```bash
npm run dev
```

Open `http://localhost:5173`.

## Scripts

| Command | Purpose |
|---|---|
| `npm run dev` | Start Vite development server |
| `npm run build` | Run TypeScript project build and create production assets |
| `npm run preview` | Preview the production build locally |

There is currently no frontend test or lint script.

## Environment configuration

`VITE_API_URL` is the API base URL including `/api`.

Development example:

```text
http://localhost:5080/api
```

Production example:

```text
https://procureflow-api.azurewebsites.net/api
```

Vite replaces `import.meta.env` values at build time. A production deployment must be rebuilt after changing `VITE_API_URL`. `baseApi.ts` permits the localhost fallback only in Vite development mode; if a production bundle is created without an API URL, the application throws a configuration error when it starts in the browser.

The Azure Static Web Apps workflow supplies the GitHub Actions repository variable `VITE_API_URL` and contains the current Azure API URL as a fallback.

## Authentication and state

Successful login returns a JWT, email, full name, and roles. The authentication slice stores that response in Redux and mirrors it to:

```text
localStorage key: procureflow_auth
```

RTK Query reads the token from Redux and adds:

```http
Authorization: Bearer <token>
```

Logging out clears the Redux state and local storage. The application currently does not decode token expiration, refresh tokens, or automatically log out on a `401` response.

## Routes and role access

| Route | Access | Purpose |
|---|---|---|
| `/login` | Public | Login |
| `/` | Authenticated | Dashboard |
| `/purchase-requests` | Authenticated | PR list and Requester actions |
| `/purchase-requests/new` | Requester | Direct create page; normal list workflow uses a dialog |
| `/purchase-requests/:id` | Authenticated | PR details |
| `/purchase-requests/:id/edit` | Requester | Direct edit page; normal UI uses a dialog |
| `/approvals` | Approver | Pending approvals |
| `/purchase-orders` | Admin | PO list and creation |
| `/deliveries` | Admin | Delivery and completion operations |

Frontend route guards are UX controls. The API is responsible for actual authorization and ownership enforcement.

## Forms and validation

The PR form is shared between create and edit flows and validates:

- Department, category, and vendor are required.
- Description is 3–1000 characters.
- Amount must be greater than zero.
- Currency is required.
- Required date cannot be earlier than the current client date.

Backend validation remains authoritative. API failures use `utils/apiError.ts` to flatten the backend error envelope into readable Snackbar messages.

## Data grids

The project retains MUI X Data Grid Community rather than AG Grid to avoid unnecessary migration risk.

- PR and PO lists use server-side pagination.
- PR list uses server-side status filtering and supported sorting.
- PO list uses server-side search and status filtering.
- Approvals and deliveries use server-side search/pagination.
- Grid containers use flex sizing so pagination stays at the bottom without fixed viewport calculations.
- Only Community features are used.

## API cache behavior

RTK Query tags include `PR`, `Approval`, `PO`, and `Dashboard`. Mutations invalidate related tags so affected screens refetch automatically. Individual PR detail queries use entity IDs in their cache tags.

## Theme and reusable UI

- Theme preference is saved under `procureflow_color_mode`.
- `NotificationProvider` provides global success/error/warning Snackbars.
- `LoadingActionButton` prevents duplicate mutation clicks.
- `ConfirmDialog` standardizes destructive/action confirmation.
- `GridShell` provides common grid sizing and styling.
- `StatusChip` centralizes PR and PO status presentation.

## Production build

```bash
cd frontend
npm install
VITE_API_URL=https://procureflow-api.azurewebsites.net/api npm run build
```

PowerShell:

```powershell
$env:VITE_API_URL = 'https://procureflow-api.azurewebsites.net/api'
npm run build
```

Build output is written to `frontend/dist` and is excluded from Git.

## Deployment

The frontend workflow is:

```text
.github/workflows/azure-static-web-apps-white-field-0fd17a610.yml
```

It deploys `frontend/dist` to Azure Static Web Apps. The backend must allow the deployed Static Web Apps origin through its `AllowedOrigins` configuration.

## Key technical decisions

- RTK Query centralizes API calls, loading state, caching, and invalidation.
- One shared PR form prevents create/edit validation drift.
- Global notification and error parsing avoid page-specific alert implementations.
- Server-side list operations avoid fetching a large client-side dataset.
- Dialog-based create/edit actions preserve list context; direct routes remain available.
- Material UI theme customization provides compact responsive density and dark mode.
- Backend roles and ownership are never assumed to be secured by hidden UI controls.

## Known limitations

- No frontend unit, component, accessibility, visual-regression, or end-to-end test suite is configured.
- JWT storage uses `localStorage`, which is accessible to JavaScript; production security should evaluate HTTP-only secure cookies.
- No refresh-token flow or centralized `401` session-expiration handler exists.
- The frontend assumes current numeric enum values from the backend.
- Client-side “today” validation uses the browser clock, while the backend uses UTC; near midnight, the backend remains authoritative.
- Approved-PR selection during PO creation retrieves up to 50 records rather than providing an independently paginated lookup.
- The application bundle currently exceeds Vite’s default 500 kB chunk advisory; route-level lazy loading/manual chunks are not configured.
- No offline support or service worker is configured.
- Accessibility has not been independently audited.
