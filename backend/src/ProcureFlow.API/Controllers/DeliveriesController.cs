using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
namespace ProcureFlow.API.Controllers;
[ApiController,Authorize(Roles="Admin"),Route("api/deliveries")]
public class DeliveriesController(IDeliveryService service) : ControllerBase
{
    [HttpPost("{poId:guid}/deliver")] public async Task<IActionResult> Deliver(Guid poId,MarkDeliveredDto dto,CancellationToken ct){await service.MarkDeliveredAsync(poId,dto,ct); return NoContent();}
    [HttpPost("{poId:guid}/complete")] public async Task<IActionResult> Complete(Guid poId,CancellationToken ct){await service.CompleteAsync(poId,ct); return NoContent();}
}