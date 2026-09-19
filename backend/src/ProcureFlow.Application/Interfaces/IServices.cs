using ProcureFlow.Application.Common;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.Interfaces;
public interface IAuthService { Task<AuthResponseDto> LoginAsync(LoginRequestDto dto,CancellationToken ct=default); }
public interface IPurchaseRequestService
{
    Task<PagedResult<PurchaseRequestDto>> SearchAsync(string? search,PurchaseRequestStatus? status,int pageNumber,int pageSize,string? sortBy,string? sortDirection,CancellationToken ct=default);
    Task<PurchaseRequestDetailsDto> GetAsync(Guid id,CancellationToken ct=default);
    Task<PurchaseRequestDto> CreateAsync(CreatePurchaseRequestDto dto,CancellationToken ct=default);
    Task<PurchaseRequestDto> UpdateAsync(Guid id,UpdatePurchaseRequestDto dto,CancellationToken ct=default);
    Task DeleteAsync(Guid id,CancellationToken ct=default);
    Task SubmitAsync(Guid id,CancellationToken ct=default);
}
public interface IApprovalService
{
    Task<PagedResult<PurchaseRequestDto>> PendingAsync(string? search,int pageNumber,int pageSize,CancellationToken ct=default);
    Task ApproveAsync(Guid id,CancellationToken ct=default);
    Task RejectAsync(Guid id,string reason,CancellationToken ct=default);
}
public interface IPurchaseOrderService
{
    Task<PagedResult<PurchaseOrderDto>> SearchAsync(string? search,PurchaseOrderStatus? status,int pageNumber,int pageSize,CancellationToken ct=default);
    Task<PurchaseOrderDto> GetAsync(Guid id,CancellationToken ct=default);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto,CancellationToken ct=default);
}
public interface IDeliveryService
{
    Task MarkDeliveredAsync(Guid poId,MarkDeliveredDto dto,CancellationToken ct=default);
    Task CompleteAsync(Guid poId,CancellationToken ct=default);
}
public interface IDashboardService { Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct=default); }
public interface IMasterDataService
{
    Task<IReadOnlyCollection<LookupDto>> DepartmentsAsync(CancellationToken ct=default);
    Task<IReadOnlyCollection<LookupDto>> CategoriesAsync(CancellationToken ct=default);
    Task<IReadOnlyCollection<LookupDto>> VendorsAsync(CancellationToken ct=default);
}