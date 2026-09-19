import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { RootState } from "../app/store";
import type {
  AuthResponse,
  DashboardSummary,
  Lookup,
  PagedResult,
  PurchaseOrder,
  PurchaseRequest,
  PurchaseRequestDetails,
  PurchaseRequestInput,
} from "../types";
const raw = fetchBaseQuery({
  baseUrl: import.meta.env.VITE_API_URL ?? "http://localhost:5080/api",
  prepareHeaders: (h, { getState }) => {
    const t = (getState() as RootState).auth.user?.token;
    if (t) h.set("authorization", `Bearer ${t}`);
    return h;
  },
});
export const api = createApi({
  reducerPath: "api",
  baseQuery: raw,
  tagTypes: ["PR", "Approval", "PO", "Dashboard"],
  endpoints: (b) => ({
    login: b.mutation<AuthResponse, { email: string; password: string }>({
      query: (body) => ({ url: "auth/login", method: "POST", body }),
    }),
    dashboard: b.query<DashboardSummary, void>({
      query: () => "dashboard/summary",
      providesTags: ["Dashboard"],
    }),
    departments: b.query<Lookup[], void>({ query: () => "departments" }),
    categories: b.query<Lookup[], void>({ query: () => "categories" }),
    vendors: b.query<Lookup[], void>({ query: () => "vendors" }),
    prs: b.query<
      PagedResult<PurchaseRequest>,
      {
        pageNumber?: number;
        pageSize?: number;
        search?: string;
        status?: number;
        sortBy?: string;
        sortDirection?: "asc" | "desc";
      }
    >({
      query: (q) => ({ url: "purchase-requests", params: q }),
      providesTags: (r) =>
        r
          ? [
              ...r.items.map((x) => ({ type: "PR" as const, id: x.id })),
              { type: "PR" as const, id: "LIST" },
            ]
          : [{ type: "PR" as const, id: "LIST" }],
    }),
    pr: b.query<PurchaseRequestDetails, string>({
      query: (id) => `purchase-requests/${id}`,
      providesTags: (_r, _e, id) => [{ type: "PR", id }],
    }),
    createPr: b.mutation<PurchaseRequest, PurchaseRequestInput>({
      query: (body) => ({ url: "purchase-requests", method: "POST", body }),
      invalidatesTags: ["PR", "Dashboard"],
    }),
    updatePr: b.mutation<
      PurchaseRequest,
      { id: string; body: PurchaseRequestInput }
    >({
      query: (x) => ({
        url: `purchase-requests/${x.id}`,
        method: "PUT",
        body: x.body,
      }),
      invalidatesTags: (_r, _e, x) => [
        { type: "PR", id: x.id },
        { type: "PR", id: "LIST" },
        "Dashboard",
      ],
    }),
    deletePr: b.mutation<void, string>({
      query: (id) => ({ url: `purchase-requests/${id}`, method: "DELETE" }),
      invalidatesTags: ["PR", "Dashboard"],
    }),
    submitPr: b.mutation<void, string>({
      query: (id) => ({
        url: `purchase-requests/${id}/submit`,
        method: "POST",
      }),
      invalidatesTags: ["PR", "Approval", "Dashboard"],
    }),
    approvals: b.query<
      PagedResult<PurchaseRequest>,
      { pageNumber?: number; pageSize?: number; search?: string }
    >({
      query: (q) => ({ url: "approvals", params: q }),
      providesTags: ["Approval"],
    }),
    approve: b.mutation<void, string>({
      query: (id) => ({ url: `approvals/${id}/approve`, method: "POST" }),
      invalidatesTags: ["Approval", "PR", "Dashboard"],
    }),
    reject: b.mutation<void, { id: string; reason: string }>({
      query: (x) => ({
        url: `approvals/${x.id}/reject`,
        method: "POST",
        body: { reason: x.reason },
      }),
      invalidatesTags: ["Approval", "PR", "Dashboard"],
    }),
    pos: b.query<
      PagedResult<PurchaseOrder>,
      {
        pageNumber?: number;
        pageSize?: number;
        search?: string;
        status?: number;
      }
    >({
      query: (q) => ({ url: "purchase-orders", params: q }),
      providesTags: ["PO"],
    }),
    createPo: b.mutation<PurchaseOrder, { purchaseRequestId: string }>({
      query: (body) => ({ url: "purchase-orders", method: "POST", body }),
      invalidatesTags: ["PO", "PR", "Dashboard"],
    }),
    deliver: b.mutation<
      void,
      { poId: string; deliveryDate: string; notes?: string }
    >({
      query: (x) => ({
        url: `deliveries/${x.poId}/deliver`,
        method: "POST",
        body: { deliveryDate: x.deliveryDate, notes: x.notes },
      }),
      invalidatesTags: ["PO", "PR", "Dashboard"],
    }),
    complete: b.mutation<void, string>({
      query: (id) => ({ url: `deliveries/${id}/complete`, method: "POST" }),
      invalidatesTags: ["PO", "PR", "Dashboard"],
    }),
  }),
});
export const {
  useLoginMutation,
  useDashboardQuery,
  useDepartmentsQuery,
  useCategoriesQuery,
  useVendorsQuery,
  usePrsQuery,
  usePrQuery,
  useCreatePrMutation,
  useUpdatePrMutation,
  useDeletePrMutation,
  useSubmitPrMutation,
  useApprovalsQuery,
  useApproveMutation,
  useRejectMutation,
  usePosQuery,
  useCreatePoMutation,
  useDeliverMutation,
  useCompleteMutation,
} = api;
