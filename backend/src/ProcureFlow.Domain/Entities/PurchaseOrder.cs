using ProcureFlow.Domain.Common;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Domain.Entities;
public class PurchaseOrder : BaseEntity
{
    public string PONumber { get; set; } = string.Empty;
    public Guid PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest { get; set; } = null!;
    public DateOnly OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Currency Currency { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Created;
    public Delivery? Delivery { get; set; }
}