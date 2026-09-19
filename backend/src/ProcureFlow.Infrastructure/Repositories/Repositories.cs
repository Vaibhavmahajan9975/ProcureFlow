using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Application.Common;
using ProcureFlow.Application.Interfaces;
using ProcureFlow.Domain.Entities;
using ProcureFlow.Domain.Enums;
using ProcureFlow.Infrastructure.Persistence;

namespace ProcureFlow.Infrastructure.Repositories;

public class PurchaseRequestRepository(ProcureFlowDbContext db) : IPurchaseRequestRepository
{
    private IQueryable<PurchaseRequest> Query() => db.PurchaseRequests
        .Include(x => x.Requester)
        .Include(x => x.Department)
        .Include(x => x.Vendor)
        .Include(x => x.Category)
        .Include(x => x.StatusHistory)
        .Include(x => x.PurchaseOrder).ThenInclude(x => x!.Delivery);

    public Task<PurchaseRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) => Query().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> TrySubmitAsync(Guid id, string changedById, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var newStatus = PurchaseRequestStatus.Submitted.ToString();
        var draftStatus = PurchaseRequestStatus.Draft.ToString();
        var submittedAt = DateTime.UtcNow;
        var rows = await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"PurchaseRequests\" SET \"Status\" = {newStatus}, \"SubmittedAt\" = {submittedAt} WHERE \"Id\" = {id} AND \"Status\" = {draftStatus}", ct);
        if (rows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }
        var history = new PurchaseRequestStatusHistory { Id = Guid.NewGuid(), PurchaseRequestId = id, FromStatus = PurchaseRequestStatus.Draft, ToStatus = PurchaseRequestStatus.Submitted, ChangedById = changedById, ChangedAt = submittedAt, CreatedAt = submittedAt };
        await db.PurchaseRequestStatusHistories.AddAsync(history, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task<bool> TryApproveAsync(Guid id, string approverId, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var approvedAt = DateTime.UtcNow;
        var approvedStatus = PurchaseRequestStatus.Approved.ToString();
        var submittedStatus = PurchaseRequestStatus.Submitted.ToString();
        var rows = await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"PurchaseRequests\" SET \"Status\" = {approvedStatus}, \"ApprovedAt\" = {approvedAt}, \"ApprovedById\" = {approverId} WHERE \"Id\" = {id} AND \"Status\" = {submittedStatus}", ct);
        if (rows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }
        var history = new PurchaseRequestStatusHistory { Id = Guid.NewGuid(), PurchaseRequestId = id, FromStatus = PurchaseRequestStatus.Submitted, ToStatus = PurchaseRequestStatus.Approved, ChangedById = approverId, ChangedAt = approvedAt, CreatedAt = approvedAt };
        await db.PurchaseRequestStatusHistories.AddAsync(history, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task<bool> TryRejectAsync(Guid id, string approverId, string reason, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var rejectedAt = DateTime.UtcNow;
        var rejectedStatus = PurchaseRequestStatus.Rejected.ToString();
        var submittedStatus = PurchaseRequestStatus.Submitted.ToString();
        var rows = await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"PurchaseRequests\" SET \"Status\" = {rejectedStatus}, \"RejectedAt\" = {rejectedAt}, \"RejectedById\" = {approverId}, \"RejectionReason\" = {reason} WHERE \"Id\" = {id} AND \"Status\" = {submittedStatus}", ct);
        if (rows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }
        var history = new PurchaseRequestStatusHistory { Id = Guid.NewGuid(), PurchaseRequestId = id, FromStatus = PurchaseRequestStatus.Submitted, ToStatus = PurchaseRequestStatus.Rejected, ChangedById = approverId, ChangedAt = rejectedAt, Comment = reason, CreatedAt = rejectedAt };
        await db.PurchaseRequestStatusHistories.AddAsync(history, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task<PagedResult<PurchaseRequest>> SearchAsync(string? requesterId, string? search, PurchaseRequestStatus? status, int pageNumber, int pageSize, string? sortBy, string? sortDirection, CancellationToken ct = default)
    {
        var q = Query().AsQueryable();
        if (!string.IsNullOrWhiteSpace(requesterId)) q = q.Where(x => x.RequesterId == requesterId);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search)) { var s = search.ToLower(); q = q.Where(x => x.PRNumber.ToLower().Contains(s) || x.Description.ToLower().Contains(s) || x.Vendor.Name.ToLower().Contains(s)); }
        q = (sortBy?.ToLower(), sortDirection?.ToLower()) switch { ("amount", "asc") => q.OrderBy(x => x.Amount), ("amount", _) => q.OrderByDescending(x => x.Amount), ("createdat", "asc") => q.OrderBy(x => x.CreatedAt), _ => q.OrderByDescending(x => x.CreatedAt) };
        var count = await q.CountAsync(ct);
        var items = await q.Skip((Math.Max(pageNumber, 1) - 1) * Math.Clamp(pageSize, 1, 100)).Take(Math.Clamp(pageSize, 1, 100)).ToListAsync(ct);
        return new(items, Math.Max(pageNumber, 1), Math.Clamp(pageSize, 1, 100), count);
    }

    public Task<int> CountByStatusAsync(PurchaseRequestStatus? status, CancellationToken ct = default) => status.HasValue ? db.PurchaseRequests.CountAsync(x => x.Status == status.Value, ct) : db.PurchaseRequests.CountAsync(ct);
    public Task AddAsync(PurchaseRequest e, CancellationToken ct = default) => db.PurchaseRequests.AddAsync(e, ct).AsTask();
    public void Remove(PurchaseRequest e) => db.PurchaseRequests.Remove(e);
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class PurchaseOrderRepository(ProcureFlowDbContext db) : IPurchaseOrderRepository
{
    private IQueryable<PurchaseOrder> Query() => db.PurchaseOrders.Include(x => x.PurchaseRequest).ThenInclude(x => x.Vendor).Include(x => x.Delivery);
    public Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) => Query().FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<PurchaseOrder?> GetByPurchaseRequestIdAsync(Guid prId, CancellationToken ct = default) => Query().FirstOrDefaultAsync(x => x.PurchaseRequestId == prId, ct);
    public async Task<PagedResult<PurchaseOrder>> SearchAsync(string? search, PurchaseOrderStatus? status, int pageNumber, int pageSize, CancellationToken ct = default) { var q = Query(); if (status.HasValue) q = q.Where(x => x.Status == status.Value); if (!string.IsNullOrWhiteSpace(search)) { var s = search.ToLower(); q = q.Where(x => x.PONumber.ToLower().Contains(s) || x.PurchaseRequest.PRNumber.ToLower().Contains(s) || x.PurchaseRequest.Vendor.Name.ToLower().Contains(s)); } q = q.OrderByDescending(x => x.CreatedAt); var count = await q.CountAsync(ct); var size = Math.Clamp(pageSize, 1, 100); var page = Math.Max(pageNumber, 1); return new(await q.Skip((page - 1) * size).Take(size).ToListAsync(ct), page, size, count); }
    public Task<int> CountAsync(CancellationToken ct = default) => db.PurchaseOrders.CountAsync(ct);
    public Task AddAsync(PurchaseOrder e, CancellationToken ct = default) => db.PurchaseOrders.AddAsync(e, ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    public async Task<PurchaseOrder?> CreateForPurchaseRequestAsync(Guid purchaseRequestId, decimal totalAmount, Currency currency, string createdBy, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        // Atomically set PR status to POCreated only when PR is Approved and no PO exists for it
        var newStatus = PurchaseRequestStatus.POCreated.ToString();
        var approvedStatus = PurchaseRequestStatus.Approved.ToString();
        var rows = await db.Database.ExecuteSqlRawAsync("UPDATE \"PurchaseRequests\" SET \"Status\" = {0} WHERE \"Id\" = {1} AND \"Status\" = {2} AND NOT EXISTS (SELECT 1 FROM \"PurchaseOrders\" WHERE \"PurchaseRequestId\" = {1})", newStatus, purchaseRequestId, approvedStatus);
        if (rows == 0)
        {
            await tx.RollbackAsync(ct);
            return null;
        }

        // Create PO (PONumber uses current count; small chance of race on PONumber but OK for now)
        var count = await db.PurchaseOrders.CountAsync(ct) + 1;
        var po = new PurchaseOrder { Id = Guid.NewGuid(), PurchaseRequestId = purchaseRequestId, PONumber = $"PO-{DateTime.UtcNow:yyyy}-{count:00000}", OrderDate = DateOnly.FromDateTime(DateTime.UtcNow), TotalAmount = totalAmount, Currency = currency, Status = PurchaseOrderStatus.Created, CreatedAt = DateTime.UtcNow, CreatedBy = createdBy };
        await db.PurchaseOrders.AddAsync(po, ct);

        // Insert history (from Approved -> POCreated)
        var history = new PurchaseRequestStatusHistory { Id = Guid.NewGuid(), PurchaseRequestId = purchaseRequestId, FromStatus = PurchaseRequestStatus.Approved, ToStatus = PurchaseRequestStatus.POCreated, ChangedById = createdBy, ChangedAt = DateTime.UtcNow, Comment = "Purchase order created", CreatedAt = DateTime.UtcNow };
        await db.PurchaseRequestStatusHistories.AddAsync(history, ct);

        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return po;
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync(ct);
            return null;
        }
    }

    // Removed earlier orphan TryCompleteAsync implementation. The concrete implementation is below in DeliveryRepository.
}

