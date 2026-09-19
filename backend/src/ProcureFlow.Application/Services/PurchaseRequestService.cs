using AutoMapper;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ProcureFlow.Application.Common;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Entities;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.Services;
public class PurchaseRequestService(IPurchaseRequestRepository repo, ICurrentUserService currentUser, IMapper mapper, ILogger<PurchaseRequestService> logger) : IPurchaseRequestService
{
    public async Task<PagedResult<PurchaseRequestDto>> SearchAsync(string? search,PurchaseRequestStatus? status,int pageNumber,int pageSize,string? sortBy,string? sortDirection,CancellationToken ct=default)
    {
        var requesterId = currentUser.IsInRole("Requester") ? currentUser.UserId : null;
        var result = await repo.SearchAsync(requesterId,search,status,pageNumber,pageSize,sortBy,sortDirection,ct);
        return new(result.Items.Select(mapper.Map<PurchaseRequestDto>).ToArray(),result.PageNumber,result.PageSize,result.TotalCount);
    }
    public async Task<PurchaseRequestDetailsDto> GetAsync(Guid id,CancellationToken ct=default)
    {
        var e=await GetAuthorizedAsync(id,ct);
        // Map to details DTO including history, PO and delivery summaries
        return mapper.Map<PurchaseRequestDetailsDto>(e);
    }
    public async Task<PurchaseRequestDto> CreateAsync(CreatePurchaseRequestDto dto,CancellationToken ct=default)
    {
        if(string.IsNullOrWhiteSpace(currentUser.UserId)) throw new BusinessRuleException("Authenticated user is required.");
        var e=mapper.Map<PurchaseRequest>(dto); e.RequesterId=currentUser.UserId!; e.PRNumber=await GeneratePrNumberAsync(ct); e.Status=PurchaseRequestStatus.Draft;
        await repo.AddAsync(e,ct); await repo.SaveChangesAsync(ct); return mapper.Map<PurchaseRequestDto>(await repo.GetByIdAsync(e.Id,ct) ?? e);
    }
    public async Task<PurchaseRequestDto> UpdateAsync(Guid id,UpdatePurchaseRequestDto dto,CancellationToken ct=default)
    {
        var e=await GetAuthorizedAsync(id,ct); if(e.Status!=PurchaseRequestStatus.Draft) throw new BusinessRuleException("Only Draft purchase requests can be edited.");
        mapper.Map(dto,e); await repo.SaveChangesAsync(ct); return mapper.Map<PurchaseRequestDto>(e);
    }
    public async Task DeleteAsync(Guid id,CancellationToken ct=default)
    {
        var e=await GetAuthorizedAsync(id,ct); if(e.Status!=PurchaseRequestStatus.Draft) throw new BusinessRuleException("Only Draft purchase requests can be deleted."); repo.Remove(e); await repo.SaveChangesAsync(ct);
    }
    public async Task SubmitAsync(Guid id,CancellationToken ct=default)
    {
        if(string.IsNullOrWhiteSpace(currentUser.UserId)) throw new BusinessRuleException("Authenticated user is required.");
        var ok = await repo.TrySubmitAsync(id, currentUser.UserId!, ct);
        if (!ok) throw new BusinessRuleException("Purchase request was modified and is no longer in a submittable state.");
    }
    private async Task<PurchaseRequest> GetAuthorizedAsync(Guid id,CancellationToken ct)
    {
        var e=await repo.GetByIdAsync(id,ct) ?? throw new NotFoundException("Purchase request not found.");
        if(currentUser.IsInRole("Requester") && e.RequesterId!=currentUser.UserId) throw new UnauthorizedAccessException("You can only access your own purchase requests."); return e;
    }
    private async Task<string> GeneratePrNumberAsync(CancellationToken ct) { var count=await repo.CountByStatusAsync(null,ct)+1; return $"PR-{DateTime.UtcNow:yyyy}-{count:00000}"; }
}