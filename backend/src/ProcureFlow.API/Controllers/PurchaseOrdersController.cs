using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.API.Controllers;
[ApiController,Authorize,Route("api/purchase-orders")]
public class PurchaseOrdersController(IPurchaseOrderService service) : ControllerBase
{
    [HttpGet] public Task<ProcureFlow.Application.Common.PagedResult<PurchaseOrderDto>> Search([FromQuery]string? search,[FromQuery]PurchaseOrderStatus? status,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=10,CancellationToken ct=default)=>service.SearchAsync(search,status,pageNumber,pageSize,ct);
    [HttpGet("{id:guid}")] public Task<PurchaseOrderDto> Get(Guid id,CancellationToken ct)=>service.GetAsync(id,ct);
    [Authorize(Roles="Admin"),HttpPost] public Task<PurchaseOrderDto> Create(CreatePurchaseOrderDto dto,CancellationToken ct)=>service.CreateAsync(dto,ct);
}