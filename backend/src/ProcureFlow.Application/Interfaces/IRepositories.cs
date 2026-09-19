using ProcureFlow.Application.Common;
using ProcureFlow.Domain.Entities;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.Interfaces;
public interface IPurchaseRequestRepository
{
    Task<PurchaseRequest?> GetByIdAsync(Guid id, CancellationToken ct=default);
    Task<PagedResult<PurchaseRequest>> SearchAsync(string? requesterId,string? search,PurchaseRequestStatus? status,int pageNumber,int pageSize,string? sortBy,string? sortDirection,CancellationToken ct=default);
    Task<int> CountByStatusAsync(PurchaseRequestStatus? status,CancellationToken ct=default);
    Task AddAsync(PurchaseRequest entity,CancellationToken ct=default);
    void Remove(PurchaseRequest entity);
    Task SaveChangesAsync(CancellationToken ct=default);
    Task<bool> TrySubmitAsync(Guid id, string changedById, CancellationToken ct=default);
    Task<bool> TryApproveAsync(Guid id, string approverId, CancellationToken ct=default);
    Task<bool> TryRejectAsync(Guid id, string approverId, string reason, CancellationToken ct=default);
}
public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id,CancellationToken ct=default);
    Task<PurchaseOrder?> GetByPurchaseRequestIdAsync(Guid purchaseRequestId,CancellationToken ct=default);
    Task<PagedResult<PurchaseOrder>> SearchAsync(string? search,PurchaseOrderStatus? status,int pageNumber,int pageSize,CancellationToken ct=default);
    Task<int> CountAsync(CancellationToken ct=default);
    Task AddAsync(PurchaseOrder entity,CancellationToken ct=default);
    Task SaveChangesAsync(CancellationToken ct=default);
    Task<PurchaseOrder?> CreateForPurchaseRequestAsync(Guid purchaseRequestId, decimal totalAmount, Currency currency, string createdBy, CancellationToken ct=default);
}
public interface IDeliveryRepository
{
    Task<Delivery?> GetByPurchaseOrderIdAsync(Guid poId,CancellationToken ct=default);
    Task AddAsync(Delivery entity,CancellationToken ct=default);
    Task SaveChangesAsync(CancellationToken ct=default);
    Task<bool> TryMarkDeliveredAsync(Guid purchaseOrderId, DateOnly deliveryDate, string? notes, string changedById, CancellationToken ct=default);
    Task<bool> TryCompleteAsync(Guid purchaseOrderId, string changedById, CancellationToken ct=default);
}
public interface IMasterDataRepository
{
    Task<IReadOnlyCollection<Department>> GetDepartmentsAsync(CancellationToken ct=default);
    Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken ct=default);
    Task<IReadOnlyCollection<Vendor>> GetVendorsAsync(CancellationToken ct=default);
}