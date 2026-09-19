using AutoMapper;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Domain.Entities;
namespace ProcureFlow.Application.Mapping;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PurchaseRequest, PurchaseRequestDto>()
            .ForMember(d=>d.RequesterName,o=>o.MapFrom(s=>s.Requester.FullName))
            .ForMember(d=>d.DepartmentName,o=>o.MapFrom(s=>s.Department.Name))
            .ForMember(d=>d.VendorName,o=>o.MapFrom(s=>s.Vendor.Name))
            .ForMember(d=>d.CategoryName,o=>o.MapFrom(s=>s.Category.Name));
            CreateMap<PurchaseRequest, PurchaseRequestDetailsDto>()
                .IncludeBase<PurchaseRequest, PurchaseRequestDto>()
                .ForMember(d => d.SubmittedAt, o => o.MapFrom(s => s.SubmittedAt))
                .ForMember(d => d.ApprovedAt, o => o.MapFrom(s => s.ApprovedAt))
                .ForMember(d => d.ApprovedById, o => o.MapFrom(s => s.ApprovedById))
                .ForMember(d => d.RejectedAt, o => o.MapFrom(s => s.RejectedAt))
                .ForMember(d => d.RejectedById, o => o.MapFrom(s => s.RejectedById))
                .ForMember(d => d.RejectionReason, o => o.MapFrom(s => s.RejectionReason))
                .ForMember(d => d.StatusHistory, o => o.MapFrom(s => s.StatusHistory.OrderBy(h => h.ChangedAt)))
                .ForMember(d => d.PurchaseOrder, o => o.MapFrom(s => s.PurchaseOrder))
                .ForMember(d => d.Delivery, o => o.MapFrom(s => s.PurchaseOrder != null ? s.PurchaseOrder.Delivery : null));
            CreateMap<PurchaseRequestStatusHistory, StatusHistoryDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.FromStatus, o => o.MapFrom(s => s.FromStatus))
                .ForMember(d => d.ToStatus, o => o.MapFrom(s => s.ToStatus))
                .ForMember(d => d.ChangedById, o => o.MapFrom(s => s.ChangedById))
                .ForMember(d => d.ChangedBy, o => o.MapFrom(s => s.ChangedBy != null && !string.IsNullOrWhiteSpace(s.ChangedBy.FullName) ? s.ChangedBy.FullName : s.ChangedBy != null ? s.ChangedBy.Email : null))
                .ForMember(d => d.ChangedAt, o => o.MapFrom(s => s.ChangedAt))
                .ForMember(d => d.Comment, o => o.MapFrom(s => s.Comment));
            CreateMap<PurchaseOrder, PurchaseOrderSummaryDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.PONumber, o => o.MapFrom(s => s.PONumber))
                .ForMember(d => d.OrderDate, o => o.MapFrom(s => s.OrderDate))
                .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount))
                .ForMember(d => d.Currency, o => o.MapFrom(s => s.Currency))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status));
            CreateMap<Delivery, DeliverySummaryDto>()
                .ForMember(d => d.DeliveryDate, o => o.MapFrom(s => s.DeliveryDate))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
                .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes));
        CreateMap<CreatePurchaseRequestDto, PurchaseRequest>();
        CreateMap<UpdatePurchaseRequestDto, PurchaseRequest>();
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d=>d.PRNumber,o=>o.MapFrom(s=>s.PurchaseRequest.PRNumber))
            .ForMember(d=>d.VendorName,o=>o.MapFrom(s=>s.PurchaseRequest.Vendor.Name));
    }
}