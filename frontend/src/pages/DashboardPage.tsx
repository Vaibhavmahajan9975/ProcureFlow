import {
  AssignmentTurnedIn,
  Description,
  LocalShipping,
  PendingActions,
  ReceiptLong,
  ShoppingCart,
  TrendingUp,
} from '@mui/icons-material';
import {Alert,Avatar,Box,Button,Card,CardContent,Divider,Grid,Paper,Skeleton,Stack,Typography,useTheme} from '@mui/material';
import {useSelector} from 'react-redux';
import {Link} from 'react-router-dom';
import {useApprovalsQuery,useDashboardQuery,usePosQuery,usePrsQuery} from '../api/baseApi';
import type {RootState} from '../app/store';
import type {PurchaseRequest} from '../types';
import {CURRENCIES,PageHeader,PR_STATUS,StatusChip} from '../components/common';

const statusPalette=['#98a2b3','#2e90fa','#12b76a','#f04438','#7f56d9','#f79009','#14b8a6'];

function DashboardPanel({children}:{children:React.ReactNode}){
  return <Paper variant="outlined" sx={{height:'100%',p:1.5,borderRadius:2.5,boxShadow:'0 6px 20px rgba(15,23,42,.04)',overflow:'hidden'}}>{children}</Paper>;
}

function PanelTitle({title,action}:{title:string;action?:React.ReactNode}){
  return <Stack direction="row" alignItems="center" justifyContent="space-between" mb={1}><Typography variant="h6">{title}</Typography>{action}</Stack>;
}

function relativeTime(value:string){
  const elapsed=Date.now()-new Date(value).getTime();
  const minutes=Math.max(0,Math.floor(elapsed/60000));
  if(minutes<1)return 'Just now';
  if(minutes<60)return `${minutes} min ago`;
  const hours=Math.floor(minutes/60);
  if(hours<24)return `${hours} hr ago`;
  const days=Math.floor(hours/24);
  if(days<7)return `${days} day${days===1?'':'s'} ago`;
  return new Date(value).toLocaleDateString('en-GB',{day:'2-digit',month:'short'});
}

function RecentRequestList({requests,loading}:{requests:PurchaseRequest[];loading:boolean}){
  if(loading)return <Stack spacing={.5}>{Array.from({length:5}).map((_,index)=><Skeleton key={index} height={34}/>)}</Stack>;
  if(!requests.length)return <Typography variant="body2" color="text.secondary" textAlign="center" py={4}>No purchase requests found.</Typography>;
  return <Box sx={{overflowX:'auto'}}><Box sx={{minWidth:660}}>
    <Box sx={{display:'grid',gridTemplateColumns:'145px 90px minmax(180px,1fr) 130px 105px',gap:1,px:1,py:.65,bgcolor:'action.hover'}}>
      {['PR Number','Date','Description','Amount','Status'].map(label=><Typography key={label} variant="caption" fontWeight={700}>{label}</Typography>)}
    </Box>
    <Box sx={{maxHeight:190,overflowY:'auto'}}>{requests.map(request=><Box key={request.id} sx={{display:'grid',gridTemplateColumns:'145px 90px minmax(180px,1fr) 130px 105px',gap:1,alignItems:'center',px:1,py:.7,borderBottom:'1px solid',borderColor:'divider','&:hover':{bgcolor:'action.hover'}}}>
      <Button component={Link} to={`/purchase-requests/${request.id}`} size="small" sx={{p:0,justifyContent:'flex-start',fontWeight:700}}>{request.prNumber}</Button>
      <Typography variant="caption" whiteSpace="nowrap">{new Date(request.createdAt).toLocaleDateString('en-GB',{day:'2-digit',month:'short'})}</Typography>
      <Typography variant="body2" noWrap>{request.description}</Typography>
      <Typography variant="body2" fontWeight={600} whiteSpace="nowrap">{new Intl.NumberFormat('en-IN',{style:'currency',currency:CURRENCIES[request.currency]||'INR'}).format(request.amount)}</Typography>
      <Box><StatusChip status={request.status}/></Box>
    </Box>)}</Box>
  </Box></Box>;
}

