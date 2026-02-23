using Microsoft.Extensions.DependencyInjection;
using OrderPersistence;

namespace Order.Api.Extensions;

public static class HealthCheckExtensions
{
 public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
 {
 services
 .AddHealthChecks()
 .AddDbContextCheck<ApplicationDbContext>(
 name: "database",
 tags: new[] { "ready" });

 return services;
 }
}
