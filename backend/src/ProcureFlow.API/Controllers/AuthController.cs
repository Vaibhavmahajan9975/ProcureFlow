using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
namespace ProcureFlow.API.Controllers;
[ApiController,Route("api/auth")]
public class AuthController(IAuthService service) : ControllerBase
{
    [AllowAnonymous,HttpPost("login")] public Task<AuthResponseDto> Login(LoginRequestDto dto,CancellationToken ct)=>service.LoginAsync(dto,ct);
    [Authorize,HttpGet("me")] public IActionResult Me()=>Ok(new{email=User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,name=User.Identity?.Name,roles=User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(x=>x.Value)});
}