using Microsoft.AspNetCore.Identity;
namespace ProcureFlow.Domain.Entities;
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
}