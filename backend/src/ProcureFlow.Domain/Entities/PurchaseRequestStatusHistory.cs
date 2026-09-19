using ProcureFlow.Domain.Common;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Domain.Entities;
public class PurchaseRequestStatusHistory : BaseEntity
{
    public Guid PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest { get; set; } = null!;
    public PurchaseRequestStatus FromStatus { get; set; }
    public PurchaseRequestStatus ToStatus { get; set; }
    public string ChangedById { get; set; } = string.Empty;
    public ApplicationUser? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }
}