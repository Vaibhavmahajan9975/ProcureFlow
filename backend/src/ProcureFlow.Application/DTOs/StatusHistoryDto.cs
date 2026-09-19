using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.DTOs;
public class StatusHistoryDto
{
    public Guid Id { get; set; }
    public PurchaseRequestStatus FromStatus { get; set; }
    public PurchaseRequestStatus ToStatus { get; set; }
    public string ChangedById { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }
}
