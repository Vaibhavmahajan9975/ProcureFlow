import {AccountCircle,Approval,Brightness4,Brightness7,Dashboard,LocalShipping,Logout,Menu,MenuOpen,ReceiptLong,ShoppingCart} from '@mui/icons-material';
import {AppBar,Avatar,Box,Drawer,IconButton,List,ListItemButton,ListItemIcon,ListItemText,Stack,Toolbar,Tooltip,Typography,useMediaQuery,useTheme} from '@mui/material';
import {useState} from 'react';
import {useDispatch,useSelector} from 'react-redux';
import {Link,Outlet,useLocation} from 'react-router-dom';
import {logout,RootState} from '../app/store';
import {useColorMode} from './ColorModeProvider';

const expandedWidth=256,collapsedWidth=76;

export default function Layout(){
  const user=useSelector((s:RootState)=>s.auth.user);
  const dispatch=useDispatch();
  const location=useLocation();
  const theme=useTheme();
  const {mode,toggle}=useColorMode();
  const mobile=useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen,setMobileOpen]=useState(false);
  const [collapsed,setCollapsed]=useState(false);
  const compact=!mobile&&collapsed;
  const width=compact?collapsedWidth:expandedWidth;
  const items=[
    {path:'/',label:'Dashboard',icon:<Dashboard/>},
    {path:'/purchase-requests',label:'Purchase Requests',icon:<ReceiptLong/>},
    ...(user?.roles.includes('Approver')?[{path:'/approvals',label:'Approvals',icon:<Approval/>}]:[]),
    ...(user?.roles.includes('Admin')?[{path:'/purchase-orders',label:'Purchase Orders',icon:<ShoppingCart/>},{path:'/deliveries',label:'Deliveries',icon:<LocalShipping/>}]:[])
  ];
  const drawer=<>
    <Toolbar sx={{px:compact?1.25:2,justifyContent:compact?'center':'flex-start',minHeight:'60px!important'}}>
      <Stack direction="row" alignItems="center" spacing={1}><Avatar variant="rounded" sx={{bgcolor:'primary.main',color:'primary.contrastText',width:34,height:34,fontWeight:800}}>P</Avatar>{!compact&&<Box><Typography fontWeight={800} fontSize={17} lineHeight={1.1}>ProcureFlow</Typography><Typography variant="caption" color="text.secondary">Procurement workspace</Typography></Box>}</Stack>
    </Toolbar>
    <List sx={{px:1,py:1.5}}>{items.map(item=>{const selected=item.path==='/'?location.pathname==='/':location.pathname.startsWith(item.path);return <Tooltip title={compact?item.label:''} placement="right" key={item.path}><ListItemButton component={Link} to={item.path} selected={selected} onClick={()=>setMobileOpen(false)} sx={{mb:.5,minHeight:42,borderRadius:2,justifyContent:compact?'center':'flex-start','&.Mui-selected':{bgcolor:'action.selected',color:'primary.main','&:hover':{bgcolor:'action.selected'}},'&:hover':{bgcolor:'action.hover'}}}><ListItemIcon sx={{minWidth:compact?0:38,color:'inherit',justifyContent:'center'}}>{item.icon}</ListItemIcon>{!compact&&<ListItemText primary={item.label} primaryTypographyProps={{fontWeight:selected?700:500,fontSize:14}}/>}</ListItemButton></Tooltip>})}</List>
  </>;
  return <Box sx={{display:'flex',minHeight:'100vh'}}>
    <AppBar position="fixed" color="inherit" elevation={0} sx={{zIndex:theme.zIndex.drawer+1,borderBottom:'1px solid',borderColor:'divider',ml:{md:`${width}px`},width:{md:`calc(100% - ${width}px)`},transition:theme.transitions.create(['width','margin'])}}>
      <Toolbar sx={{minHeight:'60px!important',px:{xs:1.5,sm:2}}}><IconButton edge="start" size="small" onClick={()=>mobile?setMobileOpen(true):setCollapsed(x=>!x)} sx={{mr:1.5}}>{mobile?<Menu/>:collapsed?<Menu/>:<MenuOpen/>}</IconButton><Box sx={{flexGrow:1}}/><Stack direction="row" spacing={1} alignItems="center"><Tooltip title={`Switch to ${mode==='light'?'dark':'light'} mode`}><IconButton size="small" color="inherit" onClick={toggle}>{mode==='light'?<Brightness4 fontSize="small"/>:<Brightness7 fontSize="small"/>}</IconButton></Tooltip><AccountCircle color="action"/><Box sx={{display:{xs:'none',sm:'block'}}}><Typography variant="body2" fontWeight={700}>{user?.fullName}</Typography><Typography variant="caption" color="text.secondary">{user?.roles.join(' · ')}</Typography></Box><Tooltip title="Logout"><IconButton size="small" color="inherit" onClick={()=>dispatch(logout())}><Logout fontSize="small"/></IconButton></Tooltip></Stack></Toolbar>
    </AppBar>
    <Drawer variant="temporary" open={mobileOpen} onClose={()=>setMobileOpen(false)} ModalProps={{keepMounted:true}} sx={{display:{xs:'block',md:'none'},'& .MuiDrawer-paper':{width:expandedWidth}}}>{drawer}</Drawer>
    <Drawer variant="permanent" sx={{display:{xs:'none',md:'block'},width,flexShrink:0,transition:theme.transitions.create('width'),'& .MuiDrawer-paper':{width,boxSizing:'border-box',borderRight:'1px solid',borderColor:'divider',transition:theme.transitions.create('width'),overflowX:'hidden'}}}>{drawer}</Drawer>
    <Box component="main" sx={{flexGrow:1,minWidth:0,height:'calc(100dvh - 60px)',p:{xs:1.25,sm:1.75,lg:2},mt:'60px',overflow:'auto'}}><Box sx={{maxWidth:1400,mx:'auto',height:'100%',minHeight:0,display:'flex',flexDirection:'column'}}><Outlet/></Box></Box>
  </Box>;
}
