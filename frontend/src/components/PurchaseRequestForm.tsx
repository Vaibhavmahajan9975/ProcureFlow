import {Box,Card,CardContent,MenuItem,Stack,TextField,Typography} from '@mui/material';
import {Controller,useForm} from 'react-hook-form';
import {z} from 'zod';
import {zodResolver} from '@hookform/resolvers/zod';
import {useCategoriesQuery,useDepartmentsQuery,useVendorsQuery} from '../api/baseApi';
import type {PurchaseRequestInput} from '../types';
import {CURRENCIES,LoadingActionButton} from './common';
import {useNotification} from './NotificationProvider';

const today=new Date().toISOString().slice(0,10);
const schema=z.object({
  departmentId:z.string().min(1,'Department is required.'),
  vendorId:z.string().min(1,'Vendor is required.'),
  categoryId:z.string().min(1,'Category is required.'),
  description:z.string().trim().min(3,'Enter at least 3 characters.').max(1000,'Description cannot exceed 1000 characters.'),
  amount:z.coerce.number().positive('Amount must be greater than zero.'),
  currency:z.coerce.number().min(1,'Currency is required.'),
  requiredDate:z.string().min(1,'Required date is required.').refine(value=>value>=today,'Required Date cannot be earlier than today.')
});

export type PurchaseRequestFormValues=z.infer<typeof schema>;
export const emptyPurchaseRequest:PurchaseRequestFormValues={departmentId:'',vendorId:'',categoryId:'',description:'',amount:0,currency:1,requiredDate:today};

export default function PurchaseRequestForm({defaultValues=emptyPurchaseRequest,loading,submitLabel,onSubmit,onCancel}:{defaultValues?:PurchaseRequestInput;loading:boolean;submitLabel:string;onSubmit:(values:PurchaseRequestFormValues)=>Promise<void>;onCancel:()=>void}){
  const {data:departments,isLoading:dLoading}=useDepartmentsQuery();
  const {data:categories,isLoading:cLoading}=useCategoriesQuery();
  const {data:vendors,isLoading:vLoading}=useVendorsQuery();
  const {notify}=useNotification();
  const {control,handleSubmit,formState:{errors}}=useForm<PurchaseRequestFormValues>({resolver:zodResolver(schema),defaultValues});
  const selects=[
    {name:'departmentId' as const,label:'Department',items:departments,loading:dLoading},
    {name:'categoryId' as const,label:'Category',items:categories,loading:cLoading},
    {name:'vendorId' as const,label:'Vendor',items:vendors,loading:vLoading}
  ];
  return <Card variant="outlined" sx={{width:'100%',maxWidth:820,mx:'auto',boxShadow:'0 6px 20px rgba(15,23,42,.05)'}}>
    <CardContent sx={{p:{xs:2,sm:3},'&:last-child':{pb:{xs:2,sm:3}}}}>
      <Box component="form" onSubmit={handleSubmit(onSubmit,()=>notify('Please correct the highlighted fields.','warning'))} noValidate>
        <Typography variant="caption" color="error.main" display="block" textAlign="right" mb={1}>* Required fields</Typography>
        <Box sx={{display:'grid',gridTemplateColumns:{xs:'1fr',md:'1fr 1fr'},gap:2}}>
          {selects.map(x=><Controller key={x.name} control={control} name={x.name} render={({field})=><TextField {...field} required select fullWidth label={x.label} disabled={x.loading} error={!!errors[x.name]} helperText={errors[x.name]?.message}><MenuItem value=""><em>Select {x.label.toLowerCase()}</em></MenuItem>{x.items?.map(item=><MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField>}/>)}
          <Controller control={control} name="requiredDate" render={({field})=><TextField {...field} required type="date" fullWidth label="Required Date" InputLabelProps={{shrink:true}} inputProps={{min:today}} error={!!errors.requiredDate} helperText={errors.requiredDate?.message}/>}/>
          <Controller control={control} name="amount" render={({field})=><TextField {...field} required type="number" fullWidth label="Amount" inputProps={{min:0,step:'.01'}} error={!!errors.amount} helperText={errors.amount?.message}/>}/>
          <Controller control={control} name="currency" render={({field})=><TextField {...field} required select fullWidth label="Currency" error={!!errors.currency} helperText={errors.currency?.message}>{CURRENCIES.slice(1).map((name,index)=><MenuItem key={name} value={index+1}>{name}</MenuItem>)}</TextField>}/>
          <Controller control={control} name="description" render={({field})=><TextField {...field} required sx={{gridColumn:{md:'1 / -1'}}} fullWidth label="Description" multiline minRows={3} error={!!errors.description} helperText={errors.description?.message??'Describe the business need and requested items.'}/>}/>
        </Box>
        <Stack direction="row" justifyContent="flex-end" spacing={1} mt={2.5}><LoadingActionButton variant="outlined" onClick={onCancel} disabled={loading}>Cancel</LoadingActionButton><LoadingActionButton type="submit" variant="contained" loading={loading}>{submitLabel}</LoadingActionButton></Stack>
      </Box>
    </CardContent>
  </Card>;
}
