using AutoMapper;
using ProcureFlow.Application.Common;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.Services;
public class ApprovalService(IPurchaseRequestRepository repo,ICurrentUserService currentUser,IMapper mapper) : IApprovalService
{
    public async Task<PagedResult<PurchaseRequestDto>> PendingAsync(string? search,int pageNumber,int pageSize,CancellationToken ct=default)
    { var r=await repo.SearchAsync(null,search,PurchaseRequestStatus.Submitted,pageNumber,pageSize,"createdAt","asc",ct); return new(r.Items.Select(mapper.Map<PurchaseRequestDto>).ToArray(),r.PageNumber,r.PageSize,r.TotalCount); }
    public async Task ApproveAsync(Guid id,CancellationToken ct=default)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId)) throw new BusinessRuleException("Authenticated user is required.");
        var ok = await repo.TryApproveAsync(id, currentUser.UserId!, ct);
        if (!ok) throw new BusinessRuleException("Purchase request was modified and is no longer in an approvable state.");
    }
    public async Task RejectAsync(Guid id,string reason,CancellationToken ct=default)
    {
        if(string.IsNullOrWhiteSpace(reason)) throw new BusinessRuleException("Rejection reason is required.");
        if (string.IsNullOrWhiteSpace(currentUser.UserId)) throw new BusinessRuleException("Authenticated user is required.");
        var ok = await repo.TryRejectAsync(id, currentUser.UserId!, reason, ct);
        if (!ok) throw new BusinessRuleException("Purchase request was modified and is no longer in a rejectable state.");
    }
}