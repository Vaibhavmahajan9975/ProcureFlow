using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.DTOs;
public record CreatePurchaseOrderDto(Guid PurchaseRequestId);
public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public Guid PurchaseRequestId { get; set; }
    public string PRNumber { get; set; } = string.Empty;
    public DateOnly OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Currency Currency { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string VendorName { get; set; } = string.Empty;
}
public record MarkDeliveredDto(DateOnly DeliveryDate, string? Notes);