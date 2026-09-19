using ProcureFlow.Application.DTOs;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Enums;
namespace ProcureFlow.Application.Services;
public class DashboardService(IPurchaseRequestRepository prRepo,IPurchaseOrderRepository poRepo) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct=default)
    {
        var total=await prRepo.CountByStatusAsync(null,ct); return new(total,await prRepo.CountByStatusAsync(PurchaseRequestStatus.Draft,ct),await prRepo.CountByStatusAsync(PurchaseRequestStatus.Submitted,ct),await prRepo.CountByStatusAsync(PurchaseRequestStatus.Approved,ct),await prRepo.CountByStatusAsync(PurchaseRequestStatus.Rejected,ct),await prRepo.CountByStatusAsync(PurchaseRequestStatus.POCreated,ct),await prRepo.CountByStatusAsync(PurchaseRequestStatus.Delivered,ct),await prRepo.CountByStatusAsync(PurchaseRequestStatus.Completed,ct),await poRepo.CountAsync(ct));
    }
}
public class MasterDataService(IMasterDataRepository repo) : IMasterDataService
{
    public async Task<IReadOnlyCollection<LookupDto>> DepartmentsAsync(CancellationToken ct=default)=>(await repo.GetDepartmentsAsync(ct)).Select(x=>new LookupDto(x.Id,x.Name)).ToArray();
    public async Task<IReadOnlyCollection<LookupDto>> CategoriesAsync(CancellationToken ct=default)=>(await repo.GetCategoriesAsync(ct)).Select(x=>new LookupDto(x.Id,x.Name)).ToArray();
    public async Task<IReadOnlyCollection<LookupDto>> VendorsAsync(CancellationToken ct=default)=>(await repo.GetVendorsAsync(ct)).Select(x=>new LookupDto(x.Id,x.Name)).ToArray();
}