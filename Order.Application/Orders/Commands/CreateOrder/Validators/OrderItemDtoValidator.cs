using FluentValidation;
using OrderApplication.Orders.DTOs;

namespace OrderApplication.Orders.Commands.CreateOrder.Validators;

public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
{
 public OrderItemDtoValidator()
 {
 RuleFor(x => x.ProductId)
 .NotEmpty().WithMessage("Product ID is required")
 .NotNull().WithMessage("Product ID cannot be null");

 RuleFor(x => x.ProductName)
 .NotEmpty().WithMessage("Product Name is required")
 .NotNull().WithMessage("Product Name cannot be null")
 .Length(1, 200).WithMessage("Product Name must be between 1 and 200 characters");

 RuleFor(x => x.Price)
 .GreaterThan(0).WithMessage("Price must be greater than zero");
 }
}
