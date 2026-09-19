type ApiErrorBody={message?:string;errors?:string[]|Record<string,string[]>};

const friendly=(message:string)=>message
  .replace(/^['"]?Required Date['"]? must be greater than or equal to ['"]?(\d{2})-(\d{2})-(\d{4})['"]?\.?$/i,(_,d,m,y)=>`Required Date cannot be earlier than ${new Date(`${y}-${m}-${d}T00:00:00`).toLocaleDateString('en-GB',{day:'2-digit',month:'short',year:'numeric'})}.`)
  .replace(/'([^']+)'/g,'$1');

export function getApiErrors(error:unknown):string[]{
  const body=(error as {data?:ApiErrorBody})?.data;
  const errors=body?.errors;
  const list=Array.isArray(errors)?errors:errors?Object.values(errors).flat():[];
  if(list.length)return list.map(friendly);
  if(body?.message)return [friendly(body.message)];
  if(error instanceof Error)return [error.message];
  return ['Something went wrong. Please try again.'];
}
export const getApiErrorMessage=(error:unknown)=>getApiErrors(error).join(' ');

