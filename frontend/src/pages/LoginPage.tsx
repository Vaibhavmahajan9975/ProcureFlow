import {ArrowForward,LockOutlined,ReceiptLong} from '@mui/icons-material';
import {Avatar,Box,Card,CardContent,Stack,TextField,Typography} from '@mui/material';
import {useState} from 'react';
import {useDispatch} from 'react-redux';
import {useNavigate} from 'react-router-dom';
import {useLoginMutation} from '../api/baseApi';
import {setCredentials} from '../app/store';
import {LoadingActionButton} from '../components/common';
import {useNotification} from '../components/NotificationProvider';
import {getApiErrorMessage} from '../utils/apiError';

export default function LoginPage(){
  const [email,setEmail]=useState('requester@procureflow.demo');
  const [password,setPassword]=useState('Demo@123');
  const [login,{isLoading}]=useLoginMutation();
  const dispatch=useDispatch();
  const navigate=useNavigate();
  const {notify}=useNotification();
  const submit=async(e:React.FormEvent)=>{e.preventDefault();if(!email||!password){notify('Email and password are required.','warning');return}try{const result=await login({email,password}).unwrap();dispatch(setCredentials(result));notify(`Welcome back, ${result.fullName}.`);navigate('/')}catch(error){notify(getApiErrorMessage(error),'error')}};
  return <Box sx={theme=>({minHeight:'100vh',display:'grid',placeItems:'center',p:2,background:theme.palette.mode==='light'?'radial-gradient(circle at top left,#dbeafe 0,transparent 38%),linear-gradient(135deg,#f8fafc,#eef4ff)':'radial-gradient(circle at top left,#1e3a5f 0,transparent 38%),linear-gradient(135deg,#0b1120,#111b31)'})}>
    <Card sx={{width:'100%',maxWidth:430,border:'1px solid',borderColor:'divider',boxShadow:'0 24px 70px rgba(15,23,42,.18)'}}>
      <CardContent sx={{p:{xs:3,sm:4.5},'&:last-child':{pb:{xs:3,sm:4.5}}}}>
        <Stack direction="row" alignItems="center" spacing={1.25} mb={4}><Avatar variant="rounded" sx={{bgcolor:'primary.main',color:'primary.contrastText',width:42,height:42,fontWeight:900}}>P</Avatar><Typography variant="h5" fontWeight={800}>ProcureFlow</Typography></Stack>
        <Typography variant="h4" fontWeight={800}>Welcome back</Typography>
        <Typography variant="body2" color="text.secondary" mt={.5} mb={3}>Sign in to continue.</Typography>
        <Box component="form" onSubmit={submit} noValidate>
          <Stack spacing={2}>
            <TextField required fullWidth label="Email address" autoComplete="email" value={email} onChange={e=>setEmail(e.target.value)} InputProps={{startAdornment:<ReceiptLong color="action" fontSize="small" sx={{mr:1}}/>}}/>
            <TextField required fullWidth label="Password" type="password" autoComplete="current-password" value={password} onChange={e=>setPassword(e.target.value)} InputProps={{startAdornment:<LockOutlined color="action" fontSize="small" sx={{mr:1}}/>}}/>
            <LoadingActionButton type="submit" fullWidth variant="contained" size="large" endIcon={!isLoading?<ArrowForward/>:undefined} sx={{mt:1,height:40}} loading={isLoading}>Sign in</LoadingActionButton>
          </Stack>
        </Box>
      </CardContent>
    </Card>
  </Box>;
}
