using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureFlow.Application.Interfaces;
namespace ProcureFlow.API.Controllers;
[ApiController,Authorize,Route("api/dashboard")]
public class DashboardController(IDashboardService service) : ControllerBase { [HttpGet("summary")] public Task<ProcureFlow.Application.DTOs.DashboardSummaryDto> Summary(CancellationToken ct)=>service.GetSummaryAsync(ct); }