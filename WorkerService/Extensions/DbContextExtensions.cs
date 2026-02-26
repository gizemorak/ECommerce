using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderPersistence;

namespace WorkerService.Extensions;

internal static class DbContextExtensions
{
 internal static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
 {
     string connectionString = configuration.GetConnectionString("ecommercedb");

            services.AddDbContext<ApplicationDbContext>((sp, optionsBuilder) =>
     {
     optionsBuilder.UseSqlServer(connectionString);
     });

 return services;
 }
}
