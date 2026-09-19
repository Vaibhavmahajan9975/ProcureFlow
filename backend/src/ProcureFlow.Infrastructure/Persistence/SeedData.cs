using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureFlow.Domain.Entities;
namespace ProcureFlow.Infrastructure.Persistence;
public static class SeedData
{
    public static async Task SeedAsync(ProcureFlowDbContext db,UserManager<ApplicationUser> users,RoleManager<IdentityRole> roles)
    {
        if ((await db.Database.GetPendingMigrationsAsync()).Any()) await db.Database.MigrateAsync(); else await db.Database.EnsureCreatedAsync(); foreach(var role in new[]{"Requester","Approver","Admin"}) if(!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        if(!await db.Departments.AnyAsync()){db.Departments.AddRange(new Department{Name="IT"},new Department{Name="Finance"},new Department{Name="HR"},new Department{Name="Operations"}); db.Categories.AddRange(new Category{Name="IT Equipment"},new Category{Name="Software"},new Category{Name="Office Supplies"},new Category{Name="Professional Services"}); db.Vendors.AddRange(new Vendor{Name="Microsoft",ContactEmail="sales@microsoft.example"},new Vendor{Name="Dell",ContactEmail="sales@dell.example"},new Vendor{Name="Amazon Business"},new Vendor{Name="Infosys"},new Vendor{Name="HP"}); await db.SaveChangesAsync();}
        await EnsureUser("requester@procureflow.demo","Requester User","Requester"); await EnsureUser("approver@procureflow.demo","Approver User","Approver"); await EnsureUser("admin@procureflow.demo","Admin User","Admin");
        async Task EnsureUser(string email,string name,string role){var u=await users.FindByEmailAsync(email); if(u is null){u=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,FullName=name}; var result=await users.CreateAsync(u,"Demo@123"); if(!result.Succeeded) throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description))); await users.AddToRoleAsync(u,role);}}
    }
}