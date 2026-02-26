using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderPersistence;

namespace Order.Api.Extensions;

public static class DbContextExtensions
{
 public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
 {
 string connectionString = configuration.GetConnectionString("ecommercedb");

 services.AddDbContext<ApplicationDbContext>((sp, optionsBuilder) =>
 {
 optionsBuilder.UseSqlServer(connectionString);
 });

 return services;
 }
}
