using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.API.Controllers;
[ApiController,Authorize,Route("api/purchase-requests")]
public class PurchaseRequestsController(IPurchaseRequestService service) : ControllerBase
{
    [HttpGet] public Task<ProcureFlow.Application.Common.PagedResult<PurchaseRequestDto>> Search([FromQuery]string? search,[FromQuery]PurchaseRequestStatus? status,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=10,[FromQuery]string? sortBy="createdAt",[FromQuery]string? sortDirection="desc",CancellationToken ct=default)=>service.SearchAsync(search,status,pageNumber,pageSize,sortBy,sortDirection,ct);
    [HttpGet("{id:guid}")] public Task<ProcureFlow.Application.DTOs.PurchaseRequestDetailsDto> Get(Guid id,CancellationToken ct)=>service.GetAsync(id,ct);
    [Authorize(Roles="Requester"),HttpPost] public Task<PurchaseRequestDto> Create(CreatePurchaseRequestDto dto,CancellationToken ct)=>service.CreateAsync(dto,ct);
    [Authorize(Roles="Requester"),HttpPut("{id:guid}")] public Task<PurchaseRequestDto> Update(Guid id,UpdatePurchaseRequestDto dto,CancellationToken ct)=>service.UpdateAsync(id,dto,ct);
    [Authorize(Roles="Requester"),HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct){await service.DeleteAsync(id,ct); return NoContent();}
    [Authorize(Roles="Requester"),HttpPost("{id:guid}/submit")] public async Task<IActionResult> Submit(Guid id,CancellationToken ct){await service.SubmitAsync(id,ct); return NoContent();}
}