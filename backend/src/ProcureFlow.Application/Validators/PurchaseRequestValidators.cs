using FluentValidation;
using ProcureFlow.Application.DTOs;
namespace ProcureFlow.Application.Validators;
public class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequestDto>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x=>x.DepartmentId).NotEmpty();
        RuleFor(x=>x.VendorId).NotEmpty();
        RuleFor(x=>x.CategoryId).NotEmpty();
        RuleFor(x=>x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x=>x.Amount).GreaterThan(0);
        RuleFor(x=>x.Currency).IsInEnum();
        var minDate = DateOnly.FromDateTime(DateTime.UtcNow);
        RuleFor(x=>x.RequiredDate).GreaterThanOrEqualTo(minDate).WithMessage($"Required Date cannot be earlier than {minDate.ToString("dd MMM yyyy")}.");
    }
}
public class UpdatePurchaseRequestValidator : AbstractValidator<UpdatePurchaseRequestDto>
{
    public UpdatePurchaseRequestValidator()
    {
        Include(new InlineValidator<UpdatePurchaseRequestDto>());
        RuleFor(x=>x.DepartmentId).NotEmpty(); RuleFor(x=>x.VendorId).NotEmpty(); RuleFor(x=>x.CategoryId).NotEmpty();
        RuleFor(x=>x.Description).NotEmpty().MaximumLength(1000); RuleFor(x=>x.Amount).GreaterThan(0);
        RuleFor(x=>x.Currency).IsInEnum(); var minDate2 = DateOnly.FromDateTime(DateTime.UtcNow);
        RuleFor(x=>x.RequiredDate).GreaterThanOrEqualTo(minDate2).WithMessage($"Required Date cannot be earlier than {minDate2.ToString("dd MMM yyyy")}.");
    }
}