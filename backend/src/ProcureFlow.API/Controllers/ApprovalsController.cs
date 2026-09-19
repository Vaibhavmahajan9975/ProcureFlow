using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
namespace ProcureFlow.API.Controllers;
[ApiController,Authorize(Roles="Approver"),Route("api/approvals")]
public class ApprovalsController(IApprovalService service) : ControllerBase
{
    [HttpGet] public Task<ProcureFlow.Application.Common.PagedResult<PurchaseRequestDto>> Pending([FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=10,CancellationToken ct=default)=>service.PendingAsync(search,pageNumber,pageSize,ct);
    [HttpPost("{id:guid}/approve")] public async Task<IActionResult> Approve(Guid id,CancellationToken ct){await service.ApproveAsync(id,ct); return NoContent();}
    [HttpPost("{id:guid}/reject")] public async Task<IActionResult> Reject(Guid id,RejectPurchaseRequestDto dto,CancellationToken ct){await service.RejectAsync(id,dto.Reason,ct); return NoContent();}
}