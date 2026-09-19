using ProcureFlow.Domain.Common;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Domain.Entities;
public class Delivery : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public DateOnly? DeliveryDate { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public string? Notes { get; set; }
}