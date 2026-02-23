using FluentValidation;
using OrderApplication.Orders.Commands.CreateOrder;

namespace OrderApplication.Orders.Commands.CreateOrder.Validators;

public class PaymentDtoValidator : AbstractValidator<PaymentDto>
{
 public PaymentDtoValidator()
 {
 RuleFor(x => x.CardNumber)
 .NotEmpty().WithMessage("Card Number is required")
 .NotNull().WithMessage("Card Number cannot be null")
 .Matches(@"^\d{13,19}$").WithMessage("Card Number must be between 13 and 19 digits");

 RuleFor(x => x.CardHolderName)
 .NotEmpty().WithMessage("Card Holder Name is required")
 .NotNull().WithMessage("Card Holder Name cannot be null")
 .Length(2, 100).WithMessage("Card Holder Name must be between 2 and 100 characters");

 RuleFor(x => x.Expiration)
 .NotEmpty().WithMessage("Expiration is required")
 .NotNull().WithMessage("Expiration cannot be null")
 .Matches(@"^(0[1-9]|1[0-2])\/\d{2}$").WithMessage("Expiration must be in MM/YY format");

 RuleFor(x => x.Cvc)
 .NotEmpty().WithMessage("CVC is required")
 .NotNull().WithMessage("CVC cannot be null")
 .Matches(@"^\d{3,4}$").WithMessage("CVC must be 3 or 4 digits");

 RuleFor(x => x.Amount)
 .GreaterThan(0).WithMessage("Amount must be greater than zero");
 }
}
