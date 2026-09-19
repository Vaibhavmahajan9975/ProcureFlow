using ProcureFlow.Application.Interfaces;
using System.Security.Claims;
namespace ProcureFlow.API.Services;
public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsInRole(string role)=>accessor.HttpContext?.User.IsInRole(role) ?? false;
}