export default function DashboardPage(){
  const theme=useTheme();
  const user=useSelector((state:RootState)=>state.auth.user);
  const isApprover=!!user?.roles.includes('Approver');
  const isAdmin=!!user?.roles.includes('Admin');
  const {data,isLoading,error}=useDashboardQuery();
  const {data:requests,isLoading:requestsLoading}=usePrsQuery({pageNumber:1,pageSize:100,sortBy:'createdAt',sortDirection:'desc'});
  const {data:orders,isLoading:ordersLoading}=usePosQuery({pageNumber:1,pageSize:100});
  const {data:approvals,isLoading:approvalsLoading}=useApprovalsQuery({pageNumber:1,pageSize:4},{skip:!isApprover});
  const actionStatus=isAdmin?3:1;
  const {data:actionRequests,isLoading:actionLoading}=usePrsQuery({pageNumber:1,pageSize:4,status:actionStatus,sortBy:'createdAt',sortDirection:'desc'},{skip:isApprover});

  if(isLoading)return <Stack spacing={2}><Skeleton height={56}/><Skeleton variant="rounded" height={120}/><Skeleton variant="rounded" height={300}/></Stack>;
  if(error||!data)return <Alert severity="error">Unable to load dashboard.</Alert>;

  const cards=[
    {label:'Total Purchase Requests',value:data.totalPRs,icon:<ReceiptLong/>,color:'#2e90fa',bg:'#eff8ff',note:'Across the workflow'},
    {label:'Pending Approval',value:data.submitted,icon:<PendingActions/>,color:'#f79009',bg:'#fffaeb',note:'Submitted requests'},
    {label:'Total Purchase Orders',value:data.totalPOs,icon:<ShoppingCart/>,color:'#12b76a',bg:'#ecfdf3',note:'Orders created'},
    {label:'Pending Delivery',value:data.poCreated,icon:<LocalShipping/>,color:'#7f56d9',bg:'#f4f3ff',note:'Awaiting delivery'},
    {label:'Completed Requests',value:data.completed,icon:<AssignmentTurnedIn/>,color:'#0e9384',bg:'#f0fdfa',note:'Fully completed'},
  ];
  const statuses=[data.draft,data.submitted,data.approved,data.rejected,data.poCreated,data.delivered,data.completed];
  const total=Math.max(data.totalPRs,1);
  let running=0;
  const stops=statuses.map((value,index)=>{const start=running;running+=value/total*100;return `${statusPalette[index]} ${start}% ${running}%`;}).join(', ');
  const queue=isApprover?approvals:actionRequests;
  const queueLoading=isApprover?approvalsLoading:actionLoading;
  const queueTitle=isApprover?'Pending Approvals':isAdmin?'Ready for Purchase Order':'Draft Requests';
  const queueLink=isApprover?'/approvals':isAdmin?'/purchase-orders':'/purchase-requests';
  const recentRequests=requests?.items.slice(0,5)??[];
  const today=new Intl.DateTimeFormat('en-GB',{weekday:'short',day:'2-digit',month:'short',year:'numeric'}).format(new Date());

  const current=new Date();
  const months=Array.from({length:6},(_,index)=>{
    const date=new Date(current.getFullYear(),current.getMonth()-5+index,1);
    const year=date.getFullYear(),month=date.getMonth();
    return {
      label:date.toLocaleDateString('en-GB',{month:'short'}),
      prs:requests?.items.filter(item=>{const created=new Date(item.createdAt);return created.getFullYear()===year&&created.getMonth()===month;}).length??0,
      pos:orders?.items.filter(item=>{const ordered=new Date(item.orderDate);return ordered.getFullYear()===year&&ordered.getMonth()===month;}).length??0,
    };
  });
  const trendMax=Math.max(1,...months.flatMap(month=>[month.prs,month.pos]));

  const activity=(requests?.items.map(item=>({id:item.id,number:item.prNumber,text:`Current status: ${PR_STATUS[item.status]}`,date:item.lastActivityAt||item.updatedAt||item.createdAt,color:statusPalette[Math.max(0,item.status-1)]}))??[])
    .sort((a,b)=>new Date(b.date).getTime()-new Date(a.date).getTime()).slice(0,5);

  return <>
    <PageHeader title="Dashboard" subtitle="Overview of your purchase-to-pay activities" action={<Stack direction="row" alignItems="center" spacing={.75} color="text.secondary"><TrendingUp sx={{fontSize:17}}/><Typography variant="caption" fontWeight={600}>{today}</Typography></Stack>}/>

    <Box sx={{display:'grid',gridTemplateColumns:{xs:'1fr',sm:'repeat(2,minmax(0,1fr))',md:'repeat(3,minmax(0,1fr))',lg:'repeat(5,minmax(0,1fr))'},gap:1.5,mb:1.5}}>
      {cards.map(card=><Card key={card.label} variant="outlined" sx={{height:'100%',boxShadow:'0 5px 16px rgba(15,23,42,.04)'}}><CardContent sx={{p:'14px!important'}}><Stack direction="row" spacing={1.25} alignItems="flex-start"><Avatar variant="rounded" sx={{width:38,height:38,color:card.color,bgcolor:card.bg}}>{card.icon}</Avatar><Box minWidth={0}><Typography variant="caption" color="text.secondary" fontWeight={600} noWrap>{card.label}</Typography><Typography variant="h4" mt={.2}>{card.value.toLocaleString()}</Typography><Typography variant="caption" color="text.secondary">{card.note}</Typography></Box></Stack></CardContent></Card>)}
    </Box>

    <Grid container spacing={1.5} mb={1.5} alignItems="stretch">
      <Grid item xs={12} md={4}>
        <DashboardPanel><PanelTitle title="Purchase Requests by Status"/><Box sx={{display:'grid',gridTemplateColumns:{xs:'120px minmax(0,1fr)',md:'1fr',lg:'120px minmax(0,1fr)'},alignItems:'center',justifyItems:'center',gap:1}}>
          <Box role="img" aria-label={`${data.totalPRs} total purchase requests`} sx={{position:'relative',width:120,height:120,minWidth:120,aspectRatio:'1 / 1',borderRadius:'50%',background:data.totalPRs?`conic-gradient(${stops})`:theme.palette.action.disabledBackground,'&:after':{content:'""',position:'absolute',inset:24,borderRadius:'50%',bgcolor:'background.paper'}}}><Stack sx={{position:'absolute',inset:0,zIndex:1}} alignItems="center" justifyContent="center"><Typography variant="h4">{data.totalPRs}</Typography><Typography variant="caption" color="text.secondary">Total</Typography></Stack></Box>
          <Stack spacing={.35} width="100%">{statuses.map((value,index)=><Stack key={PR_STATUS[index+1]} direction="row" alignItems="center" spacing={.6}><Box sx={{width:8,height:8,borderRadius:'50%',bgcolor:statusPalette[index],flexShrink:0}}/><Typography variant="caption" noWrap sx={{flex:1,minWidth:0}}>{PR_STATUS[index+1]}</Typography><Typography variant="caption" fontWeight={700} width={20} textAlign="right">{value}</Typography><Typography variant="caption" color="text.secondary" width={34} textAlign="right">{data.totalPRs?Math.round(value/data.totalPRs*100):0}%</Typography></Stack>)}</Stack>
        </Box></DashboardPanel>
      </Grid>

      <Grid item xs={12} md={5}>
        <DashboardPanel><PanelTitle title="Monthly Trend" action={<Stack direction="row" spacing={1.25}><Typography variant="caption"><Box component="span" sx={{display:'inline-block',width:8,height:8,borderRadius:.5,bgcolor:'#2e90fa',mr:.5}}/>PRs</Typography><Typography variant="caption"><Box component="span" sx={{display:'inline-block',width:8,height:8,borderRadius:.5,bgcolor:'#12b76a',mr:.5}}/>POs</Typography></Stack>}/>
          {requestsLoading||ordersLoading?<Skeleton variant="rounded" height={160}/>:<Box sx={{height:172,display:'flex',alignItems:'flex-end',gap:{xs:1,sm:2},px:1,borderBottom:'1px solid',borderColor:'divider'}}>{months.map(month=><Stack key={month.label} alignItems="center" justifyContent="flex-end" height="100%" flex={1} minWidth={0}><Stack direction="row" alignItems="flex-end" justifyContent="center" spacing={.35} flex={1} width="100%"><TooltipBar value={month.prs} max={trendMax} color="#2e90fa"/><TooltipBar value={month.pos} max={trendMax} color="#12b76a"/></Stack><Typography variant="caption" color="text.secondary" py={.4}>{month.label}</Typography></Stack>)}</Box>}
        </DashboardPanel>
      </Grid>

      <Grid item xs={12} md={3}>
        <DashboardPanel><PanelTitle title={queueTitle} action={<Button component={Link} to={queueLink} size="small">View all</Button>}/>{queueLoading?<Stack spacing={.5}>{Array.from({length:4}).map((_,index)=><Skeleton key={index} height={40}/>)}</Stack>:queue?.items.length?<Stack divider={<Divider flexItem/>} sx={{maxHeight:190,overflowY:'auto'}}>{queue.items.map(request=><Stack key={request.id} component={Link} to={`/purchase-requests/${request.id}`} direction="row" spacing={1} alignItems="center" sx={{py:.65,color:'inherit',textDecoration:'none'}}><Avatar variant="rounded" sx={{width:28,height:28,bgcolor:'primary.main'}}><Description sx={{fontSize:16}}/></Avatar><Box minWidth={0} flex={1}><Typography variant="caption" fontWeight={700} noWrap display="block">{request.prNumber}</Typography><Typography variant="caption" color="text.secondary" noWrap display="block">{request.vendorName}</Typography></Box><Typography variant="caption" fontWeight={700} whiteSpace="nowrap">{CURRENCIES[request.currency]} {request.amount.toLocaleString()}</Typography></Stack>)}</Stack>:<Stack alignItems="center" justifyContent="center" height={165}><AssignmentTurnedIn color="disabled"/><Typography variant="caption" color="text.secondary" mt={.5}>No requests need attention</Typography></Stack>}</DashboardPanel>
      </Grid>
    </Grid>

    <Grid container spacing={1.5} pb={1.5} alignItems="stretch">
      <Grid item xs={12} md={8}>
        <DashboardPanel><PanelTitle title="Recent Purchase Requests" action={<Button component={Link} to="/purchase-requests" size="small">View all</Button>}/><RecentRequestList requests={recentRequests} loading={requestsLoading}/></DashboardPanel>
      </Grid>
      <Grid item xs={12} md={4}>
        <DashboardPanel><PanelTitle title="Recent Activity"/>{requestsLoading||ordersLoading?<Stack spacing={.5}>{Array.from({length:5}).map((_,index)=><Skeleton key={index} height={32}/>)}</Stack>:activity.length?<Stack sx={{maxHeight:190,overflowY:'auto'}}>{activity.map((item,index)=><Stack key={item.id} direction="row" spacing={1} sx={{position:'relative',pb:index===activity.length-1?0:1.1,'&:after':index===activity.length-1?undefined:{content:'""',position:'absolute',left:4,top:14,bottom:0,borderLeft:'1px solid',borderColor:'divider'}}}><Box sx={{width:9,height:9,borderRadius:'50%',bgcolor:item.color,mt:.55,zIndex:1,flexShrink:0}}/><Box minWidth={0} flex={1}><Typography variant="caption" color="primary.main" fontWeight={700}>{item.number}</Typography><Typography variant="caption" color="text.secondary" display="block" noWrap>{item.text}</Typography></Box><Typography variant="caption" color="text.secondary" whiteSpace="nowrap">{relativeTime(item.date)}</Typography></Stack>)}</Stack>:<Typography variant="caption" color="text.secondary">No recent activity.</Typography>}</DashboardPanel>
      </Grid>
    </Grid>
  </>;
}

function TooltipBar({value,max,color}:{value:number;max:number;color:string}){
  return <Box title={`${value}`} sx={{width:{xs:10,sm:16},height:value?Math.max(8,value/max*125):2,bgcolor:value?color:'action.disabledBackground',borderRadius:'3px 3px 0 0',transition:'height .2s'}}/>;
}
