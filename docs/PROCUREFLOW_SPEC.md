# ProcureFlow Specification

## Stack
- React + TypeScript + Vite
- Material UI
- Redux Toolkit + RTK Query
- React Hook Form + Zod
- ASP.NET Core Web API (.NET 8)
- Controller -> Service -> individual Repository -> EF Core -> PostgreSQL
- ASP.NET Core Identity + JWT
- AutoMapper + FluentValidation

## Roles
- Requester: create/edit/delete Draft PRs, submit, view own PRs.
- Approver: view Submitted PRs, approve/reject.
- Admin: view PRs, create PO from Approved PR, record delivery, complete transaction.

## Workflow
Draft -> Submitted -> Approved -> POCreated -> Delivered -> Completed
Submitted -> Rejected is final.

## Key constraints
- One PR can create at most one PO.
- Rejected PRs cannot be resubmitted.
- No partial deliveries.
- Backend owns status transitions; clients never directly set workflow status.
