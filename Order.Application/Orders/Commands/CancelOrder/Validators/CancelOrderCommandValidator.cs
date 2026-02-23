using FluentValidation;
using OrderApplication.Orders.Commands.CancelOrder;

namespace OrderApplication.Orders.Commands.CancelOrder.Validators;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
 public CancelOrderCommandValidator()
 {
 RuleFor(x => x.OrderId)
 .GreaterThan(0).WithMessage("Order ID must be greater than zero");
 }
}
