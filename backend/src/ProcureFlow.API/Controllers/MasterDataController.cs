using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureFlow.Application.Interfaces;
namespace ProcureFlow.API.Controllers;
[ApiController,Authorize,Route("api")]
public class MasterDataController(IMasterDataService service) : ControllerBase
{
    [HttpGet("departments")] public Task<IReadOnlyCollection<ProcureFlow.Application.DTOs.LookupDto>> Departments(CancellationToken ct)=>service.DepartmentsAsync(ct);
    [HttpGet("categories")] public Task<IReadOnlyCollection<ProcureFlow.Application.DTOs.LookupDto>> Categories(CancellationToken ct)=>service.CategoriesAsync(ct);
    [HttpGet("vendors")] public Task<IReadOnlyCollection<ProcureFlow.Application.DTOs.LookupDto>> Vendors(CancellationToken ct)=>service.VendorsAsync(ct);
}