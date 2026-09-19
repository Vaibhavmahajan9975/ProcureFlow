import {Approval,Brightness4,Brightness7,Dashboard,LocalShipping,Logout,Menu,MenuOpen,ReceiptLong,ShoppingCart} from '@mui/icons-material';
import {AppBar,Avatar,Box,Drawer,IconButton,List,ListItemButton,ListItemIcon,ListItemText,Stack,Toolbar,Tooltip,Typography,useMediaQuery,useTheme} from '@mui/material';
import {useState} from 'react';
import {useDispatch,useSelector} from 'react-redux';
import {Link,Outlet,useLocation} from 'react-router-dom';
import {logout,RootState} from '../app/store';
import {useColorMode} from './ColorModeProvider';

const expandedWidth=256,collapsedWidth=76;
const drawerBackground='linear-gradient(180deg,#162a48 0%,#0e1b30 100%)';

export default function Layout(){
  const user=useSelector((state:RootState)=>state.auth.user);
  const dispatch=useDispatch();
  const location=useLocation();
  const theme=useTheme();
  const {mode,toggle}=useColorMode();
  const mobile=useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen,setMobileOpen]=useState(false);
  const [collapsed,setCollapsed]=useState(false);
  const compact=!mobile&&collapsed;
  const width=compact?collapsedWidth:expandedWidth;
  const initials=user?.fullName.split(/\s+/).map(part=>part[0]).join('').slice(0,2).toUpperCase()||'PF';
  const items=[
    {path:'/',label:'Dashboard',icon:<Dashboard/>},
    {path:'/purchase-requests',label:'Purchase Requests',icon:<ReceiptLong/>},
    ...(user?.roles.includes('Approver')?[{path:'/approvals',label:'Approvals',icon:<Approval/>}]:[]),
    ...(user?.roles.includes('Admin')?[{path:'/purchase-orders',label:'Purchase Orders',icon:<ShoppingCart/>},{path:'/deliveries',label:'Deliveries',icon:<LocalShipping/>}]:[]),
  ];
  const drawer=<>
    <Toolbar sx={{px:compact?1.25:2,justifyContent:compact?'center':'flex-start',minHeight:'60px!important'}}>
      <Stack direction="row" alignItems="center" spacing={1}>
        <Avatar variant="rounded" sx={{bgcolor:'#2e90fa',color:'#fff',width:34,height:34,fontWeight:800}}>P</Avatar>
        {!compact&&<Box><Typography color="#fff" fontWeight={800} fontSize={17} lineHeight={1.1}>ProcureFlow</Typography><Typography variant="caption" sx={{color:'rgba(255,255,255,.62)'}}>Purchase to Pay</Typography></Box>}
      </Stack>
    </Toolbar>
    <List sx={{px:1,py:1.5}}>{items.map(item=>{
      const selected=item.path==='/'?location.pathname==='/':location.pathname.startsWith(item.path);
      return <Tooltip title={compact?item.label:''} placement="right" key={item.path}>
        <ListItemButton component={Link} to={item.path} selected={selected} onClick={()=>setMobileOpen(false)} sx={{mb:.5,minHeight:42,borderRadius:1.5,color:'rgba(255,255,255,.78)',justifyContent:compact?'center':'flex-start','&.Mui-selected':{bgcolor:'#2878d4',color:'#fff','&:hover':{bgcolor:'#2878d4'}},'&:hover':{bgcolor:'rgba(255,255,255,.08)',color:'#fff'}}}>
          <ListItemIcon sx={{minWidth:compact?0:38,color:'inherit',justifyContent:'center'}}>{item.icon}</ListItemIcon>
          {!compact&&<ListItemText primary={item.label} primaryTypographyProps={{fontWeight:selected?700:500,fontSize:13}}/>}
        </ListItemButton>
      </Tooltip>;
    })}</List>
  </>;
  const drawerPaper={width,boxSizing:'border-box',borderRight:0,bgcolor:'#12233d',backgroundImage:drawerBackground,transition:theme.transitions.create('width'),overflowX:'hidden'} as const;

  return <Box sx={{display:'flex',minHeight:'100vh'}}>
    <AppBar position="fixed" color="inherit" elevation={0} sx={{zIndex:theme.zIndex.drawer+1,borderBottom:'1px solid',borderColor:'divider',ml:{md:`${width}px`},width:{md:`calc(100% - ${width}px)`},transition:theme.transitions.create(['width','margin'])}}>
      <Toolbar sx={{minHeight:'60px!important',px:{xs:1.5,sm:2}}}>
        <IconButton edge="start" size="small" aria-label="Toggle navigation" onClick={()=>mobile?setMobileOpen(true):setCollapsed(value=>!value)} sx={{mr:1.5}}>{mobile?<Menu/>:collapsed?<Menu/>:<MenuOpen/>}</IconButton>
        <Box sx={{flexGrow:1}}/>
        <Stack direction="row" spacing={1} alignItems="center">
          <Tooltip title={`Switch to ${mode==='light'?'dark':'light'} mode`}><IconButton size="small" color="inherit" onClick={toggle}>{mode==='light'?<Brightness4 fontSize="small"/>:<Brightness7 fontSize="small"/>}</IconButton></Tooltip>
          <Avatar sx={{width:30,height:30,bgcolor:'primary.main',fontSize:12,fontWeight:700}}>{initials}</Avatar>
          <Box sx={{display:{xs:'none',sm:'block'}}}><Typography variant="body2" fontWeight={700}>{user?.fullName}</Typography><Typography variant="caption" color="text.secondary">{user?.roles.join(' · ')}</Typography></Box>
          <Tooltip title="Logout"><IconButton size="small" color="inherit" onClick={()=>dispatch(logout())}><Logout fontSize="small"/></IconButton></Tooltip>
        </Stack>
      </Toolbar>
    </AppBar>
    <Drawer variant="temporary" open={mobileOpen} onClose={()=>setMobileOpen(false)} ModalProps={{keepMounted:true}} sx={{display:{xs:'block',md:'none'},'& .MuiDrawer-paper':{...drawerPaper,width:expandedWidth}}}>{drawer}</Drawer>
    <Drawer variant="permanent" sx={{display:{xs:'none',md:'block'},width,flexShrink:0,transition:theme.transitions.create('width'),'& .MuiDrawer-paper':drawerPaper}}>{drawer}</Drawer>
    <Box component="main" sx={{flexGrow:1,minWidth:0,height:'calc(100dvh - 60px)',p:{xs:1.25,sm:1.75,lg:2},mt:'60px',overflow:'auto'}}><Box sx={{maxWidth:1440,mx:'auto',width:'100%',height:'100%',minHeight:0,display:'flex',flexDirection:'column'}}><Outlet/></Box></Box>
  </Box>;
}
