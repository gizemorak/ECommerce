using Bus.Shared;
using Bus.Shared.Extensions;
using Bus.Shared.Options;
using Bus.Shared.Publishers;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Order.Api.Extensions;
using Order.Api.Services;
using OrderApplication.Orders.Queries.GetOrder;
using OrderDomain.Repositories;
using OrderPersistence;
using OrderPersistence.Repositories;

namespace Order.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddAuthentication(configuration);
       // services.AddApplicationAuthorization();
        services.AddApplicationRateLimiting(configuration);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddApiVersioning();
        services.AddApplicationHealthChecks();
        services.AddFluentValidation();

        //services.AddScoped<ITokenService, TokenService>();

        // Use new unified bus service registration
        services.AddRabbitMQBus(configuration);

        services.AddKafkaBus(configuration);

        services.AddRedisBus(configuration);

        // ? Add bus initialization service
        services.AddHostedService<BusInitializationService>();

        services.AddScoped<MessagePublisher>();

        services.AddApplicationDbContext(configuration);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetByIdOrderCommandHandler>());

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}


