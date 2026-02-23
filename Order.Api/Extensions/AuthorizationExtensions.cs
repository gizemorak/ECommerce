using Microsoft.Extensions.DependencyInjection;

namespace Order.Api.Extensions;

public static class AuthorizationExtensions
{
 public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
 {
 services.AddAuthorization(options =>
 {
 options.AddPolicy("AdminOnly", policy =>
 policy.RequireRole("Admin"));

 options.AddPolicy("UserOrAdmin", policy =>
 policy.RequireRole("User", "Admin"));
 });

 return services;
 }
}
