using AutoMapper;
using ProcureFlow.Application.Common;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Entities;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.Services;
public class PurchaseOrderService(IPurchaseOrderRepository poRepo,IPurchaseRequestRepository prRepo, ICurrentUserService currentUser, IMapper mapper) : IPurchaseOrderService
{
    public async Task<PagedResult<PurchaseOrderDto>> SearchAsync(string? search,PurchaseOrderStatus? status,int pageNumber,int pageSize,CancellationToken ct=default)
    { var r=await poRepo.SearchAsync(search,status,pageNumber,pageSize,ct); return new(r.Items.Select(mapper.Map<PurchaseOrderDto>).ToArray(),r.PageNumber,r.PageSize,r.TotalCount); }
    public async Task<PurchaseOrderDto> GetAsync(Guid id,CancellationToken ct=default) => mapper.Map<PurchaseOrderDto>(await poRepo.GetByIdAsync(id,ct) ?? throw new NotFoundException("Purchase order not found."));
    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto,CancellationToken ct=default)
    {
        // Use repository atomic operation to avoid concurrency issues when creating PO and updating PR
        var pr = await prRepo.GetByIdAsync(dto.PurchaseRequestId, ct) ?? throw new NotFoundException("Purchase request not found.");
        if (pr.Status != PurchaseRequestStatus.Approved) throw new BusinessRuleException("A PO can only be created for an Approved purchase request.");
        var createdBy = string.IsNullOrWhiteSpace(currentUser.UserId) ? "system" : currentUser.UserId!;
        var po = await poRepo.CreateForPurchaseRequestAsync(pr.Id, pr.Amount, pr.Currency, createdBy: createdBy, ct);
        if (po == null) throw new BusinessRuleException("A purchase order already exists for this purchase request or the request is no longer approvable.");
        return mapper.Map<PurchaseOrderDto>(await poRepo.GetByIdAsync(po.Id, ct) ?? po);
    }
}