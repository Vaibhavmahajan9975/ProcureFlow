using ProcureFlow.Domain.Common;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Domain.Entities;
public class PurchaseRequest : BaseEntity
{
    public string PRNumber { get; set; } = string.Empty;
    public string RequesterId { get; set; } = string.Empty;
    public ApplicationUser Requester { get; set; } = null!;
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public DateOnly RequiredDate { get; set; }
    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedById { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedById { get; set; }
    public string? RejectionReason { get; set; }
    public ICollection<PurchaseRequestStatusHistory> StatusHistory { get; set; } = new List<PurchaseRequestStatusHistory>();
    public PurchaseOrder? PurchaseOrder { get; set; }
}