import { CheckCircle, LocalShipping } from "@mui/icons-material";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { DataGrid, GridColDef, GridPaginationModel } from "@mui/x-data-grid";
import { useEffect, useState } from "react";
import {
  useCompleteMutation,
  useDeliverMutation,
  usePosQuery,
} from "../api/baseApi";
import {
  ConfirmDialog,
  GridShell,
  LoadingActionButton,
  PageHeader,
  StatusChip,
} from "../components/common";
import { useNotification } from "../components/NotificationProvider";
import { getApiErrorMessage } from "../utils/apiError";

const today = new Date().toISOString().slice(0, 10);

export default function DeliveriesPage() {
  const [pagination, setPagination] = useState<GridPaginationModel>({
    page: 0,
    pageSize: 10,
  });
  const [searchText, setSearchText] = useState("");
  const [search, setSearch] = useState("");
  const [deliveryRow, setDeliveryRow] = useState<{
    id: string;
    poNumber: string;
  } | null>(null);
  const [completeRow, setCompleteRow] = useState<{
    id: string;
    poNumber: string;
  } | null>(null);
  const [deliveryDate, setDeliveryDate] = useState(today);
  const [notes, setNotes] = useState("");
  const [dateError, setDateError] = useState("");
  const { notify } = useNotification();
  useEffect(() => {
    const timer = setTimeout(() => {
      setSearch(searchText.trim());
      setPagination((p) => ({ ...p, page: 0 }));
    }, 350);
    return () => clearTimeout(timer);
  }, [searchText]);
  const { data, isLoading, isFetching } = usePosQuery({
    pageNumber: pagination.page + 1,
    pageSize: pagination.pageSize,
    search: search || undefined,
  });
  const [deliver, { isLoading: delivering }] = useDeliverMutation();
  const [complete, { isLoading: completing }] = useCompleteMutation();
  const markDelivered = async () => {
    if (!deliveryRow) return;
    if (!deliveryDate) {
      setDateError("Delivery date is required.");
      return;
    }
    try {
      await deliver({
        poId: deliveryRow.id,
        deliveryDate,
        notes: notes.trim() || undefined,
      }).unwrap();
      notify("Delivery recorded successfully.");
      setDeliveryRow(null);
      setNotes("");
      setDeliveryDate(today);
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    }
  };
  const completePo = async () => {
    if (!completeRow) return;
    try {
      await complete(completeRow.id).unwrap();
      notify("Purchase order completed successfully.");
      setCompleteRow(null);
    } catch (error) {
      notify(getApiErrorMessage(error), "error");
    }
  };
  const columns: GridColDef[] = [
    { field: "poNumber", headerName: "PO Number", width: 160 },
    { field: "prNumber", headerName: "PR Number", width: 160 },
    { field: "vendorName", headerName: "Vendor", flex: 1, minWidth: 180 },
    {
      field: "orderDate",
      headerName: "Order Date",
      width: 145,
      valueFormatter: (v) =>
        new Date(String(v)).toLocaleDateString("en-GB", {
          day: "2-digit",
          month: "short",
          year: "numeric",
        }),
    },
    {
      field: "status",
      headerName: "Status",
      width: 130,
      renderCell: (p) => <StatusChip status={Number(p.value)} type="po" />,
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 260,
      sortable: false,
      renderCell: (p) => (
        <Stack direction="row" spacing={1}>
          {p.row.status === 1 && (
            <Button
              size="small"
              startIcon={<LocalShipping />}
              disabled={delivering || completing}
              onClick={() =>
                setDeliveryRow({ id: p.row.id, poNumber: p.row.poNumber })
              }
            >
              Mark Delivered
            </Button>
          )}
          {p.row.status === 2 && (
            <Button
              size="small"
              color="success"
              startIcon={<CheckCircle />}
              disabled={delivering || completing}
              onClick={() =>
                setCompleteRow({ id: p.row.id, poNumber: p.row.poNumber })
              }
            >
              Complete
            </Button>
          )}
        </Stack>
      ),
    },
  ];
  return (
    <>
      <PageHeader
        title="Deliveries"
        subtitle="Record deliveries and complete fulfilled purchase orders."
      />
      <TextField
        label="Search"
        placeholder="PO, PR or vendor"
        value={searchText}
        onChange={(e) => setSearchText(e.target.value)}
        sx={{ width: { xs: "100%", sm: 250 }, mb: 1.5 }}
      />
      <GridShell>
        <DataGrid
          autoHeight
          rows={data?.items ?? []}
          columns={columns}
          loading={isLoading || isFetching}
          rowCount={data?.totalCount ?? 0}
          paginationMode="server"
          paginationModel={pagination}
          onPaginationModelChange={setPagination}
          pageSizeOptions={[10, 25, 50]}
          disableRowSelectionOnClick
          localeText={{ noRowsLabel: "No purchase orders found" }}
        />
      </GridShell>
      <Dialog
        open={!!deliveryRow}
        onClose={delivering ? undefined : () => setDeliveryRow(null)}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>Mark {deliveryRow?.poNumber} as delivered</DialogTitle>
        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              type="date"
              label="Delivery Date"
              InputLabelProps={{ shrink: true }}
              value={deliveryDate}
              onChange={(e) => {
                setDeliveryDate(e.target.value);
                setDateError("");
              }}
              required
              error={!!dateError}
              helperText={dateError}
            />
            <TextField
              label="Notes (optional)"
              multiline
              minRows={3}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Add delivery notes, reference numbers, or condition details"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeliveryRow(null)} disabled={delivering}>
            Cancel
          </Button>
          <LoadingActionButton
            variant="contained"
            loading={delivering}
            onClick={markDelivered}
          >
            Mark Delivered
          </LoadingActionButton>
        </DialogActions>
      </Dialog>
      <ConfirmDialog
        open={!!completeRow}
        title={`Complete ${completeRow?.poNumber ?? "purchase order"}?`}
        message="Confirm that this delivery is fully received and the purchase order can be completed."
        confirmLabel="Complete"
        loading={completing}
        onClose={() => setCompleteRow(null)}
        onConfirm={completePo}
      />
    </>
  );
}
