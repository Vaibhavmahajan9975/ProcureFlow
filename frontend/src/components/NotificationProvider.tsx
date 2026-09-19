import {Alert,Snackbar} from '@mui/material';
import {createContext,useCallback,useContext,useMemo,useState} from 'react';

type Severity='success'|'error'|'info'|'warning';
type NotificationContextValue={notify:(message:string,severity?:Severity)=>void};
const NotificationContext=createContext<NotificationContextValue|undefined>(undefined);

export function NotificationProvider({children}:{children:React.ReactNode}){
  const [state,setState]=useState<{open:boolean;message:string;severity:Severity}>({open:false,message:'',severity:'info'});
  const notify=useCallback((message:string,severity:Severity='success')=>setState({open:true,message,severity}),[]);
  const value=useMemo(()=>({notify}),[notify]);
  return <NotificationContext.Provider value={value}>{children}<Snackbar open={state.open} autoHideDuration={5000} onClose={()=>setState(s=>({...s,open:false}))} anchorOrigin={{vertical:'bottom',horizontal:'right'}}><Alert variant="filled" severity={state.severity} onClose={()=>setState(s=>({...s,open:false}))} sx={{maxWidth:520}}>{state.message}</Alert></Snackbar></NotificationContext.Provider>;
}
export function useNotification(){const value=useContext(NotificationContext);if(!value)throw new Error('useNotification must be used inside NotificationProvider');return value;}

