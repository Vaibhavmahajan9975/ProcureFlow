using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProcureFlow.Application.Common;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace ProcureFlow.Infrastructure.Identity;
public class AuthService(UserManager<ApplicationUser> userManager,IConfiguration config) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto,CancellationToken ct=default)
    {
        var user=await userManager.FindByEmailAsync(dto.Email); if(user is null || !await userManager.CheckPasswordAsync(user,dto.Password)) throw new BusinessRuleException("Invalid email or password."); var roles=await userManager.GetRolesAsync(user); var key=config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing."); var claims=new List<Claim>{new(JwtRegisteredClaimNames.Sub,user.Id),new(ClaimTypes.NameIdentifier,user.Id),new(ClaimTypes.Email,user.Email!),new(ClaimTypes.Name,user.FullName)}; claims.AddRange(roles.Select(r=>new Claim(ClaimTypes.Role,r))); var token=new JwtSecurityToken(config["Jwt:Issuer"],config["Jwt:Audience"],claims,expires:DateTime.UtcNow.AddHours(8),signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),SecurityAlgorithms.HmacSha256)); return new(new JwtSecurityTokenHandler().WriteToken(token),user.Email!,user.FullName,roles.ToArray());
    }
}