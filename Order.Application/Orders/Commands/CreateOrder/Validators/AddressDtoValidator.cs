using FluentValidation;
using OrderApplicationOrders.Commands.CreateOrder;

namespace OrderApplication.Orders.Commands.CreateOrder.Validators;

public class AddressDtoValidator : AbstractValidator<AdressDto>
{
 public AddressDtoValidator()
 {
 RuleFor(x => x.Street)
 .NotEmpty().WithMessage("Street is required")
 .NotNull().WithMessage("Street cannot be null")
 .Length(1, 100).WithMessage("Street must be between 1 and 100 characters");

 RuleFor(x => x.City)
 .NotEmpty().WithMessage("City is required")
 .NotNull().WithMessage("City cannot be null")
 .Length(1, 50).WithMessage("City must be between 1 and 50 characters");

 RuleFor(x => x.State)
 .NotEmpty().WithMessage("State is required")
 .NotNull().WithMessage("State cannot be null")
 .Length(1, 50).WithMessage("State must be between 1 and 50 characters");

 RuleFor(x => x.Country)
 .NotEmpty().WithMessage("Country is required")
 .NotNull().WithMessage("Country cannot be null")
 .Length(1, 50).WithMessage("Country must be between 1 and 50 characters");

 RuleFor(x => x.ZipCode)
 .NotEmpty().WithMessage("ZipCode is required")
 .NotNull().WithMessage("ZipCode cannot be null")
 .Matches(@"^\d{5}(-\d{4})?$").WithMessage("ZipCode must be a valid format (e.g., 12345 or 12345-6789)");
 }
}
