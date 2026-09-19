import {Add,DeleteOutline,EditOutlined,Send} from '@mui/icons-material';
import {Button,Dialog,DialogContent,DialogTitle,IconButton,MenuItem,Stack,TextField,Tooltip} from '@mui/material';
import {DataGrid,GridColDef,GridPaginationModel,GridSortModel} from '@mui/x-data-grid';
import {useEffect,useMemo,useState} from 'react';
import {useSelector} from 'react-redux';
import {Link} from 'react-router-dom';
import {useCreatePrMutation,useDeletePrMutation,usePrsQuery,useSubmitPrMutation} from '../api/baseApi';
import type {RootState} from '../app/store';
import PurchaseRequestForm,{PurchaseRequestFormValues} from '../components/PurchaseRequestForm';
import EditPurchaseRequestDialog from '../components/EditPurchaseRequestDialog';
import {CURRENCIES,ConfirmDialog,GridShell,LoadingActionButton,PageHeader,PR_STATUS,StatusChip} from '../components/common';
import {useNotification} from '../components/NotificationProvider';
import {getApiErrorMessage} from '../utils/apiError';

export default function PurchaseRequestsPage(){
  const [searchText,setSearchText]=useState('');
  const [search,setSearch]=useState('');
  const [status,setStatus]=useState('');
  const [pagination,setPagination]=useState<GridPaginationModel>({page:0,pageSize:10});
  const [sort,setSort]=useState<GridSortModel>([{field:'createdAt',sort:'desc'}]);
  const [createOpen,setCreateOpen]=useState(false);
  const [editId,setEditId]=useState<string|null>(null);
  const [busyId,setBusyId]=useState<string|null>(null);
  const [deleteRow,setDeleteRow]=useState<{id:string;prNumber:string}|null>(null);
  const sortItem=sort[0];
  const user=useSelector((s:RootState)=>s.auth.user);
  const {notify}=useNotification();
  const {data,isLoading,isFetching}=usePrsQuery({pageNumber:pagination.page+1,pageSize:pagination.pageSize,search:search||undefined,status:status?Number(status):undefined,sortBy:sortItem?.field,sortDirection:sortItem?.sort??undefined});
  const [create,{isLoading:creating}]=useCreatePrMutation();
  const [submit,{isLoading:submitting}]=useSubmitPrMutation();
  const [remove,{isLoading:deleting}]=useDeletePrMutation();

  useEffect(()=>{const timer=setTimeout(()=>{setSearch(searchText.trim());setPagination(p=>({...p,page:0}))},350);return()=>clearTimeout(timer)},[searchText]);

  const createPr=async(values:PurchaseRequestFormValues)=>{try{await create(values).unwrap();notify('Purchase request created successfully.');setCreateOpen(false)}catch(error){notify(getApiErrorMessage(error),'error')}};
  const mutate=async(kind:'submit'|'delete',id:string)=>{setBusyId(id);try{if(kind==='submit'){await submit(id).unwrap();notify('Purchase request submitted for approval.')}else{await remove(id).unwrap();notify('Purchase request deleted.');setDeleteRow(null)}}catch(error){notify(getApiErrorMessage(error),'error')}finally{setBusyId(null)}};

  const columns=useMemo<GridColDef[]>(()=>{
    const result:GridColDef[]=[
      {field:'prNumber',headerName:'PR Number',width:140,sortable:false,renderCell:p=><Button component={Link} to={`/purchase-requests/${p.row.id}`} size="small" sx={{fontWeight:700,p:0}}>{p.value}</Button>},
      {field:'description',headerName:'Description',flex:1,minWidth:180,sortable:false},
      {field:'vendorName',headerName:'Vendor',width:150,sortable:false},
      {field:'amount',headerName:'Amount',width:120,type:'number',renderCell:p=>new Intl.NumberFormat('en-IN',{style:'currency',currency:CURRENCIES[Number(p.row.currency)]||'INR'}).format(Number(p.value))},
      {field:'status',headerName:'Status',width:115,sortable:false,renderCell:p=><StatusChip status={Number(p.value)}/>},
      {field:'createdAt',headerName:'Created',width:125,valueFormatter:value=>new Date(String(value)).toLocaleDateString('en-GB',{day:'2-digit',month:'short',year:'numeric'})}
    ];
    if(user?.roles.includes('Requester'))result.push({field:'actions',headerName:'Actions',width:170,sortable:false,filterable:false,renderCell:p=>p.row.status===1?<Stack direction="row" alignItems="center" spacing={.25}><LoadingActionButton size="small" startIcon={<Send/>} loading={busyId===p.row.id&&submitting} disabled={!!busyId} onClick={()=>mutate('submit',p.row.id)}>Submit</LoadingActionButton><Tooltip title="Edit"><IconButton size="small" onClick={()=>setEditId(p.row.id)}><EditOutlined fontSize="small"/></IconButton></Tooltip><Tooltip title="Delete"><IconButton color="error" size="small" disabled={deleting} onClick={()=>setDeleteRow({id:p.row.id,prNumber:p.row.prNumber})}><DeleteOutline fontSize="small"/></IconButton></Tooltip></Stack>:null});
    return result;
  },[user,busyId,submitting,deleting]);

  return <>
    <PageHeader title="Purchase Requests" subtitle="Track and manage purchase requests across their lifecycle." action={user?.roles.includes('Requester')?<Button variant="contained" startIcon={<Add/>} onClick={()=>setCreateOpen(true)}>New Purchase Request</Button>:undefined}/>
    <Stack direction={{xs:'column',sm:'row'}} spacing={1.25} mb={1.5}>
      <TextField label="Search" placeholder="PR number, vendor, description" value={searchText} onChange={e=>setSearchText(e.target.value)} sx={{width:{xs:'100%',sm:250}}}/>
      <TextField select label="Status" value={status} onChange={e=>{setStatus(e.target.value);setPagination(p=>({...p,page:0}))}} sx={{width:{xs:'100%',sm:150}}}>
        <MenuItem value="">All statuses</MenuItem>{PR_STATUS.slice(1).map((name,index)=><MenuItem key={name} value={index+1}>{name}</MenuItem>)}
      </TextField>
    </Stack>
    <GridShell><DataGrid autoHeight rows={data?.items??[]} columns={columns} loading={isLoading||isFetching} rowCount={data?.totalCount??0} paginationMode="server" sortingMode="server" paginationModel={pagination} onPaginationModelChange={setPagination} pageSizeOptions={[10,25,50]} sortModel={sort} onSortModelChange={model=>{setSort(model);setPagination(p=>({...p,page:0}))}} disableRowSelectionOnClick localeText={{noRowsLabel:'No purchase requests found'}}/></GridShell>
    <Dialog open={createOpen} onClose={creating?undefined:()=>setCreateOpen(false)} fullWidth maxWidth="md">
      <DialogTitle>Create Purchase Request</DialogTitle>
      <DialogContent sx={{pb:2}}><PurchaseRequestForm loading={creating} submitLabel="Create Purchase Request" onSubmit={createPr} onCancel={()=>setCreateOpen(false)}/></DialogContent>
    </Dialog>
    <EditPurchaseRequestDialog id={editId} onClose={()=>setEditId(null)}/>
    <ConfirmDialog open={!!deleteRow} title="Delete draft purchase request?" message={`This will permanently delete ${deleteRow?.prNumber??'this request'}.`} confirmLabel="Delete" danger loading={deleting&&busyId===deleteRow?.id} onClose={()=>setDeleteRow(null)} onConfirm={()=>deleteRow&&mutate('delete',deleteRow.id)}/>
  </>;
}
