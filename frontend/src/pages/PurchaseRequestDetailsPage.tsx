import {ArrowBack,Delete,Edit} from '@mui/icons-material';
import {Alert,Box,Button,Card,CardContent,Divider,Stack,Typography} from '@mui/material';
import {useState} from 'react';
import {useSelector} from 'react-redux';
import {Link,useNavigate,useParams} from 'react-router-dom';
import {useDeletePrMutation,usePrQuery} from '../api/baseApi';
import type {RootState} from '../app/store';
import {CURRENCIES,ConfirmDialog,FullPageLoader,PageHeader,PR_STATUS,StatusChip} from '../components/common';
import {useNotification} from '../components/NotificationProvider';
import {getApiErrorMessage} from '../utils/apiError';
import EditPurchaseRequestDialog from '../components/EditPurchaseRequestDialog';

const formatDate=(value?:string,withTime=false)=>value?new Date(value).toLocaleDateString('en-GB',withTime?{day:'2-digit',month:'short',year:'numeric',hour:'2-digit',minute:'2-digit'}:{day:'2-digit',month:'short',year:'numeric'}):'—';
function Detail({label,value}:{label:string;value:React.ReactNode}){return <Box><Typography variant="caption" color="text.secondary" fontWeight={600} textTransform="uppercase" letterSpacing=".04em">{label}</Typography><Typography variant="body2" mt={.35} fontWeight={500}>{value??'—'}</Typography></Box>}

export default function PurchaseRequestDetailsPage(){
  const {id=''}=useParams();
  const navigate=useNavigate();
  const user=useSelector((s:RootState)=>s.auth.user);
  const {data,isLoading,error}=usePrQuery(id,{skip:!id});
  const [remove,{isLoading:deleting}]=useDeletePrMutation();
  const [confirm,setConfirm]=useState(false);
  const [editOpen,setEditOpen]=useState(false);
  const {notify}=useNotification();
  if(isLoading)return <FullPageLoader skeleton/>;
  if(error||!data)return <Alert severity="error" action={<Button component={Link} to="/purchase-requests">Back to list</Button>}>Unable to load this purchase request.</Alert>;

  const canEdit=data.status===1&&!!user?.roles.includes('Requester');
  const hasSidebar=(data.statusHistory?.length??0)>0||!!data.purchaseOrder;
  const deletePr=async()=>{try{await remove(id).unwrap();notify('Purchase request deleted.');navigate('/purchase-requests')}catch(e){notify(getApiErrorMessage(e),'error')}};

  return <>
    <Button component={Link} to="/purchase-requests" startIcon={<ArrowBack/>} sx={{mb:1}}>Purchase Requests</Button>
    <PageHeader title={data.prNumber} subtitle={`Created ${formatDate(data.createdAt,true)}`} action={<Stack direction="row" spacing={1}>{canEdit&&<Button variant="outlined" startIcon={<Edit/>} onClick={()=>setEditOpen(true)}>Edit</Button>}{canEdit&&<Button color="error" variant="outlined" startIcon={<Delete/>} onClick={()=>setConfirm(true)}>Delete</Button>}</Stack>}/>
    <Box sx={{display:'grid',gridTemplateColumns:{xs:'1fr',lg:hasSidebar?'minmax(0,2fr) minmax(280px,1fr)':'1fr'},gap:2}}>
      <Card variant="outlined">
        <CardContent sx={{p:{xs:2,sm:2.5}}}>
          <Stack direction="row" justifyContent="space-between" mb={2.5}><Typography variant="h6">Request details</Typography><StatusChip status={data.status}/></Stack>
          <Box sx={{display:'grid',gridTemplateColumns:{xs:'1fr',sm:'1fr 1fr',lg:hasSidebar?'1fr 1fr':'repeat(3,1fr)'},gap:2.5}}>
            <Detail label="Requester" value={data.requesterName}/><Detail label="Department" value={data.departmentName}/><Detail label="Category" value={data.categoryName}/><Detail label="Vendor" value={data.vendorName}/><Detail label="Amount" value={new Intl.NumberFormat('en-IN',{style:'currency',currency:CURRENCIES[data.currency]??'INR'}).format(data.amount)}/><Detail label="Required Date" value={formatDate(data.requiredDate)}/>
            <Box sx={{gridColumn:'1 / -1'}}><Detail label="Description" value={data.description}/></Box>
            {data.status===4&&<Box sx={{gridColumn:'1 / -1'}}><Alert severity="error"><strong>Rejection reason:</strong> {data.rejectionReason||'No reason provided.'}</Alert></Box>}
          </Box>
        </CardContent>
      </Card>
      {hasSidebar&&<Stack spacing={2}>
        {data.statusHistory?.length>0&&<Card variant="outlined"><CardContent><Typography variant="h6" mb={2}>Status history</Typography><Stack>{data.statusHistory.map((item,index)=><Stack key={item.id} direction="row" spacing={1.5}><Box sx={{display:'flex',flexDirection:'column',alignItems:'center'}}><Box sx={{width:10,height:10,borderRadius:'50%',bgcolor:'primary.main',mt:.65}}/>{index<data.statusHistory.length-1&&<Box sx={{width:2,minHeight:48,bgcolor:'divider'}}/>}</Box><Box pb={1.5}><Typography variant="body2" fontWeight={650}>{PR_STATUS[item.toStatus]}</Typography><Typography variant="caption" color="text.secondary">{formatDate(item.changedAt,true)}{item.changedBy?` · ${item.changedBy}`:''}</Typography>{item.comment&&<Typography variant="body2" mt={.25}>{item.comment}</Typography>}</Box></Stack>)}</Stack></CardContent></Card>}
        {data.purchaseOrder&&<Card variant="outlined"><CardContent><Typography variant="h6">Purchase order</Typography><Divider sx={{my:1.5}}/><Stack spacing={1.5}><Detail label="PO Number" value={data.purchaseOrder.poNumber}/><Detail label="Order Date" value={formatDate(data.purchaseOrder.orderDate)}/><Detail label="Status" value={<StatusChip status={data.purchaseOrder.status} type="po"/>}/>{data.delivery&&<><Divider/><Detail label="Delivery Date" value={formatDate(data.delivery.deliveryDate)}/><Detail label="Delivery Notes" value={data.delivery.notes||'—'}/></>}</Stack></CardContent></Card>}
      </Stack>}
    </Box>
    <ConfirmDialog open={confirm} title="Delete draft purchase request?" message={`This will permanently delete ${data.prNumber}. This action cannot be undone.`} confirmLabel="Delete" danger loading={deleting} onClose={()=>setConfirm(false)} onConfirm={deletePr}/>
    <EditPurchaseRequestDialog id={editOpen?id:null} onClose={()=>setEditOpen(false)}/>
  </>;
}
