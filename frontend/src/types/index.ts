export type Role='Requester'|'Approver'|'Admin';
export interface AuthResponse{token:string;email:string;fullName:string;roles:Role[]}
export interface PagedResult<T>{items:T[];pageNumber:number;pageSize:number;totalCount:number;totalPages:number}
export interface Lookup{id:string;name:string}
export interface PurchaseRequest{ id:string;prNumber:string;requesterName:string;departmentId:string;departmentName:string;vendorId:string;vendorName:string;categoryId:string;categoryName:string;description:string;amount:number;currency:number;requiredDate:string;status:number;createdAt:string;rejectionReason?:string }
export interface StatusHistory{ id:string;fromStatus:number;toStatus:number;changedById:string;changedBy?:string;changedAt:string;comment?:string }
export interface PurchaseOrderSummary{ id:string;poNumber:string;orderDate:string;totalAmount:number;currency:number;status:number }
export interface DeliverySummary{deliveryDate:string;status:number;notes?:string}
export interface PurchaseRequestDetails extends PurchaseRequest{submittedAt?:string;approvedAt?:string;approvedBy?:string;rejectedAt?:string;rejectedBy?:string;statusHistory:StatusHistory[];purchaseOrder?:PurchaseOrderSummary;delivery?:DeliverySummary}
export interface PurchaseRequestInput{departmentId:string;vendorId:string;categoryId:string;description:string;amount:number;currency:number;requiredDate:string}
export interface PurchaseOrder{ id:string;poNumber:string;purchaseRequestId:string;prNumber:string;orderDate:string;totalAmount:number;currency:number;status:number;vendorName:string }
export interface DashboardSummary{totalPRs:number;draft:number;submitted:number;approved:number;rejected:number;poCreated:number;delivered:number;completed:number;totalPOs:number}
