using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.DTOs;
public class CreatePurchaseRequestDto
{
    public Guid DepartmentId { get; set; }
    public Guid VendorId { get; set; }
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public DateOnly RequiredDate { get; set; }
}
public class UpdatePurchaseRequestDto : CreatePurchaseRequestDto { }
public class PurchaseRequestDto
{
    public Guid Id { get; set; }
    public string PRNumber { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public DateOnly RequiredDate { get; set; }
    public PurchaseRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RejectionReason { get; set; }
}
public class PurchaseOrderSummaryDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public DateOnly OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Currency Currency { get; set; }
    public PurchaseOrderStatus Status { get; set; }
}

public class DeliverySummaryDto
{
    public DateOnly DeliveryDate { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseRequestDetailsDto : PurchaseRequestDto
{
    // Additional timing and user info
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedById { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedById { get; set; }
    public string? RejectedBy { get; set; }
    public StatusHistoryDto[] StatusHistory { get; set; } = Array.Empty<StatusHistoryDto>();
    public PurchaseOrderSummaryDto? PurchaseOrder { get; set; }
    public DeliverySummaryDto? Delivery { get; set; }
}
public record RejectPurchaseRequestDto(string Reason);