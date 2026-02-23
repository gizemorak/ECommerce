using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderApplication.Orders.Commands.CreateOrder.Validators;

namespace Order.Api.Extensions;

public static class FluentValidationExtensions
{
 public static IServiceCollection AddFluentValidation(this IServiceCollection services)
 {
 services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

 return services;
 }
}
