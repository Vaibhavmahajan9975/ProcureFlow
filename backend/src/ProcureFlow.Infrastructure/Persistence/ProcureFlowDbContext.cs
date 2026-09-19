using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Domain.Common;
using ProcureFlow.Domain.Entities;
namespace ProcureFlow.Infrastructure.Persistence;
public class ProcureFlowDbContext(DbContextOptions<ProcureFlowDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Department> Departments => Set<Department>(); public DbSet<Category> Categories => Set<Category>(); public DbSet<Vendor> Vendors => Set<Vendor>(); public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>(); public DbSet<PurchaseRequestStatusHistory> PurchaseRequestStatusHistories => Set<PurchaseRequestStatusHistory>(); public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>(); public DbSet<Delivery> Deliveries => Set<Delivery>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Department>().HasIndex(x=>x.Name).IsUnique(); b.Entity<Category>().HasIndex(x=>x.Name).IsUnique(); b.Entity<Vendor>().HasIndex(x=>x.Name);
        b.Entity<PurchaseRequest>().HasIndex(x=>x.PRNumber).IsUnique(); b.Entity<PurchaseRequest>().Property(x=>x.Amount).HasPrecision(18,2); b.Entity<PurchaseRequest>().Property(x=>x.Currency).HasConversion<string>(); b.Entity<PurchaseRequest>().Property(x=>x.Status).HasConversion<string>();
        b.Entity<PurchaseRequest>().HasOne(x=>x.Requester).WithMany().HasForeignKey(x=>x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PurchaseRequest>().HasOne(x=>x.Department).WithMany().HasForeignKey(x=>x.DepartmentId).OnDelete(DeleteBehavior.Restrict); b.Entity<PurchaseRequest>().HasOne(x=>x.Vendor).WithMany().HasForeignKey(x=>x.VendorId).OnDelete(DeleteBehavior.Restrict); b.Entity<PurchaseRequest>().HasOne(x=>x.Category).WithMany().HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PurchaseRequestStatusHistory>().Property(x=>x.FromStatus).HasConversion<string>(); b.Entity<PurchaseRequestStatusHistory>().Property(x=>x.ToStatus).HasConversion<string>();
        // Map ChangedById to ApplicationUser for display purposes (no schema change required)
        b.Entity<PurchaseRequestStatusHistory>().HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PurchaseOrder>().HasIndex(x=>x.PONumber).IsUnique(); b.Entity<PurchaseOrder>().HasIndex(x=>x.PurchaseRequestId).IsUnique(); b.Entity<PurchaseOrder>().Property(x=>x.TotalAmount).HasPrecision(18,2); b.Entity<PurchaseOrder>().Property(x=>x.Currency).HasConversion<string>(); b.Entity<PurchaseOrder>().Property(x=>x.Status).HasConversion<string>(); b.Entity<PurchaseOrder>().HasOne(x=>x.PurchaseRequest).WithOne(x=>x.PurchaseOrder).HasForeignKey<PurchaseOrder>(x=>x.PurchaseRequestId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Delivery>().HasIndex(x=>x.PurchaseOrderId).IsUnique(); b.Entity<Delivery>().Property(x=>x.Status).HasConversion<string>(); b.Entity<Delivery>().HasOne(x=>x.PurchaseOrder).WithOne(x=>x.Delivery).HasForeignKey<Delivery>(x=>x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken ct=default)
    {
        var now=DateTime.UtcNow; foreach(var e in ChangeTracker.Entries<BaseEntity>()) { if(e.State==EntityState.Added) e.Entity.CreatedAt=now; if(e.State==EntityState.Modified) e.Entity.UpdatedAt=now; } return await base.SaveChangesAsync(ct);
    }
}