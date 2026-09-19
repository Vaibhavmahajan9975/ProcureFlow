using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using ProcureFlow.Application.Services;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Application.DTOs;
using ProcureFlow.Domain.Entities;
using ProcureFlow.Domain.Enums;
using Xunit;

namespace ProcureFlow.UnitTests;
public class BusinessRulesTests
{
    [Fact]
    public async Task Draft_PR_Can_Submit()
    {
        var repo = new Mock<IPurchaseRequestRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("user-1");
        repo.Setup(r => r.TrySubmitAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var mapper = new Mock<IMapper>();
        var svc = new PurchaseRequestService(repo.Object, current.Object, mapper.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<PurchaseRequestService>>());
        await svc.SubmitAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task Submitted_PR_Cannot_Edit()
    {
        var repo = new Mock<IPurchaseRequestRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("user-1");
        current.Setup(x => x.IsInRole("Requester")).Returns(true);
        var pr = new PurchaseRequest { Id = Guid.NewGuid(), RequesterId = "user-1", Status = PurchaseRequestStatus.Submitted };
        repo.Setup(r => r.GetByIdAsync(pr.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pr);
        var mapper = new Mock<IMapper>();
        var svc = new PurchaseRequestService(repo.Object, current.Object, mapper.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<PurchaseRequestService>>());
        await Assert.ThrowsAsync<ProcureFlow.Application.Common.BusinessRuleException>(() => svc.UpdateAsync(pr.Id, new UpdatePurchaseRequestDto()));
    }

    [Fact]
    public async Task Submitted_PR_Can_Approve()
    {
        var repo = new Mock<IPurchaseRequestRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("approver-1");
        repo.Setup(r => r.TryApproveAsync(It.IsAny<Guid>(), "approver-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var svc = new ApprovalService(repo.Object, current.Object, Mock.Of<IMapper>());
        await svc.ApproveAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task Draft_PR_Cannot_Approve()
    {
        var repo = new Mock<IPurchaseRequestRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("approver-1");
        repo.Setup(r => r.TryApproveAsync(It.IsAny<Guid>(), "approver-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var svc = new ApprovalService(repo.Object, current.Object, Mock.Of<IMapper>());
        await Assert.ThrowsAsync<ProcureFlow.Application.Common.BusinessRuleException>(() => svc.ApproveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Approved_PR_Can_Create_PO()
    {
        var prRepo = new Mock<IPurchaseRequestRepository>();
        var poRepo = new Mock<IPurchaseOrderRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("admin-1");
        var pr = new PurchaseRequest { Id = Guid.NewGuid(), Status = PurchaseRequestStatus.Approved, Amount = 100m, Currency = Currency.USD };
        prRepo.Setup(r => r.GetByIdAsync(pr.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pr);
        var po = new PurchaseOrder { Id = Guid.NewGuid(), PONumber = "PO-1", PurchaseRequestId = pr.Id, TotalAmount = 100m, Currency = Currency.USD, Status = PurchaseOrderStatus.Created };
        poRepo.Setup(r => r.CreateForPurchaseRequestAsync(pr.Id, pr.Amount, pr.Currency, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(po);
        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<PurchaseOrderDto>(It.IsAny<PurchaseOrder>())).Returns(new PurchaseOrderDto { Id = po.Id, PONumber = po.PONumber, PurchaseRequestId = pr.Id, TotalAmount = po.TotalAmount, Currency = po.Currency, OrderDate = po.OrderDate, Status = po.Status });
        var svc = new PurchaseOrderService(poRepo.Object, prRepo.Object, current.Object, mapper.Object);
        var result = await svc.CreateAsync(new CreatePurchaseOrderDto(pr.Id));
        Assert.Equal(po.Id, result.Id);
    }

    [Fact]
    public async Task Duplicate_PO_Is_Rejected()
    {
        var prRepo = new Mock<IPurchaseRequestRepository>();
        var poRepo = new Mock<IPurchaseOrderRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("admin-1");
        var pr = new PurchaseRequest { Id = Guid.NewGuid(), Status = PurchaseRequestStatus.Approved, Amount = 100m, Currency = Currency.USD };
        prRepo.Setup(r => r.GetByIdAsync(pr.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pr);
        poRepo.Setup(r => r.CreateForPurchaseRequestAsync(pr.Id, pr.Amount, pr.Currency, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((PurchaseOrder?)null);
        var mapper = new Mock<IMapper>();
        var svc = new PurchaseOrderService(poRepo.Object, prRepo.Object, current.Object, mapper.Object);
        await Assert.ThrowsAsync<ProcureFlow.Application.Common.BusinessRuleException>(() => svc.CreateAsync(new CreatePurchaseOrderDto(pr.Id)));
    }

    [Fact]
    public async Task Created_PO_Can_Be_Delivered_And_Completed()
    {
        var poRepo = new Mock<IPurchaseOrderRepository>();
        var deliveryRepo = new Mock<IDeliveryRepository>();
        var prRepo = new Mock<IPurchaseRequestRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("user-1");
        deliveryRepo.Setup(d => d.TryMarkDeliveredAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<string?>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        deliveryRepo.Setup(d => d.TryCompleteAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var svc = new DeliveryService(poRepo.Object, deliveryRepo.Object, prRepo.Object, current.Object);
        await svc.MarkDeliveredAsync(Guid.NewGuid(), new MarkDeliveredDto(DateOnly.FromDateTime(DateTime.UtcNow), "notes"));
        await svc.CompleteAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task Complete_Before_Delivery_Is_Rejected()
    {
        var poRepo = new Mock<IPurchaseOrderRepository>();
        var deliveryRepo = new Mock<IDeliveryRepository>();
        var prRepo = new Mock<IPurchaseRequestRepository>();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns("user-1");
        deliveryRepo.Setup(d => d.TryCompleteAsync(It.IsAny<Guid>(), "user-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var svc = new DeliveryService(poRepo.Object, deliveryRepo.Object, prRepo.Object, current.Object);
        await Assert.ThrowsAsync<ProcureFlow.Application.Common.BusinessRuleException>(() => svc.CompleteAsync(Guid.NewGuid()));
    }
}
