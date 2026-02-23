using FluentValidation;
using OrderApplication.Orders.Commands.CreateOrder;
using OrderApplication.Orders.DTOs;

namespace OrderApplication.Orders.Commands.CreateOrder.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
 public CreateOrderCommandValidator()
 {
 RuleFor(x => x.UserId)
 .NotEmpty().WithMessage("User ID is required")
 .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty");

 RuleFor(x => x.adressdto)
 .NotNull().WithMessage("Address is required")
 .SetValidator(new AddressDtoValidator());

 RuleFor(x => x.OrderItems)
 .NotEmpty().WithMessage("Order Items cannot be empty")
 .NotNull().WithMessage("Order Items cannot be null")
 .Must(items => items.Any()).WithMessage("At least one order item is required");

 RuleForEach(x => x.OrderItems)
 .SetValidator(new OrderItemDtoValidator());

 RuleFor(x => x.Payment)
 .NotNull().WithMessage("Payment information is required")
 .SetValidator(new PaymentDtoValidator());
 }
}
