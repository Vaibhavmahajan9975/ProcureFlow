using ProcureFlow.Application.Common;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Entities;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.Services;
public class DeliveryService(IPurchaseOrderRepository poRepo,IDeliveryRepository deliveryRepo,IPurchaseRequestRepository prRepo,ICurrentUserService currentUser) : IDeliveryService
{
    public async Task MarkDeliveredAsync(Guid poId,MarkDeliveredDto dto,CancellationToken ct=default)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId)) throw new BusinessRuleException("Authenticated user is required.");
        var ok = await deliveryRepo.TryMarkDeliveredAsync(poId, dto.DeliveryDate, dto.Notes, currentUser.UserId!, ct);
        if (!ok) throw new BusinessRuleException("Purchase order was modified or delivery already recorded.");
    }
    public async Task CompleteAsync(Guid poId,CancellationToken ct=default)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId)) throw new BusinessRuleException("Authenticated user is required.");
        var ok = await deliveryRepo.TryCompleteAsync(poId, currentUser.UserId!, ct);
        if (!ok) throw new BusinessRuleException("Purchase order was modified or cannot be completed.");
    }
}