public class DeliveryRepository(ProcureFlowDbContext db) : IDeliveryRepository
{
    public Task<Delivery?> GetByPurchaseOrderIdAsync(Guid poId, CancellationToken ct = default) => db.Deliveries.FirstOrDefaultAsync(x => x.PurchaseOrderId == poId, ct);
    public Task AddAsync(Delivery e, CancellationToken ct = default) => db.Deliveries.AddAsync(e, ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task<bool> TryMarkDeliveredAsync(Guid purchaseOrderId, DateOnly deliveryDate, string? notes, string changedById, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);

        // Ensure PO exists and is in Created state
        var poId = purchaseOrderId;
        // Use ExecuteUpdateAsync to perform a conditional update without loading entities into change tracker
        var rows = await db.PurchaseOrders
            .Where(x => x.Id == poId && x.Status == PurchaseOrderStatus.Created)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, p => PurchaseOrderStatus.Delivered), ct);
        if (rows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        // Get linked PurchaseRequestId
        var prId = await db.PurchaseOrders.Where(x => x.Id == poId).Select(x => x.PurchaseRequestId).FirstOrDefaultAsync(ct);
        if (prId == Guid.Empty)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        // Update PR status from POCreated -> Delivered
        var fromStatus = PurchaseRequestStatus.POCreated.ToString();
        var toStatus = PurchaseRequestStatus.Delivered.ToString();
        var prRows = await db.PurchaseRequests
            .Where(x => x.Id == prId && x.Status == PurchaseRequestStatus.POCreated)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, p => PurchaseRequestStatus.Delivered), ct);
        if (prRows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        // Insert Delivery record
        var deliveredAt = DateTime.UtcNow;
        var delivery = new Delivery { Id = Guid.NewGuid(), PurchaseOrderId = poId, DeliveryDate = deliveryDate, Notes = notes, Status = DeliveryStatus.Delivered, CreatedAt = deliveredAt, CreatedBy = changedById };
        await db.Deliveries.AddAsync(delivery, ct);

        // Insert PR status history
        var history = new PurchaseRequestStatusHistory { Id = Guid.NewGuid(), PurchaseRequestId = prId, FromStatus = PurchaseRequestStatus.POCreated, ToStatus = PurchaseRequestStatus.Delivered, ChangedById = changedById, ChangedAt = deliveredAt, Comment = "Delivery recorded", CreatedAt = deliveredAt };
        await db.PurchaseRequestStatusHistories.AddAsync(history, ct);

        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync(ct);
            return false;
        }
    }

    public async Task<bool> TryCompleteAsync(Guid purchaseOrderId, string changedById, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);

        var poId = purchaseOrderId;

        // Atomically set PO status to Completed if it's currently Delivered
        var rows = await db.PurchaseOrders
            .Where(x => x.Id == poId && x.Status == PurchaseOrderStatus.Delivered)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, p => PurchaseOrderStatus.Completed), ct);
        if (rows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        // Update Delivery status from Delivered -> Completed
        var deliveryRows = await db.Deliveries
            .Where(d => d.PurchaseOrderId == poId && d.Status == DeliveryStatus.Delivered)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, d => DeliveryStatus.Completed), ct);
        if (deliveryRows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        // Get linked PurchaseRequestId
        var prId = await db.PurchaseOrders.Where(x => x.Id == poId).Select(x => x.PurchaseRequestId).FirstOrDefaultAsync(ct);
        if (prId == Guid.Empty)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        // Update PR status from Delivered -> Completed
        var prRows = await db.PurchaseRequests
            .Where(x => x.Id == prId && x.Status == PurchaseRequestStatus.Delivered)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, p => PurchaseRequestStatus.Completed), ct);
        if (prRows == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        // Insert PR status history
        var completedAt = DateTime.UtcNow;
        var history = new PurchaseRequestStatusHistory { Id = Guid.NewGuid(), PurchaseRequestId = prId, FromStatus = PurchaseRequestStatus.Delivered, ToStatus = PurchaseRequestStatus.Completed, ChangedById = changedById, ChangedAt = completedAt, Comment = "Transaction completed", CreatedAt = completedAt };
        await db.PurchaseRequestStatusHistories.AddAsync(history, ct);

        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync(ct);
            return false;
        }
    }
}

public class MasterDataRepository(ProcureFlowDbContext db) : IMasterDataRepository { public async Task<IReadOnlyCollection<Department>> GetDepartmentsAsync(CancellationToken ct = default) => await db.Departments.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(ct); public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken ct = default) => await db.Categories.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(ct); public async Task<IReadOnlyCollection<Vendor>> GetVendorsAsync(CancellationToken ct = default) => await db.Vendors.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(ct); }
