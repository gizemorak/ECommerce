using Bus.Shared;
using Bus.Shared.Options;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderApplication.Orders.Queries.GetOrder;
using OrderDomain.Repositories;
using OrderPersistence;
using OrderPersistence.Repositories;
using Order.Api.Extensions;
using Order.Api.Services;

namespace Order.Api.Extensions;

public static class ServiceCollectionExtensions
{
 public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
 {
 services.AddAuthentication(configuration);
 services.AddApplicationAuthorization();
 services.AddApplicationRateLimiting(configuration);
 services.AddEndpointsApiExplorer();
 services.AddSwaggerGen();

 services.AddApiVersioning();
 services.AddApplicationHealthChecks();
 services.AddFluentValidation();

 services.AddScoped<ITokenService, TokenService>();

 services.Configure<ServiceBusOption>(configuration.GetSection(nameof(ServiceBusOption)));

 services.AddSingleton<ServiceBusOption>(sp =>
 {
 IOptions<ServiceBusOption> optionsServiceBus = sp.GetRequiredService<IOptions<ServiceBusOption>>();
 return optionsServiceBus.Value;
 });

 services.AddApplicationDbContext(configuration);

 services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetByIdOrderCommandHandler>());

 services.AddScoped<IOrderRepository, OrderRepository>();
 services.AddScoped<IUnitOfWork, UnitOfWork>();

 services.AddSingleton<IBusService, RabbitMqBusService>(sp =>
 {
 ServiceBusOption serviceBusOptions = sp.GetRequiredService<ServiceBusOption>();
 RabbitMqBusService rabbitMqBus = new RabbitMqBusService(serviceBusOptions);
 rabbitMqBus.Init().Wait();
 rabbitMqBus.CreateExchanges().Wait();
 return rabbitMqBus;
 });

 return services;
 }
}
