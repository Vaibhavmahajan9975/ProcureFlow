import {CssBaseline,ThemeProvider,createTheme} from '@mui/material';
import {createContext,useContext,useMemo,useState} from 'react';
import type {} from '@mui/x-data-grid/themeAugmentation';

type ColorMode='light'|'dark';
const ColorModeContext=createContext<{mode:ColorMode;toggle:()=>void}|undefined>(undefined);

export function ColorModeProvider({children}:{children:React.ReactNode}){
  const [mode,setMode]=useState<ColorMode>(()=>localStorage.getItem('procureflow_color_mode')==='dark'?'dark':'light');
  const value=useMemo(()=>({mode,toggle:()=>setMode(current=>{const next=current==='light'?'dark':'light';localStorage.setItem('procureflow_color_mode',next);return next})}),[mode]);
  const theme=useMemo(()=>createTheme({
    palette:{mode,primary:{main:mode==='light'?'#155eef':'#84adff'},background:{default:mode==='light'?'#f6f8fb':'#0f172a',paper:mode==='light'?'#ffffff':'#172033'}},
    shape:{borderRadius:8},
    typography:{fontFamily:'Inter, Arial, sans-serif',fontSize:13,h4:{fontSize:'1.4rem',fontWeight:700,letterSpacing:'-.02em'},h5:{fontSize:'1.15rem',fontWeight:700,letterSpacing:'-.01em'},h6:{fontSize:'1rem',fontWeight:700}},
    components:{
      MuiButton:{defaultProps:{disableElevation:true,size:'small'},styleOverrides:{root:{textTransform:'none',fontWeight:600,borderRadius:7,minHeight:32}}},
      MuiTextField:{defaultProps:{variant:'outlined',size:'small'}},
      MuiOutlinedInput:{styleOverrides:{root:{minHeight:34,'&:not(.MuiInputBase-multiline)':{height:34}},input:{padding:'7px 10px'},inputSizeSmall:{padding:'7px 10px'}}},
      MuiInputLabel:{styleOverrides:{root:{fontSize:13}}},
      MuiFormControl:{defaultProps:{size:'small'}},
      MuiCard:{styleOverrides:{root:{borderRadius:10}}},
      MuiToolbar:{styleOverrides:{root:{minHeight:60}}},
      MuiTablePagination:{styleOverrides:{toolbar:{minHeight:44}}},
      MuiDataGrid:{defaultProps:{rowHeight:36,columnHeaderHeight:38},styleOverrides:{root:{fontSize:12.5},columnHeader:{paddingLeft:10,paddingRight:10},cell:{paddingLeft:10,paddingRight:10},footerContainer:{minHeight:42},panelContent:{padding:8},filterForm:{gap:6,padding:8},filterFormColumnInput:{width:150},filterFormOperatorInput:{width:115},filterFormValueInput:{width:160}}},
      MuiMenuItem:{styleOverrides:{root:{fontSize:12.5,minHeight:'32px!important',paddingTop:4,paddingBottom:4}}},
      MuiListItemText:{styleOverrides:{primary:{fontSize:13}}},
      MuiDialogTitle:{styleOverrides:{root:{fontSize:'1.1rem',fontWeight:700,padding:'16px 20px'}}},
      MuiDialogContent:{styleOverrides:{root:{padding:'12px 20px'}}},
      MuiDialogActions:{styleOverrides:{root:{padding:'12px 20px'}}},
      MuiPopover:{styleOverrides:{paper:{'& .MuiDataGrid-panelContent':{padding:8},'& .MuiDataGrid-filterForm':{gap:6,padding:8},'& .MuiDataGrid-filterFormColumnInput':{width:150},'& .MuiDataGrid-filterFormOperatorInput':{width:115},'& .MuiDataGrid-filterFormValueInput':{width:160}}}}
    }
  }),[mode]);
  return <ColorModeContext.Provider value={value}><ThemeProvider theme={theme}><CssBaseline/>{children}</ThemeProvider></ColorModeContext.Provider>;
}
export function useColorMode(){const value=useContext(ColorModeContext);if(!value)throw new Error('useColorMode must be used inside ColorModeProvider');return value;}
