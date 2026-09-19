namespace ProcureFlow.Application.DTOs;
public record LoginRequestDto(string Email, string Password);
public record AuthResponseDto(string Token, string Email, string FullName, IReadOnlyCollection<string> Roles);