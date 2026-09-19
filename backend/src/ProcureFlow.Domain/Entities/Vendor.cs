using ProcureFlow.Domain.Common;
namespace ProcureFlow.Domain.Entities;
public class Vendor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}