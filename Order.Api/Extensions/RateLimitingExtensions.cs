using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Api.Options;

namespace Order.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitOptions = new RateLimitOptions();
        configuration.GetSection(nameof(RateLimitOptions)).Bind(rateLimitOptions);

        services.AddSingleton(rateLimitOptions);

        return services;
    }
}
