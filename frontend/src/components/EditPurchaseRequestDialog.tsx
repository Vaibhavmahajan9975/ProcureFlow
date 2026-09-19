import {Alert,Dialog,DialogContent,DialogTitle} from '@mui/material';
import {usePrQuery,useUpdatePrMutation} from '../api/baseApi';
import PurchaseRequestForm,{PurchaseRequestFormValues} from './PurchaseRequestForm';
import {FullPageLoader} from './common';
import {useNotification} from './NotificationProvider';
import {getApiErrorMessage} from '../utils/apiError';

export default function EditPurchaseRequestDialog({id,onClose}:{id:string|null;onClose:()=>void}){
  const {data,isLoading,error}=usePrQuery(id??'',{skip:!id});
  const [update,{isLoading:saving}]=useUpdatePrMutation();
  const {notify}=useNotification();
  const submit=async(values:PurchaseRequestFormValues)=>{if(!id)return;try{await update({id,body:values}).unwrap();notify('Purchase request updated successfully.');onClose()}catch(e){notify(getApiErrorMessage(e),'error')}};
  const defaults=data?{departmentId:data.departmentId,vendorId:data.vendorId,categoryId:data.categoryId,description:data.description,amount:data.amount,currency:data.currency,requiredDate:data.requiredDate}:undefined;
  return <Dialog open={!!id} onClose={saving?undefined:onClose} fullWidth maxWidth="md">
    <DialogTitle>Edit {data?.prNumber??'Purchase Request'}</DialogTitle>
    <DialogContent sx={{pb:2}}>
      {isLoading&&<FullPageLoader/>}
      {error&&<Alert severity="error">Unable to load this purchase request.</Alert>}
      {data&&data.status!==1&&<Alert severity="warning">Only draft purchase requests can be edited.</Alert>}
      {data&&data.status===1&&defaults&&<PurchaseRequestForm key={data.id} defaultValues={defaults} loading={saving} submitLabel="Save Changes" onSubmit={submit} onCancel={onClose}/>} 
    </DialogContent>
  </Dialog>;
}
