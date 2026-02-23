using FluentValidation;
using OrderApplication.Orders.Queries.GetOrder;

namespace OrderApplication.Orders.Queries.GetOrder.Validators;

public class GetByIdOrderCommandValidator : AbstractValidator<GetByIdOrderCommand>
{
 public GetByIdOrderCommandValidator()
 {
 RuleFor(x => x.OrderId)
 .GreaterThan(0).WithMessage("Order ID must be greater than zero");
 }
}
